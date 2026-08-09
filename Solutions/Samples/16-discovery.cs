#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 16 -- Finding a feed when all you have is a homepage
//
//     dotnet run --file Solutions/Samples/16-discovery.cs
//
// Nobody types a feed URL. They type endjin.com, and something has to work out that the feed is
// at endjin.com/rss.xml. That convention is nearly as old as RSS itself: a page advertises its
// feeds with <link rel="alternate"> elements in its head, and every feed reader ever written
// looks for them. It is called autodiscovery, and it is why pasting a homepage into a reader
// works at all.
//
// The mechanism is simple enough to describe in a sentence and has two details that are easy to
// get wrong -- one about relative URLs, one about what the type attribute is worth. Both are the
// sort of thing that works on the site you tested against and fails on the next one.
// ---------------------------------------------------------------------------------------------

using System.Net;
using System.Net.Sockets;
using System.Text;

using Argotic.Common;
using Argotic.Syndication;

// A homepage advertising four feeds, with the mix of absolute and relative hrefs a real page has.
// The last one declares text/xml, which is legal, common, and the subject of section 3.
const string HomepageHtml = """
    <!doctype html>
    <html lang="en-GB">
      <head>
        <title>endjin</title>
        <link rel="alternate" type="application/rss+xml" title="endjin blog (RSS)" href="/rss.xml" />
        <link rel="alternate" type="application/atom+xml" title="endjin blog (Atom)" href="atom.xml" />
        <link rel="alternate" type="application/rss+xml" title="Azure Weekly" href="https://azureweekly.info/rss.xml" />
        <link rel="alternate" type="text/xml" title="Power BI Weekly" href="/powerbi.xml" />
        <link rel="stylesheet" href="/site.css" />
      </head>
      <body>
        <p>We are <a href="https://endjin.com/who-we-are/">endjin</a>, a technical consultancy.</p>
        <p>Read <a href="/blog/genai-reality-check-new-instrument-same-orchestra">the latest post</a>.</p>
      </body>
    </html>
    """;

