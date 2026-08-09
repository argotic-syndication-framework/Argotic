using System.Diagnostics.CodeAnalysis;
using Argotic.Common;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks;

/// <summary>
/// Sniffs and loads every real sample document in the repository.
/// </summary>
/// <remarks>
/// <para>
/// Breadth over fidelity: fifteen genuine documents spanning RSS, Atom, OPML, APML, BlogML, RSD
/// and the sitemap family. Their sizes are fixed facts, so there is no size parameter and no
/// scaling claim is available from them — that is what the synthetic ladder is for. What these
/// give is confidence that no format has a pathology the synthetic corpus never exercises.
/// </para>
/// <para>
/// Format sniffing is measured alongside loading because an aggregator handed an unknown document
/// pays it first, on every document, before it can choose a parser.
/// </para>
/// </remarks>
[BenchmarkCategory("realsamples")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class RealSampleBenchmarks
{
    private byte[] content = [];

    /// <summary>
    /// Gets or sets the sample document under test.
    /// </summary>
    [ParamsSource(nameof(SampleFiles))]
    public string SampleFile { get; set; } = "RssFeed.xml";

    /// <summary>
    /// Gets the sample documents shipped with the repository.
    /// </summary>
    /// <remarks>
    /// Enumerated from disk rather than hard-coded, so a sample added to the Examples project is
    /// picked up automatically instead of quietly going unmeasured.
    /// </remarks>
    public static IEnumerable<string> SampleFiles =>
        Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "SampleData"), "*.xml")
                 .Select(Path.GetFileName)
                 .Where(name => name is not null)
                 .Select(name => name!)
                 .OrderBy(name => name, StringComparer.Ordinal);

    /// <summary>
    /// Reads the current sample document into memory.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.content = FeedCorpus.ReadRealSample(this.SampleFile);

    /// <summary>
    /// Determines a document's syndication format — what an aggregator does before parsing.
    /// </summary>
    /// <returns>The detected format.</returns>
    [Benchmark(Description = "SyndicationContentFormatGet")]
    public SyndicationContentFormat DetectFormat()
    {
        using MemoryStream stream = new(this.content, writable: false);
        return SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);
    }
}