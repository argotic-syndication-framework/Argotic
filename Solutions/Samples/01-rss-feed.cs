#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 01 -- RSS 2.0
//
//     dotnet run --file Solutions/Samples/01-rss-feed.cs
//
// RSS 2.0 is a list of items wrapped in a pile of optional channel metadata, and the pile is the
// difficult part. The specification marks three channel elements required -- title, link,
// description -- and leaves roughly twenty optional. A reader therefore has to cope with any
// subset of twenty, and a writer has to decide which subset is worth emitting. Both halves of
// that problem are below: first a document carrying nearly everything, read into the object
// model, then a feed built from nothing and written back out.
//
// The document is a raw string literal rather than a file on disk. That is not laziness -- it is
// the shortest path from a bug report to a reproduction. Paste the feed that broke, run the file.
//
// Nothing here opens a socket. Samples 14 onwards do.
// ---------------------------------------------------------------------------------------------

using System.Globalization;
using System.Text;

using Argotic.Common;
using Argotic.Syndication;

// A trimmed version of endjin's own feed, carrying the optional channel elements the real one
// omits. Two items rather than three, and both are real posts.
const string FeedXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin on .NET, data, analytics and AI.</description>
        <language>en-gb</language>
        <copyright>Copyright 2026 endjin limited</copyright>
        <managingEditor>hello@endjin.com (endjin)</managingEditor>
        <webMaster>hello@endjin.com (endjin)</webMaster>
        <pubDate>Fri, 07 Aug 2026 08:00:00 GMT</pubDate>
        <lastBuildDate>Fri, 07 Aug 2026 12:00:00 GMT</lastBuildDate>
        <category domain="https://endjin.com/what-we-do/">Cloud Native App Dev</category>
        <category>Data and Analytics</category>
        <generator>Argotic Syndication Framework</generator>
        <ttl>60</ttl>
        <image>
          <url>https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png</url>
          <title>endjin blog</title>
          <link>https://endjin.com/blog/</link>
          <width>144</width>
          <height>144</height>
        </image>
        <cloud domain="rpc.endjin.com" port="80" path="/RPC2" registerProcedure="pingMe" protocol="soap" />
        <textInput>
          <title>Search</title>
          <description>Search the endjin blog</description>
          <name>query</name>
          <link>https://endjin.com/search</link>
        </textInput>
        <skipHours><hour>0</hour><hour>1</hour><hour>2</hour><hour>3</hour></skipHours>
        <skipDays><day>Saturday</day><day>Sunday</day></skipDays>
        <item>
          <title>The GenAI Reality Check: New Instrument, Same Orchestra</title>
          <link>https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra</link>
          <description>Generative AI is a new instrument in an orchestra that already existed.</description>
          <author>hello@endjin.com (Barry Smart)</author>
          <category>AI</category>
          <category domain="https://endjin.com/what-we-do/data-and-analytics/">Data</category>
          <enclosure url="https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check.mp3" length="36588921" type="audio/mpeg" />
          <guid isPermaLink="true">https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra</guid>
          <pubDate>Thu, 14 May 2026 08:54:29 GMT</pubDate>
          <source url="https://endjin.com/rss.xml">endjin blog</source>
        </item>
        <item>
          <title>Writing Effective Copilot Instructions for Complex Codebases</title>
          <link>https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases</link>
          <description>Structuring AI assistant guidance as modular skill files rather than one monolith.</description>
          <guid isPermaLink="false">urn:uuid:8f2d4c1e-9b37-4a5e-8c12-1d3e5a7b9c02</guid>
          <pubDate>Fri, 07 Aug 2026 00:00:00 GMT</pubDate>
        </item>
      </channel>
    </rss>
    """;

// ---------------------------------------------------------------------------------------------
// 1. Reading
//
// Load takes an IXPathNavigable, a Stream or an XmlReader -- never a file path. That is a
// deliberate narrowing: a path would imply the library owns the opening, the encoding fallback
// and the disposal, and it does not. CreateSafeNavigator turns a string into an IXPathNavigable
// with DTD processing off and no external entity resolution, so the entity-expansion attack
// surface is closed by the default rather than by a flag you have to remember. If you are
// building the XmlReader yourself, CreateSafeXmlReaderSettings() is the same settings object.
//
// One wrinkle worth noticing now: the declaration above says encoding="utf-8" and nothing decodes
// it, because a C# string is already decoded. Encoding declarations only matter when there are
// bytes, and section 5 is where the bytes appear.
// ---------------------------------------------------------------------------------------------

RssFeed feed = new();
feed.Load(SyndicationEncodingUtility.CreateSafeNavigator(FeedXml));

Heading("What came back");
Console.WriteLine($"  {feed.Format} {feed.Version}   {feed.Channel.Title}");
Console.WriteLine($"  {feed.Channel.Description}");
Console.WriteLine($"  {feed.Channel.Items.Count} items, {feed.Channel.Categories.Count} channel categories");

// ---------------------------------------------------------------------------------------------
// 2. Absence is spelled two different ways
//
// This is the single most useful thing to learn about the RSS object model, and no API listing
// shows it. Reference-typed properties are nullable -- Link is Uri?, Guid is RssGuid?, Image is
// RssImage? -- and null means the element was not there. Value-typed properties cannot be null
// and use a sentinel instead: TimeToLive defaults to int.MinValue, PublicationDate and
// LastBuildDate to DateTime.MinValue, RssImage.Width and Height to int.MinValue. The writer
// suppresses the element when it sees the sentinel, which is how the model round-trips "absent"
// at all.
//
// The practical consequence: `if (channel.TimeToLive > 0)` is correct by accident, and
// `if (channel.TimeToLive != 0)` is wrong -- an unstated ttl is int.MinValue, which is not zero.
// Compare against the sentinel, or against a floor you actually mean.
// ---------------------------------------------------------------------------------------------

Heading("Optional channel metadata");
Console.WriteLine($"  language        {feed.Channel.Language?.Name ?? "(absent)"}");
Console.WriteLine($"  ttl             {Absent(feed.Channel.TimeToLive == int.MinValue, feed.Channel.TimeToLive.ToString(CultureInfo.InvariantCulture))}");
Console.WriteLine($"  lastBuildDate   {Absent(feed.Channel.LastBuildDate == DateTime.MinValue, feed.Channel.LastBuildDate.ToString("R", CultureInfo.InvariantCulture))}");
Console.WriteLine($"  rating          {Absent(feed.Channel.Rating.Length == 0, feed.Channel.Rating)}");
Console.WriteLine($"  managingEditor  {Absent(feed.Channel.ManagingEditor.Length == 0, feed.Channel.ManagingEditor)}");

if (feed.Channel.Image is { } image)
{
    Console.WriteLine($"  image           {image.Width}x{image.Height}  {image.Title}");
}

if (feed.Channel.Cloud is { } cloud)
{
    // rssCloud is the 2001 answer to polling: register with the publisher's server and be pushed
    // to. Almost nothing implements it now -- sample 17 is the answer that won -- but the element
    // is still in the wild and still has to parse.
    Console.WriteLine($"  cloud           {cloud.Protocol} {cloud.Domain}:{cloud.Port}{cloud.Path} -> {cloud.RegisterProcedure}");
}

if (feed.Channel.TextInput is { } textInput)
{
    Console.WriteLine($"  textInput       ?{textInput.Name}= -> {textInput.Link}");
}

// SkipHours and SkipDays are get-only IList<T>, as is every collection in this library. You
// mutate them in place; there is no setter to assign a new list to. That is uniform across the
// whole object model, so once you have internalised it here you never have to check again.
Console.WriteLine($"  skipHours       {string.Join(", ", feed.Channel.SkipHours)}");
Console.WriteLine($"  skipDays        {string.Join(", ", feed.Channel.SkipDays)}");

Heading("Items");
foreach (RssItem item in feed.Channel.Items)
{
    Console.WriteLine($"  {item.PublicationDate:yyyy-MM-dd}  {item.Title}");

    // isPermaLink defaults to true in RSS, so a guid that is not a dereferenceable URL has to say
    // so explicitly. The second item's guid is a urn:uuid with isPermaLink="false"; following it
    // as a link would 404, and a reader that ignores the flag will do exactly that.
    if (item.Guid is { } guid)
    {
        Console.WriteLine($"    guid       {guid.Value}");
        Console.WriteLine($"    permalink  {guid.IsPermanentLink}");
    }

    foreach (RssCategory category in item.Categories)
    {
        Console.WriteLine(category.Domain.Length == 0
            ? $"    category   {category.Value}"
            : $"    category   {category.Value}   in {category.Domain}");
    }

    foreach (RssEnclosure enclosure in item.Enclosures)
    {
        Console.WriteLine($"    enclosure  {enclosure.ContentType}, {enclosure.Length:N0} bytes");
    }
}

// ---------------------------------------------------------------------------------------------
// 3. What the model refuses to hold
//
// RssChannel.Title and RssChannel.Description throw on null or the empty string. RssItem.Title
// and RssItem.Description do not -- they trim, and an empty string is a legal value. That is not
// an inconsistency; it is the specification. The channel pair is unconditionally required, so the
// type refuses to hold a state that cannot be saved as conforming RSS. The item pair is
// conditional -- an item needs one of the two, not both -- so the type cannot decide on its own
// and lets you through.
//
// The general rule, and it holds across the formats: a setter throws when the specification makes
// the element unconditionally required, and only then. Everywhere else the guard would have to
// guess, and a guess that throws is worse than no guard.
// ---------------------------------------------------------------------------------------------

Heading("Invariants");
try
{
    feed.Channel.Description = string.Empty;
}
catch (ArgumentException)
{
    Console.WriteLine("  RssChannel.Description rejected \"\": the element is required, so the setter is too.");
}

feed.Channel.Items[1].Description = string.Empty;
Console.WriteLine("  RssItem.Description accepted \"\": the element is conditional, and the type cannot judge.");

// ---------------------------------------------------------------------------------------------
// 4. Building one
//
// The object initialiser nests into Channel because a new RssFeed already has one -- Channel is
// never null, so there is nothing to construct first. After that it is ordinary property
// assignment and list mutation.
//
// Note what is not being set. Generator has a default that names this framework and its assembly
// version, so every document written here carries a <generator> nobody asked for; that is
// deliberate, and it is how a feed reader's "produced by" column gets filled in.
// ---------------------------------------------------------------------------------------------

RssFeed built = new()
{
    Channel =
    {
        Title = "endjin blog, read aloud",
        Link = new Uri("https://endjin.com/blog/"),
        Description = "Audio versions of selected posts from the endjin blog.",
        Language = new CultureInfo("en-GB"),
        TimeToLive = 60,
        SelfLink = new Uri("https://endjin.com/audio.xml"),
    },
};

built.Channel.Categories.Add(new RssCategory("Data and Analytics"));
built.Channel.Image = new RssImage(
    new Uri("https://endjin.com/blog/"),
    "endjin blog",
    new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png"))
{
    Width = 144,
    Height = 144,
};

RssItem post = new()
{
    Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
    Link = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
    Description = "Generative AI is a new instrument in an orchestra that already existed.",
    Author = "hello@endjin.com (Barry Smart)",

    // Kind matters here, and sample 07 is why. In short: the RFC 822 writer prints the wall clock
    // under a literal GMT and converts nothing, so a Local DateTime is republished as the wrong
    // instant. Construct dates as Utc, or call ToUniversalTime before assigning.
    PublicationDate = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
    Guid = new RssGuid("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra", isPermanentUrl: true),
};

post.Enclosures.Add(new RssEnclosure(
    36_588_921L,
    "audio/mpeg",
    new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check.mp3")));

built.Channel.Items.Add(post);

// ---------------------------------------------------------------------------------------------
// 5. Writing it
//
// Save writes to a Stream or an XmlWriter. There is no Save(path) and no SaveAsync, and both
// absences are deliberate: you own the stream, so you own the flush, the disposal, and the
// half-written-file failure mode. For a feed carrying no extensions, MinimizeOutputSize is the
// only knob that changes the bytes, and all it does is negate XmlWriterSettings.Indent -- the two
// documents carry identical information, so the only difference between them is length.
// ---------------------------------------------------------------------------------------------

byte[] indented = Save(built, minimise: false);
byte[] minimised = Save(built, minimise: true);

Heading("Output");
Console.WriteLine($"  indented   {indented.Length:N0} bytes");
Console.WriteLine($"  minimised  {minimised.Length:N0} bytes  ({100 - (minimised.Length * 100 / indented.Length)}% smaller)");

// CharacterEncoding defaults to Encoding.UTF8, and Encoding.UTF8 has a preamble -- so the first
// three bytes of every document this library writes are EF BB BF. Encoding.UTF8.GetString over
// those bytes hands back a string beginning U+FEFF, which then travels silently into whatever you
// concatenate it into and surfaces as a stray character somewhere far away. A StreamReader
// consumes the mark, which is the entire reason the helper at the foot of this file reads through
// one instead of calling GetString.
Console.WriteLine($"  first bytes {indented[0]:X2} {indented[1]:X2} {indented[2]:X2}  (UTF-8 byte-order mark)");
Console.WriteLine($"  GetString  first char is U+{(int)Encoding.UTF8.GetString(indented)[0]:X4}");

// ---------------------------------------------------------------------------------------------
// 6. The root element is not fixed
//
// One ordinary-looking property changes the shape of the document rather than its contents.
// RssChannel.SelfLink is an Atom element borrowed into RSS -- <atom:link rel="self"> -- so setting
// it obliges the writer to declare the Atom namespace on <rss> itself. Nothing else in the RSS
// model reaches that far up.
//
// This is a small instance of a large idea. Namespace declarations are derived from the object
// graph and never written by hand; the same machinery attaches xmlns:itunes, xmlns:dc and the
// rest when the corresponding extensions are present. Samples 09 and 10 are about that machinery,
// and this is the one case where you meet it without an extension in sight.
// ---------------------------------------------------------------------------------------------

Heading("Root element");
Console.WriteLine($"  with SelfLink     {RootElement(indented)}");

built.Channel.SelfLink = null;
Console.WriteLine($"  without SelfLink  {RootElement(Save(built, minimise: true))}");

Console.WriteLine();
Console.WriteLine("Next: 02-atom-feed.cs -- the same job, a stricter model, and why AtomLink.EffectiveRelation exists.");

static byte[] Save(RssFeed feed, bool minimise)
{
    using MemoryStream stream = new();
    feed.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = minimise });
    return stream.ToArray();
}

static string RootElement(byte[] document)
{
    using MemoryStream stream = new(document);

    // detectEncodingFromByteOrderMarks is on by default, and that is what consumes the BOM.
    using StreamReader reader = new(stream);

    string text = reader.ReadToEnd();
    int start = text.IndexOf("<rss", StringComparison.Ordinal);
    int end = text.IndexOf('>', start);

    return text[start..(end + 1)];
}

static string Absent(bool isAbsent, string value) => isAbsent ? "(absent)" : value;

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}