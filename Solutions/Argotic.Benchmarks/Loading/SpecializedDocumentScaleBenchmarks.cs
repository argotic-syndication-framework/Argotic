using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices BlogML and APML loading as a function of document size.
/// </summary>
/// <remarks>
///     The committed samples are loaded once by <c>FormatBreadthLoadBenchmarks</c>, so these
///     formats had token presence but no scaling curve — a 4-post export cannot say what a
///     1,000-post one costs, and a real BlogML export is as large as the blog it migrates
///     (<c>.endjin/build-warnings.md</c> §19). The corpus carries the shapes that make the
///     formats interesting: approval-status mixes, embedded base64 attachments, and APML's twin
///     implicit/explicit sub-trees.
/// </remarks>
[BenchmarkCategory("load", "specialized", "scale", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SpecializedDocumentScaleBenchmarks
{
    private byte[] blogml = [];
    private byte[] apml = [];

    /// <summary>
    /// Gets or sets the number of posts (BlogML) or implicit concepts (APML) in the document.
    /// </summary>
    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates both corpora once and parse-verifies counts and the approval-status mix.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.blogml = SpecializedDocumentCorpus.GenerateBlogMLUtf8(this.ItemCount);
        this.apml = SpecializedDocumentCorpus.GenerateApmlUtf8(this.ItemCount);

        BlogMLDocument export = LoadBlogML(this.blogml);
        Verify(export.Posts.Count == this.ItemCount, "BlogML post count");
        Verify(export.Authors.Count == 3, "BlogML author count");
        Verify(export.Posts.First().Comments.Count == 3, "BlogML comments on the first post (two approved, one not)");

        ApmlDocument profile = LoadApml(this.apml);
        Verify(profile.Profiles.Count == 2, "APML profile count");
        Verify(profile.Applications.Count == 1, "APML application parsed");
        Verify(profile.Profiles.First().ImplicitSources.FirstOrDefault()?.Authors.Count == 1, "APML source author parsed");
    }

    /// <summary>
    /// Loads the BlogML export.
    /// </summary>
    /// <returns>The number of posts parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. BlogML export")]
    public int LoadBlogMLExport() => LoadBlogML(this.blogml).Posts.Count;

    /// <summary>
    /// Loads the APML attention profile.
    /// </summary>
    /// <returns>The number of profiles parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. APML attention profile")]
    public int LoadApmlProfile() => LoadApml(this.apml).Profiles.Count;

    private static BlogMLDocument LoadBlogML(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        BlogMLDocument export = new();
        export.Load(stream);
        return export;
    }

    private static ApmlDocument LoadApml(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        ApmlDocument profile = new();
        profile.Load(stream);
        return profile;
    }

    private static void Verify(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Corpus guard failed: {description}.");
        }
    }
}