using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Covers the successful path through <c>Load</c> and <c>LoadAsync</c> for each resource type: the
/// <c>Loaded</c> event, the format reported, and the content parsed.
/// </summary>
/// <remarks>
///     The stream and HTTP halves assert the same three things about the same documents, so a defect
///     that reaches only one of the two entry points shows up as a single failing pair.
/// </remarks>
[TestClass]
public class LoadAsyncSuccessTests
{
    /// <summary>
    /// Loading RSS from a stream raises <c>Loaded</c>, reports the <c>Rss</c> format and parses the channel title.
    /// </summary>
    [TestMethod]
    public void RssFeed_Load_LoadsValidFeed()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// Loading Atom from a stream raises <c>Loaded</c>, reports the <c>Atom</c> format and parses the feed title.
    /// </summary>
    [TestMethod]
    public void AtomFeed_Load_LoadsValidFeed()
    {
        // Arrange
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title!.Content.ShouldBe("Test Feed");
    }

    /// <summary>
    /// Loading a standalone Atom entry from a stream raises <c>Loaded</c> and parses the entry title.
    /// </summary>
    [TestMethod]
    public void AtomEntry_Load_LoadsValidEntry()
    {
        // Arrange
        AtomEntry entry = new();
        bool eventRaised = false;
        entry.Loaded += (_, _) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtomEntry));

        // Act
        entry.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        entry.Title!.Content.ShouldBe("Test Entry");
    }

    /// <summary>
    /// Loading OPML from a stream raises <c>Loaded</c>, reports the <c>Opml</c> format and parses the head title.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_Load_LoadsValidDocument()
    {
        // Arrange
        OpmlDocument document = new();
        bool eventRaised = false;
        document.Loaded += (_, _) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml));

        // Act
        document.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        document.Format.ShouldBe(SyndicationContentFormat.Opml);
        document.Head.Title.ShouldBe("Test OPML");
    }

    /// <summary>
    /// A load given settings that turn extension auto-detection on still parses the feed itself.
    /// </summary>
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

    /// <summary>
    /// A generic feed handed RSS reports the <c>Rss</c> format and surfaces the channel title as its own.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_Load_AutoDetectsRssFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// A generic feed handed Atom reports the <c>Atom</c> format and surfaces the feed title as its own.
    /// </summary>
    [TestMethod]
    public void GenericSyndicationFeed_Load_AutoDetectsAtomFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        // Act
        feed.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// Loading RSS over a caller-supplied <c>HttpClient</c> raises <c>Loaded</c>, reports the <c>Rss</c> format and parses the channel title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RssFeed_LoadAsync_WithHttpClient_LoadsValidFeed()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// Loading Atom over a caller-supplied <c>HttpClient</c> raises <c>Loaded</c>, reports the <c>Atom</c> format and parses the feed title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithHttpClient_LoadsValidFeed()
    {
        // Arrange
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title!.Content.ShouldBe("Test Feed");
    }

    /// <summary>
    /// Loading a standalone Atom entry over a caller-supplied <c>HttpClient</c> raises <c>Loaded</c> and parses the entry title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomEntry_LoadAsync_WithHttpClient_LoadsValidEntry()
    {
        // Arrange
        AtomEntry entry = new();
        bool eventRaised = false;
        entry.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtomEntry);
        using HttpClient httpClient = new(handler);

        // Act
        await entry.LoadAsync(new Uri("http://example.com/entry.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        entry.Title!.Content.ShouldBe("Test Entry");
    }

    /// <summary>
    /// Loading OPML over a caller-supplied <c>HttpClient</c> raises <c>Loaded</c>, reports the <c>Opml</c> format and parses the head title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task OpmlDocument_LoadAsync_WithHttpClient_LoadsValidDocument()
    {
        // Arrange
        OpmlDocument document = new();
        bool eventRaised = false;
        document.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(new Uri("http://example.com/subscriptions.opml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        document.Format.ShouldBe(SyndicationContentFormat.Opml);
        document.Head.Title.ShouldBe("Test OPML");
    }

    /// <summary>
    /// A generic feed loaded over HTTP reports the <c>Rss</c> format and surfaces the channel title as its own.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task GenericSyndicationFeed_LoadAsync_WithHttpClient_AutoDetectsRssFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// A generic feed loaded over HTTP reports the <c>Atom</c> format and surfaces the feed title as its own.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task GenericSyndicationFeed_LoadAsync_WithHttpClient_AutoDetectsAtomFormat()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Format.ShouldBe(SyndicationContentFormat.Atom);
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// An asynchronous load given settings that turn extension auto-detection on still parses the feed itself.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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