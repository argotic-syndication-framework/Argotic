using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Contains the code examples for the <see cref="GeoRssSyndicationExtension"/> class.
/// </summary>
/// <remarks>
///     GeoRSS is read from both syndication formats because the two are filled by separate adapters and
///     both matter in practice: the reference publisher, the USGS earthquake service, emits Atom, while
///     Blogger and GDACS emit RSS.
/// </remarks>
internal static class GeoRssSyndicationExtensionExample
{
    /// <summary>
    /// Provides example code for the GeoRssSyndicationExtension class, reading an RSS feed.
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

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions
                && item.FindExtension(GeoRssSyndicationExtension.MatchByType) is GeoRssSyndicationExtension itemExtension)
            {
                ExampleOutput.ShowGeoRssExtension(itemExtension);
            }
        }

        int count = feed.Channel.Items.Count(i => i.FindExtension(GeoRssSyndicationExtension.MatchByType) is not null);
        ExampleOutput.ShowItemsWithExtension(count, feed.Channel.Items.Count, "GeoRSS");

        // By default the framework will automatically determine what XML namespace attributes (xmlns) to write
        // on the root of the resource based on the extensions applied to extensible parent and child entities
        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Provides example code for the GeoRssSyndicationExtension class, reading an Atom feed.
    /// </summary>
    /// <remarks>
    ///     The reference GeoRSS publisher is Atom, so reading only RSS would leave the format that
    ///     matters most unexercised. This is also the first example in the project to load
    ///     <c>AtomFeedWithExtensions.xml</c>, which is the file's whole reason for existing.
    /// </remarks>
    public static void AtomExample()
    {
        AtomFeed feed = new();
        using (Stream inputStream = SampleDataPath.OpenRead(SampleDataPath.AtomFeedWithExtensions))
        {
            feed.Load(inputStream);
        }

        ExampleOutput.ShowLoaded("AtomFeed", feed.Title?.Content ?? string.Empty);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.HasExtensions
                && entry.FindExtension(GeoRssSyndicationExtension.MatchByType) is GeoRssSyndicationExtension entryExtension)
            {
                ExampleOutput.ShowGeoRssExtension(entryExtension);
            }
        }

        int count = feed.Entries.Count(e => e.FindExtension(GeoRssSyndicationExtension.MatchByType) is not null);
        ExampleOutput.ShowItemsWithExtension(count, feed.Entries.Count, "GeoRSS");

        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("AtomFeed");
    }
}