using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="GeoRssSyndicationExtension"/>.
/// </summary>
/// <remarks>
///     <para>
///     The four geometry kinds are four independent properties rather than one geometry, because real
///     feeds carry more than one at once: Blogger writes <c>featurename</c>, <c>point</c> and
///     <c>box</c> on the same entry — the place, the point that names it, and the region it sits in.
///     Collapsing those into a single value would have to discard two of the three.
///     </para>
///     <para>
///     Every member is single-valued and first-wins. A second element of the same kind is not read;
///     nothing in either surveyed corpus emits one.
///     </para>
/// </remarks>
public class GeoRssSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeoRssSyndicationExtensionContext"/> class.
    /// </summary>
    public GeoRssSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the single position this entry describes.
    /// </summary>
    /// <value>A <see cref="GeoRssPosition"/>, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     <c>georss:point</c>, and by a wide margin the element that matters: <b>114</b> of the 211
    ///     GeoRSS elements in the repository's corpus are points, and both USGS and GDACS emit exactly
    ///     one per entry and nothing else geometric.
    /// </remarks>
    public GeoRssPosition? Point { get; set; }

    /// <summary>
    /// Gets or sets the line this entry describes.
    /// </summary>
    /// <value>A <see cref="GeoRssLine"/>, or <see langword="null"/> if none was specified.</value>
    public GeoRssLine? Line { get; set; }

    /// <summary>
    /// Gets or sets the polygon this entry describes.
    /// </summary>
    /// <value>A <see cref="GeoRssPolygon"/>, or <see langword="null"/> if none was specified.</value>
    public GeoRssPolygon? Polygon { get; set; }

    /// <summary>
    /// Gets or sets the bounding box this entry describes.
    /// </summary>
    /// <value>A <see cref="GeoRssBox"/>, or <see langword="null"/> if none was specified.</value>
    public GeoRssBox? Box { get; set; }

    /// <summary>
    /// Gets or sets the elevation, in metres above the WGS84 ellipsoid.
    /// </summary>
    /// <value>The elevation, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     Routinely negative, and not an error when it is: every one of the 47 <c>georss:elev</c> values
    ///     in the corpus is below zero, because they are earthquake hypocentre depths
    ///     (<c>-5280.0002098083</c>). Nothing here clamps or rejects a negative.
    /// </remarks>
    public decimal? Elevation { get; set; }

    /// <summary>
    /// Gets or sets the floor of a building this entry describes.
    /// </summary>
    /// <value>The floor number, or <see langword="null"/> if none was specified.</value>
    /// <remarks>Negative floors are ordinary — a basement is a floor.</remarks>
    public int? Floor { get; set; }

    /// <summary>
    /// Gets or sets the size, in metres, of a radius around the geometry.
    /// </summary>
    /// <value>The radius, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     Deliberately unvalidated. A setter that refused a negative would oblige the loader to agree
    ///     with it, and §2.45 is this repository's record of what happens when a loader can produce a
    ///     value its own property rejects. There is no corpus data to justify inventing the policy.
    /// </remarks>
    public decimal? Radius { get; set; }

    /// <summary>
    /// Gets or sets the type of feature this entry describes.
    /// </summary>
    /// <value>The feature type, such as <c>city</c>, or an <i>empty</i> string if none was specified.</value>
    public string FeatureTypeTag
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets how this entry relates to the feature it names.
    /// </summary>
    /// <value>The relationship, such as <c>is-centered-at</c>, or an <i>empty</i> string if none was specified.</value>
    public string RelationshipTag
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the feature this entry describes.
    /// </summary>
    /// <value>The feature name, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>Free text written by the publisher — the corpus value is <c>Nederland</c>.</remarks>
    public string FeatureName
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the geometry was written inside a <c>georss:where</c> element.
    /// </summary>
    /// <value><b>true</b> if the geometry is wrapped; otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     The specification allows a geometry to be a direct child or to sit inside <c>georss:where</c>,
    ///     and the wrapper carries no information a consumer can act on. It is recorded anyway so that a
    ///     wrapped feed comes back out wrapped — dropping it would be a deliberate round-trip loss, which
    ///     is the whole subject of §2.47.
    /// </remarks>
    public bool GeometryIsWrappedInWhere { get; set; }

    /// <summary>
    /// Gets or sets which GeoRSS serialisation the geometry was written in.
    /// </summary>
    /// <value>A <see cref="GeoRssEncoding"/> value. The default is <see cref="GeoRssEncoding.Simple"/>.</value>
    /// <remarks>
    ///     Recorded so a document comes back out in the serialisation it arrived in. GML always sits
    ///     inside <c>georss:where</c>, so reading it necessarily sets
    ///     <see cref="GeometryIsWrappedInWhere"/> too.
    /// </remarks>
    public GeoRssEncoding Encoding { get; set; } = GeoRssEncoding.Simple;

    /// <summary>
    /// Gets a value indicating whether any geometry was specified.
    /// </summary>
    /// <value><b>true</b> if a point, line, polygon or box is present; otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     A line or polygon holding no positions does not count. It would write an element with nothing
    ///     in it — and, wrapped, an empty <c>georss:where</c> — which reloads as no extension at all, so
    ///     an object that claimed to have geometry would round-trip into one that has none.
    /// </remarks>
    public bool HasGeometry =>
        this.Point is not null
        || this.Box is not null
        || this.Line is { Positions.Count: > 0 }
        || this.Polygon is { Positions.Count: > 0 };

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this context.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve prefixed elements.</param>
    /// <returns><b>true</b> if the context was initialized using the supplied <paramref name="source"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <remarks>
    ///     <b>Returning <see langword="false"/> when nothing was read is load-bearing.</b> An extension is
    ///     probed by namespace <em>declaration</em>, so a feed that declares <c>georss</c> and never uses
    ///     it reaches this method; only the answer below stops a meaningless extension being attached to
    ///     it. <c>ReadFeedsThatDeclareMoreThanTheyUse</c> asserts exactly that, and predates this family.
    /// </remarks>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        // Load initialises this context from the supplied source, so anything a previous call left
        // behind has to go first. Without this the two latching members below -- which only ever move
        // to Gml and true -- would let a second document inherit the first document's serialisation,
        // and a document with no geometry at all would keep publishing the previous one's coordinates.
        this.Reset();

        if (!source.HasChildren)
        {
            return false;
        }

        bool wasLoaded = this.LoadGeometry(source, manager);
        wasLoaded |= this.LoadProperties(source, manager);

        return wasLoaded;
    }

    /// <summary>
    /// Returns every member to the state a newly constructed context has.
    /// </summary>
    private void Reset()
    {
        this.Point = null;
        this.Line = null;
        this.Polygon = null;
        this.Box = null;
        this.Elevation = null;
        this.Floor = null;
        this.Radius = null;
        this.FeatureTypeTag = string.Empty;
        this.RelationshipTag = string.Empty;
        this.FeatureName = string.Empty;
        this.GeometryIsWrappedInWhere = false;
        this.Encoding = GeoRssEncoding.Simple;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is a null reference or an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        // GML has nowhere to live but inside georss:where, so it forces the wrapper regardless of how
        // the geometry was originally written.
        bool wrap = this.GeometryIsWrappedInWhere || this.Encoding == GeoRssEncoding.Gml;

        // One geometry per wrapper. georss:where carries a single geometry, so a context holding both a
        // point and a box gets two where elements rather than one holding two siblings -- which would
        // round-trip through this library and fail anybody else's schema.
        foreach (Action<XmlWriter, string> write in this.GeometryWriters())
        {
            if (wrap)
            {
                writer.WriteStartElement("where", xmlNamespace);
                write(writer, xmlNamespace);
                writer.WriteEndElement();
            }
            else
            {
                write(writer, xmlNamespace);
            }
        }

        if (this.Elevation.HasValue)
        {
            writer.WriteElementString("elev", xmlNamespace, this.Elevation.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Floor.HasValue)
        {
            writer.WriteElementString("floor", xmlNamespace, this.Floor.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Radius.HasValue)
        {
            writer.WriteElementString("radius", xmlNamespace, this.Radius.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (!string.IsNullOrEmpty(this.FeatureTypeTag))
        {
            writer.WriteElementString("featuretypetag", xmlNamespace, this.FeatureTypeTag);
        }

        if (!string.IsNullOrEmpty(this.RelationshipTag))
        {
            writer.WriteElementString("relationshiptag", xmlNamespace, this.RelationshipTag);
        }

        if (!string.IsNullOrEmpty(this.FeatureName))
        {
            writer.WriteElementString("featurename", xmlNamespace, this.FeatureName);
        }
    }

    /// <summary>
    /// Returns one writer per geometry this context holds, in a stable order.
    /// </summary>
    /// <returns>A callback per present geometry, each writing exactly one element.</returns>
    /// <remarks>
    ///     Every callback takes the namespace it should qualify with, rather than reaching for the
    ///     family constant. <see cref="WriteTo"/> accepts a namespace argument and its callers pass the
    ///     extension's own, so honouring it for two of the four kinds and hardcoding the other two would
    ///     put sibling geometry elements in two different namespaces.
    /// </remarks>
    private IEnumerable<Action<XmlWriter, string>> GeometryWriters()
    {
        bool gml = this.Encoding == GeoRssEncoding.Gml;

        if (this.Point is { } point)
        {
            yield return gml
                ? (w, _) => WriteGmlPoint(w, point)
                : (w, ns) => w.WriteElementString("point", ns, point.ToString());
        }

        if (this.Line is { Positions.Count: > 0 } line)
        {
            yield return gml
                ? (w, _) => WriteGmlLine(w, line)
                : (w, ns) => line.WriteTo(w, ns);
        }

        if (this.Polygon is { Positions.Count: > 0 } polygon)
        {
            yield return gml
                ? (w, _) => WriteGmlPolygon(w, polygon)
                : (w, ns) => polygon.WriteTo(w, ns);
        }

        if (this.Box is { } box)
        {
            yield return gml
                ? (w, _) => WriteGmlEnvelope(w, box)
                : (w, ns) => w.WriteElementString("box", ns, box.ToString());
        }
    }

    /// <summary>
    /// Writes a point as GML.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="point">The point to write.</param>
    /// <remarks>
    ///     Each element names the <c>gml</c> prefix explicitly, so the writer declares the namespace once
    ///     at the outermost GML element and reuses it for the nested ones rather than inventing
    ///     <c>p1</c>, <c>p2</c> and so on. The declaration sits on the geometry rather than the document
    ///     root deliberately — a root declaration would have to be written by every GeoRSS document
    ///     whether or not it used GML, and would be invisible to the adapter's duplicate-prefix guard.
    /// </remarks>
    private static void WriteGmlPoint(XmlWriter writer, GeoRssPosition point)
    {
        const string Gml = GeoRssExtensionUtility.GmlNamespaceUri;

        writer.WriteStartElement("gml", "Point", Gml);
        writer.WriteElementString("gml", "pos", Gml, point.ToString());
        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes a line as GML.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="line">The line to write.</param>
    private static void WriteGmlLine(XmlWriter writer, GeoRssLine line)
    {
        const string Gml = GeoRssExtensionUtility.GmlNamespaceUri;

        writer.WriteStartElement("gml", "LineString", Gml);
        writer.WriteStartElement("gml", "posList", Gml);
        GeoRssExtensionUtility.WritePositions(writer, line.Positions);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes a polygon as GML.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="polygon">The polygon to write.</param>
    private static void WriteGmlPolygon(XmlWriter writer, GeoRssPolygon polygon)
    {
        const string Gml = GeoRssExtensionUtility.GmlNamespaceUri;

        writer.WriteStartElement("gml", "Polygon", Gml);
        writer.WriteStartElement("gml", "exterior", Gml);
        writer.WriteStartElement("gml", "LinearRing", Gml);
        writer.WriteStartElement("gml", "posList", Gml);
        GeoRssExtensionUtility.WritePositions(writer, polygon.Positions);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes a bounding box as a GML envelope.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="box">The box to write.</param>
    private static void WriteGmlEnvelope(XmlWriter writer, GeoRssBox box)
    {
        const string Gml = GeoRssExtensionUtility.GmlNamespaceUri;

        writer.WriteStartElement("gml", "Envelope", Gml);
        writer.WriteElementString("gml", "lowerCorner", Gml, box.LowerLeft.ToString());
        writer.WriteElementString("gml", "upperCorner", Gml, box.UpperRight.ToString());
        writer.WriteEndElement();
    }

    /// <summary>
    /// Reads the geometry, looking first at the direct children and then inside <c>georss:where</c>.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><b>true</b> if any geometry was read; otherwise, <b>false</b>.</returns>
    private bool LoadGeometry(XPathNavigator source, XmlNamespaceManager manager)
    {
        // Both passes run. An entry may carry a direct-child geometry AND a georss:where sibling -- the
        // four kinds are independent, so a box beside a wrapped point is a shape a feed can legitimately
        // take -- and returning after the first match would silently discard whichever came second, then
        // report the wrong Encoding for it.
        bool wasLoaded = this.LoadGeometryFrom(source, manager);

        XPathNavigator? whereNavigator = source.SelectChildElement("georss", "where", manager);
        if (whereNavigator is null)
        {
            return wasLoaded;
        }

        if (this.LoadGeometryFrom(whereNavigator, manager))
        {
            this.GeometryIsWrappedInWhere = true;
            wasLoaded = true;
        }

        if (this.LoadGmlGeometryFrom(whereNavigator, manager))
        {
            this.GeometryIsWrappedInWhere = true;
            this.Encoding = GeoRssEncoding.Gml;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Reads GML geometry from inside a <c>georss:where</c> element.
    /// </summary>
    /// <param name="source">The navigator positioned on the <c>georss:where</c> element.</param>
    /// <param name="manager">The namespace manager, which must have the <c>gml</c> prefix registered.</param>
    /// <returns><b>true</b> if any geometry was read; otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>
    ///     GML nests further than GeoRSS Simple does — a polygon's coordinates are four elements deep —
    ///     so each shape is walked explicitly rather than searched for. A <c>georss:where</c> holding
    ///     something this method does not recognise yields nothing and does not throw, which is what
    ///     lets an unknown future GML shape be ignored rather than half-read.
    ///     </para>
    ///     <para>
    ///     <b>Coordinate ordering here is an assumption, not a measurement.</b> The GeoRSS GML profile
    ///     is latitude-first like Simple, and that is what is implemented — but no GML geometry exists
    ///     in either surveyed corpus to check it against, and secondary sources disagree.
    ///     <c>GeoRssGmlTests</c> states the assumption explicitly so that if it is ever shown wrong,
    ///     exactly one test changes.
    ///     </para>
    /// </remarks>
    private bool LoadGmlGeometryFrom(XPathNavigator source, XmlNamespaceManager manager)
    {
        // SelectChildElement throws XPathException on a prefix the manager cannot resolve, and this
        // method is reachable from a public Load whose contract is to return a bool. A caller who built
        // their manager the way every other extension context in this library does -- binding only the
        // extension's own prefix -- would otherwise get an exception rather than an answer.
        if (manager.LookupNamespace("gml") is null)
        {
            return false;
        }

        bool wasLoaded = false;

        XPathNavigator? pointNavigator = source.SelectChildElement("gml", "Point", manager);
        XPathNavigator? positionNavigator = pointNavigator?.SelectChildElement("gml", "pos", manager);
        if (positionNavigator is not null
            && GeoRssExtensionUtility.TryReadPosition(positionNavigator.Value, out GeoRssPosition point))
        {
            this.Point = point;
            wasLoaded = true;
        }

        XPathNavigator? lineNavigator = source.SelectChildElement("gml", "LineString", manager);
        XPathNavigator? linePositions = lineNavigator?.SelectChildElement("gml", "posList", manager);
        if (linePositions is not null
            && GeoRssExtensionUtility.TryReadPositions(linePositions.Value, out List<GeoRssPosition>? line))
        {
            this.Line = new GeoRssLine(line);
            wasLoaded = true;
        }

        XPathNavigator? ringPositions = source.SelectChildElement("gml", "Polygon", manager)
            ?.SelectChildElement("gml", "exterior", manager)
            ?.SelectChildElement("gml", "LinearRing", manager)
            ?.SelectChildElement("gml", "posList", manager);
        if (ringPositions is not null
            && GeoRssExtensionUtility.TryReadPositions(ringPositions.Value, out List<GeoRssPosition>? ring))
        {
            this.Polygon = new GeoRssPolygon(ring);
            wasLoaded = true;
        }

        XPathNavigator? envelopeNavigator = source.SelectChildElement("gml", "Envelope", manager);
        if (envelopeNavigator is not null)
        {
            XPathNavigator? lowerNavigator = envelopeNavigator.SelectChildElement("gml", "lowerCorner", manager);
            XPathNavigator? upperNavigator = envelopeNavigator.SelectChildElement("gml", "upperCorner", manager);

            if (lowerNavigator is not null
                && upperNavigator is not null
                && GeoRssExtensionUtility.TryReadPosition(lowerNavigator.Value, out GeoRssPosition lower)
                && GeoRssExtensionUtility.TryReadPosition(upperNavigator.Value, out GeoRssPosition upper))
            {
                this.Box = new GeoRssBox(lower, upper);
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Reads whichever geometries are direct children of the supplied navigator.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><b>true</b> if any geometry was read; otherwise, <b>false</b>.</returns>
    private bool LoadGeometryFrom(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? pointNavigator = this.Point is null ? source.SelectChildElement("georss", "point", manager) : null;
        if (pointNavigator is not null && GeoRssExtensionUtility.TryReadPosition(pointNavigator.Value, out GeoRssPosition point))
        {
            this.Point = point;
            wasLoaded = true;
        }

        XPathNavigator? lineNavigator = this.Line is null ? source.SelectChildElement("georss", "line", manager) : null;
        if (lineNavigator is not null)
        {
            GeoRssLine line = new();
            if (line.Load(lineNavigator))
            {
                this.Line = line;
                wasLoaded = true;
            }
        }

        XPathNavigator? polygonNavigator = this.Polygon is null ? source.SelectChildElement("georss", "polygon", manager) : null;
        if (polygonNavigator is not null)
        {
            GeoRssPolygon polygon = new();
            if (polygon.Load(polygonNavigator))
            {
                this.Polygon = polygon;
                wasLoaded = true;
            }
        }

        XPathNavigator? boxNavigator = this.Box is null ? source.SelectChildElement("georss", "box", manager) : null;
        if (boxNavigator is not null && GeoRssExtensionUtility.TryReadBox(boxNavigator.Value, out GeoRssBox box))
        {
            this.Box = box;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Reads the non-geometric properties.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><b>true</b> if any property was read; otherwise, <b>false</b>.</returns>
    private bool LoadProperties(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? elevationNavigator = source.SelectChildElement("georss", "elev", manager);
        if (elevationNavigator is not null
            && decimal.TryParse(elevationNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal elevation))
        {
            this.Elevation = elevation;
            wasLoaded = true;
        }

        XPathNavigator? floorNavigator = source.SelectChildElement("georss", "floor", manager);
        if (floorNavigator is not null
            && int.TryParse(floorNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int floor))
        {
            this.Floor = floor;
            wasLoaded = true;
        }

        XPathNavigator? radiusNavigator = source.SelectChildElement("georss", "radius", manager);
        if (radiusNavigator is not null
            && decimal.TryParse(radiusNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal radius))
        {
            this.Radius = radius;
            wasLoaded = true;
        }

        XPathNavigator? featureTypeNavigator = source.SelectChildElement("georss", "featuretypetag", manager);
        if (featureTypeNavigator is not null && !string.IsNullOrWhiteSpace(featureTypeNavigator.Value))
        {
            this.FeatureTypeTag = featureTypeNavigator.Value;
            wasLoaded = true;
        }

        XPathNavigator? relationshipNavigator = source.SelectChildElement("georss", "relationshiptag", manager);
        if (relationshipNavigator is not null && !string.IsNullOrWhiteSpace(relationshipNavigator.Value))
        {
            this.RelationshipTag = relationshipNavigator.Value;
            wasLoaded = true;
        }

        XPathNavigator? featureNameNavigator = source.SelectChildElement("georss", "featurename", manager);
        if (featureNameNavigator is not null && !string.IsNullOrWhiteSpace(featureNameNavigator.Value))
        {
            this.FeatureName = featureNameNavigator.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }
}