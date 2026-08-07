using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Covers the navigator factories on <c>SyndicationEncodingUtility</c>: the guards that stop the
/// asynchronous URI overload before it reaches the network, and the synchronous stream overloads.
/// </summary>
[TestClass]
public class CreateSafeNavigatorAsyncTests
{
    /// <summary>
    /// A null source URI is refused with an <c>ArgumentNullException</c> before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task CreateSafeNavigatorAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        ArgumentNullException ex = await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            await SyndicationEncodingUtility.CreateSafeNavigatorAsync(null!, encoding: null, TestContext.CancellationToken);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A token cancelled before the call aborts it with an <c>OperationCanceledException</c> rather than reaching the network.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task CreateSafeNavigatorAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act & Assert
        OperationCanceledException ex = await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
                new Uri("http://example.com/feed.xml"),
                encoding: null,
                cts.Token);
        });
        ex.ShouldNotBeNull();
    }

    /// <summary>
    /// A navigator built from a stream is positioned at the document root, so <c>rss</c> is reachable as its child.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_FromStream_ReturnsValidNavigator()
    {
        // Arrange
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToChild("rss", "").ShouldBeTrue();
    }

    /// <summary>
    /// Naming the encoding explicitly gives the same navigator as letting it be detected.
    /// </summary>
    [TestMethod]
    public void CreateSafeNavigator_FromStreamWithEncoding_ReturnsValidNavigator()
    {
        // Arrange
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream, System.Text.Encoding.UTF8);

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToChild("rss", "").ShouldBeTrue();
    }

    public TestContext TestContext { get; set; }
}