using System.Diagnostics.CodeAnalysis;

using Argotic.Extensions;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Measures the whole newsletter pipeline: subscription list in, every feed parsed, every item read, every feed written back.
/// </summary>
/// <remarks>
///     <para>
///     The arms are <b>cumulative</b>, so each row's cost is the previous row plus one stage, and the
///     difference between two adjacent rows is that stage's price on its own. That is the only way to
///     answer "where does the pipeline's memory go" without guessing, and it is how §2.21 measured the
///     parse pipeline stage by stage.
///     </para>
///     <para>
///     <b>Deliberately no network.</b> A benchmark that fetches is measuring the internet. The real
///     crawl is a separate field harness; this measures the work the library does with what a fetch
///     returns, and does it deterministically.
///     </para>
///     <para>
///     Scale is one production list — 88 subscriptions, the Fabric Weekly size — at ten items each,
///     which is a real week's crawl. The largest list is five times bigger and behaves linearly, and a
///     benchmark that took five times longer to say so would be worse, not better.
///     </para>
///     <para>
///     <b>Read the allocation column.</b> §2.25 records two provably identical code paths timing 48%
///     apart on this hardware while allocating byte-identically.
///     </para>
/// </remarks>
[BenchmarkCategory("pipeline")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class FeedPipelineBenchmarks
{
    private const int Subscriptions = 88;
    private const int ItemsPerFeed = 10;

    private byte[] subscriptionList = [];
    private byte[] feed = [];

    /// <summary>
    /// Builds the subscription list and the feed body every subscription resolves to.
    /// </summary>
    /// <remarks>
    ///     One feed body, reused: the pipeline's cost is per parse, not per distinct document, and a
    ///     shared array keeps the setup out of the measurement.
    /// </remarks>
    [GlobalSetup]
    public void Setup()
    {
        this.subscriptionList = OpmlCorpus.GenerateSubscriptionListUtf8(Subscriptions);
        this.feed = FeedCorpus.GenerateRssUtf8(ItemsPerFeed);
    }

    /// <summary>
    /// Stage 1 — parse the subscription list and collect every feed address.
    /// </summary>
    /// <returns>The addresses found, so nothing is optimised away.</returns>
    [Benchmark(Description = "a. OPML in, feed addresses out", Baseline = true)]
    public int ParseSubscriptionList()
    {
        using MemoryStream stream = new(this.subscriptionList, writable: false);
        OpmlDocument opml = new();
        opml.Load(stream);

        int addresses = 0;
        foreach (OpmlOutline outline in opml.Outlines)
        {
            if (outline.Attributes.ContainsKey("xmlUrl"))
            {
                addresses++;
            }
        }

        return addresses;
    }

    /// <summary>
    /// Stage 2 — and parse the feed each address resolves to.
    /// </summary>
    /// <returns>The number of feeds parsed.</returns>
    [Benchmark(Description = "b. + parse every feed")]
    public int ParseEveryFeed()
    {
        int addresses = this.ParseSubscriptionList();

        for (int i = 0; i < addresses; i++)
        {
            using MemoryStream stream = new(this.feed, writable: false);
            RssFeed parsed = new();
            parsed.Load(stream);
        }

        return addresses;
    }

    /// <summary>
    /// Stage 3 — and read every item, touching every property a consumer would.
    /// </summary>
    /// <returns>A checksum over everything read, so no read can be elided.</returns>
    [Benchmark(Description = "c. + read every item and property")]
    public long ReadEveryProperty()
    {
        int addresses = this.ParseSubscriptionList();
        long sink = 0;

        for (int i = 0; i < addresses; i++)
        {
            using MemoryStream stream = new(this.feed, writable: false);
            RssFeed parsed = new();
            parsed.Load(stream);
            sink += Touch(parsed);
        }

        return sink;
    }

    /// <summary>
    /// Stage 4 — and write every feed back out, which is the round trip.
    /// </summary>
    /// <returns>The total bytes written.</returns>
    [Benchmark(Description = "d. + save every feed back")]
    public long RoundTripEveryFeed()
    {
        int addresses = this.ParseSubscriptionList();
        long written = 0;

        for (int i = 0; i < addresses; i++)
        {
            using MemoryStream stream = new(this.feed, writable: false);
            RssFeed parsed = new();
            parsed.Load(stream);
            written += Touch(parsed);

            using MemoryStream output = new();
            parsed.Save(output);
            written += output.Length;
        }

        return written;
    }

    /// <summary>
    /// Reads everything an item carries, so a lazily-populated member cannot hide from the measurement.
    /// </summary>
    /// <param name="parsed">The feed to walk.</param>
    /// <returns>A checksum, which exists only to stop the reads being optimised away.</returns>
    private static long Touch(RssFeed parsed)
    {
        long sink = parsed.Channel.Title.Length
            + parsed.Channel.Description.Length
            + (parsed.Channel.Link?.OriginalString.Length ?? 0)
            + parsed.Channel.LastBuildDate.Ticks
            + (parsed.Channel.HasExtensions ? 1 : 0);

        foreach (RssItem item in parsed.Channel.Items)
        {
            sink += item.Title.Length
                + item.Description.Length
                + item.Author.Length
                + (item.Link?.OriginalString.Length ?? 0)
                + (item.Comments?.OriginalString.Length ?? 0)
                + item.PublicationDate.Ticks
                + (item.Guid?.Value?.Length ?? 0)
                + (item.Source?.Title.Length ?? 0)
                + (item.HasExtensions ? 1 : 0);

            foreach (RssCategory category in item.Categories)
            {
                sink += category.Value.Length + category.Domain.Length;
            }

            foreach (RssEnclosure enclosure in item.Enclosures)
            {
                sink += enclosure.Length + (enclosure.Url?.OriginalString.Length ?? 0);
            }

            foreach (ISyndicationExtension extension in item.Extensions)
            {
                sink += extension.Name.Length;
            }
        }

        return sink;
    }
}