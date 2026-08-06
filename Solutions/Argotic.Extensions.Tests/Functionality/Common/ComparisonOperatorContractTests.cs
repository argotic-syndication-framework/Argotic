using System.Reflection;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Net;
using Argotic.Publishing;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;

using Shouldly;

using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Verifies the C# 14 extension comparison operators for every type that opts into them.
/// </summary>
/// <remarks>
///     <para>
///     This replaces 72 near-identical files that derived from a generic base class of 20 test methods,
///     producing 1,440 executed tests - 38% of the whole suite - against an 8-complexity utility that was
///     already fully covered. Seventy-one of those files declared no test of their own; they existed only
///     to supply three factory expressions.
///     </para>
///     <para>
///     The contract asserted here is identical to the one those files asserted. Note in particular what it
///     does <i>not</i> claim: nothing compares the lesser or greater instance against the baseline, because
///     several registrations (<see cref="SyndicationResourceSaveSettings"/> among them) supply a lesser
///     instance that compares equal to the baseline. Tightening that is a behaviour change, not a
///     refactoring, and is not made here.
///     </para>
/// </remarks>
[TestClass]
public class ComparisonOperatorContractTests
{
    /// <summary>
    /// Gets the registration table: one row per type that implements <see cref="IComparisonOperators"/>.
    /// </summary>
    public static IEnumerable<object[]> ComparableTypes =>
    [
        Row<DiscoverableSyndicationEndpoint>(() => new(new Uri("http://example.com/feed.xml"), "application/rss+xml"), () => new(new Uri("http://example.com/atom.xml"), "application/atom+xml"), () => new(new Uri("http://example.com/rss.xml"), "application/rss+xml")),
        Row<SyndicationResourceLoadSettings>(() => new() { RetrievalLimit = 50 }, () => new() { RetrievalLimit = 10 }, () => new() { RetrievalLimit = 100 }),
        Row<SyndicationResourceSaveSettings>(() => new() { MinimizeOutputSize = false }, () => new() { MinimizeOutputSize = false }, () => new() { MinimizeOutputSize = true }),
        Row<ApmlApplication>(() => new() { Name = "App B" }, () => new() { Name = "App A" }, () => new() { Name = "App C" }),
        Row<ApmlAuthor>(() => new() { Key = "author-b" }, () => new() { Key = "author-a" }, () => new() { Key = "author-c" }),
        Row<ApmlConcept>(() => new() { Key = "concept-b" }, () => new() { Key = "concept-a" }, () => new() { Key = "concept-c" }),
        Row<ApmlHead>(() => new() { Title = "Head B" }, () => new() { Title = "Head A" }, () => new() { Title = "Head C" }),
        Row<ApmlProfile>(() => new() { Name = "profile-b" }, () => new() { Name = "profile-a" }, () => new() { Name = "profile-c" }),
        Row<ApmlSource>(() => new() { Key = "source-b" }, () => new() { Key = "source-a" }, () => new() { Key = "source-c" }),
        Row<AtomPersonConstruct>(() => new() { Name = "Bob" }, () => new() { Name = "Alice" }, () => new() { Name = "Charlie" }),
        Row<AtomTextConstruct>(() => new("Text B"), () => new("Text A"), () => new("Text C")),
        Row<BlogMLAttachment>(() => new() { MimeType = "image/png", Url = new Uri("http://example.com/b.png") }, () => new() { MimeType = "image/jpeg", Url = new Uri("http://example.com/a.jpg") }, () => new() { MimeType = "image/webp", Url = new Uri("http://example.com/c.webp") }),
        Row<BlogMLTextConstruct>(() => new() { Content = "Text B" }, () => new() { Content = "Text A" }, () => new() { Content = "Text C" }),
        Row<FeedHistoryLinkRelation>(() => new() { Uri = new Uri("http://example.com/feed/b") }, () => new() { Uri = new Uri("http://example.com/feed/a") }, () => new() { Uri = new Uri("http://example.com/feed/c") }),
        Row<FeedSynchronizationHistory>(() => new() { Sequence = 2, By = "user-b" }, () => new() { Sequence = 1, By = "user-a" }, () => new() { Sequence = 3, By = "user-c" }),
        Row<FeedSynchronizationItem>(() => new() { Id = "item-b", Updates = 2 }, () => new() { Id = "item-a", Updates = 1 }, () => new() { Id = "item-c", Updates = 3 }),
        Row<FeedSynchronizationRelatedInformation>(() => new() { Link = new Uri("http://example.com/related/b") }, () => new() { Link = new Uri("http://example.com/related/a") }, () => new() { Link = new Uri("http://example.com/related/c") }),
        Row<FeedSynchronizationSharingInformation>(() => new() { Since = "2024-02-01" }, () => new() { Since = "2024-01-01" }, () => new() { Since = "2024-03-01" }),
        Row<GenericSyndicationCategory>(() => new("category-b", "scheme-b"), () => new("category-a", "scheme-a"), () => new("category-c", "scheme-c")),
        Row<GenericSyndicationItem>(() => new(new RssItem { Title = "Item B" }), () => new(new RssItem { Title = "Item A" }), () => new(new RssItem { Title = "Item C" })),
        Row<ITunesCategory>(() => new() { Text = "Category B" }, () => new() { Text = "Category A" }, () => new() { Text = "Category C" }),
        Row<ITunesOwner>(() => new() { Name = "Owner B" }, () => new() { Name = "Owner A" }, () => new() { Name = "Owner C" }),
        Row<LiveJournalMood>(() => new() { Content = "happy", Id = 2 }, () => new() { Content = "calm", Id = 1 }, () => new() { Content = "joyful", Id = 3 }),
        Row<LiveJournalSecurity>(() => new(LiveJournalSecurityType.Friends, 2), () => new(LiveJournalSecurityType.Friends, 1), () => new(LiveJournalSecurityType.Friends, 3)),
        Row<LiveJournalUserPicture>(() => new() { Keyword = "pic-b", Url = new Uri("http://example.com/b.png") }, () => new() { Keyword = "pic-a", Url = new Uri("http://example.com/a.png") }, () => new() { Keyword = "pic-c", Url = new Uri("http://example.com/c.png") }),
        // GeoRssPosition and GeoRssBox are deliberately absent: they are record structs, and both this
        // helper and the ComparisonOperatorExtensions block it verifies are declared `where T : class`.
        // They declare their own relational operators and are covered by GeoRssValueTypeTests instead.
        Row<GeoRssLine>(() => new([new(2m, 2m)]), () => new([new(1m, 1m)]), () => new([new(3m, 3m)])),
        Row<GeoRssPolygon>(() => new([new(2m, 2m)]), () => new([new(1m, 1m)]), () => new([new(3m, 3m)])),
        Row<PodcastChapters>(() => new() { Url = new Uri("http://example.com/b.json") }, () => new() { Url = new Uri("http://example.com/a.json") }, () => new() { Url = new Uri("http://example.com/c.json") }),
        Row<PodcastFunding>(() => new() { Url = new Uri("http://example.com/b") }, () => new() { Url = new Uri("http://example.com/a") }, () => new() { Url = new Uri("http://example.com/c") }),
        Row<PodcastLicense>(() => new() { Identifier = "cc-by-4.0" }, () => new() { Identifier = "cc-by-3.0" }, () => new() { Identifier = "cc-by-sa-4.0" }),
        Row<PodcastPerson>(() => new() { Name = "Person B" }, () => new() { Name = "Person A" }, () => new() { Name = "Person C" }),
        Row<PodcastText>(() => new() { Purpose = "purpose-b" }, () => new() { Purpose = "purpose-a" }, () => new() { Purpose = "purpose-c" }),
        Row<PodcastTranscript>(() => new() { Url = new Uri("http://example.com/b.vtt") }, () => new() { Url = new Uri("http://example.com/a.vtt") }, () => new() { Url = new Uri("http://example.com/c.vtt") }),
        Row<TrackbackMessage>(() => new(new Uri("http://example.com/post/2")) { Title = "Post 2" }, () => new(new Uri("http://example.com/post/1")) { Title = "Post 1" }, () => new(new Uri("http://example.com/post/3")) { Title = "Post 3" }),
        Row<TrackbackResponse>(() => new(), () => new(), () => new("Error occurred")),
        Row<XmlRpcMessage>(() => new("methodB"), () => new("methodA"), () => new("methodC")),
        Row<XmlRpcResponse>(() => new(), () => new(), () => new(new XmlRpcScalarValue("value"))),
        Row<XmlRpcStructureMember>(() => new("memberB", new XmlRpcScalarValue("valueB")), () => new("memberA", new XmlRpcScalarValue("valueA")), () => new("memberC", new XmlRpcScalarValue("valueC"))),
        Row<OpmlOutline>(() => new() { Text = "Outline B" }, () => new() { Text = "Outline A" }, () => new() { Text = "Outline C" }),
        Row<OpmlOwner>(() => new() { Name = "Bob" }, () => new() { Name = "Alice" }, () => new() { Name = "Charlie" }),
        Row<OpmlWindow>(() => new() { Top = 100, Left = 100, Bottom = 500, Right = 500 }, () => new() { Top = 50, Left = 50, Bottom = 400, Right = 400 }, () => new() { Top = 150, Left = 150, Bottom = 600, Right = 600 }),
        Row<AtomAcceptedMediaRange>(() => new() { MediaRange = "application/xml" }, () => new() { MediaRange = "application/atom+xml" }, () => new() { MediaRange = "text/xml" }),
        Row<AtomWorkspace>(() => new() { Title = new AtomTextConstruct("Workspace B") }, () => new() { Title = new AtomTextConstruct("Workspace A") }, () => new() { Title = new AtomTextConstruct("Workspace C") }),
        Row<RsdApplicationInterface>(() => new() { Name = "API B" }, () => new() { Name = "API A" }, () => new() { Name = "API C" }),
        Row<RssCategory>(() => new() { Value = "CategoryB" }, () => new() { Value = "CategoryA" }, () => new() { Value = "CategoryC" }),
        Row<RssChannel>(() => new(new Uri("http://example.com/b"), "Channel B", "Description B"), () => new(new Uri("http://example.com/a"), "Channel A", "Description A"), () => new(new Uri("http://example.com/c"), "Channel C", "Description C")),
        Row<RssCloud>(() => new() { Domain = "cloud-b.example.com" }, () => new() { Domain = "cloud-a.example.com" }, () => new() { Domain = "cloud-c.example.com" }),
        Row<RssEnclosure>(() => new(2000, "audio/mpeg", new Uri("http://example.com/b.mp3")), () => new(1000, "audio/mpeg", new Uri("http://example.com/a.mp3")), () => new(3000, "audio/mpeg", new Uri("http://example.com/c.mp3"))),
        Row<RssGuid>(() => new() { Value = "guid-b" }, () => new() { Value = "guid-a" }, () => new() { Value = "guid-c" }),
        Row<RssImage>(() => new(new Uri("http://example.com/b.png"), "Image B", new Uri("http://example.com/b")), () => new(new Uri("http://example.com/a.png"), "Image A", new Uri("http://example.com/a")), () => new(new Uri("http://example.com/c.png"), "Image C", new Uri("http://example.com/c"))),
        Row<RssItem>(() => new() { Title = "Item B" }, () => new() { Title = "Item A" }, () => new() { Title = "Item C" }),
        Row<RssSource>(() => new() { Title = "Source B" }, () => new() { Title = "Source A" }, () => new() { Title = "Source C" }),
        Row<RssTextInput>(() => new() { Title = "Input B" }, () => new() { Title = "Input A" }, () => new() { Title = "Input C" }),
        Row<SimpleListGroup>(() => new() { Element = "group-b" }, () => new() { Element = "group-a" }, () => new() { Element = "group-c" }),
        Row<SimpleListSort>(() => new() { Element = "sort-b" }, () => new() { Element = "sort-a" }, () => new() { Element = "sort-c" }),
        Row<SitemapHreflangLink>(() => new() { Hreflang = "en-US", Href = new Uri("http://example.com/en-us") }, () => new() { Hreflang = "de-DE", Href = new Uri("http://example.com/de-de") }, () => new() { Hreflang = "fr-FR", Href = new Uri("http://example.com/fr-fr") }),
        Row<SitemapImage>(() => new(new Uri("http://example.com/image-b.png")), () => new(new Uri("http://example.com/image-a.png")), () => new(new Uri("http://example.com/image-c.png"))),
        Row<SitemapIndexEntry>(() => new() { Location = new Uri("http://example.com/sitemap-b.xml") }, () => new() { Location = new Uri("http://example.com/sitemap-a.xml") }, () => new() { Location = new Uri("http://example.com/sitemap-c.xml") }),
        Row<SitemapNewsPublication>(() => new() { Name = "Publication B", Language = "en" }, () => new() { Name = "Publication A", Language = "en" }, () => new() { Name = "Publication C", Language = "en" }),
        Row<SitemapUrl>(() => new() { Location = new Uri("http://example.com/page-b") }, () => new() { Location = new Uri("http://example.com/page-a") }, () => new() { Location = new Uri("http://example.com/page-c") }),
        Row<SitemapVideo>(() => new() { Title = "Video B", ThumbnailLocation = new Uri("http://example.com/thumb-b.jpg") }, () => new() { Title = "Video A", ThumbnailLocation = new Uri("http://example.com/thumb-a.jpg") }, () => new() { Title = "Video C", ThumbnailLocation = new Uri("http://example.com/thumb-c.jpg") }),
        Row<SitemapVideoId>(() => new() { Value = "video-id-b" }, () => new() { Value = "video-id-a" }, () => new() { Value = "video-id-c" }),
        Row<SitemapVideoSegment>(() => new() { Location = new Uri("http://example.com/segment-b.mp4") }, () => new() { Location = new Uri("http://example.com/segment-a.mp4") }, () => new() { Location = new Uri("http://example.com/segment-c.mp4") }),
        Row<SiteSummaryContentItem>(() => new() { Content = "Content B" }, () => new() { Content = "Content A" }, () => new() { Content = "Content C" }),
        Row<YahooMediaCategory>(() => new() { Content = "category-b" }, () => new() { Content = "category-a" }, () => new() { Content = "category-c" }),
        Row<YahooMediaContent>(() => new() { Url = new Uri("http://example.com/media-b.mp4") }, () => new() { Url = new Uri("http://example.com/media-a.mp4") }, () => new() { Url = new Uri("http://example.com/media-c.mp4") }),
        Row<YahooMediaCopyright>(() => new() { Text = "Copyright B" }, () => new() { Text = "Copyright A" }, () => new() { Text = "Copyright C" }),
        Row<YahooMediaCredit>(() => new() { Entity = "Credit B" }, () => new() { Entity = "Credit A" }, () => new() { Entity = "Credit C" }),
        Row<YahooMediaHash>(() => new() { Value = "hashB123" }, () => new() { Value = "hashA123" }, () => new() { Value = "hashC123" }),
        Row<YahooMediaPlayer>(() => new() { Url = new Uri("http://example.com/player-b") }, () => new() { Url = new Uri("http://example.com/player-a") }, () => new() { Url = new Uri("http://example.com/player-c") }),
        Row<YahooMediaRating>(() => new() { Content = "nonadult" }, () => new() { Content = "adult" }, () => new() { Content = "simple" }),
        Row<YahooMediaTextConstruct>(() => new() { Content = "Construct B" }, () => new() { Content = "Construct A" }, () => new() { Content = "Construct C" }),
        Row<YahooMediaText>(() => new() { Content = "Text B" }, () => new() { Content = "Text A" }, () => new() { Content = "Text C" }),
        Row<YahooMediaThumbnail>(() => new() { Url = new Uri("http://example.com/thumb-b.jpg") }, () => new() { Url = new Uri("http://example.com/thumb-a.jpg") }, () => new() { Url = new Uri("http://example.com/thumb-c.jpg") }),

        // The four registrations below need more than one expression to build an instance.
        Row<SyndicationResourceMetadata>(
            () => new(MetadataNavigator("""<rss version="2.0"><channel><title>Test B</title></channel></rss>""")),
            () => new(MetadataNavigator("""<feed xmlns="http://www.w3.org/2005/Atom"><title>Test A</title></feed>""")),
            () => new(MetadataNavigator("""<rss version="2.0"><channel><title>Test C</title></channel></rss>"""))),
        Row<TrackbackDiscoveryMetadata>(
            () => new() { About = new Uri("http://example.com/post/2"), Identifier = new Uri("http://example.com/id/2"), PingUrl = new Uri("http://example.com/trackback/2") },
            () => new() { About = new Uri("http://example.com/post/1"), Identifier = new Uri("http://example.com/id/1"), PingUrl = new Uri("http://example.com/trackback/1") },
            () => new() { About = new Uri("http://example.com/post/3"), Identifier = new Uri("http://example.com/id/3"), PingUrl = new Uri("http://example.com/trackback/3") }),
        Row<YahooMediaGroup>(
            () => MediaGroup("http://example.com/b.mp4"),
            () => MediaGroup("http://example.com/a.mp4"),
            () => MediaGroup("http://example.com/c.mp4")),
        Row<YahooMediaRestriction>(
            () => MediaRestriction("us", "gb"),
            () => MediaRestriction("de", "fr"),
            () => MediaRestriction("us", "gb", "ca")),

        // OpmlHead.CompareTo always returns 0 - ordering is not implemented on the type. The file this
        // replaced worked around that by overriding four base tests to assert the opposite outcome. Naming
        // the degeneracy is more honest than inverting the assertions.
        DegenerateRow<OpmlHead>(() => new() { Title = "Head B" }),
    ];

