using System.Diagnostics.CodeAnalysis;
using System.Xml.XPath;

using Argotic.Net;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Protocols;

/// <summary>
/// Measures every branch of <see cref="XmlRpcClient.TryParseValue(XPathNavigator, out IXmlRpcValue?)"/>,
/// eight of which have never executed.
/// </summary>
/// <remarks>
/// <para>
/// A coverage audit of <c>Argotic.Net</c> found <b>every wire-touching method at 0.000</b>, and inside
/// this method specifically found that <b>only the <c>i4</c> branch has ever run</b>. <c>int</c>,
/// <c>boolean</c>, <c>string</c>, <c>double</c>, <c>dateTime.iso8601</c>, <c>base64</c>, <c>struct</c>
/// and <c>array</c> are cold, in a method that every XML-RPC response in the library funnels through.
/// This class exists so that a change to it has a before.
/// </para>
/// <para>
/// <b>The arms are ordered by their position in the dispatch chain, because that ordering is the
/// hypothesis.</b> <c>TryParseValue</c> is a linear <c>if</c>/<c>else if</c> over nine
/// <see cref="string.Equals(string, string, StringComparison)"/> calls with
/// <see cref="StringComparison.OrdinalIgnoreCase"/>, tested in source order: <c>i4</c>, <c>int</c>,
/// <c>boolean</c>, <c>string</c>, <c>double</c>, <c>dateTime.iso8601</c>, <c>base64</c>,
/// <c>struct</c>, <c>array</c>. If dispatch position dominates, these arms form a rising staircase and
/// the remedy is a switch on the element name. If they are flat, the chain costs nothing, the
/// hypothesis is dead, and the cost is in the per-type conversion instead — which is a result worth
/// having, because it would move the investigation to <see cref="Convert.FromBase64String(string)"/>
/// and the date parser rather than to the dispatch.
/// </para>
/// <para>
/// The same reasoning that shaped <c>DateTimeParsingBenchmarks</c>: an ordered table is only a
/// suspect if the arms can show where in the table each input matched.
/// </para>
/// <para>
/// <b>Building this class is what found the defect §2.42 fixed.</b> A spec-legal untyped
/// <c>&lt;value&gt;text&lt;/value&gt;</c> — which XML-RPC 1.0 defines as a string, and which real ping
/// servers emit — was <em>not parsed by this library at all</em>, and the branch written to handle it
/// was unreachable for every possible input. Choosing an input for an arm is what surfaced it; no
/// measurement was involved. The reasoning is on <see cref="ParseUntyped"/>, which now measures the
/// success path it always should have.
/// </para>
/// <para>
/// <b>There is no <c>[Params]</c> axis here on purpose.</b> Nine of these twelve arms are a single
/// element with fixed content and do not vary with any size parameter, so a class-scoped axis would
/// reproduce nine identical rows per value and bury the two that moved. That is the defect
/// <c>docs/build-warnings.md</c> §D0 records in <c>UtilityBenchmarks</c>. Size-varying payloads live
/// in <see cref="XmlRpcCompositeParsingBenchmarks"/>, where every arm varies along the axis.
/// </para>
/// </remarks>
[BenchmarkCategory("protocols", "xmlrpc", "parse")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class XmlRpcValueParsingBenchmarks
{
    private XPathNavigator i4Value = null!;
    private XPathNavigator intValue = null!;
    private XPathNavigator booleanValue = null!;
    private XPathNavigator stringValue = null!;
    private XPathNavigator doubleValue = null!;
    private XPathNavigator dateTimeValue = null!;
    private XPathNavigator specSpelledDateTimeValue = null!;
    private XPathNavigator base64Value = null!;
    private XPathNavigator structValue = null!;
    private XPathNavigator arrayValue = null!;
    private XPathNavigator untypedValue = null!;
    private XPathNavigator unparseableValue = null!;

    /// <summary>
    /// Builds one navigator per branch, positioned exactly where the library positions one.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.i4Value = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("i4", "1138"));
        this.intValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("int", "1138"));
        this.booleanValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("boolean", "1"));
        this.stringValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("string", "The quick brown fox jumps over the lazy dog."));
        this.doubleValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("double", "-12.53"));
        this.dateTimeValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("dateTime.iso8601", "2024-01-01T10:00:00Z"));
        this.specSpelledDateTimeValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("dateTime.iso8601", "20240101T10:00:00"));

        // 1 KiB decoded. Small enough to be an ordinary ping payload, large enough that the decode is
        // not lost inside the dispatch it is being compared against.
        this.base64Value = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.Base64Scalar(1024));

        this.structValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.StructValue(8));
        this.arrayValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.ArrayValue(8));
        this.untypedValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.UntypedScalar("An untyped value, which the specification defines as a string."));
        this.unparseableValue = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.TypedScalar("i4", "not a number"));
    }

    /// <summary>
    /// Dispatch position 1: the only branch that has ever executed.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     The baseline because it is the cheapest possible dispatch — one string comparison — and
    ///     because it is what every existing measurement of this method actually measured.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "1. i4 (the only branch ever covered)")]
    public IXmlRpcValue? ParseI4()
    {
        _ = XmlRpcClient.TryParseValue(this.i4Value, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 2: the same conversion as <see cref="ParseI4"/>, one comparison further down.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     <c>int</c> and <c>i4</c> run byte-identical bodies — both call
    ///     <see cref="int.TryParse(string, System.Globalization.NumberStyles, IFormatProvider, out int)"/>
    ///     and construct one <c>XmlRpcScalarValue</c>. The <em>only</em> difference between these two
    ///     arms is one extra <see cref="string.Equals(string, string, StringComparison)"/>, which makes
    ///     this pair the cleanest possible probe of what a single chain step costs. If they measure
    ///     apart, the chain is real; if they measure together, no arm below can blame the chain either.
    /// </remarks>
    [Benchmark(Description = "2. int (identical body to i4, one comparison later)")]
    public IXmlRpcValue? ParseInt()
    {
        _ = XmlRpcClient.TryParseValue(this.intValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 3: routes through the internal four-way <c>TryParseBoolean</c>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     <c>1</c> is the first of the four accepted spellings, so this measures the boolean parser's
    ///     best case. <c>false</c> is its worst, at four comparisons — not measured separately here
    ///     because the difference is three ordinal comparisons on a two-character string, and an arm
    ///     that cannot plausibly move is an arm that will be read as noise.
    /// </remarks>
    [Benchmark(Description = "3. boolean")]
    public IXmlRpcValue? ParseBoolean()
    {
        _ = XmlRpcClient.TryParseValue(this.booleanValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 4: the only branch with no conversion at all.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     Reading <c>navigator.Value</c> and wrapping it is the whole body, so this arm isolates
    ///     dispatch plus navigator materialisation with nothing else in it. Anything the other arms
    ///     cost above this is conversion.
    /// </remarks>
    [Benchmark(Description = "4. string (no conversion; dispatch + navigator only)")]
    public IXmlRpcValue? ParseString()
    {
        _ = XmlRpcClient.TryParseValue(this.stringValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 5.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark(Description = "5. double")]
    public IXmlRpcValue? ParseDouble()
    {
        _ = XmlRpcClient.TryParseValue(this.doubleValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 6: the branch that reaches the date parser.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     <para>
    ///     This is the one branch whose cost is already known to be non-trivial from elsewhere in the
    ///     harness: it calls <c>SyndicationDateTimeUtility.TryParseRfc3339DateTime</c>, which walks a
    ///     nine-pattern table. <c>DateTimeParsingBenchmarks</c> prices that call in isolation, so the
    ///     delta between the two classes attributes cost to the XML-RPC wrapper rather than to the
    ///     date parser it is misusing.
    ///     </para>
    ///     <para>
    ///     Worth stating plainly, because it bears on how this number should be read: the element is
    ///     named <c>dateTime.iso8601</c> and XML-RPC's own examples spell it <c>19980717T14:08:55</c>
    ///     — <b>no hyphens, no colons in the date, no offset</b> — which is not RFC 3339 and which the
    ///     RFC 3339 table cannot match. That spelling did not parse at all until §2.42. This arm keeps
    ///     the RFC 3339 spelling, which is what most live servers emit and what already worked;
    ///     <see cref="ParseSpecSpelledDateTime"/> is the one that measures the fallback, and it is
    ///     strictly the more expensive of the two because RFC 3339 is still tried first.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "6. dateTime.iso8601, RFC 3339 spelling (matches the first table)")]
    public IXmlRpcValue? ParseDateTime()
    {
        _ = XmlRpcClient.TryParseValue(this.dateTimeValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 6, in XML-RPC's own spelling: the worst case of the date branch.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     <para>
    ///     <c>20240101T10:00:00</c> — the spelling XML-RPC 1.0 uses in its own examples, and the one
    ///     that did not parse at all before §2.42. It is <b>strictly the more expensive of the two date
    ///     arms by construction</b>: RFC 3339 is still attempted first and must fail its whole
    ///     nine-pattern table before the two XML-RPC formats are tried, so this pays the other arm's
    ///     work plus its own.
    ///     </para>
    ///     <para>
    ///     That is the point of measuring it rather than assuming it. The fix was written as a fallback
    ///     precisely so nothing that parsed before parses differently, and the price of that choice is
    ///     paid by whichever spelling comes second. This arm says what that price is, and whether the
    ///     ordering should be reconsidered if the XML-RPC spelling turns out to be the common one on the
    ///     wire — which nothing in this repository currently knows.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "6b. dateTime.iso8601, XML-RPC spelling (RFC 3339 table must fail first)")]
    public IXmlRpcValue? ParseSpecSpelledDateTime()
    {
        _ = XmlRpcClient.TryParseValue(this.specSpelledDateTimeValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 7: the only scalar branch whose cost scales with its content.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     One kibibyte decoded. This branch also wraps its conversion in a
    ///     <see langword="try"/>/<see langword="catch"/> for <see cref="FormatException"/>, so the
    ///     harness's <c>ExceptionDiagnoser</c> column is meaningful here: it must read zero on this
    ///     arm, and a non-zero value would mean the corpus is emitting invalid base64 rather than that
    ///     the branch is slow.
    /// </remarks>
    [Benchmark(Description = "7. base64, 1 KiB decoded")]
    public IXmlRpcValue? ParseBase64()
    {
        _ = XmlRpcClient.TryParseValue(this.base64Value, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 8: eight comparisons, then recursion through <c>XmlRpcStructureValue</c>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     Held at eight members so that it is comparable with <see cref="ParseArray"/> element for
    ///     element. <c>XmlRpcStructureValue</c> was measured at cyclomatic complexity 48 with 0.000
    ///     coverage — the largest single cold body in <c>Argotic.Net</c>.
    /// </remarks>
    [Benchmark(Description = "8. struct, 8 members (recurses)")]
    public IXmlRpcValue? ParseStruct()
    {
        _ = XmlRpcClient.TryParseValue(this.structValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// Dispatch position 9: the worst dispatch case, then recursion through <c>XmlRpcArrayValue</c>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     Nine comparisons before the branch is entered, and then every child re-enters at position 1.
    ///     Held at eight elements so the struct-versus-array comparison is at equal element count.
    /// </remarks>
    [Benchmark(Description = "9. array, 8 elements (worst dispatch, recurses)")]
    public IXmlRpcValue? ParseArray()
    {
        _ = XmlRpcClient.TryParseValue(this.arrayValue, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// A spec-legal untyped <c>value</c>, which this library cannot parse: the whole chain, then failure.
    /// </summary>
    /// <returns>Whether the value parsed, which is <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     XML-RPC 1.0 says "If no type is indicated, the type is string", and real ping servers emit
    ///     it. <c>TryParseValue</c> returns <see langword="false"/> for it. That was measured against
    ///     the shipped code, not inferred from reading it, and it is why this arm returns a
    ///     <see cref="bool"/>: a <see langword="true"/> in the results table would mean the behaviour
    ///     has changed and this remark is stale.
    ///     </para>
    ///     <para>
    ///     Why it used to fail, and why the code written to handle it was unreachable: text is a child
    ///     node in the XPath data model, so <c>HasChildren</c> is <see langword="true"/> here. Control
    ///     entered the chain, <c>MoveToFirstChild</c> landed on the text node whose <c>Name</c> is
    ///     empty, all nine comparisons failed, and the method fell through to <c>return false</c>. The
    ///     <c>else if (!string.IsNullOrEmpty(source.Value))</c> tail meant for this shape was reached
    ///     only when the element had no children — and such an element has an empty <c>Value</c>, so
    ///     its own guard failed as well. No input could execute it.
    ///     </para>
    ///     <para>
    ///     §2.42 replaced <c>MoveToFirstChild</c> with <c>MoveToChild(XPathNodeType.Element)</c>, which
    ///     changes what this arm measures rather than merely whether it succeeds. It is now the
    ///     <b>shortest</b> path rather than the longest: the element test fails immediately and the
    ///     untyped tail runs, so it skips the comparison chain entirely instead of walking all of it.
    ///     Read against <see cref="ParseString"/> — the same string, reached through the full dispatch —
    ///     the delta is what the chain itself costs.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "10. untyped value (spec-legal; skips the chain entirely)")]
    public bool ParseUntyped() => XmlRpcClient.TryParseValue(this.untypedValue, out _);

    /// <summary>
    /// A typed value whose content the type cannot parse, which returns <see langword="false"/>.
    /// </summary>
    /// <returns>Whether the value parsed, which must be <see langword="false"/>.</returns>
    /// <remarks>
    ///     The failure path of a <c>TryParse</c> is not free and is not the success path with a
    ///     different return value: it falls out of the <c>i4</c> branch, past every remaining
    ///     comparison, to the shared <c>value = null; return false;</c> tail. A server that emits a
    ///     malformed integer costs its client this, and nothing has ever measured it. Against
    ///     <see cref="ParseUntyped"/>, this fails after one comparison rather than nine.
    /// </remarks>
    [Benchmark(Description = "11. i4 with unparseable content (fails after 1 comparison)")]
    public bool ParseUnparseable() => XmlRpcClient.TryParseValue(this.unparseableValue, out _);
}