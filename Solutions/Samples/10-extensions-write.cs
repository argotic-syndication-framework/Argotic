#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 10 -- Attaching extensions, and never declaring an xmlns by hand
//
//     dotnet run --file Solutions/Samples/10-extensions-write.cs
//
// Reading extensions took one line and no configuration. Writing them takes one line too, and the
// interesting part is again what you do not have to write.
//
// Consider what publishing a namespaced element actually requires. The element goes on an item,
// but the namespace declaration goes on the document element -- somewhere else entirely, in a
// part of the document you were not thinking about when you decided to tag that one post with a
// location. Get it wrong and the document is not merely incorrect, it is not well-formed XML, and
// every consumer rejects the whole feed rather than the one element.
//
// So this is a job for the library rather than for you, and the mechanism that does it is worth
// understanding because it explains a behaviour that otherwise looks like magic.
// ---------------------------------------------------------------------------------------------

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

RssFeed feed = new();
feed.Channel.Title = "endjin blog";
feed.Channel.Link = new Uri("https://endjin.com/blog/");
feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";

feed.Channel.Items.Add(Post("The GenAI Reality Check", "genai-reality-check-new-instrument-same-orchestra"));
feed.Channel.Items.Add(Post("Writing Effective Copilot Instructions", "writing-effective-copilot-instructions-for-complex-codebases"));
feed.Channel.Items.Add(Post("Rx.NET v7.0 Released", "rx-dotnet-v7-released"));

// ---------------------------------------------------------------------------------------------
// 1. Attaching is a list operation
//
// There is no AddExtension method. Extensions is an IList<ISyndicationExtension> like every other
// collection in this library, and you mutate it in place. Every extensible entity has one: the
// feed, the channel, each item, each category, each enclosure, the image, the cloud, the text
// input. Where you attach determines where the elements are written.
//
// Each framework extension carries a Context holding its properties, so the shape is always the
// same: construct, fill in the context, add to a collection.
//
// Only the third post is tagged with a location. That is deliberate, and section 2 is why.
// ---------------------------------------------------------------------------------------------

GeoRssSyndicationExtension location = new();
location.Context.Point = new GeoRssPosition(51.5074m, -0.1278m);
location.Context.FeatureName = "endjin, London";
feed.Channel.Items[2].Extensions.Add(location);

CreativeCommonsSyndicationExtension licence = new();
licence.Context.Licenses.Add(new Uri("https://creativecommons.org/licenses/by/4.0/"));
feed.Channel.Extensions.Add(licence);

Heading("What is attached, and where");
Console.WriteLine($"  channel   {Describe(feed.Channel.Extensions.Select(e => e.XmlPrefix))}");
for (int index = 0; index < feed.Channel.Items.Count; index++)
{
    Console.WriteLine($"  item {index + 1}    {Describe(feed.Channel.Items[index].Extensions.Select(e => e.XmlPrefix))}");
}

// ---------------------------------------------------------------------------------------------
// 2. The declarations come from the object graph
//
// SyndicationResourceSaveSettings.AutoDetectExtensions defaults to true, and on save it walks the
// whole graph -- feed, channel, image, cloud, text input, every category, every item and their
// children -- collecting the type of every extension actually attached. Only then does it write
// the namespace declarations, on the document element, once each.
//
// The consequence is the behaviour that looks like magic: one item out of three carries a
// location, and xmlns:georss appears exactly once, at the top, on a document element the code
// above never touched. You did not say where the declaration goes, or that there should be one,
// or that there should not be three.
//
// It also means the declarations track reality. Remove the extension and the declaration goes
// with it; there is no separate list to keep in step, and no way to publish a document declaring
// a namespace it never uses or using one it never declared.
// ---------------------------------------------------------------------------------------------

string document = Save(feed, autoDetect: true);

Heading("The document element you never wrote");
Console.WriteLine($"  {RootElement(document)}");
Console.WriteLine();
Console.WriteLine($"  xmlns:georss declarations in the whole document  {Occurrences(document, "xmlns:georss")}");
Console.WriteLine($"  georss: elements written                        {Occurrences(document, "<georss:")}  (point and featurename, both on item 3)");
Console.WriteLine($"  xmlns:creativeCommons declarations              {Occurrences(document, "xmlns:creativeCommons")}");

