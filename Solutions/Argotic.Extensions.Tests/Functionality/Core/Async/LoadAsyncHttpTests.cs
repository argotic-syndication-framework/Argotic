namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Covers what a load does with a response that is not a feed: an error status, a transport failure, a
/// cancellation, and a body that is not well-formed XML.
/// </summary>
/// <remarks>
///     Every request is served by a <c>MockHttpMessageHandler</c>, so each failure is produced on
///     demand rather than waited for, and nothing here leaves the machine.
/// </remarks>
[TestClass]
public class LoadAsyncHttpTests
{
    /// <summary>
    /// A <c>404</c> aborts an RSS load with an <c>HttpRequestException</c> rather than parsing the error body.
    /// </summary>
    /// <remarks>
    ///     The comment in the body states the failure mode: a server that answers a 4xx or 5xx with
    ///     well-formed XML would otherwise load as an empty feed.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A <c>404</c> aborts an Atom load with an <c>HttpRequestException</c> rather than parsing the error body.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A <c>404</c> aborts a format-agnostic load with an <c>HttpRequestException</c> rather than parsing the error body.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A transport failure raised by the handler reaches the caller as the <c>HttpRequestException</c> it was, unwrapped.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// The same holds for an Atom load: a transport failure reaches the caller as an <c>HttpRequestException</c>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A token cancelled before an RSS load aborts it with an <c>OperationCanceledException</c>, without waiting out the ten-second delay the handler was given.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A token cancelled before an Atom load aborts it with an <c>OperationCanceledException</c>, without waiting out the ten-second delay the handler was given.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A response that arrives 50 milliseconds late still loads, raising <c>Loaded</c> and parsing the channel title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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

    /// <summary>
    /// A body that is not well-formed XML surfaces as an <c>XmlException</c>, not as an empty feed.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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