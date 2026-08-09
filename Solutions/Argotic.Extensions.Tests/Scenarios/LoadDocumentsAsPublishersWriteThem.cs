namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Loads documents shaped the way real publishers actually write them, through the public entry points
/// a consumer uses.
/// </summary>
/// <remarks>
///     <para>
///     Distilled from a corpus of 136 live documents, 76.2 MiB, fetched byte-exact from real publishers.
///     The corpus itself is machine-local and gitignored — 76 MiB of third-party content is not
///     something to commit, and a suite that downloaded it would go red whenever a publisher edited a
///     title. What is committed is the <em>shapes</em>: each literal below reproduces a construct the
///     corpus proved real, and names the document it came from.
///     </para>
///     <para>
///     <b>Why these are scenarios rather than functionality tests.</b> Every one spans a document
///     preamble and the object model it produces — the question is not "does this method work" but
///     "does a document written like this survive being read". The preamble constructs in particular
///     (byte-order marks, processing instructions, quoting styles) can only be tested through a real
///     load, because they are consumed before any parser the suite can call directly.
///     </para>
///     <para>
///     The census that motivated each is in the doc comment for that test. In summary, across the
///     corpus: <b>3</b> documents open with a UTF-8 byte-order mark, <b>10</b> quote their XML
///     declaration with apostrophes, <b>13</b> have no declaration at all, <b>12</b> carry an
///     <c>xml-stylesheet</c> processing instruction ahead of the root element, and <b>65</b> contain
///     non-ASCII bytes. The suite's hand-written corpus contained none of any of these.
///     </para>
/// </remarks>
[TestClass]
public sealed class LoadDocumentsAsPublishersWriteThem
{
    private const string Rss20Body = """
        <rss version="2.0"><channel>
          <title>A Channel</title>
          <link>https://example.com/</link>
          <description>A description.</description>
          <item><title>An Item</title><link>https://example.com/1</link></item>
        </channel></rss>
        """;

