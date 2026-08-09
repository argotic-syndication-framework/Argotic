using Argotic.Publishing;
namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Fetches every resource type through the convenience overloads that bind the shared <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
///     <para>
///     Eighteen <c>CreateAsync</c>/<c>LoadAsync</c> state machines were at 0% line coverage, and every
///     one is the overload that binds <c>SyndicationEncodingUtility.SharedHttpClient</c> — unreachable
///     from a <see cref="MockHttpMessageHandler"/>, because the singleton takes no handler. The
///     <see cref="HttpClient"/>-taking siblings are covered elsewhere; these are the entry points a
///     caller with no client of their own actually uses.
///     </para>
///     <para>
///     One test per type, each asserting a parsed value rather than mere non-throwing, and each riding
///     the full pipeline: real socket, real <see cref="SocketsHttpHandler"/> with the Argotic
///     defaults, headers-read completion, the bounded drain, and the streaming parse.
///     </para>
/// </remarks>
[TestClass]
public sealed class SharedClientResourceTests
{
    private const string ServiceDocument = """
        <?xml version="1.0" encoding="utf-8"?>
        <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
            <workspace>
                <atom:title>Loopback Workspace</atom:title>
                <collection href="http://example.com/posts">
                    <atom:title>Posts</atom:title>
                    <accept>application/atom+xml;type=entry</accept>
                </collection>
            </workspace>
        </service>
        """;

    private const string CategoryDocument = """
        <?xml version="1.0" encoding="utf-8"?>
        <categories xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom" fixed="yes" scheme="http://example.com/categories">
            <atom:category term="tech"/>
            <atom:category term="news"/>
        </categories>
        """;

    private const string PublishingEntry = """
        <?xml version="1.0" encoding="utf-8"?>
        <entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
            <id>tag:loopback.invalid,2026:1</id>
            <title>Member Resource</title>
            <updated>2026-01-01T00:00:00Z</updated>
            <app:edited>2026-03-04T05:06:07Z</app:edited>
            <app:control><app:draft>yes</app:draft></app:control>
        </entry>
        """;

    private const string MinimalApml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
            <Head><Title>Loopback Profile</Title></Head>
            <Body defaultprofile="default">
                <Profile name="default">
                    <ImplicitData>
                        <Concepts><Concept key="syndication" value="0.90"/></Concepts>
                    </ImplicitData>
                </Profile>
            </Body>
        </APML>
        """;

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// An RSS feed is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RssFeedCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalRss, "application/rss+xml");

        RssFeed feed = await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// An Atom feed is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomFeedCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalAtom, "application/atom+xml");

        AtomFeed feed = await AtomFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Feed");
    }

    /// <summary>
    /// A stand-alone Atom entry document is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomEntryCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalAtomEntry, "application/atom+xml");

        AtomEntry entry = await AtomEntry.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Test Entry");
    }

    /// <summary>
    /// The generic feed wrapper fetches and abstracts through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task GenericSyndicationFeedCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalRss, "application/rss+xml");

        GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// An OPML document is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task OpmlDocumentCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalOpml, "text/x-opml");

        OpmlDocument document = await OpmlDocument.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        document.Head.Title.ShouldBe("Test OPML");
    }

    /// <summary>
    /// An APML document is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ApmlDocumentCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(MinimalApml, "application/xml");

        ApmlDocument document = await ApmlDocument.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        document.Head.Title.ShouldBe("Loopback Profile");
    }

    /// <summary>
    /// A BlogML document is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task BlogMLDocumentCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.BlogMLWithAttachments, "application/xml");

        BlogMLDocument document = await BlogMLDocument.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        document.Posts.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An RSD document is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RsdDocumentCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.Rsd06Minimal, "application/rsd+xml");

        RsdDocument document = await RsdDocument.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        document.EngineName.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// A sitemap is fetched and parsed through the shared client, under its own 64 MiB allowance.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SitemapCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalSitemap, "application/xml");

        Sitemap sitemap = await Sitemap.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        sitemap.Urls.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A sitemap index is fetched and parsed through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SitemapIndexCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalSitemapIndex, "application/xml");

        SitemapIndex index = await SitemapIndex.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        index.Sitemaps.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A service document is fetched and parsed through the shared client, and through an explicit one.
    /// </summary>
    /// <remarks>
    ///     The explicit-client arm exists because that overload was cold too — nothing had ever
    ///     fetched a service document over any client at all.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomServiceDocumentCreateAsync_FetchesThroughBothClients()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(ServiceDocument, "application/atomsvc+xml");

        AtomServiceDocument shared = await AtomServiceDocument.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);
        shared.Workspaces.ShouldHaveSingleItem();

        using HttpClient client = new();
        AtomServiceDocument explicitClient = await AtomServiceDocument.CreateAsync(
            host.Uri, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);
        explicitClient.Workspaces.ShouldHaveSingleItem();
    }

    /// <summary>
    /// A category document is fetched through <c>CreateAsync</c> on both clients, and loaded in place.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomCategoryDocumentCreateAsyncAndLoadAsync_FetchThroughBothClients()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(CategoryDocument, "application/atomcat+xml");

        AtomCategoryDocument shared = await AtomCategoryDocument.CreateAsync(
            host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);
        shared.IsFixed.ShouldBeTrue();
        shared.Scheme.ShouldBe(new Uri("http://example.com/categories"));

        using HttpClient client = new();
        AtomCategoryDocument explicitClient = await AtomCategoryDocument.CreateAsync(
            host.Uri, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);
        explicitClient.IsFixed.ShouldBeTrue();

        AtomCategoryDocument loaded = new();
        await loaded.LoadAsync(host.Uri, this.TestContext.CancellationTokenSource.Token);
        loaded.Scheme.ShouldBe(new Uri("http://example.com/categories"));
    }

    /// <summary>
    /// A publishing entry is fetched through both shared-client <c>CreateAsync</c> shapes, state intact.
    /// </summary>
    /// <remarks>
    ///     The two overloads are the pair from the shadowing fix: <c>(Uri, CancellationToken)</c> and
    ///     the <c>new</c>-slotted <c>(Uri, settings, CancellationToken)</c> that returns the derived
    ///     type. <c>EditedOn</c> is the assertion because it is the value the old shadowed dispatch
    ///     silently lost.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomEntryResourceCreateAsync_FetchesThroughTheSharedClient()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(PublishingEntry, "application/atom+xml");
        DateTime edited = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

        AtomEntryResource plain = await AtomEntryResource.CreateAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);
        plain.EditedOn.ShouldBe(edited);
        plain.IsDraft.ShouldBeTrue();

        AtomEntryResource withSettings = await AtomEntryResource.CreateAsync(
            host.Uri,
            new SyndicationResourceLoadSettings(),
            this.TestContext.CancellationTokenSource.Token);
        withSettings.EditedOn.ShouldBe(edited);
    }

    /// <summary>
    /// The explicit-client Atom entry overload also fetches — it was cold despite taking a client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomEntryCreateAsyncWithAnExplicitClient_Fetches()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalAtomEntry, "application/atom+xml");
        using HttpClient client = new();

        AtomEntry entry = await AtomEntry.CreateAsync(
            host.Uri, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Test Entry");
    }
}