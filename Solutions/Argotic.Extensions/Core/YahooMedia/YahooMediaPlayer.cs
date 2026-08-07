using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a means of allowing the media object to be accessed through a web browser media player console.
/// </summary>
/// <remarks>
///     Required only when the <see cref="YahooMediaContent"/> has no <see cref="YahooMediaContent.Url"/> of its
///     own — it is how a publisher offers media that can be watched but not fetched. <see cref="Url"/> is the
///     one required attribute, and is written unconditionally: a player with a <see langword="null"/>
///     <see cref="Url"/> saves as <c>url=""</c> rather than failing.
/// </remarks>
public class YahooMediaPlayer : IComparable<YahooMediaPlayer>, IEquatable<YahooMediaPlayer>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaPlayer"/> class.
    /// </summary>
    public YahooMediaPlayer()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaPlayer"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="url">A <see cref="Uri"/> that represents the URL of this player console.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public YahooMediaPlayer(Uri url)
    {
        this.Url = url;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaPlayer"/> class using the supplied <see cref="Uri"/>, height and width.
    /// </summary>
    /// <param name="url">A <see cref="Uri"/> that represents the URL of this player console.</param>
    /// <param name="height">The height of the browser window that this player console should be opened in.</param>
    /// <param name="width">The width of the browser window that this player console should be opened in.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public YahooMediaPlayer(Uri url, int height, int width) : this(url)
    {
        this.Height = height;
        this.Width = width;
    }

    /// <summary>
    /// Gets or sets the height of the browser window that this player console should be opened in.
    /// </summary>
    /// <value>The height, in pixels, of the browser window to open the console in. The default value is <see cref="Int32.MinValue"/>, which indicates that no height was specified.</value>
    public int Height { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the location of this player console.
    /// </summary>
    /// <value>The console's URL. The default value is <see langword="null"/>, which a set operation cannot restore.</value>
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
    /// Gets or sets the width of the browser window that this player console should be opened in.
    /// </summary>
    /// <value>The width, in pixels, of the browser window to open the console in. The default value is <see cref="Int32.MinValue"/>, which indicates that no width was specified.</value>
    public int Width { get; set; } = int.MinValue;

    /// <summary>
    /// Loads this <see cref="YahooMediaPlayer"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaPlayer"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaPlayer"/>.
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
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaPlayer"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("player", extension.XmlNamespace);

        writer.WriteAttributeString("url", this.Url?.ToString() ?? string.Empty);

        if (this.Height != int.MinValue)
        {
            writer.WriteAttributeString("height", this.Height.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Width != int.MinValue)
        {
            writer.WriteAttributeString("width", this.Width.ToString(NumberFormatInfo.InvariantInfo));
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaPlayer"/>.
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
    public int CompareTo(YahooMediaPlayer? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Height.CompareTo(other.Height);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Width.CompareTo(other.Width);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaPlayer"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaPlayer"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaPlayer"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaPlayer? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaPlayer other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Height), HashCodeUtility.Component(this.Url), HashCodeUtility.Component(this.Width));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaPlayer? first, YahooMediaPlayer? second)
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
    public static bool operator !=(YahooMediaPlayer? first, YahooMediaPlayer? second) => !(first == second);
}