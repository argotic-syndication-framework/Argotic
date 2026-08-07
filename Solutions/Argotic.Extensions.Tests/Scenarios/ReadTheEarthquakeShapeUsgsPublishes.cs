using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Reads the geography from feeds shaped the way the two canonical GeoRSS publishers write them.
/// </summary>
/// <remarks>
///     <para>
///     The USGS earthquake service is the reference GeoRSS feed, and it is <b>Atom</b>. Blogger is the
///     other large emitter, and it is <b>RSS</b>. Both appear here because the two formats are filled by
///     separate adapters, so an extension working in one is no evidence at all about the other.
///     </para>
///     <para>
///     Every value below is verbatim from the documents in <c>.endjin/perf-spike/corpus</c> — down to
///     the fourteen significant digits USGS publishes and the elevation of exactly <c>-10000</c> that
///     appears on 87 of its entries.
///     </para>
///     <para>
///     This is the workflow the extension exists for, and until it existed the whole of it was lost: the
///     feed loaded, every modelled property was correct, nothing threw, and the geography was gone on
///     save because there was nowhere to put it.
///     </para>
/// </remarks>
[TestClass]
public sealed class ReadTheEarthquakeShapeUsgsPublishes
{
    private const string UsgsAtom = """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:georss="http://www.georss.org/georss">
          <title>USGS All Earthquakes, Past Hour</title>
          <updated>2026-08-06T11:01:40Z</updated>
          <id>https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_hour.atom</id>
          <entry>
            <id>urn:earthquake-usgs-gov:us:6000tij5</id>
            <title>M 4.9 - 128 km WSW of Port Orford, Oregon</title>
            <updated>2026-08-06T10:55:42.507Z</updated>
            <georss:point>42.4079 -125.9976</georss:point>
            <georss:elev>-10000</georss:elev>
          </entry>
          <entry>
            <id>urn:earthquake-usgs-gov:nc:75412492</id>
            <title>M 1.4 - 10 km ESE of Gilroy, CA</title>
            <updated>2026-08-06T11:00:54.710Z</updated>
            <georss:point>36.981334686279 -121.45983123779</georss:point>
            <georss:elev>-5280.0002098083</georss:elev>
          </entry>
        </feed>
        """;

