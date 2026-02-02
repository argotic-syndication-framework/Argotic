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
/// Unit tests for <see cref="Sitemap09SyndicationResourceAdapter"/> that verify Sitemap 0.9 parsing.
/// </summary>
[TestClass]
public class Sitemap09SyndicationResourceAdapterTests
{
    #region Retrieval Limit Tests

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
