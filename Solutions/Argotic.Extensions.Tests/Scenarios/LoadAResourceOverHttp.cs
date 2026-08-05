using System.Net;
using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Publishing;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Fetching a syndication resource from a URI, the way the documentation shows.
/// </summary>
/// <remarks>
///     <para>
///     <c>CreateAsync(Uri, …)</c> is the one-liner every README opens with, and every one of the
///     static factories was at zero coverage - across all thirteen resource types, in both the
///     shared-client and injected-client shapes. <c>Sitemap</c> and <c>SitemapIndex</c> have no
///     synchronous factory at all, so those two types could not be fetched by any covered path.
///     </para>
///     <para>
///     The overloads taking no <see cref="HttpClient"/> resolve
///     <c>SyndicationEncodingUtility.SharedHttpClient</c>, which is a read-only static with no seam,
///     so they cannot be exercised without a real socket and are deliberately not covered here. The
///     injected-client overloads carry the same body.
///     </para>
/// </remarks>
[TestClass]
public sealed class LoadAResourceOverHttp : IDisposable
{
    private static readonly Uri Source = new("http://example.com/resource.xml");

    private readonly List<IDisposable> disposables = [];

    /// <summary>
    /// Disposes the handlers and clients created by the tests.
    /// </summary>
    public void Dispose()
    {
        foreach (IDisposable disposable in this.disposables)
        {
            disposable.Dispose();
        }

        this.disposables.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets or sets the MSTest context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// An RSS feed is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnRssFeed_IsFetchedAndParsedByTheFactory()
    {
        RssFeed feed = await RssFeed.CreateAsync(
            Source, this.Client(FeedTestData.RssWithItems), cancellationToken: TestContext.CancellationTokenSource.Token);

        feed.Channel.Items.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An Atom feed is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnAtomFeed_IsFetchedAndParsedByTheFactory()
    {
        AtomFeed feed = await AtomFeed.CreateAsync(
            Source, this.Client(FeedTestData.AtomWithEntries), cancellationToken: TestContext.CancellationTokenSource.Token);

        feed.Entries.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An OPML document is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnOpmlDocument_IsFetchedAndParsedByTheFactory()
    {
        OpmlDocument document = await OpmlDocument.CreateAsync(
            Source, this.Client(FeedTestData.MinimalOpml), cancellationToken: TestContext.CancellationTokenSource.Token);

        document.Head.ShouldNotBeNull();
    }

    /// <summary>
    /// A format-agnostic feed is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AGenericFeed_IsFetchedAndParsedByTheFactory()
    {
        GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(
            Source, this.Client(FeedTestData.RssWithItems), cancellationToken: TestContext.CancellationTokenSource.Token);

        feed.Items.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An RSD document is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnRsdDocument_IsFetchedAndParsedByTheFactory()
    {
        RsdDocument document = await RsdDocument.CreateAsync(
            Source, this.Client(SampleFeeds.ReadAllText(SampleFeeds.RsdDocument)), cancellationToken: TestContext.CancellationTokenSource.Token);

        document.Interfaces.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An APML document is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnApmlDocument_IsFetchedAndParsedByTheFactory()
    {
        ApmlDocument document = await ApmlDocument.CreateAsync(
            Source, this.Client(SampleFeeds.ReadAllText(SampleFeeds.ApmlDocument)), cancellationToken: TestContext.CancellationTokenSource.Token);

        document.Head.ShouldNotBeNull();
    }

    /// <summary>
    /// A BlogML document is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ABlogMlDocument_IsFetchedAndParsedByTheFactory()
    {
        BlogMLDocument document = await BlogMLDocument.CreateAsync(
            Source, this.Client(SampleFeeds.ReadAllText(SampleFeeds.BlogMLDocument)), cancellationToken: TestContext.CancellationTokenSource.Token);

        document.Posts.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A sitemap is fetched and parsed by the static factory.
    /// </summary>
    /// <remarks>
    ///     <see cref="Argotic.Syndication.Sitemap"/> has no synchronous factory, so this is the only
    ///     way to obtain one from a URI - and it had never been executed.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ASitemap_IsFetchedAndParsedByTheFactory()
    {
        Argotic.Syndication.Sitemap sitemap = await Argotic.Syndication.Sitemap.CreateAsync(
            Source, this.Client(FeedTestData.FullSitemap), cancellationToken: TestContext.CancellationTokenSource.Token);

        sitemap.Urls.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A sitemap index is fetched and parsed by the static factory.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ASitemapIndex_IsFetchedAndParsedByTheFactory()
    {
        SitemapIndex index = await SitemapIndex.CreateAsync(
            Source, this.Client(FeedTestData.FullSitemapIndex), cancellationToken: TestContext.CancellationTokenSource.Token);

        index.Sitemaps.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A sitemap is fetched into an existing instance.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ASitemap_IsFetchedIntoAnExistingInstance()
    {
        Argotic.Syndication.Sitemap sitemap = new();

        await sitemap.LoadAsync(
            Source, this.Client(FeedTestData.FullSitemap), cancellationToken: TestContext.CancellationTokenSource.Token);

        sitemap.Urls.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A sitemap index is fetched into an existing instance.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ASitemapIndex_IsFetchedIntoAnExistingInstance()
    {
        SitemapIndex index = new();

        await index.LoadAsync(
            Source, this.Client(FeedTestData.FullSitemapIndex), cancellationToken: TestContext.CancellationTokenSource.Token);

        index.Sitemaps.ShouldNotBeEmpty();
    }

    /// <summary>
    /// An Atom service document is fetched into an existing instance.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnAtomServiceDocument_IsFetchedIntoAnExistingInstance()
    {
        AtomServiceDocument document = new();

        await document.LoadAsync(
            Source,
            this.Client("""<?xml version="1.0" encoding="utf-8"?><service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom"><workspace><atom:title>A workspace</atom:title></workspace></service>"""),
            cancellationToken: TestContext.CancellationTokenSource.Token);

        document.Workspaces.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A failing fetch surfaces as an HTTP exception rather than an empty resource.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AFetchThatFails_SurfacesAsAnHttpException()
    {
        MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        HttpClient httpClient = new(handler, disposeHandler: false);
        this.disposables.Add(handler);
        this.disposables.Add(httpClient);

        await Should.ThrowAsync<HttpRequestException>(async () => await RssFeed.CreateAsync(
            Source, httpClient, cancellationToken: TestContext.CancellationTokenSource.Token));
    }

    /// <summary>
    /// The same feed, fetched and loaded, decodes two different ways.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>The highest-value row in the characterisation set, and the clearest statement of what
    ///     Phase 6 is for.</b> One document, correctly declaring <c>iso-8859-1</c> and containing byte
    ///     <c>0xE9</c>. Loaded from a stream with a default <see cref="SyndicationResourceLoadSettings"/>
    ///     it comes back with a replacement character; fetched over HTTP it comes back correctly.
    ///     </para>
    ///     <para>
    ///     The cause is that <c>CharacterEncoding</c> defaults to <see cref="Encoding.UTF8"/> and the
    ///     two paths read that default in opposite directions. The synchronous path honours it and
    ///     forces UTF-8 over a document that said otherwise. The asynchronous path treats the very same
    ///     value as meaning "unset", maps it to <see langword="null"/>, and sniffs — which is right here,
    ///     and is wrong for the caller who set UTF-8 deliberately because a feed lies about itself.
    ///     One legitimate value doing duty as a sentinel, producing two different bugs depending on
    ///     which door you came in.
    ///     </para>
    ///     <para>
    ///     <c>windows-1252</c> cannot be used to write this test: it resolves to UTF-8 through the
    ///     encoding fallback and both paths would agree, for the wrong reason. Inverted at Phase 6,
    ///     where the synchronous arm starts producing the accented character too.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSameFeedDecodesDifferently_LoadedSynchronouslyOrFetched()
    {
        byte[] latin1 = Encoding.Latin1.GetBytes(
            """<?xml version="1.0" encoding="iso-8859-1"?><rss version="2.0"><channel><title>café</title><link>http://example.com/</link><description>d</description></channel></rss>""");

        RssFeed loaded = new();
        using (MemoryStream stream = new(latin1, writable: false))
        {
            loaded.Load(stream, new SyndicationResourceLoadSettings());
        }

        MockHttpMessageHandler handler = new((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(latin1),
        }));
        HttpClient httpClient = new(handler, disposeHandler: false);
        this.disposables.Add(handler);
        this.disposables.Add(httpClient);

        RssFeed fetched = await RssFeed.CreateAsync(
            Source, httpClient, cancellationToken: TestContext.CancellationTokenSource.Token);

        loaded.Channel.Title.ShouldBe("caf\uFFFD",
            "PINS TODAY: a default settings object forces UTF-8 over a correct iso-8859-1 declaration. "
            + "Inverted at Phase 6.");
        fetched.Channel.Title.ShouldBe("café",
            "INVARIANT: the fetch path treats the same default as 'unset' and sniffs, which is right here.");
    }

    private HttpClient Client(string body)
    {
        MockHttpMessageHandler handler = new((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/xml"),
        }));

        HttpClient httpClient = new(handler, disposeHandler: false);
        this.disposables.Add(handler);
        this.disposables.Add(httpClient);

        return httpClient;
    }
}