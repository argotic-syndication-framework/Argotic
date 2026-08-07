using System.Xml;

using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Extensions;

/// <summary>
/// Covers extension auto-discovery end to end: which namespaces are recognised while a document is
/// read, which object the resulting extension attaches to, how <c>FindExtension</c> reaches it again,
/// and what the reflection scan behind all of it returns.
/// </summary>
[TestClass]
public class ExtensionDiscoveryBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Auto-Discovery from Namespaces

    /// <summary>
    /// An item declaring the iTunes namespace acquires an <c>ITunesSyndicationExtension</c> with nothing registered by the caller, and its author is parsed.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithITunesNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""",
            @"<itunes:author>Test Author</itunes:author>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        ITunesSyndicationExtension? extension = item.FindExtension<ITunesSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Author.ShouldBe("Test Author");
    }

    /// <summary>
    /// An item declaring the Dublin Core element-set namespace acquires a <c>DublinCoreElementSetSyndicationExtension</c>, and its creator is parsed.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithDublinCoreNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Test Creator");
    }

    /// <summary>
    /// An item declaring the WGS 84 namespace acquires a <c>BasicGeocodingSyndicationExtension</c> whose coordinates keep their decimal precision, <c>40.7128</c> and <c>-74.0060</c>.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithBasicGeocodingNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""",
            @"<geo:lat>40.7128</geo:lat><geo:long>-74.0060</geo:long>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        BasicGeocodingSyndicationExtension? extension = item.FindExtension<BasicGeocodingSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Latitude.ShouldBe(40.7128m);
        extension.Context.Longitude.ShouldBe(-74.0060m);
    }

    /// <summary>
    /// An item declaring two extension namespaces acquires both extensions, each holding its own value.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithMultipleNamespaces_DiscoversMultipleExtensions()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<itunes:author>Podcast Author</itunes:author><dc:creator>Dublin Core Creator</dc:creator>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        item.Extensions.Count.ShouldBeGreaterThanOrEqualTo(2);

        ITunesSyndicationExtension? itunesExtension = item.FindExtension<ITunesSyndicationExtension>();
        itunesExtension.ShouldNotBeNull();
        itunesExtension.Context.Author.ShouldBe("Podcast Author");

        DublinCoreElementSetSyndicationExtension? dcExtension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        dcExtension.ShouldNotBeNull();
        dcExtension.Context.Creator.ShouldBe("Dublin Core Creator");
    }

    /// <summary>
    /// An item declaring the Yahoo Media namespace acquires a <c>YahooMediaSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithYahooMediaNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:media=""http://search.yahoo.com/mrss/""",
            @"<media:content url=""http://example.com/video.mp4"" type=""video/mp4"" />");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        YahooMediaSyndicationExtension? extension = item.FindExtension<YahooMediaSyndicationExtension>();
        extension.ShouldNotBeNull();
    }

    /// <summary>
    /// An item declaring the Creative Commons namespace acquires a <c>CreativeCommonsSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithCreativeCommonsNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule""",
            @"<creativeCommons:license>http://creativecommons.org/licenses/by/4.0/</creativeCommons:license>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        CreativeCommonsSyndicationExtension? extension = item.FindExtension<CreativeCommonsSyndicationExtension>();
        extension.ShouldNotBeNull();
    }

    #endregion

    #region Extension Application at Different Levels

    /// <summary>
    /// An extension element that is a child of <c>channel</c> attaches to the channel, not to the item beneath it.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithChannelLevelExtension_AttachesToChannel()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>Test Description</description>
                    <dc:creator>Channel Creator</dc:creator>
                    <item>
                        <title>Test Item</title>
                        <description>Test item description</description>
                    </item>
                </channel>
            </rss>
            """;

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new() { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert
        feed.Channel.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = feed.Channel.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Channel Creator");
    }

    /// <summary>
    /// An extension element that is a child of <c>item</c> attaches to that item.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithItemLevelExtension_AttachesToItem()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Item Creator</dc:creator>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Item Creator");
    }

    /// <summary>
    /// An extension element that is a child of <c>feed</c> attaches to the feed itself.
    /// </summary>
    [TestMethod]
    public void AtomFeed_WithFeedLevelExtension_AttachesToFeed()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <title>Test Atom Feed</title>
                <id>urn:uuid:feed-1</id>
                <updated>2025-01-01T00:00:00Z</updated>
                <dc:creator>Feed Level Creator</dc:creator>
            </feed>
            """;

        // Act
        AtomFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new() { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert
        feed.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = feed.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Feed Level Creator");
    }

    /// <summary>
    /// An extension element that is a child of <c>entry</c> attaches to that entry.
    /// </summary>
    [TestMethod]
    public void AtomFeed_WithEntryLevelExtension_AttachesToEntry()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <title>Test Atom Feed</title>
                <id>urn:uuid:feed-1</id>
                <updated>2025-01-01T00:00:00Z</updated>
                <entry>
                    <title>Test Entry</title>
                    <id>urn:uuid:entry-1</id>
                    <updated>2025-01-01T00:00:00Z</updated>
                    <dc:creator>Entry Level Creator</dc:creator>
                </entry>
            </feed>
            """;

        // Act
        AtomFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new() { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert
        feed.Entries.Count.ShouldBe(1);
        AtomEntry entry = feed.Entries.First();
        entry.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = entry.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Entry Level Creator");
    }

    /// <summary>
    /// The same extension declared at both channel and item level yields one instance at each, each holding the value written at its own level.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithExtensionsAtMultipleLevels_AttachesCorrectly()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>Test Description</description>
                    <dc:creator>Channel Level Creator</dc:creator>
                    <item>
                        <title>Test Item</title>
                        <description>Test item description</description>
                        <dc:creator>Item Level Creator</dc:creator>
                    </item>
                </channel>
            </rss>
            """;

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new() { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert - Channel level
        feed.Channel.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? channelExtension = feed.Channel.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        channelExtension.ShouldNotBeNull();
        channelExtension.Context.Creator.ShouldBe("Channel Level Creator");

        // Assert - Item level
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? itemExtension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Creator.ShouldBe("Item Level Creator");
    }

    #endregion

    #region FindExtension Behavior

    /// <summary>
    /// The generic <c>FindExtension&lt;T&gt;</c> returns the attached extension already typed as <c>T</c>.
    /// </summary>
    [TestMethod]
    public void FindExtension_WithMatchingExtension_ReturnsExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""",
            @"<itunes:author>Test Author</itunes:author>");

        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ITunesSyndicationExtension? extension = item.FindExtension<ITunesSyndicationExtension>();

        // Assert
        extension.ShouldNotBeNull();
        extension.ShouldBeOfType<ITunesSyndicationExtension>();
    }

    /// <summary>
    /// Asking for an extension that is not attached returns <see langword="null"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void FindExtension_WithNoMatchingExtension_ReturnsNull()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ITunesSyndicationExtension? extension = item.FindExtension<ITunesSyndicationExtension>();

        // Assert
        extension.ShouldBeNull();
    }

    /// <summary>
    /// The predicate overload reaches the same extension through <c>MatchByType</c>.
    /// </summary>
    [TestMethod]
    public void FindExtension_WithPredicate_ReturnsMatchingExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""",
            @"<itunes:author>Test Author</itunes:author>");

        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ISyndicationExtension? extension = item.FindExtension(ITunesSyndicationExtension.MatchByType);

        // Assert
        extension.ShouldNotBeNull();
        extension.ShouldBeOfType<ITunesSyndicationExtension>();
    }

    /// <summary>
    /// A predicate written by the caller can select on <c>XmlNamespace</c>, reaching the extension without naming its type.
    /// </summary>
    [TestMethod]
    public void FindExtension_ByNamespace_ReturnsMatchingExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ISyndicationExtension? extension = item.FindExtension(ext =>
            ext.XmlNamespace == "http://purl.org/dc/elements/1.1/");

        // Assert
        extension.ShouldNotBeNull();
        extension.ShouldBeOfType<DublinCoreElementSetSyndicationExtension>();
    }

    /// <summary>
    /// A feed built in code carries no extensions at feed, channel or item level.
    /// </summary>
    [TestMethod]
    public void HasExtensions_WhenNoExtensions_ReturnsFalse()
    {
        // Arrange
        RssFeed feed = new(new Uri("http://example.com"), "Test Feed")
        {
            Channel =
            {
                Description = "Test Description"
            }
        };
        feed.Channel.Items.Add(new RssItem
        {
            Title = "Test Item",
            Description = "Test Description"
        });

        // Assert
        feed.HasExtensions.ShouldBeFalse();
        feed.Channel.HasExtensions.ShouldBeFalse();
        feed.Channel.Items.First().HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// An item that picked up an extension while loading reports that it has one.
    /// </summary>
    [TestMethod]
    public void HasExtensions_WhenExtensionsPresent_ReturnsTrue()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
    }

    /// <summary>
    /// With three extensions on one item, each lookup returns its own extension holding its own value.
    /// </summary>
    [TestMethod]
    public void FindExtension_WithMultipleExtensions_FindsCorrectOne()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""",
            @"<itunes:author>Podcast Author</itunes:author><dc:creator>Dublin Core Creator</dc:creator><geo:lat>40.7128</geo:lat><geo:long>-74.0060</geo:long>");

        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act & Assert - Find each specific extension
        ITunesSyndicationExtension? itunesExtension = item.FindExtension<ITunesSyndicationExtension>();
        itunesExtension.ShouldNotBeNull();
        itunesExtension.Context.Author.ShouldBe("Podcast Author");

        DublinCoreElementSetSyndicationExtension? dcExtension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        dcExtension.ShouldNotBeNull();
        dcExtension.Context.Creator.ShouldBe("Dublin Core Creator");

        BasicGeocodingSyndicationExtension? geoExtension = item.FindExtension<BasicGeocodingSyndicationExtension>();
        geoExtension.ShouldNotBeNull();
        geoExtension.Context.Latitude.ShouldBe(40.7128m);
    }

    #endregion

    #region AutoDetectExtensions Setting

    /// <summary>
    /// With <c>AutoDetectExtensions</c> set, a channel-level Dublin Core element is discovered.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithAutoDetectExtensionsTrue_DiscoversExtensions()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <channel>
                    <title>Test Feed</title>
                    <link>http://example.com</link>
                    <description>Test Description</description>
                    <dc:creator>Test Creator</dc:creator>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };
        using XmlReader reader = XmlReader.Create(new StringReader(xml));

        // Act
        feed.Load(reader, settings);

        // Assert
        feed.Channel.HasExtensions.ShouldBeTrue();
        ISyndicationExtension? extension = feed.Channel.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType);
        extension.ShouldNotBeNull();
    }

    /// <summary>
    /// With <c>AutoDetectExtensions</c> set, a feed-level Dublin Core element is discovered and its publisher parsed.
    /// </summary>
    [TestMethod]
    public void AtomFeed_WithAutoDetectExtensionsTrue_DiscoversExtensions()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <title>Test Feed</title>
                <id>urn:uuid:test-feed-1</id>
                <updated>2025-01-01T00:00:00Z</updated>
                <dc:publisher>Test Publisher</dc:publisher>
            </feed>
            """;

        AtomFeed feed = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };
        using XmlReader reader = XmlReader.Create(new StringReader(xml));

        // Act
        feed.Load(reader, settings);

        // Assert
        feed.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = feed.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Publisher.ShouldBe("Test Publisher");
    }

    #endregion

    #region Specific Extension Type Tests

    /// <summary>
    /// An item declaring the Slash namespace acquires a <c>SiteSummarySlashSyndicationExtension</c>, and its comment count is parsed as the <c>int</c> <c>42</c>.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithSiteSummarySlashNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:slash=""http://purl.org/rss/1.0/modules/slash/""",
            @"<slash:comments>42</slash:comments>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        SiteSummarySlashSyndicationExtension? extension = item.FindExtension<SiteSummarySlashSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Comments.ShouldBe(42);
    }

    /// <summary>
    /// An item whose content module element wraps its markup in CDATA still acquires a <c>SiteSummaryContentSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithSiteSummaryContentNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:content=""http://purl.org/rss/1.0/modules/content/""",
            @"<content:encoded><![CDATA[<p>Full content here</p>]]></content:encoded>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        SiteSummaryContentSyndicationExtension? extension = item.FindExtension<SiteSummaryContentSyndicationExtension>();
        extension.ShouldNotBeNull();
    }

    /// <summary>
    /// An item declaring the Dublin Core <i>terms</i> namespace acquires a <c>DublinCoreMetadataTermsSyndicationExtension</c>, distinct from the element-set extension, and its abstract is parsed.
    /// </summary>
    [TestMethod]
    public void RssFeed_WithDublinCoreMetadataTermsNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dcterms=""http://purl.org/dc/terms/""",
            @"<dcterms:abstract>Test abstract content</dcterms:abstract>");

        // Act
        RssFeed feed = new();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreMetadataTermsSyndicationExtension? extension = item.FindExtension<DublinCoreMetadataTermsSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Abstract.ShouldBe("Test abstract content");
    }

    #endregion

    #region FrameworkExtensions Reflection Scan

    /// <summary>
    /// Every type the reflection scan returns derives from <c>SyndicationExtension</c>, is concrete, and has a parameterless constructor.
    /// </summary>
    /// <remarks>
    ///     <c>GetExtensions</c> calls <c>Activator.CreateInstance</c> on everything the scan returns, so an
    ///     abstract type or one without a parameterless constructor would throw while a document was
    ///     being loaded.
    /// </remarks>
    [TestMethod]
    public void FrameworkExtensions_ReturnsOnlyInstantiableSyndicationExtensions()
    {
        IList<Type> types = SyndicationExtensionAdapter.FrameworkExtensions;

        types.ShouldNotBeEmpty();
        foreach (Type type in types)
        {
            typeof(SyndicationExtension).IsAssignableFrom(type).ShouldBeTrue($"{type.Name} does not derive from SyndicationExtension");

            // GetExtensions calls Activator.CreateInstance on everything returned here, so anything
            // abstract or lacking a parameterless constructor would throw at load time.
            type.IsAbstract.ShouldBeFalse($"{type.Name} is abstract and cannot be instantiated");
            type.GetConstructor(Type.EmptyTypes).ShouldNotBeNull($"{type.Name} has no parameterless constructor");
        }
    }

    /// <summary>
    /// The abstract <c>SyndicationExtension</c> base is not itself among the types the scan returns.
    /// </summary>
    [TestMethod]
    public void FrameworkExtensions_DoesNotReturnTheAbstractBase() => SyndicationExtensionAdapter.FrameworkExtensions.ShouldNotContain(typeof(SyndicationExtension));

    /// <summary>
    /// The six extensions the hand-maintained list had drifted past are all present.
    /// </summary>
    /// <remarks>
    ///     That list sat behind a disabled <c>#if</c> branch in <c>SyndicationExtensionAdapter</c>. Because
    ///     the branch never compiled, nothing caught the drift; this asserts the reflection scan that
    ///     replaced it does not share the blind spot.
    /// </remarks>
    [TestMethod]
    public void FrameworkExtensions_ReturnsExtensionsAddedAfterTheHandMaintainedList()
    {
        // These six were missing from the hand-maintained list that used to sit behind a disabled
        // #if branch in SyndicationExtensionAdapter. Because that branch never compiled, nothing
        // caught the drift. This asserts the reflection scan does not have the same blind spot.
        IList<Type> types = SyndicationExtensionAdapter.FrameworkExtensions;

        types.ShouldContain(typeof(AtomPublishingControlSyndicationExtension));
        types.ShouldContain(typeof(AtomPublishingEditedSyndicationExtension));
        types.ShouldContain(typeof(SitemapImageExtension));
        types.ShouldContain(typeof(SitemapNewsExtension));
        types.ShouldContain(typeof(SitemapVideoExtension));
        types.ShouldContain(typeof(SitemapHreflangExtension));
    }

    /// <summary>
    /// <c>GetExtensions</c> instantiates every type the scan returns, one distinct instance each, with none
    /// dropped — all twenty-seven of them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The scan feeds <c>GetExtensions</c> directly, so instantiability is the contract that matters.
    ///     </para>
    ///     <para>
    ///     The count used to be asserted against <c>FrameworkExtensions.Count</c> — the property under test,
    ///     compared to itself. That is self-fulfilling: a scan returning nothing satisfies it, and so does a
    ///     scan returning half the assembly. A literal is the only spelling of the claim that can fail.
    ///     </para>
    ///     <para>
    ///     Twenty-seven is what the assembly holds: <c>grep ': SyndicationExtension\b' Argotic.Extensions/Core</c>
    ///     returns twenty-seven concrete classes, and the family table in <c>CLAUDE.md</c> sums to twenty-seven
    ///     across its twenty-two rows — <c>AtomPublishing</c> 2, <c>DublinCore</c> 2, <c>Sitemap</c> 4, and one
    ///     each for the other nineteen. The figure of twenty-eight quoted in that file's prose is an arithmetic
    ///     error; <c>GeoRSS</c> serves both the Simple and the GML encodings from a single
    ///     <c>GeoRssSyndicationExtension</c>.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void FrameworkExtensions_EveryReturnedTypeCanBeInstantiated()
    {
        // The scan feeds GetExtensions directly, so instantiability is the real contract.
        IList<ISyndicationExtension> extensions =
            SyndicationExtensionAdapter.GetExtensions(SyndicationExtensionAdapter.FrameworkExtensions);

        extensions.Count.ShouldBe(27);
        extensions.ShouldNotContain(extension => extension == null);
        extensions.Select(extension => extension.GetType()).Distinct().Count().ShouldBe(27);
    }

    #endregion
}