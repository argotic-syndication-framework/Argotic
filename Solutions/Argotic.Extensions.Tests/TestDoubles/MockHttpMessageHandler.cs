using System.Net;
using System.Text;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// An <see cref="HttpMessageHandler"/> that answers every request from a delegate the test supplies.
/// </summary>
/// <remarks>
///     <para>
///     <b>Reach for this first.</b> Anything the library exposes through an <see cref="HttpClient"/>
///     parameter can be driven from here, and nothing leaves the process: no socket is opened, no port
///     is bound, and the test is as fast and as deterministic as a method call. Every
///     <c>LoadAsync</c>/<c>CreateAsync</c> overload that takes a client, and both protocol clients
///     (<c>TrackbackClient</c> and <c>XmlRpcClient</c>), are reachable this way.
///     </para>
///     <para>
///     The delegate form is the general instrument: it receives the outgoing
///     <see cref="HttpRequestMessage"/>, so a test can assert on the method, the URI, the headers or
///     the body it was about to send as well as decide what comes back. The four factories below cover
///     the common answers.
///     </para>
///     <para>
///     <b>What it structurally cannot see.</b> A mock replaces the <i>primary handler</i>, so nothing
///     underneath it runs. Two seams therefore lie outside it, and
///     <see cref="LoopbackHost"/> exists for exactly those: the convenience overloads that bind
///     <c>SyndicationEncodingUtility.SharedHttpClient</c> and take no handler parameter at all, and the
///     behaviour of <see cref="SocketsHttpHandler"/> itself — decompression, chunked de-framing, cookie
///     policy — which only happens against a real socket.
///     </para>
/// </remarks>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="sendAsync">
    ///     Answers one request. It receives the outgoing message, so a test that needs to assert on what
    ///     was sent captures it here.
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="sendAsync"/> is <see langword="null"/>.</exception>
    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _sendAsync(request, cancellationToken);

    /// <summary>
    /// Creates a handler that answers every request with the same body and a 200.
    /// </summary>
    /// <param name="content">The body to serve. Encoded as UTF-8.</param>
    /// <param name="contentType">The media type to declare. The default is <c>application/xml</c>.</param>
    /// <returns>A new handler. The caller owns it.</returns>
    /// <remarks>
    ///     The workhorse: pair it with a literal from <see cref="FeedTestData"/> and any resource type
    ///     can be fetched and parsed without a network. A token cancelled before the call is observed,
    ///     so this is also usable in cancellation tests where the cancellation precedes the request.
    /// </remarks>
    public static MockHttpMessageHandler WithContent(string content, string contentType = "application/xml")
    {
        return new MockHttpMessageHandler((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            });
        });
    }

    /// <summary>
    /// Creates a handler that waits before answering, and abandons the wait if the token is cancelled.
    /// </summary>
    /// <param name="delay">How long to wait before responding. Make it comfortably longer than the timeout under test.</param>
    /// <param name="content">The body to serve if the wait completes. Encoded as UTF-8, declared <c>application/xml</c>.</param>
    /// <returns>A new handler. The caller owns it.</returns>
    /// <remarks>
    ///     The instrument for timeout and cancellation tests. The delay is awaited <i>with</i> the
    ///     request's token, so a cancellation raised while it is pending surfaces as a
    ///     <see cref="TaskCanceledException"/> rather than being swallowed until the delay elapses — which
    ///     is what makes such a test finish in milliseconds instead of waiting out <paramref name="delay"/>.
    /// </remarks>
    public static MockHttpMessageHandler WithDelay(TimeSpan delay, string content)
    {
        return new MockHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(delay, ct);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/xml")
            };
        });
    }

    /// <summary>
    /// Creates a handler that answers every request with a 404 carrying a plain-text body.
    /// </summary>
    /// <returns>A new handler. The caller owns it.</returns>
    /// <remarks>
    ///     Use this to assert that a failed fetch surfaces as an <see cref="HttpRequestException"/>
    ///     rather than an empty resource. The status is what matters; the body is present so that a
    ///     caller which reads before checking the status has something to read.
    /// </remarks>
    public static MockHttpMessageHandler WithNotFound()
    {
        return new MockHttpMessageHandler((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not Found", Encoding.UTF8, "text/plain")
            });
        });
    }

    /// <summary>
    /// Creates a handler that fails every request with the supplied exception.
    /// </summary>
    /// <param name="exception">The exception to raise. Any type; the handler does not wrap it.</param>
    /// <returns>A new handler. The caller owns it.</returns>
    /// <remarks>
    ///     Models a transport failure — DNS, connect, or TLS — rather than an HTTP error status, which
    ///     is <see cref="WithNotFound"/>'s job. The exception is thrown from the handler body rather
    ///     than returned as a faulted task, so it propagates out of the caller's <c>await</c>
    ///     unaltered and the test can assert on the exact type it expects.
    /// </remarks>
    public static MockHttpMessageHandler WithException(Exception exception)
    {
        return new MockHttpMessageHandler((_, _) =>
        {
            throw exception;
        });
    }
}