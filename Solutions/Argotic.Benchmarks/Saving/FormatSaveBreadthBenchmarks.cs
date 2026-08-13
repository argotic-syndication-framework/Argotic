using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Saving;

/// <summary>
/// Prices saving the document formats beyond RSS and Atom: OPML, APML, BlogML and RSD.
/// </summary>
/// <remarks>
///     The save table had two formats in it — <c>FeedSaveBenchmarks</c> covers RSS and Atom, and
///     no other writer had a benchmark at all (<c>.endjin/build-warnings.md</c> §19). The §17 RSS
///     save regression was exactly a save-vs-load asymmetry that went unpriced until the missing
///     save benchmark was written; this class puts a number on the remaining writers before their
///     regression, not after. Sizes are fixed at realistic points: 299 outlines is the real Power
///     BI Weekly subscription list, 100 posts/concepts is a modest real blog and profile, and RSD
///     is the committed sample.
/// </remarks>
[BenchmarkCategory("save", "formats", "breadth", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class FormatSaveBreadthBenchmarks
{
    private OpmlDocument opml = new();
    private ApmlDocument apml = new();
    private BlogMLDocument blogml = new();
    private RsdDocument rsd = new();

    /// <summary>
    /// Loads the four documents once, guarding counts, so the arms measure serialisation only.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        using (MemoryStream stream = new(OpmlCorpus.GenerateSubscriptionListUtf8(299), writable: false))
        {
            this.opml = new OpmlDocument();
            this.opml.Load(stream);
        }

        using (MemoryStream stream = new(SpecializedDocumentCorpus.GenerateApmlUtf8(100), writable: false))
        {
            this.apml = new ApmlDocument();
            this.apml.Load(stream);
        }

        using (MemoryStream stream = new(SpecializedDocumentCorpus.GenerateBlogMLUtf8(100), writable: false))
        {
            this.blogml = new BlogMLDocument();
            this.blogml.Load(stream);
        }

        using (MemoryStream stream = new(FeedCorpus.ReadRealSample("RsdDocument.xml"), writable: false))
        {
            this.rsd = new RsdDocument();
            this.rsd.Load(stream);
        }

        Verify(this.opml.Outlines.Count == 299, "OPML outline count");
        Verify(this.apml.Profiles.Count == 2, "APML profile count");
        Verify(this.blogml.Posts.Count == 100, "BlogML post count");
        Verify(this.rsd.Interfaces.Count > 0, "RSD API count");
    }

    /// <summary>
    /// Saves the 299-outline subscription list.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. OPML Save (299 outlines)")]
    public long SaveOpml()
    {
        using MemoryStream stream = new();
        this.opml.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Saves the 100-concept attention profile.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. APML Save (100 concepts)")]
    public long SaveApml()
    {
        using MemoryStream stream = new();
        this.apml.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Saves the 100-post blog export.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. BlogML Save (100 posts)")]
    public long SaveBlogml()
    {
        using MemoryStream stream = new();
        this.blogml.Save(stream);
        return stream.Length;
    }

    /// <summary>
    /// Saves the discovery document.
    /// </summary>
    /// <returns>The byte length written, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. RSD Save")]
    public long SaveRsd()
    {
        using MemoryStream stream = new();
        this.rsd.Save(stream);
        return stream.Length;
    }

    private static void Verify(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Corpus guard failed: {description}.");
        }
    }
}