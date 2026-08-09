using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents human-readable text.
/// </summary>
/// <remarks>
///     <para>
///         Atom's <c>title</c>, <c>subtitle</c>, <c>summary</c> and <c>rights</c> are all Text constructs, and RFC 4287 §3.1 gives each one of three
///         encodings, declared by the <c>type</c> attribute and modelled here by <see cref="TextType"/>. The distinction is not cosmetic — it decides what
///         <see cref="Content"/> holds.
///     </para>
///     <para>
///         <b>text</b> (§3.1.1.1) and <b>html</b> (§3.1.1.2) both live in the element's character data, so both are XML-escaped on the wire. The difference
///         is only what the escaping reveals: for <c>text</c>, <c>&amp;lt;b&amp;gt;</c> is the three characters a reader should see; for <c>html</c> it is a
///         bold tag the reader should not see. Either way <see cref="Content"/> holds the <i>unescaped</i> string, because XML processing has already
///         happened by the time the parser hands the value over. Storing the escaped form instead is the classic defect here: every load-and-save cycle
///         escapes it once more, and a feed republished through such a library corrupts a little further each pass.
///     </para>
///     <para>
///         <b>xhtml</b> (§3.1.1.3) is different in kind. The content is a single XHTML <c>div</c>, and the div itself is <i>not</i> part of the content.
///         <see cref="Content"/> therefore holds the div's inner markup, unescaped and un-flattened; saving parses it back into nodes rather than writing it
///         as text.
///     </para>
///     <para>
///         Nothing here validates that the content matches the declared type. A publisher who writes escaped HTML under <c>type="text"</c> produces a feed
///         that renders as visible tags, and this library will round-trip it faithfully.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomTextConstructExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomTextConstruct class." />
/// </example>
public class AtomTextConstruct : IComparable<AtomTextConstruct>, IEquatable<AtomTextConstruct>, IAtomCommonObjectAttributes, IExtensibleSyndicationObject, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomTextConstruct"/> class.
    /// </summary>
    public AtomTextConstruct()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomTextConstruct"/> class using the supplied content.
    /// </summary>
    /// <param name="content">The content of this human-readable text.</param>
    /// <remarks>
    ///     The <paramref name="content"/> is <i>language-sensitive</i>, with the natural language of the value being specified by the <see cref="Language"/> property.
    /// </remarks>
    public AtomTextConstruct(string content)
    {
        this.Content = content;
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
    /// Gets or sets the content of this human-readable text.
    /// </summary>
    /// <value>The text, unescaped. For <see cref="AtomTextConstructType.Xhtml"/> this is the inner markup of the wrapping <c>div</c>, without the div. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Language-sensitive: the natural language of the value is whatever <see cref="Language"/> reports. The setter trims, so leading and trailing
    ///     whitespace does not survive a round trip — immaterial for a title, worth knowing for preformatted XHTML.
    /// </remarks>
    public string Content
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the entity encoding utilized by this human-readable text.
    /// </summary>
    /// <value>
    ///     The declared encoding. The default value is <see cref="AtomTextConstructType.None"/>, which means the <c>type</c> attribute was absent and is
    ///     omitted on save. RFC 4287 §3.1.1 makes that equivalent to <see cref="AtomTextConstructType.Text"/>, and this class treats it so.
    /// </value>
    public AtomTextConstructType TextType { get; set; } = AtomTextConstructType.None;

    /// <summary>
    /// Returns the text construct identifier for the supplied <see cref="AtomTextConstructType"/>.
    /// </summary>
    /// <param name="type">The <see cref="AtomTextConstructType"/> to get the text construct identifier for.</param>
    /// <returns>The <c>type</c> attribute value — <c>text</c>, <c>html</c> or <c>xhtml</c> — or an empty string for <see cref="AtomTextConstructType.None"/>.</returns>
    /// <example>
    ///     <code language="cs">
    ///     string attribute = AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Xhtml); // "xhtml"
    ///     </code>
    /// </example>
    public static string ConstructTypeAsString(AtomTextConstructType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="AtomTextConstructType"/> enumeration value that corresponds to the specified text construct type name.
    /// </summary>
    /// <param name="name">A <c>type</c> attribute value: <c>text</c>, <c>html</c> or <c>xhtml</c>. Case is disregarded. May be <see langword="null"/> or empty.</param>
    /// <returns>The matching <see cref="AtomTextConstructType"/>; otherwise, <see cref="AtomTextConstructType.None"/>.</returns>
    /// <remarks>
    ///     Total: null, empty and unrecognised names all yield <see cref="AtomTextConstructType.None"/> rather than throwing, which is what lets a feed
    ///     carrying a nonsense <c>type</c> load as plain text instead of failing outright.
    /// </remarks>
    /// <example>
    ///     <code language="cs">
    ///     AtomTextConstructType type = AtomTextConstruct.ConstructTypeByName("HTML"); // AtomTextConstructType.Html
    ///     </code>
    /// </example>
    public static AtomTextConstructType ConstructTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, AtomTextConstructType.None);

    /// <summary>
    /// Loads this <see cref="AtomTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomTextConstruct"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomTextConstruct"/>.
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
            if (!string.IsNullOrEmpty(typeAttribute))
            {
                AtomTextConstructType type = AtomTextConstruct.ConstructTypeByName(typeAttribute);
                if (type != AtomTextConstructType.None)
                {
                    this.TextType = type;
                    wasLoaded = true;
                }
            }
        }

        if (this.TextType == AtomTextConstructType.Xhtml)
        {
            XPathNavigator? xhtmlDivNavigator = source.SelectChildElement("xhtml", "div", manager);
            if (xhtmlDivNavigator is not null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                // InnerXml, not Value: RFC 4287 s3.1.1.3 makes the div's content the construct's
                // content, markup included, and Value flattens child elements to their text. This is
                // the model AtomContent always had; the two classes used to give two different wrong
                // answers for the same clause.
                this.Content = xhtmlDivNavigator.InnerXml;
                wasLoaded = true;
            }
        }
        else if (this.TextType == AtomTextConstructType.Html && !string.IsNullOrEmpty(source.Value))
        {
            // Value, not InnerXml. RFC 4287 s3.1.1.2's escaping is XML transport; the logical content
            // after XML processing is the HTML itself, and Value is that -- the parser has already
            // unescaped it. InnerXml re-serialises the text node, handing back the escaped form, which
            // WriteTo then escaped again: every load-save cycle multiplied the escaping, so a feed
            // republished through this library corrupted a little more each pass. AtomContent has
            // always read html with Value; this was the one class of the two that did not.
            this.Content = source.Value;
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
    /// Loads this <see cref="AtomTextConstruct"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomTextConstruct"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomTextConstruct"/>.
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
    /// Saves the current <see cref="AtomTextConstruct"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="elementName">The local name of the text construct being written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="elementName"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string elementName)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(elementName);
        writer.WriteStartElement(elementName, AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        if (this.TextType != AtomTextConstructType.None)
        {
            writer.WriteAttributeString("type", AtomTextConstruct.ConstructTypeAsString(this.TextType));
        }

        if (this.TextType == AtomTextConstructType.Xhtml)
        {
            AtomUtility.WriteXhtmlDiv(writer, this.Content);
        }
        else
        {
            writer.WriteString(this.Content);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomTextConstruct"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomTextConstruct"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance, with a generic element name of <i>TextConstruct</i>.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer, "TextConstruct");
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomTextConstruct"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomTextConstruct? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = this.TextType.CompareTo(other.TextType);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomTextConstruct"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomTextConstruct"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomTextConstruct"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomTextConstruct? other)
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
    public override bool Equals(object? obj) => obj is AtomTextConstruct other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.TextType));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomTextConstruct? first, AtomTextConstruct? second)
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
    public static bool operator !=(AtomTextConstruct? first, AtomTextConstruct? second) => !(first == second);
}