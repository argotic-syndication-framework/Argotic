using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationEncodingUtilityTest
{
    #region RemoveInvalidXmlHexadecimalCharacters Tests

    [TestMethod]
    [DataRow("a", "a")]
    [DataRow("@±あ😀", "@±あ😀")] // Emoji should not be stripped
    [DataRow("a\uFFFEb", "ab")] // FFFE should be stripped
    public void RemoveInvalidXmlHexadecimalCharactersTest(string input, string expected)
    {
        string stripped = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(input);
        stripped.ShouldBe(expected);
    }

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

    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_HandlesEmptyStringException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(string.Empty));
    }

    [TestMethod]
    public void RemoveInvalidXmlHexadecimalCharacters_HandlesNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(null!));
    }

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

    [TestMethod]
    public void GetXmlEncoding_DetectsUtf8Encoding_FromDeclaration()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_DetectsUtf16Encoding_FromDeclaration()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe("utf-16");
    }

    [TestMethod]
    public void GetXmlEncoding_DetectsIso88591Encoding_FromDeclaration()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe("iso-8859-1");
    }

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

    [TestMethod]
    public void GetXmlEncoding_DefaultsToUtf8_WhenDeclarationHasNoEncoding()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_DetectsUsAsciiEncoding()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"us-ascii\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.WebName.ShouldBe("us-ascii");
    }

    [TestMethod]
    public void GetXmlEncoding_HandlesEncodingWithSingleQuotes()
    {
        // Arrange
        string xml = "<?xml version='1.0' encoding='utf-8'?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_HandlesEncodingWithExtraSpaces()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\"   encoding  =  \"utf-8\" ?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_IsCaseInsensitive()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" ENCODING=\"UTF-8\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_DefaultsToUtf8_ForInvalidEncodingName()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"invalid-encoding-name\"?><root>content</root>";

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(xml);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_FromByteArray_DetectsEncoding()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root>test</root>";
        byte[] data = Encoding.UTF8.GetBytes(xml);

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(data);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_FromStream_DetectsEncoding()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root>test</root>";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        Encoding result = SyndicationEncodingUtility.GetXmlEncoding(stream);

        // Assert
        result.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void GetXmlEncoding_ThrowsOnNullString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding((string)null!));
    }

    [TestMethod]
    public void GetXmlEncoding_ThrowsOnEmptyString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding(string.Empty));
    }

    [TestMethod]
    public void GetXmlEncoding_ThrowsOnNullByteArray()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding((byte[])null!));
    }

    [TestMethod]
    public void GetXmlEncoding_ThrowsOnNullStream()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.GetXmlEncoding((Stream)null!));
    }

    #endregion

    #region CreateSafeNavigator Tests

    [TestMethod]
    public void CreateSafeNavigator_CreatesNavigator_FromValidXml()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root><child>value</child></root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("root");
    }

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

    [TestMethod]
    public void CreateSafeNavigator_HandlesXmlWithAttributes()
    {
        // Arrange
        string xml = "<root id=\"123\" name=\"test\"><child attr=\"value\">content</child></root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.GetAttribute("id", string.Empty).ShouldBe("123");
        navigator.GetAttribute("name", string.Empty).ShouldBe("test");
    }

    [TestMethod]
    public void CreateSafeNavigator_HandlesXmlWithNamespaces()
    {
        // Arrange
        string xml = "<root xmlns=\"http://example.com\"><child>value</child></root>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.NamespaceURI.ShouldBe("http://example.com");
    }

    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnNullString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator((string)null!));
    }

    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnEmptyString()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator(string.Empty));
    }

    [TestMethod]
    public void CreateSafeNavigator_FromStream_CreatesNavigator()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root>content</root>";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("root");
    }

    [TestMethod]
    public void CreateSafeNavigator_FromStreamWithEncoding_CreatesNavigator()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\"?><root>test content</root>";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream, Encoding.UTF8);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.Value.ShouldBe("test content");
    }

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

    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnNullStream()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator((Stream)null!));
    }

    [TestMethod]
    public void CreateSafeNavigator_ThrowsOnNullTextReader()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.CreateSafeNavigator((TextReader)null!));
    }

    #endregion

    #region DecodeBase64String Tests

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

    [TestMethod]
    public void DecodeBase64String_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.DecodeBase64String(null!));
    }

    [TestMethod]
    public void DecodeBase64String_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.DecodeBase64String(string.Empty));
    }

    [TestMethod]
    public void DecodeBase64String_ThrowsOnInvalidBase64()
    {
        // Arrange
        string invalidBase64 = "Not valid base64!@#$";

        // Act & Assert
        Should.Throw<FormatException>(() =>
            SyndicationEncodingUtility.DecodeBase64String(invalidBase64));
    }

    [TestMethod]
    public void DecodeBase64String_HandlesLongContent()
    {
        // Arrange
        string longContent = new('A', 10000);
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

    [TestMethod]
    public void DecodeHtmlEscapedString_HandlesMixedEncodings()
    {
        // Arrange - Both HTML entities and URL encoding
        string escaped = "&lt;a href=&quot;http://example.com?q=hello%20world&quot;&gt;link&lt;/a&gt;";

        // Act
        string result = SyndicationEncodingUtility.DecodeHtmlEscapedString(escaped);

        // Assert
        result.ShouldBe("<a href=\"http://example.com?q=hello world\">link</a>");
    }

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

    [TestMethod]
    public void DecodeHtmlEscapedString_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.DecodeHtmlEscapedString(null!));
    }

    [TestMethod]
    public void DecodeHtmlEscapedString_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.DecodeHtmlEscapedString(string.Empty));
    }

    #endregion

    #region EncodeInvalidXmlHexadecimalCharacters Tests

    [TestMethod]
    public void EncodeInvalidXmlHexadecimalCharacters_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.EncodeInvalidXmlHexadecimalCharacters(null!));
    }

    [TestMethod]
    public void EncodeInvalidXmlHexadecimalCharacters_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.EncodeInvalidXmlHexadecimalCharacters(string.Empty));
    }

    #endregion

    #region EncodeSafeDirectoryName Tests

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

    [TestMethod]
    public void EncodeSafeDirectoryName_ThrowsOnNullInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            SyndicationEncodingUtility.EncodeSafeDirectoryName(null!));
    }

    [TestMethod]
    public void EncodeSafeDirectoryName_ThrowsOnEmptyInput()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() =>
            SyndicationEncodingUtility.EncodeSafeDirectoryName(string.Empty));
    }

    #endregion

    #region CreateSafeXmlReaderSettings Tests

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

    [TestMethod]
    public void CreateSafeXmlReaderSettings_BoundsEntityExpansion()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.MaxCharactersFromEntities.ShouldBeGreaterThan(0);
    }

    [TestMethod]
    public void CreateSafeNavigator_ResolvesEntityDeclaredInInternalDtdSubset()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\"?><!DOCTYPE rss [<!ENTITY nbsp \"&#160;\">]>"
            + "<rss version=\"2.0\"><channel><title>a&nbsp;b</title></channel></rss>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        XPathNavigator? titleNode = navigator.SelectSingleNode("//title");
        titleNode.ShouldNotBeNull();
        titleNode.Value.ShouldBe("a b");
    }

    [TestMethod]
    public void CreateSafeNavigator_DoesNotResolveExternalEntity()
    {
        // Arrange
        string xml = "<?xml version=\"1.0\"?><!DOCTYPE r [<!ENTITY x SYSTEM \"file:///etc/passwd\">]><r>&x;</r>";

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml);

        // Assert
        XPathNavigator? rootNode = navigator.SelectSingleNode("//r");
        rootNode.ShouldNotBeNull();
        rootNode.Value.ShouldBeEmpty();
    }

    [TestMethod]
    public void CreateSafeXmlReaderSettings_IgnoresComments()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.IgnoreComments.ShouldBeTrue();
    }

    [TestMethod]
    public void CreateSafeXmlReaderSettings_IgnoresWhitespace()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.IgnoreWhitespace.ShouldBeTrue();
    }

    [TestMethod]
    public void CreateSafeXmlReaderSettings_IgnoresProcessingInstructions()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.IgnoreProcessingInstructions.ShouldBeTrue();
    }

    [TestMethod]
    public void CreateSafeXmlReaderSettings_SetsDocumentConformanceLevel()
    {
        // Arrange & Act
        XmlReaderSettings settings = SyndicationEncodingUtility.CreateSafeXmlReaderSettings();

        // Assert
        settings.ConformanceLevel.ShouldBe(System.Xml.ConformanceLevel.Document);
    }

    #endregion
}