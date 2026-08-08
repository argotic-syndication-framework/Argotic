#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 12 -- Google's sitemap namespaces, and why element order is not a matter of taste
//
//     dotnet run --file Solutions/Samples/12-sitemap-extensions.cs
//
// Sample 04 covered the sitemap protocol itself, which is deliberately tiny: a location, a date,
// a change frequency, a priority. Everything a search engine actually wants to know about an
// image, a video or a news article lives in one of four namespaces Google defines on top of it.
//
// Mechanically these are ordinary extensions and samples 09 and 10 already taught them -- they
// attach to SitemapUrl.Extensions, they are found with FindExtension, the declarations are
// derived on save. There are two things here that are genuinely new, and both are about the fact
// that these documents are consumed by a validating parser rather than by a tolerant one.
//
// The first is a small API difference. The second is the most instructive defect in this
// repository's history, and it is the reason this sample exists as its own file.
// ---------------------------------------------------------------------------------------------

using System.Xml.Linq;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

// ---------------------------------------------------------------------------------------------
// 1. These four do not have a Context
//
// Every extension so far has held its data in a Context property: itunes.Context.Season,
// dc.Context.Creator. The four sitemap extensions do not. Their data hangs directly off the
// extension -- SitemapImageExtension.Images, SitemapVideoExtension.Videos,
// SitemapHreflangExtension.Links -- and SitemapNewsExtension has plain properties.
//
// That is worth knowing before you go looking for a Context that is not there. The reason is that
// the other extensions model a fixed set of elements on one entity, so a context object is a
// natural container; these model repeated child structures, and a list is the natural container
// for those. One <loc> may carry up to a thousand images.
// ---------------------------------------------------------------------------------------------

SitemapUrl talkPage = new(new Uri("https://endjin.com/what-we-think/talks/"))
{
    LastModified = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
    ChangeFrequency = SitemapChangeFrequency.Weekly,
    Priority = 0.8m,
};

SitemapImageExtension images = new();
images.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet.png")));
images.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/genai-reality-check.png")));
talkPage.Extensions.Add(images);

// A video populated on every element the library writes, which is what makes section 2 a real
// test rather than a demonstration on three fields.
SitemapVideo talk = new()
{
    ThumbnailLocation = new Uri("https://i.ytimg.com/vi/endjin-rx-dotnet/hqdefault.jpg"),
    Title = "Rx.NET v7.0: what changed and why",
    Description = "A walk through the Rx.NET v7.0 release, from the team that maintains it.",
    PlayerLocation = new Uri("https://www.youtube.com/embed/endjin-rx-dotnet"),
    Duration = 2_814,
    Rating = 4.8m,
    ViewCount = 12_403,
    PublicationDate = new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc),
    FamilyFriendly = true,
    RequiresSubscription = false,
    Live = false,
    Uploader = "endjin",
    UploaderInfo = new Uri("https://www.youtube.com/@endjin"),
    Platform = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile,
    PlatformRelationship = SitemapVideoRelationship.Allow,
    Restriction = "GB IE US",
    RestrictionRelationship = SitemapVideoRelationship.Allow,
};

talk.Tags.Add("rx.net");
talk.Tags.Add("dotnet");
talk.Tags.Add("reactive");

SitemapVideoExtension videos = new();
videos.Videos.Add(talk);
talkPage.Extensions.Add(videos);

