using System.Text;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Covers loading a feed whose author has an empty name, validated against RFC 4287.
/// </summary>
/// <remarks>
///     <para>
///     Found by crawling the Azure Weekly subscription list: three of its 478 production feeds — all
///     Jekyll sites with no author configured — emit <c>&lt;author&gt;&lt;name&gt;&lt;/name&gt;&lt;/author&gt;</c>,
///     and Argotic could not load them at all. That document is <b>conformant</b>: RFC 4287 §3.2's
///     grammar is <c>element atom:name { text }</c>, and RELAX NG <c>text</c> admits the empty string.
///     <c>AtomPersonConstruct.Load</c> assigned the value into a setter that rejects empty strings, so
///     a valid document failed with <see cref="ArgumentException"/> — an exception type outside
///     <c>Load</c>'s documented contract, about a parameter the caller never passed.
///     </para>
///     <para>
///     The author is <b>kept</b>, not dropped. nietras' real feed has no feed-level author, so it
///     satisfies §4.1.1 — "atom:feed elements MUST contain one or more atom:author elements, unless
///     all of the atom:feed element's child atom:entry elements contain at least one" — through
///     precisely those degenerate elements. A reader that dropped them would round-trip a conformant
///     document into one violating two MUSTs. The first cut of this fix did exactly that, and
///     validating it against the spec is what caught it.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomPersonConstructToleranceTests
{
    private const string FeedWithEmptyAuthorName = """
        <?xml version="1.0" encoding="utf-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <id>https://tolerance.invalid/feed.xml</id>
            <title>Unattributed</title>
            <updated>2026-01-01T00:00:00Z</updated>
            <entry>
                <id>https://tolerance.invalid/1</id>
                <title>Post</title>
                <updated>2026-01-01T00:00:00Z</updated>
                <author><name></name></author>
                <summary>s</summary>
            </entry>
        </feed>
        """;

    /// <summary>
    /// A feed whose author has an empty name loads, and the author survives.
    /// </summary>
    [TestMethod]
    public void AnEmptyAuthorName_DoesNotPreventTheFeedLoading()
    {
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedWithEmptyAuthorName));

        feed.Load(stream);

        AtomEntry entry = feed.Entries.ShouldHaveSingleItem();
        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Post");

        // Kept, with the name at its default. The element is the entry's one required person-construct
        // child and the document's way of satisfying §4.1.1 - dropping it would be data loss dressed
        // as tidiness.
        AtomPersonConstruct author = entry.Authors.ShouldHaveSingleItem();
        author.Name.ShouldBe(string.Empty);
    }

    /// <summary>
    /// The degenerate author survives a round trip, so a conformant document stays conformant.
    /// </summary>
    /// <remarks>
    ///     The row that caught the first cut of this fix, which dropped the author on load. With no
    ///     feed-level author, the entry-level elements are what satisfy the §4.1.1/§4.1.2 MUSTs —
    ///     load-then-save must not strip them.
    /// </remarks>
    [TestMethod]
    public void TheEmptyAuthorSurvivesARoundTrip()
    {
        AtomFeed feed = new();
        using (MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedWithEmptyAuthorName)))
        {
            feed.Load(stream);
        }

        using MemoryStream saved = new();
        feed.Save(saved);
        string document = Encoding.UTF8.GetString(saved.ToArray());

        document.ShouldContain("<author>");
        document.ShouldContain("<name />");
    }

    /// <summary>
    /// The same feed loads through the generic wrapper, which is where the crawl hit it.
    /// </summary>
    [TestMethod]
    public void AnEmptyAuthorName_DoesNotPreventTheGenericLoad()
    {
        Argotic.Syndication.GenericSyndicationFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedWithEmptyAuthorName));

        feed.Load(stream);

        feed.Items.ShouldHaveSingleItem().Title.ShouldBe("Post");
    }

    /// <summary>
    /// An author with a uri but no name element at all still loads.
    /// </summary>
    /// <remarks>
    ///     The inverse case, pinned to document the asymmetry: <i>this</i> shape is the invalid one —
    ///     §3.2.1 says a person construct "MUST contain exactly one atom:name element" — and Argotic
    ///     reads it tolerantly anyway. Refusing valid input while accepting invalid input is the exact
    ///     inversion the original defect amounted to.
    /// </remarks>
    [TestMethod]
    public void AnAuthorWithOnlyAUri_IsStillToleratedThoughInvalid()
    {
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(
            FeedWithEmptyAuthorName.Replace(
                "<author><name></name></author>",
                "<author><uri>https://tolerance.invalid/who</uri></author>",
                StringComparison.Ordinal)));

        feed.Load(stream);

        AtomPersonConstruct author = feed.Entries.ShouldHaveSingleItem().Authors.ShouldHaveSingleItem();
        author.Uri.ShouldBe(new Uri("https://tolerance.invalid/who"));
        author.Name.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A real author name still round-trips through the read path.
    /// </summary>
    /// <remarks>
    ///     The control: without it, the rows above are equally consistent with "author names are no
    ///     longer read at all".
    /// </remarks>
    [TestMethod]
    public void ARealAuthorName_IsStillRead()
    {
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(
            FeedWithEmptyAuthorName.Replace("<name></name>", "<name>nietras</name>", StringComparison.Ordinal)));

        feed.Load(stream);

        feed.Entries.ShouldHaveSingleItem().Authors.ShouldHaveSingleItem().Name.ShouldBe("nietras");
    }
}