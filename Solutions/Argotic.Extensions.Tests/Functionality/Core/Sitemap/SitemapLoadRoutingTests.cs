using System.Text;
using System.Xml.XPath;

using Argotic.Syndication;

using Shouldly;

using SitemapResource = Argotic.Syndication.Sitemap;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers what <c>Sitemap.Load</c> and <c>SitemapIndex.Load</c> accept now that they route through the
/// dispatcher like every other resource.
/// </summary>
/// <remarks>
///     <para>
///     The two sitemap resources were the only ones that bypassed <c>SyndicationResourceAdapter</c>,
///     running a private walk over the absolute expression <c>//sm:urlset/sm:url</c> with no format
///     check. Routing through the dispatcher buys the refusal every other resource already had: a
///     non-sitemap document, an un-namespaced root, a <c>urlset</c> nested below a foreign root, and a
///     navigator positioned on an element rather than the document node are all refused with a
///     <see cref="FormatException"/> instead of yielding an empty resource and a raised <c>Loaded</c>.
///     </para>
///     <para>
///     The element-positioned refusal is the deliberate cost of consistency: detection sniffs child
///     elements from where the navigator stands, exactly as it does for the other nine formats. The one
///     format family with a positioned-navigator concession is the Atom Publishing Protocol, whose
///     categories detection carries a self-arm because <c>AtomMemberResources</c> hands it an element;
///     nothing in this library hands a sitemap one.
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
    /// An RSS document handed to <c>Sitemap.Load</c> is refused with a message naming both formats, and
    /// <c>Loaded</c> is not raised.
    /// </summary>
    [TestMethod]
    public void ANonSitemapDocument_HandedToSitemapLoad_IsRefused()
    {
        // Arrange
        SitemapResource sitemap = new();
        int loadedCount = 0;
        sitemap.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss20Document);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => sitemap.Load(stream));

        // Assert
        exception.Message.ShouldContain("Rss");
        exception.Message.ShouldContain("Sitemap");
        sitemap.Urls.ShouldBeEmpty();
        loadedCount.ShouldBe(0);
    }

    /// <summary>
    /// A <c>urlset</c> without the sitemaps.org namespace is refused — the detection reports no format at
    /// all.
    /// </summary>
    [TestMethod]
    public void AnUnNamespacedUrlset_HandedToSitemapLoad_IsRefused()
    {
        // Arrange
        SitemapResource sitemap = new();
        using MemoryStream stream = StreamFor(UnNamespacedUrlset);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => sitemap.Load(stream));

        // Assert
        exception.Message.ShouldContain("None");
        exception.Message.ShouldContain("Sitemap");
        sitemap.Urls.ShouldBeEmpty();
    }

    /// <summary>
    /// A namespaced <c>urlset</c> nested below a foreign root is refused: the protocol requires
    /// <c>urlset</c> to be the document element, and only the old absolute <c>//</c> walk ever read one
    /// from deeper in the tree.
    /// </summary>
    [TestMethod]
    public void ANestedUrlset_HandedToSitemapLoad_IsRefused()
    {
        // Arrange
        SitemapResource sitemap = new();
        using MemoryStream stream = StreamFor(NestedUrlset);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => sitemap.Load(stream));

        // Assert
        exception.Message.ShouldContain("None");
        exception.Message.ShouldContain("Sitemap");
        sitemap.Urls.ShouldBeEmpty();
    }

    /// <summary>
    /// A conforming document behind a navigator positioned on the <c>urlset</c> element is refused —
    /// detection sniffs from where the navigator stands, for sitemaps exactly as for every other format.
    /// </summary>
    [TestMethod]
    public void AConformingSitemap_LoadedFromANavigatorPositionedOnUrlset_IsRefused()
    {
        // Arrange
        using MemoryStream stream = StreamFor(ConformingSitemap);
        XPathDocument document = new(stream);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToChild("urlset", SitemapNamespace).ShouldBeTrue();
        SitemapResource sitemap = new();

        // Act
        FormatException exception = Should.Throw<FormatException>(() => sitemap.Load(navigator));

        // Assert
        exception.Message.ShouldContain("None");
        exception.Message.ShouldContain("Sitemap");
        sitemap.Urls.ShouldBeEmpty();
    }

    /// <summary>
    /// An RSS document handed to <c>SitemapIndex.Load</c> is refused, and <c>Loaded</c> is not raised.
    /// </summary>
    [TestMethod]
    public void ANonSitemapDocument_HandedToSitemapIndexLoad_IsRefused()
    {
        // Arrange
        SitemapIndex index = new();
        int loadedCount = 0;
        index.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss20Document);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => index.Load(stream));

        // Assert
        exception.Message.ShouldContain("Rss");
        exception.Message.ShouldContain("SitemapIndex");
        index.Sitemaps.ShouldBeEmpty();
        loadedCount.ShouldBe(0);
    }

    /// <summary>
    /// A <c>sitemapindex</c> without the sitemaps.org namespace is refused.
    /// </summary>
    [TestMethod]
    public void AnUnNamespacedSitemapindex_HandedToSitemapIndexLoad_IsRefused()
    {
        // Arrange
        SitemapIndex index = new();
        using MemoryStream stream = StreamFor(UnNamespacedSitemapindex);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => index.Load(stream));

        // Assert
        exception.Message.ShouldContain("None");
        exception.Message.ShouldContain("SitemapIndex");
        index.Sitemaps.ShouldBeEmpty();
    }

    /// <summary>
    /// A namespaced <c>sitemapindex</c> nested below a foreign root is refused.
    /// </summary>
    [TestMethod]
    public void ANestedSitemapindex_HandedToSitemapIndexLoad_IsRefused()
    {
        // Arrange
        SitemapIndex index = new();
        using MemoryStream stream = StreamFor(NestedSitemapindex);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => index.Load(stream));

        // Assert
        exception.Message.ShouldContain("None");
        exception.Message.ShouldContain("SitemapIndex");
        index.Sitemaps.ShouldBeEmpty();
    }

    /// <summary>
    /// A conforming document behind a navigator positioned on the <c>sitemapindex</c> element is refused.
    /// </summary>
    [TestMethod]
    public void AConformingSitemapIndex_LoadedFromANavigatorPositionedOnSitemapindex_IsRefused()
    {
        // Arrange
        using MemoryStream stream = StreamFor(ConformingSitemapIndex);
        XPathDocument document = new(stream);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToChild("sitemapindex", SitemapNamespace).ShouldBeTrue();
        SitemapIndex index = new();

        // Act
        FormatException exception = Should.Throw<FormatException>(() => index.Load(navigator));

        // Assert
        exception.Message.ShouldContain("None");
        exception.Message.ShouldContain("SitemapIndex");
        index.Sitemaps.ShouldBeEmpty();
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