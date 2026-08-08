namespace Argotic.Extensions.Tests.Functionality.Core.Podcast;

/// <summary>
/// Covers the Podcasting 2.0 namespace elements this library reads.
/// </summary>
/// <remarks>
///     <para>
///     The namespace is maintained by Podcast Index rather than by Apple, and it is the most widely
///     adopted extension this library has taken on in years: <b>1,200 of 1,934</b> live feeds surveyed
///     from the Apple directory — <b>62%</b> — declare it.
///     </para>
///     <para>
///     Every element shape in this file is modelled on what those feeds actually contain, and the
///     percentages quoted per element are measured from that same survey.
///     </para>
/// </remarks>
[TestClass]
public sealed class PodcastSyndicationExtensionTests
{
    private const string Namespace = "https://podcastindex.org/namespace/1.0";

    private static string Feed(string channelElements, string itemElements = "") => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:podcast="{Namespace}">
          <channel>
            <title>A Podcast</title>
            <link>https://example.com/</link>
            <description>A description.</description>
            {channelElements}
            <item><title>An Episode</title>{itemElements}</item>
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

    private static PodcastSyndicationExtensionContext? ChannelContext(string channelElements) =>
        Load(Feed(channelElements)).Channel.Extensions
            .OfType<PodcastSyndicationExtension>().SingleOrDefault()?.Context;

    private static PodcastSyndicationExtensionContext? ItemContext(string itemElements) =>
        Load(Feed(string.Empty, itemElements)).Channel.Items.Single().Extensions
            .OfType<PodcastSyndicationExtension>().SingleOrDefault()?.Context;

    /// <summary>
    /// The extension is discovered and attached when a feed declares the namespace.
    /// </summary>
    /// <remarks>
    ///     Extensions are found by reflection over the assembly's exported types, so this asserts the new
    ///     family joined that scan rather than that any particular element parsed.
    /// </remarks>
    [TestMethod]
    public void AFeedDeclaringTheNamespace_GetsTheExtensionAttached()
    {
        RssFeed feed = Load(Feed("<podcast:guid>917393e3-1b1e-5cef-ace4-edaa54e1f810</podcast:guid>"));

        feed.Channel.Extensions.OfType<PodcastSyndicationExtension>().ShouldHaveSingleItem();
    }

    /// <summary>
    /// A locked feed is read as locked, in either spelling real feeds use.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>podcast:locked</c> is the namespace's most deployed element, in <b>16.6%</b> of surveyed
    ///     feeds. The specification defines <c>yes</c> and <c>no</c>; the survey found 126 <c>yes</c>,
    ///     191 <c>no</c>, and then 3 <c>true</c>, 1 <c>false</c> and at least one capitalised spelling.
    ///     </para>
    ///     <para>
    ///     Reading only the documented pair would mis-read those feeds as unlocked — permitting an
    ///     import the publisher forbade. This is the same defect §2.46 records for <c>itunes:explicit</c>,
    ///     found the same way, and the rows below are taken from the counts rather than invented.
    ///     </para>
    /// </remarks>
    /// <param name="spelling">The node value to write into <c>podcast:locked</c>; any casing of
    /// <c>yes</c>, <c>no</c>, <c>true</c> or <c>false</c>.</param>
    /// <param name="expected">Whether that spelling means the feed is locked.</param>
    /// <param name="provenance">The value's count in the survey, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("yes", true, "126 in the survey")]
    [DataRow("no", false, "191 — the most common value")]
    [DataRow("true", true, "3 — undocumented but real")]
    [DataRow("false", false, "1")]
    [DataRow("Yes", true, "case is disregarded")]
    [DataRow("True", true, "the survey found a capitalised boolean spelling")]
    public void ALockedFeed_IsReadInEitherSpelling(string spelling, bool expected, string provenance)
    {
        PodcastSyndicationExtensionContext? context = ChannelContext($"<podcast:locked>{spelling}</podcast:locked>");

        context.ShouldNotBeNull(provenance);
        context.IsLocked.ShouldBe(expected, provenance);
    }

    /// <summary>
    /// A feed that says nothing about being locked reports no opinion, rather than reporting no.
    /// </summary>
    /// <remarks>
    ///     <b>The distinction the nullable exists for.</b> "The publisher forbids importing", "the
    ///     publisher permits importing" and "the publisher did not say" are three states, and collapsing
    ///     the third into the second would both invent a statement and make an absent element appear on
    ///     the next save.
    /// </remarks>
    [TestMethod]
    public void AFeedThatSaysNothingAboutBeingLocked_ReportsNoOpinion()
    {
        PodcastSyndicationExtensionContext? context = ChannelContext("<podcast:guid>917393e3-1b1e-5cef-ace4-edaa54e1f810</podcast:guid>");

        context.ShouldNotBeNull();
        context.IsLocked.ShouldBeNull("absence is not a 'no'");
    }

