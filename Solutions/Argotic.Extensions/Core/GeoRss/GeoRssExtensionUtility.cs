using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace Argotic.Extensions.Core;

/// <summary>
/// Helpers shared by the GeoRSS element types.
/// </summary>
/// <remarks>
///     Every GeoRSS geometry is the same thing on the wire — a whitespace-separated run of decimal
///     numbers read as latitude/longitude pairs — and differs only in how many pairs are allowed. Doing
///     that in one place keeps the difference between a point and a polygon visible as the difference
///     between a point and a polygon.
/// </remarks>
internal static class GeoRssExtensionUtility
{
    /// <summary>
    /// The XML namespace GeoRSS elements are qualified with.
    /// </summary>
    public const string NamespaceUri = "http://www.georss.org/georss";

    /// <summary>
    /// The XML namespace the GML geometries inside <c>georss:where</c> are qualified with.
    /// </summary>
    public const string GmlNamespaceUri = "http://www.opengis.net/gml";

    /// <summary>
    /// Reads a GeoRSS coordinate list into positions.
    /// </summary>
    /// <param name="value">The element's node value.</param>
    /// <param name="positions">The positions read, or <see langword="null"/> if the value could not be read.</param>
    /// <returns><see langword="true"/> if the whole value was read as complete latitude/longitude pairs; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     <b>All or nothing.</b> A single unparseable token, or a trailing coordinate with no partner,
    ///     fails the whole element. Reading the good prefix and discarding the rest would produce a
    ///     geometry that is not the one the feed describes — a silently truncated line is <i>wrong</i>,
    ///     not partial — and inventing a partner for an odd coordinate is worse.
    ///     </para>
    ///     <para>
    ///     Tokenising is a span walk rather than <see cref="string.Split(char[])"/> because the
    ///     separator is <em>any</em> whitespace, not a space: a polygon written one pair per indented
    ///     line is the ordinary real-world shape, and splitting that on <c>' '</c> yields empty entries
    ///     and tokens with newlines still attached. It also avoids allocating two strings per vertex.
    ///     </para>
    ///     <para>
    ///     <see cref="NumberStyles.Float"/> permits a leading sign, a decimal point and an exponent, and
    ///     — the part that matters — <b>excludes</b> thousands separators, so a comma-separated pair such
    ///     as <c>45.256,-71.92</c> is rejected rather than half-read. That is deliberate: accepting
    ///     commas would make <c>45,256 -71,92</c> genuinely ambiguous between a coordinate pair and a
    ///     comma-decimal locale.
    ///     </para>
    /// </remarks>
    public static bool TryReadPositions(string? value, [NotNullWhen(true)] out List<GeoRssPosition>? positions)
    {
        positions = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        List<decimal> numbers = [];
        ReadOnlySpan<char> remaining = value.AsSpan();
        int index = 0;

        while (index < remaining.Length)
        {
            while (index < remaining.Length && char.IsWhiteSpace(remaining[index]))
            {
                index++;
            }

            if (index >= remaining.Length)
            {
                break;
            }

            int start = index;
            while (index < remaining.Length && !char.IsWhiteSpace(remaining[index]))
            {
                index++;
            }

            if (!decimal.TryParse(remaining[start..index], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal number))
            {
                return false;
            }

            numbers.Add(number);
        }

        if (numbers.Count == 0 || numbers.Count % 2 != 0)
        {
            return false;
        }

        positions = new List<GeoRssPosition>(numbers.Count / 2);
        for (int i = 0; i < numbers.Count; i += 2)
        {
            positions.Add(new GeoRssPosition(numbers[i], numbers[i + 1]));
        }

        return true;
    }

    /// <summary>
    /// Reads a GeoRSS coordinate list expected to hold exactly one position.
    /// </summary>
    /// <param name="value">The element's node value.</param>
    /// <param name="position">The position read, if there was exactly one.</param>
    /// <returns><see langword="true"/> if the value held exactly one complete pair; otherwise, <see langword="false"/>.</returns>
    public static bool TryReadPosition(string? value, out GeoRssPosition position)
    {
        position = default;

        if (!TryReadPositions(value, out List<GeoRssPosition>? positions) || positions.Count != 1)
        {
            return false;
        }

        position = positions[0];

        return true;
    }

    /// <summary>
    /// Reads a GeoRSS coordinate list expected to hold exactly two positions, as a bounding box.
    /// </summary>
    /// <param name="value">The element's node value.</param>
    /// <param name="box">The box read, if there were exactly two pairs.</param>
    /// <returns><see langword="true"/> if the value held exactly two complete pairs; otherwise, <see langword="false"/>.</returns>
    public static bool TryReadBox(string? value, out GeoRssBox box)
    {
        box = default;

        if (!TryReadPositions(value, out List<GeoRssPosition>? positions) || positions.Count != 2)
        {
            return false;
        }

        box = new GeoRssBox(positions[0], positions[1]);

        return true;
    }

    /// <summary>
    /// Writes a sequence of positions as a GeoRSS coordinate list.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="positions">The positions to write.</param>
    public static void WritePositions(XmlWriter writer, IEnumerable<GeoRssPosition> positions)
    {
        bool first = true;

        foreach (GeoRssPosition position in positions)
        {
            if (!first)
            {
                writer.WriteString(" ");
            }

            writer.WriteString(position.ToString());
            first = false;
        }
    }

    /// <summary>
    /// Returns a hash code over the contents of a position sequence.
    /// </summary>
    /// <param name="positions">The positions to hash.</param>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     <c>HashCodeUtility.Component&lt;T&gt;</c> returns its argument unchanged, which is right for a
    ///     scalar and wrong for a collection: it would hash the <em>reference</em>, so two geometries
    ///     carrying identical positions would compare equal and hash differently. Since every
    ///     comparison in this family is element-by-element, so is every hash.
    /// </remarks>
    public static int HashPositions(IEnumerable<GeoRssPosition> positions)
    {
        HashCode hash = new();

        foreach (GeoRssPosition position in positions)
        {
            hash.Add(position);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Renders an element to its XML representation.
    /// </summary>
    /// <param name="writeTo">The element's own write callback.</param>
    /// <returns>The XML representation of the element.</returns>
    public static string ToXmlString(Action<XmlWriter> writeTo)
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = Argotic.Common.SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            writeTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Writes an element carrying a coordinate list, when the sequence is not empty.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="localName">The local name of the element.</param>
    /// <param name="xmlNamespace">The namespace to qualify the element with.</param>
    /// <param name="positions">The positions to write.</param>
    public static void WritePositionElement(XmlWriter writer, string localName, string xmlNamespace, IReadOnlyCollection<GeoRssPosition> positions)
    {
        if (positions.Count == 0)
        {
            return;
        }

        writer.WriteStartElement(localName, xmlNamespace);
        WritePositions(writer, positions);
        writer.WriteEndElement();
    }
}