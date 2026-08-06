using System.Text;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Covers loading a feed whose author has an empty name.
/// </summary>
/// <remarks>
///     <para>
///     Found by crawling the Azure Weekly subscription list: three of its 478 production feeds — all
///     Jekyll sites with no author configured — emit <c>&lt;author&gt;&lt;name&gt;&lt;/name&gt;&lt;/author&gt;</c>,
///     and Argotic could not load them at all. <c>AtomPersonConstruct.Load</c> assigned
///     <c>nameNavigator.Value</c> into a setter that rejects empty strings, so a well-formed document
///     failed with <see cref="ArgumentException"/> — an exception type that is not part of
///     <c>Load</c>'s documented contract, thrown about an argument the caller never passed.
///     </para>
///     <para>
///     The setter stays strict: an author <i>writing</i> a feed should be told an empty name is not a
///     name. The read path is where tolerance belongs, and <c>AtomCategory.Load</c> and
///     <c>AtomGenerator.Load</c> already guard the same way — the person construct was the one read
///     path that did not.
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
    /// A feed whose author has an empty name loads, and the rest of the entry survives.
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

        // The empty author is dropped, not attached as an empty husk: its Load reports that nothing
        // was loaded, and the adapter already declines to add a person it learned nothing about.
        entry.Authors.ShouldBeEmpty();
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
    /// A real author name still round-trips through the read path.
    /// </summary>
    /// <remarks>
    ///     The control: without it, the two rows above are equally consistent with "author names are
    ///     no longer read at all".
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