using System.Net;

namespace Argotic.Examples;

/// <summary>
/// Serves one file from <c>SampleData</c> over loopback HTTP, for the examples whose subject is a <see cref="Uri"/>.
/// </summary>
/// <remarks>
///     <para>
///     Most <c>Uri</c> examples point at a live feed, which is the honest demonstration: that is what a
///     caller does. <see cref="Argotic.Syndication.AtomEntry"/> cannot be shown that way, because its
///     overloads take an RFC 4287 §2 <i>stand-alone entry document</i> — <c>&lt;entry&gt;</c> as the
///     document element — and essentially nothing on the open web serves one. They are Atom Publishing
///     Protocol member resources, which live behind authentication.
///     </para>
///     <para>
///     These examples used to point at <c>endjin.com/atom.xml</c>, which is a <c>&lt;feed&gt;</c>. They
///     reported success and produced an entry with an empty title and a <c>DateTime.MinValue</c>
///     timestamp, because the adapter looked for an <c>&lt;entry&gt;</c> child, did not find one, and
///     returned. That is now a <c>FormatException</c>, so the examples had to serve a document of the
///     right shape instead of quietly demonstrating nothing.
///     </para>
///     <para>
///     Loopback rather than a local file because <c>HttpClient</c> does not implement the <c>file</c>
///     scheme, and the point of these three is the <c>Uri</c> overloads.
///     </para>
/// </remarks>
internal sealed class SampleHost : IDisposable
{
    private readonly HttpListener listener;
    private readonly CancellationTokenSource shutdown = new();

    private SampleHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    /// <summary>
    /// Gets the address the sample document is served from.
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Starts serving the named sample file until the host is disposed.
    /// </summary>
    /// <param name="file">The file in <c>SampleData</c> to serve.</param>
    /// <param name="mediaType">The media type to serve it as.</param>
    /// <returns>A running host. Dispose it to stop.</returns>
    public static SampleHost Serving(Spectre.IO.FilePath file, string mediaType)
    {
        ArgumentNullException.ThrowIfNull(file);

        byte[] body = File.ReadAllBytes(file.FullPath);

        // Port 0 asks the OS for a free one, so concurrent runs cannot collide.
        HttpListener listener = new();
        int port = SampleHost.FreePort();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        SampleHost host = new(listener, new Uri($"http://127.0.0.1:{port}/{file.GetFilename()}"));
        _ = host.ServeAsync(body, mediaType);
        return host;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.shutdown.Cancel();
        this.listener.Close();
        this.shutdown.Dispose();
    }

    private static int FreePort()
    {
        using System.Net.Sockets.Socket probe = new(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp);

        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.LocalEndPoint!).Port;
    }

    private async Task ServeAsync(byte[] body, string mediaType)
    {
        while (!this.shutdown.IsCancellationRequested)
        {
            HttpListenerContext context;

            try
            {
                context = await this.listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                // The listener was closed by Dispose while waiting; that is how this loop ends.
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            context.Response.ContentType = mediaType;
            context.Response.ContentLength64 = body.Length;
            await context.Response.OutputStream.WriteAsync(body, this.shutdown.Token).ConfigureAwait(false);
            context.Response.Close();
        }
    }
}