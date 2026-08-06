using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GeoRss;

/// <summary>
/// Covers the GML serialisation of GeoRSS: the same geometries, written as Geography Markup Language
/// inside <c>georss:where</c>.
/// </summary>
/// <remarks>
///     <para>
///     GeoRSS defines its geometries twice. Nothing observed anywhere uses the second form — GML
///     geometry appears <b>zero</b> times in the repository's corpus and zero times across a separate
///     sample of 1,934 live feeds — so everything in this file is derived from the standard rather than
///     from data.
///     </para>
///     <para>
///     <b>Coordinate ordering is latitude-first, and the standard says so outright.</b> OGC 17-002r1
///     §6 states that values "be specified in decimal degrees with axis order Latitude/Longitude" and
///     prints the defining WKT — <c>AXIS["latitude",north,ORDER[1]]</c>,
///     <c>AXIS["longitude",east,ORDER[2]]</c> — with the worked example
///     <c>&lt;gml:pos&gt;45.256 -71.92&lt;/gml:pos&gt;</c>. This is a citation, not an inference.
///     </para>
///     <para>
///     <b>Real GML feeds disagree with the standard, and the disagreement is recorded rather than
///     accommodated.</b> NASA's EONET publishes 7,030 <c>georss:where</c> elements containing
///     <c>gml:Point</c>, and of 1,470 entries whose titles name a US state, <b>1,468 are consistent with
///     longitude first and 0 with latitude first</b>. EONET also uses the GML 3.2 namespace
///     (<c>http://www.opengis.net/gml/3.2</c>) rather than the <c>http://www.opengis.net/gml</c> that
///     GeoRSS specifies — so its elements do not match this parser at all, and nothing is silently read
///     backwards. §5.1 carries that as an open decision with the numbers attached.
///     </para>
/// </remarks>
[TestClass]
public sealed class GeoRssGmlTests
{
    private const string Namespaces =
        """xmlns:georss="http://www.georss.org/georss" xmlns:gml="http://www.opengis.net/gml" """;

