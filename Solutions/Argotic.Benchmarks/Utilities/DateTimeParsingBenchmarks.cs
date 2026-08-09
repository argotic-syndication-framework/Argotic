using System.Diagnostics.CodeAnalysis;
using Argotic.Common;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Utilities;

/// <summary>
/// Measures <see cref="SyndicationDateTimeUtility"/>, which runs once per item of every feed read.
/// </summary>
/// <remarks>
/// <para>
/// This path was previously unmeasured, which is how a 36-entry format array came to be allocated on
/// every call without being noticed. The array is now static, but the walk over it has never been
/// measured, and the walk is what the caller pays.
/// </para>
/// <para>
/// The arms are chosen so the three candidate changes can each be refuted independently, and every
/// one of them is a date shape a real publisher emits:
/// </para>
/// <list type="bullet">
///   <item><description>Named zone versus numeric offset. A named zone goes through
///   <c>ReplaceRfc822TimeZoneWithOffset</c>, which rebuilds the string; a numeric offset returns the
///   original instance untouched and allocates nothing. An arm of only one of them
///   would price the rewrite at either all or nothing.</description></item>
///   <item><description>Where the matching pattern sits in the table.
///   <see cref="DateTime.TryParseExact(string, string[], IFormatProvider, System.Globalization.DateTimeStyles, out DateTime)"/>
///   walks the array in order, so the index of the first match is the number of attempts. The
///   dominant RSS shape matches at index 7 - the seven leading patterns all carry fractional seconds,
///   which RSS dates essentially never have - and a single-digit day matches at index 23. Both are
///   measured, because reordering the table helps them by different amounts.</description></item>
///   <item><description>The fallback path. A shape the table cannot match walks all 36
///   patterns and then calls <c>DateTime.TryParse</c> - and calls the zone replacement a second time
///   to do it, because the result of the first call is not held. The two shapes that reach it here
///   are legal RFC 822: the specification makes both the day-of-week and the seconds
///   optional.</description></item>
/// </list>
/// <para>
/// RFC 3339 is included to be refuted rather than because it is suspected: that path allocates
/// nothing at any shape measured, and an arm that finds no problem is worth keeping so the claim
/// stays testable.
/// </para>
/// </remarks>
[BenchmarkCategory("utilities", "datetime")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class DateTimeParsingBenchmarks
{
    // Held as fields rather than literals at the call site so the JIT cannot fold the argument and
    // leave the benchmark measuring constant propagation instead of parsing.
    private readonly string namedZone = "Mon, 01 Jan 2024 10:00:00 GMT";
    private readonly string numericOffset = "Mon, 01 Jan 2024 10:00:00 -0500";
    private readonly string singleDigitDay = "Tue, 2 Jan 2024 10:00:00 GMT";
    private readonly string noDayOfWeek = "01 Jan 2024 10:00:00 GMT";
    private readonly string noSeconds = "Mon, 01 Jan 2024 10:00 GMT";
    private readonly string unparseable = "sometime last Thursday";
    private readonly string rfc3339 = "2024-01-01T10:00:00Z";
    private readonly string rfc3339Fractional = "2024-01-01T10:00:00.123+05:00";
    private readonly DateTime instant = new(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The dominant RSS shape: four-digit year, two-digit day, named zone. Matches at index 7.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Baseline = true, Description = "RFC 822, named zone (the dominant RSS shape)")]
    public bool ParseNamedZone() => SyndicationDateTimeUtility.TryParseRfc822DateTime(this.namedZone, out _);

    /// <summary>
    /// The same shape with a numeric offset, which skips the string rebuild entirely.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 822, numeric offset (no zone rewrite)")]
    public bool ParseNumericOffset() => SyndicationDateTimeUtility.TryParseRfc822DateTime(this.numericOffset, out _);

    /// <summary>
    /// A single-digit day, which matches three groups further down the table at index 23.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 822, single-digit day (24 pattern attempts)")]
    public bool ParseSingleDigitDay() => SyndicationDateTimeUtility.TryParseRfc822DateTime(this.singleDigitDay, out _);

    /// <summary>
    /// Legal RFC 822 with the optional day-of-week omitted: no pattern matches, so the fallback runs.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 822, no day-of-week (legal; falls through all 36)")]
    public bool ParseNoDayOfWeek() => SyndicationDateTimeUtility.TryParseRfc822DateTime(this.noDayOfWeek, out _);

    /// <summary>
    /// Legal RFC 822 with the optional seconds omitted: also falls through to the fallback.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 822, no seconds (legal; falls through all 36)")]
    public bool ParseNoSeconds() => SyndicationDateTimeUtility.TryParseRfc822DateTime(this.noSeconds, out _);

    /// <summary>
    /// The worst case: every pattern is tried, the fallback is tried, and both fail.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 822, unparseable (worst case)")]
    public bool ParseUnparseable() => SyndicationDateTimeUtility.TryParseRfc822DateTime(this.unparseable, out _);

    /// <summary>
    /// The Atom shape, expected to allocate nothing.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 3339, UTC designator (Atom)")]
    public bool ParseRfc3339() => SyndicationDateTimeUtility.TryParseRfc3339DateTime(this.rfc3339, out _);

    /// <summary>
    /// An RFC 3339 value carrying both a fraction and a numeric offset.
    /// </summary>
    /// <returns>Whether the value parsed.</returns>
    [Benchmark(Description = "RFC 3339, fractional seconds with offset")]
    public bool ParseRfc3339Fractional() => SyndicationDateTimeUtility.TryParseRfc3339DateTime(this.rfc3339Fractional, out _);

    /// <summary>
    /// The write path, run once per date on every save.
    /// </summary>
    /// <returns>The formatted value.</returns>
    [Benchmark(Description = "ToRfc822DateTime (write path)")]
    public string WriteRfc822() => SyndicationDateTimeUtility.ToRfc822DateTime(this.instant);

    /// <summary>
    /// The Atom write path.
    /// </summary>
    /// <returns>The formatted value.</returns>
    [Benchmark(Description = "ToRfc3339DateTime (write path)")]
    public string WriteRfc3339() => SyndicationDateTimeUtility.ToRfc3339DateTime(this.instant);
}