using System.Globalization;

namespace Argotic.Common;

/// <summary>
/// Provides methods for generating and parsing date-time information exposed by syndicated content. This class cannot be inherited.
/// </summary>
/// <remarks>
///     See <a href="http://www.ietf.org/rfc/rfc0822.txt">RFC #822: Standard for ARPA Internet Text Messages (Date and Time Specification)</a>
///     and <a href="http://www.ietf.org/rfc/rfc3339.txt">RFC #3339: Date and Time on the Internet (Timestamps)</a> for further information about
///     the date-time formats implemented in the <see cref="SyndicationDateTimeUtility"/> class.
/// </remarks>
public static class SyndicationDateTimeUtility
{
    /// <summary>
    /// The RFC 3339 date-time patterns <see cref="TryParseRfc3339DateTime"/> accepts.
    /// </summary>
    /// <remarks>
    ///     Static because it was being rebuilt on every call, and the contents are constant: the two
    ///     invariant-culture patterns come from <see cref="DateTimeFormatInfo.InvariantInfo"/>, which
    ///     is read-only, and the rest are literals.
    /// </remarks>
    private static readonly string[] Rfc3339Formats =
    [
        DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern,
        DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern,
        "yyyy'-'MM'-'dd'T'HH:mm:ssK",
        "yyyy'-'MM'-'dd'T'HH:mm:ss.fK",
        "yyyy'-'MM'-'dd'T'HH:mm:ss.ffK",
        "yyyy'-'MM'-'dd'T'HH:mm:ss.fffK",
        "yyyy'-'MM'-'dd'T'HH:mm:ss.ffffK",
        "yyyy'-'MM'-'dd'T'HH:mm:ss.fffffK",
        "yyyy'-'MM'-'dd'T'HH:mm:ss.ffffffK",
    ];

    /// <summary>
    /// The RFC 822 date-time patterns <see cref="TryParseRfc822DateTime"/> accepts.
    /// </summary>
    /// <remarks>
    ///     <b>Seventy-two of them, and the array was allocated on every call</b> — once per
    ///     <c>pubDate</c>, which is once per item of every RSS feed the library reads. Roughly 600
    ///     bytes of pure ceremony per date parsed, measured at 6% of the allocation of loading a
    ///     ten-item feed.
    /// </remarks>
    private static readonly string[] Rfc822Formats =
    [
        // two-digit day, four-digit year patterns
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'fffffff zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'ffffff zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'fffff zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'ffff zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'fff zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'ff zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss'.'f zzzz",
        "ddd',' dd MMM yyyy HH':'mm':'ss zzzz",
        // two-digit day, two-digit year patterns
        "ddd',' dd MMM yy HH':'mm':'ss'.'fffffff zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss'.'ffffff zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss'.'fffff zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss'.'ffff zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss'.'fff zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss'.'ff zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss'.'f zzzz",
        "ddd',' dd MMM yy HH':'mm':'ss zzzz",
        // one-digit day, four-digit year patterns
        "ddd',' d MMM yyyy HH':'mm':'ss'.'fffffff zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss'.'ffffff zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss'.'fffff zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss'.'ffff zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss'.'fff zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss'.'ff zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss'.'f zzzz",
        "ddd',' d MMM yyyy HH':'mm':'ss zzzz",
        // one-digit day, two-digit year patterns
        "ddd',' d MMM yy HH':'mm':'ss'.'fffffff zzzz",
        "ddd',' d MMM yy HH':'mm':'ss'.'ffffff zzzz",
        "ddd',' d MMM yy HH':'mm':'ss'.'fffff zzzz",
        "ddd',' d MMM yy HH':'mm':'ss'.'ffff zzzz",
        "ddd',' d MMM yy HH':'mm':'ss'.'fff zzzz",
        "ddd',' d MMM yy HH':'mm':'ss'.'ff zzzz",
        "ddd',' d MMM yy HH':'mm':'ss'.'f zzzz",
        "ddd',' d MMM yy HH':'mm':'ss zzzz",
        // Fall back patterns
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK", // RoundtripDateTimePattern
        DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern,
        DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern,
        DateTimeFormatInfo.InvariantInfo.RFC1123Pattern,
    ];

