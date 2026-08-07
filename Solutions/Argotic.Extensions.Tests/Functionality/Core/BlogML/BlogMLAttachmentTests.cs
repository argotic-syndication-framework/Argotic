using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication.Specialized;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

/// <summary>
/// Unit tests for <see cref="BlogMLAttachment"/> that verify attachment parsing, serialization, and comparison.
/// </summary>
[TestClass]
public class BlogMLAttachmentTests
{
    #region Constructor Tests

    /// <summary>
    /// A newly constructed attachment has empty content and media type, is not embedded, has no URL, and reports its size as <c>long.MinValue</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesValidInstance()
    {
        // Arrange & Act
        BlogMLAttachment attachment = new();

        // Assert
        attachment.ShouldNotBeNull();
        attachment.Content.ShouldBe(string.Empty);
        attachment.MimeType.ShouldBe(string.Empty);
        attachment.IsEmbedded.ShouldBeFalse();
        attachment.Size.ShouldBe(long.MinValue);
        attachment.Url.ShouldBeNull();
        attachment.ExternalUri.ShouldBeNull();
        attachment.Extensions.ShouldNotBeNull();
        attachment.HasExtensions.ShouldBeFalse();
    }

    #endregion

    #region Load Tests

    /// <summary>
    /// Loading an embedded attachment reads <c>embedded</c>, <c>mime-type</c>, <c>size</c> and <c>url</c>, and keeps the base64 element body as the content.
    /// </summary>
    [TestMethod]
    public void Load_EmbeddedContent_ParsesProperties()
    {
        // Arrange
        const string attachmentXml = """
            <attachment xmlns="http://www.blogml.com/2006/09/BlogML"
                        embedded="true"
                        mime-type="image/png"
                        size="12345"
                        url="http://example.com/image.png">SGVsbG8gV29ybGQ=</attachment>
            """;

        BlogMLAttachment attachment = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(attachmentXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act
        bool wasLoaded = attachment.Load(navigator);

        // Assert
        wasLoaded.ShouldBeTrue();
        attachment.IsEmbedded.ShouldBeTrue();
        attachment.MimeType.ShouldBe("image/png");
        attachment.Size.ShouldBe(12_345);
        attachment.Url.ShouldBe(new Uri("http://example.com/image.png"));
        attachment.Content.ShouldBe("SGVsbG8gV29ybGQ=");
    }

    /// <summary>
    /// Loading an attachment that is not embedded reads <c>external-uri</c> into <c>ExternalUri</c> alongside <c>Url</c>.
    /// </summary>
    [TestMethod]
    public void Load_ExternalUri_SetsExternalUri()
    {
        // Arrange
        const string attachmentXml = """
            <attachment xmlns="http://www.blogml.com/2006/09/BlogML"
                        embedded="false"
                        mime-type="application/pdf"
                        external-uri="http://example.com/doc.pdf"
                        url="http://example.com/doc.pdf"/>
            """;

        BlogMLAttachment attachment = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(attachmentXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act
        bool wasLoaded = attachment.Load(navigator);

        // Assert
        wasLoaded.ShouldBeTrue();
        attachment.IsEmbedded.ShouldBeFalse();
        attachment.ExternalUri.ShouldBe(new Uri("http://example.com/doc.pdf"));
        attachment.Url.ShouldBe(new Uri("http://example.com/doc.pdf"));
    }

    /// <summary>
    /// An attachment element carrying only <c>mime-type</c> and <c>url</c> loads, and the media type survives verbatim.
    /// </summary>
    [TestMethod]
    public void Load_WithMimeType_SetsMimeType()
    {
        // Arrange
        const string attachmentXml = """
            <attachment xmlns="http://www.blogml.com/2006/09/BlogML"
                        mime-type="video/mp4"
                        url="http://example.com/video.mp4"/>
            """;

        BlogMLAttachment attachment = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(attachmentXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act
        bool wasLoaded = attachment.Load(navigator);

        // Assert
        wasLoaded.ShouldBeTrue();
        attachment.MimeType.ShouldBe("video/mp4");
    }

    /// <summary>
    /// The load overload taking <see cref="SyndicationResourceLoadSettings"/> reads the same attributes and content as the overload without.
    /// </summary>
    [TestMethod]
    public void Load_WithSettings_LoadsAttachment()
    {
        // Arrange
        const string attachmentXml = """
            <attachment xmlns="http://www.blogml.com/2006/09/BlogML"
                        embedded="true"
                        mime-type="image/jpeg"
                        size="5000"
                        url="http://example.com/photo.jpg">Base64Content</attachment>
            """;

        BlogMLAttachment attachment = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(attachmentXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();
        SyndicationResourceLoadSettings settings = new();

        // Act
        bool wasLoaded = attachment.Load(navigator, settings);

        // Assert
        wasLoaded.ShouldBeTrue();
        attachment.MimeType.ShouldBe("image/jpeg");
        attachment.Size.ShouldBe(5000);
        attachment.Content.ShouldBe("Base64Content");
    }

    /// <summary>
    /// Loading from a <see langword="null"/> navigator throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => attachment.Load(null!));
    }

    /// <summary>
    /// <see langword="null"/> load settings throw <see cref="ArgumentNullException"/> even when the navigator is valid.
    /// </summary>
    [TestMethod]
    public void Load_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        const string attachmentXml = """
            <attachment xmlns="http://www.blogml.com/2006/09/BlogML" mime-type="image/png"/>
            """;

        BlogMLAttachment attachment = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(attachmentXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => attachment.Load(navigator, null!));
    }

    #endregion

    #region WriteTo Tests

    /// <summary>
    /// Writing an embedded attachment emits the <c>embedded</c>, <c>mime-type</c>, <c>size</c> and <c>url</c> attributes and the content as the element body.
    /// </summary>
    [TestMethod]
    public void WriteTo_EmbeddedContent_WritesCorrectXml()
    {
        // Arrange
        BlogMLAttachment attachment = new()
        {
            IsEmbedded = true,
            MimeType = "image/png",
            Size = 12_345,
            Url = new Uri("http://example.com/image.png"),
            Content = "SGVsbG8gV29ybGQ="
        };

        // Act
        using MemoryStream stream = new();
        XmlWriterSettings writerSettings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true
        };

        using (XmlWriter writer = XmlWriter.Create(stream, writerSettings))
        {
            attachment.WriteTo(writer);
        }

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert
        xml.ShouldContain("attachment");
        xml.ShouldContain("embedded=\"true\"");
        xml.ShouldContain("mime-type=\"image/png\"");
        xml.ShouldContain("size=\"12345\"");
        xml.ShouldContain("url=\"http://example.com/image.png\"");
        xml.ShouldContain("SGVsbG8gV29ybGQ=");
    }

    /// <summary>
    /// An attachment holding an external URI writes it as the <c>external-uri</c> attribute.
    /// </summary>
    [TestMethod]
    public void WriteTo_ExternalUri_WritesExternalUriAttribute()
    {
        // Arrange
        BlogMLAttachment attachment = new()
        {
            IsEmbedded = false,
            MimeType = "application/pdf",
            ExternalUri = new Uri("http://example.com/doc.pdf"),
            Url = new Uri("http://example.com/doc.pdf")
        };

        // Act
        using MemoryStream stream = new();
        using (XmlWriter writer = XmlWriter.Create(stream))
        {
            attachment.WriteTo(writer);
        }

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert
        xml.ShouldContain("external-uri=\"http://example.com/doc.pdf\"");
    }

    /// <summary>
    /// Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => attachment.WriteTo(null!));
    }

