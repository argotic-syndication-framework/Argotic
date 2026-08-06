using Argotic.Common;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the timezone spellings RFC 822 does not define but the wild uses anyway.
/// </summary>
/// <remarks>
///     <para>
///     Found by auditing the Azure Weekly corpus: Microsoft's own Q# blog dates every item
///     <c>Mon, 03 Aug 2026 21:17:00 UTC</c>, and every one of them parsed to
///     <see cref="DateTime.MinValue"/>. RFC 822 §5.1 defines <c>UT</c>, not <c>UTC</c>, so the zone
///     table had no branch for it — and the near-miss is worse than an obvious gap, because
///     <c>EndsWith(" UT")</c> is <b>false</b> for a string ending <c>" UTC"</c>, so it fell past every
///     arm to the "no conversion needed" default and then failed to parse.
///     </para>
///     <para>
///     <b>The line this draws.</b> <c>UTC</c> is tolerated because it is an unambiguous spelling of a
///     zone the specification already names: accepting it invents nothing. A date-only
///     <c>&lt;updated&gt;2026-07-19&lt;/updated&gt;</c> — also in the corpus, also invalid, also
///     silently <see cref="DateTime.MinValue"/> — is <i>not</i> tolerated, because supplying it would
///     mean inventing a time of day the document never stated. Tolerate spellings; do not invent
///     data.
///     </para>
/// </remarks>
[TestClass]
public sealed class Rfc822ZoneSpellingTests
{
    /// <summary>
    /// A UTC-suffixed RFC 822 date parses to the instant it names.
    /// </summary>
    [TestMethod]
    public void AUtcSuffixedDate_Parses()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("Mon, 03 Aug 2026 21:17:00 UTC", out DateTime result)
            .ShouldBeTrue("Microsoft's own Q# blog dates every item this way");

        result.ShouldBe(new DateTime(2026, 8, 3, 21, 17, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// UTC and the RFC's own UT agree, as two spellings of one zone must.
    /// </summary>
    /// <remarks>
    ///     The pairing is the point: if these ever disagreed, one of the two branches would be
    ///     applying an offset the other does not.
    /// </remarks>
    [TestMethod]
    public void UtcAndUt_AgreeOnTheInstant()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("Mon, 03 Aug 2026 21:17:00 UTC", out DateTime utc).ShouldBeTrue();
        SyndicationDateTimeUtility.TryParseRfc822DateTime("Mon, 03 Aug 2026 21:17:00 UT", out DateTime ut).ShouldBeTrue();

        utc.ShouldBe(ut);
    }

    /// <summary>
    /// The zones RFC 822 does define still parse, and still to different instants.
    /// </summary>
    /// <remarks>
    ///     The control. Without it the rows above are equally consistent with "every zone suffix is
    ///     now treated as UTC", which would silently shift every North American feed.
    /// </remarks>
    [TestMethod]
    [DataRow("Mon, 03 Aug 2026 21:17:00 GMT", 21, 17)]
    [DataRow("Mon, 03 Aug 2026 21:17:00 EST", 2, 17)]
    [DataRow("Mon, 03 Aug 2026 21:17:00 PDT", 4, 17)]
    [DataRow("Mon, 03 Aug 2026 21:17:00 +0530", 15, 47)]
    public void TheDefinedZones_StillParseToTheirOwnInstants(string value, int expectedHour, int expectedMinute)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime result).ShouldBeTrue();

        result.Hour.ShouldBe(expectedHour);
        result.Minute.ShouldBe(expectedMinute);
    }

    /// <summary>
    /// A date with no time of day is still refused, deliberately.
    /// </summary>
    /// <remarks>
    ///     Also in the corpus — an Atom feed whose every <c>updated</c> is <c>2026-07-19</c>, which
    ///     RFC 4287 §3.3 does not permit: it requires an RFC 3339 <c>date-time</c>, and this is a
    ///     <c>full-date</c>. Accepting it would mean choosing a time of day on the author's behalf.
    ///     Pinned so the decision is visible rather than incidental.
    /// </remarks>
    [TestMethod]
    public void ADateWithNoTimeOfDay_IsRefused()
    {
        SyndicationDateTimeUtility.TryParseRfc3339DateTime("2026-07-19", out DateTime result)
            .ShouldBeFalse("tolerating a spelling is not the same as inventing a time");

        result.ShouldBe(DateTime.MinValue);
    }
}