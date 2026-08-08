namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Covers the gap between what a real feed <em>declares</em> and what it actually <em>contains</em>.
/// </summary>
/// <remarks>
///     <para>
///     Publishers declare namespaces they never use. Across the corpus of 136 live documents,
///     <b>12 declare <c>content:</c> and never emit a single element in it</b>; 7 do the same with
///     <c>xsi:</c>, 6 with <c>wfw:</c>, 6 with <c>dc:</c>, 5 with <c>slash:</c>, 4 with <c>media:</c>
///     and 3 with <c>georss:</c>. A wider survey of 627 live feeds found <c>geo:</c> declared by 121
///     and used by 5.
///     </para>
///     <para>
///     <b>This is the behaviour the largest open performance decision would change.</b> §2.40 measured
///     extension detection at <b>26% of a 50,000-URL sitemap load in a document declaring no extensions
///     at all</b>, and §5.1 item 4 records the fix — caching the probe list at document level — as
///     deliberately not taken. Whoever takes it needs a test that says what the answer must still be,
///     and the risk of that change is precisely that a cached, hoisted probe list starts reporting
///     extensions for namespaces that were declared but never used. These tests fail if it does.
///     </para>
/// </remarks>
[TestClass]
public sealed class ReadFeedsThatDeclareMoreThanTheyUse
{
    /// <summary>
    /// The six extension namespaces most often declared and never used in the corpus.
    /// </summary>
    private const string SixDeclaredNamespaces = """
        xmlns:content="http://purl.org/rss/1.0/modules/content/"
             xmlns:wfw="http://wellformedweb.org/CommentAPI/"
             xmlns:dc="http://purl.org/dc/elements/1.1/"
             xmlns:slash="http://purl.org/rss/1.0/modules/slash/"
             xmlns:media="http://search.yahoo.com/mrss/"
             xmlns:georss="http://www.georss.org/georss"
        """;

    private static RssFeed Load(string document)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static string FeedDeclaring(string extraItemContent) => $"""
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0" {SixDeclaredNamespaces}>
          <channel>
            <title>A Channel</title>
            <link>https://example.com/</link>
            <description>A description.</description>
            <item><title>An Item</title><link>https://example.com/1</link>{extraItemContent}</item>
          </channel>
        </rss>
        """;

    /// <summary>
    /// A feed declaring six extension namespaces and using none carries no extensions.
    /// </summary>
    /// <remarks>
    ///     Both the channel and the item are asserted, because they are filled by separate adapters and
    ///     each builds its own <c>SyndicationExtensionAdapter</c> — a hoisting change could plausibly
    ///     get one right and the other wrong.
    /// </remarks>
    [TestMethod]
    public void AFeedDeclaringSixNamespacesAndUsingNone_CarriesNoExtensions()
    {
        RssFeed feed = Load(FeedDeclaring(string.Empty));

        feed.Channel.HasExtensions.ShouldBeFalse("the channel uses none of the six");
        feed.Channel.Extensions.ShouldBeEmpty();

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeFalse("nor does the item");
        item.Extensions.ShouldBeEmpty();
    }

    /// <summary>
    /// Using one of the six declared namespaces attaches that extension, and only that one.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The control, and it is the half that makes the test above mean something. Without it,
    ///     "no extensions were found" is equally consistent with extension detection being broken
    ///     outright, which would be a far worse result than the one being guarded against.
    ///     </para>
    ///     <para>
    ///     The count assertion matters as much as the type: <b>one</b> element in <b>one</b> of the six
    ///     declared namespaces must yield exactly one extension, not six.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void UsingOneOfTheDeclaredNamespaces_AttachesThatExtensionAndOnlyThatOne()
    {
        RssFeed feed = Load(FeedDeclaring("<dc:creator>Ada Lovelace</dc:creator>"));

        RssItem item = feed.Channel.Items.Single();

        item.HasExtensions.ShouldBeTrue();
        item.Extensions.Count.ShouldBe(1, "one element in one namespace is one extension, not six");
    }

