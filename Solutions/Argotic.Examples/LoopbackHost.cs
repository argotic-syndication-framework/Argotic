using System.Net;
using System.Text;

namespace Argotic.Examples;

/// <summary>
/// An <see cref="HttpListener"/> on 127.0.0.1 that serves one document, so an example can demonstrate
/// a <c>Uri</c> load without depending on a live origin.
/// </summary>
/// <remarks>
///     <para>
///     Loopback is not "the network": no DNS, no interface beyond 127.0.0.1, no third party. An example
///     that serves its own document runs inside <c>--skip-network</c> by construction, which is the
///     whole point of using it here. The counterpart in the test project,
///     <c>Argotic.Extensions.Tests/TestDoubles/LoopbackHost.cs</c>, carries request capture and
///     compression helpers that no example needs; this is the same idea with only the parts an example
///     uses.
///     </para>
///     <para>
///     It exists because of a regression that shipped behind a green gate.
///     <see cref="Core.Sitemap.SitemapIndexExample"/> fetched <c>endjin.com/sitemap.xml</c> and called it
///     a sitemap index; that URL is a <c>urlset</c>, and endjin publishes no index at all. The mismatch
///     was silent until <see cref="Argotic.Syndication.SitemapIndex"/> began checking the format, and
///     then it was invisible anyway, because a <c>[RequiresNetwork]</c> example never runs in the gate.
///     Serving the document is what puts the example somewhere a build can see it.
///     </para>
/// </remarks>
internal sealed class LoopbackHost : IDisposable
{
    private readonly HttpListener listener;

    private LoopbackHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    /// <summary>
    /// Gets the address the host answers on. Every path under it is served the same document.
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Starts a host serving one XML document, identically, for every request.
    /// </summary>
    /// <param name="body">The document to serve, encoded as UTF-8.</param>
    /// <param name="mediaType">The media type to declare.</param>
    /// <returns>A running host. Dispose it to stop.</returns>
    public static LoopbackHost ServingXml(string body, string mediaType = "application/xml")
    {
        ArgumentNullException.ThrowIfNull(body);

        byte[] bytes = Encoding.UTF8.GetBytes(body);
        HttpListener listener = new();
        int port = FreePort();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        LoopbackHost host = new(listener, new Uri($"http://127.0.0.1:{port}/"));
        _ = host.ServeAsync(bytes, mediaType);
        return host;
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

    private async Task ServeAsync(byte[] bytes, string mediaType)
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
                // Disposal closes the listener out from under the pending GetContextAsync. That is how
                // the loop is meant to end, not a failure to report.
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            using HttpListenerResponse response = context.Response;
            response.ContentType = mediaType;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
        }
    }
}