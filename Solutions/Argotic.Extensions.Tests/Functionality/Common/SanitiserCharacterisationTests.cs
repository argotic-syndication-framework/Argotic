namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins exactly which characters the sanitiser drops, and where, through all four entry points.
/// </summary>
/// <remarks>
///     <para>
///     Fourteen tests exercised this method before today and all of them passed short, clean-ish
///     strings through the <c>string</c> overload. §2.19 records what that left: the rebuild loop's
///     surrogate arm at <b>zero hits</b>, and the scan's end-of-input operand never once false. So a
///     rewrite that reads one past the end, or drops a valid astral character, or truncates on a run
///     of dropped characters, passes every one of those fourteen.
///     </para>
///     <para>
///     All four entry points, because today three of them funnel into the fourth and after the rewrite
///     none of them will.
///     </para>
/// </remarks>
[TestClass]
public sealed class SanitiserCharacterisationTests
{
    /// <summary>
    /// Gets rows 16 to 23: the document, the markup the character entry points produce, and the markup
    /// the byte entry points produce.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>The last two columns are not always equal, and that is the finding.</b> A lone surrogate
    ///     cannot be encoded, so <c>Encoding.UTF8.GetBytes</c> substitutes <c>U+FFFD</c> before Argotic
    ///     sees anything — and <c>U+FFFD</c> is a perfectly valid XML character, so the sanitiser keeps
    ///     it. The same document yields <c>&lt;r&gt;ab&lt;/r&gt;</c> handed over as a string and
    ///     <c>&lt;r&gt;a\uFFFDb&lt;/r&gt;</c> handed over as bytes.
    ///     </para>
    ///     <para>
    ///     Argotic causes neither answer and neither is wrong. But one expected value per row would
    ///     have hidden the difference, and after the rewrite the byte path decodes incrementally rather
    ///     than through a single <c>GetString</c> — so this is exactly the column that could move
    ///     without anyone deciding it should.
    ///     </para>
    /// </remarks>
    public static IEnumerable<object[]> DirtyDocuments =>
    [
        // 16-18. A C0 control in each position markup can put one. Controls encode fine, so both
        //        columns agree throughout this group.
        ["16. control in element text", "<r>a\u0001b</r>", "<r>ab</r>", "<r>ab</r>"],
        ["17. control in an element name", "<ro\u0001ot>x</ro\u0001ot>", "<root>x</root>", "<root>x</root>"],
        ["18. control in an attribute value", "<r a=\"x\u0001y\">z</r>", "<r a=\"xy\">z</r>", "<r a=\"xy\">z</r>"],
        ["18b. control in an attribute name", "<r a\u0001b=\"x\">z</r>", "<r ab=\"x\">z</r>", "<r ab=\"x\">z</r>"],

        // 19-20. Inside CDATA, and the two non-characters. Both are encodable, so they reach the
        //        sanitiser intact down either path and are dropped by both.
        ["19. control inside CDATA", "<r><![CDATA[a\u0001b]]></r>", "<r>ab</r>", "<r>ab</r>"],
        ["20. U+FFFE", "<r>a\uFFFEb</r>", "<r>ab</r>", "<r>ab</r>"],
        ["20b. U+FFFF", "<r>a\uFFFFb</r>", "<r>ab</r>", "<r>ab</r>"],

        // 21. Every surrogate arrangement. This is where the two columns part company.
        ["21. lone high surrogate", "<r>a\uD800b</r>", "<r>ab</r>", "<r>a\uFFFDb</r>"],
        ["21b. lone low surrogate", "<r>a\uDC00b</r>", "<r>ab</r>", "<r>a\uFFFDb</r>"],
        ["21c. valid pair", "<r>a\U0001F600b</r>", "<r>a\U0001F600b</r>", "<r>a\U0001F600b</r>"],
        ["21d. valid pair then a trailing lone high", "<r>\U0001F600\uD800</r>", "<r>\U0001F600</r>", "<r>\U0001F600\uFFFD</r>"],
        ["21e. invalid then a valid pair", "<r>\u0001\U0001F600</r>", "<r>\U0001F600</r>", "<r>\U0001F600</r>"],
        ["21f. two high surrogates in a row", "<r>a\uD800\uD800b</r>", "<r>ab</r>", "<r>a\uFFFD\uFFFDb</r>"],

        // 23. A run of dropped characters longer than any plausible read buffer. A streaming filter
        //     that returned zero characters here rather than reading again would truncate the document.
        ["23. twenty thousand NULs", "<r>" + Nuls(20_000) + "x</r>", "<r>x</r>", "<r>x</r>"],
    ];

