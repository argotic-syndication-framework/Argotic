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
/// Asks whether the largest <see cref="FrozenDictionary{TKey, TValue}"/> in the library should exist
/// at all.
/// </summary>
/// <remarks>
/// <para>
/// <b>The subject.</b>
/// <c>Solutions/Argotic.Extensions/Core/DublinCore/DublinCoreMetadataTerms/DublinCoreMetadataTermsSyndicationExtensionContext.cs:27</c>
/// declares <c>private static readonly FrozenDictionary&lt;string, XPathStep&gt; ChildSteps =
/// BuildChildSteps();</c> — <b>55 entries</b>, <c>string</c> keys, built by <c>ToFrozenDictionary</c>
/// with no comparer overload, so the <b>default ordinal, case-sensitive</b> comparer (<c>:1484-1486</c>).
/// It has exactly one read site, the indexer at <c>:1498</c> inside <c>SelectSingle</c>, and
/// <c>SelectSingle</c> is called from <b>55 sites</b> across <c>LoadGroup1..6</c>. So every entity that
/// declares <c>dcterms:</c> pays <b>55 lookups</b>, whether or not it carries a single Dublin Core term.
/// </para>
/// <para>
/// <b>Why it is a candidate for deletion rather than for a faster structure.</b> Every one of those 55
/// call sites passes a <em>string literal</em>: <c>SelectSingle(source, "dcterms:abstract", manager)</c>.
/// The dictionary hashes a compile-time constant to recover a prefix and a local name that the
/// compiler already knew. The change this benchmark exists to price is not "swap the structure" but
/// "widen <c>SelectSingle</c> to <c>(source, prefix, localName, manager)</c> and delete the dictionary,
/// the <c>XPathStep</c> record struct and <c>BuildChildSteps</c> with it".
/// </para>
/// <para>
/// <b>This is a reconstruction, and it has to be.</b> <c>ChildSteps</c>, <c>XPathStep</c> (<c>:1417</c>)
/// and <c>SelectSingle</c> (<c>:1496</c>) are all <c>private</c> members of the context class.
/// <c>Argotic.Common</c> grants <c>InternalsVisibleTo</c> to this assembly
/// (<c>Argotic.Common.csproj:19,23,24</c>), but <c>InternalsVisibleTo</c> reaches <c>internal</c>, not
/// <c>private</c> — no attribute makes a private field visible to another assembly. <see cref="Expressions"/>
/// therefore mirrors the literal array at <c>:1425-1482</c>, <see cref="ChildStep"/> mirrors the record
/// struct at <c>:1417</c>, and <see cref="Frozen"/> mirrors the construction at <c>:1484-1486</c>
/// including the absent comparer argument. <b>If the product's 55 terms change, this file does not
/// follow automatically.</b> The one arm that does execute the shipped code is
/// <see cref="ContextLoad"/>, which goes through the real private path.
/// </para>
/// <para>
/// <b>The arms, and what each can refute.</b>
/// </para>
/// <list type="number">
///   <item><description><b>Frozen (as shipped)</b> — the baseline. Nothing to refute; it is the
///   number the others are read against.</description></item>
///   <item><description><b>Dictionary, same comparer</b> — isolates what <em>freezing</em> buys at 55
///   entries, holding the structure's shape constant. If this ties the baseline, the freeze is
///   decoration at this size and the audit's "was Frozen right?" framing is answered
///   negatively.</description></item>
///   <item><description><b>switch over the 55 literals</b> — refutes "a lookup is unavoidable, so
///   pick the best one". It is not a jump table: a jump table needs integer cases. What Roslyn
///   actually emitted for these 55 cases was checked rather than assumed —
///   <c>strings Solutions/Argotic.Benchmarks/bin/Release/net10.0/Argotic.Benchmarks.dll | grep
///   ComputeStringHash</c> returns nothing, so <b>no hash helper was generated</b> and the dispatch is
///   by length and by discriminating character. That is the interesting case: unlike both dictionary
///   arms it never hashes the key. Whether reading a few characters beats hashing the whole string is
///   what this arm measures.</description></item>
///   <item><description><b>no lookup, prefix and local name as literals</b> — <b>the arm that
///   matters.</b> It refutes the whole family of "make the dictionary faster" changes. If it wins
///   decisively the right change is not a better structure, it is no structure.</description></item>
///   <item><description><b>context.Load (the shipped public path)</b> — not a competitor; the
///   <em>denominator</em>. A saving of N nanoseconds per entity is meaningless until it is divided by
///   what an entity costs, and measuring the two in separate runs would compare numbers taken in
///   different thermal states — the failure <c>docs/build-warnings.md</c> §2.23 records. It is also a
///   control: it does the same work for every <see cref="Probe"/> value, so if its row is not flat
///   across the axis, the machine moved during the run and no timing in the table is
///   safe.</description></item>
/// </list>
/// <para>
/// <b>The <see cref="Probe"/> axis.</b> <c>entity</c> is the realistic unit — all 55 lookups as one
/// operation, because that is what one <c>dcterms:</c>-bearing entity actually costs, and a per-lookup
/// nanosecond figure invites the reader to forget the ×55. The single-key values exist to answer
/// whether <em>which</em> key is looked up is observable. The prediction, which the run can refute, is
/// that <c>first</c>, <c>middle</c> and <c>last</c> are <b>not</b> meaningfully different for any of
/// these three structures: none of them is ordered by insertion. A frozen string dictionary buckets by
/// length and by a discriminating character slice, a <c>Dictionary</c> buckets by hash, and the switch
/// dispatches on computed characters — so those three rows differ only by key length and hash
/// distribution, not by position. The row that <em>is</em> structurally different is <c>miss</c>, where
/// a length bucket or a hash bucket can decline early.
/// </para>
/// <para>
/// <b>Three measurement caveats, stated because they change how the numbers should be read.</b>
/// </para>
/// <list type="bullet">
///   <item><description><b>The sink is a floor common to all four lookup arms.</b>
///   <see cref="Consume"/> is <c>NoInlining</c> on purpose. Inlined, the no-lookup arm's
///   <c>Consume("dcterms", "abstract")</c> folds to a constant and the arm measures nothing — which is
///   a benchmark artefact, not a product truth, because the product hands those two strings to
///   <c>manager.LookupNamespace</c> and <c>SelectChildren</c>, which cannot fold. The call floor is
///   identical in every arm, so it cancels in the <em>difference</em> between arms. <b>Read the
///   absolute delta, not the ratio</b> — the ratio column is compressed by the floor, and in the
///   product it would be compressed far further by <c>SelectChildren</c>.</description></item>
///   <item><description><b>The lookup arms pay an array load the product does not.</b> Keys are held
///   in a field array so the JIT cannot fold them, per the rule that a benchmark reading a literal at
///   the call site measures constant propagation. The product reads a literal at each of its 55 call
///   sites and pays no bounds check. So the measured Frozen − NoLookup delta is an <b>upper bound</b>
///   on the product saving, by one array element load per key. The three lookup arms all pay it, so
///   they compare to each other exactly.</description></item>
///   <item><description><b>The lookup arms use <c>TryGetValue</c>; the shipped code uses the
///   indexer.</b> <c>ChildSteps[expression]</c> at <c>:1498</c> would throw on a miss, so the
///   <c>miss</c> column could not exist at all under the shipped form — and it cannot arise in the
///   product, where every key is a literal drawn from the same 55. Both resolve through the same
///   lookup, and the difference is arm-common.</description></item>
/// </list>
/// <para>
/// <b>What decides the case.</b> Deletion is justified when the <c>entity</c> row shows the no-lookup
/// arm below the frozen arm by an amount that is (a) larger than the run's error bars and (b) a
/// non-trivial fraction of the <see cref="ContextLoad"/> row in the same table. If the delta is real
/// but is a fraction of a percent of a whole entity load, the honest conclusion is that the dictionary
/// is harmless and the 55 call sites should be left alone.
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
    /// <c>DublinCoreMetadataTermsSyndicationExtension.cs:34</c>.
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
    /// The shipped public path over an entity carrying all 55 terms: the denominator, and the control.
    /// </summary>
    /// <returns><b>true</b> if the context loaded, which it does for this entity.</returns>
    /// <remarks>
    ///     The only arm that executes the real <c>ChildSteps</c>, through the real private
    ///     <c>SelectSingle</c>. It is deliberately insensitive to <see cref="Probe"/>: it does the same
    ///     work in every column, so a row that is not flat across the axis is the machine moving rather
    ///     than the code, which is the reading <c>docs/build-warnings.md</c> §2.23 insists on before any
    ///     timing claim.
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
    ///     language throws and catches <b>once per operation</b> and writes to <c>Trace</c>. That is
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
    /// <param name="prefix">The namespace prefix, or <b>null</b> when the lookup missed.</param>
    /// <param name="localName">The element name, or <b>null</b> when the lookup missed.</param>
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
    /// <returns>The step the expression represents, or <b>default</b> if it is not one of the 55.</returns>
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
    /// <param name="Prefix">The namespace prefix. <b>null</b> when a lookup missed.</param>
    /// <param name="LocalName">The local name of the child element. <b>null</b> when a lookup missed.</param>
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