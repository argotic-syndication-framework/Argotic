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
        int start = document.IndexOf("<entry", StringComparison.Ordinal);
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
    /// A2 — an xhtml title keeps the div's markup, and round-trips it as markup.
    /// </summary>
    /// <remarks>
    ///     §3.1.1.3: the content of the single XHTML div is the construct's content — markup
    ///     included. <c>Content</c> now holds the div's inner markup; the child's re-declared
    ///     default namespace is the price of a string model and is a same-scope no-op on the wire.
    ///     Cycled twice, because a fixed point is the claim.
    /// </remarks>
    [TestMethod]
    public void A2_AnXhtmlTitle_KeepsTheMarkupAndRoundTripsIt()
    {
        AtomFeed feed = Load(
            """<title type="xhtml"><div xmlns="http://www.w3.org/1999/xhtml">bold: <b>yes</b> plain</div></title>""");

        feed.Entries.First().Title!.Content.ShouldBe(
            """bold: <b xmlns="http://www.w3.org/1999/xhtml">yes</b> plain""",
            "INVERTED: the <b> element survives into the model");

        string once = SaveEntry(feed);
        once.ShouldContain(">yes</b> plain</div>", customMessage: "INVERTED: emitted as markup, not escaped text");

        AtomFeed second = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Save(feed)));
        second.Load(stream);
        SaveEntry(second).ShouldContain(">yes</b> plain</div>", customMessage: "and the second cycle is a fixed point");
    }

    /// <summary>
    /// A3 — xhtml content keeps the markup on read and now writes it as markup too.
    /// </summary>
    /// <remarks>
    ///     The read half was always faithful and is the invariant; the save half inverted. Both
    ///     classes now share <c>AtomUtility.WriteXhtmlDiv</c>, which parses the fragment back into
    ///     nodes — so escaping cannot happen, and malformed content fails loudly instead of
    ///     producing an invalid document (pinned separately below).
    /// </remarks>
    [TestMethod]
    public void A3_XhtmlContent_KeepsMarkupOnReadAndWritesItAsMarkup()
    {
        AtomFeed feed = Load(
            """<content type="xhtml"><div xmlns="http://www.w3.org/1999/xhtml">bold: <b>yes</b> plain</div></content>""");

        feed.Entries.First().Content!.Content.ShouldBe(
            """bold: <b xmlns="http://www.w3.org/1999/xhtml">yes</b> plain""",
            "INVARIANT: read faithfully");

        string saved = SaveEntry(feed);
        saved.ShouldContain(">yes</b> plain</div>", customMessage: "INVERTED: markup emitted as markup");
        saved.ShouldNotContain("&lt;b", customMessage: "and nothing is escaped into literal text");
    }

    /// <summary>
    /// A3 — malformed caller-supplied xhtml fails loudly at save, not silently on the wire.
    /// </summary>
    /// <remarks>
    ///     The cost of writing real nodes is that the fragment must parse, and that is a feature:
    ///     the alternative to the exception is an invalid document handed to every subscriber.
    /// </remarks>
    [TestMethod]
    public void A3_MalformedXhtmlContent_ThrowsAtSaveRatherThanEmittingIt()
    {
        AtomFeed feed = Load("""<content type="xhtml"><div xmlns="http://www.w3.org/1999/xhtml">fine</div></content>""");
        feed.Entries.First().Content!.Content = "an <unclosed fragment";

        using MemoryStream sink = new();
        Should.Throw<System.Xml.XmlException>(() => feed.Save(sink));
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
    /// A5 — every one of the spec's six distinct ids round-trips character-for-character.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     §4.2.6.2 lists six IRIs that are "all different"; §4.2.6.1 says an id "MUST NOT change".
    ///     <c>System.Uri</c> lowercased the scheme and host and decoded <c>%74</c> — in the model and
    ///     in the round-tripped document — so three of the six came back altered. Identity now lives
    ///     on <c>AtomId.Value</c>, the raw character string, and the document gets its exact
    ///     characters back.
    ///     </para>
    ///     <para>
    ///     <c>Case.Sensitive</c> throughout, learned the hard way: Shouldly's default string
    ///     <c>ShouldContain</c> is case-insensitive, which silently passed the first version of this
    ///     row for the case-normalisation defects it existed to catch.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("http://www.example.org/thing", DisplayName = "plain")]
    [DataRow("http://www.example.org/Thing", DisplayName = "path case")]
    [DataRow("http://www.EXAMPLE.org/thing", DisplayName = "INVERTED: host case survives")]
    [DataRow("HTTP://www.example.org/thing", DisplayName = "INVERTED: scheme case survives")]
    [DataRow("http://www.example.org/%74hing", DisplayName = "INVERTED: percent-encoding survives")]
    public void A5_AnEntryId_RoundTripsCharacterForCharacter(string written)
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

        idFeed.Entries.First().Id!.Value.ShouldBe(written, "the model holds the exact characters");
        SaveEntry(idFeed).ShouldContain($"<id>{written}</id>", Case.Sensitive);
    }

    /// <summary>
    /// A5 — ids the spec lists as distinct now compare distinct.
    /// </summary>
    [TestMethod]
    public void A5_CaseDifferingIds_CompareDistinct()
    {
        AtomId a = new(new Uri("http://www.example.org/thing"));
        AtomId b = new(new Uri("HTTP://www.EXAMPLE.org/thing"));

        a.Equals(b).ShouldBeFalse("INVERTED: §4.2.6 comparison is character-by-character, case-sensitive");
        a.Value.ShouldBe("http://www.example.org/thing");
        b.Value.ShouldBe("HTTP://www.EXAMPLE.org/thing", "OriginalString, before System.Uri had opinions");
    }

    /// <summary>
    /// A5 — an id System.Uri cannot parse still loads, because the characters are the identity.
    /// </summary>
    /// <remarks>
    ///     A relative id is invalid Atom (§4.2.6: the content must be an IRI), but dropping it
    ///     silently lost the one value the spec says must never change. It loads with
    ///     <see cref="AtomId.Uri"/> null and the characters intact — the same tolerance-asymmetry
    ///     rule as the person construct: read what the wild wrote, refuse to invent.
    /// </remarks>
    [TestMethod]
    public void A5_AnUnparseableId_KeepsItsCharacters()
    {
        AtomFeed relFeed = new();
        string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
                <id>https://conformance.invalid/feed</id>
                <title>Shell</title>
                <updated>2026-01-02T03:04:05Z</updated>
                <entry>
                    <id>relative/identifier</id>
                    <updated>2026-01-02T03:04:05Z</updated>
                </entry>
            </feed>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        relFeed.Load(stream);

        AtomId id = relFeed.Entries.First().Id.ShouldNotBeNull();
        id.Value.ShouldBe("relative/identifier");
        id.Uri.ShouldBeNull("System.Uri cannot represent it; the characters still can");
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
    /// A9 — xml:base is inherited downward, so the link a consumer holds can resolve its own href.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     §2 requires processors to handle xml:base per W3C XML Base, and inheritance is most of
    ///     what that means. The link used to come back with a relative href and a null base — the
    ///     ancestor that knew the base was gone by the time <c>Load</c> returned.
    ///     </para>
    ///     <para>
    ///     The visible cost is on the write side, pinned here deliberately: every descendant now
    ///     re-states its effective base as its own <c>xml:base</c>. Verbose, semantically identical,
    ///     conformant — and a fixed point, because on the second cycle each element's own attribute
    ///     equals its inherited context.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void A9_XmlBase_IsInheritedDownToTheLink()
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

        AtomLink link = feed.Entries.First().Links.First();
        link.BaseUri.ShouldBe(
            new Uri("https://conformance.invalid/base/"), "INVERTED: the effective base travels with the link");
        link.Uri.ShouldBe(new Uri("relative/page", UriKind.Relative), "INVARIANT: the href itself stays as written");

        new Uri(link.BaseUri!, link.Uri!).ShouldBe(
            new Uri("https://conformance.invalid/base/relative/page"),
            "which is the resolution a consumer could not previously perform");

        SaveEntry(feed).ShouldContain(
            """<link xml:base="https://conformance.invalid/base/" href="relative/page" />""",
            Case.Sensitive,
            "the pinned cost: descendants re-state their effective base on save");
    }

    /// <summary>
    /// A9 — a relative xml:base stacks against its ancestors, outermost first.
    /// </summary>
    /// <remarks>W3C XML Base §4.3: each relative base resolves against the one above it.</remarks>
    [TestMethod]
    public void A9_ARelativeXmlBase_StacksAgainstItsAncestors()
    {
        AtomFeed feed = new();
        string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xml:base="https://conformance.invalid/base/">
                <id>https://conformance.invalid/feed</id>
                <title>Shell</title>
                <updated>2026-01-02T03:04:05Z</updated>
                <entry xml:base="deeper/">
                    <id>https://conformance.invalid/1</id>
                    <updated>2026-01-02T03:04:05Z</updated>
                    <link href="relative/page"/>
                </entry>
            </feed>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        AtomEntry entry = feed.Entries.First();
        entry.BaseUri.ShouldBe(
            new Uri("https://conformance.invalid/base/deeper/"),
            "the entry's own relative base resolves against the feed's");
        entry.Links.First().BaseUri.ShouldBe(
            new Uri("https://conformance.invalid/base/deeper/"),
            "and the link inherits the stacked result");
    }
}