    private static readonly string[] EntryPoints = ["string", "reader", "drip-reader", "stream"];

    private static readonly string[] BoundaryPayloads = ["\U0001F600", "\u0001", "\uD800"];

    /// <summary>
    /// Gets rows 24 and 25: one case per (entry point, payload), each sweeping the boundary internally.
    /// </summary>
    public static IEnumerable<object[]> BoundarySweeps =>
        from entryPoint in EntryPoints
        from payload in BoundaryPayloads
        select new object[] { entryPoint, payload };

    /// <summary>
    /// Rows 16 to 23, through every entry point, against a pinned document.
    /// </summary>
    /// <param name="name">The case name.</param>
    /// <param name="document">The document as supplied.</param>
    /// <param name="expectedFromCharacters">The markup the <c>string</c> and <c>TextReader</c> entry points produce.</param>
    /// <param name="expectedFromBytes">The markup the two <c>Stream</c> entry points produce.</param>
    [TestMethod]
    [DynamicData(nameof(DirtyDocuments))]
    public void EveryDirtyShape_SanitisesToThePinnedDocument(
        string name,
        string document,
        string expectedFromCharacters,
        string expectedFromBytes)
    {
        RootOf(SyndicationEncodingUtility.CreateSafeNavigator(document))
            .ShouldBe(expectedFromCharacters, $"{name}, string");

        using (StringReader reader = new(document))
        {
            RootOf(SyndicationEncodingUtility.CreateSafeNavigator(reader))
                .ShouldBe(expectedFromCharacters, $"{name}, reader");
        }

        byte[] utf8 = Encoding.UTF8.GetBytes(document);
        using (MemoryStream stream = new(utf8, writable: false))
        {
            RootOf(SyndicationEncodingUtility.CreateSafeNavigator(stream))
                .ShouldBe(expectedFromBytes, $"{name}, stream");
        }

        using (MemoryStream stream = new(utf8, writable: false))
        {
            RootOf(SyndicationEncodingUtility.CreateSafeNavigator(stream, Encoding.UTF8))
                .ShouldBe(expectedFromBytes, $"{name}, stream+encoding");
        }
    }

    /// <summary>
    /// Every code unit in the plane is kept or dropped exactly as <see cref="XmlConvert.IsXmlChar(char)"/> says.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     65,536 cases, and it runs in milliseconds. This is the proof obligation for replacing the
    ///     character-by-character scan with a precomputed set: the two must agree everywhere, not on
    ///     the fourteen inputs that happened to have tests.
    ///     </para>
    ///     <para>
    ///     Asserted through the public method rather than against the set itself. The set is private
    ///     and <c>Argotic.Common</c> grants no <c>InternalsVisibleTo</c> to this project — but that
    ///     constraint produces the better test anyway, because it pins the <i>behaviour</i> the
    ///     equivalence is supposed to preserve rather than the data structure that happens to
    ///     implement it today.
    ///     </para>
    ///     <para>
    ///     Surrogates need no special case. Each one is unpaired in <c>a{c}b</c>, so the pair rule
    ///     declines it and it is dropped — which is exactly what <c>IsXmlChar</c> says about it.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void EveryCodeUnitInThePlane_IsKeptOrDroppedExactlyAsXmlConvertSays()
    {
        for (int codeUnit = 0; codeUnit <= 0xFFFF; codeUnit++)
        {
            char character = (char)codeUnit;
            string sanitised = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters($"a{character}b");

            sanitised.ShouldBe(
                XmlConvert.IsXmlChar(character) ? $"a{character}b" : "ab",
                $"U+{codeUnit:X4}");
        }
    }

