using System.Text;

using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Proves the frozen parse pipeline and the live one agree, and that the comparison can tell them apart.
/// </summary>
/// <remarks>
///     <para>
///     The suite gained 107 tests around <c>CreateSafeNavigator</c> during the test expansion and not
///     one of them is a differential — they assert what the current implementation does, so they move
///     with it, and a rewrite that changes behaviour consistently passes every one. This file is the
///     only thing standing between the coming rewrite of the decode and sanitise stages and the
///     failure recorded in <c>docs/build-warnings.md</c> §4.4, where a <c>GetXmlEncoding</c> rewrite
///     shipped three regressions behind a green 3,700-test suite.
///     </para>
/// </remarks>
[TestClass]
public sealed class ParsePipelineDifferentialTests
{
    private const string Clean = """<?xml version="1.0" encoding="utf-8"?><root><child attribute="value">text</child></root>""";

    /// <summary>
    /// Gets every linked sample document, one per test case.
    /// </summary>
    public static IEnumerable<object[]> EverySample => SampleFeeds.All.Select(name => new object[] { name });

    /// <summary>
    /// The negative control, and the reason any of the rest of this file means anything.
    /// </summary>
    /// <remarks>
    ///     A frozen copy that accidentally called the live implementation would agree with it on every
    ///     input forever, including through the regression this harness exists to catch. So the harness
    ///     is required to report a difference for a pipeline that differs by exactly one line — the
    ///     surrogate-pair arm of the invalid-character scan.
    ///     <para>
    ///     Committed rather than performed by hand. The instruction "perturb one line and check it goes
    ///     red" is a one-time act that stops protecting anything the moment it is undone; this runs at
    ///     every commit from here to the end of the rewrite.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TheDifferential_ReportsADifference_WhenTheLegacyScanIsPerturbed()
    {
        // A valid astral character preceded by an invalid one, so the scan reaches the surrogate arm.
        string document = "<r>\u0001\U0001F600</r>";

        (ParseOutcome faithful, ParseOutcome live) = ParsePipelineDifferential.OverString(document);
        (ParseOutcome mutated, _) = ParsePipelineDifferential.OverString(document, LegacyParsePipeline.Mutated);

        faithful.ShouldBe(live, "the faithful copy must agree with the live pipeline");
        mutated.ShouldNotBe(live, "a one-line defect in the frozen copy must be visible, or agreement means nothing");
    }

    /// <summary>
    /// The golden corpus, through the entry point every HTTP load uses.
    /// </summary>
    /// <remarks>
    ///     Structural proof only. All fifteen documents are 7-bit ASCII with no byte-order marks and
    ///     short declarations, so this cannot fail on a mis-decode — it can only show that the shape of
    ///     a real document survives. The encoding fidelity argument lives in the matrix, not here.
    /// </remarks>
    /// <param name="fileName">The sample document under test.</param>
    [TestMethod]
    [DynamicData(nameof(EverySample))]
    public void EverySampleDocument_ParsesIdenticallyThroughBothPipelines(string fileName)
    {
        byte[] document = File.ReadAllBytes(SampleFeeds.PathTo(fileName));

        (ParseOutcome legacy, ParseOutcome live) = ParsePipelineDifferential.OverStream(document);

        legacy.ExceptionType.ShouldBeNull($"{fileName} should parse through the frozen pipeline");
        live.ShouldBe(legacy);
    }

    /// <summary>
    /// The four entry points agree with their frozen counterparts on an ordinary document.
    /// </summary>
    [TestMethod]
    public void AllFourEntryPoints_AgreeOnACleanDocument()
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(Clean);