// ---------------------------------------------------------------------------------------------
// 3. What happens without it
//
// Turn AutoDetectExtensions off and the derivation stops -- and so does the writing. The setting
// governs the whole extension pass, not merely the declarations, so what you get is neither a
// broken document nor an exception: you get a clean, well-formed, conforming feed with the
// extension content missing from it.
//
// That is the right failure mode of the three available, and it is still the dangerous one,
// because it looks exactly like success. This is the same silent drop sample 08 ended on, now in
// the writing direction: nothing warns you that the location you attached is not in the document
// you published.
//
// So the setting exists to let you take over the declaration list through SupportedExtensions,
// not to let you switch extensions off. If you are not populating SupportedExtensions, leave it
// alone.
// ---------------------------------------------------------------------------------------------

string undeclared = Save(feed, autoDetect: false);

Heading("The same feed with AutoDetectExtensions off");
Console.WriteLine($"  {RootElement(undeclared)}");
Console.WriteLine($"  xmlns:georss declarations  {Occurrences(undeclared, "xmlns:georss")}");
Console.WriteLine($"  georss: elements written   {Occurrences(undeclared, "<georss:")}");
Console.WriteLine($"  well-formed?               {IsWellFormed(undeclared)}");
Console.WriteLine($"  {undeclared.Length:N0} bytes against {document.Length:N0} with extensions -- the difference is what was dropped");

// ---------------------------------------------------------------------------------------------
// 4. Proving it, rather than reading it
//
// A document that looks right in a console is not evidence. The check that matters is whether
// what you wrote can be read back -- so load the saved document into a fresh feed and pull the
// extension off the item it should be on.
//
// This is the round trip from sample 08 applied to extensions, and it carries the same caveat: it
// proves the reader agrees with the writer, not that either matches the specification. For GeoRSS
// that is a low-stakes bet. Sample 12 is where it stops being one.
// ---------------------------------------------------------------------------------------------

RssFeed reloaded = new();
using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(document)))
{
    reloaded.Load(stream);
}

Heading("Read back from what was written");
for (int index = 0; index < reloaded.Channel.Items.Count; index++)
{
    RssItem item = reloaded.Channel.Items[index];
    string found = item.FindExtension(GeoRssSyndicationExtension.MatchByType) is GeoRssSyndicationExtension geo
        ? $"{geo.Context.FeatureName} at {geo.Context.Point?.Latitude}, {geo.Context.Point?.Longitude}"
        : "(no location, as written)";

    Console.WriteLine($"  item {index + 1}  {found}");
}

if (reloaded.Channel.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) is CreativeCommonsSyndicationExtension cc)
{
    Console.WriteLine($"  channel licence  {string.Join(", ", cc.Context.Licenses)}");
}

Console.WriteLine();
Console.WriteLine("Next: 11-podcast-feed.cs -- two namespaces on one channel, and an https that has to be https.");

static RssItem Post(string title, string slug) => new()
{
    Title = title,
    Link = new Uri($"https://endjin.com/blog/{slug}"),
    Description = $"A post about {title}.",
    PublicationDate = new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
};

static string Save(RssFeed feed, bool autoDetect)
{
    using MemoryStream stream = new();
    feed.Save(stream, new SyndicationResourceSaveSettings
    {
        AutoDetectExtensions = autoDetect,
        MinimizeOutputSize = true,
    });

    stream.Position = 0;
    using StreamReader reader = new(stream);

    return reader.ReadToEnd();
}

static string RootElement(string document)
{
    int start = document.IndexOf("<rss", StringComparison.Ordinal);
    int end = document.IndexOf('>', start);

    return document[start..(end + 1)];
}

static string IsWellFormed(string document)
{
    try
    {
        System.Xml.Linq.XDocument.Parse(document);
        return "yes -- which is exactly what makes this dangerous";
    }
    catch (System.Xml.XmlException error)
    {
        return $"no: {error.Message}";
    }
}

static int Occurrences(string document, string needle)
{
    int count = 0;
    for (int at = document.IndexOf(needle, StringComparison.Ordinal); at >= 0; at = document.IndexOf(needle, at + 1, StringComparison.Ordinal))
    {
        count++;
    }

    return count;
}

static string Describe(IEnumerable<string> prefixes)
{
    string joined = string.Join(", ", prefixes);

    return joined.Length == 0 ? "(none)" : joined;
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}