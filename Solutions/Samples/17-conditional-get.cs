#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 17 -- Polling 400 feeds without downloading 400 feeds
//
//     dotnet run --file Solutions/Samples/17-conditional-get.cs
//
// A feed reader is a polling loop. It has a list of addresses and it visits them on a schedule,
// and almost every visit finds a document identical to the one it found last time -- a blog that
// publishes twice a week, polled hourly, has changed on roughly one visit in eighty.
//
// Downloading the other seventy-nine is the single largest waste in the whole business, and HTTP
// has had the answer since 1997. You remember what the server told you about the version you
// have, you send it back, and the server either says "still that one" in a couple of hundred
// bytes or sends the new document. That is a conditional GET, and this sample is how to make one
// with this library.
//
// It is also the one place where the uniform API shape from sample 01 does not apply, and the
// reason is worth understanding before the code.
// ---------------------------------------------------------------------------------------------

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Argotic.Common;
using Argotic.Syndication;

// ---------------------------------------------------------------------------------------------
// 1. Why this is a different method
//
// Every LoadAsync in this library funnels through EnsureSuccessStatusCode, and 304 Not Modified
// is not a success status. So the ordinary load path cannot express a conditional request: the
// outcome you are hoping for would arrive as an HttpRequestException.
//
// That is not a defect to route around either, because the shapes genuinely differ. LoadAsync
// fills a resource you already hold; a not-modified response has no resource in it at all, and a
// method that fills an instance has no way to say "there was nothing to fill it with, and that is
// good news". So conditional loading is a separate static that returns a result object:
//
//     ConditionalLoadResult<TResource>
//         WasModified   false when the server said 304
//         Resource      the loaded resource, or null
//         Validators    what the server said about this version -- keep these
//         StatusCode    what actually came back
//
// The type parameter is constrained to ISyndicationResource with a public parameterless
// constructor, which every resource type in the library satisfies. So this works for sitemaps and
// OPML documents as well as feeds -- note the maxResponseContentLength parameter, which is where
// you pass SyndicationContentLengthLimits.Sitemap when TResource is a Sitemap.
// ---------------------------------------------------------------------------------------------

const string Original = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin on .NET, data, analytics and AI.</description>
        <item><title>The GenAI Reality Check</title><link>https://endjin.com/blog/1</link><description>A post.</description></item>
      </channel>
    </rss>
    """;

const string Updated = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin on .NET, data, analytics and AI.</description>
        <item><title>Writing Effective Copilot Instructions</title><link>https://endjin.com/blog/2</link><description>A newer post.</description></item>
        <item><title>The GenAI Reality Check</title><link>https://endjin.com/blog/1</link><description>A post.</description></item>
      </channel>
    </rss>
    """;

using ValidatingHost host = ValidatingHost.Serving(Original, etag: "\"v1\"");

Heading("First poll: nothing remembered yet");
ConditionalLoadResult<RssFeed> first = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
    host.Uri,
    SyndicationValidators.None,
    SyndicationEncodingUtility.SharedHttpClient);

Report(first, host);

// ---------------------------------------------------------------------------------------------
// 2. The second poll, and the rule that quietly breaks pollers
//
// Hand back the validators that came out of the last result and the server can answer 304. That
// much is obvious. What is not obvious, and what breaks real pollers, is *which* validators to
// keep.
//
//     Store what came back. Never re-send what you sent.
//
// A server is entitled to issue a fresh entity tag on a 304. It is saying "the version you hold
// is current, and this is now its name" -- and a CDN with several nodes, or an origin that
// regenerates tags on deploy, does this routinely without the document changing at all. A client
// that keeps re-sending the tag it first received is revalidating against a name the origin has
// stopped recognising, so it gets a 200 and the whole body.
//
// That failure is nasty because it looks like success. Nothing errors. You simply download every
// feed every time, and if your reader treats "the document arrived" as "the document changed",
// every item shows as new. The bug appears to be in your change detection, and it is not.
//
// Below, both loops poll the same unchanged document against a server that rotates its tag on
// every response. One stores what came back; the other re-sends its first tag. Watch the status
// codes diverge.
// ---------------------------------------------------------------------------------------------

// One host each, because the tag rotates on every response: two pollers sharing a server would
// each invalidate the other's tag and both would look broken. Same document, same behaviour, two
// independent conversations.
using ValidatingHost goodHost = ValidatingHost.Serving(Original, etag: "\"v1\"");
using ValidatingHost badHost = ValidatingHost.Serving(Original, etag: "\"v1\"");

