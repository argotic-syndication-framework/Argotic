using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the permissible audience for a media object.
/// </summary>
public class YahooMediaRating : IComparable<YahooMediaRating>, IEquatable<YahooMediaRating>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaRating"/> class.
    /// </summary>
    public YahooMediaRating()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaRating"/> class using the supplied audience.
    /// </summary>
    /// <param name="audience">A textual value that represents the permissible audience(s) for this media object.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="audience"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="audience"/> is an empty string.</exception>
    public YahooMediaRating(string audience)
    {
        this.Content = audience;
    }

    /// <summary>
    /// Gets the media simple rating scheme adult content rating.
    /// </summary>
    /// <value>The media simple rating scheme <b>adult</b> content rating value.</value>
    public static string SimpleAdultRating => "adult";

    /// <summary>
    /// Gets the media simple rating scheme non-adult content rating.
    /// </summary>
    /// <value>The media simple rating scheme <b>nonadult</b> content rating value.</value>
    public static string SimpleNonAdultRating => "nonadult";

    /// <summary>
    /// Gets the media simple rating scheme.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the media simple rating scheme. The simple scheme has a value of <b>urn:simple</b>.</value>
    public static Uri SimpleScheme => new("urn:simple");

    /// <summary>
    /// Gets or sets the permissible audience for this media object.
    /// </summary>
    /// <value>A textual value that represents the permissible audience(s) for this media object.</value>
    /// <seealso cref="SimpleAdultRating"/>
    /// <seealso cref="SimpleNonAdultRating"/>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Content
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets a URI that identifies this rating scheme.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents this rating scheme. The default value is <b>null</b>.</value>
    /// <remarks>
    ///     If no rating scheme is provided, the default scheme is <b>urn:simple</b>.
    /// </remarks>
    /// <seealso cref="SimpleScheme"/>
    public Uri? Scheme { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaRating"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaRating"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaRating"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string schemeAttribute = source.GetAttribute("scheme", string.Empty);
            if (!string.IsNullOrEmpty(schemeAttribute))
            {
                if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri? scheme))
                {
                    this.Scheme = scheme;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Content = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaRating"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("rating", extension.XmlNamespace);

        if (this.Scheme is not null)
        {
            writer.WriteAttributeString("scheme", this.Scheme.ToString());
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            writer.WriteString(this.Content);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaRating"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaRating"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    public int CompareTo(YahooMediaRating? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Scheme, other.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaRating"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaRating"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="YahooMediaRating"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(YahooMediaRating? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaRating other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.Scheme));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(YahooMediaRating? first, YahooMediaRating? second)
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
    public static bool operator !=(YahooMediaRating? first, YahooMediaRating? second) => !(first == second);
}