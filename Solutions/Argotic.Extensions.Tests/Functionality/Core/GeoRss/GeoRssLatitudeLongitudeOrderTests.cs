using System.Text;
using System.Xml;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GeoRss;

/// <summary>
/// Covers the one thing about GeoRSS that is easy to get wrong and hard to notice: which number is the
/// latitude.
/// </summary>
/// <remarks>
///     <para>
///     GeoRSS writes <b>latitude first</b>. Reversing that is the classic defect in every implementation
///     of this format, and it is silent, because across most of the populated world both orderings are
///     numerically legal coordinates. A feed read backwards does not throw; it just puts the content
///     somewhere else on Earth.
///     </para>
///     <para>
///     This file is separate and small on purpose. It is the file somebody should read when they change
///     anything about how coordinates are parsed or written.
///     </para>
/// </remarks>
[TestClass]
public sealed class GeoRssLatitudeLongitudeOrderTests
{
    private const string Namespace = "http://www.georss.org/georss";

    private static RssFeed FeedCarrying(string itemElements)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:georss="{Namespace}">
              <channel>
                <title>A Feed</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Item</title>{itemElements}</item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static GeoRssSyndicationExtensionContext ContextOf(RssFeed feed) =>
        feed.Channel.Items.Single().Extensions.OfType<GeoRssSyndicationExtension>().Single().Context;

    /// <summary>
    /// A real earthquake lands where the earthquake was.
    /// </summary>
    /// <remarks>
    ///     The value is verbatim from the USGS feed in the repository's corpus. Read correctly it is
    ///     northern-hemisphere and western-hemisphere — California. Read backwards the latitude would be
    ///     121.46, which is not a latitude at all, so this row fails loudly.
    /// </remarks>
    [TestMethod]
    public void ARealEarthquake_LandsWhereTheEarthquakeWas()
    {
        GeoRssSyndicationExtensionContext context =
            ContextOf(FeedCarrying("<georss:point>36.981334686279 -121.45983123779</georss:point>"));

        context.Point.ShouldNotBeNull();
        context.Point.Value.Latitude.ShouldBe(36.981334686279m);
        context.Point.Value.Longitude.ShouldBe(-121.45983123779m);

        context.Point.Value.Latitude.ShouldBeInRange(-90m, 90m, "a latitude out of range would mean the axes were swapped");
        context.Point.Value.Latitude.ShouldBeGreaterThan(0m, "California is north of the equator");
        context.Point.Value.Longitude.ShouldBeLessThan(0m, "and west of Greenwich");
    }

    /// <summary>
    /// London is read as London, and not as a point in the Atlantic.
    /// </summary>
    /// <remarks>
    ///     <b>The row a range check cannot catch.</b> Swap London's coordinates and you get latitude
    ///     −0.1278, longitude 51.5074 — both entirely legal, both in range, and a thousand miles from
    ///     London. Only asserting the two members by name distinguishes them, which is why no range
    ///     validation is performed anywhere in this family: it would pass on exactly the case it was
    ///     written to catch.
    /// </remarks>
    [TestMethod]
    public void London_IsReadAsLondon()
    {
        GeoRssSyndicationExtensionContext context =
            ContextOf(FeedCarrying("<georss:point>51.5074 -0.1278</georss:point>"));

        context.Point!.Value.Latitude.ShouldBe(51.5074m);
        context.Point.Value.Longitude.ShouldBe(-0.1278m);
    }

    /// <summary>
    /// A box reads its four numbers as lower-left latitude, lower-left longitude, upper-right latitude, upper-right longitude.
    /// </summary>
    /// <remarks>
    ///     Four pairwise-distinct numbers, so all twenty-three wrong orderings fail. The specification's
    ///     own example is used because its corners are unambiguous.
    /// </remarks>
    [TestMethod]
    public void ABox_ReadsItsFourNumbersInSpecificationOrder()
    {
        GeoRssSyndicationExtensionContext context =
            ContextOf(FeedCarrying("<georss:box>42.943 -71.032 43.039 -69.856</georss:box>"));

        context.Box.ShouldNotBeNull();
        context.Box.Value.LowerLeft.Latitude.ShouldBe(42.943m);
        context.Box.Value.LowerLeft.Longitude.ShouldBe(-71.032m);
        context.Box.Value.UpperRight.Latitude.ShouldBe(43.039m);
        context.Box.Value.UpperRight.Longitude.ShouldBe(-69.856m);

        context.Box.Value.LowerLeft.Latitude.ShouldBeLessThan(context.Box.Value.UpperRight.Latitude);
        context.Box.Value.LowerLeft.Longitude.ShouldBeLessThan(context.Box.Value.UpperRight.Longitude);
    }

    /// <summary>
    /// The saved document writes latitude before longitude.
    /// </summary>
    /// <remarks>
    ///     <b>The single most important test in this family.</b> Every other test here goes through the
    ///     object model, and a reader and a writer that are wrong in the <em>same</em> way round-trip
    ///     perfectly through it — the values come back exactly as they went in, and every assertion
    ///     passes, while the emitted feed is wrong for the rest of the world.
    ///     <para>
    ///     Only asserting on the serialised text catches a symmetric axis defect. So this one builds the
    ///     geometry in code, saves, and reads the token order out of the XML.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TheSavedDocument_WritesLatitudeBeforeLongitude()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "A Feed",
                Link = new Uri("https://example.com/"),
                Description = "A description.",
            },
        };

        RssItem item = new() { Title = "An Item", Description = "Text." };
        GeoRssSyndicationExtension extension = new();
        extension.Context.Point = new GeoRssPosition(36.981m, -121.46m);
        extension.Context.Box = new GeoRssBox(new GeoRssPosition(42.943m, -71.032m), new GeoRssPosition(43.039m, -69.856m));
        item.Extensions.Add(extension);
        feed.Channel.Items.Add(item);

        using MemoryStream stream = new();
        using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
        {
            feed.Save(writer);
        }

        string xml = Encoding.UTF8.GetString(stream.ToArray());

        xml.Contains(">36.981 -121.46<", StringComparison.Ordinal).ShouldBeTrue(
            "latitude is written first; a symmetric reader/writer bug is invisible in the object model");
        xml.Contains(">42.943 -71.032 43.039 -69.856<", StringComparison.Ordinal).ShouldBeTrue(
            "and a box writes lower-left before upper-right");
    }

    /// <summary>
    /// A point survives a round trip without losing a digit.
    /// </summary>
    /// <remarks>
    ///     Catches a <see cref="double"/> where a <see cref="decimal"/> was meant. The USGS value has
    ///     fourteen significant digits and a <see cref="double"/> would not return all of them.
    /// </remarks>
    [TestMethod]
    public void APoint_SurvivesARoundTripWithoutLosingADigit()
    {
        RssFeed loaded = FeedCarrying("<georss:point>36.981334686279 -121.45983123779</georss:point>");

        using MemoryStream stream = new();
        loaded.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        RssFeed reloaded = new();
        reloaded.Load(stream);

        ContextOf(reloaded).Point.ShouldBe(new GeoRssPosition(36.981334686279m, -121.45983123779m));
    }
}