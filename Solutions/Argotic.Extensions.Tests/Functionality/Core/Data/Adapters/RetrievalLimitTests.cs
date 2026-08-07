using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Syndication;

using Shouldly;

using SitemapResource = Argotic.Syndication.Sitemap;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> as one contract across every
/// collection that honours it, rather than one adapter at a time.
/// </summary>
/// <remarks>
///     <para>
///     The contract is stated at the property itself: reading the first ten items of a thousand-item feed
///     costs ten items rather than a thousand. The budget is therefore counted in entities <i>kept</i>, and
///     each document below opens with an element that cannot be loaded, so a walk that counts elements
///     seen instead answers one where the caller asked for two.
///     </para>
///     <para>
///     The Sitemap limits are exercised twice — through <c>Sitemap.Load</c> and <c>SitemapIndex.Load</c>
///     as well as through <see cref="Sitemap09SyndicationResourceAdapter"/> directly. The resources route
///     their loads through the dispatcher into that adapter, so both spellings reach the same walk, and
///     keeping both pins the convergence.
///     </para>
/// </remarks>
[TestClass]
public class RetrievalLimitTests
{
    private const string SitemapWithAnUnloadableUrl = """
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            <url></url>
            <url><loc>http://example.com/1</loc></url>
            <url><loc>http://example.com/2</loc></url>
            <url><loc>http://example.com/3</loc></url>
        </urlset>
        """;

    private const string SitemapIndexWithAnUnloadableEntry = """
        <?xml version="1.0" encoding="UTF-8"?>
        <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            <sitemap></sitemap>
            <sitemap><loc>http://example.com/sitemap1.xml</loc></sitemap>
            <sitemap><loc>http://example.com/sitemap2.xml</loc></sitemap>
            <sitemap><loc>http://example.com/sitemap3.xml</loc></sitemap>
        </sitemapindex>
        """;

    private const string Rss20WithAnUnloadableItem = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
            <channel>
                <title>Test Channel</title>
                <link>http://example.com</link>
                <description>A test channel.</description>
                <item></item>
                <item><title>Item 1</title></item>
                <item><title>Item 2</title></item>
                <item><title>Item 3</title></item>
            </channel>
        </rss>
        """;

    private const string OpmlWithAnUnloadableOutline = """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
            <head><title>Test Outline</title></head>
            <body>
                <outline/>
                <outline text="Outline 1"/>
                <outline text="Outline 2"/>
                <outline text="Outline 3"/>
            </body>
        </opml>
        """;

    private const string Rsd10WithAnUnloadableApi = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
            <service>
                <engineName>Test Blog Engine</engineName>
                <engineLink>http://example.com/engine</engineLink>
                <homePageLink>http://example.com</homePageLink>
                <apis>
                    <api/>
                    <api name="Api1" preferred="true" apiLink="http://example.com/1"/>
                    <api name="Api2" preferred="false" apiLink="http://example.com/2"/>
                    <api name="Api3" preferred="false" apiLink="http://example.com/3"/>
                </apis>
            </service>
        </rsd>
        """;

    private const string ApmlWithAnUnloadableProfile = """
        <?xml version="1.0" encoding="UTF-8"?>
        <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
            <Head><Title>Test Profile</Title></Head>
            <Body defaultprofile="Profile1">
                <Profile/>
                <Profile name="Profile1"/>
                <Profile name="Profile2"/>
                <Profile name="Profile3"/>
                <Applications>
                    <Application name="Application1"/>
                    <Application name="Application2"/>
                    <Application name="Application3"/>
                </Applications>
            </Body>
        </APML>
        """;

    private const string BlogMLWithAnUnloadablePost = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com" date-created="2025-01-01T00:00:00">
            <title>Test Blog</title>
            <authors>
                <author id="author1"><title>Author 1</title></author>
                <author id="author2"><title>Author 2</title></author>
                <author id="author3"><title>Author 3</title></author>
            </authors>
            <categories>
                <category id="category1"><title>Category 1</title></category>
                <category id="category2"><title>Category 2</title></category>
                <category id="category3"><title>Category 3</title></category>
            </categories>
            <posts>
                <post/>
                <post id="post1"><title>Post 1</title></post>
                <post id="post2"><title>Post 2</title></post>
                <post id="post3"><title>Post 3</title></post>
            </posts>
        </blog>
        """;

    private const string Atom10WithFourEntries = """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <title>Test Feed</title>
            <id>urn:uuid:test</id>
            <updated>2025-01-20T12:00:00Z</updated>
            <entry><title>Entry 1</title><id>urn:uuid:entry-1</id><updated>2025-01-20T12:00:00Z</updated></entry>
            <entry><title>Entry 2</title><id>urn:uuid:entry-2</id><updated>2025-01-20T12:00:00Z</updated></entry>
            <entry><title>Entry 3</title><id>urn:uuid:entry-3</id><updated>2025-01-20T12:00:00Z</updated></entry>
            <entry><title>Entry 4</title><id>urn:uuid:entry-4</id><updated>2025-01-20T12:00:00Z</updated></entry>
        </feed>
        """;

    private const string Rss091WithFourItems = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="0.91">
            <channel>
                <title>Test Channel</title>
                <link>http://example.com</link>
                <description>A test channel.</description>
                <language>en-us</language>
                <item><title>Item 1</title><link>http://example.com/1</link><description>One</description></item>
                <item><title>Item 2</title><link>http://example.com/2</link><description>Two</description></item>
                <item><title>Item 3</title><link>http://example.com/3</link><description>Three</description></item>
                <item><title>Item 4</title><link>http://example.com/4</link><description>Four</description></item>
            </channel>
        </rss>
        """;

