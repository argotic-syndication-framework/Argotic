namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Covers what the <c>LoadAsync(Uri, CancellationToken)</c> overloads do before they do any work: an
/// already-cancelled token, and a null source.
/// </summary>
/// <remarks>
///     Neither guard lets a request out, so every URI here is inert and no handler is needed.
/// </remarks>
[TestClass]
public class LoadAsyncCancellationTests
{
    /// <summary>
    /// A token cancelled before the call aborts an RSS load with an <c>OperationCanceledException</c>, without a request reaching the network.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RssFeed_LoadAsync_ThrowsWhenCancelled()
    {
        // Arrange
        RssFeed feed = new();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        OperationCanceledException ex = await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await feed.LoadAsync(new Uri("http://example.com/feed.xml"), cts.Token);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A token cancelled before the call aborts an Atom load with an <c>OperationCanceledException</c>, without a request reaching the network.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomFeed_LoadAsync_ThrowsWhenCancelled()
    {
        // Arrange
        AtomFeed feed = new();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        OperationCanceledException ex = await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await feed.LoadAsync(new Uri("http://example.com/feed.xml"), cts.Token);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A token cancelled before the call aborts an OPML load with an <c>OperationCanceledException</c>, without a request reaching the network.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task OpmlDocument_LoadAsync_ThrowsWhenCancelled()
    {
        // Arrange
        OpmlDocument document = new();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        OperationCanceledException ex = await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await document.LoadAsync(new Uri("http://example.com/opml.xml"), cts.Token);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A token cancelled before the call aborts a format-agnostic load with an <c>OperationCanceledException</c>, without a request reaching the network.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task GenericSyndicationFeed_LoadAsync_ThrowsWhenCancelled()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        OperationCanceledException ex = await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await feed.LoadAsync(new Uri("http://example.com/feed.xml"), cts.Token);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A null source URI is refused with an <c>ArgumentNullException</c> before an RSS load begins.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RssFeed_LoadAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        RssFeed feed = new();

        // Act & Assert
        ArgumentNullException ex = await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await feed.LoadAsync(null!, TestContext.CancellationToken);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A null source URI is refused with an <c>ArgumentNullException</c> before an Atom load begins.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        AtomFeed feed = new();

        // Act & Assert
        ArgumentNullException ex = await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await feed.LoadAsync(null!, TestContext.CancellationToken);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A null source URI is refused with an <c>ArgumentNullException</c> before an OPML load begins.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task OpmlDocument_LoadAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        OpmlDocument document = new();

        // Act & Assert
        ArgumentNullException ex = await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await document.LoadAsync(null!, TestContext.CancellationToken);
        });
        ex.ShouldNotBeNull();
    }

    public TestContext TestContext { get; set; }
}