#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 14 -- The first sample that opens a socket, and it opens it to itself
//
//     dotnet run --file Solutions/Samples/14-fetching.cs
//
// Thirteen samples have loaded documents from strings. Real feeds arrive over HTTP, and that
// changes the questions: which client makes the request, what headers does it send, when does it
// give up, and what do you get to see while it happens.
//
// Every sample from here to 21 answers those against an HttpListener bound to 127.0.0.1 that this
// file starts, serves and shuts down. That is not a compromise for the sake of a demonstration --
// it is what makes these samples runnable in a build. A sample that fetches a live origin is a
// sample that fails when somebody else's site is down, and one that fails for reasons you cannot
// fix teaches people to ignore failures. Loopback is not the network: no DNS, no third party, no
// interface beyond the local one, and an OS-assigned port so that concurrent runs cannot collide.
//
// The listener here also records what it was sent, which turns the second half of this sample
// from an assertion about headers into a printout of them.
// ---------------------------------------------------------------------------------------------

using System.Collections.Specialized;
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
        <description>Technical writing from endjin on .NET, data, analytics and AI.</description>
        <item>
          <title>The GenAI Reality Check: New Instrument, Same Orchestra</title>
          <link>https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra</link>
          <description>A new instrument in an orchestra that already existed.</description>
          <pubDate>Thu, 14 May 2026 08:54:29 GMT</pubDate>
        </item>
      </channel>
    </rss>
    """;

using LoopbackHost host = LoopbackHost.Serving(FeedXml, "application/rss+xml");

Heading("Serving from");
Console.WriteLine($"  {host.Uri}");

// ---------------------------------------------------------------------------------------------
// 1. CreateAsync and LoadAsync differ in exactly one thing
//
// CreateAsync is a static that returns a loaded resource. LoadAsync is an instance method that
// fills the resource you already hold. That is the whole difference, and it matters for exactly
// one reason: to subscribe to the Loaded event you must own the instance before the fetch starts.
//
// If you do not need the event -- and most callers do not -- CreateAsync is the better call,
// because it makes the "half-constructed feed" state unrepresentable. You never hold an RssFeed
// that has been declared but not filled.
//
// Neither has a synchronous twin. There is no Create(Uri) and no Load(Uri), which is deliberate:
// the only synchronous Load overloads take something you already have in memory, and a blocking
// network call hidden behind a method that looks like the others is how thread pools starve.
// ---------------------------------------------------------------------------------------------

RssFeed created = await RssFeed.CreateAsync(host.Uri);

Heading("CreateAsync: one call");
Console.WriteLine($"  {created.Channel.Title}, {created.Channel.Items.Count} item");

RssFeed loading = new();
loading.Loaded += (sender, args) =>
{
    // The event fires after parsing and before LoadAsync returns. Data is the navigator the parse
    // ran over, so a handler can inspect elements the object model dropped -- which, after sample
    // 08, you know is the only way to find out what those were.
    Console.WriteLine($"  Loaded fired: source {args.Source}, navigator {(args.Data is null ? "absent" : "present")}");
};

await loading.LoadAsync(host.Uri);

Heading("LoadAsync: subscribe first");
Console.WriteLine($"  {loading.Channel.Title}, {loading.Channel.Items.Count} item");

// ---------------------------------------------------------------------------------------------
// 2. Which client, and why you should usually supply one
//
// With no client argument the library uses SyndicationEncodingUtility.SharedHttpClient: one
// process-wide instance over a SocketsHttpHandler, created lazily. That is the right default --
// a fresh HttpClient per request exhausts sockets, and a fresh one per feed is worse.
//
// Every LoadAsync and CreateAsync also takes an HttpClient, and in an application you should pass
// one. Not because the shared client is bad, but because a client you resolve from
// IHttpClientFactory is one you can configure, instrument and give a Polly policy to, and the
// shared one is none of those. Sample 21 wires that up properly.
//
// One detail that surprises people: the shared client's Timeout is Timeout.InfiniteTimeSpan, and
// so is the one the DI registration configures. That is not an oversight and not a missing
// deadline -- deadlines in this library come from CancellationTokenSource.CancelAfter, driven by
// SyndicationResourceLoadSettings.Timeout, and an HttpClient.Timeout underneath could only cut a
// longer deadline short and report it as the wrong kind of failure. Sample 15 is the settings.
// ---------------------------------------------------------------------------------------------

Heading("Clients");
Console.WriteLine($"  SharedHttpClient.Timeout   {Describe(SyndicationEncodingUtility.SharedHttpClient.Timeout)}");

using HttpClient mine = new();
mine.DefaultRequestHeaders.Add("X-Sample", "14-fetching");

RssFeed viaMyClient = await RssFeed.CreateAsync(host.Uri, mine);
Console.WriteLine($"  via a caller-supplied client: {viaMyClient.Channel.Items.Count} item, and the server saw X-Sample = {host.LastRequest?["X-Sample"] ?? "(nothing)"}");

// ---------------------------------------------------------------------------------------------
// 3. Headers, and a record that refuses rather than drops
//
// SyndicationRequestOptions is how you shape the request without owning the client: an Accept, a
// User-Agent, a Referer, and a dictionary for anything else. It is a record with init-only
// members, so it is cheap to build one per call and impossible to mutate one somebody else holds.
//
// The behaviour worth knowing is what it does with a value it cannot send. It throws. A Referer
// that is not an absolute http or https URI raises a FormatException naming the value, rather
// than being quietly omitted -- and that choice is right, because a dropped header is
// indistinguishable at the client from a server that ignored it. You would be debugging the
// server.
//
// The server below prints what it actually received, which is the only way to be sure any of this
// arrived.
// ---------------------------------------------------------------------------------------------

SyndicationRequestOptions options = new()
{
    Accept = "application/rss+xml, application/xml;q=0.9",
    UserAgent = "endjin-sample/1.0 (+https://endjin.com/)",
    Referer = "https://endjin.com/what-we-think/",
    CustomHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["X-Correlation-Id"] = "b5f0c2e1-7a34-4d19-9c88-3f2a6d4e1b07",
    },
};

_ = await RssFeed.CreateAsync(host.Uri, SyndicationEncodingUtility.SharedHttpClient, settings: null, requestOptions: options);

Heading("What the server was sent");
foreach (string header in new[] { "Accept", "User-Agent", "Referer", "X-Correlation-Id" })
{
    Console.WriteLine($"  {header,-18} {host.LastRequest?[header] ?? "(absent)"}");
}

Heading("A value it cannot send");
try
{
    SyndicationRequestOptions broken = new() { Referer = "/what-we-think/" };
    using HttpRequestMessage request = new(HttpMethod.Get, host.Uri);
    broken.ApplyTo(request);
}
catch (FormatException error)
{
    Console.WriteLine($"  FormatException: {error.Message}");
    Console.WriteLine("  A relative Referer is refused rather than omitted, so you debug the right thing.");
}

// ---------------------------------------------------------------------------------------------
// 4. What happens when the origin is not helpful
//
// The last thing to know before sample 15 is what failure looks like. Every LoadAsync funnels
// through EnsureSuccessStatusCode, so anything that is not a 2xx becomes an
// HttpRequestException -- including a 304, which is why conditional requests needed a separate
// API and why sample 17 exists at all.
//
// A 404 carrying a perfectly good error page is still a 404, and you get the exception rather
// than a feed parsed out of the error page. That is worth stating because the alternative -- try
// to parse whatever came back -- is what several older libraries do.
// ---------------------------------------------------------------------------------------------

using LoopbackHost missing = LoopbackHost.Serving("<html><body>Not found</body></html>", "text/html", HttpStatusCode.NotFound);

Heading("A 404 with a body");
try
{
    _ = await RssFeed.CreateAsync(missing.Uri);
    Console.WriteLine("  loaded -- which would mean the status code was ignored");
}
catch (HttpRequestException error)
{
    Console.WriteLine($"  HttpRequestException, StatusCode {error.StatusCode}");
    Console.WriteLine("  The body was never offered to the parser.");
}

Console.WriteLine();
Console.WriteLine("Next: 15-load-settings.cs -- five knobs, and three nulls that do not mean 'default'.");

static string Describe(TimeSpan timeout) =>
    timeout == Timeout.InfiniteTimeSpan
        ? "Timeout.InfiniteTimeSpan -- deadlines come from SyndicationResourceLoadSettings"
        : timeout.ToString();

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>
/// An <see cref="HttpListener"/> on 127.0.0.1 serving one document, and recording the headers of the
/// last request it answered.
/// </summary>
/// <remarks>
///     Every networked sample in this directory inlines its own copy of this rather than sharing one.
///     That is deliberate: a shared helper would make these files depend on each other, and a sample
///     you cannot copy out of the repository and run on its own is not a single-file sample. The
///     duplication is the price of that property, and it is worth paying.
/// </remarks>
internal sealed class LoopbackHost : IDisposable
{
    private readonly HttpListener listener;

    private LoopbackHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    /// <summary>Gets the address the host answers on. Every path under it is served the same document.</summary>
    public Uri Uri { get; }

    /// <summary>Gets the headers of the most recent request, or null if none has arrived.</summary>
    public NameValueCollection? LastRequest { get; private set; }

    public static LoopbackHost Serving(string body, string mediaType, HttpStatusCode status = HttpStatusCode.OK)
    {
        // Port 0 asks the OS for a free one, so concurrent runs of this directory cannot collide.
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        // CA2000 cannot see that ownership transfers to the caller, which disposes the host with a
        // `using`. The alternative it wants -- a try/finally with a nulled local -- would be four
        // lines of ceremony around a return statement.
#pragma warning disable CA2000
        LoopbackHost host = new(listener, new Uri($"http://127.0.0.1:{port}/"));
#pragma warning restore CA2000
        _ = host.ServeAsync(Encoding.UTF8.GetBytes(body), mediaType, status);

        return host;
    }

    public void Dispose() => this.listener.Close();

    private async Task ServeAsync(byte[] bytes, string mediaType, HttpStatusCode status)
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
                // Disposal closes the listener out from under the pending GetContextAsync. That is
                // how this loop is meant to end, not a failure to report.
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            this.LastRequest = context.Request.Headers;

            using HttpListenerResponse response = context.Response;
            response.StatusCode = (int)status;
            response.ContentType = mediaType;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
        }
    }
}