using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices loading the formats that predate RSS 2.0 and Atom 1.0: the four legacy RSS versions,
/// Atom 0.3 and RSD 0.6.
/// </summary>
/// <remarks>
/// <para>
/// The six legacy adapters total 1,520 lines and had zero benchmark coverage
/// (<c>.endjin/build-warnings.md</c> §19): every load benchmark parsed RSS 2.0 or Atom 1.0, so
/// the RDF-shaped parse (items listed twice — once in the <c>rdf:Seq</c> manifest, once as the
/// item), the ISO-8859-1 + DOCTYPE wire shape real 0.91 feeds still ship, and the
/// namespace-sniffed version dispatch had no price. Legacy feeds are not archaeology for a
/// crawler: Slashdot publishes RDF today, and the real corpus's 0.9x documents were all fetched
/// live.
/// </para>
/// <para>
/// The parameter's low value is 15 because RSS 0.91's own DTD caps a channel at fifteen items —
/// the honest "typical legacy feed" size — and 100 is the stress point shared with the modern
/// corpus for cross-format comparison.
/// </para>
/// <para>
/// Setup guards are structural, because version identity is not observable from the object model
/// (<c>RssFeed.Version</c> is a fixed 2.0): each arm asserts an element only its intended adapter
/// fills — the PICS <c>rating</c> for 0.91 (the 2.0 adapter never reads it), <c>cloud</c> plus
/// enclosures for 0.92, the accented title for the ISO-8859-1 arm (a wrong decode cannot
/// reproduce it), attached Dublin Core extensions for the RDF item parse, and entry counts for
/// Atom 0.3 (its namespace differs from 1.0, so a misrouted parse finds nothing). A rejected arm
/// is recorded here rather than silently absent: no <c>version="0.94"</c> document, although one
/// real feed shipped that string — the dispatch throws for it, so the arm would measure an
/// exception and its guard could never pass.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "formats", "legacy", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class LegacyFormatLoadBenchmarks
{
    private byte[] rss091 = [];
    private byte[] rss091Iso = [];
    private byte[] rss092 = [];
    private byte[] rss090 = [];
    private byte[] rss10 = [];
    private byte[] atom03 = [];
    private byte[] rsd06 = [];

    /// <summary>
    /// Gets or sets the number of items (or entries, or APIs) in the document under test.
    /// </summary>
    [Params(15, 100)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates every corpus once and parse-verifies each against the structure only its
    /// intended adapter produces, so a silently misrouted or misparsed arm throws here instead of
    /// reporting a number.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.rss091 = LegacyFeedCorpus.GenerateRss091Utf8(this.ItemCount);
        this.rss091Iso = LegacyFeedCorpus.GenerateRss091Iso88591WithDoctype(this.ItemCount);
        this.rss092 = LegacyFeedCorpus.GenerateRss092Utf8(this.ItemCount);
        this.rss090 = LegacyFeedCorpus.GenerateRss090Utf8(this.ItemCount);
        this.rss10 = LegacyFeedCorpus.GenerateRss10Utf8(this.ItemCount);
        this.atom03 = LegacyFeedCorpus.GenerateAtom03Utf8(this.ItemCount);
        this.rsd06 = LegacyFeedCorpus.GenerateRsd06Utf8(this.ItemCount);

        RssFeed rss091Feed = LoadRss(this.rss091);
        Verify(rss091Feed.Channel.Items.Count == this.ItemCount, "0.91 item count");
        Verify(!string.IsNullOrEmpty(rss091Feed.Channel.Rating), "0.91 PICS rating (only the 0.91 adapter reads it)");
        Verify(rss091Feed.Channel.SkipDays.Count == 2, "0.91 skipDays");
        Verify(rss091Feed.Channel.TextInput is not null, "0.91 textInput");

        RssFeed rss091IsoFeed = LoadRss(this.rss091Iso);
        Verify(rss091IsoFeed.Channel.Items.Count == this.ItemCount, "0.91 ISO-8859-1 item count");
        Verify(rss091IsoFeed.Channel.Items.First().Title?.Contains('é', StringComparison.Ordinal) is true, "0.91 ISO-8859-1 accents survived decoding");

        RssFeed rss092Feed = LoadRss(this.rss092);
        Verify(rss092Feed.Channel.Items.Count == this.ItemCount, "0.92 item count");
        Verify(rss092Feed.Channel.Cloud is not null, "0.92 cloud");
        Verify(rss092Feed.Channel.Items.First().Enclosures.Any(), "0.92 enclosure on the first item");

        RssFeed rss090Feed = LoadRss(this.rss090);
        Verify(rss090Feed.Channel.Items.Count == this.ItemCount, "0.90 item count");
        Verify(rss090Feed.Channel.Image is not null, "0.90 image");

        RssFeed rss10Feed = LoadRss(this.rss10);
        Verify(rss10Feed.Channel.Items.Count == this.ItemCount, "1.0 item count (rdf:Seq manifest resolved)");
        Verify(rss10Feed.Channel.Items.First().HasExtensions, "1.0 Dublin Core attached to the first item");

        AtomFeed atom03Feed = new();
        using (MemoryStream stream = new(this.atom03, writable: false))
        {
            atom03Feed.Load(stream);
        }

        Verify(atom03Feed.Entries.Count == this.ItemCount, "Atom 0.3 entry count (namespace-differential: a 1.0 parse finds nothing)");

        RsdDocument rsdDocument = new();
        using (MemoryStream rsdStream = new(this.rsd06, writable: false))
        {
            rsdDocument.Load(rsdStream);
        }

        Verify(rsdDocument.Interfaces.Count == this.ItemCount, "RSD 0.6 API count");
    }

    /// <summary>
    /// Loads the full RSS 0.91 document.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. RSS 0.91 (UTF-8)")]
    public int LoadRss091() => LoadRss(this.rss091).Channel.Items.Count;

    /// <summary>
    /// Loads the same RSS 0.91 document as it actually travelled: ISO-8859-1 with a PUBLIC
    /// DOCTYPE.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. RSS 0.91 (ISO-8859-1 + DOCTYPE)")]
    public int LoadRss091IsoDoctype() => LoadRss(this.rss091Iso).Channel.Items.Count;

    /// <summary>
    /// Loads the RSS 0.92 document with cloud, enclosures and sources.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. RSS 0.92")]
    public int LoadRss092() => LoadRss(this.rss092).Channel.Items.Count;

    /// <summary>
    /// Loads the RSS 0.90 document — RDF-rooted and recognised by namespace alone.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. RSS 0.90 (namespace-sniffed)")]
    public int LoadRss090() => LoadRss(this.rss090).Channel.Items.Count;

    /// <summary>
    /// Loads the RSS 1.0 document — the RDF Seq manifest plus Dublin Core at native density.
    /// </summary>
    /// <returns>The number of items parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "e. RSS 1.0 (RDF)")]
    public int LoadRss10() => LoadRss(this.rss10).Channel.Items.Count;

    /// <summary>
    /// Loads the Atom 0.3 document — modified/created/issued dates and both content modes.
    /// </summary>
    /// <returns>The number of entries parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "f. Atom 0.3")]
    public int LoadAtom03()
    {
        AtomFeed feed = new();
        using MemoryStream stream = new(this.atom03, writable: false);
        feed.Load(stream);
        return feed.Entries.Count;
    }

    /// <summary>
    /// Loads the RSD 0.6 discovery document.
    /// </summary>
    /// <returns>The number of APIs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "g. RSD 0.6")]
    public int LoadRsd06()
    {
        RsdDocument document = new();
        using MemoryStream stream = new(this.rsd06, writable: false);
        document.Load(stream);
        return document.Interfaces.Count;
    }

    private static RssFeed LoadRss(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static void Verify(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Corpus guard failed: {description}.");
        }
    }
}