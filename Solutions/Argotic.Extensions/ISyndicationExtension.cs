using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions;

/// <summary>
/// Allows an object to implement a syndication extension by representing a set of properties, methods, indexers and events common to web content syndication extensions.
/// </summary>
/// <remarks>
///     This is the shape the parsing and saving machinery consumes: given a document, an extension says
///     whether it is present, reads itself out of it, and writes itself back. Implement it directly only
///     when you cannot derive from <see cref="SyndicationExtension"/> — that base class supplies every
///     member here except the two <c>Load</c> overloads and <see cref="WriteTo(XmlWriter)"/>, and it is
///     what <see cref="SyndicationExtensionAdapter.FrameworkExtensions"/> tests for when it discovers
///     extensions by reflection. A type implementing only this interface must be registered by
///     <see cref="Type"/> in
///     <see cref="Argotic.Common.SyndicationResourceLoadSettings.SupportedExtensions"/>.
/// </remarks>
/// <seealso cref="SyndicationExtension"/>
/// <seealso cref="SyndicationExtensionAdapter"/>
public interface ISyndicationExtension
{
    /// <summary>
    /// Gets a human-readable description of the syndication extension.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Gets a <see cref="Uri"/> that points to documentation for the syndication extension.
    /// </summary>
    /// <value>The location of the specification, or <see langword="null"/> if the extension does not name one.</value>
    Uri? Documentation
    {
        get;
    }

    /// <summary>
    /// Gets the human-readable name of the syndication extension.
    /// </summary>
    string Name
    {
        get;
    }

    /// <summary>
    /// Gets the <see cref="Version"/> of the specification that the syndication extension conforms to.
    /// </summary>
    /// <value>The specification version, or <see langword="null"/> if the extension does not name one.</value>
    Version? Version
    {
        get;
    }

    /// <summary>
    /// Gets the XML namespace that is used when qualifying the syndication extension's element and attribute names.
    /// </summary>
    /// <value>The namespace URI the specification defines. This identifies the format; the prefix does not.</value>
    string XmlNamespace
    {
        get;
    }

    /// <summary>
    /// Gets the prefix used to associate the syndication extension's element and attribute names with the syndication extension's XML namespace.
    /// </summary>
    /// <value>The conventional prefix for the extension, used when writing. A document may bind a different one.</value>
    string XmlPrefix
    {
        get;
    }

    /// <summary>
    /// Occurs when the syndication extension state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="ISyndicationExtension.Load(IXPathNavigable)"/>
    /// <seealso cref="ISyndicationExtension.Load(XmlReader)"/>
    event EventHandler<SyndicationExtensionLoadedEventArgs> Loaded;

    /// <summary>
    /// Determines if the <see cref="ISyndicationExtension"/> exists in the XML data in the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to parse.</param>
    /// <returns><see langword="true"/> if the <see cref="ISyndicationExtension"/> elements or attributes are present in the <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Notes to implementers. This runs once per extension per entity, ahead of the load it exists to
    ///     avoid, so it must be cheap: answer from the namespaces in scope — see
    ///     <see cref="XPathNavigator.LookupNamespace(string)"/> and
    ///     <see cref="XPathNavigator.GetNamespacesInScope(XmlNamespaceScope)"/> — and do not walk the
    ///     document. It must also be free of side effects and depend on nothing but the argument and
    ///     immutable per-type data: <see cref="SyndicationExtensionAdapter"/> keeps one instance per
    ///     framework extension type and probes every entity in the feed with it.
    /// </remarks>
    bool ExistsInSource(XPathNavigator source);

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces utilized by this <see cref="ISyndicationExtension"/>.
    /// </summary>
    /// <param name="navigator">Provides a cursor model for navigating syndication extension data.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <remarks>
    ///     This method will return a <see cref="XmlNamespaceManager"/> that has a namespace added to it using the <see cref="XmlPrefix"/> and <see cref="XmlNamespace"/> 
    ///     of the extension unless the supplied <see cref="XPathNavigator"/> already has an XML namespace associated to the <see cref="XmlPrefix"/>, in which case 
    ///     the associated XML namespace is used instead. This is to prevent collisions and is an attempt to gracefully handle the case where a XML namespace that 
    ///     is not per the extension's specification has been declared on the syndication resource.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    XmlNamespaceManager CreateNamespaceManager(XPathNavigator navigator);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication extension.</param>
    /// <returns><see langword="true"/> if the syndication extension was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Notes to implementers. Return <see langword="false"/> when the source carried none of this
    ///     extension's data: <see cref="SyndicationExtensionAdapter"/> attaches the instance to the
    ///     entity only on <see langword="true"/>, so an unconditional <see langword="true"/> hangs an
    ///     empty extension off every entity in the feed and writes each one back out on save. Raise
    ///     <see cref="ISyndicationExtension.Loaded"/> before returning, whatever the answer.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    bool Load(IXPathNavigable source);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication extension.</param>
    /// <returns><see langword="true"/> if the syndication extension was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Notes to implementers. All 27 extensions in this assembly implement this the same way — wrap
    ///     the reader in an <see cref="XPathDocument"/> and hand its navigator to
    ///     <see cref="ISyndicationExtension.Load(IXPathNavigable)"/> — which keeps the parsing in one
    ///     place and raises <see cref="ISyndicationExtension.Loaded"/> exactly once. Write it any other
    ///     way and the obligations of the other overload become yours as well.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    bool Load(XmlReader reader);

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    void WriteTo(XmlWriter writer);

    /// <summary>
    /// Writes the prefixed XML namespace for the current syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the prefixed XML namespace declaration to.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    void WriteXmlNamespaceDeclaration(XmlWriter writer);
}