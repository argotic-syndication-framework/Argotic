using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents an image that can be used as a representative image for a media object.
/// </summary>
/// <remarks>
///     Where a media object carries several and none sets <see cref="Time"/>, they are in order of importance,
///     so the first is the one to show. Where they do set <see cref="Time"/> they are keyframes of one video and
///     the order means something else entirely — a consumer that takes the first without checking will show a
///     frame from the opening second.
/// </remarks>
public class YahooMediaThumbnail : IComparable<YahooMediaThumbnail>, IEquatable<YahooMediaThumbnail>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaThumbnail"/> class.
    /// </summary>
    public YahooMediaThumbnail()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaThumbnail"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="url">A <see cref="Uri"/> that represents the URL of this thumbnail image.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public YahooMediaThumbnail(Uri url)
    {
        this.Url = url;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaThumbnail"/> class using the supplied <see cref="Uri"/>, height and width.
    /// </summary>
    /// <param name="url">A <see cref="Uri"/> that represents the URL of this thumbnail image.</param>
    /// <param name="height">The height of this thumbnail, typically in pixels.</param>
    /// <param name="width">The width of this thumbnail, typically in pixels.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public YahooMediaThumbnail(Uri url, int height, int width) : this(url)
    {
        this.Height = height;
        this.Width = width;
    }

    /// <summary>
    /// Gets or sets the height of this thumbnail.
    /// </summary>
    /// <value>The height of this thumbnail, typically in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates that no height was specified.</value>
    public int Height { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the time offset in relation to the media object.
    /// </summary>
    /// <value>
    ///     The offset into the media that this image is a frame of. The default value is
    ///     <see cref="TimeSpan.MinValue"/>, which indicates that no time offset was specified.
    /// </value>
    /// <remarks>
    ///     Set on each of several thumbnails, this turns them into keyframes of one video rather than a ranked
    ///     list of candidate images.
    /// </remarks>
    public TimeSpan Time { get; set; } = TimeSpan.MinValue;

    /// <summary>
    /// Gets or sets the location of this thumbnail image.
    /// </summary>
    /// <value>The image's URL. The default value is <see langword="null"/>, which a set operation cannot restore.</value>
    /// <remarks>
    ///     The one required attribute, and it is written unconditionally: a thumbnail with a
    ///     <see langword="null"/> <see cref="Url"/> saves as <c>url=""</c> rather than failing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Url
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the width of this thumbnail.
    /// </summary>
    /// <value>The width of this thumbnail, typically in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates that no width was specified.</value>
    public int Width { get; set; } = int.MinValue;

    /// <summary>
    /// Loads this <see cref="YahooMediaThumbnail"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaThumbnail"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaThumbnail"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string urlAttribute = source.GetAttribute("url", string.Empty);
            string heightAttribute = source.GetAttribute("height", string.Empty);
            string widthAttribute = source.GetAttribute("width", string.Empty);
            string timeAttribute = source.GetAttribute("time", string.Empty);

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(heightAttribute))
            {
                if (int.TryParse(heightAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int height))
                {
                    this.Height = height;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(widthAttribute))
            {
                if (int.TryParse(widthAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int width))
                {
                    this.Width = width;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(timeAttribute))
            {
                if (TimeSpan.TryParse(timeAttribute, out TimeSpan time))
                {
                    this.Time = time;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaThumbnail"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("thumbnail", extension.XmlNamespace);

        writer.WriteAttributeString("url", this.Url?.ToString() ?? string.Empty);

        if (this.Height != int.MinValue)
        {
            writer.WriteAttributeString("height", this.Height.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Width != int.MinValue)
        {
            writer.WriteAttributeString("width", this.Width.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Time != TimeSpan.MinValue)
        {
            writer.WriteAttributeString("time", this.Time.ToString());
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaThumbnail"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(YahooMediaThumbnail? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Height.CompareTo(other.Height);
        if (result == 0) result = this.Time.CompareTo(other.Time);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Width.CompareTo(other.Width);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaThumbnail"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaThumbnail"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaThumbnail"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaThumbnail? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaThumbnail other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Height), HashCodeUtility.Component(this.Time), HashCodeUtility.Component(this.Url), HashCodeUtility.Component(this.Width));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaThumbnail? first, YahooMediaThumbnail? second)
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
    public static bool operator !=(YahooMediaThumbnail? first, YahooMediaThumbnail? second) => !(first == second);
}