    private static XPathNavigator NavigatorFor(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        XPathDocument document = new(stream);

        return document.CreateNavigator();
    }

    private static SyndicationResourceLoadSettings LimitOf(int limit) => new() { RetrievalLimit = limit };

    /// <summary>
    /// A limit of two on a <c>urlset</c> whose first <c>url</c> cannot be loaded yields two urls.
    /// </summary>
    [TestMethod]
    public void Sitemap09Adapter_WithALimitOfTwoAndAnUnloadableUrl_YieldsTwoUrls()
    {
        // Arrange
        SitemapResource sitemap = new();
        Sitemap09SyndicationResourceAdapter adapter = new(NavigatorFor(SitemapWithAnUnloadableUrl), LimitOf(2));

        // Act
        adapter.Fill(sitemap);

        // Assert
        sitemap.Urls.Count.ShouldBe(2);
    }

    /// <summary>
    /// The live <c>Sitemap.Load</c> path answers the same, and it is the one a consumer reaches.
    /// </summary>
    [TestMethod]
    public void SitemapLoad_WithALimitOfTwoAndAnUnloadableUrl_YieldsTwoUrls()
    {
        // Arrange
        SitemapResource sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(SitemapWithAnUnloadableUrl));

        // Act
        sitemap.Load(stream, LimitOf(2));

        // Assert
        sitemap.Urls.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on a <c>sitemapindex</c> whose first <c>sitemap</c> cannot be loaded yields two
    /// entries.
    /// </summary>
    [TestMethod]
    public void Sitemap09Adapter_WithALimitOfTwoAndAnUnloadableEntry_YieldsTwoSitemaps()
    {
        // Arrange
        SitemapIndex index = new();
        Sitemap09SyndicationResourceAdapter adapter = new(NavigatorFor(SitemapIndexWithAnUnloadableEntry), LimitOf(2));

        // Act
        adapter.Fill(index);

        // Assert
        index.Sitemaps.Count.ShouldBe(2);
    }

    /// <summary>
    /// The live <c>SitemapIndex.Load</c> path answers the same.
    /// </summary>
    [TestMethod]
    public void SitemapIndexLoad_WithALimitOfTwoAndAnUnloadableEntry_YieldsTwoSitemaps()
    {
        // Arrange
        SitemapIndex index = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(SitemapIndexWithAnUnloadableEntry));

        // Act
        index.Load(stream, LimitOf(2));

