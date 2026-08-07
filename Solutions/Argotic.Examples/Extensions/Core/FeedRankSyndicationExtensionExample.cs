using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads a <see cref="FeedRankSyndicationExtension"/> out of a feed the framework has already parsed, then writes the feed back out.
/// </summary>
/// <remarks>
///     Nothing registers the extension. Reflection over the assembly's exported types produces the
///     candidates; those whose namespace or prefix is bound on the document are asked whether they are
///     present; and each that says yes is attached to the entity that carried its elements — so the only
///     call a consumer makes is <c>FindExtension(MatchByType)</c>. The namespace declarations written
///     back on save are derived the same way, from the extensions actually present.
/// </remarks>
internal static class FeedRankSyndicationExtensionExample
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
            if (feed.Channel.FindExtension(FeedRankSyndicationExtension.MatchByType) is FeedRankSyndicationExtension channelExtension)
            {
                ExampleOutput.ShowFeedRankExtension(channelExtension);
            }
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions)
            {
                if (item.FindExtension(FeedRankSyndicationExtension.MatchByType) is FeedRankSyndicationExtension itemExtension)
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
}