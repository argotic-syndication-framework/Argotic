using Argotic.Data.Adapters;
using Argotic.Publishing;
namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers how <see cref="SyndicationResourceAdapter.Fill(ISyndicationResource, SyndicationContentFormat)"/>
/// answers documents whose declared version its routing does not recognise, and resources whose runtime
/// type cannot read the requested format.
/// </summary>
/// <remarks>
///     <para>
///     Version routing answers every input. For the formats whose specifications define a version
///     attribute — RSS, OPML, APML, RSD — a declared version no adapter reads is refused with a
///     <see cref="FormatException"/> naming the version found and the versions read, and the comparison
///     ignores build and revision components, so <c>2.0.1</c> reaches the 2.0 adapter. Atom, the Atom
///     Publishing Protocol and BlogML define no version attribute at all, so an unrecognised value there
///     is foreign markup and the namespace decides the route. A resource of the wrong runtime type is
///     refused with an <see cref="ArgumentException"/> naming both types, and a format the detection
///     recognises but no adapter reads is refused rather than ignored.
///     </para>
///     <para>
///     Two pins survive from the characterisation round unchanged: the format-mismatch message, and the
///     rss-root-claiming-an-RDF-version silence, which is the adapter's boundary rather than the
///     routing's.
///     </para>
/// </remarks>
[TestClass]
public class SyndicationResourceAdapterRoutingTests
{
    private const string Rss093WithATitledChannel = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="0.93">
            <channel>
                <title>An Unread Title</title>
                <link>http://example.com</link>
                <description>A channel the routing declines to read.</description>
                <item><title>Item 1</title><link>http://example.com/1</link></item>
            </channel>
        </rss>
        """;

    private const string Rss201WithATitledChannel = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0.1">
            <channel>
                <title>A Title</title>
                <link>http://example.com</link>
                <description>A channel read under build-component tolerance.</description>
                <item><title>Item 1</title><link>http://example.com/1</link></item>
            </channel>
        </rss>
        """;

    private const string Rss10VersionOnAnRssRoot = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="1.0">
            <channel>
                <title>An Unread Title</title>
                <link>http://example.com</link>
                <description>A plain-XML channel claiming an RDF version.</description>
                <item><title>Item 1</title><link>http://example.com/1</link></item>
            </channel>
        </rss>
        """;

    private const string Rss20WithATitledChannel = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
            <channel>
                <title>A Title</title>
                <link>http://example.com</link>
                <description>A channel.</description>
            </channel>
        </rss>
        """;

    private const string Opml15WithAnOutline = """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="1.5">
            <head><title>An Unread Outline</title></head>
            <body><outline text="Outline 1"/></body>
        </opml>
        """;

    private const string Opml201WithAnOutline = """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0.1">
            <head><title>An Outline</title></head>
            <body><outline text="Outline 1"/></body>
        </opml>
        """;

    private const string Opml20WithAnOutline = """
        <?xml version="1.0" encoding="UTF-8"?>
        <opml version="2.0">
            <head><title>An Outline</title></head>
            <body><outline text="Outline 1"/></body>
        </opml>
        """;

    private const string Apml05WithAProfile = """
        <?xml version="1.0" encoding="UTF-8"?>
        <APML xmlns="http://www.apml.org/apml-0.6" version="0.5">
            <Head><Title>An Unread Profile</Title></Head>
            <Body defaultprofile="Profile1">
                <Profile name="Profile1"/>
            </Body>
        </APML>
        """;

    private const string Rsd051WithAnApi = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rsd version="0.51" xmlns="http://archipelago.phrasewise.com/rsd">
            <service>
                <engineName>An Unread Engine</engineName>
                <engineLink>http://example.com/engine</engineLink>
                <homePageLink>http://example.com</homePageLink>
                <apis>
                    <api name="Api1" preferred="true" apiLink="http://example.com/1"/>
                </apis>
            </service>
        </rsd>
        """;

    private const string Atom10ClaimingVersion05 = """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" version="0.5">
            <title>A Feed</title>
            <id>urn:uuid:d3a6b1c0-0000-0000-0000-000000000001</id>
            <updated>2025-01-20T12:00:00Z</updated>
            <entry><title>Entry 1</title><id>urn:uuid:entry-1</id><updated>2025-01-20T12:00:00Z</updated></entry>
        </feed>
        """;

    private const string Atom10WithATitle = """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
            <title>A Feed</title>
            <id>urn:uuid:d3a6b1c0-0000-0000-0000-000000000002</id>
            <updated>2025-01-20T12:00:00Z</updated>
        </feed>
        """;

    private const string BlogML10WithAPost = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" version="1.0" root-url="http://example.com" date-created="2025-01-01T00:00:00">
            <title>A Blog</title>
            <posts>
                <post id="post1"><title>Post 1</title></post>
            </posts>
        </blog>
        """;

