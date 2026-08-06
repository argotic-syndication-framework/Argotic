using System.Text;
using System.Xml;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

/// <summary>
/// Covers an enclosure that does not state its size, which RSS 2.0 requires and real publishers omit.
/// </summary>
/// <remarks>
///     <para>
///     <b>The sentinel was a value the property itself rejected.</b> <c>Length</c> defaulted to
///     <see cref="long.MinValue"/> — documented as "no size was specified" — while its setter ran
///     <c>ArgumentOutOfRangeException.ThrowIfLessThan(value, 0)</c>. Loading produced the default through
///     the field initialiser, so a consumer could read it but could never write it back: assigning the
///     property to itself threw, and so did copying an enclosure.
///     </para>
///     <para>
///     Reachable from real data. <b>27 enclosures</b> in the real-world corpus omit <c>length</c> — the
///     YouTube thumbnails in a video feed and the flyer images in DNA Lounge's — against <b>1,068</b>
///     that state <c>length="0"</c>, which is what the RSS 2.0 specification tells a publisher to write
///     when the size cannot be determined. Those two cases are genuinely different and both are common,
///     so collapsing them was not an option.
///     </para>
///     <para>
///     <c>Length</c> is now <see cref="long"/>?, which is the same answer <c>SitemapUrl.LastModified</c>
///     and <c>SitemapUrl.Priority</c> already give to the same question, and <c>WriteTo</c> omits the
///     attribute rather than writing <c>length=""</c> — an empty string is not a valid RSS 2.0 length,
///     and echoing the input is the more faithful round-trip.
///     </para>
/// </remarks>
[TestClass]
public sealed class RssEnclosureLengthTests
{
    private static RssEnclosure LoadEnclosure(string enclosure)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>A Channel</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Item</title>{enclosure}</item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Items.Single().Enclosures.Single();
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

    /// <summary>
    /// An enclosure that omits its length reports no length, rather than a sentinel.
    /// </summary>
    [TestMethod]
    public void AnEnclosureWithNoLength_ReportsNoLength()
    {
        RssEnclosure enclosure = LoadEnclosure("""<enclosure url="https://i.ytimg.com/vi/x/hq2.jpg" type="image/jpeg"/>""");

        enclosure.Length.ShouldBeNull("the attribute was absent, and absent is not a number");
        enclosure.ContentType.ShouldBe("image/jpeg");
    }

    /// <summary>
    /// An enclosure stating <c>length="0"</c> reports zero, which is not the same as reporting nothing.
    /// </summary>
    /// <remarks>
    ///     RSS 2.0 tells a publisher to write 0 when the size cannot be determined, and 1,068 corpus
    ///     enclosures do exactly that. Reading it as "absent" would discard a deliberate statement.
    /// </remarks>
    [TestMethod]
    public void AnEnclosureStatingZeroLength_ReportsZeroRatherThanNothing()
    {
        RssEnclosure enclosure = LoadEnclosure("""<enclosure url="https://example.com/a.mp3" length="0" type="audio/mpeg"/>""");

        enclosure.Length.ShouldBe(0L);
        enclosure.Length.ShouldNotBeNull("an explicit zero is a statement, not an absence");
    }

    /// <summary>
    /// An enclosure stating a real length reports it.
    /// </summary>
    /// <remarks>
    ///     The control, taken from a real Microsoft DevBlogs enclosure.
    /// </remarks>
    [TestMethod]
    public void AnEnclosureStatingALength_ReportsIt()
    {
        RssEnclosure enclosure = LoadEnclosure("""<enclosure url="https://example.com/a.webm" length="2602692" type="video/webm"/>""");

        enclosure.Length.ShouldBe(2602692L);
    }

