using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Loads and re-saves a feed written the way Podcasting 2.0 publishers write one.
/// </summary>
/// <remarks>
///     <para>
///     The namespace is declared by <b>1,200 of 1,934</b> live feeds surveyed from the Apple directory —
///     <b>62%</b>. Until this extension existed, every element of it was dropped by a load followed by a
///     save: not misread, not partially read, but silently discarded, because there was nowhere to put
///     it. That is the §2.47 failure mode at the scale of a whole namespace.
///     </para>
///     <para>
///     The single most consequential loss was the Apple Podcasts ownership token. §2.49 found it
///     delivered as <c>&lt;podcast:txt purpose="applepodcastsverify"&gt;</c> by 21 feeds and as
///     <c>itunes:applepodcastsverify</c> by 26 — so a library reading only the iTunes form served
///     roughly half the publishers who set one, and the other half lost their claim.
///     </para>
/// </remarks>
[TestClass]
public sealed class RoundTripAPodcastingTwoPointZeroFeed
{
    private const string Document = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0"
             xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd"
             xmlns:podcast="https://podcastindex.org/namespace/1.0">
          <channel>
            <title>A Podcast</title>
            <link>https://example.com/</link>
            <description>A description.</description>
            <podcast:locked owner="owner@example.com">yes</podcast:locked>
            <podcast:guid>917393e3-1b1e-5cef-ace4-edaa54e1f810</podcast:guid>
            <podcast:medium>music</podcast:medium>
            <podcast:podping usesPodping="true"/>
            <podcast:txt purpose="applepodcastsverify">e6cfaae0-9496-11f0-a272-f9e230f88be0</podcast:txt>
            <podcast:funding url="https://example.com/donate">Support the show!</podcast:funding>
            <podcast:person role="host" href="https://example.com/alice">Alice Example</podcast:person>
            <itunes:author>Alice Example</itunes:author>
            <item>
              <title>An Episode</title>
              <podcast:season name="The First Season">1</podcast:season>
              <podcast:episode display="Ep. 3.5 — the interlude">3.5</podcast:episode>
              <podcast:transcript url="https://example.com/ep1.vtt" type="text/vtt" language="en" rel="captions" />
              <podcast:chapters url="https://example.com/ep1.json" type="application/json+chapters" />
              <podcast:license url="https://example.com/license">cc-by-4.0</podcast:license>
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

    private static RssFeed RoundTrip(RssFeed feed)
    {
        using MemoryStream saved = new();
        feed.Save(saved);
        saved.Seek(0, SeekOrigin.Begin);

        RssFeed reloaded = new();
        reloaded.Load(saved);
        return reloaded;
    }

    private static PodcastSyndicationExtensionContext Channel(RssFeed feed) =>
        feed.Channel.Extensions.OfType<PodcastSyndicationExtension>().Single().Context;

    private static PodcastSyndicationExtensionContext Item(RssFeed feed) =>
        feed.Channel.Items.Single().Extensions.OfType<PodcastSyndicationExtension>().Single().Context;

    /// <summary>
    /// Every channel element survives being loaded and written back.
    /// </summary>
    [TestMethod]
    public void EveryChannelElement_SurvivesBeingLoadedAndWrittenBack()
    {
        PodcastSyndicationExtensionContext channel = Channel(RoundTrip(Load(Document)));

        channel.IsLocked.ShouldBe(true);
        channel.LockOwner.ShouldBe("owner@example.com");
        channel.Identifier.ShouldBe("917393e3-1b1e-5cef-ace4-edaa54e1f810");
        channel.Medium.ShouldBe(PodcastMedium.Music);
        channel.MediumIsList.ShouldBeFalse();
        channel.UsesPodping.ShouldBeTrue();

        channel.FundingLinks.ShouldHaveSingleItem();
        channel.FundingLinks[0].Url.ShouldBe(new Uri("https://example.com/donate"));
        channel.FundingLinks[0].Message.ShouldBe("Support the show!");

        channel.People.ShouldHaveSingleItem();
        channel.People[0].Name.ShouldBe("Alice Example");
        channel.People[0].Role.ShouldBe("host");
        channel.People[0].Url.ShouldBe(new Uri("https://example.com/alice"));
    }

