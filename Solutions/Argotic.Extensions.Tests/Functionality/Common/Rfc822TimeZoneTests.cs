namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the RFC 822 named time zones that <see cref="SyndicationDateTimeUtility"/> translates.
/// </summary>
/// <remarks>
///     <para>
///     RFC 822 section 5.1 permits a date to end in a named zone rather than a numeric offset, and
///     <c>ReplaceRfc822TimeZoneWithOffset</c> implements a seventeen-way table for exactly that. Every
///     branch but <c>GMT</c> was cold: every date in every fixture in this repository - all forty-eight
///     inline literals and all fifteen sample documents - ends in <c>GMT</c>.
///     </para>
///     <para>
///     Real feeds use these. Sixty lines were written to handle them and nothing had ever run them.
///     </para>
/// </remarks>
[TestClass]
public class Rfc822TimeZoneTests
{
    /// <summary>
    /// A date in a named zone parses to the corresponding instant in UTC.
    /// </summary>
    /// <param name="zone">The RFC 822 zone name, as it appears at the end of the date.</param>
    /// <param name="expectedUtcDay">The day of August 2010 the instant falls on in UTC.</param>
    /// <param name="expectedUtcHour">The UTC hour the instant falls on.</param>
    [TestMethod]
    [DataRow("UT", 1, 12)]
    [DataRow("GMT", 1, 12)]
    [DataRow("Z", 1, 12)]
    [DataRow("EST", 1, 17)]
    [DataRow("EDT", 1, 16)]
    [DataRow("CST", 1, 18)]
    [DataRow("CDT", 1, 17)]
    [DataRow("MST", 1, 19)]
    [DataRow("MDT", 1, 18)]
    [DataRow("PST", 1, 20)]
    [DataRow("PDT", 1, 19)]
    [DataRow("A", 1, 13)]
    [DataRow("N", 1, 11)]
    [DataRow("Y", 1, 0)]
    [DataRow("M", 2, 0)]
    public void ADateInANamedZone_ParsesToTheCorrectInstant(string zone, int expectedUtcDay, int expectedUtcHour)
    {
        string value = $"Sun, 01 Aug 2010 12:00:00 {zone}";

        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime parsed)
            .ShouldBeTrue($"'{value}' was not recognised as an RFC 822 date");

