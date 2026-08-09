using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing geographic location.
/// </summary>
/// <remarks>
///     <para>
///     The <see cref="GeoRssSyndicationExtension"/> implements <b>GeoRSS</b>, standardised by the Open
///     Geospatial Consortium as OGC 17-002r1, which can be found at
///     <a href="https://docs.ogc.org/cs/17-002r1/17-002r1.html">https://docs.ogc.org/cs/17-002r1/17-002r1.html</a>.
///     </para>
///     <para>
///     It is distinct from <see cref="BasicGeocodingSyndicationExtension"/>, which implements the older
///     and unrelated W3C Basic Geo vocabulary (<c>geo:lat</c> / <c>geo:long</c>). The two are not
///     alternatives so much as successive generations: the canonical live source of geographic feeds,
///     the USGS earthquake service, publishes <c>georss:</c> and emits <b>zero</b> <c>geo:</c> elements.
///     A feed may of course carry both, and this library will attach both extensions when it does.
///     </para>
///     <para>
///     <b>Coordinates are latitude first.</b> <c>&lt;georss:point&gt;36.981334686279
///     -121.45983123779&lt;/georss:point&gt;</c> is a place in California; the same two numbers read the
///     other way round are not a place at all. Getting this backwards is the classic GeoRSS defect and
///     it is silent, because for most of the populated world both orderings are numerically legal.
///     </para>
/// </remarks>
public class GeoRssSyndicationExtension : SyndicationExtension, IComparable<GeoRssSyndicationExtension>, IEquatable<GeoRssSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// The XML namespace GeoRSS elements are qualified with.
    /// </summary>
    public const string NamespaceUri = GeoRssExtensionUtility.NamespaceUri;

    /// <summary>
    /// The XML namespace the GML geometries inside <c>georss:where</c> are qualified with.
    /// </summary>
    public const string GmlNamespaceUri = GeoRssExtensionUtility.GmlNamespaceUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoRssSyndicationExtension"/> class.
    /// </summary>
    public GeoRssSyndicationExtension()
        : base("georss", NamespaceUri, new Version("1.0"), new Uri("https://docs.ogc.org/cs/17-002r1/17-002r1.html"), "GeoRSS", "Extends syndication feeds to provide a means of describing the geographic location of published content.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="GeoRssSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="GeoRssSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public GeoRssSyndicationExtensionContext Context
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is GeoRssSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="GeoRssSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the extension was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        bool wasLoaded = this.Context.Load(navigator, this.CreateGeoRssNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="GeoRssSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the extension was initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Creates a namespace manager that resolves both the GeoRSS and the GML prefixes.
    /// </summary>
    /// <param name="navigator">The navigator whose name table and in-scope namespaces are used.</param>
    /// <returns>A namespace manager able to resolve <c>georss</c> and <c>gml</c>.</returns>
    /// <remarks>
    ///     <para>
    ///     <see cref="SyndicationExtension.CreateNamespaceManager"/> binds one prefix — this extension's
    ///     own — and <c>SelectChildElement</c> <b>throws</b> on a prefix it cannot resolve, so reading
    ///     GML at all requires binding <c>gml</c> here first.
    ///     </para>
    ///     <para>
    ///     <b>It is bound to the namespace GeoRSS specifies, never to whatever the document happens to
    ///     declare.</b> Selection matches on the resolved namespace <i>URI</i>, so a feed spelling this
    ///     namespace with some other prefix still matches — but a feed using a different GML
    ///     <i>namespace</i> is a different format and must not be read as though it were this one.
    ///     </para>
    ///     <para>
    ///     That distinction is load bearing. Preferring the document's binding made
    ///     <c>http://www.opengis.net/gml/3.2</c> resolve through this parser, and real GML 3.2 feeds —
    ///     NASA's EONET among them — write longitude before latitude. Every one of its 7,030 geometries
    ///     was read transposed, and 3,762 of them produced a latitude outside ±90. Binding the constant
    ///     is what makes "a different namespace does not match" true rather than merely intended.
    ///     </para>
    /// </remarks>
    private XmlNamespaceManager CreateGeoRssNamespaceManager(XPathNavigator navigator)
    {
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);
        manager.AddNamespace("gml", GmlNamespaceUri);

        return manager;
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="GeoRssSyndicationExtension"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => GeoRssExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <remarks>
    ///     <b>Every member of <see cref="GeoRssSyndicationExtensionContext"/> must appear below, in
    ///     alphabetical order.</b> §2.48 records what happens when a member is added to a context and not
    ///     to its comparison: two extensions describing different things report themselves equal.
    ///     <c>GeoRssComparisonCoversEveryMemberTests</c> holds one row per member and fails on the row it
    ///     is missing.
    /// </remarks>
    public int CompareTo(GeoRssSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Nullable.Compare(this.Context.Box, other.Context.Box);
        if (result == 0) result = Nullable.Compare(this.Context.Elevation, other.Context.Elevation);
        if (result == 0) result = this.Context.Encoding.CompareTo(other.Context.Encoding);
        if (result == 0) result = string.Compare(this.Context.FeatureName, other.Context.FeatureName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.FeatureTypeTag, other.Context.FeatureTypeTag, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Nullable.Compare(this.Context.Floor, other.Context.Floor);
        if (result == 0) result = this.Context.GeometryIsWrappedInWhere.CompareTo(other.Context.GeometryIsWrappedInWhere);
        if (result == 0) result = Comparer<GeoRssLine>.Default.Compare(this.Context.Line, other.Context.Line);
        if (result == 0) result = Nullable.Compare(this.Context.Point, other.Context.Point);
        if (result == 0) result = Comparer<GeoRssPolygon>.Default.Compare(this.Context.Polygon, other.Context.Polygon);
        if (result == 0) result = Nullable.Compare(this.Context.Radius, other.Context.Radius);
        if (result == 0) result = string.Compare(this.Context.RelationshipTag, other.Context.RelationshipTag, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="GeoRssSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="GeoRssSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(GeoRssSyndicationExtension? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is GeoRssSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Context.Box));
        hash.Add(HashCodeUtility.Component(this.Context.Elevation));
        hash.Add(HashCodeUtility.Component(this.Context.Encoding));
        hash.Add(HashCodeUtility.Component(this.Context.FeatureName));
        hash.Add(HashCodeUtility.Component(this.Context.FeatureTypeTag));
        hash.Add(HashCodeUtility.Component(this.Context.Floor));
        hash.Add(HashCodeUtility.Component(this.Context.GeometryIsWrappedInWhere));
        hash.Add(HashCodeUtility.Component(this.Context.Line));
        hash.Add(HashCodeUtility.Component(this.Context.Point));
        hash.Add(HashCodeUtility.Component(this.Context.Polygon));
        hash.Add(HashCodeUtility.Component(this.Context.Radius));
        hash.Add(HashCodeUtility.Component(this.Context.RelationshipTag));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(GeoRssSyndicationExtension? first, GeoRssSyndicationExtension? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(GeoRssSyndicationExtension? first, GeoRssSyndicationExtension? second) => !(first == second);
}