using System.Diagnostics.CodeAnalysis;
using Argotic.Syndication;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Cross-checks the synthetic generator against a real document.
/// </summary>
/// <remarks>
/// <para>
/// This class exists to catch the failure mode that makes benchmarks lie: a synthetic input
/// shaped for the convenience of the generator rather than for fidelity to production, producing
/// confident and wrong evidence. The guard is a like-for-like pair — the repository's real
/// <c>RssFeed.xml</c> (3 items) against a synthetic feed of the same item count — so per-item
/// cost and per-item allocation can be compared directly.
/// </para>
/// <para>
/// The two are not expected to match exactly; the real sample's text lengths differ. They are
/// expected to agree in <em>character</em>: same order of magnitude for time and allocation per
/// item. If they diverge sharply, the generator is wrong and must be fixed before any conclusion
/// is drawn from a synthetic-only measurement.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[BenchmarkCategory("load", "rss", "fidelity")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class RealSampleLoadBenchmarks
{
    private const int RealSampleItemCount = 3;

    private byte[] realSample = [];
    private byte[] syntheticMatched = [];

    /// <summary>
    /// Loads the real document and generates a synthetic one of matching item count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.realSample = FeedCorpus.ReadRealSample("RssFeed.xml");
        this.syntheticMatched = FeedCorpus.GenerateRssUtf8(RealSampleItemCount);
    }

    /// <summary>
    /// Parses the repository's real RSS sample document.
    /// </summary>
    /// <returns>The parsed feed, returned so the JIT cannot elide the work.</returns>
    [Benchmark(Baseline = true, Description = "real RssFeed.xml (3 items)")]
    public RssFeed LoadRealSample()
    {
        using MemoryStream stream = new(this.realSample, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// Parses a synthetic document of the same item count, for direct comparison.
    /// </summary>
    /// <returns>The parsed feed, returned so the JIT cannot elide the work.</returns>
    [Benchmark(Description = "synthetic (3 items)")]
    public RssFeed LoadSyntheticMatched()
    {
        using MemoryStream stream = new(this.syntheticMatched, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }
}
