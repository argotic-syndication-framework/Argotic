using Argotic.Common;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the RFC 822 dates that no pattern in the format table matches, and that therefore reach
/// <c>DateTime.TryParse</c>.
/// </summary>
/// <remarks>
///     <para>
///     <b>Why these assert the <see cref="DateTimeKind"/> and not the instant.</b> The fallback applies
///     no <see cref="System.Globalization.DateTimeStyles"/>, so an offset-bearing value is converted to
///     the <em>machine's</em> local time and returned as <see cref="DateTimeKind.Local"/>. Every
///     existing assertion in this suite compares instants — and
///     <see cref="DateTime.Equals(DateTime)"/> compares ticks alone, ignoring Kind — so on a machine
///     running UTC, where Local and Utc coincide, nothing can tell the two apart. This container runs
///     UTC. So does most CI.
///     </para>
///     <para>
///     Measured under <c>TZ=America/New_York</c>, <c>01 Jan 2024 10:00:00 GMT</c> parses to
///     <c>05:00</c> and is then written back by <c>ToRfc822DateTime</c> as
///     <c>Mon, 01 Jan 2024 05:00:00 GMT</c> — the RFC 1123 pattern appends the literal <c>GMT</c>
///     without converting, so a Local value is relabelled rather than converted. Five hours of silent
///     loss, on every machine that is not UTC.
///     </para>
///     <para>
///     A parsed absolute instant has no business being <see cref="DateTimeKind.Local"/>, whatever the
///     machine. That invariant is what these assert, because it is false on every machine when the
///     defect is present and true on every machine when it is not — unlike the instant, which is only
///     wrong somewhere else.
///     </para>
///     <para>
///     Both shapes below are <b>legal RFC 822</b>: §5.1 makes the day-of-week optional and the seconds
///     optional. These are not malformed inputs being tolerated; they are conformant inputs being
///     mishandled.
///     </para>
/// </remarks>
[TestClass]
public sealed class Rfc822FallbackKindTests
{
    /// <summary>
    /// A conformant date that omits the optional day-of-week does not come back machine-local.
    /// </summary>
    [TestMethod]
    public void ADateWithoutADayOfWeek_DoesNotParseAsMachineLocal()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("01 Jan 2024 10:00:00 GMT", out DateTime result)
            .ShouldBeTrue("RFC 822 §5.1 makes the day-of-week optional");

        result.Kind.ShouldNotBe(
            DateTimeKind.Local,
            "an instant carrying an explicit zone must not be rebased onto the reader's machine");
    }

    /// <summary>
    /// A conformant date that omits the optional seconds does not come back machine-local.
    /// </summary>
    [TestMethod]
    public void ADateWithoutSeconds_DoesNotParseAsMachineLocal()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("Mon, 01 Jan 2024 10:00 GMT", out DateTime result)
            .ShouldBeTrue("RFC 822 §5.1 makes the seconds optional");

        result.Kind.ShouldNotBe(
            DateTimeKind.Local,
            "an instant carrying an explicit zone must not be rebased onto the reader's machine");
    }

    /// <summary>
    /// A W3C date-time written into a <c>pubDate</c> does not come back machine-local either.
    /// </summary>
    /// <remarks>
    ///     Not legal RSS, but common: generators that emit Atom and RSS from one code path write the
    ///     Atom spelling into both. It parses, so the tolerance is deliberate; it should parse to the
    ///     instant it names on every machine.
    /// </remarks>
    [TestMethod]
    public void AW3cDateTimeInAPubDate_DoesNotParseAsMachineLocal()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("2024-01-01T10:00:00Z", out DateTime result)
            .ShouldBeTrue();

        result.Kind.ShouldNotBe(
            DateTimeKind.Local,
            "an instant carrying an explicit zone must not be rebased onto the reader's machine");
    }

    /// <summary>
    /// The fallback shapes name the same instants as their table-matched equivalents.
    /// </summary>
    /// <remarks>
    ///     The control that stops the Kind assertions above being satisfied by breaking the value.
    ///     Comparing a fallback shape against a table-matched spelling of the same instant pins the
    ///     conversion, not merely the annotation — and it holds on a UTC machine and a non-UTC one
    ///     alike, because both sides move together if the offset handling is wrong in the same way.
    /// </remarks>
    [TestMethod]
    [DataRow("01 Jan 2024 10:00:00 GMT", "Mon, 01 Jan 2024 10:00:00 GMT")]
    [DataRow("Mon, 01 Jan 2024 10:00 GMT", "Mon, 01 Jan 2024 10:00:00 GMT")]
    [DataRow("2024-01-01T10:00:00Z", "Mon, 01 Jan 2024 10:00:00 GMT")]
    public void AFallbackShape_NamesTheSameInstantAsItsTableMatchedEquivalent(string fallbackShape, string tableShape)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(fallbackShape, out DateTime viaFallback).ShouldBeTrue();
        SyndicationDateTimeUtility.TryParseRfc822DateTime(tableShape, out DateTime viaTable).ShouldBeTrue();

        viaFallback.ToUniversalTime().ShouldBe(viaTable.ToUniversalTime());
    }

    /// <summary>
    /// A date the table does match is still returned as UTC.
    /// </summary>
    /// <remarks>
    ///     The second control. Without it, "return Unspecified for everything" would satisfy every
    ///     assertion above while discarding the zone information the document supplied.
    /// </remarks>
    [TestMethod]
    [DataRow("Mon, 01 Jan 2024 10:00:00 GMT")]
    [DataRow("Mon, 01 Jan 2024 10:00:00 -0500")]
    [DataRow("Mon, 01 Jan 2024 10:00:00 EST")]
    public void ATableMatchedDate_IsStillReturnedAsUtc(string value)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime result).ShouldBeTrue();

        result.Kind.ShouldBe(DateTimeKind.Utc);
    }

    /// <summary>
    /// Writing a parsed date back reproduces the instant the document stated.
    /// </summary>
    /// <remarks>
    ///     This is the consequence, stated as a round trip. <c>ToRfc822DateTime</c> formats with the
    ///     RFC 1123 pattern, which ends in a literal <c>GMT</c> and performs no conversion, so
    ///     whatever wall-clock reading it is handed is published as though it were UTC.
    /// </remarks>
    [TestMethod]
    [DataRow("01 Jan 2024 10:00:00 GMT")]
    [DataRow("Mon, 01 Jan 2024 10:00 GMT")]
    [DataRow("2024-01-01T10:00:00Z")]
    [DataRow("Mon, 01 Jan 2024 10:00:00 GMT")]
    public void AParsedDate_RoundTripsToTheInstantTheDocumentStated(string value)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime result).ShouldBeTrue();

        SyndicationDateTimeUtility.ToRfc822DateTime(result)
            .ShouldBe("Mon, 01 Jan 2024 10:00:00 GMT");
    }
}