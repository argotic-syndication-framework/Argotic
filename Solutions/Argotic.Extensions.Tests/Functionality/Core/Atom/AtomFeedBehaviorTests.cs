using System.Xml;

using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Covers an Atom 1.0 feed end to end: building one in memory, parsing one, the text, person and link
/// constructs it is made of, the round trips between the two, and loading one over HTTP.
/// </summary>
[TestClass]
public class AtomFeedBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Feed Creation Tests

    /// <summary>
    /// The three-argument constructor keeps the id, title and update time it was handed.
    /// </summary>
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

    /// <summary>
    /// Entries are kept in the order they were added, each with its own title.
    /// </summary>
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

    /// <summary>
    /// Authors are kept in order, each keeping its name, email address and uri.
    /// </summary>
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

    /// <summary>
    /// Links are kept in order, each keeping its relation and its declared content type.
    /// </summary>
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

    /// <summary>
    /// Categories are kept in order, with scheme and label preserved on the one that carries them.
    /// </summary>
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

    /// <summary>
    /// A well-formed Atom document read through an <see cref="XmlReader"/> yields its title and its id.
    /// </summary>
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

    /// <summary>
    /// Both entries in the document reach the entry collection, in document order.
    /// </summary>
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

    /// <summary>
    /// An entry's own <c>category</c> elements are parsed onto that entry, in document order.
    /// </summary>
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

    /// <summary>
    /// A document with an unclosed element fails with <see cref="XmlException"/> rather than yielding a
    /// half-filled feed.
    /// </summary>
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

    /// <summary>
    /// Loading from a stream gives the same title as loading through an <see cref="XmlReader"/>.
    /// </summary>
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

    /// <summary>
    /// Id, title, subtitle and rights all survive a save and a reload.
    /// </summary>
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

    /// <summary>
    /// Every entry survives a round trip with its title and id, and none is added or lost.
    /// </summary>
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

    /// <summary>
    /// An entry's summary survives a round trip verbatim.
    /// </summary>
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

    /// <summary>
    /// A second round trip changes nothing a first one did not: title, entries, authors and categories
    /// all still match the original.
    /// </summary>
    /// <remarks>
    ///     Two cycles are the minimum that can see a compounding defect — one where each pass through
    ///     the model alters the document a little further.
    /// </remarks>
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

    /// <summary>
    /// A text construct declared as <c>text</c> keeps both its content and that declaration.
    /// </summary>
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

    /// <summary>
    /// A text construct declared as <c>html</c> holds the markup itself, unescaped, in memory.
    /// </summary>
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

    /// <summary>
    /// A text construct declared as <c>xhtml</c> keeps that declaration.
    /// </summary>
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

    /// <summary>
    /// A construct's <c>type</c> is written out and read back, so an <c>html</c> title does not return
    /// as plain text.
    /// </summary>
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

    /// <summary>
    /// Each construct type spells itself on the wire in lower case: <c>html</c>, <c>text</c>, <c>xhtml</c>.
    /// </summary>
    [TestMethod]
    public void AtomTextConstruct_TypeConversion_ReturnsCorrectString()
    {
        // Assert
        AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Html).ShouldBe("html");
        AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Text).ShouldBe("text");
        AtomTextConstruct.ConstructTypeAsString(AtomTextConstructType.Xhtml).ShouldBe("xhtml");
    }

    /// <summary>
    /// Reading a type back off the wire is case-insensitive, so <c>HTML</c> is recognised as <c>html</c>.
    /// </summary>
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

    /// <summary>
    /// A person construct keeps all three of its parts: name, email address and uri.
    /// </summary>
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

    /// <summary>
    /// Authors and contributors survive a round trip in their own collections, without one absorbing
    /// the other.
    /// </summary>
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

    /// <summary>
    /// A link keeps the uri and the relation it was constructed with.
    /// </summary>
    [TestMethod]
    public void AtomLink_WhenCreatedWithRelAttribute_ContainsCorrectValues()
    {
        // Arrange & Act
        AtomLink link = new(new Uri("http://example.com/"), "alternate");

        // Assert
        link.Uri.ShouldBe(new Uri("http://example.com/"));
        link.Relation.ShouldBe("alternate");
    }

    /// <summary>
    /// A link also carries its content type, title and length, the last as a number rather than text.
    /// </summary>
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

    /// <summary>
    /// Three links with distinct relations survive a round trip, each keeping its own content type, and
    /// the enclosure keeping its five-million-byte length.
    /// </summary>
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
            Length = 5_000_000
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
        enclosureLink.Length.ShouldBe(5_000_000);
    }

    /// <summary>
    /// The <c>via</c> relation is stored as written, not normalised away to a better-known one.
    /// </summary>
    [TestMethod]
    public void AtomLink_WithViaRelation_SetsCorrectly()
    {
        // Arrange & Act
        AtomLink link = new(new Uri("http://source.example.com/original"), "via");

        // Assert
        link.Relation.ShouldBe("via");
    }

    /// <summary>
    /// The <c>related</c> relation is likewise stored as written.
    /// </summary>
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

    /// <summary>
    /// A generator's name, uri and version all survive a round trip.
    /// </summary>
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

    /// <summary>
    /// Icon and logo survive a round trip without being confused for one another.
    /// </summary>
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

    /// <summary>
    /// A feed reports <c>Atom</c> as its format before anything has been loaded into it.
    /// </summary>
    [TestMethod]
    public void AtomFeed_Format_ReturnsAtom()
    {
        // Arrange
        AtomFeed feed = new();

        // Assert
        feed.Format.ShouldBe(Argotic.Common.SyndicationContentFormat.Atom);
    }

    /// <summary>
    /// A feed reports version 1.0 — the RFC 4287 format, not the 0.3 draft.
    /// </summary>
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

    /// <summary>
    /// Loading through an <see cref="XmlReader"/> raises the <c>Loaded</c> event.
    /// </summary>
    [TestMethod]
    public void AtomFeed_WhenLoaded_RaisesLoadedEvent()
    {
        // Arrange
        string xml = FeedTestData.MinimalAtom;
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        feed.Load(reader);

        // Assert
        eventRaised.ShouldBeTrue();
    }

    #endregion

    #region Async Operations Tests

    /// <summary>
    /// A feed fetched over HTTP is parsed into title and id, and raises <c>Loaded</c> as the synchronous
    /// path does.
    /// </summary>
    /// <remarks>
    ///     The response comes from a mock message handler, so nothing leaves the machine.
    /// </remarks>
    [TestMethod]
    public async Task AtomFeed_LoadAsync_LoadsFeedCorrectly()
    {
        // Arrange
        AtomFeed feed = new();
        bool eventRaised = false;
        feed.Loaded += (_, _) => eventRaised = true;

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

    /// <summary>
    /// Entries fetched over HTTP arrive complete and in document order, as they do from a stream.
    /// </summary>
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

    /// <summary>
    /// The static factory returns a feed already loaded from the uri, saving the caller a construct-then-load pair.
    /// </summary>
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

    /// <summary>
    /// Load settings reach the async path: with extension auto-detection on, a feed carrying a
    /// <c>dc:creator</c> comes back reporting that it has extensions.
    /// </summary>
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

    /// <summary>
    /// The <c>Loaded</c> event reports the uri the document was fetched from, so a handler watching
    /// several feeds can tell which one arrived.
    /// </summary>
    [TestMethod]
    public async Task AtomFeed_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        AtomFeed feed = new();
        Uri? sourceFromEvent = null;

        feed.Loaded += (_, args) =>
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