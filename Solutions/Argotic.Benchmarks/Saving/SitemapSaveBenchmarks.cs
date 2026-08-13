using System.Diagnostics.CodeAnalysis;

using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Saving;

/// <summary>
/// Prices saving sitemaps: plain, with <c>xsi:schemaLocation</c> stamping, and with the video and
/// hreflang extensions.
/// </summary>
/// <remarks>
/// <para>
/// No sitemap writer had a benchmark at all (<c>.endjin/build-warnings.md</c> §19) —
/// <c>SitemapSchemaLocations</c> and every extension's <c>WriteTo</c> sat at zero coverage. Arm c
/// is the instrument that would have caught the §13.5 regression: the video writer once emitted
/// elements in an order Google's schema rejects while every round-trip test agreed with it, and a
/// priced, guard-verified save path is where such a defect becomes observable the day it ships
/// rather than the day a publisher's search console complains.
/// </para>
/// <para>
/// Setup loads each document once (guarding counts and attachment) so the measurement is
/// serialisation only, mirroring <see cref="FeedSaveBenchmarks"/>.
/// </para>
/// </remarks>
[BenchmarkCategory("save", "sitemap", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SitemapSaveBenchmarks
{
    private Sitemap plain = new();
    private Sitemap video = new();
    private Sitemap hreflang = new();
    private SyndicationResourceSaveSettings schemaLocationSettings = new();

    /// <summary>
    /// Gets or sets the number of URLs in each sitemap under test.
    /// </summary>
    [Params(100, 1000)]
    public int UrlCount { get; set; }

    /// <summary>
    /// Loads the three documents once, guarding counts and extension attachment, so the arms
    /// measure serialisation rather than parsing.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.schemaLocationSettings = new SyndicationResourceSaveSettings { WriteXsiSchemaLocation = true };

        this.plain = Load(FeedCorpus.GenerateSitemapUtf8(this.UrlCount));
        this.video = Load(SitemapExtensionCorpus.GenerateVideoSitemapUtf8(this.UrlCount));
        this.hreflang = Load(SitemapExtensionCorpus.GenerateHreflangSitemapUtf8(this.UrlCount, localeCount: 8));

        Verify(this.plain.Urls.Count == this.UrlCount, "plain URL count");
        Verify(this.video.Urls.Count == this.UrlCount, "video URL count");
        Verify(this.video.Urls.First().Extensions.OfType<SitemapVideoExtension>().Any(), "video extension attached before saving");
        Verify(this.hreflang.Urls.First().Extensions.OfType<SitemapHreflangExtension>().Any(), "hreflang extension attached before saving");
    }

    /// <summary>
    /// Saves the plain sitemap.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. Plain Save(Stream)")]
    public long SavePlain()
    {
        using MemoryStream stream = new();
        this.plain.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Saves the plain sitemap with <c>xsi:schemaLocation</c> stamping enabled.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. Plain + WriteXsiSchemaLocation")]
    public long SavePlainWithSchemaLocation()
    {
        using MemoryStream stream = new();
        this.plain.Save(stream, this.schemaLocationSettings);
        return stream.Length;
    }

    /// <summary>
    /// Saves the video-extension sitemap — the writer whose element order Google's schema pins.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. Video extension save")]
    public long SaveVideo()
    {
        using MemoryStream stream = new();
        this.video.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Saves the hreflang sitemap — the attribute-heavy write path.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. Hreflang save")]
    public long SaveHreflang()
    {
        using MemoryStream stream = new();
        this.hreflang.Save(stream);
        return stream.Length;
    }

    private static Sitemap Load(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        Sitemap sitemap = new();
        sitemap.Load(stream);
        return sitemap;
    }

    private static void Verify(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Corpus guard failed: {description}.");
        }
    }
}