        DateTime utc = parsed.ToUniversalTime();
        utc.ShouldBe(new DateTime(2010, 8, expectedUtcDay, expectedUtcHour, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// The central European zones parse, and are the two the table matches without a leading space.
    /// </summary>
    /// <param name="zone">The zone name.</param>
    /// <param name="expectedUtcHour">The UTC hour the instant falls on.</param>
    [TestMethod]
    [DataRow("CET", 11)]
    [DataRow("CEST", 10)]
    public void ADateInACentralEuropeanZone_ParsesToTheCorrectInstant(string zone, int expectedUtcHour)
    {
        string value = $"Sun, 01 Aug 2010 12:00:00 {zone}";

        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime parsed)
            .ShouldBeTrue($"'{value}' was not recognised as an RFC 822 date");

        parsed.ToUniversalTime().ShouldBe(new DateTime(2010, 8, 1, expectedUtcHour, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// A numeric offset still parses, and is unaffected by the named-zone table.
    /// </summary>
    /// <param name="offset">The numeric offset.</param>
    /// <param name="expectedUtcHour">The UTC hour the instant falls on.</param>
    [TestMethod]
    [DataRow("+0000", 12)]
    [DataRow("-0500", 17)]
    [DataRow("+0100", 11)]
    public void ADateWithANumericOffset_ParsesToTheCorrectInstant(string offset, int expectedUtcHour)
    {
        string value = $"Sun, 01 Aug 2010 12:00:00 {offset}";

        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime parsed).ShouldBeTrue();

        parsed.ToUniversalTime().ShouldBe(new DateTime(2010, 8, 1, expectedUtcHour, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// A zone the table does not name is left alone rather than mistranslated.
    /// </summary>
    /// <remarks>
    ///     Pins the fall-through. <c>UTC</c> is the interesting case: the table matches <c>" UT"</c> by
    ///     suffix, and a date ending <c>" UTC"</c> does not match it, so it reaches the final else and is
    ///     handed to <see cref="DateTime.TryParse(string, out DateTime)"/> unchanged. Whatever that does,
    ///     it must not silently produce a wrong instant.
    /// </remarks>
    /// <param name="zone">A zone name the table does not translate.</param>
    /// <param name="expectedToParse">Whether the fall-through is expected to accept this spelling.</param>
    [TestMethod]
    [DataRow("UTC", true)]
    [DataRow("BST", false)]
    [DataRow("NZDT", false)]
    public void ADateInAnUnnamedZone_DoesNotProduceAMistranslatedInstant(string zone, bool expectedToParse)
    {
        string value = $"Sun, 01 Aug 2010 12:00:00 {zone}";

        bool parsedOk = SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime parsed);

        // The outcome used to be accepted either way, so the test could not detect a change in which
        // branch was taken. That mattered concretely: the product was fixed so that a " UTC" suffix
        // parses, and reverting that fix would have moved this row silently into the else arm.
        parsedOk.ShouldBe(expectedToParse, $"'{value}'");

        if (expectedToParse)
        {
            // It must land on the stated wall-clock time; the table must not have applied an offset
            // belonging to some other zone.
            parsed.ToUniversalTime().Hour.ShouldBe(12, $"'{value}' parsed to a shifted instant");
        }
        else
        {
            parsed.ShouldBe(DateTime.MinValue);
        }
    }

    /// <summary>
    /// A date that is not RFC 822 at all is rejected, and yields the default.
    /// </summary>
    /// <param name="value">A value that is not an RFC 822 date.</param>
    [TestMethod]
    [DataRow("not a date at all")]
    [DataRow("")]
    [DataRow("Sun, 32 Aug 2010 12:00:00 GMT")]
    public void AValueThatIsNotAnRfc822Date_IsRejected(string value)
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime(value, out DateTime parsed).ShouldBeFalse();
        parsed.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// The RFC 822 parser also accepts an ISO 8601 date.
    /// </summary>
    /// <remarks>
    ///     Pins leniency rather than endorsing it. After the zone table declines to match, the value is
    ///     handed to <see cref="DateTime.TryParse(string, out DateTime)"/> unchanged, which parses ISO
    ///     8601 happily. A caller cannot use this method to tell the two formats apart, and a feed that
    ///     puts an Atom-style date in an RSS pubDate is read rather than rejected.
    /// </remarks>
    [TestMethod]
    public void AnIso8601Date_IsAlsoAcceptedByTheRfc822Parser()
    {
        SyndicationDateTimeUtility.TryParseRfc822DateTime("2010-08-01T12:00:00Z", out DateTime parsed)
            .ShouldBeTrue();

        parsed.ToUniversalTime().ShouldBe(new DateTime(2010, 8, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// A feed carrying a named zone in its publication date exposes the correct instant.
    /// </summary>
    /// <remarks>
    ///     The utility is reachable from a real document, not only from its own entry point: this is the
    ///     path an actual consumer takes.
    /// </remarks>
    [TestMethod]
    public void AnRssItemDatedInANamedZone_ExposesTheCorrectInstant()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <title>A feed</title>
                <link>http://example.com/</link>
                <description>A feed</description>
                <item>
                  <title>An item</title>
                  <link>http://example.com/1</link>
                  <pubDate>Sun, 01 Aug 2010 12:00:00 EST</pubDate>
                </item>
              </channel>
            </rss>
            """;

        Argotic.Syndication.RssFeed feed = new();
        using StringReader stringReader = new(xml);
        using System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stringReader);
        feed.Load(reader);

        feed.Channel.Items.Single().PublicationDate.ToUniversalTime()
            .ShouldBe(new DateTime(2010, 8, 1, 17, 0, 0, DateTimeKind.Utc));
    }
}