    /// <summary>
    /// <c>ToString</c> renders the attachment as its XML element, carrying the <c>mime-type</c> attribute.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        BlogMLAttachment attachment = new()
        {
            IsEmbedded = true,
            MimeType = "image/gif",
            Url = new Uri("http://example.com/image.gif")
        };

        // Act
        string result = attachment.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("attachment");
        result.ShouldContain("mime-type=\"image/gif\"");
    }

    #endregion

    #region Comparison Tests

    /// <summary>
    /// Two attachments agreeing on embedding, media type, size, URL and content compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_EqualAttachments_ReturnsZero()
    {
        // Arrange
        BlogMLAttachment attachment1 = new()
        {
            IsEmbedded = true,
            MimeType = "image/png",
            Size = 12_345,
            Url = new Uri("http://example.com/image.png"),
            Content = "SGVsbG8="
        };

        BlogMLAttachment attachment2 = new()
        {
            IsEmbedded = true,
            MimeType = "image/png",
            Size = 12_345,
            Url = new Uri("http://example.com/image.png"),
            Content = "SGVsbG8="
        };

        // Act
        int result = attachment1.CompareTo(attachment2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Attachments differing in media type and URL do not compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_DifferentAttachments_ReturnsNonZero()
    {
        // Arrange
        BlogMLAttachment attachment1 = new()
        {
            MimeType = "image/png",
            Url = new Uri("http://example.com/image1.png")
        };

        BlogMLAttachment attachment2 = new()
        {
            MimeType = "image/jpeg",
            Url = new Uri("http://example.com/image2.jpg")
        };

        // Act
        int result = attachment1.CompareTo(attachment2);

        // Assert
        result.ShouldNotBe(0);
    }

    /// <summary>
    /// Comparing an attachment against <see langword="null"/> returns <c>1</c>, so <see langword="null"/> sorts first.
    /// </summary>
    [TestMethod]
    public void CompareTo_NullObject_ReturnsPositive()
    {
        // Arrange
        BlogMLAttachment attachment = new()
        {
            MimeType = "image/png"
        };

        // Act
        int result = attachment.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// Attachments differing in media type, URL and size do not compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_DifferentAttachment_ReturnsNonZero()
    {
        // Arrange
        BlogMLAttachment attachment1 = new()
        {
            MimeType = "image/png",
            Url = new Uri("http://example.com/image1.png"),
            Size = 1000
        };
        BlogMLAttachment attachment2 = new()
        {
            MimeType = "image/jpeg",
            Url = new Uri("http://example.com/image2.jpg"),
            Size = 2000
        };

        // Act
        int result = attachment1.CompareTo(attachment2);

        // Assert
        result.ShouldNotBe(0);
    }

    #endregion

    #region Equality Operators Tests

    /// <summary>
    /// <c>==</c>, <c>!=</c> and <c>Equals</c> agree on value equality, and <c>Equals</c> rejects both <see langword="null"/> and an unrelated type.
    /// </summary>
    [TestMethod]
    public void Operators_EqualityAndComparison_WorkCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment1 = new()
        {
            MimeType = "image/png",
            Url = new Uri("http://example.com/image.png"),
            Size = 1000
        };

        BlogMLAttachment attachment2 = new()
        {
            MimeType = "image/png",
            Url = new Uri("http://example.com/image.png"),
            Size = 1000
        };

        BlogMLAttachment attachment3 = new()
        {
            MimeType = "image/jpeg",
            Url = new Uri("http://example.com/image.jpg"),
            Size = 2000
        };

        // Act & Assert - Equality
        (attachment1 == attachment2).ShouldBeTrue();
        (attachment1 != attachment3).ShouldBeTrue();

        // Act & Assert - Equals method
        attachment1.Equals(attachment2).ShouldBeTrue();
        attachment1.Equals(attachment3).ShouldBeFalse();
        attachment1.Equals(null).ShouldBeFalse();
        attachment1.Equals("not an attachment").ShouldBeFalse();
    }

    /// <summary>
    /// <c>==</c> against a <see langword="null"/> left operand is <see langword="false"/>, and the <c>is null</c> pattern agrees with each reference's state.
    /// </summary>
    [TestMethod]
    public void Operator_Equality_WithNulls_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment = new() { MimeType = "image/png" };
        BlogMLAttachment? nullAttachment = null;

        // Act & Assert
        (nullAttachment is null).ShouldBeTrue();
        (attachment is null).ShouldBeFalse();
        (null == attachment).ShouldBeFalse();
    }

    /// <summary>
    /// Attachments with different media types are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void Operator_Inequality_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment1 = new() { MimeType = "image/png" };
        BlogMLAttachment attachment2 = new() { MimeType = "image/jpeg" };

        // Act & Assert
        (attachment1 != attachment2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&lt;</c> orders the smaller of two otherwise identical attachments first, and puts <see langword="null"/> ahead of any instance.
    /// </summary>
    [TestMethod]
    public void Operator_LessThan_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment1 = new() { MimeType = "image/png", Size = 100 };
        BlogMLAttachment attachment2 = new() { MimeType = "image/png", Size = 200 };
        BlogMLAttachment? nullAttachment = null;

        // Act & Assert
        (attachment1 < attachment2).ShouldBeTrue();
        (nullAttachment < attachment1).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;</c> orders the larger of two otherwise identical attachments first, and <see langword="null"/> is never greater than an instance.
    /// </summary>
    [TestMethod]
    public void Operator_GreaterThan_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment1 = new() { MimeType = "image/png", Size = 200 };
        BlogMLAttachment attachment2 = new() { MimeType = "image/png", Size = 100 };
        BlogMLAttachment? nullAttachment = null;

        // Act & Assert
        (attachment1 > attachment2).ShouldBeTrue();
        (nullAttachment > attachment1).ShouldBeFalse();
    }

    /// <summary>
    /// <c>&lt;=</c> holds for two equal attachments and for <see langword="null"/> against an instance.
    /// </summary>
    [TestMethod]
    public void Operator_LessThanOrEqual_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment1 = new() { MimeType = "image/png", Size = 100 };
        BlogMLAttachment attachment2 = new() { MimeType = "image/png", Size = 100 };
        BlogMLAttachment? nullAttachment = null;

        // Act & Assert
        (attachment1 <= attachment2).ShouldBeTrue();
        (nullAttachment <= attachment1).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> holds for two equal attachments.
    /// </summary>
    [TestMethod]
    public void Operator_GreaterThanOrEqual_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment1 = new() { MimeType = "image/png", Size = 100 };
        BlogMLAttachment attachment2 = new() { MimeType = "image/png", Size = 100 };

        // Act & Assert
        (attachment1 >= attachment2).ShouldBeTrue();
    }

    #endregion

    #region FindExtension Tests

    /// <summary>
    /// Searching an attachment that carries no extensions returns <see langword="null"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void FindExtension_WithPredicate_ReturnsCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act
        ISyndicationExtension? result = attachment.FindExtension(ext => ext.XmlNamespace == "http://nonexistent.example.com");

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// A <see langword="null"/> match predicate throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void FindExtension_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => attachment.FindExtension(null!));
    }

    #endregion

    #region Property Tests

    /// <summary>
    /// Assigning content trims the surrounding whitespace.
    /// </summary>
    [TestMethod]
    public void Content_SetAndGet_WorksCorrectly()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act
        attachment.Content = "  test content  ";

        // Assert
        attachment.Content.ShouldBe("test content");
    }

    /// <summary>
    /// Assigning <see langword="null"/> content yields an empty string rather than throwing.
    /// </summary>
    [TestMethod]
    public void Content_SetToNull_ReturnsEmptyString()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act
        attachment.Content = null!;

        // Assert
        attachment.Content.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Assigning a <see langword="null"/> media type throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void MimeType_SetNull_ThrowsArgumentException()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => attachment.MimeType = null!);
    }

    /// <summary>
    /// Assigning an <i>empty</i> media type throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void MimeType_SetEmpty_ThrowsArgumentException()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => attachment.MimeType = string.Empty);
    }

    /// <summary>
    /// Assigning a media type trims the surrounding whitespace.
    /// </summary>
    [TestMethod]
    public void MimeType_SetValidValue_TrimsWhitespace()
    {
        // Arrange
        BlogMLAttachment attachment = new();

        // Act
        attachment.MimeType = "  image/png  ";

        // Assert
        attachment.MimeType.ShouldBe("image/png");
    }

    /// <summary>
    /// An attachment with a media type and a URL hashes to something other than <c>0</c>.
    /// </summary>
    [TestMethod]
    public void GetHashCode_ReturnsValue()
    {
        // Arrange
        BlogMLAttachment attachment = new()
        {
            MimeType = "image/png",
            Url = new Uri("http://example.com/image.png")
        };

        // Act
        int hashCode = attachment.GetHashCode();

        // Assert
        hashCode.ShouldNotBe(0);
    }

    #endregion

    #region Integration Tests with BlogML Document

    /// <summary>
    /// Loading a whole BlogML document reaches the attachments on its post, in document order, and keeps the embedded and external forms distinct.
    /// </summary>
    [TestMethod]
    public void Load_FromBlogMLDocument_ParsesAttachments()
    {
        // Arrange
        BlogMLDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.BlogMLWithAttachments));

        // Act
        document.Load(stream);

        // Assert
        document.Posts.Count.ShouldBe(1);
        document.Posts[0].Attachments.Count.ShouldBe(2);

        // First attachment - embedded
        BlogMLAttachment embedded = document.Posts[0].Attachments[0];
        embedded.IsEmbedded.ShouldBeTrue();
        embedded.MimeType.ShouldBe("image/png");
        embedded.Size.ShouldBe(12_345);
        embedded.Url.ShouldBe(new Uri("http://example.com/image.png"));
        embedded.Content.ShouldBe("SGVsbG8gV29ybGQ=");

        // Second attachment - external
        BlogMLAttachment external = document.Posts[0].Attachments[1];
        external.IsEmbedded.ShouldBeFalse();
        external.MimeType.ShouldBe("application/pdf");
        external.ExternalUri.ShouldBe(new Uri("http://example.com/doc.pdf"));
    }

    #endregion
}