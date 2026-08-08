#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 07 -- Two date formats, and the one that will bite you
//
//     dotnet run --file Solutions/Samples/07-dates-and-time-zones.cs
//
// RSS dates are RFC 822. Atom dates are RFC 3339. Both are printed forms of an instant, both are
// implemented in this library by a pair of parse and format methods, and the two pairs behave
// differently in a way that has nothing to do with taste.
//
// RFC 3339 can express an offset: 2026-05-14T08:54:29Z and 2026-05-14T04:54:29-04:00 are the same
// instant written two ways, and a reader can recover the instant from either. RFC 822's most
// common shape, the one .NET calls RFC1123Pattern, ends in a literal "GMT" -- it has nowhere to
// put an offset, so the only instant it can express is one already in UTC.
//
// Everything below follows from that asymmetry. A format that cannot represent an offset cannot
// convert to one, so the RFC 822 writer does not look at DateTime.Kind at all; the RFC 3339
// writer does. That is one sentence, and it is the whole sample.
//
// A warning about reproducing this: on a machine whose local time zone is UTC -- most containers,
// most CI runners -- DateTime.Now and DateTime.UtcNow are the same instant with the same wall
// clock, and the defect below is completely invisible. So this file never uses the ambient zone.
// It names one explicitly, which is also the right habit in a test.
// ---------------------------------------------------------------------------------------------

using System.Globalization;

using Argotic.Common;
using Argotic.Syndication;

// One instant, stated unambiguously. Everything in this file is a way of writing this down.
DateTime instant = new(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc);

Heading("One instant, two formats");
Console.WriteLine($"  the instant             {instant:O}");
Console.WriteLine($"  RFC 822   (RSS pubDate) {SyndicationDateTimeUtility.ToRfc822DateTime(instant)}");
Console.WriteLine($"  RFC 3339  (Atom updated) {SyndicationDateTimeUtility.ToRfc3339DateTime(instant)}");

// ---------------------------------------------------------------------------------------------
// 1. What each writer does with Kind
//
// Take one wall clock -- 08:54:29 on 14 May 2026 -- and label it three ways. The instants these
// three DateTimes denote are genuinely different, because Utc and Local mean different points on
// the timeline, and Unspecified means the caller declined to say.
//
// The RFC 3339 writer distinguishes all three. Local gets an offset; Utc gets a Z; and
// Unspecified gets a Z as well, which is an assertion rather than a conversion -- the writer
// cannot know what zone an Unspecified value meant, so it publishes the wall clock and claims it
// is UTC. That is a guess, but it is a documented, single, consistent guess.
//
// The RFC 822 writer produces one string for all three. Not "usually the same" -- the same, by
// construction, because RFC1123Pattern has no offset field to differ in.
// ---------------------------------------------------------------------------------------------

Heading("The same wall clock, three Kinds");
Console.WriteLine($"  {"Kind",-12} {"RFC 3339",-28} RFC 822");
foreach (DateTimeKind kind in new[] { DateTimeKind.Utc, DateTimeKind.Local, DateTimeKind.Unspecified })
{
    DateTime value = new(2026, 5, 14, 8, 54, 29, kind);
    Console.WriteLine($"  {kind,-12} {SyndicationDateTimeUtility.ToRfc3339DateTime(value),-28} {SyndicationDateTimeUtility.ToRfc822DateTime(value)}");
}

Console.WriteLine();
Console.WriteLine("  RFC 3339 tells the three apart. RFC 822 cannot, and does not try.");
Console.WriteLine($"  (this machine's local zone is {TimeZoneInfo.Local.Id}, so the Local row shows a");
Console.WriteLine("   +00:00 offset. That is why the next section names a zone instead of inheriting one.)");

// ---------------------------------------------------------------------------------------------
// 2. So here is the bug, in full
//
// It is not that RFC 822 loses precision. It is that a wall clock which is not UTC gets published
// under a label saying it is, and every reader in the world then believes the label. The item is
// not slightly wrong; it is wrong by exactly the offset, which for most of the world's population
// is between one and eleven hours.
//
// The sequence that produces it is entirely ordinary. A CMS stores local publication times. Your
// code reads one out -- it arrives as Unspecified, because a database column has no zone -- and
// assigns it to RssItem.PublicationDate. Nothing converts it, nothing warns, and the feed says
// the post appeared several hours before it did. If your reader sorts by date, it is now in the
// wrong place; if a subscriber polls hourly, they may see it before it existed.
//
// The demonstration below is deterministic on any machine, because the zone is named rather than
// inherited. Note that a Local value on a UTC host would show a zero-hour error and prove nothing
// -- which is precisely why this defect survives so long in codebases with a green test suite.
// ---------------------------------------------------------------------------------------------

TimeZoneInfo newYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
DateTime newYorkWallClock = TimeZoneInfo.ConvertTimeFromUtc(instant, newYork);

string published = SyndicationDateTimeUtility.ToRfc822DateTime(newYorkWallClock);

// ParseRfc822DateTime rather than TryParse: these strings were produced two lines above, so a
// failure here would be a defect in the library rather than bad input. Reserve TryParse for text
// that came from somewhere you do not control -- which is what section 3 is about.
DateTime asRead = SyndicationDateTimeUtility.ParseRfc822DateTime(published);

