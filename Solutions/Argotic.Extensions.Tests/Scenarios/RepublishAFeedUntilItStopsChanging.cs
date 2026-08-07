using System.Text;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Writing a loaded document back out, loading that, and writing it again — and demanding the two
/// outputs be identical.
/// </summary>
/// <remarks>
///     <para>
///     The contract is <c>save(load(x)) == save(load(save(load(x))))</c>. Once a document is inside
///     the model, a further trip through it has to be idempotent; anything else is data being invented
///     or lost. It needs no oracle, which is what makes it checkable against a document nobody wrote a
///     fixture for.
///     </para>
///     <para>
///     Every test here cycles <b>twice</b> and asserts a fixed point, because a compounding defect
///     cannot be told apart from correct behaviour by one clean cycle: a version that doubled half as
///     fast would pass a single-cycle assertion just as well. That is the lesson
///     <c>.endjin/build-warnings.md</c> §2.31 records against the <c>type="html"</c> double-escape, and
///     §2.32 against the whitespace-only category.
///     </para>
/// </remarks>
[TestClass]
public class RepublishAFeedUntilItStopsChanging
{
    /// <summary>
    /// An Atom feed carrying RFC 5005 paging links, one control link whose relation is not an RFC 5005
    /// one, and the <c>fh:complete</c> flag.
    /// </summary>
    private const string PagedAtomFeed = """
        <?xml version="1.0" encoding="utf-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:fh="http://purl.org/syndication/history/1.0">
          <title type="text">A paged feed</title>
          <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>
          <updated>2024-01-01T12:00:00Z</updated>
          <fh:complete />
          <link href="https://example.com/" rel="alternate" type="text/html" />
          <link href="https://example.com/feed.atom?page=2" rel="next" type="application/atom+xml" />
          <link href="https://example.com/feed.atom?page=0" rel="first" type="application/atom+xml" />
        </feed>
        """;

