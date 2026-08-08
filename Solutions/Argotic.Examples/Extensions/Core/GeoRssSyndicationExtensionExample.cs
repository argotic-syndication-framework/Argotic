using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads a <see cref="GeoRssSyndicationExtension"/> out of both an RSS and an Atom feed.
/// </summary>
/// <remarks>
///     GeoRSS is read from both syndication formats because the two are filled by separate adapters and
///     both matter in practice: the reference publisher, the USGS earthquake service, emits Atom, while
///     Blogger and GDACS emit RSS.
/// </remarks>
internal static class GeoRssSyndicationExtensionExample
{
    /// <summary>
    /// Finds the extension on an <see cref="RssFeed"/>'s channel and items, then writes the feed back out.
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
    /// Finds the extension on an <see cref="AtomFeed"/> and its entries, then writes the feed back out.
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

    /// <summary>
    /// Builds each GeoRSS geometry in turn, in both encodings, and reads them back.
    /// </summary>
    /// <remarks>
    ///     GeoRSS models four geometries and this library supports two encodings of them, and the sample
    ///     corpus only ever exercises point and box in Simple form. Line, polygon and the GML encoding are
    ///     reached here and nowhere else.
    ///     <para>
    ///     A polygon's ring must close: the last position repeats the first. That is the rule a
    ///     hand-assembled polygon most often breaks, and a consumer is entitled to reject an open ring.
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

        // A point, in the Simple encoding. The most common shape by a wide margin.
        GeoRssSyndicationExtension point = new();
        point.Context.Point = new GeoRssPosition(51.5074m, -0.1278m);
        point.Context.Elevation = 11m;
        point.Context.FeatureName = "London, United Kingdom";
        item.Extensions.Add(point);

        // A line: an ordered run of positions, here a rough London to Amsterdam path.
        GeoRssSyndicationExtension line = new();
        line.Context.Line = new GeoRssLine(
        [
            new GeoRssPosition(51.5074m, -0.1278m),
            new GeoRssPosition(52.0907m, 5.1214m),
            new GeoRssPosition(52.3676m, 4.9041m),
        ]);
        Console.WriteLine($"Line positions: {line.Context.Line.Positions.Count}");

        // A box: lower-left and upper-right corners. Roughly the United Kingdom.
        GeoRssSyndicationExtension box = new();
        box.Context.Box = new GeoRssBox(
            new GeoRssPosition(49.96m, -7.57m),
            new GeoRssPosition(58.64m, 1.68m));
        Console.WriteLine($"Box well oriented: {box.Context.Box.Value.IsWellOriented}");

        // A polygon: a closed ring, so the final position repeats the first.
        GeoRssSyndicationExtension polygon = new();
        polygon.Context.Polygon = new GeoRssPolygon(
        [
            new GeoRssPosition(51.28m, -0.51m),
            new GeoRssPosition(51.28m, 0.33m),
            new GeoRssPosition(51.69m, 0.33m),
            new GeoRssPosition(51.69m, -0.51m),
            new GeoRssPosition(51.28m, -0.51m),
        ]);
        Console.WriteLine($"Polygon positions: {polygon.Context.Polygon.Positions.Count} (first repeated last to close the ring)");

        // The same point, in the GML encoding rather than Simple. Different markup, same geometry --
        // and the encoding is a property of how it is written, not of what it means.
        GeoRssSyndicationExtension gml = new();
        gml.Context.Encoding = GeoRssEncoding.Gml;
        gml.Context.Point = new GeoRssPosition(52.3676m, 4.9041m);
        gml.Context.FeatureName = "Amsterdam, Netherlands";
        Console.WriteLine($"GML encoding: {gml.Context.Encoding}");
        ExampleOutput.ShowGeoRssExtension(gml);

        feed.Channel.Items.Add(item);

        using MemoryStream saved = new();
        feed.Save(saved);
        ExampleOutput.ShowSaved("RssFeed");

        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);

        RssItem readItem = reloaded.Channel.Items[0];
        if (readItem.FindExtension(GeoRssSyndicationExtension.MatchByType) is GeoRssSyndicationExtension readBack)
        {
            ExampleOutput.ShowGeoRssExtension(readBack);
        }
    }
}