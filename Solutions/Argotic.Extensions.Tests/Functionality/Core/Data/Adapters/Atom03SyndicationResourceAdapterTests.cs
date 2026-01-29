using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Unit tests for <see cref="Atom03SyndicationResourceAdapter"/> that verify Atom 0.3 parsing.
/// </summary>
[TestClass]
public class Atom03SyndicationResourceAdapterTests
{
    #region Feed Parsing Tests

    [TestMethod]
    public void Fill_MinimalAtom03Feed_PopulatesFeed()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Feed));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Atom 0.3 Feed");
        feed.Id.ShouldNotBeNull();
        feed.Id.Uri.ShouldBe(new Uri("urn:uuid:12345678-1234-1234-1234-123456789012"));
        feed.UpdatedOn.ShouldNotBe(DateTime.MinValue);
    }

    [TestMethod]
    public void Fill_FullAtom03Feed_PopulatesAllProperties()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        // Title
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Atom 0.3 Feed");
        feed.Title.TextType.ShouldBe(AtomTextConstructType.Html);

        // Id and Updated
        feed.Id.ShouldNotBeNull();
        feed.UpdatedOn.Year.ShouldBe(2025);

        // Links
        feed.Links.Count.ShouldBeGreaterThan(0);

        // Authors
        feed.Authors.Count.ShouldBe(1);
        feed.Authors[0].Name.ShouldBe("Test Author");
        feed.Authors[0].Uri.ShouldBe(new Uri("http://example.com/author"));
        feed.Authors[0].EmailAddress.ShouldBe("author@example.com");

        // Contributors
        feed.Contributors.Count.ShouldBe(1);
        feed.Contributors[0].Name.ShouldBe("Test Contributor");
        feed.Contributors[0].EmailAddress.ShouldBe("contributor@example.com");

        // Generator
        feed.Generator.ShouldNotBeNull();
        feed.Generator.Content.ShouldBe("Test Generator");
        feed.Generator.Uri.ShouldBe(new Uri("http://example.com/generator"));
        feed.Generator.Version.ShouldBe("1.0");

        // Rights (copyright in Atom 0.3)
        feed.Rights.ShouldNotBeNull();
        feed.Rights.Content.ShouldBe("Copyright 2025");

        // Subtitle (tagline in Atom 0.3)
        feed.Subtitle.ShouldNotBeNull();
        feed.Subtitle.Content.ShouldBe("A tagline for the feed");

        // Entries
        feed.Entries.Count.ShouldBe(1);
    }

    [TestMethod]
    public void Fill_Atom03EntryFromFeed_PopulatesEntry()
    {
        // Arrange - Note: Fill(AtomEntry) expects an entry wrapped in a feed or with atom:entry XPath available.
        // Testing the entry parsing by loading a full feed and checking entries instead.
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert - Check the entry within the feed
        feed.Entries.Count.ShouldBe(1);
        AtomEntry entry = feed.Entries[0];
        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Test Entry");
        entry.Id.ShouldNotBeNull();
        entry.Id.Uri.ShouldBe(new Uri("urn:uuid:entry-1"));
        entry.UpdatedOn.Year.ShouldBe(2025);
        entry.Links.Count.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Text Construct Mode Tests

    [TestMethod]
    public void Fill_WithTextConstructEscapedMode_ParsesAsHtml()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert - title has mode="escaped" which maps to Html
        feed.Title.ShouldNotBeNull();
        feed.Title.TextType.ShouldBe(AtomTextConstructType.Html);
    }

    [TestMethod]
    public void Fill_WithXmlModeContent_ParsesXhtmlContent()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert - entry content has mode="xml" which maps to Xhtml
        feed.Entries.Count.ShouldBe(1);
        feed.Entries[0].Content.ShouldNotBeNull();
        feed.Entries[0].Content.Content.ShouldBe("Entry content in XHTML");
    }

    #endregion

    #region Person Construct Tests

    [TestMethod]
    public void Fill_WithPersonConstruct_PopulatesAuthor()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Authors.Count.ShouldBe(1);
        AtomPersonConstruct author = feed.Authors[0];
        author.Name.ShouldBe("Test Author");
        author.Uri.ShouldBe(new Uri("http://example.com/author"));
        author.EmailAddress.ShouldBe("author@example.com");
    }

    [TestMethod]
    public void Fill_WithContributors_PopulatesContributors()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Contributors.Count.ShouldBe(1);
        feed.Contributors[0].Name.ShouldBe("Test Contributor");
        feed.Contributors[0].EmailAddress.ShouldBe("contributor@example.com");
    }

    #endregion

    #region Generator Tests

    [TestMethod]
    public void Fill_WithGenerator_PopulatesGenerator()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Generator.ShouldNotBeNull();
        feed.Generator.Content.ShouldBe("Test Generator");
        feed.Generator.Uri.ShouldBe(new Uri("http://example.com/generator"));
        feed.Generator.Version.ShouldBe("1.0");
    }

    #endregion

    #region Entry Collections Tests

    [TestMethod]
    public void Fill_EntryWithAuthors_PopulatesEntryAuthors()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Entries.Count.ShouldBe(1);
        feed.Entries[0].Authors.Count.ShouldBe(1);
        feed.Entries[0].Authors[0].Name.ShouldBe("Entry Author");
    }

    [TestMethod]
    public void Fill_EntryWithSummary_PopulatesSummary()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Entries[0].Summary.ShouldNotBeNull();
        feed.Entries[0].Summary.Content.ShouldBe("Entry summary");
        feed.Entries[0].Summary.TextType.ShouldBe(AtomTextConstructType.Html);
    }

    [TestMethod]
    public void Fill_EntryWithCreated_PopulatesPublishedOn()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        // The created element in Atom 0.3 maps to PublishedOn
        feed.Entries[0].PublishedOn.ShouldNotBe(DateTime.MinValue);
        feed.Entries[0].PublishedOn.Year.ShouldBe(2025);
        feed.Entries[0].PublishedOn.Month.ShouldBe(1);
        feed.Entries[0].PublishedOn.Day.ShouldBe(19);
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public void Fill_NullFeedResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Feed));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill((AtomFeed)null!));
    }

    [TestMethod]
    public void Fill_NullEntryResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Entry));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill((AtomEntry)null!));
    }

    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Atom03SyndicationResourceAdapter(null!, settings));
    }

    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Feed));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Atom03SyndicationResourceAdapter(navigator, null!));
    }

    #endregion

    #region Links Tests

    [TestMethod]
    public void Fill_WithLinks_PopulatesLinks()
    {
        // Arrange
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03FeedFull));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Links.Count.ShouldBeGreaterThan(0);
        AtomLink link = feed.Links[0];
        link.Uri.ShouldBe(new Uri("http://example.com"));
        link.Relation.ShouldBe("alternate");
        link.ContentType.ShouldBe("text/html");
    }

    #endregion

    #region Retrieval Limit Tests

    [TestMethod]
    public void Fill_WithRetrievalLimit_RespectsLimit()
    {
        // Arrange - Create a feed with multiple entries
        const string atomWithMultipleEntries = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed version="0.3" xmlns="http://purl.org/atom/ns#">
                <title>Test Feed</title>
                <id>urn:uuid:test</id>
                <modified>2025-01-20T12:00:00Z</modified>
                <entry>
                    <title>Entry 1</title>
                    <id>urn:uuid:entry-1</id>
                    <modified>2025-01-20T12:00:00Z</modified>
                </entry>
                <entry>
                    <title>Entry 2</title>
                    <id>urn:uuid:entry-2</id>
                    <modified>2025-01-20T12:00:00Z</modified>
                </entry>
                <entry>
                    <title>Entry 3</title>
                    <id>urn:uuid:entry-3</id>
                    <modified>2025-01-20T12:00:00Z</modified>
                </entry>
            </feed>
            """;

        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(atomWithMultipleEntries));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 2
        };
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(feed);

        // Assert
        feed.Entries.Count.ShouldBe(2);
    }

    #endregion
}