    /// <summary>
    /// Verifies that a type's comparison operators satisfy the contract.
    /// </summary>
    /// <param name="typeName">The name of the type under test, used for the display name.</param>
    /// <param name="verify">The verification closed over the concrete type.</param>
    [TestMethod]
    [DynamicData(nameof(ComparableTypes), DynamicDataDisplayName = nameof(DisplayName))]
    public void ComparisonOperators_SatisfyTheContract(string typeName, Action verify)
    {
        ArgumentNullException.ThrowIfNull(verify);

        typeName.ShouldNotBeNullOrEmpty();
        verify();
    }

    /// <summary>
    /// Builds the display name for a registration row.
    /// </summary>
    /// <param name="methodInfo">The test method.</param>
    /// <param name="data">The row data.</param>
    /// <returns>A display name naming the type under test.</returns>
    public static string DisplayName(System.Reflection.MethodInfo methodInfo, object[] data)
        => $"{methodInfo?.Name} ({data?[0]})";

    /// <summary>
    /// Builds a navigator over the supplied XML, for the registrations that need a parsed document.
    /// </summary>
    /// <param name="xml">The XML to parse.</param>
    /// <returns>A navigator over <paramref name="xml"/>.</returns>
    private static XPathNavigator MetadataNavigator(string xml)
    {
        using StringReader reader = new(xml);
        return new XPathDocument(reader).CreateNavigator();
    }