SitemapUrl article = new(new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"));
SitemapNewsExtension news = new()
{
    Publication = new SitemapNewsPublication("endjin blog", "en"),
    PublicationDate = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
    Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
};
article.Extensions.Add(news);

// Hreflang says "this page exists in these other languages". Note that it is reciprocal: every
// page in the cluster must name every other, itself included, or Google discards the cluster
// entirely rather than treating it as partial. That is an easy thing to get half right.
SitemapHreflangExtension alternates = new();
alternates.Links.Add(new SitemapHreflangLink("en-gb", new Uri("https://endjin.com/what-we-think/talks/")));
alternates.Links.Add(new SitemapHreflangLink("en-us", new Uri("https://endjin.com/us/what-we-think/talks/")));
alternates.Links.Add(new SitemapHreflangLink("x-default", new Uri("https://endjin.com/what-we-think/talks/")));
talkPage.Extensions.Add(alternates);

Sitemap sitemap = new();
sitemap.Urls.Add(talkPage);
sitemap.Urls.Add(article);

string document = Save(sitemap);

Heading("One sitemap, four namespaces");
Console.WriteLine($"  {document.Length:N0} bytes, {sitemap.Urls.Count} urls");
foreach (SitemapUrl url in sitemap.Urls)
{
    Console.WriteLine($"  {url.Location}");
    Console.WriteLine($"    {string.Join(", ", url.Extensions.Select(extension => $"{extension.XmlPrefix} ({extension.Name})"))}");
}

// ---------------------------------------------------------------------------------------------
// 2. The defect that a round trip cannot see
//
// sitemap-video-1.1.xsd declares the children of <video:video> as an xsd:sequence, not an
// xsd:all. Order is therefore part of the contract: tag belongs directly after publication_date,
// restriction after family_friendly, and platform and live near the end.
//
// This library used to write tag last and group uploader, platform and restriction together after
// live. Every video sitemap it produced carrying any one of those four was rejected by Google's
// own schema, and it stayed that way for years.
//
// The reason it survived is the point of this section, and it generalises well beyond sitemaps.
// The suite had round-trip tests and they all passed -- because the *reader* accepts children in
// any order, so writing them wrongly and reading them back produced exactly the object graph you
// started with. A round trip asserts that the reader agrees with the writer. When both are wrong
// in the same direction, that agreement is perfect and completely uninformative.
//
// Only an instrument that is not the library can disagree with the library. Here that instrument
// is the schema, and the check below reproduces it: the declared sequence is written down as a
// literal, the document's actual element order is read back out, and the two are compared. It is
// a check that can fail, which is the only kind worth having.
// ---------------------------------------------------------------------------------------------

// The child order declared by sitemap-video-1.1.xsd, restricted to the elements this library
// writes. Google's schema is not embedded here and must not be: it carries an explicit copyright
// reservation with no licence grant, which is why Argotic.Extensions.Tests/Schemas holds the
// sitemaps.org and APML schemas but deliberately not Google's three. The conformance tier in that
// project fetches them live instead. This literal is the same one its ordering test uses.
string[] schemaOrder =
[
    "thumbnail_loc", "title", "description", "content_loc", "player_loc", "duration",
    "expiration_date", "rating", "view_count", "publication_date", "tag", "family_friendly",
    "restriction", "requires_subscription", "uploader", "platform", "live", "id",
];

XNamespace video = "http://www.google.com/schemas/sitemap-video/1.1";
string[] written = XDocument.Parse(document)
    .Descendants(video + "video")
    .First()
    .Elements()
    .Select(element => element.Name.LocalName)
    .ToArray();

Heading("Element order inside <video:video>");
Console.WriteLine($"  written   {string.Join(" ", written)}");
Console.WriteLine();

int[] positions = written.Select(name => Array.IndexOf(schemaOrder, name)).ToArray();
string unknown = string.Join(", ", written.Where(name => !schemaOrder.Contains(name)));
bool ordered = positions.Zip(positions.Skip(1), (first, second) => first <= second).All(inOrder => inOrder);

Console.WriteLine($"  {written.Length} elements written, {unknown.Length} unrecognised{(unknown.Length == 0 ? string.Empty : $": {unknown}")}");
Console.WriteLine($"  schema positions  {string.Join(" ", positions)}");
Console.WriteLine($"  non-decreasing?   {ordered}  <- the whole assertion");
Console.WriteLine();
Console.WriteLine("  Move any one of them across another and that line reads False. Nothing else in");
Console.WriteLine("  a round-trip suite would notice, because the reader does not care about order.");

// ---------------------------------------------------------------------------------------------
// 3. Reading them back
//
// Nothing surprising, and that is the point: after samples 09 to 11 the retrieval idiom is the
// same for the fifth family as for the first. What is worth printing is the platform value,
// because SitemapVideoPlatform is a [Flags] enum -- Web | Mobile is one value, not two elements,
// and it round-trips as the pair of words Google's schema expects.
// ---------------------------------------------------------------------------------------------

Sitemap reloaded = new();
reloaded.Load(SyndicationEncodingUtility.CreateSafeNavigator(document));

Heading("Read back");
foreach (SitemapUrl url in reloaded.Urls)
{
    Console.WriteLine($"  {url.Location}");

    if (url.FindExtension(SitemapVideoExtension.MatchByType) is SitemapVideoExtension videoBack)
    {
        SitemapVideo first = videoBack.Videos[0];
        Console.WriteLine($"    video      {first.Title}");
        Console.WriteLine($"    duration   {TimeSpan.FromSeconds(first.Duration ?? 0):g}");
        Console.WriteLine($"    platform   {first.Platform}  (a [Flags] value, one element)");
        Console.WriteLine($"    tags       {string.Join(", ", first.Tags)}  (up to {SitemapVideo.MaxTagCount})");
    }

    if (url.FindExtension(SitemapImageExtension.MatchByType) is SitemapImageExtension imagesBack)
    {
        Console.WriteLine($"    images     {imagesBack.Images.Count}");
    }

    if (url.FindExtension(SitemapHreflangExtension.MatchByType) is SitemapHreflangExtension linksBack)
    {
        Console.WriteLine($"    hreflang   {string.Join(", ", linksBack.Links.Select(link => link.Hreflang))}");
    }

    if (url.FindExtension(SitemapNewsExtension.MatchByType) is SitemapNewsExtension newsBack)
    {
        Console.WriteLine($"    news       {newsBack.Publication?.Name} ({newsBack.Publication?.Language}), {newsBack.PublicationDate:yyyy-MM-dd}");
    }
}

Console.WriteLine();
Console.WriteLine("Next: 13-custom-extension.cs -- your own namespace, and the three ways discovery can miss it.");

static string Save(Sitemap sitemap)
{
    using MemoryStream stream = new();
    sitemap.Save(stream, new SyndicationResourceSaveSettings());
    stream.Position = 0;

    using StreamReader reader = new(stream);

    return reader.ReadToEnd();
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}