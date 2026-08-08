using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads an <see cref="ITunesSyndicationExtension"/> out of a feed the framework has already parsed, then writes the feed back out.
/// </summary>
/// <remarks>
///     Nothing registers the extension. Reflection over the assembly's exported types produces the
///     candidates; those whose namespace or prefix is bound on the document are asked whether they are
///     present; and each that says yes is attached to the entity that carried its elements — so the only
///     call a consumer makes is <c>FindExtension(MatchByType)</c>. The namespace declarations written
///     back on save are derived the same way, from the extensions actually present.
/// </remarks>
internal static class ITunesSyndicationExtensionExample
{
    /// <summary>
    /// Loads a feed carrying the extension, finds it on the channel and on the items, then writes the feed back out.
    /// </summary>
    public static void ClassExample()
    {
        // Framework auto-discovers supported extensions based on XML namespace attributes (xmlns) defined on root of resource
        RssFeed feed = new();
        using (Stream inputStream = SampleDataPath.OpenRead(SampleDataPath.RssFeedWithExtensions))
        {
            feed.Load(inputStream);
        }

        ExampleOutput.ShowLoaded("RssFeed", feed.Channel.Title);

        // Extensible framework entities provide properties/methods to determine if entity is extended and predicate based searching against available extensions
        if (feed.Channel.HasExtensions)
        {
            if (feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension channelExtension)
            {
                ExampleOutput.ShowITunesExtension(channelExtension);
            }
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions)
            {
                if (item.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension itemExtension)
                {
                    // Process extension for current item
                }
            }
        }

        int count = feed.Channel.Items.Count(i => i.FindExtension(ITunesSyndicationExtension.MatchByType) is not null);
        ExampleOutput.ShowItemsWithExtension(count, feed.Channel.Items.Count, "iTunes");

        // By default the framework will automatically determine what XML namespace attributes (xmlns) to write
        // on the root of the resource based on the extensions applied to extensible parent and child entities
        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Builds an <see cref="ITunesSyndicationExtension"/> from scratch and reads it back.
    /// </summary>
    /// <remarks>
    ///     Reaches the members a read-only example never shows: the nested category, the owner block, the
    ///     explicit flag and the two enums that describe what kind of show and what kind of episode.
    ///     <para>
    ///     ExplicitMaterial has four values, not two. None means the publisher said nothing, which is a
    ///     different statement from No -- and Apple treats the absence and the denial differently.
    ///     </para>
    /// </remarks>
    public static void AuthorExample()
    {
        RssFeed feed = new();
        feed.Channel.Title = "endjin blog";
        feed.Channel.Link = new Uri("https://endjin.com/blog/");
        feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";

        RssItem item = new()
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            Description = "Moving UI framework support out of System.Reactive can cut 95MB from a deployment.",
        };

        ITunesSyndicationExtension show = new();
        show.Context.Author = "endjin";
        show.Context.Subtitle = "Audio versions of the endjin blog";
        show.Context.Summary = "Selected endjin blog posts, recorded for listening.";
        show.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
        show.Context.PodcastType = ITunesPodcastType.Episodic;
        show.Context.Owner = new ITunesOwner("hello@endjin.com", "endjin");
        show.Context.Categories.Add(new ITunesCategory("Technology"));
        show.Context.Keywords.Add("dotnet");
        show.Context.Keywords.Add("data");
        feed.Channel.Extensions.Add(show);

        ITunesSyndicationExtension episode = new();
        episode.Context.Title = "Rx.NET v7.0 Released";
        episode.Context.Season = 1;
        episode.Context.Episode = 3;
        episode.Context.EpisodeType = ITunesEpisodeType.Full;
        episode.Context.Duration = TimeSpan.FromMinutes(26.9);
        episode.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
        item.Extensions.Add(episode);

        feed.Channel.Items.Add(item);

        using MemoryStream saved = new();
        feed.Save(saved);
        ExampleOutput.ShowSaved("RssFeed");

        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);

        RssItem readItem = reloaded.Channel.Items[0];
        if (reloaded.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension readChannel)
        {
            ExampleOutput.ShowITunesExtension(readChannel);
        }

        if (readItem.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension readBack)
        {
            ExampleOutput.ShowITunesExtension(readBack);
        }
    }
}