namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Reading the same extension data from an RSS feed and from an Atom feed.
/// </summary>
/// <remarks>
///     <para>
///     Atom and RSS are filled by separate adapters, and every extension-attachment test in this suite
///     went through RSS: <c>ExtensionTestUtil.GetWrappedXml</c> builds an RSS 2.0 document and is used
///     at a hundred call sites. Atom attachment was covered by two tests, both using DublinCore alone,
///     so twenty-four of the twenty-five extensions had never been attached to an Atom entry.
///     </para>
///     <para>
///     The fragment below is taken from the first item of <c>SampleData/RssFeedWithExtensions.xml</c>,
///     the only realistic extension-bearing document in the repository. Asserting <i>parity</i> rather
///     than a hand-written expected list is deliberate: it tests the property that actually matters -
///     the two adapters agree - without depending on my reading of which extensions ought to match.
///     </para>
/// </remarks>
[TestClass]
public class ReadExtensionsFromBothFormats
{
    private const string Namespaces =
        """xmlns:content="http://purl.org/rss/1.0/modules/content/" """ +
        """xmlns:dc="http://purl.org/dc/elements/1.1/" """ +
        """xmlns:dcterms="http://purl.org/dc/terms/" """ +
        """xmlns:geo="http://www.w3.org/2003/01/geo/wgs84_pos#" """ +
        """xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd" """ +
        """xmlns:media="http://search.yahoo.com/mrss/" """ +
        """xmlns:photo="http://www.pheed.com/pheed/" """ +
        """xmlns:slash="http://purl.org/rss/1.0/modules/slash/" """ +
        """xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/" """ +
        """xmlns:wfw="http://wellformedweb.org/CommentAPI/" """ +
        """
        xmlns:cf="http://www.microsoft.com/schemas/rss/core/2005"
        """;

    private const string ExtensionElements =
        "<content:encoded><![CDATA[<p>The body</p>]]></content:encoded>" +
        "<dc:creator>A Creator</dc:creator>" +
        "<dc:subject>A Subject</dc:subject>" +
        "<dcterms:abstract>An abstract</dcterms:abstract>" +
        "<geo:lat>51.5074</geo:lat><geo:long>-0.1278</geo:long>" +
        "<itunes:author>An Author</itunes:author><itunes:duration>1234</itunes:duration>" +
        """<media:content url="http://example.com/media.mp4" />""" +
        "<media:title>A media title</media:title>" +
        "<photo:imgsrc>http://example.com/photo.jpg</photo:imgsrc>" +
        "<slash:section>articles</slash:section>" +
        "<slash:comments>17</slash:comments>" +
        "<trackback:ping>http://example.com/trackback</trackback:ping>" +
        "<wfw:comment>http://example.com/comment</wfw:comment>" +
        "<cf:treatAs>list</cf:treatAs>";

    /// <summary>
    /// The RSS and Atom adapters attach the same extensions from the same extension elements.
    /// </summary>
    /// <remarks>
    ///     This is the assertion the suite was missing. If the Atom adapter ever stops filling an
    ///     extension the RSS adapter fills - or vice versa - this fails and names the difference.
    /// </remarks>
    [TestMethod]
    public void BothAdapters_AttachTheSameExtensionsFromTheSameElements()
    {
        IReadOnlyCollection<Type> fromRss = ExtensionTypesOnRssItem();
        IReadOnlyCollection<Type> fromAtom = ExtensionTypesOnAtomEntry();

        fromRss.ShouldNotBeEmpty("the RSS fixture attached no extensions at all, so the comparison is vacuous");

        string rssOnly = string.Join(", ", fromRss.Except(fromAtom).Select(t => t.Name).Order());
        string atomOnly = string.Join(", ", fromAtom.Except(fromRss).Select(t => t.Name).Order());

        rssOnly.ShouldBeEmpty($"attached to the RSS item but not the Atom entry: {rssOnly}");
        atomOnly.ShouldBeEmpty($"attached to the Atom entry but not the RSS item: {atomOnly}");
    }

    /// <summary>
    /// The fixture attaches a meaningful number of extensions, so parity is not trivially satisfied.
    /// </summary>
    [TestMethod]
    public void TheSharedFixture_AttachesSeveralExtensions()
    {
        ExtensionTypesOnAtomEntry().Count.ShouldBeGreaterThanOrEqualTo(
            8,
            "the fixture is meant to carry roughly a dozen extension families; if this drops, parity is being asserted over almost nothing");
    }

