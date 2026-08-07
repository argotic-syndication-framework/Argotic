using System.Reflection;
using System.Xml.XPath;

using Argotic.Extensions.Core;
using Argotic.Publishing;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Extensions;

/// <summary>
/// Covers the two <see cref="SyndicationExtension"/> constructor arguments that look alike and are
/// not: <c>xmlNamespace</c>, which is the extension's identity on the wire, and <c>documentation</c>,
/// which is a link for a human to follow.
/// </summary>
/// <remarks>
///     <para>
///     Nothing in the framework dereferences <see cref="SyndicationExtension.Documentation"/>. It is
///     read by <c>CompareTo</c>, <c>GetHashCode</c> and <c>ToString</c>, and it is handed to consumers
///     as a public property — which is the whole problem. A documentation link rots silently: the
///     suite stays green while the shipped package points a consumer at whoever re-registered the
///     domain. <c>wellformedweb.org</c> lapsed and now serves a gambling site, and
///     <c>apml.org</c> a farm shop. Both were reached from a green build.
///     </para>
///     <para>
///     <see cref="SyndicationExtension.XmlNamespace"/> is the opposite. It is matched by
///     <see cref="SyndicationExtension.ExistsInSource"/> and by the adapter's probe filter, so it is
///     load-bearing in every real feed, and it must <b>not</b> be modernised alongside the link it sits
///     next to. Five extensions pass one string for both arguments, one differs from its namespace by a
///     single trailing slash, one shares a host with it, and one is a proper prefix of it. A regex over
///     the URL, or a line-level edit, silently unregisters the extension.
///     </para>
///     <para>
///     So these pin both together. <see cref="EveryFrameworkExtension_PinsItsXmlNamespaceAndItsDocumentationLink"/>
///     is the characterisation net — it fails on either argument. The sweeps state the rule the links
///     now keep, and <see cref="AnExtensionIsRecognisedByItsNamespaceAlone_NotByItsDocumentationLink"/>
///     asserts the wire behaviour for the eight extensions where the two arguments resemble each other.
///     </para>
/// </remarks>
[TestClass]
public sealed class ExtensionDocumentationLinkTests
{
    /// <summary>
    /// Hosts that no longer serve the specification they were cited for, and that must therefore never
    /// reappear in a <see cref="SyndicationExtension.Documentation"/>.
    /// </summary>
    /// <remarks>
    ///     Each was fetched. <c>wellformedweb.org</c> answers 200 at a Thai gambling site, which is the
    ///     defect that prompted this; <c>backend.userland.com</c> and <c>pheed.com</c> no longer resolve;
    ///     <c>madskills.com</c> answers 522; <c>neugierig.org</c> answers 404; <c>dev.live.com</c> and
    ///     <c>msdn2.microsoft.com</c> redirect to unrelated pages, the latter landing quietly on the
    ///     <c>System.Xml</c> API reference; <c>bitworking.org</c> moved the document and dropped the
    ///     anchors that were cited into it.
    ///     <para>
    ///     Where a replacement is an archived snapshot the host is <c>web.archive.org</c>, so a dead host
    ///     surviving inside the archived URL's path is not a false positive here.
    ///     </para>
    /// </remarks>
    private static readonly string[] HostsThatNoLongerServeTheirSpecification =
    [
        "wellformedweb.org",
        "www.wellformedweb.org",
        "backend.userland.com",
        "madskills.com",
        "www.madskills.com",
        "pheed.com",
        "www.pheed.com",
        "dev.live.com",
        "neugierig.org",
        "msdn2.microsoft.com",
        "bitworking.org",
        "www.bitworking.org",
    ];

    /// <summary>
    /// Gets every concrete <see cref="SyndicationExtension"/> the framework ships, constructed through
    /// its parameterless constructor.
    /// </summary>
    /// <remarks>
    ///     Reflection rather than a list, for the same reason
    ///     <c>SyndicationExtensionAdapter.FrameworkExtensions</c> uses reflection: a new extension is
    ///     covered by the sweeps the moment it compiles, with no registration step to forget. Both
    ///     product assemblies are scanned, because <see cref="AtomMemberResources"/> is a
    ///     <see cref="SyndicationExtension"/> that lives in <c>Argotic.Core</c>.
    /// </remarks>
    private static IEnumerable<SyndicationExtension> FrameworkExtensions =>
        new[] { typeof(SyndicationExtension).Assembly, typeof(AtomMemberResources).Assembly }
            .SelectMany(assembly => assembly.GetExportedTypes())
            .Where(type => type.IsSubclassOf(typeof(SyndicationExtension))
                && !type.IsAbstract
                && type.GetConstructor(Type.EmptyTypes) is not null)
            .Select(type => (SyndicationExtension)Activator.CreateInstance(type)!);

