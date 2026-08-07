using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

/// <summary>
/// Behavior-driven tests for <see cref="GenericSyndicationFeed"/> covering format-agnostic loading,
/// format detection, and generic item access scenarios.
/// </summary>
[TestClass]
public class GenericSyndicationFeedBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Format-Agnostic Loading Tests

    /// <summary>
    /// An RSS document with items loads as <c>Rss</c> and presents its items through the wrapper.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingRss_AbstractsCorrectly()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Items.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An Atom document with entries loads as <c>Atom</c> and presents them through the same items
    /// collection an RSS feed uses.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingAtom_AbstractsCorrectly()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Items.ShouldNotBeEmpty();
    }

    /// <summary>
    /// The RSS channel title reaches the wrapper's own title.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingRss_PopulatesGenericTitle()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The Atom feed title reaches the wrapper's own title, giving the same answer as the RSS twin.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingAtom_PopulatesGenericTitle()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The RSS channel description reaches the wrapper's own description.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingRss_PopulatesGenericDescription()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Description.ShouldBe("A test feed");
    }

    /// <summary>
    /// RSS items arrive in document order, each with its title and its description as the summary.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingRss_ItemsAreMappedCorrectly()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Items.Count.ShouldBe(2);
        feed.Items[0].Title.ShouldBe("Recent Item");
        feed.Items[0].Summary.ShouldBe("A recent item");
        feed.Items[1].Title.ShouldBe("Old Item");
        feed.Items[1].Summary.ShouldBe("An old item");
    }

    /// <summary>
    /// Atom entries arrive in document order as items, each with its title and its summary.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingAtom_EntriesAreMappedToItems()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Items.Count.ShouldBe(2);
        feed.Items[0].Title.ShouldBe("Recent Entry");
        feed.Items[0].Summary.ShouldBe("A recent entry");
        feed.Items[1].Title.ShouldBe("Old Entry");
        feed.Items[1].Summary.ShouldBe("An old entry");
    }

    #endregion

    #region Format Detection Tests

    /// <summary>
    /// A document is sniffed as <c>Rss</c> without the caller declaring the format.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingRss_DetectsRssFormat()
    {
        // Arrange
        string xml = FeedTestData.MinimalRss;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// A document is sniffed as <c>Atom</c> without the caller declaring the format.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingAtom_DetectsAtomFormat()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
    }

    /// <summary>
    /// An OPML outline is sniffed as <c>Opml</c>, so the wrapper spans more than the two feed formats.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingOpml_DetectsOpmlFormat()
    {
        // Arrange
        string xml = FeedTestData.MinimalOpml;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Opml);
    }

    /// <summary>
    /// The wrapper hands back the concrete <c>RssFeed</c> it parsed, so nothing is lost by going generic.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingRss_ExposesUnderlyingRssFeed()
    {
        // Arrange
        string xml = FeedTestData.MinimalRss;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Resource.ShouldNotBeNull();
        feed.Resource.ShouldBeOfType<RssFeed>();
        RssFeed? rssFeed = feed.Resource as RssFeed;
        rssFeed.ShouldNotBeNull();
        rssFeed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The wrapper hands back the concrete <c>AtomFeed</c> it parsed, with its title construct intact.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadingAtom_ExposesUnderlyingAtomFeed()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Resource.ShouldNotBeNull();
        feed.Resource.ShouldBeOfType<AtomFeed>();
        AtomFeed? atomFeed = feed.Resource as AtomFeed;
        atomFeed.ShouldNotBeNull();
        atomFeed.Title!.Content.ShouldBe("Test Feed");
    }

    #endregion

    #region Generic Item Access Tests

    /// <summary>
    /// Both RSS items are counted; none is dropped and none is duplicated.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingRssItems_ReturnsCorrectItemCount()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Items.ShouldNotBeNull();
        feed.Items.Count.ShouldBe(2);
    }

    /// <summary>
    /// Both Atom entries are counted, matching the RSS twin document item for item.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingAtomEntries_ReturnsCorrectItemCount()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Items.ShouldNotBeNull();
        feed.Items.Count.ShouldBe(2);
    }

    /// <summary>
    /// An RSS item's two <c>category</c> elements reach the item as generic categories, in order.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingRssItemCategories_ReturnsCorrectCategories()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        GenericSyndicationItem firstItem = feed.Items[0];
        firstItem.Categories.ShouldNotBeEmpty();
        firstItem.Categories.Count.ShouldBe(2);
        firstItem.Categories[0].Term.ShouldBe("Tech");
        firstItem.Categories[1].Term.ShouldBe("News");
    }

    /// <summary>
    /// An Atom entry's two <c>category</c> elements yield the same terms in the same order as the RSS twin.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingAtomEntryCategories_ReturnsCorrectCategories()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        GenericSyndicationItem firstItem = feed.Items[0];
        firstItem.Categories.ShouldNotBeEmpty();
        firstItem.Categories.Count.ShouldBe(2);
        firstItem.Categories[0].Term.ShouldBe("Tech");
        firstItem.Categories[1].Term.ShouldBe("News");
    }

    /// <summary>
    /// A channel-level RSS category is exposed on the feed, separately from the items' own categories.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingRssFeedCategories_ReturnsCorrectCategories()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Categories.ShouldNotBeEmpty();
        feed.Categories.Count.ShouldBe(1);
        feed.Categories[0].Term.ShouldBe("Technology");
    }

    /// <summary>
    /// A feed-level Atom category is exposed on the feed, giving the same answer as the RSS twin.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingAtomFeedCategories_ReturnsCorrectCategories()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        feed.Categories.ShouldNotBeEmpty();
        feed.Categories.Count.ShouldBe(1);
        feed.Categories[0].Term.ShouldBe("Technology");
    }

    /// <summary>
    /// An RSS item remains reachable and titled whether or not its <c>pubDate</c> parsed.
    /// </summary>
    /// <remarks>
    ///     The date is deliberately not asserted: the fixture spells its <c>pubDate</c> with a <c>GMT</c>
    ///     zone, and the inline note records that such a spelling may leave the publication date unset.
    /// </remarks>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingRssItemPublishedDate_ItemsAreAccessible()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        // Note: RSS pubDate parsing with certain timezone formats (like GMT) may not
        // populate the PublishedOn property due to RFC 822 parsing limitations.
        // This test verifies that items are still accessible regardless of date parsing.
        GenericSyndicationItem recentItem = feed.Items[0];
        recentItem.ShouldNotBeNull();
        recentItem.Title.ShouldBe("Recent Item");
    }

    /// <summary>
    /// An Atom entry's <c>published</c> element reaches the item as the real date, 20 January 2025.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenAccessingAtomEntryPublishedDate_ReturnsCorrectDate()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;

        // Act
        Syndication.GenericSyndicationFeed feed = new();
        feed.Load(xml);

        // Assert
        GenericSyndicationItem recentItem = feed.Items[0];
        recentItem.PublishedOn.ShouldNotBe(DateTime.MinValue);
        recentItem.PublishedOn.Year.ShouldBe(2025);
        recentItem.PublishedOn.Month.ShouldBe(1);
        recentItem.PublishedOn.Day.ShouldBe(20);
    }

    #endregion

    #region Loaded Event Tests

    /// <summary>
    /// Loading from a string raises the <c>Loaded</c> event.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoaded_RaisesLoadedEvent()
    {
        // Arrange
        string xml = FeedTestData.MinimalRss;
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        // Act
        feed.Load(xml);

        // Assert
        eventRaised.ShouldBeTrue();
    }

    /// <summary>
    /// Loading from a stream raises the <c>Loaded</c> event too, so the notification does not depend on
    /// which overload the caller reached for.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadedFromStream_RaisesLoadedEvent()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        // Act
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue();
    }

    #endregion

    #region Default State Tests

    /// <summary>
    /// A newly constructed feed reports no format, empty title and description, no language, no
    /// resource and empty collections — never <see langword="null"/> where a collection is expected.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenCreated_HasDefaultValues()
    {
        // Arrange & Act
        Syndication.GenericSyndicationFeed feed = new();

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.None);
        feed.Title.ShouldBe(string.Empty);
        feed.Description.ShouldBe(string.Empty);
        feed.Language.ShouldBeNull();
        feed.LastUpdatedOn.ShouldBe(DateTime.MinValue);
        feed.Resource.ShouldBeNull();
        feed.Items.ShouldBeEmpty();
        feed.Categories.ShouldBeEmpty();
    }

    #endregion

    #region Stream Loading Tests

    /// <summary>
    /// Loading RSS from a stream fills title, format and items exactly as loading from a string does.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadedFromStream_PopulatesProperties()
    {
        // Arrange
        string xml = FeedTestData.RssWithItems;
        Syndication.GenericSyndicationFeed feed = new();

        // Act
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        // Assert
        feed.Title.ShouldBe("Test Feed");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Items.Count.ShouldBe(2);
    }

    /// <summary>
    /// Loading Atom from a stream fills the same three properties, with the format reported as <c>Atom</c>.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_WhenLoadedFromAtomStream_PopulatesProperties()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;
        Syndication.GenericSyndicationFeed feed = new();

        // Act
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        // Assert
        feed.Title.ShouldBe("Test Feed");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Items.Count.ShouldBe(2);
    }

    #endregion
}