    /// <summary>
    /// The lock owner address is read.
    /// </summary>
    [TestMethod]
    public void TheLockOwnerAddress_IsRead()
    {
        PodcastSyndicationExtensionContext? context =
            ChannelContext("""<podcast:locked owner="email@example.com">no</podcast:locked>""");

        context.ShouldNotBeNull();
        context.IsLocked.ShouldBe(false);
        context.LockOwner.ShouldBe("email@example.com");
    }

    /// <summary>
    /// The podcast's global identifier is read.
    /// </summary>
    /// <remarks>
    ///     Both values are the specification's own worked examples. <c>podcast:guid</c> is in
    ///     <b>12.9%</b> of surveyed feeds and is how a podcast keeps one identity when its address
    ///     changes.
    /// </remarks>
    /// <param name="identifier">The node value to write into <c>podcast:guid</c>; a UUID, and the
    /// value expected back.</param>
    [TestMethod]
    [DataRow("917393e3-1b1e-5cef-ace4-edaa54e1f810")]
    [DataRow("9b024349-ccf0-5f69-a609-6b82873eab3c")]
    public void ThePodcastsGlobalIdentifier_IsRead(string identifier)
    {
        PodcastSyndicationExtensionContext? context = ChannelContext($"<podcast:guid>{identifier}</podcast:guid>");

        context.ShouldNotBeNull();
        context.Identifier.ShouldBe(identifier);
    }

    /// <summary>
    /// Every medium the specification defines is read, in both its plain and list spellings.
    /// </summary>
    /// <remarks>
    ///     The specification defines ten mediums and says each has a "list" counterpart spelled with an
    ///     <c>L</c> suffix. That is modelled as the enumeration plus a flag rather than as twenty
    ///     members, so these rows are what proves the suffix is actually split off and not just matched
    ///     as an unknown value.
    /// </remarks>
    /// <param name="spelling">The node value to write into <c>podcast:medium</c>; one of the ten
    /// defined mediums, optionally suffixed <c>L</c>.</param>
    /// <param name="expected">The member of <c>PodcastMedium</c> that spelling names.</param>
    /// <param name="isList">Whether the spelling carried the <c>L</c> suffix that marks a list.</param>
    [TestMethod]
    [DataRow("podcast", PodcastMedium.Podcast, false)]
    [DataRow("music", PodcastMedium.Music, false)]
    [DataRow("video", PodcastMedium.Video, false)]
    [DataRow("film", PodcastMedium.Film, false)]
    [DataRow("audiobook", PodcastMedium.Audiobook, false)]
    [DataRow("newsletter", PodcastMedium.Newsletter, false)]
    [DataRow("blog", PodcastMedium.Blog, false)]
    [DataRow("publisher", PodcastMedium.Publisher, false)]
    [DataRow("course", PodcastMedium.Course, false)]
    [DataRow("mixed", PodcastMedium.Mixed, false)]
    [DataRow("podcastL", PodcastMedium.Podcast, true)]
    [DataRow("musicL", PodcastMedium.Music, true)]
    [DataRow("audiobookL", PodcastMedium.Audiobook, true)]
    public void EveryMediumTheSpecificationDefines_IsRead(string spelling, PodcastMedium expected, bool isList)
    {
        PodcastSyndicationExtensionContext? context = ChannelContext($"<podcast:medium>{spelling}</podcast:medium>");

        context.ShouldNotBeNull(spelling);
        context.Medium.ShouldBe(expected, spelling);
        context.MediumIsList.ShouldBe(isList, spelling);
    }

    /// <summary>
    /// A medium that names nothing is not read as a list of nothing.
    /// </summary>
    /// <remarks>
    ///     <b>The trap the list suffix sets.</b> Stripping a trailing <c>L</c> and retrying means the
    ///     single character <c>L</c> strips to the empty string — which matches
    ///     <see cref="PodcastMedium.None"/>'s own alternate value. Without a length guard that would
    ///     report a feed as having a medium of "list of nothing".
    /// </remarks>
    /// <param name="spelling">The node value to write into <c>podcast:medium</c>; anything that is
    /// not a defined medium.</param>
    /// <param name="why">Why the value names nothing, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("L", "strips to the empty string, which None itself carries")]
    [DataRow("nonsense", "an unknown medium")]
    [DataRow("nonsenseL", "an unknown list medium")]
    public void AMediumThatNamesNothing_IsNotRead(string spelling, string why)
    {
        PodcastSyndicationExtensionContext? context = ChannelContext($"<podcast:medium>{spelling}</podcast:medium>");

        (context?.Medium ?? PodcastMedium.None).ShouldBe(PodcastMedium.None, why);
        (context?.MediumIsList ?? false).ShouldBeFalse(why);
    }

