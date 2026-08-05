using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins how every combination of byte-order mark and declared encoding decodes today.
/// </summary>
/// <remarks>
///     <para>
///     The rewrite replaces a whole-document buffer-then-decode with a bounded head sniff and a
///     streaming decode. Nothing here is a judgement about what the answers <i>should</i> be — some of
///     them are wrong, and are pinned as wrong with a comment saying so. The value is that after the
///     rewrite they must still be exactly these answers, or the change is a behaviour change nobody
///     chose.
///     </para>
///     <para>
///     Every case asserts twice: the decoded value, and that the frozen pipeline and the live one agree
///     on it. The second assertion is what survives the rewrite; the first is what makes a failure
///     legible.
///     </para>
///     <para>
///     <b>None of this can be proved by the sample corpus.</b> All fifteen linked documents are 7-bit
///     ASCII with no byte-order marks and short declarations, so they decode identically under any
///     implementation that is not actively broken.
///     </para>
/// </remarks>
[TestClass]
public sealed class EncodingCharacterisationTests
{
    /// <summary>The payload where the encoding can carry it: accented, punctuated, and astral.</summary>
    private const string RichPayload = "café — “smart” \U0001F600";

    /// <summary>The payload where the encoding cannot: Latin-1 has no em dash, curly quotes or emoji.</summary>
    private const string LatinPayload = "café";

    /// <summary>
    /// Gets the encoding matrix, one case per row.
    /// </summary>
    public static IEnumerable<object[]> Matrix =>
        Cases().Select(c => new object[] { c.Name, c.Document, c.ExpectedTitle });

    /// <summary>
    /// Every row of the matrix decodes to the pinned value, and both pipelines agree.
    /// </summary>
    /// <param name="name">The case name, so a failure names itself.</param>
    /// <param name="document">The document's bytes.</param>
    /// <param name="expected">The title text the current implementation produces.</param>
    [TestMethod]
    [DynamicData(nameof(Matrix))]
    public void EveryEncodingShape_DecodesToThePinnedValue(string name, byte[] document, string expected)
    {
        using MemoryStream stream = new(document, writable: false);
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        TitleOf(navigator).ShouldBe(expected, name);

        (ParseOutcome legacy, ParseOutcome live) = ParsePipelineDifferential.OverStream(document);
        live.ShouldBe(legacy, $"{name}: the frozen and live pipelines disagree");
    }

    /// <summary>
    /// Row 15 — every row again through a non-seekable stream that returns one byte per read.
    /// </summary>
    /// <param name="name">The case name.</param>
    /// <param name="document">The document's bytes.</param>
    /// <param name="expected">The title text the current implementation produces.</param>
    /// <remarks>
    ///     This drives <c>GetStreamBytes</c> down its <c>CopyTo</c> arm, which §2.19 records as never
    ///     executed. The answers must not depend on how the bytes arrive — and after the rewrite
    ///     replaces a full drain with a bounded head read, this is the arm most likely to disagree.
    /// </remarks>
    [TestMethod]
    [DynamicData(nameof(Matrix))]
    public void EveryEncodingShape_DecodesTheSameFromANonSeekableStream(string name, byte[] document, string expected)
    {
        using DripStream stream = new(document);
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        TitleOf(navigator).ShouldBe(expected, $"{name}, one byte per read");
    }

