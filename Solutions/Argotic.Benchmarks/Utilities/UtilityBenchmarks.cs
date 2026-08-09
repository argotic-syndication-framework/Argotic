using System.Diagnostics.CodeAnalysis;
using Argotic.Common;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Utilities;

/// <summary>
/// Measures the Common utility layer that every load and every equality check runs through.
/// </summary>
/// <remarks>
/// <para>
/// These are targeted at recent churn rather than chosen for breadth.
/// <c>ComparisonUtility.CompareSequence</c> was rewritten on this branch — from an
/// <c>Aggregate</c> that OR-ed every element comparison together to a <c>FirstOrDefault</c> that
/// short-circuits on the first difference — and <c>HashCodeUtility</c> is new, added so hash codes
/// agree with the framework's case-insensitive equality. Both sit on paths walked once per element
/// of every feed, so a regression here would be quiet and pervasive.
/// </para>
/// <para>
/// The short-circuit rewrite has a measurable consequence worth capturing: cost now depends on
/// where sequences first differ. Identical sequences are the worst case (every element
/// compared); sequences differing at the first element are the best. Both are measured, because a
/// benchmark of only one would describe the change dishonestly.
/// </para>
/// <para>
/// The scalar operations carry no size parameter, because a single string hash has no meaningful size
/// axis. The omission is deliberate and is recorded here so that it is not mistaken for an oversight.
/// </para>
/// </remarks>
[BenchmarkCategory("utilities")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class UtilityBenchmarks
{
    private List<string> identical = [];
    private List<string> differsFirst = [];
    private List<string> reference = [];

    /// <summary>
    /// Gets or sets the sequence length for the comparison benchmarks.
    /// </summary>
    [Params(10, 100, 1000)]
    public int SequenceLength { get; set; }

    /// <summary>
    /// Builds the best-case and worst-case sequences for the short-circuit comparison.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.reference = [.. Enumerable.Range(0, this.SequenceLength).Select(i => $"element-{i}")];
        this.identical = [.. this.reference];
        this.differsFirst = [.. this.reference];
        if (this.differsFirst.Count > 0)
        {
            this.differsFirst[0] = "zzz-differs-immediately";
        }
    }

    /// <summary>
    /// Worst case for the short-circuiting rewrite: every element must be compared.
    /// </summary>
    /// <returns>The comparison result.</returns>
    [Benchmark(Baseline = true, Description = "CompareSequence, identical (worst case)")]
    public int CompareIdentical() => ComparisonUtility.CompareSequence(this.reference, this.identical, StringComparison.Ordinal);

    /// <summary>
    /// Best case for the short-circuiting rewrite: the first element already differs.
    /// </summary>
    /// <returns>The comparison result.</returns>
    [Benchmark(Description = "CompareSequence, differs at first (best case)")]
    public int CompareDiffersFirst() => ComparisonUtility.CompareSequence(this.reference, this.differsFirst, StringComparison.Ordinal);

}

/// <summary>
/// Scalar hashing, which has no size axis.
/// </summary>
/// <remarks>
/// Split out of <see cref="UtilityBenchmarks"/>. BenchmarkDotNet scopes <c>[Params]</c> to the type,
/// so these two methods - which never read the parameter - were run three times each and emitted
/// six duplicate rows. The original class documented the exemption in a comment; a comment cannot
/// implement it, and splitting the type is the only thing that can.
/// </remarks>
[BenchmarkCategory("utilities")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class HashComponentBenchmarks
{
    // Held as instance fields rather than handed to the measured call as literals: a literal can be
    // constant-folded or its hash cached, which would measure the optimiser instead of the hash. The
    // Uri is built once at construction so that parsing it does not land inside a benchmark claiming
    // to measure hashing.
    //
    // This reasoning arrived here with the fields it describes. It used to sit above two fields of
    // UtilityBenchmarks that nothing ever read - the comment explained a technique the code below it
    // did not perform, while the fields it actually applied to were in this type and undocumented.
    private readonly Uri sampleUri = new("http://www.example.com/feeds/all.atom.xml");
    private readonly string sampleUriText = "http://www.example.com/feeds/all.atom.xml";

    /// <summary>
    /// Hash component for a string, as used by the framework's equality-consistent hashing.
    /// </summary>
    /// <returns>The hash component.</returns>
    [Benchmark(Baseline = true, Description = "HashCodeUtility.Component(string)")]
    public int HashStringComponent() => HashCodeUtility.Component(this.sampleUriText);

    /// <summary>
    /// Hash component for a Uri.
    /// </summary>
    /// <returns>The hash component.</returns>
    [Benchmark(Description = "HashCodeUtility.Component(Uri)")]
    public int HashUriComponent() => HashCodeUtility.Component(this.sampleUri);
}