    /// <summary>
    /// A feed carrying the DOCTYPE that RSS 0.91 requires still loads.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>The compatibility half of a security setting.</b>
    ///     <c>SyndicationEncodingUtility.CreateSafeXmlReaderSettings</c> pairs
    ///     <c>DtdProcessing.Parse</c> with <c>XmlResolver = null</c>, and the pairing is the point:
    ///     <c>Parse</c> alone would allow external entity resolution, while <c>Prohibit</c> would reject
    ///     documents that are entirely legitimate.
    ///     </para>
    ///     <para>
    ///     Two corpus documents prove the second half is not hypothetical —
    ///     <c>rss09x/wired-netcenter-rss091.xml</c> and <c>rss09x/xmlcom-news-rss091-2001.xml</c> both
    ///     carry the Netscape RSS 0.91 DOCTYPE, because the 0.91 specification defined the format with a
    ///     DTD and told publishers to reference it.
    ///     </para>
    ///     <para>
    ///     <c>ParseEntryPointGuardTests</c> already covers the attack this setting defends against, by
    ///     way of a billion-laughs document. This is the other direction: the defence must not cost the
    ///     library the feeds it exists to read. The external identifier here is a real URL that must
    ///     <em>not</em> be fetched, so a test run stays offline — if resolution were ever enabled, this
    ///     test would start making a network request and the suite's offline guarantee would be the
    ///     thing that broke.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AFeedCarryingTheRss091Doctype_StillLoads()
    {
        const string Document = """
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE rss PUBLIC "-//Netscape Communications//DTD RSS 0.91//EN" "http://my.netscape.com/publish/formats/rss-0.91.dtd">
            <rss version="0.91">
              <channel>
                <title>A Channel</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Item</title><link>https://example.com/1</link></item>
              </channel>
            </rss>
            """;

        RssFeed feed = Load(Document);

        feed.Channel.Title.ShouldBe("A Channel");
        feed.Channel.Items.Single().Title.ShouldBe("An Item");
    }

    /// <summary>
    /// HTML in a CDATA section survives verbatim, including entities XML does not define.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     38 corpus documents use CDATA, and the corpus contains <b>1,418</b> <c>&amp;nbsp;</c>,
    ///     <b>337</b> <c>&amp;rsquo;</c> and <b>123</b> <c>&amp;aacute;</c> references. XML predefines
    ///     exactly five entities and none of those is among them, so every one of these would be a
    ///     well-formedness error as markup. They parse because they are inside CDATA, where they are not
    ///     markup at all — this excerpt is modelled on <c>rss20/nasa-breaking-news.xml</c>.
    ///     </para>
    ///     <para>
    ///     <b>The assertion is that they arrive undecoded</b>, which is the correct answer and the
    ///     surprising one. A consumer handing this to an HTML renderer gets what the publisher intended;
    ///     a consumer treating it as plain text sees <c>&amp;nbsp;</c> literally. Pinning it means a
    ///     future change that starts HTML-decoding descriptions has to be a decision rather than an
    ///     accident.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void HtmlInsideACdataSection_ArrivesVerbatimIncludingUndefinedEntities()
    {
        const string Document = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <title>A Channel</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item>
                  <title>An Item</title>
                  <link>https://example.com/1</link>
                  <description><![CDATA[<p>Karen Fox&nbsp;/ Alana Johnson<br>NASA&nbsp;HQ &rsquo;26</p>]]></description>
                </item>
              </channel>
            </rss>
            """;

        string description = Load(Document).Channel.Items.Single().Description;

        description.ShouldBe("<p>Karen Fox&nbsp;/ Alana Johnson<br>NASA&nbsp;HQ &rsquo;26</p>");
        description.Contains("&nbsp;", StringComparison.Ordinal)
            .ShouldBeTrue("CDATA content is text, so the entity is not expanded");
        description.StartsWith("<p>", StringComparison.Ordinal)
            .ShouldBeTrue("nor is the markup escaped on the way out");
    }
}