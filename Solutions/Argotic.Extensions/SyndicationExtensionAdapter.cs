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
    private const int MaxTrackedNamespaces = 16;

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
    /// For each namespace URI and each conventional prefix, the set of framework probes it
    /// implicates, as a bitmask over <see cref="FrameworkProbes"/> indices.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This exists because auto-detection runs once per entity, and the candidate-selection
    ///         cost used to be paid in full even when the answer was "none": a
    ///         <c>GetNamespacesInScope</c> dictionary built per entity, a candidate list allocated
    ///         per entity, and a scan of every framework probe against both — 26% of the whole load
    ///         of a 50,000-URL sitemap that declares no extension namespace at all
    ///         (<c>.endjin/build-warnings.md</c> §2.40). Walking the entity's namespace axis and
    ///         OR-ing per-key masks answers the same question with no allocation.
    ///     </para>
    ///     <para>
    ///         A mask, not a list, for two reasons: several probes can share a key (the Atom
    ///         Publishing pair shares both its prefix and its namespace), and emitting matches by
    ///         ascending bit index preserves <see cref="FrameworkProbes"/> order — which is
    ///         attachment order, which is the order extensions later save in. A <see cref="ulong"/>
    ///         holds 64 probes; <see cref="CreateProbeMasks"/> refuses loudly rather than truncating
    ///         if the framework ever exceeds that.
    ///     </para>
    /// </remarks>
    private static readonly Lazy<(FrozenDictionary<string, ulong> ByNamespace, FrozenDictionary<string, ulong> ByPrefix, FrozenDictionary<string, ulong> ByContentNamespace)> ProbeMasks = new(CreateProbeMasks);

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
    /// Builds the per-key probe masks over <see cref="FrameworkProbes"/>.
    /// </summary>
    /// <returns>The framework probes implicated by each namespace URI and each prefix.</returns>
    /// <exception cref="InvalidOperationException">The framework has more probes than the mask can hold.</exception>
    /// <remarks>
    ///     The content map is the declaration map plus one override: the Atom namespace also
    ///     implicates FeedHistory, because RFC 5005 Appendix B expresses archive relationships as
    ///     <c>atom:link</c> elements borrowed into RSS — the one framework family whose content
    ///     can sit entirely outside its own namespace
    ///     (<c>FeedHistorySyndicationExtensionContext</c> reads them). The override lives ONLY in
    ///     the content map: putting it in the declaration map would make FeedHistory a candidate
    ///     on every Atom document — whose default namespace is Atom — and its
    ///     <c>ExistsInSource</c> would then rebuild, per entity, the namespace dictionary this
    ///     type exists to avoid (measured: +90 KB on a 100-entry Atom load). An audit of every
    ///     other context found only family-local child elements: the GML profile is read beneath
    ///     <c>georss:where</c>, and no context reads namespaced attributes of the entity itself.
    /// </remarks>
    private static (FrozenDictionary<string, ulong> ByNamespace, FrozenDictionary<string, ulong> ByPrefix, FrozenDictionary<string, ulong> ByContentNamespace) CreateProbeMasks()
    {
        ImmutableArray<ISyndicationExtension> probes = FrameworkProbes.Value;
        if (probes.Length > 64)
        {
            throw new InvalidOperationException($"{probes.Length} framework extensions exceed the 64 the probe bitmask holds; widen the mask in {nameof(SyndicationExtensionAdapter)}.");
        }

        Dictionary<string, ulong> byNamespace = new(StringComparer.Ordinal);
        Dictionary<string, ulong> byPrefix = new(StringComparer.Ordinal);
        Dictionary<string, ulong> byContentNamespace = new(StringComparer.Ordinal);

        for (int i = 0; i < probes.Length; i++)
        {
            ulong bit = 1UL << i;
            byNamespace[probes[i].XmlNamespace] = byNamespace.GetValueOrDefault(probes[i].XmlNamespace) | bit;
            byPrefix[probes[i].XmlPrefix] = byPrefix.GetValueOrDefault(probes[i].XmlPrefix) | bit;
            byContentNamespace[probes[i].XmlNamespace] = byContentNamespace.GetValueOrDefault(probes[i].XmlNamespace) | bit;

            if (probes[i] is Core.FeedHistorySyndicationExtension)
            {
                const string AtomNamespace = "http://www.w3.org/2005/Atom";
                byContentNamespace[AtomNamespace] = byContentNamespace.GetValueOrDefault(AtomNamespace) | bit;
            }
        }

        return (
            byNamespace.ToFrozenDictionary(StringComparer.Ordinal),
            byPrefix.ToFrozenDictionary(StringComparer.Ordinal),
            byContentNamespace.ToFrozenDictionary(StringComparer.Ordinal));
    }

    /// <summary>
    /// Narrows candidate framework probes to those whose namespaces actually occur among the
    /// entity's child elements.
    /// </summary>
    /// <param name="candidates">The declaration-matched candidate mask.</param>
    /// <param name="tracker">The document namespace bindings the declaration walk recorded.</param>
    /// <returns>The candidates with at least one in-namespace child element on this entity.</returns>
    /// <remarks>
    ///     <para>
    ///         This is the layer below §2.40's fix. Declaration matching answers "could this
    ///         family appear anywhere in the document"; on the shape most production feeds take —
    ///         namespaces stamped into the root and never used, declared-vs-used gaps of 96% in
    ///         the live census — every declaration-matched family still constructed a fresh
    ///         extension and ran a full context <c>Load</c> per entity to discover there was
    ///         nothing to read. One walk over the entity's child elements answers presence for
    ///         every family at once, and the measured price of not asking was larger than parsing
    ///         six families' actual payloads (§21).
    ///     </para>
    ///     <para>
    ///         Skipping a family this way is observationally identical to running its
    ///         <c>Load</c> and having it return <see langword="false"/>: the instance was fresh,
    ///         so nothing could have subscribed to its <c>Loaded</c> event, and a
    ///         <see langword="false"/> return was never attached. The walk mutates
    ///         <see cref="Navigator"/> and restores it via <see cref="XPathNavigator.MoveToParent"/>,
    ///         like <see cref="MatchFrameworkProbes"/>.
    ///     </para>
    /// </remarks>
    private ulong FilterByPresentContent(ulong candidates, ref ContentTracker tracker)
    {
        if (tracker.Overflowed)
        {
            // More matched namespaces than the buffer holds: skip filtering rather than filter
            // with an incomplete map. Correct, merely not optimised, and effectively unreachable
            // for real documents.
            return candidates;
        }

        XPathNavigator navigator = this.Navigator;
        if (!navigator.MoveToChild(XPathNodeType.Element))
        {
            return 0;
        }

        FrozenDictionary<string, ulong> byContentNamespace = ProbeMasks.Value.ByContentNamespace;
        ulong present = 0;

        do
        {
            string childNamespace = navigator.NamespaceURI;

            // The canonical map covers content under a probe's own URI (and the Atom->FeedHistory
            // override) even when that URI is declared locally on the child; the tracked entries
            // cover content under whatever URI the document bound a conventional prefix to.
            if (byContentNamespace.TryGetValue(childNamespace, out ulong namespaceBits))
            {
                present |= namespaceBits;
            }

            for (int i = 0; i < tracker.Count; i++)
            {
                if (string.Equals(tracker.Uris[i], childNamespace, StringComparison.Ordinal))
                {
                    present |= tracker.Bits[i];
                }
            }

            if ((candidates & ~present) == 0)
            {
                break;
            }
        }
        while (navigator.MoveToNext(XPathNodeType.Element));

        navigator.MoveToParent();
        return candidates & present;
    }

    /// <summary>
    /// Stack-resident record of which document namespace URIs implicated probes at the current
    /// entity, so content presence can be answered against the document's own bindings.
    /// </summary>
    private struct ContentTracker
    {
        public NamespaceUriBuffer Uris;
        public NamespaceBitsBuffer Bits;
        public int Count;
        public bool Overflowed;
    }

    /// <summary>
    /// Inline storage for the namespace URIs the current entity's scope matched probes under.
    /// </summary>
    [System.Runtime.CompilerServices.InlineArray(MaxTrackedNamespaces)]
    private struct NamespaceUriBuffer
    {
        private string? element0;
    }

    /// <summary>
    /// Inline storage for the probe bits each tracked namespace URI implicates.
    /// </summary>
    [System.Runtime.CompilerServices.InlineArray(MaxTrackedNamespaces)]
    private struct NamespaceBitsBuffer
    {
        private ulong element0;
    }

    /// <summary>
    /// Walks the namespace axis of <see cref="Navigator"/>'s current node and returns the mask of
    /// framework probes its in-scope namespaces implicate.
    /// </summary>
    /// <param name="tracker">Receives the document namespace bindings that implicated probes.</param>
    /// <returns>A bitmask over <see cref="FrameworkProbes"/> indices; zero when nothing matches.</returns>
    /// <remarks>
    ///     <para>
    ///         The walk mutates <see cref="Navigator"/> and restores it: from a namespace node,
    ///         <see cref="XPathNavigator.MoveToParent"/> returns to the owning element — namespace
    ///         axis behaviour the XPath data model guarantees. Nothing that can throw runs between
    ///         the first move and the restore. The axis yields one node per in-scope prefix with
    ///         the nearest binding winning, exactly the set <c>GetNamespacesInScope(ExcludeXml)</c>
    ///         used to build a dictionary from — at 152 B per walk instead of 368 B, both measured
    ///         on the §2.40 document's entities.
    ///     </para>
    ///     <para>
    ///         The residual 152 B is the axis synthesising its namespace nodes, and it is priced
    ///         per walk that <i>returns</i> anything, not per scope: a
    ///         <see cref="XPathNamespaceScope.Local"/> walk on the declaring element costs the same
    ///         152 B (measured), so a walk-local-declarations-up-the-ancestor-chain tier was built,
    ///         measured slower than this (65.78 MB vs 63.49 MB on the 50,000-URL load — it pays the
    ///         same synthesis at the declaring ancestor plus a clone), and removed. Getting under
    ///         the residual means computing each declaring element's mask once per document, which
    ///         is caching machinery with thread-safety questions — recorded as the §2.40 remainder,
    ///         ceiling ~5.7 MB on that load.
    ///     </para>
    /// </remarks>
    private ulong MatchFrameworkProbes(ref ContentTracker tracker)
    {
        XPathNavigator navigator = this.Navigator;
        if (!navigator.MoveToFirstNamespace(XPathNamespaceScope.ExcludeXml))
        {
            return 0;
        }

        (FrozenDictionary<string, ulong> byNamespace, FrozenDictionary<string, ulong> byPrefix, _) = ProbeMasks.Value;
        ulong matched = 0;

        do
        {
            ulong entryBits = 0;

            if (byNamespace.TryGetValue(navigator.Value, out ulong namespaceBits))
            {
                entryBits |= namespaceBits;
            }

            if (byPrefix.TryGetValue(navigator.LocalName, out ulong prefixBits))
            {
                entryBits |= prefixBits;
            }

            if (entryBits != 0)
            {
                matched |= entryBits;

                // Remember which DOCUMENT namespace URI implicated these probes. A probe matched
                // by its conventional prefix may be bound to a variant URI here - real feeds bind
                // "media" to at least three different URIs, and CreateNamespaceManager honours the
                // document's binding when the extension loads - so content presence must be
                // answered against the document's URI, not only the canonical one.
                if (tracker.Count < MaxTrackedNamespaces)
                {
                    tracker.Uris[tracker.Count] = navigator.Value;
                    tracker.Bits[tracker.Count] = entryBits;
                    tracker.Count++;
                }
                else
                {
                    tracker.Overflowed = true;
                }
            }
        }
        while (navigator.MoveToNextNamespace(XPathNamespaceScope.ExcludeXml));

        navigator.MoveToParent();
        return matched;
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
    /// <remarks>
    ///     The candidate set is never materialised as a list. Framework candidates come from
    ///     <see cref="MatchFrameworkProbes"/> as a bitmask consumed in ascending index order —
    ///     <see cref="FrameworkProbes"/> order, as before — and user types follow, as before. On a
    ///     document whose scope implicates nothing, an entity costs one namespace-axis walk and
    ///     nothing else; it used to cost a namespace dictionary, a candidate list and a scan of
    ///     every framework probe, per entity (§2.40: 26% of a 50,000-URL sitemap load).
    /// </remarks>
    private void FillCore(IExtensibleSyndicationObject entity)
    {
        Type entityType = entity.GetType();

        if (this.Settings.AutoDetectExtensions)
        {
            // Stack-resident: sized for the widest realistic declaration set, and costing nothing
            // on the entities (most of them, in most documents) that match no probe at all.
            ContentTracker tracker = default;

            ulong matched = this.MatchFrameworkProbes(ref tracker);
            if (matched != 0)
            {
                matched = this.FilterByPresentContent(matched, ref tracker);
            }

            if (matched != 0)
            {
                ImmutableArray<ISyndicationExtension> probes = FrameworkProbes.Value;
                for (int i = 0; i < probes.Length; i++)
                {
                    if ((matched & (1UL << i)) != 0)
                    {
                        this.TryAttach(entity, entityType, probes[i]);
                    }
                }
            }
        }

        if (this.Settings.SupportedExtensions.Count > 0)
        {
            foreach (ISyndicationExtension extension in SyndicationExtensionAdapter.GetExtensions(this.Settings.SupportedExtensions))
            {
                this.TryAttach(entity, entityType, extension);
            }
        }
    }

    /// <summary>
    /// Probes the source for one candidate and attaches a freshly loaded instance on a hit.
    /// </summary>
    /// <param name="entity">The <see cref="IExtensibleSyndicationObject"/> being filled.</param>
    /// <param name="entityType">The entity's type, hoisted by the caller across candidates.</param>
    /// <param name="candidate">The candidate extension; shared probes are only read.</param>
    private void TryAttach(IExtensibleSyndicationObject entity, Type entityType, ISyndicationExtension candidate)
    {
        // The type test comes first because it is free, where ExistsInSource evaluates namespaces
        // against the document. Both are side-effect free, so the order is not observable.
        Type extensionType = candidate.GetType();
        if (extensionType != entityType
            && candidate.ExistsInSource(this.Navigator)
            && Activator.CreateInstance(extensionType) is ISyndicationExtension instance
            && instance.Load(this.Navigator))
        {
            entity.Extensions.Add(instance);
        }
    }
}