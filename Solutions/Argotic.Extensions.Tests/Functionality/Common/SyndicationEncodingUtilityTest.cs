namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers <c>SyndicationEncodingUtility</c>: the removal of characters XML forbids, the detection of a
/// document's declared encoding from text and from bytes, the sanitising navigator factory and its
/// entity policy, and the base-64, HTML-escape and directory-name helpers.
/// </summary>
[TestClass]
public class SyndicationEncodingUtilityTest
{
    #region RemoveInvalidXmlHexadecimalCharacters Tests

    /// <summary>
    /// Code units XML forbids are stripped and everything else is returned untouched, astral characters
    /// such as <c>😀</c> included.
    /// </summary>
    /// <param name="input">A non-empty string, which may contain code units that are not valid XML characters.</param>
    /// <param name="expected">
    ///     The string with those code units removed; identical to <paramref name="input"/> when it holds
    ///     nothing invalid.
    /// </param>
    [TestMethod]
    [DataRow("a", "a")]
    [DataRow("@±あ😀", "@±あ😀")] // Emoji should not be stripped
    [DataRow("a\uFFFEb", "ab")] // FFFE should be stripped
    public void RemoveInvalidXmlHexadecimalCharactersTest(string input, string expected)
    {
        string stripped = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);
        stripped.ShouldBe(expected);
    }

    /// <summary>
    /// An embedded U+0000 is removed, joining the text on either side of it.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_RemovesNullCharacter()
    {
        // Arrange
        string input = "Hello\0World";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe("HelloWorld");
    }

    /// <summary>
    /// Every control character from U+0001 to U+0008 is removed.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_RemovesControlCharacters_0x01_To_0x08()
    {
        // Arrange - Using \u escape sequences for proper 4-digit hex codes
        string input = "A\u0001B\u0002C\u0003D\u0004E\u0005F\u0006G\u0007H\u0008I";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe("ABCDEFGHI");
    }

    /// <summary>
    /// Tab, line feed and carriage return — the three control characters XML permits — are left in place.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_PreservesValidWhitespace_Tab_LF_CR()
    {
        // Arrange - Tab (0x09), Line Feed (0x0A), Carriage Return (0x0D) are valid
        string input = "Line1\tTabbed\nLine2\rLine3";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe(input);
    }

    /// <summary>
    /// Vertical tab (U+000B) and form feed (U+000C) are removed, though they sit between the permitted
    /// whitespace characters.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_RemovesVerticalTab_And_FormFeed()
    {
        // Arrange - Vertical Tab (0x0B) and Form Feed (0x0C) are invalid
        string input = "A\u000BB\u000CC";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe("ABC");
    }

    /// <summary>
    /// Every control character from U+000E to U+001F is removed.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_RemovesControlCharacters_0x0E_To_0x1F()
    {
        // Arrange - Using \u escape sequences for proper 4-digit hex codes
        string input = "Start\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001FEnd";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe("StartEnd");
    }

    /// <summary>
    /// U+FFFF is removed.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_RemovesFFFF()
    {
        // Arrange
        string input = "Test\uFFFFValue";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe("TestValue");
    }

    /// <summary>
    /// Japanese, Greek and Arabic text and emoji all survive unchanged.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_PreservesValidUnicodeCharacters()
    {
        // Arrange - Various valid Unicode ranges
        string input = "Hello World! 日本語 Ελληνικά العربية 🎉🚀";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe(input);
    }

    /// <summary>
    /// Private Use Area characters, U+E000 and U+F8FF among them, are valid XML and survive unchanged.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_PreservesPrivateUseArea()
    {
        // Arrange - Private Use Area characters (E000-F8FF) are valid
        string input = "Text\uE000\uF8FFEnd";

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe(input);
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_HandlesEmptyStringException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(string.Empty));
    }

    /// <summary>
    /// A <see langword="null"/> string throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_HandlesNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(null!));
    }

    /// <summary>
    /// A well-formed surrogate pair survives whole, though neither half is a valid XML character alone.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_PreservesSurrogatePairs()
    {
        // Arrange - Emoji and other characters outside BMP use surrogate pairs
        string input = "Test\U0001F600Smile"; // U+1F600 = 😀

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe(input);
    }

    /// <summary>
    /// Consecutive surrogate pairs all survive, the scan keeping its place from one pair to the next.
    /// </summary>
    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_HandlesMultipleSurrogatePairs()
    {
        // Arrange - Multiple supplementary characters
        string input = "\U0001F600\U0001F601\U0001F602"; // 😀😁😂

        // Act
        string result = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);

        // Assert
        result.ShouldBe(input);
    }

    #endregion

    #region GetXmlEncoding (DetectXmlEncoding) Tests

    /// <summary>
    /// A declaration naming <c>utf-8</c> yields <see cref="Encoding.UTF8"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DetectsUtf8Encoding_FromDeclaration()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="utf-8"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// A declaration naming <c>utf-16</c> yields an encoding whose web name is <c>utf-16</c>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DetectsUtf16Encoding_FromDeclaration()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="utf-16"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe("utf-16");
    }

    /// <summary>
    /// A declaration naming <c>iso-8859-1</c> yields an encoding whose web name is <c>iso-8859-1</c>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DetectsIso88591Encoding_FromDeclaration()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="iso-8859-1"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe("iso-8859-1");
    }

    /// <summary>
    /// A document with no XML declaration at all falls back to <see cref="Encoding.UTF8"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DefaultsToUtf8_WhenNoDeclaration()
    {
        // Arrange
        string xml = "<root>content without xml declaration</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// A declaration carrying only <c>version</c> falls back to <see cref="Encoding.UTF8"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DefaultsToUtf8_WhenDeclarationHasNoEncoding()
    {
        // Arrange
        string xml = """<?xml version="1.0"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// A declaration naming <c>us-ascii</c> yields an encoding whose web name is <c>us-ascii</c>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DetectsUsAsciiEncoding()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="us-ascii"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe("us-ascii");
    }

    /// <summary>
    /// A single-quoted declaration naming an encoding that is NOT the fallback.
    /// </summary>
    /// <remarks>
    ///     This test used to declare <c>utf-8</c> and assert <see cref="Encoding.UTF8"/>. UTF-8 is also
    ///     what this method returns when it cannot determine an encoding at all — so the assertion held
    ///     whether single quotes were supported or not, and they were not. A test named for a capability
    ///     is not a test of it; the declared encoding has to differ from the fallback or the two
    ///     outcomes are indistinguishable.
    ///     <para>
    ///     XML permits either quote character around a declaration's value, and real publishers use
    ///     both: ten of the 136 documents in the real-world corpus are single-quoted, among them arXiv,
    ///     Blogger, LiveJournal and Tim Bray's <i>ongoing</i>.
    ///     </para>
    /// </remarks>
    /// <param name="declared">
    ///     The encoding name to write into the declaration. It must be one the runtime knows and must
    ///     not be UTF-8, which is also the fallback.
    /// </param>
    [TestMethod]
    [DataRow("iso-8859-1")]
    [DataRow("utf-16")]
    [DataRow("us-ascii")]
    public void GetXmlEncoding_HandlesEncodingWithSingleQuotes(string declared)
    {
        // Arrange
        string xml = $"<?xml version='1.0' encoding='{declared}'?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe(declared, "XML allows either quote character around a declaration value");
    }

    /// <summary>
    /// Both quote spellings of the same declaration name the same encoding.
    /// </summary>
    /// <remarks>
    ///     The pairing is what makes this hard to satisfy accidentally: any answer that differs between
    ///     the two spellings is wrong, whichever one is wrong.
    /// </remarks>
    /// <param name="declared">
    ///     The encoding name to write into both declarations. Any name the runtime knows will do here,
    ///     UTF-8 included, because the comparison is between the two spellings rather than against a
    ///     fixed answer.
    /// </param>
    [TestMethod]
    [DataRow("iso-8859-1")]
    [DataRow("utf-16")]
    [DataRow("utf-8")]
    public void GetXmlEncoding_AgreesAcrossBothQuoteSpellings(string declared)
    {
        // Arrange
        string single = $"<?xml version='1.0' encoding='{declared}'?><root>content</root>";
        string @double = $"""<?xml version="1.0" encoding="{declared}"?><root>content</root>""";

        // Act
        Encoding fromSingle = SyndicationEncodingUtility.GetXmlEncoding(single);
        Encoding fromDouble = SyndicationEncodingUtility.GetXmlEncoding(@double);

        // Assert
        fromSingle.WebName.ShouldBe(fromDouble.WebName);
    }

    /// <summary>
    /// A single-quoted non-UTF-8 document decodes to the characters it actually contains.
    /// </summary>
    /// <remarks>
    ///     The consequence, stated end to end rather than as an encoding name. Falling back to UTF-8 for
    ///     a document that declared ISO-8859-1 does not throw and does not look wrong from inside the
    ///     parser — it silently yields replacement characters where the accented letters were.
    /// </remarks>
    [TestMethod]
    public void ASingleQuotedLatin1Document_DecodesItsAccentedCharacters()
    {
        // Arrange
        byte[] document = Encoding.Latin1.GetBytes(
            "<?xml version='1.0' encoding='iso-8859-1'?><root><title>café naïve</title></root>");

        // Act
        using MemoryStream stream = new(document, false);
        System.Xml.XPath.XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        // Assert
        navigator.SelectSingleNode("//title")!.Value.ShouldBe("café naïve");
    }

    /// <summary>
    /// Whitespace around the <c>encoding</c> pseudo-attribute and its equals sign does not defeat
    /// detection.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_HandlesEncodingWithExtraSpaces()
    {
        // Arrange
        string xml = """<?xml version="1.0"   encoding  =  "utf-8" ?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// The pseudo-attribute and the encoding name are both read case-insensitively, so
    /// <c>ENCODING="UTF-8"</c> is detected.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_IsCaseInsensitive()
    {
        // Arrange
        string xml = """<?xml version="1.0" ENCODING="UTF-8"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// A declaration naming an encoding the runtime does not know falls back to
    /// <see cref="Encoding.UTF8"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_DefaultsToUtf8_ForInvalidEncodingName()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="invalid-encoding-name"?><root>content</root>""";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// The byte-array overload reads the declaration out of undecoded bytes.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_DetectsEncoding()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="utf-8"?><root>test</root>""";
        byte[] data = Encoding.UTF8.GetBytes(xml);

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(data);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// A <see langword="null"/> string throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_ThrowsOnNullString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding((string)null!));
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_ThrowsOnEmptyString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding(string.Empty));
    }

    /// <summary>
    /// A <see langword="null"/> byte array throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_ThrowsOnNullByteArray()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding((byte[])null!));
    }

    // The byte[] overload only decodes the head of the document rather than all of it. These cases
    // cover the two things that makes possible to get wrong: the byte-order mark, which is consumed
    // before the declaration is readable, and the boundary where the declaration does not fit in
    // the probe window. Both were previously untested - the only positive test for this overload
    // passed a 54-byte document with no BOM, which exercises neither.

    /// <summary>
    /// A UTF-8 byte-order mark ahead of the declaration does not hide it.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_Utf8ByteOrderMark_DetectsEncoding()
    {
        byte[] data = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes("""<?xml version="1.0" encoding="utf-8"?><r/>""")];

        SyndicationEncodingUtility.GetXmlEncoding(data).WebName.ShouldBe("utf-8");
    }

    /// <summary>
    /// A UTF-16 little-endian byte-order mark is consumed and the declared <c>utf-16</c> still read.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_Utf16LittleEndianByteOrderMark_DetectsEncoding()
    {
        byte[] data = [.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes("""<?xml version="1.0" encoding="utf-16"?><r/>""")];

        SyndicationEncodingUtility.GetXmlEncoding(data).WebName.ShouldBe("utf-16");
    }

    /// <summary>
    /// A UTF-16 big-endian byte-order mark is consumed and the declared <c>utf-16</c> still read.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_Utf16BigEndianByteOrderMark_DetectsEncoding()
    {
        byte[] data = [.. Encoding.BigEndianUnicode.GetPreamble(), .. Encoding.BigEndianUnicode.GetBytes("""<?xml version="1.0" encoding="utf-16"?><r/>""")];

        SyndicationEncodingUtility.GetXmlEncoding(data).WebName.ShouldBe("utf-16");
    }

    /// <summary>
    /// A 50 KB document is answered from the declaration at its head, which names <c>iso-8859-1</c>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_DocumentFarLargerThanProbeWindow_DetectsEncoding()
    {
        // The declaration is at the start but the document is 50 KB; only the head should be read.
        byte[] data = Encoding.UTF8.GetBytes(
            """<?xml version="1.0" encoding="iso-8859-1"?><r>""" + new string('x', 50_000) + "</r>");

        SyndicationEncodingUtility.GetXmlEncoding(data).WebName.ShouldBe("iso-8859-1");
    }

    /// <summary>
    /// A legal declaration padded past the 512-byte probe window still yields <c>iso-8859-1</c>, the
    /// bounded fast path falling back rather than answering wrongly.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_DeclarationOverrunsProbeWindow_StillDetectsEncoding()
    {
        // XML permits arbitrary whitespace between the declaration's pseudo-attributes, so a legal
        // declaration can run past the probe window. Nothing does this in practice, but the fast
        // path must not change the answer when it happens.
        byte[] data = Encoding.UTF8.GetBytes(
            """<?xml version="1.0" """ + new string(' ', 600) + """encoding="iso-8859-1"?><r/>""");

        data.Length.ShouldBeGreaterThan(512);
        SyndicationEncodingUtility.GetXmlEncoding(data).WebName.ShouldBe("iso-8859-1");
    }

    /// <summary>
    /// A 50 KB document with no declaration falls back to <see cref="Encoding.UTF8"/>.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_LargeDocumentWithNoDeclaration_DefaultsToUtf8()
    {
        byte[] data = Encoding.UTF8.GetBytes("<r>" + new string('x', 50_000) + "</r>");

        SyndicationEncodingUtility.GetXmlEncoding(data).ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// An empty byte array throws <see cref="ArgumentException"/> — inherited from the string overload's
    /// guard rather than designed, and preserved by the bounded-probe fast path.
    /// </summary>
    [TestMethod]
    public void GetXmlEncoding_FromByteArray_Empty_ThrowsArgumentException()
    {
        // Inherited from the string overload's guard rather than designed, but it is the documented
        // behaviour consumers compile against, and the bounded-probe rewrite must preserve it.
        Should.Throw<ArgumentException>(() => SyndicationEncodingUtility.GetXmlEncoding(Array.Empty<byte>()));
    }

    #endregion

    #region CreateSafeNavigator Tests

    /// <summary>
    /// A well-formed document yields a navigator whose first child element is <c>root</c>.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_CreatesNavigator_FromValidXml()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="utf-8"?><root><child>value</child></root>""";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("root");
    }

    /// <summary>
    /// An XPath select over the returned navigator finds both <c>item</c> elements.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_NavigatesToElements()
    {
        // Arrange
        string xml = "<root><item>first</item><item>second</item></root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        XPathNodeIterator items = navigator.Select("//item");
        items.Count.ShouldBe(2);
    }

    /// <summary>
    /// A control character in element text is stripped before parsing, so a document an XML reader would
    /// reject still loads — and loads without the character.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_RemovesInvalidCharacters_BeforeCreatingNavigator()
    {
        // Arrange - XML with invalid control character (U+0001)
        string xml = "<root>Hello\u0001World</root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.Value.ShouldBe("HelloWorld");
    }

    /// <summary>
    /// U+FFFE in element text is stripped before parsing, leaving <c>TestValue</c>.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_HandlesXmlWithFFFE()
    {
        // Arrange - FFFE is invalid in XML
        string xml = "<root>Test\uFFFEValue</root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.Value.ShouldBe("TestValue");
    }

    /// <summary>
    /// Japanese element text passes through the sanitising step intact.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_PreservesValidUnicode()
    {
        // Arrange
        string xml = "<root>日本語テスト</root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.Value.ShouldBe("日本語テスト");
    }

    /// <summary>
    /// Attributes on the document element are readable from the returned navigator.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_HandlesXmlWithAttributes()
    {
        // Arrange
        string xml = """<root id="123" name="test"><child attr="value">content</child></root>""";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.GetAttribute("id", string.Empty).ShouldBe("123");
        navigator.GetAttribute("name", string.Empty).ShouldBe("test");
    }

    /// <summary>
    /// A default namespace declaration reaches the navigator as the element's namespace URI.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_HandlesXmlWithNamespaces()
    {
        // Arrange
        string xml = """<root xmlns="http://example.com"><child>value</child></root>""";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.NamespaceURI.ShouldBe("http://example.com");
    }

    /// <summary>
    /// A <see langword="null"/> string throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnNullString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator((string)null!));
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnEmptyString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator(string.Empty));
    }

    /// <summary>
    /// The stream overload reads a UTF-8 document and yields a navigator over its <c>root</c> element.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_FromStream_CreatesNavigator()
    {
        // Arrange
        string xml = """<?xml version="1.0" encoding="utf-8"?><root>content</root>""";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("root");
    }

    /// <summary>
    /// The stream-and-encoding overload decodes with the encoding it is given and yields the element text.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_FromStreamWithEncoding_CreatesNavigator()
    {
        // Arrange
        string xml = """<?xml version="1.0"?><root>test content</root>""";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream, Encoding.UTF8);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.Value.ShouldBe("test content");
    }

    /// <summary>
    /// The text-reader overload yields a navigator that can be selected over.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_FromTextReader_CreatesNavigator()
    {
        // Arrange
        string xml = "<root><element>text</element></root>";
        using StringReader reader = new(xml);

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(reader);

        // Assert
        navigator.ShouldNotBeNull();
        XPathNodeIterator elements = navigator.Select("//element");
        elements.Count.ShouldBe(1);
    }

    /// <summary>
    /// A <see langword="null"/> stream throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnNullStream()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator((Stream)null!));
    }

    /// <summary>
    /// A <see langword="null"/> text reader throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnNullTextReader()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator((TextReader)null!));
    }

    #endregion

    #region DecodeBase64String Tests

    /// <summary>
    /// Base-64 text decodes to a stream carrying the original characters.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_DecodesValidBase64()
    {
        // Arrange
        string originalText = "Hello, World!";
        string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(originalText));

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64Encoded);
        using StreamReader reader = new(result, Encoding.UTF8);
        string decodedText = reader.ReadToEnd();

        // Assert
        decodedText.ShouldBe(originalText);
    }

    /// <summary>
    /// A singly padded value, <c>TWE=</c>, decodes to <c>Ma</c>.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_HandlesStandardPadding()
    {
        // Arrange - "Ma" encodes to "TWE=" (with padding)
        string base64WithPadding = "TWE=";

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64WithPadding);
        using StreamReader reader = new(result, Encoding.UTF8);
        string decodedText = reader.ReadToEnd();

        // Assert
        decodedText.ShouldBe("Ma");
    }

    /// <summary>
    /// A doubly padded value, <c>TQ==</c>, decodes to <c>M</c>.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_HandlesDoublePadding()
    {
        // Arrange - "M" encodes to "TQ==" (with double padding)
        string base64WithDoublePadding = "TQ==";

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64WithDoublePadding);
        using StreamReader reader = new(result, Encoding.UTF8);
        string decodedText = reader.ReadToEnd();

        // Assert
        decodedText.ShouldBe("M");
    }

    /// <summary>
    /// An unpadded value, <c>TWFu</c>, decodes to <c>Man</c>.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_HandlesNoPadding()
    {
        // Arrange - "Man" encodes to "TWFu" (no padding needed)
        string base64NoPadding = "TWFu";

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64NoPadding);
        using StreamReader reader = new(result, Encoding.UTF8);
        string decodedText = reader.ReadToEnd();

        // Assert
        decodedText.ShouldBe("Man");
    }

    /// <summary>
    /// Bytes that are not text, <c>0x00</c> and <c>0xFF</c> among them, come back byte for byte.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_DecodesBinaryData()
    {
        // Arrange
        byte[] binaryData = [0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD];
        string base64Encoded = Convert.ToBase64String(binaryData);

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64Encoded);
        using MemoryStream memoryStream = new();
        result.CopyTo(memoryStream);
        byte[] decodedBytes = memoryStream.ToArray();

        // Assert
        decodedBytes.ShouldBe(binaryData);
    }

    /// <summary>
    /// The returned stream is seekable, so a caller may rewind it.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_ReturnsSeekableStream()
    {
        // Arrange
        string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("test"));

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64Encoded);

        // Assert
        result.CanSeek.ShouldBeTrue();
        result.Position.ShouldBe(0);
    }

    /// <summary>
    /// The returned stream is positioned at zero, so a caller need not seek before reading.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_StreamIsAtBeginning()
    {
        // Arrange
        string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("test data"));

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64Encoded);

        // Assert
        result.Position.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> string throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.DecodeBase64String(null!));
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.DecodeBase64String(string.Empty));
    }

    /// <summary>
    /// Text that is not base-64 throws <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_ThrowsOnInvalidBase64()
    {
        // Arrange
        string invalidBase64 = "Not valid base64!@#$";

        // Act & Assert
        Should.Throw<FormatException>(() =>
            SyndicationEncodingUtility.DecodeBase64String(invalidBase64));
    }

    /// <summary>
    /// A 10,000-character payload decodes in full rather than being truncated to a buffer.
    /// </summary>
    [TestMethod]
    public void DecodeBase64String_HandlesLongContent()
    {
        // Arrange
        string longContent = new('A', 10_000);
        string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(longContent));

        // Act
        using Stream result = SyndicationEncodingUtility.DecodeBase64String(base64Encoded);
        using StreamReader reader = new(result, Encoding.UTF8);
        string decodedText = reader.ReadToEnd();

        // Assert
        decodedText.ShouldBe(longContent);
    }

    #endregion

    #region DecodeHtmlEscapedString Tests

    /// <summary>
    /// The named entities <c>&amp;lt;</c>, <c>&amp;gt;</c> and <c>&amp;amp;</c> decode to the characters
    /// they stand for.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesBasicHtmlEntities()
    {
        // Arrange
        string escaped = "&lt;div&gt;Hello &amp; World&lt;/div&gt;";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldBe("<div>Hello & World</div>");
    }

    /// <summary>
    /// <c>&amp;quot;</c> decodes to a double quote.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesQuoteEntities()
    {
        // Arrange
        string escaped = "&quot;quoted text&quot;";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldBe("\"quoted text\"");
    }

    /// <summary>
    /// The numeric reference <c>&amp;#39;</c> decodes to an apostrophe.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesApostropheEntity()
    {
        // Arrange
        string escaped = "It&#39;s working";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldBe("It's working");
    }

    /// <summary>
    /// A decimal character reference, <c>&amp;#169;</c>, decodes to the copyright sign U+00A9.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesNumericEntities()
    {
        // Arrange
        string escaped = "&#169; 2024"; // Copyright symbol

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldContain("\u00A9");
    }

    /// <summary>
    /// A hexadecimal character reference, <c>&amp;#x00A9;</c>, decodes to the same copyright sign as its
    /// decimal spelling.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesHexadecimalEntities()
    {
        // Arrange
        string escaped = "&#x00A9; copyright"; // Hexadecimal copyright symbol

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldContain("\u00A9");
    }

    /// <summary>
    /// Percent-encoding is decoded as well as HTML escaping: <c>Hello%20World</c> becomes
    /// <c>Hello World</c>.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesUrlEncodedValues()
    {
        // Arrange
        string escaped = "Hello%20World";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldBe("Hello World");
    }

    /// <summary>
    /// <c>&amp;nbsp;</c> decodes to a non-breaking space, U+00A0, and not to an ordinary one.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_DecodesNbsp()
    {
        // Arrange
        string escaped = "word&nbsp;word";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldContain("\u00A0"); // Non-breaking space
    }

    /// <summary>
    /// A value carrying both HTML entities and percent-encoding is decoded in both respects, in one call.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_HandlesMixedEncodings()
    {
        // Arrange - Both HTML entities and URL encoding
        string escaped = "&lt;a href=&quot;http://example.com?q=hello%20world&quot;&gt;link&lt;/a&gt;";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldBe("""<a href="http://example.com?q=hello world">link</a>""");
    }

    /// <summary>
    /// Text holding nothing escaped is returned unchanged.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_PreservesPlainText()
    {
        // Arrange
        string plainText = "Simple text without entities";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(plainText);

        // Assert
        result.ShouldBe(plainText);
    }

    /// <summary>
    /// A <see langword="null"/> string throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.DecodeHtmlEscapedString(null!));
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void DecodeHtmlEscapedString_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.DecodeHtmlEscapedString(string.Empty));
    }

    #endregion

    #region EncodeSafeDirectoryName Tests

    /// <summary>
    /// A backslash is removed, closing the gap it leaves.
    /// </summary>
    [TestMethod]
    public void EncodeSafeDirectoryName_RemovesBackslash()
    {
        // Arrange
        string input = "folder\\name";

        // Act
        string result = SyndicationEncodingUtility.EncodeSafeDirectoryName(input);

        // Assert
        result.ShouldBe("foldername");
    }

    /// <summary>
    /// A forward slash is removed, closing the gap it leaves.
    /// </summary>
    [TestMethod]
    public void EncodeSafeDirectoryName_RemovesForwardSlash()
    {
        // Arrange
        string input = "folder/name";

        // Act
        string result = SyndicationEncodingUtility.EncodeSafeDirectoryName(input);

        // Assert
        result.ShouldBe("foldername");
    }

    /// <summary>
    /// The removed set is exactly <c>\</c>, <c>/</c>, <c>:</c>, <c>*</c>, <c>?</c>, <c>&lt;</c>,
    /// <c>&gt;</c> and <c>|</c> — the double quote survives, though Windows rejects it in a path.
    /// </summary>
    [TestMethod]
    public void EncodeSafeDirectoryName_RemovesAllInvalidCharacters()
    {
        // Arrange
        string input = "test\\/:*?\"<>|name";

        // Act
        string result = SyndicationEncodingUtility.EncodeSafeDirectoryName(input);

        // Assert
        result.ShouldBe("test\"name"); // Note: double quote is not removed by this method
    }

    /// <summary>
    /// Hyphens, underscores and dots are left where they are.
    /// </summary>
    [TestMethod]
    public void EncodeSafeDirectoryName_PreservesValidCharacters()
    {
        // Arrange
        string input = "valid-folder_name.test";

        // Act
        string result = SyndicationEncodingUtility.EncodeSafeDirectoryName(input);

        // Assert
        result.ShouldBe(input);
    }

    /// <summary>
    /// A <see langword="null"/> name throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void EncodeSafeDirectoryName_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.EncodeSafeDirectoryName(null!));
    }

    /// <summary>
    /// An empty name throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void EncodeSafeDirectoryName_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.EncodeSafeDirectoryName(string.Empty));
    }

    #endregion

    #region CreateSafeXmlReaderSettings Tests

    /// <summary>
    /// The settings parse a DTD rather than prohibiting one, so a feed declaring its own entities loads.
    /// </summary>
    /// <remarks>
    ///     Prohibiting the DTD would be the blunter defence, and would break those feeds. External entity
    ///     resolution is refused instead, which is what the two navigator tests below hold in place.
    /// </remarks>
    [TestMethod]
    public void CreateSafeXmlReaderSettings_ParsesInternalDtdSubset()
    {
        // Arrange & Act
        // Feeds that declare entities in an internal DTD subset must still load, so the subset is parsed
        // rather than ignored; XXE is prevented by refusing to resolve external entities instead.
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.DtdProcessing.ShouldBe(System.Xml.DtdProcessing.Parse);
    }

    /// <summary>
    /// Entity expansion is bounded by a positive character budget, so a billion-laughs document cannot
    /// expand without limit.
    /// </summary>
    [TestMethod]
    public void CreateSafeXmlReaderSettings_BoundsEntityExpansion()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.MaxCharactersFromEntities.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// An entity declared in the internal subset is expanded in element text: <c>nbsp</c>, defined as
    /// <c>&amp;#160;</c>, resolves to U+00A0.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_ResolvesEntityDeclaredInInternalDtdSubset()
    {
        // Arrange
        string xml = """<?xml version="1.0"?><!DOCTYPE rss [<!ENTITY nbsp "&#160;">]>"""
            + """<rss version="2.0"><channel><title>a&nbsp;b</title></channel></rss>""";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        XPathNavigator? titleNode = navigator.SelectSingleNode("//title");
        titleNode.ShouldNotBeNull();
        titleNode.Value.ShouldBe("a b");
    }

    /// <summary>
    /// An external entity pointing at <c>file:///etc/passwd</c> is not fetched, leaving the element
    /// empty rather than carrying the file.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_DoesNotResolveExternalEntity()
    {
        // Arrange
        string xml = """<?xml version="1.0"?><!DOCTYPE r [<!ENTITY x SYSTEM "file:///etc/passwd">]><r>&x;</r>""";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        XPathNavigator? rootNode = navigator.SelectSingleNode("//r");
        rootNode.ShouldNotBeNull();
        rootNode.Value.ShouldBeEmpty();
    }

    /// <summary>
    /// Comments are dropped rather than surfaced as nodes.
    /// </summary>
    [TestMethod]
    public void CreateSafeXmlReaderSettings_IgnoresComments()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.IgnoreComments.ShouldBeTrue();
    }

    /// <summary>
    /// Insignificant whitespace is dropped rather than surfaced as text nodes.
    /// </summary>
    [TestMethod]
    public void CreateSafeXmlReaderSettings_IgnoresWhitespace()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.IgnoreWhitespace.ShouldBeTrue();
    }

    /// <summary>
    /// Processing instructions are dropped rather than surfaced as nodes.
    /// </summary>
    [TestMethod]
    public void CreateSafeXmlReaderSettings_IgnoresProcessingInstructions()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.IgnoreProcessingInstructions.ShouldBeTrue();
    }

    /// <summary>
    /// The reader requires a whole document, not a fragment: two sibling roots are rejected.
    /// </summary>
    /// <remarks>
    ///     The assertion used to be <c>settings.ConformanceLevel.ShouldBe(ConformanceLevel.Document)</c>,
    ///     read straight back off the object the factory returned. <c>Document</c> is also
    ///     <see cref="XmlReaderSettings"/>'s own default, so that passed against a factory whose whole
    ///     body was <c>return new XmlReaderSettings();</c>. What the setting buys is asserted instead.
    /// </remarks>
    [TestMethod]
    public void CreateSafeXmlReaderSettings_SetsDocumentConformanceLevel()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.ConformanceLevel.ShouldBe(System.Xml.ConformanceLevel.Document);

        using XmlReader reader = XmlReader.Create(new StringReader("<a/><b/>"), settings);
        Should.Throw<XmlException>(() =>
        {
            while (reader.Read())
            {
            }
        });
    }

    #endregion
}