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
    public bool HasGeometry =>
        this.Point is not null || this.Line is not null || this.Polygon is not null || this.Box is not null;

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

        if (!source.HasChildren)
        {
            return false;
        }

        bool wasLoaded = this.LoadGeometry(source, manager);
        wasLoaded |= this.LoadProperties(source, manager);

        return wasLoaded;
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
        if (this.HasGeometry && (this.GeometryIsWrappedInWhere || this.Encoding == GeoRssEncoding.Gml))
        {
            writer.WriteStartElement("where", xmlNamespace);

            if (this.Encoding == GeoRssEncoding.Gml)
            {
                this.WriteGmlGeometry(writer);
            }
            else
            {
                this.WriteGeometry(writer, xmlNamespace);
            }

            writer.WriteEndElement();
        }
        else
        {
            this.WriteGeometry(writer, xmlNamespace);
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
    /// Writes whichever geometries are present.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="xmlNamespace">The namespace to qualify the elements with.</param>
    private void WriteGeometry(XmlWriter writer, string xmlNamespace)
    {
        if (this.Point is { } point)
        {
            writer.WriteElementString("point", xmlNamespace, point.ToString());
        }

        this.Line?.WriteTo(writer);
        this.Polygon?.WriteTo(writer);

        if (this.Box is { } box)
        {
            writer.WriteElementString("box", xmlNamespace, box.ToString());
        }
    }

    /// <summary>
    /// Writes whichever geometries are present, as GML.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <remarks>
    ///     Each element is written with an explicit <c>gml</c> prefix. The prefix is already declared on
    ///     the document root — <see cref="GeoRssSyndicationExtension.WriteXmlNamespaceDeclaration"/>
    ///     declares it alongside <c>georss</c> — so naming it here reuses that declaration instead of
    ///     letting the writer invent <c>p1</c>, <c>p2</c> and so on per element.
    /// </remarks>
    private void WriteGmlGeometry(XmlWriter writer)
    {
        const string Gml = GeoRssExtensionUtility.GmlNamespaceUri;

        if (this.Point is { } point)
        {
            writer.WriteStartElement("gml", "Point", Gml);
            writer.WriteElementString("gml", "pos", Gml, point.ToString());
            writer.WriteEndElement();
        }

        if (this.Line is { Positions.Count: > 0 } line)
        {
            writer.WriteStartElement("gml", "LineString", Gml);
            writer.WriteStartElement("gml", "posList", Gml);
            GeoRssExtensionUtility.WritePositions(writer, line.Positions);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        if (this.Polygon is { Positions.Count: > 0 } polygon)
        {
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

        if (this.Box is { } box)
        {
            writer.WriteStartElement("gml", "Envelope", Gml);
            writer.WriteElementString("gml", "lowerCorner", Gml, box.LowerLeft.ToString());
            writer.WriteElementString("gml", "upperCorner", Gml, box.UpperRight.ToString());
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Reads the geometry, looking first at the direct children and then inside <c>georss:where</c>.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><b>true</b> if any geometry was read; otherwise, <b>false</b>.</returns>
    private bool LoadGeometry(XPathNavigator source, XmlNamespaceManager manager)
    {
        if (this.LoadGeometryFrom(source, manager))
        {
            return true;
        }

        XPathNavigator? whereNavigator = source.SelectChildElement("georss", "where", manager);
        if (whereNavigator is null)
        {
            return false;
        }

        if (this.LoadGeometryFrom(whereNavigator, manager))
        {
            this.GeometryIsWrappedInWhere = true;
            return true;
        }

        if (this.LoadGmlGeometryFrom(whereNavigator, manager))
        {
            this.GeometryIsWrappedInWhere = true;
            this.Encoding = GeoRssEncoding.Gml;
            return true;
        }

        return false;
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

        XPathNavigator? pointNavigator = source.SelectChildElement("georss", "point", manager);
        if (pointNavigator is not null && GeoRssExtensionUtility.TryReadPosition(pointNavigator.Value, out GeoRssPosition point))
        {
            this.Point = point;
            wasLoaded = true;
        }

        XPathNavigator? lineNavigator = source.SelectChildElement("georss", "line", manager);
        if (lineNavigator is not null)
        {
            GeoRssLine line = new();
            if (line.Load(lineNavigator))
            {
                this.Line = line;
                wasLoaded = true;
            }
        }

        XPathNavigator? polygonNavigator = source.SelectChildElement("georss", "polygon", manager);
        if (polygonNavigator is not null)
        {
            GeoRssPolygon polygon = new();
            if (polygon.Load(polygonNavigator))
            {
                this.Polygon = polygon;
                wasLoaded = true;
            }
        }

        XPathNavigator? boxNavigator = source.SelectChildElement("georss", "box", manager);
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
        if (featureTypeNavigator is not null && !string.IsNullOrEmpty(featureTypeNavigator.Value))
        {
            this.FeatureTypeTag = featureTypeNavigator.Value;
            wasLoaded = true;
        }

        XPathNavigator? relationshipNavigator = source.SelectChildElement("georss", "relationshiptag", manager);
        if (relationshipNavigator is not null && !string.IsNullOrEmpty(relationshipNavigator.Value))
        {
            this.RelationshipTag = relationshipNavigator.Value;
            wasLoaded = true;
        }

        XPathNavigator? featureNameNavigator = source.SelectChildElement("georss", "featurename", manager);
        if (featureNameNavigator is not null && !string.IsNullOrEmpty(featureNameNavigator.Value))
        {
            this.FeatureName = featureNameNavigator.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }
}