        ParsePipelineDifferential.OverString(Clean).ShouldSatisfyAllConditions(
            pair => pair.Live.ShouldBe(pair.Legacy));
        ParsePipelineDifferential.OverTextReader(Clean).ShouldSatisfyAllConditions(
            pair => pair.Live.ShouldBe(pair.Legacy));
        ParsePipelineDifferential.OverStream(utf8).ShouldSatisfyAllConditions(
            pair => pair.Live.ShouldBe(pair.Legacy));
        ParsePipelineDifferential.OverStreamWithEncoding(utf8, Encoding.UTF8).ShouldSatisfyAllConditions(
            pair => pair.Live.ShouldBe(pair.Legacy));
    }

    /// <summary>
    /// The two pipelines fail identically, not merely both.
    /// </summary>
    /// <remarks>
    ///     Comparing exception types alone would make any two parse failures equal. These four inputs
    ///     produce four different failures — a guard, a guard reached through a different overload, and
    ///     a parse error carrying a line position — and the harness has to distinguish them.
    /// </remarks>
    [TestMethod]
    public void TheTwoPipelines_FailIdenticallyOnDocumentsThatCannotParse()
    {
        (ParseOutcome legacyEmpty, ParseOutcome liveEmpty) = ParsePipelineDifferential.OverString(string.Empty);
        legacyEmpty.ExceptionType.ShouldBe(typeof(ArgumentException).FullName);
        liveEmpty.ShouldBe(legacyEmpty);

        (ParseOutcome legacyStream, ParseOutcome liveStream) = ParsePipelineDifferential.OverStream([]);
        legacyStream.ExceptionType.ShouldBe(typeof(ArgumentException).FullName);
        liveStream.ShouldBe(legacyStream);

        (ParseOutcome legacyMalformed, ParseOutcome liveMalformed) =
            ParsePipelineDifferential.OverString("<root><unclosed></root>");
        legacyMalformed.ExceptionType.ShouldBe(typeof(System.Xml.XmlException).FullName);
        liveMalformed.ShouldBe(legacyMalformed);

        (ParseOutcome legacyReader, ParseOutcome liveReader) = ParsePipelineDifferential.OverTextReader(string.Empty);
        legacyReader.ExceptionType.ShouldBe(typeof(ArgumentException).FullName);
        liveReader.ShouldBe(legacyReader);
    }

    /// <summary>
    /// Dirty documents survive both pipelines identically, including the shapes the corpus never had.
    /// </summary>
    /// <remarks>
    ///     These are the inputs Phases 1 and 2 rewrite the handling of. Every one of them is new to this
    ///     repository: §2.19 records the surrogate arm of the rebuild loop at zero hits, and the scan's
    ///     end-of-string operand never once false.
    /// </remarks>
    [TestMethod]
    public void TheTwoPipelines_AgreeOnEveryDirtyShape()
    {
        string[] documents =
        [
            "<r>a\u0001b</r>",                      // a control character in text
            "<r>a\0b</r>",                          // a NUL
            "<r>a\uFFFEb</r>",                      // a non-character
            "<r>a\uFFFFb</r>",                      // the other non-character
            "<r>a\u0001</r>",                       // invalid immediately before the close tag
            "<r>\u0001\U0001F600</r>",              // invalid, then a valid pair - the untested arm
            "<r>\U0001F600\u0001</r>",              // a valid pair, then invalid
            "<r>a\uD800b</r>",                      // a lone high surrogate
            "<r>a\uDC00b</r>",                      // a lone low surrogate
            "<r>a\uD800\uD800b</r>",                // two highs in a row
            "<r><![CDATA[a\u0001b]]></r>",          // inside CDATA
            "<r>" + new string('\0', 20_000) + "x</r>",   // a whole chunk of nothing
        ];

        foreach (string document in documents)
        {
            (ParseOutcome legacy, ParseOutcome live) = ParsePipelineDifferential.OverString(document);
            live.ShouldBe(legacy, $"the string entry point diverged on {Describe(document)}");

            (ParseOutcome readerLegacy, ParseOutcome readerLive) = ParsePipelineDifferential.OverTextReader(document);
            readerLive.ShouldBe(readerLegacy, $"the reader entry point diverged on {Describe(document)}");
        }
    }

    private static string Describe(string document) =>
        string.Concat(document.Take(40).Select(c => char.IsControl(c) || char.IsSurrogate(c)
            ? $"\\u{(int)c:X4}"
            : c.ToString()));
}