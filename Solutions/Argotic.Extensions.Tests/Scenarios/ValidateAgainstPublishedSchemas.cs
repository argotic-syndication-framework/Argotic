using System.Xml.Schema;
namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Validates this library's output against the schemas and validators their publishers actually serve.
/// </summary>
/// <remarks>
///     <para>
///     <b>Everything here reaches the network, and is categorised so it can be excluded.</b> The offline
///     gate runs with <c>--filter "TestCategory!=Integration"</c> and stays deterministic; this tier is
///     run explicitly, and covers the three things no offline test can:
///     </para>
///     <list type="bullet">
///     <item>
///     Google's news, image and video sitemap schemas, which carry an "All Rights Reserved" notice and
///     are therefore never committed. Fetching them is what lets the check be against Google's own file
///     rather than a paraphrase — and this is the tier that would have caught
///     <c>SitemapVideo.WriteTo</c> writing <c>tag</c> in a position the schema rejects.
///     </item>
///     <item>
///     The W3C Feed Validator, the canonical conformance checker for RSS and Atom, neither of which has
///     a schema .NET can use.
///     </item>
///     <item>
///     endjin's live feeds, which is the only check that notices the publisher changing what it serves.
///     </item>
///     </list>
///     <para>
///     <b>Unreachable is reported as inconclusive, never as success.</b> A test that goes green without
///     reaching the validator is worse than no test, so every one of these asserts it got an answer
///     before it asserts anything about the answer. That is safe here only because the offline tier
///     already covers everything checkable offline — this tier adds reach, it does not hold the only
///     copy of any check.
///     </para>
/// </remarks>
[TestClass]
[TestCategory("Integration")]
public class ValidateAgainstPublishedSchemas
{
    /// <summary>
    /// GeoRSS issues the W3C validator raises that are its limitation rather than a document defect.
    /// </summary>
    /// <remarks>
    ///     The validator reports <c>georss:box</c> and <c>georss:featurename</c> as undefined elements.
    ///     OGC 17-002r1 defines both — checked against the archived copy — so the documents are correct
    ///     and the validator's GeoRSS vocabulary is incomplete. Excluded by name, and by nothing else.
    /// </remarks>
    private static readonly string[] KnownValidatorGaps = ["UndefinedElement"];

    /// <summary>
    /// Gets the corpus sitemaps that need a Google extension schema, paired with the schemas they need.
    /// </summary>
    /// <remarks>
    ///     A document may carry more than one extension, and the news sample does: endjin's real
    ///     <c>sitemap-news.xml</c> declares the image namespace alongside the news one and puts an
    ///     <c>image:image</c> in every <c>url</c>. Validating it against the news schema alone reports
    ///     the image element as undeclared, which says nothing about the document and everything about
    ///     the schema set — so the schemas are a set per file rather than one each.
    /// </remarks>
    public static IEnumerable<object[]> GoogleExtensionSitemaps =>
    [
        [SampleFeeds.SitemapNews, new[] { LiveSchemaSource.NewsSchemaUrl, LiveSchemaSource.ImageSchemaUrl }],
        [SampleFeeds.SitemapImage, new[] { LiveSchemaSource.ImageSchemaUrl }],
        [SampleFeeds.SitemapVideo, new[] { LiveSchemaSource.VideoSchemaUrl }],
    ];

    /// <summary>
    /// Gets the corpus feeds the W3C Feed Validator can check.
    /// </summary>
    public static IEnumerable<object[]> ValidatableFeeds =>
    [
        [SampleFeeds.RssFeed],
        [SampleFeeds.GenericFeed],
        [SampleFeeds.AtomFeed],
        [SampleFeeds.AtomEntryDocument],
        [SampleFeeds.PodcastFeed],
        [SampleFeeds.RssFeedWithExtensions],
        [SampleFeeds.AtomFeedWithExtensions],
    ];

