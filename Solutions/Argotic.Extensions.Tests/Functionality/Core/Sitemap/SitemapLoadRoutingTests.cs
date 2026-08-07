using System.Text;
using System.Xml.XPath;

using Argotic.Syndication;

using Shouldly;

using SitemapResource = Argotic.Syndication.Sitemap;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Pins what <c>Sitemap.Load</c> and <c>SitemapIndex.Load</c> accept, which is currently everything.
/// </summary>
/// <remarks>
///     <para>
///     The two sitemap resources are the only ones that do not route through
///     <c>SyndicationResourceAdapter</c>: their private walks select with the absolute expression
///     <c>//sm:urlset/sm:url</c> and never sniff the format, so a non-sitemap document yields an empty
///     resource and a raised <c>Loaded</c>, a nested <c>urlset</c> loads from anywhere in the tree, and a
///     navigator positioned on an element rather than the document node loads regardless — the absolute
///     path starts at the root no matter where the navigator stands.
///     </para>
///     <para>
///     Characterisations, not guards: each pins an answer the dispatcher rewiring deliberately inverts,
///     and exists so that the inversion is seen red before the behaviour moves.
///     </para>
/// </remarks>
[TestClass]
public class SitemapLoadRoutingTests
{
    private const string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private const string ConformingSitemap = """
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            <url><loc>http://example.com/a</loc></url>
        </urlset>
        """;

    private const string UnNamespacedUrlset = """
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset>
            <url><loc>http://example.com/a</loc></url>
        </urlset>
        """;

    private const string NestedUrlset = """
        <?xml version="1.0" encoding="UTF-8"?>
        <wrapper>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url><loc>http://example.com/a</loc></url>
            </urlset>
        </wrapper>
        """;

    private const string ConformingSitemapIndex = """
        <?xml version="1.0" encoding="UTF-8"?>
        <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            <sitemap><loc>http://example.com/sitemap1.xml</loc></sitemap>
        </sitemapindex>
        """;

    private const string UnNamespacedSitemapindex = """
        <?xml version="1.0" encoding="UTF-8"?>
        <sitemapindex>
            <sitemap><loc>http://example.com/sitemap1.xml</loc></sitemap>
        </sitemapindex>
        """;

    private const string NestedSitemapindex = """
        <?xml version="1.0" encoding="UTF-8"?>
        <wrapper>
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <sitemap><loc>http://example.com/sitemap1.xml</loc></sitemap>
            </sitemapindex>
        </wrapper>
        """;

