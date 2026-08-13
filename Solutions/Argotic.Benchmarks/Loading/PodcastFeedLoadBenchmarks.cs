using System.Diagnostics.CodeAnalysis;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices loading the modern podcast feed: Podcasting 2.0 at census-ratio element density with
/// iTunes at genuine podcast density alongside it.
/// </summary>
/// <remarks>
/// <para>
/// Podcasting 2.0 is declared by 62% of the 1,934 feeds the §2.50 census scanned, and its element
/// classes had zero benchmark coverage — the extension-bearing corpus emits none of its elements
/// (<c>.endjin/build-warnings.md</c> §19). This class prices the parse and, separately, the read:
/// an aggregator does not stop at <c>Load</c>, it walks transcripts, people, seasons and the
/// iTunes category tree, so arm b is the consumer-shaped composite and the a→b delta is the pure
/// read cost over the loaded object model.
/// </para>
/// <para>
/// The setup guard pins the namespace: the Podcasting 2.0 URI is the https:// form, matched
/// exactly, and a generator drifting to http:// yields a feed with no podcast extension at all —
/// the guard fails loudly instead of letting both arms silently price the extension-free path.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "extensions", "podcast", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class PodcastFeedLoadBenchmarks
{
    private byte[] document = [];

    /// <summary>
    /// Gets or sets the number of episodes in the feed under test. The top value approaches the
    /// real ceiling: the corpus's largest podcast feed carries 2,939 episodes.
    /// </summary>
    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates the corpus once and parse-verifies it: every item must carry both the Podcast
    /// and iTunes extensions, and the transcript count must equal the item count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.document = ExtensionFeedCorpus.GeneratePodcastFeedUtf8(this.ItemCount);

        RssFeed feed = this.LoadFeed();
        if (feed.Channel.Items.Count != this.ItemCount)
        {
            throw new InvalidOperationException("Corpus guard failed: item count.");
        }

        if (feed.Channel.Extensions.OfType<PodcastSyndicationExtension>().SingleOrDefault() is null)
        {
            throw new InvalidOperationException("Corpus guard failed: the channel carries no Podcasting 2.0 extension - check the namespace URI (https form, matched exactly).");
        }

        int transcripts = feed.Channel.Items
            .Select(item => item.Extensions.OfType<PodcastSyndicationExtension>().SingleOrDefault())
            .Sum(extension => extension?.Context.Transcripts.Count ?? 0);
        if (transcripts != this.ItemCount)
        {
            throw new InvalidOperationException($"Corpus guard failed: expected {this.ItemCount} transcripts, found {transcripts}.");
        }

        if (feed.Channel.Extensions.OfType<ITunesSyndicationExtension>().SingleOrDefault()?.Context.Owner is null)
        {
            throw new InvalidOperationException("Corpus guard failed: the channel carries no iTunes owner.");
        }
    }

    /// <summary>
    /// Loads the podcast feed.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. Load")]
    public int Load() => this.LoadFeed().Channel.Items.Count;

    /// <summary>
    /// Loads the podcast feed and reads what an aggregator reads: transcript URLs and types,
    /// people, seasons, episodes, and the iTunes category tree and owner.
    /// </summary>
    /// <returns>A checksum over everything read, so no read can be elided.</returns>
    [Benchmark(Description = "b. Load + read every podcast property")]
    public int LoadAndReadEverything()
    {
        RssFeed feed = this.LoadFeed();
        int checksum = 0;

        ITunesSyndicationExtension? channelITunes = feed.Channel.Extensions.OfType<ITunesSyndicationExtension>().SingleOrDefault();
        if (channelITunes is not null)
        {
            checksum += channelITunes.Context.Categories.Count;
            checksum += channelITunes.Context.Owner?.EmailAddress is null ? 0 : 1;
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            PodcastSyndicationExtension? podcast = item.Extensions.OfType<PodcastSyndicationExtension>().SingleOrDefault();
            if (podcast is null)
            {
                continue;
            }

            foreach (PodcastTranscript transcript in podcast.Context.Transcripts)
            {
                checksum += transcript.Url is null ? 0 : 1;
                checksum += transcript.MediaType.Length;
            }

            checksum += podcast.Context.People.Count;
            checksum += podcast.Context.SeasonName.Length;
            checksum += podcast.Context.EpisodeDisplay.Length;
            checksum += podcast.Context.Chapters is null ? 0 : 1;
        }

        return checksum;
    }

    private RssFeed LoadFeed()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }
}