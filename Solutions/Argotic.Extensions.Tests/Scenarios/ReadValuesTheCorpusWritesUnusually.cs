using System.Text;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Pins values that real publishers write in ways the specifications permit and nobody would invent.
/// </summary>
/// <remarks>
///     <para>
///     The third tranche distilled from the real-world corpus. Nothing here was broken when it was
///     written — these are characterisations, and each exists because the shape occurs in real documents
///     in quantity and had no test at all, so a change to it would have been silent.
///     </para>
///     <para>
///     Each census below is counted over the corpus of 136 live documents.
///     </para>
/// </remarks>
[TestClass]
public sealed class ReadValuesTheCorpusWritesUnusually
{
    private static RssItem LoadItem(string itemContent)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>A Channel</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Item</title>{itemContent}</item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Items.Single();
    }

    /// <summary>
    /// A sitemap priority keeps the precision and the scale the publisher wrote.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The corpus holds 19 distinct priorities, and they are not the tidy one-decimal values the
    ///     sitemap protocol's examples use: <c>0.05437499999999999</c> and <c>0.21749999999999997</c> are
    ///     real, and are what a generator computing a priority in binary floating point and printing it
    ///     round-trippably produces.
    ///     </para>
    ///     <para>
    ///     <b><c>0.5000</c> is the row that says which numeric type this is.</b> A <see cref="double"/>
    ///     would render it back as <c>0.5</c>; the value keeps its trailing zeros, so the property is a
    ///     <see cref="decimal"/> and preserves scale. That matters for a load-modify-save cycle, where
    ///     silently renormalising every priority in a 50,000-URL sitemap would be a large and invisible
    ///     diff.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("0.05437499999999999")]
    [DataRow("0.21749999999999997")]
    [DataRow("0.5000")]
    [DataRow("0.82")]
    public void ASitemapPriority_KeepsThePrecisionItWasWrittenWith(string priority)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url><loc>https://example.com/a</loc><priority>{priority}</priority></url>
            </urlset>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        Sitemap sitemap = new();
        sitemap.Load(stream);

        SitemapUrl url = sitemap.Urls.Single();

        url.Priority.ShouldNotBeNull(priority);
        url.Priority.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            .ShouldBe(priority, "the value must survive unrenormalised");
    }

    /// <summary>
    /// An empty description and an absent one are indistinguishable.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>Characterisation, not endorsement.</b> 168 corpus items carry a self-closing
    ///     <c>&lt;description/&gt;</c>. The property is a <see cref="string"/> rather than a nullable
    ///     one, so an element that was present but empty and an element that was never there both
    ///     arrive as the empty string, and a consumer cannot tell "the publisher said nothing" from
    ///     "the publisher said it is empty".
    ///     </para>
    ///     <para>
    ///     Pinned rather than changed because making it nullable is a public API decision of the same
    ///     kind as <c>RssEnclosure.Length</c>, and unlike that one it has no defect behind it — nothing
    ///     throws and nothing is lost that RSS itself distinguishes. Recorded so a future change is
    ///     deliberate.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("<description/>", "self-closing, as 168 corpus items write it")]
    [DataRow("<description></description>", "an empty pair")]
    [DataRow("", "absent entirely")]
    public void AnEmptyDescriptionAndAnAbsentOne_AreIndistinguishable(string description, string shape)
    {
        LoadItem(description).Description.ShouldBe(string.Empty, shape);
    }

    /// <summary>
    /// A guid is a permanent link unless the publisher says otherwise.
    /// </summary>
    /// <remarks>
    ///     RSS 2.0 defines <c>isPermaLink</c> as defaulting to <see langword="true"/>, and the corpus
    ///     shows how much that default matters in the other direction: <b>5,700</b> guids say
    ///     <c>isPermaLink="false"</c> against <b>275</b> that say <c>true</c>. Treating an opaque
    ///     identifier such as <c>urn:uuid:…</c> as a URL is the exact mistake the attribute exists to
    ///     prevent, so the default and the override both need pinning.
    /// </remarks>
    [TestMethod]
    [DataRow("""<guid isPermaLink="false">urn:uuid:60a76c80-d399-11d9-b91C</guid>""", false, "the 5,700 case")]
    [DataRow("""<guid isPermaLink="true">https://example.com/1</guid>""", true, "the 275 case")]
    [DataRow("<guid>https://example.com/1</guid>", true, "absent, which RSS 2.0 defines as true")]
    public void AGuid_IsAPermanentLinkUnlessTheFeedSaysOtherwise(string identifier, bool expected, string shape)
    {
        RssItem item = LoadItem(identifier);

        item.Guid.ShouldNotBeNull(shape);
        item.Guid.IsPermanentLink.ShouldBe(expected, shape);
    }

    /// <summary>
    /// Numeric character references are decoded into the characters they name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The corpus contains <b>36,051</b> numeric character references. Publishers reach for them
    ///     rather than literal bytes for exactly the punctuation that is easiest to corrupt — curly
    ///     quotes, dashes and non-breaking spaces — so this is the same risk as the non-ASCII titles
    ///     covered in <c>LoadDocumentsAsPublishersWriteThem</c>, arriving by a different route.
    ///     </para>
    ///     <para>
    ///     Both spellings are asserted because they are handled by different branches of the XML reader:
    ///     <c>&amp;#8217;</c> is decimal and <c>&amp;#x2013;</c> is hexadecimal.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void NumericCharacterReferences_AreDecoded()
    {
        RssItem item = LoadItem("<description>It&#8217;s a test &#x2013; really&#160;so</description>");

        item.Description.ShouldBe("It’s a test – really so");
        item.Description.Contains('&', StringComparison.Ordinal)
            .ShouldBeFalse("a decoded reference leaves no ampersand behind");
    }

    /// <summary>
    /// The paging and discovery link relations real feeds use are all read.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Beyond <c>alternate</c> (277 in the corpus) and <c>self</c> (66), real feeds carry
    ///     <c>edit</c> from Atom Publishing, <c>next</c>, <c>first</c>, <c>last</c> and
    ///     <c>prev-archive</c> from RFC 5005 feed paging, and <c>hub</c> from WebSub. Argotic does not
    ///     model these as anything special — they are ordinary links with a relation string — and that
    ///     is worth pinning, because it means a consumer implementing paging gets what it needs without
    ///     the library having to know what paging is.
    ///     </para>
    ///     <para>
    ///     Asserted as a set rather than one at a time, so that a link silently dropped anywhere in the
    ///     list fails rather than shifting the indices of a positional assertion.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ThePagingAndDiscoveryRelationsRealFeedsUse_AreAllRead()
    {
        const string Document = """
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>A Feed</title>
              <id>urn:uuid:60a76c80</id>
              <updated>2026-01-01T00:00:00Z</updated>
              <link rel="self" href="https://example.com/feed"/>
              <link rel="alternate" href="https://example.com/"/>
              <link rel="next" href="https://example.com/feed?page=2"/>
              <link rel="first" href="https://example.com/feed?page=1"/>
              <link rel="last" href="https://example.com/feed?page=9"/>
              <link rel="prev-archive" href="https://example.com/feed?page=0"/>
              <link rel="hub" href="https://hub.example.com/"/>
            </feed>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Document), writable: false);
        AtomFeed feed = new();
        feed.Load(stream);

        feed.Links.Select(link => link.Relation)
            .ShouldBe(["self", "alternate", "next", "first", "last", "prev-archive", "hub"], ignoreOrder: true);

        feed.Links.Single(link => link.Relation == "prev-archive").Uri
            .ShouldBe(new Uri("https://example.com/feed?page=0"), "a relation the library does not model still carries its target");
    }
}