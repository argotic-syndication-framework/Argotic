#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 04 -- Sitemaps, and the index that exists because of a size cap
//
//     dotnet run --file Solutions/Samples/04-sitemaps.cs
//
// The first three samples read documents meant for people, delivered through software. A sitemap
// is the opposite: it exists only for crawlers, no human will ever read one, and its entire job
// is to say "these URLs exist, and here is roughly how much attention each deserves".
//
// That narrow purpose gives it something the syndication formats lack -- hard numeric limits. A
// conforming sitemap holds at most 50,000 URLs and at most 50 MB uncompressed. Those two numbers
// are the reason the sitemap index exists, the reason the load path gives sitemaps a different
// size budget from feeds, and the reason this sample is as much about arithmetic as about XML.
//
// It also introduces a third spelling of "absent", which is the small surprise waiting for anyone
// who arrives here from sample 01.
// ---------------------------------------------------------------------------------------------

using System.Globalization;

using Argotic.Common;
using Argotic.Syndication;

const string SitemapXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
      <url>
        <loc>https://endjin.com/</loc>
        <lastmod>2026-08-07</lastmod>
        <changefreq>daily</changefreq>
        <priority>1.0</priority>
      </url>
      <url>
        <loc>https://endjin.com/blog/</loc>
        <lastmod>2026-08-07T12:00:00+00:00</lastmod>
        <changefreq>daily</changefreq>
        <priority>0.9</priority>
      </url>
      <url>
        <loc>https://endjin.com/who-we-are/</loc>
        <changefreq>yearly</changefreq>
      </url>
      <url>
        <loc>https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra</loc>
        <lastmod>2026-05-14</lastmod>
      </url>
    </urlset>
    """;

Sitemap sitemap = new();
sitemap.Load(SyndicationEncodingUtility.CreateSafeNavigator(SitemapXml));

Heading("What came back");
Console.WriteLine($"  {sitemap.Format} {sitemap.Version}   {sitemap.Urls.Count} urls");

// ---------------------------------------------------------------------------------------------
// 1. A third spelling of absent
//
// Sample 01 established that the RSS model spells absence two ways: null for reference types, a
// sentinel for value types, because an int cannot be null and the writer needs to know whether to
// emit the element.
//
// The sitemap types do not do that. LastModified is DateTime?, ChangeFrequency is
// SitemapChangeFrequency?, Priority is decimal? -- nullable value types throughout, so absence
// has exactly one spelling and `is null` answers it everywhere.
//
// The useful lesson is not that one convention is better. It is that the convention is per type,
// and you have to look. A helper written against RssImage.Width that compares to int.MinValue
// will silently never fire on a SitemapUrl, and one written against SitemapUrl.LastModified that
// tests `is null` will never fire on an RssChannel.
// ---------------------------------------------------------------------------------------------

Heading("Three optional members, all nullable");
foreach (SitemapUrl url in sitemap.Urls)
{
    Console.WriteLine($"  {url.Location}");
    Console.WriteLine($"    lastmod     {url.LastModified?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(absent)"}");
    Console.WriteLine($"    changefreq  {url.ChangeFrequency?.ToString() ?? "(absent)"}");
    Console.WriteLine($"    priority    {url.Priority?.ToString(CultureInfo.InvariantCulture) ?? "(absent -- crawlers assume 0.5)"}");
}

// ---------------------------------------------------------------------------------------------
// 2. Priority is decimal, and it is deliberately imprecise
//
// Priority is a decimal? rather than a double?, and the choice is not decoration. The protocol
// compares the text of the element, so what matters is the digits you emit, and decimal is the
// type whose arithmetic produces the digits you meant. A double holding 0.7 prints as 0.7 but
// arrives there through a value that is not 0.7, and the moment you compute a priority rather
// than writing a literal, the difference surfaces.
//
// Two guards sit on the property, and they behave differently. Out of range throws -- the
// protocol says 0.0 to 1.0 and a value outside it is not a judgement call. Excess precision does
// not throw; the writer formats to one decimal place, rounding rather than truncating, so 0.12 is
// published as 0.1 and 0.75 as 0.8. That asymmetry is right, because 0.12 is a legal decimal that
// means something the format cannot express, and refusing it would be refusing arithmetic.
//
// It also means a round trip is lossy in a way nothing warns you about. Below is the proof.
// ---------------------------------------------------------------------------------------------

Heading("Priority, exactly");
Sitemap precise = new();
precise.Urls.Add(new SitemapUrl(new Uri("https://endjin.com/what-we-do/")) { Priority = 0.12m });
precise.Urls.Add(new SitemapUrl(new Uri("https://endjin.com/contact-us/")) { Priority = 0.75m });

Sitemap reloaded = new();
using (MemoryStream roundTrip = new())
{
    precise.Save(roundTrip, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });
    roundTrip.Position = 0;
    reloaded.Load(roundTrip);
}

for (int index = 0; index < precise.Urls.Count; index++)
{
    Console.WriteLine($"  set {precise.Urls[index].Priority,-6} saved and reloaded as {reloaded.Urls[index].Priority}");
}

try
{
    _ = new SitemapUrl(new Uri("https://endjin.com/")) { Priority = 1.5m };
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("  set 1.5    rejected -- out of range is an error, excess precision is not");
}

// ---------------------------------------------------------------------------------------------
// 3. The two limits, and who keeps them
//
// A conforming sitemap holds at most 50,000 <url> elements and at most 50 MB uncompressed. This
// library enforces neither, and the omission is deliberate rather than an oversight: an object
// graph in memory does not know where it will be published, and a Sitemap you are still building
// is legitimately over the limit until the moment you split it. Keeping the cap is the caller's
// job, which means it is your job, which means it should be a test in your build.
//
// The load path does impose a limit, but it is a different one and it is defensive rather than
// conformance-related: a cap on how many bytes an origin may send before the read is abandoned.
// And here is the part worth remembering -- which cap you get is chosen by the static type you
// called the method on.
// ---------------------------------------------------------------------------------------------

Heading("Byte budgets, chosen by type");
Budget("RssFeed, AtomFeed, OpmlDocument", SyndicationContentLengthLimits.Feed, "Feed");
Budget("Sitemap, SitemapIndex", SyndicationContentLengthLimits.Sitemap, "Sitemap");
Budget("BlogMLDocument", SyndicationContentLengthLimits.Archive, "Archive");
Budget("discovery fetches of HTML", SyndicationContentLengthLimits.Discovery, "Discovery");
Console.WriteLine();
Console.WriteLine("  Same method shape, an eight-fold difference in what the origin may send. A helper");
Console.WriteLine("  generic over ISyndicationResource inherits whichever budget its type argument has.");

// ---------------------------------------------------------------------------------------------
// 4. The index, which is what you reach for at 50,001
//
// A sitemap index is a sitemap of sitemaps: the same two limits apply to it, so one index
// addresses at most 50,000 child documents, and 50,000 x 50,000 is two and a half billion URLs.
// Nobody has needed the second level of nesting, and the protocol does not permit it anyway -- an
// index may not point at another index.
//
// The entry type is deliberately thinner than SitemapUrl. An index entry has a location and a
// last-modified date and nothing else: change frequency and priority describe a page, and a child
// sitemap is not a page. Reaching for a property that is not there is the quickest way to notice
// you have confused the two.
// ---------------------------------------------------------------------------------------------

SitemapIndex sitemapIndex = new();
sitemapIndex.Sitemaps.Add(new SitemapIndexEntry(
    new Uri("https://endjin.com/sitemap-pages.xml"),
    new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc)));
sitemapIndex.Sitemaps.Add(new SitemapIndexEntry(
    new Uri("https://endjin.com/sitemap-blog.xml"),
    new DateTime(2026, 8, 7, 6, 0, 0, DateTimeKind.Utc)));
sitemapIndex.Sitemaps.Add(new SitemapIndexEntry(new Uri("https://endjin.com/sitemap-video.xml")));

Heading("An index over three documents");
foreach (SitemapIndexEntry entry in sitemapIndex.Sitemaps)
{
    Console.WriteLine($"  {entry.Location}");
    Console.WriteLine($"    lastmod  {entry.LastModified?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(absent)"}");
}

const int PerDocument = 50_000;
Console.WriteLine();
Console.WriteLine($"  one sitemap addresses  {PerDocument:N0} urls");
Console.WriteLine($"  one index addresses    {(long)PerDocument * PerDocument:N0} urls, and may not nest further");

using MemoryStream stream = new();
sitemapIndex.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });
Console.WriteLine($"  this index             {stream.Length:N0} bytes");

Console.WriteLine();
Console.WriteLine("Next: 05-format-agnostic.cs -- you have bytes and no idea what they are.");

static void Budget(string callers, long bytes, string constant) =>
    Console.WriteLine($"  {callers,-33} {bytes / 1024 / 1024,3} MiB   SyndicationContentLengthLimits.{constant}");

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}