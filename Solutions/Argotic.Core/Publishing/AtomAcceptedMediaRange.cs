using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Publishing;

/// <summary>
/// Represents a media range as defined in <a href="https://www.rfc-editor.org/rfc/rfc2616.html">RFC 2616: Hypertext Transfer Protocol</a> that
/// specifies a type of representation that can be added to a <see cref="AtomMemberResources"/>.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="AtomAcceptedMediaRange"/> class implements the <i>app:accept</i> element of the <a href="https://www.rfc-editor.org/rfc/rfc5023.html">Atom Publishing Protocol</a>.
///     </para>
///     <para>
///         <see cref="MediaRange"/> holds a media range, and RFC 5023 §8.3.4 pins its grammar to section 14.1 of
///         <a href="https://www.rfc-editor.org/rfc/rfc2616.html">RFC 2616</a> — the citation the protocol makes, so that is the one kept here. RFC 2616
///         has since been split into three documents; the corresponding clause is <b>§12.5.1 of RFC 9110</b>, not §14.1 of it. The range names a type of
///         representation that may be added to a <see cref="AtomMemberResources">collection</see> by POST.
///     </para>
///     <para>
///         It is <i>similar</i> to an HTTP <c>Accept</c> request-header, and the difference is the trap: media type parameters are allowed, but this
///         element has no notion of preference. The <i>accept-params</i> and <i>q</i> arguments RFC 2616 §14.1 permits are not significant here, so a
///         server writing <c>image/png;q=0.8</c> expresses nothing a client is entitled to act on.
///     </para>
///     <para>See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">https://www.iana.org/assignments/media-types/media-types.xhtml</a> for a listing of the registered IANA MIME media types and subtypes.</para>
/// </remarks>
/// <seealso cref="AtomMemberResources.Accepts"/>
/// <seealso cref="AtomMemberResources"/>
public class AtomAcceptedMediaRange : IComparable<AtomAcceptedMediaRange>, IEquatable<AtomAcceptedMediaRange>, IExtensibleSyndicationObject, IAtomCommonObjectAttributes, IComparisonOperators
{
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
    /// Gets or sets the base against which relative references inside this element are resolved.
    /// </summary>
    /// <value>The <c>xml:base</c> in effect for this element, or <see langword="null"/> when none is. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §2 gives <c>xml:base</c> the function described in section 5.1.1 of
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986: Uniform Resource Identifier (URI): Generic Syntax</a> — it establishes the base URI,
    ///         or IRI, for every relative reference in the attribute's effective scope. The value itself is a URI reference after processing according to
    ///         <a href="https://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.
    ///     </para>
    ///     <para>
    ///         Loading resolves inheritance: an element without an <c>xml:base</c> of its own reports the nearest ancestor's, so the value here is the
    ///         <i>effective</i> base a consumer can resolve an href against, not the literal attribute.
    ///     </para>
    /// </remarks>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>The language declared by <c>xml:lang</c>, or <see langword="null"/> when none is in scope. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 defines <c>atomLanguageTag</c> as a language identifier per
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066 (BCP 47; now RFC 5646)</a>, or its successor. A tag this runtime cannot turn
    ///         into a <see cref="CultureInfo"/> is traced and dropped rather than failing the load.
    ///     </para>
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Extensions"/> holds at least one <see cref="ISyndicationExtension"/>; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the media range meaning "<see cref="AtomEntry">Atom Entry Documents</see> may be added to this collection".
    /// </summary>
    /// <value><c>application/atom+xml;type=entry</c>.</value>
    /// <remarks>
    ///     RFC 5023 §8.3.4 makes this the assumed range when a collection carries no <c>app:accept</c> at all. Its <i>presence</i> is therefore not what
    ///     distinguishes an entry collection; its <i>absence</i> alongside some other range is.
    /// </remarks>
    public static string AtomEntryMediaRange => "application/atom+xml;type=entry";

    /// <summary>
    /// Gets the media range meaning "<see cref="AtomFeed">Atom Feed Documents</see> may be added to this collection".
    /// </summary>
    /// <value><c>application/atom+xml;type=feed</c>.</value>
    public static string AtomFeedMediaRange => "application/atom+xml;type=feed";

    /// <summary>
    /// Gets or sets the value of this accepted media range.
    /// </summary>
    /// <value>A media range such as <c>image/*</c> or <c>application/atom+xml;type=entry</c>. The default value is an <i>empty</i> string, which means the collection accepts nothing.</value>
    /// <remarks>
    ///     <para>
    ///         See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">https://www.iana.org/assignments/media-types/media-types.xhtml</a> for a listing of the registered IANA MIME media types and subtypes.
    ///     </para>
    ///     <para>
    ///         Media type parameters are allowed, but this element has no notion of preference: the <i>accept-params</i> and <i>q</i> arguments of an HTTP
    ///         <c>Accept</c> header (RFC 2616 §14.1, now <a href="https://www.rfc-editor.org/rfc/rfc9110.html">RFC 9110</a> §12.5.1) are not significant.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AtomAcceptedMediaRange.AtomEntryMediaRange"/>
    public string MediaRange
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="AtomAcceptedMediaRange"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomAcceptedMediaRange"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomAcceptedMediaRange"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
    /// <returns><see langword="true"/> if the <see cref="AtomAcceptedMediaRange"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomAcceptedMediaRange"/>.
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
    /// Saves the current <see cref="AtomAcceptedMediaRange"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
    public int CompareTo(AtomAcceptedMediaRange? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.MediaRange, other.MediaRange, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = AtomUtility.CompareCommonObjectAttributes(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomAcceptedMediaRange"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomAcceptedMediaRange"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomAcceptedMediaRange"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is AtomAcceptedMediaRange other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.MediaRange), HashCodeUtility.Component(this.BaseUri), HashCodeUtility.Component(this.Language));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomAcceptedMediaRange? first, AtomAcceptedMediaRange? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(AtomAcceptedMediaRange? first, AtomAcceptedMediaRange? second) => !(first == second);
}