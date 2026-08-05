using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads a sitemap at the sizes the protocol actually permits.
/// </summary>
/// <remarks>
/// <para>
/// The sitemap protocol caps a single document at 50,000 URLs, and <c>SitemapIndex</c> documents that
/// limit in its own remarks. The existing sweep stops at 1,000 URLs - about 162 KB - against a legal
/// ceiling near 8.2 MB. That is fifty times past anything the harness had measured, and it is the one
/// place in this library where a super-linear term would actually reach a user: a site with a
/// hundred thousand pages produces two documents at exactly this size, every time it publishes.
/// </para>
/// <para>
/// The point of the sweep is the shape, not the absolute numbers. If cost per URL is flat from 1,000
/// to 50,000 there is nothing here; if it climbs, the climb is the finding.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "sitemap", "scale")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SitemapScaleBenchmarks
{
    private byte[] document = [];

    /// <summary>
    /// Gets or sets the number of URLs in the sitemap under test.
    /// </summary>
    /// <remarks>
    /// 50,000 is the protocol maximum, not an arbitrary large number.
    /// </remarks>
    [Params(1_000, 10_000, 50_000)]
    public int UrlCount { get; set; }

    /// <summary>
    /// Generates the sitemap once per parameter value.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.document = FeedCorpus.GenerateSitemapUtf8(this.UrlCount);

    /// <summary>
    /// Parses the sitemap.
    /// </summary>
    /// <returns>The parsed sitemap.</returns>
    [Benchmark(Description = "Sitemap.Load(Stream)")]
    public Sitemap LoadSitemap()
    {
        using MemoryStream stream = new(this.document, writable: false);
        Sitemap sitemap = new();
        sitemap.Load(stream);
        return sitemap;
    }
}