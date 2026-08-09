using Argotic.Data.Adapters;
namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="Rss10SyndicationResourceAdapter"/>. RSS 1.0 is RDF Site Summary, an RDF document in
/// <c>http://purl.org/rss/1.0/</c> whose <c>channel</c>, <c>image</c>, <c>textinput</c> and <c>item</c>
/// elements are siblings under <c>rdf:RDF</c> rather than nested; these tests state what is re-parented
/// onto the <see cref="RssFeed"/>'s channel, along with the retrieval limit and the argument guards.
/// </summary>
[TestClass]
public class Rss10SyndicationResourceAdapterTests
{
    #region Fill Channel Tests

    /// <summary>
    /// A minimal RSS 1.0 document fills the channel's title, link and description.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalRss10_PopulatesChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 1.0 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 1.0 feed");
    }

    /// <summary>
    /// The <c>item</c> elements sitting beside <c>channel</c> fill the channel's items in document order,
    /// each with the title, link and description RSS 1.0 defines.
    /// </summary>
    /// <remarks>
    ///     The document also carries the <c>channel/items/rdf:Seq</c> manifest that RSS 1.0 uses to declare a
    ///     channel's membership and ordering. The adapter ignores it and takes every <c>item</c> element in
    ///     document order, which is what the expected order here states.
    /// </remarks>
    [TestMethod]
    public void Fill_WithItems_PopulatesItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

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

    /// <summary>
    /// The sibling <c>image</c> element fills the channel's image with its title, URL and link.
    /// </summary>
    [TestMethod]
    public void Fill_WithImage_PopulatesImage()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.Image.Title.ShouldBe("Feed Image");
        feed.Channel.Image.Url.ShouldBe(new Uri("http://example.com/image.png"));
        feed.Channel.Image.Link.ShouldBe(new Uri("http://example.com"));
    }

    /// <summary>
    /// The sibling <c>textinput</c> element fills the channel's text input with its title, description, name
    /// and link.
    /// </summary>
    [TestMethod]
    public void Fill_WithTextInput_PopulatesTextInput()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

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

    /// <summary>
    /// Filling a <see langword="null"/> feed throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

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
        Should.Throw<ArgumentNullException>(() => new Rss10SyndicationResourceAdapter(null!, settings));
    }

    /// <summary>
    /// Constructing the adapter without load settings throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss10SyndicationResourceAdapter(navigator, null!));
    }

    #endregion

    #region Retrieval Limit Tests

    /// <summary>
    /// A retrieval limit of <c>1</c> keeps only the first item in document order.
    /// </summary>
    [TestMethod]
    public void Fill_WithRetrievalLimit_RespectsLimit()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 1
        };
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(1);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
    }

    #endregion

    #region Channel Properties Tests

    /// <summary>
    /// A document carrying every RSS 1.0 element fills the channel's own title, link and description as
    /// well as its items, image and text input.
    /// </summary>
    [TestMethod]
    public void Fill_ChannelWithAllElements_PopulatesAllProperties()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss10WithItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss10SyndicationResourceAdapter adapter = new(navigator, settings);

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