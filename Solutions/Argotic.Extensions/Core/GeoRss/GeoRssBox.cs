namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a geographic bounding box, as a pair of opposite corners.
/// </summary>
/// <remarks>
///     <para>
///     GeoRSS writes a box as four numbers — <c>&lt;georss:box&gt;23.822399163821153 -29.864984
///     80.442866836178837 40.447516&lt;/georss:box&gt;</c> — being the latitude and longitude of the
///     lower-left corner followed by those of the upper-right.
///     </para>
///     <para>
///     Modelled as two named corners rather than a list of positions so that "exactly two pairs" is
///     unrepresentable otherwise. A box with three corners cannot be constructed, which means no setter
///     has to reject one and no loader can produce one — the failure §2.45 records.
///     </para>
///     <para>
///     Like <see cref="GeoRssPosition"/> this is a <see langword="struct"/> and deliberately does not
///     implement <c>IComparisonOperators</c>; see that type's remarks for why.
///     </para>
/// </remarks>
/// <param name="LowerLeft">The lower-left corner.</param>
/// <param name="UpperRight">The upper-right corner.</param>
public readonly record struct GeoRssBox(GeoRssPosition LowerLeft, GeoRssPosition UpperRight) : IComparable<GeoRssBox>
{
    /// <summary>
    /// Gets a value indicating whether the corners are the right way round in latitude.
    /// </summary>
    /// <value><b>true</b> if <see cref="LowerLeft"/> is no further north than <see cref="UpperRight"/>; otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     <para>
    ///     Reported rather than enforced, and reported rather than repaired. A publisher who writes the
    ///     corners the wrong way round has still described a box, and swapping them here would report a
    ///     geometry the feed does not contain — while refusing to read it would lose the only location
    ///     that entry has. Consumers that care can ask.
    ///     </para>
    ///     <para>
    ///     <b>Latitude only, deliberately.</b> The obvious companion check — that the lower-left
    ///     longitude is no greater than the upper-right — is wrong at the antimeridian, where a box
    ///     spanning the Pacific is correctly encoded as lower-left 170 to upper-right −170. Longitude
    ///     wraps and latitude does not, so testing longitude the same way would report every
    ///     correctly-encoded Pacific-spanning box as malformed.
    ///     </para>
    /// </remarks>
    public bool IsWellOriented => this.LowerLeft.Latitude <= this.UpperRight.Latitude;

    /// <summary>
    /// Compares the current instance with another box.
    /// </summary>
    /// <param name="other">A box to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the boxes being compared.</returns>
    public int CompareTo(GeoRssBox other)
    {
        int result = this.LowerLeft.CompareTo(other.LowerLeft);

        return result == 0 ? this.UpperRight.CompareTo(other.UpperRight) : result;
    }

    /// <summary>
    /// Returns the wire representation of this box.
    /// </summary>
    /// <returns>The four coordinates, space separated, in the invariant culture.</returns>
    public override string ToString() => string.Concat(this.LowerLeft.ToString(), " ", this.UpperRight.ToString());

    /// <summary>
    /// Determines whether one box sorts before another.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> sorts before <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator <(GeoRssBox first, GeoRssBox second) => first.CompareTo(second) < 0;

    /// <summary>
    /// Determines whether one box sorts after another.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> sorts after <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator >(GeoRssBox first, GeoRssBox second) => first.CompareTo(second) > 0;

    /// <summary>
    /// Determines whether one box sorts before another, or equals it.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> does not sort after <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator <=(GeoRssBox first, GeoRssBox second) => first.CompareTo(second) <= 0;

    /// <summary>
    /// Determines whether one box sorts after another, or equals it.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if <paramref name="first"/> does not sort before <paramref name="second"/>; otherwise, <b>false</b>.</returns>
    public static bool operator >=(GeoRssBox first, GeoRssBox second) => first.CompareTo(second) >= 0;
}