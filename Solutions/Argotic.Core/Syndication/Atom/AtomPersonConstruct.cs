using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a person, corporation, or similar entity.
/// </summary>
/// <remarks>
///     <para>
///         The shape behind Atom's <c>author</c> and <c>contributor</c> elements (RFC 4287 §3.2). Exactly one <c>atom:name</c> is required; <c>atom:uri</c>
///         and <c>atom:email</c> are optional and appear at most once each.
///     </para>
///     <para>
///         <c>&lt;name&gt;&lt;/name&gt;</c> is conformant — the grammar is <c>element atom:name { text }</c>, and RELAX NG <c>text</c> admits the empty
///         string. Static site generators emit exactly that when no author is configured. So loading a person with an empty name leaves
///         <see cref="Name"/> at its default rather than throwing, while the setter stays strict for callers building a feed to publish.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomPersonConstructExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomPersonConstruct class." />
/// </example>
public class AtomPersonConstruct : IComparable<AtomPersonConstruct>, IEquatable<AtomPersonConstruct>, IAtomCommonObjectAttributes, IExtensibleSyndicationObject, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomPersonConstruct"/> class.
    /// </summary>
    public AtomPersonConstruct()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomPersonConstruct"/> class using the supplied name.
    /// </summary>
    /// <param name="name">The human-readable name for this entity.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    public AtomPersonConstruct(string name)
    {
        this.Name = name;
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
    /// Gets or sets the e-mail address associated with this entity.
    /// </summary>
    /// <value>A bare address such as <c>jane@example.org</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     RFC 4287 §3.2.3 pins the value to the <c>addr-spec</c> production of
    ///     <a href="https://www.rfc-editor.org/rfc/rfc2822.html">RFC 2822: Internet Message Format</a> §3.4.1 — restated as RFC 5322 §3.4.1. That is the
    ///     address alone: no display name, no angle brackets, so <c>Jane Doe &lt;jane@example.org&gt;</c> is not conformant. Nothing here validates it; a
    ///     malformed address is loaded, stored and written back unchanged.
    /// </remarks>
    public string EmailAddress
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable name for this entity.
    /// </summary>
    /// <value>The name. The default value is an <i>empty</i> string, which is what a loaded <c>&lt;name&gt;&lt;/name&gt;</c> leaves behind.</value>
    /// <remarks>
    ///     Language-sensitive: the natural language of the value is whatever <see cref="Language"/> reports. The setter rejects null and empty, so a feed
    ///     you construct always writes a name; the loader is the deliberate exception, described on <see cref="AtomPersonConstruct"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the IRI associated with this entity.
    /// </summary>
    /// <value>The <c>atom:uri</c> — typically the person's home page — or <see langword="null"/> when the element is absent. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>RFC 4287 §3.2.2 makes this an IRI reference (<a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a>), so it may be relative and resolved against <see cref="BaseUri"/>.</para>
    ///     <para>See <see cref="Uri"/> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    public Uri? Uri { get; set; }

    /// <summary>
    /// Loads this <see cref="AtomPersonConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomPersonConstruct"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomPersonConstruct"/>.
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
        XPathNavigator? nameNavigator = source.SelectChildElement("atom", "name", manager);
        XPathNavigator? uriNavigator = source.SelectChildElement("atom", "uri", manager);
        XPathNavigator? emailNavigator = source.SelectChildElement("atom", "email", manager);

        // <name></name> is conformant Atom: RFC 4287's grammar is `element atom:name { text }`, and
        // RELAX NG text admits the empty string. Jekyll emits exactly that whenever a site has no
        // author configured, and three of Azure Weekly's 478 production feeds were unloadable for it -
        // failing with an ArgumentException about a parameter the caller never passed. So the setter
        // stays strict for writers, the assignment is guarded like AtomCategory.Load's term already
        // is, and the empty name simply leaves the property at its default.
        //
        // wasLoaded is set by the element's PRESENCE, not by the assignment. A person construct's one
        // required child was found, so this construct genuinely loaded - and the caller keeps it. That
        // matters for conformance: a feed with no feed-level author satisfies RFC 4287 section 4.1.1
        // through its entries' author elements, so a reader that dropped the degenerate ones would
        // round-trip a conformant document into one that violates two MUSTs.
        if (nameNavigator is not null)
        {
            if (!string.IsNullOrEmpty(nameNavigator.Value))
            {
                this.Name = nameNavigator.Value;
            }

            wasLoaded = true;
        }

        if (uriNavigator is not null)
        {
            if (Uri.TryCreate(uriNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                this.Uri = uri;
                wasLoaded = true;
            }
        }

        if (emailNavigator is not null)
        {
            this.EmailAddress = emailNavigator.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomPersonConstruct"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomPersonConstruct"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomPersonConstruct"/>.
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
    /// Saves the current <see cref="AtomPersonConstruct"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="elementName">The local name of the person construct being written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="elementName"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string elementName)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(elementName);
        writer.WriteStartElement(elementName, AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        writer.WriteElementString("name", AtomUtility.AtomNamespace, this.Name);

        if (this.Uri is not null)
        {
            writer.WriteElementString("uri", AtomUtility.AtomNamespace, this.Uri.ToString());
        }

        if (!string.IsNullOrEmpty(this.EmailAddress))
        {
            writer.WriteElementString("email", AtomUtility.AtomNamespace, this.EmailAddress);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomPersonConstruct"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomPersonConstruct"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance, with a generic element name of <i>PersonConstruct</i>.
    /// </remarks>
    public override string ToString()
    {
        using StringWriter stringWriter = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
        {
            this.WriteTo(writer, "PersonConstruct");
        }


        return stringWriter.ToString();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomPersonConstruct"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomPersonConstruct? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.EmailAddress, other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = Uri.Compare(this.Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomPersonConstruct"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomPersonConstruct"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomPersonConstruct"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomPersonConstruct? other)
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
    public override bool Equals(object? obj) => obj is AtomPersonConstruct other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.EmailAddress), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Uri));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomPersonConstruct? first, AtomPersonConstruct? second)
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
    public static bool operator !=(AtomPersonConstruct? first, AtomPersonConstruct? second) => !(first == second);
}