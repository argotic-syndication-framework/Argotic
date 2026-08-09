#:project ../Argotic.Core/Argotic.Core.csproj
#:package Microsoft.Extensions.DependencyInjection

// ---------------------------------------------------------------------------------------------
// 21 -- Everything so far, wired the way an application would wire it
//
//     dotnet run --file Solutions/Samples/21-a-polling-service.cs
//
// Twenty samples, each about one thing. This one has no new API in it at all -- it is the same
// surface, assembled into the smallest program that is recognisably an aggregator: read a
// subscription list, poll every feed in it efficiently, merge what is new into one output feed,
// and do it again without re-downloading anything.
//
// That is what endjin's Azure Weekly and Power BI Weekly do, and it is what this library was
// dusted off to keep doing. The point of the sample is the shape of the loop, not the parsing.
//
// Four things from earlier samples do the work:
//
//     03  OPML is the subscription list
//     05  the feeds may be RSS or Atom and we should not have to care
//     17  conditional GET is what makes polling cheap
//     14  a caller-supplied HttpClient is what makes it configurable
//
// The one thing genuinely new here is how the client gets configured, and it is the part people
// most often get subtly wrong.
// ---------------------------------------------------------------------------------------------

using System.Net;
using System.Net.Sockets;
using System.Text;

using Argotic.Common;
using Argotic.Configuration;
using Argotic.Syndication;

using Microsoft.Extensions.DependencyInjection;

// ---------------------------------------------------------------------------------------------
// 1. Register the client by name, not the resource by type
//
// A syndication resource is constructed, not resolved. You write `new RssFeed()`; you never ask
// a container for one, because it is a document you are about to fill rather than a service with
// dependencies. So there is nothing to register for RssFeed, and looking for an AddArgotic() that
// hands you feeds is looking for something that would not make sense.
//
// What does need registering is the HttpClient, and AddArgoticSyndicationClient registers it as a
// *named* client: the Argotic handler defaults, a framework User-Agent, and -- deliberately --
// Timeout.InfiniteTimeSpan, because deadlines belong to SyndicationResourceLoadSettings and a
// client-level timeout underneath could only cut a longer one short.
//
// Then there is the trap, and it is worth stating in full because the compiler cannot help you:
//
//     Always ArgoticHttpClients.Syndication. Never the string literal.
//
// A named client is looked up by string. Misspell it and IHttpClientFactory does not throw -- it
// hands you a brand-new, entirely default HttpClient. It works. It fetches feeds. And every
// decision above is silently absent: no handler defaults, no User-Agent, and a 100-second client
// timeout back underneath your settings. The output below shows both.
//
// TrackbackClient and XmlRpcClient are different: they *are* services, so AddTrackbackClient and
// AddXmlRpcClient register them as typed clients and you resolve them by type.
// ---------------------------------------------------------------------------------------------

ServiceCollection services = new();
services.AddArgoticSyndicationClient();

using ServiceProvider provider = services.BuildServiceProvider();
IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

using HttpClient configured = factory.CreateClient(ArgoticHttpClients.Syndication);
using HttpClient mistyped = factory.CreateClient("Argotic.Syndicaton");

Heading("The named client, and the typo that is not a compile error");
Console.WriteLine($"  {"resolved with",-32} {"User-Agent",-42} Timeout");
Show("ArgoticHttpClients.Syndication", configured);
Show("\"Argotic.Syndicaton\" (typo)", mistyped);
Console.WriteLine();
Console.WriteLine("  The second one works. It fetches feeds. It is simply not the client you configured.");

// ---------------------------------------------------------------------------------------------
// 2. The subscription list
//
// OPML is where the addresses come from, and sample 03's lesson applies directly: the feed URL is
// in Attributes["xmlUrl"], not in a typed property, because OPML defines that attribute per type
// rather than globally. A walk that reads only the typed surface produces a tidy tree of labels
// and no addresses at all.
// ---------------------------------------------------------------------------------------------

