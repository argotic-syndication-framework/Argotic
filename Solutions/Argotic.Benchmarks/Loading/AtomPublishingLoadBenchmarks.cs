using System.Diagnostics.CodeAnalysis;
using System.Text;

using Argotic.Publishing;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads the three Atom Publishing Protocol document types, none of which any benchmark has parsed.
/// </summary>
/// <remarks>
/// <para>
/// <c>FormatBreadthLoadBenchmarks</c> closed the gap for five of the eight formats that had a public
/// <c>Load(Stream)</c> and no benchmark. It left three: <see cref="AtomServiceDocument"/>,
/// <see cref="AtomCategoryDocument"/> and <see cref="SitemapIndex"/>. This class takes the first two
/// and the entry resource that goes with them; <c>SitemapIndexLoadBenchmarks</c> takes the third.
/// </para>
/// <para>
/// Provenance, stated per arm because it differs per arm. Real Atom Publishing documents are
/// scarce on the open web — they are the control surface of an editing API, not something a publisher
/// leaves at a URL — and what survives is uneven:
/// </para>
/// <list type="bullet">
///   <item><description>Service documents are real. Four <c>app:service</c> documents fetched
///   byte-exact from the OData reference services live in the corpus. They are the genuine article,
///   and they are also revealing: all four are <c>collection</c> plus <c>atom:title</c> and nothing
///   else, with an <c>xml:base</c> and a prefixed Atom namespace. That is what a real service document
///   turns out to look like, as against what RFC 5023's examples suggest.</description></item>
///   <item><description>The category document is synthetic. No public <c>app:categories</c>
///   document could be found — searching produced only the specification's own examples — so the one
///   used here is written from RFC 5023 §7.2.1 and is labelled as such wherever it appears. It is a
///   fair test of the parser and it is not evidence about what publishers emit, because
///   there is no such evidence.</description></item>
///   <item><description>The entry documents are the repository's own sample plus one synthetic
///   variant. <c>SampleData/AtomEntryDocument.xml</c> is hand-authored and carries no
///   <c>app:</c> elements at all, which is precisely why the synthetic variant exists.</description></item>
/// </list>
/// <para>
/// The categories arm exists because writing this class exposed the defect that had blocked it.
/// <c>AtomServiceDocument.Load</c> previously threw <see cref="FormatException"/> for any service
/// document whose collection declared an <c>app:categories</c> element, whether inline or
/// out-of-line, including the example service document printed in RFC 5023 §8.3.3; this arm could
/// therefore only have measured a throw. The four real corpus documents load successfully because
/// OData emits no categories, which is why the defect went unobserved.
/// <see cref="LoadServiceDocumentWithCategories"/> became measurable once that defect and the two
/// behind it were fixed, and it is the only arm here that exercises
/// <c>AtomCategoryDocument.Load</c> through a service document rather than directly.
/// </para>
/// <para>
/// The load-bearing pair is the entry one. <see cref="AtomEntryResource"/> differs from
/// <see cref="AtomEntry"/> in exactly one respect: it overrides
/// <c>Load(IXPathNavigable, settings)</c> to call the base and then run
/// <c>LoadAtomPublishingExtensions</c>, which is two <c>FindExtension</c> calls — each a
/// <c>List&lt;T&gt;</c> copy and a linear predicate scan over the entry's extensions. Loading the same
/// bytes through both types isolates that override and nothing else. Running it over the sample, which
/// carries no <c>app:</c> elements, prices the override when it finds nothing; running it over the
/// synthetic variant, which carries both, prices it when it finds both. A change to extension lookup
/// that helps only the empty case cannot hide between those two arms.
/// </para>
/// <para>
/// No <c>[Params]</c>. Every arm here is a document of fixed size — three of them real files
/// that cannot be resized without ceasing to be real — so a class-scoped axis would reproduce all
/// eight rows unchanged at every value. Scaling belongs where the corpus is generated, which for this
/// area it is not.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "formats", "atompub")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class AtomPublishingLoadBenchmarks
{
    /// <summary>
    /// A synthetic <c>app:categories</c> document, written from RFC 5023 §7.2.1 and not taken from any
    /// live service.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This is not a real-world document and must not be cited as one. No publicly
    ///     reachable <c>app:categories</c> document was found when the corpus was collected, so
    ///     nothing here reflects what a real server emits — only what the specification permits and
    ///     what <c>AtomPublishing10SyndicationResourceAdapter.Fill(AtomCategoryDocument)</c> reads.
    ///     </para>
    ///     <para>
    ///     It is written to exercise the adapter fully rather than minimally: the <c>fixed</c>,
    ///     <c>scheme</c> and <c>href</c> attributes are all present, because the adapter reads all
    ///     three, and the categories vary between term-only, term-plus-label and
    ///     term-plus-own-scheme, because <c>AtomCategory.Load</c> branches on each.
    ///     </para>
    ///     <para>
    ///     One shape it deliberately does not cover: the childless out-of-line document. That
    ///     used to lose all three attributes, because the adapter read them inside an
    ///     <c>if (documentNavigator.HasChildren)</c>. That is now fixed and pinned by
    ///     <c>AtomPublishingCategoriesTests</c>. It is a correctness question rather than a timing one,
    ///     and it is cheaper than this document by construction, so it stays in the suite and out of
    ///     here.
    ///     </para>
    /// </remarks>
    private const string SyntheticCategoryDocument = """
        <?xml version="1.0" encoding="utf-8"?>
        <app:categories xmlns:app="http://www.w3.org/2007/app"
                        xmlns:atom="http://www.w3.org/2005/Atom"
                        fixed="yes"
                        scheme="http://example.com/cats/big3"
                        href="http://example.com/cats/forMain.atomcat">
          <atom:category term="animal" />
          <atom:category term="vegetable" label="Vegetable" />
          <atom:category term="mineral" label="Mineral" scheme="http://example.com/cats/alternate" />
          <atom:category term="fungus" label="Fungus" />
          <atom:category term="protist" label="Protist" scheme="http://example.com/cats/alternate" />
          <atom:category term="bacterium" label="Bacterium" />
        </app:categories>
        """;

    /// <summary>
    /// A synthetic Atom entry document carrying both Atom Publishing extension elements, not taken from
    /// any live service.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Synthetic, and for a reason the sample cannot serve.
    ///     <c>SampleData/AtomEntryDocument.xml</c> declares no <c>app:</c> namespace and carries no
    ///     <c>app:</c> elements, so <c>AtomEntryResource</c>'s override finds nothing there and every
    ///     measurement over it prices the empty case. This document declares the namespace and carries
    ///     both <c>app:edited</c> and <c>app:control</c>/<c>app:draft</c>, so extension detection
    ///     genuinely runs, both extensions are genuinely attached, and both <c>FindExtension</c> calls
    ///     genuinely match.
    ///     </para>
    ///     <para>
    ///     It is otherwise deliberately small. The subject of the arm that uses it is the publishing
    ///     override, and padding the entry with unrelated Atom content would bury that delta under
    ///     work the two entry types share.
    ///     </para>
    /// </remarks>
    private const string SyntheticEntryWithPublishingControl = """
        <?xml version="1.0" encoding="utf-8"?>
        <entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
          <title type="text">An Entry Under Editorial Control</title>
          <id>urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6c</id>
          <updated>2024-01-01T12:00:00Z</updated>
          <published>2024-01-01T10:00:00Z</published>
          <author>
            <name>Entry Author</name>
            <email>author@example.com</email>
          </author>
          <link href="https://example.com/entry" rel="alternate" type="text/html" />
          <link href="https://example.com/entry/edit" rel="edit" type="application/atom+xml" />
          <summary type="text">The summary of an entry that is still a draft.</summary>
          <content type="text">The body of an entry that is still a draft.</content>
          <app:edited>2024-01-01T12:00:00Z</app:edited>
          <app:control>
            <app:draft>yes</app:draft>
          </app:control>
        </entry>
        """;

    /// <summary>
    /// The service document printed verbatim in RFC 5023 §8.3.3.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Not a real-world document, but not invented either. It is the specification's own
    ///     example, reproduced unchanged, which makes it the fairest available stand-in for the shape
    ///     the four OData documents cannot express: collections that declare categories.
    ///     </para>
    ///     <para>
    ///     It carries both spellings — an out-of-line <c>categories</c> with an <c>href</c> and no
    ///     children, and an inline <c>categories</c> with <c>fixed="yes"</c> and two child categories —
    ///     because those take different paths through the adapter and cost differently. Read against
    ///     <see cref="LoadSmallestServiceDocument"/>, which has three collections and no categories, the
    ///     delta is what a category-bearing collection costs.
    ///     </para>
    /// </remarks>
    private const string Rfc5023ServiceDocument = """
        <?xml version="1.0" encoding='utf-8'?>
        <service xmlns="http://www.w3.org/2007/app"
                 xmlns:atom="http://www.w3.org/2005/Atom">
          <workspace>
            <atom:title>Main Site</atom:title>
            <collection href="http://example.org/blog/main" >
              <atom:title>My Blog Entries</atom:title>
              <categories href="http://example.com/cats/forMain.cats" />
            </collection>
            <collection href="http://example.org/blog/pic" >
              <atom:title>Pictures</atom:title>
              <accept>image/png</accept>
              <accept>image/jpeg</accept>
              <accept>image/gif</accept>
            </collection>
          </workspace>
          <workspace>
            <atom:title>Sidebar Blog</atom:title>
            <collection href="http://example.org/sidebar/list" >
              <atom:title>Remaindered Links</atom:title>
              <accept>application/atom+xml;type=entry</accept>
              <categories fixed="yes">
                <atom:category scheme="http://example.org/extra-cats/" term="joke" />
                <atom:category scheme="http://example.org/extra-cats/" term="serious" />
              </categories>
            </collection>
          </workspace>
        </service>
        """;

    private byte[] smallestServiceDocument = [];
    private byte[] largestServiceDocument = [];
    private byte[][] everyServiceDocument = [];
    private byte[] categoryDocument = [];
    private byte[] serviceDocumentWithCategories = [];
    private byte[] entryDocument = [];
    private byte[] entryWithPublishingControl = [];

    /// <summary>
    /// Reads the corpus and materialises the synthetic documents once.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.smallestServiceDocument = RealWorldCorpus.Read("atompub/odata-v2-odata-svc-service.xml");
        this.largestServiceDocument = RealWorldCorpus.Read("atompub/odata-northwind-v2-service.xml");
        this.everyServiceDocument = RealWorldCorpus.ReadAll("atompub");
        this.categoryDocument = Encoding.UTF8.GetBytes(SyntheticCategoryDocument);
        this.serviceDocumentWithCategories = Encoding.UTF8.GetBytes(Rfc5023ServiceDocument);
        this.entryDocument = FeedCorpus.ReadRealSample("AtomEntryDocument.xml");
        this.entryWithPublishingControl = Encoding.UTF8.GetBytes(SyntheticEntryWithPublishingControl);
    }

    /// <summary>
    /// Loads the smallest real service document, of three collections and 474 bytes.
    /// </summary>
    /// <returns>The number of collections parsed, so the load cannot be elided.</returns>
    /// <remarks>
    ///     The baseline, because it is the floor: whatever this costs is what a service document costs
    ///     before any of its content does. A service document is fetched once per session by an
    ///     editing client, so the fixed cost is the whole story for most callers.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "a. AtomServiceDocument.Load, 3 collections (real)")]
    public int LoadSmallestServiceDocument() => LoadServiceDocument(this.smallestServiceDocument);

    /// <summary>
    /// Loads the largest real service document, of twenty-six collections and 2,644 bytes.
    /// </summary>
    /// <returns>The number of collections parsed.</returns>
    /// <remarks>
    ///     Nearly nine times the collection count of the baseline for six times the bytes. The ratio
    ///     between the two arms is the per-collection cost, and it is the only per-element figure this
    ///     class can produce honestly, because the corpus supplies two sizes and not a curve.
    /// </remarks>
    [Benchmark(Description = "b. AtomServiceDocument.Load, 26 collections (real)")]
    public int LoadLargestServiceDocument() => LoadServiceDocument(this.largestServiceDocument);

    /// <summary>
    /// Loads every real service document in the corpus.
    /// </summary>
    /// <returns>The total number of collections parsed.</returns>
    /// <remarks>
    ///     Four documents from two OData service versions. Two of them are byte-identical in shape and
    ///     differ only in <c>xml:base</c>, which is not variety — the point of the arm is that it
    ///     fails loudly if the corpus is missing rather than quietly measuring an empty set, and that
    ///     it will pick up any document added to <c>atompub/</c> later without a code change.
    /// </remarks>
    [Benchmark(Description = "c. AtomServiceDocument.Load over every real service document")]
    public int LoadEveryServiceDocument()
    {
        int collections = 0;

        foreach (byte[] document in this.everyServiceDocument)
        {
            collections += LoadServiceDocument(document);
        }

        return collections;
    }

    /// <summary>
    /// Loads the RFC 5023 §8.3.3 service document, of three collections, two of which declare
    /// categories.
    /// </summary>
    /// <returns>The total number of category documents parsed, which must be 2.</returns>
    /// <remarks>
    ///     <para>
    ///     Every input of this shape previously threw <see cref="FormatException"/>, so this arm would
    ///     have measured a stack unwind rather than a parse. The return
    ///     value is the count of parsed category documents rather than of collections, because that is
    ///     the number that was structurally zero for every document ever loaded — the loaded
    ///     <see cref="AtomCategoryDocument"/> was built, filled, and then dropped on the floor.
    ///     </para>
    ///     <para>
    ///     Read against <see cref="LoadSmallestServiceDocument"/>: both are small service documents with
    ///     three collections, and this one additionally pays two nested category loads, each of which
    ///     constructs its own <c>SyndicationResourceMetadata</c> and its own
    ///     <c>SyndicationExtensionAdapter</c>. If nested categories are expensive out of proportion to
    ///     their size, that is where it shows.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "d. AtomServiceDocument.Load, 2 collections with categories (RFC 5023 §8.3.3)")]
    public int LoadServiceDocumentWithCategories()
    {
        using MemoryStream stream = new(this.serviceDocumentWithCategories, writable: false);
        AtomServiceDocument document = new();
        document.Load(stream);

        int categories = 0;

        foreach (AtomWorkspace workspace in document.Workspaces)
        {
            foreach (AtomMemberResources collection in workspace.Collections)
            {
                categories += collection.Categories.Count;
            }
        }

        return categories;
    }

    /// <summary>
    /// Loads the synthetic category document, of six categories with all three document attributes
    /// present.
    /// </summary>
    /// <returns>The number of categories parsed.</returns>
    /// <remarks>
    ///     Synthetic input — see <see cref="SyntheticCategoryDocument"/>. There is no public
    ///     corpus of <c>app:categories</c> documents, so this number describes the parser and not the
    ///     web. It is still worth having: <see cref="AtomCategoryDocument"/> is one of the twelve types
    ///     implementing the uniform resource contract and the only one whose <c>Load</c> had never run
    ///     under measurement.
    /// </remarks>
    [Benchmark(Description = "e. AtomCategoryDocument.Load, 6 categories (SYNTHETIC)")]
    public int LoadCategoryDocument()
    {
        using MemoryStream stream = new(this.categoryDocument, writable: false);
        AtomCategoryDocument document = new();
        document.Load(stream);

        return document.Categories.Count;
    }

    /// <summary>
    /// Loads the repository's sample entry through <see cref="AtomEntry"/>, as the control.
    /// </summary>
    /// <returns>The number of links parsed.</returns>
    /// <remarks>
    ///     Paired with <see cref="LoadEntryAsResource"/> over the same bytes. This arm is here only so
    ///     that the next one has something to be subtracted from; on its own it measures Atom entry
    ///     parsing, which <c>FormatBreadthLoadBenchmarks</c> already covers.
    /// </remarks>
    [Benchmark(Description = "f. AtomEntry.Load, sample entry (control for g)")]
    public int LoadEntryAsPlainEntry()
    {
        using MemoryStream stream = new(this.entryDocument, writable: false);
        AtomEntry entry = new();
        entry.Load(stream);

        return entry.Links.Count;
    }

    /// <summary>
    /// Loads the same bytes through <see cref="AtomEntryResource"/>, which adds the publishing
    /// projection.
    /// </summary>
    /// <returns>The number of links parsed.</returns>
    /// <remarks>
    ///     The difference from <see cref="LoadEntryAsPlainEntry"/> is one override that runs two
    ///     <c>FindExtension</c> calls. Each copies <c>Extensions</c> into a new <c>List&lt;T&gt;</c>
    ///     and scans it with a predicate, and this document has no <c>app:</c> extensions to find —
    ///     so the delta here is the price of asking, paid by every entry an editing client reads
    ///     whether or not the server sent any publishing metadata. Two list copies over an empty
    ///     collection should be very close to free; if this arm is measurably above the control, the
    ///     copies are not free and <c>FindExtension</c> is worth changing.
    /// </remarks>
    [Benchmark(Description = "g. AtomEntryResource.Load, same bytes (finds nothing)")]
    public int LoadEntryAsResource()
    {
        using MemoryStream stream = new(this.entryDocument, writable: false);
        AtomEntryResource entry = new();
        entry.Load(stream);

        return entry.Links.Count;
    }

    /// <summary>
    /// Loads a synthetic entry carrying both <c>app:edited</c> and <c>app:control</c>.
    /// </summary>
    /// <returns>Whether the entry parsed as a draft, which must be <see langword="true"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     Synthetic input — see <see cref="SyntheticEntryWithPublishingControl"/>. The arm
    ///     exists because <see cref="LoadEntryAsResource"/> cannot fail in the interesting direction:
    ///     a document with no <c>app:</c> namespace gives extension auto-detection nothing to detect
    ///     and both <c>FindExtension</c> calls nothing to find, so any change to either would measure
    ///     as free over it. This document declares the namespace and carries both elements, so the
    ///     detection runs, the extensions are constructed and attached, and both lookups match.
    ///     </para>
    ///     <para>
    ///     Returning <c>IsDraft</c> rather than a count is deliberate: it is <see langword="true"/>
    ///     only if the whole chain worked — namespace declared, extension detected, context loaded,
    ///     <c>FindExtension</c> matched, property projected. A <see langword="false"/> in the results
    ///     table means the arm measured a load that quietly did nothing, and the number above it is
    ///     worthless.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "h. AtomEntryResource.Load, entry WITH app:control (SYNTHETIC)")]
    public bool LoadEntryWithPublishingControl()
    {
        using MemoryStream stream = new(this.entryWithPublishingControl, writable: false);
        AtomEntryResource entry = new();
        entry.Load(stream);

        return entry.IsDraft;
    }

    private static int LoadServiceDocument(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        AtomServiceDocument service = new();
        service.Load(stream);

        int collections = 0;
        foreach (AtomWorkspace workspace in service.Workspaces)
        {
            collections += workspace.Collections.Count;
        }

        return collections;
    }
}