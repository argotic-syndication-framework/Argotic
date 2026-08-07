using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Publishing;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Pins what <see cref="SyndicationResourceAdapter.Fill(ISyndicationResource, SyndicationContentFormat)"/>
/// does with documents its version routing does not claim, and with resources whose runtime type cannot
/// read the requested format.
/// </summary>
/// <remarks>
///     <para>
///     Version routing is a chain of equality tests with no fallback arm, and these tests state the
///     consequences as they stand: a document whose format matches but whose declared version no adapter
///     reads fills nothing and raises <c>Loaded</c>; a resource of the wrong runtime type is met with an
///     unannounced <see cref="InvalidCastException"/> where the routing casts, and with silence where it
///     pattern-matches; a format the detection recognises but no arm routes falls through the switch
///     without a word.
///     </para>
///     <para>
///     Characterisations, not guards: each pins an answer a later change deliberately inverts, and exists
///     so that the inversion is seen red before the behaviour moves.
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
                <title>An Unread Title</title>
                <link>http://example.com</link>
                <description>A channel the routing declines to read.</description>
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
            <head><title>An Unread Outline</title></head>
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
            <title>An Unread Feed</title>
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
            <title>An Unread Blog</title>
            <posts>
                <post id="post1"><title>Post 1</title></post>
            </posts>
        </blog>
        """;

    private const string AppService05WithAWorkspace = """
        <?xml version="1.0" encoding="UTF-8"?>
        <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom" version="0.5">
            <workspace>
                <atom:title>An Unread Workspace</atom:title>
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
    /// An RSS document declaring version 0.93 fills nothing and still announces a successful load.
    /// </summary>
    [TestMethod]
    public void AnRss093Document_LoadedThroughRssFeed_FillsNothingAndRaisesLoaded()
    {
        // Arrange
        RssFeed feed = new();
        int loadedCount = 0;
        feed.Loaded += (_, _) => loadedCount++;
        using MemoryStream stream = StreamFor(Rss093WithATitledChannel);

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe(string.Empty);
        feed.Channel.Items.ShouldBeEmpty();
        loadedCount.ShouldBe(1);
    }

    /// <summary>
    /// The same document through <see cref="Syndication.GenericSyndicationFeed"/> yields a feed that
    /// claims to be RSS and holds nothing.
    /// </summary>
    [TestMethod]
    public void AnRss093Document_LoadedThroughGenericSyndicationFeed_FillsNothing()
    {
        // Arrange
        Syndication.GenericSyndicationFeed feed = new();
        using MemoryStream stream = StreamFor(Rss093WithATitledChannel);

        // Act
        feed.Load(stream);

        // Assert
        feed.Format.ShouldBe(SyndicationContentFormat.Rss);
        feed.Title.ShouldBe(string.Empty);
        feed.Items.ShouldBeEmpty();
    }

    /// <summary>
    /// An RSS document declaring version 2.0.1 fills nothing, because <c>Version</c> equality compares the
    /// build component and <c>2.0.1</c> is not <c>2.0</c>.
    /// </summary>
    [TestMethod]
    public void AnRss201Document_LoadedThroughRssFeed_FillsNothing()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = StreamFor(Rss201WithATitledChannel);

        // Act
        feed.Load(stream);

        // Assert
        feed.Channel.Title.ShouldBe(string.Empty);
        feed.Channel.Items.ShouldBeEmpty();
    }

    /// <summary>
    /// An OPML document declaring version 1.5 fills nothing.
    /// </summary>
    [TestMethod]
    public void AnOpml15Document_LoadedThroughOpmlDocument_FillsNothing()
    {
        // Arrange
        OpmlDocument document = new();
        using MemoryStream stream = StreamFor(Opml15WithAnOutline);

        // Act
        document.Load(stream);

        // Assert
        document.Outlines.ShouldBeEmpty();
    }

    /// <summary>
    /// An OPML document declaring version 2.0.1 fills nothing either; patch components lose the document.
    /// </summary>
    [TestMethod]
    public void AnOpml201Document_LoadedThroughOpmlDocument_FillsNothing()
    {
        // Arrange
        OpmlDocument document = new();
        using MemoryStream stream = StreamFor(Opml201WithAnOutline);

        // Act
        document.Load(stream);

        // Assert
        document.Outlines.ShouldBeEmpty();
    }

    /// <summary>
    /// An APML document declaring version 0.5 fills nothing.
    /// </summary>
    [TestMethod]
    public void AnApml05Document_LoadedThroughApmlDocument_FillsNothing()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = StreamFor(Apml05WithAProfile);

        // Act
        document.Load(stream);

        // Assert
        document.Profiles.ShouldBeEmpty();
    }

    /// <summary>
    /// An RSD document declaring version 0.51 fills nothing.
    /// </summary>
    [TestMethod]
    public void AnRsd051Document_LoadedThroughRsdDocument_FillsNothing()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = StreamFor(Rsd051WithAnApi);

        // Act
        document.Load(stream);

        // Assert
        document.EngineName.ShouldBeNullOrEmpty();
        document.Interfaces.ShouldBeEmpty();
    }

    /// <summary>
    /// An Atom 1.0 feed carrying a junk <c>version="0.5"</c> attribute fills nothing, because the detector
    /// lets the attribute override the namespace and the routing then claims neither value.
    /// </summary>
    [TestMethod]
    public void AnAtomFeedClaimingVersion05_LoadedThroughAtomFeed_FillsNothing()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = StreamFor(Atom10ClaimingVersion05);

        // Act
        feed.Load(stream);

        // Assert
        feed.Title.ShouldBeNull();
        feed.Entries.ShouldBeEmpty();
    }

    /// <summary>
    /// A BlogML document declaring version 1.0 in the 2.0 namespace fills nothing.
    /// </summary>
    [TestMethod]
    public void ABlogML10Document_LoadedThroughBlogMLDocument_FillsNothing()
    {
        // Arrange
        BlogMLDocument document = new();
        using MemoryStream stream = StreamFor(BlogML10WithAPost);

        // Act
        document.Load(stream);

        // Assert
        document.Posts.ShouldBeEmpty();
    }

    /// <summary>
    /// An Atom Publishing Protocol service document carrying a junk <c>version="0.5"</c> attribute fills
    /// nothing.
    /// </summary>
    [TestMethod]
    public void AnAppServiceDocumentClaimingVersion05_LoadedThroughAtomServiceDocument_FillsNothing()
    {
        // Arrange
        AtomServiceDocument document = new();
        using MemoryStream stream = StreamFor(AppService05WithAWorkspace);

        // Act
        document.Load(stream);

        // Assert
        document.Workspaces.ShouldBeEmpty();
    }

    /// <summary>
    /// A resource whose runtime type cannot read the requested format dies on an unannounced
    /// <see cref="InvalidCastException"/> — the cast runs before anything else looks at the document.
    /// </summary>
    [TestMethod]
    public void AWrongResourceType_HandedToTheDispatcher_ThrowsInvalidCastException()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(Rss20WithATitledChannel), new SyndicationResourceLoadSettings());

        // Act & Assert
        Should.Throw<InvalidCastException>(() => adapter.Fill(new OpmlDocument(), SyndicationContentFormat.Rss));
    }

    /// <summary>
    /// The Atom arm pattern-matches instead of casting, so a resource that is neither an
    /// <see cref="AtomFeed"/> nor an <see cref="AtomEntry"/> is not refused — it is ignored.
    /// </summary>
    [TestMethod]
    public void AnAtomFormat_WithAResourceThatIsNeitherFeedNorEntry_FillsNothing()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(Atom10WithATitle), new SyndicationResourceLoadSettings());
        RssFeed foreignResource = new();

        // Act
        Should.NotThrow(() => adapter.Fill(foreignResource, SyndicationContentFormat.Atom));

        // Assert
        foreignResource.Channel.Title.ShouldBe(string.Empty);
    }

    /// <summary>
    /// The Atom Publishing arm behaves the same way: a resource that is neither a category document nor a
    /// service document is silently ignored.
    /// </summary>
    [TestMethod]
    public void AnAppFormat_WithAResourceThatIsNeitherCategoryNorServiceDocument_FillsNothing()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(AppCategoriesWithACategory), new SyndicationResourceLoadSettings());
        RssFeed foreignResource = new();

        // Act
        Should.NotThrow(() => adapter.Fill(foreignResource, SyndicationContentFormat.AtomCategoryDocument));

        // Assert
        foreignResource.Channel.Title.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A format the detection recognises but no switch arm routes falls through <c>Fill</c> without a word.
    /// </summary>
    [TestMethod]
    public void ADetectedButUnreadFormat_HandedToTheDispatcher_FillsNothing()
    {
        // Arrange
        SyndicationResourceAdapter adapter = new(NavigatorFor(NewsMLDocument), new SyndicationResourceLoadSettings());
        RssFeed foreignResource = new();

        // Act
        Should.NotThrow(() => adapter.Fill(foreignResource, SyndicationContentFormat.NewsML));

        // Assert
        foreignResource.Channel.Title.ShouldBe(string.Empty);
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
}