using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads real <c>sitemapindex</c> documents, the last of the eight formats no benchmark had parsed.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SitemapIndex"/> is one of the twelve types implementing the uniform resource contract
/// and the only sitemap type never measured — <c>SitemapScaleBenchmarks</c> and
/// <c>RealWorldLoadBenchmarks</c> both parse <c>urlset</c> documents, which are a different root, a
/// different adapter path and a different per-entry object.
/// </para>
/// <para>
/// It matters more than its size suggests. An index is the document a crawler fetches <em>first</em>
/// and refetches on every crawl, and it is how a site with more than 50,000 pages publishes at all.
/// A site at ten million pages has an index of 200 entries; the protocol permits 50,000, which is a
/// document a crawler must be able to read.
/// </para>
/// <para>
/// <b>A note on the corpus, because the file names mislead.</b>
/// <c>sitemap/apple-index.xml</c> is <em>not</em> an index: despite its name it has a
/// <c>&lt;urlset&gt;</c> root and 847 <c>&lt;url&gt;</c> elements, and handing it to
/// <see cref="SitemapIndex"/> would throw <see cref="FormatException"/> rather than measure anything.
/// The corpus holds exactly two genuine indexes — <c>bbc-co-uk-index.xml</c> and
/// <c>gov-uk-index.xml</c> — and they are the two used here.
/// </para>
/// <para>
/// The two of them differ in the one way that matters to this parser, which is why two real documents
/// are worth more here than two of the same shape: the BBC's entries are <c>loc</c> and nothing else,
/// while gov.uk's carry a <c>lastmod</c> apiece. Per entry, that is the difference between no date
/// parse and one. <c>SitemapIndexScaleBenchmarks</c> takes that observation to the protocol ceiling;
/// this class establishes that it holds on documents nobody wrote for a benchmark.
/// </para>
/// <para>
/// <b>No <c>[Params]</c>.</b> Every arm is a fixed file, and a size axis would reproduce all three
/// rows unchanged at every value — the defect <c>docs/build-warnings.md</c> §D0 records. The scaling
/// sweep is a separate class over a generator, which is the only honest place for one.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "sitemap", "realworld")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SitemapIndexLoadBenchmarks
{
    private byte[] repositorySample = [];
    private byte[] locOnlyIndex = [];
    private byte[] lastModifiedIndex = [];

    /// <summary>
    /// Reads the sample and the two real indexes into memory, so the measured call sees no disk.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.repositorySample = FeedCorpus.ReadRealSample("sitemap_index.xml");
        this.locOnlyIndex = RealWorldCorpus.Read("sitemap/bbc-co-uk-index.xml");
        this.lastModifiedIndex = RealWorldCorpus.Read("sitemap/gov-uk-index.xml");
    }

    /// <summary>
    /// The repository's own 501-byte sample, which no benchmark had ever loaded.
    /// </summary>
    /// <returns>The number of entries parsed, so the load cannot be elided.</returns>
    /// <remarks>
    ///     The baseline, and the floor: at this size the measurement is almost entirely fixed cost —
    ///     buffer, sniff, build the <c>XPathDocument</c>, construct the metadata — with barely any
    ///     entries to walk. Every other arm's ratio therefore reads as the price of content rather
    ///     than the price of calling <c>Load</c> at all.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "a. SitemapIndex.Load, repository sample (501 B)")]
    public int LoadRepositorySample() => Load(this.repositorySample);

    /// <summary>
    /// A real index whose entries carry a location and nothing else.
    /// </summary>
    /// <returns>The number of entries parsed.</returns>
    /// <remarks>
    ///     Six entries, 745 bytes, and one <c>Uri.TryCreate</c> per entry with no date parse anywhere.
    ///     It also carries an XML comment before the root, which the repository's synthetic generators
    ///     never emit — a small reminder that real documents contain things nobody thought to generate.
    /// </remarks>
    [Benchmark(Description = "b. SitemapIndex.Load, 6 entries, loc only (real)")]
    public int LoadLocOnlyIndex() => Load(this.locOnlyIndex);

    /// <summary>
    /// A real index whose entries all carry an RFC 3339 <c>lastmod</c>.
    /// </summary>
    /// <returns>The number of entries parsed.</returns>
    /// <remarks>
    ///     Thirty-five entries, 4,733 bytes, each with a full <c>2026-08-06T02:50:02+00:00</c>
    ///     timestamp. Against the previous arm this is roughly six times the entries <em>and</em> a
    ///     date parse on each, so the two cannot be separated from these two arms alone — which is
    ///     exactly what <c>SitemapIndexScaleBenchmarks</c> exists to do, by holding the entry count
    ///     equal and varying only the date shape.
    /// </remarks>
    [Benchmark(Description = "c. SitemapIndex.Load, 35 entries with lastmod (real)")]
    public int LoadLastModifiedIndex() => Load(this.lastModifiedIndex);

    private static int Load(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        SitemapIndex index = new();
        index.Load(stream);

        return index.Sitemaps.Count;
    }
}