using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="IExtensibleSyndicationObject"/>.
/// </summary>
/// <remarks>
///     <para>
///         This is where extensions are discovered and attached, and it is the only part of the
///         extensibility model a custom extension has to satisfy. An adapter is constructed for the
///         entity being loaded, and <see cref="Fill(IExtensibleSyndicationObject)"/> runs the pipeline:
///     </para>
///     <list type="number">
///         <item>
///             <description>
///                 Build the candidate set. When
///                 <see cref="SyndicationResourceLoadSettings.AutoDetectExtensions"/> is
///                 <see langword="true"/> — the default — that is every framework extension whose
///                 <see cref="ISyndicationExtension.XmlNamespace"/> appears among the namespaces in scope
///                 at <see cref="Navigator"/>, or whose <see cref="ISyndicationExtension.XmlPrefix"/> is
///                 bound there, plus every type in
///                 <see cref="SyndicationResourceLoadSettings.SupportedExtensions"/>. When it is
///                 <see langword="false"/>, only the latter.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Ask each candidate <see cref="ISyndicationExtension.ExistsInSource(XPathNavigator)"/>.
///             </description>
///         </item>
///         <item>
///             <description>
///                 For each that says yes, construct a <i>fresh</i> instance and call
///                 <see cref="ISyndicationExtension.Load(IXPathNavigable)"/>; add it to
///                 <see cref="IExtensibleSyndicationObject.Extensions"/> only if that returns
///                 <see langword="true"/>.
///             </description>
///         </item>
///     </list>
///     <para>
///         The framework half of step 1 is reflection over this assembly, described on
///         <see cref="FrameworkExtensions"/>; nothing registers an extension by hand. Those framework
///         candidates are one shared instance per type, reused to probe every entity in the document —
///         see the remarks on <see cref="SyndicationExtension.ExistsInSource(XPathNavigator)"/> for the
///         constraint that places on an override. Step 3 always builds a new object, because that one
///         escapes into the caller's object graph.
///     </para>
///     <para>
///         Every instantiation here goes through <see cref="Activator.CreateInstance(Type)"/>, so an
///         extension type without a public parameterless constructor throws
///         <see cref="MissingMethodException"/> at load time rather than being skipped.
///     </para>
/// </remarks>
public class SyndicationExtensionAdapter
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the extended syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="IExtensibleSyndicationObject"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public SyndicationExtensionAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);

        Navigator = navigator;
        Settings = settings;
    }

    /// <summary>
    /// One reusable instance per framework extension type, used only to probe a document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="Fill(IExtensibleSyndicationObject, XmlNamespaceManager)"/> runs once per entity, so a
    ///         thousand-item feed used to construct all twenty-odd framework extensions a thousand times over
    ///         just to ask each one whether it applies. The benchmark decomposition attributes roughly 85% of
    ///         auto-detection's allocation to those <see cref="Activator.CreateInstance(Type)"/> calls.
    ///     </para>
    ///     <para>
    ///         Sharing them is safe because the probe only reads immutable per-type data:
    ///         <see cref="SyndicationExtension.ExistsInSource(XPathNavigator)"/> is declared virtual but is not
    ///         overridden anywhere in the solution, and its body reads only <c>XmlNamespace</c> and
    ///         <c>XmlPrefix</c>, both fixed by each extension's constructor. Nothing here is handed to a caller
    ///         or loaded into - a matching probe still gets a fresh instance for the actual
    ///         <see cref="ISyndicationExtension.Load(IXPathNavigable)"/>.
    ///     </para>
    /// </remarks>
    private static readonly Lazy<ImmutableArray<ISyndicationExtension>> FrameworkProbes = new(CreateFrameworkProbes);

    /// <summary>
    /// The probes of <see cref="FrameworkProbes"/>, keyed by their concrete type.
    /// </summary>
    /// <remarks>
    ///     Exists for <see cref="WriteXmlNamespaceDeclarations(IList{Type}, XmlWriter)"/>, which
    ///     otherwise runs <see cref="Activator.CreateInstance(Type)"/> on every supported extension
    ///     type on every save purely to read its prefix and namespace back. Reuse is safe on the
    ///     same argument the probes themselves rest on: <see cref="SyndicationExtension.WriteXmlNamespaceDeclaration(XmlWriter)"/>
    ///     is not virtual and reads only <c>XmlPrefix</c> and <c>XmlNamespace</c>, both fixed by
    ///     each extension's constructor.
    /// </remarks>
    private static readonly Lazy<FrozenDictionary<Type, ISyndicationExtension>> FrameworkProbesByType =
        new(static () => FrameworkProbes.Value.ToFrozenDictionary(static probe => probe.GetType()));

    /// <summary>
    /// Gets the collection of <see cref="Type"/> objects that represent <see cref="ISyndicationExtension"/> instances natively supported by the framework.
    /// </summary>
    /// <value>A newly built list, in reflection order. Every get repeats the reflection and allocates again; hold the result rather than calling this in a loop.</value>
    /// <remarks>
    ///     <para>
    ///         The list is discovered, not maintained: every exported type of <c>Argotic.Extensions</c>
    ///         that is assignable to <see cref="SyndicationExtension"/> and is not abstract. A new
    ///         extension added to this assembly appears here with no registration step — and,
    ///         symmetrically, one made <c>internal</c> silently disappears, because
    ///         <see cref="Assembly.GetExportedTypes"/> sees only public types.
    ///     </para>
    ///     <para>
    ///         The test is assignability to <see cref="SyndicationExtension"/>, not to
    ///         <see cref="ISyndicationExtension"/>. A type that implements the interface directly is not
    ///         found here; register it through
    ///         <see cref="SyndicationResourceLoadSettings.SupportedExtensions"/> instead.
    ///     </para>
    /// </remarks>
    public static IList<Type> FrameworkExtensions
    {
        get
        {
            // Discovered by reflection rather than a hand-maintained list. The list this replaced sat
            // behind a disabled #if branch where the compiler never checked it, and had silently
            // drifted six extensions out of date by the time it was removed.
            //
            // Filtering on !IsAbstract rather than excluding SyndicationExtension by name also covers
            // any future abstract intermediate base: GetExtensions calls Activator.CreateInstance on
            // everything returned here, so an abstract type would throw at runtime.
            List<Type> extensions =
            [
                .. Assembly.GetExecutingAssembly()
                            .GetExportedTypes()
                            .Where(static t => typeof(SyndicationExtension).IsAssignableFrom(t) && !t.IsAbstract),
            ];

            return extensions;
        }
    }

    /// <summary>
    /// Gets the <see cref="XPathNavigator"/> used to fill an extensible syndication resource.
    /// </summary>
    /// <value>The navigator positioned on the entity being filled. Its in-scope namespaces are what auto-detection matches against, so they include declarations inherited from ancestors, not only those written on the entity's own element.</value>
    public XPathNavigator Navigator { get; }

    /// <summary>
    /// Gets the <see cref="SyndicationResourceLoadSettings"/> used to configure the fill of an extensible syndication resource.
    /// </summary>
    public SyndicationResourceLoadSettings Settings { get; } = new();

    /// <summary>
    /// Fills the specified collection of <see cref="Type"/> objects using the supplied <see cref="IExtensibleSyndicationObject"/>.
    /// </summary>
    /// <param name="entity">A <see cref="IExtensibleSyndicationObject"/> to extract syndication extensions from.</param>
    /// <param name="types">The <see cref="IList{T}"/> collection of <see cref="Type"/> objects to be filled.</param>
    /// <remarks>
    ///    This method provides implementers of the <see cref="ISyndicationResource"/> interface with a simple way 
    ///    to fill a <see cref="SyndicationResourceSaveSettings.SupportedExtensions"/> collection when implementing the 
    ///    <see cref="ISyndicationResource.Save(XmlWriter, SyndicationResourceSaveSettings)"/> abstract method.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="types"/> is <see langword="null"/>.</exception>
    public static void FillExtensionTypes(IExtensibleSyndicationObject entity, IList<Type> types)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(types);

        if (entity.HasExtensions)
        {
            foreach (ISyndicationExtension extension in entity.Extensions)
            {
                if (extension is not null)
                {
                    Type type = extension.GetType();
                    if (!types.Contains(type))
                    {
                        types.Add(type);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a collection of <see cref="ISyndicationExtension"/> instances for the specified types.
    /// </summary>
    /// <param name="types">A <see cref="IList{T}"/> collection of <see cref="Type"/> objects to be instantiated.</param>
    /// <returns>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects instantiated using the supplied <paramref name="types"/>.</returns>
    /// <remarks>
    ///     <para>Each <see cref="ISyndicationExtension"/> instance in the <see cref="IList{T}"/> collection will be instantiated using its default constructor. </para>
    ///     <para>Types that are <see langword="null"/> or do not implement the <see cref="ISyndicationExtension"/> interface are ignored.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="types"/> is <see langword="null"/>.</exception>
    public static IList<ISyndicationExtension> GetExtensions(IList<Type> types)
    {
        List<ISyndicationExtension> extensions = [];
        ArgumentNullException.ThrowIfNull(types);

        foreach (Type type in types)
        {
            if (type is not null)
            {
                if (Activator.CreateInstance(type) is ISyndicationExtension extension)
                {
                    extensions.Add(extension);
                }
            }
        }

        return extensions;
    }

    /// <summary>
    /// Creates a collection of <see cref="ISyndicationExtension"/> instances for the specified types.
    /// </summary>
    /// <param name="types">A <see cref="IList{T}"/> collection of <see cref="Type"/> objects that represent user-defined syndication extensions to be instantiated.</param>
    /// <param name="namespaces">The XML namespaces in scope, keyed by prefix, used to filter the native framework syndication extensions. A framework extension is kept when its namespace URI is one of the values, <i>or</i> its conventional prefix is one of the keys.</param>
    /// <returns>
    ///     A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects instantiated using the supplied <paramref name="types"/> and <paramref name="namespaces"/>.
    /// </returns>
    /// <remarks>
    ///     This method instantiates all the available native framework syndication extensions, and then filters them based on the XML namespaces and prefixes contained in the supplied <paramref name="namespaces"/>. 
    ///     The user defined syndication extensions are then instantiated, and are added to the return collection if they do not already exist.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="types"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="namespaces"/> is <see langword="null"/>.</exception>
    public static IList<ISyndicationExtension> GetExtensions(IList<Type> types, IDictionary<string, string> namespaces)
    {
        List<ISyndicationExtension> supportedExtensions = [];
        ArgumentNullException.ThrowIfNull(types);
        ArgumentNullException.ThrowIfNull(namespaces);

        IList<ISyndicationExtension> nativeExtensions = SyndicationExtensionAdapter.GetExtensions(SyndicationExtensionAdapter.FrameworkExtensions);

        foreach (ISyndicationExtension extension in nativeExtensions)
        {
            // Values.Contains rather than Dictionary.ContainsValue: the latter is not on IDictionary.
            // Both are a linear scan over the values using the default equality comparer.
            if (namespaces.Values.Contains(extension.XmlNamespace) || namespaces.ContainsKey(extension.XmlPrefix))
            {
                if (!supportedExtensions.Contains(extension))
                {
                    supportedExtensions.Add(extension);
                }
            }
        }

        IList<ISyndicationExtension> userExtensions = SyndicationExtensionAdapter.GetExtensions(types);
        foreach (ISyndicationExtension extension in userExtensions)
        {
            if (!supportedExtensions.Contains(extension))
            {
                supportedExtensions.Add(extension);
            }
        }

        return supportedExtensions;
    }

    /// <summary>
    /// Creates the shared probe instance for every framework extension type.
    /// </summary>
    /// <returns>One instance per type returned by <see cref="FrameworkExtensions"/>.</returns>
    private static ImmutableArray<ISyndicationExtension> CreateFrameworkProbes()
    {
        ImmutableArray<ISyndicationExtension>.Builder probes = ImmutableArray.CreateBuilder<ISyndicationExtension>();

        foreach (Type type in SyndicationExtensionAdapter.FrameworkExtensions)
        {
            if (Activator.CreateInstance(type) is ISyndicationExtension extension)
            {
                probes.Add(extension);
            }
        }

        return probes.ToImmutable();
    }

    /// <summary>
    /// Returns the extensions worth probing this document for, without instantiating the framework ones.
    /// </summary>
    /// <param name="types">User-defined syndication extension types to include.</param>
    /// <param name="namespaces">The XML namespaces in scope, used to filter the framework extensions.</param>
    /// <returns>The candidate extensions. These instances are shared and must be treated as read-only.</returns>
    /// <remarks>
    ///     Mirrors <see cref="GetExtensions(IList{Type}, IDictionary{string, string})"/>, but reuses the cached
    ///     probes rather than constructing a fresh set. The public overload keeps allocating, because what it
    ///     returns escapes to a caller who may do anything with it.
    /// </remarks>
    private static List<ISyndicationExtension> GetExtensionProbes(IList<Type> types, IDictionary<string, string> namespaces)
    {
        List<ISyndicationExtension> supportedExtensions = [];

        foreach (ISyndicationExtension extension in SyndicationExtensionAdapter.FrameworkProbes.Value)
        {
            // Values.Contains rather than Dictionary.ContainsValue: the latter is not on IDictionary.
            // Both are a linear scan over the values using the default equality comparer.
            if ((namespaces.Values.Contains(extension.XmlNamespace) || namespaces.ContainsKey(extension.XmlPrefix))
                && !supportedExtensions.Contains(extension))
            {
                supportedExtensions.Add(extension);
            }
        }

        foreach (ISyndicationExtension extension in SyndicationExtensionAdapter.GetExtensions(types))
        {
            if (!supportedExtensions.Contains(extension))
            {
                supportedExtensions.Add(extension);
            }
        }

        return supportedExtensions;
    }

    /// <summary>
    /// Saves the supplied <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="extensions">A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent the syndication extensions to be written.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="extensions"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public static void WriteExtensionsTo(IEnumerable<ISyndicationExtension> extensions, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        ArgumentNullException.ThrowIfNull(writer);

        foreach (ISyndicationExtension extension in extensions)
        {
            extension.WriteTo(writer);
        }
    }

    /// <summary>
    /// Writes the prefixed XML namespace declarations for the supplied <see cref="IList{T}"/> collection of syndication extension <see cref="Type"/> objects to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="types">A <see cref="IList{T}"/> collection of <see cref="Type"/> objects that represent the syndication extensions to write prefixed XML namespace declarations for.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="types"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public static void WriteXmlNamespaceDeclarations(IList<Type> types, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(types);
        ArgumentNullException.ThrowIfNull(writer);

        HashSet<string> declaredPrefixes = new(StringComparer.Ordinal);

        foreach (Type type in types)
        {
            if (type is not null)
            {
                // Framework types reuse the cached probe; only consumer-defined types pay for a
                // fresh instance, because their constructors are not this assembly's to vouch for.
                ISyndicationExtension? extension = FrameworkProbesByType.Value.TryGetValue(type, out ISyndicationExtension? probe)
                    ? probe
                    : Activator.CreateInstance(type) as ISyndicationExtension;

                if (extension is not null)
                {
                    // Extensions can share a prefix - the Atom Publishing control and edited extensions
                    // both use "app" - and repeating a declaration is not well-formed XML.
                    if (declaredPrefixes.Add($"{extension.XmlPrefix}:{extension.XmlNamespace}"))
                    {
                        extension.WriteXmlNamespaceDeclaration(writer);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Modifies the <see cref="IExtensibleSyndicationObject"/> to match the data source.
    /// </summary>
    /// <remarks>
    ///     This only adds. Nothing already in <see cref="IExtensibleSyndicationObject.Extensions"/> is
    ///     inspected, so filling the same entity twice attaches a second copy of every extension the
    ///     source declares. Prefix resolution is each extension's own business — see
    ///     <see cref="SyndicationExtension.CreateNamespaceManager(XPathNavigator)"/> — so this method
    ///     builds no <see cref="XmlNamespaceManager"/> of its own.
    /// </remarks>
    /// <param name="entity">The <see cref="IExtensibleSyndicationObject"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is <see langword="null"/>.</exception>
    public void Fill(IExtensibleSyndicationObject entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Straight to the implementation. This used to construct an XmlNamespaceManager purely to
        // satisfy the overload below, which validates that parameter and then never reads it - a
        // whole manager allocated and discarded per call. On an OPML subscription list that is one
        // per outline, and OpmlOutline.Load takes this overload for every one of them.
        this.FillCore(entity);
    }

    /// <summary>
    /// Modifies the <see cref="IExtensibleSyndicationObject"/> to match the data source.
    /// </summary>
    /// <param name="entity">The <see cref="IExtensibleSyndicationObject"/> to be filled.</param>
    /// <param name="manager">Ignored. It is rejected when <see langword="null"/> and otherwise never read.</param>
    /// <remarks>
    ///     Identical to <see cref="Fill(IExtensibleSyndicationObject)"/>. The <paramref name="manager"/>
    ///     parameter is vestigial: extension probing resolves namespaces from
    ///     <see cref="Navigator"/> itself, and each extension builds the manager it needs inside its own
    ///     <c>Load</c>. It survives because this is public API with twenty-five call sites, and its
    ///     null check survives because rejecting <see langword="null"/> is observable behaviour. Prefer
    ///     the single-argument overload in new code.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public void Fill(IExtensibleSyndicationObject entity, XmlNamespaceManager manager)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // The manager is not used, and has not been for as long as this method has existed: extension
        // probing resolves namespaces from the navigator itself. It stays in the signature because
        // this is public API and twenty-five call sites pass one, and its null check stays because
        // rejecting null is observable behaviour a caller may depend on.
        ArgumentNullException.ThrowIfNull(manager);

        this.FillCore(entity);
    }

    /// <summary>
    /// Adds a loaded instance of every extension the data source declares.
    /// </summary>
    /// <param name="entity">The <see cref="IExtensibleSyndicationObject"/> to be filled.</param>
    private void FillCore(IExtensibleSyndicationObject entity)
    {
        IList<ISyndicationExtension> extensions = this.Settings.AutoDetectExtensions
            ? SyndicationExtensionAdapter.GetExtensionProbes(this.Settings.SupportedExtensions, this.Navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml))
            : SyndicationExtensionAdapter.GetExtensions(this.Settings.SupportedExtensions);

        Type entityType = entity.GetType();

        foreach (ISyndicationExtension extension in extensions)
        {
            // The type test comes first because it is free, where ExistsInSource evaluates namespaces
            // against the document. Both are side-effect free, so the order is not observable.
            Type extensionType = extension.GetType();
            if (extensionType != entityType
                && extension.ExistsInSource(this.Navigator)
                && Activator.CreateInstance(extensionType) is ISyndicationExtension instance
                && instance.Load(this.Navigator))
            {
                entity.Extensions.Add(instance);
            }
        }
    }
}