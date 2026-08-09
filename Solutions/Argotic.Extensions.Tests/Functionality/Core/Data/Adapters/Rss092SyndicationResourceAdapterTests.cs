using Argotic.Data.Adapters;
namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="Rss092SyndicationResourceAdapter"/>. RSS 0.92 is the RSS 0.91 document plus four
/// things — <c>cloud</c> on the channel, and <c>category</c>, <c>enclosure</c> and <c>source</c> on an
/// item — and these tests state what each of those fills, what the inherited 0.91 vocabulary still does,
/// what a minimal channel leaves unset, and how the retrieval limit behaves.
/// </summary>
[TestClass]
public class Rss092SyndicationResourceAdapterTests
{
    /// <summary>
    /// A minimal RSS 0.92 channel fills its title, link and description.
    /// </summary>
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

    /// <summary>
    /// A <c>cloud</c> element fills its domain, port, path and register procedure, and its <c>protocol</c>
    /// of <c>soap</c> maps onto <see cref="RssCloudProtocol.Soap"/>.
    /// </summary>
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

    /// <summary>
    /// An item's two <c>category</c> elements fill in document order, the second keeping the <c>domain</c>
    /// attribute that names its taxonomy.
    /// </summary>
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

    /// <summary>
    /// An item's <c>enclosure</c> fills its URL, its length in bytes and its <c>audio/mpeg</c> content type.
    /// </summary>
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

    /// <summary>
    /// An item's <c>source</c> fills the originating feed's title from the element text and its URL from
    /// the <c>url</c> attribute.
    /// </summary>
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

    /// <summary>
    /// Both items fill in document order, each with its title, link and description.
    /// </summary>
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

    /// <summary>
    /// An <c>image</c> fills its title, URL, link and its <c>88</c> by <c>31</c> dimensions.
    /// </summary>
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

    /// <summary>
    /// A <c>textInput</c> fills its title, description, name and link.
    /// </summary>
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

    /// <summary>
    /// A retrieval limit of <c>1</c> keeps only the first item.
    /// </summary>
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

    /// <summary>
    /// Filling a <see langword="null"/> feed throws <c>ArgumentNullException</c>.
    /// </summary>
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

    /// <summary>
    /// Constructing the adapter without a navigator throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss092SyndicationResourceAdapter(null!, settings));
    }

    /// <summary>
    /// Constructing the adapter without load settings throws <c>ArgumentNullException</c>.
    /// </summary>
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

    /// <summary>
    /// A channel with no <c>cloud</c> leaves the property <see langword="null"/>, not an empty instance.
    /// </summary>
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

    /// <summary>
    /// A channel with no <c>image</c> leaves the property <see langword="null"/>, not an empty instance.
    /// </summary>
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

    /// <summary>
    /// A channel with no <c>textInput</c> leaves the property <see langword="null"/>, not an empty instance.
    /// </summary>
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

    /// <summary>
    /// A channel with no <c>item</c> leaves the item collection empty.
    /// </summary>
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

    /// <summary>
    /// A <c>language</c> of <c>en-us</c> is parsed into a culture whose name is the canonically cased
    /// <c>en-US</c>.
    /// </summary>
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

    /// <summary>
    /// The optional <c>copyright</c>, <c>managingEditor</c> and <c>webMaster</c> elements are filled
    /// verbatim.
    /// </summary>
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

    /// <summary>
    /// The <c>day</c> children of <c>skipDays</c> are parsed by name into <c>DayOfWeek</c> values.
    /// </summary>
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

    /// <summary>
    /// The <c>hour</c> children of <c>skipHours</c> are renumbered on the way in, so a document saying
    /// <c>1</c> and <c>2</c> yields <c>0</c> and <c>1</c>.
    /// </summary>
    /// <remarks>
    ///     RSS 0.92 inherits the 0.91 clock, which counts the hours of the day from 1, while the object model
    ///     holds the RSS 2.0 0-based form; the adapter subtracts one from every hour it reads.
    /// </remarks>
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

    /// <summary>
    /// A retrieval limit of <c>0</c> means no limit, and both items are read.
    /// </summary>
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

    /// <summary>
    /// An item carrying two <c>enclosure</c> elements fills both, in document order.
    /// </summary>
    /// <remarks>
    ///     Deliberately beyond the specification. RSS 0.92 allows one enclosure per item; the adapter accepts
    ///     several because real feeds emit them and dropping the extras loses media.
    /// </remarks>
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

    /// <summary>
    /// An item with no <c>source</c> leaves the property <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// An empty <c>language</c> element leaves the channel's language <see langword="null"/> instead of
    /// throwing.
    /// </summary>
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

    /// <summary>
    /// A channel carrying <c>pubDate</c> and <c>lastBuildDate</c> fills without throwing, whether or not
    /// those dates parse.
    /// </summary>
    /// <remarks>
    ///     Nothing is asserted about the two dates themselves, only about the channel around them: the file
    ///     records that date-format compatibility is covered by the <c>SyndicationDateTimeUtility</c> tests.
    /// </remarks>
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

    /// <summary>
    /// A <c>category</c> with no <c>domain</c> attribute fills its value and leaves the domain an
    /// <i>empty</i> string, not <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// A <c>protocol</c> of <c>xml-rpc</c> maps onto <see cref="RssCloudProtocol.XmlRpc"/>, hyphen and all.
    /// </summary>
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

    /// <summary>
    /// A <c>cloud</c> with no <c>protocol</c> attribute still fills its domain and port, and keeps
    /// <c>RssCloud</c>'s default protocol of <see cref="RssCloudProtocol.XmlRpc"/>.
    /// </summary>
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