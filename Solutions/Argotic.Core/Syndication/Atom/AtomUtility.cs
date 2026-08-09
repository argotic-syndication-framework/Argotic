using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Argotic.Syndication;

/// <summary>
/// Provides methods that comprise common utility features shared across the Atom syndication entities. This class cannot be inherited.
/// </summary>
/// <remarks>This utility class is not intended for use outside the Atom syndication entities within the framework.</remarks>
internal static class AtomUtility
{

    /// <summary>
    /// Private member to hold the Atom 1.0 namespace identifier.
    /// </summary>
    private const string ATOM_NAMESPACE = "http://www.w3.org/2005/Atom";

    /// <summary>
    /// Private member to hold the Atom Publishing Protocol 1.0 namespace identifier.
    /// </summary>
    private const string ATOMPUB_NAMESPACE = "http://www.w3.org/2007/app";

    /// <summary>
    /// Private member to hold the XHTML namespace identifier.
    /// </summary>
    private const string XHTML_NAMESPACE = "http://www.w3.org/1999/xhtml";

    /// <summary>
    /// Private member to hold the XML namespace identifier, the one <c>xml:base</c> and <c>xml:lang</c> are bound to.
    /// </summary>
    private const string XML_NAMESPACE = "http://www.w3.org/XML/1998/namespace";

    /// <summary>
    /// Gets the XML namespace URI for the Atom 1.0 specification.
    /// </summary>
    /// <value><c>http://www.w3.org/2005/Atom</c>.</value>
    public static string AtomNamespace => ATOM_NAMESPACE;

    /// <summary>
    /// Gets the XML namespace URI for the Atom Publishing Protocol 1.0 specification.
    /// </summary>
    /// <value><c>http://www.w3.org/2007/app</c>.</value>
    public static string AtomPublishingNamespace => ATOMPUB_NAMESPACE;

    /// <summary>
    /// Gets the XML namespace URI for the XHTML specification.
    /// </summary>
    /// <value><c>http://www.w3.org/1999/xhtml</c>.</value>
    public static string XhtmlNamespace => XHTML_NAMESPACE;

    /// <summary>
    /// Builds the message for an Atom document of the wrong shape.
    /// </summary>
    /// <param name="expected">The root element the resource type reads.</param>
    /// <param name="other">The root element it was most likely given instead.</param>
    /// <returns>A message naming both document types.</returns>
    /// <remarks>
    ///     <para>
    ///     RFC 4287 defines two document types — a feed document rooted at <c>&lt;feed&gt;</c> and a
    ///     stand-alone entry document rooted at <c>&lt;entry&gt;</c> — and
    ///     <see cref="Argotic.Common.SyndicationContentFormat"/> has one value covering both. So the
    ///     format check in <c>SyndicationResourceAdapter</c>, which rejects every other mismatched
    ///     pairing, compares <c>Atom</c> against <c>Atom</c> and passes. The distinction has to be made
    ///     here instead, at the point the root element is actually looked for.
    ///     </para>
    ///     <para>
    ///     The message names the other document type because that is almost always what the caller has:
    ///     stand-alone entry documents are rare, so an Atom URL is usually a feed.
    ///     </para>
    /// </remarks>
    public static string WrongDocumentShape(string expected, string other) =>
        $"The supplied document has no <{expected}> root element, so it is not an Atom {expected} document. "
        + $"A document rooted at <{other}> is an Atom {other} document and is read by a different type.";
    /// <summary>
    /// Writes an XHTML <c>div</c> whose inner markup is the supplied fragment.
    /// </summary>
    /// <param name="writer">The writer to emit to.</param>
    /// <param name="content">The inner XHTML markup of the div. May be empty.</param>
    /// <remarks>
    ///     <para>
    ///     The write side of the xhtml model both <c>AtomTextConstruct</c> and <c>AtomContent</c>
    ///     share: <c>Content</c> holds the div's inner markup, and saving parses that markup back
    ///     into nodes rather than escaping it into text. Writing with <c>WriteString</c> was the
    ///     review's A3 — markup read faithfully and then betrayed into literal angle brackets on
    ///     save, which a browser renders as visible tags.
    ///     </para>
    ///     <para>
    ///     Going through a real <see cref="XmlReader"/> also makes malformed caller-supplied content
    ///     a loud <see cref="XmlException"/> at save time rather than a silently invalid document,
    ///     and leaves namespace bookkeeping to the writer.
    ///     </para>
    /// </remarks>
    /// <exception cref="XmlException">The <paramref name="content"/> is not a well-formed XML fragment.</exception>
    public static void WriteXhtmlDiv(XmlWriter writer, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            writer.WriteStartElement("div", XHTML_NAMESPACE);
            writer.WriteEndElement();
            return;
        }

