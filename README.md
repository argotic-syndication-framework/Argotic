[![Build Status](https://github.com/argotic-syndication-framework/Argotic/actions/workflows/build.yml/badge.svg)](https://github.com/argotic-syndication-framework/Argotic/actions/workflows/build.yml)
[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/argotic-syndication-framework/argotic/master/LICENSE)
[![IMM](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/total?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/total?cache=false)

# Argotic Syndication Framework

Argotic is a .NET library that reads and writes web content syndication formats. It supports
[RSS 2.0](https://www.rssboard.org/rss-specification),
[Atom 1.0](https://www.rfc-editor.org/rfc/rfc4287),
[OPML 2.0](http://opml.org/spec2.opml),
[APML](https://en.wikipedia.org/wiki/Attention_Profiling_Mark-up_Language),
[BlogML](https://github.com/BlogML/BlogML),
[RSD](https://cyber.harvard.edu/blogs/gems/tech/rsd.html) and the
[Sitemap protocol](https://www.sitemaps.org/protocol.html). It also implements the
[Atom Publishing Protocol](https://www.rfc-editor.org/rfc/rfc5023), Trackback and XML-RPC. The
library includes **27 syndication extensions in 22 families**: GeoRSS, Podcasting 2.0, iTunes,
Dublin Core, Yahoo Media, Creative Commons, Google's Sitemap News/Image/Video/Hreflang, and more.

Brian William Kuhn created Argotic in **2007**, and the project later became dormant.
[endjin](https://endjin.com) now maintains it, and uses it in production to produce the
[Azure Weekly](https://azureweekly.info), [Microsoft Fabric Weekly](https://fabricweekly.info) and
[Power BI Weekly](https://powerbiweekly.info) newsletters.

> **This release is a rewrite for .NET 10 and contains many breaking changes.** Loads across the
> network are now `async` and take a `CancellationToken`. `HttpClient` replaces `HttpWebRequest`,
> and you can supply the client yourself or through `IHttpClientFactory`. `WebRequestOptions` is
> now `SyndicationRequestOptions`. The full public API has nullable reference type annotations.
> The [CHANGELOG](CHANGELOG.md) lists each change.

*ar·got·ic* (_ahr-got-ik_) — a specialized idiomatic vocabulary peculiar to a particular class or
group of people.

## Requirements

- The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). The packages target
  `net10.0` and no other framework.
- C# 14, if you build from source. `LangVersion` is pinned to `14.0`, and the implementation uses
  `extension` blocks and the `field` keyword.

> This release does not support .NET Standard 2.0/2.1, .NET 8 or .NET 9. To target those
> frameworks, use an earlier package version.

## Installation

Argotic has three packages in one dependency chain:

```
Argotic.Common          core interfaces and utilities, no dependencies
    ↑
Argotic.Extensions      the 27 syndication extensions
    ↑
Argotic.Core            RSS, Atom, OPML, APML, BlogML, RSD, Sitemap, AtomPub, Trackback, XML-RPC
```

**Most applications need only `Argotic.Core`.** It includes the other two packages transitively.

```bash
dotnet add package Argotic.Core
```

Reference `Argotic.Extensions` alone to build your own `SyndicationExtension` without the format
implementations. Reference `Argotic.Common` alone to use only the utilities
(`SyndicationDiscoveryUtility`, `SyndicationEncodingUtility`, `SyndicationDateTimeUtility`).

## Getting started

Each resource type — `RssFeed`, `AtomFeed`, `AtomEntry`, `OpmlDocument`, `ApmlDocument`,
`BlogMLDocument`, `RsdDocument`, `Sitemap`, `SitemapIndex`, `AtomServiceDocument` — has the same
four members. Code that you write for one type applies to the others without change:

| Member                                                          | Purpose                                                                 |
|-----------------------------------------------------------------|-------------------------------------------------------------------------|
| `static Task<T> CreateAsync(Uri, …, CancellationToken)`         | Fetch and parse in one call.                                            |
| `Task LoadAsync(Uri, …, CancellationToken)`                     | Fetch into an existing instance. This lets you subscribe to `Loaded` first. |
| `void Load(Stream \| XmlReader \| IXPathNavigable[, settings])` | Parse a document that you already have.                                 |
| `void Save(Stream \| XmlWriter[, settings])`                    | Write the document out.                                                 |

If you give `Load` your own `XmlReader`, create it with
`SyndicationEncodingUtility.CreateSafeXmlReaderSettings()`. Argotic uses these settings
internally: DTD processing is off, and external entities do not resolve.

### Read a feed

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

`Link`, `Guid` and `Source` are nullable — `Uri? Link`, `RssGuid? Guid`. RSS makes almost every
element optional, and live feeds frequently omit these elements.

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

The RSS specification requires `RssChannel.Title` and `RssChannel.Description`. Their setters
throw on `null` or an empty string, so you cannot save a document that does not conform.

### Use a syndication extension

Argotic discovers extensions by reflection over `Argotic.Extensions`; there is no registration
step. On load, Argotic reads the XML namespaces that the document declares, creates the applicable
extensions, and attaches each one to the entity that carried its elements. On save, Argotic
derives the `xmlns:` declarations from the extensions that are attached, so you do not declare a
namespace by hand.

To read an extension, call `FindExtension` with the extension's static `MatchByType` predicate. To
write one, add a populated extension to an extensible entity, then save. In this snippet, `feed`
is an `RssFeed` from the sections above:

```csharp
using Argotic.Extensions.Core;
using Argotic.Syndication;

// Read: get the iTunes metadata, if the feed carries it.
if (feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType)
    is ITunesSyndicationExtension show)
{
    Console.WriteLine($"{show.Context.Author} — {show.Context.Summary}");
}

// Write: attach a populated extension, then save.
ITunesSyndicationExtension podcast = new();
podcast.Context.Author = "endjin";
podcast.Context.Owner = new ITunesOwner("hello@endjin.com", "endjin");
podcast.Context.Categories.Add(new ITunesCategory("Technology"));
feed.Channel.Extensions.Add(podcast);

using FileStream output = File.Create("podcast.xml");
feed.Save(output);   // Argotic writes xmlns:itunes for you
```

`FindExtension` does a linear scan of `Extensions`. Hold the result; do not call `FindExtension`
once for each property.

The other 26 extensions attach in the same way: `PodcastSyndicationExtension`,
`DublinCoreElementSetSyndicationExtension`, `GeoRssSyndicationExtension`, `SitemapNewsExtension`,
and the rest.

### Dependency injection

By default, each network call uses `SyndicationEncodingUtility.SharedHttpClient`: a process-wide
`Lazy<HttpClient>` over a `SocketsHttpHandler`. Each `LoadAsync` and `CreateAsync` also has an
overload that takes a client that you own. Use that overload for credentials, a proxy, a client
certificate, or a delegating handler. With `Microsoft.Extensions.DependencyInjection`, register
the clients as follows:

```csharp
using Argotic.Configuration;
using Argotic.Syndication;

using Microsoft.Extensions.DependencyInjection;

services.AddArgoticSyndicationClient();   // named HttpClient, Argotic's handler defaults
services.AddTrackbackClient();            // typed TrackbackClient
services.AddXmlRpcClient();               // typed XmlRpcClient
```

A syndication resource is constructed, not resolved: you write `new RssFeed()`, and you do not ask
the container for one. The syndication client is therefore registered by *name*. Get the client
from the factory and give it to `LoadAsync`:

```csharp
IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

// Use the constant, not the string. The factory does not validate the name: for an unknown
// name, it returns a new HttpClient with default settings.
HttpClient httpClient = factory.CreateClient(ArgoticHttpClients.Syndication);

RssFeed feed = new();
await feed.LoadAsync(new Uri("https://endjin.com/rss.xml"), httpClient, cancellationToken: cancellationToken);
```

`TrackbackClient` and `XmlRpcClient` *are* services. They are registered as typed clients, and you
resolve them directly. `AddTrackbackClient` and `AddXmlRpcClient` also take an `Action<TOptions>`,
or an `IConfiguration` plus a section name (`Argotic:Trackback` and `Argotic:XmlRpc` by default).

Each client that these methods register gets `Timeout.InfiniteTimeSpan`. This is not an absence of
a deadline. Argotic applies each deadline with `CancellationTokenSource.CancelAfter`, on a token
linked to yours. A client-level timeout could only truncate a longer deadline that you asked for.

### More topics

The tutorial continues in [`Solutions/Samples`](Solutions/Samples). Each of these topics has a
runnable, commented program there:

- Feed discovery, your own `HttpClient`, load settings and limits, and conditional GET
  (samples 14–17).
- Sitemaps and Google's sitemap extensions (samples 04 and 12).
- Format detection with `GenericSyndicationFeed` (sample 05).
- Podcast feeds with iTunes and Podcasting 2.0 metadata (sample 11).
- Your own custom extension (sample 13).

See [Samples](#samples) for how the directory is organised.

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

All 27 extensions are in the `Argotic.Extensions.Core` namespace.

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

Nineteen families contain one extension each. AtomPublishing, DublinCore and Sitemap contain the
other eight.

## Samples

[`Solutions/Samples`](Solutions/Samples) contains 21 single-file programs. Each sample is one
`.cs` file, with no project to restore. One command runs a sample:

```bash
dotnet run --file Solutions/Samples/01-rss-feed.cs
```

The samples are .NET 10
[file-based apps](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk). They form a
course in five parts: documents, persistence, extensions, the network, and beyond the feed. Each
file explains why before how, and prints its own evidence, so you can compare the prose with the
output. Read the files in order, or use the "start here" table in
[the index](Solutions/Samples/README.md) to find the file that matches your problem.

No sample touches the network. Samples 01–13 open no sockets. Samples 14–21 serve themselves over
an `HttpListener` on 127.0.0.1. The full set therefore runs in CI at every commit:

```powershell
./run-samples.ps1
```

## Examples

[`Solutions/Argotic.Examples`](Solutions/Argotic.Examples) is an interactive
[Spectre.Console](https://spectreconsole.net/) CLI that contains 223 runnable examples in 77
classes, and it mirrors the Core and Extensions structure. The samples are a course that you read;
the examples are a reference that you query. The examples cover every format, all 27 extensions,
and every overload. Each example compiles against the current API and runs end-to-end in CI.

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

A PowerShell wrapper is also available:

```powershell
./run-all-examples.ps1 -SkipNetwork
./run-all-examples.ps1 -Category Rss
./run-all-examples.ps1 -JsonOutput
```

## Building and testing from source

The solution file is `Solutions/Argotic.slnx`.
[ZeroFailed](https://github.com/zerofailed/ZeroFailed), a PowerShell framework built on
[InvokeBuild](https://github.com/nightroman/Invoke-Build), orchestrates the build. The build
requires PowerShell 7.0 or later.

```powershell
./build.ps1                          # compile, test, package
./build.ps1 -Clean                   # remove bin/obj first
./build.ps1 -Tasks Build             # compile only
./build.ps1 -Tasks Test              # tests with code coverage
./build.ps1 -Tasks Package           # NuGet packages
./build.ps1 -Configuration Release
```

Or use the .NET CLI directly:

```bash
# Build in both configurations -- Argotic.Benchmarks compiles only in Release.
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

# Samples. The gates above cannot see these: they are file-based apps, they are not in the
# .slnx, and no csproj compiles them.
./run-samples.ps1

# Benchmarks
dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --list flat
dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --filter '*ParsePipeline*' --job Short
```

The default test run is offline. The tests mock HTTP, and for the two seams that a mock handler
cannot reach, an `HttpListener` on 127.0.0.1 serves the responses from an OS-assigned port.
Nothing leaves the machine. The fixtures are C# string literals, plus the sample documents linked
in from `Argotic.Examples/SampleData`.

### Conformance testing

Tests with `[TestCategory("Integration")]` do reach the network; the filter above excludes them
from the default run. These tests validate the library's output against the schemas and
validators that the publishers serve:

- **Google's sitemap extension schemas.** The tests fetch each schema at test time and do not copy
  it into the repository. Each schema carries an "All Rights Reserved" notice, so a check against
  the real file, without a copy, is the point.
- **The W3C Feed Validator** — the canonical conformance checker for RSS and Atom. Neither format
  has a schema that .NET can validate against.
- **endjin's published feeds**, so that the tests notice a change at the publisher.

Offline, the sitemaps.org and APML schemas are embedded in the test assembly. They validate the
documents that the library writes, and also the sample corpus. This tier caught the
`SitemapVideo` writer: it wrote its elements in an order that Google's schema rejects, and every
round-trip test agreed with it, because the reader accepts children in any order.

An integration test never passes without a connection to its service. An unreachable service is
reported as inconclusive. Only a rejected document is a failure.

Build output goes to `_packages/` (NuGet packages), `_codeCoverage/` (coverage reports) and
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
├── Argotic.Benchmarks/         # BenchmarkDotNet harness
└── Samples/                    # 21 single-file apps; not a project, not in the solution
```

`Solutions/Directory.Packages.props` holds all package versions
([Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)),
so `PackageReference` items carry no `Version` attribute. `MSTest.Sdk` is pinned in `global.json`.

## Documentation

- **Architecture:** [ARCHITECTURE.md](ARCHITECTURE.md) — how the library works and why: the load
  pipeline, format dispatch, extension discovery, the network layer, and what the .NET 10
  modernisation changed
- **Wiki:** <https://argotic-syndication-framework.github.io/Argotic>
- **Changelog:** [CHANGELOG.md](CHANGELOG.md) — the full list of breaking changes in this release
- **Repository:** <https://github.com/argotic-syndication-framework/Argotic>

## Contributing

Issues and pull requests are welcome on
[GitHub](https://github.com/argotic-syndication-framework/Argotic). Before you open a PR, run the
commands in [Building and testing from source](#building-and-testing-from-source) and make sure
that each gate passes, in both `Debug` and `Release`:

1. The build completes with zero warnings and zero errors. Nullable warnings are errors.
2. The test suite passes.
3. Both `dotnet format` gates pass.
4. The examples run: `run-all --skip-network`.
5. The samples run: `./run-samples.ps1`.

Tests use [Shouldly](https://docs.shouldly.org/) for assertions. The one exception is
`Assert.Inconclusive`, which only the integration tier uses, to report that a live service was
not reachable.

## Licence

[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/argotic-syndication-framework/argotic/master/LICENSE)

The Argotic Syndication Framework is available under the Apache 2.0 open source licence.

For licensing questions, email [&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;](&#109;&#97;&#105;&#108;&#116;&#111;&#58;&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;)

## Code of conduct

This project uses a code of conduct adapted from the
[Contributor Covenant](http://contributor-covenant.org/). The code of conduct states the
behaviour that we expect in our community, and
[many other projects](http://contributor-covenant.org/adopters/) have adopted it. For more
information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;](&#109;&#097;&#105;&#108;&#116;&#111;:&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;)
with questions or comments.

## Project sponsor

[endjin](https://endjin.com) sponsors this project. We are a UK-based, fully-remote consultancy
that specializes in Data & Analytics, AI, and Cloud Native App Dev.

We help small teams achieve big things.

For more information about our products and services, or for commercial support of this project,
[contact us](https://endjin.com/contact-us).

We produce three free weekly newsletters: [Azure Weekly](https://azureweekly.info) for the
Microsoft Azure platform, [Fabric Weekly](https://fabricweekly.info) for Microsoft Fabric, and
[Power BI Weekly](https://powerbiweekly.info).

Follow endjin on our [blog](https://blogs.endjin.com/), our
[YouTube channel](https://www.youtube.com/endjin),
[Bluesky](https://bsky.app/profile/endjin.com) and
[LinkedIn](https://www.linkedin.com/company/1671851/).

Our other open-source projects are listed on [our website](https://endjin.com/open-source).

## IP Maturity Model (IMM)

The [IP Maturity Model](https://github.com/endjin/Endjin.Ip.Maturity.Matrix) is endjin's IP
quality framework. It defines a
[configurable set of rules](https://github.com/endjin/Endjin.Ip.Maturity.Matrix.RuleDefinitions),
which are committed into the [root of a repo](imm.yaml). An
[Azure Function](https://github.com/endjin/Endjin.Ip.Maturity.Matrix/tree/master/Solutions/Endjin.Ip.Maturity.Matrix.Host)
evaluates the ruleset and renders an SVG badge for display in the repo's README.

The approach comes from more than 15 years of delivery of complex, high-performance projects, and
from due-diligence assessments of third-party systems. For detailed information about the
ruleset, see the [IP Maturity Model repo](https://github.com/endjin/Endjin.Ip.Maturity.Matrix).

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
