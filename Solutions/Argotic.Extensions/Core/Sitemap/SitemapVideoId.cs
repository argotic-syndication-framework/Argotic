using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a <c>video:id</c> element, naming a video in some external identifier system.
/// </summary>
/// <remarks>
///     The point of an identifier is to tie the same programme across sites that host it — a broadcaster's
///     page and an aggregator's page carrying one <c>tms:program</c> value are recognisably the same
///     thing. The systems the 1.1 schema names are Tribune Media Services, Rovi, Freebase and a plain URL;
///     <see cref="SitemapVideoIdType"/> enumerates them.
/// </remarks>
/// <seealso cref="SitemapVideo.Identifiers"/>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
public class SitemapVideoId : IComparable<SitemapVideoId>, IEquatable<SitemapVideoId>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the identifier value.
    /// </summary>
    private string identifierValue = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideoId"/> class.
    /// </summary>
    public SitemapVideoId()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideoId"/> class with the specified value and type.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <param name="type">The type of the identifier.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is an empty string.</exception>
    public SitemapVideoId(string value, SitemapVideoIdType type)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        this.identifierValue = value;
        this.Type = type;
    }

    /// <summary>
    /// Gets or sets the identifier value.
    /// </summary>
    /// <value>
    ///     The identifier, in whatever form <see cref="Type"/> implies. The default value is an
    ///     <i>empty</i> string.
    /// </value>
    /// <remarks>Stored verbatim — unlike most string properties here, it is not trimmed.</remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Value
    {
        get => identifierValue;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            identifierValue = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of the identifier.
    /// </summary>
    /// <value>
    ///     The identifier system. The default value is <see cref="SitemapVideoIdType.None"/>, which
    ///     suppresses the <c>type</c> attribute on write.
    /// </value>
    public SitemapVideoIdType Type { get; set; } = SitemapVideoIdType.None;

    /// <summary>
    /// Converts a <see cref="SitemapVideoIdType"/> enumeration value to its string representation.
    /// </summary>
    /// <param name="type">The <see cref="SitemapVideoIdType"/> to convert.</param>
    /// <returns>
    ///     The <c>type</c> attribute value — <c>tms:series</c>, <c>rovi:program</c> and so on — or an
    ///     <i>empty</i> string for <see cref="SitemapVideoIdType.None"/> and any unrecognised value.
    /// </returns>
    public static string TypeToString(SitemapVideoIdType type)
    {
        return type switch
        {
            SitemapVideoIdType.TmsSeries => "tms:series",
            SitemapVideoIdType.TmsProgram => "tms:program",
            SitemapVideoIdType.RoviSeries => "rovi:series",
            SitemapVideoIdType.RoviProgram => "rovi:program",
            SitemapVideoIdType.Freebase => "freebase",
            SitemapVideoIdType.Url => "url",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Converts a string representation to a <see cref="SitemapVideoIdType"/> enumeration value.
    /// </summary>
    /// <param name="value">The <c>type</c> attribute value to convert. Matching is case-insensitive.</param>
    /// <returns>
    ///     The matching <see cref="SitemapVideoIdType"/>, or <see cref="SitemapVideoIdType.None"/> if the
    ///     value is empty or names a system this library does not know. An unrecognised type is therefore
    ///     indistinguishable from an absent one, and is dropped rather than round-tripped.
    /// </returns>
    public static SitemapVideoIdType StringToType(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return SitemapVideoIdType.None;
        }

        return value.ToLowerInvariant() switch
        {
            "tms:series" => SitemapVideoIdType.TmsSeries,
            "tms:program" => SitemapVideoIdType.TmsProgram,
            "rovi:series" => SitemapVideoIdType.RoviSeries,
            "rovi:program" => SitemapVideoIdType.RoviProgram,
            "freebase" => SitemapVideoIdType.Freebase,
            "url" => SitemapVideoIdType.Url,
            _ => SitemapVideoIdType.None
        };
    }

    /// <summary>
    /// Initializes the video identifier using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapVideoId"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapVideoId"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.identifierValue = source.Value;
            wasLoaded = true;
        }

        string typeAttribute = source.GetAttribute("type", string.Empty);
        if (!string.IsNullOrEmpty(typeAttribute))
        {
            this.Type = StringToType(typeAttribute);
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the video identifier to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the video identifier will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("id", xmlNamespace);

        if (this.Type != SitemapVideoIdType.None)
        {
            writer.WriteAttributeString("type", TypeToString(this.Type));
        }

        if (!string.IsNullOrEmpty(this.Value))
        {
            writer.WriteString(this.Value);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapVideoId? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);
        if (result == 0)
        {
            result = this.Type.CompareTo(other.Type);
        }
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapVideoId"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapVideoId"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapVideoId"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapVideoId? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is SitemapVideoId other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Value), HashCodeUtility.Component(this.Type));

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideoId"/>.
    /// </summary>
    /// <returns>
    ///     The value prefixed by its type, as <c>tms:program:12345</c>, or just the value when
    ///     <see cref="Type"/> is <see cref="SitemapVideoIdType.None"/>. An <i>empty</i> string if no value
    ///     was set. This is a display form, not the sitemap's serialization.
    /// </returns>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(this.Value))
        {
            return string.Empty;
        }

        if (this.Type != SitemapVideoIdType.None)
        {
            return $"{TypeToString(this.Type)}:{this.Value}";
        }

        return this.Value;
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapVideoId? first, SitemapVideoId? second)
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
    public static bool operator !=(SitemapVideoId? first, SitemapVideoId? second) => !(first == second);
}