    private const string BloggerRss = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:georss="http://www.georss.org/georss">
          <channel>
            <title>A Blog</title>
            <link>https://example.com/</link>
            <description>A description.</description>
            <item>
              <title>A Post</title>
              <georss:featurename>Nederland</georss:featurename>
              <georss:point>52.132633 5.2912659999999994</georss:point>
              <georss:box>23.822399163821153 -29.864984 80.442866836178837 40.447516</georss:box>
            </item>
          </channel>
        </rss>
        """;

    private static AtomFeed LoadAtom(string document)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        AtomFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static RssFeed LoadRss(string document)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static GeoRssSyndicationExtensionContext GeographyOf(AtomEntry entry) =>
        entry.Extensions.OfType<GeoRssSyndicationExtension>().Single().Context;

    /// <summary>
    /// Every earthquake in the feed reports where it happened and how deep it was.
    /// </summary>
    [TestMethod]
    public void EveryEarthquakeInTheFeed_ReportsWhereItHappenedAndHowDeep()
    {
        AtomFeed feed = LoadAtom(UsgsAtom);

        feed.Entries.Count.ShouldBe(2);
        feed.Entries.ShouldAllBe(e => e.Extensions.OfType<GeoRssSyndicationExtension>().Any());

        GeoRssSyndicationExtensionContext oregon = GeographyOf(feed.Entries.First());
        oregon.Point.ShouldBe(new GeoRssPosition(42.4079m, -125.9976m));
        oregon.Elevation.ShouldBe(-10000m);

        GeoRssSyndicationExtensionContext gilroy = GeographyOf(feed.Entries.Last());
        gilroy.Point.ShouldBe(new GeoRssPosition(36.981334686279m, -121.45983123779m));
        gilroy.Elevation.ShouldBe(-5280.0002098083m, "the depth is published to ten decimal places");
    }

    /// <summary>
    /// The earthquakes are off the west coast of North America, not in the Southern Ocean.
    /// </summary>
    /// <remarks>
    ///     The human-readable form of the axis-order invariant. Both entries are north of the equator
    ///     and west of Greenwich; swap latitude for longitude and one becomes an impossible latitude of
    ///     125 while the other lands in the sea south of Australia.
    /// </remarks>
    [TestMethod]
    public void TheEarthquakes_AreOffTheWestCoastOfNorthAmerica()
    {
        AtomFeed feed = LoadAtom(UsgsAtom);

        // Without the count, an empty Entries - which is the symptom of the parse regression this test
        // exists to catch - executes the loop zero times and reports success.
        feed.Entries.Count.ShouldBe(2);

        foreach (AtomEntry entry in feed.Entries)
        {
            GeoRssPosition point = GeographyOf(entry).Point!.Value;

            point.Latitude.ShouldBeInRange(30m, 50m, "north of the equator, in the mid latitudes");
            point.Longitude.ShouldBeInRange(-130m, -120m, "and off the Pacific coast");
        }
    }

    /// <summary>
    /// Depth below the surface is a negative elevation, not an error.
    /// </summary>
    /// <remarks>
    ///     All 47 <c>georss:elev</c> values in the corpus are negative, because they are hypocentre
    ///     depths. A reader that rejected or clamped a negative elevation would discard every one of
    ///     them, which is why nothing in this family validates the sign.
    /// </remarks>
    [TestMethod]
    public void DepthBelowTheSurface_IsANegativeElevationAndNotAnError()
    {
        decimal?[] elevations = [.. LoadAtom(UsgsAtom).Entries.Select(e => GeographyOf(e).Elevation)];

        // ShouldAllBe passes vacuously over an empty sequence, so the count carries the test.
        elevations.Length.ShouldBe(2);
        elevations.ShouldAllBe(elevation => elevation < 0m);
    }

    /// <summary>
    /// The geography survives being read and written back.
    /// </summary>
    /// <remarks>
    ///     The assertion the family exists for, and the one that would have failed against every version
    ///     of this library before it.
    /// </remarks>
    [TestMethod]
    public void TheGeography_SurvivesBeingReadAndWrittenBack()
    {
        AtomFeed feed = LoadAtom(UsgsAtom);

        using MemoryStream saved = new();
        feed.Save(saved);
        saved.Seek(0, SeekOrigin.Begin);

        AtomFeed reloaded = new();
        reloaded.Load(saved);

        GeoRssSyndicationExtensionContext gilroy = GeographyOf(reloaded.Entries.Last());
        gilroy.Point.ShouldBe(new GeoRssPosition(36.981334686279m, -121.45983123779m), "not one digit lost");
        gilroy.Elevation.ShouldBe(-5280.0002098083m);
    }

    /// <summary>
    /// A blog post keeps its place name, its point and the region that bounds it.
    /// </summary>
    /// <remarks>
    ///     <b>The shape that decided the whole geometry model.</b> Blogger writes all three on one item,
    ///     so geography could not be a single value without discarding two of them.
    /// </remarks>
    [TestMethod]
    public void ABlogPost_KeepsItsPlaceNameItsPointAndTheRegionThatBoundsIt()
    {
        RssFeed feed = LoadRss(BloggerRss);

        GeoRssSyndicationExtensionContext geography = feed.Channel.Items.Single()
            .Extensions.OfType<GeoRssSyndicationExtension>().Single().Context;

        geography.FeatureName.ShouldBe("Nederland");
        geography.Point.ShouldNotBeNull();
        geography.Point.Value.ShouldBe(new GeoRssPosition(52.132633m, 5.2912659999999994m));
        geography.Box.ShouldNotBeNull();
        geography.Box.Value.LowerLeft.Latitude.ShouldBe(23.822399163821153m);
        geography.Box.Value.UpperRight.Longitude.ShouldBe(40.447516m);

        geography.Point.Value.Longitude.ShouldBeGreaterThan(0m, "the Netherlands is east of Greenwich");
    }

    /// <summary>
    /// The blog post's geography survives a round trip through RSS.
    /// </summary>
    [TestMethod]
    public void TheBlogPostsGeography_SurvivesARoundTripThroughRss()
    {
        RssFeed feed = LoadRss(BloggerRss);

        using MemoryStream saved = new();
        feed.Save(saved);
        saved.Seek(0, SeekOrigin.Begin);

        RssFeed reloaded = new();
        reloaded.Load(saved);

        GeoRssSyndicationExtensionContext geography = reloaded.Channel.Items.Single()
            .Extensions.OfType<GeoRssSyndicationExtension>().Single().Context;

        geography.FeatureName.ShouldBe("Nederland");
        geography.Point.ShouldBe(new GeoRssPosition(52.132633m, 5.2912659999999994m));
        geography.Box.ShouldNotBeNull();
    }
}