    /// <summary>
    /// Row 14 — an explicit encoding overrides the declaration, but a byte-order mark still wins.
    /// </summary>
    /// <remarks>
    ///     <c>StreamReader</c>'s two-argument constructor forwards
    ///     <c>detectEncodingFromByteOrderMarks: true</c>, so the mark takes precedence over the
    ///     encoding the caller passed. That is easy to read as a bug and is not one; it is pinned here
    ///     so that a rewrite passing the flag explicitly does not "correct" it.
    /// </remarks>
    [TestMethod]
    public void AnExplicitEncoding_OverridesTheDeclarationButNotAByteOrderMark()
    {
        byte[] latin1 = Latin1().GetBytes(Document("utf-8", LatinPayload));
        using (MemoryStream stream = new(latin1, writable: false))
        {
            TitleOf(SyndicationEncodingUtility.CreateSafeNavigator(stream, Latin1()))
                .ShouldBe(LatinPayload, "an explicit Latin-1 beats a lying utf-8 declaration");
        }

        byte[] utf16WithMark = [.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes(Document("utf-16", RichPayload))];
        using (MemoryStream stream = new(utf16WithMark, writable: false))
        {
            TitleOf(SyndicationEncodingUtility.CreateSafeNavigator(stream, Encoding.UTF8))
                .ShouldBe(RichPayload, "the byte-order mark beats the explicitly supplied UTF-8");
        }
    }

    /// <summary>
    /// The sniff answers correctly when the byte-order mark and the declaration disagree.
    /// </summary>
    /// <param name="name">The case name.</param>
    /// <param name="document">The document's bytes.</param>
    /// <param name="expectedWebName">The encoding name the sniff must return.</param>
    /// <remarks>
    ///     <para>
    ///     <b>Asserted against <c>GetXmlEncoding(byte[])</c>, deliberately not through
    ///     <c>CreateSafeNavigator</c>.</b> The navigator hands the sniffed encoding to a
    ///     <see cref="StreamReader"/> built with byte-order-mark detection on, which would silently
    ///     correct a wrong sniff — so a test routed through it cannot see the defect it is looking for.
    ///     </para>
    ///     <para>
    ///     Every pre-existing byte-order-mark test in this repository declares an encoding that
    ///     <i>matches its own mark</i>, so a decoder that forgot to strip the mark before applying the
    ///     declaration regex would return the right answer for the wrong reason and pass all of them.
    ///     These are the cells where the two disagree, in both the under- and over-512-byte forms so
    ///     that the bounded probe and the whole-document fallback are both exercised.
    ///     </para>
    ///     <para>
    ///     <b>Perturbation-checked, and the result narrows what these rows are worth.</b> Setting the
    ///     sniff's <c>detectEncodingFromByteOrderMarks</c> to <see langword="false"/> turns exactly one
    ///     row red: <c>utf-16 BE mark, declared iso-8859-1, past the probe window</c>. The UTF-8 rows
    ///     survive it, because <see cref="StreamReader"/> strips the preamble of the encoding it was
    ///     <i>given</i> whatever that flag says — the flag only governs switching to a different
    ///     encoding. So it is the <b>UTF-16 cells alone</b> that hold byte-order-mark detection in place
    ///     here, and deleting them as redundant would silently remove the only guard there is.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DynamicData(nameof(MarkAndDeclarationDisagreements))]
    public void WhenTheMarkAndTheDeclarationDisagree_TheSniffStillAnswersCorrectly(
        string name, byte[] document, string expectedWebName)
        => SyndicationEncodingUtility.GetXmlEncoding(document).WebName.ShouldBe(expectedWebName, name);

    /// <summary>
    /// Gets the cells where the byte-order mark and the declared encoding disagree.
    /// </summary>
    public static IEnumerable<object[]> MarkAndDeclarationDisagreements
    {
        get
        {
            foreach (bool overrunsProbe in new[] { false, true })
            {
                string padding = overrunsProbe ? new string(' ', 700) : string.Empty;
                string suffix = overrunsProbe ? ", past the probe window" : ", inside the probe window";

                // A UTF-8 mark in front of a declaration naming something else. The mark must be
                // stripped before the regex runs, or the declaration is never seen.
                yield return
                [
                    "utf-8 mark, declared iso-8859-1" + suffix,
                    (byte[])[.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes($"<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><r>{padding}</r>")],
                    "iso-8859-1",
                ];

                yield return
                [
                    "utf-8 mark, declared us-ascii" + suffix,
                    (byte[])[.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes($"<?xml version=\"1.0\" encoding=\"us-ascii\"?><r>{padding}</r>")],
                    "us-ascii",
                ];

                // A UTF-16 mark: the declaration is UTF-16 encoded, so it is only legible at all if the
                // mark was honoured when decoding the window.
                yield return
                [
                    "utf-16 LE mark, declared utf-8" + suffix,
                    (byte[])[.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes($"<?xml version=\"1.0\" encoding=\"utf-8\"?><r>{padding}</r>")],
                    "utf-8",
                ];

                yield return
                [
                    "utf-16 BE mark, declared iso-8859-1" + suffix,
                    (byte[])[.. Encoding.BigEndianUnicode.GetPreamble(), .. Encoding.BigEndianUnicode.GetBytes($"<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><r>{padding}</r>")],
                    "iso-8859-1",
                ];

                // No mark at all, so nothing to strip - the control that says the rows above are about
                // the mark rather than about the padding.
                yield return
                [
                    "no mark, declared iso-8859-1" + suffix,
                    Encoding.ASCII.GetBytes($"<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><r>{padding}</r>"),
                    "iso-8859-1",
                ];
            }
        }
    }

