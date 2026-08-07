# Argotic.Common

The foundation package of the **Argotic Syndication Framework** — a .NET web content syndication library
supporting RSS, Atom, OPML, APML, BlogML, RSD and Sitemap/SitemapIndex, plus the Atom Publishing Protocol.

`Argotic.Common` has **no package dependencies**. It holds the `ISyndicationResource` contract that every
format implements, the settings and options objects that configure a load or a save, and three static
utility classes that are useful entirely on their own: encoding, RFC date handling, and feed discovery.

> ### Most people want `Argotic.Core`, not this package
>
> [`Argotic.Core`](https://www.nuget.org/packages/Argotic.Core) references `Argotic.Common`, so installing
> `Argotic.Core` brings every type described here in transitively. Reference `Argotic.Common` **directly**
> only when one of these is true:
>
> - You are **implementing your own syndication resource type** against `ISyndicationResource` and want the
>   contract without the shipped object model.
> - You want the **discovery and date utilities standalone** — sniffing what format a URL serves, finding
>   the feeds an HTML page advertises, doing a conditional GET, or parsing RFC 822 / RFC 3339 timestamps —
>   without taking a dependency on the feed parsers.

## Install

```bash
dotnet add package Argotic.Common
```

Target framework: **`net10.0`**. Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
or later. Earlier targets (.NET Standard 2.0/2.1, .NET 8, .NET 9) are not supported by this release.

## What is in the package

| Type                              | Namespace        | Purpose                                                                                                                |
|-----------------------------------|------------------|------------------------------------------------------------------------------------------------------------------------|
| `ISyndicationResource`            | `Argotic.Common` | The contract every format implements — `Load`, `LoadAsync`, `Save`, `CreateNavigator`, `Loaded`                        |
| `SyndicationEncodingUtility`      | `Argotic.Common` | Encoding detection, safe `XmlReader`/`XmlWriter` settings, safe `XPathNavigator` construction, the shared `HttpClient` |
| `SyndicationDateTimeUtility`      | `Argotic.Common` | RFC 822 (RSS) and RFC 3339 (Atom) date parsing and formatting                                                          |
| `SyndicationDiscoveryUtility`     | `Argotic.Common` | Format sniffing, feed autodiscovery, conditional GET, Pingback/Trackback endpoint location                             |
| `SyndicationResourceLoadSettings` | `Argotic.Common` | Timeout, retrieval limit, response size cap, encoding, extension registration                                          |
| `SyndicationResourceSaveSettings` | `Argotic.Common` | Output encoding, minimisation, extension namespaces                                                                    |
| `SyndicationRequestOptions`       | `Argotic.Common` | Per-request `Accept`, `User-Agent`, `Referer` and custom headers                                                       |
| `ConditionalGetResult`            | `Argotic.Common` | The result of an `If-Modified-Since` / `If-None-Match` fetch                                                           |
| `DiscoverableSyndicationEndpoint` | `Argotic.Common` | A feed advertised by an HTML page's `<link rel="alternate">`                                                           |
| `SyndicationContentFormat`        | `Argotic.Common` | The format enumeration — `Rss`, `Atom`, `Opml`, `Apml`, `BlogML`, `Rsd`, `Sitemap`, …                                  |
| `SyndicationResourceMetadata`     | `Argotic.Common` | Format, version and in-scope namespaces read off a navigator                                                           |

## Parsing syndication dates

RSS dates are RFC 822; Atom dates are RFC 3339. Both spellings appear in the wild with a dozen variations
each, and `DateTime.Parse` handles neither reliably. These two families do.

```csharp
using Argotic.Common;

// RFC 822 — the RSS <pubDate> form.
if (SyndicationDateTimeUtility.TryParseRfc822DateTime("Mon, 14 Oct 2007 05:00:00 GMT", out DateTime published))
{
    // Parsed values are always DateTimeKind.Utc, whatever offset the origin wrote.
    Console.WriteLine(published.Kind);   // Utc
}

// RFC 3339 — the Atom <updated> form. "Z" and "+00:00" both yield Kind=Utc at the same instant.
DateTime updated = SyndicationDateTimeUtility.ParseRfc3339DateTime("2003-12-13T18:30:02Z");

// Going the other way.
string rss  = SyndicationDateTimeUtility.ToRfc822DateTime(updated);   // "Sat, 13 Dec 2003 18:30:02 GMT"
string atom = SyndicationDateTimeUtility.ToRfc3339DateTime(updated);  // "2003-12-13T18:30:02.00Z"
```

`ToRfc822DateTime` publishes the value's components verbatim under a literal `GMT` and does **not** consult
`DateTime.Kind` — call `ToUniversalTime()` first, or pass back what `TryParseRfc822DateTime` produced.
`ToRfc3339DateTime` writes a `DateTimeKind.Local` value with its numeric offset and everything else with `Z`.

The `Try…` overloads never throw; the `Parse…` overloads throw `FormatException` on an unrecognised value
and `ArgumentException` on an empty one.

## Feed autodiscovery from an HTML page

Given a site's home page, find the feeds it advertises through `<link rel="alternate">`.

```csharp
using Argotic.Common;

Uri page = new("https://www.dotnetrocks.com/");

IList<DiscoverableSyndicationEndpoint> endpoints =
    await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(page);

foreach (DiscoverableSyndicationEndpoint endpoint in endpoints)
{
    Console.WriteLine($"{endpoint.ContentFormat}: {endpoint.Title} -> {endpoint.Source}");
}
```

If you already hold the markup, parse it directly. **Pass the base URI**: `href="/feed.xml"` is the commonest
form an autodiscovery link takes, and without a base URI the endpoint's `Source` stays relative and cannot be
fetched.

```csharp
IList<DiscoverableSyndicationEndpoint> endpoints =
    SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(html, baseUri: page);
```

Each endpoint can build a navigator over its own content without any of the feed parsers:

```csharp
XPathNavigator navigator = await endpoints[0].CreateNavigatorAsync();
```

## Sniffing what a URL actually serves

```csharp
using Argotic.Common;

SyndicationContentFormat format =
    await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(new Uri("https://endjin.com/rss.xml"));

if (format == SyndicationContentFormat.Rss)
{
    // Hand it to Argotic.Core's RssFeed, or to your own parser.
}
```

`SyndicationContentFormatGet` has synchronous overloads taking a `Stream`, an `XmlReader` or an
`XPathNavigator` when the content is already in hand.

## Conditional GET

Re-fetch a resource only when it changed, using the modification date and entity tag from the previous
response. `ConditionalGetResult` is disposable and owns the response.

```csharp
using Argotic.Common;

Uri source = new("https://endjin.com/rss.xml");

// Both come from your store, recorded by the previous fetch.
DateTime lastModified = previousLastModified;   // DateTime.MinValue on the first ever fetch
string? entityTag = previousETag;               // null if the origin sent no ETag

using ConditionalGetResult result =
    await SyndicationDiscoveryUtility.ConditionalGetAsync(source, lastModified, entityTag);

if (result.WasModified)
{
    using Stream stream = await result.GetResponseStreamAsync();
    // Parse the new content, then persist result.LastModified and result.ETag for next time.
}
```

## Does this URL exist?

```csharp
bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(new Uri("https://endjin.com/rss.xml"));
```

Returns `false` rather than throwing when the URL is inaccessible. Requests are subject to a 100-second
default deadline; a timed-out request counts as inaccessible.

## Load settings, request options and the shared `HttpClient`

Every asynchronous load in the framework is bounded by a `CancellationTokenSource.CancelAfter` deadline, not
by `HttpClient.Timeout` — the shared client is deliberately built with `Timeout.InfiniteTimeSpan`, so reading
its `Timeout` tells you nothing. `SyndicationEncodingUtility.DefaultRequestTimeout` (100 seconds) is the value
that does.

```csharp
using Argotic.Common;

SyndicationResourceLoadSettings settings = new()
{
    Timeout = TimeSpan.FromSeconds(30),   // null means *no* deadline, not "use the default"
    RetrievalLimit = 25,                  // 0 means unlimited
    MaxResponseContentLength = 8 * 1024 * 1024,
    AutoDetectExtensions = true,
};

SyndicationRequestOptions options = new()
{
    UserAgent = "MyAggregator/1.0",
    Accept = "application/rss+xml, application/atom+xml;q=0.9",
    CustomHeaders = new Dictionary<string, string> { ["X-Correlation-Id"] = Guid.NewGuid().ToString() },
};
```

The process-wide client is `SyndicationEncodingUtility.SharedHttpClient` — a `Lazy<HttpClient>` over a
`SocketsHttpHandler` that rotates its own pooled connections. Every `LoadAsync`/`CreateAsync`/discovery entry
point also has an overload taking a caller-supplied `HttpClient`, which is what you want with
`IHttpClientFactory`, a proxy, client certificates or a retry handler.

## Implementing your own syndication resource

`ISyndicationResource` is the whole contract. Implement it and your type slots into the same shape as every
shipped format. `SyndicationContentFormat` already names several formats the framework does not implement —
`NewsML`, `MicroSummaryGenerator`, `OpenSearchDescription`, `Rdf` — so there is a value to return.

```csharp
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

public sealed class NewsMLDocument : ISyndicationResource
{
    // A fixed value per implementing type: it reports what the type *is*, not what was parsed,
    // so it never returns SyndicationContentFormat.None.
    public SyndicationContentFormat Format => SyndicationContentFormat.NewsML;

    public Version Version => new(1, 0);

    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    public XPathNavigator CreateNavigator()
    {
        using MemoryStream stream = new();

        using (XmlWriter writer = XmlWriter.Create(stream, SyndicationEncodingUtility.CreateDocumentXmlWriterSettings()))
        {
            this.Save(writer);
            writer.Flush();
        }

        stream.Seek(0, SeekOrigin.Begin);

        using XmlReader reader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        return new XPathDocument(reader).CreateNavigator();
    }

    public void Load(IXPathNavigable source) => this.Load(source, null);

    public void Load(IXPathNavigable source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Fill your object model from source.CreateNavigator(), then raise Loaded.
        this.Loaded?.Invoke(this, new SyndicationResourceLoadedEventArgs(source));
    }

    // …plus the Load(Stream), Load(XmlReader), LoadAsync(Uri, …), Save(Stream) and Save(XmlWriter) overloads.
}
```

Three rules the interface documents and every shipped implementation honours:

1. The parameterless overloads delegate to the `settings`-taking overload with `null`.
2. `Loaded` is raised **after** a successful load, and not at all when one throws.
3. On a `FormatException` or `XmlException` the resource is left empty rather than half-filled.

`SyndicationEncodingUtility.CreateSafeXmlReaderSettings()` and the `CreateSafeNavigator` overloads give you
the same hardened XML reader configuration the framework uses internally.

## Dependency chain

```
Argotic.Common                 (no package dependencies)  <- you are here
    ^
Argotic.Extensions             (references Common)
    ^
Argotic.Core                   (references Common, Extensions)
```

| Package                                                                 | Install                                 |
|-------------------------------------------------------------------------|-----------------------------------------|
| [Argotic.Common](https://www.nuget.org/packages/Argotic.Common)         | `dotnet add package Argotic.Common`     |
| [Argotic.Extensions](https://www.nuget.org/packages/Argotic.Extensions) | `dotnet add package Argotic.Extensions` |
| [Argotic.Core](https://www.nuget.org/packages/Argotic.Core)             | `dotnet add package Argotic.Core`       |

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