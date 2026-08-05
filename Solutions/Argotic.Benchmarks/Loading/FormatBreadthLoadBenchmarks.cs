using System.Diagnostics.CodeAnalysis;

using Argotic.Common;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads the formats the harness previously only sniffed.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RealSampleBenchmarks"/> reads all fifteen sample documents and runs format detection
/// over them, then discards them - its class summary promises it "sniffs and loads" every one, and
/// it contains a single benchmark that only sniffs. Eight types with a public
/// <c>Load(Stream)</c> were therefore never parsed by any benchmark: OpmlDocument, ApmlDocument,
/// BlogMLDocument, RsdDocument, AtomEntry, SitemapIndex, AtomServiceDocument and
/// AtomCategoryDocument.
/// </para>
/// <para>
/// This is coverage insurance rather than a decision input, which is why it sits below the polling
/// and retrieval-limit measurements in priority. It is the cheapest way to find out whether any
/// format carries a pathology the RSS-shaped corpus cannot express.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "formats", "breadth")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class FormatBreadthLoadBenchmarks
{
    private byte[] opml = [];
    private byte[] apml = [];
    private byte[] blogML = [];
    private byte[] rsd = [];
    private byte[] atomEntry = [];

    /// <summary>
    /// Reads the real sample documents once.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.opml = FeedCorpus.ReadRealSample("OpmlDocument.xml");
        this.apml = FeedCorpus.ReadRealSample("ApmlDocument.xml");
        this.blogML = FeedCorpus.ReadRealSample("BlogMLDocument.xml");
        this.rsd = FeedCorpus.ReadRealSample("RsdDocument.xml");
        this.atomEntry = FeedCorpus.ReadRealSample("AtomEntryDocument.xml");
    }

    /// <summary>
    /// Parses an OPML outline document.
    /// </summary>
    /// <returns>The parsed document.</returns>
    [Benchmark(Baseline = true, Description = "OpmlDocument.Load(Stream)")]
    public OpmlDocument LoadOpml() => Load<OpmlDocument>(this.opml);

    /// <summary>
    /// Parses an APML attention profile.
    /// </summary>
    /// <returns>The parsed document.</returns>
    [Benchmark(Description = "ApmlDocument.Load(Stream)")]
    public ApmlDocument LoadApml() => Load<ApmlDocument>(this.apml);

    /// <summary>
    /// Parses a BlogML export.
    /// </summary>
    /// <returns>The parsed document.</returns>
    [Benchmark(Description = "BlogMLDocument.Load(Stream)")]
    public BlogMLDocument LoadBlogML() => Load<BlogMLDocument>(this.blogML);

    /// <summary>
    /// Parses an RSD service discovery document.
    /// </summary>
    /// <returns>The parsed document.</returns>
    [Benchmark(Description = "RsdDocument.Load(Stream)")]
    public RsdDocument LoadRsd() => Load<RsdDocument>(this.rsd);

    /// <summary>
    /// Parses a standalone Atom entry document.
    /// </summary>
    /// <returns>The parsed entry.</returns>
    [Benchmark(Description = "AtomEntry.Load(Stream)")]
    public AtomEntry LoadAtomEntry() => Load<AtomEntry>(this.atomEntry);

    private static TResource Load<TResource>(byte[] document)
        where TResource : ISyndicationResource, new()
    {
        using MemoryStream stream = new(document, writable: false);
        TResource resource = new();
        resource.Load(stream);
        return resource;
    }
}