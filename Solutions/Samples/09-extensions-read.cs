#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 09 -- How a namespace becomes an object
//
//     dotnet run --file Solutions/Samples/09-extensions-read.cs
//
// Sample 08 ended on a small disappointment: an element in an unrecognised namespace was loaded,
// saved, and silently lost, because the object model had nowhere to put it. That is where most
// people's first encounter with a real feed goes wrong, because a real feed is mostly not RSS.
//
// Open any podcast feed and count. The RSS 2.0 vocabulary gets you a title, a link, a date and an
// enclosure; the episode number, the season, the artwork, the transcript, the author of that one
// episode and the licence it is published under are all in other namespaces. RSS was designed for
// that -- the specification says an element in a namespace other than the default is somebody
// else's business -- and it is the reason RSS is still in use twenty-five years later.
//
// So the interesting question is not "how do I read RSS" but "how does this library turn a
// namespace it recognises into something I can call properties on". The answer takes one line of
// consumer code, and this sample is mostly about what that line does not have to say.
// ---------------------------------------------------------------------------------------------

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

// Four namespaces on one feed, which no real publisher would do -- Dublin Core for per-item
// authorship, content:encoded for the full article, slash for comment counts, wfw for the comment
// endpoints. Each is carried by a different entity, which is the point of section 2.
const string FeedXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0"
         xmlns:dc="http://purl.org/dc/elements/1.1/"
         xmlns:content="http://purl.org/rss/1.0/modules/content/"
         xmlns:slash="http://purl.org/rss/1.0/modules/slash/"
         xmlns:wfw="http://wellformedweb.org/CommentAPI/">
      <channel>
        <title>endjin blog</title>
        <link>https://endjin.com/blog/</link>
        <description>Technical writing from endjin on .NET, data, analytics and AI.</description>
        <dc:publisher>endjin limited</dc:publisher>
        <dc:rights>Copyright 2026 endjin limited</dc:rights>
        <dc:language>en-gb</dc:language>
        <item>
          <title>The GenAI Reality Check: New Instrument, Same Orchestra</title>
          <link>https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra</link>
          <description>Generative AI is a new instrument in an orchestra that already existed.</description>
          <pubDate>Thu, 14 May 2026 08:54:29 GMT</pubDate>
          <dc:creator>Barry Smart</dc:creator>
          <dc:subject>Artificial Intelligence</dc:subject>
          <content:encoded><![CDATA[<p>Every wave of technology arrives claiming to replace the orchestra.</p>]]></content:encoded>
          <slash:comments>17</slash:comments>
          <slash:section>ai</slash:section>
          <wfw:comment>https://endjin.com/blog/genai-reality-check/comment</wfw:comment>
          <wfw:commentRss>https://endjin.com/blog/genai-reality-check/comments.xml</wfw:commentRss>
        </item>
        <item>
          <title>Writing Effective Copilot Instructions for Complex Codebases</title>
          <link>https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases</link>
          <description>Modular skill files rather than one monolith.</description>
          <pubDate>Fri, 07 Aug 2026 00:00:00 GMT</pubDate>
          <dc:creator>Howard van Rooijen</dc:creator>
        </item>
      </channel>
    </rss>
    """;

// ---------------------------------------------------------------------------------------------
// 1. Nothing registers anything
//
// This is the whole of the setup:
//
//     RssFeed feed = new();
//     feed.Load(navigator);
//
// There is no configuration step, no list of extensions to enable, no attribute on a class. On
// load, the library reflects over its own exported types to find every SyndicationExtension it
// ships, asks the ones whose namespace or prefix is bound on the document whether they are
// present, and attaches each that says yes to the entity whose elements it found.
//
// SyndicationResourceLoadSettings.AutoDetectExtensions controls that and defaults to true, which
// is why the two lines above are enough. Section 4 is about turning it off, and about the one
// case where you have to do something -- an extension living in your assembly rather than this
// one, which reflection over *this* assembly cannot possibly find.
// ---------------------------------------------------------------------------------------------

RssFeed feed = new();
feed.Load(SyndicationEncodingUtility.CreateSafeNavigator(FeedXml));

Heading("What attached, and to what");
Console.WriteLine($"  channel   {Describe(feed.Channel.Extensions)}");
for (int index = 0; index < feed.Channel.Items.Count; index++)
{
    Console.WriteLine($"  item {index + 1}    {Describe(feed.Channel.Items[index].Extensions)}");
}

// ---------------------------------------------------------------------------------------------
// 2. Extensions attach to the entity that carried them
//
// Note what the list above shows: the channel has Dublin Core, and so does the first item, and
// they are different objects holding different values. That follows from the markup -- <dc:rights>
// was a child of <channel> and <dc:creator> a child of <item> -- but it is worth stating because
// the alternative design, one bag of extensions per document, is what people expect and would be
// useless. Per-item authorship is the entire reason Dublin Core appears in RSS feeds.
//
// The second item declares none of the four namespaces' elements except dc:creator, and it has
// exactly one extension attached. An extension is present when its elements are, not when the
// document declares its namespace: the xmlns bindings are on <rss>, so all four are in scope for
// every item, and only the ones actually used attach.
// ---------------------------------------------------------------------------------------------

Heading("The same extension, different entities");
if (feed.Channel.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) is DublinCoreElementSetSyndicationExtension channelDc)
{
    Console.WriteLine($"  channel dc:publisher  {channelDc.Context.Publisher}");
    Console.WriteLine($"  channel dc:rights     {channelDc.Context.Rights}");
    Console.WriteLine($"  channel dc:creator    {Absent(channelDc.Context.Creator)}");
}

foreach (RssItem item in feed.Channel.Items)
{
    if (item.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) is DublinCoreElementSetSyndicationExtension itemDc)
    {
        Console.WriteLine($"  item dc:creator       {itemDc.Context.Creator}");
        Console.WriteLine($"  item dc:publisher     {Absent(itemDc.Context.Publisher)}   <- not inherited from the channel");
    }
}

// ---------------------------------------------------------------------------------------------
// 3. The one line of consumer code, and how to write it well
//
// FindExtension takes a predicate, and every framework extension supplies a static MatchByType
// suitable for it. The pattern-matching form below does the find and the cast together, which is
// why it reads as one line rather than three.
//
// Two things to get right. FindExtension is a linear scan of the Extensions list with no index by
// type, so reading six properties off one extension should call it once and hold the result -- the
// loop below binds `slash` once and then reads three properties. Calling it per property is a scan
// per property, and on a thousand-item feed that is a thousand scans you did not need.
//
// The other is the same sentinel problem sample 01 raised, and extensions are not exempt. Slash's
// comment count is an int, so its absent value is int.MinValue rather than null or zero. A feed
// that mentions slash but not slash:comments has a comment count of int.MinValue, and adding that
// to a running total produces a number worth looking at.
// ---------------------------------------------------------------------------------------------

Heading("Reading one extension properly");
foreach (RssItem item in feed.Channel.Items)
{
    Console.WriteLine($"  {item.Title[..40]}...");

    // Bound once, read three times.
    if (item.FindExtension(SiteSummarySlashSyndicationExtension.MatchByType) is SiteSummarySlashSyndicationExtension slash)
    {
        string comments = slash.Context.Comments == int.MinValue
            ? "(absent -- and it is int.MinValue, not 0)"
            : slash.Context.Comments.ToString(System.Globalization.CultureInfo.InvariantCulture);

        Console.WriteLine($"    slash:comments  {comments}");
        Console.WriteLine($"    slash:section   {Absent(slash.Context.Section)}");
    }
    else
    {
        Console.WriteLine("    slash           (no extension attached -- the item used none of its elements)");
    }

    if (item.FindExtension(WellFormedWebCommentsSyndicationExtension.MatchByType) is WellFormedWebCommentsSyndicationExtension wfw)
    {
        Console.WriteLine($"    wfw:commentRss  {wfw.Context.CommentsFeed}");
    }

    if (item.FindExtension(SiteSummaryContentSyndicationExtension.MatchByType) is SiteSummaryContentSyndicationExtension content)
    {
        // content:encoded is the full article body, and the reason it exists is that RSS's own
        // <description> is conventionally a summary. A reader wanting the whole post looks here
        // first and falls back to description.
        Console.WriteLine($"    content:encoded {content.Context.Encoded.Length} chars of HTML");
    }
}

// ---------------------------------------------------------------------------------------------
// 4. Turning it off, and narrowing it down
//
// AutoDetectExtensions = false stops the discovery entirely: the elements are still in the
// document, they are simply not turned into objects, and Extensions comes back empty. That is
// worth having when you are reading a very large number of feeds and want only the core
// vocabulary, because attaching an extension costs a parse of its elements.
//
// SupportedExtensions is the middle setting, and the more useful one. Put types in it and the
// discovery considers those *in addition to* the framework set filtered by namespace -- so it is
// how a consumer registers an extension living in their own assembly, which reflection over this
// assembly could never find. Sample 13 writes one.
//
// The measurement below is the honest way to decide: if the difference is one object per item,
// leave it on.
// ---------------------------------------------------------------------------------------------

Heading("AutoDetectExtensions");
foreach (bool autoDetect in new[] { true, false })
{
    RssFeed probe = new();
    probe.Load(
        SyndicationEncodingUtility.CreateSafeNavigator(FeedXml),
        new SyndicationResourceLoadSettings { AutoDetectExtensions = autoDetect });

    int attached = probe.Channel.Extensions.Count + probe.Channel.Items.Sum(item => item.Extensions.Count);
    Console.WriteLine($"  AutoDetectExtensions = {autoDetect,-5}  {attached} extensions attached across the document");
}

Console.WriteLine();
Console.WriteLine("  With it off the elements are still in the document -- they are simply not objects,");
Console.WriteLine("  and a save from that graph writes none of them. Sample 10 is the writing direction.");

Console.WriteLine();
Console.WriteLine("Next: 10-extensions-write.cs -- attaching extensions, and never declaring an xmlns by hand.");

static string Describe(IList<ISyndicationExtension> extensions) =>
    extensions.Count == 0
        ? "(none)"
        : $"{extensions.Count}: {string.Join(", ", extensions.Select(extension => extension.XmlPrefix))}";

static string Absent(string value) => value.Length == 0 ? "(absent)" : value;

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}