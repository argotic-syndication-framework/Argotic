using System.Xml;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Provides methods that comprise common utility features shared across the Attention Profiling Markup Language (APML) syndication entities. This class cannot be inherited.
/// </summary>
/// <remarks>This utility class is not intended for use outside the Attention Profiling Markup Language (APML) syndication entities within the framework.</remarks>
internal static class ApmlUtility
{

    /// <summary>
    /// Private member to hold the Attention Profiling Markup Language (APML) 0.6 namespace identifier.
    /// </summary>
    private const string APML_NAMESPACE = "http://www.apml.org/apml-0.6";

    /// <summary>
    /// Gets the XML namespace URI for the Attention Profiling Markup Language (APML) 0.6 specification.
    /// </summary>
    /// <value>Always <c>http://www.apml.org/apml-0.6</c>.</value>
    public static string ApmlNamespace => APML_NAMESPACE;

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Attention Profiling Markup Language (APML) syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <remarks>
    ///     The <c>apml</c> prefix always binds to the APML 0.6 constant, never to whatever default namespace
    ///     a document happened to declare: binding it to the document's own would make any document parse as
    ///     though it were APML.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is <see langword="null"/>.</exception>
    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);
        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("apml", APML_NAMESPACE);

        return manager;
    }
}