    /// <summary>
    /// A corpus sitemap carrying a Google extension conforms to Google's published schema.
    /// </summary>
    /// <param name="fileName">The sample document under test.</param>
    /// <param name="schemaUrls">The extension schemas it needs.</param>
    [TestMethod]
    [DynamicData(nameof(GoogleExtensionSitemaps))]
    public void ACorpusSitemap_ConformsToGooglesPublishedSchema(string fileName, string[] schemaUrls)
    {
        ArgumentNullException.ThrowIfNull(schemaUrls);

        XmlSchemaSet schemas = FetchSchemasOrSkip(schemaUrls);

        ConformanceSchemas.Describe(SampleFeeds.ReadAllText(fileName), schemas)
            .ShouldBeEmpty($"{fileName} does not conform to Google's published schemas");
    }

    /// <summary>
    /// A corpus sitemap carrying a Google extension still conforms after this library rewrites it.
    /// </summary>
    /// <param name="fileName">The sample document under test.</param>
    /// <param name="schemaUrls">The extension schemas it needs.</param>
    /// <remarks>
    ///     <b>This is the test the video element-order defect needed.</b> The document going in was
    ///     valid, the reader understood it, and the writer emitted it in an order the schema rejects —
    ///     which no round-trip test could see, because the reader accepts any order.
    /// </remarks>
    [TestMethod]
    [DynamicData(nameof(GoogleExtensionSitemaps))]
    public void ACorpusSitemap_StillConformsToGooglesSchemaAfterBeingRewritten(string fileName, string[] schemaUrls)
    {
        ArgumentNullException.ThrowIfNull(schemaUrls);

        XmlSchemaSet schemas = FetchSchemasOrSkip(schemaUrls);

        Sitemap sitemap = new();
        using (FileStream input = SampleFeeds.Open(fileName))
        {
            sitemap.Load(input);
        }

        using MemoryStream saved = new();
        sitemap.Save(saved);

        ConformanceSchemas.Describe(Encoding.UTF8.GetString(saved.ToArray()), schemas)
            .ShouldBeEmpty($"{fileName} conformed to Google's schemas on disk but not after being loaded and saved");
    }

    /// <summary>
    /// A corpus feed is accepted by the W3C Feed Validator.
    /// </summary>
    /// <param name="fileName">The sample document under test.</param>
    [TestMethod]
    [DynamicData(nameof(ValidatableFeeds))]
    public void ACorpusFeed_IsAcceptedByTheW3CFeedValidator(string fileName)
    {
        LiveResult verdict = LiveSchemaSource.ValidateFeed(SampleFeeds.ReadAllText(fileName));

        if (!verdict.Reachable)
        {
            Assert.Inconclusive($"{verdict.Endpoint} could not be reached, so {fileName} was not validated.");
        }

        verdict.ErrorsExcept(KnownValidatorGaps).ShouldBeEmpty(
            $"the W3C Feed Validator rejected {fileName}");
    }

    /// <summary>
    /// A feed this library writes from scratch is accepted by the W3C Feed Validator.
    /// </summary>
    /// <remarks>
    ///     The corpus tests check documents someone authored; this checks one the library composed,
    ///     which is the case a consumer actually depends on.
    /// </remarks>
    [TestMethod]
    public void AFeedThisLibraryWrites_IsAcceptedByTheW3CFeedValidator()
    {
        RssFeed feed = new();
        feed.Channel.Title = "endjin blog";
        feed.Channel.Link = new Uri("https://endjin.com/blog/");
        feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";

        feed.Channel.Items.Add(new RssItem
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            Description = "Moving UI framework support out of System.Reactive can cut 95MB from a deployment.",
            PublicationDate = new DateTime(2026, 7, 29, 9, 0, 0, DateTimeKind.Utc),
            Guid = new RssGuid("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released", true),
        });

        using MemoryStream saved = new();
        feed.Save(saved);

        LiveResult verdict = LiveSchemaSource.ValidateFeed(Encoding.UTF8.GetString(saved.ToArray()));

        if (!verdict.Reachable)
        {
            Assert.Inconclusive($"{verdict.Endpoint} could not be reached, so the written feed was not validated.");
        }

