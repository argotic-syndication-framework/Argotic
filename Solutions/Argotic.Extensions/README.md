# Argotic.Extensions

The syndication extension library of the **Argotic Syndication Framework** — **27 extension types across 22
families** that add namespaced foreign markup to RSS, Atom, OPML and Sitemap documents: iTunes podcast
metadata, Podcasting 2.0, Dublin Core, Yahoo Media, GeoRSS, Creative Commons, Google's sitemap extensions,
Trackback, Pingback and more.

Extensions are **auto-detected on load** from the XML namespaces declared on the document, and the namespace
declarations written back on save are derived from the extensions actually present. There is no registration
step and no configuration.

## Install

```bash
dotnet add package Argotic.Extensions
```

Target framework: **`net10.0`**. Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
or later.

> **You probably already have this package.**
> [`Argotic.Core`](https://www.nuget.org/packages/Argotic.Core) references `Argotic.Extensions`, so
> installing `Argotic.Core` brings it in transitively. Install `Argotic.Extensions` on its own only when you
> are building against the extension model without the feed object model — for example, writing your own
> `SyndicationExtension` in a library that must not depend on `Argotic.Core`.

## The families

Extensions live under the `Argotic.Extensions.Core` namespace. Three families hold more than one extension —
AtomPublishing (2), DublinCore (2) and Sitemap (4) — which is why 22 families yield 27 concrete
`SyndicationExtension` subclasses.

| Family                 | Description                                                                                                       |
|------------------------|-------------------------------------------------------------------------------------------------------------------|
| AtomPublishing         | Atom Publishing Protocol (`Control`, `Edited` — 2 extensions)                                                     |
| BasicGeocoding         | Geographic coordinates                                                                                            |
| BlogChannel            | Blog channel metadata                                                                                             |
| CreativeCommons        | CC licensing                                                                                                      |
| DublinCore             | `DublinCoreElementSet` and `DublinCoreMetadataTerms` — 2 extensions                                               |
| FeedHistory            | Feed paging/archiving                                                                                             |
| FeedRank               | Feed ranking                                                                                                      |
| FeedSync               | Feed synchronization                                                                                              |
| **GeoRSS**             | Geographic location (OGC 17-002r1) — Simple and GML                                                               |
| iTunes                 | iTunes podcast metadata                                                                                           |
| LiveJournal            | LiveJournal-specific                                                                                              |
| Pheed                  | Pheed media                                                                                                       |
| Pingback               | Pingback protocol                                                                                                 |
| **Podcast**            | [Podcasting 2.0](https://github.com/Podcastindex-org/podcast-namespace/blob/main/docs/1.0.md) — 62% of live feeds |
| SimpleList             | Microsoft Simple List Extensions                                                                                  |
| **Sitemap**            | Google sitemap extensions: `News`, `Image`, `Video`, `Hreflang` — 4 extensions                                    |
| SiteSummaryContent     | RSS content module                                                                                                |
| SiteSummarySlash       | Slashdot comment counts                                                                                           |
| SiteSummarySyndication | RSS syndication module                                                                                            |
| Trackback              | Trackback protocol                                                                                                |
| WellFormedWebComments  | Comment threading                                                                                                 |
| YahooMedia             | Yahoo Media RSS                                                                                                   |

## The two things you actually need

Every extensible entity — a feed, a channel, an item, an entry, an outline, a sitemap URL — implements
`IExtensibleSyndicationObject`, which is two members:

```csharp
IList<ISyndicationExtension> Extensions { get; }   // mutable; adding here is what makes it appear on save
bool HasExtensions { get; }
```

…plus one extension method, `FindExtension`, which is how you read one back.

### 1. Reading an extension off a loaded feed

Nothing registers the extension. Reflection over this assembly's exported types produces the candidates;
those whose namespace or prefix is bound on the document are asked whether they are present; and each that
says yes is attached to the entity that carried its elements. So the only call you make is
`FindExtension(MatchByType)`.

```csharp
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

RssFeed feed = new();
using (Stream stream = File.OpenRead("podcast.xml"))
{
    feed.Load(stream);
}

// On the channel
if (feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension channelItunes)
{
    Console.WriteLine(channelItunes.Context.Author);
    Console.WriteLine(channelItunes.Context.Summary);
    Console.WriteLine(channelItunes.Context.ExplicitMaterial);   // ITunesExplicitMaterial.Clean / .No / .Yes
}

// On each item
foreach (RssItem item in feed.Channel.Items)
{
    if (item.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension itemItunes)
    {
        Console.WriteLine($"{item.Title}  s{itemItunes.Context.Season}e{itemItunes.Context.Episode}  {itemItunes.Context.Duration}");
    }
}
```

Every shipped extension exposes a static `MatchByType` predicate for exactly this purpose. `FindExtension` is
a **linear scan** of `Extensions` — reading several properties off one extension should call it once and
hold the result, not call it per property.

`HasExtensions` is the cheap guard when most entities carry nothing:

```csharp
if (item.HasExtensions
    && item.FindExtension(GeoRssSyndicationExtension.MatchByType) is GeoRssSyndicationExtension geo)
{
    // …
}
```

The same code works against Atom, because the extension model sits below the format:

```csharp
using Argotic.Syndication;

AtomFeed atom = new();
atom.Load(stream);

foreach (AtomEntry entry in atom.Entries)
{
    if (entry.FindExtension(GeoRssSyndicationExtension.MatchByType) is GeoRssSyndicationExtension entryGeo)
    {
        // …
    }
}
```

### 2. Attaching an extension before saving

Add it to the entity's `Extensions` collection. That is the whole mechanism — the save path walks every
extensible entity, collects the types actually attached, and writes the corresponding `xmlns` declarations on
the document root for you.

```csharp
using Argotic.Extensions.Core;
using Argotic.Syndication;

RssFeed feed = new()
{
    Channel =
    {
        Title = "My Podcast",
        Link = new Uri("https://example.com/"),
        Description = "A podcast about things.",
    },
};

RssItem episode = new()
{
    Title = "Episode 1",
    Link = new Uri("https://example.com/1"),
    PublicationDate = DateTime.UtcNow,
};

// iTunes metadata on the episode
ITunesSyndicationExtension itunes = new();
itunes.Context.Author = "Jane Doe";
itunes.Context.Subtitle = "The first one";
itunes.Context.Summary = "In which we begin.";
itunes.Context.Duration = TimeSpan.FromMinutes(42);
itunes.Context.Episode = 1;
itunes.Context.Season = 1;
itunes.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
itunes.Context.Keywords.Add("dotnet");

episode.Extensions.Add(itunes);

// Slashdot comment counts on the same item — an entity can carry any number of extensions
SiteSummarySlashSyndicationExtension slash = new();
slash.Context.Section = "articles";
slash.Context.Comments = 42;

episode.Extensions.Add(slash);

feed.Channel.Items.Add(episode);

using Stream output = File.Create("podcast.xml");
feed.Save(output);
```

The saved document declares `xmlns:itunes` and `xmlns:slash` on `<rss>`, and carries the elements under
`<item>`. Load it back and `FindExtension` returns them — the round trip is symmetric.

### Sitemap extensions

The four Google sitemap extensions attach to a `SitemapUrl` rather than to a feed item, but the mechanism is
identical.

```csharp
using Argotic.Extensions.Core;
using Argotic.Syndication;

Sitemap sitemap = new();

SitemapUrl url = new()
{
    Location = new Uri("https://www.example.com/news/breaking-story"),
    LastModified = DateTime.UtcNow,
};

SitemapNewsExtension news = new()
{
    Publication = new SitemapNewsPublication("Example News", "en"),
    PublicationDate = DateTime.UtcNow,
    Title = "Breaking: Major Technology Announcement",
};

url.Extensions.Add(news);
sitemap.Urls.Add(url);

using Stream stream = File.Create("sitemap-news.xml");
sitemap.Save(stream);
```

Reading them back:

```csharp
foreach (SitemapUrl each in sitemap.Urls)
{
    if (each.FindExtension(SitemapNewsExtension.MatchByType) is SitemapNewsExtension found)
    {
        Console.WriteLine($"{each.Location}: {found.Title}");
    }
}
```

## How auto-discovery actually works

`SyndicationExtensionAdapter.FrameworkExtensions` is the candidate list, and it is **discovered, not
maintained**:

```csharp
Assembly.GetExecutingAssembly()
        .GetExportedTypes()
        .Where(t => typeof(SyndicationExtension).IsAssignableFrom(t) && !t.IsAbstract)
```

Three consequences worth knowing:

- A new `SyndicationExtension` subclass added **to this assembly** appears with no registration step — and,
  symmetrically, one made `internal` silently disappears, because `GetExportedTypes` sees only public types.
- The test is assignability to `SyndicationExtension`, not to `ISyndicationExtension`. A type that implements
  the interface directly is not found.
- Every candidate is instantiated through `Activator.CreateInstance`, so an extension type **must** have a
  public parameterless constructor or it throws `MissingMethodException` at load time.

Detection can be turned off, or narrowed, through `SyndicationResourceLoadSettings`:

```csharp
using Argotic.Common;

SyndicationResourceLoadSettings settings = new()
{
    AutoDetectExtensions = false,   // consider only what is in SupportedExtensions
};

settings.SupportedExtensions.Add(typeof(ITunesSyndicationExtension));

feed.Load(stream, settings);
```

## Writing your own extension

Derive from `SyndicationExtension`, pass your prefix, namespace and version to the base constructor, and
override the three abstract members: `Load(IXPathNavigable)`, `Load(XmlReader)` and `WriteTo(XmlWriter)`.

```csharp
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;

public sealed class MyCustomSyndicationExtension : SyndicationExtension
{
    public MyCustomSyndicationExtension()
        : base(
            xmlPrefix: "myPrefix",
            xmlNamespace: "http://www.example.com/2008/03/custom",
            version: new Version("1.0"),
            documentation: new Uri("http://www.example.com/spec"),
            name: "My Extension",
            description: "Example of a custom syndication extension.")
    {
    }

    public string MyAttribute { get; set; } = string.Empty;

    // The predicate consumers pass to FindExtension.
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is MyCustomSyndicationExtension;
    }

    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        bool wasLoaded = false;
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));

        if (navigator.HasAttributes)
        {
            string value = navigator.GetAttribute("someAttribute", string.Empty);
            if (!string.IsNullOrEmpty(value))
            {
                this.MyAttribute = value;
                wasLoaded = true;
            }
        }

        this.OnExtensionLoaded(new SyndicationExtensionLoadedEventArgs(source, this));

        return wasLoaded;
    }

    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return this.Load(new XPathDocument(reader).CreateNavigator());
    }

    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("CustomExtension", this.XmlNamespace);

        if (!string.IsNullOrEmpty(this.MyAttribute))
        {
            writer.WriteAttributeString("someAttribute", this.MyAttribute);
        }

        writer.WriteEndElement();
    }
}
```

### Registering it

This is the one place the "no registration step" rule has an edge, and it is worth being precise about:

- **Saving needs nothing.** Attach your extension to an entity's `Extensions` collection and the save path
  picks it up, writing its `xmlns` declaration on the document root along with everyone else's.
- **Loading needs one line.** Auto-discovery reflects over `Argotic.Extensions` only, so an extension living
  in *your* assembly is not a candidate. Register the type on the load settings:

```csharp
using Argotic.Common;

SyndicationResourceLoadSettings settings = new();
settings.SupportedExtensions.Add(typeof(MyCustomSyndicationExtension));

RssFeed feed = new();
feed.Load(stream, settings);

if (feed.Channel.FindExtension(MyCustomSyndicationExtension.MatchByType) is MyCustomSyndicationExtension mine)
{
    Console.WriteLine(mine.MyAttribute);
}
```

Registered types are added on top of the framework's auto-detected set, not instead of it, so the shipped
extensions keep working alongside yours. (An extension contributed to `Argotic.Extensions` itself genuinely
needs no registration — that is the case reflection covers.)

### Comparison and equality

The shipped extensions implement `IComparable<T>`, `IEquatable<T>` and `Argotic.Common.IComparisonOperators`,
which opts them into the `<`, `>`, `<=` and `>=` extension operators in
`Argotic.Common.ComparisonOperatorExtensions` instead of hand-writing four operators. `==` and `!=` must
still be declared explicitly: predefined reference equality wins over an extension operator. Following the
same idiom in your own extension is optional but keeps it consistent with the rest of the library.

## Dependency chain

```
Argotic.Common                 (no package dependencies)
    ^
Argotic.Extensions             (references Common)  <- you are here
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