using Argotic.Common;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the date spellings real publishers emit, taken verbatim from a corpus of 136 live documents.
/// </summary>
/// <remarks>
///     <para>
///     Every literal here is a byte-exact value from a real feed, and each names the document it came
///     from. That provenance is the point: a corpus you author yourself cannot surprise you about the
///     conventions you already hold, and every one of these spellings is something no generator in this
///     repository would have produced. <c>Rfc822TimeZoneTests</c> and <c>Rfc822ZoneSpellingTests</c>
///     already cover the named-zone table; this file covers what is left, which is the parts of RFC 822
///     that publishers get creative with.
///     </para>
///     <para>
///     <b>These are not regression tests for a fix.</b> Everything asserted here already passed when the
///     file was written. They exist because the shapes were entirely absent from the suite, so nothing
///     would have caught a change to them — and because the date parsers are the largest identified
///     optimisation target in the library (§2.43, §5.1 item 5). Any shape-test dispatch written to make
///     that table faster decides which of these spellings still parses, and this file is the list it has
///     to satisfy.
///     </para>
/// </remarks>
[TestClass]
public sealed class RealWorldDateSpellingTests
{
    /// <summary>
    /// RFC 822's day and month names are abbreviations, and real feeds write them out in full.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     RFC 822 §5.1 gives <c>day</c> as <c>"Mon" / "Tue" / …</c> and <c>month</c> as
    ///     <c>"Jan" / "Feb" / …</c> — three letters, no alternative. Publishers ignore this.
    ///     </para>
    ///     <para>
    ///     Provenance: the full day names are from
    ///     <c>extensions/creativecommons/microsoft-cloud-show.xml</c>, which spells <em>every</em> one of
    ///     its 63 publication dates that way. The full month name is from
    ///     <c>rss09x/wired-netcenter-rss091.xml</c> — Wired's RSS 0.91 feed from 2001 — which also drops
    ///     the seconds and uses a named zone, three deviations in one value.
    ///     </para>
    /// </remarks>
    /// <param name="spelling">The date exactly as the feed writes it.</param>
    /// <param name="year">The year of the UTC instant the spelling names.</param>
    /// <param name="month">The month of that instant.</param>
    /// <param name="day">The day of that instant.</param>
    /// <param name="hour">The hour of that instant, after the row's offset has been applied.</param>
    /// <param name="minute">The minute of that instant.</param>
    /// <param name="second">The second of that instant.</param>
    /// <param name="shape">What makes the row deviate from the RFC, used as the failure message.</param>
    [TestMethod]
    [DataRow("Tuesday, 10 Aug 2021 11:30:00 -0400", 2021, 8, 10, 15, 30, 0, "full day name")]
    [DataRow("Wednesday, 21 May 2025 14:00:00 -0400", 2025, 5, 21, 18, 0, 0, "full day name")]
    [DataRow("Tue, 11 December 2001 13:41 PST", 2001, 12, 11, 21, 41, 0, "full month name, no seconds, named zone")]
    public void ADateSpeltOutInFull_IsRead(string spelling, int year, int month, int day, int hour, int minute, int second, string shape)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(spelling, out DateTime parsed).ShouldBeTrue(shape);