        // Assert
        index.Sitemaps.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on an RSS 2.0 channel whose first <c>item</c> cannot be loaded yields two items.
    /// </summary>
    [TestMethod]
    public void Rss20Channel_WithALimitOfTwoAndAnUnloadableItem_YieldsTwoItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Rss20WithAnUnloadableItem));

        // Act
        feed.Load(stream, LimitOf(2));

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on an OPML body whose first <c>outline</c> cannot be loaded yields two outlines.
    /// </summary>
    [TestMethod]
    public void Opml20Adapter_WithALimitOfTwoAndAnUnloadableOutline_YieldsTwoOutlines()
    {
        // Arrange
        OpmlDocument document = new();
        Opml20SyndicationResourceAdapter adapter = new(NavigatorFor(OpmlWithAnUnloadableOutline), LimitOf(2));

        // Act
        adapter.Fill(document);

        // Assert
        document.Outlines.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on an RSD service whose first <c>api</c> cannot be loaded yields two interfaces.
    /// </summary>
    [TestMethod]
    public void Rsd10Adapter_WithALimitOfTwoAndAnUnloadableApi_YieldsTwoInterfaces()
    {
        // Arrange
        RsdDocument document = new();
        Rsd10SyndicationResourceAdapter adapter = new(NavigatorFor(Rsd10WithAnUnloadableApi), LimitOf(2));

        // Act
        adapter.Fill(document);

        // Assert
        document.Interfaces.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on an APML body whose first <c>Profile</c> cannot be loaded yields two profiles.
    /// </summary>
    [TestMethod]
    public void Apml06Adapter_WithALimitOfTwoAndAnUnloadableProfile_YieldsTwoProfiles()
    {
        // Arrange
        ApmlDocument document = new();
        Apml06SyndicationResourceAdapter adapter = new(NavigatorFor(ApmlWithAnUnloadableProfile), LimitOf(2));

        // Act
        adapter.Fill(document);

        // Assert
        document.Profiles.Count.ShouldBe(2);
    }

    /// <summary>
    /// The same limit reaches <c>Applications</c>, which used to be read in full however long the list was.
    /// </summary>
    [TestMethod]
    public void Apml06Adapter_WithALimitOfTwo_CapsApplicationsAsWell()
    {
        // Arrange
        ApmlDocument document = new();
        Apml06SyndicationResourceAdapter adapter = new(NavigatorFor(ApmlWithAnUnloadableProfile), LimitOf(2));

        // Act
        adapter.Fill(document);

        // Assert
        document.Applications.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on a BlogML export whose first <c>post</c> cannot be loaded yields two posts.
    /// </summary>
    [TestMethod]
    public void BlogML20Adapter_WithALimitOfTwoAndAnUnloadablePost_YieldsTwoPosts()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogML20SyndicationResourceAdapter adapter = new(NavigatorFor(BlogMLWithAnUnloadablePost), LimitOf(2));

        // Act
        adapter.Fill(document);

        // Assert
        document.Posts.Count.ShouldBe(2);
    }

    /// <summary>
    /// The same limit reaches <c>authors</c> and <c>categories</c>, which used to be read in full however
    /// long the export was.
    /// </summary>
    [TestMethod]
    public void BlogML20Adapter_WithALimitOfTwo_CapsAuthorsAndCategoriesAsWell()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogML20SyndicationResourceAdapter adapter = new(NavigatorFor(BlogMLWithAnUnloadablePost), LimitOf(2));

        // Act
        adapter.Fill(document);

        // Assert
        document.Authors.Count.ShouldBe(2);
        document.Categories.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on a four-entry Atom 1.0 feed yields two entries.
    /// </summary>
    /// <remarks>
    ///     A guard rather than a characterisation. The Atom walks have no failure branch — every entry
    ///     encountered is added — so moving the limit test to the top of the loop changes only how much
    ///     work is done, not what is produced. Green on both sides.
    /// </remarks>
    [TestMethod]
    public void Atom10Adapter_WithALimitOfTwo_YieldsTwoEntries()
    {
        // Arrange
        AtomFeed feed = new();
        Atom10SyndicationResourceAdapter adapter = new(NavigatorFor(Atom10WithFourEntries), LimitOf(2));

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Entries.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of two on a four-item RSS 0.91 channel yields two items.
    /// </summary>
    /// <remarks>
    ///     A guard, for the same reason: the RSS 0.91 item walk adds unconditionally.
    /// </remarks>
    [TestMethod]
    public void Rss091Adapter_WithALimitOfTwo_YieldsTwoItems()
    {
        // Arrange
        RssFeed feed = new();
        Rss091SyndicationResourceAdapter adapter = new(NavigatorFor(Rss091WithFourItems), LimitOf(2));

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
    }

    /// <summary>
    /// A limit of zero still means no limit, on the collections that are newly bounded as much as on the
    /// ones that always were.
    /// </summary>
    [TestMethod]
    public void EveryCollection_WithALimitOfZero_ReadsEverythingLoadable()
    {
        // Arrange
        ApmlDocument apmlDocument = new();
        BlogMLDocument blogMLDocument = new();
        Apml06SyndicationResourceAdapter apmlAdapter = new(NavigatorFor(ApmlWithAnUnloadableProfile), LimitOf(0));
        BlogML20SyndicationResourceAdapter blogMLAdapter = new(NavigatorFor(BlogMLWithAnUnloadablePost), LimitOf(0));

        // Act
        apmlAdapter.Fill(apmlDocument);
        blogMLAdapter.Fill(blogMLDocument);

        // Assert
        apmlDocument.Profiles.Count.ShouldBe(3);
        apmlDocument.Applications.Count.ShouldBe(3);
        blogMLDocument.Posts.Count.ShouldBe(3);
        blogMLDocument.Authors.Count.ShouldBe(3);
        blogMLDocument.Categories.Count.ShouldBe(3);
    }
}