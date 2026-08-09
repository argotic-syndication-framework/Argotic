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
///     GeoRSS specifies. §5.1 carries that as an open decision with the numbers attached.
///     </para>
///     <para>
///     <b>That those feeds do not match is enforced, not assumed.</b> It first shipped as an assumption
///     and was false: the namespace manager preferred whatever URI the document bound to <c>gml</c>, so
///     GML 3.2 resolved through this parser and its coordinates were read transposed. The manager now
///     binds the constant, and
///     <see cref="AFeedUsingADifferentGmlNamespace_IsNotRead"/> is what keeps it that way.
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

    /// <summary>
    /// Returns a navigator positioned on the feed's single item, for loading an extension directly.
    /// </summary>
    /// <param name="document">The feed document.</param>
    /// <returns>A navigator on the <c>item</c> element.</returns>
    private static XPathNavigator ItemNavigator(string document)
    {
        using StringReader reader = new(document);
        XPathNavigator navigator = new XPathDocument(reader).CreateNavigator();
        navigator.MoveToFollowing("item", string.Empty).ShouldBeTrue();

        return navigator;
    }

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
    ///     <para>
    ///     Rewriting a publisher's GML as Simple would be a transformation nobody asked for, and one
    ///     their own consumers might not accept.
    /// </para>
    ///     <para>
    ///     The <c>gml</c> prefix is declared on the outermost GML element rather than on the document
    ///     root, and the nested elements reuse it. Declaring it at the root would mean every GeoRSS
    ///     document carried it whether or not it used GML, and would put a second declaration outside
    ///     the adapter's duplicate-prefix guard — where a second extension claiming <c>gml</c> would
    ///     abort the save with a duplicate-attribute error part way through the document.
    ///     </para>
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

        xml.Contains("<gml:Point", StringComparison.Ordinal).ShouldBeTrue("GML is written back as GML");
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
    /// A feed using a different GML namespace is not read.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>The regression test for a defect that shipped.</b> The namespace manager originally
    ///     preferred whatever URI the document bound to the <c>gml</c> prefix, so a GML 3.2 feed resolved
    ///     through this parser after all — and GML 3.2 feeds in the wild write longitude first. NASA's
    ///     EONET has 7,030 such geometries; every one was read transposed, and 3,762 produced a latitude
    ///     outside ±90.
    ///     </para>
    ///     <para>
    ///     The manager now binds the constant, so the prefix a document chooses is irrelevant and the
    ///     <em>namespace</em> decides. That is what makes "a different GML namespace does not match"
    ///     true rather than merely intended, and this is the assertion that holds it true.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AFeedUsingADifferentGmlNamespace_IsNotRead()
    {
        string document = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:georss="http://www.georss.org/georss" xmlns:gml="http://www.opengis.net/gml/3.2">
              <channel>
                <title>A Feed</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item>
                  <title>Wildfire, Colorado</title>
                  <georss:where><gml:Point><gml:pos>-105.5 39.5</gml:pos></gml:Point></georss:where>
                </item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        GeoRssSyndicationExtensionContext? context = feed.Channel.Items.Single()
            .Extensions.OfType<GeoRssSyndicationExtension>().SingleOrDefault()?.Context;

        (context?.Point).ShouldBeNull(
            "a latitude of -105.5 is not a latitude; reading this feed would place a Colorado fire in Turkey");
    }

    /// <summary>
    /// A geometry beside a where element is not hidden by it, in either direction.
    /// </summary>
    /// <remarks>
    ///     The geometry kinds are independent, so an entry may carry a direct-child box and a wrapped
    ///     point at once. Returning after the first match discarded whichever came second and then
    ///     reported the wrong <see cref="GeoRssEncoding"/> for the survivor — which meant the save wrote
    ///     the document back in a serialisation the publisher had not used.
    /// </remarks>
    [TestMethod]
    public void AGeometryBesideAWhereElement_IsNotHiddenByIt()
    {
        GeoRssSyndicationExtensionContext? context = Context("""
            <georss:box>1 2 3 4</georss:box>
            <georss:where><gml:Point><gml:pos>45.256 -71.92</gml:pos></gml:Point></georss:where>
            """);

        context.ShouldNotBeNull();
        context.Box.ShouldBe(new GeoRssBox(new GeoRssPosition(1m, 2m), new GeoRssPosition(3m, 4m)));
        context.Point.ShouldBe(new GeoRssPosition(45.256m, -71.92m), "the wrapped point is not discarded");
        context.Encoding.ShouldBe(GeoRssEncoding.Gml);
    }

    /// <summary>
    /// Loading a second document does not leave the first document's geometry behind.
    /// </summary>
    /// <remarks>
    ///     <see cref="GeoRssSyndicationExtensionContext.Encoding"/> and
    ///     <see cref="GeoRssSyndicationExtensionContext.GeometryIsWrappedInWhere"/> only ever move
    ///     towards GML and <see langword="true"/>, so without a reset a reused extension instance would
    ///     publish coordinates the second document never contained.
    /// </remarks>
    [TestMethod]
    public void LoadingASecondDocument_DoesNotLeaveTheFirstBehind()
    {
        GeoRssSyndicationExtension extension = new();

        extension.Load(ItemNavigator(Feed("<georss:where><gml:Point><gml:pos>45.256 -71.92</gml:pos></gml:Point></georss:where>")));
        extension.Context.Point.ShouldNotBeNull("the first document did carry a point");

        extension.Load(ItemNavigator(Feed("<georss:elev>313</georss:elev>")));

        extension.Context.Point.ShouldBeNull("the second document carried none");
        extension.Context.Encoding.ShouldBe(GeoRssEncoding.Simple);
        extension.Context.GeometryIsWrappedInWhere.ShouldBeFalse();
        extension.Context.Elevation.ShouldBe(313m);
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