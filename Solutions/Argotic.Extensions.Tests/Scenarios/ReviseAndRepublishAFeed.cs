namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Loading a published feed, changing it, and writing it back out.
/// </summary>
/// <remarks>
///     <para>
///     This is the single most common thing a consumer of a syndication library does, and before this
///     the suite contained no test that did it. Scanning every test body for a load followed by a
///     mutation followed by a save returned exactly one hit, and it was a false positive - a
///     <c>stream.Position = 0</c>.
///     </para>
///     <para>
///     Each test starts from a real document rather than an object graph built in code, because the
///     interesting failures live in what the parser produced and the serialiser then has to preserve.
///     </para>
/// </remarks>
[TestClass]
public class ReviseAndRepublishAFeed
{
    /// <summary>
    /// An item added to a loaded feed survives republication, and the existing items are kept.
    /// </summary>
    [TestMethod]
    public void AnItemAddedToALoadedFeed_SurvivesRepublication()
    {
        RssFeed feed = LoadRealRssFeed();
        int originalCount = feed.Channel.Items.Count;
        originalCount.ShouldBeGreaterThan(0);

        feed.Channel.Items.Add(new RssItem
        {
            Title = "A newly added item",
            Description = "Added after loading",
            Link = new Uri("http://example.com/added"),
            PublicationDate = new DateTime(2024, 6, 1, 9, 0, 0, DateTimeKind.Utc),
        });

        RssFeed republished = SaveAndReload(feed);

        republished.Channel.Items.Count.ShouldBe(originalCount + 1);
        republished.Channel.Items.Select(i => i.Title).ShouldContain("A newly added item");
    }

    /// <summary>
    /// A changed channel title survives republication.
    /// </summary>
    [TestMethod]
    public void AChangedChannelTitle_SurvivesRepublication()
    {
        RssFeed feed = LoadRealRssFeed();
        feed.Channel.Title.ShouldNotBe("A revised title");

        feed.Channel.Title = "A revised title";

        SaveAndReload(feed).Channel.Title.ShouldBe("A revised title");
    }

    /// <summary>
    /// An item removed from a loaded feed stays removed.
    /// </summary>
    [TestMethod]
    public void AnItemRemovedFromALoadedFeed_StaysRemoved()
    {
        RssFeed feed = LoadRealRssFeed();
        int originalCount = feed.Channel.Items.Count;
        originalCount.ShouldBeGreaterThan(1);

        string removedTitle = feed.Channel.Items[0].Title;
        feed.Channel.Items.RemoveAt(0);

        RssFeed republished = SaveAndReload(feed);

        republished.Channel.Items.Count.ShouldBe(originalCount - 1);
        republished.Channel.Items.Select(i => i.Title).ShouldNotContain(removedTitle);
    }

    /// <summary>
    /// An extension attached after loading survives republication.
    /// </summary>
    /// <remarks>
    ///     The load path fills extensions from the document; this asserts the save path emits one the
    ///     caller attached afterwards, which is a different direction through
    ///     <c>SyndicationExtensionAdapter</c>.
    /// </remarks>
    [TestMethod]
    public void AnExtensionAttachedAfterLoading_SurvivesRepublication()
    {
        RssFeed feed = LoadRealRssFeed();
        RssItem item = feed.Channel.Items[0];

        SiteSummarySlashSyndicationExtension slash = new();
        slash.Context.Section = "articles";
        slash.Context.Comments = 42;
        item.Extensions.Add(slash);

        RssItem republished = SaveAndReload(feed).Channel.Items[0];

        SiteSummarySlashSyndicationExtension read =
            republished.Extensions.OfType<SiteSummarySlashSyndicationExtension>().ShouldHaveSingleItem();
        read.Context.Section.ShouldBe("articles");
        read.Context.Comments.ShouldBe(42);
    }

    /// <summary>
    /// Extension data already in the document survives a load and republish unchanged.
    /// </summary>
    [TestMethod]
    public void ExtensionDataAlreadyInTheDocument_SurvivesRepublication()
    {
        RssFeed feed = new();
        using (FileStream stream = SampleFeeds.Open(SampleFeeds.RssFeedWithExtensions))
        {
            feed.Load(stream);
        }

        RssItem original = feed.Channel.Items[0];
        original.HasExtensions.ShouldBeTrue("the sample is meant to carry extensions");
        int originalExtensionCount = original.Extensions.Count;

        // The expectation used to be originalExtensionCount alone, which is read from the same loader
        // under test: a load that recovered one extension of many, followed by a save that preserved
        // that one, passed. The literal pins what the sample actually carries.
        originalExtensionCount.ShouldBe(14, "the sample item declares fourteen recognised extensions");

        RssItem republished = SaveAndReload(feed).Channel.Items[0];

        republished.Extensions.Count.ShouldBe(
            originalExtensionCount,
            "republishing lost extension data that the load had recovered");

        // A count alone cannot tell a preserved extension from an emptied one, so one value is followed
        // through the republication as well.
        republished.Extensions.OfType<SiteSummarySlashSyndicationExtension>().Single().Context.Comments
            .ShouldBe(original.Extensions.OfType<SiteSummarySlashSyndicationExtension>().Single().Context.Comments);
    }

    /// <summary>
    /// An entry added to a loaded Atom feed survives republication.
    /// </summary>
    [TestMethod]
    public void AnEntryAddedToALoadedAtomFeed_SurvivesRepublication()
    {
        AtomFeed feed = new();
        using (FileStream stream = SampleFeeds.Open(SampleFeeds.AtomFeed))
        {
            feed.Load(stream);
        }

        int originalCount = feed.Entries.Count;
        originalCount.ShouldBeGreaterThan(0);

        feed.Entries.Add(new AtomEntry(
            new AtomId(new Uri("urn:example:added")),
            new AtomTextConstruct("A newly added entry"),
            new DateTime(2024, 6, 1, 9, 0, 0, DateTimeKind.Utc)));

        using MemoryStream saved = new();
        feed.Save(saved);
        saved.Seek(0, SeekOrigin.Begin);

        AtomFeed republished = new();
        republished.Load(saved);

        republished.Entries.Count.ShouldBe(originalCount + 1);
        republished.Entries.Select(e => e.Title?.Content).ShouldContain("A newly added entry");
    }

    private static RssFeed LoadRealRssFeed()
    {
        RssFeed feed = new();
        using FileStream stream = SampleFeeds.Open(SampleFeeds.RssFeed);
        feed.Load(stream);
        return feed;
    }

    private static RssFeed SaveAndReload(RssFeed feed)
    {
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        RssFeed read = new();
        read.Load(stream);
        return read;
    }
}