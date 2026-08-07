# Argotic.Core

The main package of the **Argotic Syndication Framework** — read, write and manipulate web content
syndication feeds in .NET. This is the package most people install.

Supports **RSS**, **Atom**, **OPML**, **APML**, **BlogML**, **RSD** and **Sitemap/SitemapIndex**, a
format-agnostic `GenericSyndicationFeed` wrapper, the **Atom Publishing Protocol**, and **Trackback** and
**XML-RPC** clients.

## Install

```bash
dotnet add package Argotic.Core
```

Target framework: **`net10.0`**. Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
or later. Earlier targets (.NET Standard 2.0/2.1, .NET 8, .NET 9) are not supported by this release.

`Argotic.Core` pulls in [`Argotic.Common`](https://www.nuget.org/packages/Argotic.Common) and
[`Argotic.Extensions`](https://www.nuget.org/packages/Argotic.Extensions) transitively — you do **not** need
to install them separately.

## 30-second start

```csharp
using Argotic.Syndication;

RssFeed feed = await RssFeed.CreateAsync(new Uri("https://endjin.com/rss.xml"));

Console.WriteLine(feed.Channel.Title);
Console.WriteLine(feed.Channel.Description);

foreach (RssItem item in feed.Channel.Items)
{
    Console.WriteLine($"{item.PublicationDate:yyyy-MM-dd}  {item.Title}  {item.Link}");
}
```

## What is in the package

| Type                                                                                                       | Namespace               | Format                                                                             |
|------------------------------------------------------------------------------------------------------------|-------------------------|------------------------------------------------------------------------------------|
| `RssFeed`, `RssChannel`, `RssItem`                                                                         | `Argotic.Syndication`   | [RSS 2.0](https://www.rssboard.org/rss-specification)                              |
| `AtomFeed`, `AtomEntry`                                                                                    | `Argotic.Syndication`   | [Atom 1.0](https://www.rfc-editor.org/rfc/rfc4287.html)                            |
| `OpmlDocument`, `OpmlOutline`                                                                              | `Argotic.Syndication`   | [OPML 2.0](https://www.opml.org/spec2)                                             |
| `Sitemap`, `SitemapUrl`, `SitemapIndex`, `SitemapIndexEntry`                                               | `Argotic.Syndication`   | [Sitemaps 0.9](https://www.sitemaps.org/protocol.html)                             |
| `GenericSyndicationFeed`, `GenericSyndicationItem`, `GenericSyndicationCategory`                           | `Argotic.Syndication`   | RSS **or** Atom, format-agnostic                                                   |
| `ApmlDocument`                                                                                             | `Argotic.Syndication`   | APML 1.0                                                                           |
| `BlogMLDocument`                                                                                           | `Argotic.Syndication`   | BlogML 2.0                                                                         |
| `RsdDocument`                                                                                              | `Argotic.Syndication`   | RSD 1.0                                                                            |
| `AtomServiceDocument`, `AtomCategoryDocument`, `AtomEntryResource`, `AtomWorkspace`, `AtomMemberResources` | `Argotic.Publishing`    | [Atom Publishing Protocol (RFC 5023)](https://www.rfc-editor.org/rfc/rfc5023.html) |
| `TrackbackClient`, `TrackbackMessage`, `XmlRpcClient`, `XmlRpcMessage`                                     | `Argotic.Net`           | Trackback, XML-RPC                                                                 |
| `ServiceCollectionExtensions`, `ArgoticHttpClients`                                                        | `Argotic.Configuration` | Dependency injection                                                               |

## One shape, every format

Every resource type exposes the same members, so what you learn for `RssFeed` reads across to `AtomFeed`,
`OpmlDocument`, `Sitemap`, `ApmlDocument`, `BlogMLDocument`, `RsdDocument` and the Atom Publishing documents
unchanged.

```csharp
// Synchronous, from something already in hand
void Load(IXPathNavigable source [, SyndicationResourceLoadSettings? settings]);
void Load(Stream stream          [, SyndicationResourceLoadSettings? settings]);
void Load(XmlReader reader       [, SyndicationResourceLoadSettings? settings]);

// Asynchronous, from a URL
Task LoadAsync(Uri source, CancellationToken cancellationToken = default);
Task LoadAsync(Uri source, HttpClient httpClient,
               SyndicationResourceLoadSettings? settings = null,
               SyndicationRequestOptions? requestOptions = null,
               CancellationToken cancellationToken = default);

// One-call static factory — the same two overloads, but constructing the instance for you
static Task<T> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null,
                           CancellationToken cancellationToken = default);
static Task<T> CreateAsync(Uri source, HttpClient httpClient, …);

// Writing
void Save(Stream stream    [, SyndicationResourceSaveSettings? settings]);
void Save(XmlWriter writer [, SyndicationResourceSaveSettings? settings]);

// Plus
XPathNavigator CreateNavigator();
event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;
```

`CreateAsync` is the one-call form. `LoadAsync` on an instance is the form that lets you subscribe to
`Loaded` before the parse happens.

### Loading from a stream, a reader or a navigator

```csharp
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

RssFeed feed = new();

// From a Stream
using (Stream stream = File.OpenRead("feed.xml"))
{
    feed.Load(stream);
}

// From an XmlReader
using (XmlReader reader = XmlReader.Create("feed.xml", SyndicationEncodingUtility.CreateSafeXmlReaderSettings()))
{
    feed.Load(reader);
}

// From anything IXPathNavigable
using (XmlReader reader = XmlReader.Create("feed.xml", SyndicationEncodingUtility.CreateSafeXmlReaderSettings()))
{
    feed.Load(new XPathDocument(reader));
}
```

`SyndicationEncodingUtility.CreateSafeXmlReaderSettings()` returns the hardened reader configuration the
framework uses internally — prefer it to a bare `XmlReader.Create(path)` when the XML is not yours.

## Building and saving a feed

```csharp
using System.Globalization;
using System.Xml;
using Argotic.Syndication;

RssFeed feed = new()
{
    Channel =
    {
        Title = "Dallas Times-Herald",
        Link = new Uri("https://dallas.example.com"),
        Description = "Current headlines from the Dallas Times-Herald newspaper",
        Language = new CultureInfo("en-US"),
        Copyright = "Copyright 2026 Dallas Times-Herald",
    },
};

RssItem item = new()
{
    Title = "Seventh Heaven! Ryan Hurls Another No Hitter",
    Link = new Uri("https://dallas.example.com/1991/05/02/nolan.htm"),
    Description = "Nolan Ryan hurled the seventh no-hitter of his career.",
    PublicationDate = DateTime.UtcNow,
    Guid = new RssGuid("https://dallas.example.com/1991/05/02/nolan.htm"),
};

item.Categories.Add(new RssCategory("sports"));
item.Enclosures.Add(new RssEnclosure(24_986_239L, "audio/mpeg", new Uri("https://dallas.example.com/joebob.mp3")));

feed.Channel.Items.Add(item);

// To a Stream…
using (Stream stream = File.Create("feed.xml"))
{
    feed.Save(stream);
}

// …or through an XmlWriter you configure
using (XmlWriter writer = XmlWriter.Create("feed.xml", new XmlWriterSettings { Indent = true }))
{
    feed.Save(writer);
}
```

Atom is the same shape with Atom's vocabulary:

```csharp
using Argotic.Syndication;

AtomFeed feed = new()
{
    Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
    Title = new AtomTextConstruct("Example Feed"),
    UpdatedOn = DateTime.UtcNow,
};

feed.Links.Add(new AtomLink(new Uri("https://example.org/")));
feed.Links.Add(new AtomLink(new Uri("/feed", UriKind.Relative), "self"));
feed.Authors.Add(new AtomPersonConstruct("John Doe"));

feed.Entries.Add(new AtomEntry
{
    Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
    Title = new AtomTextConstruct("Atom-Powered Robots Run Amok"),
    UpdatedOn = DateTime.UtcNow,
    Summary = new AtomTextConstruct("Some text."),
});
```

## Reading a feed without knowing its format

`GenericSyndicationFeed` exposes only what RSS and Atom agree on — title, description, categories, items —
and hands back the underlying `RssFeed` or `AtomFeed` through `Resource` when you need the rest.

```csharp
using Argotic.Common;
using Argotic.Syndication;

GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(new Uri("https://endjin.com/rss.xml"));

foreach (GenericSyndicationItem item in feed.Items)
{
    if (item.PublishedOn > DateTime.UtcNow.AddDays(-7))
    {
        Console.WriteLine(item.Title);

        foreach (GenericSyndicationCategory category in item.Categories)
        {
            Console.WriteLine($"  #{category.Term}");
        }
    }
}

if (feed.Format == SyndicationContentFormat.Rss && feed.Resource is RssFeed rss)
{
    // Reach for RSS-only members such as rss.Channel.TimeToLive.
}
```

## Sitemaps

```csharp
using Argotic.Syndication;

Sitemap sitemap = new();

sitemap.Urls.Add(new SitemapUrl
{
    Location = new Uri("https://www.example.com/"),
    LastModified = DateTime.UtcNow,
    ChangeFrequency = SitemapChangeFrequency.Daily,
    Priority = 1.0m,
});

using Stream stream = File.Create("sitemap.xml");
sitemap.Save(stream);
```

For a site too large for one file, a `SitemapIndex` points at the individual sitemaps:

```csharp
SitemapIndex index = new();
index.Sitemaps.Add(new SitemapIndexEntry(new Uri("https://www.example.com/sitemap-1.xml"), DateTime.UtcNow));
```

Google's News, Image, Video and Hreflang sitemap extensions ship in
[`Argotic.Extensions`](https://www.nuget.org/packages/Argotic.Extensions), which arrives with this package.

## HTTP: the shared client, timeouts and cancellation

A single process-wide `HttpClient` — `SyndicationEncodingUtility.SharedHttpClient`, a `Lazy<HttpClient>` over
a `SocketsHttpHandler` that rotates its own pooled connections — backs every overload that does not take a
client of its own.

**Timeouts do not come from `HttpClient.Timeout`.** The shared client is deliberately built with
`Timeout.InfiniteTimeSpan`, so reading its `Timeout` tells you nothing. Deadlines are enforced with
`CancellationTokenSource.CancelAfter`, which means they cover the whole load — the response body included —
and not merely the point at which the headers arrive. The default is
`SyndicationEncodingUtility.DefaultRequestTimeout`, 100 seconds.

```csharp
using Argotic.Common;
using Argotic.Syndication;

SyndicationResourceLoadSettings settings = new()
{
    Timeout = TimeSpan.FromSeconds(20),   // null means *no* deadline, not "use the default"
    RetrievalLimit = 50,                  // cap the items parsed; 0 means unlimited
    MaxResponseContentLength = 8 * 1024 * 1024,
};

SyndicationRequestOptions options = new()
{
    UserAgent = "MyAggregator/1.0 (+https://example.com/bot)",
    Accept = "application/rss+xml, application/atom+xml;q=0.9",
};

using HttpClient client = new();

RssFeed feed = new();
await feed.LoadAsync(new Uri("https://endjin.com/rss.xml"), client, settings, options, cancellationToken);
```

### Dependency injection

Register the named client Argotic's loads should use, then hand the resolved client to `LoadAsync`. A
syndication resource is **constructed, not resolved** — you write `new RssFeed()`, you do not ask the
container for one — so the client is registered by name rather than as a typed client.

```csharp
using Argotic.Configuration;
using Microsoft.Extensions.DependencyInjection;

services.AddArgoticSyndicationClient();

// …later, wherever you load a feed
IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
HttpClient client = factory.CreateClient(ArgoticHttpClients.Syndication);

RssFeed feed = await RssFeed.CreateAsync(source, client, cancellationToken: cancellationToken);
```

Always use the `ArgoticHttpClients.Syndication` constant, never the literal string: a named client is looked
up by name, so a typo is not a compile error — the factory hands back a brand-new, entirely default
`HttpClient` and every decision you configured is quietly absent.

`AddXmlRpcClient` and `AddTrackbackClient` register the two protocol clients with options binding.

### Conditional GET

Poll a feed without re-downloading it when nothing changed.

```csharp
using Argotic.Common;
using Argotic.Syndication;

Uri source = new("https://endjin.com/rss.xml");

// Both come from your store, recorded by the previous poll.
DateTime lastModified = previousLastModified;   // DateTime.MinValue on the first ever poll
string? entityTag = previousETag;               // null if the origin sent no ETag

using ConditionalGetResult result =
    await SyndicationDiscoveryUtility.ConditionalGetAsync(source, lastModified, entityTag);

if (result.WasModified)
{
    using Stream stream = await result.GetResponseStreamAsync();

    RssFeed feed = new();
    feed.Load(stream);

    // Persist result.LastModified and result.ETag for the next poll.
}
```

## The `Loaded` event

Every resource raises `Loaded` after a successful parse — synchronous or asynchronous — and not at all when
one throws. Subscribe on an instance and call `LoadAsync` on it, rather than using `CreateAsync`, when you
want the handler to see the resource the moment it is parsed. The event arguments carry the `XPathNavigator`
the resource was built from, and a `Source` **only** where the load began with a `Uri` — the stream and
reader overloads have never known one.

```csharp
using Argotic.Common;
using Argotic.Syndication;

RssFeed feed = new();
feed.Loaded += (sender, e) =>
{
    if (e.Source is not null)
    {
        Console.WriteLine($"Loaded from {e.Source}");
    }
};

await feed.LoadAsync(new Uri("https://endjin.com/rss.xml"));
```

## Atom Publishing Protocol

```csharp
using Argotic.Publishing;

AtomServiceDocument service = await AtomServiceDocument.CreateAsync(new Uri("https://example.com/atomsvc"));

foreach (AtomWorkspace workspace in service.Workspaces)
{
    foreach (AtomMemberResources collection in workspace.Collections)
    {
        Console.WriteLine($"{workspace.Title.Content}: {collection.Uri}");
    }
}
```

`AtomCategoryDocument` and `AtomEntryResource` follow the same `CreateAsync`/`Load`/`Save` shape.

## Trackback and XML-RPC

```csharp
using System.Text;
using Argotic.Net;

TrackbackClient trackback = new() { Host = new Uri("https://www.example.com/trackback/5") };

TrackbackMessage message = new(new Uri("https://www.mysite.com/post"))
{
    Encoding = Encoding.UTF8,
    WeblogName = "My Weblog",
    Title = "My Post",
    Excerpt = "An excerpt.",
};

TrackbackResponse response = await trackback.SendAsync(message);
```

```csharp
using System.Text;
using Argotic.Net;

XmlRpcClient rpc = new() { Host = new Uri("https://bob.example.net/xmlrpcserver") };

XmlRpcMessage ping = new("pingback.ping") { Encoding = Encoding.UTF8 };
ping.Parameters.Add(new XmlRpcScalarValue("https://alice.example.org/#p123"));  // sourceURI
ping.Parameters.Add(new XmlRpcScalarValue("https://bob.example.net/#foo"));     // targetURI

XmlRpcResponse response = await rpc.SendAsync(ping);
```

## Extensions

27 syndication extensions across 22 families — iTunes, Podcasting 2.0, Dublin Core, Yahoo Media, GeoRSS,
Creative Commons, the Google sitemap extensions and more — ship in
[`Argotic.Extensions`](https://www.nuget.org/packages/Argotic.Extensions), which this package already
references. They are auto-detected on load from the XML namespaces declared on the document, and the
namespace declarations written back on save are derived from the extensions actually present.

```csharp
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

if (feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension itunes)
{
    Console.WriteLine(itunes.Context.Author);
    Console.WriteLine(itunes.Context.Summary);
}
```

See the [`Argotic.Extensions` README](https://www.nuget.org/packages/Argotic.Extensions) for the full family
table, how to attach an extension before saving, and how to write your own.

## Dependency chain

```
Argotic.Common                 (no package dependencies)
    ^
Argotic.Extensions             (references Common)
    ^
Argotic.Core                   (references Common, Extensions)  <- you are here
```

`Argotic.Core` additionally references `Microsoft.Extensions.Http`, `Microsoft.Extensions.Options`,
`Microsoft.Extensions.Options.ConfigurationExtensions` and
`Microsoft.Extensions.DependencyInjection.Abstractions`.

## Links

- **Documentation / wiki** — [argotic-syndication-framework.github.io/Argotic](https://argotic-syndication-framework.github.io/Argotic)
- **Source** — [github.com/argotic-syndication-framework/Argotic](https://github.com/argotic-syndication-framework/Argotic)
- **Runnable examples** — [Solutions/Argotic.Examples](https://github.com/argotic-syndication-framework/Argotic/tree/main/Solutions/Argotic.Examples)
- **Changelog** — [CHANGELOG.md](https://github.com/argotic-syndication-framework/Argotic/blob/main/CHANGELOG.md)
- **Issues** — [github.com/argotic-syndication-framework/Argotic/issues](https://github.com/argotic-syndication-framework/Argotic/issues)

## Licence and provenance

Licensed under [Apache-2.0](https://github.com/argotic-syndication-framework/Argotic/blob/main/LICENSE).

The Argotic Syndication Framework was created in 2007 by Brian William Kuhn. It is now maintained by
[endjin](https://endjin.com), and is used in production to build the
[Azure Weekly](https://azureweekly.info), [Microsoft Fabric Weekly](https://fabricweekly.info) and
[Power BI Weekly](https://powerbiweekly.info) newsletters.