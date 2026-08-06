using System.Text;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Pins the divergences the RFC 4287 conformance review found, so each fix inverts named rows.
/// </summary>
/// <remarks>
///     <para>
///     The review (<c>.endjin/ATOM-CONFORMANCE-REVIEW.md</c>) probed each suspicion with running
///     code; these are those probes as tests. Rows marked <b>PINS TODAY</b> assert behaviour the
///     review classified as divergent and are inverted by the fix commits; rows marked
///     <b>INVARIANT</b> assert behaviour that must survive every fix.
///     </para>
///     <para>
///     The worst of them compounds: an <c>html</c>-typed text construct double-escapes on every
///     load-save cycle, so a feed republished through Argotic corrupts a little more each pass.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomSpecConformanceTests
{
    private static AtomFeed Load(string entryChildren)
    {
        AtomFeed feed = new();
        string xml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
                <id>https://conformance.invalid/feed</id>
                <title>Shell</title>
                <updated>2026-01-02T03:04:05Z</updated>
                <entry>
                    <id>https://conformance.invalid/1</id>
                    <updated>2026-01-02T03:04:05Z</updated>
                    {entryChildren}
                </entry>
            </feed>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);
        return feed;
    }

    private static string Save(AtomFeed feed)
    {
        using MemoryStream stream = new();
        feed.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string SaveEntry(AtomFeed feed)
    {
        string document = Save(feed);
        int start = document.IndexOf("<entry>", StringComparison.Ordinal);
        int end = document.IndexOf("</entry>", StringComparison.Ordinal);
        return document[start..end];
    }

    // ---- A1: type="html" text constructs -------------------------------------------------------

    /// <summary>
    /// A1 — an html title exposes the HTML itself.
    /// </summary>
    /// <remarks>
    ///     §3.1.1.2: the markup is escaped for XML transport; the logical content after XML
    ///     processing is the HTML itself. <c>Load</c> used to read <c>InnerXml</c> — the transport
    ///     form — and now reads <c>Value</c>, converging on the model <c>AtomContent</c> always had.
    /// </remarks>
    [TestMethod]
    public void A1_AnHtmlTitle_ExposesTheHtmlItself()
    {
        AtomFeed feed = Load("""<title type="html">bold: &lt;b&gt;yes&lt;/b&gt; plain</title>""");

        feed.Entries.First().Title!.Content.ShouldBe(
            "bold: <b>yes</b> plain",
            "INVERTED: the logical HTML, exactly as AtomContent exposes the same input");
    }

    /// <summary>
    /// A1 — an html title round-trips byte-identically, however many times it cycles.
    /// </summary>
    /// <remarks>
    ///     Cycled twice deliberately: the defect this inverts compounded, so a single round trip is
    ///     not proof of a fixed point.
    /// </remarks>
    [TestMethod]
    public void A1_AnHtmlTitle_RoundTripsIdenticallyEveryCycle()
    {
        AtomFeed first = Load("""<title type="html">bold: &lt;b&gt;yes&lt;/b&gt; plain</title>""");
        string once = SaveEntry(first);

        once.ShouldContain(
            "bold: &lt;b&gt;yes&lt;/b&gt; plain",
            customMessage: "INVERTED: escaped exactly once, as written");

        AtomFeed second = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Save(first)));
        second.Load(stream);
        string twice = SaveEntry(second);

        twice.ShouldContain(
            "bold: &lt;b&gt;yes&lt;/b&gt; plain",
            customMessage: "and the second cycle is a fixed point, which is what the defect was not");
    }

    /// <summary>
    /// A1 invariant — AtomContent's html handling is the correct model the fix converges on.
    /// </summary>
    [TestMethod]
    public void A1_AtomContentHtml_ExposesTheHtmlItself()
    {
        AtomFeed feed = Load("""<content type="html">bold: &lt;b&gt;yes&lt;/b&gt; plain</content>""");

        feed.Entries.First().Content!.Content.ShouldBe(
            "bold: <b>yes</b> plain", "INVARIANT: the logical HTML, escaped exactly once on save");

        SaveEntry(feed).ShouldContain("bold: &lt;b&gt;yes&lt;/b&gt; plain");
    }

    // ---- A2/A3: type="xhtml" -------------------------------------------------------------------

    /// <summary>
    /// A2 — an xhtml title silently strips the div's markup.
    /// </summary>
    /// <remarks>
    ///     §3.1.1.3: the content of the single XHTML div is the construct's content — markup
    ///     included. <c>AtomTextConstruct</c> takes <c>.Value</c>, the flattened text.
    /// </remarks>
    [TestMethod]
    public void A2_AnXhtmlTitle_StripsTheMarkup()
    {
        AtomFeed feed = Load(
            """<title type="xhtml"><div xmlns="http://www.w3.org/1999/xhtml">bold: <b>yes</b> plain</div></title>""");

        feed.Entries.First().Title!.Content.ShouldBe(
            "bold: yes plain", "PINS TODAY: the <b> element is silently destroyed");
    }

    /// <summary>
    /// A3 — xhtml content keeps the markup on read and escapes it into text on save.
    /// </summary>
    /// <remarks>
    ///     The two classes give the two different wrong answers: the text construct flattens, the
    ///     content element preserves-then-betrays. A browser renders the round-tripped tags
    ///     literally.
    /// </remarks>
    [TestMethod]
    public void A3_XhtmlContent_KeepsMarkupOnReadThenEscapesItOnSave()
    {
        AtomFeed feed = Load(
            """<content type="xhtml"><div xmlns="http://www.w3.org/1999/xhtml">bold: <b>yes</b> plain</div></content>""");

        feed.Entries.First().Content!.Content.ShouldBe(
            """bold: <b xmlns="http://www.w3.org/1999/xhtml">yes</b> plain""",
            "PINS TODAY: read faithfully, xmlns noise included");

        SaveEntry(feed).ShouldContain(
            "&lt;b xmlns=",
            customMessage: "PINS TODAY: the faithfully-read markup is escaped into literal text on save");
    }

    // ---- A4: inline XML media types ------------------------------------------------------------

    /// <summary>
    /// A4 — rule-5 inline XML content is flattened to its text value.
    /// </summary>
    /// <remarks>§4.1.3.3 rule 5: XML media types MAY carry child elements.</remarks>
    [TestMethod]
    public void A4_InlineXmlContent_IsFlattenedToText()
    {
        AtomFeed feed = Load(
            """<content type="application/xml"><data xmlns=""><value>42</value></data></content>""");

        feed.Entries.First().Content!.Content.ShouldBe(
            "42", "PINS TODAY: the element structure is unrecoverable");
    }

    // ---- A5: atom:id identity ------------------------------------------------------------------

    /// <summary>
    /// A5 — three of the spec's own six distinct ids are normalised into each other.
    /// </summary>
    /// <remarks>
    ///     §4.2.6.2 lists six IRIs that are "all different"; §4.2.6 requires character-by-character
    ///     case-sensitive comparison; §4.2.6.1 says an id "MUST NOT change". <c>System.Uri</c>
    ///     lowercases the scheme and host and decodes <c>%74</c>, in the model and in the
    ///     round-tripped document.
    /// </remarks>
    [TestMethod]
    [DataRow("http://www.example.org/thing", "http://www.example.org/thing", DisplayName = "lowercase survives")]
    [DataRow("http://www.example.org/Thing", "http://www.example.org/Thing", DisplayName = "path case survives")]
    [DataRow("http://www.EXAMPLE.org/thing", "http://www.example.org/thing", DisplayName = "PINS TODAY: host lowercased")]
    [DataRow("HTTP://www.example.org/thing", "http://www.example.org/thing", DisplayName = "PINS TODAY: scheme lowercased")]
    [DataRow("http://www.example.org/%74hing", "http://www.example.org/thing", DisplayName = "PINS TODAY: percent-encoding decoded")]
    public void A5_AnEntryId_RoundTripsAs(string written, string roundTripped)
    {
        AtomFeed idFeed = new();
        string xml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
                <id>https://conformance.invalid/feed</id>
                <title>Shell</title>
                <updated>2026-01-02T03:04:05Z</updated>
                <entry>
                    <id>{written}</id>
                    <updated>2026-01-02T03:04:05Z</updated>
                </entry>
            </feed>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        idFeed.Load(stream);

        SaveEntry(idFeed).ShouldContain($"<id>{roundTripped}</id>");
    }

    /// <summary>
    /// A5 — ids the spec lists as distinct compare equal.
    /// </summary>
    [TestMethod]
    public void A5_CaseDifferingIds_CompareEqual()
    {
        AtomId a = new(new Uri("http://www.example.org/thing"));
        AtomId b = new(new Uri("HTTP://www.EXAMPLE.org/thing"));

        a.Equals(b).ShouldBeTrue("PINS TODAY: Uri equality unifies ids the spec says are different");
    }

    // ---- A6: date constructs -------------------------------------------------------------------

    /// <summary>
    /// A6 — the parsed Kind depends on how the offset was spelled.
    /// </summary>
    /// <remarks>
    ///     A numeric offset is genuinely adjusted to UTC; a <c>Z</c> is matched as a format
    ///     literal, yielding <see cref="DateTimeKind.Unspecified"/> with a UTC wall-clock. The
    ///     round trip holds only because the writer stamps <c>Z</c> on every non-Local Kind.
    /// </remarks>
    [TestMethod]
    public void A6_TheParsedKind_DependsOnTheOffsetSpelling()
    {
        AtomFeed feed = Load("""<published>2026-01-02T08:49:05+05:45</published>""");
        AtomEntry entry = feed.Entries.First();

        entry.PublishedOn.ShouldBe(
            new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            "INVARIANT: a numeric offset is adjusted to the right instant");
        entry.PublishedOn.Kind.ShouldBe(DateTimeKind.Utc);

        entry.UpdatedOn.Kind.ShouldBe(
            DateTimeKind.Unspecified,
            "PINS TODAY: the same instant written with Z parses to a different Kind");
    }

    // ---- A9: xml:base --------------------------------------------------------------------------

    /// <summary>
    /// A9 — xml:base is captured where it appeared and not inherited downward.
    /// </summary>
    /// <remarks>
    ///     §2 requires processors to handle xml:base per W3C XML Base. The one object a consumer
    ///     holds — the link — has neither an absolute href nor the base needed to make one.
    /// </remarks>
    [TestMethod]
    public void A9_XmlBase_IsNotInheritedDownToTheLink()
    {
        AtomFeed feed = new();
        string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xml:base="https://conformance.invalid/base/">
                <id>https://conformance.invalid/feed</id>
                <title>Shell</title>
                <updated>2026-01-02T03:04:05Z</updated>
                <entry>
                    <id>https://conformance.invalid/1</id>
                    <updated>2026-01-02T03:04:05Z</updated>
                    <link href="relative/page"/>
                </entry>
            </feed>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        feed.BaseUri.ShouldBe(new Uri("https://conformance.invalid/base/"), "INVARIANT: captured where it appeared");

        AtomLink link = feed.Entries.First().Links.First();
        link.BaseUri.ShouldBeNull("PINS TODAY: nothing is inherited downward");
        link.Uri.ShouldBe(new Uri("relative/page", UriKind.Relative), "INVARIANT: the href itself stays as written");
    }
}