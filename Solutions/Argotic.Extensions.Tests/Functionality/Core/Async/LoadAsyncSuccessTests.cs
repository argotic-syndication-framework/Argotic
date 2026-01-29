using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Tests for successful LoadAsync operations.
/// </summary>
[TestClass]
public class LoadAsyncSuccessTests
{
    [TestMethod]
    public void RssFeed_Load_LoadsValidFeed()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void AtomFeed_Load_LoadsValidFeed()
    {
        // Arrange
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title.Content.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void AtomEntry_Load_LoadsValidEntry()
    {
        // Arrange
        AtomEntry entry = new();
        bool eventRaised = false;
        entry.Loaded += (sender, args) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtomEntry));

        // Act
        entry.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        entry.Title.Content.ShouldBe("Test Entry");
    }

    [TestMethod]
    public void OpmlDocument_Load_LoadsValidDocument()
    {
        // Arrange
        OpmlDocument document = new();
        bool eventRaised = false;
        document.Loaded += (sender, args) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml));

        // Act
        document.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        document.Format.ShouldBe(SyndicationContentFormat.Opml);
        document.Head.Title.ShouldBe("Test OPML");
    }

    [TestMethod]
    public void RssFeed_Load_WithSettings_AppliesSettings()
    {
        // Arrange
        RssFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream, settings);

        // Assert
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void GenericSyndicationFeed_Load_AutoDetectsRssFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void GenericSyndicationFeed_Load_AutoDetectsAtomFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithHttpClient_LoadsValidFeed()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithHttpClient_LoadsValidFeed()
    {
        // Arrange
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title.Content.ShouldBe("Test Feed");
    }

    [TestMethod]
    public async Task AtomEntry_LoadAsync_WithHttpClient_LoadsValidEntry()
    {
        // Arrange
        AtomEntry entry = new();
        bool eventRaised = false;
        entry.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtomEntry);
        using HttpClient httpClient = new(handler);

        // Act
        await entry.LoadAsync(new Uri("http://example.com/entry.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        entry.Title.Content.ShouldBe("Test Entry");
    }

    [TestMethod]
    public async Task OpmlDocument_LoadAsync_WithHttpClient_LoadsValidDocument()
    {
        // Arrange
        OpmlDocument document = new();
        bool eventRaised = false;
        document.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(new Uri("http://example.com/subscriptions.opml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        document.Format.ShouldBe(SyndicationContentFormat.Opml);
        document.Head.Title.ShouldBe("Test OPML");
    }

    [TestMethod]
    public async Task GenericSyndicationFeed_LoadAsync_WithHttpClient_AutoDetectsRssFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public async Task GenericSyndicationFeed_LoadAsync_WithHttpClient_AutoDetectsAtomFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithSettings_AppliesSettings()
    {
        // Arrange
        RssFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, settings, cancellationToken: TestContext.CancellationToken);

        // Assert
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    public TestContext TestContext { get; set; }
}