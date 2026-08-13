using System.Diagnostics.CodeAnalysis;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices four points on the extension-density axis: none, declared-but-unused, the six thriving
/// families in use, and every family at once.
/// </summary>
/// <remarks>
/// <para>
/// Arm b is the arm production actually takes. The 627-feed census found <c>geo:</c> declared by
/// 121 feeds and used by 5, <c>georss:</c> by 144 and used by 4 — "declaration is not evidence of
/// use in ~96% of cases" — and detection answers on the prefix alone
/// (<c>SyndicationExtension.ExistsInSource</c>), so an unused declaration still forces a fresh
/// extension instance through <c>Load</c> to discover there is nothing to read, on every
/// extensible entity. The b−a delta prices that declaration tax; c−b prices actual payload.
/// </para>
/// <para>
/// Arm d is deliberately a fiction — no real feed carries every family, and no number measured
/// over it may be quoted as production-relevant. It exists so the families with no live corpus at
/// all (FeedSync, SimpleList, FeedRank exist in the wild only as their spec examples; LiveJournal
/// publishes under a namespace URI this library does not match) have a price, which is the
/// precondition for ever optimising their paths. Arm c reuses
/// <see cref="FeedCorpus.GenerateRssWithExtensionsUtf8"/> unmodified, anchoring this class to
/// every committed number derived from that corpus.
/// </para>
/// <para>
/// The guards encode the arms' meanings, not just their sizes: arm b asserts Dublin Core IS
/// attached and geo/GeoRSS/Creative Commons are NOT — if detection semantics ever change (for
/// example to namespace-URI-first), the guard fails and the arm is re-examined rather than
/// silently inverting from "declaration tax" into something else.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "extensions", "elementmix", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class ExtensionElementMixBenchmarks
{
    private byte[] extensionFree = [];
    private byte[] declaredOnly = [];
    private byte[] sixFamilies = [];
    private byte[] maximal = [];

    /// <summary>
    /// Gets or sets the number of items in each feed under test. No 1,000 value: the maximal
    /// document is a ceiling instrument, not a throughput scenario.
    /// </summary>
    [Params(10, 100)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates all four corpora and parse-verifies each arm's meaning.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.extensionFree = FeedCorpus.GenerateRssUtf8(this.ItemCount);
        this.declaredOnly = ExtensionFeedCorpus.GenerateDeclaredOnlyProductionUtf8(this.ItemCount);
        this.sixFamilies = FeedCorpus.GenerateRssWithExtensionsUtf8(this.ItemCount);
        this.maximal = ExtensionFeedCorpus.GenerateMaximalExtensionFeedUtf8(this.ItemCount);

        RssFeed plain = Load(this.extensionFree);
        Verify(plain.Channel.Items.Count == this.ItemCount, "extension-free item count");
        Verify(!plain.Channel.Items.First().HasExtensions, "extension-free arm must attach nothing");

        RssFeed declared = Load(this.declaredOnly);
        RssItem declaredItem = declared.Channel.Items.First();
        Verify(declared.Channel.Items.Count == this.ItemCount, "declared-only item count");
        Verify(declaredItem.Extensions.OfType<DublinCoreElementSetSyndicationExtension>().Any(), "declared-only: Dublin Core must be attached (it is used)");
        Verify(!declaredItem.Extensions.OfType<BasicGeocodingSyndicationExtension>().Any(), "declared-only: geo must NOT attach (declared, never used)");
        Verify(!declaredItem.Extensions.OfType<GeoRssSyndicationExtension>().Any(), "declared-only: GeoRSS must NOT attach (declared, never used)");
        Verify(!declaredItem.Extensions.OfType<CreativeCommonsSyndicationExtension>().Any(), "declared-only: Creative Commons must NOT attach (declared, never used)");

        RssFeed six = Load(this.sixFamilies);
        Verify(six.Channel.Items.Count == this.ItemCount, "six-family item count");
        Verify(six.Channel.Items.First().HasExtensions, "six-family extensions attached");

        RssFeed max = Load(this.maximal);
        RssItem maximalItem = max.Channel.Items.First();
        Verify(max.Channel.Items.Count == this.ItemCount, "maximal item count");
        Verify(maximalItem.Extensions.OfType<FeedSynchronizationSyndicationExtension>().Any(), "maximal: FeedSync attached to the first item");
        Verify(maximalItem.Extensions.OfType<GeoRssSyndicationExtension>().Any(), "maximal: GeoRSS attached to the first item");
        Verify(maximalItem.Extensions.OfType<LiveJournalSyndicationExtension>().Any(), "maximal: LiveJournal attached to the first item");
        Verify(max.Channel.Extensions.OfType<SimpleListSyndicationExtension>().Any(), "maximal: SimpleList attached to the channel");
        Verify(max.Channel.Extensions.OfType<FeedHistorySyndicationExtension>().SingleOrDefault()?.Context.Relations.Count == 2, "maximal: FeedHistory archive links parsed from atom:link");
    }

    /// <summary>
    /// Loads the extension-free floor.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. No extension namespaces")]
    public int LoadExtensionFree() => Load(this.extensionFree).Channel.Items.Count;

    /// <summary>
    /// Loads the ~96% production shape: nine namespaces declared, five families used.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. Declared but unused (the production shape)")]
    public int LoadDeclaredOnly() => Load(this.declaredOnly).Channel.Items.Count;

    /// <summary>
    /// Loads the six thriving families in active use — the existing extension corpus, unmodified.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. Six families in use")]
    public int LoadSixFamilies() => Load(this.sixFamilies).Channel.Items.Count;

    /// <summary>
    /// Loads the ceiling document: every family, every collection-bearing element.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. Every family (deliberate fiction)")]
    public int LoadMaximal() => Load(this.maximal).Channel.Items.Count;

    private static RssFeed Load(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static void Verify(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Corpus guard failed: {description}.");
        }
    }
}