    /// <summary>
    /// Builds a media group carrying a single content entry.
    /// </summary>
    /// <param name="contentUrl">The URL of the content entry.</param>
    /// <returns>The media group.</returns>
    private static YahooMediaGroup MediaGroup(string contentUrl)
    {
        YahooMediaGroup group = new();
        group.Contents.Add(new YahooMediaContent { Url = new Uri(contentUrl) });
        return group;
    }

    /// <summary>
    /// Builds a media restriction carrying the supplied entities.
    /// </summary>
    /// <param name="entities">The entities the restriction applies to.</param>
    /// <returns>The media restriction.</returns>
    private static YahooMediaRestriction MediaRestriction(params string[] entities)
    {
        YahooMediaRestriction restriction = new();
        foreach (string entity in entities)
        {
            restriction.Entities.Add(entity);
        }

        return restriction;
    }

    /// <summary>
    /// Registers a type whose <see cref="IComparable{T}.CompareTo"/> imposes a real ordering.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="create">Creates the baseline instance.</param>
    /// <param name="lesser">Creates an instance that compares less than <paramref name="greater"/>.</param>
    /// <param name="greater">Creates an instance that compares greater than <paramref name="lesser"/>.</param>
    /// <returns>A registration row.</returns>
    private static object[] Row<T>(Func<T> create, Func<T> lesser, Func<T> greater)
        where T : class, IComparable<T>, IComparisonOperators
        => [typeof(T).Name, () => VerifyOrdered(create, lesser, greater)];

