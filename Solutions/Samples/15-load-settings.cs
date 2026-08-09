#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 15 -- Five knobs, and three nulls that do not mean "default"
//
//     dotnet run --file Solutions/Samples/15-load-settings.cs
//
// SyndicationResourceLoadSettings is the optional last argument on every Load and LoadAsync in
// the library, and most of the time you pass null and never think about it. This sample is about
// the cases where you should.
//
// It carries five settings. Two you have already met -- AutoDetectExtensions in sample 09 and
// RetrievalLimit in sample 08 -- and the other three exist because a feed you fetch is a document
// somebody else controls, arriving over a link neither of you controls. Left alone they are
// sensible. Set wrongly, two of them will hang a process and the third will corrupt text.
//
// The unifying trap is that null does not mean "use the default" on any of the three. Passing no
// settings object and passing a settings object with a null property are different requests, and
// on Timeout they are opposite requests. Section 1 is that, because it is the one that costs the
// most to learn the hard way.
// ---------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Argotic.Common;
using Argotic.Syndication;

const string FeedXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin.</description>
        <item><title>An item</title><link>https://endjin.com/blog/1</link><description>Text.</description></item>
      </channel>
    </rss>
    """;

// ---------------------------------------------------------------------------------------------
// 1. Timeout: the null that means "never give up"
//
// The property is TimeSpan?, and it is initialised to SyndicationEncodingUtility
// .DefaultRequestTimeout -- 100 seconds, inherited from the HttpWebRequest default this library
// used to be built on. So a settings object you construct and do not touch already has a deadline.
//
// Setting it to null removes the deadline. Not "resets it to the default" -- removes it. And
// because the deadline is imposed by CancellationTokenSource.CancelAfter rather than by
// HttpClient.Timeout, and the shared client is deliberately built with
// Timeout.InfiniteTimeSpan, there is nothing underneath to catch you. A null Timeout and no
// cancellation token is a load that waits for as long as the origin cares to keep the socket
// open, which for a hostile or merely broken server is indefinitely.
//
// That is the right setting for a 60 MB archive on a bad link, where any fixed deadline is a
// guess that will sometimes be wrong. It is the wrong setting for everything else. And note that
// TimeSpan.Zero is not a way to spell it -- zero cancels immediately.
//
// The deadline covers the whole load including the body, which is what the slow server below
// demonstrates: the headers arrive at once and the body dribbles out afterwards.
// ---------------------------------------------------------------------------------------------

Heading("Timeout");
Console.WriteLine($"  {"new SyndicationResourceLoadSettings().Timeout",-48} {new SyndicationResourceLoadSettings().Timeout}");
Console.WriteLine($"  {"SyndicationEncodingUtility.DefaultRequestTimeout",-48} {SyndicationEncodingUtility.DefaultRequestTimeout}");
Console.WriteLine($"  {"SharedHttpClient.Timeout",-48} {Describe(SyndicationEncodingUtility.SharedHttpClient.Timeout)}");
Console.WriteLine();

using SlowHost slow = SlowHost.Serving(FeedXml, bodyDelay: TimeSpan.FromSeconds(3));

Stopwatch clock = Stopwatch.StartNew();
try
{
    RssFeed feed = new();
    await feed.LoadAsync(
        slow.Uri,
        SyndicationEncodingUtility.SharedHttpClient,
        new SyndicationResourceLoadSettings { Timeout = TimeSpan.FromMilliseconds(500) });

    Console.WriteLine("  loaded -- which would mean the deadline did not apply to the body");
}
catch (OperationCanceledException)
{
    Console.WriteLine($"  Timeout = 500ms against a server that stalls for 3s: cancelled after {clock.ElapsedMilliseconds} ms");
    Console.WriteLine("  The headers had already arrived. The deadline covers the body too.");
}

// A second host, because the first is still part-way through writing the body it stalled on and a
// queued request would inflate the number below into something the prose does not predict.
using SlowHost slowAgain = SlowHost.Serving(FeedXml, bodyDelay: TimeSpan.FromSeconds(3));

clock.Restart();
RssFeed patient = new();
await patient.LoadAsync(
    slowAgain.Uri,
    SyndicationEncodingUtility.SharedHttpClient,
    new SyndicationResourceLoadSettings { Timeout = TimeSpan.FromSeconds(30) });

Console.WriteLine($"  Timeout = 30s against the same server: loaded in {clock.ElapsedMilliseconds} ms");

// ---------------------------------------------------------------------------------------------
// 2. MaxResponseContentLength, and the bytes it actually counts
//
// A feed is a document a stranger serves you, and nothing in HTTP obliges them to tell you how
// big it is before they start. So there is a cap, and it is per format rather than global: 8 MiB
// for a feed, 64 MiB for a sitemap or an archive, 2 MiB for a discovery fetch of an HTML page.
// Sample 04 showed how the static type you call decides which you get.
//
// Null here means "the format default", not "unbounded". If you genuinely want no cap the
// spelling is SyndicationResourceLoadSettings.Unbounded, which is long.MaxValue and is
// deliberately verbose -- you should have to mean it.
//
// The important detail is which bytes are counted: the decompressed ones. A gzip response is
// measured after decoding, because measuring before it is exactly the hole a decompression bomb
// goes through -- a few hundred kilobytes on the wire expanding to gigabytes in memory. The
// server below sends a compressed feed and the numbers show both sizes.
// ---------------------------------------------------------------------------------------------

string bigFeed = BuildFeed(itemCount: 2_000);
using CompressingHost compressed = CompressingHost.Serving(bigFeed);

Heading("MaxResponseContentLength");
Console.WriteLine($"  feed uncompressed   {Encoding.UTF8.GetByteCount(bigFeed):N0} bytes");
Console.WriteLine($"  sent over the wire  {compressed.CompressedLength:N0} bytes (gzip)");
Console.WriteLine($"  Feed default        {SyndicationContentLengthLimits.Feed:N0} bytes");
Console.WriteLine($"  Unbounded is        {SyndicationResourceLoadSettings.Unbounded:N0}");
Console.WriteLine();

// A cap below the decompressed size but well above the compressed size. If the library measured
// the wire bytes this would succeed, and the demonstration would be worthless.
long cap = Encoding.UTF8.GetByteCount(bigFeed) / 2;

try
{
    RssFeed feed = new();
    await feed.LoadAsync(
        compressed.Uri,
        SyndicationEncodingUtility.SharedHttpClient,
        new SyndicationResourceLoadSettings { MaxResponseContentLength = cap });

    Console.WriteLine($"  cap {cap:N0}: loaded -- so the wire bytes were what was counted");
}
catch (SyndicationContentTooLargeException error)
{
    Console.WriteLine($"  cap {cap:N0} (above the {compressed.CompressedLength:N0} sent, below the decompressed size)");
    Console.WriteLine($"  SyndicationContentTooLargeException: MaxBytes {error.MaxBytes:N0}, DeclaredLength {error.DeclaredLength?.ToString("N0", CultureInfo.InvariantCulture) ?? "(not declared)"}");
    Console.WriteLine("  Counted after decompression, which is the only place it is safe to count.");
}

RssFeed allowed = new();
await allowed.LoadAsync(
    compressed.Uri,
    SyndicationEncodingUtility.SharedHttpClient,
    new SyndicationResourceLoadSettings { MaxResponseContentLength = SyndicationResourceLoadSettings.Unbounded });

Console.WriteLine($"  Unbounded: loaded {allowed.Channel.Items.Count:N0} items");

// ---------------------------------------------------------------------------------------------
// 3. CharacterEncoding: the null that means "work it out"
//
// Left null, the encoding is detected -- byte-order mark first, then the XML declaration, then
// UTF-8. That is almost always right, because a well-formed XML document is required to be
// self-describing and most of them are.
//
// Setting it does not add a fallback. It overrides. A document correctly declaring
// iso-8859-1 that you load with CharacterEncoding = Encoding.UTF8 is decoded as UTF-8, and the
// bytes that are not valid UTF-8 become replacement characters. No exception; the document parses
// and the text is wrong.
//
// So this setting is for one situation only: an origin you know serves a specific encoding and
// declares it incorrectly or not at all. If you are reaching for it because a feed "looks wrong",
// check the declaration first -- you are more likely to be about to make it worse.
// ---------------------------------------------------------------------------------------------

const string Latin1Feed = """
    <?xml version="1.0" encoding="iso-8859-1"?>
    <rss version="2.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Naïve Bayes, résumés and £100</description>
        <item><title>An item</title><link>https://endjin.com/blog/1</link><description>Text.</description></item>
      </channel>
    </rss>
    """;

using SlowHost latin1 = SlowHost.Serving(Latin1Feed, TimeSpan.Zero, Encoding.Latin1, "application/rss+xml");

Heading("CharacterEncoding");
foreach (Encoding? requested in new Encoding?[] { null, Encoding.Latin1, Encoding.UTF8 })
{
    RssFeed feed = new();
    await feed.LoadAsync(
        latin1.Uri,
        SyndicationEncodingUtility.SharedHttpClient,
        new SyndicationResourceLoadSettings { CharacterEncoding = requested });

    string label = requested?.WebName ?? "null (detect)";
    Console.WriteLine($"  {label,-16} {feed.Channel.Description}");
}

Console.WriteLine();
Console.WriteLine("  The document declares iso-8859-1 and is served as iso-8859-1. Detection and the");
Console.WriteLine("  matching override agree; forcing UTF-8 overrides a correct declaration.");

// ---------------------------------------------------------------------------------------------
// 4. All five, and what ToString will tell you
//
// SyndicationResourceLoadSettings.ToString spells the unset cases out as "detect", "none" and
// "default" rather than as an empty value, which makes it worth logging when a load behaves in a
// way you did not expect. It is the quickest way to find out that the settings object you passed
// is not the one you thought you built.
// ---------------------------------------------------------------------------------------------

Heading("The whole object");
Console.WriteLine($"  default:  {new SyndicationResourceLoadSettings()}");
Console.WriteLine();
Console.WriteLine($"  no deadline, no cap: {new SyndicationResourceLoadSettings { Timeout = null, MaxResponseContentLength = SyndicationResourceLoadSettings.Unbounded }}");

Console.WriteLine();
Console.WriteLine("Next: 16-discovery.cs -- finding a feed when all you have is a homepage.");

static string Describe(TimeSpan timeout) =>
    timeout == Timeout.InfiniteTimeSpan ? "Timeout.InfiniteTimeSpan (no deadline, on purpose)" : timeout.ToString();

static string BuildFeed(int itemCount)
{
    StringBuilder builder = new();
    builder.Append("""
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0"><channel><title>endjin blog</title><link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin.</description>
        """);

    for (int index = 0; index < itemCount; index++)
    {
        builder.Append(CultureInfo.InvariantCulture, $"<item><title>Post number {index}</title><link>https://endjin.com/blog/{index}</link><description>A post about the {index}th thing, padded so the document is worth compressing.</description></item>");
    }

    builder.Append("</channel></rss>");

    return builder.ToString();
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>
/// Serves one document after an optional delay between the headers and the body, so a deadline can
/// be shown to cover the whole load rather than only the response headers.
/// </summary>
internal sealed class SlowHost : IDisposable
{
    private readonly HttpListener listener;

    private SlowHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    public Uri Uri { get; }

    public static SlowHost Serving(string body, TimeSpan bodyDelay, Encoding? encoding = null, string mediaType = "application/rss+xml")
    {
        // CA2000 cannot see that ownership of the listener passes to the host, and of the host to
        // the caller, which disposes it with a `using`. The pattern it wants is a try/finally with a
        // nulled local -- four lines of ceremony around a return.
#pragma warning disable CA2000
        HttpListener listener = Loopback.Start(out Uri uri);
        SlowHost host = new(listener, uri);
#pragma warning restore CA2000
        _ = host.ServeAsync((encoding ?? Encoding.UTF8).GetBytes(body), mediaType, bodyDelay);

        return host;
    }

    public void Dispose() => this.listener.Close();

    private async Task ServeAsync(byte[] bytes, string mediaType, TimeSpan bodyDelay)
    {
        while (this.listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await this.listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            using HttpListenerResponse response = context.Response;
            response.ContentType = mediaType;
            response.ContentLength64 = bytes.Length;

            // Flush the headers, then stall. A client that only bounded the header wait would think
            // this request had succeeded by now.
            await response.OutputStream.FlushAsync();

            if (bodyDelay > TimeSpan.Zero)
            {
                await Task.Delay(bodyDelay);
            }

            try
            {
                await response.OutputStream.WriteAsync(bytes);
            }
            catch (HttpListenerException)
            {
                // The client gave up and closed the socket, which is the outcome being demonstrated.
            }
        }
    }
}

/// <summary>
/// Serves one document gzip-encoded, so the difference between wire bytes and decompressed bytes is
/// visible to the caller.
/// </summary>
internal sealed class CompressingHost : IDisposable
{
    private readonly HttpListener listener;

    private CompressingHost(HttpListener listener, Uri uri, byte[] payload)
    {
        this.listener = listener;
        this.Uri = uri;
        this.CompressedLength = payload.Length;
        _ = this.ServeAsync(payload);
    }

    public Uri Uri { get; }

    public int CompressedLength { get; }

    public static CompressingHost Serving(string body)
    {
        using MemoryStream buffer = new();
        using (GZipStream gzip = new(buffer, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(Encoding.UTF8.GetBytes(body));
        }

#pragma warning disable CA2000 // Ownership transfers to the caller; see SlowHost.Serving.
        HttpListener listener = Loopback.Start(out Uri uri);

        return new CompressingHost(listener, uri, buffer.ToArray());
#pragma warning restore CA2000
    }

    public void Dispose() => this.listener.Close();

    private async Task ServeAsync(byte[] payload)
    {
        while (this.listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await this.listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            using HttpListenerResponse response = context.Response;
            response.ContentType = "application/rss+xml";
            response.AddHeader("Content-Encoding", "gzip");
            response.ContentLength64 = payload.Length;

            try
            {
                await response.OutputStream.WriteAsync(payload);
            }
            catch (HttpListenerException)
            {
                // The client abandoned the response once it exceeded the cap.
            }
        }
    }
}

/// <summary>Binds an <see cref="HttpListener"/> to an OS-assigned port on 127.0.0.1.</summary>
internal static class Loopback
{
    public static HttpListener Start(out Uri uri)
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        uri = new Uri($"http://127.0.0.1:{port}/");

        return listener;
    }
}