    private const string Rss20Document = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
            <channel>
                <title>A Title</title>
                <link>http://example.com</link>
                <description>Not a sitemap.</description>
            </channel>
        </rss>
        """;

    private static MemoryStream StreamFor(string xml) => new(Encoding.UTF8.GetBytes(xml));

    /// <summary>
    /// An RSS document handed to <c>Sitemap.Load</c> yields an empty sitemap and a raised <c>Loaded</c>.
    /// </summary>
    [TestMethod]
    public void ANonSitemapDocument_HandedToSitemapLoad_YieldsAnEmptySitemapAndRaisesLoaded()
    {
        // Arrange
        SitemapResource sitemap = new();
        int loadedCount = 0;
        sitemap.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss20Document);

        // Act
        sitemap.Load(stream);

        // Assert
        sitemap.Urls.ShouldBeEmpty();
        loadedCount.ShouldBe(1);
    }

    /// <summary>
    /// A <c>urlset</c> without the sitemaps.org namespace yields an empty sitemap.
    /// </summary>
    [TestMethod]
    public void AnUnNamespacedUrlset_HandedToSitemapLoad_YieldsAnEmptySitemap()
    {
        // Arrange
        SitemapResource sitemap = new();
        using MemoryStream stream = StreamFor(UnNamespacedUrlset);

        // Act
        sitemap.Load(stream);

        // Assert
        sitemap.Urls.ShouldBeEmpty();
    }

    /// <summary>
    /// A namespaced <c>urlset</c> nested below a foreign root still loads — the absolute <c>//</c> walk
    /// finds it at any depth.
    /// </summary>
    [TestMethod]
    public void ANestedUrlset_HandedToSitemapLoad_ReadsTheNestedUrls()
    {
        // Arrange
        SitemapResource sitemap = new();
        using MemoryStream stream = StreamFor(NestedUrlset);

        // Act
        sitemap.Load(stream);

        // Assert
        sitemap.Urls.Count.ShouldBe(1);
        sitemap.Urls[0].Location.ShouldBe(new Uri("http://example.com/a"));
    }

    /// <summary>
    /// A conforming document behind a navigator positioned on the <c>urlset</c> element still loads — the
    /// absolute path starts at the document root regardless of where the navigator stands.
    /// </summary>
    [TestMethod]
    public void AConformingSitemap_LoadedFromANavigatorPositionedOnUrlset_ReadsTheUrls()
    {
        // Arrange
        using MemoryStream stream = StreamFor(ConformingSitemap);
        XPathDocument document = new(stream);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToChild("urlset", SitemapNamespace).ShouldBeTrue();
        SitemapResource sitemap = new();

        // Act
        sitemap.Load(navigator);

        // Assert
        sitemap.Urls.Count.ShouldBe(1);
        sitemap.Urls[0].Location.ShouldBe(new Uri("http://example.com/a"));
    }

    /// <summary>
    /// An RSS document handed to <c>SitemapIndex.Load</c> yields an empty index and a raised <c>Loaded</c>.
    /// </summary>
    [TestMethod]
    public void ANonSitemapDocument_HandedToSitemapIndexLoad_YieldsAnEmptyIndex()
    {
        // Arrange
        SitemapIndex index = new();
        int loadedCount = 0;
        index.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss20Document);

        // Act
        index.Load(stream);

        // Assert
        index.Sitemaps.ShouldBeEmpty();
        loadedCount.ShouldBe(1);
    }

    /// <summary>
    /// A <c>sitemapindex</c> without the sitemaps.org namespace yields an empty index.
    /// </summary>
    [TestMethod]
    public void AnUnNamespacedSitemapindex_HandedToSitemapIndexLoad_YieldsAnEmptyIndex()
    {
        // Arrange
        SitemapIndex index = new();
        using MemoryStream stream = StreamFor(UnNamespacedSitemapindex);

        // Act
        index.Load(stream);

        // Assert
        index.Sitemaps.ShouldBeEmpty();
    }

    /// <summary>
    /// A namespaced <c>sitemapindex</c> nested below a foreign root still loads.
    /// </summary>
    [TestMethod]
    public void ANestedSitemapindex_HandedToSitemapIndexLoad_ReadsTheNestedEntries()
    {
        // Arrange
        SitemapIndex index = new();
        using MemoryStream stream = StreamFor(NestedSitemapindex);

        // Act
        index.Load(stream);

        // Assert
        index.Sitemaps.Count.ShouldBe(1);
        index.Sitemaps[0].Location.ShouldBe(new Uri("http://example.com/sitemap1.xml"));
    }

    /// <summary>
    /// A conforming document behind a navigator positioned on the <c>sitemapindex</c> element still loads.
    /// </summary>
    [TestMethod]
    public void AConformingSitemapIndex_LoadedFromANavigatorPositionedOnSitemapindex_ReadsTheEntries()
    {
        // Arrange
        using MemoryStream stream = StreamFor(ConformingSitemapIndex);
        XPathDocument document = new(stream);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToChild("sitemapindex", SitemapNamespace).ShouldBeTrue();
        SitemapIndex index = new();

        // Act
        index.Load(navigator);

        // Assert
        index.Sitemaps.Count.ShouldBe(1);
        index.Sitemaps[0].Location.ShouldBe(new Uri("http://example.com/sitemap1.xml"));
    }

    /// <summary>
    /// A conforming sitemap loads its urls and raises <c>Loaded</c> exactly once.
    /// </summary>
    /// <remarks>
    ///     A guard, not a characterisation: green on both sides of the dispatcher rewiring.
    /// </remarks>
    [TestMethod]
    public void AConformingSitemap_RaisesLoadedExactlyOnce()
    {
        // Arrange
        SitemapResource sitemap = new();
        int loadedCount = 0;
        sitemap.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(ConformingSitemap);

        // Act
        sitemap.Load(stream);

        // Assert
        sitemap.Urls.Count.ShouldBe(1);
        loadedCount.ShouldBe(1);
    }
}