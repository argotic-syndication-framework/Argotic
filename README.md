[![Build Status](https://github.com/argotic-syndication-framework/Argotic/actions/workflows/build.yml/badge.svg)](https://github.com/argotic-syndication-framework/Argotic/actions/workflows/build.yml)
[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/argotic-syndication-framework/argotic/master/LICENSE)
[![IMM](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/total?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/total?cache=false)

# Argotic Syndication Framework

Argotic reads and writes web content syndication formats on .NET. It covers
[RSS 2.0](https://www.rssboard.org/rss-specification),
[Atom 1.0](https://www.rfc-editor.org/rfc/rfc4287),
[OPML 2.0](http://opml.org/spec2.opml),
[APML](https://en.wikipedia.org/wiki/Attention_Profiling_Mark-up_Language),
[BlogML](https://github.com/BlogML/BlogML),
[RSD](https://cyber.harvard.edu/blogs/gems/tech/rsd.html) and the
[Sitemap protocol](https://www.sitemaps.org/protocol.html), plus the
[Atom Publishing Protocol](https://www.rfc-editor.org/rfc/rfc5023), Trackback and XML-RPC. On top of
those it ships **27 syndication extensions across 22 families** — GeoRSS, Podcasting 2.0, iTunes,
Dublin Core, Yahoo Media, Creative Commons, and Google's Sitemap News/Image/Video/Hreflang among
them.

Originally created by **Brian William Kuhn in 2007**, the project became dormant and has been brought
back to life by [endjin](https://endjin.com). It is used in production to produce the
[Azure Weekly](https://azureweekly.info), [Microsoft Fabric Weekly](https://fabricweekly.info) and
[Power BI Weekly](https://powerbiweekly.info) newsletters.

> **This release is a rewrite for .NET 10, and there are many breaking changes.** The .NET Framework
> v1 idioms are gone: loads over the network are `async` and take a `CancellationToken`, `HttpClient`
> replaces `HttpWebRequest` (and can be supplied by you or by `IHttpClientFactory`),
> `WebRequestOptions` is now `SyndicationRequestOptions`, and the whole public surface is annotated
> for nullable reference types. See the [CHANGELOG](CHANGELOG.md).

*ar·got·ic* (_ahr-got-ik_) — a specialized idiomatic vocabulary peculiar to a particular class or
group of people.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — the packages target `net10.0`
  and nothing else.
- C# 14 if you are building from source. `LangVersion` is pinned to `14.0`, and features such as
  `extension` blocks and the `field` keyword are load-bearing in the implementation.

> Support for .NET Standard 2.0/2.1, .NET 8 and .NET 9 has been dropped. Use a previous package
> version if you need to target those.

## Installation

Three packages, in a straight dependency chain:

```
Argotic.Common          core interfaces and utilities, no dependencies
    ↑
Argotic.Extensions      the 27 syndication extensions
    ↑
Argotic.Core            RSS, Atom, OPML, APML, BlogML, RSD, Sitemap, AtomPub, Trackback, XML-RPC
```

**Most consumers want `Argotic.Core`** — it pulls in the other two transitively.

```bash
dotnet add package Argotic.Core
```

Reference `Argotic.Extensions` on its own only if you are building your own `SyndicationExtension`
without needing the format implementations, and `Argotic.Common` on its own only if you want the
utilities (`SyndicationDiscoveryUtility`, `SyndicationEncodingUtility`, `SyndicationDateTimeUtility`)
without either.

## Getting started

Every resource type — `RssFeed`, `AtomFeed`, `AtomEntry`, `OpmlDocument`, `ApmlDocument`,
`BlogMLDocument`, `RsdDocument`, `Sitemap`, `SitemapIndex`, `AtomServiceDocument` — exposes the same
shape, so a snippet written for one reads across to the others unchanged:

| Member                                                          | Purpose                                                                  |
|-----------------------------------------------------------------|--------------------------------------------------------------------------|
| `static Task<T> CreateAsync(Uri, …, CancellationToken)`         | Fetch and parse in one call.                                             |
| `Task LoadAsync(Uri, …, CancellationToken)`                     | Fetch into an existing instance, so you can subscribe to `Loaded` first. |
| `void Load(Stream \| XmlReader \| IXPathNavigable[, settings])` | Parse from something you already have.                                   |
| `void Save(Stream \| XmlWriter[, settings])`                    | Write it back out.                                                       |

### Start here: find out what a site publishes

If you do not already know a site's feed URL, ask the site. `LocateDiscoverableSyndicationEndpointsAsync`
reads the page and returns the `<link rel="alternate">` endpoints it advertises:

```csharp
using Argotic.Common;

IList<DiscoverableSyndicationEndpoint> endpoints =
    await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(
        new Uri("https://endjin.com/"),
        cancellationToken);

foreach (DiscoverableSyndicationEndpoint endpoint in endpoints)
{
    Console.WriteLine($"{endpoint.ContentFormat,-8} {endpoint.Source}");
}

// Atom     https://endjin.com/atom.xml
// Rss      https://endjin.com/rss.xml
```

A relative `href` — `href="/rss.xml"`, which is the common form — resolves against the address the page
came from, after redirects. `endpoint.Title` is whatever the `title` attribute held, and many sites omit
it, as endjin does here.

If you already have a URL and only need to know what it is, ask for the format alone. This reads the
first 64 KiB, not the whole document:

```csharp
SyndicationContentFormat format =
    await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
        new Uri("https://endjin.com/rss.xml"),
        cancellationToken);

// Rss
```

The same call returns `Atom` for `https://endjin.com/atom.xml` and `Sitemap` for
`https://endjin.com/sitemap.xml`. It returns `SyndicationContentFormat.None` when it cannot tell, which
includes a document whose prolog is longer than 64 KiB.

Once you know the format, construct the matching type — or hand the URL to `GenericSyndicationFeed` and
let it decide, as [below](#when-you-dont-know-whether-its-rss-or-atom).

### Read a feed from a URL

```csharp
using Argotic.Syndication;

RssFeed feed = await RssFeed.CreateAsync(
    new Uri("https://endjin.com/rss.xml"),
    cancellationToken: cancellationToken);

Console.WriteLine(feed.Channel.Title);
Console.WriteLine(feed.Channel.Description);

foreach (RssItem item in feed.Channel.Items)
{
    Console.WriteLine($"{item.PublicationDate:yyyy-MM-dd}  {item.Title}");
    Console.WriteLine($"  {item.Link}");
}
```

`Link`, `Guid`, `Source` and friends are nullable — `Uri? Link`, `RssGuid? Guid` — because RSS makes
almost every element optional and a feed in the wild will omit them.

### Read a feed from a `Stream`

```csharp
using Argotic.Syndication;

RssFeed feed = new();

using FileStream stream = File.OpenRead("feed.xml");
feed.Load(stream);
```

### Read a feed from an `XmlReader`

```csharp
using System.Xml;

using Argotic.Common;
using Argotic.Syndication;

AtomFeed feed = new();

using XmlReader reader = XmlReader.Create(
    "feed.xml",
    SyndicationEncodingUtility.CreateSafeXmlReaderSettings());

feed.Load(reader);

foreach (AtomEntry entry in feed.Entries)
{
    Console.WriteLine(entry.Title?.Content);
}
```

`CreateSafeXmlReaderSettings()` is what Argotic uses internally: DTD processing off, no external
entity resolution. Use it for any reader you hand to `Load`.

### When you don't know whether it's RSS or Atom

`GenericSyndicationFeed` exposes only what the two formats agree on — title, description, language,
categories, items — and hands you the concrete resource through `Resource` when you need the rest.

```csharp
using Argotic.Common;
using Argotic.Syndication;

GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(
    new Uri("https://endjin.com/rss.xml"),
    cancellationToken: cancellationToken);

Console.WriteLine($"{feed.Title} ({feed.Format})");

foreach (GenericSyndicationItem item in feed.Items)
{
    Console.WriteLine($"{item.PublishedOn:yyyy-MM-dd}  {item.Title}");

    foreach (GenericSyndicationCategory category in item.Categories)
    {
        Console.WriteLine($"  #{category.Term}");
    }
}

// Drop down to the format-specific object model when you need it.
if (feed.Resource is RssFeed rss)
{
    Console.WriteLine(rss.Channel.Generator);
}
```

`GenericSyndicationFeed` reads RSS, Atom and OPML. It takes `Load(Stream)` and `Load(string)` but not
`Load(XmlReader)`.

### Create a feed and save it

```csharp
using Argotic.Syndication;

RssFeed feed = new()
{
    Channel =
    {
        Title = "endjin blog",
        Link = new Uri("https://endjin.com/blog/"),
        Description = "Latest posts from the endjin blog",
        SelfLink = new Uri("https://endjin.com/rss.xml"),
    },
};

feed.Channel.Items.Add(new RssItem
{
    Title = "Polars Workloads on Microsoft Fabric",
    Link = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
    Description = "Leveraging Polars within Microsoft Fabric for efficient data transformation.",
    PublicationDate = DateTime.UtcNow,
    Guid = new RssGuid("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
});

using FileStream stream = File.Create("feed.xml");
feed.Save(stream);
```

`RssChannel.Title` and `RssChannel.Description` are required by the specification, and their setters
throw on `null` or an empty string rather than letting you save a non-conforming document.

The Atom equivalent:

```csharp
using Argotic.Syndication;

AtomFeed feed = new()
{
    Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
    Title = new AtomTextConstruct("endjin blog"),
    UpdatedOn = DateTime.UtcNow,
};

feed.Links.Add(new AtomLink(new Uri("https://endjin.com/blog/")));
feed.Links.Add(new AtomLink(new Uri("https://endjin.com/atom.xml"), "self"));
feed.Authors.Add(new AtomPersonConstruct("endjin"));

feed.Entries.Add(new AtomEntry
{
    Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
    Title = new AtomTextConstruct("Practical Polars"),
    UpdatedOn = DateTime.UtcNow,
    Summary = new AtomTextConstruct("Concrete code examples for common data workflows."),
});

using FileStream stream = File.Create("atom.xml");
feed.Save(stream);
```

### Syndication extensions

Extensions are discovered by reflection over `Argotic.Extensions`; there is no registration step. A
load looks at the XML namespaces the document declares, instantiates the matching extensions, and
attaches each one to the entity that actually carried its elements. To read one, call `FindExtension`
with the extension's static `MatchByType` predicate.

**Reading iTunes podcast metadata:**

endjin publishes an audio version of many posts, but no podcast feed, so the feed URL below is
illustrative. Every other URL in this README resolves.

```csharp
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

RssFeed feed = await RssFeed.CreateAsync(
    new Uri("https://endjin.com/podcast.xml"),
    cancellationToken: cancellationToken);

if (feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType)
    is ITunesSyndicationExtension show)
{
    Console.WriteLine($"{show.Context.Author} — {show.Context.Summary}");
    Console.WriteLine($"Artwork: {show.Context.Image}");
}

foreach (RssItem item in feed.Channel.Items)
{
    if (item.FindExtension(ITunesSyndicationExtension.MatchByType)
        is ITunesSyndicationExtension episode)
    {
        Console.WriteLine(
            $"S{episode.Context.Season}E{episode.Context.Episode} " +
            $"({episode.Context.Duration}) {episode.Context.Title}");
    }
}
```

`FindExtension` is a linear scan of `Extensions`, so hold the result rather than calling it once per
property.

**Writing them:** add a populated extension to any extensible entity and save. The `xmlns:` prefixes
on the root element are derived from the extensions actually present, so there is nothing to declare
by hand.

```csharp
using Argotic.Extensions.Core;
using Argotic.Syndication;

RssFeed feed = new()
{
    Channel =
    {
        Title = "endjin blog, read aloud",
        Link = new Uri("https://endjin.com/blog/"),
        Description = "Audio versions of selected posts from the endjin blog.",
    },
};

ITunesSyndicationExtension show = new();
show.Context.Author = "endjin";
show.Context.Summary = "Audio versions of selected posts from the endjin blog.";
show.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
show.Context.Image = new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png");
show.Context.Owner = new ITunesOwner("hello@endjin.com", "endjin");
show.Context.Categories.Add(new ITunesCategory("Technology"));
feed.Channel.Extensions.Add(show);

RssItem item = new()
{
    Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
    Link = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
    PublicationDate = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
};

// The enclosure is what makes an item an episode. Without it a podcast client has
// nothing to play, whatever the iTunes metadata says.
item.Enclosures.Add(new RssEnclosure(
    36_588_921,
    "audio/mpeg",
    new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check-new-Instrument-same-orchestra.mp3")));

ITunesSyndicationExtension episode = new();
episode.Context.Season = 1;
episode.Context.Episode = 1;
episode.Context.EpisodeType = ITunesEpisodeType.Full;
item.Extensions.Add(episode);

feed.Channel.Items.Add(item);

using FileStream stream = File.Create("podcast.xml");
feed.Save(stream);   // xmlns:itunes is written for you
```

**Podcasting 2.0** attaches the same way, and carries what iTunes has no element for. A survey of
1,934 live feeds from the Apple directory found the namespace in 1,200 of them:

```csharp
using Argotic.Extensions.Core;

PodcastSyndicationExtension show = new();
show.Context.Identifier = "917393e3-1b1e-5cef-ace4-edaa54e1f810";   // podcast:guid — stable across feed moves
show.Context.Medium = PodcastMedium.Podcast;
show.Context.IsLocked = true;                                        // no other host may import this feed
show.Context.LockOwner = "hello@endjin.com";

show.Context.People.Add(new PodcastPerson
{
    Name = "Barry Smart",
    Role = "host",
    Url = new Uri("https://endjin.com/who-we-are/"),
});

show.Context.FundingLinks.Add(new PodcastFunding
{
    Message = "Talk to endjin",
    Url = new Uri("https://endjin.com/contact-us/"),
});

// The Apple ownership token. Publishers split roughly evenly between sending it here
// and as itunes:applepodcastsverify, so reading only one form finds about half of them.
show.Context.TextEntries.Add(new PodcastText
{
    Purpose = "applepodcastsverify",
    Value = "e6cfaae0-9496-11f0-a272-f9e230f88be0",
});

feed.Channel.Extensions.Add(show);
```

Per-episode it adds transcripts, chapters and season names:

```csharp
PodcastSyndicationExtension episodeExtension = new();
episodeExtension.Context.Season = 1;
episodeExtension.Context.SeasonName = "Data and AI";
episodeExtension.Context.Episode = 1m;                  // decimal, so "Ep. 2.1" is expressible
episodeExtension.Context.Transcripts.Add(new PodcastTranscript
{
    Url = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra/transcript.vtt"),
    MediaType = "text/vtt",
    Language = "en",
    Relationship = "captions",
});

item.Extensions.Add(episodeExtension);
```

Everything above works identically for the other 25 extensions —
`DublinCoreElementSetSyndicationExtension`, `GeoRssSyndicationExtension`,
`YahooMediaSyndicationExtension`, `SitemapNewsExtension`, and the rest.

### Sitemaps

```csharp
using Argotic.Syndication;

Sitemap sitemap = new();

sitemap.Urls.Add(new SitemapUrl
{
    Location = new Uri("https://endjin.com/"),
    LastModified = DateTime.UtcNow,
    ChangeFrequency = SitemapChangeFrequency.Daily,
    Priority = 1.0m,
});

sitemap.Urls.Add(new SitemapUrl
{
    Location = new Uri("https://endjin.com/what-we-think/talks/"),
    ChangeFrequency = SitemapChangeFrequency.Monthly,
    Priority = 0.8m,
});

using FileStream stream = File.Create("sitemap.xml");
sitemap.Save(stream);
```

The protocol caps a single sitemap at 50,000 URLs, so larger sites publish a `SitemapIndex` pointing
at several files:

```csharp
using Argotic.Syndication;

SitemapIndex index = new();

index.Sitemaps.Add(new SitemapIndexEntry
{
    Location = new Uri("https://endjin.com/sitemap-news.xml"),
    LastModified = DateTime.UtcNow,
});

index.Sitemaps.Add(new SitemapIndexEntry
{
    Location = new Uri("https://endjin.com/sitemap-video.xml"),
    LastModified = DateTime.UtcNow.AddDays(-1),
});

using FileStream stream = File.Create("sitemap-index.xml");
index.Save(stream);
```

Google's sitemap extensions attach to a `SitemapUrl` the same way any other extension attaches to a
feed item:

```csharp
using Argotic.Extensions.Core;
using Argotic.Syndication;

Sitemap sitemap = new();

SitemapUrl url = new()
{
    Location = new Uri("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases"),
    LastModified = DateTime.UtcNow,
};

url.Extensions.Add(new SitemapNewsExtension
{
    Publication = new SitemapNewsPublication("endjin.com", "en"),
    PublicationDate = DateTime.UtcNow,
    Title = "Writing Effective Copilot Instructions for Complex Codebases",
});

sitemap.Urls.Add(url);
```

### Supplying your own `HttpClient`

By default every network call uses `SyndicationEncodingUtility.SharedHttpClient` — a process-wide
`Lazy<HttpClient>` over a `SocketsHttpHandler`. Every `LoadAsync` and `CreateAsync` also has an
overload taking a client you own, which is what you want for credentials, a proxy, a client
certificate, or a delegating handler for retries:

```csharp
using System.Net;

using Argotic.Syndication;

SocketsHttpHandler handler = new()
{
    Credentials = CredentialCache.DefaultNetworkCredentials,
};

using HttpClient httpClient = new(handler);

RssFeed feed = await RssFeed.CreateAsync(
    new Uri("https://endjin.com/rss.xml"),
    httpClient,
    cancellationToken: cancellationToken);
```

### Dependency injection

```csharp
using Argotic.Configuration;
using Argotic.Syndication;

using Microsoft.Extensions.DependencyInjection;

services.AddArgoticSyndicationClient();   // named HttpClient, Argotic's handler defaults
services.AddTrackbackClient();            // typed TrackbackClient
services.AddXmlRpcClient();               // typed XmlRpcClient
```

A syndication resource is constructed, not resolved — you write `new RssFeed()`, you do not ask the
container for one — so the client is registered by *name* and you hand it to `LoadAsync`:

```csharp
IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

// Always the constant, never the string. A named client is looked up by name, so a typo is not a
// compile error — the factory silently hands back a brand-new, entirely default HttpClient.
HttpClient httpClient = factory.CreateClient(ArgoticHttpClients.Syndication);

RssFeed feed = new();
await feed.LoadAsync(new Uri("https://endjin.com/rss.xml"), httpClient, cancellationToken: cancellationToken);
```

`TrackbackClient` and `XmlRpcClient` *are* services, so those are registered as typed clients and
resolved directly. Both `AddTrackbackClient` and `AddXmlRpcClient` also take an
`Action<TOptions>` or an `IConfiguration` plus a section name (`Argotic:Trackback`, `Argotic:XmlRpc`
by default).

Every client registered this way is given `Timeout.InfiniteTimeSpan`. That is not an absence of a
deadline: every deadline in Argotic is imposed by `CancellationTokenSource.CancelAfter` on a token
linked to yours, and a client-level timeout could only truncate a longer one you asked for.

### Polling: conditional GET

The workload Argotic exists for is polling many feeds repeatedly, most of which have not changed.
`SyndicationResourceReader.LoadIfModifiedAsync<T>` sends the validators you are holding, and comes
back either with a freshly parsed resource or with the fact that the origin answered `304`. Both are
successes — a `304` arrives as a result, not as an exception.

```csharp
using Argotic.Common;
using Argotic.Syndication;

// Nothing held yet, so the first request is unconditional.
SyndicationValidators validators = SyndicationValidators.None;

while (!cancellationToken.IsCancellationRequested)
{
    ConditionalLoadResult<RssFeed> result =
        await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            new Uri("https://endjin.com/rss.xml"),
            validators,
            httpClient,
            cancellationToken: cancellationToken);

    if (result.WasModified)
    {
        // No null check and no `!`: WasModified is annotated [MemberNotNullWhen(true, nameof(Resource))].
        Console.WriteLine($"{result.Resource.Channel.Items.Count} items");
    }

    // Always store what came back rather than re-sending what you had. An origin is entitled to
    // rotate an ETag on a 304, and a caller who keeps sending their original revalidates against a
    // value the origin has stopped recognising.
    validators = result.Validators;

    await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
}
```

There is a shared-client overload too — `LoadIfModifiedAsync<RssFeed>(source, validators,
cancellationToken: cancellationToken)` — but a polling caller is exactly the caller who benefits from
a handler `IHttpClientFactory` rotates.

For finer control, `SyndicationDiscoveryUtility.ConditionalGetAsync` returns a raw
`ConditionalGetResult` and hands you the live response stream to read as you see fit. Note that the
response-size limits do **not** apply on that path, by design; `LoadIfModifiedAsync` applies them.

### Load settings and request options

```csharp
using Argotic.Common;
using Argotic.Syndication;

SyndicationResourceLoadSettings settings = new()
{
    RetrievalLimit = 20,                                // build only the first 20 items
    Timeout = TimeSpan.FromSeconds(15),                 // null means no deadline at all
    MaxResponseContentLength = 4L * 1024 * 1024,        // decompressed bytes; null means the format default
    CharacterEncoding = null,                           // null means detect from BOM / XML declaration
    AutoDetectExtensions = true,
};

SyndicationRequestOptions requestOptions = new()
{
    Accept = "application/rss+xml, application/xml;q=0.9",
    UserAgent = "AzureWeekly/1.0 (+https://azureweekly.info)",
    CustomHeaders = new Dictionary<string, string> { ["X-Correlation-Id"] = correlationId },
};

RssFeed feed = await RssFeed.CreateAsync(
    new Uri("https://endjin.com/rss.xml"),
    httpClient,
    settings,
    requestOptions,
    cancellationToken);
```

Three details worth knowing, because each of them used to be a silent trap:

- **`CharacterEncoding = null` means "detect"**, and is the default. Setting it *overrides* the
  document's own declaration, which is right for a server known to lie about its encoding and wrong
  otherwise.
- **`Timeout = null` means no deadline**, not "use the default". The shared client is
  `Timeout.InfiniteTimeSpan`, so nothing but your own `CancellationToken` would bound the load.
  `TimeSpan.Zero` is not a way to spell it — it cancels immediately.
- **`MaxResponseContentLength` counts decompressed bytes.** `null` means the format's default
  (`SyndicationContentLengthLimits.Feed` is 8 MiB, `Sitemap` and `Archive` are 64 MiB); use
  `SyndicationResourceLoadSettings.Unbounded` to ask for no limit at all.

`SyndicationRequestOptions` is a `record` with `init`-only members. A value it cannot send — a
malformed `Accept`, a relative `Referer`, a content header in `CustomHeaders` — throws a
`FormatException` rather than being quietly dropped.

## What's in the box

### Formats

| Type                                                                                                       | Namespace             | Format                              |
|------------------------------------------------------------------------------------------------------------|-----------------------|-------------------------------------|
| `RssFeed`, `RssChannel`, `RssItem`                                                                         | `Argotic.Syndication` | RSS 2.0                             |
| `AtomFeed`, `AtomEntry`                                                                                    | `Argotic.Syndication` | Atom 1.0 (RFC 4287)                 |
| `OpmlDocument`                                                                                             | `Argotic.Syndication` | OPML 2.0                            |
| `Sitemap`, `SitemapIndex`                                                                                  | `Argotic.Syndication` | Sitemap 0.9                         |
| `GenericSyndicationFeed`                                                                                   | `Argotic.Syndication` | Format-agnostic wrapper             |
| `ApmlDocument`                                                                                             | `Argotic.Syndication` | APML 0.6                            |
| `BlogMLDocument`                                                                                           | `Argotic.Syndication` | BlogML 2.0                          |
| `RsdDocument`                                                                                              | `Argotic.Syndication` | RSD 1.0                             |
| `AtomServiceDocument`, `AtomCategoryDocument`, `AtomEntryResource`, `AtomWorkspace`, `AtomMemberResources` | `Argotic.Publishing`  | Atom Publishing Protocol (RFC 5023) |
| `TrackbackClient`, `XmlRpcClient`                                                                          | `Argotic.Net`         | Trackback, XML-RPC                  |

### Extensions

All 27 live in `Argotic.Extensions.Core`.

| Family                 | What it adds                                                                                         |
|------------------------|------------------------------------------------------------------------------------------------------|
| AtomPublishing         | `AtomPublishingControlSyndicationExtension`, `AtomPublishingEditedSyndicationExtension`              |
| BasicGeocoding         | W3C `geo` latitude/longitude                                                                         |
| BlogChannel            | Blog channel metadata                                                                                |
| CreativeCommons        | Creative Commons licensing                                                                           |
| DublinCore             | `DublinCoreElementSetSyndicationExtension`, `DublinCoreMetadataTermsSyndicationExtension`            |
| FeedHistory            | Feed paging and archiving (RFC 5005)                                                                 |
| FeedRank               | Feed ranking                                                                                         |
| FeedSync               | Feed synchronization                                                                                 |
| **GeoRSS**             | Geographic location, Simple and GML (OGC 17-002r1)                                                   |
| iTunes                 | Apple Podcasts metadata                                                                              |
| LiveJournal            | LiveJournal-specific elements                                                                        |
| Pheed                  | Pheed media                                                                                          |
| Pingback               | Pingback protocol                                                                                    |
| **Podcast**            | Podcasting 2.0 (`podcastindex.org/namespace/1.0`)                                                    |
| SimpleList             | Microsoft Simple List Extensions                                                                     |
| **Sitemap**            | `SitemapNewsExtension`, `SitemapImageExtension`, `SitemapVideoExtension`, `SitemapHreflangExtension` |
| SiteSummaryContent     | RSS content module                                                                                   |
| SiteSummarySlash       | Slashdot comment counts                                                                              |
| SiteSummarySyndication | RSS syndication module                                                                               |
| Trackback              | Trackback protocol                                                                                   |
| WellFormedWebComments  | Comment threading                                                                                    |
| YahooMedia             | Yahoo Media RSS                                                                                      |

Nineteen families contribute one extension each; AtomPublishing, DublinCore and Sitemap contribute
the other eight.

## Examples

`Solutions/Argotic.Examples` is an interactive [Spectre.Console](https://spectreconsole.net/) CLI
holding 77 example classes and 223 runnable examples, mirroring the Core and Extensions structure.
It is the best place to look for idiomatic present-day usage — every example compiles against the
current API and runs end-to-end in CI.

```bash
# List everything
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- list

# List one category (Common, Atom, Rss, Opml, Apml, BlogML, Rsd, Net, Generic, Sitemap,
#                     Publishing, Extensions)
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- list --category Rss

# Run one
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- run "Rss Feed - Class"

# Run them all; --skip-network omits the ones that fetch from a live origin
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- run-all --skip-network

# Interactive browser
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj
```

A PowerShell wrapper is available too:

```powershell
./run-all-examples.ps1 -SkipNetwork
./run-all-examples.ps1 -Category Rss
./run-all-examples.ps1 -JsonOutput
```

## Building and testing from source

The solution file is `Solutions/Argotic.slnx`. The build is orchestrated by
[ZeroFailed](https://github.com/zerofailed/ZeroFailed), a PowerShell framework built on
[InvokeBuild](https://github.com/nightroman/Invoke-Build), and needs PowerShell 7.0 or later.

```powershell
./build.ps1                          # compile, test, package
./build.ps1 -Clean                   # remove bin/obj first
./build.ps1 -Tasks Build             # compile only
./build.ps1 -Tasks Test              # tests with code coverage
./build.ps1 -Tasks Package           # NuGet packages
./build.ps1 -Configuration Release
```

Or straight from the .NET CLI:

```bash
# Build. Do this in both configurations -- Argotic.Benchmarks compiles only in Release.
dotnet build Solutions/Argotic.slnx -c Debug
dotnet build Solutions/Argotic.slnx -c Release

# Test. The suite uses MSTest on Microsoft Testing Platform, which requires --project.
# The filter keeps the run offline; see "Conformance testing" below.
dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj \
  --filter "TestCategory!=Integration"

# The live conformance tier: Google's sitemap schemas, the W3C Feed Validator, endjin's own feeds
dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj \
  --filter "TestCategory=Integration"

# One test
dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj \
  --filter "FullyQualifiedName~PollAFeedForChanges"

# Coverage
dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj \
  --coverage --coverage-output-format cobertura

# Formatting gates
dotnet format whitespace Solutions/Argotic.slnx --verify-no-changes --no-restore
dotnet format style      Solutions/Argotic.slnx --verify-no-changes --no-restore

# Benchmarks
dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --list flat
dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --filter '*ParsePipeline*' --job Short
```

The default test run is offline-safe: HTTP is mocked, and the two seams a mock handler cannot reach
are served over an `HttpListener` bound to 127.0.0.1 on an OS-assigned port. Nothing leaves the
machine. Fixtures are C# string literals plus the sample documents linked in from
`Argotic.Examples/SampleData`.

### Conformance testing

Tests carrying `[TestCategory("Integration")]` do reach the network, and are excluded from the
default run by the filter above. They validate this library's output against the schemas and
validators their publishers actually serve:

- **Google's sitemap extension schemas**, fetched at test time rather than vendored — each carries an
  "All Rights Reserved" notice, so checking against the real file without copying it into the
  repository is the point.
- **The W3C Feed Validator**, which is the canonical conformance checker for RSS and Atom; neither
  format has a schema .NET can validate against.
- **endjin's published feeds**, so a change at the publisher is noticed.

Offline, the sitemaps.org and APML schemas are embedded in the test assembly and validate both what
the library writes and the sample corpus. That tier is what caught `SitemapVideo` writing its
elements in an order Google's schema rejects — a defect every round-trip test agreed with, because
the reader accepts children in any order.

An integration test never passes without reaching its service: unreachable is reported inconclusive,
and only a rejected document fails.

Build output lands in `_packages/` (NuGet packages), `_codeCoverage/` (coverage reports) and
`Solutions/Argotic.Extensions.Tests/TestResults/`.

### Solution layout

```
Solutions/
├── Argotic.slnx                # solution (XML-based .slnx format)
├── Argotic.Common/             # core interfaces and utilities
├── Argotic.Extensions/         # 27 extensions across 22 families
├── Argotic.Core/               # syndication format implementations
├── Argotic.Extensions.Tests/   # MSTest suite covering all three product assemblies
├── Argotic.Examples/           # runnable examples (Spectre.Console CLI)
└── Argotic.Benchmarks/         # BenchmarkDotNet harness
```

Package versions are managed centrally in `Solutions/Directory.Packages.props`
([Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)),
so `PackageReference` items carry no `Version` attribute. `MSTest.Sdk` is pinned in `global.json`
instead.

## Documentation

- **Architecture:** [ARCHITECTURE.md](ARCHITECTURE.md) — how the library is put together and why: the
  load pipeline, format dispatch, extension discovery, the network layer, and what the .NET 10
  modernisation changed
- **Wiki:** <https://argotic-syndication-framework.github.io/Argotic>
- **Changelog:** [CHANGELOG.md](CHANGELOG.md) — the full list of breaking changes in this release
- **Repository:** <https://github.com/argotic-syndication-framework/Argotic>

## Contributing

Issues and pull requests are welcome on
[GitHub](https://github.com/argotic-syndication-framework/Argotic). Before opening a PR, please make
sure the following all pass, in both `Debug` and `Release`:

1. `dotnet build Solutions/Argotic.slnx` — zero warnings, zero errors. Nullable warnings are errors.
2. `dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj`
3. `dotnet format whitespace Solutions/Argotic.slnx --verify-no-changes --no-restore`
4. `dotnet format style Solutions/Argotic.slnx --verify-no-changes --no-restore`
5. `dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- run-all --skip-network`

Tests use [Shouldly](https://docs.shouldly.org/) exclusively — there are zero `Assert.` calls in the
suite — and hold their sample documents as C# raw string literals rather than as files on disk.

## Licence

[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/argotic-syndication-framework/argotic/master/LICENSE)

Argotic Syndication Framework is available under the Apache 2.0 open source licence.

For any licensing questions, please email [&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;](&#109;&#97;&#105;&#108;&#116;&#111;&#58;&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;)

## Code of conduct

This project has adopted a code of conduct adapted from the
[Contributor Covenant](http://contributor-covenant.org/) to clarify expected behaviour in our
community. This code of conduct has been [adopted by many other projects](http://contributor-covenant.org/adopters/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;](&#109;&#097;&#105;&#108;&#116;&#111;:&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;)
with any additional questions or comments.

## Project sponsor

This project is sponsored by [endjin](https://endjin.com), a UK based, fully-remote consultancy which
specializes in Data & Analytics, AI, and Cloud Native App Dev.

We help small teams achieve big things.

For more information about our products and services, or for commercial support of this project,
please [contact us](https://endjin.com/contact-us).

We produce three free weekly newsletters; [Azure Weekly](https://azureweekly.info) for all things
about the Microsoft Azure Platform, [Fabric Weekly](https://fabricweekly.info) for all things about
Microsoft Fabric, and [Power BI Weekly](https://powerbiweekly.info).

Keep up with everything that's going on at endjin via our [blog](https://blogs.endjin.com/), watch
our talks and tutorials on our [YouTube Channel](https://www.youtube.com/endjin), follow us on
[Bluesky](https://bsky.app/profile/endjin.com), or [LinkedIn](https://www.linkedin.com/company/1671851/).

Our other Open Source projects can be found on [our website](https://endjin.com/open-source).

## IP Maturity Model (IMM)

The [IP Maturity Model](https://github.com/endjin/Endjin.Ip.Maturity.Matrix) is endjin's IP quality framework; it defines a [configurable set of rules](https://github.com/endjin/Endjin.Ip.Maturity.Matrix.RuleDefinitions), which are committed into the [root of a repo](imm.yaml), and a [Azure Function HttpTrigger](https://github.com/endjin/Endjin.Ip.Maturity.Matrix/tree/master/Solutions/Endjin.Ip.Maturity.Matrix.Host) which can evaluate the ruleset, and render an svg badge for display in repo's `readme.md`.

This approach is based on our 15+ years experience of delivering complex, high performance, bleeding-edge projects, and due diligence assessments of 3rd party systems. For detailed information about the ruleset see the [IP Maturity Model repo](https://github.com/endjin/Endjin.Ip.Maturity.Matrix).

### IMM for Argotic

[![Shared Engineering Standards](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/74e29f9b-6dca-4161-8fdd-b468a1eb185d?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/74e29f9b-6dca-4161-8fdd-b468a1eb185d?cache=false)

[![Coding Standards](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f6f6490f-9493-4dc3-a674-15584fa951d8?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f6f6490f-9493-4dc3-a674-15584fa951d8?cache=false)

[![Executable Specifications](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/bb49fb94-6ab5-40c3-a6da-dfd2e9bc4b00?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/bb49fb94-6ab5-40c3-a6da-dfd2e9bc4b00?cache=false)

[![Code Coverage](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/0449cadc-0078-4094-b019-520d75cc6cbb?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/0449cadc-0078-4094-b019-520d75cc6cbb?cache=false)

[![Benchmarks](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/64ed80dc-d354-45a9-9a56-c32437306afa?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/64ed80dc-d354-45a9-9a56-c32437306afa?cache=false)

[![Reference Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/2a7fc206-d578-41b0-85f6-a28b6b0fec5f?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/2a7fc206-d578-41b0-85f6-a28b6b0fec5f?cache=false)

[![Design & Implementation Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f026d5a2-ce1a-4e04-af15-5a35792b164b?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f026d5a2-ce1a-4e04-af15-5a35792b164b?cache=false)

[![How-to Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/145f2e3d-bb05-4ced-989b-7fb218fc6705?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/145f2e3d-bb05-4ced-989b-7fb218fc6705?cache=false)

[![Date of Last IP Review](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/da4ed776-0365-4d8a-a297-c4e91a14d646?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/da4ed776-0365-4d8a-a297-c4e91a14d646?cache=false)

[![Framework Version](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/6c0402b3-f0e3-4bd7-83fe-04bb6dca7924?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/6c0402b3-f0e3-4bd7-83fe-04bb6dca7924?cache=false)

[![Associated Work Items](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/79b8ff50-7378-4f29-b07c-bcd80746bfd4?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/79b8ff50-7378-4f29-b07c-bcd80746bfd4?cache=false)

[![Source Code Availability](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/30e1b40b-b27d-4631-b38d-3172426593ca?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/30e1b40b-b27d-4631-b38d-3172426593ca?cache=false)

[![License](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/d96b5bdc-62c7-47b6-bcc4-de31127c08b7?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/d96b5bdc-62c7-47b6-bcc4-de31127c08b7?cache=false)

[![Production Use](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/87ee2c3e-b17a-4939-b969-2c9c034d05d7?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/87ee2c3e-b17a-4939-b969-2c9c034d05d7?cache=false)

[![Insights](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/71a02488-2dc9-4d25-94fa-8c2346169f8b?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/71a02488-2dc9-4d25-94fa-8c2346169f8b?cache=false)

[![Packaging](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/547fd9f5-9caf-449f-82d9-4fba9e7ce13a?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/547fd9f5-9caf-449f-82d9-4fba9e7ce13a?cache=false)

[![Deployment](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/edea4593-d2dd-485b-bc1b-aaaf18f098f9?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/edea4593-d2dd-485b-bc1b-aaaf18f098f9?cache=false)
