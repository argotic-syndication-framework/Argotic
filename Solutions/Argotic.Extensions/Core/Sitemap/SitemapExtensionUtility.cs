using System.Globalization;

namespace Argotic.Extensions.Core;

/// <summary>
/// Provides date formatting shared by the sitemap extension writers.
/// </summary>
internal static class SitemapExtensionUtility
{
    /// <summary>
    /// Formats a date for a sitemap extension element, in UTC, with the <c>Z</c> designator.
    /// </summary>
    /// <param name="dateTime">The date to format. An <see cref="DateTimeKind.Unspecified"/> kind is a claim of UTC.</param>
    /// <returns>The date in the form <c>2026-08-10T05:30:00Z</c>.</returns>
    /// <remarks>
    ///     <para>
    ///     The writers used <c>zzz</c> here. On .NET 10, <c>zzz</c> writes <c>+00:00</c> for a
    ///     <see cref="DateTimeKind.Utc"/> value on every machine, but it stamps the machine's own
    ///     offset on a <see cref="DateTimeKind.Local"/> or <see cref="DateTimeKind.Unspecified"/> wall
    ///     clock. The load paths parse with <c>AssumeUniversal | AdjustToUniversal</c>, so an
    ///     Unspecified value written on a non-UTC machine came back as a different instant. This
    ///     method normalises the kind first. The output is then identical on every machine.
    ///     </para>
    ///     <para>
    ///     The kind must be normalised before the format call. The <c>K</c> specifier writes <c>Z</c>
    ///     for a Utc kind, but it writes nothing for an Unspecified kind.
    ///     </para>
    ///     <para>
    ///     This method does not share <c>SyndicationDateTimeUtility.ToRfc3339DateTime</c>. That method
    ///     writes fractional seconds, and it keeps a numeric offset for a Local kind — both are
    ///     correct for Atom, and both are forms issue 177 asks this writer to avoid.
    ///     </para>
    /// </remarks>
    public static string ToSitemapDateTime(DateTime dateTime)
    {
        DateTime utc = dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime,
        };

        return utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", CultureInfo.InvariantCulture);
    }
}