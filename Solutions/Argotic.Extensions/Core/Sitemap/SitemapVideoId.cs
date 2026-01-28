using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a video identifier element (video:id) in a Google Video Sitemap.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapVideoId"/> class represents a video identifier that can be included in a sitemap
///         to help search engines identify videos using external identifier systems such as TMS, Rovi, Freebase, or URL.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
[Serializable]
public class SitemapVideoId : IComparable
{
    /// <summary>
    /// Private member to hold the identifier value.
    /// </summary>
    private string identifierValue = string.Empty;

    /// <summary>
    /// Private member to hold the identifier type.
    /// </summary>
    private SitemapVideoIdType identifierType = SitemapVideoIdType.None;

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
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
    public SitemapVideoId(string value, SitemapVideoIdType type)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        this.identifierValue = value;
        this.identifierType = type;
    }

    /// <summary>
    /// Gets or sets the identifier value.
    /// </summary>
    /// <value>A <see cref="string"/> that represents the identifier value.</value>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
    public string Value
    {
        get
        {
            return identifierValue;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            identifierValue = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of the identifier.
    /// </summary>
    /// <value>A <see cref="SitemapVideoIdType"/> that represents the type of the identifier.</value>
    public SitemapVideoIdType Type
    {
        get
        {
            return identifierType;
        }

        set
        {
            identifierType = value;
        }
    }

    /// <summary>
    /// Converts a <see cref="SitemapVideoIdType"/> enumeration value to its string representation.
    /// </summary>
    /// <param name="type">The <see cref="SitemapVideoIdType"/> to convert.</param>
    /// <returns>The string representation of the identifier type.</returns>
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
    /// <param name="value">The string value to convert.</param>
    /// <returns>The <see cref="SitemapVideoIdType"/> corresponding to the string value.</returns>
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
    /// <returns><b>true</b> if the <see cref="SitemapVideoId"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
            this.identifierType = StringToType(typeAttribute);
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the video identifier to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the video identifier will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference or empty string.</exception>
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
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }

        SitemapVideoId other = obj as SitemapVideoId;

        if (other != null)
        {
            int result = string.Compare(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);
            if (result == 0)
            {
                result = this.Type.CompareTo(other.Type);
            }
            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not SitemapVideoId)
        {
            return false;
        }

        return this.CompareTo(obj) == 0;
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Value, this.Type);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideoId"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapVideoId"/>.</returns>
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
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapVideoId first, SitemapVideoId second)
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
    public static bool operator !=(SitemapVideoId first, SitemapVideoId second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapVideoId first, SitemapVideoId second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(SitemapVideoId first, SitemapVideoId second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(SitemapVideoId first, SitemapVideoId second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(SitemapVideoId first, SitemapVideoId second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}