    /// <summary>
    /// A length-less enclosure can be assigned to itself and copied.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This is the defect stated as the operation a consumer actually performs. Both of these threw
    ///     <see cref="ArgumentOutOfRangeException"/>, because the value the loader produced was one the
    ///     setter refused — so any code that read an enclosure and wrote it somewhere else fell over on
    ///     the 27 corpus enclosures that omit the attribute.
    ///     </para>
    ///     <para>
    ///     The copy is asserted as well as the self-assignment because they fail for the same reason but
    ///     a partial fix could plausibly rescue only the first.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ALengthLessEnclosure_CanBeAssignedToItselfAndCopied()
    {
        RssEnclosure enclosure = LoadEnclosure("""<enclosure url="https://example.com/a.jpg" type="image/jpeg"/>""");

        Should.NotThrow(() => enclosure.Length = enclosure.Length);

        RssEnclosure copy = new()
        {
            Url = enclosure.Url,
            ContentType = enclosure.ContentType,
            Length = enclosure.Length,
        };

        copy.Length.ShouldBeNull();
        copy.Url.ShouldBe(enclosure.Url);
    }

    /// <summary>
    /// A negative length is still refused.
    /// </summary>
    /// <remarks>
    ///     The boundary control. Making the absent case representable must not make nonsense
    ///     representable: a byte count below zero is still meaningless.
    /// </remarks>
    [TestMethod]
    public void ANegativeLength_IsStillRefused()
    {
        RssEnclosure enclosure = new();

        Should.Throw<ArgumentOutOfRangeException>(() => enclosure.Length = -1);
    }

    /// <summary>
    /// Saving a length-less enclosure omits the attribute rather than writing an empty one.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>length=""</c> is not a valid RSS 2.0 length — the attribute is defined as a byte count —
    ///     so emitting it produced a document that other readers may reject and that says something
    ///     different from what the publisher wrote. Omitting the attribute reproduces the input.
    ///     </para>
    ///     <para>
    ///     The second assertion is the one with teeth: it checks the round-trip, not the text. A change
    ///     that emitted something merely different-looking would still have to reload as absent.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void SavingALengthLessEnclosure_OmitsTheAttribute()
    {
        const string Document = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>A Channel</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Item</title><enclosure url="https://example.com/a.jpg" type="image/jpeg"/></item>
              </channel>
            </rss>
            """;

        RssFeed feed = new();
        using (MemoryStream stream = new(Encoding.UTF8.GetBytes(Document), writable: false))
        {
            feed.Load(stream);
        }

        string saved = Save(feed);

        saved.Contains("length=\"\"", StringComparison.Ordinal)
            .ShouldBeFalse("an empty string is not a valid RSS 2.0 length");

        using MemoryStream reloadStream = new(Encoding.UTF8.GetBytes(saved), writable: false);
        RssFeed reloaded = new();
        reloaded.Load(reloadStream);

        reloaded.Channel.Items.Single().Enclosures.Single().Length
            .ShouldBeNull("what was absent on the way in is absent on the way out");
    }

    /// <summary>
    /// Saving an enclosure that states a length still writes it.
    /// </summary>
    /// <remarks>
    ///     The control for the test above: omitting the attribute when it is absent must not turn into
    ///     omitting it always.
    /// </remarks>
    [TestMethod]
    public void SavingAnEnclosureWithALength_StillWritesIt()
    {
        const string Document = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <title>A Channel</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <item><title>An Item</title><enclosure url="https://example.com/a.mp3" length="2602692" type="audio/mpeg"/></item>
              </channel>
            </rss>
            """;

        RssFeed feed = new();
        using (MemoryStream stream = new(Encoding.UTF8.GetBytes(Document), writable: false))
        {
            feed.Load(stream);
        }

        string saved = Save(feed);

        saved.Contains("length=\"2602692\"", StringComparison.Ordinal).ShouldBeTrue();

        using MemoryStream reloadStream = new(Encoding.UTF8.GetBytes(saved), writable: false);
        RssFeed reloaded = new();
        reloaded.Load(reloadStream);

        reloaded.Channel.Items.Single().Enclosures.Single().Length.ShouldBe(2602692L);
    }
}