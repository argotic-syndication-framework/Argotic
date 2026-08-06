using System.Net;
using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Exercises the nine discovery entry points that bind the shared <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
///     <para>
///     Every discovery method has two overloads: one taking an <see cref="HttpClient"/>, covered
///     through <see cref="MockHttpMessageHandler"/>, and a convenience overload binding
///     <c>SyndicationEncodingUtility.SharedHttpClient</c> — which no mock can reach, because the
///     singleton takes no handler. All nine convenience overloads were at 0% line coverage: the only
///     thing that had ever executed them was the example harness, run manually, against live origins.
///     </para>
///     <para>
///     These tests point the singleton at a <see cref="LoopbackHost"/>. That covers more than the
///     one-line delegation: the request crosses a real socket through the real
///     <see cref="SocketsHttpHandler"/> with the Argotic defaults applied, which is the pipeline the
///     mock-based tests deliberately replace.
///     </para>
/// </remarks>
[TestClass]
public sealed class SharedClientDiscoveryTests
{
    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// The format of a served feed is detected through the shared client.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientDetectsAServedFormat()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalRss, "application/rss+xml");

        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);

        format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// A URI that answers with content exists.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientReportsAServedUriAsExisting()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.MinimalRss, "application/rss+xml");

        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);

        exists.ShouldBeTrue();
    }

    /// <summary>
    /// A page that links to the target is reported as referencing it.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientFindsATargetReference()
    {
        Uri target = new("http://example.com/target");
        string page = $"""<html><body><p>See <a href="{target}">this</a>.</p></body></html>""";
        using LoopbackHost host = LoopbackHost.ServingXml(page, "text/html");

        bool references = await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(
            host.Uri, target, this.TestContext.CancellationTokenSource.Token);

        references.ShouldBeTrue();
    }

    /// <summary>
    /// A pingback endpoint declared in an X-Pingback header is found.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientLocatesAPingbackServerFromTheHeader()
    {
        byte[] page = Encoding.UTF8.GetBytes("<html><body>x</body></html>");
        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.Headers.Add("X-Pingback", "http://pingback.invalid/xmlrpc");
            response.ContentType = "text/html";
            response.ContentLength64 = page.Length;
            response.OutputStream.Write(page);
        });

        Uri? server = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);

        server.ShouldBe(new Uri("http://pingback.invalid/xmlrpc"));

        bool enabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);

        enabled.ShouldBeTrue();
    }

    /// <summary>
    /// A trackback endpoint embedded as RDF is found.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientLocatesATrackbackServerFromEmbeddedRdf()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(FeedTestData.HtmlWithTrackbackRdf, "text/html");

        IList<TrackbackDiscoveryMetadata> servers = await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);

        servers.Count.ShouldBe(1);
        servers[0].PingUrl.ShouldBe(new Uri("http://example.com/trackback/1"));

        bool enabled = await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(
            host.Uri, this.TestContext.CancellationTokenSource.Token);

        enabled.ShouldBeTrue();
    }

    /// <summary>
    /// A relative auto-discovery link resolves against the loopback origin it was served from.
    /// </summary>
    /// <remarks>
    ///     The strongest of the nine: it re-proves the base-URI resolution fix through the singleton.
    ///     The served <c>href</c> is root-relative, so the endpoint is only fetchable if it was
    ///     resolved against the address the page actually came from — which here is an OS-assigned
    ///     port nothing could have hard-coded.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientResolvesADiscoveredEndpointAgainstTheOrigin()
    {
        const string Page =
            """<html><head><link rel="alternate" type="application/rss+xml" href="/feed.xml" /></head><body>x</body></html>""";
        using LoopbackHost host = LoopbackHost.ServingXml(Page, "text/html");

        IList<DiscoverableSyndicationEndpoint> endpoints =
            await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
                host.Uri, this.TestContext.CancellationTokenSource.Token);

        endpoints.Count.ShouldBe(1);
        endpoints[0].Source.ShouldBe(new Uri(host.Uri, "/feed.xml"));
    }

    /// <summary>
    /// A conditional GET through the shared client reports a 304 as unmodified, validators intact.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientReportsA304AsUnmodified()
    {
        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.StatusCode = (int)HttpStatusCode.NotModified;
            response.Headers.Add("ETag", "\"loopback-v1\"");
        });

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            host.Uri,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "\"loopback-v0\"",
            this.TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeFalse();
        result.ETag.ShouldBe("\"loopback-v1\"", "the rotated validator survives through the singleton path too");

        LoopbackHost.CapturedRequest sent = host.Requests.ShouldHaveSingleItem();
        sent.Headers.ShouldContainKey("If-None-Match");
        sent.Headers["If-None-Match"].ShouldBe("\"loopback-v0\"");
        sent.Headers.ShouldContainKey("If-Modified-Since");
    }
}