    /// <summary>
    /// Registers a type whose <see cref="IComparable{T}.CompareTo"/> always returns zero.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="create">Creates an instance.</param>
    /// <returns>A registration row.</returns>
    private static object[] DegenerateRow<T>(Func<T> create)
        where T : class, IComparable<T>, IComparisonOperators
        => [typeof(T).Name, () => VerifyDegenerate(create)];

    /// <summary>
    /// Asserts the full ordering contract.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="create">Creates the baseline instance.</param>
    /// <param name="lesserFactory">Creates the lesser instance.</param>
    /// <param name="greaterFactory">Creates the greater instance.</param>
    private static void VerifyOrdered<T>(Func<T> create, Func<T> lesserFactory, Func<T> greaterFactory)
        where T : class, IComparable<T>, IComparisonOperators
    {
        T lesser = lesserFactory();
        T greater = greaterFactory();
        T first = create();
        T second = create();
        T? none = null;
        T? alsoNone = null;

        (lesser < greater).ShouldBeTrue($"{typeof(T).Name}: lesser < greater");
        (greater < lesser).ShouldBeFalse($"{typeof(T).Name}: greater < lesser");
        (first < second).ShouldBeFalse($"{typeof(T).Name}: equal instances are not <");
        (none < first).ShouldBeTrue($"{typeof(T).Name}: null < instance");
        (first < none).ShouldBeFalse($"{typeof(T).Name}: instance < null");

        (greater > lesser).ShouldBeTrue($"{typeof(T).Name}: greater > lesser");
        (lesser > greater).ShouldBeFalse($"{typeof(T).Name}: lesser > greater");
        (first > second).ShouldBeFalse($"{typeof(T).Name}: equal instances are not >");
        (none > first).ShouldBeFalse($"{typeof(T).Name}: null > instance");
        (first > none).ShouldBeTrue($"{typeof(T).Name}: instance > null");

        (lesser <= greater).ShouldBeTrue($"{typeof(T).Name}: lesser <= greater");
        (first <= second).ShouldBeTrue($"{typeof(T).Name}: equal instances are <=");
        (greater <= lesser).ShouldBeFalse($"{typeof(T).Name}: greater <= lesser");
        (none <= first).ShouldBeTrue($"{typeof(T).Name}: null <= instance");
        (none <= alsoNone).ShouldBeTrue($"{typeof(T).Name}: null <= null");

        (greater >= lesser).ShouldBeTrue($"{typeof(T).Name}: greater >= lesser");
        (first >= second).ShouldBeTrue($"{typeof(T).Name}: equal instances are >=");
        (lesser >= greater).ShouldBeFalse($"{typeof(T).Name}: lesser >= greater");
        (first >= none).ShouldBeTrue($"{typeof(T).Name}: instance >= null");
        (none >= alsoNone).ShouldBeTrue($"{typeof(T).Name}: null >= null");

        EqualityOperator(lesser, greater).ShouldBeFalse($"{typeof(T).Name}: unequal instances are not ==");
        InequalityOperator(lesser, greater).ShouldBeTrue($"{typeof(T).Name}: unequal instances are !=");

        VerifyEquality(first, second, none, alsoNone);
    }