    /// <summary>
    /// A feed that signals Podping is read as doing so.
    /// </summary>
    /// <remarks>
    ///     All 68 occurrences in the survey are written <c>usesPodping="true"</c>, two-thirds of them as
    ///     a self-closing element. The element carries no node value, so a bare <c>&lt;podcast:podping/&gt;</c>
    ///     asserts nothing and must not turn the flag on.
    /// </remarks>
    [TestMethod]
    public void AFeedThatSignalsPodping_IsReadAsDoingSo()
    {
        ChannelContext("""<podcast:podping usesPodping="true"/>""")!.UsesPodping.ShouldBeTrue();
        ChannelContext("""<podcast:podping usesPodping="true" />""")!.UsesPodping.ShouldBeTrue();

        PodcastSyndicationExtensionContext? bare = ChannelContext("<podcast:podping/>");
        (bare?.UsesPodping ?? false).ShouldBeFalse("the element without its attribute asserts nothing");
    }

    /// <summary>
    /// Every transcript on an episode is read, with its attributes.
    /// </summary>
    /// <remarks>
    ///     <c>podcast:transcript</c> is in <b>13.4%</b> of surveyed feeds and repeats, one per format.
    ///     These four are the specification's own examples.
    /// </remarks>
    [TestMethod]
    public void EveryTranscriptOnAnEpisode_IsRead()
    {
        PodcastSyndicationExtensionContext? context = ItemContext("""
            <podcast:transcript url="https://example.com/episode1/transcript.html" type="text/html" />
            <podcast:transcript url="https://example.com/episode1/transcript.vtt" type="text/vtt" />
            <podcast:transcript url="https://example.com/episode1/transcript.json" type="application/json" language="es" rel="captions" />
            """);

        context.ShouldNotBeNull();
        context.Transcripts.Count.ShouldBe(3);

        context.Transcripts[0].Url.ShouldBe(new Uri("https://example.com/episode1/transcript.html"));
        context.Transcripts[0].MediaType.ShouldBe("text/html");
        context.Transcripts[0].IsCaptions.ShouldBeFalse();

        context.Transcripts[2].Language.ShouldBe("es");
        context.Transcripts[2].Relationship.ShouldBe("captions");
        context.Transcripts[2].IsCaptions.ShouldBeTrue("rel=captions marks it whatever the media type");
    }

    /// <summary>
    /// Every funding link is read, with the wording the publisher wants shown.
    /// </summary>
    [TestMethod]
    public void EveryFundingLink_IsRead()
    {
        PodcastSyndicationExtensionContext? context = ChannelContext("""
            <podcast:funding url="https://www.example.com/donations">Support the show!</podcast:funding>
            <podcast:funding url="https://www.example.com/members">Become a member!</podcast:funding>
            """);

        context.ShouldNotBeNull();
        context.FundingLinks.Count.ShouldBe(2);
        context.FundingLinks[0].Url.ShouldBe(new Uri("https://www.example.com/donations"));
        context.FundingLinks[0].Message.ShouldBe("Support the show!");
        context.FundingLinks[1].Message.ShouldBe("Become a member!");
    }

    /// <summary>
    /// The Apple Podcasts ownership token delivered through this namespace is read.
    /// </summary>
    /// <remarks>
    ///     <b>The gap that motivated this whole extension.</b> §2.49 found the Apple verification token
    ///     written as <c>&lt;podcast:txt purpose="applepodcastsverify"&gt;</c> in 21 of 1,934 live feeds,
    ///     against 26 using <c>itunes:applepodcastsverify</c>. Argotic read the iTunes form and not this
    ///     one, so a publisher mid-claim on Omny, Captivate or Buzzsprout lost their token on a
    ///     round-trip and their ownership claim failed. Both values below are real.
    /// </remarks>
    [TestMethod]
    public void TheApplePodcastsOwnershipToken_IsReadFromThisNamespaceToo()
    {
        PodcastSyndicationExtensionContext? context = ChannelContext("""
            <podcast:txt purpose="applepodcastsverify">e6cfaae0-9496-11f0-a272-f9e230f88be0</podcast:txt>
            <podcast:txt purpose="example.com">a third party's own value</podcast:txt>
            <podcast:txt>naked text with no purpose at all</podcast:txt>
            """);

        context.ShouldNotBeNull();
        context.TextEntries.Count.ShouldBe(3);

        PodcastText token = context.TextEntries.Single(t => t.IsApplePodcastsVerification);
        token.Value.ShouldBe("e6cfaae0-9496-11f0-a272-f9e230f88be0");

        context.TextEntries[2].Purpose.ShouldBeEmpty("the purpose attribute is optional");
        context.TextEntries[2].Value.ShouldBe("naked text with no purpose at all");
    }

