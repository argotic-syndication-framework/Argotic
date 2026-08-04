using System.Xml;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Tests for HTTP error scenarios during LoadAsync operations.
/// </summary>
[TestClass]
public class LoadAsyncHttpTests
{
    [TestMethod]
    public async Task RssFeed_LoadAsync_WithNotFoundResponse_ThrowsHttpRequestException()
    {
        // Arrange
        // An error response body must be surfaced as an HTTP failure rather than parsed as feed content,
        // otherwise a server that returns well-formed XML with a 4xx/5xx status loads as an empty feed.
        RssFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/notfound.xml"), httpClient, cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithNotFoundResponse_ThrowsHttpRequestException()
    {
        // Arrange
        AtomFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/notfound.xml"), httpClient, cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task GenericSyndicationFeed_LoadAsync_WithNotFoundResponse_ThrowsHttpRequestException()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/notfound.xml"), httpClient, cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithNetworkError_ThrowsHttpRequestException()
    {
        // Arrange
        RssFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithException(new HttpRequestException("Network error"));
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithNetworkError_ThrowsHttpRequestException()
    {
        // Arrange
        AtomFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithException(new HttpRequestException("Network error"));
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        RssFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithDelay(TimeSpan.FromSeconds(10), FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);
        using CancellationTokenSource cts = new();

        // Cancel immediately
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: cts.Token));
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        AtomFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithDelay(TimeSpan.FromSeconds(10), FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);
        using CancellationTokenSource cts = new();

        // Cancel immediately
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: cts.Token));
    }

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithDelayedContent_LoadsSuccessfully()
    {
        // Arrange
        RssFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithDelay(TimeSpan.FromMilliseconds(50), FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue("Loaded event should be raised");
        feed.Channel.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public async Task RssFeed_LoadAsync_WithMalformedXml_ThrowsXmlException()
    {
        // Arrange
        RssFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MalformedXml);
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<XmlException>(
            async () => await feed.LoadAsync(new Uri("http://example.com/feed.xml"), httpClient, cancellationToken: TestContext.CancellationToken));
    }

    public TestContext TestContext { get; set; }
}