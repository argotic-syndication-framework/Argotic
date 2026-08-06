using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

/// <summary>
/// Covers the spellings real podcasts use for <c>itunes:explicit</c>.
/// </summary>
/// <remarks>
///     <para>
///     Apple changed this element. The original podcasting specification defined
///     <c>yes</c> / <c>no</c> / <c>clean</c>; the current one defines <c>true</c> / <c>false</c>.
///     <see cref="ITunesExplicitMaterial"/> models only the original three, so the two spellings Apple
///     now documents were not recognised at all and left <c>ExplicitMaterial</c> at
///     <see cref="ITunesExplicitMaterial.None"/>.
///     </para>
///     <para>
///     The corpus says which spelling matters. Across every <c>itunes:explicit</c> element in 136 live
///     documents:
///     </para>
///     <list type="bullet">
///       <item><description><c>false</c> — <b>2,722</b></description></item>
///       <item><description><c>no</c> — 896</description></item>
///       <item><description><c>clean</c> — 889</description></item>
///       <item><description><c>true</c> — <b>218</b></description></item>
///       <item><description><c>yes</c> — 45</description></item>
///     </list>
///     <para>
///     <b>2,940 of 4,770 values — 62% — used the unsupported spelling</b>, and it outnumbers the
///     supported boolean spellings better than three to one. <c>true</c> and <c>false</c> are not new
///     states: they are how Apple now writes the two states <c>yes</c> and <c>no</c> already name, so
///     they map onto the existing members rather than extending the enumeration. <c>clean</c> stays a
///     distinct third answer, which is why this cannot simply become a <see cref="bool"/>.
///     </para>
/// </remarks>
[TestClass]
public sealed class ITunesExplicitSpellingTests
{
    private static ITunesSyndicationExtensionContext? LoadContext(string itunesElements)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
              <channel>
                <title>A Podcast</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Episode</title>{itunesElements}</item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Items.Single().FindExtension<ITunesSyndicationExtension>()?.Context;
    }

    /// <summary>
    /// Every spelling a real podcast uses maps to the state it names.
    /// </summary>
    /// <remarks>
    ///     The two rows Apple's current specification defines are the two that were unsupported; the
    ///     three legacy rows are the controls, and they must keep working because 1,830 corpus values
    ///     still use them.
    /// </remarks>
    [TestMethod]
    [DataRow("false", ITunesExplicitMaterial.No, "current spelling, 2,722 in the corpus")]
    [DataRow("true", ITunesExplicitMaterial.Yes, "current spelling, 218 in the corpus")]
    [DataRow("no", ITunesExplicitMaterial.No, "legacy spelling, 896")]
    [DataRow("yes", ITunesExplicitMaterial.Yes, "legacy spelling, 45")]
    [DataRow("clean", ITunesExplicitMaterial.Clean, "legacy, and a third state neither boolean covers")]
    public void EverySpellingRealPodcastsUse_MapsToTheStateItNames(string spelling, ITunesExplicitMaterial expected, string provenance)
    {
        ITunesSyndicationExtensionContext? context = LoadContext($"<itunes:explicit>{spelling}</itunes:explicit>");

        context.ShouldNotBeNull(provenance);
        context.ExplicitMaterial.ShouldBe(expected, provenance);
    }

    /// <summary>
    /// The spelling is matched without regard to case.
    /// </summary>
    /// <remarks>
    ///     The existing lookup is case-insensitive, and the new spellings must not be stricter than the
    ///     ones they join.
    /// </remarks>
    [TestMethod]
    [DataRow("FALSE", ITunesExplicitMaterial.No)]
    [DataRow("True", ITunesExplicitMaterial.Yes)]
    [DataRow("CLEAN", ITunesExplicitMaterial.Clean)]
    public void TheSpellingIsMatched_WithoutRegardToCase(string spelling, ITunesExplicitMaterial expected)
    {
        LoadContext($"<itunes:explicit>{spelling}</itunes:explicit>")!.ExplicitMaterial.ShouldBe(expected, spelling);
    }

    /// <summary>
    /// A value that names no state leaves the property unset.
    /// </summary>
    /// <remarks>
    ///     The boundary control. Accepting two more spellings must not turn into accepting anything —
    ///     an unrecognised value is still <see cref="ITunesExplicitMaterial.None"/>, and the duration
    ///     alongside it proves the extension itself still attached, so the assertion is about the one
    ///     property rather than about the element being ignored wholesale.
    /// </remarks>
    [TestMethod]
    public void AValueThatNamesNoState_LeavesThePropertyUnset()
    {
        ITunesSyndicationExtensionContext? context =
            LoadContext("<itunes:duration>01:23:45</itunes:duration><itunes:explicit>maybe</itunes:explicit>");

        context.ShouldNotBeNull();
        context.Duration.ShouldBe(new TimeSpan(1, 23, 45), "the extension attached, so the check below is meaningful");
        context.ExplicitMaterial.ShouldBe(ITunesExplicitMaterial.None);
    }

    /// <summary>
    /// Every duration format real podcasts use is read.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Not a defect — all six already worked — but none had a test and the corpus shows all six are
    ///     in use, so a change to <c>ParseDuration</c> would have been unguarded. Counted over the
    ///     corpus: <c>HH:MM:SS</c> 2,942; bare seconds of four digits 2,615; <c>MM:SS</c> 60; three
    ///     digits 24; two digits 3; five digits 2.
    ///     </para>
    ///     <para>
    ///     The bare-integer rows are the ones worth having. A reader that assumed a colon would treat
    ///     <c>3600</c> as unparseable and drop an hour-long episode's duration, and 2,644 corpus values
    ///     are written that way — more than are written <c>MM:SS</c> by a factor of forty.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow("01:23:45", 1, 23, 45, "HH:MM:SS — 2,942 in the corpus")]
    [DataRow("3600", 1, 0, 0, "bare seconds — 2,615")]
    [DataRow("23:45", 0, 23, 45, "MM:SS — 60")]
    [DataRow("360", 0, 6, 0, "three digits — 24")]
    [DataRow("45", 0, 0, 45, "two digits — 3")]
    [DataRow("12345", 3, 25, 45, "five digits — 2")]
    public void EveryDurationFormatRealPodcastsUse_IsRead(string duration, int hours, int minutes, int seconds, string provenance)
    {
        ITunesSyndicationExtensionContext? context = LoadContext($"<itunes:duration>{duration}</itunes:duration>");

        context.ShouldNotBeNull(provenance);
        context.Duration.ShouldBe(new TimeSpan(hours, minutes, seconds), provenance);
    }
}