    /// <summary>
    /// Asserts the contract for a type whose comparison always reports equality.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="create">Creates an instance.</param>
    private static void VerifyDegenerate<T>(Func<T> create)
        where T : class, IComparable<T>, IComparisonOperators
    {
        T first = create();
        T second = create();
        T? none = null;
        T? alsoNone = null;

        (first < second).ShouldBeFalse($"{typeof(T).Name}: CompareTo returns 0, so < is never true");
        (first > second).ShouldBeFalse($"{typeof(T).Name}: CompareTo returns 0, so > is never true");
        (first <= second).ShouldBeTrue($"{typeof(T).Name}: CompareTo returns 0, so <= is always true");
        (first >= second).ShouldBeTrue($"{typeof(T).Name}: CompareTo returns 0, so >= is always true");

        (none < first).ShouldBeTrue($"{typeof(T).Name}: null < instance");
        (first < none).ShouldBeFalse($"{typeof(T).Name}: instance < null");
        (none > first).ShouldBeFalse($"{typeof(T).Name}: null > instance");
        (first > none).ShouldBeTrue($"{typeof(T).Name}: instance > null");
        (none <= first).ShouldBeTrue($"{typeof(T).Name}: null <= instance");
        (first >= none).ShouldBeTrue($"{typeof(T).Name}: instance >= null");
        (none <= alsoNone).ShouldBeTrue($"{typeof(T).Name}: null <= null");
        (none >= alsoNone).ShouldBeTrue($"{typeof(T).Name}: null >= null");

        VerifyEquality(first, second, none, alsoNone);
    }

