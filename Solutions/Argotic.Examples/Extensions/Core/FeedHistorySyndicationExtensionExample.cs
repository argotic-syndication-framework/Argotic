using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads a <see cref="FeedHistorySyndicationExtension"/> out of a feed the framework has already parsed, then writes the feed back out.
/// </summary>
/// <remarks>
///     Nothing registers the extension. Reflection over the assembly's exported types produces the
///     candidates; those whose namespace or prefix is bound on the document are asked whether they are
///     present; and each that says yes is attached to the entity that carried its elements — so the only
///     call a consumer makes is <c>FindExtension(MatchByType)</c>. The namespace declarations written
///     back on save are derived the same way, from the extensions actually present.
/// </remarks>
internal static class FeedHistorySyndicationExtensionExample
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
            if (feed.Channel.FindExtension(FeedHistorySyndicationExtension.MatchByType) is FeedHistorySyndicationExtension channelExtension)
            {
                ExampleOutput.ShowFeedHistoryExtension(channelExtension);
            }
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions)
            {
                if (item.FindExtension(FeedHistorySyndicationExtension.MatchByType) is FeedHistorySyndicationExtension itemExtension)
                {
                    // Process extension for current item
                }
            }
        }

        // By default the framework will automatically determine what XML namespace attributes (xmlns) to write
        // on the root of the resource based on the extensions applied to extensible parent and child entities
        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Builds a <see cref="FeedHistorySyndicationExtension"/> from scratch and reads it back.
    /// </summary>
    /// <remarks>
    ///     IsComplete and IsArchive are mutually exclusive by RFC 5005: a complete feed holds every entry
    ///     there is, an archive document holds a fixed slice of history, and a document claiming both is
    ///     making two contradictory promises. This builds the archive form, with the paging relations that
    ///     belong to it and would be rejected on a complete feed.
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

        FeedHistorySyndicationExtension history = new();
        history.Context.IsArchive = true;
        history.Context.Relations.Add(new FeedHistoryLinkRelation
        {
            RelationType = FeedHistoryLinkRelationType.PreviousArchive,
            Uri = new Uri("https://endjin.com/rss.xml?archive=2025"),
        });
        history.Context.Relations.Add(new FeedHistoryLinkRelation
        {
            RelationType = FeedHistoryLinkRelationType.Current,
            Uri = new Uri("https://endjin.com/rss.xml"),
        });
        feed.Channel.Extensions.Add(history);

        feed.Channel.Items.Add(item);

        using MemoryStream saved = new();
        feed.Save(saved);
        ExampleOutput.ShowSaved("RssFeed");

        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);


        if (reloaded.Channel.FindExtension(FeedHistorySyndicationExtension.MatchByType) is FeedHistorySyndicationExtension readBack)
        {
            ExampleOutput.ShowFeedHistoryExtension(readBack);
        }
    }
}