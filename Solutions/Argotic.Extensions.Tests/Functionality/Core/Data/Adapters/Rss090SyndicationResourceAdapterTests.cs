using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Unit tests for <see cref="Rss090SyndicationResourceAdapter"/>.
/// </summary>
[TestClass]
public class Rss090SyndicationResourceAdapterTests
{
    [TestMethod]
    public void Fill_MinimalRss090_PopulatesChannel()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 0.90 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 0.90 feed");
    }

    [TestMethod]
    public void Fill_WithItems_PopulatesItems()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
        feed.Channel.Items[0].Link.ShouldBe(new Uri("http://example.com/item1"));
        feed.Channel.Items[1].Title.ShouldBe("Second Item");
        feed.Channel.Items[1].Link.ShouldBe(new Uri("http://example.com/item2"));
    }

    [TestMethod]
    public void Fill_WithImage_PopulatesImage()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.Image.Title.ShouldBe("Feed Image");
        feed.Channel.Image.Url.ShouldBe(new Uri("http://example.com/image.png"));
        feed.Channel.Image.Link.ShouldBe(new Uri("http://example.com"));
    }

    [TestMethod]
    public void Fill_WithTextInput_PopulatesTextInput()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldNotBeNull();
        feed.Channel.TextInput.Title.ShouldBe("Search");
        feed.Channel.TextInput.Description.ShouldBe("Search the feed");
        feed.Channel.TextInput.Name.ShouldBe("query");
        feed.Channel.TextInput.Link.ShouldBe(new Uri("http://example.com/search"));
    }

    [TestMethod]
    public void Fill_WithRetrievalLimit_EnforcesLimit()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
        {
            RetrievalLimit = 1
        };
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(1);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
    }

    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill(null!));
    }

    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss090SyndicationResourceAdapter(null!, settings));
    }

    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss090SyndicationResourceAdapter(navigator, null!));
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesImageNull()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesTextInputNull()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesItemsEmpty()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.ShouldBeEmpty();
    }

    [TestMethod]
    public void Fill_WithZeroRetrievalLimit_RetrievesAllItems()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
        {
            RetrievalLimit = 0  // 0 means no limit
        };
        Rss090SyndicationResourceAdapter adapter = new Rss090SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
    }
}
