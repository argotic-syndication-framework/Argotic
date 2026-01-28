using System.Collections.ObjectModel;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Async tests for <see cref="SyndicationDiscoveryUtility"/> methods that perform network operations.
/// </summary>
[TestClass]
public class SyndicationDiscoveryUtilityAsyncTests
{
    public TestContext TestContext { get; set; } = null!;

    #region LocateDiscoverableSyndicationEndpointsAsync Tests

    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_WithRssLink_ReturnsEndpoints()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithRssLink,
            "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/page.html");

        // Act
        Collection<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        endpoints.ShouldNotBeNull();
        endpoints.Count.ShouldBe(1);
        endpoints[0].Source.ShouldBe(new Uri("http://example.com/feed.rss"));
        endpoints[0].ContentType.ShouldBe("application/rss+xml");
        endpoints[0].Title.ShouldBe("RSS Feed");
    }

    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_NoLinks_ReturnsEmpty()
    {
        // Arrange
        const string htmlWithNoLinks = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>No Feeds Page</title>
            </head>
            <body>
                <p>This page has no feed links.</p>
            </body>
            </html>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(htmlWithNoLinks, "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/no-feeds.html");

        // Act
        Collection<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        endpoints.ShouldNotBeNull();
        endpoints.Count.ShouldBe(0);
    }

    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_WithMultipleLinks_ReturnsAllEndpoints()
    {
        // Arrange
        const string htmlWithMultipleFeeds = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Multiple Feeds Page</title>
                <link rel="alternate" type="application/rss+xml" title="RSS Feed" href="http://example.com/feed.rss" />
                <link rel="alternate" type="application/atom+xml" title="Atom Feed" href="http://example.com/feed.atom" />
            </head>
            <body>
                <p>Content</p>
            </body>
            </html>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(htmlWithMultipleFeeds, "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/multi-feeds.html");

        // Act
        Collection<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        endpoints.Count.ShouldBe(2);
        endpoints.ShouldContain(e => e.ContentType == "application/rss+xml");
        endpoints.ShouldContain(e => e.ContentType == "application/atom+xml");
    }

    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new Uri("http://example.com/");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                null!,
                TestContext.CancellationToken));
    }

    #endregion

    #region SyndicationContentFormatGetAsync Tests

    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_ValidRssFeed_ReturnsRss()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/feed.rss");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.Rss);
    }

    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_ValidAtomFeed_ReturnsAtom()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/feed.atom");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.Atom);
    }

    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_ValidOpmlDocument_ReturnsOpml()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/subscriptions.opml");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.Opml);
    }

    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_UnknownFormat_ReturnsNone()
    {
        // Arrange
        const string unknownXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <unknown>
                <element>data</element>
            </unknown>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(unknownXml);
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/unknown.xml");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.None);
    }

    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new Uri("http://example.com/feed.rss");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
                uri,
                null!,
                TestContext.CancellationToken));
    }

    #endregion

    #region UriExistsAsync Tests

    [TestMethod]
    public async Task UriExistsAsync_ValidUri_ReturnsTrue()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/feed.rss");

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeTrue();
    }

    [TestMethod]
    public async Task UriExistsAsync_NotFoundUri_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/nonexistent.rss");

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    [TestMethod]
    public async Task UriExistsAsync_NullUri_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            null!,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    [TestMethod]
    public async Task UriExistsAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new Uri("http://example.com/feed.rss");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.UriExistsAsync(
                uri,
                null!,
                TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task UriExistsAsync_HttpRequestException_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithException(new HttpRequestException("Connection failed"));
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/feed.rss");

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    #endregion

    #region SourceReferencesTargetAsync Tests

    [TestMethod]
    public async Task SourceReferencesTargetAsync_SourceContainsTarget_ReturnsTrue()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithTargetLink,
            "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri source = new Uri("http://example.com/page.html");
        Uri target = new Uri("http://example.com/target");

        // Act
        bool references = await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
            source,
            target,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        references.ShouldBeTrue();
    }

    [TestMethod]
    public async Task SourceReferencesTargetAsync_SourceDoesNotContainTarget_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithTargetLink,
            "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri source = new Uri("http://example.com/page.html");
        Uri target = new Uri("http://example.com/different-page");

        // Act
        bool references = await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
            source,
            target,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        references.ShouldBeFalse();
    }

    [TestMethod]
    public async Task SourceReferencesTargetAsync_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);
        Uri target = new Uri("http://example.com/target");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
                null!,
                target,
                httpClient,
                TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task SourceReferencesTargetAsync_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);
        Uri source = new Uri("http://example.com/page.html");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
                source,
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region IsPingbackEnabledAsync Tests

    [TestMethod]
    public async Task IsPingbackEnabledAsync_WithPingbackLink_ReturnsTrue()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithPingbackLink,
            "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/post.html");

        // Act
        bool isPingbackEnabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        isPingbackEnabled.ShouldBeTrue();
    }

    [TestMethod]
    public async Task IsPingbackEnabledAsync_NoPingbackLink_ReturnsFalse()
    {
        // Arrange
        const string htmlWithoutPingback = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>No Pingback Page</title>
            </head>
            <body>
                <p>This page has no pingback link.</p>
            </body>
            </html>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(htmlWithoutPingback, "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/no-pingback.html");

        // Act
        bool isPingbackEnabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        isPingbackEnabled.ShouldBeFalse();
    }

    [TestMethod]
    public async Task IsPingbackEnabledAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region LocatePingbackNotificationServerAsync Tests

    [TestMethod]
    public async Task LocatePingbackNotificationServerAsync_WithPingbackLink_ReturnsServerUri()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithPingbackLink,
            "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/post.html");

        // Act
        Uri? pingbackServer = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        pingbackServer.ShouldNotBeNull();
        pingbackServer.ShouldBe(new Uri("http://example.com/xmlrpc.php"));
    }

    [TestMethod]
    public async Task LocatePingbackNotificationServerAsync_NoPingbackLink_ReturnsNull()
    {
        // Arrange
        const string htmlWithoutPingback = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>No Pingback Page</title>
            </head>
            <body>
                <p>This page has no pingback link.</p>
            </body>
            </html>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(htmlWithoutPingback, "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/no-pingback.html");

        // Act
        Uri? pingbackServer = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        pingbackServer.ShouldBeNull();
    }

    #endregion

    #region IsTrackbackEnabledAsync Tests

    [TestMethod]
    public async Task IsTrackbackEnabledAsync_NoTrackbackRdf_ReturnsFalse()
    {
        // Arrange
        const string htmlWithoutTrackback = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>No Trackback Page</title>
            </head>
            <body>
                <p>This page has no trackback RDF.</p>
            </body>
            </html>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(htmlWithoutTrackback, "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/no-trackback.html");

        // Act
        bool isTrackbackEnabled = await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        isTrackbackEnabled.ShouldBeFalse();
    }

    [TestMethod]
    public async Task IsTrackbackEnabledAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region LocateTrackbackNotificationServersAsync Tests

    [TestMethod]
    public async Task LocateTrackbackNotificationServersAsync_NoTrackback_ReturnsEmptyCollection()
    {
        // Arrange
        const string htmlWithoutTrackback = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>No Trackback Page</title>
            </head>
            <body>
                <p>This page has no trackback RDF.</p>
            </body>
            </html>
            """;
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(htmlWithoutTrackback, "text/html");
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/no-trackback.html");

        // Act
        Collection<TrackbackDiscoveryMetadata> metadata =
            await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        metadata.ShouldNotBeNull();
        metadata.Count.ShouldBe(0);
    }

    [TestMethod]
    public async Task LocateTrackbackNotificationServersAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new HttpClient(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region ConditionalGetAsync Tests

    [TestMethod]
    public async Task ConditionalGetAsync_ContentNotModified_ReturnsNotModifiedResult()
    {
        // Arrange
        using MockHttpMessageHandler handler = new MockHttpMessageHandler((req, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotModified));
        });
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/feed.rss");
        DateTime lastModified = DateTime.UtcNow.AddHours(-1);

        // Act
        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            uri,
            lastModified,
            "\"etag123\"",
            httpClient,
            TestContext.CancellationToken);

        // Assert
        result.WasModified.ShouldBeFalse();
        result.StatusCode.ShouldBe(default(System.Net.HttpStatusCode));
    }

    [TestMethod]
    public async Task ConditionalGetAsync_ContentModified_ReturnsModifiedResult()
    {
        // Arrange
        using MockHttpMessageHandler handler = new MockHttpMessageHandler((req, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(FeedTestData.MinimalRss, System.Text.Encoding.UTF8, "application/xml")
            };
            response.Content.Headers.LastModified = DateTimeOffset.UtcNow;
            return Task.FromResult(response);
        });
        using HttpClient httpClient = new HttpClient(handler);
        Uri uri = new Uri("http://example.com/feed.rss");
        DateTime lastModified = DateTime.UtcNow.AddDays(-1);

        // Act
        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            uri,
            lastModified,
            "\"old-etag\"",
            httpClient,
            TestContext.CancellationToken);

        // Assert
        result.WasModified.ShouldBeTrue();
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        result.ContentLength.ShouldBeGreaterThan(0);
    }

    [TestMethod]
    public async Task ConditionalGetAsync_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new HttpClient(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.ConditionalGetAsync(
                null!,
                DateTime.UtcNow,
                "etag",
                httpClient,
                TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ConditionalGetAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new Uri("http://example.com/feed.rss");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.ConditionalGetAsync(
                uri,
                DateTime.UtcNow,
                "etag",
                null!,
                TestContext.CancellationToken));
    }

    #endregion
}