    /// <summary>
    /// Asserts the equality half of the contract, which the 72 files this replaced never covered.
    /// </summary>
    /// <remarks>
    ///     The four ordering operators come from the C# 14 extension block in
    ///     <see cref="ComparisonOperatorExtensions"/> and route through <see cref="IComparable{T}.CompareTo"/>,
    ///     which is why <c>CompareTo</c> was already fully covered while <c>op_Equality</c>,
    ///     <c>Equals(object)</c> and <c>GetHashCode</c> sat at zero on every type. Predefined reference
    ///     equality beats an extension operator, so <c>==</c> and <c>!=</c> cannot be supplied by the
    ///     extension block and each type declares them itself - which is precisely why they need asserting.
    /// </remarks>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="first">An instance.</param>
    /// <param name="second">A separately constructed instance that compares equal to <paramref name="first"/>.</param>
    /// <param name="none">A null reference.</param>
    /// <param name="alsoNone">A second null reference, so no comparison is made to the same variable.</param>
    private static void VerifyEquality<T>(T first, T second, T? none, T? alsoNone)
        where T : class, IComparable<T>, IComparisonOperators
    {
        // Equals and GetHashCode are virtual methods, so these dispatch to the type's own overrides
        // even through a type parameter.
        first.Equals(second).ShouldBeTrue($"{typeof(T).Name}: Equals(T) on equal instances");
        first.Equals((object)second).ShouldBeTrue($"{typeof(T).Name}: Equals(object) on equal instances");
        first.Equals((object)first).ShouldBeTrue($"{typeof(T).Name}: Equals(object) is reflexive");
        first.Equals(null).ShouldBeFalse($"{typeof(T).Name}: Equals(null)");

        // Exercises the `is T other` pattern in every Equals(object) override.
        first.Equals("a value of an unrelated type").ShouldBeFalse($"{typeof(T).Name}: Equals(object) rejects another type");

        first.GetHashCode().ShouldBe(
            second.GetHashCode(),
            $"{typeof(T).Name}: instances that are Equals must return the same GetHashCode");

        EqualityOperator(first, second).ShouldBeTrue($"{typeof(T).Name}: equal instances are ==");
        InequalityOperator(first, second).ShouldBeFalse($"{typeof(T).Name}: equal instances are not !=");
        EqualityOperator(first, none).ShouldBeFalse($"{typeof(T).Name}: instance == null");
        EqualityOperator(none, first).ShouldBeFalse($"{typeof(T).Name}: null == instance");
        InequalityOperator(first, none).ShouldBeTrue($"{typeof(T).Name}: instance != null");
        EqualityOperator(none, alsoNone).ShouldBeTrue($"{typeof(T).Name}: null == null");
        InequalityOperator(none, alsoNone).ShouldBeFalse($"{typeof(T).Name}: null is not != null");
    }