using Origin origin = Origin.Start();

string subscriptions = $"""
    <?xml version="1.0" encoding="utf-8"?>
    <opml version="2.0">
      <head><title>endjin's aggregator</title></head>
      <body>
        <outline text="endjin">
          <outline text="endjin blog" type="rss" xmlUrl="{origin.Uri}rss.xml" />
          <outline text="endjin talks" type="feed" xmlUrl="{origin.Uri}atom.xml" />
        </outline>
        <outline text="A folder with nothing subscribable in it" />
      </body>
    </opml>
    """;

OpmlDocument list = new();
list.Load(SyndicationEncodingUtility.CreateSafeNavigator(subscriptions));

List<Uri> feeds = [.. Subscriptions(list.Outlines)];

Heading($"{list.Head.Title}");
foreach (Uri feed in feeds)
{
    Console.WriteLine($"  {feed}");
}

// ---------------------------------------------------------------------------------------------
// 3. The poll
//
// One pass over the list. For each feed: send whatever validators we stored last time, and if the
// server says 304 do nothing at all.
//
// There is one awkwardness worth naming rather than hiding. LoadIfModifiedAsync is generic over
// ISyndicationResource, and GenericSyndicationFeed -- sample 05's format-agnostic reader -- is
// deliberately not one, because it is a projection rather than a resource. So the conditional
// path cannot be format-agnostic: you have to name RssFeed or AtomFeed as the type argument. A
// production aggregator stores the format alongside the subscription after the first successful
// fetch and uses it thereafter, which is what the branch below stands in for.
//
// Note what is stored after every response: result.Validators, whatever they are. Sample 17 is
// why. A poller that keeps re-sending its first ETag is a poller that downloads everything, every
// time, and reports every item as new.
// ---------------------------------------------------------------------------------------------

Dictionary<Uri, SyndicationValidators> seen = [];
List<(string Feed, string Title, DateTime Published)> harvested = [];

Heading("First pass");
await PollAsync();

Heading("Second pass, nothing published in between");
await PollAsync();

origin.Publish("rss.xml", Documents.RssFeed(includeNewPost: true));

Heading("Third pass, after one new post");
await PollAsync();

// ---------------------------------------------------------------------------------------------
// 4. The output feed
//
// Everything harvested, newest first, as one Atom feed. Atom rather than RSS for the reason
// sample 02 gave: an aggregator's entries come from many sources, so identity has to be separate
// from location, and <id> is the only element in either format that means "this thing, forever"
// rather than "this address, today".
//
// The dates are constructed Utc, because sample 07.
// ---------------------------------------------------------------------------------------------

AtomFeed output = new(
    new AtomId(new Uri("https://endjin.com/aggregated.xml")),
    new AtomTextConstruct("Everything endjin follows") { TextType = AtomTextConstructType.Text },
    new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc));

output.Links.Add(new AtomLink(new Uri("https://endjin.com/aggregated.xml"), "self"));

foreach ((string feed, string title, DateTime published) in harvested.OrderByDescending(item => item.Published))
{
    AtomEntry entry = new(
        new AtomId(new Uri($"urn:sha256:{Documents.Fingerprint(feed + title)}")),
        new AtomTextConstruct(title) { TextType = AtomTextConstructType.Text },
        published == DateTime.MinValue ? new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc) : published);

    entry.Categories.Add(new AtomCategory(new Uri(feed).AbsolutePath.Trim('/')));
    output.Entries.Add(entry);
}

using MemoryStream stream = new();
output.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });

Heading("The aggregated feed");
Console.WriteLine($"  {output.Entries.Count} entries, {stream.Length:N0} bytes");
foreach (AtomEntry entry in output.Entries)
{
    Console.WriteLine($"  {entry.UpdatedOn:yyyy-MM-dd}  {entry.Title?.Content}");
}

