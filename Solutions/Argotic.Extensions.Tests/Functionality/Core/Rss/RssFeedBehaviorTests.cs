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
/// Behavior tests for <see cref="RssFeed"/> that verify the public API.
/// </summary>
[TestClass]
public class RssFeedBehaviorTests
{
    public TestContext TestContext { get; set; }

    #region Feed Creation Tests

    [TestMethod]
    public void RssFeed_WhenCreatedProgrammatically_ProducesValidXml()
    {
        // Arrange
        RssFeed feed = new RssFeed(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        // Act
        using MemoryStream stream = new MemoryStream();
        feed.Save(stream);
        stream.Position = 0;

        // Assert
        XDocument xml = XDocument.Load(stream);
        xml.Root.ShouldNotBeNull();
        xml.Root.Name.LocalName.ShouldBe("rss");
        xml.Root.Attribute("version")?.Value.ShouldBe("2.0");
    }

    [TestMethod]
    public void RssFeed_WhenCreatedWithLinkAndTitle_SetsChannelProperties()
    {
        // Arrange & Act
        RssFeed feed = new RssFeed(new Uri("http://example.com"), "Test Feed");

        // Assert
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void RssFeed_WhenCreatedWithDescription_SetsChannelDescription()
    {
        // Arrange & Act
        RssFeed feed = new RssFeed("Test Description");

        // Assert
        feed.Channel.Description.ShouldBe("Test Description");
    }

    [TestMethod]
    public void RssFeed_WhenCreatedWithDefaultConstructor_HasEmptyChannel()
    {
        // Arrange & Act
        RssFeed feed = new RssFeed();

        // Assert
        feed.Channel.ShouldNotBeNull();
        feed.Channel.Title.ShouldBe(string.Empty);
        feed.Channel.Description.ShouldBe(string.Empty);
        feed.Channel.Items.ShouldBeEmpty();
    }

    [TestMethod]
    public void RssFeed_AddingItemsToChannel_WorksCorrectly()
    {
        // Arrange
        RssFeed feed = new RssFeed(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        RssItem item1 = new RssItem
        {
            Title = "First Item",
            Link = new Uri("http://example.com/item1"),
            Description = "First item description"
        };

        RssItem item2 = new RssItem
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

    [TestMethod]
    public void RssFeed_SetChannelToNull_ThrowsArgumentNullException()
    {
        // Arrange
        RssFeed feed = new RssFeed();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => feed.Channel = null!);
    }

    #endregion

    #region Feed Parsing Tests

    [TestMethod]
    public void RssFeed_LoadingValidRssXml_PopulatesAllProperties()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

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

    [TestMethod]
    public void RssFeed_LoadingRssWithItems_PopulatesItemCollection()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.RssWithItems));

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

    [TestMethod]
    public void RssFeed_LoadingMalformedXml_ThrowsXmlException()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.MalformedXml));

        // Act & Assert
        Should.Throw<XmlException>(() => feed.Load(stream));
    }

    [TestMethod]
    public void RssFeed_LoadingValidRss_RaisesLoadedEvent()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        bool eventRaised = false;
        SyndicationResourceLoadedEventArgs? eventArgs = null;

        feed.Loaded += (sender, args) =>
        {
            eventRaised = true;
            eventArgs = args;
        };

        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue();
        eventArgs.ShouldNotBeNull();
    }

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

        RssFeed feed = new RssFeed();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
        {
            AutoDetectExtensions = true
        };

        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(rssWithExtension));

        // Act
        feed.Load(stream, settings);

        // Assert
        feed.Channel.Title.ShouldBe("Test Feed");
        // Extensions are discovered and populated on the channel and/or items
        feed.Channel.HasExtensions.ShouldBeTrue();
    }

    [TestMethod]
    public void RssFeed_LoadingChannelCategories_PopulatesCategories()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.RssWithItems));

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Categories.Count.ShouldBe(1);
        feed.Channel.Categories[0].Value.ShouldBe("Technology");
    }

    #endregion

    #region Round-Trip Serialization Tests

    [TestMethod]
    public void RssFeed_RoundTrip_PreservesChannelProperties()
    {
        // Arrange
        RssFeed originalFeed = new RssFeed(new Uri("http://example.com"), "Test Feed")
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
        using MemoryStream stream = new MemoryStream();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new RssFeed();
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

    [TestMethod]
    public void RssFeed_RoundTrip_PreservesItems()
    {
        // Arrange
        RssFeed originalFeed = new RssFeed(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        RssItem item = new RssItem
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
        using MemoryStream stream = new MemoryStream();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new RssFeed();
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

    [TestMethod]
    public void RssFeed_RoundTrip_PreservesMultipleItems()
    {
        // Arrange
        RssFeed originalFeed = new RssFeed(new Uri("http://example.com"), "Test Feed")
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
        using MemoryStream stream = new MemoryStream();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new RssFeed();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Channel.Items.Count.ShouldBe(5);
        for (int i = 0; i < 5; i++)
        {
            loadedFeed.Channel.Items[i].Title.ShouldBe($"Item {i + 1}");
            loadedFeed.Channel.Items[i].Link.ShouldBe(new Uri($"http://example.com/item{i + 1}"));
        }
    }

    [TestMethod]
    public void RssFeed_RoundTrip_PreservesEnclosures()
    {
        // Arrange
        RssFeed originalFeed = new RssFeed(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        RssItem item = new RssItem
        {
            Title = "Podcast Episode"
        };
        item.Enclosures.Add(new RssEnclosure(
            12345678L,
            "audio/mpeg",
            new Uri("http://example.com/episode.mp3")));
        originalFeed.Channel.Items.Add(item);

        // Act - Save
        using MemoryStream stream = new MemoryStream();
        originalFeed.Save(stream);

        // Act - Load
        stream.Position = 0;
        RssFeed loadedFeed = new RssFeed();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Channel.Items.Count.ShouldBe(1);
        loadedFeed.Channel.Items[0].Enclosures.Count.ShouldBe(1);
        RssEnclosure enclosure = loadedFeed.Channel.Items[0].Enclosures[0];
        enclosure.Length.ShouldBe(12345678L);
        enclosure.ContentType.ShouldBe("audio/mpeg");
        enclosure.Url.ShouldBe(new Uri("http://example.com/episode.mp3"));
    }

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

        RssFeed originalFeed = new RssFeed();
        SyndicationResourceLoadSettings loadSettings = new SyndicationResourceLoadSettings
        {
            AutoDetectExtensions = true
        };
        using MemoryStream loadStream = new MemoryStream(Encoding.UTF8.GetBytes(rssWithExtension));
        originalFeed.Load(loadStream, loadSettings);

        // Capture original extension count
        int originalExtensionCount = originalFeed.Channel.Extensions.Count;

        // Act - Save
        using MemoryStream stream = new MemoryStream();
        SyndicationResourceSaveSettings saveSettings = new SyndicationResourceSaveSettings
        {
            AutoDetectExtensions = true
        };
        originalFeed.Save(stream, saveSettings);

        // Act - Load
        stream.Position = 0;
        RssFeed reloadedFeed = new RssFeed();
        reloadedFeed.Load(stream, loadSettings);

        // Assert
        reloadedFeed.Channel.HasExtensions.ShouldBeTrue();
        reloadedFeed.Channel.Extensions.Count.ShouldBe(originalExtensionCount);
    }

    [TestMethod]
    public void RssFeed_ParseSerializeParse_ProducesSameFeed()
    {
        // Arrange
        RssFeed firstFeed = new RssFeed();
        using MemoryStream firstStream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.RssWithItems));
        firstFeed.Load(firstStream);

        // Act - First serialize
        using MemoryStream serializeStream = new MemoryStream();
        firstFeed.Save(serializeStream);

        // Act - Second parse
        serializeStream.Position = 0;
        RssFeed secondFeed = new RssFeed();
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

    [TestMethod]
    public async Task RssFeed_LoadAsync_LoadsFeedCorrectly()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);

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

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithItems_LoadsAllItems()
    {
        // Arrange
        RssFeed feed = new RssFeed();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.RssWithItems);
        using HttpClient httpClient = new HttpClient(handler);

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

    [TestMethod]
    public async Task RssFeed_CreateAsync_CreatesAndLoadsNewFeed()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);

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

        RssFeed feed = new RssFeed();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
        {
            AutoDetectExtensions = true
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(rssWithExtension);
        using HttpClient httpClient = new HttpClient(handler);

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

    [TestMethod]
    public async Task RssFeed_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        RssFeed feed = new RssFeed();
        Uri? sourceFromEvent = null;

        feed.Loaded += (sender, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);
        Uri requestUri = new Uri("http://example.com/feed.xml");

        // Act
        await feed.LoadAsync(requestUri, httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    #endregion

    #region Additional Behavior Tests

    [TestMethod]
    public void RssFeed_CreateNavigator_ReturnsValidNavigator()
    {
        // Arrange
        RssFeed feed = new RssFeed(new Uri("http://example.com"), "Test Feed")
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

        RssFeed feed = new RssFeed();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
        {
            AutoDetectExtensions = true
        };
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(rssWithExtension));
        feed.Load(stream, settings);

        // Act
        ISyndicationExtension? dublinCoreExtension = feed.Channel.FindExtension(ext =>
            ext.XmlNamespace == "http://purl.org/dc/elements/1.1/");

        // Assert
        dublinCoreExtension.ShouldNotBeNull();
    }

    [TestMethod]
    public void RssFeed_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        RssFeed feed = new RssFeed(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };

        // Assert
        feed.HasExtensions.ShouldBeFalse();
    }

    [TestMethod]
    public void RssFeed_Format_ReturnsRss()
    {
        // Arrange & Act
        RssFeed feed = new RssFeed();

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
    }

    [TestMethod]
    public void RssFeed_Version_Returns2Point0()
    {
        // Arrange & Act
        RssFeed feed = new RssFeed();

        // Assert
        feed.Version.Major.ShouldBe(2);
        feed.Version.Minor.ShouldBe(0);
    }

    #endregion
}
