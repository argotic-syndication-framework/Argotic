using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a GeoRSS polygon: a closed ring of positions.
/// </summary>
/// <remarks>
///     Structurally the same as <see cref="GeoRssLine"/>, and kept as its own type anyway so that the
///     word <i>polygon</i> exists in the type system and so that <see cref="IsClosed"/> — which is
///     meaningless on a line — has somewhere to live.
/// </remarks>
/// <seealso cref="GeoRssSyndicationExtensionContext.Polygon"/>
public class GeoRssPolygon : IComparable<GeoRssPolygon>, IEquatable<GeoRssPolygon>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeoRssPolygon"/> class.
    /// </summary>
    public GeoRssPolygon()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoRssPolygon"/> class using the supplied positions.
    /// </summary>
    /// <param name="positions">The positions around the ring.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="positions"/> is <see langword="null"/>.</exception>
    public GeoRssPolygon(IEnumerable<GeoRssPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        foreach (GeoRssPosition position in positions)
        {
            this.Positions.Add(position);
        }
    }

    /// <summary>
    /// Gets the positions around this ring, in order.
    /// </summary>
    /// <value>A collection of <see cref="GeoRssPosition"/> values. The default is an <i>empty</i> collection.</value>
    public IList<GeoRssPosition> Positions { get; } = [];

    /// <summary>
    /// Gets a value indicating whether this ring is closed.
    /// </summary>
    /// <value><see langword="true"/> if there are at least four positions and the last repeats the first; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     <b>Reported, not enforced, and never repaired.</b> The specification says a polygon's first
    ///     position must be repeated as its last, and real publishers omit it. Closing the ring
    ///     automatically would report a geometry the feed does not contain; refusing to read it would
    ///     lose a usable one. Saying so is the third option, and it is the only one that keeps both the
    ///     data and the truth about it.
    /// </remarks>
    public bool IsClosed =>
        this.Positions.Count >= 4 && this.Positions[0] == this.Positions[^1];

    /// <summary>
    /// Loads this <see cref="GeoRssPolygon"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the whole coordinate list was read; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!GeoRssExtensionUtility.TryReadPositions(source.Value, out List<GeoRssPosition>? positions))
        {
            return false;
        }

        this.Positions.Clear();
        foreach (GeoRssPosition position in positions)
        {
            this.Positions.Add(position);
        }

        return true;
    }

    /// <summary>
    /// Saves the current <see cref="GeoRssPolygon"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer) => this.WriteTo(writer, GeoRssExtensionUtility.NamespaceUri);

    /// <summary>
    /// Saves the current <see cref="GeoRssPolygon"/> to the specified <see cref="XmlWriter"/>, in the supplied namespace.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="xmlNamespace">The XML namespace to qualify the element with.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is <see langword="null"/> or an empty string.</exception>
    /// <remarks>
    ///     The context passes the namespace its own caller supplied, so that every geometry it writes
    ///     lands in one namespace rather than two.
    /// </remarks>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        GeoRssExtensionUtility.WritePositionElement(writer, "polygon", xmlNamespace, (IReadOnlyCollection<GeoRssPosition>)this.Positions);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="GeoRssPolygon"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => GeoRssExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(GeoRssPolygon? other) =>
        other is null ? 1 : ComparisonUtility.CompareSequence(this.Positions, other.Positions);

    /// <summary>
    /// Determines whether the specified <see cref="GeoRssPolygon"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="GeoRssPolygon"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(GeoRssPolygon? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is GeoRssPolygon other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>See <see cref="GeoRssLine.GetHashCode"/> for why the positions are hashed by content.</remarks>
    public override int GetHashCode() => GeoRssExtensionUtility.HashPositions(this.Positions);

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(GeoRssPolygon? first, GeoRssPolygon? second)
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
    public static bool operator !=(GeoRssPolygon? first, GeoRssPolygon? second) => !(first == second);
}