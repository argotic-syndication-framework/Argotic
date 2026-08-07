using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

/// <summary>
/// Covers <see cref="RssFeed"/> end to end: building a channel, parsing RSS 2.0,
/// extension auto-detection, the asynchronous loads, and what survives a save and reload.
/// </summary>
[TestClass]
public class RssFeedBehaviorTests
{
    public TestContext TestContext { get; set; }

    #region Feed Creation Tests

    /// <summary>
    /// A feed built through the object model saves as an <c>rss</c> root element declaring <c>version="2.0"</c>.
    /// </summary>
    [TestMethod]
    public void RssFeed_WhenCreatedProgrammatically_ProducesValidXml()
    {
        // Arrange
        RssFeed feed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        // Act
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Position = 0;

        // Assert
        XDocument xml = XDocument.Load(stream);
        xml.Root.ShouldNotBeNull();
        xml.Root.Name.LocalName.ShouldBe("rss");
        xml.Root.Attribute("version")?.Value.ShouldBe("2.0");
    }

    /// <summary>
    /// The link and title given to the constructor land on the channel.
    /// </summary>
    [TestMethod]
    public void RssFeed_WhenCreatedWithLinkAndTitle_SetsChannelProperties()
    {
        // Arrange & Act
        RssFeed feed = new(new Uri("http://example.com"), "Test Feed");

        // Assert
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The single-argument constructor takes a channel description.
    /// </summary>
    [TestMethod]
    public void RssFeed_WhenCreatedWithDescription_SetsChannelDescription()
    {
        // Arrange & Act
        RssFeed feed = new("Test Description");

        // Assert
        feed.Channel.Description.ShouldBe("Test Description");
    }

    /// <summary>
    /// A default-constructed feed has a channel whose title and
    /// description are empty strings, not null, and no items.
    /// </summary>
    [TestMethod]
    public void RssFeed_WhenCreatedWithDefaultConstructor_HasEmptyChannel()
    {
        // Arrange & Act
        RssFeed feed = new();

        // Assert
        feed.Channel.ShouldNotBeNull();
        feed.Channel.Title.ShouldBe(string.Empty);
        feed.Channel.Description.ShouldBe(string.Empty);
        feed.Channel.Items.ShouldBeEmpty();
    }

    /// <summary>
    /// Items are held in the order they were added.
    /// </summary>
    [TestMethod]
    public void RssFeed_AddingItemsToChannel_WorksCorrectly()
    {
        // Arrange
        RssFeed feed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        RssItem item1 = new()
        {
            Title = "First Item",
            Link = new Uri("http://example.com/item1"),
            Description = "First item description"
        };

        RssItem item2 = new()
        {
            Title = "Second Item",
            Link = new Uri("http://example.com/item2"),
            Description = "Second item description"
        };

        // Act
        feed.Channel.Items.Add(item1);
        feed.Channel.Items.Add(item2);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
        feed.Channel.Items[1].Title.ShouldBe("Second Item");
    }

    /// <summary>
    /// Assigning <see langword="null"/> to the channel throws
    /// <see cref="ArgumentNullException"/> rather than leaving the feed channelless.
    /// </summary>
    [TestMethod]
    public void RssFeed_SetChannelToNull_ThrowsArgumentNullException()
    {
        // Arrange
        RssFeed feed = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => feed.Channel = null!);
    }

    #endregion

    #region Feed Parsing Tests

    // guid and ttl were parsed by RssItem and RssChannel but appeared in no fixture anywhere in the
    // suite - they were only ever set through the object model, so the two SelectSingleNode calls
    // that read them were executed by no test. Added before those lines were rewritten.

