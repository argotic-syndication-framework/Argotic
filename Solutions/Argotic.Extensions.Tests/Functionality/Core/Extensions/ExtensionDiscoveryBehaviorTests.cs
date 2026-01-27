using System.Xml;

using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Extensions;

/// <summary>
/// Behavior tests that verify the extension auto-discovery system works correctly.
/// These tests cover auto-discovery from namespaces, extension application at various
/// levels (feed, channel, item/entry), and FindExtension behavior.
/// </summary>
[TestClass]
public class ExtensionDiscoveryBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Auto-Discovery from Namespaces

    [TestMethod]
    public void RssFeed_WithITunesNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""",
            @"<itunes:author>Test Author</itunes:author>");

        // Act
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        ITunesSyndicationExtension? extension = item.FindExtension<ITunesSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Author.ShouldBe("Test Author");
    }

    [TestMethod]
    public void RssFeed_WithDublinCoreNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        // Act
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Test Creator");
    }

    [TestMethod]
    public void RssFeed_WithBasicGeocodingNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""",
            @"<geo:lat>40.7128</geo:lat><geo:long>-74.0060</geo:long>");

        // Act
        RssFeed feed = new RssFeed();
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

    [TestMethod]
    public void RssFeed_WithMultipleNamespaces_DiscoversMultipleExtensions()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<itunes:author>Podcast Author</itunes:author><dc:creator>Dublin Core Creator</dc:creator>");

        // Act
        RssFeed feed = new RssFeed();
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

    [TestMethod]
    public void RssFeed_WithYahooMediaNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:media=""http://search.yahoo.com/mrss/""",
            @"<media:content url=""http://example.com/video.mp4"" type=""video/mp4"" />");

        // Act
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        YahooMediaSyndicationExtension? extension = item.FindExtension<YahooMediaSyndicationExtension>();
        extension.ShouldNotBeNull();
    }

    [TestMethod]
    public void RssFeed_WithCreativeCommonsNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule""",
            @"<creativeCommons:license>http://creativecommons.org/licenses/by/4.0/</creativeCommons:license>");

        // Act
        RssFeed feed = new RssFeed();
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
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert
        feed.Channel.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = feed.Channel.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Channel Creator");
    }

    [TestMethod]
    public void RssFeed_WithItemLevelExtension_AttachesToItem()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Item Creator</dc:creator>");

        // Act
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Item Creator");
    }

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
        AtomFeed feed = new AtomFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert
        feed.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = feed.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Feed Level Creator");
    }

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
        AtomFeed feed = new AtomFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings { AutoDetectExtensions = true };
        feed.Load(reader, settings);

        // Assert
        feed.Entries.Count.ShouldBe(1);
        AtomEntry entry = feed.Entries.First();
        entry.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension? extension = entry.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
        extension.ShouldNotBeNull();
        extension.Context.Creator.ShouldBe("Entry Level Creator");
    }

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
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings { AutoDetectExtensions = true };
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

    [TestMethod]
    public void FindExtension_WithMatchingExtension_ReturnsExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""",
            @"<itunes:author>Test Author</itunes:author>");

        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ITunesSyndicationExtension? extension = item.FindExtension<ITunesSyndicationExtension>();

        // Assert
        extension.ShouldNotBeNull();
        extension.ShouldBeOfType<ITunesSyndicationExtension>();
    }

    [TestMethod]
    public void FindExtension_WithNoMatchingExtension_ReturnsNull()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ITunesSyndicationExtension? extension = item.FindExtension<ITunesSyndicationExtension>();

        // Assert
        extension.ShouldBeNull();
    }

    [TestMethod]
    public void FindExtension_WithPredicate_ReturnsMatchingExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd""",
            @"<itunes:author>Test Author</itunes:author>");

        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);
        RssItem item = feed.Channel.Items.First();

        // Act
        ISyndicationExtension? extension = item.FindExtension(ITunesSyndicationExtension.MatchByType);

        // Assert
        extension.ShouldNotBeNull();
        extension.ShouldBeOfType<ITunesSyndicationExtension>();
    }

    [TestMethod]
    public void FindExtension_ByNamespace_ReturnsMatchingExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        RssFeed feed = new RssFeed();
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

    [TestMethod]
    public void HasExtensions_WhenNoExtensions_ReturnsFalse()
    {
        // Arrange
        RssFeed feed = new RssFeed(new Uri("http://example.com"), "Test Feed")
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

    [TestMethod]
    public void HasExtensions_WhenExtensionsPresent_ReturnsTrue()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dc=""http://purl.org/dc/elements/1.1/""",
            @"<dc:creator>Test Creator</dc:creator>");

        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
    }

    [TestMethod]
    public void FindExtension_WithMultipleExtensions_FindsCorrectOne()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""",
            @"<itunes:author>Podcast Author</itunes:author><dc:creator>Dublin Core Creator</dc:creator><geo:lat>40.7128</geo:lat><geo:long>-74.0060</geo:long>");

        RssFeed feed = new RssFeed();
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

        RssFeed feed = new RssFeed();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
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

        AtomFeed feed = new AtomFeed();
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings
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

    [TestMethod]
    public void RssFeed_WithSiteSummarySlashNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:slash=""http://purl.org/rss/1.0/modules/slash/""",
            @"<slash:comments>42</slash:comments>");

        // Act
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        SiteSummarySlashSyndicationExtension? extension = item.FindExtension<SiteSummarySlashSyndicationExtension>();
        extension.ShouldNotBeNull();
        extension.Context.Comments.ShouldBe(42);
    }

    [TestMethod]
    public void RssFeed_WithSiteSummaryContentNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:content=""http://purl.org/rss/1.0/modules/content/""",
            @"<content:encoded><![CDATA[<p>Full content here</p>]]></content:encoded>");

        // Act
        RssFeed feed = new RssFeed();
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.First();
        item.HasExtensions.ShouldBeTrue();
        SiteSummaryContentSyndicationExtension? extension = item.FindExtension<SiteSummaryContentSyndicationExtension>();
        extension.ShouldNotBeNull();
    }

    [TestMethod]
    public void RssFeed_WithDublinCoreMetadataTermsNamespace_AutoDiscoversExtension()
    {
        // Arrange
        string xml = ExtensionTestUtil.GetWrappedXml(
            @"xmlns:dcterms=""http://purl.org/dc/terms/""",
            @"<dcterms:abstract>Test abstract content</dcterms:abstract>");

        // Act
        RssFeed feed = new RssFeed();
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
}
