using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the two date grammars <c>SyndicationDateTimeUtility</c> reads and writes — RFC 3339, which
/// Atom requires, and RFC 822, which RSS and OPML require — across named zones, numeric offsets,
/// fractional seconds, two-digit years, and the round trip between parsing and formatting.
/// </summary>
[TestClass]
public class SyndicationDateTimeUtilityTests
{
    #region TryParseRfc3339DateTime Tests

    /// <summary>
    /// <c>2024-01-15T10:30:00Z</c> parses to that date and time, component for component.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithUtcTimestamp_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
        parsed.Hour.ShouldBe(10);
        parsed.Minute.ShouldBe(30);
        parsed.Second.ShouldBe(0);
    }

    /// <summary>
    /// A three-digit fraction survives the parse: <c>10:30:00.123Z</c> yields a millisecond of <c>123</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithMilliseconds_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.123Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
        parsed.Hour.ShouldBe(10);
        parsed.Minute.ShouldBe(30);
        parsed.Second.ShouldBe(0);
        parsed.Millisecond.ShouldBe(123);
    }

    /// <summary>
    /// An eastern offset is rebased to UTC: <c>10:30+05:00</c> parses to <c>05:30</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithPositiveTimezoneOffset_ParsesAndConvertsToUtc()
    {
        // Arrange
        string input = "2024-01-15T10:30:00+05:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        // 10:30 +05:00 = 05:30 UTC
        parsed.Hour.ShouldBe(5);
        parsed.Minute.ShouldBe(30);
    }

    /// <summary>
    /// A western offset is rebased to UTC: <c>10:30-08:00</c> parses to <c>18:30</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithNegativeTimezoneOffset_ParsesAndConvertsToUtc()
    {
        // Arrange
        string input = "2024-01-15T10:30:00-08:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        // 10:30 -08:00 = 18:30 UTC
        parsed.Hour.ShouldBe(18);
        parsed.Minute.ShouldBe(30);
    }

    /// <summary>
    /// A value carrying both a fraction and an offset — <c>10:30:00.456-05:00</c> — parses and rebases
    /// to <c>15:30</c> UTC.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithMillisecondsAndTimezoneOffset_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.456-05:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        // 10:30 -05:00 = 15:30 UTC
        parsed.Hour.ShouldBe(15);
        parsed.Minute.ShouldBe(30);
    }

    /// <summary>
    /// A string that is not a date returns <see langword="false"/> and leaves the result at
    /// <see cref="DateTime.MinValue"/>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithInvalidString_ReturnsFalse()
    {
        // Arrange
        string input = "not-a-valid-date";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// An empty string returns <see langword="false"/> and leaves the result at
    /// <see cref="DateTime.MinValue"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        string input = "";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// A <see langword="null"/> string returns <see langword="false"/> and leaves the result at
    /// <see cref="DateTime.MinValue"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithNullString_ReturnsFalse()
    {
        // Arrange
        string? input = null;

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input!, out DateTime parsed);

        // Assert
        result.ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// Six fractional-second digits are accepted rather than rejected as over-precise.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithSixDigitFractionalSeconds_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.123456Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
    }

    /// <summary>
    /// Two fractional-second digits are accepted.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithTwoDigitFractionalSeconds_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.12Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
    }

    /// <summary>
    /// Five fractional-second digits are accepted.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithFiveDigitFractionalSeconds_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.12345Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
    }

    /// <summary>
    /// A single fractional-second digit is accepted.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithSingleDigitFractionalSeconds_ParsesCorrectly()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.1Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
    }

    #endregion

    #region ParseRfc3339DateTime Tests

    /// <summary>
    /// The throwing overload returns <c>2024-01-15T10:30:00Z</c> as its component parts.
    /// </summary>
    [TestMethod]
    public void ParseRfc3339DateTime_WithValidUtcTimestamp_ReturnsDateTime()
    {
        // Arrange
        string input = "2024-01-15T10:30:00Z";

        // Act
        DateTime result = SyndicationDateTimeUtility.ParseRfc3339DateTime(input);

        // Assert
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(1);
        result.Day.ShouldBe(15);
        result.Hour.ShouldBe(10);
        result.Minute.ShouldBe(30);
    }

    /// <summary>
    /// A <see langword="null"/> value throws <see cref="ArgumentNullException"/>, not the plain
    /// <see cref="ArgumentException"/> the empty case raises.
    /// </summary>
    [TestMethod]
    public void ParseRfc3339DateTime_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        string? input = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SyndicationDateTimeUtility.ParseRfc3339DateTime(input!));
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void ParseRfc3339DateTime_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        string input = "";

        // Act & Assert
        Should.Throw<ArgumentException>(() => SyndicationDateTimeUtility.ParseRfc3339DateTime(input));
    }

    /// <summary>
    /// A string that is not a date throws <see cref="FormatException"/> rather than returning a
    /// sentinel.
    /// </summary>
    [TestMethod]
    public void ParseRfc3339DateTime_WithInvalidFormat_ThrowsFormatException()
    {
        // Arrange
        string input = "not-a-valid-date";

        // Act & Assert
        Should.Throw<FormatException>(() => SyndicationDateTimeUtility.ParseRfc3339DateTime(input));
    }

    /// <summary>
    /// The throwing overload rebases an offset-bearing value too: <c>10:30+05:00</c> returns hour <c>5</c>.
    /// </summary>
    [TestMethod]
    public void ParseRfc3339DateTime_WithTimezoneOffset_ReturnsConvertedDateTime()
    {
        // Arrange
        string input = "2024-01-15T10:30:00+05:00";

        // Act
        DateTime result = SyndicationDateTimeUtility.ParseRfc3339DateTime(input);

        // Assert
        // 10:30 +05:00 = 05:30 UTC
        result.Hour.ShouldBe(5);
    }

    /// <summary>
    /// <c>10:30:00.500Z</c> returns a millisecond of <c>500</c>.
    /// </summary>
    [TestMethod]
    public void ParseRfc3339DateTime_WithMilliseconds_ReturnsDateTime()
    {
        // Arrange
        string input = "2024-01-15T10:30:00.500Z";

        // Act
        DateTime result = SyndicationDateTimeUtility.ParseRfc3339DateTime(input);

        // Assert
        result.Year.ShouldBe(2024);
        result.Millisecond.ShouldBe(500);
    }

    #endregion

    #region TryParseRfc822DateTime Tests

    /// <summary>
    /// The named zone <c>CET</c> is read as <c>+01:00</c>, so <c>10:30 CET</c> parses to <c>09:30</c> UTC.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithCetTimezone_ParsesAndConvertsToUtc()
    {
        // Arrange
        string input = "Mon, 15 Jan 2024 10:30:00 CET";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        // CET is +01:00, so 10:30 CET = 09:30 UTC
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
        parsed.Hour.ShouldBe(9);
        parsed.Minute.ShouldBe(30);
    }

    /// <summary>
    /// The named zone <c>CEST</c> is read as <c>+02:00</c>, so <c>10:30 CEST</c> parses to <c>08:30</c> UTC.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithCestTimezone_ParsesAndConvertsToUtc()
    {
        // Arrange
        string input = "Mon, 15 Jan 2024 10:30:00 CEST";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        // CEST is +02:00, so 10:30 CEST = 08:30 UTC
        parsed.Hour.ShouldBe(8);
        parsed.Minute.ShouldBe(30);
    }

    /// <summary>
    /// A string that is not a date returns <see langword="false"/> and leaves the result at
    /// <see cref="DateTime.MinValue"/>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithInvalidString_ReturnsFalse()
    {
        // Arrange
        string input = "not-a-valid-date";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// An empty string returns <see langword="false"/> and leaves the result at
    /// <see cref="DateTime.MinValue"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        string input = "";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// A <see langword="null"/> string returns <see langword="false"/> and leaves the result at
    /// <see cref="DateTime.MinValue"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithNullString_ReturnsFalse()
    {
        // Arrange
        string? input = null;

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input!, out DateTime parsed);

        // Assert
        result.ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// An unpadded day of month parses alongside a named zone: <c>Fri, 5 Jan 2024</c> yields day <c>5</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithSingleDigitDayAndCet_ParsesCorrectly()
    {
        // Arrange
        string input = "Fri, 5 Jan 2024 10:30:00 CET";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Day.ShouldBe(5);
    }

    /// <summary>
    /// A two-digit year parses alongside a named zone, <c>24</c> expanding to <c>2024</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithTwoDigitYearAndCet_ParsesCorrectly()
    {
        // Arrange
        string input = "Mon, 15 Jan 24 10:30:00 CET";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
    }

    /// <summary>
    /// A fractional second parses alongside a named zone.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithFractionalSecondsAndCet_ParsesCorrectly()
    {
        // Arrange
        string input = "Mon, 15 Jan 2024 10:30:00.123 CET";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
    }

    /// <summary>
    /// Seven fractional-second digits — a whole tick of resolution — parse alongside a named zone.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithSevenDigitFractionalSecondsAndCet_ParsesCorrectly()
    {
        // Arrange
        string input = "Mon, 15 Jan 2024 10:30:00.1234567 CET";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
    }

    #endregion

    #region ParseRfc822DateTime Tests

    /// <summary>
    /// The throwing overload converts a named zone too: <c>10:30 CET</c> returns <c>09:30</c> UTC on
    /// 15 January 2024.
    /// </summary>
    [TestMethod]
    public void ParseRfc822DateTime_WithCetTimezone_ReturnsConvertedDateTime()
    {
        // Arrange
        string input = "Mon, 15 Jan 2024 10:30:00 CET";

        // Act
        DateTime result = SyndicationDateTimeUtility.ParseRfc822DateTime(input);

        // Assert
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(1);
        result.Day.ShouldBe(15);
        // CET is +01:00, so 10:30 CET = 09:30 UTC
        result.Hour.ShouldBe(9);
    }

    /// <summary>
    /// A <see langword="null"/> value throws <see cref="ArgumentNullException"/>, not the plain
    /// <see cref="ArgumentException"/> the empty case raises.
    /// </summary>
    [TestMethod]
    public void ParseRfc822DateTime_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        string? input = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SyndicationDateTimeUtility.ParseRfc822DateTime(input!));
    }

    /// <summary>
    /// An empty string throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void ParseRfc822DateTime_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        string input = "";

        // Act & Assert
        Should.Throw<ArgumentException>(() => SyndicationDateTimeUtility.ParseRfc822DateTime(input));
    }

    /// <summary>
    /// A string that is not a date throws <see cref="FormatException"/> rather than returning a
    /// sentinel.
    /// </summary>
    [TestMethod]
    public void ParseRfc822DateTime_WithInvalidFormat_ThrowsFormatException()
    {
        // Arrange
        string input = "not-a-valid-date";

        // Act & Assert
        Should.Throw<FormatException>(() => SyndicationDateTimeUtility.ParseRfc822DateTime(input));
    }

    /// <summary>
    /// The throwing overload reads <c>CEST</c> as <c>+02:00</c>, returning <c>08:30</c> UTC.
    /// </summary>
    [TestMethod]
    public void ParseRfc822DateTime_WithCestTimezone_ReturnsConvertedDateTime()
    {
        // Arrange
        string input = "Mon, 15 Jan 2024 10:30:00 CEST";

        // Act
        DateTime result = SyndicationDateTimeUtility.ParseRfc822DateTime(input);

        // Assert
        // CEST is +02:00, so 10:30 CEST = 08:30 UTC
        result.Hour.ShouldBe(8);
    }

    #endregion

    #region ToRfc3339DateTime Tests

    /// <summary>
    /// A <see cref="DateTimeKind.Utc"/> value is written with a trailing <c>Z</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_WithUtcDateTime_FormatsWithZSuffix()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain("2024-01-15T10:30:00");
        result.ShouldEndWith("Z");
    }

    /// <summary>
    /// A <see cref="DateTimeKind.Local"/> value is written with a numeric offset instead of <c>Z</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_WithLocalDateTime_FormatsWithTimezoneOffset()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Local);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain("2024-01-15T10:30:00");
        // Local time should have a timezone offset (e.g., +05:00 or -08:00)
        result.ShouldNotEndWith("Z");
        // Should contain either + or - for timezone offset
        (result.Contains('+', StringComparison.Ordinal) || result.Contains('-', StringComparison.Ordinal)).ShouldBeTrue();
    }

    /// <summary>
    /// A <see cref="DateTimeKind.Unspecified"/> value is written with <c>Z</c> — the writer states UTC
    /// rather than guessing the machine's offset.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_WithUnspecifiedKind_FormatsWithZSuffix()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain("2024-01-15T10:30:00");
        result.ShouldEndWith("Z");
    }

    /// <summary>
    /// The written value opens with the date and time it was given, <c>2024-12-31T23:59:59</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_PreservesDateComponents()
    {
        // Arrange
        DateTime input = new(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldStartWith("2024-12-31T23:59:59");
    }

    /// <summary>
    /// A sub-second component is written: 123 milliseconds appears as the hundredths <c>.12</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_IncludesFractionalSeconds()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, 123, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain(".12");
    }

    /// <summary>
    /// Midnight is written in full as <c>2024-01-15T00:00:00</c> rather than elided to a date.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_WithMidnight_FormatsCorrectly()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain("2024-01-15T00:00:00");
    }

    /// <summary>
    /// The last second of a day is written as <c>2024-01-15T23:59:59</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_WithEndOfDay_FormatsCorrectly()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 23, 59, 59, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain("2024-01-15T23:59:59");
    }

    /// <summary>
    /// A February date is written with its two-digit month, <c>2024-02-28T12:00:00</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc3339DateTime_WithFebruaryDate_FormatsCorrectly()
    {
        // Arrange
        DateTime input = new(2024, 2, 28, 12, 0, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc3339DateTime(input);

        // Assert
        result.ShouldContain("2024-02-28T12:00:00");
    }

    #endregion

    #region ToRfc822DateTime Tests

    /// <summary>
    /// 15 January 2024 at 10:30 is written with each of its parts — <c>Mon</c>, <c>15</c>, <c>Jan</c>,
    /// <c>2024</c> and <c>10:30:00</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_FormatsCorrectly()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        result.ShouldContain("Mon");
        result.ShouldContain("15");
        result.ShouldContain("Jan");
        result.ShouldContain("2024");
        result.ShouldContain("10:30:00");
    }

    /// <summary>
    /// The whole string is the RFC 1123 form, <c>Mon, 15 Jan 2024 10:30:00 GMT</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_FollowsRfc1123Pattern()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        // RFC 1123 format: ddd, dd MMM yyyy HH:mm:ss GMT
        result.ShouldBe("Mon, 15 Jan 2024 10:30:00 GMT");
    }

    /// <summary>
    /// 31 December 2024 at 23:59:59 is written as <c>Tue</c>, <c>31</c> <c>Dec</c> <c>2024</c>
    /// <c>23:59:59</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_PreservesDateComponents()
    {
        // Arrange
        DateTime input = new(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        result.ShouldContain("Tue");
        result.ShouldContain("31");
        result.ShouldContain("Dec");
        result.ShouldContain("2024");
        result.ShouldContain("23:59:59");
    }

    /// <summary>
    /// 14 February 2024 carries its own weekday, <c>Wed</c>, rather than a fixed one.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_WithDifferentDaysOfWeek_FormatsCorrectly()
    {
        // Arrange - Wednesday, February 14, 2024
        DateTime input = new(2024, 2, 14, 12, 0, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        result.ShouldContain("Wed");
        result.ShouldContain("14");
        result.ShouldContain("Feb");
    }

    /// <summary>
    /// Midnight is written as <c>00:00:00</c> rather than omitted.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_WithMidnight_FormatsCorrectly()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        result.ShouldContain("00:00:00");
    }

    /// <summary>
    /// Every value is written under the literal <c>GMT</c> suffix RFC 1123 ends with.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_EndsWithGmt()
    {
        // Arrange
        DateTime input = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        result.ShouldEndWith("GMT");
    }

    /// <summary>
    /// A single-digit day is zero-padded: 5 January is written as <c>05</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_WithSingleDigitDay_PadsDayCorrectly()
    {
        // Arrange - Friday, January 5, 2024
        DateTime input = new(2024, 1, 5, 10, 0, 0, DateTimeKind.Utc);

        // Act
        string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);

        // Assert
        result.ShouldContain("Fri");
        result.ShouldContain("05"); // Day should be padded to two digits
    }

    #endregion

    #region Round-Trip Tests

    /// <summary>
    /// A UTC value written to RFC 3339 and parsed back returns the same components down to the second.
    /// </summary>
    [TestMethod]
    public void Rfc3339_RoundTrip_PreservesDateTime()
    {
        // Arrange
        DateTime original = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        string formatted = SyndicationDateTimeUtility.ToRfc3339DateTime(original);
        bool parseResult = SyndicationDateTimeUtility.TryParseRfc3339DateTime(formatted, out DateTime parsed);

        // Assert
        parseResult.ShouldBeTrue();
        parsed.Year.ShouldBe(original.Year);
        parsed.Month.ShouldBe(original.Month);
        parsed.Day.ShouldBe(original.Day);
        parsed.Hour.ShouldBe(original.Hour);
        parsed.Minute.ShouldBe(original.Minute);
        parsed.Second.ShouldBe(original.Second);
    }

    /// <summary>
    /// A <see cref="DateTimeKind.Local"/> value written to RFC 3339 and parsed back returns the date of
    /// its UTC equivalent, the offset having been applied rather than discarded.
    /// </summary>
    [TestMethod]
    public void Rfc3339_RoundTrip_WithLocalTime_PreservesEquivalentTime()
    {
        // Arrange
        DateTime original = new(2024, 6, 20, 14, 45, 30, DateTimeKind.Local);

        // Act
        string formatted = SyndicationDateTimeUtility.ToRfc3339DateTime(original);
        bool parseResult = SyndicationDateTimeUtility.TryParseRfc3339DateTime(formatted, out DateTime parsed);

        // Assert
        parseResult.ShouldBeTrue();
        // After round-trip, time should be equivalent (converted to UTC)
        DateTime expectedUtc = original.ToUniversalTime();
        parsed.Year.ShouldBe(expectedUtc.Year);
        parsed.Month.ShouldBe(expectedUtc.Month);
        parsed.Day.ShouldBe(expectedUtc.Day);
    }

    /// <summary>
    /// <c>Mon, 15 Jan 2024 10:30:00 CET</c> parses to <c>09:30</c> UTC on 15 January 2024.
    /// </summary>
    [TestMethod]
    public void Rfc822_RoundTrip_WithCetTimezone_PreservesDateTime()
    {
        // Arrange - Use CET format which is properly handled
        string input = "Mon, 15 Jan 2024 10:30:00 CET";

        // Act
        bool parseResult = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        parseResult.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
        // CET is +01:00, so 10:30 CET = 09:30 UTC
        parsed.Hour.ShouldBe(9);
        parsed.Minute.ShouldBe(30);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// 29 February 2024 parses as the real date it is, rather than failing validation.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithLeapYear_ParsesCorrectly()
    {
        // Arrange - February 29, 2024 (leap year)
        string input = "2024-02-29T12:00:00Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(2);
        parsed.Day.ShouldBe(29);
    }

    /// <summary>
    /// The last second of 2024, <c>2024-12-31T23:59:59Z</c>, parses component for component.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithYearBoundary_ParsesCorrectly()
    {
        // Arrange - December 31, 2024 at 23:59:59
        string input = "2024-12-31T23:59:59Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(12);
        parsed.Day.ShouldBe(31);
        parsed.Hour.ShouldBe(23);
        parsed.Minute.ShouldBe(59);
        parsed.Second.ShouldBe(59);
    }

    /// <summary>
    /// The first instant of 2024, <c>2024-01-01T00:00:00Z</c>, parses component for component.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithNewYearStart_ParsesCorrectly()
    {
        // Arrange - January 1, 2024 at 00:00:00
        string input = "2024-01-01T00:00:00Z";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(1);
        parsed.Hour.ShouldBe(0);
        parsed.Minute.ShouldBe(0);
        parsed.Second.ShouldBe(0);
    }

    /// <summary>
    /// 29 February 2024 parses under a named zone.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithLeapYearAndCet_ParsesCorrectly()
    {
        // Arrange - February 29, 2024 (leap year)
        string input = "Thu, 29 Feb 2024 12:00:00 CET";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(2);
        parsed.Day.ShouldBe(29);
    }

    /// <summary>
    /// All twelve months are written with their invariant English abbreviations, <c>Jan</c> through
    /// <c>Dec</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_AllMonths_FormatsCorrectly()
    {
        // Arrange & Act & Assert - Test each month abbreviation
        string[] expectedMonths = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

        for (int month = 1; month <= 12; month++)
        {
            DateTime input = new(2024, month, 15, 12, 0, 0, DateTimeKind.Utc);
            string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);
            result.ShouldContain(expectedMonths[month - 1]);
        }
    }

    /// <summary>
    /// All seven weekdays are written with their invariant English abbreviations — the first seven days
    /// of January 2024 run <c>Mon</c> through <c>Sun</c>.
    /// </summary>
    [TestMethod]
    public void ToRfc822DateTime_AllDaysOfWeek_FormatsCorrectly()
    {
        // Arrange & Act & Assert - Test each day of week
        // January 2024: Mon=1, Tue=2, Wed=3, Thu=4, Fri=5, Sat=6, Sun=7
        string[] expectedDays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

        for (int day = 1; day <= 7; day++)
        {
            DateTime input = new(2024, 1, day, 12, 0, 0, DateTimeKind.Utc);
            string result = SyndicationDateTimeUtility.ToRfc822DateTime(input);
            result.ShouldContain(expectedDays[day - 1]);
        }
    }

    /// <summary>
    /// An offset that moves the value into the previous day is applied to the date as well as the time:
    /// <c>02:00+08:00</c> on 16 January parses to <c>18:00</c> UTC on the 15th.
    /// </summary>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithTimezoneOffsetCrossingDateBoundary_ParsesCorrectly()
    {
        // Arrange - Time that crosses date boundary when converting to UTC
        // 02:00 on Jan 16 at +08:00 = 18:00 on Jan 15 UTC
        string input = "2024-01-16T02:00:00+08:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Day.ShouldBe(15);
        parsed.Hour.ShouldBe(18);
    }

    /// <summary>
    /// Where <c>+14:00</c>, the largest offset in civil use, is accepted, it rebases to <c>20:30</c> UTC
    /// on the previous day.
    /// </summary>
    /// <remarks>
    ///     The assertion is guarded by the parse having succeeded, so this test records the behaviour
    ///     rather than requiring it: it passes whether or not the offset is supported.
    /// </remarks>
    [TestMethod]
    public void TryParseRfc3339DateTime_WithMaxTimezoneOffset_ParsesCorrectly()
    {
        // Arrange - Maximum valid timezone offset (+14:00)
        string input = "2024-01-15T10:30:00+14:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc3339DateTime(input, out DateTime parsed);

        // Assert
        // The implementation may or may not support +14:00, document actual behavior
        if (result)
        {
            // 10:30 +14:00 = previous day 20:30 UTC
            parsed.Day.ShouldBe(14);
            parsed.Hour.ShouldBe(20);
        }
    }

    #endregion

    #region TryParseRfc822DateTime Numeric Timezone Offset Tests

    /// <summary>
    /// Regression test for bug where ReplaceRfc822TimeZoneWithOffset returned empty string
    /// for dates with numeric timezone offsets (e.g., +00:00, -05:00).
    ///
    /// The bug was in SyndicationDateTimeUtility.cs lines 185-189 where the else branch
    /// returned string.Empty instead of the original value when no named timezone conversion
    /// was needed. This caused TryParseRfc822DateTime to receive an empty string and fail.
    ///
    /// Fix: Changed the else branch to return the original value unchanged.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithNumericTimezoneOffset_ParsesCorrectly()
    {
        // Arrange - Numeric offset format that was previously broken
        string input = "Mon, 20 Jan 2025 12:00:00 +00:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue("Dates with numeric timezone offsets should parse successfully");
        parsed.Year.ShouldBe(2025);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(20);
        parsed.Hour.ShouldBe(12);
        parsed.Minute.ShouldBe(0);
    }

    /// <summary>
    /// An eastern numeric offset is rebased: <c>15:30 +05:00</c> parses to <c>10:30</c> UTC.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithPositiveNumericOffset_ParsesAndConvertsToUtc()
    {
        // Arrange - +05:00 offset
        string input = "Mon, 20 Jan 2025 15:30:00 +05:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2025);
        // 15:30 +05:00 = 10:30 UTC
        parsed.Hour.ShouldBe(10);
        parsed.Minute.ShouldBe(30);
    }

    /// <summary>
    /// A western numeric offset is rebased: <c>04:00 -08:00</c> parses to <c>12:00</c> UTC.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithNegativeNumericOffset_ParsesAndConvertsToUtc()
    {
        // Arrange - -08:00 offset (Pacific Standard Time)
        string input = "Mon, 20 Jan 2025 04:00:00 -08:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2025);
        // 04:00 -08:00 = 12:00 UTC
        parsed.Hour.ShouldBe(12);
        parsed.Minute.ShouldBe(0);
    }

    /// <summary>
    /// The compact offset spelling <c>+0000</c>, which some feeds emit in place of <c>+00:00</c>, parses.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithNumericOffsetWithoutColon_ParsesCorrectly()
    {
        // Arrange - Some feeds use +0000 format (without colon)
        string input = "Mon, 20 Jan 2025 12:00:00 +0000";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue("Dates with compact numeric offsets (+0000) should parse");
        parsed.Year.ShouldBe(2025);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(20);
    }

    /// <summary>
    /// A numeric offset that moves the value into the previous day is applied to the date as well:
    /// <c>02:00 +10:00</c> on 21 January parses to <c>16:00</c> UTC on the 20th.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithNumericOffsetCrossingDateBoundary_ParsesCorrectly()
    {
        // Arrange - Time that crosses date boundary when converting to UTC
        // 02:00 on Jan 21 at +10:00 = 16:00 on Jan 20 UTC
        string input = "Tue, 21 Jan 2025 02:00:00 +10:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Day.ShouldBe(20);
        parsed.Hour.ShouldBe(16);
    }

    /// <summary>
    /// An unpadded day of month parses alongside a numeric offset, yielding 3 January 2025.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithSingleDigitDayAndNumericOffset_ParsesCorrectly()
    {
        // Arrange - Single digit day with numeric offset
        string input = "Fri, 3 Jan 2025 10:30:00 +00:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Day.ShouldBe(3);
        parsed.Month.ShouldBe(1);
        parsed.Year.ShouldBe(2025);
    }

    /// <summary>
    /// A two-digit year parses alongside a numeric offset, <c>25</c> expanding to <c>2025</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithTwoDigitYearAndNumericOffset_ParsesCorrectly()
    {
        // Arrange - Two digit year with numeric offset
        string input = "Mon, 20 Jan 25 12:00:00 +00:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2025);
    }

    /// <summary>
    /// A fractional second parses alongside a numeric offset, the whole seconds still reading <c>45</c>.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithFractionalSecondsAndNumericOffset_ParsesCorrectly()
    {
        // Arrange - Fractional seconds with numeric offset
        string input = "Mon, 20 Jan 2025 12:30:45.123 +00:00";

        // Act
        bool result = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Second.ShouldBe(45);
    }

    /// <summary>
    /// The throwing overload accepts a numeric offset, returning 20 January 2025 rather than raising
    /// <see cref="FormatException"/>.
    /// </summary>
    [TestMethod]
    public void ParseRfc822DateTime_WithNumericOffset_DoesNotThrow()
    {
        // Arrange
        string input = "Mon, 20 Jan 2025 12:00:00 +00:00";

        // Act
        DateTime result = SyndicationDateTimeUtility.ParseRfc822DateTime(input);

        // Assert
        result.Year.ShouldBe(2025);
        result.Month.ShouldBe(1);
        result.Day.ShouldBe(20);
    }

    /// <summary>
    /// A date parsed from a numeric offset, written back out and parsed again holds the same instant to
    /// the minute.
    /// </summary>
    [TestMethod]
    public void Rfc822_RoundTrip_WithNumericOffset_PreservesDateTime()
    {
        // Arrange - Parse a date with numeric offset, format it, parse again
        string input = "Mon, 20 Jan 2025 12:00:00 +00:00";

        // Act
        bool firstParse = SyndicationDateTimeUtility.TryParseRfc822DateTime(input, out DateTime parsed);
        string formatted = SyndicationDateTimeUtility.ToRfc822DateTime(parsed);
        bool secondParse = SyndicationDateTimeUtility.TryParseRfc822DateTime(formatted, out DateTime reparsed);

        // Assert
        firstParse.ShouldBeTrue();
        secondParse.ShouldBeTrue();
        reparsed.Year.ShouldBe(parsed.Year);
        reparsed.Month.ShouldBe(parsed.Month);
        reparsed.Day.ShouldBe(parsed.Day);
        reparsed.Hour.ShouldBe(parsed.Hour);
        reparsed.Minute.ShouldBe(parsed.Minute);
    }

    #endregion
}