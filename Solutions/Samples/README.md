# Samples

```bash
dotnet run --file Solutions/Samples/01-rss-feed.cs
```

That is the whole setup. There is no project to restore, no runner to learn, and no shared helper
to read first — each file here is a complete .NET 10 program that you read top to bottom and run
with one command.

The examples project next door answers "show me the call": 223 methods, uniform and complete, that
you query by name. This directory answers a different question — "I have never used this library,
teach me the model" — so the comments carry more weight than the code, and the two artefacts
deliberately share nothing but the types they touch.

## Before you start

These are [file-based apps](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk), new
in .NET 10: a `.cs` file with `#:` directives at the top, compiled through a virtual project the
SDK builds for you. The directive every sample carries is

```csharp
#:project ../Argotic.Core/Argotic.Core.csproj
```

`Argotic.Common` and `Argotic.Extensions` come along transitively, so one line covers the whole
surface. To run a sample **outside** this repository, swap that line for `#:package Argotic.Core`
— which is what a consumer writes, and which does not work in-place here for two reasons: this
repository has central package management switched on, so a version-pinned `#:package` is an
NU1008 error, and the .NET 10 rewrite is not published yet.

**Nothing here touches the network.** Samples 01–13 open no sockets at all. Samples 14–21 need
HTTP, so each starts an `HttpListener` on `127.0.0.1` with an OS-assigned port, serves itself, and
shuts it down. That is not squeamishness — it is what lets the whole directory run in a build, at
every commit, with no third party able to break it.

## The one API shape

Learn this and most of the library follows. Every one of the eleven document types exposes the
same four members:

| | |
|---|---|
| `static Task<T> CreateAsync(Uri, …)` | fetch and load in one call |
| `Task LoadAsync(Uri, …)` | fill an instance you already hold, so you can subscribe to `Loaded` first |
| `void Load(IXPathNavigable \| Stream \| XmlReader, …)` | load from something already in memory |
| `void Save(Stream \| XmlWriter, …)` | write it out |

Three absences are as informative as the four members, and all three are deliberate:

- **no synchronous `Create`** — a blocking network call hidden behind a method that looks like the
  others is how thread pools starve;
- **no `SaveAsync`** — serialisation is CPU work over an in-memory graph, and the only part that
  blocks is the stream you supplied;
- **no `Save(string path)`** — a path would mean the library owns the opening, the share mode, the
  encoding and the disposal, and it has no basis to decide any of them.

## Start here

You do not have to read all of these, and you should not read them in order unless you have twenty
minutes and no particular problem.

| If you… | read |
|---|---|
| have a URL and want its contents | **01**, then **14** |
| have bytes and do not know what they are | **05** |
| are publishing a feed rather than consuming one | **01**, **06**, **10** |
| are polling a lot of feeds and want it to be cheap | **14**, **15**, **17** |
| are debugging "my podcast feed reads nothing" | **11**, section 4 |
| need a namespace nobody has standardised | **13** |
| have twenty minutes and no specific problem | in order |

## The five movements

**I. Documents (01–05).** What the object model *is*, one format at a time, from XML embedded in
each file. Every sample both reads and builds, because a model you have only read is a model you
have half learnt.

**II. Persistence (06–08).** The boundary where an object graph becomes bytes somebody else will
parse — encodings, dates, and what actually survives a round trip.

**III. Extensions (09–13).** The half of the library that is not in `Argotic.Syndication`. A real
feed is mostly not RSS, and this is how the rest of it becomes objects.

**IV. The network (14–17).** Nothing before 14 opens a socket. That boundary is where it is because
a networking sample should be about HTTP, and it can only be about HTTP once you already know what
a feed is.

**V. Beyond the feed (18–21).** Publishing, notification, the long-tail formats, and one capstone
that uses the lot.

## The samples