    private const string AppService05WithAWorkspace = """
        <?xml version="1.0" encoding="UTF-8"?>
        <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom" version="0.5">
            <workspace>
                <atom:title>A Workspace</atom:title>
            </workspace>
        </service>
        """;

    private const string AppCategoriesWithACategory = """
        <?xml version="1.0" encoding="UTF-8"?>
        <app:categories xmlns:app="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
            <atom:category term="animal"/>
        </app:categories>
        """;

    private const string NewsMLDocument = """
        <?xml version="1.0" encoding="UTF-8"?>
        <NewsML version="1.0">
            <NewsItem/>
        </NewsML>
        """;

    private static XPathNavigator NavigatorFor(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        XPathDocument document = new(stream);

        return document.CreateNavigator();
    }

    private static MemoryStream StreamFor(string xml) => new(Encoding.UTF8.GetBytes(xml));

    /// <summary>
    /// An RSS document declaring version 0.93 is refused with a message naming the version found and the
    /// versions read, and <c>Loaded</c> is not raised.
    /// </summary>
    [TestMethod]
    public void AnRss093Document_LoadedThroughRssFeed_IsRefusedNamingTheVersion()
    {
        // Arrange
        RssFeed feed = new();
        int loadedCount = 0;
        feed.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss093WithATitledChannel);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => feed.Load(stream));

        // Assert
        exception.Message.ShouldContain("0.93");
        exception.Message.ShouldContain("2.0");
        feed.Channel.Title.ShouldBe(string.Empty);
        loadedCount.ShouldBe(0);
    }

    /// <summary>
    /// The same document through <see cref="Syndication.GenericSyndicationFeed"/> is refused too, leaving
    /// the feed in its default state.
    /// </summary>
    [TestMethod]
    public void AnRss093Document_LoadedThroughGenericSyndicationFeed_IsRefused()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        using MemoryStream stream = StreamFor(Rss093WithATitledChannel);

        // Act
        Should.Throw<FormatException>(() => feed.Load(stream));

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.None);
        feed.Title.ShouldBe(string.Empty);
        feed.Items.ShouldBeEmpty();
    }

    /// <summary>
    /// An RSS document declaring version 2.0.1 fills through the 2.0 adapter — routing ignores build and
    /// revision components, so the version the RSS Advisory Board actually publishes under is readable.
    /// </summary>
    [TestMethod]
    public void AnRss201Document_LoadedThroughRssFeed_FillsTheChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = StreamFor(Rss201WithATitledChannel);

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe("A Title");
        feed.Channel.Items.Count.ShouldBe(1);
    }

    /// <summary>
    /// An OPML document declaring version 1.5 is refused with a message naming the version found and the
    /// versions read.
    /// </summary>
    [TestMethod]
    public void AnOpml15Document_LoadedThroughOpmlDocument_IsRefusedNamingTheVersion()
    {
        // Arrange
        OpmlDocument document = new();
        using MemoryStream stream = StreamFor(Opml15WithAnOutline);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => document.Load(stream));

        // Assert
        exception.Message.ShouldContain("1.5");
        exception.Message.ShouldContain("2.0");
        document.Outlines.ShouldBeEmpty();
    }

    /// <summary>
    /// An OPML document declaring version 2.0.1 fills through the 2.0 adapter under the same build-component
    /// tolerance as RSS.
    /// </summary>
    [TestMethod]
    public void AnOpml201Document_LoadedThroughOpmlDocument_FillsTheBody()
    {
        // Arrange
        OpmlDocument document = new();
        using MemoryStream stream = StreamFor(Opml201WithAnOutline);

        // Act
        document.Load(stream);

        // Assert
        document.Outlines.Count.ShouldBe(1);
    }

    /// <summary>
    /// An APML document declaring version 0.5 is refused with a message naming the version found and the
    /// version read.
    /// </summary>
    [TestMethod]
    public void AnApml05Document_LoadedThroughApmlDocument_IsRefusedNamingTheVersion()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = StreamFor(Apml05WithAProfile);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => document.Load(stream));

        // Assert
        exception.Message.ShouldContain("0.5");
        exception.Message.ShouldContain("0.6");
        document.Profiles.ShouldBeEmpty();
    }

    /// <summary>
    /// An RSD document declaring version 0.51 is refused with a message naming the version found and the
    /// versions read.
    /// </summary>
    [TestMethod]
    public void AnRsd051Document_LoadedThroughRsdDocument_IsRefusedNamingTheVersion()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = StreamFor(Rsd051WithAnApi);

        // Act
        FormatException exception = Should.Throw<FormatException>(() => document.Load(stream));

        // Assert
        exception.Message.ShouldContain("0.51");
        exception.Message.ShouldContain("1.0");
        document.Interfaces.ShouldBeEmpty();
    }

    /// <summary>
    /// An Atom 1.0 feed carrying a junk <c>version="0.5"</c> attribute fills as Atom 1.0: RFC 4287 defines
    /// no version attribute on a feed, so the namespace decides and the foreign markup is ignored.
    /// </summary>
    [TestMethod]
    public void AnAtomFeedClaimingVersion05_LoadedThroughAtomFeed_FillsTheFeed()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = StreamFor(Atom10ClaimingVersion05);

        // Act
        feed.Load(stream);

        // Assert
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("A Feed");
        feed.Entries.Count.ShouldBe(1);
    }

    /// <summary>
    /// A BlogML document declaring version 1.0 in the 2.0 namespace fills as 2.0: the BlogML schema admits
    /// no version attribute on the <c>blog</c> root, so the dated namespace decides.
    /// </summary>
    [TestMethod]
    public void ABlogML10Document_LoadedThroughBlogMLDocument_FillsThePosts()
    {
        // Arrange
        BlogMLDocument document = new();
        using MemoryStream stream = StreamFor(BlogML10WithAPost);

        // Act
        document.Load(stream);

        // Assert
        document.Posts.Count.ShouldBe(1);
    }

    /// <summary>
    /// An Atom Publishing Protocol service document carrying a junk <c>version="0.5"</c> attribute fills:
    /// RFC 5023 defines no version attribute, so the protocol namespace alone decides.
    /// </summary>
    [TestMethod]
    public void AnAppServiceDocumentClaimingVersion05_LoadedThroughAtomServiceDocument_FillsTheWorkspaces()
    {
        // Arrange
        AtomServiceDocument document = new();
        using MemoryStream stream = StreamFor(AppService05WithAWorkspace);

        // Act
        document.Load(stream);

        // Assert
        document.Workspaces.Count.ShouldBe(1);
    }

    /// <summary>
    /// A resource whose runtime type cannot read the requested format is refused with an
    /// <see cref="ArgumentException"/> naming both types.
    /// </summary>
    [TestMethod]
    public void AWrongResourceType_HandedToTheDispatcher_IsRefusedWithArgumentException()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(Rss20WithATitledChannel), new SyndicationResourceLoadSettings());

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => adapter.Fill(new OpmlDocument(), SyndicationContentFormat.Rss));

        // Assert
        exception.ParamName.ShouldBe("resource");
        exception.Message.ShouldContain("OpmlDocument");
        exception.Message.ShouldContain("RssFeed");
    }

    /// <summary>
    /// The Atom arm refuses a resource that is neither an <see cref="AtomFeed"/> nor an
    /// <see cref="AtomEntry"/> instead of ignoring it.
    /// </summary>
    [TestMethod]
    public void AnAtomFormat_WithAResourceThatIsNeitherFeedNorEntry_IsRefusedWithArgumentException()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(Atom10WithATitle), new SyndicationResourceLoadSettings());

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => adapter.Fill(new RssFeed(), SyndicationContentFormat.Atom));

        // Assert
        exception.ParamName.ShouldBe("resource");
        exception.Message.ShouldContain("RssFeed");
        exception.Message.ShouldContain("AtomFeed");
    }

    /// <summary>
    /// The Atom Publishing arm refuses a resource that is neither a category document nor a service
    /// document instead of ignoring it.
    /// </summary>
    [TestMethod]
    public void AnAppFormat_WithAResourceThatIsNeitherCategoryNorServiceDocument_IsRefusedWithArgumentException()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(AppCategoriesWithACategory), new SyndicationResourceLoadSettings());

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => adapter.Fill(new RssFeed(), SyndicationContentFormat.AtomCategoryDocument));

        // Assert
        exception.ParamName.ShouldBe("resource");
        exception.Message.ShouldContain("RssFeed");
        exception.Message.ShouldContain("AtomCategoryDocument");
    }

    /// <summary>
    /// A format the detection recognises but no adapter reads is refused rather than ignored.
    /// </summary>
    [TestMethod]
    public void ADetectedButUnreadFormat_HandedToTheDispatcher_IsRefused()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(NewsMLDocument), new SyndicationResourceLoadSettings());

        // Act
        FormatException exception = Should.Throw<FormatException>(() => adapter.Fill(new RssFeed(), SyndicationContentFormat.NewsML));

        // Assert
        exception.Message.ShouldContain("NewsML");
        exception.Message.ShouldContain("does not read");
    }

    /// <summary>
    /// A document of one format handed to a resource expecting another is refused with a message naming
    /// both formats.
    /// </summary>
    /// <remarks>
    ///     A guard, not a characterisation: the format check is the dispatcher's contract and survives the
    ///     routing rewrite verbatim. The Atom feed/entry detail sentence is pinned separately by
    ///     <c>AtomDocumentShapeTests</c>.
    /// </remarks>
    [TestMethod]
    public void AFormatMismatch_NamesBothFormatsInTheMessage()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(Opml20WithAnOutline), new SyndicationResourceLoadSettings());

        // Act
        FormatException exception = Should.Throw<FormatException>(() => adapter.Fill(new RssFeed(), SyndicationContentFormat.Rss));

        // Assert
        exception.Message.ShouldContain("does not match the expected content format");
        exception.Message.ShouldContain("Opml");
        exception.Message.ShouldContain("Rss");
    }

    /// <summary>
    /// A plain-XML <c>rss</c> document claiming an RDF version routes to the RDF adapter, which finds no
    /// <c>rdf:RDF</c> root and fills nothing, silently.
    /// </summary>
    /// <remarks>
    ///     Pins the boundary the routing rewrite deliberately does not move: the version 1.0 arm is taken,
    ///     so this is the adapter's silence, not the dispatcher's. Closing it is an adapter question.
    /// </remarks>
    [TestMethod]
    public void AnRssRootClaimingAnRdfVersion_FillsNothingSilently()
    {
        // Arrange
        RssFeed feed = new();
        int loadedCount = 0;
        feed.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss10VersionOnAnRssRoot);

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe(string.Empty);
        feed.Channel.Items.ShouldBeEmpty();
        loadedCount.ShouldBe(1);
    }

    /// <summary>
    /// An RSS 0.92 document fills through the dispatcher's 0.92 arm.
    /// </summary>
    /// <remarks>
    ///     A reach guard: the 0.92 adapter has direct tests, but nothing else executes this dispatcher arm
    ///     through a public <c>Load</c>. Branch coverage found the arm unexecuted after the switch rewrite.
    /// </remarks>
    [TestMethod]
    public void AnRss092Document_LoadedThroughRssFeed_FillsTheChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = StreamFor(FeedTestData.Rss092Minimal);

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 0.92 Feed");
    }

    /// <summary>
    /// An RSS 0.90 document — RDF-rooted, detected from the Netscape namespace — fills through the
    /// dispatcher's 0.9 arm.
    /// </summary>
    /// <remarks>
    ///     A reach guard, for the same reason as the 0.92 test.
    /// </remarks>
    [TestMethod]
    public void AnRss090Document_LoadedThroughRssFeed_FillsTheChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = StreamFor(FeedTestData.Rss090Minimal);

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 0.90 Feed");
    }

    /// <summary>
    /// An OPML 1.1 document fills through its own case label on the shared 2.0 arm.
    /// </summary>
    /// <remarks>
    ///     A reach guard: the whole test corpus declared <c>version="2.0"</c>, so the 1.1 and 1.0 labels
    ///     had never executed through a public <c>Load</c>.
    /// </remarks>
    [TestMethod]
    public void AnOpml11Document_LoadedThroughOpmlDocument_FillsTheBody()
    {
        // Arrange
        OpmlDocument document = new();
        using MemoryStream stream = StreamFor("""
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="1.1">
                <head><title>An Outline</title></head>
                <body><outline text="Outline 1"/></body>
            </opml>
            """);

        // Act
        document.Load(stream);

        // Assert
        document.Outlines.Count.ShouldBe(1);
    }

    /// <summary>
    /// An OPML 1.0 document fills through its own case label on the shared 2.0 arm.
    /// </summary>
    [TestMethod]
    public void AnOpml10Document_LoadedThroughOpmlDocument_FillsTheBody()
    {
        // Arrange
        OpmlDocument document = new();
        using MemoryStream stream = StreamFor("""
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="1.0">
                <head><title>An Outline</title></head>
                <body><outline text="Outline 1"/></body>
            </opml>
            """);

        // Act
        document.Load(stream);

        // Assert
        document.Outlines.Count.ShouldBe(1);
    }

    /// <summary>
    /// An RSD 0.6 document fills through the dispatcher's 0.6 arm.
    /// </summary>
    /// <remarks>
    ///     A reach guard: the 0.6 adapter has direct tests, but nothing else executes this dispatcher arm
    ///     through a public <c>Load</c>.
    /// </remarks>
    [TestMethod]
    public void AnRsd06Document_LoadedThroughRsdDocument_FillsTheDocument()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = StreamFor(FeedTestData.Rsd06Minimal);

        // Act
        document.Load(stream);

        // Assert
        document.EngineName.ShouldBe("Test Blog Engine");
    }

    /// <summary>
    /// A feed in the Atom 0.3 namespace carrying an unrecognised <c>version</c> fills as Atom 0.3 — the
    /// namespace fallback's other leg.
    /// </summary>
    /// <remarks>
    ///     The 2005-namespace leg of the fallback is pinned above; this executes the branch that chooses
    ///     <c>Atom03SyndicationResourceAdapter</c> when the 2005 namespace is not in scope.
    /// </remarks>
    [TestMethod]
    public void AnAtomFeedInThePurlNamespaceClaimingAnUnknownVersion_FillsAsAtom03()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = StreamFor("""
            <?xml version="1.0" encoding="UTF-8"?>
            <feed version="0.5" xmlns="http://purl.org/atom/ns#">
                <title>A Purl Feed</title>
                <id>urn:uuid:12345678-1234-1234-1234-123456789012</id>
                <modified>2025-01-20T12:00:00Z</modified>
                <link rel="alternate" type="text/html" href="http://example.com"/>
            </feed>
            """);

        // Act
        feed.Load(stream);

        // Assert
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("A Purl Feed");
    }

    /// <summary>
    /// A stand-alone Atom 0.3 entry document fills through the dispatcher's entry overload.
    /// </summary>
    /// <remarks>
    ///     A reach guard: the 0.3 entry adapter has direct tests, but the dispatcher's Atom 0.3 branch had
    ///     only ever dispatched feeds through a public <c>Load</c>.
    /// </remarks>
    [TestMethod]
    public void AnAtom03EntryDocument_LoadedThroughAtomEntry_FillsTheEntry()
    {
        // Arrange
        AtomEntry entry = new();
        using MemoryStream stream = StreamFor(FeedTestData.Atom03Entry);

        // Act
        entry.Load(stream);

        // Assert
        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Test Entry");
    }

    /// <summary>
    /// The Atom 0.3 branch refuses a resource that is neither a feed nor an entry, exactly as the 1.0
    /// branch does.
    /// </summary>
    [TestMethod]
    public void AnAtom03Format_WithAResourceThatIsNeitherFeedNorEntry_IsRefusedWithArgumentException()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(FeedTestData.Atom03Feed), new SyndicationResourceLoadSettings());

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => adapter.Fill(new RssFeed(), SyndicationContentFormat.Atom));

        // Assert
        exception.ParamName.ShouldBe("resource");
        exception.Message.ShouldContain("RssFeed");
        exception.Message.ShouldContain("AtomFeed");
    }

    /// <summary>
    /// A format of <see cref="SyndicationContentFormat.None"/> is refused before the document is sniffed.
    /// </summary>
    /// <remarks>
    ///     A reach guard: the guard predates this rewrite, and nothing had ever executed its throw.
    /// </remarks>
    [TestMethod]
    public void AFormatOfNone_HandedToTheDispatcher_IsRefused()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(Rss20WithATitledChannel), new SyndicationResourceLoadSettings());

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(() => adapter.Fill(new RssFeed(), SyndicationContentFormat.None));

        // Assert
        exception.ParamName.ShouldBe("format");
        exception.Message.ShouldContain("invalid");
    }
}