Heading("What the three passes cost");
Console.WriteLine($"  requests        {origin.Requests}");
Console.WriteLine($"  bodies sent     {origin.FullResponses}  ({origin.BytesServed:N0} bytes)");
Console.WriteLine($"  304 responses   {origin.NotModifiedResponses}  (0 bytes)");
Console.WriteLine();
Console.WriteLine($"  Six fetches would have cost {origin.BytesIfAlwaysFull:N0} bytes. Conditional requests");
Console.WriteLine($"  cost {origin.BytesServed:N0}, and the second pass did no work at all -- which is the point.");

Console.WriteLine();
Console.WriteLine("That is the whole tour. The README has a map of where to go back to.");

async Task PollAsync()
{
    foreach (Uri feed in feeds)
    {
        seen.TryGetValue(feed, out SyndicationValidators? validators);

        if (feed.AbsolutePath.EndsWith("atom.xml", StringComparison.Ordinal))
        {
            ConditionalLoadResult<AtomFeed> result = await SyndicationResourceReader.LoadIfModifiedAsync<AtomFeed>(
                feed, validators, configured);

            seen[feed] = result.Validators;
            ReportPoll(feed, result.StatusCode, result.WasModified);

            if (result.Resource is { } atom)
            {
                foreach (AtomEntry entry in atom.Entries)
                {
                    Record(feed, entry.Title?.Content ?? "(untitled)", entry.PublishedOn == DateTime.MinValue ? entry.UpdatedOn : entry.PublishedOn);
                }
            }

            continue;
        }

        ConditionalLoadResult<RssFeed> asRss = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(feed, validators, configured);
        seen[feed] = asRss.Validators;
        ReportPoll(feed, asRss.StatusCode, asRss.WasModified);

        if (asRss.Resource is { } rss)
        {
            foreach (RssItem item in rss.Channel.Items)
            {
                Record(feed, item.Title, item.PublicationDate);
            }
        }
    }
}

void Record(Uri feed, string title, DateTime published)
{
    // An aggregator sees the same item on every pass that returns a body, so de-duplicate on
    // something stable. Title plus source is crude; a real one uses the guid or the Atom id.
    if (!harvested.Any(item => item.Feed == feed.ToString() && item.Title == title))
    {
        harvested.Add((feed.ToString(), title, published));
    }
}

static void ReportPoll(Uri feed, HttpStatusCode status, bool modified) =>
    Console.WriteLine($"  {feed.AbsolutePath,-12} {(int)status} {status,-12} {(modified ? "read and merged" : "nothing to do")}");

static void Show(string label, HttpClient client)
{
    string agent = client.DefaultRequestHeaders.UserAgent.ToString();
    string timeout = client.Timeout == Timeout.InfiniteTimeSpan ? "infinite (as configured)" : client.Timeout.ToString();

    Console.WriteLine($"  {label,-32} {(agent.Length == 0 ? "(none -- not your client)" : agent),-42} {timeout}");
}

