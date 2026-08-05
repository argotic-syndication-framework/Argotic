using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Saving;

/// <summary>
/// Prices aggregate-and-re-emit as one operation, on extension-bearing input.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FeedSaveBenchmarks"/> names this workload in its own remarks - "newsletter aggregation
/// loads feeds from many sources and re-emits a normalised feed" - and then hoists the load into
/// <c>[GlobalSetup]</c>, so the composite it describes is never measured.
/// </para>
/// <para>
/// It also saves feeds parsed from the extension-free generators, so <c>FillExtensionTypes</c> finds
/// <c>HasExtensions == false</c> and returns immediately every time. The class's stated prediction is
/// about extension handling on the write path, and it is tested against the one input that cannot
/// exercise it. This arm uses the extension-bearing corpus for exactly that reason.
/// </para>
/// </remarks>
[BenchmarkCategory("save", "roundtrip", "rss")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class RoundTripBenchmarks
{
    private byte[] source = [];

    /// <summary>
    /// Gets or sets the number of source feeds merged into the output.
    /// </summary>
    [Params(5, 25, 100)]
    public int SourceFeedCount { get; set; }

    /// <summary>
    /// Builds one twenty-item extension-bearing source document, reused for every source feed.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.source = FeedCorpus.GenerateRssWithExtensionsUtf8(20);

    /// <summary>
    /// Loads every source feed, merges their items into one feed, and writes it out.
    /// </summary>
    /// <returns>The number of bytes emitted, so the work cannot be elided.</returns>
    [Benchmark(Description = "load N feeds -> merge -> Save(Stream)")]
    public long AggregateAndRepublish()
    {
        RssFeed aggregate = new(new Uri("https://example.com/"), "Aggregated");
        aggregate.Channel.Description = "Items merged from several source feeds";

        for (int i = 0; i < this.SourceFeedCount; i++)
        {
            using MemoryStream stream = new(this.source, writable: false);
            RssFeed feed = new();
            feed.Load(stream);

            foreach (RssItem item in feed.Channel.Items)
            {
                aggregate.Channel.Items.Add(item);
            }
        }

        using MemoryStream output = new();
        aggregate.Save(output);
        return output.Length;
    }
}