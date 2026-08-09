#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 08 -- What actually survives being written and read back
//
//     dotnet run --file Solutions/Samples/08-round-trip-and-equality.cs
//
// A round trip is the cheapest test you can write against a serialiser: build a thing, save it,
// load it, and check you got the same thing back. It is also the easiest test to fool, and this
// sample is as much about that as about the API.
//
// The trap has a name worth remembering. A round trip asserts that the reader understands the
// writer, and that stays true when both are wrong together. If the writer emits an element in an
// order a schema forbids, and the reader accepts any order, then load-save-load agrees with
// itself perfectly and the document is still rejected by everyone else. Symmetry hides asymmetric
// defects, which is why an external validator is worth more than any number of round trips.
//
// So: first how to write the round trip properly, then two things that genuinely do not survive
// one, and finally the reason the comparison is a single == rather than fifty assertions.
// ---------------------------------------------------------------------------------------------

using System.Globalization;

using Argotic.Common;
using Argotic.Syndication;

const string FeedXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin on .NET, data, analytics and AI.</description>
        <language>en-gb</language>
        <ttl>60</ttl>
        <item><title>Item one</title><link>https://endjin.com/blog/1</link><description>The first.</description>
              <pubDate>Mon, 03 Aug 2026 09:00:00 GMT</pubDate></item>
        <item><title>Item two</title><link>https://endjin.com/blog/2</link><description>The second.</description>
              <pubDate>Tue, 04 Aug 2026 09:00:00 GMT</pubDate></item>
        <item><title>Item three</title><link>https://endjin.com/blog/3</link><description>The third.</description>
              <pubDate>Wed, 05 Aug 2026 09:00:00 GMT</pubDate></item>
        <item><title>Item four</title><link>https://endjin.com/blog/4</link><description>The fourth.</description>
              <pubDate>Thu, 06 Aug 2026 09:00:00 GMT</pubDate></item>
        <item><title>Item five</title><link>https://endjin.com/blog/5</link><description>The fifth.</description>
              <pubDate>Fri, 07 Aug 2026 09:00:00 GMT</pubDate></item>
      </channel>
    </rss>
    """;

// ---------------------------------------------------------------------------------------------
// 1. Comparing the text is the wrong test
//
// The obvious round trip compares the document you loaded with the document you saved, and it
// fails immediately -- on a feed nothing is wrong with. RssChannel.Generator is initialised at
// construction to a string naming this framework and its version, so the property has a value
// whether or not the source said anything. The source above declares no <generator>; the re-saved
// document does. The text changed and the model never did.
//
// Chase that and you will spend an afternoon adding exceptions for whitespace, attribute order,
// self-closing tags and the XML declaration, and end up with a comparison that no longer tests
// anything. The document is a representation; what you actually care about is the thing it
// represents. So compare the object graphs.
// ---------------------------------------------------------------------------------------------

RssFeed original = new();
original.Load(SyndicationEncodingUtility.CreateSafeNavigator(FeedXml));

string reSaved = SaveText(original);

Heading("Text in, text out");
Console.WriteLine($"  loaded document   {FeedXml.Length:N0} chars");
Console.WriteLine($"  re-saved document {reSaved.Length:N0} chars");
Console.WriteLine($"  identical?        {string.Equals(FeedXml, reSaved, StringComparison.Ordinal)}");
Console.WriteLine($"  generator now     {Between(reSaved, "<generator>", "</generator>")[..46]}...");
Console.WriteLine("  Nothing is wrong. The writer filled in a default the source omitted.");

// ---------------------------------------------------------------------------------------------
// 2. Comparing the graphs, and doing it in one line
//
// Every entity type in this library implements IEquatable<T> and IComparable<T> and declares ==,
// so `before.Channel == after.Channel` is a structural comparison of the whole subtree, not a
// reference check. That is what makes a round-trip assertion one line instead of fifty, and it is
// the reason to reach for it: fifty hand-written assertions test the fifty properties whose names
// you thought of, and a round trip that compares graphs tests all of them including the ones
// added next year.
//
// One caution about what such a test proves. Comparing a graph to its own reload is a fixed-point
// check, and a fixed point can be reached by two implementations that are wrong in matching ways.
// It tells you the writer and the reader agree; it does not tell you either is right. Keep it --
// it is cheap and catches real regressions -- but do not mistake it for validation.
// ---------------------------------------------------------------------------------------------

RssFeed firstPass = Reload(original);
RssFeed secondPass = Reload(firstPass);

Heading("Comparing graphs instead");
Console.WriteLine($"  original.Channel == firstPass.Channel    {original.Channel == firstPass.Channel}");
Console.WriteLine($"  firstPass.Channel == secondPass.Channel  {firstPass.Channel == secondPass.Channel}");
Console.WriteLine($"  same object?                             {ReferenceEquals(original.Channel, firstPass.Channel)}  -- so == is structural, not reference");
Console.WriteLine($"  every item equal?                        {firstPass.Channel.Items.SequenceEqual(secondPass.Channel.Items)}");

// A structural comparison is only useful if it can fail. Change one character in one item and the
// same one-line assertion must go red -- otherwise it was never testing anything.
RssFeed mutated = Reload(firstPass);
mutated.Channel.Items[2].Title = "Item three, edited";

Console.WriteLine($"  after editing one title:                {firstPass.Channel == mutated.Channel}   <- the assertion can fail");

// ---------------------------------------------------------------------------------------------
// 3. RetrievalLimit makes save-after-load lossy
//
// RetrievalLimit bounds how many entities are built into the object model, which is exactly what
// you want when you are reading the three most recent items of a thousand-item feed and do not
// care about the rest.
//
// It is not a windowing or paging feature, though, and the difference matters the moment you save.
// The object model is what Save writes, so a feed loaded with RetrievalLimit = 2 and then saved is
// a two-item feed. Nothing warns you. If that document is what you publish, you have just
// truncated your own feed to whatever number you chose as an optimisation.
//
// The other half is worth knowing for the opposite reason: it does not save you any I/O. The whole
// document is still downloaded and still parsed into a navigator, because the limit is applied
// while walking that navigator. It bounds object construction, and nothing earlier.
// ---------------------------------------------------------------------------------------------

Heading("RetrievalLimit");
foreach (int limit in new[] { 0, 2, 3 })
{
    RssFeed limited = new();
    limited.Load(
        SyndicationEncodingUtility.CreateSafeNavigator(FeedXml),
        new SyndicationResourceLoadSettings { RetrievalLimit = limit });

    string saved = SaveText(limited);
    int savedItems = saved.Split("<item>", StringSplitOptions.None).Length - 1;

    string label = limit == 0 ? "0 (no limit)" : limit.ToString(CultureInfo.InvariantCulture);
    Console.WriteLine($"  RetrievalLimit = {label,-12} loaded {limited.Channel.Items.Count} items, saved a feed containing {savedItems}");
}

Console.WriteLine();
Console.WriteLine("  The source document has 5 items throughout. The limit shaped the output, not the input.");

// ---------------------------------------------------------------------------------------------
// 4. The other thing that does not survive: what the model has nowhere to put
//
// A round trip preserves what the object model can hold, and drops what it cannot. That is not a
// bug, it is the definition of a model -- but it means "it round-trips" is a claim about the
// model's coverage rather than about the serialiser's correctness.
//
// The feed below carries an element in a namespace nothing here understands. Load it, save it,
// and the element is gone: no error, no warning, no trace. This is precisely the problem
// extensions exist to solve, and samples 09 to 13 are about them. Until then, the honest summary
// is that anything outside the model is dropped silently, and the only way to find out what your
// feed is losing is to look.
// ---------------------------------------------------------------------------------------------

const string WithUnknown = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0" xmlns:endjin="https://endjin.com/ns/invented-for-this-sample">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin.</description>
        <item>
          <title>Item one</title>
          <link>https://endjin.com/blog/1</link>
          <description>The first.</description>
          <endjin:readingTime>4 minutes</endjin:readingTime>
        </item>
      </channel>
    </rss>
    """;