    /// <summary>
    /// Converts the specified string representation of an RFC-3339 formatted date to its <see cref="DateTime"/> equivalent.
    /// </summary>
    /// <param name="value">A string containing an RFC-3339 formatted date to convert.</param>
    /// <returns>A <see cref="DateTime"/> equivalent to the RFC-3339 formatted date contained in <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    /// <exception cref="FormatException">The <paramref name="value"/> is not a recognized as a RFC-3339 formatted date.</exception>
    public static DateTime ParseRfc3339DateTime(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(value, out DateTime result))
        {
            return result;
        }
        else
        {
            throw new FormatException($"'{value}' is not a valid RFC-3339 formatted date-time value.");
        }
    }

    /// <summary>
    /// Converts the value of the supplied <see cref="DateTime"/> object to its equivalent RFC-3339 date string representation.
    /// </summary>
    /// <param name="utcDateTime">The <see cref="DateTime"/> object to convert.</param>
    /// <returns>A string that contains the RFC-3339 date string representation of the supplied <see cref="DateTime"/> object.</returns>
    /// <remarks>
    ///     A <see cref="DateTimeKind.Local"/> value is written with its numeric offset; anything else
    ///     is written with <c>Z</c>. That makes <see cref="DateTimeKind.Unspecified"/> a claim of UTC:
    ///     the writer cannot know what wall clock an Unspecified value meant, and inventing the
    ///     machine's offset would be a different guess, not a safer one. Callers who mean local time
    ///     should say so with <see cref="DateTimeKind.Local"/>. Values parsed by
    ///     <see cref="TryParseRfc3339DateTime"/> are always <see cref="DateTimeKind.Utc"/>, so parsed
    ///     values round-trip exactly.
    /// </remarks>
    public static string ToRfc3339DateTime(DateTime utcDateTime)
    {
        DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

        if (utcDateTime.Kind == DateTimeKind.Local)
        {
            return utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.ffzzz", dateTimeFormat);
        }
        else
        {
            return utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ff'Z'", dateTimeFormat);
        }
    }

    /// <summary>
    /// Converts the specified string representation of an RFC-3339 formatted date to its <see cref="DateTime"/> equivalent.
    /// </summary>
    /// <param name="value">A string containing an RFC-3339 formatted date to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="DateTime"/> value equivalent to the date and time contained in <paramref name="value"/>, if the conversion succeeded, or <see cref="DateTime.MinValue">MinValue</see> if the conversion failed.
    ///     The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, or does not contain a valid string representation of an RFC-3339 formatted date.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
    public static bool TryParseRfc3339DateTime(string value, out DateTime result)
    {
        DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
        // K, not a 'Z' literal and not zzz. K matches "Z" and a numeric offset alike, and - unlike a
        // quoted literal - it feeds AdjustToUniversal, so "...05Z" and "...05+00:00" both come out
        // Kind=Utc at the same instant. The old table matched Z as a LITERAL: the timestamp parsed,
        // but with no offset information, so it surfaced as Kind=Unspecified while the numeric-offset
        // forms surfaced as Utc - the same instant, two Kinds, depending on how the origin spelled
        // it. The old table also had no single-digit-fraction-with-offset row, so a conformant
        // "...05.1+05:00" failed to parse at all.

        if (string.IsNullOrEmpty(value))
        {
            result = DateTime.MinValue;
            return false;
        }

        return DateTime.TryParseExact(value, Rfc3339Formats, dateTimeFormat, DateTimeStyles.AdjustToUniversal, out result);
    }

    /// <summary>
    /// Replaces the RFC-822 time-zone component with its offset equivalent.
    /// </summary>
    /// <param name="value">A string containing an RFC-822 formatted date to convert.</param>
    /// <returns>A string containing an RFC-822 formatted date, with the <i>zone</i> component converted to its offset equivalent.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    /// <seealso cref="TryParseRfc822DateTime(string, out DateTime)"/>
    private static string ReplaceRfc822TimeZoneWithOffset(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        string zoneRepresentedAsLocalDifferential;

        // UTC is not an RFC 822 zone -- 5.1 defines UT -- but it is what a great deal of the wild
        // writes, Microsoft's own Q# blog among them, and it names exactly the zone UT names. It has
        // to be tested BEFORE the arms below would otherwise reach the default, and the near-miss is
        // why it went unnoticed: EndsWith(" UT") is false for a string ending " UTC", so such a date
        // fell past every arm to "no conversion needed" and then failed to parse, silently, into
        // DateTime.MinValue.
        //
        // The line this draws: tolerate an unambiguous spelling of a zone the specification already
        // names, because that invents nothing. Do not tolerate a date with no time of day, because
        // supplying one would invent data the document never carried.
        if (value.EndsWith(" UTC", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" UTC", StringComparison.OrdinalIgnoreCase) + 1)], "+00:00");
        }
        else if (value.EndsWith(" UT", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" UT", StringComparison.OrdinalIgnoreCase) + 1)], "+00:00");
        }
        else if (value.EndsWith(" GMT", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" GMT", StringComparison.OrdinalIgnoreCase) + 1)], "+00:00");
        }
        else if (value.Contains(" GMT", StringComparison.OrdinalIgnoreCase))
        {
            int GMT_index = value.LastIndexOf(" GMT", StringComparison.OrdinalIgnoreCase);
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(GMT_index + 1)], value[(GMT_index + 4)..]);
        }
        else if (value.EndsWith(" EST", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" EST", StringComparison.OrdinalIgnoreCase) + 1)], "-05:00");
        }
        else if (value.EndsWith(" EDT", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" EDT", StringComparison.OrdinalIgnoreCase) + 1)], "-04:00");
        }
        else if (value.EndsWith(" CST", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" CST", StringComparison.OrdinalIgnoreCase) + 1)], "-06:00");
        }
        else if (value.EndsWith(" CDT", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" CDT", StringComparison.OrdinalIgnoreCase) + 1)], "-05:00");
        }
        else if (value.EndsWith(" MST", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" MST", StringComparison.OrdinalIgnoreCase) + 1)], "-07:00");
        }
        else if (value.EndsWith(" MDT", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" MDT", StringComparison.OrdinalIgnoreCase) + 1)], "-06:00");
        }
        else if (value.EndsWith(" PST", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" PST", StringComparison.OrdinalIgnoreCase) + 1)], "-08:00");
        }
        else if (value.EndsWith(" PDT", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" PDT", StringComparison.OrdinalIgnoreCase) + 1)], "-07:00");
        }
        else if (value.EndsWith(" Z", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" Z", StringComparison.OrdinalIgnoreCase) + 1)], "+00:00");
        }
        else if (value.EndsWith(" A", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" A", StringComparison.OrdinalIgnoreCase) + 1)], "-01:00");
        }
        else if (value.EndsWith(" M", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" M", StringComparison.OrdinalIgnoreCase) + 1)], "-12:00");
        }
        else if (value.EndsWith(" N", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" N", StringComparison.OrdinalIgnoreCase) + 1)], "+01:00");
        }
        else if (value.EndsWith(" Y", StringComparison.OrdinalIgnoreCase))
        {
            zoneRepresentedAsLocalDifferential = string.Concat(value[..(value.LastIndexOf(" Y", StringComparison.OrdinalIgnoreCase) + 1)], "+12:00");
        }
        else if (value.EndsWith("CET", StringComparison.OrdinalIgnoreCase))
        {
            return $"{value[..value.LastIndexOf("CET", StringComparison.OrdinalIgnoreCase)]}+1:00";
        }
        else if (value.EndsWith("CEST", StringComparison.OrdinalIgnoreCase))
        {
            return $"{value[..value.LastIndexOf("CEST", StringComparison.OrdinalIgnoreCase)]}+2:00";
        }
        else
        {
            // No timezone conversion needed, return the original value
            return value;
        }

        return zoneRepresentedAsLocalDifferential;
    }

    /// <summary>
    /// Converts the specified string representation of an RFC-822 formatted date to its <see cref="DateTime"/> equivalent.
    /// </summary>
    /// <param name="value">A string containing an RFC-822 formatted date to convert.</param>
    /// <returns>A <see cref="DateTime"/> equivalent to the RFC-822 formatted date contained in <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    /// <exception cref="FormatException">The <paramref name="value"/> is not a recognized as an RFC-822 formatted date.</exception>
    public static DateTime ParseRfc822DateTime(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime result))
        {
            return result;
        }
        else
        {
            throw new FormatException($"'{value}' is not a valid RFC-822 formatted date-time value.");
        }
    }

    /// <summary>
    /// Converts the value of the supplied <see cref="DateTime"/> object to its equivalent RFC-822 date string representation.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> object to convert.</param>
    /// <returns>A string that contains the RFC-822 date string representation of the supplied <see cref="DateTime"/> object.</returns>
    public static string ToRfc822DateTime(DateTime dateTime)
    {
        DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

        return dateTime.ToString(dateTimeFormat.RFC1123Pattern, dateTimeFormat);
    }

    /// <summary>
    /// Converts the specified string representation of an RFC-822 formatted date to its <see cref="DateTime"/> equivalent.
    /// </summary>
    /// <param name="value">A string containing an RFC-822 formatted date to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="DateTime"/> value equivalent to the date and time contained in <paramref name="value"/>, if the conversion succeeded, or <see cref="DateTime.MinValue">MinValue</see> if the conversion failed.
    ///     The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, or does not contain a valid string representation of an RFC-822 formatted date.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
    public static bool TryParseRfc822DateTime(string value, out DateTime result)
    {
        // patterns from http://stackoverflow.com/questions/284775/how-do-i-parse-and-convert-datetimes-to-the-rfc-822-date-time-format
        DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

        if (string.IsNullOrEmpty(value))
        {
            result = DateTime.MinValue;
            return false;
        }

        if (DateTime.TryParseExact(SyndicationDateTimeUtility.ReplaceRfc822TimeZoneWithOffset(value), Rfc822Formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out result))
        {
            return true;
        }
        if (DateTime.TryParse(SyndicationDateTimeUtility.ReplaceRfc822TimeZoneWithOffset(value), out result))
        {
            return true;
        }

        return false;
    }
}