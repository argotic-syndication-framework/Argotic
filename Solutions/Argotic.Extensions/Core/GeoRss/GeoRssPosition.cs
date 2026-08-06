using System.Globalization;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a single geographic position, in WGS84 decimal degrees.
/// </summary>
/// <remarks>
///     <para>
///     Every GeoRSS geometry is a sequence of these. <b>Latitude comes first</b>, both in the wire
///     format and in this type's constructor — <c>&lt;georss:point&gt;36.981334686279
///     -121.45983123779&lt;/georss:point&gt;</c> is a place in California, and the same two numbers the
///     other way round are not a place at all.
///     </para>
///     <para>
///     A <see langword="struct"/> rather than a class, because a position that is <see langword="null"/>
///     has no meaning in the format and a thousand-vertex polygon should not be a thousand allocations.
///     Absence is expressed by the nullable geometry property that would have held it.
///     </para>
///     <para>
///     <b>It deliberately does not implement <c>IComparisonOperators</c>.</b> That marker exists only to
///     opt a type into <c>ComparisonOperatorExtensions</c>, whose extension block is declared
///     <c>where T : class</c> — a struct claiming the marker would advertise operators it never
///     receives. The four relational operators are therefore declared here instead, and equality comes
///     from the record.
///     </para>
///     <para>
///     <see cref="decimal"/> rather than <see cref="double"/> for three reasons: scale survives a
///     round-trip, so <c>-71.920</c> comes back as <c>-71.920</c>; the sibling geographic extension
///     already uses <see cref="decimal"/> and the two should not disagree about what a coordinate is;
///     and <see cref="decimal"/> cannot represent <c>NaN</c> or <c>Infinity</c>, so a feed containing
///     either is rejected by the parse rather than by a check somebody has to remember to write. No
///     arithmetic is performed anywhere in this family, so nothing pays for the slower type.
///     </para>
/// </remarks>
/// <param name="Latitude">The latitude, in decimal degrees.</param>
/// <param name="Longitude">The longitude, in decimal degrees.</param>
public readonly record struct GeoRssPosition(decimal Latitude, decimal Longitude) : IComparable<GeoRssPosition>
{
    /// <summary>
    /// Compares the current instance with another position.
    /// </summary>
    /// <param name="other">A position to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the positions being compared.</returns>
    /// <remarks>
    ///     Ordered by latitude and then longitude. The ordering carries no geographic meaning — it exists
    ///     so that sequences of positions can be compared element by element, which is what a line and a
    ///     polygon need.
    /// </remarks>
    public int CompareTo(GeoRssPosition other)
    {
        int result = this.Latitude.CompareTo(other.Latitude);

        return result == 0 ? this.Longitude.CompareTo(other.Longitude) : result;
    }

    /// <summary>
    /// Returns the wire representation of this position.
    /// </summary>
    /// <returns>The latitude and longitude, space separated, in the invariant culture.</returns>
    /// <remarks>
    ///     A position has no element of its own, so its wire form is the closest thing it has to an XML
    ///     representation — and it is what the enclosing geometry writes.
    /// </remarks>
    public override string ToString() => string.Concat(
        this.Latitude.ToString(NumberFormatInfo.InvariantInfo),
        " ",
        this.Longitude.ToString(NumberFormatInfo.InvariantInfo));

    /// <summary>
    /// Determines whether one position sorts before another.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> sorts before <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator <(GeoRssPosition first, GeoRssPosition second) => first.CompareTo(second) < 0;

    /// <summary>
    /// Determines whether one position sorts after another.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> sorts after <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator >(GeoRssPosition first, GeoRssPosition second) => first.CompareTo(second) > 0;

    /// <summary>
    /// Determines whether one position sorts before another, or equals it.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> does not sort after <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator <=(GeoRssPosition first, GeoRssPosition second) => first.CompareTo(second) <= 0;

    /// <summary>
    /// Determines whether one position sorts after another, or equals it.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> does not sort before <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator >=(GeoRssPosition first, GeoRssPosition second) => first.CompareTo(second) >= 0;
}