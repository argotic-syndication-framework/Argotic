using System.Diagnostics.CodeAnalysis;
using System.Xml.XPath;

using Argotic.Net;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Protocols;

/// <summary>
/// Scales the two recursive branches of <c>XmlRpcClient.TryParseValue</c>, in width and in depth.
/// </summary>
/// <remarks>
/// <para>
/// <c>struct</c> and <c>array</c> are the only branches that re-enter the parser, and both were at
/// zero coverage. <c>XmlRpcStructureValue</c> alone carries cyclomatic complexity 48 that has never
/// executed. They are also the only branches whose cost is not bounded by the element itself: a
/// <c>metaWeblog.getRecentPosts</c> response is a struct of arrays of structs, and nothing in the
/// library caps how large or how deep one may be.
/// </para>
/// <para>
/// Every arm varies along the <see cref="LeafCount"/> axis, and that is the constraint the class
/// was designed around. A class-scoped axis that most arms ignore yields rows which merely reproduce
/// the parameter, to within measurement error, and carry no information. Here the leaf count is held
/// equal across all four arms at every value of the axis, so the arms differ in shape and in nothing
/// else:
/// </para>
/// <list type="bullet">
///   <item><description>Flat struct — N members, each a named string. One level.</description></item>
///   <item><description>Flat array — N integers. One level. Its delta against the struct is the
///   price of the <c>name</c> lookup and the <c>XmlRpcStructureMember</c> per element, at identical
///   element counts and identical recursion depth.</description></item>
///   <item><description>Struct of arrays — N leaves spread over ten array-valued members. Two
///   levels. Its delta against the flat arms is the price of one level of recursion at constant leaf
///   count, which is the number that says whether nesting is free.</description></item>
///   <item><description>Array chain — N leaves spread ten per level down an N/10-deep chain of
///   nested arrays. Depth grows with the axis while the leaf count does not, so this is the arm that
///   separates depth from width. At the top of the sweep it is a hundred levels deep.</description></item>
/// </list>
/// <para>
/// The unbounded-recursion defect this class stops short of. The chain arm is capped at ten
/// leaves per level so that the deepest payload measured is a hundred levels. Nothing in
/// <c>TryParseValue</c>, <c>XmlRpcArrayValue.Load</c> or <c>XmlRpcStructureValue.Load</c> bounds
/// depth, and the recursion is not tail-recursive, so a server can send a payload that overflows the
/// stack of the process reading it — an unrecoverable failure, not an exception. That is a defect to
/// report and fix; deliberately overflowing the stack would tell the harness nothing it does not
/// already know from reading the three methods.
/// </para>
/// </remarks>
[BenchmarkCategory("protocols", "xmlrpc", "parse", "scale")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class XmlRpcCompositeParsingBenchmarks
{
    /// <summary>
    /// The number of array-valued members the struct-of-arrays arm spreads its leaves over.
    /// </summary>
    private const int GroupCount = 10;

    /// <summary>
    /// The number of scalar leaves each level of the chain arm holds, which caps its depth.
    /// </summary>
    private const int LeavesPerLevel = 10;

    private XPathNavigator flatStruct = null!;
    private XPathNavigator flatArray = null!;
    private XPathNavigator structOfArrays = null!;
    private XPathNavigator arrayChain = null!;

    /// <summary>
    /// Gets or sets the number of scalar leaves in the payload under test.
    /// </summary>
    /// <remarks>
    ///     Held identical across all four arms so that shape, not size, is what differs between them.
    ///     A thousand leaves is a large but entirely ordinary <c>metaWeblog</c> response; it is not a
    ///     protocol ceiling, because XML-RPC defines none.
    /// </remarks>
    [Params(10, 100, 1000)]
    public int LeafCount { get; set; }

    /// <summary>
    /// Builds the four shapes at the current leaf count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.flatStruct = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.StructValue(this.LeafCount));
        this.flatArray = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.ArrayValue(this.LeafCount));
        this.structOfArrays = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.StructOfArrays(this.LeafCount, GroupCount));
        this.arrayChain = XmlRpcCorpus.ValueNavigator(XmlRpcCorpus.ArrayChain(this.LeafCount, LeavesPerLevel));
    }

    /// <summary>
    /// A flat array of N integers: one level, N re-entries at dispatch position 1.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     The baseline because it is the cheapest shape that still recurses, so every other arm's
    ///     ratio reads as the price of a structural choice rather than of recursion as such.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "flat array, N integers")]
    public IXmlRpcValue? ParseFlatArray()
    {
        _ = XmlRpcClient.TryParseValue(this.flatArray, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// A flat struct of N named members: one level, N re-entries plus N name lookups.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     <c>XmlRpcStructureValue.Load</c> selects <c>struct/member</c> through
    ///     <see cref="XPathNavigator.Select(string)"/>, which compiles an XPath expression on every
    ///     call — the exact cost that <c>XPathNavigatorExtensions.SelectChildElements</c> was
    ///     introduced to remove from the feed adapters, measured there at 7.9× the allocation. This
    ///     path never got the same treatment. If the struct arm's allocation per member is far above
    ///     the array arm's, that is where to look first.
    /// </remarks>
    [Benchmark(Description = "flat struct, N named members")]
    public IXmlRpcValue? ParseFlatStruct()
    {
        _ = XmlRpcClient.TryParseValue(this.flatStruct, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// N leaves over ten array-valued struct members: two levels, same leaf count.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     The realistic shape — this is what a weblog API hands back — and the arm that prices one
    ///     level of nesting at constant leaf count. If it sits on top of the flat struct, nesting is
    ///     free and only leaf count matters; if it climbs, the per-level overhead is real and the
    ///     chain arm will show how it accumulates.
    /// </remarks>
    [Benchmark(Description = "struct of 10 arrays, N leaves (two levels)")]
    public IXmlRpcValue? ParseStructOfArrays()
    {
        _ = XmlRpcClient.TryParseValue(this.structOfArrays, out IXmlRpcValue? value);
        return value;
    }

    /// <summary>
    /// N leaves down an N/10-deep chain of nested arrays: depth varies, leaf count does not.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     <para>
    ///     The only arm in the harness where recursion depth is the variable. Ten leaves at each of
    ///     N/10 levels, so at the top of the sweep this is a hundred nested <c>array</c> elements
    ///     holding a thousand integers — the same thousand integers the flat array arm parses in one
    ///     level.
    ///     </para>
    ///     <para>
    ///     What would refute the hypothesis that depth costs anything: this arm tracking the flat
    ///     array arm across the whole sweep. What would confirm it: a widening gap, which would also
    ///     put a number on how far a hostile payload has to nest before it hurts.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "array chain, N/10 levels deep, N leaves")]
    public IXmlRpcValue? ParseArrayChain()
    {
        _ = XmlRpcClient.TryParseValue(this.arrayChain, out IXmlRpcValue? value);
        return value;
    }
}