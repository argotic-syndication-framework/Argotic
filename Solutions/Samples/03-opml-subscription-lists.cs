#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 03 -- OPML
//
//     dotnet run --file Solutions/Samples/03-opml-subscription-lists.cs
//
// The first two samples read documents that carry content. This one reads a document that carries
// addresses: OPML is how a feed reader exports what you are subscribed to, and how you import it
// into the next one. It is also the format an aggregator publishes when it wants to say "here is
// everything I follow" -- a blogroll -- which is where the term subscription list comes from.
//
// OPML is unusual among the formats in this library, and the difference is worth understanding
// before you write any code against it. RSS and Atom define elements. OPML defines exactly one
// element, <outline>, and puts the meaning in its attributes -- and then declines to say which
// attributes exist. The specification names six with fixed meanings and says the rest depend on
// the value of `type`. So the object model cannot be a set of properties, because nobody knows
// what the properties are.
//
// What that costs you in practice is the subject of section 2, and it is the thing most people
// get wrong on their first attempt.
// ---------------------------------------------------------------------------------------------

using System.Globalization;
using System.Text;

using Argotic.Common;
using Argotic.Syndication;

// A blogroll: two categories of feed, one link out to somebody else's OPML, and one commented
// entry that a reader is expected to skip.
const string DocumentXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <opml version="2.0">
      <head>
        <title>endjin's blogroll</title>
        <dateCreated>Mon, 05 Jan 2026 09:00:00 GMT</dateCreated>
        <dateModified>Fri, 07 Aug 2026 12:00:00 GMT</dateModified>
        <ownerName>endjin</ownerName>
        <ownerEmail>hello@endjin.com</ownerEmail>
        <ownerId>https://endjin.com/</ownerId>
        <expansionState>1,3</expansionState>
        <vertScrollState>1</vertScrollState>
      </head>
      <body>
        <outline text=".NET">
          <outline text="endjin blog" type="rss" version="RSS2"
                   xmlUrl="https://endjin.com/rss.xml" htmlUrl="https://endjin.com/blog/"
                   description="Technical writing from endjin" language="en-gb" />
          <outline text="Azure Weekly" type="rss"
                   xmlUrl="https://azureweekly.info/rss.xml" htmlUrl="https://azureweekly.info/" />
        </outline>
        <outline text="Data" category="/endjin/analytics,/endjin/ai">
          <outline text="Power BI Weekly" type="feed"
                   xmlUrl="https://powerbiweekly.info/atom.xml" htmlUrl="https://powerbiweekly.info/" />
          <outline text="A feed we unsubscribed from" type="rss" isComment="true"
                   xmlUrl="https://example.invalid/rss.xml" />
        </outline>
        <outline text="Somebody else's blogroll" type="link" url="https://example.com/blogroll.opml" />
      </body>
    </opml>
    """;

OpmlDocument document = new();
document.Load(SyndicationEncodingUtility.CreateSafeNavigator(DocumentXml));

Heading("What came back");
Console.WriteLine($"  {document.Format} {document.Version}   {document.Head.Title}");
Console.WriteLine($"  owner     {document.Head.Owner?.Name} <{document.Head.Owner?.EmailAddress}>");
Console.WriteLine($"  modified  {document.Head.ModifiedOn:R}");
Console.WriteLine($"  {document.Outlines.Count} top-level outlines");

// ---------------------------------------------------------------------------------------------
// 1. One element, recursively
//
// An outline holds outlines, and that is the whole structure. There are no separate types for a
// folder and a feed -- a folder is simply an outline with children and no type attribute, and the
// tree walk below is all the traversal code OPML ever needs.
//
// The `type` attribute is what tells you which kind of thing you are looking at, and Argotic
// surfaces it as ContentType with two convenience predicates over it. Both compare without regard
// to case, because OPML says to: type="LINK" means what type="link" means. IsSubscriptionList-
// Outline also accepts "feed" as well as the specification's "rss", because that is what
// aggregators write for Atom subscriptions and a reader that insists on the letter of the spec
// silently drops half of a real export.
// ---------------------------------------------------------------------------------------------

Heading("The tree");
foreach (OpmlOutline outline in document.Outlines)
{
    Walk(outline, depth: 1);
}

// ---------------------------------------------------------------------------------------------
// 2. The data is in Attributes, and this is the thing people get wrong
//
// Look at what the walk above prints for a subscription entry. The text is there, the type is
// there -- and the feed URL, which is the entire reason the document exists, is not a property.
//
// This is not an oversight in the object model; it is the format. OPML 2.0 gives fixed meanings
// to exactly six attributes -- text, type, isComment, isBreakpoint, created and category -- and
// those six are modelled as properties. Everything else is defined per type: xmlUrl, htmlUrl,
// description, language, title and version belong to a subscription list, url belongs to a link
// or an include, and an extension may define anything it likes. So they land in Attributes,
// keyed by unprefixed local name, exactly as written.
//
// The consequence is that reading only the typed surface of an OPML document loses the payload.
// A reader that iterates properties gets a tree of labels; the addresses are all in the
// dictionary. Note also that the parser never puts the six modelled attributes into Attributes,
// and you should not either -- the writer emits properties first and the dictionary second, so a
// duplicated key becomes a repeated attribute, which OPML forbids.
// ---------------------------------------------------------------------------------------------

Heading("Where the addresses actually are");
foreach (OpmlOutline feedOutline in AllOutlines(document.Outlines).Where(o => o.IsSubscriptionListOutline))
{
    Console.WriteLine($"  {feedOutline.Text}");
    Console.WriteLine($"    typed properties     Text, ContentType={Quote(feedOutline.ContentType)}, IsCommented={feedOutline.IsCommented}");
    Console.WriteLine($"    Attributes           {string.Join(", ", feedOutline.Attributes.Select(pair => $"{pair.Key}={Quote(pair.Value)}"))}");
}

// Categories is the other place a single attribute expands into a collection. OPML packs a
// comma-separated list into one `category` attribute, and each entry is a slash-delimited path
// borrowed from RSS 2.0 -- so /endjin/analytics is a path and a bare word is a plain tag.
Heading("One attribute, several values");
foreach (OpmlOutline outline in AllOutlines(document.Outlines).Where(o => o.Categories.Count > 0))
{
    Console.WriteLine($"  {outline.Text}: {outline.Categories.Count} categories -- {string.Join(" | ", outline.Categories)}");
}

// ---------------------------------------------------------------------------------------------
// 3. Nesting is bounded, and the bound is a defence rather than a rule
//
// OPML puts no limit on how deep outlines may nest, and the parser is genuinely recursive. Those
// two facts together are a denial of service: a few kilobytes of nothing but opening tags is a
// stack overflow in whatever process parses it, and a stack overflow cannot be caught.
//
// So OpmlOutline.MaxOutlineNestingDepth is a parse limit, not a specification limit. It is 256,
// which is far past any real subscription list, and an outline below it is not loaded. The
// document still parses; you simply do not get the part that could not be reached safely.
//
// The demonstration below is the honest kind: it builds a document deeper than the limit and
// reports the depth that actually came back. If the number printed is not 256, the constant
// changed and this comment is now wrong.
// ---------------------------------------------------------------------------------------------

Heading("A document deeper than the limit");
const int Attempted = 400;
OpmlDocument deep = new();
deep.Load(SyndicationEncodingUtility.CreateSafeNavigator(NestedDocument(Attempted)));

Console.WriteLine($"  MaxOutlineNestingDepth  {OpmlOutline.MaxOutlineNestingDepth}");
Console.WriteLine($"  nested in the document  {Attempted}");
Console.WriteLine($"  depth actually loaded   {Depth(deep.Outlines[0])}");
Console.WriteLine("  The document parsed. The part below the limit did not, and no exception said so.");

// ---------------------------------------------------------------------------------------------
// 4. Building one
//
// Because the payload lives in a dictionary, constructing an outline by hand means remembering
// which keys a given type needs -- and that is exactly the kind of thing a factory should know
// for you. OpmlOutline offers two: CreateSubscriptionListOutline fills in xmlUrl and the optional
// htmlUrl, title, description, version and language; CreateInclusionOutline fills in url.
//
// Use them. Hand-populating Attributes works and is sometimes necessary for an extension, but for
// the two types the specification defines it is a way to ship an outline that names a feed
// without saying where it is -- which is the common malformation, and one nothing will flag.
// ---------------------------------------------------------------------------------------------

OpmlDocument built = new();
built.Head.Title = "endjin's blogroll";
built.Head.ModifiedOn = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc);
built.Head.Owner = new OpmlOwner("endjin", "hello@endjin.com", new Uri("https://endjin.com/"));

OpmlOutline dotnet = new(".NET");
dotnet.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline(
    "endjin blog",
    "rss",
    new Uri("https://endjin.com/rss.xml"),
    new Uri("https://endjin.com/blog/"),
    "RSS2",
    "endjin blog",
    "Technical writing from endjin",
    new CultureInfo("en-GB")));

built.Outlines.Add(dotnet);
built.Outlines.Add(OpmlOutline.CreateInclusionOutline(
    "Somebody else's blogroll",
    new Uri("https://example.com/blogroll.opml")));

using MemoryStream stream = new();
built.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });

Heading("Written back out");
Console.WriteLine($"  {stream.Length:N0} bytes");
Console.WriteLine($"  the factory filled in: {string.Join(", ", dotnet.Outlines[0].Attributes.Keys.Order())}");

Console.WriteLine();
Console.WriteLine("Next: 04-sitemaps.cs -- a format with a hard size cap, and the index that exists because of it.");

static void Walk(OpmlOutline outline, int depth)
{
    string kind = outline switch
    {
        { IsSubscriptionListOutline: true } => "feed",
        { IsInclusionOutline: true } => "link",
        _ => "folder",
    };

    string commented = outline.IsCommented ? "  (commented out -- readers skip this)" : string.Empty;
    Console.WriteLine($"  {new string(' ', depth * 2)}{outline.Text}  [{kind}]{commented}");

    foreach (OpmlOutline child in outline.Outlines)
    {
        Walk(child, depth + 1);
    }
}

static IEnumerable<OpmlOutline> AllOutlines(IEnumerable<OpmlOutline> outlines)
{
    foreach (OpmlOutline outline in outlines)
    {
        yield return outline;

        foreach (OpmlOutline descendant in AllOutlines(outline.Outlines))
        {
            yield return descendant;
        }
    }
}

static int Depth(OpmlOutline outline)
{
    int deepest = 0;
    foreach (OpmlOutline child in outline.Outlines)
    {
        deepest = Math.Max(deepest, Depth(child));
    }

    return deepest + 1;
}

static string NestedDocument(int levels)
{
    StringBuilder builder = new();
    builder.Append("<opml version=\"2.0\"><head><title>deep</title></head><body>");
    for (int level = 0; level < levels; level++)
    {
        builder.Append(CultureInfo.InvariantCulture, $"<outline text=\"level {level}\">");
    }

    builder.Append(string.Concat(Enumerable.Repeat("</outline>", levels)));
    builder.Append("</body></opml>");

    return builder.ToString();
}

static string Quote(string value) => value.Length == 0 ? "\"\"" : $"\"{value}\"";

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}