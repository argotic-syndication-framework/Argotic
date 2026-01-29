using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Tests for LoadAsync cancellation via CancellationToken.
/// </summary>
[TestClass]
public class LoadAsyncCancellationTests
{
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