namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the discovery methods that fetch a page before answering: what markup a feed, pingback or
/// trackback endpoint has to be declared in to be found, and how an absent, missing or failed response
/// is reported.
/// </summary>
/// <remarks>
///     Every response here is served by <see cref="MockHttpMessageHandler"/>, so nothing leaves the
///     machine. The convenience overloads that bind the shared client instead, which no mock can reach,
///     are covered by <see cref="SharedClientDiscoveryTests"/>.
/// </remarks>
[TestClass]
public class SyndicationDiscoveryUtilityAsyncTests
{
    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #region LocateDiscoverableSyndicationEndpointsAsync Tests

    /// <summary>
    /// A page carrying one <c>link rel="alternate" type="application/rss+xml"</c> yields one endpoint,
    /// with its href, type and title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_WithRssLink_ReturnsEndpoints()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithRssLink,
            "text/html");
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/page.html");

        // Act
        IList<DiscoverableSyndicationEndpoint> endpoints =
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

    /// <summary>
    /// A page advertising no feed yields an empty collection rather than <see langword="null"/>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/no-feeds.html");

        // Act
        IList<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        endpoints.ShouldNotBeNull();
        endpoints.Count.ShouldBe(0);
    }

    /// <summary>
    /// A page advertising both an RSS and an Atom feed yields both endpoints.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/multi-feeds.html");

        // Act
        IList<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        endpoints.Count.ShouldBe(2);
        endpoints.ShouldContain(e => e.ContentType == "application/rss+xml");
        endpoints.ShouldContain(e => e.ContentType == "application/atom+xml");
    }

    /// <summary>
    /// A <see langword="null"/> address is refused before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    /// <summary>
    /// A <see langword="null"/> client is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task LocateDiscoverableSyndicationEndpointsAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new("http://example.com/");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                uri,
                null!,
                TestContext.CancellationToken));
    }

    #endregion

    #region SyndicationContentFormatGetAsync Tests

    /// <summary>
    /// A fetched document whose root element is <c>rss</c> is detected as RSS.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_ValidRssFeed_ReturnsRss()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/feed.rss");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// A fetched document whose root element is <c>feed</c> is detected as Atom.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_ValidAtomFeed_ReturnsAtom()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/feed.atom");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.Atom);
    }

    /// <summary>
    /// A fetched document whose root element is <c>opml</c> is detected as OPML.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_ValidOpmlDocument_ReturnsOpml()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/subscriptions.opml");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.Opml);
    }

    /// <summary>
    /// A fetched document with an unrecognised root element yields
    /// <see cref="SyndicationContentFormat.None"/>, which is in contract for "unable to determine".
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/unknown.xml");

        // Act
        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        format.ShouldBe(SyndicationContentFormat.None);
    }

    /// <summary>
    /// A <see langword="null"/> address is refused before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    /// <summary>
    /// A <see langword="null"/> client is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SyndicationContentFormatGetAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new("http://example.com/feed.rss");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
                uri,
                null!,
                TestContext.CancellationToken));
    }

    #endregion

    #region UriExistsAsync Tests

    /// <summary>
    /// An address answering with a successful response and a body exists.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task UriExistsAsync_ValidUri_ReturnsTrue()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/feed.rss");

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeTrue();
    }

    /// <summary>
    /// An address answering <c>404</c> does not exist.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task UriExistsAsync_NotFoundUri_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/nonexistent.rss");

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> address is answered <see langword="false"/> rather than refused — alone
    /// among the methods covered here.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task UriExistsAsync_NullUri_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);

        // Act
        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            null!,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> client is still refused, even though a <see langword="null"/> address is
    /// not.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task UriExistsAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new("http://example.com/feed.rss");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.UriExistsAsync(
                uri,
                null!,
                TestContext.CancellationToken));
    }

    /// <summary>
    /// A connection that fails outright counts as not existing, rather than propagating the exception.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task UriExistsAsync_HttpRequestException_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithException(new HttpRequestException("Connection failed"));
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/feed.rss");

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

    /// <summary>
    /// A page whose body anchors point at the target is reported as referencing it.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SourceReferencesTargetAsync_SourceContainsTarget_ReturnsTrue()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithTargetLink,
            "text/html");
        using HttpClient httpClient = new(handler);
        Uri source = new("http://example.com/page.html");
        Uri target = new("http://example.com/target");

        // Act
        bool references = await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
            source,
            target,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        references.ShouldBeTrue();
    }

    /// <summary>
    /// A page linking elsewhere is not reported as referencing the target: the whole absolute URI has to
    /// match, not the host.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SourceReferencesTargetAsync_SourceDoesNotContainTarget_ReturnsFalse()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithTargetLink,
            "text/html");
        using HttpClient httpClient = new(handler);
        Uri source = new("http://example.com/page.html");
        Uri target = new("http://example.com/different-page");

        // Act
        bool references = await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
            source,
            target,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        references.ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> source page is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SourceReferencesTargetAsync_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);
        Uri target = new("http://example.com/target");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
                null!,
                target,
                httpClient,
                TestContext.CancellationToken));
    }

    /// <summary>
    /// A <see langword="null"/> target is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SourceReferencesTargetAsync_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);
        Uri source = new("http://example.com/page.html");

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

    /// <summary>
    /// A page carrying <c>link rel="pingback"</c> is pingback enabled, even with no <c>X-Pingback</c>
    /// header on the response.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task IsPingbackEnabledAsync_WithPingbackLink_ReturnsTrue()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithPingbackLink,
            "text/html");
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/post.html");

        // Act
        bool isPingbackEnabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        isPingbackEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// A page with neither an <c>X-Pingback</c> header nor a pingback link is not pingback enabled.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/no-pingback.html");

        // Act
        bool isPingbackEnabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        isPingbackEnabled.ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> address is refused before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task IsPingbackEnabledAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region LocatePingbackNotificationServerAsync Tests

    /// <summary>
    /// The href of a <c>link rel="pingback"</c> is returned as the XML-RPC server address.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task LocatePingbackNotificationServerAsync_WithPingbackLink_ReturnsServerUri()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(
            FeedTestData.HtmlWithPingbackLink,
            "text/html");
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/post.html");

        // Act
        Uri? pingbackServer = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        pingbackServer.ShouldNotBeNull();
        pingbackServer.ShouldBe(new Uri("http://example.com/xmlrpc.php"));
    }

    /// <summary>
    /// A page declaring no pingback server yields <see langword="null"/>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/no-pingback.html");

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

    /// <summary>
    /// A page with no embedded <c>rdf:RDF</c> island is not trackback enabled.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/no-trackback.html");

        // Act
        bool isTrackbackEnabled = await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(
            uri,
            httpClient,
            TestContext.CancellationToken);

        // Assert
        isTrackbackEnabled.ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> address is refused before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task IsTrackbackEnabledAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region LocateTrackbackNotificationServersAsync Tests

    /// <summary>
    /// A page with no embedded Trackback RDF yields an empty collection rather than
    /// <see langword="null"/>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
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
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/no-trackback.html");

        // Act
        IList<TrackbackDiscoveryMetadata> metadata =
            await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(
                uri,
                httpClient,
                TestContext.CancellationToken);

        // Assert
        metadata.ShouldNotBeNull();
        metadata.Count.ShouldBe(0);
    }

    /// <summary>
    /// A <see langword="null"/> address is refused before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task LocateTrackbackNotificationServersAsync_NullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent("content");
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(
                null!,
                httpClient,
                TestContext.CancellationToken));
    }

    #endregion

    #region ConditionalGetAsync Tests

    /// <summary>
    /// A <c>304</c> is reported as unmodified, with the status code intact on the result.
    /// </summary>
    /// <remarks>
    ///     The status assertion was <c>ShouldBeNull</c>. A <c>304</c> used to be reported by discarding
    ///     everything about it, so the one status code a conditional GET most needs to distinguish was the
    ///     one it could not.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ConditionalGetAsync_ContentNotModified_ReturnsNotModifiedResult()
    {
        // Arrange
        using MockHttpMessageHandler handler = new((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotModified));
        });
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/feed.rss");
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

        // Was ShouldBeNull. A 304 used to be reported by discarding everything about it, so the one
        // status code a conditional GET most needs to distinguish was the one it could not.
        result.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotModified);
    }

    /// <summary>
    /// A <c>200</c> carrying a body is reported as modified, with its status and content length.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ConditionalGetAsync_ContentModified_ReturnsModifiedResult()
    {
        // Arrange
        using MockHttpMessageHandler handler = new((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(FeedTestData.MinimalRss, System.Text.Encoding.UTF8, "application/xml")
            };
            response.Content.Headers.LastModified = DateTimeOffset.UtcNow;
            return Task.FromResult(response);
        });
        using HttpClient httpClient = new(handler);
        Uri uri = new("http://example.com/feed.rss");
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

    /// <summary>
    /// A <see langword="null"/> address is refused before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ConditionalGetAsync_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await SyndicationDiscoveryUtility.ConditionalGetAsync(
                null!,
                DateTime.UtcNow,
                "etag",
                httpClient,
                TestContext.CancellationToken));
    }

    /// <summary>
    /// A <see langword="null"/> client is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ConditionalGetAsync_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        Uri uri = new("http://example.com/feed.rss");

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