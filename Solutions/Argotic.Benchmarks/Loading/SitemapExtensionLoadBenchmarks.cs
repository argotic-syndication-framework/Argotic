using System.Diagnostics.CodeAnalysis;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices loading each Google sitemap extension family over the plain-urlset floor.
/// </summary>
/// <remarks>
/// <para>
/// The four families total 930 lines with zero benchmark coverage — <c>SitemapVideo</c> alone is
/// 519 (<c>.endjin/build-warnings.md</c> §19). §2.40 priced the extension-free sitemap; the
/// extension-bearing shapes real publishers serve — Yoast's 14-child videos, BBC/NYT/Guardian
/// news, GitLab's attribute-only hreflang pages — had no number at all. The a→b/c/d/e deltas are
/// each family's parse price over the same URL count.
/// </para>
/// <para>
/// Parameters stop at 1,000: real extension-bearing sitemaps sit there or below (Yoast 130
/// videos, BBC 985 news URLs), and the 50,000-URL protocol ceiling is already priced by
/// <c>SitemapScaleBenchmarks</c> for the plain shape.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "sitemap", "extensions", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SitemapExtensionLoadBenchmarks
{
    private byte[] plain = [];
    private byte[] video = [];
    private byte[] news = [];
    private byte[] image = [];
    private byte[] hreflang = [];

    /// <summary>
    /// Gets or sets the number of URLs in each sitemap under test.
    /// </summary>
    [Params(100, 1000)]
    public int UrlCount { get; set; }

    /// <summary>
    /// Generates every corpus once and parse-verifies counts, the expected extension on the first
    /// URL, and that the video arm's intra-document optionality survived generation.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.plain = FeedCorpus.GenerateSitemapUtf8(this.UrlCount);
        this.video = SitemapExtensionCorpus.GenerateVideoSitemapUtf8(this.UrlCount);
        this.news = SitemapExtensionCorpus.GenerateNewsSitemapUtf8(this.UrlCount);
        this.image = SitemapExtensionCorpus.GenerateImageSitemapUtf8(this.UrlCount);
        this.hreflang = SitemapExtensionCorpus.GenerateHreflangSitemapUtf8(this.UrlCount, localeCount: 8);

        Sitemap plainMap = Load(this.plain);
        Verify(plainMap.Urls.Count == this.UrlCount, "plain URL count");
        Verify(!plainMap.Urls.First().HasExtensions, "plain arm must attach nothing");

        Sitemap videoMap = Load(this.video);
        Verify(videoMap.Urls.Count == this.UrlCount, "video URL count");
        SitemapVideo firstVideo = FirstExtension<SitemapVideoExtension>(videoMap, "video").Videos.First();
        Verify(firstVideo.ContentLocation is not null, "video optionality: url 0 carries content_loc");
        SitemapVideo thirdVideo = FirstExtension<SitemapVideoExtension>(videoMap, "video", urlIndex: 2).Videos.First();
        Verify(thirdVideo.ViewCount is null, "video optionality: url 2 omits view_count");

        Sitemap newsMap = Load(this.news);
        Verify(newsMap.Urls.Count == this.UrlCount, "news URL count");
        Verify(FirstExtension<SitemapNewsExtension>(newsMap, "news") is not null, "news extension attached");

        Sitemap imageMap = Load(this.image);
        Verify(imageMap.Urls.Count == this.UrlCount, "image URL count");
        Verify(FirstExtension<SitemapImageExtension>(imageMap, "image").Images.Count == 2, "two images per URL");

        Sitemap hreflangMap = Load(this.hreflang);
        Verify(hreflangMap.Urls.Count == this.UrlCount, "hreflang URL count");
        Verify(FirstExtension<SitemapHreflangExtension>(hreflangMap, "hreflang").Links.Count == 8, "eight locale links per URL");
    }

    /// <summary>
    /// Loads the plain urlset — the floor every extension delta is measured against.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. Plain urlset")]
    public int LoadPlain() => Load(this.plain).Urls.Count;

    /// <summary>
    /// Loads the video sitemap — the richest family, in the Yoast shape.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. Video extension")]
    public int LoadVideo() => Load(this.video).Urls.Count;

    /// <summary>
    /// Loads the news sitemap — three real publisher shapes cycling.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. News extension")]
    public int LoadNews() => Load(this.news).Urls.Count;

    /// <summary>
    /// Loads the image sitemap — bare loc pairs, the only shape the wild still uses.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. Image extension")]
    public int LoadImage() => Load(this.image).Urls.Count;

    /// <summary>
    /// Loads the hreflang sitemap — empty elements, all payload in attributes.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "e. Hreflang links")]
    public int LoadHreflang() => Load(this.hreflang).Urls.Count;

    private static Sitemap Load(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        Sitemap sitemap = new();
        sitemap.Load(stream);
        return sitemap;
    }

    private static TExtension FirstExtension<TExtension>(Sitemap sitemap, string arm, int urlIndex = 0)
        where TExtension : class
    {
        TExtension? extension = sitemap.Urls.ElementAt(urlIndex).Extensions.OfType<TExtension>().FirstOrDefault();
        return extension ?? throw new InvalidOperationException($"Corpus guard failed: no {typeof(TExtension).Name} attached on the {arm} arm.");
    }

    private static void Verify(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Corpus guard failed: {description}.");
        }
    }
}