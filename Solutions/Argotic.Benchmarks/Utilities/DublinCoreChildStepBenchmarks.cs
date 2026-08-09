using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Core;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Utilities;

/// <summary>
/// Measures whether the 55-entry <see cref="FrozenDictionary{TKey, TValue}"/> that resolves Dublin
/// Core term names is worth retaining, against removing it entirely.
/// </summary>
/// <remarks>
/// <para>
/// The subject is <c>ChildSteps</c> in
/// <c>Argotic.Extensions/Core/DublinCore/DublinCoreMetadataTerms/DublinCoreMetadataTermsSyndicationExtensionContext.cs</c>:
/// a <c>private static readonly FrozenDictionary&lt;string, XPathStep&gt;</c> of 55 entries with
/// <see cref="string"/> keys, constructed through <c>ToFrozenDictionary</c> without a comparer
/// argument and therefore using the default ordinal, case-sensitive comparer. It has one read site,
/// the indexer inside <c>SelectSingle</c>, and <c>SelectSingle</c> is called from 55 sites across the
/// six load groups. Every entity that declares the <c>dcterms:</c> prefix consequently performs 55
/// lookups, whether or not it carries any Dublin Core term.
/// </para>
/// <para>
/// The dictionary is a candidate for removal rather than for replacement by a faster structure,
/// because all 55 call sites pass a string literal such as
/// <c>SelectSingle(source, "dcterms:abstract", manager)</c>. The lookup hashes a compile-time constant
/// to recover a prefix and a local name already known to the compiler. The change being priced is
/// therefore widening <c>SelectSingle</c> to accept the prefix and local name directly, and deleting
/// the dictionary, the <c>XPathStep</c> record struct and <c>BuildChildSteps</c> with it.
/// </para>
/// <para>
/// The lookup arms are a reconstruction rather than the shipped members. <c>ChildSteps</c>,
/// <c>XPathStep</c> and <c>SelectSingle</c> are <c>private</c>, and although <c>Argotic.Common</c>
/// grants <c>InternalsVisibleTo</c> to this assembly, that attribute reaches <c>internal</c> members
/// and not <c>private</c> ones. <see cref="Expressions"/>, <see cref="ChildStep"/> and
/// <see cref="Frozen"/> therefore mirror the product's literal array, record struct and construction
/// respectively, including the omitted comparer argument. A change to the product's 55 terms does not
/// propagate here automatically. <see cref="ContextLoad"/> is the only arm that executes the shipped
/// code.
/// </para>
/// <para>
/// Each arm is designed to refute a specific proposition.
/// </para>
/// <list type="number">
///   <item><description>
///   Frozen, as shipped. The baseline against which the remaining arms are read.
///   </description></item>
///   <item><description>
///   Dictionary with the same comparer. Isolates the contribution of freezing at 55 entries while
///   holding the structure's shape constant. A tie with the baseline indicates that freezing confers
///   no advantage at this size.
///   </description></item>
///   <item><description>
///   A <c>switch</c> over the 55 literals. Refutes the proposition that a lookup is unavoidable and
///   only its implementation is open. This is not a jump table, which requires integer cases; the
///   emitted code was inspected rather than assumed, and the assembly contains no
///   <c>ComputeStringHash</c> helper, so dispatch proceeds by length and by discriminating character.
///   Unlike both dictionary arms it never hashes the key, and this arm measures whether reading a few
///   characters is cheaper than hashing the whole string.
///   </description></item>
///   <item><description>
///   No lookup, with prefix and local name supplied as literals. This is the decisive arm. It refutes
///   the entire family of changes that make the dictionary faster: a clear win indicates that the
///   correct change is no structure rather than a better one.
///   </description></item>
///   <item><description>
///   <c>context.Load</c>, the shipped public path. Not a competitor but the denominator: a saving of
///   a given number of nanoseconds per entity cannot be interpreted until it is divided by the cost
///   of an entity, and measuring the two in separate runs would compare numbers taken in different
///   thermal states. It also serves as a control, because it performs identical work at every
///   <see cref="Probe"/> value; a row that is not flat across that axis indicates the machine changed
///   state during the run, which invalidates every timing in the table.
///   </description></item>
/// </list>
/// <para>
/// The <see cref="Probe"/> axis exists for two reasons. The <c>entity</c> value is the realistic unit,
/// treating all 55 lookups as one operation, because that is the cost a single entity bearing the
/// <c>dcterms:</c> prefix incurs; a per-lookup figure omits the factor of 55. The single-key values
/// determine whether the identity of the key is observable. The expectation, which the run may refute,
/// is that <c>first</c>, <c>middle</c> and <c>last</c> do not differ meaningfully for any of the three
/// structures, none of which is ordered by insertion: a frozen string dictionary buckets by length and
/// by a discriminating character slice, a <see cref="Dictionary{TKey, TValue}"/> buckets by hash, and
/// the <c>switch</c> dispatches on computed characters. Those three rows differ only by key length and
/// hash distribution. The structurally distinct value is <c>miss</c>, where a length or hash bucket can
/// decline early.
/// </para>
/// <para>
/// Three measurement characteristics govern how the results are to be read.
/// </para>
/// <list type="bullet">
///   <item><description>
///   The sink imposes a floor common to all four lookup arms. <see cref="Consume"/> is marked
///   <see cref="MethodImplOptions.NoInlining"/> deliberately: if inlined, the no-lookup arm's call
///   folds to a constant and the arm measures nothing. That outcome would be an artefact of the
///   benchmark rather than a property of the product, which passes those two strings to
///   <c>LookupNamespace</c> and <c>SelectChildren</c>, neither of which can fold. The floor is
///   identical in every arm and therefore cancels in the difference between arms. Read the absolute
///   difference rather than the ratio, which the floor compresses, and which the product would
///   compress further through <c>SelectChildren</c>.
///   </description></item>
///   <item><description>
///   The lookup arms pay an array element load that the product does not. Keys are held in a field
///   array so that the just-in-time compiler cannot fold them, whereas the product reads a literal at
///   each of its 55 call sites and performs no bounds check. The measured difference between the
///   frozen and no-lookup arms is therefore an upper bound on the saving available in the product, by
///   one array element load per key. All three lookup arms pay it, so they remain exactly comparable
///   to one another.
///   </description></item>
///   <item><description>
///   The lookup arms use <c>TryGetValue</c> where the shipped code uses the indexer. The indexer
///   throws on a miss, so the <c>miss</c> value could not exist under the shipped form; nor can a miss
///   arise in the product, where every key is a literal drawn from the same 55. Both forms resolve
///   through the same lookup, and the difference is common to the arms.
///   </description></item>
/// </list>
/// <para>
/// Removal is justified when the <c>entity</c> row places the no-lookup arm below the frozen arm by a
/// margin that both exceeds the run's error bars and forms a non-trivial fraction of the
/// <see cref="ContextLoad"/> row in the same table. A difference that is real but amounts to a
/// fraction of one percent of a whole entity load indicates that the dictionary is harmless and that
/// the 55 call sites should be left unchanged.
/// </para>
/// </remarks>
[BenchmarkCategory("utilities", "frozen", "dublincore")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class DublinCoreChildStepBenchmarks
{
    /// <summary>
    /// The namespace the <c>dcterms</c> prefix binds to, as declared at
    /// <c>DublinCoreMetadataTermsSyndicationExtension.cs:29</c>.
    /// </summary>
    private const string DublinCoreTermsNamespace = "http://purl.org/dc/terms/";

    /// <summary>
    /// The key that is deliberately not in the set, for the miss column.
    /// </summary>
    private const string AbsentExpression = "dcterms:notATerm";

    // A transcription of the literal array at DublinCoreMetadataTermsSyndicationExtensionContext.cs:1425-1482,
    // in declaration order. Not read from the product - the array is a local inside a private method.
    private static readonly string[] Expressions =
    [
        "dcterms:abstract",
        "dcterms:accessRights",
        "dcterms:accrualMethod",
        "dcterms:accrualPeriodicity",
        "dcterms:accrualPolicy",
        "dcterms:alternative",
        "dcterms:audience",
        "dcterms:available",
        "dcterms:bibliographicCitation",
        "dcterms:conformsTo",
        "dcterms:contributor",
        "dcterms:coverage",
        "dcterms:created",
        "dcterms:creator",
        "dcterms:date",
        "dcterms:dateAccepted",
        "dcterms:dateCopyrighted",
        "dcterms:dateSubmitted",
        "dcterms:description",
        "dcterms:educationLevel",
        "dcterms:extent",
        "dcterms:format",
        "dcterms:hasFormat",
        "dcterms:hasPart",
        "dcterms:hasVersion",
        "dcterms:identifier",
        "dcterms:instructionalMethod",
        "dcterms:isFormatOf",
        "dcterms:isPartOf",
        "dcterms:isReferencedBy",
        "dcterms:isReplacedBy",
        "dcterms:isRequiredBy",
        "dcterms:isVersionOf",
        "dcterms:issued",
        "dcterms:language",
        "dcterms:license",
        "dcterms:mediator",
        "dcterms:medium",
        "dcterms:modified",
        "dcterms:provenance",
        "dcterms:publisher",
        "dcterms:references",
        "dcterms:relation",
        "dcterms:replaces",
        "dcterms:requires",
        "dcterms:rights",
        "dcterms:rightsHolder",
        "dcterms:source",
        "dcterms:spatial",
        "dcterms:subject",
        "dcterms:tableOfContents",
        "dcterms:temporal",
        "dcterms:title",
        "dcterms:type",
        "dcterms:valid",
    ];

    // Mirrors :1484-1486 exactly, including the absent comparer argument - ToFrozenDictionary with no
    // comparer means EqualityComparer<string>.Default, which is ordinal and case-sensitive.
    private static readonly FrozenDictionary<string, ChildStep> Frozen =
        Expressions.ToFrozenDictionary(static text => text, Split);

    // The control. Same keys, same default comparer, unfrozen.
    private static readonly Dictionary<string, ChildStep> Hashed =
        Expressions.ToDictionary(static text => text, Split);

    private string[] probes = [];
    private string probePrefix = string.Empty;
    private string probeLocalName = string.Empty;
    private bool wholeEntity;
    private XPathNavigator entry = null!;
    private XmlNamespaceManager namespaces = null!;

    /// <summary>
    /// Gets or sets which lookup or lookups one operation performs.
    /// </summary>
    /// <value>
    /// <c>entity</c> for all 55 in call order — the realistic unit — or <c>first</c>, <c>middle</c>,
    /// <c>last</c> or <c>miss</c> for a single lookup.
    /// </value>
    [Params("entity", "first", "middle", "last", "miss")]
    public string Probe { get; set; } = "entity";

    /// <summary>
    /// Selects the probe keys, and builds the entity the shipped path is measured against.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        string single = this.Probe switch
        {
            "first" => Expressions[0],
            "middle" => Expressions[Expressions.Length / 2],
            "last" => Expressions[^1],
            _ => AbsentExpression,
        };

        this.wholeEntity = this.Probe == "entity";
        this.probes = this.wholeEntity ? Expressions : [single];

        ChildStep step = Split(single);
        this.probePrefix = step.Prefix!;
        this.probeLocalName = step.LocalName!;

        // An entity carrying every term the context looks for. The worst realistic case, and the one
        // that makes the denominator comparable to the numerator: 55 lookups, 55 elements found.
        StringBuilder builder = new();
        builder.Append("""<?xml version="1.0" encoding="utf-8"?><item xmlns:dcterms=""")
            .Append('"').Append(DublinCoreTermsNamespace).Append('"')
            .Append('>');
        foreach (string expression in Expressions)
        {
            string localName = Split(expression).LocalName!;
            builder.Append('<').Append(expression).Append('>')
                .Append(SampleValue(localName))
                .Append("</").Append(expression).Append('>');
        }

        builder.Append("</item>");

        XPathNavigator document = SyndicationEncodingUtility.CreateSafeNavigator(builder.ToString());
        _ = document.MoveToChild(XPathNodeType.Element);
        this.entry = document;
        this.namespaces = new XmlNamespaceManager(document.NameTable);
        this.namespaces.AddNamespace("dcterms", DublinCoreTermsNamespace);
    }

    /// <summary>
    /// What the library does today: a frozen dictionary keyed by the expression text.
    /// </summary>
    /// <returns>A running total, so the resolved step cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "Frozen (as shipped)")]
    public int FrozenLookup()
    {
        int total = 0;
        foreach (string expression in this.probes)
        {
            _ = Frozen.TryGetValue(expression, out ChildStep step);
            total += Consume(step.Prefix, step.LocalName);
        }

        return total;
    }

    /// <summary>
    /// The same 55 entries in an ordinary hash dictionary, isolating what freezing buys at this size.
    /// </summary>
    /// <returns>A running total, so the resolved step cannot be elided.</returns>
    [Benchmark(Description = "Dictionary, same comparer")]
    public int DictionaryLookup()
    {
        int total = 0;
        foreach (string expression in this.probes)
        {
            _ = Hashed.TryGetValue(expression, out ChildStep step);
            total += Consume(step.Prefix, step.LocalName);
        }

        return total;
    }

    /// <summary>
    /// No structure: a switch expression over the same 55 literals.
    /// </summary>
    /// <returns>A running total, so the resolved step cannot be elided.</returns>
    [Benchmark(Description = "switch over the 55 literals")]
    public int SwitchLookup()
    {
        int total = 0;
        foreach (string expression in this.probes)
        {
            ChildStep step = StepBySwitch(expression);
            total += Consume(step.Prefix, step.LocalName);
        }

        return total;
    }

    /// <summary>
    /// No lookup at all: the prefix and local name handed over as literals, the dictionary deleted.
    /// </summary>
    /// <returns>A running total, matching what the other arms accumulate.</returns>
    /// <remarks>
    ///     <para>
    ///     This is the shape the product takes if the change is made — 55 call sites, each passing two
    ///     constants, and no <c>ChildSteps</c>, <c>XPathStep</c> or <c>BuildChildSteps</c> left in the
    ///     file. The entity path is written straight-line rather than as a loop for exactly that
    ///     reason: putting the 55 pairs in an array to iterate them would reintroduce the indirection
    ///     the change removes.
    ///     </para>
    ///     <para>
    ///     The single-key path reads two fields rather than two literals, because one benchmark method
    ///     cannot be both straight-line and parameterised. That costs two field loads, and it makes the
    ///     single-key columns a fair like-for-like against the other arms, which also load their key
    ///     from a field.
    ///     </para>
    ///     <para>
    ///     In the <c>miss</c> column this arm is measuring the absence of the question: with no
    ///     dictionary there is no such thing as a missing key, so it does the same work it does on a
    ///     hit.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "no lookup: prefix and local name as literals")]
    public int NoLookup() =>
        this.wholeEntity ? ConsumeEveryTerm() : Consume(this.probePrefix, this.probeLocalName);

    /// <summary>
    /// Loads an entity carrying all 55 terms through the shipped public path, providing both the
    /// denominator and the control.
    /// </summary>
    /// <returns><see langword="true"/> if the context loaded, which it does for this entity.</returns>
    /// <remarks>
    ///     The only arm that executes the product's <c>ChildSteps</c> through its private
    ///     <c>SelectSingle</c>. It is deliberately insensitive to <see cref="Probe"/> and performs
    ///     identical work in every column, so a row that is not flat across that axis indicates a
    ///     change in machine state rather than in the code, and no timing claim should be made from
    ///     that run.
    /// </remarks>
    [Benchmark(Description = "context.Load, all 55 terms present (denominator)")]
    public bool ContextLoad() =>
        new DublinCoreMetadataTermsSyndicationExtensionContext().Load(this.entry, this.namespaces);

    /// <summary>
    /// Splits an expression into the prefix and local name it selects on.
    /// </summary>
    /// <param name="text">The expression text, of the form <c>prefix:localName</c>.</param>
    /// <returns>The step the expression represents.</returns>
    /// <remarks>
    ///     Transcribed from the value selector at
    ///     <c>DublinCoreMetadataTermsSyndicationExtensionContext.cs:1486</c>. It runs once per entry at
    ///     construction and in <see cref="Setup"/>, never inside a measured operation.
    /// </remarks>
    private static ChildStep Split(string text)
    {
        int separator = text.IndexOf(':', StringComparison.Ordinal);

        return new ChildStep(text[..separator], text[(separator + 1)..]);
    }

    /// <summary>
    /// A value the context can actually parse for the given term.
    /// </summary>
    /// <param name="localName">The term's local name.</param>
    /// <returns>Element text that lands in the corresponding property.</returns>
    /// <remarks>
    ///     Nine of the 55 terms are not strings — seven <see cref="DateTime"/>, one
    ///     <see cref="System.Globalization.CultureInfo"/>, one <c>DublinCoreTypeVocabularies</c> —
    ///     and filler text makes the
    ///     denominator lie in two directions. It skips the parse-and-assign work those nine would do,
    ///     and <c>dcterms:language</c> is worse than that: the context builds
    ///     <c>new CultureInfo(value)</c> inside a <c>try</c>/<c>catch</c> that traces a warning
    ///     (<c>DublinCoreMetadataTermsSyndicationExtensionContext.cs:1898-1909</c>), so an unparseable
    ///     language throws and catches once per operation and writes to <c>Trace</c>. That is
    ///     microseconds of noise on top of the number the lookup delta is meant to be divided by, and
    ///     the harness's <c>ExceptionDiagnoser</c> column would show it. Every value here parses, so
    ///     the exception column should read zero — if it does not, this method has drifted from what
    ///     the context accepts.
    /// </remarks>
    private static string SampleValue(string localName) => localName switch
    {
        "created" or "date" or "dateAccepted" or "dateCopyrighted" or "dateSubmitted" or "issued"
            or "modified" => "2026-08-06T12:00:00Z",
        "language" => "en-GB",
        "type" => "Text",
        _ => localName + " value",
    };

    /// <summary>
    /// Stands in for what the resolved step is handed to.
    /// </summary>
    /// <param name="prefix">The namespace prefix, or <see langword="null"/> when the lookup missed.</param>
    /// <param name="localName">The element name, or <see langword="null"/> when the lookup missed.</param>
    /// <returns>A value derived from both strings, so neither can be discarded.</returns>
    /// <remarks>
    ///     <c>NoInlining</c> is load-bearing. Inlined, the literal arm folds to a constant and measures
    ///     nothing, which would be an artefact of the sink rather than a fact about the change: the
    ///     product passes these two strings to <c>manager.LookupNamespace</c> and
    ///     <c>SelectChildren</c>, neither of which folds. Every arm pays exactly one of these calls per
    ///     probe, so the floor cancels when arms are subtracted and only distorts their ratio.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Consume(string? prefix, string? localName) =>
        (prefix?.Length ?? 0) + (localName?.Length ?? 0);

    /// <summary>
    /// The whole entity with the dictionary deleted: 55 call sites, each passing two constants.
    /// </summary>
    /// <returns>A running total, matching what the other arms accumulate.</returns>
    private static int ConsumeEveryTerm() =>
        Consume("dcterms", "abstract") +
        Consume("dcterms", "accessRights") +
        Consume("dcterms", "accrualMethod") +
        Consume("dcterms", "accrualPeriodicity") +
        Consume("dcterms", "accrualPolicy") +
        Consume("dcterms", "alternative") +
        Consume("dcterms", "audience") +
        Consume("dcterms", "available") +
        Consume("dcterms", "bibliographicCitation") +
        Consume("dcterms", "conformsTo") +
        Consume("dcterms", "contributor") +
        Consume("dcterms", "coverage") +
        Consume("dcterms", "created") +
        Consume("dcterms", "creator") +
        Consume("dcterms", "date") +
        Consume("dcterms", "dateAccepted") +
        Consume("dcterms", "dateCopyrighted") +
        Consume("dcterms", "dateSubmitted") +
        Consume("dcterms", "description") +
        Consume("dcterms", "educationLevel") +
        Consume("dcterms", "extent") +
        Consume("dcterms", "format") +
        Consume("dcterms", "hasFormat") +
        Consume("dcterms", "hasPart") +
        Consume("dcterms", "hasVersion") +
        Consume("dcterms", "identifier") +
        Consume("dcterms", "instructionalMethod") +
        Consume("dcterms", "isFormatOf") +
        Consume("dcterms", "isPartOf") +
        Consume("dcterms", "isReferencedBy") +
        Consume("dcterms", "isReplacedBy") +
        Consume("dcterms", "isRequiredBy") +
        Consume("dcterms", "isVersionOf") +
        Consume("dcterms", "issued") +
        Consume("dcterms", "language") +
        Consume("dcterms", "license") +
        Consume("dcterms", "mediator") +
        Consume("dcterms", "medium") +
        Consume("dcterms", "modified") +
        Consume("dcterms", "provenance") +
        Consume("dcterms", "publisher") +
        Consume("dcterms", "references") +
        Consume("dcterms", "relation") +
        Consume("dcterms", "replaces") +
        Consume("dcterms", "requires") +
        Consume("dcterms", "rights") +
        Consume("dcterms", "rightsHolder") +
        Consume("dcterms", "source") +
        Consume("dcterms", "spatial") +
        Consume("dcterms", "subject") +
        Consume("dcterms", "tableOfContents") +
        Consume("dcterms", "temporal") +
        Consume("dcterms", "title") +
        Consume("dcterms", "type") +
        Consume("dcterms", "valid");

    /// <summary>
    /// The dispatch a 55-case switch expression compiles to, over the same keys.
    /// </summary>
    /// <param name="expression">The expression text to resolve.</param>
    /// <returns>The step the expression represents, or the default value if it is not one of the 55.</returns>
    private static ChildStep StepBySwitch(string expression) => expression switch
    {
        "dcterms:abstract" => new ChildStep("dcterms", "abstract"),
        "dcterms:accessRights" => new ChildStep("dcterms", "accessRights"),
        "dcterms:accrualMethod" => new ChildStep("dcterms", "accrualMethod"),
        "dcterms:accrualPeriodicity" => new ChildStep("dcterms", "accrualPeriodicity"),
        "dcterms:accrualPolicy" => new ChildStep("dcterms", "accrualPolicy"),
        "dcterms:alternative" => new ChildStep("dcterms", "alternative"),
        "dcterms:audience" => new ChildStep("dcterms", "audience"),
        "dcterms:available" => new ChildStep("dcterms", "available"),
        "dcterms:bibliographicCitation" => new ChildStep("dcterms", "bibliographicCitation"),
        "dcterms:conformsTo" => new ChildStep("dcterms", "conformsTo"),
        "dcterms:contributor" => new ChildStep("dcterms", "contributor"),
        "dcterms:coverage" => new ChildStep("dcterms", "coverage"),
        "dcterms:created" => new ChildStep("dcterms", "created"),
        "dcterms:creator" => new ChildStep("dcterms", "creator"),
        "dcterms:date" => new ChildStep("dcterms", "date"),
        "dcterms:dateAccepted" => new ChildStep("dcterms", "dateAccepted"),
        "dcterms:dateCopyrighted" => new ChildStep("dcterms", "dateCopyrighted"),
        "dcterms:dateSubmitted" => new ChildStep("dcterms", "dateSubmitted"),
        "dcterms:description" => new ChildStep("dcterms", "description"),
        "dcterms:educationLevel" => new ChildStep("dcterms", "educationLevel"),
        "dcterms:extent" => new ChildStep("dcterms", "extent"),
        "dcterms:format" => new ChildStep("dcterms", "format"),
        "dcterms:hasFormat" => new ChildStep("dcterms", "hasFormat"),
        "dcterms:hasPart" => new ChildStep("dcterms", "hasPart"),
        "dcterms:hasVersion" => new ChildStep("dcterms", "hasVersion"),
        "dcterms:identifier" => new ChildStep("dcterms", "identifier"),
        "dcterms:instructionalMethod" => new ChildStep("dcterms", "instructionalMethod"),
        "dcterms:isFormatOf" => new ChildStep("dcterms", "isFormatOf"),
        "dcterms:isPartOf" => new ChildStep("dcterms", "isPartOf"),
        "dcterms:isReferencedBy" => new ChildStep("dcterms", "isReferencedBy"),
        "dcterms:isReplacedBy" => new ChildStep("dcterms", "isReplacedBy"),
        "dcterms:isRequiredBy" => new ChildStep("dcterms", "isRequiredBy"),
        "dcterms:isVersionOf" => new ChildStep("dcterms", "isVersionOf"),
        "dcterms:issued" => new ChildStep("dcterms", "issued"),
        "dcterms:language" => new ChildStep("dcterms", "language"),
        "dcterms:license" => new ChildStep("dcterms", "license"),
        "dcterms:mediator" => new ChildStep("dcterms", "mediator"),
        "dcterms:medium" => new ChildStep("dcterms", "medium"),
        "dcterms:modified" => new ChildStep("dcterms", "modified"),
        "dcterms:provenance" => new ChildStep("dcterms", "provenance"),
        "dcterms:publisher" => new ChildStep("dcterms", "publisher"),
        "dcterms:references" => new ChildStep("dcterms", "references"),
        "dcterms:relation" => new ChildStep("dcterms", "relation"),
        "dcterms:replaces" => new ChildStep("dcterms", "replaces"),
        "dcterms:requires" => new ChildStep("dcterms", "requires"),
        "dcterms:rights" => new ChildStep("dcterms", "rights"),
        "dcterms:rightsHolder" => new ChildStep("dcterms", "rightsHolder"),
        "dcterms:source" => new ChildStep("dcterms", "source"),
        "dcterms:spatial" => new ChildStep("dcterms", "spatial"),
        "dcterms:subject" => new ChildStep("dcterms", "subject"),
        "dcterms:tableOfContents" => new ChildStep("dcterms", "tableOfContents"),
        "dcterms:temporal" => new ChildStep("dcterms", "temporal"),
        "dcterms:title" => new ChildStep("dcterms", "title"),
        "dcterms:type" => new ChildStep("dcterms", "type"),
        "dcterms:valid" => new ChildStep("dcterms", "valid"),
        _ => default,
    };

    /// <summary>
    /// A single child-axis step: the namespace prefix to resolve, and the element name to match.
    /// </summary>
    /// <param name="Prefix">The namespace prefix, or <see langword="null"/> when a lookup missed.</param>
    /// <param name="LocalName">
    /// The local name of the child element, or <see langword="null"/> when a lookup missed.
    /// </param>
    /// <remarks>
    ///     Mirrors the private <c>XPathStep</c> at
    ///     <c>DublinCoreMetadataTermsSyndicationExtensionContext.cs:1417</c>. The one deliberate
    ///     difference is nullability: the product's members are non-nullable because the indexer at
    ///     <c>:1498</c> throws rather than returning <c>default</c>, and a miss column needs a
    ///     representable miss. Nullable annotations do not change codegen, so the measurement is
    ///     unaffected.
    /// </remarks>
    private readonly record struct ChildStep(string? Prefix, string? LocalName);
}