    private static RssFeed LoadRss(byte[] document)
    {
        using MemoryStream stream = new(document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// A feed that opens with a UTF-8 byte-order mark loads, and the mark does not leak into the title.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Three corpus documents begin with <c>EF BB BF</c>: a BlogEngine.NET BlogML export and two
    ///     BlogEngine.NET feeds. A byte-order mark ahead of an XML declaration is legal and .NET tooling
    ///     emits it by default, so it is what a great deal of Windows-authored XML looks like.
    ///     </para>
    ///     <para>
    ///     <b>The second assertion is the one that matters.</b> A mark handled by the wrong layer does
    ///     not throw — it survives as U+FEFF at the front of the first text node, and the feed then
    ///     parses "successfully" with an invisible character welded to its title. Asserting only that
    ///     the load succeeded would pass in exactly that case.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AFeedOpeningWithAByteOrderMark_LoadsWithoutTheMarkLeakingIntoItsContent()
    {
        byte[] document = [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes($"""<?xml version="1.0" encoding="utf-8"?>{Rss20Body}""")];

        document[0].ShouldBe((byte)0xEF, "the fixture must actually carry a mark, or this test proves nothing");

        RssFeed feed = LoadRss(document);

        feed.Channel.Title.ShouldBe("A Channel");
        feed.Channel.Title.StartsWith('﻿')
            .ShouldBeFalse("the byte-order mark must not survive into the object model");
    }

    /// <summary>
    /// A feed whose declaration is quoted with apostrophes is read in its declared encoding.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     XML permits either quoting style, and 10 corpus documents use apostrophes — arXiv, Blogger,
    ///     LiveJournal and Tim Bray's <i>ongoing</i> among them. The encoding was read with a regular
    ///     expression that handled only the double-quoted and bare forms, so an apostrophe-quoted value
    ///     was captured <em>with its quotes</em>, <c>Encoding.GetEncoding("'utf-8'")</c> threw, and the
    ///     reader fell back to UTF-8. That is §2.42's fix; this is the end-to-end guard on it.
    ///     </para>
    ///     <para>
    ///     <b>The ISO-8859-1 row is the one with teeth.</b> All ten real documents happen to be UTF-8,
    ///     so the fallback silently produced the right answer and nothing looked wrong. A document that
    ///     is genuinely not UTF-8 is where the defect shows, so the fixture is encoded in ISO-8859-1 for
    ///     real — <c>é</c> as the single byte <c>0xE9</c> — and the assertion is on the decoded
    ///     character. Under the old behaviour that byte is invalid UTF-8 and arrives as U+FFFD.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow('\'', "apostrophes, as arXiv and Blogger write it")]
    [DataRow('"', "quotation marks — the control")]
    public void AFeedDeclaringANonUtf8EncodingWithEitherQuotingStyle_IsDecodedCorrectly(char quote, string style)
    {
        Encoding latin1 = Encoding.GetEncoding("iso-8859-1");
        string document = $"<?xml version={quote}1.0{quote} encoding={quote}iso-8859-1{quote}?>"
            + Rss20Body.Replace("A Channel", "Café naïve", StringComparison.Ordinal);

        byte[] bytes = latin1.GetBytes(document);

        bytes.ShouldContain((byte)0xE9, "the fixture must really be ISO-8859-1, not UTF-8 in disguise");

        RssFeed feed = LoadRss(bytes);

        feed.Channel.Title.ShouldBe("Café naïve", style);
        feed.Channel.Title.Contains('�', StringComparison.Ordinal).ShouldBeFalse(style);
    }

    /// <summary>
    /// A feed with a stylesheet instruction ahead of its root element loads.
    /// </summary>
    /// <remarks>
    ///     Twelve corpus documents carry one, which is how a feed renders as a page when opened in a
    ///     browser: Blogger, FeedBurner, Yoast sitemaps and Hanselman's feed all do it. The instruction
    ///     sits between the declaration and the root, which is the one position an encoding sniff
    ///     reading a fixed-size prefix can be thrown by.
    /// </remarks>
    [TestMethod]
    public void AFeedWithAStylesheetInstructionBeforeItsRoot_Loads()
    {
        string document = """
            <?xml version="1.0" encoding="utf-8"?>
            <?xml-stylesheet type="text/xsl" media="screen" href="/~d/styles/atom10full.xsl"?>
            """ + "\n" + Rss20Body;

        RssFeed feed = LoadRss(Encoding.UTF8.GetBytes(document));

        feed.Channel.Title.ShouldBe("A Channel");
        feed.Channel.Items.Count.ShouldBe(1);
    }

    /// <summary>
    /// A feed with no XML declaration at all loads.
    /// </summary>
    /// <remarks>
    ///     Thirteen corpus documents omit it. It is legal — the declaration is optional and the encoding
    ///     then defaults to UTF-8 — and it is the shape that exercises the sniffing path's "found
    ///     nothing" branch rather than its match branch.
    /// </remarks>
    [TestMethod]
    public void AFeedWithNoXmlDeclaration_Loads()
    {
        RssFeed feed = LoadRss(Encoding.UTF8.GetBytes(Rss20Body));

        feed.Channel.Title.ShouldBe("A Channel");
    }

    /// <summary>
    /// A declaration carrying a standalone pseudo-attribute loads.
    /// </summary>
    /// <remarks>
    ///     Five corpus documents declare <c>standalone</c>, in both spellings. It sits after the
    ///     encoding, so a pattern anchored too tightly to the end of the declaration stops matching.
    /// </remarks>
    [TestMethod]
    [DataRow("yes")]
    [DataRow("no")]
    public void AFeedDeclaringStandalone_Loads(string standalone)
    {
        string document = $"""<?xml version="1.0" encoding="utf-8" standalone="{standalone}"?>{Rss20Body}""";

        LoadRss(Encoding.UTF8.GetBytes(document)).Channel.Title.ShouldBe("A Channel", standalone);
    }

    /// <summary>
    /// Non-ASCII titles survive a load intact, in the scripts real feeds actually carry.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     65 of the 136 corpus documents contain non-ASCII bytes and the suite's hand-written corpus
    ///     contained none, so nothing here had ever read a character outside ASCII. Each of these is a
    ///     real title, shortened: Norwegian from a Drupal feed, German from heise.de, Portuguese from
    ///     NASA, a typographic apostrophe from the New York Times, an en dash from Wikipedia, and an
    ///     umlaut from Reddit.
    ///     </para>
    ///     <para>
    ///     The last two rows are the interesting ones. <c>’</c> and <c>–</c> are not accented letters —
    ///     they are punctuation that looks like ASCII punctuation, which is exactly the substitution a
    ///     lossy decode makes without anything looking obviously broken.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("Helge Notø Makes Open-Source Stewardship a Focus", "Norwegian ø")]
    [DataRow("Agenten dürfen vorerst wieder auf Amazon shoppen", "German ü")]
    [DataRow("The Paradox of Lençóis Maranhenses National Park", "Portuguese ç and ó")]
    [DataRow("In Texas’ Most Eligible Senate Race", "typographic apostrophe U+2019")]
    [DataRow("2026–27 Neftçi PFK season", "en dash U+2013")]
    [DataRow("Gödel, Escher, Elisp: The Beauty of Macros", "umlaut ö")]
    public void ANonAsciiTitle_SurvivesALoadIntact(string title, string script)
    {
        string document = $"""<?xml version="1.0" encoding="utf-8"?>{Rss20Body}""".Replace("An Item", title, StringComparison.Ordinal);

        RssFeed feed = LoadRss(Encoding.UTF8.GetBytes(document));

        feed.Channel.Items.Single().Title.ShouldBe(title, script);
    }

    /// <summary>
    /// A sitemap whose every <c>lastmod</c> is a bare date keeps them all.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     GitLab's sitemap does exactly this: <b>2,794 of 2,794</b> <c>lastmod</c> values in
    ///     <c>extensions/sitemap-hreflang/gitlab-pages.xml</c> are <c>YYYY-MM-DD</c> with no time. The
    ///     sitemap protocol permits it by citing the W3C Datetime profile.
    ///     </para>
    ///     <para>
    ///     It is worth an end-to-end test because the obvious reading of the code says it should fail:
    ///     <c>SitemapUrl.Load</c> reaches for <c>TryParseRfc3339DateTime</c> first, and that correctly
    ///     refuses a date with no time. A second, more permissive <c>DateTime.TryParse</c> behind it is
    ///     what saves the value — so the behaviour depends on a fallback that reads like belt-and-braces
    ///     and is in fact the only thing standing between GitLab's sitemap and total loss of its dates.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ASitemapWhoseEveryLastmodIsABareDate_KeepsThem()
    {
        const string Document = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url><loc>https://example.com/a</loc><lastmod>2026-08-05</lastmod></url>
              <url><loc>https://example.com/b</loc><lastmod>2026-08-05T14:30:00+00:00</lastmod></url>
            </urlset>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Document), writable: false);
        Sitemap sitemap = new();
        sitemap.Load(stream);

        sitemap.Urls.Count.ShouldBe(2);

        SitemapUrl bare = sitemap.Urls[0];
        bare.LastModified.ShouldNotBeNull("a bare date is legal in a sitemap and must not be dropped");
        bare.LastModified.Value.Date.ShouldBe(new DateTime(2026, 8, 5, 0, 0, 0, bare.LastModified.Value.Kind));

        sitemap.Urls[1].LastModified.ShouldNotBeNull("the full timestamp — the control");
    }

