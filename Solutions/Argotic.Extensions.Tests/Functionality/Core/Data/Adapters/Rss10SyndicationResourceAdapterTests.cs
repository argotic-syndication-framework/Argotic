using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Unit tests for <see cref="Rss10SyndicationResourceAdapter"/> that verify RSS 1.0 parsing.
/// </summary>
[TestClass]
public class Rss10SyndicationResourceAdapterTests
{
    #region Fill Channel Tests

    [TestMethod]
    public void Fill_MinimalRss10_PopulatesChannel()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 1.0 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 1.0 feed");
    }

    [TestMethod]
    public void Fill_WithItems_PopulatesItems()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
        feed.Channel.Items[0].Link.ShouldBe(new Uri("http://example.com/item1"));
        feed.Channel.Items[0].Description.ShouldBe("First item description");
        feed.Channel.Items[1].Title.ShouldBe("Second Item");
        feed.Channel.Items[1].Link.ShouldBe(new Uri("http://example.com/item2"));
        feed.Channel.Items[1].Description.ShouldBe("Second item description");
    }

    [TestMethod]
    public void Fill_WithImage_PopulatesImage()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

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
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldNotBeNull();
        feed.Channel.TextInput.Title.ShouldBe("Search");
        feed.Channel.TextInput.Description.ShouldBe("Search the feed");
        feed.Channel.TextInput.Name.ShouldBe("query");
        feed.Channel.TextInput.Link.ShouldBe(new Uri("http://example.com/search"));
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill(null!));
    }

    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss10SyndicationResourceAdapter(null!, settings));
    }

    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10Minimal));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss10SyndicationResourceAdapter(navigator, null!));
    }

    #endregion

    #region Retrieval Limit Tests

    [TestMethod]
    public void Fill_WithRetrievalLimit_RespectsLimit()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
        {
            RetrievalLimit = 1
        };
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(1);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
    }

    #endregion

    #region Channel Properties Tests

    [TestMethod]
    public void Fill_ChannelWithAllElements_PopulatesAllProperties()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        Rss10SyndicationResourceAdapter adapter = new Rss10SyndicationResourceAdapter(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 1.0 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 1.0 feed with items");
        feed.Channel.Items.ShouldNotBeNull();
        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.TextInput.ShouldNotBeNull();
    }

    #endregion
}
