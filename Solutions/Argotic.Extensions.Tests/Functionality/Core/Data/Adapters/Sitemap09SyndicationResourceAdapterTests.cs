using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

using SitemapResource = Argotic.Syndication.Sitemap;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers the retrieval limit in <see cref="Sitemap09SyndicationResourceAdapter"/>, on both of the
/// documents it fills: a <c>urlset</c> read into a <c>Sitemap</c>, and a <c>sitemapindex</c> read into a
/// <c>SitemapIndex</c>.
/// </summary>
/// <remarks>
///     This adapter tests the limit <i>before</i> parsing each entry, where the Atom and BlogML adapters
///     test it after and throw one parsed entry away. On a sitemap, which may legitimately carry 50,000
///     URLs, that is the difference between reading the file and reading a prefix of it.
/// </remarks>
[TestClass]
public class Sitemap09SyndicationResourceAdapterTests
{
    #region Retrieval Limit Tests

    /// <summary>
    /// A retrieval limit of <c>1</c> stops a three-URL <c>urlset</c> at one URL.
    /// </summary>
    [TestMethod]
    public void Fill_Sitemap_WithRetrievalLimit_RespectsLimit()
    {
        // Arrange - FullSitemap has 3 URLs
        SitemapResource sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.FullSitemap));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 1
        };
        Sitemap09SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(sitemap);

        // Assert
        sitemap.Urls.Count.ShouldBe(1);
    }

    /// <summary>
    /// A retrieval limit of <c>2</c> stops the same three-URL <c>urlset</c> at two.
    /// </summary>
    [TestMethod]
    public void Fill_Sitemap_WithRetrievalLimitTwo_ReturnsTwoUrls()
    {
        // Arrange - FullSitemap has 3 URLs
        SitemapResource sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.FullSitemap));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 2
        };
        Sitemap09SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(sitemap);

        // Assert
        sitemap.Urls.Count.ShouldBe(2);
    }

    /// <summary>
    /// A retrieval limit of <c>0</c> means no limit, and all three URLs are read.
    /// </summary>
    [TestMethod]
    public void Fill_Sitemap_WithZeroRetrievalLimit_ReturnsAllUrls()
    {
        // Arrange - RetrievalLimit = 0 means no limit
        SitemapResource sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.FullSitemap));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 0
        };
        Sitemap09SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(sitemap);

        // Assert
        sitemap.Urls.Count.ShouldBe(3);
    }

    /// <summary>
    /// A retrieval limit of <c>1</c> stops a three-entry <c>sitemapindex</c> at one sitemap.
    /// </summary>
    [TestMethod]
    public void Fill_SitemapIndex_WithRetrievalLimit_RespectsLimit()
    {
        // Arrange - FullSitemapIndex has 3 sitemaps
        SitemapIndex sitemapIndex = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.FullSitemapIndex));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 1
        };
        Sitemap09SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(sitemapIndex);

        // Assert
        sitemapIndex.Sitemaps.Count.ShouldBe(1);
    }

    /// <summary>
    /// A retrieval limit of <c>2</c> stops the same <c>sitemapindex</c> at two sitemaps.
    /// </summary>
    [TestMethod]
    public void Fill_SitemapIndex_WithRetrievalLimitTwo_ReturnsTwoSitemaps()
    {
        // Arrange - FullSitemapIndex has 3 sitemaps
        SitemapIndex sitemapIndex = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.FullSitemapIndex));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 2
        };
        Sitemap09SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(sitemapIndex);

        // Assert
        sitemapIndex.Sitemaps.Count.ShouldBe(2);
    }

    /// <summary>
    /// A retrieval limit of <c>0</c> means no limit, and all three sitemaps are read.
    /// </summary>
    [TestMethod]
    public void Fill_SitemapIndex_WithZeroRetrievalLimit_ReturnsAllSitemaps()
    {
        // Arrange - RetrievalLimit = 0 means no limit
        SitemapIndex sitemapIndex = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.FullSitemapIndex));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 0
        };
        Sitemap09SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(sitemapIndex);

        // Assert
        sitemapIndex.Sitemaps.Count.ShouldBe(3);
    }

    #endregion
}