const string FeedXml = """
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

// ---------------------------------------------------------------------------------------------
// 1. Relative hrefs need a base, and the base is not the URL you asked for
//
// Two of the four links above are relative, which is normal -- a site does not write its own
// hostname into its own head. So a resolved absolute URL requires knowing what to resolve against,
// and ExtractDiscoverableSyndicationEndpoints has an overload that takes it explicitly for that
// reason.
//
// The subtlety is which URL is the right base. It is the address the page finally came from, after
// any redirects -- not the one you requested. Ask for endjin.com, get redirected to www.endjin.com,
// resolve /rss.xml against the address you typed, and you have built a URL on a host that redirects
// every request, which works right up until it does not.
//
// The async overload does that for you: it fetches, follows redirects, and resolves against where
// it ended up. Prefer it. Use the synchronous overloads when the HTML is already in your hand.
// ---------------------------------------------------------------------------------------------

Heading("Without a base");
foreach (DiscoverableSyndicationEndpoint endpoint in SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(HomepageHtml))
{
    Console.WriteLine($"  {endpoint.Source}");
}

Heading("With one");
foreach (DiscoverableSyndicationEndpoint endpoint in SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(HomepageHtml, new Uri("https://endjin.com/")))
{
    Console.WriteLine($"  {endpoint.Source}");
}

// ---------------------------------------------------------------------------------------------
// 2. Doing it over HTTP
//
// LocateDiscoverableSyndicationEndpointsAsync fetches the page and returns the endpoints with
// their URLs already resolved. Note the size limit it uses -- SyndicationContentLengthLimits
// .Discovery is 2 MiB, the smallest of the four, because the page it is reading is never the
// document you wanted. It is a means to an end and does not deserve a feed's allowance.
// ---------------------------------------------------------------------------------------------

using SiteHost site = SiteHost.Start();
site.Serve("/", HomepageHtml, "text/html");
site.Serve("/rss.xml", FeedXml, "application/rss+xml");
site.Serve("/atom.xml", FeedXml, "application/atom+xml");
site.Serve("/powerbi.xml", FeedXml, "text/xml");

IList<DiscoverableSyndicationEndpoint> discovered =
    await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(site.Uri, SyndicationEncodingUtility.SharedHttpClient);

Heading($"Discovered from {site.Uri}");
foreach (DiscoverableSyndicationEndpoint endpoint in discovered)
{
    Console.WriteLine($"  {endpoint.Title}");
    Console.WriteLine($"    {endpoint.Source}");
    Console.WriteLine($"    declared {Quote(endpoint.ContentType)} -> ContentFormat.{endpoint.ContentFormat}");
}

// ---------------------------------------------------------------------------------------------
// 3. ContentFormat is only as good as the type attribute
//
// DiscoverableSyndicationEndpoint.ContentFormat is derived from the type attribute the page
// wrote, and nothing else. It never fetches the feed. That makes it free, and it makes it wrong
// whenever the page is vague -- and text/xml is extremely common, because it is what several
// popular publishing platforms emit by default.
//
// So a None here does not mean "not a feed". It means "the page did not say", and the next move
// is to ask the feed itself: SyndicationContentFormatGetAsync fetches the first 64 KiB and reads
// the document element, which is the sniffing from sample 05 with a bound on how much it will
// download to answer.
//
// The pattern is: trust the declaration when there is one, sniff when there is not, and never
// discard an endpoint merely because it declined to describe itself.
// ---------------------------------------------------------------------------------------------

Heading("Resolving the ones that did not say");
foreach (DiscoverableSyndicationEndpoint endpoint in discovered)
{
    if (endpoint.ContentFormat != SyndicationContentFormat.None)
    {
        Console.WriteLine($"  {endpoint.Title,-22} declared {endpoint.ContentFormat}, no fetch needed");
        continue;
    }

    // Structural, not incidental. The fixture advertises one genuinely external feed, and today it
    // declares its type so this branch never reaches it -- but "never" would then depend on a
    // string in a fixture nobody would think to check before editing. Every sample in this
    // directory has to run in a build with no network, so the guarantee belongs in the code.
    if (!endpoint.Source!.IsLoopback)
    {
        Console.WriteLine($"  {endpoint.Title,-22} off-box, not fetched -- this directory stays offline");
        continue;
    }

    SyndicationContentFormat sniffed = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
        endpoint.Source!,
        SyndicationEncodingUtility.SharedHttpClient);

    Console.WriteLine($"  {endpoint.Title,-22} declared nothing usable; sniffed {sniffed}");
}

// ---------------------------------------------------------------------------------------------
// 4. From an endpoint to a feed
//
// An endpoint is an address and a claim about it. Turning one into a document is one call:
// CreateNavigatorAsync fetches and returns something every Load overload accepts. It has the
// same two shapes as everything else in this library -- with a client and without -- and the
// advice is the same as sample 14's: pass one.
// ---------------------------------------------------------------------------------------------

DiscoverableSyndicationEndpoint first = discovered[0];

RssFeed feed = new();
feed.Load(await first.CreateNavigatorAsync(SyndicationEncodingUtility.SharedHttpClient));

Heading("Fetched through the endpoint");
Console.WriteLine($"  {first.Source}");
Console.WriteLine($"  {feed.Channel.Title}: {feed.Channel.Items.Count} item");

// ---------------------------------------------------------------------------------------------
// 5. The other things on that page
//
// Two smaller utilities live alongside discovery and are worth knowing about because people
// reimplement them. ExtractUrls pulls every href out of a document, which is what a link checker
// or a trackback sender needs -- sample 19 uses it for exactly that. UriExistsAsync issues a HEAD
// and reports whether the address answers, without downloading anything.
// ---------------------------------------------------------------------------------------------

IList<Uri> links = SyndicationDiscoveryUtility.ExtractUrls(HomepageHtml);

Heading("Every URL on the page");
foreach (Uri link in links)
{
    Console.WriteLine($"  {link}");
}

bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
    new Uri(site.Uri, "rss.xml"),
    SyndicationEncodingUtility.SharedHttpClient);

bool missing = await SyndicationDiscoveryUtility.UriExistsAsync(
    new Uri(site.Uri, "nothing-here.xml"),
    SyndicationEncodingUtility.SharedHttpClient);

Console.WriteLine();
Console.WriteLine($"  UriExistsAsync(/rss.xml)          {exists}");
Console.WriteLine($"  UriExistsAsync(/nothing-here.xml) {missing}");

Console.WriteLine();
Console.WriteLine("Next: 17-conditional-get.cs -- polling 400 feeds without downloading 400 feeds.");

static string Quote(string value) => value.Length == 0 ? "(nothing)" : $"\"{value}\"";

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

/// <summary>
/// A loopback host serving several documents by path, so discovery has somewhere to discover.
/// </summary>
internal sealed class SiteHost : IDisposable
{
    private readonly HttpListener listener;
    private readonly Dictionary<string, (byte[] Body, string MediaType)> routes = new(StringComparer.Ordinal);

    private SiteHost(HttpListener listener, Uri uri)
    {
        this.listener = listener;
        this.Uri = uri;
    }

    public Uri Uri { get; }

    public static SiteHost Start()
    {
        using Socket probe = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        probe.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)probe.LocalEndPoint!).Port;
        probe.Close();

        HttpListener listener = new();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.IgnoreWriteExceptions = true;
        listener.Start();

        // CA2000 cannot see that ownership of the listener passes to the host, and of the host to
        // the caller, which disposes it with a `using`.
#pragma warning disable CA2000
        SiteHost host = new(listener, new Uri($"http://127.0.0.1:{port}/"));
#pragma warning restore CA2000
        _ = host.ServeAsync();

        return host;
    }

    public void Serve(string path, string body, string mediaType) =>
        this.routes[path] = (Encoding.UTF8.GetBytes(body), mediaType);

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

            using HttpListenerResponse response = context.Response;
            string path = context.Request.Url?.AbsolutePath ?? "/";

            if (this.routes.TryGetValue(path, out (byte[] Body, string MediaType) route))
            {
                response.ContentType = route.MediaType;

                // A HEAD request gets the headers and no body, which is what UriExistsAsync relies on.
                response.ContentLength64 = route.Body.Length;
                if (!string.Equals(context.Request.HttpMethod, "HEAD", StringComparison.Ordinal))
                {
                    await response.OutputStream.WriteAsync(route.Body);
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.ContentLength64 = 0;
            }
        }
    }
}