using System.Text;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="Atom03SyndicationResourceAdapter"/>: what the Atom 0.3 vocabulary in
/// <c>http://purl.org/atom/ns#</c> fills on an <see cref="AtomFeed"/>, including the elements RFC 4287
/// later renamed — <c>modified</c>, <c>tagline</c>, <c>copyright</c>, <c>created</c> and a person's
/// <c>url</c> — the <c>mode</c> attribute that types a text construct, the retrieval limit, and the
/// argument guards.
/// </summary>
[TestClass]
public class Atom03SyndicationResourceAdapterTests
{
    #region Feed Parsing Tests

    /// <summary>
    /// A minimal Atom 0.3 feed fills the title, the <c>id</c> as a URN, and the <c>modified</c> date that
    /// Atom 1.0 spells <c>updated</c>.
    /// </summary>
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

    /// <summary>
    /// A full Atom 0.3 feed fills every construct it carries, with <c>tagline</c> reaching <c>Subtitle</c>
    /// and <c>copyright</c> reaching <c>Rights</c>.
    /// </summary>
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

    /// <summary>
    /// The single <c>entry</c> of an Atom 0.3 feed is filled with its own title, <c>id</c>, <c>modified</c>
    /// date and links.
    /// </summary>
    /// <remarks>
    ///     Reached through <c>Fill(AtomFeed)</c> rather than <c>Fill(AtomEntry)</c>: the file records that
    ///     the entry overload selects an <c>atom:entry</c> child, so it wants a navigator sitting above the
    ///     entry rather than on it.
    /// </remarks>
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

    /// <summary>
    /// A text construct written <c>mode="escaped"</c> is typed <see cref="AtomTextConstructType.Html"/>.
    /// </summary>
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

    /// <summary>
    /// An entry's <c>content</c> written <c>mode="xml"</c> yields the text of the <c>xhtml:div</c> it wraps.
    /// </summary>
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
        feed.Entries[0].Content!.Content.ShouldBe("Entry content in XHTML");
    }

    #endregion

    #region Person Construct Tests

    /// <summary>
    /// An Atom 0.3 <c>author</c> fills a name, an email address, and the <c>url</c> element that Atom 1.0
    /// renamed <c>uri</c>.
    /// </summary>
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

    /// <summary>
    /// A feed-level <c>contributor</c> fills its own name and email address, separately from the author.
    /// </summary>
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

    /// <summary>
    /// The <c>generator</c> element fills its text, its <c>url</c> attribute and its <c>version</c>.
    /// </summary>
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

    /// <summary>
    /// An entry carrying its own <c>author</c> keeps that author, rather than the feed-level one.
    /// </summary>
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

    /// <summary>
    /// An entry's <c>summary</c> fills its content, typed HTML by its <c>mode="escaped"</c>.
    /// </summary>
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
        feed.Entries[0].Summary!.Content.ShouldBe("Entry summary");
        feed.Entries[0].Summary!.TextType.ShouldBe(AtomTextConstructType.Html);
    }

    /// <summary>
    /// Atom 0.3's <c>created</c> element fills the entry's <c>PublishedOn</c>, which Atom 1.0 spells
    /// <c>published</c>.
    /// </summary>
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

    /// <summary>
    /// Filling a <see langword="null"/> feed throws <c>ArgumentNullException</c>.
    /// </summary>
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

    /// <summary>
    /// Filling a <see langword="null"/> entry throws <c>ArgumentNullException</c>.
    /// </summary>
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

    /// <summary>
    /// Constructing the adapter without a navigator throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        SyndicationResourceLoadSettings settings = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Atom03SyndicationResourceAdapter(null!, settings));
    }

    /// <summary>
    /// Constructing the adapter without load settings throws <c>ArgumentNullException</c>.
    /// </summary>
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

    /// <summary>
    /// A feed <c>link</c> fills its href, its <c>rel</c> of <c>alternate</c> and its <c>type</c> of
    /// <c>text/html</c>.
    /// </summary>
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

    /// <summary>
    /// A retrieval limit of <c>2</c> stops a three-entry feed at two entries.
    /// </summary>
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

    #region Entry Document Tests

    /// <summary>
    /// A stand-alone Atom 0.3 entry document fills its title, its <c>id</c> and its <c>modified</c> date.
    /// </summary>
    /// <remarks>
    ///     The overload used to build its namespace manager with <c>AtomUtility</c>, which binds
    ///     <c>atom</c> to the Atom <i>1.0</i> namespace. Selection resolves a prefix to a namespace URI and
    ///     matches on the URI, so that manager could never match an element in
    ///     <c>http://purl.org/atom/ns#</c>: the overload threw for every input it exists to handle, with a
    ///     message saying the document was not entry-rooted when it was.
    /// </remarks>
    [TestMethod]
    public void Fill_Atom03EntryDocument_PopulatesEntry()
    {
        // Arrange
        AtomEntry entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Entry));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(entry);

        // Assert
        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Test Entry");
        entry.Id.ShouldNotBeNull();
        entry.Id.Uri.ShouldBe(new Uri("urn:uuid:entry-1"));
        entry.UpdatedOn.Year.ShouldBe(2025);
        entry.Links.Count.ShouldBe(1);
    }

    /// <summary>
    /// A feed document handed to the entry overload still throws, and says which root it wanted.
    /// </summary>
    [TestMethod]
    public void Fill_Atom03FeedDocumentAsAnEntry_Throws()
    {
        // Arrange
        AtomEntry entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Feed));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<FormatException>(() => adapter.Fill(entry));
        entry.Title.ShouldBeNull();
        entry.Id.ShouldBeNull();
    }

    /// <summary>
    /// A feed-shaped document in some namespace other than Atom 0.3 fills nothing.
    /// </summary>
    /// <remarks>
    ///     A guard for the invariant behind the adapter's own namespace manager: <c>atom</c> is bound to
    ///     the constant <c>http://purl.org/atom/ns#</c> and never to whatever default namespace the
    ///     document declares. Binding the document's own default would make any feed-shaped document parse
    ///     as though it were Atom 0.3. Green before and after the dead ternary that expressed the other
    ///     intention is removed.
    /// </remarks>
    [TestMethod]
    public void Fill_FeedInAnotherDefaultNamespace_FillsNothing()
    {
        // Arrange
        const string foreignFeed = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed version="0.3" xmlns="http://example.com/not-atom">
                <title>Not An Atom 0.3 Feed</title>
                <id>urn:uuid:not-atom</id>
                <modified>2025-01-20T12:00:00Z</modified>
            </feed>
            """;

        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(foreignFeed));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Atom03SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<FormatException>(() => adapter.Fill(feed));
        feed.Title.ShouldBeNull();
        feed.Id.ShouldBeNull();
    }

    #endregion
}