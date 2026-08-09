namespace Argotic.Extensions.Tests.Functionality.Core.GeoRss;

/// <summary>
/// Covers the GeoRSS elements this library reads.
/// </summary>
/// <remarks>
///     <para>
///     GeoRSS is what the canonical geographic feeds actually publish. The USGS earthquake service emits
///     <c>georss:point</c> and <c>georss:elev</c> and <b>zero</b> <c>geo:</c> elements, so
///     <see cref="BasicGeocodingSyndicationExtension"/> — the W3C Basic Geo vocabulary this library has
///     supported since 2007 — reads nothing from it.
///     </para>
///     <para>
///     Every value in this file comes from a census of real published feeds — a local corpus, not
///     committed here — which found <b>114</b> <c>point</c>, <b>47</b> <c>elev</c>, <b>25</b>
///     <c>featurename</c>, <b>25</b> <c>box</c>, and none of the rest.
///     </para>
/// </remarks>
[TestClass]
public sealed class GeoRssSyndicationExtensionTests
{
    private const string Namespace = "http://www.georss.org/georss";

    private static string RssFeed(string itemElements) => $"""
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

    private static string AtomFeed(string entryElements) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:georss="{Namespace}">
          <title>A Feed</title>
          <id>urn:example:feed</id>
          <updated>2026-08-06T00:00:00Z</updated>
          <entry>
            <title>An Entry</title>
            <id>urn:example:entry:1</id>
            <updated>2026-08-06T00:00:00Z</updated>
            {entryElements}
          </entry>
        </feed>
        """;

