using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents information that contains or links to the content of an <see cref="AtomEntry"/>.
/// </summary>
/// <remarks>
///     <para>
///         <b>The rules are ordered, and the first applicable one wins.</b> RFC 4287 §4.1.3.3 lists six, keyed on <see cref="ContentType"/>; reading them
///         out of order is how <c>application/xhtml+xml</c> ends up Base64-decoded. This class follows the same order on both load and save.
///         <list type="number">
///             <item>
///                 <description>
///                      If the value of the <see cref="ContentType"/> property is <c>text</c>, the value of the <see cref="Content"/> property must not contain child elements.
///                      Such text is intended to be presented to humans in a readable fashion. Thus, Atom Processors <i>may</i> collapse white space (including line breaks),
///                      and display the text using typographic techniques such as justification and proportional fonts.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                      If the value of the <see cref="ContentType"/> property is <c>html</c>, the value of the <see cref="Content"/> property must not contain child elements
///                      and <i>should</i> be suitable for handling as HTML. The HTML markup must be escaped. The HTML markup <i>should</i> be such that it could validly appear
///                      directly within an HTML <c>div</c> element. Atom Processors that display the content <i>may</i> use the markup to aid in displaying it.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                      If the value of the <see cref="ContentType"/> property is <c>xhtml</c>, the value of the <see cref="Content"/> property must be a single XHTML div element
///                      and <i>should</i> be suitable for handling as XHTML. The XHTML div element itself must not be considered part of the content. Atom Processors that display the
///                      content <i>may</i> use the markup to aid in displaying it. The escaped versions of characters represent those characters, not markup.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                      If the value is an <a href="https://www.rfc-editor.org/rfc/rfc3023.html">XML media type</a> (RFC 3023, now RFC 7303) or ends with <c>+xml</c> or <c>/xml</c> (case-insensitive),
///                     the content <i>may</i> include child elements and <i>should</i> be suitable for handling as the indicated media type.
///                     If the <see cref="AtomContent.Source"/> is not provided, this would normally mean that the <see cref="AtomContent.Content"/> would contain a
///                     single child element that would serve as the root element of the XML document of the indicated type.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                      If the value begins with <c>text/</c> (case-insensitive), the <see cref="AtomContent.Content"/> must not contain child elements.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     For all other values, the <see cref="AtomContent.Content"/> must be a valid Base64 encoding, as described in
///                     <a href="https://www.rfc-editor.org/rfc/rfc3548.html">RFC 3548: The Base16, Base32, and Base64 Data Encodings</a>, section 3 —
///                     the section RFC 4287 names. RFC 4648 obsoletes RFC 3548 and renumbers Base64 to <i>section 4</i>, so quoting "RFC 4648 section 3"
///                     points at the wrong clause. When decoded, the content <i>should</i> be suitable for handling as the indicated media type. The
///                     Base64 characters <i>may</i> be preceded and followed by white space, and lines are separated by a single newline (U+000A).
///                     This class stores and writes such content verbatim; it neither decodes nor validates it.
///                 </description>
///             </item>
///         </list>
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomContentExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomContent class." />
/// </example>
public class AtomContent : IAtomCommonObjectAttributes, IComparable<AtomContent>, IEquatable<AtomContent>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomContent"/> class.
    /// </summary>
    public AtomContent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomContent"/> class using the supplied textual content.
    /// </summary>
    /// <param name="content">The local content of the entry.</param>
    public AtomContent(string content)
    {
        this.Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomContent"/> class using the supplied content and entity encoding.
    /// </summary>
    /// <param name="content">The local content of the entry.</param>
    /// <param name="encoding">A value indicating the entity encoding of the content.</param>
    /// <remarks>
    ///     <para>
    ///         The Atom specification defines three initial values for the type of entry content:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                      <i>html</i>: The content must not contain child elements and <i>should</i> be suitable for handling as HTML.
    ///                      The HTML markup must be escaped, and <i>should</i> be such that it could validly appear directly within an HTML <c>div</c> element.
    ///                      Atom Processors that display the content <i>may</i> use the markup to aid in displaying it.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>text</i>: The content must not contain child elements. Such text is intended to be presented to humans in a readable fashion.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>xhtml</i>: The content must be a single XHTML div element and <i>should</i> be suitable for handling as XHTML.
    ///                      The XHTML div element itself must not be considered part of the content. Atom Processors that display the content
    ///                      <i>may</i> use the markup to aid in displaying it. The escaped versions of characters represent those characters, not markup.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public AtomContent(string content, string encoding) : this(content)
    {
        this.ContentType = encoding;
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
    /// Gets or sets the local content of this entry.
    /// </summary>
    /// <value>
    ///     The content, unescaped. For <c>xhtml</c> this is the inner markup of the wrapping <c>div</c>; for an XML media type it is the raw fragment.
    ///     Empty when <see cref="Source"/> names the content instead. The default value is an <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         Language-sensitive: the natural language of the value is whatever <see cref="Language"/> reports. What this string means depends entirely on
    ///         <see cref="ContentType"/> — see the ordered rules on <see cref="AtomContent"/>.
    ///     </para>
    ///     <para>
    ///         The setter trims, so leading and trailing whitespace does not survive a round trip. For rules 1 and 2 that is sanctioned — §3.1.1.1 lets a
    ///         processor collapse white space in text. <b>For rule 5 it is not:</b> a <see cref="ContentType"/> beginning <c>text/</c> is plain text of the
    ///         named media type, and RFC 4287 grants no collapse permission there, so a <c>text/plain</c> body whose leading indentation is significant
    ///         comes back without it. That is a knowing trade — the alternative leaks every pretty-printer's indentation into every value — and it is
    ///         pinned by <c>A10_ATextPlainContent_LosesItsSurroundingWhitespace</c>.
    ///     </para>
    ///     <para>
    ///         Rule 6 content — any other media type — is <b>Base64 that this class never decodes</b>. It is stored and written back verbatim, so what you
    ///         read here is the encoded text, not the bytes. To get the bytes, pass this value to
    ///         <see cref="SyndicationEncodingUtility.DecodeBase64String(string)"/>, which returns a seekable stream over them. Nothing validates the
    ///         encoding on load, so that call is also where a malformed body first announces itself, as a <see cref="FormatException"/>.
    ///     </para>
    /// </remarks>
    /// <seealso cref="SyndicationEncodingUtility.DecodeBase64String(string)"/>
    public string Content
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating the entity encoding of this content.
    /// </summary>
    /// <value>
    ///     The <c>type</c> attribute: one of the keywords <c>text</c>, <c>html</c>, <c>xhtml</c>, or a media type. The default value is an <i>empty</i>
    ///     string, which RFC 4287 §4.1.3.1 makes equivalent to <c>text</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         The Atom specification defines three initial values for the type of entry content:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                      <i>html</i>: The content must not contain child elements and <i>should</i> be suitable for handling as HTML.
    ///                      The HTML markup must be escaped, and <i>should</i> be such that it could validly appear directly within an HTML <c>div</c> element.
    ///                      Atom Processors that display the content <i>may</i> use the markup to aid in displaying it.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>text</i>: The content must not contain child elements. Such text is intended to be presented to humans in a readable fashion.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>xhtml</i>: The content must be a single XHTML div element and <i>should</i> be suitable for handling as XHTML.
    ///                      The XHTML div element itself must not be considered part of the content. Atom Processors that display the content
    ///                      <i>may</i> use the markup to aid in displaying it. The escaped versions of characters represent those characters, not markup.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         If the value is an <a href="https://www.rfc-editor.org/rfc/rfc3023.html">XML media type</a> (RFC 3023, now RFC 7303) or ends with <c>+xml</c> or <c>/xml</c> (case-insensitive),
    ///         the content <i>may</i> include child elements and <i>should</i> be suitable for handling as the indicated media type.
    ///         If the <see cref="AtomContent.Source"/> is not provided, this would normally mean that the <see cref="AtomContent.Content"/> would contain a
    ///         single child element that would serve as the root element of the XML document of the indicated type.
    ///     </para>
    ///     <para>
    ///         If the content type is not one of those specified above, it must conform to the syntax of a MIME media type, but must not be a composite type.
    ///         See <a href="https://www.rfc-editor.org/rfc/rfc4288.html">BCP 13 (RFC 4288, now RFC 6838)</a> for more details.
    ///     </para>
    ///     <para>
    ///         If the value begins with <c>text/</c> (case-insensitive), the <see cref="AtomContent.Content"/> must not contain child elements.
    ///     </para>
    ///     <para>
    ///         For all other values, the <see cref="AtomContent.Content"/> must be a valid Base64 encoding, as described in
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3548.html">RFC 3548: The Base16, Base32, and Base64 Data Encodings</a>, section 3 — the section
    ///         RFC 4287 names. RFC 4648 obsoletes RFC 3548 and renumbers Base64 to <i>section 4</i>. When decoded, the content <i>should</i> be suitable
    ///         for handling as the indicated media type. The Base64 characters <i>may</i> be preceded and followed by white space, and lines are
    ///         separated by a single newline (U+000A) character.
    ///     </para>
    ///     <para>
    ///         If neither the <see cref="AtomContent.ContentType"/> nor the <see cref="AtomContent.Source"/> is provided,
    ///         Atom Processors must behave as though this property has a value of <c>text</c>.
    ///     </para>
    /// </remarks>
    public string ContentType
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets an IRI that identifies the remote location of this content.
    /// </summary>
    /// <value>The <c>src</c> attribute as an IRI reference, or <see langword="null"/> when the content is carried in the element. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §4.1.3.2 makes the value an IRI reference (<a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a>) — a <i>reference</i>,
    ///         so unlike <see cref="AtomId"/> it may be relative and resolved against <see cref="BaseUri"/>. When it is present the element must be empty,
    ///         and §4.1.2 then requires the entry to carry an <c>atom:summary</c>, since there is no local text at all. Atom Processors <i>may</i> retrieve
    ///         the remote content, ignore it, or present it differently from local content. Nothing here dereferences it.
    ///     </para>
    ///     <para>
    ///         If a <see cref="AtomContent.Source"/> property is specified, the <see cref="AtomContent.ContentType"/> <i>should</i> be provided and must be a
    ///         <a href="https://www.rfc-editor.org/rfc/rfc4288.html">MIME media type</a> in the sense of BCP 13 (RFC 4288, now RFC 6838), rather than <c>text</c>, <c>html</c>, or <c>xhtml</c>. The value is advisory;
    ///         that is to say, when the corresponding URI (mapped from an IRI, if necessary) is dereferenced, if the server providing that content also provides
    ///         a media type, the server-provided media type is authoritative.
    ///     </para>
    ///     <para>See <a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
    ///     <para>See <see cref="Uri"/> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    public Uri? Source { get; set; }

    /// <summary>
    /// Loads this <see cref="AtomContent"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomContent"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomContent"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);

        if (AtomUtility.FillCommonObjectAttributes(this, source))
        {
            wasLoaded = true;
        }

        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string sourceAttribute = source.GetAttribute("src", string.Empty);

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                this.ContentType = typeAttribute;
                wasLoaded = true;
            }
            if (!string.IsNullOrEmpty(sourceAttribute))
            {
                if (Uri.TryCreate(sourceAttribute, UriKind.RelativeOrAbsolute, out Uri? src))
                {
                    this.Source = src;
                    wasLoaded = true;
                }
            }
        }

        if (string.Equals(this.ContentType, "xhtml", StringComparison.OrdinalIgnoreCase))
        {
            XPathNavigator? xhtmlDivNavigator = source.SelectChildElement("xhtml", "div", manager);
            if (xhtmlDivNavigator is not null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                this.Content = xhtmlDivNavigator.InnerXml;
                wasLoaded = true;
            }
        }
        else if (AtomUtility.IsXmlMediaType(this.ContentType) && !string.IsNullOrEmpty(source.InnerXml))
        {
            // Rule 5 of RFC 4287 s4.1.3.3: an XML media type MAY carry child elements, and Value
            // flattens them to their concatenated text - <data><value>42</value></data> became "42",
            // with the structure unrecoverable.
            this.Content = source.InnerXml;
            wasLoaded = true;
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            this.Content = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomContent"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomContent"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomContent"/>.
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
    /// Saves the current <see cref="AtomContent"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("content", AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        if (!string.IsNullOrEmpty(this.ContentType))
        {
            writer.WriteAttributeString("type", this.ContentType);
        }
        if (this.Source is not null)
        {
            writer.WriteAttributeString("src", this.Source.ToString());
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            if (string.Equals(this.ContentType, "xhtml", StringComparison.OrdinalIgnoreCase))
            {
                AtomUtility.WriteXhtmlDiv(writer, this.Content);
            }
            else if (AtomUtility.IsXmlMediaType(this.ContentType))
            {
                AtomUtility.WriteXmlFragment(writer, this.Content);
            }
            else
            {
                writer.WriteString(this.Content);
            }
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomContent"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomContent"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomContent"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomContent? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(this.ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = Uri.Compare(this.Source, other.Source, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomContent"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomContent"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomContent"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomContent? other)
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
    public override bool Equals(object? obj) => obj is AtomContent other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.ContentType), HashCodeUtility.Component(this.Source));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomContent? first, AtomContent? second)
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
    public static bool operator !=(AtomContent? first, AtomContent? second) => !(first == second);

}