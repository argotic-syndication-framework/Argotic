namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Loads and re-saves a podcast feed written the way Apple's current specification says to write one.
/// </summary>
/// <remarks>
///     <para>
///     Apple revised the podcasting specification in 2017 and added <c>itunes:episode</c>,
///     <c>itunes:season</c>, <c>itunes:episodeType</c>, <c>itunes:title</c> and <c>itunes:type</c>.
///     Four of the five are among the most common extension elements in existence, and none of them was
///     modelled here.
///     </para>
///     <para>
///     <b>The consequence was silent, and it only shows on a round-trip.</b> Nothing threw and nothing
///     looked wrong on load — the elements simply had nowhere to go, so a load followed by a save
///     dropped them. Counted across the 136-document corpus: <c>episodeType</c> 4,695, <c>title</c>
///     4,544, <c>episode</c> 1,003, <c>type</c> 6 — <b>10,248 element occurrences</b> that would not
///     survive being read and written back.
///     </para>
///     <para>
///     <c>itunes:new-feed-url</c> is deliberately in the fixture and was <em>already</em> supported. It
///     is here as a control: an element census that reported it missing would be wrong, and this test
///     would say so. <c>itunes:season</c> is also present though the corpus contains none of it, because
///     Apple defines it in the same breath as <c>episode</c> and an API with one and not the other
///     would be lopsided.
///     </para>
///     <para>
///     <c>itunes:complete</c> joined the fixture when Apple's current specification was reviewed
///     element by element and it turned out to be the only one still unmodelled. Like <c>season</c> it
///     has no corpus occurrences, because a podcast emits it once — when it ends.
///     <see cref="Functionality.Core.iTunes.ITunesCompleteTests"/> covers its values; here it only has
///     to survive the journey.
///     </para>
/// </remarks>
[TestClass]
public sealed class RoundTripAPodcastAsApplePublishesIt
{
    private const string Podcast = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
          <channel>
            <title>A Podcast</title>
            <link>https://example.com/</link>
            <description>A description.</description>
            <itunes:type>serial</itunes:type>
            <itunes:complete>yes</itunes:complete>
            <itunes:new-feed-url>https://example.com/moved.xml</itunes:new-feed-url>
            <item>
              <title>Episode 42: The Long Headline A Publisher Writes In The Ordinary Title</title>
              <itunes:title>The Clean Episode Name</itunes:title>
              <itunes:episode>42</itunes:episode>
              <itunes:season>3</itunes:season>
              <itunes:episodeType>bonus</itunes:episodeType>
              <itunes:duration>01:23:45</itunes:duration>
              <itunes:explicit>false</itunes:explicit>
            </item>
          </channel>
        </rss>
        """;

    private static RssFeed Load(string document)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    private static string Save(RssFeed feed)
    {
        using MemoryStream stream = new();
        using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
        {
            feed.Save(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static ITunesSyndicationExtensionContext ItemContext(RssFeed feed) =>
        feed.Channel.Items.Single().Extensions.OfType<ITunesSyndicationExtension>().Single().Context;

    /// <summary>
    /// The episode metadata Apple's current specification defines is read.
    /// </summary>
    [TestMethod]
    public void TheEpisodeMetadataApplesSpecificationDefines_IsRead()
    {
        ITunesSyndicationExtensionContext context = ItemContext(Load(Podcast));

        context.Title.ShouldBe("The Clean Episode Name", "itunes:title is not the item's title");
        context.Episode.ShouldBe(42);
        context.Season.ShouldBe(3);
        context.EpisodeType.ShouldBe(ITunesEpisodeType.Bonus);
        context.Duration.ShouldBe(new TimeSpan(1, 23, 45), "the control — this already worked");
    }

    /// <summary>
    /// The channel-level podcast type is read.
    /// </summary>
    /// <remarks>
    ///     <c>itunes:type</c> sits on the channel rather than the item, so it is filled by a different
    ///     call than everything above and could plausibly have been missed on its own.
    /// </remarks>
    [TestMethod]
    public void TheChannelLevelPodcastType_IsRead()
    {
        ITunesSyndicationExtensionContext channel = Load(Podcast).Channel.Extensions
            .OfType<ITunesSyndicationExtension>().Single().Context;

        channel.PodcastType.ShouldBe(ITunesPodcastType.Serial);
        channel.IsComplete.ShouldBeTrue("itunes:complete says this podcast has finished");
        channel.NewFeedUrl.ShouldBe(new Uri("https://example.com/moved.xml"), "already supported — the control");
    }

    /// <summary>
    /// Every element survives being loaded and written back.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>This is the test that would have caught the gap.</b> Reading is not the property that was
    ///     broken — having somewhere to put what was read is. Asserting the reloaded object rather than
    ///     the emitted text keeps the test about the outcome, so a change to how the elements are
    ///     written still passes provided they come back.
    ///     </para>
    ///     <para>
    ///     <c>itunes:explicit</c> normalises from <c>false</c> to <c>no</c> on the way out, because the
    ///     enumeration writes its own <c>AlternateValue</c> and both spellings name the same state.
    ///     That is asserted through the reloaded value rather than the text for exactly that reason.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void EveryElement_SurvivesBeingLoadedAndWrittenBack()
    {
        RssFeed reloaded = Load(Save(Load(Podcast)));

        ITunesSyndicationExtensionContext item = ItemContext(reloaded);

        item.Title.ShouldBe("The Clean Episode Name");
        item.Episode.ShouldBe(42);
        item.Season.ShouldBe(3);
        item.EpisodeType.ShouldBe(ITunesEpisodeType.Bonus);
        item.Duration.ShouldBe(new TimeSpan(1, 23, 45));
        item.ExplicitMaterial.ShouldBe(ITunesExplicitMaterial.No, "false and no name the same state");

        ITunesSyndicationExtensionContext channel = reloaded.Channel.Extensions
            .OfType<ITunesSyndicationExtension>().Single().Context;

        channel.PodcastType.ShouldBe(ITunesPodcastType.Serial);
        channel.IsComplete.ShouldBeTrue();
        channel.NewFeedUrl.ShouldBe(new Uri("https://example.com/moved.xml"));
    }

    /// <summary>
    /// An episode number that is not a positive integer is refused rather than stored.
    /// </summary>
    /// <remarks>
    ///     Apple defines both <c>episode</c> and <c>season</c> as positive integers, and the properties
    ///     enforce that. The loader has to agree with them: §2.45 is this repository's record of what
    ///     happens when a loader can produce a value the property's own setter would reject.
    /// </remarks>
    [TestMethod]
    [DataRow("0", "zero is not a valid episode number")]
    [DataRow("-1", "nor is a negative one")]
    [DataRow("not a number", "nor is a word")]
    public void AnEpisodeNumberThatIsNotAPositiveInteger_IsRefused(string episode, string why)
    {
        string document = Podcast.Replace("<itunes:episode>42</itunes:episode>", $"<itunes:episode>{episode}</itunes:episode>", StringComparison.Ordinal);

        ITunesSyndicationExtensionContext context = ItemContext(Load(document));

        context.Episode.ShouldBeNull(why);
        context.EpisodeType.ShouldBe(ITunesEpisodeType.Bonus, "the elements beside it still load");
    }

    /// <summary>
    /// Every episode type and podcast type Apple defines is recognised.
    /// </summary>
    [TestMethod]
    [DataRow("full", ITunesEpisodeType.Full, "4,657 in the corpus")]
    [DataRow("bonus", ITunesEpisodeType.Bonus, "36")]
    [DataRow("trailer", ITunesEpisodeType.Trailer, "2")]
    [DataRow("Full", ITunesEpisodeType.Full, "case is disregarded")]
    [DataRow("nonsense", ITunesEpisodeType.None, "an unknown value names no type")]
    public void EveryEpisodeTypeAppleDefines_IsRecognised(string spelling, ITunesEpisodeType expected, string provenance)
    {
        string document = Podcast.Replace("<itunes:episodeType>bonus</itunes:episodeType>", $"<itunes:episodeType>{spelling}</itunes:episodeType>", StringComparison.Ordinal);

        ItemContext(Load(document)).EpisodeType.ShouldBe(expected, provenance);
    }

    /// <summary>
    /// The podcast type is recognised in both spellings, including the one the corpus miscapitalises.
    /// </summary>
    /// <remarks>
    ///     Of the six <c>itunes:type</c> values in the corpus, five read <c>episodic</c> and one reads
    ///     <c>Episodic</c>. A case-sensitive match would drop that one, so the capitalised row is taken
    ///     from real data rather than invented.
    /// </remarks>
    [TestMethod]
    [DataRow("episodic", ITunesPodcastType.Episodic)]
    [DataRow("Episodic", ITunesPodcastType.Episodic)]
    [DataRow("serial", ITunesPodcastType.Serial)]
    public void ThePodcastType_IsRecognisedWhateverItsCase(string spelling, ITunesPodcastType expected)
    {
        string document = Podcast.Replace("<itunes:type>serial</itunes:type>", $"<itunes:type>{spelling}</itunes:type>", StringComparison.Ordinal);

        RssFeed feed = Load(document);

        feed.Channel.Extensions.OfType<ITunesSyndicationExtension>().Single().Context
            .PodcastType.ShouldBe(expected, spelling);
    }
}