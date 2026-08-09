namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

/// <summary>
/// Covers what the format-agnostic wrapper reports about a document it has just loaded: the format it
/// recognised, the concrete resource underneath, and the title and description lifted onto the wrapper.
/// </summary>
[TestClass]
public class GenericSyndicationFeedTest
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// An RSS document that declares a custom namespace prefix it never uses loads without throwing.
    /// </summary>
    /// <remarks>
    ///     Carries the <c>fix-39</c> test category, which is how the regression it was written for is
    ///     selected from the suite.
    /// </remarks>
    [TestMethod, TestCategory("fix-39")]
    public void TestCustomXmlNamespace()
    {
        string xml = """<rss xmlns:app="http:/example.com" version="2.0"></rss>""";

        Syndication.GenericSyndicationFeed feed = new();

        feed.Load(xml);
        feed.ShouldNotBeSameAs(new Syndication.GenericSyndicationFeed());
    }

    /// <summary>
    /// A loaded RSS 2.0 document reports its format as <c>Rss</c>.
    /// </summary>
    [TestMethod]
    public void Load_MinimalRssFeed_SetsFormatToRss()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.MinimalRss);

        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// A loaded Atom 1.0 document reports its format as <c>Atom</c>.
    /// </summary>
    [TestMethod]
    public void Load_MinimalAtomFeed_SetsFormatToAtom()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.MinimalAtom);

        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
    }

    /// <summary>
    /// An RSS document leaves a real <c>RssFeed</c> behind the wrapper, with its channel intact.
    /// </summary>
    [TestMethod]
    public void Resource_CastToRssFeed_WhenRssFormat()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.MinimalRss);

        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Resource.ShouldNotBeNull();
        feed.Resource.ShouldBeOfType<RssFeed>();

        RssFeed? rssFeed = feed.Resource as RssFeed;
        rssFeed.ShouldNotBeNull();
        rssFeed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// An Atom document leaves a real <c>AtomFeed</c> behind the wrapper, with its title intact.
    /// </summary>
    [TestMethod]
    public void Resource_CastToAtomFeed_WhenAtomFormat()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.MinimalAtom);

        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Resource.ShouldNotBeNull();
        feed.Resource.ShouldBeOfType<AtomFeed>();

        AtomFeed? atomFeed = feed.Resource as AtomFeed;
        atomFeed.ShouldNotBeNull();
        atomFeed.Title!.Content.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The wrapper's title and description are lifted from the RSS channel's own elements.
    /// </summary>
    [TestMethod]
    public void Feed_Title_IsPopulatedFromRss()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.MinimalRss);

        feed.Title.ShouldBe("Test Feed");
        feed.Description.ShouldBe("A test feed");
    }

    /// <summary>
    /// The wrapper's title is lifted from the Atom feed's <c>title</c>, giving the same answer as RSS.
    /// </summary>
    [TestMethod]
    public void Feed_Title_IsPopulatedFromAtom()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.MinimalAtom);

        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The items an RSS document contributes can be filtered by the date they were published: of the two
    /// items in the fixture, only the one dated 20 January 2025 survives a cutoff at the start of that
    /// year, and the one dated 1 January 2024 is what is left behind.
    /// </summary>
    [TestMethod]
    public void Items_FilteredByPublicationDate_SelectsOnlyTheItemPublishedAfterTheCutoff()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.RssWithItems);

        DateTime cutoff = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<GenericSyndicationItem> recent = feed.Items.Where(item => item.PublishedOn >= cutoff).ToList();

        feed.Items.Count.ShouldBe(2);
        recent.Count.ShouldBe(1);
        recent[0].Title.ShouldBe("Recent Item");
        feed.Items.Single(item => item.PublishedOn < cutoff).Title.ShouldBe("Old Item");
    }

    /// <summary>
    /// The items an Atom document contributes can be filtered by the categories they carry: <c>Tech</c>
    /// selects the recent entry, <c>Archive</c> selects the old one, and the feed-level <c>Technology</c>
    /// category stays on the feed rather than reaching either entry.
    /// </summary>
    [TestMethod]
    public void Items_FilteredByCategoryTerm_SelectsOnlyTheEntriesCarryingThatTerm()
    {
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(FeedTestData.AtomWithEntries);

        List<GenericSyndicationItem> tagged = feed.Items
            .Where(item => item.Categories.Any(category => string.Equals(category.Term, "Tech", StringComparison.Ordinal)))
            .ToList();

        feed.Items.Count.ShouldBe(2);
        tagged.Count.ShouldBe(1);
        tagged[0].Title.ShouldBe("Recent Entry");
        feed.Items
            .Single(item => item.Categories.Any(category => string.Equals(category.Term, "Archive", StringComparison.Ordinal)))
            .Title.ShouldBe("Old Entry");
        feed.Categories.Single().Term.ShouldBe("Technology");
        feed.Items.SelectMany(item => item.Categories).ShouldNotContain(category => string.Equals(category.Term, "Technology", StringComparison.Ordinal));
    }
}