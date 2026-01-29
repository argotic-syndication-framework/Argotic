using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Publishing;

/// <summary>
/// Represents a media range as defined in <a href="http://tools.ietf.org/html/rfc2616">RFC 2616: Hypertext Transfer Protocol</a> that
/// specifies a type of representation that can be added to a <see cref="AtomMemberResources"/>.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="AtomAcceptedMediaRange"/> class implements the <i>app:accept</i> element of the <a href="http://bitworking.org/projects/atom/rfc5023.html">Atom Publishing Protocol</a>.
///     </para>
///     <para>
///         The content value of the <see cref="MediaRange"/> property for an <see cref="AtomAcceptedMediaRange"/> is a media range as defined in <a href="http://tools.ietf.org/html/rfc2616">RFC 2616</a>.
///         The media range specifies a type of representation that can be added to a <see cref="AtomMemberResources">collection</see> via a POST operation.
///     </para>
///     <para>
///         The <see cref="AtomAcceptedMediaRange"/> is similar to the HTTP Accept request-header [<a href="http://tools.ietf.org/html/rfc2616">RFC 2616</a>].
///         Media type parameters are allowed within <see cref="AtomAcceptedMediaRange"/>, but <see cref="AtomAcceptedMediaRange"/> has no notion of preference e.g. <i>accept-params</i> or <i>q</i> arguments,
///         as specified in section 14.1 of <a href="http://tools.ietf.org/html/rfc2616">RFC 2616</a> are not significant.
///     </para>
///     <para>See <a href="http://www.iana.org/assignments/media-types">http://www.iana.org/assignments/media-types</a> for a listing of the registered IANA MIME media types and subtypes.</para>
/// </remarks>
/// <seealso cref="AtomMemberResources.Accepts"/>
/// <seealso cref="AtomMemberResources"/>
[Serializable]
public class AtomAcceptedMediaRange : IComparable<AtomAcceptedMediaRange>, IEquatable<AtomAcceptedMediaRange>, IExtensibleSyndicationObject, IAtomCommonObjectAttributes, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the base URI other than the base URI of the document or external entity.
    /// </summary>
    private Uri commonObjectBaseUri;
    /// <summary>
    /// Private member to hold the natural or formal language in which the content is written.
    /// </summary>
    private CultureInfo commonObjectLanguage;
    /// <summary>
    /// Private member to hold the value of the accepted media range.
    /// </summary>
    private string acceptedMediaRangeValue = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomAcceptedMediaRange"/> class.
    /// </summary>
    public AtomAcceptedMediaRange()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomAcceptedMediaRange"/> class using the specified media range.
    /// </summary>
    /// <param name="mediaRange">The value of the accepted media range.</param>
    public AtomAcceptedMediaRange(string mediaRange)
    {
        this.MediaRange = mediaRange;
    }

    /// <summary>
    /// Gets or sets the base URI other than the base URI of the document or external entity.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>,
    ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
    /// </remarks>
    public Uri BaseUri
    {
        get
        {
            return commonObjectBaseUri;
        }

        set
        {
            commonObjectBaseUri = value;
        }
    }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
    ///     </para>
    /// </remarks>
    public CultureInfo Language
    {
        get
        {
            return commonObjectLanguage;
        }

        set
        {
            commonObjectLanguage = value;
        }
    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets a <see cref="MediaRange"/> that indicates that <see cref="AtomEntry">Atom Entry Documents</see> can be added to a <see cref="AtomMemberResources"/>.
    /// </summary>
    /// <value>A <see cref="MediaRange"/> value that indicates that <see cref="AtomEntry">Atom Entry Documents</see> can be added to a <see cref="AtomMemberResources"/>.</value>
    public static string AtomEntryMediaRange
    {
        get
        {
            return "application/atom+xml;type=entry";
        }
    }

    /// <summary>
    /// Gets a <see cref="MediaRange"/> that indicates that <see cref="AtomFeed">Atom Feed Documents</see> can be added to a <see cref="AtomMemberResources"/>.
    /// </summary>
    /// <value>A <see cref="MediaRange"/> value that indicates that <see cref="AtomFeed">Atom Feed Documents</see> can be added to a <see cref="AtomMemberResources"/>.</value>
    public static string AtomFeedMediaRange
    {
        get
        {
            return "application/atom+xml;type=feed";
        }
    }

    /// <summary>
    /// Gets or sets the value of this accepted media range.
    /// </summary>
    /// <value>The value of this accepted media range.</value>
    /// <remarks>
    ///     <para>
    ///         See <a href="http://www.iana.org/assignments/media-types">http://www.iana.org/assignments/media-types</a> for a listing of the registered IANA MIME media types and subtypes.
    ///     </para>
    ///     <para>
    ///         The <see cref="AtomAcceptedMediaRange"/> is similar to the HTTP Accept request-header [<a href="http://tools.ietf.org/html/rfc2616">RFC 2616</a>].
    ///         Media type parameters are allowed within <see cref="AtomAcceptedMediaRange"/>, but <see cref="AtomAcceptedMediaRange"/> has no notion of preference e.g. <i>accept-params</i> or <i>q</i> arguments,
    ///         as specified in section 14.1 of [<a href="http://tools.ietf.org/html/rfc2616">RFC 2616</a>] are not significant.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AtomAcceptedMediaRange.AtomEntryMediaRange"/>
    public string MediaRange
    {
        get
        {
            return acceptedMediaRangeValue;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                acceptedMediaRangeValue = string.Empty;
            }
            else
            {
                acceptedMediaRangeValue = value.Trim();
            }
        }
    }

    /// <summary>
    /// Loads this <see cref="AtomAcceptedMediaRange"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="AtomAcceptedMediaRange"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomAcceptedMediaRange"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (AtomUtility.FillCommonObjectAttributes(this, source))
        {
        }

        this.MediaRange = !string.IsNullOrEmpty(source.Value) ? source.Value.Trim() : string.Empty;
        bool wasLoaded = true;
        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomAcceptedMediaRange"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="AtomAcceptedMediaRange"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomAcceptedMediaRange"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);

        bool wasLoaded = this.Load(source);

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="AtomAcceptedMediaRange"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("accept", AtomUtility.AtomPublishingNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        if (!string.IsNullOrEmpty(this.MediaRange))
        {
            writer.WriteString(this.MediaRange);
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomAcceptedMediaRange"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomAcceptedMediaRange"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

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
    public int CompareTo(AtomAcceptedMediaRange? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.MediaRange, other.MediaRange, StringComparison.OrdinalIgnoreCase);
        result |= AtomUtility.CompareCommonObjectAttributes(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomAcceptedMediaRange"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomAcceptedMediaRange"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="AtomAcceptedMediaRange"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(AtomAcceptedMediaRange? other)
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
        return obj is AtomAcceptedMediaRange other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.MediaRange, this.BaseUri, this.Language);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(AtomAcceptedMediaRange first, AtomAcceptedMediaRange second)
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
    public static bool operator !=(AtomAcceptedMediaRange first, AtomAcceptedMediaRange second)
    {
        return !(first == second);
    }
}