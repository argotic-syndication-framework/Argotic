using System.Text;

using Argotic.Extensions.Core;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Validates the sitemaps this library writes, and the ones in the sample corpus, against the published
/// sitemaps.org schemas.
/// </summary>
/// <remarks>
///     <para>
///     <b>Validating is the point.</b> A round-trip test asserts that the reader understands the writer,
///     which stays true when both are wrong together. <c>SitemapVideo.WriteTo</c> emitted its optional
///     elements in an order Google's schema rejects for the life of the extension, and every round-trip
///     test agreed with it, because the reader accepts children in any order. Only a validating parser
///     could disagree.
///     </para>
///     <para>
///     <b>Two halves, and both are needed.</b> The output tests catch writer defects. The corpus tests
///     catch fixture defects, which are not hypothetical: <c>sitemap_image.xml</c> and
///     <c>sitemap_video.xml</c> were both schema-invalid for as long as they had existed, and a green
///     suite said nothing.
///     </para>
///     <para>
///     Only the sitemaps.org schemas are embedded, so this class covers <c>urlset</c>,
///     <c>sitemapindex</c> and hreflang. The Google news, image and video extensions are validated by
///     <c>Scenarios/ValidateAgainstPublishedSchemas</c>, which fetches their schemas at test time
///     because they may not be redistributed — see <c>Schemas/NOTICE.md</c>.
///     </para>
/// </remarks>
[TestClass]
public class SitemapSchemaConformanceTests
{
    /// <summary>
    /// Gets the corpus sitemap documents the embedded schemas can validate.
    /// </summary>
    /// <remarks>
    ///     The news, image and video samples are absent deliberately: their extension schemas are not
    ///     embedded, and <c>sitemap-0.9.xsd</c> wildcards foreign elements with
    ///     <c>processContents="strict"</c>, so validating them here would report "not declared" for a
    ///     document that is perfectly correct.
    /// </remarks>
    public static IEnumerable<object[]> CorpusSitemaps =>
    [
        [SampleFeeds.Sitemap],
        [SampleFeeds.SitemapIndex],
        [SampleFeeds.SitemapHreflang],
    ];

    /// <summary>
    /// Every corpus sitemap the embedded schemas cover conforms to them.
    /// </summary>
    /// <param name="fileName">The sample document under test.</param>
    [TestMethod]
    [DynamicData(nameof(CorpusSitemaps))]
    public void ACorpusSitemap_ConformsToTheSitemapsOrgSchema(string fileName)
    {
        string document = SampleFeeds.ReadAllText(fileName);

        ConformanceSchemas.Describe(document, ConformanceSchemas.Sitemap)
            .ShouldBeEmpty($"{fileName} does not conform to the published sitemap schema");
    }

    /// <summary>
    /// A sitemap carrying every core element this library writes conforms once saved.
    /// </summary>
    [TestMethod]
    public void ASitemapCarryingEveryCoreElement_IsWrittenConformantly()
    {
        Syndication.Sitemap sitemap = new();

        // One url per changefreq the protocol defines, so the whole enumeration is written and checked
        // rather than the one value a real site mostly uses.
        foreach ((string path, SitemapChangeFrequency frequency, decimal priority) in
            ((string, SitemapChangeFrequency, decimal)[])
            [
                ("https://endjin.com/", SitemapChangeFrequency.Always, 1.0m),
                ("https://endjin.com/blog/", SitemapChangeFrequency.Hourly, 0.9m),
                ("https://endjin.com/what-we-think/", SitemapChangeFrequency.Daily, 0.8m),
                ("https://endjin.com/what-we-do/", SitemapChangeFrequency.Weekly, 0.7m),
                ("https://endjin.com/who-we-are/", SitemapChangeFrequency.Monthly, 0.6m),
                ("https://endjin.com/contact-us/", SitemapChangeFrequency.Yearly, 0.5m),
                ("https://endjin.com/what-we-do/pricing/", SitemapChangeFrequency.Never, 0.0m),
            ])
        {
            sitemap.Urls.Add(new SitemapUrl
            {
                Location = new Uri(path),
                LastModified = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
                ChangeFrequency = frequency,
                Priority = priority,
            });
        }

        ConformanceSchemas.Describe(Save(sitemap), ConformanceSchemas.Sitemap)
            .ShouldBeEmpty("Sitemap.Save wrote a document the published schema rejects");
    }

