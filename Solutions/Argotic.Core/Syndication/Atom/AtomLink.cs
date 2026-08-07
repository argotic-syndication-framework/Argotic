using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a reference from an <see cref="AtomEntry"/> or <see cref="AtomFeed"/> to a Web resource.
/// </summary>
/// <seealso cref="AtomEntry.Links"/>
/// <seealso cref="AtomFeed.Links"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomLinkExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomLink class." />
/// </example>
public class AtomLink : IAtomCommonObjectAttributes, IComparable<AtomLink>, IEquatable<AtomLink>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomLink"/> class.
    /// </summary>
    public AtomLink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomLink"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents an IRI that identifies the location of this Web resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    public AtomLink(Uri href)
    {
        this.Uri = href;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomLink"/> class using the supplied <see cref="Uri"/> and link relation type.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents an IRI that identifies the location of this Web resource.</param>
    /// <param name="relation">A value that indicates the link relation type.</param>
    /// <remarks>
    ///     <para>
    ///         The Atom specification defines five initial values for the Registry of Link Relations:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                      <i>alternate</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property 
    ///                      identifies an alternate version of the resource described by the containing element.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>related</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property 
    ///                      identifies a resource related to the resource described by the containing element.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>self</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property 
    ///                      identifies a resource equivalent to the containing element.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>enclosure</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property identifies 
    ///                      a related resource that is potentially large and might require special handling.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>via</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property identifies 
    ///                      a resource that is the source of the information provided in the containing element.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    public AtomLink(Uri href, string relation) : this(href)
    {
        this.Relation = relation;
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
    /// Gets or sets the natural or formal language in which this Web resource content is written.
    /// </summary>
    /// <value>The <c>hreflang</c> attribute, or <see langword="null"/> when it is absent. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         This is the language of the <i>linked</i> resource, and it is advisory — distinct from <see cref="Language"/>, which is the <c>xml:lang</c>
    ///         governing the link element itself. RFC 4287 §4.2.7.4 pins the tag to
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066 (BCP 47; now RFC 5646)</a>. A tag this runtime cannot turn into a
    ///         <see cref="CultureInfo"/> is traced and dropped, so an exotic-but-legal BCP 47 tag can be silently lost on a round trip.
    ///     </para>
    /// </remarks>
    public CultureInfo? ContentLanguage { get; set; }

    /// <summary>
    /// Gets or sets an advisory media type for this Web resource.
    /// </summary>
    /// <value>The <c>type</c> attribute, such as <c>application/atom+xml</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     A hint only: it does not override the media type the server actually returns, and a client that trusts it over the response header will
    ///     eventually be wrong. RFC 4287 §4.2.7.3 requires the syntax of a MIME media type as specified by
    ///     <a href="https://www.rfc-editor.org/rfc/rfc4288.html">BCP 13 (RFC 4288, now RFC 6838)</a>; nothing here checks that.
    /// </remarks>
    public string ContentType
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets an advisory length for this Web resource content in octets.
    /// </summary>
    /// <value>The <c>length</c> attribute in octets. The default value is <see cref="Int64.MinValue"/>, which means no length was specified and none is written on save.</value>
    /// <remarks>
    ///     Advisory, like <see cref="ContentType"/>: it does not override the content length the underlying protocol reports. Note the sentinel — the
    ///     "absent" value is <see cref="Int64.MinValue"/>, not <c>0</c>, because <c>0</c> is a legal advertised length. Test for the sentinel, not for
    ///     falsiness.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>0</c>.</exception>
    public long Length
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
            field = value;
        }
    } = long.MinValue;

    /// <summary>
    /// Gets or sets a value that indicates the link relation type of this Web resource.
    /// </summary>
    /// <value>The <c>rel</c> attribute exactly as written. The default value is an <i>empty</i> string, meaning the attribute was absent — see <see cref="EffectiveRelation"/> for the interpreted value.</value>
    /// <remarks>
    ///     <para>
    ///         This property is the attribute, not its meaning. RFC 4287 §4.2.7.2 makes an absent <c>rel</c> mean <c>alternate</c>, but the emptiness is
    ///         kept here so that saving does not invent an attribute the publisher never wrote. Read <see cref="EffectiveRelation"/> when you want the
    ///         relation a processor should act on.
    ///     </para>
    ///     <para>
    ///         When present the value must be non-empty and match either the <i>isegment-nz-nc</i> or
    ///         the <i>IRI</i> production in <a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987: Internationalized Resource Identifiers (IRIs)</a>.
    ///         Note that use of a relative reference other than a simple name is not allowed. If a name is given, implementations must consider the link relation type equivalent 
    ///         to the same name registered within the IANA Registry of Link Relations (<a href="https://www.rfc-editor.org/rfc/rfc4287.html">Section 7</a>), 
    ///         and thus to the IRI that would be obtained by appending the value of the rel attribute to the string "<i>http://www.iana.org/assignments/relation/</i>". 
    ///         The value of <see cref="Relation"/> property describes the meaning of the link, but does not impose any behavioral requirements on Atom Processors.
    ///     </para>
    ///     <para>
    ///         The Atom specification defines five initial values for the Registry of Link Relations:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                      <i>alternate</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property 
    ///                      identifies an alternate version of the resource described by the containing element.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>related</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property 
    ///                      identifies a resource related to the resource described by the containing element.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>self</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property 
    ///                      identifies a resource equivalent to the containing element.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>enclosure</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property identifies 
    ///                      a related resource that is potentially large and might require special handling.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      <i>via</i>: Signifies that the IRI in the value of the <see cref="AtomLink.Uri"/> property identifies 
    ///                      a resource that is the source of the information provided in the containing element.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public string Relation
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets the link relation with RFC 4287's default applied.
    /// </summary>
    /// <value><see cref="Relation"/> when one was specified; otherwise <c>alternate</c>.</value>
    /// <remarks>
    ///     §4.2.7.2: a link with no <c>rel</c> attribute "MUST be interpreted as if the link relation
    ///     type is 'alternate'". <see cref="Relation"/> stays as written — empty when the attribute
    ///     was absent, which is also what keeps the attribute off the wire on save — and this
    ///     property is the interpretation, so each consumer does not re-implement the default.
    /// </remarks>
    public string EffectiveRelation => string.IsNullOrEmpty(this.Relation) ? "alternate" : this.Relation;

    /// <summary>
    /// Gets or sets human-readable information about this Web resource.
    /// </summary>
    /// <value>The <c>title</c> attribute. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Language-sensitive: the natural language of the value is whatever <see cref="Language"/> reports. It is plain text — entities represent their
    ///     corresponding characters, never markup — so a title containing <c>&lt;em&gt;</c> is those five characters, not emphasis.
    /// </remarks>
    public string Title
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets an IRI that identifies the location of this Web resource.
    /// </summary>
    /// <value>The <c>href</c> attribute. The default value is <see langword="null"/>, and no <c>href</c> is written when it is.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §4.2.7.1 requires the attribute and makes its value an IRI <i>reference</i>
    ///         (<a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a>) — so, unlike <see cref="AtomId"/>, a relative value is legal and is
    ///         resolved against <see cref="BaseUri"/>. Loading accepts relative and absolute alike; a caller that assumes
    ///         <see cref="System.Uri.IsAbsoluteUri"/> will be surprised by real feeds.
    ///     </para>
    ///     <para>
    ///         <see langword="null"/> means "no target", and saving omits the attribute rather than writing <c>href=""</c>. The empty string is a
    ///         well-formed IRI reference — a <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986</a> §4.4 <i>same-document reference</i> — so
    ///         writing it would pass a validator while telling every consumer the link points at the containing feed. A missing REQUIRED attribute is a
    ///         detectable fault; a false statement is not. <see cref="Load(XPathNavigator)"/> is the other half: it refuses a link with no usable href, so
    ///         this state is only reachable from a caller who never supplied one.
    ///     </para>
    ///     <para>See <see cref="Uri"/> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Uri
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="AtomLink"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns>
    ///     <see langword="true"/> if the <paramref name="source"/> yielded a usable <c>href</c>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomLink"/>.
    ///     </para>
    ///     <para>
    ///         <b>The href alone decides the answer.</b> RFC 4287 §4.2.7.1 makes <c>href</c> REQUIRED, so an element that carries only a <c>rel</c>,
    ///         a <c>title</c> or an <c>xml:base</c> is not a link this class can honestly represent, and every other attribute present is loaded but
    ///         cannot make the result <see langword="true"/>. Reporting success without a target is what previously let
    ///         <c>&lt;link rel="self"/&gt;</c> into the model and back out as <c>href=""</c> — a well-formed
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986</a> §4.4 same-document reference asserting that the link points at the
    ///         containing feed.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);

        AtomUtility.FillCommonObjectAttributes(this, source);

        bool wasLoaded = false;

        if (source.HasAttributes)
        {
            string hrefAttribute = source.GetAttribute("href", string.Empty);
            string relAttribute = source.GetAttribute("rel", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string hreflangAttribute = source.GetAttribute("hreflang", string.Empty);
            string titleAttribute = source.GetAttribute("title", string.Empty);
            string lengthAttribute = source.GetAttribute("length", string.Empty);

            if (!string.IsNullOrEmpty(hrefAttribute))
            {
                if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out Uri? href))
                {
                    this.Uri = href;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(relAttribute))
            {
                this.Relation = relAttribute;
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                this.ContentType = typeAttribute;
            }

            if (!string.IsNullOrEmpty(hreflangAttribute))
            {
                try
                {
                    CultureInfo language = new(hreflangAttribute);
                    this.ContentLanguage = language;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning("AtomLink unable to determine CultureInfo with a name of {0}.", hreflangAttribute);
                }
            }

            if (!string.IsNullOrEmpty(titleAttribute))
            {
                this.Title = titleAttribute;
            }

            if (!string.IsNullOrEmpty(lengthAttribute))
            {
                if (long.TryParse(lengthAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long length))
                {
                    this.Length = length;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomLink"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomLink"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomLink"/>.
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
    /// Saves the current <see cref="AtomLink"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("link", AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        if (this.Uri is not null)
        {
            writer.WriteAttributeString("href", this.Uri.ToString());
        }

        if (!string.IsNullOrEmpty(this.Relation))
        {
            writer.WriteAttributeString("rel", this.Relation);
        }

        if (!string.IsNullOrEmpty(this.ContentType))
        {
            writer.WriteAttributeString("type", this.ContentType);
        }

        if (this.ContentLanguage is not null)
        {
            writer.WriteAttributeString("hreflang", this.ContentLanguage.Name);
        }

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteAttributeString("title", this.Title);
        }

        if (this.Length != long.MinValue)
        {
            writer.WriteAttributeString("length", this.Length.ToString(NumberFormatInfo.InvariantInfo));
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomLink"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomLink"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomLink"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomLink? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Length.CompareTo(other.Length);
        if (result != 0) return result;

        result = string.Compare(this.ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(this.Relation, other.Relation, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        string sourceLanguageName = this.ContentLanguage?.Name ?? string.Empty;
        string targetLanguageName = other.ContentLanguage?.Name ?? string.Empty;
        result = string.Compare(sourceLanguageName, targetLanguageName, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = Uri.Compare(this.Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomLink"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomLink"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomLink"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomLink? other)
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
    public override bool Equals(object? obj) => obj is AtomLink other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Length), HashCodeUtility.Component(this.ContentType), HashCodeUtility.Component(this.Relation), HashCodeUtility.Component(this.ContentLanguage), HashCodeUtility.Component(this.Title), HashCodeUtility.Component(this.Uri));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomLink? first, AtomLink? second)
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
    public static bool operator !=(AtomLink? first, AtomLink? second) => !(first == second);

}