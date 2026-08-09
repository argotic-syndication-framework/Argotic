#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 05 -- You have bytes and no idea what they are
//
//     dotnet run --file Solutions/Samples/05-format-agnostic.cs
//
// Samples 01 to 04 each began by knowing the answer. You wrote `new RssFeed()` because you knew
// it was RSS. That is fine for a feed you control and useless for the general case: a user pastes
// a URL, an OPML import hands you 400 addresses, a crawler follows a <link rel="alternate">. In
// none of those do you know what will come back, and guessing from the file extension is not a
// plan -- endjin.com/rss.xml could be Atom tomorrow and nothing would announce it.
//
// This sample is about the three instruments the library gives you for that situation, and about
// exactly how much each one actually knows. The last part matters more than it sounds: format
// detection here is cheaper than most people assume, and correspondingly less certain.
// ---------------------------------------------------------------------------------------------

using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;

const string RssXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin.</description>
        <language>en-gb</language>
        <lastBuildDate>Fri, 07 Aug 2026 12:00:00 GMT</lastBuildDate>
        <category>Cloud Native App Dev</category>
        <item>
          <title>The GenAI Reality Check</title>
          <link>https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra</link>
          <description>A new instrument in an orchestra that already existed.</description>
          <dc:creator>Barry Smart</dc:creator>
          <pubDate>Thu, 14 May 2026 08:54:29 GMT</pubDate>
          <category>AI</category>
        </item>
      </channel>
    </rss>
    """;

const string AtomXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <feed xmlns="http://www.w3.org/2005/Atom">
      <id>https://endjin.com/</id>
      <title type="text">endjin blog</title>
      <subtitle type="text">Technical writing from endjin.</subtitle>
      <updated>2026-08-07T12:00:00Z</updated>
      <entry>
        <id>urn:uuid:8f2d4c1e-9b37-4a5e-8c12-1d3e5a7b9c02</id>
        <title type="text">Writing Effective Copilot Instructions</title>
        <updated>2026-08-07T00:00:00Z</updated>
        <published>2026-08-07T00:00:00Z</published>
        <link rel="alternate" type="text/html" href="https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases" />
        <summary type="text">Modular skill files rather than one monolith.</summary>
        <category term="ai" />
      </entry>
    </feed>
    """;

// ---------------------------------------------------------------------------------------------
// 1. What detection actually looks at
//
// SyndicationContentFormatGet takes a Stream, an XmlReader or an XPathNavigator, and in all three
// cases it does the same thing: advance to the first element and look up its local name in a
// table. <rss> is Rss, <feed> is Atom, <urlset> is Sitemap, <opml> is Opml, <entry> is a
// stand-alone Atom entry document. That is the whole algorithm.
//
// This is worth knowing for two opposite reasons. It is very cheap -- no DOM is built, and the
// XmlReader overload stops at the first element rather than reading a five-megabyte feed to learn
// one word. And it is correspondingly shallow: it looks at the *local name* and never at the
// namespace, so any document whose root element happens to be called `feed` is reported as Atom
// no matter which vocabulary it belongs to.
//
// That is not a defect to route around. Detection tells you which parser to try; the parser is
// what tells you whether you were right. Section 3 is where being wrong shows up.
// ---------------------------------------------------------------------------------------------

Heading("Detection by root element");
Detect("RSS", RssXml);
Detect("Atom", AtomXml);
Detect("a sitemap", """<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"></urlset>""");
Detect("an OPML file", """<opml version="2.0"><head /><body /></opml>""");
Detect("an APP service doc", """<service xmlns="http://www.w3.org/2007/app"></service>""");
Detect("something else", """<html><body>Not a feed at all.</body></html>""");

Console.WriteLine();
Console.WriteLine("  And now the part that surprises people:");
Detect("feed, wrong namespace", """<feed xmlns="https://example.invalid/definitely-not-atom"><nonsense /></feed>""");
Console.WriteLine("  The namespace was never consulted. Detection names a parser to try, not a verdict.");

// ---------------------------------------------------------------------------------------------
// 2. Reading without choosing
//
// GenericSyndicationFeed detects the format, parses it with the right implementation, and then
// projects the result onto the small set of things RSS, Atom and OPML can all express: a title, a
// description, a language, a last-updated timestamp, categories, and items with a title, a
// summary and a published date.
//
// Notice what is not in that list. No links, no enclosures, no author -- because the three
// formats disagree about all of them, and a lowest common denominator that quietly picked one
// interpretation would be worse than not offering the property. If you need the parts that differ
// you have to commit to a format, and section 3 is how you do that without re-parsing.
//
// One asymmetry to know about, because it will otherwise cost you five minutes: every other
// resource type in this library offers Load(IXPathNavigable), Load(Stream) and Load(XmlReader).
// GenericSyndicationFeed offers Load(string) and Load(Stream), and no XmlReader overload -- and it
// has no Save at all, which follows from what it is. A projection cannot be written back out,
// because the thing it projected from is where the information lives.
// ---------------------------------------------------------------------------------------------