    private static string Feed(string itemElements) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" {Namespaces}>
          <channel>
            <title>A Feed</title>
            <link>https://example.com/</link>
            <description>A description.</description>
            <item><title>An Item</title>{itemElements}</item>
          </channel>
        </rss>
        """;

    private static GeoRssSyndicationExtensionContext? Context(string itemElements)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Feed(itemElements)), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Items.Single().Extensions.OfType<GeoRssSyndicationExtension>().SingleOrDefault()?.Context;
    }

    /// <summary>
    /// A GML point is read, and reports that it arrived as GML.
    /// </summary>
    /// <remarks>
    ///     <b>This is the row that pins the ordering.</b> The fixture is the standard's own worked
    ///     example, and <c>45.256 -71.92</c> is read as latitude 45.256 and longitude −71.92 — the same
    ///     as GeoRSS Simple. Feeds exist that write GML the other way round; see this class's remarks
    ///     for why they do not reach this code path.
    /// </remarks>
    [TestMethod]
    public void AGmlPoint_IsReadAndReportsItsEncoding()
    {
        GeoRssSyndicationExtensionContext? context = Context("""
            <georss:where><gml:Point><gml:pos>45.256 -71.92</gml:pos></gml:Point></georss:where>
            """);

        context.ShouldNotBeNull();
        context.Point.ShouldBe(new GeoRssPosition(45.256m, -71.92m), "latitude first, as in GeoRSS Simple");
        context.Encoding.ShouldBe(GeoRssEncoding.Gml);
        context.GeometryIsWrappedInWhere.ShouldBeTrue("GML has nowhere to live but inside georss:where");
    }

    /// <summary>
    /// A GML line and polygon are read from their position lists.
    /// </summary>
    /// <remarks>
    ///     A polygon's coordinates sit four elements deep — <c>Polygon</c>, <c>exterior</c>,
    ///     <c>LinearRing</c>, <c>posList</c> — which is why each shape is walked explicitly rather than
    ///     searched for.
    /// </remarks>
    [TestMethod]
    public void AGmlLineAndPolygon_AreReadFromTheirPositionLists()
    {
        GeoRssSyndicationExtensionContext? context = Context("""
            <georss:where>
              <gml:LineString><gml:posList>45.256 -110.45 46.46 -109.48</gml:posList></gml:LineString>
              <gml:Polygon><gml:exterior><gml:LinearRing>
                <gml:posList>45.256 -110.45 46.46 -109.48 43.84 -109.86 45.256 -110.45</gml:posList>
              </gml:LinearRing></gml:exterior></gml:Polygon>
            </georss:where>
            """);

        context.ShouldNotBeNull();
        context.Line.ShouldNotBeNull();
        context.Line.Positions.Count.ShouldBe(2);
        context.Line.Positions[0].ShouldBe(new GeoRssPosition(45.256m, -110.45m));

        context.Polygon.ShouldNotBeNull();
        context.Polygon.Positions.Count.ShouldBe(4);
        context.Polygon.IsClosed.ShouldBeTrue();
    }

    /// <summary>
    /// A GML envelope is read as a bounding box.
    /// </summary>
    /// <remarks>
    ///     GML names the corners rather than running them together, so this is the one shape where the
    ///     two serialisations disagree structurally: Simple writes four numbers in one element, GML
    ///     writes two pairs in two named ones.
    /// </remarks>
    [TestMethod]
    public void AGmlEnvelope_IsReadAsABoundingBox()
    {
        GeoRssSyndicationExtensionContext? context = Context("""
            <georss:where><gml:Envelope>
              <gml:lowerCorner>42.943 -71.032</gml:lowerCorner>
              <gml:upperCorner>43.039 -69.856</gml:upperCorner>
            </gml:Envelope></georss:where>
            """);

        context.ShouldNotBeNull();
        context.Box.ShouldNotBeNull();
        context.Box.Value.LowerLeft.ShouldBe(new GeoRssPosition(42.943m, -71.032m));
        context.Box.Value.UpperRight.ShouldBe(new GeoRssPosition(43.039m, -69.856m));
    }

    /// <summary>
    /// A GML document comes back out as GML.
    /// </summary>
    /// <remarks>
    ///     Rewriting a publisher's GML as Simple would be a transformation nobody asked for, and one
    ///     their own consumers might not accept. The <c>gml</c> prefix is declared once on the document
    ///     root rather than repeated on every geometry.
    /// </remarks>
    [TestMethod]
    public void AGmlDocument_ComesBackOutAsGml()
    {
        using MemoryStream source = new(Encoding.UTF8.GetBytes(
            Feed("<georss:where><gml:Point><gml:pos>45.256 -71.92</gml:pos></gml:Point></georss:where>")),
            writable: false);
        RssFeed feed = new();
        feed.Load(source);

        using MemoryStream saved = new();
        feed.Save(saved);
        string xml = Encoding.UTF8.GetString(saved.ToArray());

        xml.Contains("<gml:Point>", StringComparison.Ordinal).ShouldBeTrue("GML is written back as GML");
        xml.Contains(">45.256 -71.92<", StringComparison.Ordinal).ShouldBeTrue("latitude still first");
        xml.Contains("xmlns:gml=\"http://www.opengis.net/gml\"", StringComparison.Ordinal).ShouldBeTrue();

        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);

        GeoRssSyndicationExtensionContext reread = reloaded.Channel.Items.Single()
            .Extensions.OfType<GeoRssSyndicationExtension>().Single().Context;
        reread.Point.ShouldBe(new GeoRssPosition(45.256m, -71.92m));
        reread.Encoding.ShouldBe(GeoRssEncoding.Gml);
    }

    /// <summary>
    /// A where element holding something unrecognised yields nothing and does not throw.
    /// </summary>
    /// <remarks>
    ///     GML has shapes this library does not read — <c>gml:MultiSurface</c> and the rest — and the
    ///     standard will gain more. Ignoring them cleanly is what makes reading them later a decision
    ///     rather than a bug fix, and it is the same containment rule the Simple parser follows.
    /// </remarks>
    [TestMethod]
    public void AWhereElementHoldingSomethingUnrecognised_YieldsNothing()
    {
        GeoRssSyndicationExtensionContext? context = Context("""
            <georss:where><gml:MultiSurface><gml:surfaceMember/></gml:MultiSurface></georss:where>
            <georss:elev>313</georss:elev>
            """);

        context.ShouldNotBeNull("the rest of the entry still loads");
        context.HasGeometry.ShouldBeFalse();
        context.Encoding.ShouldBe(GeoRssEncoding.Simple, "nothing GML was read, so nothing claims to be GML");
        context.Elevation.ShouldBe(313m);
    }

    /// <summary>
    /// A feed spelling the GML namespace with a different prefix is still read.
    /// </summary>
    /// <remarks>
    ///     Selection resolves the prefix to a namespace URI and then matches on the URI, so the prefix a
    ///     publisher happens to choose is irrelevant. Worth pinning, because the obvious implementation —
    ///     matching the literal string <c>gml:</c> — would pass every other test in this file.
    /// </remarks>
    [TestMethod]
    public void AFeedSpellingTheGmlNamespaceDifferently_IsStillRead()
    {
        string document = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:georss="http://www.georss.org/georss" xmlns:g="http://www.opengis.net/gml">
              <channel>
                <title>A Feed</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item>
                  <title>An Item</title>
                  <georss:where><g:Point><g:pos>45.256 -71.92</g:pos></g:Point></georss:where>
                </item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        feed.Channel.Items.Single().Extensions.OfType<GeoRssSyndicationExtension>().Single()
            .Context.Point.ShouldBe(new GeoRssPosition(45.256m, -71.92m));
    }

    /// <summary>
    /// A Simple geometry inside a where element is not mistaken for GML.
    /// </summary>
    [TestMethod]
    public void ASimpleGeometryInsideAWhereElement_IsNotMistakenForGml()
    {
        GeoRssSyndicationExtensionContext? context =
            Context("<georss:where><georss:point>45.256 -71.92</georss:point></georss:where>");

        context.ShouldNotBeNull();
        context.Point.ShouldBe(new GeoRssPosition(45.256m, -71.92m));
        context.Encoding.ShouldBe(GeoRssEncoding.Simple, "the wrapper does not make it GML");
        context.GeometryIsWrappedInWhere.ShouldBeTrue();
    }
}