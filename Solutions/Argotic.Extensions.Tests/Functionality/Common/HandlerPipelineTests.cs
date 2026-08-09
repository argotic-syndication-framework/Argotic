using System.Net;
namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Proves the handler defaults do something, by observing them from the other end of a socket.
/// </summary>
/// <remarks>
///     <para>
///     Every prior assertion about <c>ApplyArgoticHandlerDefaults</c> read configuration back:
///     <c>handler.AutomaticDecompression.ShouldBe(...)</c>, <c>handler.UseCookies.ShouldBeFalse()</c>.
///     Those pin the settings, not the behaviour — a <see cref="MockHttpMessageHandler"/> replaces the
///     primary handler, so no mocked test ever decompressed a byte, de-framed a chunk, or declined a
///     cookie. These do, against <see cref="LoopbackHost"/>.
///     </para>
///     <para>
///     Where a test needs a size cap smaller than a format default, it builds its own client from
///     <c>new SocketsHttpHandler()</c> + <c>ApplyArgoticHandlerDefaults</c> — the same construction
///     the <c>IHttpClientFactory</c> registration performs — so the pipeline is the real one and the
///     cap is the test's.
///     </para>
/// </remarks>
[TestClass]
public sealed class HandlerPipelineTests
{
    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Brotli is advertised, and a Brotli response body decodes into a parsed feed.
    /// </summary>
    /// <remarks>
    ///     The discriminating half is the parse: if decompression were not happening, the bytes
    ///     reaching the XML reader would be Brotli framing and the load would throw, so a correct
    ///     title is proof the handler decoded — not merely that it said it would.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ABrotliResponseIsAdvertisedForAndDecoded()
    {
        byte[] compressed = LoopbackHost.Compress(FeedTestData.MinimalRss, "br");
        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.ContentType = "application/rss+xml";
            response.Headers.Add("Content-Encoding", "br");
            response.ContentLength64 = compressed.Length;
            response.OutputStream.Write(compressed);
        });

        RssFeed feed = await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Channel.Title.ShouldBe("Test Feed");

        LoopbackHost.CapturedRequest sent = host.Requests.ShouldHaveSingleItem();
        sent.Headers.ShouldContainKey("Accept-Encoding");
        sent.Headers["Accept-Encoding"].ShouldContain("br");
        sent.Headers["Accept-Encoding"].ShouldContain("gzip");
    }

    /// <summary>
    /// A gzip response body decodes into a parsed feed.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AGzipResponseIsDecoded()
    {
        byte[] compressed = LoopbackHost.Compress(FeedTestData.MinimalRss, "gzip");
        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.ContentType = "application/rss+xml";
            response.Headers.Add("Content-Encoding", "gzip");
            response.ContentLength64 = compressed.Length;
            response.OutputStream.Write(compressed);
        });

        RssFeed feed = await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// The size cap counts decompressed bytes, which is the only count that matters.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The changelog claims the cap is "counted in decompressed bytes", and until now nothing
    ///     proved it — no mocked test can, because decompression happens inside the real handler. The
    ///     served body is a few hundred compressed bytes declaring their compressed length; what
    ///     crosses the cap is the 300,000 bytes they inflate to.
    ///     </para>
    ///     <para>
    ///     <c>DeclaredLength</c> must be null: the handler strips <c>Content-Length</c> when it
    ///     decompresses, so the declared-length fast path cannot have fired and the refusal can only
    ///     have come from the streaming count. That is the assertion that distinguishes "counted
    ///     decompressed" from "compared the header".
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheCapCountsDecompressedBytes()
    {
        string oversized =
            $"""<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel><title>{new string('x', 300_000)}</title><link>http://cap.invalid/</link><description>d</description></channel></rss>""";
        byte[] compressed = LoopbackHost.Compress(oversized, "gzip");
        compressed.Length.ShouldBeLessThan(64 * 1024, "the compressed body must be inside the cap for the test to mean anything");

        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.ContentType = "application/rss+xml";
            response.Headers.Add("Content-Encoding", "gzip");
            response.ContentLength64 = compressed.Length;
            response.OutputStream.Write(compressed);
        });

        using HttpClient client = RealPipelineClient();
        RssFeed feed = new();

        SyndicationContentTooLargeException thrown =
            await Should.ThrowAsync<SyndicationContentTooLargeException>(async () => await feed.LoadAsync(
                host.Uri,
                client,
                new SyndicationResourceLoadSettings { MaxResponseContentLength = 64 * 1024 },
                null,
                this.TestContext.CancellationTokenSource.Token));

        thrown.MaxBytes.ShouldBe(64 * 1024);
        thrown.DeclaredLength.ShouldBeNull(
            "the handler strips Content-Length when decompressing, so only the streaming count can have refused this");
    }

    /// <summary>
    /// A chunked response with no declared length is stopped at the cap.
    /// </summary>
    /// <remarks>
    ///     The shape the streaming cap exists for, previously provable only through
    ///     <see cref="ControllableHttpContent"/> — which never reaches the real handler. Here the
    ///     chunked framing is genuine: <see cref="HttpListener"/> emits it, <see cref="SocketsHttpHandler"/>
    ///     de-frames it, and no <c>Content-Length</c> ever exists.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AChunkedBodyIsStoppedAtTheCap()
    {
        byte[] oversized = Encoding.UTF8.GetBytes(
            $"""<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel><title>{new string('x', 300_000)}</title><link>http://cap.invalid/</link><description>d</description></channel></rss>""");

        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.ContentType = "application/rss+xml";
            response.SendChunked = true;
            response.OutputStream.Write(oversized);
        });

        using HttpClient client = RealPipelineClient();
        RssFeed feed = new();

        SyndicationContentTooLargeException thrown =
            await Should.ThrowAsync<SyndicationContentTooLargeException>(async () => await feed.LoadAsync(
                host.Uri,
                client,
                new SyndicationResourceLoadSettings { MaxResponseContentLength = 64 * 1024 },
                null,
                this.TestContext.CancellationTokenSource.Token));

        thrown.DeclaredLength.ShouldBeNull("chunked transfer encoding declares nothing");
    }

    /// <summary>
    /// A chunked response under the cap loads normally.
    /// </summary>
    /// <remarks>
    ///     The control. Without it the row above is equally consistent with "chunked responses are now
    ///     refused", which would break every origin that streams.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AChunkedBodyUnderTheCapLoads()
    {
        byte[] body = Encoding.UTF8.GetBytes(FeedTestData.MinimalRss);
        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.ContentType = "application/rss+xml";
            response.SendChunked = true;
            response.OutputStream.Write(body);
        });

        RssFeed feed = await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Channel.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// A Set-Cookie from one response is not replayed on the next request.
    /// </summary>
    /// <remarks>
    ///     Through the shared client, because the singleton is where cookie accumulation would be
    ///     unbounded: per-domain session state for the lifetime of the process, with no API to inspect
    ///     it. <c>UseCookies</c> is asserted elsewhere as a property; this is the first test in which a
    ///     server actually offers a cookie and observes whether it comes back.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ACookieOfferedByTheOriginIsNotReplayed()
    {
        byte[] body = Encoding.UTF8.GetBytes(FeedTestData.MinimalRss);
        using LoopbackHost host = LoopbackHost.Start((_, response) =>
        {
            response.Headers.Add("Set-Cookie", "sid=track-me; Path=/");
            response.ContentType = "application/rss+xml";
            response.ContentLength64 = body.Length;
            response.OutputStream.Write(body);
        });

        await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);
        await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        host.Requests.Count.ShouldBe(2);
        host.Requests[1].Headers.ShouldNotContainKey(
            "Cookie", "the shared client keeps no jar, so the second request must arrive bare");
    }

    /// <summary>
    /// A redirect is followed, and the feed parses from where it landed.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ARedirectIsFollowedToTheFeed()
    {
        byte[] body = Encoding.UTF8.GetBytes(FeedTestData.MinimalRss);
        using LoopbackHost host = LoopbackHost.Start((request, response) =>
        {
            if (request.RawUrl == "/feed.xml")
            {
                response.ContentType = "application/rss+xml";
                response.ContentLength64 = body.Length;
                response.OutputStream.Write(body);
                return;
            }

            response.StatusCode = (int)HttpStatusCode.Found;
            response.RedirectLocation = "/feed.xml";
        });

        RssFeed feed = await RssFeed.CreateAsync(host.Uri, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        feed.Channel.Title.ShouldBe("Test Feed");
        host.Requests.Count.ShouldBe(2, "the original request and the followed redirect");
        host.Requests[1].Path.ShouldBe("/feed.xml");
    }

    private static HttpClient RealPipelineClient()
    {
#pragma warning disable CA2000 // HttpClient takes ownership of the handler and disposes it
        SocketsHttpHandler handler = new();
#pragma warning restore CA2000
        SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(handler);
        return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
    }
}