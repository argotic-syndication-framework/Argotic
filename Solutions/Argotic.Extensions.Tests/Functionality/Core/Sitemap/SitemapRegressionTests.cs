using System.Text;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Tests covering how sitemap URLs bind their extension data, honour load settings, and interpret dates.
/// </summary>
[TestClass]
public class SitemapRegressionTests
{
    private const string ImageSitemap = """
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
          <url>
            <loc>http://example.com/a</loc>
            <image:image><image:loc>http://example.com/1.jpg</image:loc></image:image>
            <image:image><image:loc>http://example.com/2.jpg</image:loc></image:image>
          </url>
          <url>
            <loc>http://example.com/b</loc>
            <image:image><image:loc>http://example.com/3.jpg</image:loc></image:image>
          </url>
        </urlset>
        """;

    private static Argotic.Syndication.Sitemap LoadSitemap(string xml, SyndicationResourceLoadSettings? settings = null)
    {
        Argotic.Syndication.Sitemap sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        if (settings is null)
        {
            sitemap.Load(stream);
        }
        else
        {
            sitemap.Load(stream, settings);
        }

        return sitemap;
    }

    private static int ImageCountFor(SitemapUrl url) =>
        url.Extensions.OfType<SitemapImageExtension>().FirstOrDefault()?.Images.Count ?? 0;

    [TestMethod]
    public void Load_BindsImagesToTheUrlTheyAppearUnder()
    {
        // Arrange & Act
        // Each extension is handed the navigator for its own <url>; selecting from the document root
        // instead would give every URL every image in the file.
        Argotic.Syndication.Sitemap sitemap = LoadSitemap(ImageSitemap);

        // Assert
        sitemap.Urls.Count.ShouldBe(2);
        ImageCountFor(sitemap.Urls[0]).ShouldBe(2);
        ImageCountFor(sitemap.Urls[1]).ShouldBe(1);
    }

    [TestMethod]
    public void Load_WithRetrievalLimit_StopsAtTheLimit()
    {
        // Arrange & Act
        Argotic.Syndication.Sitemap sitemap = LoadSitemap(ImageSitemap, new SyndicationResourceLoadSettings { RetrievalLimit = 1 });

        // Assert
        sitemap.Urls.Count.ShouldBe(1);
    }

    [TestMethod]
    public void Load_WithoutRetrievalLimit_ReadsEveryUrl()
    {
        // Arrange & Act
        Argotic.Syndication.Sitemap sitemap = LoadSitemap(ImageSitemap, new SyndicationResourceLoadSettings());

        // Assert
        sitemap.Urls.Count.ShouldBe(2);
    }

    [TestMethod]
    public void Load_WithDateOnlyLastModified_KeepsTheStatedCalendarDayInUtc()
    {
        // Arrange
        // A date-only W3C value carries no offset; interpreting it in the machine's zone would move
        // the modification date onto the neighbouring day for any host that is not at UTC.
        string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url><loc>http://example.com/a</loc><lastmod>2024-01-15</lastmod></url>
            </urlset>
            """;

        // Act
        Argotic.Syndication.Sitemap sitemap = LoadSitemap(xml);
        DateTime? lastModified = sitemap.Urls[0].LastModified;

        // Assert
        lastModified.HasValue.ShouldBeTrue();
        lastModified.Value.Kind.ShouldBe(DateTimeKind.Utc);
        lastModified.Value.ShouldBe(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void SitemapIndex_WithRetrievalLimit_StopsAtTheLimit()
    {
        // Arrange
        string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <sitemap><loc>http://example.com/sitemap1.xml</loc></sitemap>
              <sitemap><loc>http://example.com/sitemap2.xml</loc></sitemap>
              <sitemap><loc>http://example.com/sitemap3.xml</loc></sitemap>
            </sitemapindex>
            """;
        SitemapIndex index = new();

        // Act
        using (MemoryStream stream = new(Encoding.UTF8.GetBytes(xml)))
        {
            index.Load(stream, new SyndicationResourceLoadSettings { RetrievalLimit = 2 });
        }

        // Assert
        index.Sitemaps.Count.ShouldBe(2);
    }
}