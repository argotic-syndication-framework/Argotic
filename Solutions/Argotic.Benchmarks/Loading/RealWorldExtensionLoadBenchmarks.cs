using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads the real documents that calibrate the gapfill generators: the largest podcast feed, the
/// attribute-only hreflang sitemap, and every legacy-format document in the corpus.
/// </summary>
/// <remarks>
/// <para>
/// Machine-local and opt-in, like <see cref="RealWorldLoadBenchmarks"/>: the corpus is gitignored
/// and byte-exact from live publishers, so these arms fail loudly on a machine without it rather
/// than existing in the commit gate. Their purpose is calibration — if
/// <c>ExtensionFeedCorpus.GeneratePodcastFeedUtf8</c>'s per-item cost diverges from
/// simplecast-thedaily's, or the hreflang generator's from gitlab-pages', by more than the §21
/// gate allows, the synthetic corpus's realism claim fails and the generator is adjusted before
/// its baseline is trusted.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "realworld", "extensions", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class RealWorldExtensionLoadBenchmarks
{
    private byte[] theDaily = [];
    private byte[] gitlabPages = [];
    private byte[][] legacyRss = [];
    private byte[][] atom03 = [];
    private byte[][] rejectedByDesign = [];

    /// <summary>
    /// Reads the corpus documents, failing loudly when the corpus is absent, and parse-verifies
    /// the two calibration anchors.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.theDaily = RealWorldCorpus.Read("extensions/itunes/simplecast-thedaily.xml");
        this.gitlabPages = RealWorldCorpus.Read("extensions/sitemap-hreflang/gitlab-pages.xml");

        // Two corpus documents declare version strings the dispatch deliberately rejects:
        // scripting.com's version="0.94" (a version that never existed) and Tim Bray's
        // version="draft-ietf-atompub-format-04 : do not deploy". They are excluded here and
        // asserted below to STILL throw - if the library ever learns to read them, that guard
        // fails and this skip list is the thing to delete.
        this.rejectedByDesign =
        [
            RealWorldCorpus.Read("rss09x/scripting-com-radio-userland-rss094-2002.xml"),
            RealWorldCorpus.Read("atom03/tbray-ongoing-atom-draft04-2005.xml"),
        ];

        byte[][] allLegacyRss = [.. RealWorldCorpus.ReadAll("rss09x"), .. RealWorldCorpus.ReadAll("rss10")];
        this.legacyRss = [.. allLegacyRss.Where(document => !this.rejectedByDesign.Any(rejected => rejected.AsSpan().SequenceEqual(document)))];
        this.atom03 = [.. RealWorldCorpus.ReadAll("atom03").Where(document => !this.rejectedByDesign.Any(rejected => rejected.AsSpan().SequenceEqual(document)))];

        VerifyStillRejected(() => LoadRss(this.rejectedByDesign[0]), "the version=\"0.94\" document");
        VerifyStillRejected(
            () =>
            {
                AtomFeed feed = new();
                using MemoryStream stream = new(this.rejectedByDesign[1], writable: false);
                feed.Load(stream);
                return feed;
            },
            "the atom draft-04 document");

        RssFeed daily = LoadRss(this.theDaily);
        if (daily.Channel.Items.Count < 2_000)
        {
            throw new InvalidOperationException("Corpus guard failed: simplecast-thedaily parsed to fewer than 2,000 items.");
        }

        Sitemap pages = new();
        using (MemoryStream stream = new(this.gitlabPages, writable: false))
        {
            pages.Load(stream);
        }

        if (pages.Urls.Count == 0)
        {
            throw new InvalidOperationException("Corpus guard failed: gitlab-pages parsed to zero URLs.");
        }
    }

    /// <summary>
    /// Loads the 18.5 MB, 2,939-item podcast feed — the podcast generator's calibration anchor.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. simplecast-thedaily.xml (18.5 MB podcast)")]
    public int LoadTheDaily() => LoadRss(this.theDaily).Channel.Items.Count;

    /// <summary>
    /// Loads the 17,854-link hreflang sitemap — the hreflang generator's calibration anchor.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. gitlab-pages.xml (hreflang sitemap)")]
    public int LoadGitlabPages()
    {
        Sitemap sitemap = new();
        using MemoryStream stream = new(this.gitlabPages, writable: false);
        sitemap.Load(stream);
        return sitemap.Urls.Count;
    }

    /// <summary>
    /// Loads every real RSS 0.9x and 1.0 document — the legacy generators' calibration set,
    /// including the real encodings and DOCTYPEs.
    /// </summary>
    /// <returns>The number of items parsed across all documents, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. every rss09x + rss10 document")]
    public int LoadEveryLegacyRss()
    {
        int items = 0;
        foreach (byte[] document in this.legacyRss)
        {
            items += LoadRss(document).Channel.Items.Count;
        }

        return items;
    }

    /// <summary>
    /// Loads every real Atom 0.3 document, including the iso-8859-1 one.
    /// </summary>
    /// <returns>The number of entries parsed across all documents, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. every atom03 document")]
    public int LoadEveryAtom03()
    {
        int entries = 0;
        foreach (byte[] document in this.atom03)
        {
            AtomFeed feed = new();
            using MemoryStream stream = new(document, writable: false);
            feed.Load(stream);
            entries += feed.Entries.Count;
        }

        return entries;
    }

    private static RssFeed LoadRss(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static void VerifyStillRejected(Func<object> load, string description)
    {
        try
        {
            _ = load();
        }
        catch (FormatException)
        {
            return;
        }

        throw new InvalidOperationException($"Corpus guard failed: {description} now loads - the library gained support for it, so remove it from the rejected-by-design list.");
    }
}