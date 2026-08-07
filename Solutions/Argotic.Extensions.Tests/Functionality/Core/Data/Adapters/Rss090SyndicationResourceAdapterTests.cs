using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="Rss090SyndicationResourceAdapter"/>. RSS 0.90 is an RDF document in
/// <c>http://my.netscape.com/rdf/simple/0.9/</c> whose <c>channel</c>, <c>image</c>, <c>textinput</c> and
/// <c>item</c> elements are siblings under <c>rdf:RDF</c> rather than nested; these tests state which of
/// them are re-parented onto the <see cref="RssFeed"/>'s channel, what a minimal document leaves unset,
/// and how the retrieval limit behaves.
/// </summary>
[TestClass]
public class Rss090SyndicationResourceAdapterTests
{
    /// <summary>
    /// A minimal RSS 0.90 document fills the channel's title, link and description.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalRss090_PopulatesChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 0.90 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 0.90 feed");
    }

    /// <summary>
    /// The <c>item</c> elements sitting beside <c>channel</c> under <c>rdf:RDF</c> fill the channel's items
    /// in document order, with a title and a link each — the only two elements 0.90 defines on an item.
    /// </summary>
    [TestMethod]
    public void Fill_WithItems_PopulatesItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
        feed.Channel.Items[0].Link.ShouldBe(new Uri("http://example.com/item1"));
        feed.Channel.Items[1].Title.ShouldBe("Second Item");
        feed.Channel.Items[1].Link.ShouldBe(new Uri("http://example.com/item2"));
    }

    /// <summary>
    /// The sibling <c>image</c> element fills the channel's image with its title, URL and link.
    /// </summary>
    [TestMethod]
    public void Fill_WithImage_PopulatesImage()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.Image.Title.ShouldBe("Feed Image");
        feed.Channel.Image.Url.ShouldBe(new Uri("http://example.com/image.png"));
        feed.Channel.Image.Link.ShouldBe(new Uri("http://example.com"));
    }

    /// <summary>
    /// The sibling <c>textinput</c> element — all lower case, unlike the <c>textInput</c> of RSS 0.91 and
    /// 0.92 — fills the channel's text input with its title, description, name and link.
    /// </summary>
    [TestMethod]
    public void Fill_WithTextInput_PopulatesTextInput()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldNotBeNull();
        feed.Channel.TextInput.Title.ShouldBe("Search");
        feed.Channel.TextInput.Description.ShouldBe("Search the feed");
        feed.Channel.TextInput.Name.ShouldBe("query");
        feed.Channel.TextInput.Link.ShouldBe(new Uri("http://example.com/search"));
    }

    /// <summary>
    /// A retrieval limit of <c>1</c> keeps only the first item.
    /// </summary>
    [TestMethod]
    public void Fill_WithRetrievalLimit_EnforcesLimit()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 1
        };
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(1);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
    }

    /// <summary>
    /// Filling a <see langword="null"/> feed throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill(null!));
    }

    /// <summary>
    /// Constructing the adapter without a navigator throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss090SyndicationResourceAdapter(null!, settings));
    }

    /// <summary>
    /// Constructing the adapter without load settings throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss090SyndicationResourceAdapter(navigator, null!));
    }

    /// <summary>
    /// A document with no <c>image</c> leaves the channel's image <see langword="null"/>, not an empty
    /// instance.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesImageNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldBeNull();
    }

    /// <summary>
    /// A document with no <c>textinput</c> leaves the channel's text input <see langword="null"/>, not an
    /// empty instance.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesTextInputNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldBeNull();
    }

    /// <summary>
    /// A document with no <c>item</c> leaves the channel's item collection empty.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesItemsEmpty()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.ShouldBeEmpty();
    }

    /// <summary>
    /// A retrieval limit of <c>0</c> means no limit, and both items are read.
    /// </summary>
    [TestMethod]
    public void Fill_WithZeroRetrievalLimit_RetrievesAllItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss090WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 0  // 0 means no limit
        };
        Rss090SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
    }
}