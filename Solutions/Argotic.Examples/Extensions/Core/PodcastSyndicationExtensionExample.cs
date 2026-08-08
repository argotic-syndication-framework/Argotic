using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads a <see cref="PodcastSyndicationExtension"/> out of a Podcasting 2.0 feed, then builds one from scratch.
/// </summary>
/// <remarks>
///     <para>
///     Podcasting 2.0 is the newest namespace this library reads and the most widely deployed of the
///     recent ones: a survey of the Apple directory found it declared by 1,200 of 1,934 live feeds.
///     Before this extension existed every element of it was dropped by a load followed by a save —
///     not misread, not partially read, silently discarded, because there was nowhere to put it.
///     </para>
///     <para>
///     The namespace is matched exactly, and it is <c>https</c>:
///     <c>https://podcastindex.org/namespace/1.0</c>. A feed declaring the <c>http</c> form yields no
///     extension at all, and does so quietly, which is the first thing to check when a podcast feed
///     appears to carry nothing.
///     </para>
/// </remarks>
internal static class PodcastSyndicationExtensionExample
{
    /// <summary>
    /// Loads a Podcasting 2.0 feed, finds the extension on the channel and on the items, then writes the feed back out.
    /// </summary>
    public static void ClassExample()
    {
        RssFeed feed = new();
        using (Stream inputStream = SampleDataPath.OpenRead(SampleDataPath.PodcastFeed))
        {
            feed.Load(inputStream);
        }

        ExampleOutput.ShowLoaded("RssFeed", feed.Channel.Title);

        if (feed.Channel.FindExtension(PodcastSyndicationExtension.MatchByType) is PodcastSyndicationExtension channelExtension)
        {
            ExampleOutput.ShowPodcastExtension(channelExtension);
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.FindExtension(PodcastSyndicationExtension.MatchByType) is PodcastSyndicationExtension itemExtension)
            {
                Console.WriteLine($"Episode: {item.Title}");
                ExampleOutput.ShowPodcastExtension(itemExtension);
            }
        }

        int count = feed.Channel.Items.Count(i => i.FindExtension(PodcastSyndicationExtension.MatchByType) is not null);
        ExampleOutput.ShowItemsWithExtension(count, feed.Channel.Items.Count, "Podcasting 2.0");

        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Builds a Podcasting 2.0 feed from scratch, saves it, and reads the extension back out.
    /// </summary>
    /// <remarks>
    ///     Nothing declares the namespace by hand. <c>AutoDetectExtensions</c> fills the supported set
    ///     from the extensions actually attached to the object graph before the namespace declarations
    ///     are written, so attaching the extension is the whole of the work.
    /// </remarks>
    public static void AuthorExample()
    {
        RssFeed feed = new();
        feed.Channel.Title = "endjin blog, read aloud";
        feed.Channel.Link = new Uri("https://endjin.com/blog/");
        feed.Channel.Description = "Audio versions of selected posts from the endjin blog.";

        PodcastSyndicationExtension channelExtension = new();
        channelExtension.Context.Identifier = "917393e3-1b1e-5cef-ace4-edaa54e1f810";
        channelExtension.Context.Medium = PodcastMedium.Podcast;
        channelExtension.Context.IsLocked = true;
        channelExtension.Context.LockOwner = "hello@endjin.com";
        channelExtension.Context.UsesPodping = true;
        channelExtension.Context.People.Add(new PodcastPerson
        {
            Name = "Barry Smart",
            Role = "host",
            Url = new Uri("https://endjin.com/who-we-are/"),
        });
        channelExtension.Context.FundingLinks.Add(new PodcastFunding
        {
            Message = "Talk to endjin",
            Url = new Uri("https://endjin.com/contact-us/"),
        });

        // The Apple Podcasts ownership token. Publishers split roughly evenly between delivering it
        // here and as itunes:applepodcastsverify, so writing only one form loses half the audience.
        channelExtension.Context.TextEntries.Add(new PodcastText
        {
            Purpose = "applepodcastsverify",
            Value = "e6cfaae0-9496-11f0-a272-f9e230f88be0",
        });

        feed.Channel.Extensions.Add(channelExtension);

        RssItem episode = new()
        {
            Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
            Link = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
            Description = "Generative AI is a new instrument in an orchestra that already existed.",
            PublicationDate = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
        };

        episode.Enclosures.Add(new RssEnclosure(
            36588921,
            "audio/mpeg",
            new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check-new-Instrument-same-orchestra.mp3")));

        PodcastSyndicationExtension episodeExtension = new();
        episodeExtension.Context.Season = 1;
        episodeExtension.Context.SeasonName = "Data and AI";
        episodeExtension.Context.Episode = 1m;
        episodeExtension.Context.EpisodeDisplay = "Ep. 1";
        episodeExtension.Context.Transcripts.Add(new PodcastTranscript
        {
            Url = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra/transcript.vtt"),
            MediaType = "text/vtt",
            Language = "en",
            Relationship = "captions",
        });

        episode.Extensions.Add(episodeExtension);
        feed.Channel.Items.Add(episode);

        using MemoryStream saved = new();
        feed.Save(saved);
        ExampleOutput.ShowSaved("Podcasting 2.0 RssFeed");

        // Read it back, so the example demonstrates the round trip rather than the setters.
        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);

        if (reloaded.Channel.FindExtension(PodcastSyndicationExtension.MatchByType) is PodcastSyndicationExtension readBack)
        {
            ExampleOutput.ShowPodcastExtension(readBack);
        }

        foreach (RssItem item in reloaded.Channel.Items)
        {
            if (item.FindExtension(PodcastSyndicationExtension.MatchByType) is PodcastSyndicationExtension itemExtension)
            {
                ExampleOutput.ShowPodcastExtension(itemExtension);
            }
        }
    }
}