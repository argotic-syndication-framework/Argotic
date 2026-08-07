using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads a <c>sitemapindex</c> at the sizes the protocol permits, with the <c>lastmod</c> shape as
/// the only thing that differs between arms.
/// </summary>
/// <remarks>
/// <para>
/// The sitemap protocol caps an index at 50,000 <c>sitemap</c> elements, the same ceiling it
/// puts on a <c>urlset</c>, and <see cref="SitemapIndex"/> documents that limit in its own remarks. An
/// index at the ceiling addresses 2.5 billion URLs; it is the shape a site of any real size publishes,
/// and it is refetched by every crawler on every crawl. <c>SitemapScaleBenchmarks</c> took the
/// <c>urlset</c> to that ceiling. Nothing had taken the index there, and the two are different
/// documents parsed by different code.
/// </para>
/// <para>
/// All three arms vary along <see cref="SitemapCount"/>, and they differ from one another in
/// exactly one element. That constraint is deliberate. A class-scoped axis that most arms ignore
/// produces rows which merely reproduce the parameter, carrying no information; here the entry count,
/// the location text and the document structure are identical across the three arms, and the only
/// difference is how, or whether, each entry spells its <c>lastmod</c>.
/// </para>
/// <para>
/// All three spellings are legal. The protocol defers to the W3C Datetime profile, which makes the
/// time part optional, and the element itself is optional. They reach the parser by three paths of
/// very different length:
/// </para>
/// <list type="bullet">
///   <item><description>No <c>lastmod</c> — <c>SelectChildElement</c> returns null and the date
///   parser is never entered. One <c>Uri.TryCreate</c> per entry and nothing else.</description></item>
///   <item><description>Full RFC 3339 — <c>TryParseRfc3339DateTime</c> matches inside its
///   nine-pattern table. The delta against the first arm is the whole cost of the date column.</description></item>
///   <item><description>Date only — fails all nine patterns, because every one of them requires
///   a time, and falls through to
///   <see cref="DateTime.TryParse(string, IFormatProvider, System.Globalization.DateTimeStyles, out DateTime)"/>.
///   Its delta against the second arm is the cost of the fallback, and at the ceiling that is
///   450,000 failed pattern matches in one document.</description></item>
/// </list>
/// <para>
/// What would refute the premise: the three arms landing together across the whole sweep, which would
/// mean the date column is lost inside document construction and the pattern table is not worth
/// reordering. What would confirm it: a gap that widens with the axis, which would put a number on
/// what a publisher's choice of date spelling costs the crawler reading it.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "sitemap", "scale")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SitemapIndexScaleBenchmarks
{
    private byte[] withoutLastModified = [];
    private byte[] withRfc3339LastModified = [];
    private byte[] withDateOnlyLastModified = [];

    /// <summary>
    /// Gets or sets the number of <c>sitemap</c> entries in the index under test.
    /// </summary>
    /// <remarks>
    ///     50,000 is the protocol maximum for an index, not an arbitrary large number. 200 is the
    ///     bottom of the sweep because it is roughly what a ten-million-page site actually publishes,
    ///     so the low end is a real working size rather than a toy.
    /// </remarks>
    [Params(200, 5_000, 50_000)]
    public int SitemapCount { get; set; }

    /// <summary>
    /// Generates the three documents once per parameter value.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.withoutLastModified = FeedCorpus.GenerateSitemapIndexUtf8(this.SitemapCount, SitemapIndexLastModified.None);
        this.withRfc3339LastModified = FeedCorpus.GenerateSitemapIndexUtf8(this.SitemapCount, SitemapIndexLastModified.Rfc3339);
        this.withDateOnlyLastModified = FeedCorpus.GenerateSitemapIndexUtf8(this.SitemapCount, SitemapIndexLastModified.DateOnly);
    }

    /// <summary>
    /// N entries carrying a location and no <c>lastmod</c>.
    /// </summary>
    /// <returns>The number of entries parsed, so the load cannot be elided.</returns>
    /// <remarks>
    ///     The baseline: everything the parser must do for an index entry, with the optional element
    ///     absent. Whatever the other two arms cost above this is the date column and nothing else.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "no lastmod (date parser never entered)")]
    public int LoadWithoutLastModified() => Load(this.withoutLastModified);

    /// <summary>
    /// The same N entries, each carrying a full RFC 3339 <c>lastmod</c>.
    /// </summary>
    /// <returns>The number of entries parsed.</returns>
    /// <remarks>
    ///     The spelling gov.uk uses, and the one the nine-pattern table can match. The delta against
    ///     the baseline is what a successful date parse costs per entry — the same call
    ///     <c>DateTimeParsingBenchmarks</c> prices in isolation, so the two together say how much of
    ///     it is the parser and how much is the element around it.
    /// </remarks>
    [Benchmark(Description = "RFC 3339 lastmod (matches the pattern table)")]
    public int LoadWithRfc3339LastModified() => Load(this.withRfc3339LastModified);

    /// <summary>
    /// The same N entries, each carrying a legal date-only <c>lastmod</c>.
    /// </summary>
    /// <returns>The number of entries parsed.</returns>
    /// <remarks>
    ///     <para>
    ///     Every one of the nine RFC 3339 patterns requires a time, so this shape fails all nine and
    ///     is then parsed successfully by <c>DateTime.TryParse</c>. The result is correct; the route
    ///     to it is nine wasted attempts per entry.
    ///     </para>
    ///     <para>
    ///     This is not a malformed-input arm. <c>2024-01-15</c> is exactly what the sitemap
    ///     protocol's own documentation shows for <c>lastmod</c>, so this is the common case
    ///     taking the long path, not an edge case. If the gap against the previous arm is large at
    ///     50,000 entries, the finding is that the pattern table should test a cheap shape guard
    ///     before walking itself.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "date-only lastmod (9 failed patterns, then the fallback)")]
    public int LoadWithDateOnlyLastModified() => Load(this.withDateOnlyLastModified);

    private static int Load(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        SitemapIndex index = new();
        index.Load(stream);

        return index.Sitemaps.Count;
    }
}