SyndicationValidators storing = (await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
    goodHost.Uri, SyndicationValidators.None, SyndicationEncodingUtility.SharedHttpClient)).Validators;

SyndicationValidators stale = (await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
    badHost.Uri, SyndicationValidators.None, SyndicationEncodingUtility.SharedHttpClient)).Validators;

Heading("Two pollers, same feed, same server behaviour");
Console.WriteLine("  poll   storing what came back      re-sending the first tag");

for (int poll = 2; poll <= 4; poll++)
{
    ConditionalLoadResult<RssFeed> good = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
        goodHost.Uri, storing, SyndicationEncodingUtility.SharedHttpClient);

    ConditionalLoadResult<RssFeed> bad = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
        badHost.Uri, stale, SyndicationEncodingUtility.SharedHttpClient);

    Console.WriteLine($"  {poll}      {(int)good.StatusCode} {good.StatusCode,-22} {(int)bad.StatusCode} {bad.StatusCode}");

    // The single most important line in this sample.
    storing = good.Validators;

    // And the mistake it prevents: `stale` is never reassigned.
}

Console.WriteLine();
Console.WriteLine("  The document never changed once. The first poller kept matching and transferred no");
Console.WriteLine("  body at all; the second matched once, fell behind the rotation, and refetched the");
Console.WriteLine("  whole feed from then on -- with nothing anywhere to say it had gone wrong.");

// Now somebody actually publishes. A correctly-behaved poller notices exactly once.
goodHost.Update(Updated, etag: "\"v2\"");

Heading("And when it really does change");
ConditionalLoadResult<RssFeed> changed = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
    goodHost.Uri, storing, SyndicationEncodingUtility.SharedHttpClient);

Report(changed, goodHost);
storing = changed.Validators;

ConditionalLoadResult<RssFeed> settled = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
    goodHost.Uri, storing, SyndicationEncodingUtility.SharedHttpClient);

Report(settled, goodHost);

SyndicationValidators validators = settled.Validators;

// ---------------------------------------------------------------------------------------------
// 3. What it saves
//
// The numbers below are for one feed over four polls on a document of a few hundred bytes, so
// they are small. Scale them: a reader with 400 subscriptions polling hourly makes 9,600 requests
// a day. At a realistic 40 KB per feed that is 384 MB fetched to discover that, on a good day,
// perhaps a hundred documents changed.
//
// With conditional requests the same day costs a few hundred kilobytes of headers plus the
// hundred documents that actually moved. That is not an optimisation; it is the difference
// between a well-behaved client and one that gets rate-limited.
// ---------------------------------------------------------------------------------------------

Heading("Bytes served, per poller");
Console.WriteLine($"  {"",-28} requests  200s  304s  body bytes");
Console.WriteLine($"  {"storing what came back",-28} {goodHost.Requests,8}  {goodHost.FullResponses,4}  {goodHost.NotModifiedResponses,4}  {goodHost.BytesServed,10:N0}");
Console.WriteLine($"  {"re-sending the first tag",-28} {badHost.Requests,8}  {badHost.FullResponses,4}  {badHost.NotModifiedResponses,4}  {badHost.BytesServed,10:N0}");

// ---------------------------------------------------------------------------------------------
// 4. The lower-level door, and when to use it
//
// SyndicationDiscoveryUtility.ConditionalGetAsync is the same conversation without the parsing:
// it returns a ConditionalGetResult carrying WasModified, the validators, and a live response
// stream you can read yourself.
//
// Two differences decide which to reach for. ConditionalGetResult is IDisposable and hands you an
// undecoded stream, so it is the right tool when the thing you are polling is not a syndication
// resource at all -- a JSON endpoint, an image, an arbitrary file. And it applies no size limit,
// precisely because it does not know what you are fetching; LoadIfModifiedAsync does, because it
// does.
//
// For polling feeds, use LoadIfModifiedAsync. For polling anything else, this.
// ---------------------------------------------------------------------------------------------

Heading("The lower-level door");
using ConditionalGetResult raw = await SyndicationDiscoveryUtility.ConditionalGetAsync(
    goodHost.Uri,
    validators,
    SyndicationEncodingUtility.SharedHttpClient);

Console.WriteLine($"  WasModified   {raw.WasModified}");
Console.WriteLine($"  StatusCode    {raw.StatusCode}");
Console.WriteLine($"  ETag          {raw.ETag ?? "(none)"}");
Console.WriteLine($"  ContentType   {raw.ContentType ?? "(none)"}");
Console.WriteLine("  and a stream you read yourself, with no size limit -- because it cannot know what you asked for.");