    /// <summary>
    /// Gets how many extensions the two sweeps below must actually visit.
    /// </summary>
    /// <remarks>
    ///     Read off the pinned table's own <see cref="DataRowAttribute"/> list rather than written as a
    ///     literal, so adding an extension updates it in the same edit that adds its row — while a
    ///     reflection scan that returned nothing still fails, because the table cannot be empty.
    /// </remarks>
    private static int ExpectedExtensionCount =>
        typeof(ExtensionDocumentationLinkTests)
            .GetMethod(nameof(EveryFrameworkExtension_PinsItsXmlNamespaceAndItsDocumentationLink))!
            .GetCustomAttributes<DataRowAttribute>()
            .Count();

    /// <summary>
    /// Every shipped extension declares exactly this prefix, this namespace and this documentation link.
    /// </summary>
    /// <remarks>
    ///     The namespace is asserted beside the link so that an edit which walks one argument left or
    ///     right fails here rather than in production. The link is asserted as
    ///     <see cref="Uri.OriginalString"/> rather than through <see cref="Uri"/> equality, because
    ///     <see cref="Uri.Equals(object)"/> ignores the fragment and would pass a link that had lost its
    ///     anchor.
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(AtomPublishingControlSyndicationExtension), "app", "http://www.w3.org/2007/app", "https://www.rfc-editor.org/rfc/rfc5023.html")]
    [DataRow(typeof(AtomPublishingEditedSyndicationExtension), "app", "http://www.w3.org/2007/app", "https://www.rfc-editor.org/rfc/rfc5023.html")]
    [DataRow(typeof(AtomMemberResources), "app", "http://www.w3.org/2007/app", "https://www.rfc-editor.org/rfc/rfc5023.html")]
    [DataRow(typeof(BasicGeocodingSyndicationExtension), "geo", "http://www.w3.org/2003/01/geo/wgs84_pos#", "https://www.w3.org/2003/01/geo/")]
    [DataRow(typeof(BlogChannelSyndicationExtension), "blogChannel", "http://backend.userland.com/blogChannelModule", "https://web.archive.org/web/20090902125603/http://backend.userland.com/blogChannelModule")]
    [DataRow(typeof(CreativeCommonsSyndicationExtension), "creativeCommons", "http://backend.userland.com/creativeCommonsRssModule", "https://www.rssboard.org/creative-commons")]
    [DataRow(typeof(DublinCoreElementSetSyndicationExtension), "dc", "http://purl.org/dc/elements/1.1/", "https://www.dublincore.org/specifications/dublin-core/dces/")]
    [DataRow(typeof(DublinCoreMetadataTermsSyndicationExtension), "dcterms", "http://purl.org/dc/terms/", "https://www.dublincore.org/specifications/dublin-core/dcmi-terms/")]
    [DataRow(typeof(FeedHistorySyndicationExtension), "fh", "http://purl.org/syndication/history/1.0", "https://www.rfc-editor.org/rfc/rfc5005.html")]
    [DataRow(typeof(FeedRankSyndicationExtension), "re", "http://purl.org/atompub/rank/1.0", "https://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt")]
    [DataRow(typeof(FeedSynchronizationSyndicationExtension), "sx", "http://feedsync.org/2007/feedsync", "https://web.archive.org/web/20080705204645/http://dev.live.com/feedsync/spec/")]
    [DataRow(typeof(GeoRssSyndicationExtension), "georss", "http://www.georss.org/georss", "https://docs.ogc.org/cs/17-002r1/17-002r1.html")]
    [DataRow(typeof(ITunesSyndicationExtension), "itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd", "https://podcasters.apple.com/support/823-podcast-requirements")]
    [DataRow(typeof(LiveJournalSyndicationExtension), "lj", "http://livejournal.org/rss/lj/2.0/", "https://web.archive.org/web/20080710121013/http://neugierig.org/drop/lj/rss/")]
    [DataRow(typeof(PheedSyndicationExtension), "photo", "http://www.pheed.com/pheed/", "https://web.archive.org/web/20061231170212/http://www.pheed.com/pheed/")]
    [DataRow(typeof(PingbackSyndicationExtension), "pingback", "http://madskills.com/public/xml/rss/module/pingback/", "https://web.archive.org/web/20091111093504/http://madskills.com/public/xml/rss/module/pingback/")]
    [DataRow(typeof(PodcastSyndicationExtension), "podcast", "https://podcastindex.org/namespace/1.0", "https://github.com/Podcastindex-org/podcast-namespace/blob/main/docs/1.0.md")]
    [DataRow(typeof(SimpleListSyndicationExtension), "cf", "http://www.microsoft.com/schemas/rss/core/2005", "https://learn.microsoft.com/en-us/previous-versions/bb190612(v=msdn.10)")]
    [DataRow(typeof(SitemapHreflangExtension), "xhtml", "http://www.w3.org/1999/xhtml", "https://developers.google.com/search/docs/specialty/international/localized-versions")]
    [DataRow(typeof(SitemapImageExtension), "image", "http://www.google.com/schemas/sitemap-image/1.1", "https://developers.google.com/search/docs/crawling-indexing/sitemaps/image-sitemaps")]
    [DataRow(typeof(SitemapNewsExtension), "news", "http://www.google.com/schemas/sitemap-news/0.9", "https://developers.google.com/search/docs/crawling-indexing/sitemaps/news-sitemap")]
    [DataRow(typeof(SitemapVideoExtension), "video", "http://www.google.com/schemas/sitemap-video/1.1", "https://developers.google.com/search/docs/crawling-indexing/sitemaps/video-sitemaps")]
    [DataRow(typeof(SiteSummaryContentSyndicationExtension), "content", "http://purl.org/rss/1.0/modules/content/", "https://web.resource.org/rss/1.0/modules/content/")]
    [DataRow(typeof(SiteSummarySlashSyndicationExtension), "slash", "http://purl.org/rss/1.0/modules/slash/", "https://web.resource.org/rss/1.0/modules/slash/")]
    [DataRow(typeof(SiteSummaryUpdateSyndicationExtension), "sy", "http://purl.org/rss/1.0/modules/syndication/", "https://web.resource.org/rss/1.0/modules/syndication/")]
    [DataRow(typeof(TrackbackSyndicationExtension), "trackback", "http://madskills.com/public/xml/rss/module/trackback/", "https://www.rssboard.org/trackback")]
    [DataRow(typeof(WellFormedWebCommentsSyndicationExtension), "wfw", "http://wellformedweb.org/CommentAPI/", "https://www.rssboard.org/comment-api")]
    [DataRow(typeof(YahooMediaSyndicationExtension), "media", "http://search.yahoo.com/mrss/", "https://www.rssboard.org/media-rss")]
    public void EveryFrameworkExtension_PinsItsXmlNamespaceAndItsDocumentationLink(Type extensionType, string expectedPrefix, string expectedXmlNamespace, string expectedDocumentation)
    {
        // Arrange & Act
        SyndicationExtension extension = (SyndicationExtension)Activator.CreateInstance(extensionType)!;

        // Assert
        extension.XmlPrefix.ShouldBe(expectedPrefix);
        extension.XmlNamespace.ShouldBe(expectedXmlNamespace);
        extension.Documentation.ShouldNotBeNull();
        extension.Documentation!.OriginalString.ShouldBe(expectedDocumentation);
    }

    /// <summary>
    /// The pinned table above names every extension the framework ships, so a new one cannot be added
    /// without a row.
    /// </summary>
    /// <remarks>
    ///     Without this, the table is a list of the extensions that existed when it was written. A new
    ///     <see cref="SyndicationExtension"/> is auto-discovered by the adapter and would ship an
    ///     unpinned link.
    /// </remarks>
    [TestMethod]
    public void ThePinnedTable_NamesEveryExtensionTheFrameworkShips()
    {
        // Arrange
        IEnumerable<string> pinned = typeof(ExtensionDocumentationLinkTests)
            .GetMethod(nameof(EveryFrameworkExtension_PinsItsXmlNamespaceAndItsDocumentationLink))!
            .GetCustomAttributes<DataRowAttribute>()
            .Select(row => ((Type)row.Data[0]!).FullName!);

        // Act
        IEnumerable<string> shipped = FrameworkExtensions.Select(extension => extension.GetType().FullName!);

        // Assert
        shipped.OrderBy(name => name, StringComparer.Ordinal)
            .ShouldBe(pinned.OrderBy(name => name, StringComparer.Ordinal));
    }

    /// <summary>
    /// Every extension's documentation link is an absolute <c>https</c> URL.
    /// </summary>
    /// <remarks>
    ///     A plaintext link in a shipped package is an invitation to a redirect nobody audits, and every
    ///     host cited here serves TLS. Where the original document is gone the link is an archived
    ///     snapshot, which is served over <c>https</c> too — so the rule holds without exception, and an
    ///     exception is exactly what would let the next rotted <c>http</c> link in.
    /// </remarks>
    [TestMethod]
    public void EveryFrameworkExtensionDocumentation_IsAnAbsoluteHttpsUrl()
    {
        // Arrange & Act
        IReadOnlyList<SyndicationExtension> extensions = [.. FrameworkExtensions];

        // Assert
        // The floor is what stops the sweep self-reporting success over nothing. FrameworkExtensions is
        // a reflection scan; make the extensions internal, give the base class a required constructor
        // parameter, or have trimming drop them, and it yields an empty sequence that satisfies every
        // assertion in the loop below without executing one of them.
        extensions.Count.ShouldBe(ExpectedExtensionCount, "the reflection scan found nothing to sweep");

        foreach (SyndicationExtension extension in extensions)
        {
            extension.Documentation.ShouldNotBeNull($"{extension.GetType().Name} has no documentation link.");
            extension.Documentation!.IsAbsoluteUri.ShouldBeTrue($"{extension.GetType().Name} has a relative documentation link.");
            extension.Documentation.Scheme.ShouldBe(Uri.UriSchemeHttps, $"{extension.GetType().Name} does not cite its specification over https.");
        }
    }

    /// <summary>
    /// No extension's documentation link points at a host that stopped serving the specification.
    /// </summary>
    /// <remarks>
    ///     This is the test that would have caught the shipped defect. It reads
    ///     <see cref="SyndicationExtension.Documentation"/> and nothing else — an
    ///     <see cref="SyndicationExtension.XmlNamespace"/> on a dead host is not a defect, it is an
    ///     identifier, and several of these extensions legitimately keep one.
    /// </remarks>
    [TestMethod]
    public void NoFrameworkExtensionDocumentation_PointsAtAHostThatStoppedServingIt()
    {
        // Arrange & Act
        IReadOnlyList<SyndicationExtension> extensions = [.. FrameworkExtensions];

        // Assert
        extensions.Count.ShouldBe(ExpectedExtensionCount, "the reflection scan found nothing to sweep");

        foreach (SyndicationExtension extension in extensions)
        {
            extension.Documentation.ShouldNotBeNull();
            HostsThatNoLongerServeTheirSpecification.ShouldNotContain(
                extension.Documentation!.Host,
                $"{extension.GetType().Name} cites {extension.Documentation.Host}, which no longer serves the specification.");
        }
    }

    /// <summary>
    /// An extension is recognised in a document that binds its namespace under an unfamiliar prefix,
    /// proving the match comes from the namespace and never from the documentation link.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Every row here is an extension whose namespace and documentation link resembled each other
    ///     closely enough to be confused: the first five passed one string for both constructor
    ///     arguments, <see cref="YahooMediaSyndicationExtension"/>'s two differ by a single trailing
    ///     slash, <see cref="WellFormedWebCommentsSyndicationExtension"/>'s share a host, and
    ///     <see cref="BasicGeocodingSyndicationExtension"/>'s link is a proper prefix of its namespace.
    ///     </para>
    ///     <para>
    ///     The namespace is bound under <c>unconventional</c> rather than the extension's own prefix so
    ///     that <see cref="SyndicationExtension.ExistsInSource"/> cannot answer from its prefix
    ///     shortcut and has to match the namespace itself. The strings below are therefore the
    ///     historical namespaces as real feeds spell them, and they are deliberately still <c>http</c>.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DataRow(typeof(BlogChannelSyndicationExtension), "http://backend.userland.com/blogChannelModule")]
    [DataRow(typeof(CreativeCommonsSyndicationExtension), "http://backend.userland.com/creativeCommonsRssModule")]
    [DataRow(typeof(PheedSyndicationExtension), "http://www.pheed.com/pheed/")]
    [DataRow(typeof(PingbackSyndicationExtension), "http://madskills.com/public/xml/rss/module/pingback/")]
    [DataRow(typeof(TrackbackSyndicationExtension), "http://madskills.com/public/xml/rss/module/trackback/")]
    [DataRow(typeof(YahooMediaSyndicationExtension), "http://search.yahoo.com/mrss/")]
    [DataRow(typeof(WellFormedWebCommentsSyndicationExtension), "http://wellformedweb.org/CommentAPI/")]
    [DataRow(typeof(BasicGeocodingSyndicationExtension), "http://www.w3.org/2003/01/geo/wgs84_pos#")]
    public void AnExtensionIsRecognisedByItsNamespaceAlone_NotByItsDocumentationLink(Type extensionType, string namespaceTheDocumentBinds)
    {
        // Arrange
        string xml = $"""<rss version="2.0" xmlns:unconventional="{namespaceTheDocumentBinds}"><channel /></rss>""";
        XPathNavigator source = new XPathDocument(new StringReader(xml)).CreateNavigator();
        source.MoveToChild(XPathNodeType.Element).ShouldBeTrue();

        SyndicationExtension extension = (SyndicationExtension)Activator.CreateInstance(extensionType)!;

        // Act
        bool recognised = extension.ExistsInSource(source);

        // Assert
        recognised.ShouldBeTrue($"{extension.GetType().Name} no longer matches the namespace real feeds declare.");
    }

    /// <summary>
    /// Extensions that emit an element even when nothing has been set on them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Every other shipped extension guards each write, so attaching a default-constructed one to an
    ///     item adds nothing to the document. These four do not: they emit an empty element carrying only
    ///     the namespace declaration. That is characterised rather than asserted-away because it is
    ///     observable in published output — an <c>&lt;trackback:ping/&gt;</c> with no URL, or an
    ///     <c>&lt;re:rank/&gt;</c> with no value, is what a consumer receives.
    ///     </para>
    ///     <para>
    ///     <c>AtomMemberResources</c> is a deliberate member of the list: an Atom Publishing collection is
    ///     required to carry a title, so it writes an empty one rather than an absent one.
    ///     </para>
    /// </remarks>
    private static readonly string[] ExtensionsThatWriteWhenEmpty =
    [
        nameof(AtomMemberResources),
        nameof(FeedRankSyndicationExtension),
        nameof(SitemapNewsExtension),
        nameof(TrackbackSyndicationExtension),
    ];

    /// <summary>
    /// A default-constructed extension writes no elements, except for the four that are pinned as writing
    /// one anyway.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This replaces the assertion that twenty-one per-family <c>*ConstructorTest</c> methods were
    ///     making, which was <c>new X().ShouldNotBeNull()</c> followed by
    ///     <c>ShouldBeOfType&lt;X&gt;()</c> on a variable already statically typed <c>X</c> — two
    ///     compiler guarantees, neither able to fail for any implementation.
    ///     </para>
    ///     <para>
    ///     What a parameterless extension constructor actually determines is whether the context it
    ///     allocates is empty enough that writing it contributes nothing, and that is a property a
    ///     regression can break in both directions: a new unguarded write moves a type into the
    ///     offending list, and a guard added to one of the four moves it out.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ADefaultConstructedExtension_WritesNothingUnlessItIsOneOfTheFourThatDo()
    {
        // Arrange & Act
        IReadOnlyList<SyndicationExtension> extensions = [.. FrameworkExtensions];
        extensions.Count.ShouldBe(ExpectedExtensionCount, "the reflection scan found nothing to sweep");

        List<string> wrote = [.. extensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension.ToString()))
            .Select(extension => extension.GetType().Name)
            .OrderBy(name => name, StringComparer.Ordinal)];

        // Assert
        wrote.ShouldBe(ExtensionsThatWriteWhenEmpty.OrderBy(name => name, StringComparer.Ordinal));
    }
}