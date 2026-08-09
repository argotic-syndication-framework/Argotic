using System.Xml;
using System.Xml.XPath;

namespace Argotic.Syndication;

/// <summary>
/// Provides methods that comprise common utility features shared across the Really Simple Discoverability (RSD) syndication entities. This class cannot be inherited.
/// </summary>
/// <remarks>This utility class is not intended for use outside the Really Simple Discoverability (RSD) syndication entities within the framework.</remarks>
internal static class RsdUtility
{

    /// <summary>
    /// Private member to hold the Really Simple Discoverability (RSD) 1.0 namespace identifier.
    /// </summary>
    private const string RSD_NAMESPACE = "http://archipelago.phrasewise.com/rsd";

    /// <summary>
    /// Gets the XML namespace URI for the Really Simple Discoverability (RSD) 1.0 specification.
    /// </summary>
    /// <value>Always <c>http://archipelago.phrasewise.com/rsd</c>.</value>
    public static string RsdNamespace => RSD_NAMESPACE;

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Really Simple Discoverability (RSD) syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <remarks>
    ///     The <c>rsd</c> prefix always binds to the RSD 1.0 constant, never to whatever default namespace a
    ///     document happened to declare: binding it to the document's own would make any document parse as
    ///     though it were RSD. Documents that put the elements in no namespace at all are handled instead by
    ///     <see cref="SelectSafe"/>'s unprefixed retry, which is a narrower tolerance and a deliberate one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is <see langword="null"/>.</exception>
    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);
        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("rsd", RSD_NAMESPACE);

        return manager;
    }

    /// <summary>
    /// Selects a node set using the specified XPath expression with the <see cref="IXmlNamespaceResolver"/> object specified to resolve namespace prefixes.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to execute the XPath query against.</param>
    /// <param name="xpath">An XPath expression, whose steps must be prefixed <c>rsd:</c> so that the unprefixed retry can be derived from it.</param>
    /// <param name="resolver">The <see cref="IXmlNamespaceResolver"/> object used to resolve namespace prefixes in the XPath query.</param>
    /// <returns>An <see cref="XPathNodeIterator"/> over the selected node set, empty if neither attempt matched.</returns>
    /// <remarks>
    ///     RSD documents in the wild are inconsistent about the namespace: many put the elements in no
    ///     namespace at all. The query is therefore tried as given and, if it selects nothing, tried again with
    ///     every <c>rsd:</c> stripped out. The stripping is textual, so a query written without the prefix
    ///     gets no second chance, and a literal <c>rsd:</c> anywhere else in the expression would be mangled.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xpath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xpath"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resolver"/> is <see langword="null"/>.</exception>
    public static XPathNodeIterator SelectSafe(XPathNavigator source, string xpath, IXmlNamespaceResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(xpath);
        ArgumentNullException.ThrowIfNull(resolver);

        XPathNodeIterator iterator = source.Select(xpath, resolver);

        if (iterator is not { Count: > 0 })
        {
            string safeXpath = xpath.Replace("rsd:", string.Empty, StringComparison.Ordinal);
            iterator = source.Select(safeXpath, resolver);
        }

        return iterator;
    }

    /// <summary>
    /// Selects a single node in the <see cref="XPathNavigator"/> object using the specified XPath query with the <see cref="IXmlNamespaceResolver"/> object specified to resolve namespace prefixes.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to execute the XPath query against.</param>
    /// <param name="xpath">An XPath expression, whose steps must be prefixed <c>rsd:</c> so that the unprefixed retry can be derived from it.</param>
    /// <param name="resolver">The <see cref="IXmlNamespaceResolver"/> object used to resolve namespace prefixes in the XPath query.</param>
    /// <returns>The first matching node, or <see langword="null"/> if neither attempt matched.</returns>
    /// <remarks>
    ///     RSD documents in the wild are inconsistent about the namespace: many put the elements in no
    ///     namespace at all. The query is therefore tried as given and, if it selects nothing, tried again with
    ///     every <c>rsd:</c> stripped out. The stripping is textual, so a query written without the prefix
    ///     gets no second chance, and a literal <c>rsd:</c> anywhere else in the expression would be mangled.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xpath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xpath"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resolver"/> is <see langword="null"/>.</exception>
    public static XPathNavigator? SelectSafeSingleNode(XPathNavigator source, string xpath, IXmlNamespaceResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(xpath);
        ArgumentNullException.ThrowIfNull(resolver);

        XPathNavigator? navigator = source.SelectSingleNode(xpath, resolver);

        navigator ??= source.SelectSingleNode(xpath.Replace("rsd:", string.Empty, StringComparison.Ordinal), resolver);

        return navigator;
    }
}