    /// <summary>
    /// A sitemap index conforms once saved, in both of the date forms the protocol allows.
    /// </summary>
    [TestMethod]
    public void ASitemapIndex_IsWrittenConformantly()
    {
        SitemapIndex index = new();

        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://endjin.com/sitemap.xml"),
            LastModified = new DateTime(2026, 8, 7, 9, 30, 0, DateTimeKind.Utc),
        });

        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://endjin.com/sitemap-news.xml"),
        });

        ConformanceSchemas.Describe(Save(index), ConformanceSchemas.Sitemap)
            .ShouldBeEmpty("SitemapIndex.Save wrote a document the published schema rejects");
    }

    /// <summary>
    /// A sitemap carrying hreflang annotations conforms once saved.
    /// </summary>
    /// <remarks>
    ///     <c>x-default</c> is a legal <c>hreflang</c> value and is not a language tag, so this is also
    ///     the check that the library does not narrow the attribute to something stricter than Google
    ///     accepts.
    /// </remarks>
    [TestMethod]
    public void ASitemapCarryingHreflangAnnotations_IsWrittenConformantly()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
        };

        SitemapHreflangExtension hreflang = new();
        hreflang.Links.Add(new SitemapHreflangLink("x-default", new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released")));
        hreflang.Links.Add(new SitemapHreflangLink("en-gb", new Uri("https://endjin.com/en/what-we-think/talks/rxdotnet-v7-0-released")));
        hreflang.Links.Add(new SitemapHreflangLink("de", new Uri("https://endjin.com/de/what-we-think/talks/rxdotnet-v7-0-released")));

        url.Extensions.Add(hreflang);
        sitemap.Urls.Add(url);

        ConformanceSchemas.Describe(Save(sitemap), ConformanceSchemas.Sitemap)
            .ShouldBeEmpty("a sitemap carrying hreflang annotations was written non-conformantly");
    }

    /// <summary>
    /// Every corpus sitemap the embedded schemas cover still conforms after this library rewrites it.
    /// </summary>
    /// <param name="fileName">The sample document under test.</param>
    /// <remarks>
    ///     Load-then-save is where a writer defect shows up on a document that was correct going in, so
    ///     this is the shape that catches "we read it fine and wrote it wrong".
    /// </remarks>
    [TestMethod]
    [DynamicData(nameof(CorpusSitemaps))]
    public void ACorpusSitemap_StillConformsAfterBeingRewritten(string fileName)
    {
        string rewritten;

        if (SampleFeeds.ReadAllText(fileName).Contains("<sitemapindex", StringComparison.Ordinal))
        {
            SitemapIndex index = new();
            using (FileStream input = SampleFeeds.Open(fileName))
            {
                index.Load(input);
            }

            rewritten = Save(index);
        }
        else
        {
            Syndication.Sitemap sitemap = new();
            using (FileStream input = SampleFeeds.Open(fileName))
            {
                sitemap.Load(input);
            }

            rewritten = Save(sitemap);
        }

        ConformanceSchemas.Describe(rewritten, ConformanceSchemas.Sitemap)
            .ShouldBeEmpty($"{fileName} conformed on disk but not after being loaded and saved");
    }

    /// <summary>
    /// The validator reports a document the schema rejects, rather than passing everything.
    /// </summary>
    /// <remarks>
    ///     <b>The check on the checker.</b> Every other test here asserts an empty problem list, so all
    ///     of them would pass against a validator that never reported anything — a schema set that
    ///     failed to compile, a namespace mismatch, or the missing
    ///     <c>ReportValidationWarnings</c> flag would each produce exactly that. A priority of 2.0 is
    ///     outside the 0.0-1.0 the schema permits, so this must fail, and if it ever stops failing the
    ///     rest of the class is worthless.
    /// </remarks>
    [TestMethod]
    public void AnInvalidSitemap_IsReportedRatherThanAccepted()
    {
        const string outOfRangePriority = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url>
                <loc>https://endjin.com/</loc>
                <priority>2.0</priority>
              </url>
            </urlset>
            """;

        ConformanceSchemas.Validate(outOfRangePriority, ConformanceSchemas.Sitemap)
            .ShouldNotBeEmpty("the validator accepted a priority of 2.0, which the schema caps at 1.0");
    }

    private static string Save(Syndication.Sitemap sitemap)
    {
        using MemoryStream stream = new();
        sitemap.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string Save(SitemapIndex index)
    {
        using MemoryStream stream = new();
        index.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}