| # | File | |
|---|---|---|
| 01 | [`01-rss-feed.cs`](01-rss-feed.cs) | Absence is spelled two ways in one object, and `ttl != 0` is the wrong test. |
| 02 | [`02-atom-feed.cs`](02-atom-feed.cs) | `rel` has a default that is not in the document, so comparing `Relation` misses most alternate links. |
| 03 | [`03-opml-subscription-lists.cs`](03-opml-subscription-lists.cs) | The feed URLs are not in properties. They are in `Attributes`, and reading only the typed surface loses them. |
| 04 | [`04-sitemaps.cs`](04-sitemaps.cs) | `Priority` is `decimal?` because the protocol compares text, and a round trip through it is lossy. |
| 05 | [`05-format-agnostic.cs`](05-format-agnostic.cs) | Detection is one lookup on the root element's local name. It never checks the namespace. |
| 06 | [`06-saving-and-encoding.cs`](06-saving-and-encoding.cs) | Every document you write starts `EF BB BF`, and a narrow encoding escapes rather than loses. |
| 07 | [`07-dates-and-time-zones.cs`](07-dates-and-time-zones.cs) | The RFC 822 writer never looks at `DateTime.Kind`, and your feed is hours wrong. |
| 08 | [`08-round-trip-and-equality.cs`](08-round-trip-and-equality.cs) | `RetrievalLimit` makes save-after-load truncate your own feed. |
| 09 | [`09-extensions-read.cs`](09-extensions-read.cs) | Nothing registers anything: a declared namespace becomes an object on the entity that carried it. |
| 10 | [`10-extensions-write.cs`](10-extensions-write.cs) | You never write an `xmlns`. It is derived from the object graph, and switching that off drops the data silently. |
| 11 | [`11-podcast-feed.cs`](11-podcast-feed.cs) | Matching is prefix **or** namespace, so a feed is lost only when both are wrong — a truth table. |
| 12 | [`12-sitemap-extensions.cs`](12-sitemap-extensions.cs) | Element order inside `<video:video>` is schema-significant, and no round-trip test can see it. |
| 13 | [`13-custom-extension.cs`](13-custom-extension.cs) | Discovery has three qualifiers and each one excludes your type in silence. |
| 14 | [`14-fetching.cs`](14-fetching.cs) | `CreateAsync` and `LoadAsync` differ in one thing: whether you get to subscribe first. |
| 15 | [`15-load-settings.cs`](15-load-settings.cs) | `Timeout = null` means *no deadline at all*, and the size cap counts decompressed bytes. |
| 16 | [`16-discovery.cs`](16-discovery.cs) | Relative hrefs resolve against where the page came from **after redirects**. |
| 17 | [`17-conditional-get.cs`](17-conditional-get.cs) | Store the validators that came back; never re-send the ones you sent. |
| 18 | [`18-atom-publishing.cs`](18-atom-publishing.cs) | Statics cannot be overridden, which is why `AtomEntryResource` shadows `CreateAsync`. |
| 19 | [`19-pings-and-trackbacks.cs`](19-pings-and-trackbacks.cs) | A fault arrives with a 200 OK, so checking the status code records every failure as a success. |
| 20 | [`20-the-long-tail.cs`](20-the-long-tail.cs) | BlogML, APML and RSD have nothing in common and identical APIs. One function round-trips all three. |
| 21 | [`21-a-polling-service.cs`](21-a-polling-service.cs) | The whole tour, wired up: OPML in, one merged Atom feed out, nothing re-downloaded. |

## Things every sample assumes

Stated once here so no sample has to restate them:

- Collections are get-only `IList<T>`. You mutate them in place; there is no setter to assign to.
- Extensions attach with `entity.Extensions.Add(ext)` — there is no `AddExtension` — and are read
  back with `entity.FindExtension(SomeExtension.MatchByType)`.
- `FindExtension` is a linear scan with no index by type. Call it once and hold the result.
- Namespace declarations are derived from the object graph on save. You never write one by hand.
- A `null` in `SyndicationResourceLoadSettings` usually means "detect" or "none", not "default".
- Absence is `null` on reference-typed properties and, in the older types, a sentinel on
  value-typed ones. Which convention a type uses is worth checking rather than assuming.

## Running them all

```bash
pwsh -NoProfile -File ./run-samples.ps1
```

This is the fifth quality gate, and it is what `./build.ps1` runs after building the solution. It
restores and runs every sample in parallel with warnings as errors, reports `N samples, N
succeeded, 0 failed`, and fails the build if any sample breaks — or if this README falls behind the
directory.

## If you are looking for the reference

This directory is deliberately incomplete. It covers what is worth explaining, not what exists.

For the exhaustive version — every format, all 27 extensions, every overload — see
[`Solutions/Argotic.Examples`](../Argotic.Examples), which has 223 runnable examples and a CLI to
find them with:

```bash
dotnet run --project Solutions/Argotic.Examples -- list
dotnet run --project Solutions/Argotic.Examples -- run-all --skip-network
```