    /// <summary>
    /// The Apple Podcasts ownership token survives being loaded and written back.
    /// </summary>
    /// <remarks>
    ///     <b>The assertion this whole extension was built for.</b> Apple compares the token it issued
    ///     against the one it finds in the feed; a feed that has been round-tripped through a library
    ///     that dropped it fails the claim, with no error anywhere to explain why.
    /// </remarks>
    [TestMethod]
    public void TheApplePodcastsOwnershipToken_SurvivesBeingLoadedAndWrittenBack()
    {
        PodcastSyndicationExtensionContext channel = Channel(RoundTrip(Load(Document)));

        PodcastText token = channel.TextEntries.Single(t => t.IsApplePodcastsVerification);

        token.Purpose.ShouldBe("applepodcastsverify");
        token.Value.ShouldBe("e6cfaae0-9496-11f0-a272-f9e230f88be0", "a lost token is a failed ownership claim");
    }

    /// <summary>
    /// Every item element survives being loaded and written back.
    /// </summary>
    /// <remarks>
    ///     The fractional episode number is the row worth watching: <c>3.5</c> going in and coming back
    ///     out as <c>3.5</c> is what proves the value is not being rounded into a collision with an
    ///     episode that already exists.
    /// </remarks>
    [TestMethod]
    public void EveryItemElement_SurvivesBeingLoadedAndWrittenBack()
    {
        PodcastSyndicationExtensionContext item = Item(RoundTrip(Load(Document)));

        item.Season.ShouldBe(1);
        item.SeasonName.ShouldBe("The First Season");
        item.Episode.ShouldBe(3.5m, "a decimal episode number is not an integer");
        item.EpisodeDisplay.ShouldBe("Ep. 3.5 — the interlude");

        item.Transcripts.ShouldHaveSingleItem();
        item.Transcripts[0].Url.ShouldBe(new Uri("https://example.com/ep1.vtt"));
        item.Transcripts[0].MediaType.ShouldBe("text/vtt");
        item.Transcripts[0].Language.ShouldBe("en");
        item.Transcripts[0].IsCaptions.ShouldBeTrue();

        item.Chapters.ShouldNotBeNull();
        item.Chapters.MediaType.ShouldBe("application/json+chapters");

        item.License.ShouldNotBeNull();
        item.License.Identifier.ShouldBe("cc-by-4.0");
    }

    /// <summary>
    /// The iTunes extension still loads from the same document.
    /// </summary>
    /// <remarks>
    ///     The control. A feed carrying two extension namespaces must produce two extensions, and this
    ///     is what would fail if the new family had somehow displaced the existing one during discovery.
    /// </remarks>
    [TestMethod]
    public void TheITunesExtension_StillLoadsFromTheSameDocument()
    {
        RssFeed reloaded = RoundTrip(Load(Document));

        reloaded.Channel.Extensions.OfType<ITunesSyndicationExtension>().Single()
            .Context.Author.ShouldBe("Alice Example");
        reloaded.Channel.Extensions.OfType<PodcastSyndicationExtension>().ShouldHaveSingleItem();
    }

    /// <summary>
    /// A feed writing a boolean lock value comes back out in the specification's spelling.
    /// </summary>
    /// <remarks>
    ///     Reading accepts <c>true</c> because real feeds use it; writing emits <c>yes</c>, so a feed
    ///     round-tripped through this library is more conformant leaving than it was arriving.
    /// </remarks>
    [TestMethod]
    public void AFeedWritingABooleanLockValue_ComesBackInTheSpecificationsSpelling()
    {
        RssFeed feed = Load(Document.Replace(
            """<podcast:locked owner="owner@example.com">yes</podcast:locked>""",
            "<podcast:locked>true</podcast:locked>",
            StringComparison.Ordinal));

        using MemoryStream saved = new();
        feed.Save(saved);
        string xml = Encoding.UTF8.GetString(saved.ToArray());

        xml.Contains(">yes<", StringComparison.Ordinal).ShouldBeTrue("the documented spelling is written");
        Channel(Load(xml)).IsLocked.ShouldBe(true);
    }
}