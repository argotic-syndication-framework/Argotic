using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a media object such as an audio, video, or executable file that can be associated with an <see cref="RssItem"/>.
/// </summary>
/// <seealso cref="RssItem.Enclosures"/>
/// <remarks>
///     <para>
///         All three attributes are required: "url says where the enclosure is located, length says how big
///         it is in bytes, and type says what its type is, a standard MIME type." The specification adds one
///         constraint that catches publishers out — "the url must be an http url". A relative path, or a
///         <c>file:</c> or <c>ftp:</c> URL, is not a conforming enclosure, and this class does not reject
///         one: <see cref="Url"/> accepts any <see cref="Uri"/>, including a relative one.
///     </para>
///     <para>
///         Support for the enclosure element in RSS software varies significantly because of disagreement over whether the specification permits more than one enclosure per item. 
///         Although the original author intended to permit no more than one enclosure in each item, this limit is not explicit in the specification. 
///         For best support in the widest number of aggregators, an item <i>should not</i> contain more than one enclosure.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rss\RssEnclosureExample.cs" language="cs" title="The following code example demonstrates the usage of the RssEnclosure class." />
/// </example>
public class RssEnclosure : IComparable<RssEnclosure>, IEquatable<RssEnclosure>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="RssEnclosure"/> class.
    /// </summary>
    public RssEnclosure()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssEnclosure"/> class using the supplied length, MIME type, and <see cref="Uri"/>.
    /// </summary>
    /// <param name="length">The size, in bytes, of the media object.</param>
    /// <param name="type">The media object's MIME content type.</param>
    /// <param name="url">A <see cref="Uri"/> that represents the URL of the media object.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="length"/> is less than <c>0</c>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public RssEnclosure(long length, string type, Uri url)
    {
        this.ContentType = type;
        this.Length = length;
        this.Url = url;
    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets the media object's MIME content type.
    /// </summary>
    /// <value>A registered MIME media type, such as <c>audio/mpeg</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">https://www.iana.org/assignments/media-types/media-types.xhtml</a> for a listing of the registered IANA MIME media types.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string ContentType
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the media object.
    /// </summary>
    /// <value>The size, in bytes, of the media object, or <see langword="null"/> if the enclosure did not state one.</value>
    /// <remarks>
    ///     <para>
    ///         The specification makes <c>length</c> required, but the size of some media objects cannot be
    ///         determined by the publisher, and the RSS Best Practices Profile directs those publishers to
    ///         write <c>length="0"</c> rather than omit the attribute. This property is nullable so that the
    ///         three cases stay distinguishable: a real size, an admitted <c>0</c>, and an attribute that was
    ///         never there. <see langword="null"/> is written back as no attribute at all, because
    ///         <c>length=""</c> is not a byte count.
    ///     </para>
    ///     <para>
    ///         The peer-to-peer file-sharing protocol BitTorrent deploys files using a small key file called a torrent that tells a client how to find and download the file. 
    ///         When an enclosure is delivered in a multistep process like the one used by BitTorrent, the length <i>should</i> be the size 
    ///         of the first file that must be downloaded to begin the process.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>0</c>.</exception>
    public long? Length
    {
        get;
        set
        {
            if (value.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.Value, 0);
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the URL of the media object.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the media object.</value>
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
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <see langword="true"/> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is <see langword="null"/>.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        foreach (ISyndicationExtension extension in this.Extensions)
        {
            if (match(extension))
            {
                return extension;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads this <see cref="RssEnclosure"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="RssEnclosure"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssEnclosure"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string lengthAttribute = source.GetAttribute("length", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string urlAttribute = source.GetAttribute("url", string.Empty);

            if (!string.IsNullOrEmpty(lengthAttribute))
            {
                if (long.TryParse(lengthAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out long length))
                {
                    if (length >= 0)
                    {
                        this.Length = length;
                    }
                    else
                    {
                        this.Length = 0;
                    }
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                this.ContentType = typeAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssEnclosure"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="RssEnclosure"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssEnclosure"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="RssEnclosure"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("enclosure");

        // Omitted rather than written empty. length="" is not a valid RSS 2.0 byte count, and an
        // enclosure that never stated a size should come out of a round-trip the way it went in.
        if (this.Length.HasValue)
        {
            writer.WriteAttributeString("length", this.Length.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        writer.WriteAttributeString("type", this.ContentType);
        writer.WriteAttributeString("url", this.Url?.ToString() ?? string.Empty);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RssEnclosure"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssEnclosure"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssEnclosure"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssEnclosure? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Nullable.Compare(this.Length, other.Length);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssEnclosure"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssEnclosure"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RssEnclosure"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(RssEnclosure? other)
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
    public override bool Equals(object? obj) => obj is RssEnclosure other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.ContentType ?? string.Empty),
            HashCodeUtility.Component(this.Length),
            HashCodeUtility.Component(this.Url));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(RssEnclosure? first, RssEnclosure? second)
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
    public static bool operator !=(RssEnclosure? first, RssEnclosure? second) => !(first == second);
}