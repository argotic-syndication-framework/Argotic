#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 02 -- Atom 1.0
//
//     dotnet run --file Solutions/Samples/02-atom-feed.cs
//
// Atom was written by people who had implemented RSS and knew where it hurt. RSS says a channel
// needs a title, a link and a description, and then leaves almost every question of meaning to
// convention: is a <guid> a URL you can follow? is a <description> text or HTML? what time zone
// is that date in? Atom answers each of those in the format itself, and the object model here is
// shaped by those answers rather than by the elements.
//
// Three of them matter enough to organise this file around:
//
//   * Identity is separate from location. <id> is an IRI that names the entry forever; <link> is
//     where it currently lives. RSS conflated the two and had to invent isPermaLink to unpick it.
//   * Every human-readable string carries its own type. A title is an AtomTextConstruct, not a
//     string, because "is this markup?" has to be answerable without guessing.
//   * A link's relation has a default. That default is not written in the document, which is why
//     the naive comparison you are about to write is wrong.
//
// Sample 01 covered the API shape -- Load, Save, the get-only collections -- and none of it
// changes here. This file is about the model.
// ---------------------------------------------------------------------------------------------

using Argotic.Common;
using Argotic.Syndication;

// endjin's Atom feed, trimmed to two entries and given the link variety a real feed accumulates.
// The alternate link on the second entry deliberately has no rel attribute, which is legal and
// common: RFC 4287 section 4.2.7.2 makes "alternate" the default.
const string FeedXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <feed xmlns="http://www.w3.org/2005/Atom" xml:lang="en-GB">
      <id>https://endjin.com/</id>
      <title type="text">endjin blog</title>
      <subtitle type="html">Technical writing on &lt;em&gt;.NET, data and AI&lt;/em&gt;</subtitle>
      <updated>2026-08-07T12:00:00Z</updated>
      <rights type="text">Copyright 2026 endjin limited</rights>
      <generator uri="https://github.com/argotic-syndication-framework/argotic/" version="4.0">Argotic</generator>
      <icon>https://endjin.com/favicon.ico</icon>
      <logo>https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png</logo>
      <link rel="self" type="application/atom+xml" href="https://endjin.com/atom.xml" />
      <link rel="alternate" type="text/html" href="https://endjin.com/blog/" />
      <link rel="hub" href="https://pubsubhubbub.appspot.com/" />
      <author>
        <name>endjin</name>
        <email>hello@endjin.com</email>
        <uri>https://endjin.com/</uri>
      </author>
      <category term="dotnet" scheme="https://endjin.com/what-we-do/" label=".NET" />
      <entry>
        <id>urn:uuid:60a76c80-d399-11d9-b93c-0003939e0af6</id>
        <title type="text">The GenAI Reality Check: New Instrument, Same Orchestra</title>
        <updated>2026-05-14T08:54:29Z</updated>
        <published>2026-05-14T08:54:29Z</published>
        <link rel="alternate" type="text/html" href="https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra" />
        <link rel="enclosure" type="audio/mpeg" length="36588921" href="https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check.mp3" />
        <author><name>Barry Smart</name></author>
        <category term="ai" label="AI" />
        <summary type="text">Generative AI is a new instrument in an orchestra that already existed.</summary>
        <content type="html">&lt;p&gt;Every wave of technology arrives claiming to replace the orchestra.&lt;/p&gt;</content>
      </entry>
      <entry>
        <id>urn:uuid:8f2d4c1e-9b37-4a5e-8c12-1d3e5a7b9c02</id>
        <title type="text">Writing Effective Copilot Instructions for Complex Codebases</title>
        <updated>2026-08-07T00:00:00Z</updated>
        <link href="https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases" />
        <link rel="related" href="https://endjin.com/blog/tag/ai/" />
        <content type="text" src="https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases.txt" />
      </entry>
    </feed>
    """;

AtomFeed feed = new();
feed.Load(SyndicationEncodingUtility.CreateSafeNavigator(FeedXml));

Heading("What came back");
Console.WriteLine($"  {feed.Format} {feed.Version}   {feed.Title?.Content}");
Console.WriteLine($"  id       {feed.Id?.Value}");
Console.WriteLine($"  updated  {feed.UpdatedOn:O}");
Console.WriteLine($"  lang     {feed.Language?.Name ?? "(absent)"}");
Console.WriteLine($"  {feed.Entries.Count} entries");

// ---------------------------------------------------------------------------------------------
// 1. Three things are required, so the constructor asks for them
//
// RFC 4287 makes <id>, <title> and <updated> mandatory on a feed and on every entry. That is why
// AtomFeed and AtomEntry both offer a constructor taking exactly those three, and why the
// parameterless one leaves Id and Title null: the type will not invent an identity for you, and
// there is no sentinel that could mean "this entry has no name".
//
// UpdatedOn is the one exception to the "reference types are nullable" habit from sample 01. It
// is a DateTime, so it takes the DateTime.MinValue sentinel -- and a feed whose updated timestamp
// is MinValue is a feed that will not save as conforming Atom. Check it before you publish.
// ---------------------------------------------------------------------------------------------

Heading("Required members");
Console.WriteLine($"  feed.Id       {(feed.Id is null ? "(missing -- will not save)" : "present")}");
Console.WriteLine($"  feed.Title    {(feed.Title is null ? "(missing -- will not save)" : "present")}");
Console.WriteLine($"  feed.Updated  {(feed.UpdatedOn == DateTime.MinValue ? "(missing -- will not save)" : "present")}");

// An id is an IRI, not a URL. urn:uuid: is the idiomatic choice for an entry whose canonical
// location may move, and Argotic exposes both readings: Value is the text exactly as written, and
// Uri is the parsed form for the cases where you want to compare schemes or hosts.
Heading("Identity is not location");
foreach (AtomEntry entry in feed.Entries)
{
    AtomLink? alternate = entry.Links.FirstOrDefault(link => link.EffectiveRelation == "alternate");
    Console.WriteLine($"  id        {entry.Id?.Value}");
    Console.WriteLine($"    scheme  {entry.Id?.Uri?.Scheme ?? "(unparseable as a Uri)"}");
    Console.WriteLine($"    lives at {alternate?.Uri?.ToString() ?? "(no alternate link)"}");
}

// ---------------------------------------------------------------------------------------------
// 2. The default relation that is not in the document
//
// RFC 4287 section 4.2.7.2: "If the 'rel' attribute is not present, the link element MUST be
// interpreted as if the link relation type is 'alternate'." So a document is entitled to omit it,
// and plenty do -- the second entry above writes <link href="..."/> with nothing else.
//
// Argotic keeps both readings apart on purpose. Relation is the attribute exactly as written, and
// it is an empty string when the attribute was absent, so that saving does not invent an
// attribute the publisher never wrote. EffectiveRelation is the interpreted value, and it is what
// you almost always want.
//
// The failure this prevents is quiet. `links.Where(l => l.Relation == "alternate")` compiles,
// runs, returns some links, and silently misses every entry that relied on the default -- which
// in a mixed corpus is most of them.
// ---------------------------------------------------------------------------------------------

Heading("Relation vs EffectiveRelation");
foreach (AtomLink link in feed.Entries[1].Links)
{
    Console.WriteLine($"  written {Quote(link.Relation),-12} interpreted {Quote(link.EffectiveRelation),-12} {link.Uri}");
}

int naive = feed.Entries.Sum(entry => entry.Links.Count(link => link.Relation == "alternate"));
int correct = feed.Entries.Sum(entry => entry.Links.Count(link => link.EffectiveRelation == "alternate"));
Console.WriteLine();
Console.WriteLine($"  alternate links, comparing Relation:           {naive}");
Console.WriteLine($"  alternate links, comparing EffectiveRelation:  {correct}   <- the true count");

// ---------------------------------------------------------------------------------------------
// 3. Text is never just text
//
// A title, a subtitle, a summary and a rights statement are all AtomTextConstruct, and the type
// exists to carry one extra bit of information: whether Content is plain text, escaped HTML, or
// inline XHTML. RSS has no way to say this, which is why every RSS reader has a heuristic for
// deciding whether to render a description as markup, and why those heuristics disagree.
//
// TextType.None means the type attribute was absent, and RFC 4287 says to treat that as "text".
// So the safe read is: Html or Xhtml means you have markup and must sanitise it; anything else
// means you have characters and must escape them before putting them in a page. Getting that
// backwards is how a feed reader becomes an XSS vector.
// ---------------------------------------------------------------------------------------------

Heading("Text constructs");
Show("feed.Title", feed.Title);
Show("feed.Subtitle", feed.Subtitle);
Show("feed.Rights", feed.Rights);
Show("entry[0].Summary", feed.Entries[0].Summary);

// Content is the entry's body and is a different type again, because it can do something no text
// construct can: point somewhere else. The second entry carries content type="text" with a src
// attribute and no body at all -- the content exists, but you have to fetch it. A reader that
// prints Content.Content for that entry prints nothing and reports no problem.
Heading("Content, which may not be here");
foreach (AtomEntry entry in feed.Entries)
{
    AtomContent? content = entry.Content;
    Console.WriteLine($"  {entry.Title?.Content[..40]}...");
    Console.WriteLine($"    type    {Quote(content?.ContentType ?? string.Empty)}");
    Console.WriteLine(content?.Source is { } source
        ? $"    src     {source}   <- body is remote, Content is empty"
        : $"    inline  {content?.Content}");
}

// ---------------------------------------------------------------------------------------------
// 4. Building one
//
// The three-argument constructor is the honest way to make a feed, because it is impossible to
// forget one of the three required members. Everything after it is optional and reads the same as
// sample 01 -- get-only collections mutated in place, nullable references for absent elements.
//
// Note that AtomLink takes its relation as a plain string. There is no enum, and that is correct:
// RFC 5988 made link relations an IANA registry that grows without a specification change, so an
// enum would be wrong by the time it shipped. Use the registered names; invent an IRI if you need
// something the registry does not have.
// ---------------------------------------------------------------------------------------------

AtomFeed built = new(
    new AtomId(new Uri("https://endjin.com/audio.xml")),
    new AtomTextConstruct("endjin blog, read aloud") { TextType = AtomTextConstructType.Text },
    new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc));

built.Links.Add(new AtomLink(new Uri("https://endjin.com/audio.xml"), "self"));
built.Links.Add(new AtomLink(new Uri("https://endjin.com/blog/"), "alternate"));
built.Authors.Add(new AtomPersonConstruct("endjin") { EmailAddress = "hello@endjin.com" });

AtomEntry episode = new(
    new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93c-0003939e0af6")),
    new AtomTextConstruct("The GenAI Reality Check") { TextType = AtomTextConstructType.Text },
    new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc))
{
    PublishedOn = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
    Summary = new AtomTextConstruct("A new instrument in an orchestra that already existed.")
    {
        TextType = AtomTextConstructType.Text,
    },
};

episode.Links.Add(new AtomLink(
    new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check.mp3"),
    "enclosure")
{
    ContentType = "audio/mpeg",
    Length = 36_588_921L,
});

built.Entries.Add(episode);

// Atom dates are RFC 3339, which unlike RFC 822 carries an unambiguous offset -- so a round trip
// through the wire preserves the instant rather than the wall clock. Sample 07 is where that
// distinction is worked through properly; it is the reason these DateTimes are constructed Utc.
using MemoryStream stream = new();
built.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });

Heading("Written back out");
Console.WriteLine($"  {stream.Length:N0} bytes, {built.Entries.Count} entry");
Console.WriteLine($"  updated serialised as {Serialised(stream, "<updated>", "</updated>")}");

Console.WriteLine();
Console.WriteLine("Next: 03-opml-subscription-lists.cs -- the format that describes other feeds, and where it hides its data.");

static void Show(string label, AtomTextConstruct? text)
{
    if (text is null)
    {
        Console.WriteLine($"  {label,-18} (absent)");
        return;
    }

    // None means no type attribute was written, which RFC 4287 says to read as text.
    string effective = text.TextType == AtomTextConstructType.None ? "None -> text" : text.TextType.ToString();
    Console.WriteLine($"  {label,-18} [{effective}] {text.Content}");
}

static string Quote(string value) => value.Length == 0 ? "\"\"" : $"\"{value}\"";

static string Serialised(MemoryStream stream, string open, string close)
{
    string document = System.Text.Encoding.UTF8.GetString(stream.ToArray());
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