    /// <summary>
    /// Invokes the type's declared <c>operator ==</c>.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the type's equality operator.</returns>
    private static bool EqualityOperator<T>(T? left, T? right)
        where T : class, IComparable<T>, IComparisonOperators
        => InvokeOperator("op_Equality", left, right);

    /// <summary>
    /// Invokes the type's declared <c>operator !=</c>.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the type's inequality operator.</returns>
    private static bool InequalityOperator<T>(T? left, T? right)
        where T : class, IComparable<T>, IComparisonOperators
        => InvokeOperator("op_Inequality", left, right);

    /// <summary>
    /// Invokes a declared equality operator by reflection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Reflection is not incidental here, it is required. Writing <c>left == right</c> inside a generic
    ///     method binds to <i>predefined reference equality</i>, not to the type's own operator: user-defined
    ///     operators are not resolved through a type parameter. The four ordering operators do work through
    ///     the constraint, because they come from the C# 14 extension block in
    ///     <see cref="ComparisonOperatorExtensions"/> - and the reason that block cannot supply <c>==</c> and
    ///     <c>!=</c> is the same rule. So these two operators cannot be reached generically at all.
    ///     </para>
    ///     <para>
    ///     The lookup doubles as an assertion that the operator is declared. A type that implements
    ///     <see cref="IComparisonOperators"/> and forgets <c>==</c> silently gets reference equality, which
    ///     is exactly the defect worth catching.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="name">The operator's metadata name.</param>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the operator.</returns>
    private static bool InvokeOperator<T>(string name, T? left, T? right)
        where T : class, IComparable<T>, IComparisonOperators
    {
        MethodInfo? op = typeof(T).GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(T), typeof(T)],
            modifiers: null);

        op.ShouldNotBeNull(
            $"{typeof(T).Name} implements {nameof(IComparisonOperators)} but does not declare {name}. " +
            "The extension block cannot supply it - predefined reference equality wins - so the type must.");

        return (bool)op.Invoke(null, [left, right])!;
    }
}