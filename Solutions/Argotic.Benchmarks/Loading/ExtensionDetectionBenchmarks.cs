using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Decomposes the extension auto-detection path, to find which part of it costs the 54.7 MB that
/// F1 attributes to auto-detection as a whole.
/// </summary>
/// <remarks>
/// <para>
/// F1 established that auto-detection costs 3.87x allocation, but not which of its three
/// sub-steps dominates. Those steps have very different fixes and very different risks:
/// </para>
/// <list type="number">
///   <item><description>the assembly reflection scan in the <c>FrameworkExtensions</c> property —
///   trivially cacheable, since a loaded assembly's exported types cannot change;</description></item>
///   <item><description><c>Activator.CreateInstance</c> for ~22 extension types — cacheable only
///   if the instances are genuinely used read-only, which would need proving;</description></item>
///   <item><description><c>ExistsInSource(navigator)</c> probing each extension against the
///   document — ~22 XPath evaluations per entity, and not cacheable at all.</description></item>
/// </list>
/// <para>
/// Guessing here would be the frozen-collections mistake in miniature: adopt the plausible fix
/// (cache the reflection scan), ship it, and discover it addressed a few percent because the real
/// cost was elsewhere. One cheap benchmark decides it instead.
/// </para>
/// <para>
/// The measured points are nested, so the sub-steps fall out by subtraction: (a) is the property
/// alone, (b) contains (a), and (c) contains (b).
/// </para>
/// <para>
/// <strong>The question above has been answered, and acted on.</strong> The reflection scan was
/// 0.13 of auto-detection's allocation and <c>Activator.CreateInstance</c> was the remaining ~0.85 —
/// so the plausible fix would indeed have addressed a few percent, exactly as feared. The instances
/// were then shown to be used read-only during probing (<c>ExistsInSource</c> is virtual but
/// unoverridden, and reads only the immutable <c>XmlNamespace</c> and <c>XmlPrefix</c>), so
/// <c>SyndicationExtensionAdapter.Fill</c> now probes with one cached instance per type. That cut
/// <c>Load(Stream)</c> allocation by 74%.
/// </para>
/// <para>
/// <strong>Consequently this class no longer measures what <c>Fill</c> does.</strong> It exercises
/// the public <c>GetExtensions</c> overloads, which still allocate deliberately — what they return
/// escapes to a caller who may do anything with it, so they cannot hand out shared instances. Read
/// these numbers as the cost of the public API, not of a feed load; <c>ParsePipelineBenchmarks</c>
/// f/g/h measure the path a load actually takes.
/// </para>
/// </remarks>
[BenchmarkCategory("extensions", "diagnostic")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class ExtensionDetectionBenchmarks
{
    private XPathNavigator navigator = null!;
    private Dictionary<string, string> namespaces = [];

    /// <summary>
    /// Gets or sets how many times the detection path runs, standing in for entity count.
    /// </summary>
    /// <remarks>
    /// The real load path invokes this sequence once per extensible entity, so the parameter is
    /// entity count expressed directly rather than feed size expressed indirectly.
    /// </remarks>
    // One value only. Each body is `for (i = 0; i < EntityCount; i++) total += loop-invariant work`,
    // so the sweep reproduced N times the N=1 number by construction - the committed artifact matched
    // the prediction to within 0.15%. Six of the nine rows carried no information.
    [Params(1)]
    public int EntityCount { get; set; }

    /// <summary>
    /// Builds a navigator over a representative feed, once.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // The extension-bearing corpus, not the extension-free one. Built over GenerateRssUtf8 the
        // namespaces-in-scope dictionary was empty, so the arm named "namespace filtering" had nothing
        // to filter and measured a 2% difference that meant nothing.
        byte[] document = FeedCorpus.GenerateRssWithExtensionsUtf8(10);
        using MemoryStream stream = new(document, writable: false);
        this.navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);
        this.namespaces = (Dictionary<string, string>)this.navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
    }

    /// <summary>
    /// Performs the assembly reflection scan behind the <c>FrameworkExtensions</c> property, in
    /// isolation.
    /// </summary>
    /// <returns>A running count, so the work cannot be elided.</returns>
    [Benchmark(Description = "1. FrameworkExtensions (reflection scan)")]
    public int ReflectionScanOnly()
    {
        int total = 0;
        for (int i = 0; i < this.EntityCount; i++)
        {
            total += SyndicationExtensionAdapter.FrameworkExtensions.Count;
        }

        return total;
    }

    /// <summary>
    /// Steps 1 and 2: the scan plus Activator.CreateInstance for every framework extension type.
    /// </summary>
    /// <returns>A running count, so the work cannot be elided.</returns>
    [Benchmark(Description = "2. + Activator.CreateInstance x ~22")]
    public int ScanAndInstantiate()
    {
        int total = 0;
        for (int i = 0; i < this.EntityCount; i++)
        {
            total += SyndicationExtensionAdapter.GetExtensions(SyndicationExtensionAdapter.FrameworkExtensions).Count;
        }

        return total;
    }

    /// <summary>
    /// Steps 1, 2 and 3: everything above plus namespace filtering, as the load path performs it.
    /// </summary>
    /// <returns>A running count, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "3. + namespace filtering (full detection)")]
    public int FullDetection()
    {
        int total = 0;
        Collection<Type> empty = [];
        for (int i = 0; i < this.EntityCount; i++)
        {
            total += SyndicationExtensionAdapter.GetExtensions(empty, this.namespaces).Count;
        }

        return total;
    }
}