    /// <summary>
    /// A valid astral character early in a document does not stop the search being vectorised.
    /// </summary>
    /// <remarks>
    ///     The precomputed set contains every surrogate, so an emoji is a candidate hit. If the scan
    ///     responded by abandoning the vectorised search and walking the rest of the document one
    ///     character at a time, a feed with an emoji in its first title would pay scalar cost for its
    ///     whole body. This asserts the answer is unaffected; the cost is covered by the astral arm of
    ///     <c>SanitiserShapeBenchmarks</c>.
    /// </remarks>
    [TestMethod]
    public void AnAstralCharacterEarlyInADocument_DoesNotChangeTheAnswerForWhatFollows()
    {
        string clean = string.Concat("<r>\U0001F600", new string('a', 20_000), "</r>");
        RootOf(SyndicationEncodingUtility.CreateSafeNavigator(clean)).ShouldBe(clean);

        string dirtyAtTheEnd = string.Concat("<r>\U0001F600", new string('a', 20_000), "\u0001", "</r>");
        string expected = string.Concat("<r>\U0001F600", new string('a', 20_000), "</r>");
        RootOf(SyndicationEncodingUtility.CreateSafeNavigator(dirtyAtTheEnd)).ShouldBe(expected);
    }

    /// <summary>
    /// Row 22 — a numeric character reference to an invalid character is not the sanitiser's business.
    /// </summary>
    /// <remarks>
    ///     The sanitiser sees the literal text <c>&amp;#1;</c>, which contains nothing invalid, so the
    ///     document reaches the parser intact and the parser rejects the reference. Worth pinning
    ///     precisely because it looks like something the sanitiser should have caught: a rewrite tempted
    ///     to resolve entity references while filtering would turn this throw into a silent drop.
    /// </remarks>
    [TestMethod]
    public void ANumericReferenceToAnInvalidCharacter_StillThrows()
        => Should.Throw<XmlException>(() => SyndicationEncodingUtility.CreateSafeNavigator("<r>&#1;</r>"));

    /// <summary>
    /// An unpaired surrogate is dropped, and its second character is examined without being consumed.
    /// </summary>
    /// <remarks>
    ///     Settles two things the plan left open. <c>XmlConvert.IsXmlSurrogatePair</c> <b>returns
    ///     false</b> rather than throwing when the high surrogate is real and the low one is not, so the
    ///     current scan is total over every input; and an unpaired surrogate really is dropped rather
    ///     than passed through, which the byte-path column cannot show because the encoder gets there
    ///     first.
    /// </remarks>
    [TestMethod]
    public void AnUnpairedSurrogate_IsDroppedAndDoesNotThrow()
    {
        XmlConvert.IsXmlChar('\uD800').ShouldBeFalse();
        XmlConvert.IsXmlChar('\uFFFD').ShouldBeTrue("U+FFFD is valid XML, which is why the byte path keeps it");
        XmlConvert.IsXmlSurrogatePair('b', '\uD800').ShouldBeFalse("a real high surrogate with a non-low partner returns false, it does not throw");

        SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters("<r>a\uD800b</r>").ShouldBe("<r>ab</r>");
    }

