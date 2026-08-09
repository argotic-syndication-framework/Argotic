using System.Diagnostics.CodeAnalysis;
using Argotic.Syndication;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads documents real publishers actually emit, at sizes the synthetic generators never reach.
/// </summary>
/// <remarks>
/// <para>
/// Every other load benchmark in this harness runs on generated documents or on the fifteen small
/// hand-authored samples in <c>Argotic.Examples/SampleData</c>. Both are useful and both are clean:
/// pure 7-bit ASCII, no byte-order mark, no unusual date spelling, a namespace declared only when it
/// is used. Real feeds are none of those things, and the paths that only real feeds reach have never
/// been timed.
/// </para>
/// <para>
/// Two specimens here test documented limits directly:
/// </para>
/// <list type="bullet">
///   <item><description>A sitemap holding exactly 50,000 <c>&lt;url&gt;</c> elements — the
///   ceiling the protocol defines and this library documents. <c>SitemapScaleBenchmarks</c> reaches
///   that count with a generator; this is 7.7 MB of it from a real publisher, so it carries the
///   encodings, date spellings and URL shapes a generator has no reason to
///   produce.</description></item>
///   <item><description>A 13.2 MB Atom feed containing 112 elements — the inverse shape. Cost
///   here is buffer growth and decoding, not element dispatch, which is the axis every synthetic feed
///   in this harness holds constant by construction.</description></item>
/// </list>
/// <para>
/// The corpus is machine-local and gitignored, so <see cref="RealWorldCorpus"/> throws when it is
/// absent rather than yielding nothing. A benchmark that quietly measured an empty set would report an
/// excellent time and a tiny allocation, and both would be false.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "realworld")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class RealWorldLoadBenchmarks
{
    private byte[] sitemapAtTheCeiling = [];
    private byte[] largeSparseAtom = [];
    private byte[][] everyRealRssFeed = [];

    /// <summary>
    /// Reads the corpus documents into memory so the measured call sees a stream over bytes, not a disk read.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.sitemapAtTheCeiling = RealWorldCorpus.Read("sitemap/wikihow-urlset-50000.xml");
        this.largeSparseAtom = RealWorldCorpus.Read("atom10/projectzero-google-large.atom");
        this.everyRealRssFeed = RealWorldCorpus.ReadAll("rss20");
    }

    /// <summary>
    /// The sitemap protocol's documented ceiling, loaded end to end.
    /// </summary>
    /// <returns>The number of URLs parsed, so the load cannot be optimised away.</returns>
    [Benchmark(Baseline = true, Description = "Sitemap.Load, 50,000 urls (the protocol ceiling)")]
    public int SitemapAtTheProtocolCeiling()
    {
        using MemoryStream stream = new(this.sitemapAtTheCeiling, false);
        Sitemap sitemap = new();
        sitemap.Load(stream);

        return sitemap.Urls.Count;
    }

    /// <summary>
    /// 13.2 MB of Atom carrying only 112 elements: buffer growth rather than element dispatch.
    /// </summary>
    /// <returns>The number of entries parsed.</returns>
    [Benchmark(Description = "AtomFeed.Load, 13.2 MB across 112 elements")]
    public int LargeSparseAtomFeed()
    {
        using MemoryStream stream = new(this.largeSparseAtom, false);
        AtomFeed feed = new();
        feed.Load(stream);

        return feed.Entries.Count;
    }

    /// <summary>
    /// Every real RSS 2.0 document in the corpus, from ten different generators.
    /// </summary>
    /// <returns>The total number of items parsed.</returns>
    /// <remarks>
    ///     WordPress, Substack, Ghost, Squarespace, Blogger, Tumblr, Medium, Drupal, Hugo and several
    ///     hand-rolled feeds. The variety is the point: a single generator's output measures that
    ///     generator's habits, and the tolerance paths exist for everyone else's.
    /// </remarks>
    [Benchmark(Description = "RssFeed.Load over every real RSS 2.0 document")]
    public int EveryRealRssFeed()
    {
        int items = 0;

        foreach (byte[] document in this.everyRealRssFeed)
        {
            using MemoryStream stream = new(document, false);
            RssFeed feed = new();
            feed.Load(stream);
            items += feed.Channel.Items.Count;
        }

        return items;
    }

    /// <summary>
    /// The same documents through the format-agnostic facade, which sniffs before it parses.
    /// </summary>
    /// <returns>The total number of items projected.</returns>
    [Benchmark(Description = "GenericSyndicationFeed over the same documents (adds the sniff)")]
    public int EveryRealFeedThroughTheFacade()
    {
        int items = 0;

        foreach (byte[] document in this.everyRealRssFeed)
        {
            using MemoryStream stream = new(document, false);
            GenericSyndicationFeed feed = new();
            feed.Load(stream);
            items += feed.Items.Count;
        }

        return items;
    }
}