using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a <c>video:content_segment_loc</c> element — one media file of a video split across several.
/// </summary>
/// <remarks>
///     Segments describe a single video delivered in pieces, so their order is the playback order and the
///     durations are meant to sum to the whole. The list is a plain <see cref="IList{T}"/> that preserves
///     insertion order and enforces neither property.
/// </remarks>
/// <seealso cref="SitemapVideo.ContentSegments"/>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
public class SitemapVideoSegment : IComparable<SitemapVideoSegment>, IEquatable<SitemapVideoSegment>, IComparisonOperators
{
    /// <summary>
    /// The longest segment duration Google accepts, in seconds — eight hours.
    /// </summary>
    /// <remarks>
    ///     Advisory. <see cref="Duration"/> is an unvalidated property; nothing in this class consults this
    ///     constant.
    /// </remarks>
    /// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd"/>
    public const int MaxDuration = 28_800;

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
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
    public SitemapVideoSegment(Uri location, int? duration)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.segmentLocation = location;
        this.Duration = duration;
    }

    /// <summary>
    /// Gets or sets the URL of the video segment.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> pointing at the media file itself — not a player page — or
    ///     <see langword="null"/> if none was specified. Required by the specification.
    /// </value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Location
    {
        get => segmentLocation;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            segmentLocation = value;
        }
    }

    /// <summary>
    /// Gets or sets the duration of the video segment in seconds.
    /// </summary>
    /// <value>
    ///     Seconds, at most <see cref="MaxDuration"/>, or <see langword="null"/> if the <c>duration</c>
    ///     attribute was absent.
    /// </value>
    /// <remarks>The range is not enforced; any <see cref="int"/> assigned here is written out verbatim.</remarks>
    public int? Duration { get; set; }

    /// <summary>
    /// Initializes the video segment using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapVideoSegment"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapVideoSegment"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("content_segment_loc", xmlNamespace);

        if (this.Duration.HasValue)
        {
            writer.WriteAttributeString("duration", this.Duration.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (this.Location is not null)
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
    /// <returns><see langword="true"/> if the specified <see cref="SitemapVideoSegment"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is SitemapVideoSegment other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     Routed through <see cref="HashCodeUtility.Component(Uri)"/> rather than calling
    ///     <see cref="Uri.GetHashCode"/> directly. <see cref="CompareTo(SitemapVideoSegment)"/> compares
    ///     the location with <see cref="StringComparison.OrdinalIgnoreCase"/>, but <see cref="Uri.GetHashCode"/>
    ///     is case-sensitive over the path, so two segments differing only in path case compared equal
    ///     yet hashed differently.
    /// </remarks>
    public override int GetHashCode() => HashCodeUtility.Component(this.Location);

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideoSegment"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapVideoSegment"/>.</returns>
    public override string ToString() => this.Location?.ToString() ?? string.Empty;

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(SitemapVideoSegment? first, SitemapVideoSegment? second) => !(first == second);
}