    internal static GeoRssSyndicationExtensionContext? ItemContext(string itemElements)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(RssFeed(itemElements)), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Items.Single().Extensions.OfType<GeoRssSyndicationExtension>().SingleOrDefault()?.Context;
    }

    private static GeoRssSyndicationExtensionContext? EntryContext(string entryElements)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(AtomFeed(entryElements)), writable: false);
        AtomFeed feed = new();
        feed.Load(stream);

        return feed.Entries.Single().Extensions.OfType<GeoRssSyndicationExtension>().SingleOrDefault()?.Context;
    }

    /// <summary>
    /// An entry carrying a point is read, in both syndication formats.
    /// </summary>
    /// <remarks>
    ///     RSS and Atom are filled by separate adapters, so an extension working in one is no evidence at
    ///     all about the other. USGS publishes Atom, which is the path that would otherwise go untested.
    /// </remarks>
    [TestMethod]
    public void AnEntryCarryingAPoint_IsReadInBothFormats()
    {
        const string Element = "<georss:point>36.981334686279 -121.45983123779</georss:point>";

        foreach (GeoRssSyndicationExtensionContext? context in new[] { ItemContext(Element), EntryContext(Element) })
        {
            context.ShouldNotBeNull();
            context.Point.ShouldNotBeNull();
            context.Point.Value.Latitude.ShouldBe(36.981334686279m);
            context.Point.Value.Longitude.ShouldBe(-121.45983123779m);
        }
    }

    /// <summary>
    /// A negative elevation is read as written.
    /// </summary>
    /// <remarks>
    ///     Every one of the 47 <c>georss:elev</c> values in the corpus is negative — they are earthquake
    ///     hypocentre depths. A reader that treated a negative as invalid would discard all of them.
    /// </remarks>
    /// <param name="written">The node value to write into <c>georss:elev</c>, in invariant-culture
    /// decimal notation; metres, and usually below the ellipsoid.</param>
    /// <param name="provenance">Where the value came from, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("-5280.0002098083", "the corpus value, in full precision")]
    [DataRow("-8100.0003814697", "another")]
    [DataRow("-8460", "a whole number of metres below the ellipsoid")]
    [DataRow("313", "the specification's own positive example")]
    public void ANegativeElevation_IsReadAsWritten(string written, string provenance)
    {
        GeoRssSyndicationExtensionContext? context = ItemContext($"<georss:elev>{written}</georss:elev>");

        context.ShouldNotBeNull(provenance);
        context.Elevation.ShouldBe(decimal.Parse(written, System.Globalization.NumberFormatInfo.InvariantInfo), provenance);
    }

    /// <summary>
    /// A bounding box keeps its corners in the order the feed wrote them.
    /// </summary>
    /// <remarks>
    ///     The value is Blogger's, from the corpus. Its four numbers are pairwise distinct, so any of the
    ///     twenty-three wrong orderings fails this.
    /// </remarks>
    [TestMethod]
    public void ABoundingBox_KeepsItsCornersInOrder()
    {
        GeoRssSyndicationExtensionContext? context =
            ItemContext("<georss:box>23.822399163821153 -29.864984 80.442866836178837 40.447516</georss:box>");

        context.ShouldNotBeNull();
        context.Box.ShouldNotBeNull();
        context.Box.Value.LowerLeft.Latitude.ShouldBe(23.822399163821153m);
        context.Box.Value.LowerLeft.Longitude.ShouldBe(-29.864984m);
        context.Box.Value.UpperRight.Latitude.ShouldBe(80.442866836178837m);
        context.Box.Value.UpperRight.Longitude.ShouldBe(40.447516m);
        context.Box.Value.IsWellOriented.ShouldBeTrue();
    }

    /// <summary>
    /// An entry carrying several kinds of geography keeps all of them.
    /// </summary>
    /// <remarks>
    ///     <b>The shape that ruled out a single geometry property.</b> Blogger writes the feature's name,
    ///     the point that locates it and the box that bounds it on the same entry. Modelling geography as
    ///     one value would have to discard two of the three.
    /// </remarks>
    [TestMethod]
    public void AnEntryCarryingSeveralKindsOfGeography_KeepsAllOfThem()
    {
        GeoRssSyndicationExtensionContext? context = ItemContext("""
            <georss:featurename>Nederland</georss:featurename>
            <georss:point>52.132633 5.2912659999999994</georss:point>
            <georss:box>23.822399163821153 -29.864984 80.442866836178837 40.447516</georss:box>
            """);

        context.ShouldNotBeNull();
        context.FeatureName.ShouldBe("Nederland");
        context.Point.ShouldNotBeNull();
        context.Point.Value.Latitude.ShouldBe(52.132633m);
        context.Box.ShouldNotBeNull();
        context.HasGeometry.ShouldBeTrue();
    }

    /// <summary>
    /// A line and a polygon are read as their sequences of positions.
    /// </summary>
    /// <remarks>
    ///     Both values are the specification's own examples. Neither element appears anywhere in the
    ///     corpus, which is recorded rather than hidden — they are implemented because the standard
    ///     defines them, not because anything observed emits them.
    /// </remarks>
    [TestMethod]
    public void ALineAndAPolygon_AreReadAsSequencesOfPositions()
    {
        GeoRssSyndicationExtensionContext? context = ItemContext("""
            <georss:line>45.256 -110.45 46.46 -109.48 43.84 -109.86</georss:line>
            <georss:polygon>45.256 -110.45 46.46 -109.48 43.84 -109.86 45.256 -110.45</georss:polygon>
            """);

        context.ShouldNotBeNull();
        context.Line.ShouldNotBeNull();
        context.Line.Positions.Count.ShouldBe(3);
        context.Line.Positions[0].ShouldBe(new GeoRssPosition(45.256m, -110.45m));
        context.Line.Positions[2].ShouldBe(new GeoRssPosition(43.84m, -109.86m));

        context.Polygon.ShouldNotBeNull();
        context.Polygon.Positions.Count.ShouldBe(4);
        context.Polygon.IsClosed.ShouldBeTrue("the last position repeats the first");
    }

    /// <summary>
    /// A polygon that does not close is read, and says so.
    /// </summary>
    /// <remarks>
    ///     Neither auto-closing nor rejecting. Closing the ring would report a geometry the feed does not
    ///     contain; rejecting it would lose a usable one. Reporting keeps both the data and the truth
    ///     about it.
    /// </remarks>
    [TestMethod]
    public void APolygonThatDoesNotClose_IsReadAndSaysSo()
    {
        GeoRssSyndicationExtensionContext? context =
            ItemContext("<georss:polygon>45.256 -110.45 46.46 -109.48 43.84 -109.86 44.0 -110.0</georss:polygon>");

        context.ShouldNotBeNull();
        context.Polygon.ShouldNotBeNull();
        context.Polygon.Positions.Count.ShouldBe(4, "the positions are kept verbatim");
        context.Polygon.IsClosed.ShouldBeFalse("and the non-conformance is reported, not repaired");
    }

    /// <summary>
    /// The non-geometric properties are read.
    /// </summary>
    [TestMethod]
    public void TheNonGeometricProperties_AreRead()
    {
        GeoRssSyndicationExtensionContext? context = ItemContext("""
            <georss:point>45.256 -110.45</georss:point>
            <georss:featuretypetag>city</georss:featuretypetag>
            <georss:relationshiptag>is-centered-at</georss:relationshiptag>
            <georss:featurename>Podunk</georss:featurename>
            <georss:elev>313</georss:elev>
            <georss:floor>2</georss:floor>
            <georss:radius>500</georss:radius>
            """);

        context.ShouldNotBeNull();
        context.FeatureTypeTag.ShouldBe("city");
        context.RelationshipTag.ShouldBe("is-centered-at");
        context.FeatureName.ShouldBe("Podunk");
        context.Elevation.ShouldBe(313m);
        context.Floor.ShouldBe(2);
        context.Radius.ShouldBe(500m);
    }

    /// <summary>
    /// A basement is a floor.
    /// </summary>
    [TestMethod]
    public void ABasement_IsAFloor()
    {
        ItemContext("<georss:floor>-2</georss:floor>")!.Floor.ShouldBe(-2);
    }

    /// <summary>
    /// Absent elements report nothing rather than a sentinel.
    /// </summary>
    /// <remarks>
    ///     The sibling <see cref="BasicGeocodingSyndicationExtensionContext"/> uses
    ///     <see cref="decimal.MinValue"/> to mean "absent", which §2.45 records as a defect shape: a
    ///     value the property's own setter would never accept, handed back to callers. Nothing here does
    ///     that — absence is <see langword="null"/>, and zero is a real coordinate off the coast of
    ///     Ghana rather than a way of saying nothing.
    /// </remarks>
    [TestMethod]
    public void AbsentElements_ReportNothingRatherThanASentinel()
    {
        GeoRssSyndicationExtensionContext? context = ItemContext("<georss:featurename>Somewhere</georss:featurename>");

        context.ShouldNotBeNull();
        context.Point.ShouldBeNull();
        context.Box.ShouldBeNull();
        context.Line.ShouldBeNull();
        context.Polygon.ShouldBeNull();
        context.Elevation.ShouldBeNull();
        context.Floor.ShouldBeNull();
        context.Radius.ShouldBeNull();
        context.HasGeometry.ShouldBeFalse();

        ItemContext("<georss:point>0 0</georss:point>")!.Point.ShouldBe(new GeoRssPosition(0m, 0m), "zero is a place");
    }

    /// <summary>
    /// A geometry wrapped in a where element is read.
    /// </summary>
    /// <remarks>
    ///     The specification defines <c>georss:where</c> as the element that signals geographic content,
    ///     so a geometry may be a direct child or nested inside it. A nested one is a <em>grandchild</em>,
    ///     which the ordinary child-axis lookup does not see — this is a different code path, not a
    ///     formality.
    /// </remarks>
    [TestMethod]
    public void AGeometryWrappedInAWhereElement_IsRead()
    {
        GeoRssSyndicationExtensionContext? wrapped =
            ItemContext("<georss:where><georss:point>45.256 -110.45</georss:point></georss:where>");

        wrapped.ShouldNotBeNull();
        wrapped.Point.ShouldBe(new GeoRssPosition(45.256m, -110.45m));
        wrapped.GeometryIsWrappedInWhere.ShouldBeTrue("so that it can be written back the way it arrived");

        GeoRssSyndicationExtensionContext? flat = ItemContext("<georss:point>45.256 -110.45</georss:point>");
        flat!.Point.ShouldBe(wrapped.Point, "the wrapper changes where it is found, not what it means");
        flat.GeometryIsWrappedInWhere.ShouldBeFalse();
    }

    /// <summary>
    /// A channel does not pick up geography belonging to its items.
    /// </summary>
    /// <remarks>
    ///     The guard against fixing the <c>where</c> case with a descendant query. An over-broad lookup
    ///     would give the whole feed the location of whichever item happened to be first.
    /// </remarks>
    [TestMethod]
    public void AChannel_DoesNotPickUpGeographyBelongingToItsItems()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(RssFeed("<georss:point>45.256 -110.45</georss:point>")), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        feed.Channel.Extensions.OfType<GeoRssSyndicationExtension>().ShouldBeEmpty(
            "the point belongs to the item, not to the feed");
        feed.Channel.Items.Single().Extensions.OfType<GeoRssSyndicationExtension>().ShouldHaveSingleItem();
    }

    /// <summary>
    /// The extension is matched by type.
    /// </summary>
    [TestMethod]
    public void TheExtension_IsMatchedByType()
    {
        GeoRssSyndicationExtension extension = new();

        GeoRssSyndicationExtension.MatchByType(extension).ShouldBeTrue();
        GeoRssSyndicationExtension.MatchByType(new BasicGeocodingSyndicationExtension()).ShouldBeFalse();
        extension.XmlPrefix.ShouldBe("georss");
        extension.XmlNamespace.ShouldBe(Namespace);
    }
}