    /// <summary>
    /// Row 6's premise, asserted rather than assumed.
    /// </summary>
    /// <remarks>
    ///     Row 6 pins a <i>degrade</i>: a document declaring <c>windows-1252</c> decodes as UTF-8,
    ///     because no code-page provider is registered and <c>EncodingFromMatch</c> catches the
    ///     <see cref="ArgumentException"/> and falls back. That is process-global state which any
    ///     dependency could change by calling <c>Encoding.RegisterProvider</c>, so the premise is
    ///     asserted here and the row fails for the right reason if it ever stops holding.
    /// </remarks>
    [TestMethod]
    public void WindowsCodePagesAreNotRegistered_WhichIsWhyRowSixDegrades()
        => Should.Throw<ArgumentException>(() => Encoding.GetEncoding("windows-1252"));

    private static IEnumerable<(string Name, byte[] Document, string ExpectedTitle)> Cases()
    {
        byte[] utf8Mark = Encoding.UTF8.GetPreamble();
        byte[] utf16LeMark = Encoding.Unicode.GetPreamble();
        byte[] utf16BeMark = Encoding.BigEndianUnicode.GetPreamble();

        // 1-4. UTF-8, with and without a mark, declared and undeclared. The baseline: all four agree.
        yield return ("1. utf-8, no mark, declared", Encoding.UTF8.GetBytes(Document("utf-8", RichPayload)), RichPayload);
        yield return ("2. utf-8, mark, declared", [.. utf8Mark, .. Encoding.UTF8.GetBytes(Document("utf-8", RichPayload))], RichPayload);
        yield return ("3. utf-8, no mark, undeclared", Encoding.UTF8.GetBytes(Document(null, RichPayload)), RichPayload);
        yield return ("4. utf-8, mark, undeclared", [.. utf8Mark, .. Encoding.UTF8.GetBytes(Document(null, RichPayload))], RichPayload);

        // 5. Latin-1 is built in on .NET 10, so this one genuinely decodes rather than degrading.
        yield return ("5. iso-8859-1 bytes, declared", Latin1().GetBytes(Document("iso-8859-1", LatinPayload)), LatinPayload);

        // 6. windows-1252 is NOT built in. EncodingFromMatch catches the ArgumentException and returns
        //    UTF-8, so byte 0x92 - a curly apostrophe in that code page - is not valid UTF-8 on its own
        //    and decodes to the replacement character. This row pins the degrade; it does not exercise
        //    a windows-1252 decode, and cannot, which is why it is worth writing down.
        yield return (
            "6. declared windows-1252, body has 0x92",
            [
                .. Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"windows-1252\"?><r><title>it"),
                0x92,
                .. Encoding.ASCII.GetBytes("s</title></r>"),
            ],
            "it�s");

