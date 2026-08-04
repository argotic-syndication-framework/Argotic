using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Unit tests for <see cref="SitemapImage"/>.
/// </summary>
[TestClass]
public class SitemapImageTests
{
    private const string ImageNamespace = "http://www.google.com/schemas/sitemap-image/1.1";

    #region Constructor Tests

    [TestMethod]
    public void SitemapImage_DefaultConstructor_CreatesInstance()
    {
        // Act
        SitemapImage image = new();

        // Assert
        image.ShouldNotBeNull();
    }

    [TestMethod]
    public void SitemapImage_ConstructorWithLocation_SetsLocation()
    {
        // Arrange
        Uri location = new("https://example.com/image.jpg");

        // Act
        SitemapImage image = new(location);

        // Assert
        image.Location.ShouldBe(location);
    }

    [TestMethod]
    public void SitemapImage_ConstructorWithNullLocation_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new SitemapImage(null!));

    #endregion

    #region Location Property Tests

    [TestMethod]
    public void Location_SetValidUri_StoresValue()
    {
        // Arrange
        SitemapImage image = new();
        Uri location = new("https://example.com/image.png");

        // Act
        image.Location = location;

        // Assert
        image.Location.ShouldBe(location);
    }

    [TestMethod]
    public void Location_SetNullUri_ThrowsArgumentNullException()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/initial.jpg"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.Location = null!);
    }

    [TestMethod]
    public void Location_SetRelativeUri_StoresValue()
    {
        // Arrange
        SitemapImage image = new();
        Uri location = new("/images/photo.jpg", UriKind.Relative);

        // Act
        image.Location = location;

        // Assert
        image.Location.ShouldBe(location);
    }

    #endregion

    #region Load Tests

    [TestMethod]
    public void Load_ValidImageElement_ReturnsTrue()
    {
        // Arrange
        const string xml = """
            <image xmlns="http://www.google.com/schemas/sitemap-image/1.1">
                <loc>https://example.com/image.jpg</loc>
            </image>
            """;

        SitemapImage image = new();
        XPathNavigator navigator = CreateNavigator(xml);
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        bool result = image.Load(navigator, manager);

        // Assert
        result.ShouldBeTrue();
        image.Location.ShouldBe(new Uri("https://example.com/image.jpg"));
    }

    [TestMethod]
    public void Load_EmptyLocElement_ReturnsFalse()
    {
        // Arrange
        const string xml = """
            <image xmlns="http://www.google.com/schemas/sitemap-image/1.1">
                <loc></loc>
            </image>
            """;

        SitemapImage image = new();
        XPathNavigator navigator = CreateNavigator(xml);
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        bool result = image.Load(navigator, manager);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void Load_MissingLocElement_ReturnsFalse()
    {
        // Arrange
        const string xml = """
            <image xmlns="http://www.google.com/schemas/sitemap-image/1.1">
            </image>
            """;

        SitemapImage image = new();
        XPathNavigator navigator = CreateNavigator(xml);
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        bool result = image.Load(navigator, manager);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void Load_RelativeUri_LoadsSuccessfully()
    {
        // Arrange
        const string xml = """
            <image xmlns="http://www.google.com/schemas/sitemap-image/1.1">
                <loc>/images/photo.jpg</loc>
            </image>
            """;

        SitemapImage image = new();
        XPathNavigator navigator = CreateNavigator(xml);
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        bool result = image.Load(navigator, manager);

        // Assert
        result.ShouldBeTrue();
        image.Location!.ToString().ShouldBe("/images/photo.jpg");
    }

    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        SitemapImage image = new();
        XmlNamespaceManager manager = new(new NameTable());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.Load(null!, manager));
    }

    [TestMethod]
    public void Load_NullManager_ThrowsArgumentNullException()
    {
        // Arrange
        const string xml = """
            <image xmlns="http://www.google.com/schemas/sitemap-image/1.1">
                <loc>https://example.com/image.jpg</loc>
            </image>
            """;

        SitemapImage image = new();
        XPathNavigator navigator = CreateNavigator(xml);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.Load(navigator, null!));
    }

    #endregion

    #region WriteTo Tests

    [TestMethod]
    public void WriteTo_ValidImage_WritesCorrectXml()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        using StringWriter stringWriter = new();
        using XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = false
        });

        // Act
        image.WriteTo(writer, ImageNamespace);
        writer.Flush();

        // Assert
        string result = stringWriter.ToString();
        result.ShouldContain("<image");
        result.ShouldContain("<loc");
        result.ShouldContain("https://example.com/image.jpg");
        result.ShouldContain("</image>");
    }

    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.WriteTo(null!, ImageNamespace));
    }

    [TestMethod]
    public void WriteTo_NullNamespace_ThrowsArgumentException()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        using StringWriter stringWriter = new();
        using XmlWriter writer = XmlWriter.Create(stringWriter);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.WriteTo(writer, null!));
    }

    [TestMethod]
    public void WriteTo_EmptyNamespace_ThrowsArgumentException()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        using StringWriter stringWriter = new();
        using XmlWriter writer = XmlWriter.Create(stringWriter);

        // Act & Assert
        Should.Throw<ArgumentException>(() => image.WriteTo(writer, string.Empty));
    }

    #endregion

    #region CompareTo Tests

    [TestMethod]
    public void CompareTo_SameLocation_ReturnsZero()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act
        int result = image1.CompareTo(image2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_DifferentLocation_ReturnsNonZero()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/a.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/b.jpg"));

        // Act
        int result = image1.CompareTo(image2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_NullObject_ReturnsPositive()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act
        int result = image.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_Null_ReturnsPositive()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        image.CompareTo(null).ShouldBe(1);
    }

    #endregion

    #region Equals Tests

    [TestMethod]
    public void Equals_SameLocation_ReturnsTrue()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act
        bool result = image1.Equals(image2);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_DifferentLocation_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/a.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/b.jpg"));

        // Act
        bool result = image1.Equals(image2);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_NullObject_ReturnsFalse()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act
        bool result = image.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_DifferentType_ReturnsFalse()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act
        bool result = image.Equals("not an image");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [TestMethod]
    public void GetHashCode_SameLocation_ReturnsSameHash()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        image1.GetHashCode().ShouldBe(image2.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_Consistency_ReturnsSameHashOnMultipleCalls()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act
        int hash1 = image.GetHashCode();
        int hash2 = image.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_WithLocation_ReturnsLocationString()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act
        string result = image.ToString();

        // Assert
        result.ShouldBe("https://example.com/image.jpg");
    }

    [TestMethod]
    public void ToString_WithoutLocation_ReturnsEmptyString()
    {
        // Arrange
        SitemapImage image = new();

        // Act
        string result = image.ToString();

        // Assert
        result.ShouldBe(string.Empty);
    }

    #endregion

    #region Operator Tests

    [TestMethod]
    public void OperatorEquals_SameLocation_ReturnsTrue()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        (image1 == image2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_DifferentLocation_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/a.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/b.jpg"));

        // Act & Assert
        (image1 == image2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorEquals_BothNull_ReturnsTrue()
    {
        // Arrange
        SitemapImage? image1 = null;
        SitemapImage? image2 = null;

        // Act & Assert
        (image1 == image2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_OneNull_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage? image2 = null;

        // Act & Assert
        (image1 == image2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorNotEquals_SameLocation_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        (image1 != image2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentLocation_ReturnsTrue()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/a.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/b.jpg"));

        // Act & Assert
        (image1 != image2).ShouldBeTrue();
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void RoundTrip_WriteAndLoad_PreservesLocation()
    {
        // Arrange
        Uri originalLocation = new("https://example.com/roundtrip.jpg");
        SitemapImage originalImage = new(originalLocation);

        // Act - Write
        string xml;
        using (StringWriter stringWriter = new())
        {
            using XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false
            });

            writer.WriteStartElement("url", "http://www.sitemaps.org/schemas/sitemap/0.9");
            originalImage.WriteTo(writer, ImageNamespace);
            writer.WriteEndElement();
            writer.Flush();
            xml = stringWriter.ToString();
        }

        // Act - Load
        SitemapImage loadedImage = new();
        XPathNavigator navigator = CreateNavigator(xml);

        // Navigate to the image element
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("image", ImageNamespace);
        manager.AddNamespace("sm", "http://www.sitemaps.org/schemas/sitemap/0.9");

        XPathNavigator? imageNav = navigator.SelectSingleNode("//image:image", manager);
        imageNav.ShouldNotBeNull();

        bool loaded = loadedImage.Load(imageNav, manager);

        // Assert
        loaded.ShouldBeTrue();
        loadedImage.Location.ShouldBe(originalLocation);
    }

    #endregion

    #region Helper Methods

    private static XPathNavigator CreateNavigator(string xml)
    {
        using StringReader documentReader = new(xml);
        XPathDocument document = new(documentReader);
        return document.CreateNavigator();
    }

    private static XmlNamespaceManager CreateNamespaceManager(XPathNavigator navigator)
    {
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("image", ImageNamespace);
        return manager;
    }

    #endregion
}