Console.WriteLine();
Console.WriteLine("Next: 18-atom-publishing.cs -- discovering where to POST, and what a draft is.");

static void Report(ConditionalLoadResult<RssFeed> result, ValidatingHost host)
{
    string body = result.WasModified
        ? $"{result.Resource!.Channel.Items.Count} items"
        : "no body at all";

    Console.WriteLine($"    {(int)result.StatusCode} {result.StatusCode,-12} WasModified {result.WasModified,-6} Resource {(result.Resource is null ? "null" : "present"),-8} {body}");
    Console.WriteLine($"    validators now  ETag {result.Validators.ETag ?? "(none)"}, LastModified {result.Validators.LastModified?.ToString("R", CultureInfo.InvariantCulture) ?? "(none)"}");
    Console.WriteLine($"    server sent     If-None-Match {host.LastIfNoneMatch ?? "(nothing)"}");
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>
/// A loopback host that honours If-None-Match, counts what it served, and can change both the
/// document and its ETag part-way through -- which is what makes section 2 a demonstration rather
/// than an assertion.
/// </summary>
internal sealed class ValidatingHost : IDisposable
{
    private readonly HttpListener listener;
    private readonly Lock gate = new();
    private byte[] body;
    private string baseTag;
    private string etag;
    private int revision;

    private ValidatingHost(HttpListener listener, Uri uri, byte[] body, string etag)
    {
        this.listener = listener;
        this.Uri = uri;
        this.body = body;
        this.baseTag = etag;
        this.etag = etag;
    }

    public Uri Uri { get; }

    public int Requests { get; private set; }

    public int FullResponses { get; private set; }

    public int NotModifiedResponses { get; private set; }

    public long BytesServed { get; private set; }

    /// <summary>Gets what a fetch-every-time client would have transferred, minus what this one did.</summary>
    public long BytesSaved { get; private set; }

    public string? LastIfNoneMatch { get; private set; }

    public static ValidatingHost Serving(string document, string etag)
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        // CA2000 cannot see that ownership passes to the caller, which disposes with `using`.
#pragma warning disable CA2000
        ValidatingHost host = new(listener, new Uri($"http://127.0.0.1:{port}/"), Encoding.UTF8.GetBytes(document), etag);
#pragma warning restore CA2000
        _ = host.ServeAsync();

        return host;
    }

    /// <summary>Issues the next entity tag for the current document and remembers it.</summary>
    private string Rotate()
    {
        lock (this.gate)
        {
            this.revision++;
            this.etag = $"{this.baseTag.Trim('"')}-r{this.revision.ToString(CultureInfo.InvariantCulture)}";

            return $"\"{this.etag}\"";
        }
    }

    /// <summary>Publishes a new document under a new entity tag.</summary>
    public void Update(string document, string etag)
    {
        lock (this.gate)
        {
            this.body = Encoding.UTF8.GetBytes(document);
            this.baseTag = etag;
            this.etag = etag;
            this.revision = 0;
        }
    }

    public void Dispose() => this.listener.Close();

    private async Task ServeAsync()
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

            byte[] currentBody;
            string currentEtag;
            lock (this.gate)
            {
                currentBody = this.body;
                currentEtag = this.etag;
            }

            string? ifNoneMatch = context.Request.Headers["If-None-Match"];
            this.LastIfNoneMatch = ifNoneMatch;
            this.Requests++;

            using HttpListenerResponse response = context.Response;

            // A fresh tag on every response, including 304s. Legal, and what a multi-node CDN or an
            // origin that regenerates tags on deploy does in practice. Only the most recently issued
            // tag is honoured, so a client that re-sends a stale one gets the whole body back.
            bool matches = string.Equals(ifNoneMatch?.Trim('"'), currentEtag.Trim('"'), StringComparison.Ordinal);
            string issued = this.Rotate();

            response.AddHeader("ETag", issued);
            response.AddHeader("Last-Modified", DateTimeOffset.UnixEpoch.ToString("R", CultureInfo.InvariantCulture));

            if (matches)
            {
                // The client already has this version. Say so and send nothing.
                response.StatusCode = (int)HttpStatusCode.NotModified;
                response.ContentLength64 = 0;
                this.NotModifiedResponses++;
                this.BytesSaved += currentBody.Length;
                continue;
            }

            response.ContentType = "application/rss+xml";
            response.ContentLength64 = currentBody.Length;
            this.FullResponses++;
            this.BytesServed += currentBody.Length;

            await response.OutputStream.WriteAsync(currentBody);
        }
    }
}