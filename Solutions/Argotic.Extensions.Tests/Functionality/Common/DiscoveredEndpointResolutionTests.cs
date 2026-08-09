namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers what a discovered syndication endpoint's address resolves to.
/// </summary>
/// <remarks>
///     <para>
///     <c>href="/feed.xml"</c> is the commonest form an auto-discovery link takes, and it produced an
///     endpoint that <b>could not be fetched</b>: the address was stored exactly as written, and
///     <c>DiscoverableSyndicationEndpoint.CreateNavigatorAsync</c> hands it to <c>HttpClient</c>, which
///     rejects a relative URI.
///     </para>
///     <para>
///     The one existing test for discovery uses a fixture whose <c>href</c> is absolute, so it passed
///     throughout and was not evidence either way.
///     </para>
/// </remarks>
[TestClass]
public sealed class DiscoveredEndpointResolutionTests
{
    private const string PageWithRootRelativeLink =
        """<html><head><link rel="alternate" type="application/rss+xml" href="/feed.xml" /></head><body>x</body></html>""";

    private const string PageWithDocumentRelativeLink =
        """<html><head><link rel="alternate" type="application/rss+xml" href="feed.xml" /></head><body>x</body></html>""";

    private const string PageWithAbsoluteLink =
        """<html><head><link rel="alternate" type="application/rss+xml" href="http://elsewhere.invalid/feed.xml" /></head><body>x</body></html>""";

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// A root-relative href resolves against the page it was found on.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ARootRelativeHref_ResolvesAgainstThePageItWasFoundOn()
    {
        IList<DiscoverableSyndicationEndpoint> endpoints = await DiscoverAsync(
            PageWithRootRelativeLink, new Uri("http://example.invalid/blog/index.html"));

        endpoints.Count.ShouldBe(1);
        endpoints[0].Source!.IsAbsoluteUri.ShouldBeTrue("a relative endpoint cannot be fetched");
        endpoints[0].Source.ShouldBe(new Uri("http://example.invalid/feed.xml"));
    }

    /// <summary>
    /// A document-relative href resolves against the directory of the page.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ADocumentRelativeHref_ResolvesAgainstTheDirectory()
    {
        IList<DiscoverableSyndicationEndpoint> endpoints = await DiscoverAsync(
            PageWithDocumentRelativeLink, new Uri("http://example.invalid/blog/index.html"));

        endpoints.Count.ShouldBe(1);
        endpoints[0].Source.ShouldBe(new Uri("http://example.invalid/blog/feed.xml"));
    }

    /// <summary>
    /// An absolute href is left exactly as it was written.
    /// </summary>
    /// <remarks>
    ///     The control. Without it, the two rows above are equally consistent with "every href is now
    ///     rewritten relative to the page", which would break every feed hosted somewhere else.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnAbsoluteHref_IsLeftAlone()
    {
        IList<DiscoverableSyndicationEndpoint> endpoints = await DiscoverAsync(
            PageWithAbsoluteLink, new Uri("http://example.invalid/blog/index.html"));

        endpoints.Count.ShouldBe(1);
        endpoints[0].Source.ShouldBe(new Uri("http://elsewhere.invalid/feed.xml"));
    }

    /// <summary>
    /// A redirect resolves against where the page ended up, not where it was asked for.
    /// </summary>
    /// <remarks>
    ///     Resolving against the requested address would send the caller to <c>example.invalid</c> for a
    ///     feed that lives on <c>www.example.invalid</c> — which is the whole point of following the
    ///     redirect in the first place.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ARedirectedPage_ResolvesAgainstWhereItEndedUp()
    {
        Uri requested = new("http://example.invalid/blog/");
        Uri landed = new("http://www.example.invalid/weblog/");

        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(PageWithRootRelativeLink));
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage response = content.InAResponse("text/html");

            // What HttpClient does after following a redirect: the response carries the request that
            // finally succeeded, not the one originally issued.
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, landed);
            return Task.FromResult(response);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        IList<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                requested, client, TestContext.CancellationTokenSource.Token);

        endpoints.Count.ShouldBe(1);
        endpoints[0].Source.ShouldBe(new Uri("http://www.example.invalid/feed.xml"));
    }

    /// <summary>
    /// Parsing markup with no address to resolve against still stores the href as written.
    /// </summary>
    /// <remarks>
    ///     The overload taking only markup is unchanged. A caller parsing HTML they already hold has no
    ///     address to resolve against, and inventing one would be worse than leaving the href alone.
    /// </remarks>
    [TestMethod]
    public void ParsingMarkupWithNoBaseUri_LeavesARelativeHrefRelative()
    {
        IList<DiscoverableSyndicationEndpoint> endpoints =
            SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(PageWithRootRelativeLink);

        endpoints.Count.ShouldBe(1);
        endpoints[0].Source!.IsAbsoluteUri.ShouldBeFalse();
    }

    private async Task<IList<DiscoverableSyndicationEndpoint>> DiscoverAsync(string markup, Uri source)
    {
        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(markup));
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        return await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
            source, client, TestContext.CancellationTokenSource.Token);
    }
}