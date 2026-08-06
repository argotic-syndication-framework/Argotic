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
    /// Private member to hold the XML 1.1 namespace identifier.
    /// </summary>
    private const string XML_NAMESPACE = "http://www.w3.org/XML/1998/namespace";

    /// <summary>
    /// Gets the XML namespace URI for the Atom 1.0 specification.
    /// </summary>
    /// <value>The XML namespace URI for the Atom 1.0 specification.</value>
    public static string AtomNamespace => ATOM_NAMESPACE;

    /// <summary>
    /// Gets the XML namespace URI for the Atom Publishing Protocol 1.0 specification.
    /// </summary>
    /// <value>The XML namespace URI for the Atom Publishing Protocol 1.0 specification.</value>
    public static string AtomPublishingNamespace => ATOMPUB_NAMESPACE;

    /// <summary>
    /// Gets the XML namespace URI for the XHTML specification.
    /// </summary>
    /// <value>The XML namespace URI for the Extensible HyperText Markup Language (XHTML) specification.</value>
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

        using StringReader wrapped = new($"<div xmlns=\"{XHTML_NAMESPACE}\">{content}</div>");
        using XmlReader reader = XmlReader.Create(wrapped, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        writer.WriteNode(reader, defattr: false);
    }

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Atom syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference.</exception>

    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);
        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("atom", !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : ATOM_NAMESPACE);
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
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
                target.BaseUri = baseUri;
                wasLoaded = true;
            }
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
    /// Saves the current <see cref="IAtomCommonObjectAttributes"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="source">An object that implements the <see cref="IAtomCommonObjectAttributes"/> interface to extract Atom common attribute information from.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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