    /// <summary>
    /// A podcast feed spelling its dates the way Simplecast does keeps every publication date.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>extensions/itunes/simplecast-thedaily.xml</c> — the New York Times' <i>The Daily</i> —
    ///     writes <b>859 of its 2,940</b> episode dates with an unpadded day of month, which RFC 822
    ///     permits (<c>1*2DIGIT</c>) and which no generator in this repository produces. The remaining
    ///     episodes are padded, so a real feed carries both spellings and a reader has to take them
    ///     equally.
    ///     </para>
    ///     <para>
    ///     Both items below carry the same instant, so the assertion that they are equal fails if either
    ///     spelling is dropped — a dropped date arrives as <see cref="DateTime.MinValue"/> rather than as
    ///     an exception.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void APodcastFeedMixingPaddedAndUnpaddedDays_KeepsEveryPublicationDate()
    {
        const string Document = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0"><channel>
              <title>A Podcast</title>
              <link>https://example.com/</link>
              <description>A description.</description>
              <item><title>unpadded</title><pubDate>Fri, 1 Apr 2022 09:50:00 +0000</pubDate></item>
              <item><title>padded</title><pubDate>Fri, 01 Apr 2022 09:50:00 +0000</pubDate></item>
            </channel></rss>
            """;

        RssFeed feed = LoadRss(Encoding.UTF8.GetBytes(Document));

        DateTime unpadded = feed.Channel.Items.First(item => item.Title == "unpadded").PublicationDate;
        DateTime padded = feed.Channel.Items.First(item => item.Title == "padded").PublicationDate;

        unpadded.ShouldNotBe(DateTime.MinValue, "an unpadded day of month is legal RFC 822");
        unpadded.ShouldBe(padded, "the two spellings name the same instant");
    }
}