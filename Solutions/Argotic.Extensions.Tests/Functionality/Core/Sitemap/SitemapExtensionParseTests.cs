using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Parses the sitemap extension fixtures through <see cref="Argotic.Syndication.Sitemap"/>.
/// </summary>
/// <remarks>
///     <para>
///     The fixtures these use have existed in <see cref="FeedTestData"/> all along. The tests that
///     referenced them loaded the string into a raw <c>XPathDocument</c> and asserted with
///     <c>SelectSingleNode</c>, which verifies that a string literal contains what the string literal
///     contains - it exercises <c>System.Xml.XPath</c>, not this library.
///     </para>
///     <para>
///     The consequence was measurable: <c>SitemapNewsExtension</c> sat at 4.3% with its <c>Load</c>
///     cold at 39 of 39 lines <i>including its guard clauses</i>, which is proof the method was never
///     entered. <c>SitemapHreflangExtension.Load</c> reported partial coverage only by accident - the
///     one thing that had ever invoked it was an unrelated Atom 0.3 fixture that happens to declare
///     <c>xmlns:xhtml</c>, and it found no links and returned false.
///     </para>
/// </remarks>
[TestClass]
public class SitemapExtensionParseTests
{
    /// <summary>
    /// The news extension is attached to the url and carries its publication.
    /// </summary>
    [TestMethod]
    public void ASitemapWithTheNewsExtension_AttachesItToTheUrl()
    {
        SitemapUrl url = LoadSingleUrl(FeedTestData.SitemapWithNewsExtension);

        SitemapNewsExtension news = url.Extensions.OfType<SitemapNewsExtension>().ShouldHaveSingleItem();
        news.Title.ShouldBe("Breaking News Article");
        news.Publication.ShouldNotBeNull();
        news.Publication.Name.ShouldBe("Example News");
        news.Publication.Language.ShouldBe("en");
        news.PublicationDate.Date.ShouldBe(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc).Date);
    }

    /// <summary>
    /// The image extension is attached to the url and carries its images.
    /// </summary>
    [TestMethod]
    public void ASitemapWithTheImageExtension_AttachesItToTheUrl()
    {
        SitemapUrl url = LoadSingleUrl(FeedTestData.SitemapWithImageExtension);

        SitemapImageExtension images = url.Extensions.OfType<SitemapImageExtension>().ShouldHaveSingleItem();
        images.Images.ShouldNotBeEmpty();
        images.Images[0].Location.ShouldNotBeNull();
    }

    /// <summary>
    /// Every image in a multi-image sitemap is attached.
    /// </summary>
    [TestMethod]
    public void ASitemapWithSeveralImages_AttachesEveryOne()
    {
        SitemapUrl url = LoadSingleUrl(FeedTestData.SitemapWithMultipleImages);

        SitemapImageExtension images = url.Extensions.OfType<SitemapImageExtension>().ShouldHaveSingleItem();
        images.Images.Count.ShouldBeGreaterThan(1);
        images.Images.ShouldAllBe(image => image.Location != null);
    }

    /// <summary>
    /// The video extension is attached to the url and carries its video.
    /// </summary>
    [TestMethod]
    public void ASitemapWithTheVideoExtension_AttachesItToTheUrl()
    {
        SitemapUrl url = LoadSingleUrl(FeedTestData.SitemapWithVideoExtension);

        SitemapVideoExtension videos = url.Extensions.OfType<SitemapVideoExtension>().ShouldHaveSingleItem();
        SitemapVideo video = videos.Videos.ShouldHaveSingleItem();
        video.Title.ShouldNotBeNullOrEmpty();
        video.Description.ShouldNotBeNullOrEmpty();
        video.ThumbnailLocation.ShouldNotBeNull();
    }

    /// <summary>
    /// The hreflang extension is attached to the url and carries its alternates.
    /// </summary>
    [TestMethod]
    public void ASitemapWithTheHreflangExtension_AttachesItToTheUrl()
    {
        SitemapUrl url = LoadSingleUrl(FeedTestData.SitemapWithHreflangExtension);

        SitemapHreflangExtension hreflang = url.Extensions.OfType<SitemapHreflangExtension>().ShouldHaveSingleItem();
        hreflang.Links.ShouldNotBeEmpty();
        hreflang.Links.ShouldAllBe(link => link.Href != null);
        hreflang.Links.ShouldAllBe(link => link.Hreflang.Length > 0);
    }

    /// <summary>
    /// A sitemap declaring no extension namespace attaches no extensions.
    /// </summary>
    /// <remarks>
    ///     The negative case. Without it, a change that attached every extension to every url would
    ///     satisfy all of the above.
    /// </remarks>
    [TestMethod]
    public void ASitemapWithNoExtensionNamespaces_AttachesNothing()
    {
        SitemapUrl url = LoadSingleUrl(FeedTestData.MinimalSitemap);

        url.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// A single image loads from a navigator positioned on its element.
    /// </summary>
    /// <remarks>
    ///     Carried over from the file this replaces, which contained two tests that genuinely exercised
    ///     a product type among twenty-nine that did not.
    /// </remarks>
    [TestMethod]
    public void AnImage_LoadsFromANavigatorPositionedOnIt()
    {
        using StringReader documentReader = new(FeedTestData.SitemapWithImageExtension);
        XPathNavigator navigator = new XPathDocument(documentReader).CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

        XPathNavigator imageNode = navigator.SelectSingleNode("//image:image", manager).ShouldNotBeNull();

        SitemapImage image = new();

        image.Load(imageNode, manager).ShouldBeTrue();
        image.Location.ShouldBe(new Uri("https://example.com/image1.jpg"));
    }

    /// <summary>
    /// Several images load in document order.
    /// </summary>
    [TestMethod]
    public void SeveralImages_LoadInDocumentOrder()
    {
        using StringReader documentReader = new(FeedTestData.SitemapWithMultipleImages);
        XPathNavigator navigator = new XPathDocument(documentReader).CreateNavigator();
        XmlNamespaceManager manager = CreateNamespaceManager(navigator);

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

        images.Count.ShouldBe(3);
        images[0].Location.ShouldBe(new Uri("https://example.com/photo1.jpg"));
        images[1].Location.ShouldBe(new Uri("https://example.com/photo2.jpg"));
        images[2].Location.ShouldBe(new Uri("https://example.com/photo3.jpg"));
    }

    private static XmlNamespaceManager CreateNamespaceManager(XPathNavigator navigator)
    {
        XmlNamespaceManager manager = new(navigator.NameTable);
        manager.AddNamespace("sm", "http://www.sitemaps.org/schemas/sitemap/0.9");
        manager.AddNamespace("image", "http://www.google.com/schemas/sitemap-image/1.1");
        manager.AddNamespace("video", "http://www.google.com/schemas/sitemap-video/1.1");
        manager.AddNamespace("news", "http://www.google.com/schemas/sitemap-news/0.9");
        manager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
        return manager;
    }

    private static SitemapUrl LoadSingleUrl(string xml)
    {
        Argotic.Syndication.Sitemap sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        sitemap.Load(stream);

        sitemap.Urls.ShouldNotBeEmpty("the fixture produced no urls, so nothing downstream is being tested");
        return sitemap.Urls[0];
    }
}