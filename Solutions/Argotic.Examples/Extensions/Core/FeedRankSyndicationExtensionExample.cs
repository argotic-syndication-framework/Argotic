using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Contains the code examples for the <see cref="FeedRankSyndicationExtension"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="FeedRankSyndicationExtension"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class FeedRankSyndicationExtensionExample
{
    /// <summary>
    /// Provides example code for the FeedRankSyndicationExtension class.
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
            FeedRankSyndicationExtension channelExtension = feed.Channel.FindExtension(FeedRankSyndicationExtension.MatchByType) as FeedRankSyndicationExtension;
            if (channelExtension != null)
            {
                ExampleOutput.ShowFeedRankExtension(channelExtension);
            }
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions)
            {
                FeedRankSyndicationExtension itemExtension = item.FindExtension(FeedRankSyndicationExtension.MatchByType) as FeedRankSyndicationExtension;
                if (itemExtension != null)
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