    /// <summary>
    /// A <c>guid</c> element loads with its text and with
    /// <c>isPermaLink="false"</c> read as a false permanent-link flag.
    /// </summary>
    /// <remarks>
    ///     The element appeared in no fixture anywhere in the suite: it was only ever
    ///     set through the object model, so the line that reads it during a parse was
    ///     executed by no test. This test was added before that line was rewritten.
    /// </remarks>
    [TestMethod]
    public void RssFeed_LoadingItemWithGuid_PopulatesGuid()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <title>Test Feed</title>
                <link>http://example.com</link>
                <description>A test feed</description>
                <item>
                  <title>Item</title>
                  <guid isPermaLink="false">urn:uuid:6a7b1c2d</guid>
                </item>
              </channel>
            </rss>
            """;
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        feed.Load(stream);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        item.Guid.ShouldNotBeNull();
        item.Guid.Value.ShouldBe("urn:uuid:6a7b1c2d");
        item.Guid.IsPermanentLink.ShouldBeFalse();
    }

    /// <summary>
    /// A <c>ttl</c> element loads as the channel time to live, in minutes.
    /// </summary>
    /// <remarks>
    ///     The element appeared in no fixture anywhere in the suite: it was only ever
    ///     set through the object model, so the line that reads it during a parse was
    ///     executed by no test. This test was added before that line was rewritten.
    /// </remarks>
    [TestMethod]
    public void RssFeed_LoadingChannelWithTimeToLive_PopulatesTimeToLive()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <title>Test Feed</title>
                <link>http://example.com</link>
                <description>A test feed</description>
                <ttl>60</ttl>
              </channel>
            </rss>
            """;
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.TimeToLive.ShouldBe(60);
    }

    /// <summary>
    /// Loading a minimal RSS 2.0 stream populates the channel
    /// title, link and description, and the feed reports RSS 2.0.
    /// </summary>
    [TestMethod]
    public void RssFeed_LoadingValidRssXml_PopulatesAllProperties()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe("Test Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test feed");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Version.Major.ShouldBe(2);
        feed.Version.Minor.ShouldBe(0);
    }

    /// <summary>
    /// Items load in document order, each with its title, link, description and every one of its categories.
    /// </summary>
    [TestMethod]
    public void RssFeed_LoadingRssWithItems_PopulatesItemCollection()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.RssWithItems));

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);

        RssItem firstItem = feed.Channel.Items[0];
        firstItem.Title.ShouldBe("Recent Item");
        firstItem.Link.ShouldBe(new Uri("http://example.com/recent"));
        firstItem.Description.ShouldBe("A recent item");
        firstItem.Categories.Count.ShouldBe(2);
        firstItem.Categories[0].Value.ShouldBe("Tech");
        firstItem.Categories[1].Value.ShouldBe("News");

        RssItem secondItem = feed.Channel.Items[1];
        secondItem.Title.ShouldBe("Old Item");
        secondItem.Link.ShouldBe(new Uri("http://example.com/old"));
        secondItem.Categories.Count.ShouldBe(1);
        secondItem.Categories[0].Value.ShouldBe("Archive");
    }

    /// <summary>
    /// An unclosed element surfaces as <see cref="XmlException"/> rather than as a partly populated feed.
    /// </summary>
    [TestMethod]
    public void RssFeed_LoadingMalformedXml_ThrowsXmlException()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MalformedXml));

        // Act & Assert
        Should.Throw<XmlException>(() => feed.Load(stream));
    }

    /// <summary>
    /// Loading raises <c>Loaded</c>, and the handler receives event arguments rather than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void RssFeed_LoadingValidRss_RaisesLoadedEvent()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        SyndicationResourceLoadedEventArgs? eventArgs = null;

        feed.Loaded += (_, args) =>
        {
            eventRaised = true;
            eventArgs = args;
        };

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue();
        eventArgs.ShouldNotBeNull();
    }

    /// <summary>
    /// With extension auto-detection on, a <c>dc:creator</c> element leaves the channel reporting an extension.
    /// </summary>
    [TestMethod]
    public void RssFeed_LoadingWithAutoDetectExtensions_DiscoversExtensions()
    {
        // Arrange
        const string rssWithExtension = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>A test feed</description>
                    <dc:creator>Test Author</dc:creator>
                    <item>
                        <title>Test Item</title>
                        <dc:creator>Item Author</dc:creator>
                    </item>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rssWithExtension));

        // Act
        feed.Load(stream, settings);

        // Assert
        feed.Channel.Title.ShouldBe("Test Feed");
        // Extensions are discovered and populated on the channel and/or items
        feed.Channel.HasExtensions.ShouldBeTrue();
    }

    /// <summary>
    /// A <c>category</c> element on the channel loads separately from the categories on the items.
    /// </summary>
    [TestMethod]
    public void RssFeed_LoadingChannelCategories_PopulatesCategories()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.RssWithItems));

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Categories.Count.ShouldBe(1);
        feed.Channel.Categories[0].Value.ShouldBe("Technology");
    }

    #endregion

    #region Round-Trip Serialization Tests

    /// <summary>
    /// Title, link, description, copyright, managing editor,
    /// webmaster and time to live all survive a save and reload.
    /// </summary>
    [TestMethod]
    public void RssFeed_RoundTrip_PreservesChannelProperties()
    {
        // Arrange
        RssFeed originalFeed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description",
                Copyright = "Copyright 2024",
                ManagingEditor = "editor@example.com",
                Webmaster = "webmaster@example.com",
                TimeToLive = 60
            }
        };

        // Act - Save
        using MemoryStream stream = new();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Channel.Title.ShouldBe(originalFeed.Channel.Title);
        loadedFeed.Channel.Link.ShouldBe(originalFeed.Channel.Link);
        loadedFeed.Channel.Description.ShouldBe(originalFeed.Channel.Description);
        loadedFeed.Channel.Copyright.ShouldBe(originalFeed.Channel.Copyright);
        loadedFeed.Channel.ManagingEditor.ShouldBe(originalFeed.Channel.ManagingEditor);
        loadedFeed.Channel.Webmaster.ShouldBe(originalFeed.Channel.Webmaster);
        loadedFeed.Channel.TimeToLive.ShouldBe(originalFeed.Channel.TimeToLive);
    }

    /// <summary>
    /// An item survives a save and reload with its author and both categories, the domain of the second included.
    /// </summary>
    [TestMethod]
    public void RssFeed_RoundTrip_PreservesItems()
    {
        // Arrange
        RssFeed originalFeed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        RssItem item = new()
        {
            Title = "Test Item",
            Link = new Uri("http://example.com/item"),
            Description = "Item description",
            Author = "author@example.com"
        };
        item.Categories.Add(new RssCategory("Category1"));
        item.Categories.Add(new RssCategory("Category2", "domain"));
        originalFeed.Channel.Items.Add(item);

        // Act - Save
        using MemoryStream stream = new();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Channel.Items.Count.ShouldBe(1);
        RssItem loadedItem = loadedFeed.Channel.Items[0];
        loadedItem.Title.ShouldBe(item.Title);
        loadedItem.Link.ShouldBe(item.Link);
        loadedItem.Description.ShouldBe(item.Description);
        loadedItem.Author.ShouldBe(item.Author);
        loadedItem.Categories.Count.ShouldBe(2);
        loadedItem.Categories[0].Value.ShouldBe("Category1");
        loadedItem.Categories[1].Value.ShouldBe("Category2");
        loadedItem.Categories[1].Domain.ShouldBe("domain");
    }

    /// <summary>
    /// Five items survive a save and reload in the order they were added.
    /// </summary>
    [TestMethod]
    public void RssFeed_RoundTrip_PreservesMultipleItems()
    {
        // Arrange
        RssFeed originalFeed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        for (int i = 0; i < 5; i++)
        {
            originalFeed.Channel.Items.Add(new RssItem
            {
                Title = $"Item {i + 1}",
                Link = new Uri($"http://example.com/item{i + 1}"),
                Description = $"Description for item {i + 1}"
            });
        }

        // Act - Save
        using MemoryStream stream = new();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Channel.Items.Count.ShouldBe(5);
        for (int i = 0; i < 5; i++)
        {
            loadedFeed.Channel.Items[i].Title.ShouldBe($"Item {i + 1}");
            loadedFeed.Channel.Items[i].Link.ShouldBe(new Uri($"http://example.com/item{i + 1}"));
        }
    }

    /// <summary>
    /// An enclosure survives a save and reload with its length, content type and URL.
    /// </summary>
    [TestMethod]
    public void RssFeed_RoundTrip_PreservesEnclosures()
    {
        // Arrange
        RssFeed originalFeed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        RssItem item = new()
        {
            Title = "Podcast Episode"
        };
        item.Enclosures.Add(new RssEnclosure(
            12_345_678L,
            "audio/mpeg",
            new Uri("http://example.com/episode.mp3")));
        originalFeed.Channel.Items.Add(item);

        // Act - Save
        using MemoryStream stream = new();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Channel.Items.Count.ShouldBe(1);
        loadedFeed.Channel.Items[0].Enclosures.Count.ShouldBe(1);
        RssEnclosure enclosure = loadedFeed.Channel.Items[0].Enclosures[0];
        enclosure.Length.ShouldBe(12_345_678L);
        enclosure.ContentType.ShouldBe("audio/mpeg");
        enclosure.Url.ShouldBe(new Uri("http://example.com/episode.mp3"));
    }

    /// <summary>
    /// A Dublin Core element read from a feed is written back out
    /// and read again, leaving the extension count unchanged.
    /// </summary>
    [TestMethod]
    public void RssFeed_RoundTrip_PreservesExtensions()
    {
        // Arrange
        const string rssWithExtension = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>A test feed</description>
                    <dc:creator>Test Author</dc:creator>
                </channel>
            </rss>
            """;

        RssFeed originalFeed = new();
        SyndicationResourceLoadSettings loadSettings = new()
        {
            AutoDetectExtensions = true
        };
        using MemoryStream loadStream = new(Encoding.UTF8.GetBytes(rssWithExtension));
        originalFeed.Load(loadStream, loadSettings);

        // Capture original extension count
        int originalExtensionCount = originalFeed.Channel.Extensions.Count;

        // Act - Save
        using MemoryStream stream = new();
        SyndicationResourceSaveSettings saveSettings = new()
        {
            AutoDetectExtensions = true
        };
        originalFeed.Save(stream, saveSettings);

        // Act - Load
        stream.Position = 0;
        RssFeed reloadedFeed = new();
        reloadedFeed.Load(stream, loadSettings);

        // Assert
        reloadedFeed.Channel.HasExtensions.ShouldBeTrue();
        reloadedFeed.Channel.Extensions.Count.ShouldBe(originalExtensionCount);
    }

    /// <summary>
    /// Parsing, serialising and parsing again gives the same channel properties and the same items, title by title.
    /// </summary>
    [TestMethod]
    public void RssFeed_ParseSerializeParse_ProducesSameFeed()
    {
        // Arrange
        RssFeed firstFeed = new();
        using MemoryStream firstStream = new(Encoding.UTF8.GetBytes(FeedTestData.RssWithItems));
        firstFeed.Load(firstStream);

        // Act - First serialize
        using MemoryStream serializeStream = new();
        firstFeed.Save(serializeStream);

        // Act - Second parse
        serializeStream.Position = 0;
        RssFeed secondFeed = new();
        secondFeed.Load(serializeStream);

        // Assert - Core properties match
        secondFeed.Channel.Title.ShouldBe(firstFeed.Channel.Title);
        secondFeed.Channel.Link.ShouldBe(firstFeed.Channel.Link);
        secondFeed.Channel.Description.ShouldBe(firstFeed.Channel.Description);
        secondFeed.Channel.Items.Count.ShouldBe(firstFeed.Channel.Items.Count);

        // Assert - Items match
        for (int i = 0; i < firstFeed.Channel.Items.Count; i++)
        {
            secondFeed.Channel.Items[i].Title.ShouldBe(firstFeed.Channel.Items[i].Title);
            secondFeed.Channel.Items[i].Link.ShouldBe(firstFeed.Channel.Items[i].Link);
            secondFeed.Channel.Items[i].Description.ShouldBe(firstFeed.Channel.Items[i].Description);
        }
    }

    #endregion

    #region Async Operations Tests

    /// <summary>
    /// Loading over a caller-supplied client populates the channel and raises <c>Loaded</c>.
    /// </summary>
    [TestMethod]
    public async Task RssFeed_LoadAsync_LoadsFeedCorrectly()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(
            new Uri("http://example.com/feed.xml"),
            httpClient,
            cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue();
        feed.Channel.Title.ShouldBe("Test Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test feed");
    }

    /// <summary>
    /// Loading over HTTP preserves the document order of the items, as loading from a stream does.
    /// </summary>
    [TestMethod]
    public async Task RssFeed_LoadAsync_WithItems_LoadsAllItems()
    {
        // Arrange
        RssFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.RssWithItems);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(
            new Uri("http://example.com/feed.xml"),
            httpClient,
            cancellationToken: TestContext.CancellationToken);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("Recent Item");
        feed.Channel.Items[1].Title.ShouldBe("Old Item");
    }

    /// <summary>
    /// The static create returns a feed already populated from the response body.
    /// </summary>
    [TestMethod]
    public async Task RssFeed_CreateAsync_CreatesAndLoadsNewFeed()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        RssFeed feed = await RssFeed.CreateAsync(
            new Uri("http://example.com/feed.xml"),
            httpClient,
            cancellationToken: TestContext.CancellationToken);

        // Assert
        feed.ShouldNotBeNull();
        feed.Channel.Title.ShouldBe("Test Feed");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// Load settings reach the asynchronous path: with auto-detection
    /// on, the fetched channel reports its Dublin Core extension.
    /// </summary>
    [TestMethod]
    public async Task RssFeed_LoadAsync_WithSettings_AppliesSettings()
    {
        // Arrange
        const string rssWithExtension = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>A test feed</description>
                    <dc:creator>Test Author</dc:creator>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(rssWithExtension);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(
            new Uri("http://example.com/feed.xml"),
            httpClient,
            settings,
            cancellationToken: TestContext.CancellationToken);

        // Assert
        feed.Channel.Title.ShouldBe("Test Feed");
        feed.Channel.HasExtensions.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Loaded</c> event reports the URI the feed was fetched from.
    /// </summary>
    [TestMethod]
    public async Task RssFeed_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        RssFeed feed = new();
        Uri? sourceFromEvent = null;

        feed.Loaded += (_, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);
        Uri requestUri = new("http://example.com/feed.xml");

        // Act
        await feed.LoadAsync(requestUri, httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    #endregion

    #region Additional Behavior Tests

    /// <summary>
    /// The navigator a feed creates is rooted on an <c>rss</c> element.
    /// </summary>
    [TestMethod]
    public void RssFeed_CreateNavigator_ReturnsValidNavigator()
    {
        // Arrange
        RssFeed feed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };
        feed.Channel.Items.Add(new RssItem
        {
            Title = "Test Item"
        });

        // Act
        XPathNavigator navigator = feed.CreateNavigator();

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("rss");
    }

    /// <summary>
    /// A channel extension can be found by its XML namespace, here the Dublin Core element set.
    /// </summary>
    [TestMethod]
    public void RssFeed_FindExtension_ReturnsMatchingExtension()
    {
        // Arrange
        const string rssWithExtension = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>A test feed</description>
                    <dc:creator>Test Author</dc:creator>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rssWithExtension));
        feed.Load(stream, settings);

        // Act
        ISyndicationExtension? dublinCoreExtension = feed.Channel.FindExtension(ext =>
            ext.XmlNamespace == "http://purl.org/dc/elements/1.1/");

        // Assert
        dublinCoreExtension.ShouldNotBeNull();
    }

    /// <summary>
    /// A feed carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void RssFeed_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        RssFeed feed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        // Assert
        feed.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// A feed reports <c>SyndicationContentFormat.Rss</c> as the format it implements.
    /// </summary>
    [TestMethod]
    public void RssFeed_Format_ReturnsRss()
    {
        // Arrange & Act
        RssFeed feed = new();

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// A feed reports version <c>2.0</c>, which is the version it writes rather than one it was told.
    /// </summary>
    [TestMethod]
    public void RssFeed_Version_Returns2Point0()
    {
        // Arrange & Act
        RssFeed feed = new();

        // Assert
        feed.Version.Major.ShouldBe(2);
        feed.Version.Minor.ShouldBe(0);
    }

    #endregion
}