using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a GeoRSS line: an ordered run of two or more positions.
/// </summary>
/// <remarks>
///     A class rather than a value type because it holds a variable-length, mutable collection. Its
///     sibling <see cref="GeoRssBox"/> is a <see langword="struct"/> precisely because a box is neither
///     of those things — that inconsistency is the value/object distinction, not an oversight.
/// </remarks>
/// <seealso cref="GeoRssSyndicationExtensionContext.Line"/>
public class GeoRssLine : IComparable<GeoRssLine>, IEquatable<GeoRssLine>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeoRssLine"/> class.
    /// </summary>
    public GeoRssLine()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoRssLine"/> class using the supplied positions.
    /// </summary>
    /// <param name="positions">The positions along the line.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="positions"/> is a null reference.</exception>
    public GeoRssLine(IEnumerable<GeoRssPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        foreach (GeoRssPosition position in positions)
        {
            this.Positions.Add(position);
        }
    }

    /// <summary>
    /// Gets the positions along this line, in order.
    /// </summary>
    /// <value>A collection of <see cref="GeoRssPosition"/> values. The default is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     The specification calls for two or more. That is not enforced: the collection is filled
    ///     element by element, so there is no setter to validate through, and an enforcing one would be
    ///     a rule the loader breaks on its own first <c>Add</c>.
    /// </remarks>
    public IList<GeoRssPosition> Positions { get; } = [];

    /// <summary>
    /// Loads this <see cref="GeoRssLine"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the whole coordinate list was read; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    /// Saves the current <see cref="GeoRssLine"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        GeoRssExtensionUtility.WritePositionElement(writer, "line", GeoRssExtensionUtility.NamespaceUri, (IReadOnlyCollection<GeoRssPosition>)this.Positions);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="GeoRssLine"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => GeoRssExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(GeoRssLine? other) =>
        other is null ? 1 : ComparisonUtility.CompareSequence(this.Positions, other.Positions);

    /// <summary>
    /// Determines whether the specified <see cref="GeoRssLine"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="GeoRssLine"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public bool Equals(GeoRssLine? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is GeoRssLine other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     <b>The positions are hashed by content, not handed to <c>HashCodeUtility.Component</c>.</b>
    ///     That helper returns its argument unchanged, which for a collection means hashing the
    ///     reference — and two lines carrying identical positions would then compare equal while hashing
    ///     differently, so a <see cref="HashSet{T}"/> would keep both.
    /// </remarks>
    public override int GetHashCode() => GeoRssExtensionUtility.HashPositions(this.Positions);

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(GeoRssLine? first, GeoRssLine? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(GeoRssLine? first, GeoRssLine? second) => !(first == second);
}