        parsed.ShouldBe(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc), shape);
        parsed.Kind.ShouldBe(DateTimeKind.Utc, shape);
    }

    /// <summary>
    /// A two-digit year is read, and read into the right century.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Provenance: <c>rss09x/bbc-rss091-2004.xml</c>, the BBC's RSS 0.91 feed, whose
    ///     <c>lastBuildDate</c> reads <c>Sun, 01 Feb 04 20:50:19 GMT</c>. RFC 822 §5.1 actually
    ///     <em>specifies</em> two digits — <c>year = 2DIGIT</c> — so this is the conformant spelling and
    ///     the four-digit form everyone now uses is the RFC 1123 update.
    ///     </para>
    ///     <para>
    ///     The assertion on the century is the load-bearing half. <c>04</c> becoming 1904 would parse,
    ///     return <see langword="true"/>, and be wrong by a hundred years — which no "does it parse"
    ///     test would notice.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ATwoDigitYear_IsReadIntoTheCurrentCentury()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("Sun, 01 Feb 04 20:50:19 GMT", out DateTime parsed)
            .ShouldBeTrue();

        parsed.Year.ShouldBe(2004, "a two-digit year must not land in 1904");
        parsed.ShouldBe(new DateTime(2004, 2, 1, 20, 50, 19, DateTimeKind.Utc));
    }

    /// <summary>
    /// A single-digit day of month is read, with or without a leading zero.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     RFC 822 §5.1 gives <c>date = 1*2DIGIT month 2*4DIGIT</c> — one <em>or</em> two digits — so
    ///     both spellings are conformant and the unpadded one is common.
    ///     </para>
    ///     <para>
    ///     Provenance: <c>extensions/blogchannel/paradigmbi-blogengine.xml</c> and its BlogEngine.NET
    ///     sibling, and most strikingly <c>extensions/itunes/simplecast-thedaily.xml</c> — the New York
    ///     Times' <i>The Daily</i> — where <b>859 of 2,940 episodes</b> use it. Both rows carry the same
    ///     instant so the pair is a true A/B on the padding alone.
    ///     </para>
    /// </remarks>
    /// <param name="spelling">The date, padded or unpadded; both rows name the same instant.</param>
    /// <param name="shape">Which of the pair this row is, used as the failure message.</param>
    [TestMethod]
    [DataRow("Fri, 1 Apr 2022 09:50:00 +0000", "unpadded, as Simplecast and BlogEngine.NET emit it")]
    [DataRow("Fri, 01 Apr 2022 09:50:00 +0000", "zero-padded — the control")]
    public void ASingleDigitDayOfMonth_IsRead(string spelling, string shape)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(spelling, out DateTime parsed).ShouldBeTrue(shape);

        parsed.ShouldBe(new DateTime(2022, 4, 1, 9, 50, 0, DateTimeKind.Utc), shape);
    }

    /// <summary>
    /// The day-of-week name is verified against the date, and a date that disagrees with it is refused.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>Characterisation of a real trap, and the reason this file exists in the form it does.</b>
    ///     The name is redundant — the date already determines the weekday — so it is natural to assume
    ///     it is ignored. It is not: the format strings use <c>ddd</c>, and
    ///     <see cref="DateTime.TryParseExact(string, string[], IFormatProvider, System.Globalization.DateTimeStyles, out DateTime)"/>
    ///     validates it. A publisher whose feed says <c>Mon</c> for a Friday loses the date entirely and
    ///     silently.
    ///     </para>
    ///     <para>
    ///     Worth pinning because this assumption produced a false defect report while this file was being
    ///     written: a hand-written probe used <c>Thu, 5 Aug 2026</c>, which is a Wednesday, and the
    ///     resulting failure looked exactly like a parser that could not read an unpadded day. The
    ///     corpus value was fine; the invented one was not. Both rows below are here so that neither
    ///     mistake can be made again from this file.
    ///     </para>
    /// </remarks>
    /// <param name="spelling">The date, with a day name that either agrees with it or does not.</param>
    /// <param name="expected"><see langword="true"/> where the day name agrees with the date; otherwise, <see langword="false"/>.</param>
    /// <param name="why">Why the row expects that answer, used as the failure message.</param>
    [TestMethod]
    [DataRow("Wed, 5 Aug 2026 10:00:00 -0400", true, "5 August 2026 is a Wednesday")]
    [DataRow("Thu, 5 Aug 2026 10:00:00 -0400", false, "the same date called a Thursday")]
    [DataRow("Fri, 1 Apr 2022 09:50:00 +0000", true, "1 April 2022 is a Friday")]
    [DataRow("Mon, 1 Apr 2022 09:50:00 +0000", false, "the same date called a Monday")]
    public void ADayNameThatDisagreesWithTheDate_IsRefused(string spelling, bool expected, string why)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(spelling, out DateTime parsed).ShouldBe(expected, why);

        if (!expected)
        {
            parsed.ShouldBe(DateTime.MinValue, why);
        }
    }

    /// <summary>
    /// An RFC 3339 timestamp with no offset is read, and does not claim to be UTC.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     25 values in the corpus are spelt this way. A timestamp with no offset carries no zone, so
    ///     <see cref="DateTimeKind.Unspecified"/> is the only honest answer — calling it
    ///     <see cref="DateTimeKind.Utc"/> would invent an offset the wire never carried, and calling it
    ///     <see cref="DateTimeKind.Local"/> would rebase it onto the reader's machine clock, which is
    ///     the defect §2.37 records.
    ///     </para>
    ///     <para>
    ///     The <c>Kind</c> assertion is the whole test. This container runs UTC, so an instant-based
    ///     assertion passes whichever of the three the parser picks.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnRfc3339TimestampWithNoOffset_IsUnspecifiedRatherThanUtc()
    {
        SyndicationDateTimeUtility.TryParseRfc3339DateTime("2026-08-05T14:30:00", out DateTime parsed)
            .ShouldBeTrue();

        parsed.Kind.ShouldBe(DateTimeKind.Unspecified, "no offset was carried, so none may be claimed");
        parsed.Hour.ShouldBe(14);
    }

    /// <summary>
    /// An offset-bearing RFC 3339 timestamp is normalised to UTC.
    /// </summary>
    /// <remarks>
    ///     The control for the test above, and the overwhelmingly common spelling: 76,478 of the corpus's
    ///     timestamps carry a numeric offset and a further 3,121 end in <c>Z</c>.
    /// </remarks>
    /// <param name="spelling">The timestamp, carrying <c>Z</c>, a zero offset, a real offset or a fraction.</param>
    /// <param name="expectedUtcHour">The hour the instant falls on once converted to UTC.</param>
    [TestMethod]
    [DataRow("2026-08-05T14:30:00Z", 14)]
    [DataRow("2026-08-05T14:30:00+00:00", 14)]
    [DataRow("2026-08-05T09:30:00-05:00", 14)]
    [DataRow("2026-08-05T14:30:00.123+00:00", 14)]
    public void AnOffsetBearingRfc3339Timestamp_IsNormalisedToUtc(string spelling, int expectedUtcHour)
    {
        SyndicationDateTimeUtility.TryParseRfc3339DateTime(spelling, out DateTime parsed).ShouldBeTrue(spelling);

        parsed.Kind.ShouldBe(DateTimeKind.Utc, spelling);
        parsed.Hour.ShouldBe(expectedUtcHour, spelling);
    }

    /// <summary>
    /// A date with no time is refused by the RFC 3339 reader and accepted by the RFC 822 one.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>Characterisation of a split the corpus makes unavoidable.</b> 2,794 <c>lastmod</c> values in
    ///     <c>extensions/sitemap-hreflang/gitlab-pages.xml</c> — every single one GitLab publishes — are
    ///     bare dates. The sitemap protocol permits it: it cites the W3C Datetime profile, whose shortest
    ///     legal form is <c>YYYY-MM-DD</c>.
    ///     </para>
    ///     <para>
    ///     <see cref="SyndicationDateTimeUtility.TryParseRfc3339DateTime"/> refuses it, which is correct
    ///     — RFC 3339 requires a full timestamp. <c>SitemapUrl.Load</c> therefore does not rely on it
    ///     alone; it falls through to a <see cref="DateTime.TryParse(string, IFormatProvider, System.Globalization.DateTimeStyles, out DateTime)"/>
    ///     that accepts the bare date. <c>LoadDocumentsAsPublishersWriteThem</c> asserts the outcome
    ///     that arrangement produces, end to end.
    ///     </para>
    ///     <para>
    ///     Pinned as a pair because the two answers disagree and both are right. Anyone making the date
    ///     tables faster needs to know that the RFC 3339 reader's <see langword="false"/> here is
    ///     load-bearing rather than an oversight to be "fixed".
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ABareDate_IsRefusedByRfc3339AndAcceptedByRfc822()
    {
        SyndicationDateTimeUtility.TryParseRfc3339DateTime("2026-08-05", out DateTime rfc3339)
            .ShouldBeFalse("RFC 3339 requires a time of day");
        rfc3339.ShouldBe(DateTime.MinValue);

        SyndicationDateTimeUtility.TryParseRfc822DateTime("2026-08-05", out DateTime rfc822)
            .ShouldBeTrue("the RFC 822 reader ends in a permissive fallback");
        rfc822.Date.ShouldBe(new DateTime(2026, 8, 5, 0, 0, 0, rfc822.Kind));
    }
}