        verdict.ErrorsExcept(KnownValidatorGaps).ShouldBeEmpty(
            "the W3C Feed Validator rejected a feed this library composed");
    }

    /// <summary>
    /// endjin's live RSS feed still loads through this library.
    /// </summary>
    /// <remarks>
    ///     The 19 remaining <c>[RequiresNetwork]</c> examples exercise this informally; nothing asserted
    ///     it until now. It is also the only check that would notice endjin changing what it publishes.
    /// </remarks>
    [TestMethod]
    public void EndjinsLiveRssFeed_LoadsThroughThisLibrary()
    {
        string? document = LiveSchemaSource.TryFetch("https://endjin.com/rss.xml");

        if (document is null)
        {
            Assert.Inconclusive("endjin.com/rss.xml could not be reached.");
        }

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document));
        feed.Load(stream);

        feed.Channel.Title.ShouldNotBeNullOrEmpty();
        feed.Channel.Items.ShouldNotBeEmpty("endjin's feed carried no items");
    }

    /// <summary>
    /// endjin's live Atom feed still loads through this library.
    /// </summary>
    [TestMethod]
    public void EndjinsLiveAtomFeed_LoadsThroughThisLibrary()
    {
        string? document = LiveSchemaSource.TryFetch("https://endjin.com/atom.xml");

        if (document is null)
        {
            Assert.Inconclusive("endjin.com/atom.xml could not be reached.");
        }

        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document));
        feed.Load(stream);

        feed.Title?.Content.ShouldNotBeNullOrEmpty();
        feed.Entries.ShouldNotBeEmpty("endjin's Atom feed carried no entries");
    }

    /// <summary>
    /// Every live endjin sitemap loads through this library.
    /// </summary>
    /// <param name="url">The sitemap to fetch.</param>
    /// <remarks>
    ///     A library concern, and therefore a hard assertion: whether Argotic can read what a real
    ///     publisher serves is this repository's problem and nobody else's. Whether the document
    ///     <i>conforms</i> is a different question with a different owner, and lives in
    ///     <see cref="EndjinSitePublishesConformantSitemaps"/>.
    /// </remarks>
    [TestMethod]
    [DataRow("https://endjin.com/sitemap.xml")]
    [DataRow("https://endjin.com/sitemap-news.xml")]
    [DataRow("https://endjin.com/sitemap-video.xml")]
    public void AnEndjinLiveSitemap_LoadsThroughThisLibrary(string url)
    {
        string? document = LiveSchemaSource.TryFetch(url);

        if (document is null)
        {
            Assert.Inconclusive($"{url} could not be reached.");
        }

        Sitemap sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document));
        sitemap.Load(stream);

        sitemap.Urls.ShouldNotBeEmpty($"{url} carried no urls");
    }

    /// <summary>
    /// The W3C Feed Validator rejects a feed that is genuinely wrong.
    /// </summary>
    /// <remarks>
    ///     <b>The check on the checker.</b> Every other validator test asserts an empty error list, so
    ///     all of them would pass if the response were misparsed, the endpoint changed its contract, or
    ///     the exclusion list swallowed everything. A channel missing its mandatory description must come
    ///     back rejected; if it does not, the rest of this class proves nothing.
    /// </remarks>
    [TestMethod]
    public void AnInvalidFeed_IsRejectedByTheW3CFeedValidator()
    {
        const string missingDescription = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <title>endjin blog</title>
                <link>https://endjin.com/blog/</link>
              </channel>
            </rss>
            """;

        LiveResult verdict = LiveSchemaSource.ValidateFeed(missingDescription);

        if (!verdict.Reachable)
        {
            Assert.Inconclusive($"{verdict.Endpoint} could not be reached.");
        }

        verdict.IsValid.ShouldBeFalse("the validator accepted an RSS channel with no description");
        verdict.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Builds a schema set from the embedded sitemap schemas plus the fetched extension schemas.
    /// </summary>
    /// <param name="schemaUrls">The extension schemas to fetch.</param>
    /// <returns>A compiled schema set.</returns>
    private static XmlSchemaSet FetchSchemasOrSkip(params string[] schemaUrls)
    {
        List<string> fetched = [];

        foreach (string url in schemaUrls)
        {
            string? schema = LiveSchemaSource.TryFetch(url);

            if (schema is null)
            {
                Assert.Inconclusive($"{url} could not be fetched, so nothing was validated against it.");
            }

            fetched.Add(schema);
        }

        return ConformanceSchemas.SitemapWith([.. fetched]);
    }
}