Heading("The same code over both documents");
foreach ((string label, string xml) in new[] { ("RSS", RssXml), ("Atom", AtomXml) })
{
    GenericSyndicationFeed generic = new();
    generic.Load(xml);

    Console.WriteLine($"  {label}");
    Console.WriteLine($"    format     {generic.Format}");
    Console.WriteLine($"    title      {generic.Title}");
    Console.WriteLine($"    language   {generic.Language?.Name ?? "(absent)"}");
    Console.WriteLine($"    updated    {(generic.LastUpdatedOn == DateTime.MinValue ? "(absent)" : generic.LastUpdatedOn.ToString("u"))}");

    foreach (GenericSyndicationItem item in generic.Items)
    {
        string terms = string.Join(", ", item.Categories.Select(category => category.Term));
        Console.WriteLine($"    item       {item.PublishedOn:yyyy-MM-dd}  {item.Title}   [{terms}]");
    }
}

// ---------------------------------------------------------------------------------------------
// 3. Getting back to the real thing
//
// GenericSyndicationFeed.Resource holds the ISyndicationResource it parsed, so the projection is
// a view rather than a replacement. Read generically to decide what you have, then downcast once
// to reach the members only that format has. The document is parsed exactly once either way.
//
// This is the pattern to reach for when a requirement arrives that the common subset cannot meet
// -- "we need the enclosure URL", say. You do not restructure around it; you switch on Format and
// take the branch you need.
// ---------------------------------------------------------------------------------------------

Heading("Downcasting when the common subset is not enough");
foreach (string xml in new[] { RssXml, AtomXml })
{
    GenericSyndicationFeed generic = new();
    generic.Load(xml);

    string firstLink = generic.Resource switch
    {
        RssFeed rss => rss.Channel.Items[0].Link?.ToString() ?? "(no link)",
        AtomFeed atom => atom.Entries[0].Links.FirstOrDefault()?.Uri?.ToString() ?? "(no link)",
        _ => "(a format this branch does not handle)",
    };

    Console.WriteLine($"  {generic.Format,-6} first item link  {firstLink}");
}

// ---------------------------------------------------------------------------------------------
// 4. Version, and what the document says it carries
//
// SyndicationContentFormatGet answers "which parser", and for most decisions that is enough. Two
// further questions come up often enough to be worth knowing where to send them.
//
// The first is which version of the format you have -- RSS 2.0 and RSS 0.91 are both <rss>, and
// the difference is an attribute. SyndicationResourceMetadata reads that in one pass without
// building an object model. Hand it the navigator exactly as CreateSafeNavigator returns it,
// positioned on the document root; it walks down to the document element itself.
//
// The second is which vocabularies the document declares, and that one you ask the navigator
// directly. Namespace declarations live on the document element, so move to it and call
// GetNamespacesInScope -- from the document root, which is the node above <rss>, nothing has been
// declared yet and you will correctly be told there is nothing in scope. That is the trap, and an
// empty list is a lie that looks like data.
//
// The namespace list is how you decide how much work a feed deserves before doing any of it. A
// document declaring the iTunes namespace is a podcast; one declaring Dublin Core has per-item
// authorship that RSS cannot express on its own. Samples 09 onwards turn those declarations into
// objects -- this is how you see them coming.
// ---------------------------------------------------------------------------------------------

Heading("Version, and declared vocabularies");

SyndicationResourceMetadata metadata = new(SyndicationEncodingUtility.CreateSafeNavigator(RssXml));
Console.WriteLine($"  format   {metadata.Format} {metadata.Version}");

XPathNavigator element = SyndicationEncodingUtility.CreateSafeNavigator(RssXml);
element.MoveToChild(XPathNodeType.Element);

Console.WriteLine($"  in scope at the document root, above <{element.LocalName}>:");
Console.WriteLine("    (nothing -- declarations have not been reached yet)");
Console.WriteLine($"  in scope on <{element.LocalName}>:");
foreach ((string prefix, string uri) in element.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml)
    .OrderBy(pair => pair.Key, StringComparer.Ordinal))
{
    Console.WriteLine($"    {(prefix.Length == 0 ? "(default)" : prefix),-10} {uri}");
}

Console.WriteLine();
Console.WriteLine("Next: 06-saving-and-encoding.cs -- the boundary where an object graph becomes bytes.");

static void Detect(string label, string xml)
{
    SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(
        SyndicationEncodingUtility.CreateSafeNavigator(xml));

    // Skip the XML declaration if there is one -- <?xml ...?> is a processing instruction, not the
    // document element, and detection never looks at it.
    int start = xml.IndexOf("?>", StringComparison.Ordinal) is int prolog and >= 0 ? prolog + 2 : 0;
    start = xml.IndexOf('<', start);
    int end = xml.IndexOfAny([' ', '>'], start);

    Console.WriteLine($"  {label,-22} root <{xml[(start + 1)..end]}>  ->  {format}");
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}