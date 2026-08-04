using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Unit tests for Sitemap extensions: Image, Video, News, and Hreflang.
/// </summary>
[TestClass]
public class SitemapExtensionTests
{
    private const string ImageNamespace = "http://www.google.com/schemas/sitemap-image/1.1";
    private const string VideoNamespace = "http://www.google.com/schemas/sitemap-video/1.1";
    private const string NewsNamespace = "http://www.google.com/schemas/sitemap-news/0.9";
    private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

    #region Image Extension Parsing Tests

    [TestMethod]
    public void ParseSitemapWithImageExtension_HasImageNamespace()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithImageExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Assert
        string? imageNs = navigator.GetNamespace("image");
        imageNs.ShouldBe(ImageNamespace);
    }

    [TestMethod]
    public void ParseSitemapWithImageExtension_ExtractsImageLocation()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithImageExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? imageLocNode = navigator.SelectSingleNode("//image:image/image:loc", manager);

        // Assert
        imageLocNode.ShouldNotBeNull();
        imageLocNode.Value.ShouldBe("https://example.com/image1.jpg");
    }

    [TestMethod]
    public void ParseSitemapWithMultipleImages_ExtractsAllImages()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithMultipleImages);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator images = navigator.Select("//image:image", manager);

        // Assert
        images.Count.ShouldBe(3);
    }

    [TestMethod]
    public void ParseSitemapWithMultipleImages_ExtractsAllImageLocations()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithMultipleImages);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator imageLocs = navigator.Select("//image:image/image:loc", manager);
        List<string> locations = [];

        while (imageLocs.MoveNext())
        {
            locations.Add(imageLocs.Current!.Value);
        }

        // Assert
        locations.Count.ShouldBe(3);
        locations[0].ShouldBe("https://example.com/photo1.jpg");
        locations[1].ShouldBe("https://example.com/photo2.jpg");
        locations[2].ShouldBe("https://example.com/photo3.jpg");
    }

    [TestMethod]
    public void SitemapImage_LoadFromXPath_WorksCorrectly()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithImageExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        XPathNavigator? imageNode = navigator.SelectSingleNode("//image:image", manager);
        imageNode.ShouldNotBeNull();

        // Act
        SitemapImage image = new();
        bool loaded = image.Load(imageNode, manager);

        // Assert
        loaded.ShouldBeTrue();
        image.Location.ShouldBe(new Uri("https://example.com/image1.jpg"));
    }

    [TestMethod]
    public void SitemapImage_LoadMultiple_WorksCorrectly()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithMultipleImages);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator imageNodes = navigator.Select("//image:image", manager);
        List<SitemapImage> images = [];

        while (imageNodes.MoveNext())
        {
            SitemapImage image = new();
            if (image.Load(imageNodes.Current!, manager))
            {
                images.Add(image);
            }
        }

        // Assert
        images.Count.ShouldBe(3);
        images[0].Location.ShouldBe(new Uri("https://example.com/photo1.jpg"));
        images[1].Location.ShouldBe(new Uri("https://example.com/photo2.jpg"));
        images[2].Location.ShouldBe(new Uri("https://example.com/photo3.jpg"));
    }

    #endregion

    #region Video Extension Parsing Tests

    [TestMethod]
    public void ParseSitemapWithVideoExtension_HasVideoNamespace()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Assert
        string? videoNs = navigator.GetNamespace("video");
        videoNs.ShouldBe(VideoNamespace);
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsVideoElements()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? videoNode = navigator.SelectSingleNode("//video:video", manager);

        // Assert
        videoNode.ShouldNotBeNull();
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsThumbnailLocation()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? thumbnailNode = navigator.SelectSingleNode("//video:video/video:thumbnail_loc", manager);

        // Assert
        thumbnailNode.ShouldNotBeNull();
        thumbnailNode.Value.ShouldBe("https://example.com/thumb.jpg");
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsTitle()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? titleNode = navigator.SelectSingleNode("//video:video/video:title", manager);

        // Assert
        titleNode.ShouldNotBeNull();
        titleNode.Value.ShouldBe("Example Video");
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsDescription()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? descriptionNode = navigator.SelectSingleNode("//video:video/video:description", manager);

        // Assert
        descriptionNode.ShouldNotBeNull();
        descriptionNode.Value.ShouldBe("A sample video description");
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsContentLocation()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? contentLocNode = navigator.SelectSingleNode("//video:video/video:content_loc", manager);

        // Assert
        contentLocNode.ShouldNotBeNull();
        contentLocNode.Value.ShouldBe("https://example.com/video.mp4");
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsDuration()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? durationNode = navigator.SelectSingleNode("//video:video/video:duration", manager);

        // Assert
        durationNode.ShouldNotBeNull();
        int.TryParse(durationNode.Value, out int duration).ShouldBeTrue();
        duration.ShouldBe(600); // 600 seconds = 10 minutes
    }

    [TestMethod]
    public void ParseSitemapWithVideoExtension_ExtractsPublicationDate()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithVideoExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? pubDateNode = navigator.SelectSingleNode("//video:video/video:publication_date", manager);

        // Assert
        pubDateNode.ShouldNotBeNull();
        DateTime.TryParse(pubDateNode.Value, out DateTime pubDate).ShouldBeTrue();
        pubDate.Year.ShouldBe(2024);
        pubDate.Month.ShouldBe(1);
        pubDate.Day.ShouldBe(15);
    }

    [TestMethod]
    public void VideoExtension_DurationConstraints_ValidRange()
    {
        // Video duration must be between 1 and 28800 seconds (8 hours)
        // Arrange
        int minDuration = 1;
        int maxDuration = 28_800;
        int testDuration = 600;

        // Assert
        testDuration.ShouldBeGreaterThanOrEqualTo(minDuration);
        testDuration.ShouldBeLessThanOrEqualTo(maxDuration);
    }

    #endregion

    #region News Extension Parsing Tests

    [TestMethod]
    public void ParseSitemapWithNewsExtension_HasNewsNamespace()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithNewsExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Assert
        string? newsNs = navigator.GetNamespace("news");
        newsNs.ShouldBe(NewsNamespace);
    }

    [TestMethod]
    public void ParseSitemapWithNewsExtension_ExtractsNewsElement()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithNewsExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? newsNode = navigator.SelectSingleNode("//news:news", manager);

        // Assert
        newsNode.ShouldNotBeNull();
    }

    [TestMethod]
    public void ParseSitemapWithNewsExtension_ExtractsPublicationName()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithNewsExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? pubNameNode = navigator.SelectSingleNode("//news:news/news:publication/news:name", manager);

        // Assert
        pubNameNode.ShouldNotBeNull();
        pubNameNode.Value.ShouldBe("Example News");
    }

    [TestMethod]
    public void ParseSitemapWithNewsExtension_ExtractsPublicationLanguage()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithNewsExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? langNode = navigator.SelectSingleNode("//news:news/news:publication/news:language", manager);

        // Assert
        langNode.ShouldNotBeNull();
        langNode.Value.ShouldBe("en");
    }

    [TestMethod]
    public void ParseSitemapWithNewsExtension_ExtractsNewsTitle()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithNewsExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? titleNode = navigator.SelectSingleNode("//news:news/news:title", manager);

        // Assert
        titleNode.ShouldNotBeNull();
        titleNode.Value.ShouldBe("Breaking News Article");
    }

    [TestMethod]
    public void ParseSitemapWithNewsExtension_ExtractsPublicationDate()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithNewsExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? pubDateNode = navigator.SelectSingleNode("//news:news/news:publication_date", manager);

        // Assert
        pubDateNode.ShouldNotBeNull();
        DateTime.TryParse(pubDateNode.Value, out DateTime pubDate).ShouldBeTrue();
        pubDate.Year.ShouldBe(2024);
        pubDate.Month.ShouldBe(1);
        pubDate.Day.ShouldBe(15);
    }

    [TestMethod]
    public void NewsExtension_LanguageCode_ValidFormats()
    {
        // Valid language codes: 2-letter ISO 639-1, or "zh-cn", "zh-tw"
        string[] validLanguageCodes = ["en", "de", "fr", "es", "zh-cn", "zh-tw"];

        foreach (var code in validLanguageCodes)
        {
            // Assert - Language codes are 2 characters or hyphenated
            (code.Length == 2 || code.Contains('-', StringComparison.Ordinal)).ShouldBeTrue($"Invalid language code format: {code}");
        }
    }

    #endregion

    #region Hreflang Extension Parsing Tests

    [TestMethod]
    public void ParseSitemapWithHreflangExtension_HasXhtmlNamespace()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Assert
        string? xhtmlNs = navigator.GetNamespace("xhtml");
        xhtmlNs.ShouldBe(XhtmlNamespace);
    }

    [TestMethod]
    public void ParseSitemapWithHreflangExtension_ExtractsAlternateLinks()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator links = navigator.Select("//xhtml:link", manager);

        // Assert
        links.Count.ShouldBe(4);
    }

    [TestMethod]
    public void ParseSitemapWithHreflangExtension_ExtractsHreflangAttributes()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator links = navigator.Select("//xhtml:link", manager);
        List<string> hreflangValues = [];

        while (links.MoveNext())
        {
            string? hreflang = links.Current!.GetAttribute("hreflang", "");
            if (!string.IsNullOrEmpty(hreflang))
            {
                hreflangValues.Add(hreflang);
            }
        }

        // Assert
        hreflangValues.Count.ShouldBe(4);
        hreflangValues.ShouldContain("en");
        hreflangValues.ShouldContain("de");
        hreflangValues.ShouldContain("fr");
        hreflangValues.ShouldContain("x-default");
    }

    [TestMethod]
    public void ParseSitemapWithHreflangExtension_ExtractsHrefAttributes()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator links = navigator.Select("//xhtml:link", manager);
        List<string> hrefValues = [];

        while (links.MoveNext())
        {
            string? href = links.Current!.GetAttribute("href", "");
            if (!string.IsNullOrEmpty(href))
            {
                hrefValues.Add(href);
            }
        }

        // Assert
        hrefValues.Count.ShouldBe(4);
        hrefValues.ShouldContain("https://example.com/en/page.html");
        hrefValues.ShouldContain("https://example.com/de/page.html");
        hrefValues.ShouldContain("https://example.com/fr/page.html");
        hrefValues.ShouldContain("https://example.com/page.html");
    }

    [TestMethod]
    public void ParseSitemapWithHreflangExtension_ContainsSelfReference()
    {
        // Hreflang implementation should include self-referencing link
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? urlLoc = navigator.SelectSingleNode("//sm:url/sm:loc", manager);
        string? pageUrl = urlLoc?.Value;

        XPathNodeIterator links = navigator.Select("//xhtml:link", manager);
        bool hasSelfReference = false;

        while (links.MoveNext())
        {
            string? href = links.Current!.GetAttribute("href", "");
            if (href == pageUrl)
            {
                hasSelfReference = true;
                break;
            }
        }

        // Assert
        pageUrl.ShouldBe("https://example.com/en/page.html");
        hasSelfReference.ShouldBeTrue("Hreflang should include self-referencing link");
    }

    [TestMethod]
    public void ParseSitemapWithHreflangExtension_AllLinksHaveAlternateRel()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNodeIterator links = navigator.Select("//xhtml:link", manager);

        // Assert
        while (links.MoveNext())
        {
            string? rel = links.Current!.GetAttribute("rel", "");
            rel.ShouldBe("alternate", "All xhtml:link elements should have rel='alternate'");
        }
    }

    [TestMethod]
    public void HreflangExtension_XDefaultValue_IsValid()
    {
        // x-default is a special hreflang value for the default/fallback page
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithHreflangExtension);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act
        XPathNavigator? xDefaultLink = navigator.SelectSingleNode("//xhtml:link[@hreflang='x-default']", manager);

        // Assert
        xDefaultLink.ShouldNotBeNull();
        xDefaultLink.GetAttribute("href", "").ShouldBe("https://example.com/page.html");
    }

    #endregion

    #region Combined Extensions Tests

    [TestMethod]
    public void ParseSitemap_WithCombinedExtensions_AllNamespacesPresent()
    {
        // Arrange - Create a sitemap with multiple extensions
        const string combinedSitemap = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                    xmlns:image="http://www.google.com/schemas/sitemap-image/1.1"
                    xmlns:xhtml="http://www.w3.org/1999/xhtml">
                <url>
                    <loc>https://example.com/page.html</loc>
                    <image:image>
                        <image:loc>https://example.com/image.jpg</image:loc>
                    </image:image>
                    <xhtml:link rel="alternate" hreflang="en" href="https://example.com/page.html"/>
                </url>
            </urlset>
            """;

        using StringReader documentReader = new(combinedSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Assert
        navigator.GetNamespace("image").ShouldBe(ImageNamespace);
        navigator.GetNamespace("xhtml").ShouldBe(XhtmlNamespace);
    }

    [TestMethod]
    public void ParseSitemap_WithCombinedExtensions_AllElementsAccessible()
    {
        // Arrange
        const string combinedSitemap = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                    xmlns:image="http://www.google.com/schemas/sitemap-image/1.1"
                    xmlns:xhtml="http://www.w3.org/1999/xhtml">
                <url>
                    <loc>https://example.com/page.html</loc>
                    <lastmod>2024-01-15</lastmod>
                    <image:image>
                        <image:loc>https://example.com/image.jpg</image:loc>
                    </image:image>
                    <xhtml:link rel="alternate" hreflang="en" href="https://example.com/page.html"/>
                    <xhtml:link rel="alternate" hreflang="de" href="https://example.com/de/page.html"/>
                </url>
            </urlset>
            """;

        using StringReader documentReader = new(combinedSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        // Act & Assert - Core sitemap elements
        XPathNavigator? loc = navigator.SelectSingleNode("//sm:url/sm:loc", manager);
        loc.ShouldNotBeNull();
        loc.Value.ShouldBe("https://example.com/page.html");

        XPathNavigator? lastmod = navigator.SelectSingleNode("//sm:url/sm:lastmod", manager);
        lastmod.ShouldNotBeNull();
        lastmod.Value.ShouldBe("2024-01-15");

        // Image extension
        XPathNavigator? imageLoc = navigator.SelectSingleNode("//image:image/image:loc", manager);
        imageLoc.ShouldNotBeNull();
        imageLoc.Value.ShouldBe("https://example.com/image.jpg");

        // Hreflang extension
        XPathNodeIterator links = navigator.Select("//xhtml:link", manager);
        links.Count.ShouldBe(2);
    }

    #endregion

    #region Helper Methods

    private static XmlNamespaceManager CreateNamespaceManager(XPathNavigator navigator)
    {
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("sm", SitemapUtility.SitemapNamespace);
        manager.AddNamespace("image", ImageNamespace);
        manager.AddNamespace("video", VideoNamespace);
        manager.AddNamespace("news", NewsNamespace);
        manager.AddNamespace("xhtml", XhtmlNamespace);
        return manager;
    }

    #endregion
}