RssFeed withUnknown = new();
withUnknown.Load(SyndicationEncodingUtility.CreateSafeNavigator(WithUnknown));
string afterRoundTrip = SaveText(withUnknown);

Heading("An element the model cannot hold");
Console.WriteLine($"  in the source     {WithUnknown.Contains("readingTime", StringComparison.Ordinal)}");
Console.WriteLine($"  after a round trip {afterRoundTrip.Contains("readingTime", StringComparison.Ordinal)}");
Console.WriteLine($"  item.Extensions   {withUnknown.Channel.Items[0].Extensions.Count} attached");
Console.WriteLine("  Dropped in silence. Sample 09 is how a namespace becomes something the model can hold.");

Console.WriteLine();
Console.WriteLine("Next: 09-extensions-read.cs -- how a namespace in a document becomes an object on an entity.");

static RssFeed Reload(RssFeed feed)
{
    using MemoryStream stream = new();
    feed.Save(stream, new SyndicationResourceSaveSettings());
    stream.Position = 0;

    RssFeed reloaded = new();
    reloaded.Load(stream);

    return reloaded;
}

static string SaveText(RssFeed feed)
{
    using MemoryStream stream = new();
    feed.Save(stream, new SyndicationResourceSaveSettings());
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
    Console.WriteLine(new string('-', title.Length));
}