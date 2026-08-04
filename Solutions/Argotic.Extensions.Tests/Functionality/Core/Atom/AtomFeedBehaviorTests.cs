using System.Xml;

using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Behavior-driven tests for <see cref="AtomFeed"/> covering creation, parsing, and round-trip scenarios.
/// </summary>
[TestClass]
public class AtomFeedBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Feed Creation Tests

    [TestMethod]
    public void AtomFeed_WhenCreatedWithRequiredProperties_ContainsCorrectValues()
    {
        // Arrange
        AtomId id = new(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"));
        AtomTextConstruct title = new("My Test Feed");
        DateTime updatedOn = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        AtomFeed feed = new(id, title, updatedOn);

        // Assert
        feed.Id.ShouldNotBeNull();
        feed.Id.Uri.ShouldBe(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"));
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("My Test Feed");
        feed.UpdatedOn.ShouldBe(updatedOn);
    }

    [TestMethod]
    public void AtomFeed_WhenEntriesAdded_ContainsAllEntries()
    {
        // Arrange
        AtomFeed feed = new();
        AtomEntry entry1 = new()
        {
            Id = new AtomId(new Uri("urn:uuid:entry-1")),
            Title = new AtomTextConstruct("First Entry"),
            UpdatedOn = DateTime.UtcNow
        };
        AtomEntry entry2 = new()
        {
            Id = new AtomId(new Uri("urn:uuid:entry-2")),
            Title = new AtomTextConstruct("Second Entry"),
            UpdatedOn = DateTime.UtcNow
        };

        // Act
        feed.Entries.Add(entry1);
        feed.Entries.Add(entry2);

        // Assert
        feed.Entries.Count.ShouldBe(2);
        feed.Entries[0].Title!.Content.ShouldBe("First Entry");
        feed.Entries[1].Title!.Content.ShouldBe("Second Entry");
    }

    [TestMethod]
    public void AtomFeed_WhenAuthorsAdded_ContainsAllAuthors()
    {
        // Arrange
        AtomFeed feed = new();
        AtomPersonConstruct author1 = new("John Doe")
        {
            EmailAddress = "john@example.com"
        };
        AtomPersonConstruct author2 = new("Jane Smith")
        {
            EmailAddress = "jane@example.com",
            Uri = new Uri("http://example.com/jane")
        };

        // Act
        feed.Authors.Add(author1);
        feed.Authors.Add(author2);

        // Assert
        feed.Authors.Count.ShouldBe(2);
        feed.Authors[0].Name.ShouldBe("John Doe");
        feed.Authors[0].EmailAddress.ShouldBe("john@example.com");
        feed.Authors[1].Name.ShouldBe("Jane Smith");
        feed.Authors[1].Uri.ShouldBe(new Uri("http://example.com/jane"));
    }

    [TestMethod]
    public void AtomFeed_WhenLinksAdded_ContainsAllLinks()
    {
        // Arrange
        AtomFeed feed = new();
        AtomLink selfLink = new(new Uri("http://example.com/feed.xml"), "self");
        AtomLink alternateLink = new(new Uri("http://example.com/"), "alternate")
        {
            ContentType = "text/html",
            Title = "Website"
        };

        // Act
        feed.Links.Add(selfLink);
        feed.Links.Add(alternateLink);

        // Assert
        feed.Links.Count.ShouldBe(2);
        feed.Links[0].Relation.ShouldBe("self");
        feed.Links[1].Relation.ShouldBe("alternate");
        feed.Links[1].ContentType.ShouldBe("text/html");
    }

    [TestMethod]
    public void AtomFeed_WhenCategoriesAdded_ContainsAllCategories()
    {
        // Arrange
        AtomFeed feed = new();
        AtomCategory category1 = new("technology");
        AtomCategory category2 = new("news")
        {
            Scheme = new Uri("http://example.com/categories"),
            Label = "News Articles"
        };

        // Act
        feed.Categories.Add(category1);
        feed.Categories.Add(category2);

        // Assert
        feed.Categories.Count.ShouldBe(2);
        feed.Categories[0].Term.ShouldBe("technology");
        feed.Categories[1].Term.ShouldBe("news");
        feed.Categories[1].Scheme.ShouldBe(new Uri("http://example.com/categories"));
        feed.Categories[1].Label.ShouldBe("News Articles");
    }

    #endregion

    #region Feed Parsing Tests

    [TestMethod]
    public void AtomFeed_WhenLoadedFromValidXml_PopulatesProperties()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;
        AtomFeed feed = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Feed");
        feed.Id.ShouldNotBeNull();
        feed.Id.Uri.ShouldBe(new Uri("urn:uuid:12345678-1234-1234-1234-123456789012"));
    }

    [TestMethod]
    public void AtomFeed_WhenLoadedFromAtomWithEntries_PopulatesEntryCollection()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;
        AtomFeed feed = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Feed");
        feed.Entries.ShouldNotBeEmpty();
        feed.Entries.Count.ShouldBe(2);
        feed.Entries[0].Title!.Content.ShouldBe("Recent Entry");
        feed.Entries[1].Title!.Content.ShouldBe("Old Entry");
    }

    [TestMethod]
    public void AtomFeed_WhenLoadedFromAtomWithEntries_PopulatesEntryCategories()
    {
        // Arrange
        string xml = FeedTestData.AtomWithEntries;
        AtomFeed feed = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        AtomEntry firstEntry = feed.Entries[0];
        firstEntry.Categories.Count.ShouldBe(2);
        firstEntry.Categories[0].Term.ShouldBe("Tech");
        firstEntry.Categories[1].Term.ShouldBe("News");
    }

    [TestMethod]
    public void AtomFeed_WhenLoadedFromMalformedXml_ThrowsXmlException()
    {
        // Arrange
        string malformedXml = FeedTestData.MalformedXml;
        AtomFeed feed = new();

        // Act & Assert
        Should.Throw<XmlException>(() =>
        {
            using XmlReader reader = XmlReader.Create(new StringReader(malformedXml));
            feed.Load(reader);
        });
    }

    [TestMethod]
    public void AtomFeed_WhenLoadedFromStream_PopulatesProperties()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;
        AtomFeed feed = new();

        // Act
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        // Assert
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Feed");
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void AtomFeed_WhenSavedAndReloaded_PreservesBasicProperties()
    {
        // Arrange
        AtomFeed originalFeed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:round-trip-test")),
            Title = new AtomTextConstruct("Round Trip Feed"),
            UpdatedOn = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Subtitle = new AtomTextConstruct("A feed for testing round trips"),
            Rights = new AtomTextConstruct("Copyright 2025")
        };

        // Act
        using MemoryStream stream = new();
        originalFeed.Save(stream);
        stream.Position = 0;

        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Id!.Uri.ShouldBe(originalFeed.Id.Uri);
        loadedFeed.Title!.Content.ShouldBe(originalFeed.Title.Content);
        loadedFeed.Subtitle.ShouldNotBeNull();
        loadedFeed.Subtitle.Content.ShouldBe(originalFeed.Subtitle.Content);
        loadedFeed.Rights.ShouldNotBeNull();
        loadedFeed.Rights.Content.ShouldBe(originalFeed.Rights.Content);
    }

    [TestMethod]
    public void AtomFeed_WhenSavedAndReloaded_PreservesEntries()
    {
        // Arrange
        AtomFeed originalFeed = CreateFeedWithEntries();

        // Act
        using MemoryStream stream = new();
        originalFeed.Save(stream);
        stream.Position = 0;

        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Entries.Count.ShouldBe(originalFeed.Entries.Count);
        for (int i = 0; i < originalFeed.Entries.Count; i++)
        {
            loadedFeed.Entries[i].Title!.Content.ShouldBe(originalFeed.Entries[i].Title!.Content);
            loadedFeed.Entries[i].Id!.Uri.ShouldBe(originalFeed.Entries[i].Id!.Uri);
        }
    }

    [TestMethod]
    public void AtomFeed_WhenSavedAndReloaded_PreservesEntrySummary()
    {
        // Arrange
        AtomFeed originalFeed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:summary-test")),
            Title = new AtomTextConstruct("Summary Test Feed"),
            UpdatedOn = DateTime.UtcNow
        };
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:entry-with-summary")),
            Title = new AtomTextConstruct("Entry With Summary"),
            UpdatedOn = DateTime.UtcNow,
            Summary = new AtomTextConstruct("This is the entry summary text.")
        };
        originalFeed.Entries.Add(entry);

        // Act
        using MemoryStream stream = new();
        originalFeed.Save(stream);
        stream.Position = 0;

        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Entries[0].Summary.ShouldNotBeNull();
        loadedFeed.Entries[0].Summary!.Content.ShouldBe("This is the entry summary text.");
    }

    [TestMethod]
    public void AtomFeed_WhenSavedAndReloadedTwice_MaintainsDataIntegrity()
    {
        // Arrange
        AtomFeed originalFeed = CreateCompleteFeed();

        // Act - First round trip
        using MemoryStream stream1 = new();
        originalFeed.Save(stream1);
        stream1.Position = 0;
        AtomFeed feed1 = new();
        feed1.Load(stream1);

        // Act - Second round trip
        using MemoryStream stream2 = new();
        feed1.Save(stream2);
        stream2.Position = 0;
        AtomFeed feed2 = new();
        feed2.Load(stream2);

        // Assert
        feed2.Title!.Content.ShouldBe(originalFeed.Title!.Content);
        feed2.Entries.Count.ShouldBe(originalFeed.Entries.Count);
        feed2.Authors.Count.ShouldBe(originalFeed.Authors.Count);
        feed2.Categories.Count.ShouldBe(originalFeed.Categories.Count);
    }

    #endregion

    #region AtomTextConstruct Type Tests

    [TestMethod]
    public void AtomTextConstruct_WhenCreatedWithPlainText_HasCorrectType()
    {
        // Arrange & Act
        AtomTextConstruct textConstruct = new("Plain text content")
        {
            TextType = AtomTextConstructType.Text
        };

        // Assert
        textConstruct.Content.ShouldBe("Plain text content");
        textConstruct.TextType.ShouldBe(AtomTextConstructType.Text);
    }

    [TestMethod]
    public void AtomTextConstruct_WhenCreatedWithHtml_HasCorrectType()
    {
        // Arrange & Act
        AtomTextConstruct textConstruct = new("<p>HTML content</p>")
        {
            TextType = AtomTextConstructType.Html
        };

        // Assert
        textConstruct.Content.ShouldBe("<p>HTML content</p>");
        textConstruct.TextType.ShouldBe(AtomTextConstructType.Html);
    }

    [TestMethod]
    public void AtomTextConstruct_WhenCreatedWithXhtml_HasCorrectType()
    {
        // Arrange & Act
        AtomTextConstruct textConstruct = new("XHTML content")
        {
            TextType = AtomTextConstructType.Xhtml
        };

        // Assert
        textConstruct.TextType.ShouldBe(AtomTextConstructType.Xhtml);
    }

    [TestMethod]
    public void AtomTextConstruct_WhenSavedAndReloaded_PreservesTextType()
    {
        // Arrange
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:text-type-test")),
            Title = new AtomTextConstruct("Feed with HTML Title")
            {
                TextType = AtomTextConstructType.Html
            },
            UpdatedOn = DateTime.UtcNow
        };

        // Act
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Position = 0;
        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Title!.TextType.ShouldBe(AtomTextConstructType.Html);
    }

    [TestMethod]
    public void AtomTextConstruct_TypeConversion_ReturnsCorrectString()
    {
        // Assert
        AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Html).ShouldBe("html");
        AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Text).ShouldBe("text");
        AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Xhtml).ShouldBe("xhtml");
    }

    [TestMethod]
    public void AtomTextConstruct_TypeConversionByName_ReturnsCorrectEnum()
    {
        // Assert
        AtomTextConstruct.ConstructTypeByName("html").ShouldBe(AtomTextConstructType.Html);
        AtomTextConstruct.ConstructTypeByName("text").ShouldBe(AtomTextConstructType.Text);
        AtomTextConstruct.ConstructTypeByName("xhtml").ShouldBe(AtomTextConstructType.Xhtml);
        AtomTextConstruct.ConstructTypeByName("HTML").ShouldBe(AtomTextConstructType.Html); // Case insensitive
    }

    #endregion

    #region AtomPersonConstruct Tests

    [TestMethod]
    public void AtomPersonConstruct_WhenCreatedWithAllProperties_ContainsCorrectValues()
    {
        // Arrange & Act
        AtomPersonConstruct person = new("John Doe")
        {
            EmailAddress = "john@example.com",
            Uri = new Uri("http://example.com/john")
        };

        // Assert
        person.Name.ShouldBe("John Doe");
        person.EmailAddress.ShouldBe("john@example.com");
        person.Uri.ShouldBe(new Uri("http://example.com/john"));
    }

    [TestMethod]
    public void AtomPersonConstruct_WhenSavedAndReloaded_PreservesAllProperties()
    {
        // Arrange
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:person-test")),
            Title = new AtomTextConstruct("Person Test Feed"),
            UpdatedOn = DateTime.UtcNow
        };
        feed.Authors.Add(new AtomPersonConstruct("Author Name")
        {
            EmailAddress = "author@example.com",
            Uri = new Uri("http://example.com/author")
        });
        feed.Contributors.Add(new AtomPersonConstruct("Contributor Name")
        {
            EmailAddress = "contributor@example.com"
        });

        // Act
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Position = 0;
        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Authors.Count.ShouldBe(1);
        loadedFeed.Authors[0].Name.ShouldBe("Author Name");
        loadedFeed.Authors[0].EmailAddress.ShouldBe("author@example.com");
        loadedFeed.Authors[0].Uri.ShouldBe(new Uri("http://example.com/author"));
        loadedFeed.Contributors.Count.ShouldBe(1);
        loadedFeed.Contributors[0].Name.ShouldBe("Contributor Name");
    }

    #endregion

    #region AtomLink Tests

    [TestMethod]
    public void AtomLink_WhenCreatedWithRelAttribute_ContainsCorrectValues()
    {
        // Arrange & Act
        AtomLink link = new(new Uri("http://example.com/"), "alternate");

        // Assert
        link.Uri.ShouldBe(new Uri("http://example.com/"));
        link.Relation.ShouldBe("alternate");
    }

    [TestMethod]
    public void AtomLink_WhenCreatedWithAllAttributes_ContainsCorrectValues()
    {
        // Arrange & Act
        AtomLink link = new(new Uri("http://example.com/feed.xml"), "self")
        {
            ContentType = "application/atom+xml",
            Title = "Feed Link",
            Length = 1024
        };

        // Assert
        link.Relation.ShouldBe("self");
        link.ContentType.ShouldBe("application/atom+xml");
        link.Title.ShouldBe("Feed Link");
        link.Length.ShouldBe(1024);
    }

    [TestMethod]
    public void AtomLink_WhenSavedAndReloaded_PreservesAllAttributes()
    {
        // Arrange
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:link-test")),
            Title = new AtomTextConstruct("Link Test Feed"),
            UpdatedOn = DateTime.UtcNow
        };
        feed.Links.Add(new AtomLink(new Uri("http://example.com/feed.xml"), "self")
        {
            ContentType = "application/atom+xml",
            Title = "Self Link"
        });
        feed.Links.Add(new AtomLink(new Uri("http://example.com/"), "alternate")
        {
            ContentType = "text/html"
        });
        feed.Links.Add(new AtomLink(new Uri("http://example.com/enclosure.mp3"), "enclosure")
        {
            ContentType = "audio/mpeg",
            Length = 5000000
        });

        // Act
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Position = 0;
        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Links.Count.ShouldBe(3);

        AtomLink selfLink = loadedFeed.Links.First(l => l.Relation == "self");
        selfLink.ContentType.ShouldBe("application/atom+xml");
        selfLink.Title.ShouldBe("Self Link");

        AtomLink alternateLink = loadedFeed.Links.First(l => l.Relation == "alternate");
        alternateLink.ContentType.ShouldBe("text/html");

        AtomLink enclosureLink = loadedFeed.Links.First(l => l.Relation == "enclosure");
        enclosureLink.ContentType.ShouldBe("audio/mpeg");
        enclosureLink.Length.ShouldBe(5000000);
    }

    [TestMethod]
    public void AtomLink_WithViaRelation_SetsCorrectly()
    {
        // Arrange & Act
        AtomLink link = new(new Uri("http://source.example.com/original"), "via");

        // Assert
        link.Relation.ShouldBe("via");
    }

    [TestMethod]
    public void AtomLink_WithRelatedRelation_SetsCorrectly()
    {
        // Arrange & Act
        AtomLink link = new(new Uri("http://example.com/related-resource"), "related");

        // Assert
        link.Relation.ShouldBe("related");
    }

    #endregion

    #region Feed Optional Properties Tests

    [TestMethod]
    public void AtomFeed_WhenGeneratorSet_SavesAndLoadsCorrectly()
    {
        // Arrange
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:generator-test")),
            Title = new AtomTextConstruct("Generator Test"),
            UpdatedOn = DateTime.UtcNow,
            Generator = new AtomGenerator("Test Generator")
            {
                Uri = new Uri("http://example.com/generator"),
                Version = "1.0.0"
            }
        };

        // Act
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Position = 0;
        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Generator.ShouldNotBeNull();
        loadedFeed.Generator.Content.ShouldBe("Test Generator");
        loadedFeed.Generator.Uri.ShouldBe(new Uri("http://example.com/generator"));
        loadedFeed.Generator.Version.ShouldBe("1.0.0");
    }

    [TestMethod]
    public void AtomFeed_WhenIconAndLogoSet_SavesAndLoadsCorrectly()
    {
        // Arrange
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:icon-logo-test")),
            Title = new AtomTextConstruct("Icon Logo Test"),
            UpdatedOn = DateTime.UtcNow,
            Icon = new AtomIcon(new Uri("http://example.com/icon.png")),
            Logo = new AtomLogo(new Uri("http://example.com/logo.png"))
        };

        // Act
        using MemoryStream stream = new();
        feed.Save(stream);
        stream.Position = 0;
        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Assert
        loadedFeed.Icon.ShouldNotBeNull();
        loadedFeed.Icon.Uri.ShouldBe(new Uri("http://example.com/icon.png"));
        loadedFeed.Logo.ShouldNotBeNull();
        loadedFeed.Logo.Uri.ShouldBe(new Uri("http://example.com/logo.png"));
    }

    #endregion

    #region Feed Format Tests

    [TestMethod]
    public void AtomFeed_Format_ReturnsAtom()
    {
        // Arrange
        AtomFeed feed = new();

        // Assert
        feed.Format.ShouldBe(Argotic.Common.SyndicationContentFormat.Atom);
    }

    [TestMethod]
    public void AtomFeed_Version_Returns1_0()
    {
        // Arrange
        AtomFeed feed = new();

        // Assert
        feed.Version.ShouldBe(new Version(1, 0));
    }

    #endregion

    #region Loaded Event Tests

    [TestMethod]
    public void AtomFeed_WhenLoaded_RaisesLoadedEvent()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        eventRaised.ShouldBeTrue();
    }

    #endregion

    #region Async Operations Tests

    [TestMethod]
    public async Task AtomFeed_LoadAsync_LoadsFeedCorrectly()
    {
        // Arrange
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(
            new Uri("http://example.com/feed.atom"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue();
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Feed");
        feed.Id.ShouldNotBeNull();
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithEntries_LoadsAllEntries()
    {
        // Arrange
        AtomFeed feed = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.AtomWithEntries);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(
            new Uri("http://example.com/feed.atom"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        feed.Entries.Count.ShouldBe(2);
        feed.Entries[0].Title!.Content.ShouldBe("Recent Entry");
        feed.Entries[1].Title!.Content.ShouldBe("Old Entry");
    }

    [TestMethod]
    public async Task AtomFeed_CreateAsync_CreatesAndLoadsNewFeed()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        // Act
        AtomFeed feed = await AtomFeed.CreateAsync(
            new Uri("http://example.com/feed.atom"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        feed.ShouldNotBeNull();
        feed.Title!.Content.ShouldBe("Test Feed");
        feed.Format.ShouldBe(Argotic.Common.SyndicationContentFormat.Atom);
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_WithSettings_AppliesSettings()
    {
        // Arrange
        const string atomWithExtension = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xmlns:dc="http://purl.org/dc/elements/1.1/">
                <title>Test Feed</title>
                <id>urn:uuid:12345678-1234-1234-1234-123456789012</id>
                <updated>2024-01-01T00:00:00Z</updated>
                <dc:creator>Test Author</dc:creator>
            </feed>
            """;

        AtomFeed feed = new();
        Argotic.Common.SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(atomWithExtension);
        using HttpClient httpClient = new(handler);

        // Act
        await feed.LoadAsync(
            new Uri("http://example.com/feed.atom"),
            httpClient,
            settings,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        feed.Title!.Content.ShouldBe("Test Feed");
        feed.HasExtensions.ShouldBeTrue();
    }

    [TestMethod]
    public async Task AtomFeed_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        AtomFeed feed = new();
        Uri? sourceFromEvent = null;

        feed.Loaded += (sender, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);
        Uri requestUri = new("http://example.com/feed.atom");

        // Act
        await feed.LoadAsync(requestUri, httpClient, cancellationToken: TestContext!.CancellationToken);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    #endregion

    #region Helper Methods

    private static AtomFeed CreateFeedWithEntries()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:entries-test")),
            Title = new AtomTextConstruct("Feed With Entries"),
            UpdatedOn = DateTime.UtcNow
        };

        for (int i = 1; i <= 3; i++)
        {
            feed.Entries.Add(new AtomEntry
            {
                Id = new AtomId(new Uri($"urn:uuid:entry-{i}")),
                Title = new AtomTextConstruct($"Entry {i}"),
                UpdatedOn = DateTime.UtcNow,
                Summary = new AtomTextConstruct($"Summary for entry {i}")
            });
        }

        return feed;
    }

    private static AtomFeed CreateCompleteFeed()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:complete-feed")),
            Title = new AtomTextConstruct("Complete Feed"),
            UpdatedOn = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Subtitle = new AtomTextConstruct("A complete feed with all properties"),
            Rights = new AtomTextConstruct("Copyright 2025"),
            Generator = new AtomGenerator("Test Generator")
        };

        feed.Authors.Add(new AtomPersonConstruct("Test Author")
        {
            EmailAddress = "author@example.com"
        });

        feed.Categories.Add(new AtomCategory("test"));
        feed.Categories.Add(new AtomCategory("complete"));

        feed.Links.Add(new AtomLink(new Uri("http://example.com/feed.xml"), "self"));
        feed.Links.Add(new AtomLink(new Uri("http://example.com/"), "alternate"));

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:complete-entry")),
            Title = new AtomTextConstruct("Complete Entry"),
            UpdatedOn = new DateTime(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc),
            Summary = new AtomTextConstruct("A complete entry")
        };
        entry.Authors.Add(new AtomPersonConstruct("Entry Author"));
        entry.Links.Add(new AtomLink(new Uri("http://example.com/entry")));
        feed.Entries.Add(entry);

        return feed;
    }

    #endregion
}