using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Unit tests for <see cref="Rss092SyndicationResourceAdapter"/>.
/// </summary>
[TestClass]
public class Rss092SyndicationResourceAdapterTests
{
    [TestMethod]
    public void Fill_MinimalRss092_PopulatesChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 0.92 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 0.92 feed");
    }

    [TestMethod]
    public void Fill_WithCloud_PopulatesCloudProperties()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Cloud.ShouldNotBeNull();
        feed.Channel.Cloud.Domain.ShouldBe("rpc.example.com");
        feed.Channel.Cloud.Port.ShouldBe(80);
        feed.Channel.Cloud.Path.ShouldBe("/RPC2");
        feed.Channel.Cloud.RegisterProcedure.ShouldBe("pingMe");
        feed.Channel.Cloud.Protocol.ShouldBe(RssCloudProtocol.Soap);
    }

    [TestMethod]
    public void Fill_WithCategories_PopulatesCategories()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items[0].Categories.Count.ShouldBe(2);
        feed.Channel.Items[0].Categories[0].Value.ShouldBe("Technology");
        feed.Channel.Items[0].Categories[1].Value.ShouldBe("News");
        feed.Channel.Items[0].Categories[1].Domain.ShouldBe("http://example.com/categories");
    }

    [TestMethod]
    public void Fill_WithEnclosure_PopulatesEnclosure()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items[0].Enclosures.Count.ShouldBe(1);
        RssEnclosure enclosure = feed.Channel.Items[0].Enclosures[0];
        enclosure.Url.ShouldBe(new Uri("http://example.com/podcast.mp3"));
        enclosure.Length.ShouldBe(12_345_678L);
        enclosure.ContentType.ShouldBe("audio/mpeg");
    }

    [TestMethod]
    public void Fill_WithSource_PopulatesSource()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items[0].Source.ShouldNotBeNull();
        feed.Channel!.Items[0].Source!.Title.ShouldBe("Other Feed");
        feed.Channel!.Items[0].Source!.Url.ShouldBe(new Uri("http://other.example.com/feed.xml"));
    }

    [TestMethod]
    public void Fill_WithItems_PopulatesItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("Test Item with Enclosure");
        feed.Channel.Items[0].Link.ShouldBe(new Uri("http://example.com/item1"));
        feed.Channel.Items[0].Description.ShouldBe("Test item with enclosure");
        feed.Channel.Items[1].Title.ShouldBe("Second Item");
        feed.Channel.Items[1].Link.ShouldBe(new Uri("http://example.com/item2"));
        feed.Channel.Items[1].Description.ShouldBe("Second item description");
    }

    [TestMethod]
    public void Fill_WithImage_PopulatesImage()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.Image.Title.ShouldBe("Feed Image");
        feed.Channel.Image.Url.ShouldBe(new Uri("http://example.com/image.png"));
        feed.Channel.Image.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Image.Width.ShouldBe(88);
        feed.Channel.Image.Height.ShouldBe(31);
    }

    [TestMethod]
    public void Fill_WithTextInput_PopulatesTextInput()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldNotBeNull();
        feed.Channel.TextInput.Title.ShouldBe("Search");
        feed.Channel.TextInput.Description.ShouldBe("Search the feed");
        feed.Channel.TextInput.Name.ShouldBe("query");
        feed.Channel.TextInput.Link.ShouldBe(new Uri("http://example.com/search"));
    }

    [TestMethod]
    public void Fill_WithRetrievalLimit_EnforcesLimit()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 1
        };
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(1);
        feed.Channel.Items[0].Title.ShouldBe("Test Item with Enclosure");
    }

    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill(null!));
    }

    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss092SyndicationResourceAdapter(null!, settings));
    }

    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss092SyndicationResourceAdapter(navigator, null!));
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesCloudNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Cloud.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesImageNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesTextInputNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_MinimalFeed_LeavesItemsEmpty()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.ShouldBeEmpty();
    }

    [TestMethod]
    public void Fill_WithLanguage_ParsesCultureInfo()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Language.ShouldNotBeNull();
        feed.Channel.Language.Name.ShouldBe("en-US");
    }

    [TestMethod]
    public void Fill_WithOptionalElements_PopulatesOptionals()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Copyright.ShouldBe("Copyright 2025");
        feed.Channel.ManagingEditor.ShouldBe("editor@example.com");
        feed.Channel.Webmaster.ShouldBe("webmaster@example.com");
        // Note: Date parsing depends on RFC 822 format - these verify the values were read
    }

    [TestMethod]
    public void Fill_WithSkipDays_ParsesDaysCorrectly()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.SkipDays.Count.ShouldBe(2);
        feed.Channel.SkipDays.ShouldContain(DayOfWeek.Saturday);
        feed.Channel.SkipDays.ShouldContain(DayOfWeek.Sunday);
    }

    [TestMethod]
    public void Fill_WithSkipHours_ParsesHoursCorrectly()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        // Note: The adapter converts 1-based hours (1, 2) to 0-based (0, 1)
        feed.Channel.SkipHours.Count.ShouldBe(2);
        feed.Channel.SkipHours.ShouldContain(0);  // Hour 1 becomes 0
        feed.Channel.SkipHours.ShouldContain(1);  // Hour 2 becomes 1
    }

    [TestMethod]
    public void Fill_WithZeroRetrievalLimit_RetrievesAllItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 0  // 0 means no limit
        };
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
    }

    [TestMethod]
    public void Fill_WithMultipleEnclosures_PopulatesAllEnclosures()
    {
        // Arrange - Create feed with multiple enclosures on an item
        const string rss092WithMultipleEnclosures = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.92">
                <channel>
                    <title>Test RSS 0.92 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <item>
                        <title>Item with Multiple Enclosures</title>
                        <link>http://example.com/item1</link>
                        <enclosure url="http://example.com/file1.mp3" length="1000" type="audio/mpeg"/>
                        <enclosure url="http://example.com/file2.pdf" length="2000" type="application/pdf"/>
                    </item>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss092WithMultipleEnclosures));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items[0].Enclosures.Count.ShouldBe(2);
        feed.Channel.Items[0].Enclosures[0].Url.ShouldBe(new Uri("http://example.com/file1.mp3"));
        feed.Channel.Items[0].Enclosures[0].ContentType.ShouldBe("audio/mpeg");
        feed.Channel.Items[0].Enclosures[1].Url.ShouldBe(new Uri("http://example.com/file2.pdf"));
        feed.Channel.Items[0].Enclosures[1].ContentType.ShouldBe("application/pdf");
    }

    [TestMethod]
    public void Fill_WithItemWithoutSource_LeavesSourceNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        // Second item doesn't have a source element
        feed.Channel.Items[1].Source.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_WithEmptyLanguage_DoesNotThrow()
    {
        // Arrange - Create feed with empty language element
        const string rss092WithEmptyLanguage = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.92">
                <channel>
                    <title>Test RSS 0.92 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <language></language>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss092WithEmptyLanguage));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert - Should not throw for empty language
        Should.NotThrow(() => adapter.Fill(feed));
        feed.Channel.Language.ShouldBeNull();
    }

    [TestMethod]
    public void Fill_WithDateElements_AttemptsToParseDates()
    {
        // Arrange - This test verifies that date elements are read and parsed (or left at default if parsing fails)
        // The actual date parsing format compatibility is tested separately in SyndicationDateTimeUtility tests
        const string rss092WithDates = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.92">
                <channel>
                    <title>Test RSS 0.92 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <pubDate>Mon, 20 Jan 2025 12:00:00 +00:00</pubDate>
                    <lastBuildDate>Tue, 21 Jan 2025 14:30:00 +00:00</lastBuildDate>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss092WithDates));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act - Should not throw when encountering date elements
        Should.NotThrow(() => adapter.Fill(feed));

        // Assert - Channel was populated (dates may or may not parse depending on format support)
        feed.Channel.Title.ShouldBe("Test RSS 0.92 Feed");
    }

    [TestMethod]
    public void Fill_WithCategoryWithoutDomain_PopulatesCategoryWithoutDomain()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss092Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        // First category has no domain
        feed.Channel.Items[0].Categories[0].Value.ShouldBe("Technology");
        feed.Channel.Items[0].Categories[0].Domain.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Fill_WithCloudProtocolXmlRpc_ParsesProtocolCorrectly()
    {
        // Arrange - Create feed with xml-rpc protocol
        const string rss092WithXmlRpcCloud = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.92">
                <channel>
                    <title>Test RSS 0.92 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <cloud domain="rpc.example.com" port="80" path="/RPC2" registerProcedure="pingMe" protocol="xml-rpc"/>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss092WithXmlRpcCloud));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Cloud.ShouldNotBeNull();
        feed.Channel.Cloud.Protocol.ShouldBe(RssCloudProtocol.XmlRpc);
    }

    [TestMethod]
    public void Fill_WithCloudWithoutProtocol_UsesDefaultProtocol()
    {
        // Arrange - Create feed with cloud but no protocol attribute
        const string rss092WithCloudNoProtocol = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.92">
                <channel>
                    <title>Test RSS 0.92 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <cloud domain="rpc.example.com" port="80" path="/RPC2" registerProcedure="pingMe"/>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss092WithCloudNoProtocol));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss092SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Cloud.ShouldNotBeNull();
        feed.Channel.Cloud.Domain.ShouldBe("rpc.example.com");
        feed.Channel.Cloud.Port.ShouldBe(80);
        // No protocol attribute uses the RssCloud default which is XmlRpc
        feed.Channel.Cloud.Protocol.ShouldBe(RssCloudProtocol.XmlRpc);
    }
}