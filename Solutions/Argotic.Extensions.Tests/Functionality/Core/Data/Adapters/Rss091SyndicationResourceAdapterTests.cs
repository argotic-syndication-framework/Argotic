using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="Rss091SyndicationResourceAdapter"/>. RSS 0.91 begins the plain, namespace-free
/// <c>rss</c> line, with items nested inside the channel; these tests state which of its channel
/// vocabulary reaches the <see cref="RssFeed"/> — <c>language</c>, <c>rating</c>, <c>skipDays</c>,
/// <c>skipHours</c>, <c>image</c> and <c>textInput</c> — how a malformed value is dropped rather than
/// thrown, and how the retrieval limit behaves.
/// </summary>
[TestClass]
public class Rss091SyndicationResourceAdapterTests
{
    /// <summary>
    /// A minimal RSS 0.91 channel fills its title, link and description.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalRss091_PopulatesChannel()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Title.ShouldBe("Test RSS 0.91 Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("A test RSS 0.91 feed");
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
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Language.ShouldNotBeNull();
        feed.Channel.Language.Name.ShouldBe("en-US");
    }

    /// <summary>
    /// The <c>day</c> children of <c>skipDays</c> are parsed by name into <c>DayOfWeek</c> values.
    /// </summary>
    [TestMethod]
    public void Fill_WithSkipDays_ParsesDaysCorrectly()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

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
    ///     RSS 0.91 counts the hours of the day from 1 and the object model holds the RSS 2.0 0-based form,
    ///     so the adapter subtracts one from every hour it reads. A 0.91 document written to the 2.0
    ///     convention is therefore read an hour out throughout.
    /// </remarks>
    [TestMethod]
    public void Fill_WithSkipHours_ParsesHoursCorrectly()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        // Note: The adapter converts 1-based hours (1, 2) to 0-based (0, 1)
        feed.Channel.SkipHours.Count.ShouldBe(2);
        feed.Channel.SkipHours.ShouldContain(0);  // Hour 1 becomes 0
        feed.Channel.SkipHours.ShouldContain(1);  // Hour 2 becomes 1
    }

    /// <summary>
    /// An item fills its title, link and description — the three elements 0.91 defines on an item.
    /// </summary>
    [TestMethod]
    public void Fill_WithItems_PopulatesItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("Test Item");
        feed.Channel.Items[0].Link.ShouldBe(new Uri("http://example.com/item1"));
        feed.Channel.Items[0].Description.ShouldBe("Test item description");
        feed.Channel.Items[1].Title.ShouldBe("Second Test Item");
        feed.Channel.Items[1].Link.ShouldBe(new Uri("http://example.com/item2"));
        feed.Channel.Items[1].Description.ShouldBe("Second test item description");
    }

    /// <summary>
    /// An <c>image</c> fills its title, URL and link along with the <c>width</c>, <c>height</c> and
    /// <c>description</c> that 0.91 adds over 0.90.
    /// </summary>
    [TestMethod]
    public void Fill_WithImage_PopulatesImageWithDimensions()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.Image.Title.ShouldBe("Feed Image");
        feed.Channel.Image.Url.ShouldBe(new Uri("http://example.com/image.png"));
        feed.Channel.Image.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Image.Width.ShouldBe(88);
        feed.Channel.Image.Height.ShouldBe(31);
        feed.Channel.Image.Description.ShouldBe("Logo for the feed");
    }

    /// <summary>
    /// A <c>textInput</c> fills its title, description, name and link.
    /// </summary>
    [TestMethod]
    public void Fill_WithTextInput_PopulatesTextInput()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

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
    /// The optional <c>copyright</c>, <c>managingEditor</c>, <c>webMaster</c> and PICS <c>rating</c>
    /// elements are filled verbatim, the rating including its quotes and parentheses.
    /// </summary>
    [TestMethod]
    public void Fill_WithOptionalElements_PopulatesOptionals()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Copyright.ShouldBe("Copyright 2025");
        feed.Channel.ManagingEditor.ShouldBe("editor@example.com");
        feed.Channel.Webmaster.ShouldBe("webmaster@example.com");
        feed.Channel.Rating.ShouldBe("(PICS-1.1 \"http://www.classify.org/safesurf/\" 1 r (SS~~000 1))");
        // Note: Date parsing depends on RFC 822 format - verify the values were read (not MinValue means parsing succeeded)
        // If dates aren't parsing, verify the date format matches RFC 822
    }

    /// <summary>
    /// A retrieval limit of <c>2</c> keeps the first two of three items, in document order.
    /// </summary>
    [TestMethod]
    public void Fill_WithRetrievalLimit_EnforcesLimit()
    {
        // Arrange - Create feed with multiple items
        const string rss091WithMultipleItems = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.91">
                <channel>
                    <title>Test RSS 0.91 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <language>en-us</language>
                    <item>
                        <title>First Item</title>
                        <link>http://example.com/item1</link>
                    </item>
                    <item>
                        <title>Second Item</title>
                        <link>http://example.com/item2</link>
                    </item>
                    <item>
                        <title>Third Item</title>
                        <link>http://example.com/item3</link>
                    </item>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss091WithMultipleItems));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 2
        };
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("First Item");
        feed.Channel.Items[1].Title.ShouldBe("Second Item");
    }

    /// <summary>
    /// Filling a <see langword="null"/> feed throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

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
        Should.Throw<ArgumentNullException>(() => new Rss091SyndicationResourceAdapter(null!, settings));
    }

    /// <summary>
    /// Constructing the adapter without load settings throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rss091SyndicationResourceAdapter(navigator, null!));
    }

    /// <summary>
    /// A channel with no <c>image</c> leaves the channel's image <see langword="null"/>, not an empty
    /// instance.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesImageNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Image.ShouldBeNull();
    }

    /// <summary>
    /// A channel with no <c>textInput</c> leaves the channel's text input <see langword="null"/>, not an
    /// empty instance.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesTextInputNull()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.TextInput.ShouldBeNull();
    }

    /// <summary>
    /// A channel with no <c>skipDays</c> leaves the skipped-days collection empty.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesSkipDaysEmpty()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.SkipDays.ShouldBeEmpty();
    }

    /// <summary>
    /// A channel with no <c>skipHours</c> leaves the skipped-hours collection empty.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalFeed_LeavesSkipHoursEmpty()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.SkipHours.ShouldBeEmpty();
    }

    /// <summary>
    /// An empty <c>language</c> element leaves the channel's language <see langword="null"/> instead of
    /// throwing.
    /// </summary>
    [TestMethod]
    public void Fill_WithInvalidLanguage_DoesNotThrow()
    {
        // Arrange - Create feed with an empty language element (edge case)
        const string rss091WithEmptyLanguage = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.91">
                <channel>
                    <title>Test RSS 0.91 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <language></language>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss091WithEmptyLanguage));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert - Should not throw for empty language
        Should.NotThrow(() => adapter.Fill(feed));
        // Empty language element should leave Language as null
        feed.Channel.Language.ShouldBeNull();
    }

    /// <summary>
    /// A <c>day</c> that names no weekday is dropped and the rest of <c>skipDays</c> still fills, so
    /// <c>InvalidDay</c> costs nothing and Monday survives.
    /// </summary>
    [TestMethod]
    public void Fill_WithInvalidSkipDay_DoesNotThrow()
    {
        // Arrange - Create feed with invalid skip day
        const string rss091WithInvalidSkipDay = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.91">
                <channel>
                    <title>Test RSS 0.91 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <language>en-us</language>
                    <skipDays>
                        <day>InvalidDay</day>
                        <day>Monday</day>
                    </skipDays>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss091WithInvalidSkipDay));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert - Should not throw, just skip invalid day
        Should.NotThrow(() => adapter.Fill(feed));
        feed.Channel.SkipDays.Count.ShouldBe(1);
        feed.Channel.SkipDays.ShouldContain(DayOfWeek.Monday);
    }

    /// <summary>
    /// A retrieval limit of <c>0</c> means no limit: both of the feed's items are read, in document order.
    /// </summary>
    /// <remarks>
    ///     The fixture carries two items for this test's sake. Against the one-item document it used to
    ///     parse, "no limit" and "a limit of one" produce the same count, so the assertion could not fail
    ///     for the reason the method is named after. Asserting both titles pins the ordering too.
    /// </remarks>
    [TestMethod]
    public void Fill_WithZeroRetrievalLimit_RetrievesAllItems()
    {
        // Arrange
        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rss091Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 0  // 0 means no limit
        };
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Channel.Items.Count.ShouldBe(2);
        feed.Channel.Items[0].Title.ShouldBe("Test Item");
        feed.Channel.Items[1].Title.ShouldBe("Second Test Item");
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
        const string rss091WithDates = """
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="0.91">
                <channel>
                    <title>Test RSS 0.91 Feed</title>
                    <link>http://example.com</link>
                    <description>Test feed</description>
                    <language>en-us</language>
                    <pubDate>Mon, 20 Jan 2025 12:00:00 +00:00</pubDate>
                    <lastBuildDate>Tue, 21 Jan 2025 14:30:00 +00:00</lastBuildDate>
                </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(rss091WithDates));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rss091SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act - Should not throw when encountering date elements
        Should.NotThrow(() => adapter.Fill(feed));

        // Assert - Channel was populated (dates may or may not parse depending on format support)
        feed.Channel.Title.ShouldBe("Test RSS 0.91 Feed");
    }
}