        // 7-8. UTF-16 in both byte orders, mark present and declaration agreeing.
        yield return ("7. utf-16 LE, mark, declared", [.. utf16LeMark, .. Encoding.Unicode.GetBytes(Document("utf-16", RichPayload))], RichPayload);
        yield return ("8. utf-16 BE, mark, declared", [.. utf16BeMark, .. Encoding.BigEndianUnicode.GetBytes(Document("utf-16", RichPayload))], RichPayload);

        // 9-10. Precedence: the mark against a declaration that contradicts it, in both directions.
        yield return ("9. utf-16 LE mark, declared utf-8", [.. utf16LeMark, .. Encoding.Unicode.GetBytes(Document("utf-8", RichPayload))], RichPayload);
        yield return ("10. utf-8 mark, declared iso-8859-1", [.. utf8Mark, .. Encoding.UTF8.GetBytes(Document("iso-8859-1", LatinPayload))], LatinPayload);

        // 11. UTF-16 with no mark. The sniff reads the head as UTF-8, where the interleaved NUL bytes
        //     stop the declaration regex matching, so the document decodes as UTF-8: every ASCII
        //     character survives, its NUL is stripped by the sanitiser, and every non-ASCII character
        //     becomes a replacement character. THIS IS WRONG AND IS PINNED AS WRONG. Do not "fix" it
        //     here - it is a behaviour change with its own blast radius, and pinning it is what stops
        //     the rewrite fixing it by accident and calling that a refactor.
        yield return (
            "11. utf-16 LE, no mark, declared utf-16",
            Encoding.Unicode.GetBytes(Document("utf-16", LatinPayload)),
            "caf�");

        // 12. An encoding name nothing can resolve must still parse. This is the row that rules out
        //     handing the raw Stream to XmlReader.Create, which validates the declared name and throws.
        yield return ("12. declared invalid-encoding-name", Encoding.UTF8.GetBytes(Document("invalid-encoding-name", "plain")), "plain");

        // 13. A document that declares utf-8 truthfully and then contains a byte that is not valid
        //     UTF-8. StreamReader's replacement fallback yields U+FFFD; it must not throw. This is the
        //     second reason the decode cannot be handed to XmlReader.Create over a raw Stream, which
        //     decodes declared-utf-8 input with throwOnInvalidBytes: true.
        yield return (
            "13. declared utf-8, body has invalid utf-8",
            [
                .. Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?><r><title>caf"),
                0xE9,
                .. Encoding.ASCII.GetBytes("</title></r>"),
            ],
            "caf�");

        // 15a. A declaration padded past the 512-byte probe window. The bounded sniff must fall back to
        //      reading the document whole, or this silently becomes utf-8 and the accents break.
        yield return (
            "15a. declaration padded past the probe window",
            Latin1().GetBytes($"<?xml version=\"1.0\"{new string(' ', 600)}encoding=\"iso-8859-1\"?><r><title>{LatinPayload}</title></r>"),
            LatinPayload);

        // 15b. The declaration's `encoding=` sits INSIDE the 512-byte window but its `?>` does not.
        //      The regex requires the closing `?>`, so the fast path fails to match even though the
        //      encoding is right there - and a growth rule phrased as "grow only when `encoding=` is
        //      absent" would stop early and answer utf-8. Row 15a cannot catch that: it pads before
        //      `encoding=`, so both rules behave the same on it.
        yield return (
            "15b. encoding inside the window, close tag outside it",
            Latin1().GetBytes($"<?xml version=\"1.0\" encoding=\"iso-8859-1\"{new string(' ', 500)}?><r><title>{LatinPayload}</title></r>"),
            LatinPayload);
    }

    private static string Document(string? declaredEncoding, string payload)
    {
        string declaration = declaredEncoding is null
            ? "<?xml version=\"1.0\"?>"
            : $"<?xml version=\"1.0\" encoding=\"{declaredEncoding}\"?>";

        return $"{declaration}<r><title>{payload}</title></r>";
    }

    private static Encoding Latin1() => Encoding.Latin1;

    private static string TitleOf(XPathNavigator navigator)
        => navigator.SelectSingleNode("//title")?.Value ?? throw new InvalidOperationException("no title element");
}