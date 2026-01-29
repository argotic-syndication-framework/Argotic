using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Tests for SyndicationEncodingUtility.CreateSafeNavigatorAsync.
/// </summary>
[TestClass]
public class CreateSafeNavigatorAsyncTests
{
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

    [TestMethod]
    public void CreateSafeNavigator_WithValidXml_ReturnsValidNavigator()
    {
        // Arrange
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        // Act
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);

        // Assert
        navigator.ShouldNotBeNull();
    }

    public TestContext TestContext { get; set; }
}