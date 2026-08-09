using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Argotic.Extensions;

/// <summary>
/// Provides the set of methods, properties and events that web content syndication extensions should inherit from.
/// </summary>
/// <remarks>
///     <para>
///         A subclass supplies very little: a <b>public parameterless constructor</b> that chains to one
///         of the base constructors with the extension's prefix, namespace URI and version, and
///         overrides of <see cref="Load(IXPathNavigable)"/>, <see cref="Load(XmlReader)"/> and
///         <see cref="WriteTo(XmlWriter)"/>. Everything else is done here — namespace-manager
///         construction, the presence test, the namespace declaration, and the
///         <see cref="IXmlSerializable"/> plumbing. By convention rather than requirement it also
///         exposes a static <c>MatchByType</c> predicate, which is how callers reach it through
///         <c>FindExtension</c>.
///     </para>
///     <para>
///         The parameterless constructor is not a style preference. Every instance the framework creates
///         comes from <see cref="Activator.CreateInstance(Type)"/>, which binds to public constructors
///         only and throws <see cref="MissingMethodException"/> when there is no match; nothing on the
///         load path catches it.
///     </para>
///     <para>
///         <b>Discovery is by reflection, and it sees only this assembly.</b>
///         <see cref="SyndicationExtensionAdapter.FrameworkExtensions"/> reflects over
///         <c>Argotic.Extensions</c>'s own exported types and returns every non-abstract type assignable
///         to <see cref="SyndicationExtension"/>, so a subclass added <i>here</i> is picked up with no
///         registration step. A subclass you write in your own assembly is not: add its
///         <see cref="Type"/> to
///         <see cref="Argotic.Common.SyndicationResourceLoadSettings.SupportedExtensions"/> before the
///         load and it is offered the document alongside the framework's own.
///     </para>
///     <para>
///         <b><see cref="Load(IXPathNavigable)"/> must return <see langword="false"/> when it read
///         nothing.</b> The adapter attaches the instance to the entity only when <c>Load</c> returns
///         <see langword="true"/>, so an implementation that answers <see langword="true"/>
///         unconditionally hangs an empty extension off every entity in the feed — and writes every one
///         of them back out on save.
///     </para>
///     <para>
///         Implementing <see cref="ISyndicationExtension"/> directly is permitted and works for an
///         extension you register by type, but such a type is invisible to
///         <see cref="SyndicationExtensionAdapter.FrameworkExtensions"/>, which tests assignability to
///         this class and not to the interface.
///     </para>
/// </remarks>
/// <seealso cref="SyndicationExtensionAdapter"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\SyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the SyndicationExtension abstract base class." />
/// </example>
public abstract class SyndicationExtension : ISyndicationExtension, IXmlSerializable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtension"/> class.
    /// </summary>
    protected SyndicationExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtension"/> class using the supplied parameters.
    /// </summary>
    /// <param name="xmlPrefix">The prefix used to associate this syndication extension's element and attribute names with this syndication extension's XML namespace.</param>
    /// <param name="xmlNamespace">The XML namespace that is used when qualifying this syndication extension's element and attribute names.</param>
    /// <param name="version">The <see cref="Version"/> of the specification that this syndication extension conforms to.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlPrefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlPrefix"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
    protected SyndicationExtension(string xmlPrefix, string xmlNamespace, Version version)
    {
        ArgumentException.ThrowIfNullOrEmpty(xmlPrefix);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        ArgumentNullException.ThrowIfNull(version);

        XmlPrefix = xmlPrefix.Trim();
        XmlNamespace = xmlNamespace.Trim();
        Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtension"/> class using the supplied parameters.
    /// </summary>
    /// <param name="xmlPrefix">The prefix used to associate this syndication extension's element and attribute names with this syndication extension's XML namespace.</param>
    /// <param name="xmlNamespace">The XML namespace that is used when qualifying this syndication extension's element and attribute names.</param>
    /// <param name="version">The <see cref="Version"/> of the specification that this syndication extension conforms to.</param>
    /// <param name="documentation">A <see cref="Uri"/> that points to the documentation for this syndication extension.</param>
    /// <param name="name">A human-readable name for this syndication extension.</param>
    /// <param name="description">A human-readable description for this syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlPrefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlPrefix"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="documentation"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    protected SyndicationExtension(string xmlPrefix, string xmlNamespace, Version version, Uri documentation, string name, string description) : this(xmlPrefix, xmlNamespace, version)
    {
        ArgumentNullException.ThrowIfNull(documentation);
        ArgumentException.ThrowIfNullOrEmpty(name);

        Documentation = documentation;
        Name = name.Trim();
        if (!string.IsNullOrEmpty(description))
        {
            Description = description.Trim();
        }
    }

    /// <summary>
    /// Gets a human-readable description of this syndication extension.
    /// </summary>
    /// <value>The description, or an <i>empty</i> string if the constructor was not given one.</value>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// Gets a <see cref="Uri"/> that points to documentation for this syndication extension.
    /// </summary>
    /// <value>The location of the specification, or <see langword="null"/> if the constructor was not given one.</value>
    public Uri? Documentation { get; }

    /// <summary>
    /// Gets a human-readable name of this syndication extension.
    /// </summary>
    /// <value>The name, or an <i>empty</i> string if the constructor was not given one.</value>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the <see cref="Version"/> of the specification that this syndication extension conforms to.
    /// </summary>
    /// <value>The specification version, or <see langword="null"/> if the constructor was not given one.</value>
    public Version? Version { get; }

    /// <summary>
    /// Gets the XML namespace that is used when qualifying this syndication extension's element and attribute names.
    /// </summary>
    /// <value>The namespace URI the specification defines, fixed by the constructor. A document that spells this namespace with some other prefix still matches; a document using a different namespace is a different format.</value>
    public string XmlNamespace { get; } = string.Empty;

    /// <summary>
    /// Gets the prefix used to associate this syndication extension's element and attribute names with this syndication extension's XML namespace.
    /// </summary>
    /// <value>The conventional prefix for the extension, used when writing. Documents are free to bind a different one, and <see cref="CreateNamespaceManager(XPathNavigator)"/> follows the document when reading.</value>
    public string XmlPrefix { get; } = string.Empty;

    /// <summary>
    /// Occurs when the <see cref="SyndicationExtension"/> state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="SyndicationExtension.Load(IXPathNavigable)"/>
    /// <seealso cref="SyndicationExtension.Load(XmlReader)"/>
    public event EventHandler<SyndicationExtensionLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="SyndicationExtension.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationExtensionLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnExtensionLoaded(SyndicationExtensionLoadedEventArgs e)
    {
        if (Loaded is { } handler)
        {
            handler(this, e);
        }
    }

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces utilized by this <see cref="SyndicationExtension"/>.
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
    public XmlNamespaceManager CreateNamespaceManager(XPathNavigator navigator)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        XmlNamespaceManager manager = new(navigator.NameTable);

        // LookupNamespace, not GetNamespacesInScope: this needs one prefix's binding, and building a
        // dictionary of every namespace in scope to read a single key out of it cost 616 B against
        // 152 B for the direct lookup, once per extension per entity. No extension declares the
        // reserved "xml" prefix, so dropping ExcludeXml's filtering changes no answer here.
        string? existingXmlNamespace = navigator.LookupNamespace(this.XmlPrefix);

        manager.AddNamespace(this.XmlPrefix, !string.IsNullOrEmpty(existingXmlNamespace) ? existingXmlNamespace : this.XmlNamespace);

        return manager;
    }

    /// <summary>
    /// Determines if the <see cref="SyndicationExtension"/> exists in the XML data in the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to parse.</param>
    /// <returns><see langword="true"/> if the <see cref="SyndicationExtension"/> elements or attributes are present in the <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         This is the gate the adapter puts in front of a possibly costly
    ///         <see cref="SyndicationExtension.Load(IXPathNavigable)"/>, and it runs once per extension
    ///         per entity, so it must stay cheap. The default implementation asks whether
    ///         <see cref="XmlPrefix"/> is bound at all, and only if that misses does it build the
    ///         in-scope namespace dictionary to look for <see cref="XmlNamespace"/> under any prefix.
    ///     </para>
    ///     <para>
    ///         The first of those tests answers on the prefix alone and does not check what the prefix is
    ///         bound <i>to</i>. A document that binds this extension's conventional prefix to some other
    ///         namespace therefore reaches <c>Load</c>, where
    ///         <see cref="CreateNamespaceManager(XPathNavigator)"/> deliberately follows the document's
    ///         binding. An override wanting stricter matching should test <see cref="XmlNamespace"/>
    ///         directly.
    ///     </para>
    ///     <para>
    ///         Declared virtual, but overridden nowhere in this solution — which is what makes the
    ///         adapter's shared probe instances safe. See
    ///         <see cref="SyndicationExtensionAdapter"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public virtual bool ExistsInSource(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Two side-effect-free tests joined by "or", so their order is not observable and the cheap
        // one goes first. A document that declares this extension under its conventional prefix -
        // which is nearly all of them - answers here, and never builds the dictionary at all.
        if (source.LookupNamespace(this.XmlPrefix) is not null)
        {
            return true;
        }

        // The prefix missed, so ask the question the prefix cannot: is this namespace bound at all,
        // under any prefix? That genuinely needs every binding in scope.
        return source.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml).Values.Contains(this.XmlNamespace);
    }

    /// <summary>
    /// Writes the prefixed XML namespace for the current syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the prefixed XML namespace declaration to.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     <b>Deliberately not virtual.</b> An override that declared a second namespace would be
    ///     invisible to the duplicate-prefix guard in
    ///     <see cref="SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations"/>, which de-duplicates
    ///     on this extension's own prefix and namespace alone — so two extensions claiming the same
    ///     second prefix would emit it twice and abort the save with a duplicate-attribute error, part
    ///     way through the document. An extension needing a second namespace should declare it at the
    ///     element that uses it, via <c>WriteStartElement(prefix, localName, namespaceUri)</c>, which is
    ///     what the GeoRSS family does for GML.
    /// </remarks>
    public void WriteXmlNamespaceDeclaration(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteAttributeString("xmlns", this.XmlPrefix, null, this.XmlNamespace);
    }

    /// <summary>
    /// This method is reserved and <u>should not be used</u>. When implementing the <see cref="IXmlSerializable"/> interface, it is recommended 
    /// that a <see langword="null"/> reference is returned from this method, and instead, if 
    /// specifying a custom schema is required, to apply the <see cref="XmlSchemaProviderAttribute"/> to the class.
    /// </summary>
    /// <returns>
    ///     Always <see langword="null"/>. The declared type is <see cref="XmlSchema"/>, the in-memory
    ///     representation of a W3C XML Schema, but this class does not supply one.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         When serializing or deserializing an object, the <see cref="XmlSerializer"/> class does not perform XML validation. 
    ///         For this reason, it is often safe to omit schema information by providing a trivial implementation of this method, 
    ///         for example by returning a <see langword="null"/> reference.
    ///     </para>
    ///     <para>
    ///         Some .NET Framework types as well as legacy custom types implementing the <see cref="IXmlSerializable"/> interface may be using <see cref="IXmlSerializable.GetSchema()"/> 
    ///         instead of <see cref="XmlSchemaProviderAttribute"/>. In this case, the method returns an accurate XML schema that describes the XML representation 
    ///         of the object generated by the <see cref="WriteXml(XmlWriter)"/> method.
    ///     </para>
    /// </remarks>
    public XmlSchema? GetSchema() => null;

    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> stream from which the object is deserialized.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public void ReadXml(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        this.Load(reader);
    }

    /// <summary>
    /// Converts an object into its XML representation.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> stream to which the object is serialized.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteXml(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        this.WriteTo(writer);
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication extension.</param>
    /// <returns><see langword="true"/> if the <see cref="SyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers. An override owes the caller two things. It must return
    ///         <see langword="false"/> when the source carried none of this extension's data — the
    ///         adapter uses that answer to decide whether to attach the instance at all, and an
    ///         unconditional <see langword="true"/> puts an empty extension on every entity in the feed.
    ///         And it must raise <see cref="SyndicationExtension.Loaded"/> before returning, by calling
    ///         <see cref="OnExtensionLoaded(SyndicationExtensionLoadedEventArgs)"/>, whatever the answer.
    ///     </para>
    ///     <para>
    ///         Resolve prefixes with <see cref="CreateNamespaceManager(XPathNavigator)"/> rather than a
    ///         hand-built manager: it honours a document that binds this extension's prefix to a
    ///         different namespace, which a fixed manager would silently fail to read.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public abstract bool Load(IXPathNavigable source);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication extension.</param>
    /// <returns><see langword="true"/> if the <see cref="SyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Notes to implementers. Every extension in this assembly implements this the same way — wrap
    ///     the reader in an <see cref="XPathDocument"/> and hand its navigator to
    ///     <see cref="SyndicationExtension.Load(IXPathNavigable)"/> — which keeps the parsing in one
    ///     place and raises <see cref="SyndicationExtension.Loaded"/> exactly once. Write it any other
    ///     way and the obligations of the other overload become yours as well.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public abstract bool Load(XmlReader reader);

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public abstract void WriteTo(XmlWriter writer);
}