    /// <summary>
    /// Every person is read, with their role and links.
    /// </summary>
    [TestMethod]
    public void EveryPerson_IsRead()
    {
        PodcastSyndicationExtensionContext? context = ChannelContext("""
            <podcast:person role="host" href="https://example.com/host" img="https://example.com/host.jpg">Alice Example</podcast:person>
            <podcast:person role="guest" group="writing">Bob Example</podcast:person>
            <podcast:person>Carol Example</podcast:person>
            """);

        context.ShouldNotBeNull();
        context.People.Count.ShouldBe(3);

        context.People[0].Name.ShouldBe("Alice Example");
        context.People[0].Role.ShouldBe("host");
        context.People[0].Url.ShouldBe(new Uri("https://example.com/host"));
        context.People[0].ImageUrl.ShouldBe(new Uri("https://example.com/host.jpg"));

        context.People[1].Group.ShouldBe("writing");
        context.People[2].Role.ShouldBeEmpty("role is optional; 'host' is the caller's inference");
    }

    /// <summary>
    /// An episode number that is not a whole number is kept.
    /// </summary>
    /// <remarks>
    ///     <b>Why the property is a decimal.</b> The specification defines the node value as a decimal
    ///     number precisely so a publisher can slot an episode between two others as <c>3.5</c>. Reading
    ///     it as an integer would either refuse that feed or round it into a collision with an episode
    ///     that already exists.
    /// </remarks>
    /// <param name="written">The node value to write into <c>podcast:episode</c>; a decimal number,
    /// not necessarily whole.</param>
    /// <param name="expected">The number it names, widened to <see cref="double"/> because an
    /// attribute argument cannot be a <see cref="decimal"/>.</param>
    [TestMethod]
    [DataRow("42", 42)]
    [DataRow("3.5", 3.5)]
    [DataRow("0", 0)]
    public void AnEpisodeNumberThatIsNotAWholeNumber_IsKept(string written, double expected)
    {
        PodcastSyndicationExtensionContext? context = ItemContext($"<podcast:episode>{written}</podcast:episode>");

        context.ShouldNotBeNull(written);
        context.Episode.ShouldBe((decimal)expected, written);
    }

    /// <summary>
    /// The season and episode display values are read.
    /// </summary>
    [TestMethod]
    public void TheSeasonAndEpisodeDisplayValues_AreRead()
    {
        PodcastSyndicationExtensionContext? context = ItemContext("""
            <podcast:season name="Podcasting 2.0">3</podcast:season>
            <podcast:episode display="Ep. 42 — the one about feeds">42</podcast:episode>
            """);

        context.ShouldNotBeNull();
        context.Season.ShouldBe(3);
        context.SeasonName.ShouldBe("Podcasting 2.0");
        context.Episode.ShouldBe(42m);
        context.EpisodeDisplay.ShouldBe("Ep. 42 — the one about feeds");
    }

    /// <summary>
    /// The chapters file and the license are read.
    /// </summary>
    [TestMethod]
    public void TheChaptersFileAndTheLicense_AreRead()
    {
        PodcastSyndicationExtensionContext? context = ItemContext("""
            <podcast:chapters url="https://example.com/ep1/chapters.json" type="application/json+chapters" />
            <podcast:license url="https://example.com/license">cc-by-4.0</podcast:license>
            """);

        context.ShouldNotBeNull();
        context.Chapters.ShouldNotBeNull();
        context.Chapters.Url.ShouldBe(new Uri("https://example.com/ep1/chapters.json"));
        context.Chapters.MediaType.ShouldBe("application/json+chapters");

        context.License.ShouldNotBeNull();
        context.License.Identifier.ShouldBe("cc-by-4.0");
        context.License.Url.ShouldBe(new Uri("https://example.com/license"));
    }
}