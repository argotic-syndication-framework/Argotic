using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a content segment location element in a Google Video Sitemap.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapVideoSegment"/> class represents a video segment that can be included
///         in a video sitemap to describe a portion of a video. Each segment has a URL pointing to the
///         segment content and an optional duration attribute.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
[Serializable]
public class SitemapVideoSegment : IComparable<SitemapVideoSegment>, IEquatable<SitemapVideoSegment>, IComparisonOperators
{
    /// <summary>
    /// The maximum allowed duration in seconds for a video segment (8 hours).
    /// </summary>
    /// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd"/>
    public const int MaxDuration = 28800;

    /// <summary>
    /// Private member to hold the URL of the video segment.
    /// </summary>
    private Uri? segmentLocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideoSegment"/> class.
    /// </summary>
    public SitemapVideoSegment()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideoSegment"/> class with the specified location.
    /// </summary>
    /// <param name="location">The URL of the video segment.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapVideoSegment(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.segmentLocation = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideoSegment"/> class with the specified location and duration.
    /// </summary>
    /// <param name="location">The URL of the video segment.</param>
    /// <param name="duration">The duration of the video segment in seconds.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapVideoSegment(Uri location, int? duration)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.segmentLocation = location;
        this.Duration = duration;
    }

    /// <summary>
    /// Gets or sets the URL of the video segment.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the video segment. This is a required property.</value>
    /// <remarks>
    ///     The URL must point to the actual video segment content file.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri? Location
    {
        get
        {
            return segmentLocation;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            segmentLocation = value;
        }
    }

    /// <summary>
    /// Gets or sets the duration of the video segment in seconds.
    /// </summary>
    /// <value>The duration of the video segment in seconds. Optional.</value>
    /// <remarks>
    ///     The duration should be between 1 and 28800 seconds (8 hours).
    /// </remarks>
    public int? Duration { get; set; }

    /// <summary>
    /// Initializes the video segment using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapVideoSegment"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapVideoSegment"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        // Load the URL from the element value
        if (!string.IsNullOrEmpty(source.Value))
        {
            if (Uri.TryCreate(source.Value, UriKind.RelativeOrAbsolute, out Uri? location))
            {
                this.segmentLocation = location;
                wasLoaded = true;
            }
        }

        // Load the optional duration attribute
        string durationAttr = source.GetAttribute("duration", string.Empty);
        if (!string.IsNullOrEmpty(durationAttr) && int.TryParse(durationAttr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int duration))
        {
            this.Duration = duration;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the video segment to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the video segment will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference or empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("content_segment_loc", xmlNamespace);

        if (this.Duration.HasValue)
        {
            writer.WriteAttributeString("duration", this.Duration.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (this.Location != null)
        {
            writer.WriteString(this.Location.ToString());
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapVideoSegment? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Uri.Compare(this.Location, other.Location, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapVideoSegment"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapVideoSegment"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapVideoSegment"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SitemapVideoSegment? other)
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SitemapVideoSegment other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return this.Location?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideoSegment"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapVideoSegment"/>.</returns>
    public override string ToString()
    {
        return this.Location?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapVideoSegment? first, SitemapVideoSegment? second)
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
    public static bool operator !=(SitemapVideoSegment? first, SitemapVideoSegment? second)
    {
        return !(first == second);
    }
}