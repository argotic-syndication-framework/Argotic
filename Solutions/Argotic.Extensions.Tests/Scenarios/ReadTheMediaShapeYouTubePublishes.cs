using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Reads the Media RSS shape that accounts for most of the <c>media:</c> elements in the wild.
/// </summary>
/// <remarks>
///     <para>
///     <c>media:thumbnail</c> is the single most common extension element in the whole corpus —
///     <b>4,009</b> occurrences, against 679 <c>media:content</c> and 326 <c>media:title</c> — and
///     almost all of it arrives the same way: a YouTube channel feed, wrapping everything in a
///     <c>media:group</c>. This fixture reproduces the structure of
///     <c>atom10/youtube-channel-veritasium.atom</c>.
///     </para>
///     <para>
///     Two things are worth pinning about it, and neither is visible from a feed that puts its media
///     elements at the top level.
///     </para>
/// </remarks>
[TestClass]
public sealed class ReadTheMediaShapeYouTubePublishes
{
    /// <summary>
    /// A YouTube channel entry, structured as YouTube structures it.
    /// </summary>
    /// <remarks>
    ///     <c>media:community</c> and its two children are Media RSS 1.5 and are <b>not modelled</b> by
    ///     this library. They are kept in the fixture on purpose — see
    ///     <see cref="AnUnmodelledMediaElement_DoesNotCostTheOnesAroundIt"/>.
    /// </remarks>
    private const string YouTubeChannelEntry = """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/">
          <title>A Channel</title>
          <id>urn:example:channel</id>
          <updated>2026-01-01T00:00:00Z</updated>
          <entry>
            <title>An Entry</title>
            <id>urn:example:entry</id>
            <updated>2026-01-01T00:00:00Z</updated>
            <media:group>
              <media:title>A Video</media:title>
              <media:content url="https://www.youtube.com/v/abc" type="application/x-shockwave-flash" width="640" height="390"/>
              <media:thumbnail url="https://i.ytimg.com/vi/abc/hqdefault.jpg" width="480" height="360"/>
              <media:description>A description.</media:description>
              <media:community>
                <media:starRating count="79336" average="5.00" min="1" max="5"/>
                <media:statistics views="2900457"/>
              </media:community>
            </media:group>
          </entry>
        </feed>
        """;

    private static YahooMediaSyndicationExtension LoadMediaExtension()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(YouTubeChannelEntry), writable: false);
        AtomFeed feed = new();
        feed.Load(stream);

        return feed.Entries.Single().Extensions.OfType<YahooMediaSyndicationExtension>().Single();
    }

    /// <summary>
    /// Media elements inside a group are reachable through the group, and not through the top level.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>The trap this pins is the empty top-level collection.</b> <c>Thumbnails</c> and
    ///     <c>Contents</c> on the context read <b>zero</b> for this entry even though the entry plainly
    ///     has a thumbnail and a content — because both live inside the <c>media:group</c>, and the
    ///     group is a separate object. A consumer that reads <c>Context.Thumbnails</c> gets nothing from
    ///     the feeds that supply most of the thumbnails on the web, and nothing about that failure looks
    ///     like a failure.
    ///     </para>
    ///     <para>
    ///     Asserted in both directions on purpose: the zero is as much the subject of this test as the
    ///     one.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void MediaElementsInsideAGroup_AreReachableThroughTheGroupRatherThanTheTopLevel()
    {
        YahooMediaSyndicationExtensionContext context = LoadMediaExtension().Context;

        context.Thumbnails.ShouldBeEmpty("the thumbnail is inside the group, not beside it");
        context.Contents.ShouldBeEmpty("and so is the content");

        YahooMediaGroup group = context.Groups.ShouldHaveSingleItem();

        group.Title.ShouldNotBeNull().Content.ShouldBe("A Video");
        group.Contents.Count.ShouldBe(1);

        YahooMediaThumbnail thumbnail = group.Thumbnails.ShouldHaveSingleItem();
        thumbnail.Url.ShouldBe(new Uri("https://i.ytimg.com/vi/abc/hqdefault.jpg"));
        thumbnail.Width.ShouldBe(480);
        thumbnail.Height.ShouldBe(360);
    }

    /// <summary>
    /// An element the library does not model does not cost the elements around it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>media:community</c>, <c>media:starRating</c> and <c>media:statistics</c> are Media RSS 1.5
    ///     and are not modelled here — 30 corpus occurrences each, all from YouTube. What matters is
    ///     that meeting one does not cause the group around it to be abandoned: the thumbnail and
    ///     content that sit beside it in the same <c>media:group</c> still arrive.
    ///     </para>
    ///     <para>
    ///     This is the property that makes a partial extension model tolerable, and it is worth a test
    ///     because the failure mode is not an exception — it is a silently shorter list.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnUnmodelledMediaElement_DoesNotCostTheOnesAroundIt()
    {
        YahooMediaGroup group = LoadMediaExtension().Context.Groups.ShouldHaveSingleItem();

        group.Thumbnails.ShouldNotBeEmpty("media:community sits beside the thumbnail in the same group");
        group.Contents.ShouldNotBeEmpty();
        group.Title.ShouldNotBeNull();
    }
}