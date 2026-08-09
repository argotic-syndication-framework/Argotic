#:project ../Argotic.Core/Argotic.Core.csproj

// ---------------------------------------------------------------------------------------------
// 11 -- A podcast feed, which is RSS plus two other vocabularies
//
//     dotnet run --file Solutions/Samples/11-podcast-feed.cs
//
// Podcasting is the largest thing ever built on RSS, and it is built almost entirely outside it.
// Strip a podcast feed down to the RSS 2.0 vocabulary and you have a list of titles, links, dates
// and one attachment per item. Everything that makes it a podcast -- the artwork, the category
// Apple files it under, the episode and season numbers, the duration, whether it is explicit, the
// transcript, who to pay -- lives in namespaces RSS knows nothing about.
//
// Two of them matter. Apple's iTunes namespace dates from 2005 and is what every directory reads.
// Podcasting 2.0 is the open successor, published by the Podcast Index at
// https://podcastindex.org/namespace/1.0, and it carries what iTunes never modelled: transcripts,
// funding links, per-episode people, chapter markers, licences.
//
// This sample builds a feed with both, reads it back, and spends its last section on a single
// character that decides whether any of it works.
// ---------------------------------------------------------------------------------------------

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

// ---------------------------------------------------------------------------------------------
// 1. The channel: what a directory needs before it will list you
//
// The channel-level iTunes block is the submission form. Apple will not accept a feed without an
// image, a category from its fixed taxonomy, and an owner e-mail it can send a verification code
// to -- and those three are the ones people forget, because a feed missing them is still valid
// RSS and still parses everywhere.
//
// ExplicitMaterial is worth pausing on. It has four values, not two: None, Clean, No and Yes.
// None means the publisher said nothing at all, which is a different statement from No, and Apple
// treats the silence and the denial differently. Modelling it as a bool would collapse that
// distinction and there would be no way to get it back.
// ---------------------------------------------------------------------------------------------

RssFeed feed = new();
feed.Channel.Title = "The endjin Podcast";
feed.Channel.Link = new Uri("https://endjin.com/podcast/");
feed.Channel.Description = "Conversations about .NET, data, analytics and AI, from the endjin team.";
feed.Channel.Language = new System.Globalization.CultureInfo("en-GB");

ITunesSyndicationExtension show = new();
show.Context.Author = "endjin";
show.Context.Owner = new ITunesOwner("hello@endjin.com", "endjin");
show.Context.Image = new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/podcast/cover.png");
show.Context.Summary = "Conversations about .NET, data, analytics and AI, from the endjin team.";
show.Context.PodcastType = ITunesPodcastType.Episodic;
show.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
show.Context.Categories.Add(new ITunesCategory("Technology"));
feed.Channel.Extensions.Add(show);

PodcastSyndicationExtension showExtras = new();
showExtras.Context.Medium = PodcastMedium.Podcast;
showExtras.Context.Identifier = "5b5b9b7c-8e59-5c0f-a1c3-3d5f7a9b1d02";
showExtras.Context.FundingLinks.Add(new PodcastFunding(
    new Uri("https://endjin.com/contact-us/"),
    "Work with endjin"));
feed.Channel.Extensions.Add(showExtras);

// ---------------------------------------------------------------------------------------------
// 2. The episode, and the element that actually makes it one
//
// All the metadata below describes something a client cannot play. What makes an item an episode
// is the enclosure -- the RSS 2.0 element, unchanged since 2002 -- and an item without one is a
// blog post that happens to carry iTunes tags.
//
// RssItem.Enclosures is a list because the XML permits repetition, and that is the only reason.
// Podcast clients take the first and ignore the rest, so publish exactly one; a second is not an
// alternative bitrate, it is an element most of your audience will never hear.
//
// Note the two duration values in play. iTunes has itunes:duration, a TimeSpan whose absent value
// is TimeSpan.MinValue rather than null -- the same sentinel pattern as sample 01. The enclosure
// has a byte length, which is a different fact and is not derivable from the duration.
// ---------------------------------------------------------------------------------------------

RssItem episode = new()
{
    Title = "The GenAI Reality Check: New Instrument, Same Orchestra",
    Link = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
    Description = "Barry Smart on why generative AI is a new instrument in an orchestra that already existed.",
    PublicationDate = new DateTime(2026, 5, 14, 8, 54, 29, DateTimeKind.Utc),
    Guid = new RssGuid("urn:uuid:60a76c80-d399-11d9-b93c-0003939e0af6", isPermanentUrl: false),
};

