﻿namespace Argotic.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

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
        /// Converts the specified string representation of a RFC-3339 formatted date to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a RFC-3339 formatted date to convert.</param>
        /// <returns>A <see cref="DateTime"/> equivalent to the RFC-3339 formatted date contained in <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        /// <exception cref="FormatException">The <paramref name="value"/> is not a recognized as a RFC-3339 formatted date.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc")]
        public static DateTime ParseRfc3339DateTime(string value)
        {
            DateTime result = DateTime.MinValue;

            Guard.ArgumentNotNullOrEmptyString(value, "value");

            if (TryParseRfc3339DateTime(value, out result))
            {
                return result;
            }

            throw new FormatException(string.Format(null, "'{0}' is not a valid RFC-3339 formatted date-time value.", value));
        }

        /// <summary>
        /// Converts the value of the supplied <see cref="DateTime"/> object to its equivalent RFC-3339 date string representation.
        /// </summary>
        /// <param name="utcDateTime">The UTC <see cref="DateTime"/> object to convert.</param>
        /// <returns>A string that contains the RFC-3339 date string representation of the supplied <see cref="DateTime"/> object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc")]
        public static string ToRfc3339DateTime(DateTime utcDateTime)
        {
            DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

            if (utcDateTime.Kind == DateTimeKind.Local)
            {
                return utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.ffzzz", dateTimeFormat);
            }

            return utcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ff'Z'", dateTimeFormat);
        }

        /// <summary>
        /// Converts the specified string representation of a RFC-3339 formatted date to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a RFC-3339 formatted date to convert.</param>
        /// <param name="result">
        ///     When this method returns, contains the <see cref="DateTime"/> value equivalent to the date and time contained in <paramref name="value"/>, if the conversion succeeded, or <see cref="DateTime.MinValue">MinValue</see> if the conversion failed.
        ///     The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, or does not contain a valid string representation of a RFC-3339 formatted date.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc")]
        public static bool TryParseRfc3339DateTime(string value, out DateTime result)
        {
            DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
            string[] formats = new string[15];

            formats[0] = dateTimeFormat.SortableDateTimePattern;
            formats[1] = dateTimeFormat.UniversalSortableDateTimePattern;
            formats[2] = "yyyy'-'MM'-'dd'T'HH:mm:ss'Z'";
            formats[3] = "yyyy'-'MM'-'dd'T'HH:mm:ss.f'Z'";
            formats[4] = "yyyy'-'MM'-'dd'T'HH:mm:ss.ff'Z'";
            formats[5] = "yyyy'-'MM'-'dd'T'HH:mm:ss.fff'Z'";
            formats[6] = "yyyy'-'MM'-'dd'T'HH:mm:ss.ffff'Z'";
            formats[7] = "yyyy'-'MM'-'dd'T'HH:mm:ss.fffff'Z'";
            formats[8] = "yyyy'-'MM'-'dd'T'HH:mm:ss.ffffff'Z'";
            formats[9] = "yyyy'-'MM'-'dd'T'HH:mm:sszzz";
            formats[10] = "yyyy'-'MM'-'dd'T'HH:mm:ss.ffzzz";
            formats[11] = "yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz";
            formats[12] = "yyyy'-'MM'-'dd'T'HH:mm:ss.ffffzzz";
            formats[13] = "yyyy'-'MM'-'dd'T'HH:mm:ss.fffffzzz";
            formats[14] = "yyyy'-'MM'-'dd'T'HH:mm:ss.ffffffzzz";

            if (string.IsNullOrEmpty(value))
            {
                result = DateTime.MinValue;
                return false;
            }

            return DateTime.TryParseExact(value, formats, dateTimeFormat, DateTimeStyles.AdjustToUniversal, out result);
        }

        /// <summary>
        /// Converts the specified string representation of a RFC-822 formatted date to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a RFC-822 formatted date to convert.</param>
        /// <returns>A <see cref="DateTime"/> equivalent to the RFC-822 formatted date contained in <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        /// <exception cref="FormatException">The <paramref name="value"/> is not a recognized as a RFC-822 formatted date.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc")]
        public static DateTime ParseRfc822DateTime(string value)
        {
            DateTime result = DateTime.MinValue;

            Guard.ArgumentNotNullOrEmptyString(value, "value");

            if (TryParseRfc822DateTime(value, out result))
            {
                return result;
            }

            throw new FormatException(string.Format(null, "'{0}' is not a valid RFC-822 formatted date-time value.", value));
        }

        /// <summary>
        /// Converts the value of the supplied <see cref="DateTime"/> object to its equivalent RFC-822 date string representation.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object to convert.</param>
        /// <returns>A string that contains the RFC-822 date string representation of the supplied <see cref="DateTime"/> object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc")]
        public static string ToRfc822DateTime(DateTime dateTime)
        {
            DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

            return dateTime.ToString(dateTimeFormat.RFC1123Pattern, dateTimeFormat);
        }

        /// <summary>
        /// Converts the specified string representation of a RFC-822 formatted date to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a RFC-822 formatted date to convert.</param>
        /// <param name="result">
        ///     When this method returns, contains the <see cref="DateTime"/> value equivalent to the date and time contained in <paramref name="value"/>, if the conversion succeeded, or <see cref="DateTime.MinValue">MinValue</see> if the conversion failed.
        ///     The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, or does not contain a valid string representation of a RFC-822 formatted date.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rfc")]
        public static bool TryParseRfc822DateTime(string value, out DateTime result)
        {
            // patterns from http://stackoverflow.com/questions/284775/how-do-i-parse-and-convert-datetimes-to-the-rfc-822-date-time-format
            DateTimeFormatInfo dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
            string[] formats = new string[36];

            // two-digit day, four-digit year patterns
            formats[0] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'fffffff zzzz";
            formats[1] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'ffffff zzzz";
            formats[2] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'fffff zzzz";
            formats[3] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'ffff zzzz";
            formats[4] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'fff zzzz";
            formats[5] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'ff zzzz";
            formats[6] = "ddd',' dd MMM yyyy HH':'mm':'ss'.'f zzzz";
            formats[7] = "ddd',' dd MMM yyyy HH':'mm':'ss zzzz";

            // two-digit day, two-digit year patterns
            formats[8] = "ddd',' dd MMM yy HH':'mm':'ss'.'fffffff zzzz";
            formats[9] = "ddd',' dd MMM yy HH':'mm':'ss'.'ffffff zzzz";
            formats[10] = "ddd',' dd MMM yy HH':'mm':'ss'.'fffff zzzz";
            formats[11] = "ddd',' dd MMM yy HH':'mm':'ss'.'ffff zzzz";
            formats[12] = "ddd',' dd MMM yy HH':'mm':'ss'.'fff zzzz";
            formats[13] = "ddd',' dd MMM yy HH':'mm':'ss'.'ff zzzz";
            formats[14] = "ddd',' dd MMM yy HH':'mm':'ss'.'f zzzz";
            formats[15] = "ddd',' dd MMM yy HH':'mm':'ss zzzz";

            // one-digit day, four-digit year patterns
            formats[16] = "ddd',' d MMM yyyy HH':'mm':'ss'.'fffffff zzzz";
            formats[17] = "ddd',' d MMM yyyy HH':'mm':'ss'.'ffffff zzzz";
            formats[18] = "ddd',' d MMM yyyy HH':'mm':'ss'.'fffff zzzz";
            formats[19] = "ddd',' d MMM yyyy HH':'mm':'ss'.'ffff zzzz";
            formats[20] = "ddd',' d MMM yyyy HH':'mm':'ss'.'fff zzzz";
            formats[21] = "ddd',' d MMM yyyy HH':'mm':'ss'.'ff zzzz";
            formats[22] = "ddd',' d MMM yyyy HH':'mm':'ss'.'f zzzz";
            formats[23] = "ddd',' d MMM yyyy HH':'mm':'ss zzzz";

            // two-digit day, two-digit year patterns
            formats[24] = "ddd',' d MMM yy HH':'mm':'ss'.'fffffff zzzz";
            formats[25] = "ddd',' d MMM yy HH':'mm':'ss'.'ffffff zzzz";
            formats[26] = "ddd',' d MMM yy HH':'mm':'ss'.'fffff zzzz";
            formats[27] = "ddd',' d MMM yy HH':'mm':'ss'.'ffff zzzz";
            formats[28] = "ddd',' d MMM yy HH':'mm':'ss'.'fff zzzz";
            formats[29] = "ddd',' d MMM yy HH':'mm':'ss'.'ff zzzz";
            formats[30] = "ddd',' d MMM yy HH':'mm':'ss'.'f zzzz";
            formats[31] = "ddd',' d MMM yy HH':'mm':'ss zzzz";

            // Fall back patterns
            formats[32] = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK"; // RoundtripDateTimePattern
            formats[33] = DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern;
            formats[34] = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;
            formats[35] = dateTimeFormat.RFC1123Pattern;

            if (string.IsNullOrEmpty(value))
            {
                result = DateTime.MinValue;
                return false;
            }

            if (DateTime.TryParseExact(ReplaceRfc822TimeZoneWithOffset(value), formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out result))
            {
                return true;
            }

            if (DateTime.TryParse(ReplaceRfc822TimeZoneWithOffset(value), out result))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces the RFC-822 time-zone component with its offset equivalent.
        /// </summary>
        /// <param name="value">A string containing a RFC-822 formatted date to convert.</param>
        /// <returns>A string containing a RFC-822 formatted date, with the <i>zone</i> component converted to its offset equivalent.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        /// <seealso cref="TryParseRfc822DateTime(string, out DateTime)"/>
        private static string ReplaceRfc822TimeZoneWithOffset(string value)
        {
            string zoneRepresentedAsLocalDifferential = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("s");
            }

            if (value.EndsWith(" UT", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" UT") + 1), "+00:00");
            }
            else if (value.EndsWith(" GMT", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" GMT") + 1), "+00:00");
            }
            else if (value.Contains(" GMT"))
            {
                int gmtIndex = value.LastIndexOf(" GMT");
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, gmtIndex + 1), value.Substring(gmtIndex + 4, value.Length - gmtIndex - 4));
            }
            else if (value.EndsWith(" EST", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" EST") + 1), "-05:00");
            }
            else if (value.EndsWith(" EDT", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" EDT") + 1), "-04:00");
            }
            else if (value.EndsWith(" CST", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" CST") + 1), "-06:00");
            }
            else if (value.EndsWith(" CDT", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" CDT") + 1), "-05:00");
            }
            else if (value.EndsWith(" MST", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" MST") + 1), "-07:00");
            }
            else if (value.EndsWith(" MDT", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" MDT") + 1), "-06:00");
            }
            else if (value.EndsWith(" PST", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" PST") + 1), "-08:00");
            }
            else if (value.EndsWith(" PDT", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" PDT") + 1), "-07:00");
            }
            else if (value.EndsWith(" Z", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" Z") + 1), "+00:00");
            }
            else if (value.EndsWith(" A", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" A") + 1), "-01:00");
            }
            else if (value.EndsWith(" M", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" M") + 1), "-12:00");
            }
            else if (value.EndsWith(" N", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" N") + 1), "+01:00");
            }
            else if (value.EndsWith(" Y", StringComparison.OrdinalIgnoreCase))
            {
                zoneRepresentedAsLocalDifferential = string.Concat(value.Substring(0, value.LastIndexOf(" Y") + 1), "+12:00");
            }
            else if (value.EndsWith("CET", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(null, "{0}+1:00", value.TrimEnd("CET".ToCharArray()));
            }
            else if (value.EndsWith("CEST", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(null, "{0}+2:00", value.TrimEnd("CEST".ToCharArray()));
            }
            else
            {
                zoneRepresentedAsLocalDifferential = value;
            }

            return zoneRepresentedAsLocalDifferential;
        }
    }
}