    /// <summary>
    /// Row 21g — a trailing lone surrogate loads as a string and <b>throws</b> as bytes.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The sharpest form of the asymmetry, and the reason it is worth a test of its own rather than
    ///     a row with a magic marker in it. As characters, the surrogate is invalid XML, the sanitiser
    ///     drops it, and the document is well formed. As bytes, the encoder has already turned it into
    ///     <c>U+FFFD</c> — which is <i>valid</i> XML, so the sanitiser keeps it, and it becomes
    ///     character data after the root element closes. That is not well formed, and the parser says
    ///     so.
    ///     </para>
    ///     <para>
    ///     Nothing here is Argotic's doing and there is nothing to fix. It is pinned because the byte
    ///     path is about to stop decoding through a single <c>GetString</c> and start decoding
    ///     incrementally, and "a document that used to throw now parses" is precisely the kind of
    ///     change that gets waved through as an improvement.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ATrailingLoneSurrogate_LoadsAsCharactersAndThrowsAsBytes()
    {
        const string Document = "<r>ab</r>\uD800";

        RootOf(SyndicationEncodingUtility.CreateSafeNavigator(Document)).ShouldBe("<r>ab</r>");

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Document), writable: false);
        Should.Throw<XmlException>(() => SyndicationEncodingUtility.CreateSafeNavigator(stream))
            .Message.ShouldContain("root level");
    }

    /// <summary>
    /// Rows 24 and 25 — a dirty character swept across every plausible buffer boundary.
    /// </summary>
    /// <param name="entryPoint">The entry point under test.</param>
    /// <param name="payload">The character sequence placed at the boundary.</param>
    /// <remarks>
    ///     The offsets bracket 1,024, 4,096 and 8,192 rather than naming one of them. Hard-coding a
    ///     single buffer size would test the implementation's current arithmetic rather than the
    ///     property that no arithmetic may matter.
    /// </remarks>
    [TestMethod]
    [DynamicData(nameof(BoundarySweeps))]
    public void ADirtyCharacterAtEveryBufferBoundary_SanitisesIdentically(string entryPoint, string payload)
    {
        string expectedTail = payload switch
        {
            "\U0001F600" => payload,

            // The byte entry point never sees the surrogate: the encoder replaced it before Argotic
            // was involved, and U+FFFD survives sanitising. See the DirtyDocuments remarks.
            "\uD800" when entryPoint == "stream" => "\uFFFD",
            _ => string.Empty,
        };

        foreach (int n in Offsets())
        {
            string document = string.Concat("<r>", new string('a', n), payload, "</r>");
            string expected = string.Concat("<r>", new string('a', n), expectedTail, "</r>");

            RootOf(Parse(entryPoint, document)).ShouldBe(expected, $"{entryPoint}, offset {n}");
        }
    }

    /// <summary>
    /// Row 26 — a read-block boundary that divides a surrogate pair does not divide the character.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The sanitising reader classifies a pair when it sees the high half, and when the caller's
    ///     buffer fills between the halves the approved low half is held to the next call and emitted
    ///     <i>verbatim</i>. Sending it through classification again would find a low surrogate with no
    ///     high before it, drop it as unpaired, and hand the parser a lone high surrogate — a parse
    ///     error manufactured out of a valid emoji.
    ///     </para>
    ///     <para>
    ///     Rows 24 and 25 already catch this, but only because their offset brackets happen to
    ///     straddle today's block size — measured by perturbing the reader to reclassify a held low
    ///     half at the start of each read call, which turned exactly the four astral sweep cases red
    ///     (issue #176 records the experiment). This row removes the dependency on any particular
    ///     block size: the run is longer than any plausible read block, so every block length puts
    ///     boundaries inside it, and of two runs that start one position apart, one must place a
    ///     boundary between the halves of some pair whatever that length is.
    ///     </para>
    ///     <para>
    ///     Full string equality, not length: equality also finds a dropped half, a substituted
    ///     replacement character, and a reordering.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ASurrogatePairDividedByAReadBlockBoundary_SurvivesIntact()
    {
        string run = string.Concat(Enumerable.Repeat("\U0001F600", 20_000));
        string[] alignments = ["", "x"];

        foreach (string prefix in alignments)
        {
            string document = string.Concat("<r>", prefix, run, "</r>");

            foreach (string entryPoint in EntryPoints)
            {
                RootOf(Parse(entryPoint, document))
                    .ShouldBe(document, $"{entryPoint}, run at offset {prefix.Length}");
            }
        }
    }

    private static IEnumerable<int> Offsets()
    {
        for (int n = 1_000; n <= 1_050; n++)
        {
            yield return n;
        }

        for (int n = 4_080; n <= 4_110; n++)
        {
            yield return n;
        }

        for (int n = 8_180; n <= 8_200; n++)
        {
            yield return n;
        }
    }

    private static XPathNavigator Parse(string entryPoint, string document)
    {
        switch (entryPoint)
        {
            case "string":
                return SyndicationEncodingUtility.CreateSafeNavigator(document);

            case "reader":
                using (StringReader reader = new(document))
                {
                    return SyndicationEncodingUtility.CreateSafeNavigator(reader);
                }

            case "drip-reader":
                using (DripTextReader reader = new(document))
                {
                    XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(reader);
                    reader.DisposeCount.ShouldBe(0, "nothing should dispose a reader it did not open");
                    return navigator;
                }

            default:
                using (MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false))
                {
                    return SyndicationEncodingUtility.CreateSafeNavigator(stream);
                }
        }
    }

    private static string Nuls(int count) => new('\0', count);

    private static string RootOf(XPathNavigator navigator)
    {
        navigator.MoveToRoot();
        return navigator.OuterXml;
    }
}