        using StringReader wrapped = new($"""<div xmlns="{XHTML_NAMESPACE}">{content}</div>""");
        using XmlReader reader = XmlReader.Create(wrapped, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        writer.WriteNode(reader, defattr: false);
    }

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Atom syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <remarks>
    ///     <b>The prefixes are bound to constants, deliberately.</b> <paramref name="nameTable"/> is an atomised <i>string</i> table and carries no prefix
    ///     bindings, so a manager built from one has no default namespace of its own — <see cref="XmlNamespaceManager.DefaultNamespace"/> returns an empty
    ///     string until <c>AddNamespace(string.Empty, …)</c> gives it one, which nothing here does. Binding <c>atom</c> to whatever default namespace a
    ///     document happened to declare would make every document parse as though it were Atom, so the source document gets no say in what these prefixes
    ///     mean.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is <see langword="null"/>.</exception>
    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);
        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("atom", ATOM_NAMESPACE);
        manager.AddNamespace("app", ATOMPUB_NAMESPACE);
        manager.AddNamespace("xhtml", XHTML_NAMESPACE);

        return manager;
    }

    /// <summary>
    /// Compares objects that implement the <see cref="IAtomCommonObjectAttributes"/> interface.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to be compared.</param>
    /// <param name="target">A object that implements the <see cref="IAtomCommonObjectAttributes"/> to compare with the <paramref name="source"/>.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public static int CompareCommonObjectAttributes(IAtomCommonObjectAttributes? source, IAtomCommonObjectAttributes? target)
    {
        int result = (source, target) switch
        {
            (null, null) => 0,
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0
        };

        if (result != 0 || source is null || target is null) return result;

        result = Uri.Compare(source.BaseUri, target.BaseUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(source.Language?.Name ?? string.Empty, target.Language?.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Modifies the <see cref="IAtomCommonObjectAttributes"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract Atom common attribute information from.</param>
    /// <returns><see langword="true"/> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool FillCommonObjectAttributes(IAtomCommonObjectAttributes target, XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);
        string xmlBaseAttribute = source.GetAttribute("base", manager.LookupNamespace("xml") ?? string.Empty);
        if (!string.IsNullOrEmpty(xmlBaseAttribute))
        {
            if (Uri.TryCreate(xmlBaseAttribute, UriKind.RelativeOrAbsolute, out Uri? baseUri))
            {
                // A relative xml:base resolves against the nearest ancestor's - W3C XML Base s4.3 -
                // so the stored value is the EFFECTIVE base, the only one a consumer can resolve an
                // href against.
                Uri? inherited = ResolveInheritedXmlBase(source);
                target.BaseUri = inherited is not null && !baseUri.IsAbsoluteUri
                    ? new Uri(inherited, baseUri)
                    : baseUri;
                wasLoaded = true;
            }
        }
        else
        {
            // RFC 4287 s2 requires processors to handle xml:base per W3C XML Base, and inheritance
            // is most of what that means: an element with no xml:base of its own is governed by the
            // nearest ancestor's. Without this, the one object a consumer holds - an AtomLink, say -
            // had neither an absolute href nor the base needed to make one, and the ancestor that
            // knew it was gone by the time Load returned.
            //
            // Deliberately NOT counted toward wasLoaded: presence is what wasLoaded reports, and an
            // inherited base is context, not content. Counting it would re-attach constructs the
            // adapters rightly drop as empty - a nameless author inside a feed that happens to set
            // xml:base would come back as an empty husk.
            target.BaseUri = ResolveInheritedXmlBase(source);
        }
        string xmlLangAttribute = source.GetAttribute("lang", manager.LookupNamespace("xml") ?? string.Empty);
        if (!string.IsNullOrEmpty(xmlLangAttribute))
        {
            try
            {
                CultureInfo language = new(source.XmlLang);
                target.Language = language;
                wasLoaded = true;
            }
            catch (ArgumentException)
            {
                System.Diagnostics.Trace.TraceWarning("Unable to determine CultureInfo with a name of {0}.", source.XmlLang);
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Resolves the effective xml:base governing an element from its ancestors.
    /// </summary>
    /// <param name="source">The element whose context to resolve. The supplied navigator is not moved.</param>
    /// <returns>The nearest ancestor's effective base, or <see langword="null"/> when no ancestor declares one.</returns>
    /// <remarks>
    ///     Walks ancestors nearest-first collecting <c>xml:base</c> attributes, stopping at the first
    ///     absolute one; relative bases then stack outermost-first per W3C XML Base §4.3. A relative
    ///     base with no absolute ancestor above it resolves to nothing rather than to an invented
    ///     root.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public static Uri? ResolveInheritedXmlBase(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);

        List<string> bases = [];
        XPathNavigator ancestor = source.CreateNavigator();

        while (ancestor.MoveToParent() && ancestor.NodeType == XPathNodeType.Element)
        {
            string value = ancestor.GetAttribute("base", "http://www.w3.org/XML/1998/namespace");
            if (!string.IsNullOrEmpty(value))
            {
                bases.Add(value);
                if (Uri.TryCreate(value, UriKind.Absolute, out _))
                {
                    break;
                }
            }
        }

        Uri? effective = null;
        for (int i = bases.Count - 1; i >= 0; i--)
        {
            if (effective is null)
            {
                Uri.TryCreate(bases[i], UriKind.Absolute, out effective);
            }
            else if (Uri.TryCreate(bases[i], UriKind.RelativeOrAbsolute, out Uri? next))
            {
                effective = next.IsAbsoluteUri ? next : new Uri(effective, next);
            }
        }

        return effective;
    }

    /// <summary>
    /// Determines whether a content type names an XML media type.
    /// </summary>
    /// <param name="contentType">The value of a <c>type</c> attribute.</param>
    /// <returns><see langword="true"/> for an XML media type per RFC 4287 §4.1.3.3 rule 5; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     The rule-5 set: a subtype of <c>xml</c> or a subtype ending in <c>+xml</c>. The three
    ///     keyword values <c>text</c>/<c>html</c>/<c>xhtml</c> are not media types and are handled by
    ///     their own rules before this question is asked.
    /// </remarks>
    public static bool IsXmlMediaType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        int slash = contentType.IndexOf('/', StringComparison.Ordinal);
        if (slash < 0)
        {
            return false;
        }

        ReadOnlySpan<char> subtype = contentType.AsSpan(slash + 1);
        int semicolon = subtype.IndexOf(';');
        if (semicolon >= 0)
        {
            subtype = subtype[..semicolon];
        }

        subtype = subtype.Trim();
        return subtype.Equals("xml", StringComparison.OrdinalIgnoreCase)
            || subtype.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Writes a stored XML fragment as nodes.
    /// </summary>
    /// <param name="writer">The writer to emit to.</param>
    /// <param name="content">The fragment — element, text, or mixed. May be empty.</param>
    /// <remarks>
    ///     The rule-5 counterpart of <see cref="WriteXhtmlDiv"/>: inline XML content is XML, and
    ///     writing it with <c>WriteString</c> would escape it into text. The fragment is wrapped in a
    ///     namespace-neutral root, parsed, and its children written node by node — so malformed
    ///     caller-supplied content throws at save rather than producing an invalid document.
    /// </remarks>
    /// <exception cref="XmlException">The <paramref name="content"/> is not a well-formed XML fragment.</exception>
    public static void WriteXmlFragment(XmlWriter writer, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        using StringReader wrapped = new($"<x>{content}</x>");
        using XmlReader reader = XmlReader.Create(wrapped, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

        reader.MoveToContent();
        reader.Read();

        while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Depth == 0))
        {
            writer.WriteNode(reader, defattr: false);
        }
    }

    /// <summary>
    /// Saves the current <see cref="IAtomCommonObjectAttributes"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="source">An object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to extract Atom common attribute information from.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public static void WriteCommonObjectAttributes(IAtomCommonObjectAttributes source, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(writer);
        if (source.BaseUri is not null)
        {
            writer.WriteAttributeString("xml", "base", XML_NAMESPACE, source.BaseUri.ToString());
        }
        if (source.Language is not null)
        {
            writer.WriteAttributeString("xml", "lang", XML_NAMESPACE, source.Language.Name);
        }
    }
}