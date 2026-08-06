using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// An <see cref="HttpListener"/> on 127.0.0.1 that serves whatever a test tells it to.
/// </summary>
/// <remarks>
///     <para>
///     The counterpart to <see cref="MockHttpMessageHandler"/>, for the seams a mock handler cannot
///     reach. A mock replaces the <i>primary handler</i>, so everything beneath it — the shared
///     <see cref="HttpClient"/> singleton that binds no handler parameter at all, and
///     <see cref="SocketsHttpHandler"/> itself, whose decompression, chunked de-framing and cookie
///     policy are the things <c>ApplyArgoticHandlerDefaults</c> exists to configure — executes only
///     against a real socket. Every shared-client convenience overload in the library was at 0%
///     coverage for exactly this reason.
///     </para>
///     <para>
///     Loopback is not "the network": no DNS, no interface beyond 127.0.0.1, no third party, and
///     deterministic. The suite remains offline-safe with no conditional skips. What is served comes
///     from C# literals, never from disk, so the no-test-resources rule holds too.
///     </para>
///     <para>
///     Each instance binds its own OS-assigned port, so method-level parallel tests cannot collide —
///     and because the shared client pools connections per origin, a unique port per test also keeps
///     one test's pooled connection out of another's assertions.
///     </para>
/// </remarks>
internal sealed class LoopbackHost : IDisposable
{
    private readonly HttpListener listener;
    private readonly ConcurrentQueue<CapturedRequest> requests = new();

    private LoopbackHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    /// <summary>
    /// Gets the address the host answers on. Every path under it is served by the same responder.
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Gets the requests received so far, oldest first, captured before the responder ran.
    /// </summary>
    /// <remarks>
    ///     Headers are copied at capture time — an <see cref="HttpListenerRequest"/> is not valid after
    ///     its response is closed, so holding the request itself would hand the test a disposed object.
    /// </remarks>
    public IReadOnlyList<CapturedRequest> Requests => [.. this.requests];

    /// <summary>
    /// Starts a host whose every request is answered by the supplied responder.
    /// </summary>
    /// <param name="respond">Writes the response for one request. The host closes the response afterwards.</param>
    /// <returns>A running host. Dispose it to stop.</returns>
    public static LoopbackHost Start(Action<HttpListenerRequest, HttpListenerResponse> respond)
    {
        ArgumentNullException.ThrowIfNull(respond);

        HttpListener? listener = null;
        int port = 0;

        // Bind-probe then bind-listen is a race under parallel tests, so losing it is retried rather
        // than surfaced as a flake. Three collisions in a row on a fresh OS-assigned port would mean
        // something stranger than bad luck.
        for (int attempt = 0; ; attempt++)
        {
            port = FreePort();
            listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");

            try
            {
                listener.Start();
                break;
            }
            catch (HttpListenerException) when (attempt < 3)
            {
                listener.Close();
            }
        }

        listener.IgnoreWriteExceptions = true;

        LoopbackHost host = new(listener, new Uri($"http://127.0.0.1:{port}/"));
        _ = host.ServeAsync(respond);
        return host;
    }

    /// <summary>
    /// Starts a host serving one XML document, identically, for every request.
    /// </summary>
    /// <param name="body">The document to serve, encoded as UTF-8.</param>
    /// <param name="mediaType">The media type to declare.</param>
    /// <returns>A running host. Dispose it to stop.</returns>
    public static LoopbackHost ServingXml(string body, string mediaType = "application/xml")
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);

        return Start((_, response) =>
        {
            response.ContentType = mediaType;
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes);
        });
    }

    /// <summary>
    /// Compresses a body for a response that declares a <c>Content-Encoding</c>.
    /// </summary>
    /// <param name="body">The text to compress, encoded as UTF-8 first.</param>
    /// <param name="encoding">Either <c>gzip</c> or <c>br</c>.</param>
    /// <returns>The compressed bytes.</returns>
    public static byte[] Compress(string body, string encoding)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        using MemoryStream compressed = new();

        using (Stream compressor = encoding switch
        {
            "gzip" => new System.IO.Compression.GZipStream(compressed, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true),
            "br" => new System.IO.Compression.BrotliStream(compressed, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true),
            _ => throw new ArgumentException($"Unknown encoding '{encoding}'.", nameof(encoding)),
        })
        {
            compressor.Write(bytes);
        }

        return compressed.ToArray();
    }

    /// <inheritdoc/>
    public void Dispose() => this.listener.Close();

    private static int FreePort()
    {
        using System.Net.Sockets.Socket probe = new(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp);

        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.LocalEndPoint!).Port;
    }

    private async Task ServeAsync(Action<HttpListenerRequest, HttpListenerResponse> respond)
    {
        while (this.listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await this.listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                return; // Closed by Dispose while waiting; this is how the loop ends.
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
            foreach (string? key in context.Request.Headers.AllKeys)
            {
                if (key is not null)
                {
                    headers[key] = context.Request.Headers[key] ?? string.Empty;
                }
            }

            this.requests.Enqueue(new CapturedRequest(
                context.Request.HttpMethod, context.Request.RawUrl ?? "/", headers));

            try
            {
                respond(context.Request, context.Response);
            }
            finally
            {
                try
                {
                    context.Response.Close();
                }
                catch (ObjectDisposedException)
                {
                    // The responder closed it, or the client aborted; either way there is nothing left to do.
                }
            }
        }
    }

    /// <summary>
    /// One request as the server saw it, with the headers copied while they were still readable.
    /// </summary>
    /// <param name="Method">The HTTP method.</param>
    /// <param name="Path">The raw path requested.</param>
    /// <param name="Headers">The request headers, compared case-insensitively.</param>
    internal sealed record CapturedRequest(string Method, string Path, IReadOnlyDictionary<string, string> Headers);
}