    /// <summary>
    /// An RSS 2.0 channel carrying the same paging information the way RFC 5005 Appendix B says an RSS
    /// feed must carry it: borrowed <c>atom:link</c> elements, because RSS has no element of its own
    /// that can express a relation.
    /// </summary>
    private const string PagedRssFeed = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:fh="http://purl.org/syndication/history/1.0">
          <channel>
            <title>A paged channel</title>
            <link>https://example.com/</link>
            <description>Paged across several documents</description>
            <fh:archive />
            <atom:link href="https://example.com/feed.rss?page=1" rel="prev-archive" type="application/rss+xml" />
            <atom:link href="https://example.com/feed.rss" rel="current" type="application/rss+xml" />
            <item>
              <title>An item</title>
              <description>Body</description>
            </item>
          </channel>
        </rss>
        """;

    /// <summary>
    /// An Atom feed whose first entry is explicitly not a draft and whose second explicitly is.
    /// </summary>
    private const string AtomFeedWithPublishingControls = """
        <?xml version="1.0" encoding="utf-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
          <title type="text">A feed with publishing controls</title>
          <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>
          <updated>2024-01-01T12:00:00Z</updated>
          <entry>
            <title type="text">A published entry</title>
            <id>urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6b</id>
            <updated>2024-01-01T10:00:00Z</updated>
            <app:control><app:draft>no</app:draft></app:control>
          </entry>
          <entry>
            <title type="text">A draft entry</title>
            <id>urn:uuid:1225c695-cfb8-4ebb-bbbb-80da344efa6b</id>
            <updated>2024-01-01T11:00:00Z</updated>
            <app:control><app:draft>yes</app:draft></app:control>
          </entry>
        </feed>
        """;

    /// <summary>
    /// An Atom feed carrying RFC 5005 paging links settles, with one link per relation and the
    /// attributes the publisher wrote still on it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Before the fix this doubled: <c>3 → 5 → 9 → 17</c>. Both <c>AtomFeed.Links</c> and the Feed
    ///     History extension parsed the same <c>atom:link</c> elements, and both wrote them back.
    ///     </para>
    ///     <para>
    ///     The <c>type</c> assertion is the fidelity half. <c>FeedHistoryLinkRelation</c> models only
    ///     <c>href</c> and <c>rel</c>, so the copies it emitted had already lost <c>type</c>,
    ///     <c>title</c>, <c>hreflang</c> and <c>length</c> — which is why the core model, not the
    ///     extension, is the layer that has to own an <c>atom:link</c> in an Atom document.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnAtomFeedCarryingRfc5005PagingLinks_SettlesWithOneLinkPerRelation()
    {
        string once = RepublishAtom(PagedAtomFeed);
        string twice = RepublishAtom(once);

        twice.ShouldBe(once);
        CountOf(once, "<link").ShouldBe(3);
        CountOf(once, @"rel=""next""").ShouldBe(1);
        CountOf(once, @"rel=""first""").ShouldBe(1);
        CountOf(once, @"rel=""alternate""").ShouldBe(1);
        CountOf(once, @"type=""application/atom+xml""").ShouldBe(2);
    }

    /// <summary>
    /// An RSS channel carrying RFC 5005 paging links keeps publishing them, and settles.
    /// </summary>
    /// <remarks>
    ///     This is the guard against the wrong fix rather than a characterisation of a defect: RSS has
    ///     no core home for a relation-bearing link, so the extension is the only writer there and must
    ///     stay one. It passes before and after.
    /// </remarks>
    [TestMethod]
    public void AnRssChannelCarryingRfc5005PagingLinks_KeepsPublishingThemAndSettles()
    {
        string once = RepublishRss(PagedRssFeed);
        string twice = RepublishRss(once);

        twice.ShouldBe(once);
        CountOf(once, @"rel=""prev-archive""").ShouldBe(1);
        CountOf(once, @"rel=""current""").ShouldBe(1);
        once.ShouldContain("<fh:archive", Case.Sensitive);
    }

    /// <summary>
    /// An entry that is not a draft is published without an <c>app:control</c> element at all, and the
    /// draft entry keeps its own — so the document settles on the first republication.
    /// </summary>
    /// <remarks>
    ///     Before the fix, <c>app:control</c> was written unconditionally while <c>app:draft</c> was
    ///     written only for a draft, so a published entry got an empty <c>&lt;app:control /&gt;</c> that
    ///     then failed to load back: two elements at the first save, one at the second. RFC 5023 §13.1
    ///     is what makes omission the correct fix rather than making the empty element load — "if the
    ///     app:draft element is not present, then servers that support the extension MUST behave as
    ///     though an app:draft element containing 'no' was sent", so an <c>app:control</c> carrying
    ///     nothing else says exactly what its absence says.
    /// </remarks>
    [TestMethod]
    public void AnAtomEntryThatIsNotADraft_IsPublishedWithNoControlElementAtAll()
    {
        string once = RepublishAtom(AtomFeedWithPublishingControls);
        string twice = RepublishAtom(once);

        twice.ShouldBe(once);
        CountOf(once, "<app:control").ShouldBe(1);
        once.ShouldContain("<app:draft>yes</app:draft>", Case.Sensitive);
        once.ShouldNotContain("<app:draft>no</app:draft>", Case.Sensitive);
    }

    private static string RepublishAtom(string document)
    {
        AtomFeed feed = new();
        using (MemoryStream input = new(Encoding.UTF8.GetBytes(document)))
        {
            feed.Load(input);
        }

        using MemoryStream output = new();
        feed.Save(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static string RepublishRss(string document)
    {
        RssFeed feed = new();
        using (MemoryStream input = new(Encoding.UTF8.GetBytes(document)))
        {
            feed.Load(input);
        }

        using MemoryStream output = new();
        feed.Save(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static int CountOf(string document, string token)
    {
        int count = 0;
        int index = document.IndexOf(token, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = document.IndexOf(token, index + token.Length, StringComparison.Ordinal);
        }

        return count;
    }
}