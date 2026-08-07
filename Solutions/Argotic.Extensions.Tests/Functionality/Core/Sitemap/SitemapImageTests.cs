using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers <see cref="SitemapImage"/>: constructing one, the guards on its location, reading an
/// <c>image:image</c> element off a navigator, writing one back, and the equality, ordering, hashing
/// and rendering contracts.
/// </summary>
[TestClass]
public class SitemapImageTests
{
    private const string ImageNamespace = "http://www.google.com/schemas/sitemap-image/1.1";

    #region Constructor Tests

    /// <summary>
    /// An image can be constructed with no location, ready to be filled in by a load or by the setter.
    /// </summary>
    [TestMethod]
    public void SitemapImage_DefaultConstructor_CreatesInstance()
    {
        // Act
        SitemapImage image = new();

        // Assert
        image.ShouldNotBeNull();
    }

    /// <summary>
    /// The URI given to the constructor is the location the image reports back.
    /// </summary>
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

    /// <summary>
    /// Constructing an image without a location throws <see cref="ArgumentNullException"/> rather than
    /// yielding a half-built element.
    /// </summary>
    [TestMethod]
    public void SitemapImage_ConstructorWithNullLocation_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new SitemapImage(null!));

    #endregion

    #region Location Property Tests

    /// <summary>
    /// An absolute URI assigned to the location is stored unchanged.
    /// </summary>
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

    /// <summary>
    /// Assigning <see langword="null"/> over an existing location throws
    /// <see cref="ArgumentNullException"/>, so an image that has a location cannot lose it.
    /// </summary>
    [TestMethod]
    public void Location_SetNullUri_ThrowsArgumentNullException()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/initial.jpg"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.Location = null!);
    }

    /// <summary>
    /// A relative URI is accepted as an image location and stored unchanged.
    /// </summary>
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

    /// <summary>
    /// An <c>image</c> element carrying a <c>loc</c> loads, reporting <see langword="true"/> and taking
    /// its location from that child.
    /// </summary>
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

    /// <summary>
    /// An <c>image</c> whose <c>loc</c> is present but empty reports <see langword="false"/>, so no
    /// image with no address is added.
    /// </summary>
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

    /// <summary>
    /// An <c>image</c> with no <c>loc</c> child at all reports <see langword="false"/>.
    /// </summary>
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

    /// <summary>
    /// A <c>loc</c> holding a relative path loads, and the location keeps the relative form
    /// <c>/images/photo.jpg</c> rather than being resolved against a base.
    /// </summary>
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

    /// <summary>
    /// Loading from a <see langword="null"/> navigator throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        SitemapImage image = new();
        XmlNamespaceManager manager = new(new NameTable());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.Load(null!, manager));
    }

    /// <summary>
    /// Loading with a <see langword="null"/> namespace manager throws
    /// <see cref="ArgumentNullException"/>, even when the navigator is sound.
    /// </summary>
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

    /// <summary>
    /// Writing an image emits an <c>image</c> element wrapping a <c>loc</c> that holds the location.
    /// </summary>
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

    /// <summary>
    /// Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        SitemapImage image = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => image.WriteTo(null!, ImageNamespace));
    }

    /// <summary>
    /// Writing with a <see langword="null"/> namespace throws <see cref="ArgumentNullException"/> — the
    /// guard is <c>ArgumentException.ThrowIfNullOrEmpty</c>, whose null branch is the more specific type.
    /// </summary>
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

    /// <summary>
    /// Writing with an <i>empty</i> namespace throws <see cref="ArgumentException"/>.
    /// </summary>
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

    /// <summary>
    /// Two images built from the same location compare equal.
    /// </summary>
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

    /// <summary>
    /// Images with different locations do not compare equal.
    /// </summary>
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

    /// <summary>
    /// An image compares greater than <see langword="null"/>, returning <c>1</c>.
    /// </summary>
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

    /// <summary>
    /// Comparing against <see langword="null"/> returns <c>1</c> when the call is asserted directly
    /// rather than through a local.
    /// </summary>
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

    /// <summary>
    /// Two images built from the same location are equal.
    /// </summary>
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

    /// <summary>
    /// Images with different locations are not equal.
    /// </summary>
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

    /// <summary>
    /// An image is never equal to <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// An image is never equal to an object of another type, here a <see cref="string"/>.
    /// </summary>
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

    /// <summary>
    /// Equal images hash equally, which is the contract a hash set relies on.
    /// </summary>
    [TestMethod]
    public void GetHashCode_SameLocation_ReturnsSameHash()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        image1.GetHashCode().ShouldBe(image2.GetHashCode());
    }

    /// <summary>
    /// An image's hash does not change between calls on the same unmodified instance.
    /// </summary>
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

    /// <summary>
    /// An image renders as its location alone, with no element name or punctuation around it.
    /// </summary>
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

    /// <summary>
    /// An image with no location renders as an <i>empty</i> string rather than throwing or naming the type.
    /// </summary>
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

    /// <summary>
    /// The equality operator agrees with <c>Equals</c> for two images sharing a location.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_SameLocation_ReturnsTrue()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        (image1 == image2).ShouldBeTrue();
    }

    /// <summary>
    /// The equality operator separates images with different locations.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_DifferentLocation_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/a.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/b.jpg"));

        // Act & Assert
        (image1 == image2).ShouldBeFalse();
    }

    /// <summary>
    /// Two <see langword="null"/> references compare equal under the operator rather than dereferencing.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_BothNull_ReturnsTrue()
    {
        // Arrange
        SitemapImage? image1 = null;
        SitemapImage? image2 = null;

        // Act & Assert
        (image1 == image2).ShouldBeTrue();
    }

    /// <summary>
    /// An image compared against a <see langword="null"/> reference is not equal, and the operator does
    /// not throw.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_OneNull_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage? image2 = null;

        // Act & Assert
        (image1 == image2).ShouldBeFalse();
    }

    /// <summary>
    /// The inequality operator is the negation of the equality operator for two images sharing a location.
    /// </summary>
    [TestMethod]
    public void OperatorNotEquals_SameLocation_ReturnsFalse()
    {
        // Arrange
        SitemapImage image1 = new(new Uri("https://example.com/image.jpg"));
        SitemapImage image2 = new(new Uri("https://example.com/image.jpg"));

        // Act & Assert
        (image1 != image2).ShouldBeFalse();
    }

    /// <summary>
    /// The inequality operator reports images with different locations as different.
    /// </summary>
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

    /// <summary>
    /// An image written inside a <c>url</c> element and read back out of the resulting XML carries the
    /// location it started with.
    /// </summary>
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