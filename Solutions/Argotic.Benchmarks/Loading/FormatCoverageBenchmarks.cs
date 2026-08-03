using System.Diagnostics.CodeAnalysis;
using Argotic.Syndication;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads the remaining formats at the standard size ladder, to establish whether the extension
/// auto-detection cost found on the RSS path is format-specific or systemic.
/// </summary>
/// <remarks>
/// <para>
/// Atom, Sitemap and <see cref="GenericSyndicationFeed"/> all route through the same
/// <c>SyndicationExtensionAdapter.Fill</c> machinery as RSS, so the expectation is that they share
/// the cost. Expectations are what this harness exists to check rather than assume — a format that
/// does <em>not</em> show it would be the interesting result, because it would mean the RSS path
/// does something extra.
/// </para>
/// <para>
/// <see cref="GenericSyndicationFeed"/> matters disproportionately here: it is the path an
/// aggregator uses when the source format is unknown, which is exactly the newsletter case this
/// library is used for in production.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "formats")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class FormatCoverageBenchmarks
{
    private byte[] atom = [];
    private byte[] rss = [];
    private byte[] sitemap = [];

    /// <summary>
    /// Gets or sets the number of items, entries or URLs in the document under test.
    /// </summary>
    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates one document per format at the current size.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.rss = FeedCorpus.GenerateRssUtf8(this.ItemCount);
        this.atom = FeedCorpus.GenerateAtomUtf8(this.ItemCount);
        this.sitemap = FeedCorpus.GenerateSitemapUtf8(this.ItemCount);
    }

    /// <summary>
    /// Parses an Atom feed.
    /// </summary>
    /// <returns>The parsed feed, returned so the JIT cannot elide the work.</returns>
    [Benchmark(Baseline = true, Description = "Atom Load(Stream)")]
    public AtomFeed LoadAtom()
    {
        using MemoryStream stream = new(this.atom, writable: false);
        AtomFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// Parses a feed through the format-agnostic wrapper an aggregator would use.
    /// </summary>
    /// <returns>The parsed feed, returned so the JIT cannot elide the work.</returns>
    [Benchmark(Description = "GenericSyndicationFeed Load(Stream)")]
    public GenericSyndicationFeed LoadGeneric()
    {
        using MemoryStream stream = new(this.rss, writable: false);
        GenericSyndicationFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// Parses a Sitemap 0.9 urlset.
    /// </summary>
    /// <returns>The parsed sitemap, returned so the JIT cannot elide the work.</returns>
    /// <remarks>
    /// Sitemap extension scoping was rewritten on this branch (extensions previously bound to every
    /// URL in the document rather than the one they appeared under), so this path carries recent
    /// churn and warrants measurement rather than assumption.
    /// </remarks>
    [Benchmark(Description = "Sitemap Load(Stream)")]
    public Sitemap LoadSitemap()
    {
        using MemoryStream stream = new(this.sitemap, writable: false);
        Sitemap map = new();
        map.Load(stream);
        return map;
    }
}