static IEnumerable<Uri> Subscriptions(IEnumerable<OpmlOutline> outlines)
{
    foreach (OpmlOutline outline in outlines)
    {
        // Sample 03: the address is in Attributes, never in a typed property.
        if (outline.IsSubscriptionListOutline
            && outline.Attributes.TryGetValue("xmlUrl", out string? url)
            && Uri.TryCreate(url, UriKind.Absolute, out Uri? feed))
        {
            yield return feed;
        }

        foreach (Uri nested in Subscriptions(outline.Outlines))
        {
            yield return nested;
        }
    }
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>
/// The documents the loopback origin serves, and a stable fingerprint for entity tags and entry
/// identifiers. A static class rather than local functions: a local function declared among
/// top-level statements is scoped to the generated Main and cannot be reached from a type
/// declared in the same file (CS8801).
/// </summary>
internal static class Documents
{
    public static string Fingerprint(string value) =>
        Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public static string RssFeed(bool includeNewPost)
    {
        string extra = includeNewPost
            ? "<item><title>Rx.NET v7.0 Released</title><link>https://endjin.com/blog/3</link><description>New.</description><pubDate>Fri, 07 Aug 2026 09:00:00 GMT</pubDate></item>"
            : string.Empty;

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0"><channel>
              <title>endjin blog</title><link>https://endjin.com/blog/</link>
              <description>Technical writing from endjin.</description>
              {extra}
              <item><title>The GenAI Reality Check</title><link>https://endjin.com/blog/1</link>
                    <description>A post.</description><pubDate>Thu, 14 May 2026 08:54:29 GMT</pubDate></item>
            </channel></rss>
            """;
    }

    public static string AtomFeed() => """
        <?xml version="1.0" encoding="utf-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
          <id>https://endjin.com/talks/</id>
          <title type="text">endjin talks</title>
          <updated>2026-03-02T09:00:00Z</updated>
          <entry>
            <id>urn:uuid:3c9b1f47-6d2a-4e8b-91a5-7f0c2d5e8a13</id>
            <title type="text">Rx.NET v7.0: what changed and why</title>
            <updated>2026-03-02T09:00:00Z</updated>
            <published>2026-03-02T09:00:00Z</published>
          </entry>
        </feed>
        """;
}

/// <summary>
/// Two feeds on one loopback origin, honouring If-None-Match and counting what it served.
/// </summary>
internal sealed class Origin : IDisposable
{
    private readonly HttpListener listener;
    private readonly Lock gate = new();
    private readonly Dictionary<string, (byte[] Body, string MediaType, string ETag)> routes = new(StringComparer.Ordinal);

    private Origin(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    public Uri Uri { get; }

    public int Requests { get; private set; }

    public int FullResponses { get; private set; }

    public int NotModifiedResponses { get; private set; }

    public long BytesServed { get; private set; }

    public long BytesIfAlwaysFull { get; private set; }

    public static Origin Start()
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        // CA2000 cannot see that ownership passes to the caller, which disposes with `using`.
#pragma warning disable CA2000
        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        Origin origin = new(listener, new Uri($"http://127.0.0.1:{port}/"));
#pragma warning restore CA2000

        origin.Publish("rss.xml", Documents.RssFeed(includeNewPost: false));
        origin.Publish("atom.xml", Documents.AtomFeed());
        _ = origin.ServeAsync();

        return origin;
    }

    public void Publish(string path, string body)
    {
        lock (this.gate)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            string mediaType = path.EndsWith("atom.xml", StringComparison.Ordinal) ? "application/atom+xml" : "application/rss+xml";

            // A new entity tag every time the document changes, and only then.
            this.routes[$"/{path}"] = (bytes, mediaType, $"\"{Documents.Fingerprint(body)[..12]}\"");
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

            string path = context.Request.Url?.AbsolutePath ?? "/";
            string? ifNoneMatch = context.Request.Headers["If-None-Match"];

            using HttpListenerResponse response = context.Response;

            (byte[] Body, string MediaType, string ETag) route;
            lock (this.gate)
            {
                this.Requests++;
                if (!this.routes.TryGetValue(path, out route))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.ContentLength64 = 0;
                    continue;
                }

                this.BytesIfAlwaysFull += route.Body.Length;
            }

            response.AddHeader("ETag", route.ETag);

            if (string.Equals(ifNoneMatch, route.ETag, StringComparison.Ordinal))
            {
                response.StatusCode = (int)HttpStatusCode.NotModified;
                response.ContentLength64 = 0;
                lock (this.gate)
                {
                    this.NotModifiedResponses++;
                }

                continue;
            }

            response.ContentType = route.MediaType;
            response.ContentLength64 = route.Body.Length;
            lock (this.gate)
            {
                this.FullResponses++;
                this.BytesServed += route.Body.Length;
            }

            await response.OutputStream.WriteAsync(route.Body);
        }
    }
}