// The element without which none of the rest matters.
episode.Enclosures.Add(new RssEnclosure(
    36_588_921L,
    "audio/mpeg",
    new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check.mp3")));

ITunesSyndicationExtension episodeItunes = new();
episodeItunes.Context.Title = "The GenAI Reality Check";
episodeItunes.Context.Season = 1;
episodeItunes.Context.Episode = 3;
episodeItunes.Context.EpisodeType = ITunesEpisodeType.Full;
episodeItunes.Context.Duration = TimeSpan.FromMinutes(26.9);
episodeItunes.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
episode.Extensions.Add(episodeItunes);

// Podcasting 2.0 is where the things iTunes never modelled go. A transcript is the clearest
// example: it is the difference between an episode being searchable and accessible and not, and
// there was simply nowhere to put one until this namespace existed.
PodcastSyndicationExtension episodeExtras = new();
episodeExtras.Context.Season = 1;
episodeExtras.Context.Episode = 3m;
episodeExtras.Context.Transcripts.Add(new PodcastTranscript(
    new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-transcript.vtt"),
    "text/vtt")
{
    Language = "en",
    Relationship = "captions",
});
episodeExtras.Context.People.Add(new PodcastPerson("Barry Smart")
{
    Role = "guest",
    Url = new Uri("https://endjin.com/who-we-are/our-people/barry-smart/"),
});
episodeExtras.Context.Chapters = new PodcastChapters(
    new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-chapters.json"),
    "application/json+chapters");
episode.Extensions.Add(episodeExtras);

feed.Channel.Items.Add(episode);

string document = Save(feed);

Heading("What was written");
Console.WriteLine($"  {document.Length:N0} bytes");
Console.WriteLine($"  namespaces declared   {string.Join(", ", DeclaredPrefixes(document))}");
Console.WriteLine($"  itunes: elements      {Count(document, "<itunes:")}");
Console.WriteLine($"  podcast: elements     {Count(document, "<podcast:")}");
Console.WriteLine($"  enclosures            {Count(document, "<enclosure")}   <- without this it is not an episode");

// ---------------------------------------------------------------------------------------------
// 3. Reading it back
//
// Nothing new here after samples 09 and 10 -- FindExtension with the static MatchByType, bound
// once and read many times. It is worth running anyway, because a podcast feed is where the cost
// of getting the read wrong is highest: a client that misses itunes:duration shows a progress bar
// of unknown length, and one that misses the transcript makes an episode unsearchable.
// ---------------------------------------------------------------------------------------------

RssFeed reloaded = Load(document);
RssItem first = reloaded.Channel.Items[0];

Heading("Read back");
if (reloaded.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension showBack)
{
    Console.WriteLine($"  show category    {string.Join(", ", showBack.Context.Categories.Select(category => category.Text))}");
    Console.WriteLine($"  show owner       {showBack.Context.Owner?.Name} <{showBack.Context.Owner?.EmailAddress}>");
    Console.WriteLine($"  show explicit    {showBack.Context.ExplicitMaterial}   (None would mean 'unstated', which is not the same)");
}

if (first.FindExtension(ITunesSyndicationExtension.MatchByType) is ITunesSyndicationExtension episodeBack)
{
    string duration = episodeBack.Context.Duration == TimeSpan.MinValue
        ? "(absent -- TimeSpan.MinValue, not TimeSpan.Zero)"
        : episodeBack.Context.Duration.ToString("g", System.Globalization.CultureInfo.InvariantCulture);

    Console.WriteLine($"  episode          S{episodeBack.Context.Season}E{episodeBack.Context.Episode}, {episodeBack.Context.EpisodeType}");
    Console.WriteLine($"  duration         {duration}");
}

if (first.FindExtension(PodcastSyndicationExtension.MatchByType) is PodcastSyndicationExtension extrasBack)
{
    foreach (PodcastTranscript transcript in extrasBack.Context.Transcripts)
    {
        Console.WriteLine($"  transcript       {transcript.MediaType} ({transcript.Language}, {transcript.Relationship})");
    }

    foreach (PodcastPerson person in extrasBack.Context.People)
    {
        Console.WriteLine($"  person           {person.Name} -- {person.Role}");
    }

    Console.WriteLine($"  chapters         {extrasBack.Context.Chapters?.MediaType ?? "(none)"}");
}

Console.WriteLine($"  enclosure        {first.Enclosures[0].ContentType}, {first.Enclosures[0].Length:N0} bytes");

// ---------------------------------------------------------------------------------------------
// 4. Two signals, and a feed is lost only when both miss
//
// The Podcasting 2.0 namespace is https://podcastindex.org/namespace/1.0 -- with the s. Plenty of
// feeds in the wild declare the http form, because the namespace was published under http first
// and a namespace URI is an identifier rather than an address: nothing dereferences it, nothing
// redirects it, and the two strings are simply two different namespaces.
//
// A strict reader would drop every one of those feeds. This one does not, and the reason is worth
// knowing because it is the difference between "my podcast feed reads nothing" being a five-minute
// problem and a five-hour one.
//
// Attachment asks two questions, in this order (SyndicationExtension.ExistsInSource):
//
//   1. Is this extension's conventional prefix -- "podcast" -- bound to anything at all? It does
//      not check what to. That test is one lookup, and it answers yes for nearly every real feed.
//   2. If not: is this extension's namespace URI bound, under any prefix whatsoever?
//
// Either one is enough. And when the first one carries it, the reader then follows the document's
// binding rather than the specification's, deliberately, so the elements are actually found.
//
// So there are four combinations and only one of them loses data -- the feed that gets both the
// prefix and the namespace wrong. That one is silent: no exception, no warning, no log line, just
// an item with nothing attached that looks exactly like an item that carried nothing.
// ---------------------------------------------------------------------------------------------

Heading("Prefix or namespace: the four combinations");
Console.WriteLine($"  the constant  {PodcastSyndicationExtension.NamespaceUri}");
Console.WriteLine();
Console.WriteLine($"  {"prefix",-10} {"namespace",-12} {"attached",-10} season");

foreach ((string prefix, string scheme) in new[] { ("podcast", "https"), ("podcast", "http"), ("pi", "https"), ("pi", "http") })
{
    string xml = $"""
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0" xmlns:{prefix}="{scheme}://podcastindex.org/namespace/1.0">
          <channel>
            <title>The endjin Podcast</title>
            <link>https://endjin.com/podcast/</link>
            <description>Conversations from the endjin team.</description>
            <item>
              <title>The GenAI Reality Check</title>
              <link>https://endjin.com/podcast/1</link>
              <description>An episode.</description>
              <{prefix}:season>1</{prefix}:season>
              <{prefix}:episode>3</{prefix}:episode>
            </item>
          </channel>
        </rss>
        """;

    RssItem item = Load(xml).Channel.Items[0];
    PodcastSyndicationExtension? attached = item.FindExtension(PodcastSyndicationExtension.MatchByType) as PodcastSyndicationExtension;
    string season = attached?.Context.Season?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "nothing";

    Console.WriteLine($"  {prefix,-10} {scheme,-12} {item.Extensions.Count,-10} {season}");
}

Console.WriteLine();
Console.WriteLine("  Three of the four read the episode. The fourth -- wrong prefix and wrong namespace --");
Console.WriteLine("  reads nothing and says nothing, which is the case worth being able to recognise.");

Console.WriteLine();
Console.WriteLine("Next: 12-sitemap-extensions.cs -- where element order is the difference between valid and rejected.");

static string Save(RssFeed feed)
{
    using MemoryStream stream = new();
    feed.Save(stream, new SyndicationResourceSaveSettings { MinimizeOutputSize = true });
    stream.Position = 0;

    using StreamReader reader = new(stream);

    return reader.ReadToEnd();
}

static RssFeed Load(string xml)
{
    RssFeed feed = new();
    feed.Load(SyndicationEncodingUtility.CreateSafeNavigator(xml));

    return feed;
}

static IEnumerable<string> DeclaredPrefixes(string document)
{
    int end = document.IndexOf('>', document.IndexOf("<rss", StringComparison.Ordinal));

    return document[..end]
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Where(token => token.StartsWith("xmlns:", StringComparison.Ordinal))
        .Select(token => token["xmlns:".Length..token.IndexOf('=', StringComparison.Ordinal)]);
}

static int Count(string document, string needle)
{
    int count = 0;
    for (int at = document.IndexOf(needle, StringComparison.Ordinal); at >= 0; at = document.IndexOf(needle, at + 1, StringComparison.Ordinal))
    {
        count++;
    }

    return count;
}

static void Heading(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}