Heading("Publishing a New York wall clock as if it were GMT");
Console.WriteLine($"  the instant meant       {instant:O}");
Console.WriteLine($"  as a New York clock     {newYorkWallClock:O}   (Kind is {newYorkWallClock.Kind})");
Console.WriteLine($"  published as            {published}");
Console.WriteLine($"  every reader reads      {asRead:O}");
Console.WriteLine($"  error                   {(asRead - instant).TotalHours:0.#} hours, silently");

string corrected = SyndicationDateTimeUtility.ToRfc822DateTime(
    TimeZoneInfo.ConvertTimeToUtc(newYorkWallClock, newYork));
DateTime asReadCorrected = SyndicationDateTimeUtility.ParseRfc822DateTime(corrected);

Console.WriteLine();
Console.WriteLine($"  convert first, then publish  {corrected}");
Console.WriteLine($"  which reads back as          {asReadCorrected:O}");
Console.WriteLine($"  error                        {(asReadCorrected - instant).TotalHours:0.#} hours");

// ---------------------------------------------------------------------------------------------
// 3. Reading is the forgiving direction
//
// Writing is strict because there is one right answer. Reading has to cope with thirty years of
// publishers, and RFC 822 section 5.1 made two things optional that everybody assumed were not:
// the day-of-week prefix and the seconds. It also permits named zones -- GMT, UT, EST, and the
// military single letters -- as well as numeric offsets.
//
// That is why TryParseRfc822DateTime carries a table of 36 patterns and rewrites named zones to
// offsets before it starts. All of the shapes below are legal RFC 822 and all of them appear in
// real feeds.
//
// One property matters more than the tolerance: whatever goes in, what comes out is UTC. Parsing
// normalises with DateTimeStyles.AdjustToUniversal, so a value you obtained by parsing is always
// safe to hand straight back to ToRfc822DateTime. The round trip is exact. It is only values you
// invented yourself that need care.
// ---------------------------------------------------------------------------------------------

Heading("Shapes a reader must accept");
foreach (string candidate in new[]
{
    "Thu, 14 May 2026 08:54:29 GMT",
    "Thu, 14 May 2026 08:54:29 +0000",
    "14 May 2026 08:54:29 GMT",
    "Thu, 14 May 2026 08:54 GMT",
    "Thu, 14 May 26 08:54:29 GMT",
    "Thu, 14 May 2026 04:54:29 -0400",
    "Thu, 14 May 2026 08:54:29 UT",
    "not a date at all",
})
{
    bool parsed = SyndicationDateTimeUtility.TryParseRfc822DateTime(candidate, out DateTime value);
    string outcome = parsed ? $"{value:yyyy-MM-dd HH:mm:ss} {value.Kind}" : "rejected";
    Console.WriteLine($"  {candidate,-32} {outcome}");
}

Heading("A parsed value round-trips exactly");
DateTime fromOffset = SyndicationDateTimeUtility.ParseRfc822DateTime("Thu, 14 May 2026 04:54:29 -0400");
Console.WriteLine($"  parsed from an offset   {fromOffset:O}  (Kind {fromOffset.Kind})");
Console.WriteLine($"  written back            {SyndicationDateTimeUtility.ToRfc822DateTime(fromOffset)}");
Console.WriteLine($"  same instant            {fromOffset == instant}");

// ---------------------------------------------------------------------------------------------
// 4. Where this reaches the object model
//
// You will rarely call these utilities directly; the adapters do it for you. What you control is
// the Kind of the DateTime you assign, and the rule reduces to one line:
//
//     assign UTC, or assign what you parsed out of a feed.
//
// Both feeds below carry the same instant, and both are correct, because both were built from a
// DateTime whose Kind was Utc. Had either been built from an Unspecified local clock, the RSS one
// would be wrong and the Atom one would be wrong differently -- which is worse, because a
// discrepancy between two of your own feeds is the sort of thing nobody notices for a year.
// ---------------------------------------------------------------------------------------------

RssFeed rss = new();
rss.Channel.Title = "endjin blog";
rss.Channel.Link = new Uri("https://endjin.com/blog/");
rss.Channel.Description = "Technical writing from endjin.";
rss.Channel.Items.Add(new RssItem
{
    Title = "The GenAI Reality Check",
    Link = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
    Description = "A new instrument in an orchestra that already existed.",
    PublicationDate = instant,
});

AtomFeed atom = new(
    new AtomId(new Uri("https://endjin.com/")),
    new AtomTextConstruct("endjin blog"),
    instant);
atom.Entries.Add(new AtomEntry(
    new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93c-0003939e0af6")),
    new AtomTextConstruct("The GenAI Reality Check"),
    instant));

Heading("The same instant, published both ways");
Console.WriteLine($"  RSS  <pubDate>  {Between(Save(rss), "<pubDate>", "</pubDate>")}");
Console.WriteLine($"  Atom <updated>  {Between(Save(atom), "<updated>", "</updated>")}");

Console.WriteLine();
Console.WriteLine("Next: 08-round-trip-and-equality.cs -- what actually survives being written and read back.");

static string Save(ISyndicationResource resource)
{
    using MemoryStream stream = new();
    resource.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });

    stream.Position = 0;
    using StreamReader reader = new(stream);

    return reader.ReadToEnd();
}

static string Between(string document, string open, string close)
{
    int start = document.IndexOf(open, StringComparison.Ordinal) + open.Length;
    int end = document.IndexOf(close, start, StringComparison.Ordinal);

    return document[start..end];
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', CultureInfo.InvariantCulture.TextInfo.ToUpper(title).Length));
}