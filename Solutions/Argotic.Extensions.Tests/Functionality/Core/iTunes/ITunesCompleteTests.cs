using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

/// <summary>
/// Covers <c>itunes:complete</c>, the channel element that says a podcast has finished.
/// </summary>
/// <remarks>
///     <para>
///     Apple defines exactly one meaningful value for this element: <c>yes</c>. It tells Apple Podcasts
///     that no further episode will ever be added, so it can stop polling the feed — and Apple warns
///     the effect may be <b>irreversible</b>, in that a feed marked complete may never publish again at
///     that URL.
///     </para>
///     <para>
///     That asymmetry is the whole design here, and it is why this element is not simply the
///     <c>itunes:block</c> code path with a different name. <c>block</c> documents both spellings and a
///     podcast can unblock itself by saying <c>no</c>; <c>complete</c> has no <c>no</c>, and absence is
///     how a running podcast says it is still running. So only <c>yes</c> is recognised on the way in,
///     and only <c>yes</c> is ever written on the way out.
///     </para>
///     <para>
///     It appears <b>zero</b> times in the 136-document real-world corpus. That is not evidence against
///     it — it is what "situational" looks like for an element a podcast emits once, at the end of its
///     life. It is implemented because it was the one element of Apple's current specification this
///     library did not model.
///     </para>
/// </remarks>
[TestClass]
public sealed class ITunesCompleteTests
{
    private static ITunesSyndicationExtensionContext? LoadChannelContext(string itunesElements)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
              <channel>
                <title>A Podcast</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                {itunesElements}
                <item><title>The Last Episode</title></item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Extensions.OfType<ITunesSyndicationExtension>().SingleOrDefault()?.Context;
    }

    /// <summary>
    /// A podcast that says it is complete is read as complete.
    /// </summary>
    /// <remarks>
    ///     Case is disregarded, matching how <c>itunes:block</c> and <c>itunes:explicit</c> are already
    ///     read, and matching the corpus — which capitalises <c>itunes:type</c> inconsistently and would
    ///     do the same here.
    /// </remarks>
    /// <param name="spelling">The node value to write into <c>itunes:complete</c>; any casing of
    /// <c>yes</c>.</param>
    /// <param name="why">Why the row is here, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("yes", "the spelling Apple documents")]
    [DataRow("Yes", "case is disregarded")]
    [DataRow("YES", "case is disregarded")]
    public void APodcastThatSaysItIsComplete_IsReadAsComplete(string spelling, string why)
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext($"<itunes:complete>{spelling}</itunes:complete>");

        context.ShouldNotBeNull(why);
        context.IsComplete.ShouldBeTrue(why);
    }

    /// <summary>
    /// Anything other than <c>yes</c> leaves the podcast running.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     There is no value that means "not complete" — Apple defines only <c>yes</c> — so every other
    ///     value has to fall through to the same answer as saying nothing at all. The <c>no</c> row
    ///     matters most: a publisher who writes it is expressing the default, and reading it as anything
    ///     else would mark a live podcast dead.
    ///     </para>
    ///     <para>
    ///     The empty-element row is taken from real data. One of the 778 <c>itunes:block</c> elements in
    ///     the corpus is written <c>&lt;itunes:block&gt;&lt;/itunes:block&gt;</c> with no content, so a
    ///     publisher emitting an empty sibling here is not hypothetical.
    ///     </para>
    /// </remarks>
    /// <param name="spelling">The node value to write into <c>itunes:complete</c>; anything at all
    /// except <c>yes</c>.</param>
    /// <param name="why">Why the row is here, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("no", "the default, spelled out")]
    [DataRow("", "an empty element -- the corpus contains an empty itunes:block")]
    [DataRow("true", "the boolean spelling itunes:explicit uses is not defined for this element")]
    [DataRow("maybe", "an unknown value")]
    public void AnythingOtherThanYes_LeavesThePodcastRunning(string spelling, string why)
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext($"<itunes:complete>{spelling}</itunes:complete>");

        // The context is legitimately absent here: an unrecognised value is the only iTunes content in
        // the document, so nothing loads and the extension is never attached. Both shapes mean the same
        // thing -- the podcast is still running -- so both are accepted.
        (context?.IsComplete ?? false).ShouldBeFalse(why);
    }

    /// <summary>
    /// A podcast that says nothing is not complete.
    /// </summary>
    [TestMethod]
    public void APodcastThatSaysNothing_IsNotComplete()
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext("<itunes:author>An Author</itunes:author>");

        context.ShouldNotBeNull();
        context.IsComplete.ShouldBeFalse();
    }

    /// <summary>
    /// Surrounding whitespace does not stop the element being recognised.
    /// </summary>
    /// <remarks>
    ///     A publisher who pretty-prints their feed writes the value on its own indented line. Every
    ///     other value in this extension is trimmed before it is matched, and this one has to agree or a
    ///     formatted feed silently reads as a running podcast.
    /// </remarks>
    [TestMethod]
    public void SurroundingWhitespace_DoesNotStopTheElementBeingRecognised()
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext("<itunes:complete>\n      yes\n    </itunes:complete>");

        context.ShouldNotBeNull();
        context.IsComplete.ShouldBeTrue();
    }

    /// <summary>
    /// A running podcast writes no <c>itunes:complete</c> element at all.
    /// </summary>
    /// <remarks>
    ///     <b>The assertion that protects publishers.</b> Because Apple treats the element as
    ///     potentially irreversible, writing <c>&lt;itunes:complete&gt;no&lt;/itunes:complete&gt;</c> on
    ///     every save — the obvious symmetrical implementation — would put an undefined value into every
    ///     feed this library round-trips, at an element where being wrong ends a show.
    /// </remarks>
    [TestMethod]
    public void ARunningPodcast_WritesNoCompleteElementAtAll()
    {
        ITunesSyndicationExtension extension = new();
        extension.Context.Author = "An Author";
        extension.Context.IsComplete = false;

        extension.ToString().Contains("complete", StringComparison.OrdinalIgnoreCase).ShouldBeFalse(
            "a running podcast must not be able to emit this element by accident");
    }

    /// <summary>
    /// A completed podcast writes the element back.
    /// </summary>
    [TestMethod]
    public void ACompletedPodcast_WritesTheElementBack()
    {
        ITunesSyndicationExtension extension = new();
        extension.Context.IsComplete = true;

        extension.ToString().Contains("complete", StringComparison.Ordinal).ShouldBeTrue();
        extension.ToString().Contains(">yes<", StringComparison.Ordinal).ShouldBeTrue();
    }
}