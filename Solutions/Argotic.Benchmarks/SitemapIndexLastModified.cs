namespace Argotic.Benchmarks;

/// <summary>
/// Which of the three legal spellings of a sitemap index entry's <c>lastmod</c> to emit.
/// </summary>
/// <remarks>
///     All three are permitted by the sitemap protocol, which defers to the W3C Datetime profile and
///     makes the time part optional. They are separated here because <c>SitemapIndexEntry.Load</c>
///     reaches them by three different paths of very different length, and a corpus that emitted only
///     one of them would price the element at whichever path it happened to pick.
/// </remarks>
internal enum SitemapIndexLastModified
{
    /// <summary>
    /// No <c>lastmod</c> element at all: the date parser is never entered.
    /// </summary>
    None = 0,

    /// <summary>
    /// A full RFC 3339 timestamp, which the nine-pattern table matches.
    /// </summary>
    Rfc3339 = 1,

    /// <summary>
    /// A date with no time, which fails all nine patterns and falls through to <c>DateTime.TryParse</c>.
    /// </summary>
    DateOnly = 2,
}