    /// <summary>
    /// Each named extension is attached to an Atom entry, not only to an RSS item.
    /// </summary>
    /// <param name="extensionTypeName">The extension expected on the Atom entry.</param>
    [TestMethod]
    [DataRow(nameof(SiteSummaryContentSyndicationExtension))]
    [DataRow(nameof(DublinCoreElementSetSyndicationExtension))]
    [DataRow(nameof(DublinCoreMetadataTermsSyndicationExtension))]
    [DataRow(nameof(BasicGeocodingSyndicationExtension))]
    [DataRow(nameof(ITunesSyndicationExtension))]
    [DataRow(nameof(YahooMediaSyndicationExtension))]
    [DataRow(nameof(PheedSyndicationExtension))]
    [DataRow(nameof(SiteSummarySlashSyndicationExtension))]
    [DataRow(nameof(TrackbackSyndicationExtension))]
    [DataRow(nameof(WellFormedWebCommentsSyndicationExtension))]
    [DataRow(nameof(SimpleListSyndicationExtension))]
    public void AnAtomEntry_AttachesTheNamedExtension(string extensionTypeName)
    {
        ExtensionTypesOnAtomEntry()
            .Select(t => t.Name)
            .ShouldContain(extensionTypeName, $"{extensionTypeName} was not attached to the Atom entry");
    }

    /// <summary>
    /// Extension data read from an Atom entry carries the same values as from an RSS item.
    /// </summary>
    [TestMethod]
    public void ExtensionDataReadFromAtom_MatchesTheDataReadFromRss()
    {
        AtomEntry entry = LoadAtomEntry();
        RssItem item = LoadRssItem();

        DublinCoreElementSetSyndicationExtension atomDc =
            entry.Extensions.OfType<DublinCoreElementSetSyndicationExtension>().Single();
        DublinCoreElementSetSyndicationExtension rssDc =
            item.Extensions.OfType<DublinCoreElementSetSyndicationExtension>().Single();

        // Each cross-format comparison is anchored to the literal the fixture declares. Without the
        // anchor, two adapters that both dropped the element agree on null and the assertion passes.
        atomDc.Context.Creator.ShouldBe("A Creator");
        atomDc.Context.Creator.ShouldBe(rssDc.Context.Creator);
        atomDc.Context.Subject.ShouldBe("A Subject");
        atomDc.Context.Subject.ShouldBe(rssDc.Context.Subject);

        BasicGeocodingSyndicationExtension atomGeo =
            entry.Extensions.OfType<BasicGeocodingSyndicationExtension>().Single();
        BasicGeocodingSyndicationExtension rssGeo =
            item.Extensions.OfType<BasicGeocodingSyndicationExtension>().Single();

        atomGeo.Context.Latitude.ShouldBe(51.5074m);
        atomGeo.Context.Latitude.ShouldBe(rssGeo.Context.Latitude);
        atomGeo.Context.Longitude.ShouldBe(-0.1278m);
        atomGeo.Context.Longitude.ShouldBe(rssGeo.Context.Longitude);
    }

    private static IReadOnlyCollection<Type> ExtensionTypesOnAtomEntry() =>
        [.. LoadAtomEntry().Extensions.Select(e => e.GetType())];

    private static IReadOnlyCollection<Type> ExtensionTypesOnRssItem() =>
        [.. LoadRssItem().Extensions.Select(e => e.GetType())];

    private static AtomEntry LoadAtomEntry()
    {
        using StringReader stringReader = new(ExtensionTestUtil.GetWrappedAtomXml(Namespaces, ExtensionElements));
        using XmlReader reader = XmlReader.Create(stringReader);
        AtomFeed feed = new();
        feed.Load(reader);

        return feed.Entries.Single();
    }

    private static RssItem LoadRssItem()
    {
        using StringReader stringReader = new(ExtensionTestUtil.GetWrappedXml(Namespaces, ExtensionElements));
        using XmlReader reader = XmlReader.Create(stringReader);
        RssFeed feed = new();
        feed.Load(reader);

        return feed.Channel.Items.Single();
    }
}