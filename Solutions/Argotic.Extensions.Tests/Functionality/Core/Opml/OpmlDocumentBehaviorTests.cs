using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

/// <summary>
/// Covers <see cref="OpmlDocument"/> end to end: building an outline tree, parsing one, the
/// subscription-list and inclusion outlines OPML 2.0 defines, and what survives a save and reload.
/// </summary>
[TestClass]
public class OpmlDocumentBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Document Creation Tests

    /// <summary>
    /// A title assigned through the document initialiser reaches the head.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenCreatedWithTitle_SetsHeadTitle()
    {
        // Arrange & Act
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "My Subscriptions"
            }
        };

        // Assert
        document.Head.Title.ShouldBe("My Subscriptions");
    }

    /// <summary>
    /// A default-constructed document has a head and an outline collection, neither null, and no outlines.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenCreatedWithDefaultConstructor_HasEmptyOutlines()
    {
        // Arrange & Act
        OpmlDocument document = new();

        // Assert
        document.ShouldNotBeNull();
        document.Outlines.ShouldNotBeNull();
        document.Outlines.Count.ShouldBe(0);
        document.Head.ShouldNotBeNull();
    }

    /// <summary>
    /// Outlines are held in the order they were added.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenOutlinesAdded_ContainsAllOutlines()
    {
        // Arrange
        OpmlDocument document = new();
        OpmlOutline outline1 = new("First Outline");
        OpmlOutline outline2 = new("Second Outline");

        // Act
        document.Outlines.Add(outline1);
        document.Outlines.Add(outline2);

        // Assert
        document.Outlines.Count.ShouldBe(2);
        document.Outlines[0].Text.ShouldBe("First Outline");
        document.Outlines[1].Text.ShouldBe("Second Outline");
    }

    /// <summary>
    /// An outline tree assembled three levels deep keeps every parent-child link and the order of siblings.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenNestedOutlinesAdded_PreservesHierarchy()
    {
        // Arrange
        OpmlDocument document = new();
        OpmlOutline parentOutline = new("Parent");
        OpmlOutline childOutline1 = new("Child 1");
        OpmlOutline childOutline2 = new("Child 2");
        OpmlOutline grandchildOutline = new("Grandchild");

        // Act
        childOutline1.Outlines.Add(grandchildOutline);
        parentOutline.Outlines.Add(childOutline1);
        parentOutline.Outlines.Add(childOutline2);
        document.Outlines.Add(parentOutline);

        // Assert
        document.Outlines.Count.ShouldBe(1);
        document.Outlines[0].Outlines.Count.ShouldBe(2);
        document.Outlines[0].Outlines[0].Text.ShouldBe("Child 1");
        document.Outlines[0].Outlines[0].Outlines.Count.ShouldBe(1);
        document.Outlines[0].Outlines[0].Outlines[0].Text.ShouldBe("Grandchild");
        document.Outlines[0].Outlines[1].Text.ShouldBe("Child 2");
    }

    /// <summary>
    /// The document indexer reads the outline at a position in the collection.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_Indexer_ReturnsCorrectOutline()
    {
        // Arrange
        OpmlDocument document = new();
        document.Outlines.Add(new OpmlOutline("First"));
        document.Outlines.Add(new OpmlOutline("Second"));
        document.Outlines.Add(new OpmlOutline("Third"));

        // Act & Assert
        document[0].Text.ShouldBe("First");
        document[1].Text.ShouldBe("Second");
        document[2].Text.ShouldBe("Third");
    }

    /// <summary>
    /// The document indexer replaces the outline at a position in the collection.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_Indexer_CanSetOutline()
    {
        // Arrange
        OpmlDocument document = new();
        document.Outlines.Add(new OpmlOutline("Original"));

        // Act
        document[0] = new OpmlOutline("Replaced");

        // Assert
        document[0].Text.ShouldBe("Replaced");
    }

    /// <summary>
    /// Assigning <see langword="null"/> to the head throws
    /// <see cref="ArgumentNullException"/> rather than leaving the document headless.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_HeadSetToNull_ThrowsArgumentNullException()
    {
        // Arrange
        OpmlDocument document = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => document.Head = null!);
    }

    /// <summary>
    /// The head keeps the UTC creation and modification dates it is given.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_HeadWithDates_SetsCreatedAndModifiedDates()
    {
        // Arrange
        DateTime createdOn = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime modifiedOn = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "Test OPML",
                CreatedOn = createdOn,
                ModifiedOn = modifiedOn
            }
        };

        // Assert
        document.Head.CreatedOn.ShouldBe(createdOn);
        document.Head.ModifiedOn.ShouldBe(modifiedOn);
    }

    /// <summary>
    /// An owner given a name, an email address and an identifying URI exposes all three.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_HeadWithOwner_SetsOwnerProperties()
    {
        // Arrange & Act
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "Test OPML",
                Owner = new OpmlOwner("John Doe", "john@example.com", new Uri("http://example.com/john"))
            }
        };

        // Assert
        document.Head.Owner.ShouldNotBeNull();
        document.Head.Owner.Name.ShouldBe("John Doe");
        document.Head.Owner.EmailAddress.ShouldBe("john@example.com");
        document.Head.Owner.Id.ShouldBe(new Uri("http://example.com/john"));
    }

    #endregion

    #region Document Parsing Tests

    /// <summary>
    /// Loading an OPML 2.0 stream populates the head title.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenLoadedFromValidXml_PopulatesProperties()
    {
        // Arrange
        string xml = FeedTestData.MinimalOpml;
        OpmlDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        document.Load(stream);

        // Assert
        document.Head.Title.ShouldBe("Test OPML");
    }

    /// <summary>
    /// Loading fills the outline collection from the body, one entry per <c>outline</c> element.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenLoadedFromOpmlWithOutlines_PopulatesOutlineCollection()
    {
        // Arrange
        string xml = FeedTestData.MinimalOpml;
        OpmlDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        document.Load(stream);

        // Assert
        document.Outlines.ShouldNotBeEmpty();
        document.Outlines.Count.ShouldBe(1);
        document.Outlines[0].Text.ShouldBe("Test Outline");
    }

    /// <summary>
    /// Loading from an <see cref="XmlReader"/> yields the same head and outlines as loading from a stream.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenLoadedFromXmlReader_PopulatesProperties()
    {
        // Arrange
        string xml = FeedTestData.MinimalOpml;
        OpmlDocument document = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        document.Load(reader);

        // Assert
        document.Head.Title.ShouldBe("Test OPML");
        document.Outlines.Count.ShouldBe(1);
    }

    /// <summary>
    /// An unclosed element surfaces as <see cref="XmlException"/> rather than as a partly populated document.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenLoadedFromMalformedXml_ThrowsXmlException()
    {
        // Arrange
        string malformedXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="2.0">
                <head>
                    <title>Unclosed Tag
                </head>
            </opml>
            """;
        OpmlDocument document = new();

        // Act & Assert
        Should.Throw<XmlException>(() =>
        {
            using XmlReader reader = XmlReader.Create(new StringReader(malformedXml));
            document.Load(reader);
        });
    }

    /// <summary>
    /// Nested <c>outline</c> elements load into a tree of the same shape, three levels deep and in document order.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenLoadedWithNestedOutlines_PreservesHierarchy()
    {
        // Arrange
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <opml version="2.0">
                <head>
                    <title>Nested OPML</title>
                </head>
                <body>
                    <outline text="Parent">
                        <outline text="Child 1">
                            <outline text="Grandchild"/>
                        </outline>
                        <outline text="Child 2"/>
                    </outline>
                </body>
            </opml>
            """;
        OpmlDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        document.Load(stream);

        // Assert
        document.Outlines.Count.ShouldBe(1);
        document.Outlines[0].Text.ShouldBe("Parent");
        document.Outlines[0].Outlines.Count.ShouldBe(2);
        document.Outlines[0].Outlines[0].Text.ShouldBe("Child 1");
        document.Outlines[0].Outlines[0].Outlines.Count.ShouldBe(1);
        document.Outlines[0].Outlines[0].Outlines[0].Text.ShouldBe("Grandchild");
        document.Outlines[0].Outlines[1].Text.ShouldBe("Child 2");
    }

    /// <summary>
    /// Loading raises <c>Loaded</c>, and the handler receives event arguments rather than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenLoaded_RaisesLoadedEvent()
    {
        // Arrange
        string xml = FeedTestData.MinimalOpml;
        OpmlDocument document = new();
        bool eventRaised = false;
        SyndicationResourceLoadedEventArgs? eventArgs = null;

        document.Loaded += (_, args) =>
        {
            eventRaised = true;
            eventArgs = args;
        };

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        document.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue();
        eventArgs.ShouldNotBeNull();
    }

    #endregion

    #region Round-Trip Tests

    /// <summary>
    /// A saved document reloads with the same head title and the same outline text.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenSavedAndReloaded_PreservesBasicProperties()
    {
        // Arrange
        OpmlDocument originalDocument = new()
        {
            Head =
            {
                Title = "Round Trip OPML"
            }
        };
        originalDocument.Outlines.Add(new OpmlOutline("Test Outline"));

        // Act
        using MemoryStream stream = new();
        originalDocument.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Head.Title.ShouldBe(originalDocument.Head.Title);
        loadedDocument.Outlines.Count.ShouldBe(originalDocument.Outlines.Count);
        loadedDocument.Outlines[0].Text.ShouldBe(originalDocument.Outlines[0].Text);
    }

    /// <summary>
    /// A three-level outline tree survives a save and reload, children and sibling order included.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenSavedAndReloaded_PreservesNestedOutlines()
    {
        // Arrange
        OpmlDocument originalDocument = new()
        {
            Head = { Title = "Nested Outlines Test" }
        };
        OpmlOutline parent = new("Parent");
        parent.Outlines.Add(new OpmlOutline("Child 1"));
        parent.Outlines.Add(new OpmlOutline("Child 2"));
        parent.Outlines[0].Outlines.Add(new OpmlOutline("Grandchild"));
        originalDocument.Outlines.Add(parent);

        // Act
        using MemoryStream stream = new();
        originalDocument.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Outlines.Count.ShouldBe(1);
        loadedDocument.Outlines[0].Text.ShouldBe("Parent");
        loadedDocument.Outlines[0].Outlines.Count.ShouldBe(2);
        loadedDocument.Outlines[0].Outlines[0].Text.ShouldBe("Child 1");
        loadedDocument.Outlines[0].Outlines[0].Outlines.Count.ShouldBe(1);
        loadedDocument.Outlines[0].Outlines[0].Outlines[0].Text.ShouldBe("Grandchild");
        loadedDocument.Outlines[0].Outlines[1].Text.ShouldBe("Child 2");
    }

    /// <summary>
    /// Head metadata survives a round trip: the owner, the vertical scroll
    /// state, and the expansion state as an ordered list of outline numbers.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenSavedAndReloaded_PreservesHeadMetadata()
    {
        // Arrange
        DateTime createdOn = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime modifiedOn = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        OpmlDocument originalDocument = new()
        {
            Head =
            {
                Title = "Metadata Test",
                CreatedOn = createdOn,
                ModifiedOn = modifiedOn,
                Owner = new OpmlOwner("Test Owner", "owner@example.com"),
                VerticalScrollState = 5
            }
        };
        originalDocument.Head.ExpansionState.Add(1);
        originalDocument.Head.ExpansionState.Add(3);
        originalDocument.Outlines.Add(new OpmlOutline("Test"));

        // Act
        using MemoryStream stream = new();
        originalDocument.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Head.Title.ShouldBe("Metadata Test");
        loadedDocument.Head.Owner.ShouldNotBeNull();
        loadedDocument.Head.Owner.Name.ShouldBe("Test Owner");
        loadedDocument.Head.Owner.EmailAddress.ShouldBe("owner@example.com");
        loadedDocument.Head.VerticalScrollState.ShouldBe(5);
        loadedDocument.Head.ExpansionState.Count.ShouldBe(2);
        loadedDocument.Head.ExpansionState[0].ShouldBe(1);
        loadedDocument.Head.ExpansionState[1].ShouldBe(3);
    }

    /// <summary>
    /// A second save and reload leaves the title and the outline count where the first one left them.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WhenSavedAndReloadedTwice_MaintainsDataIntegrity()
    {
        // Arrange
        OpmlDocument originalDocument = CreateCompleteDocument();

        // Act - First round trip
        using MemoryStream stream1 = new();
        originalDocument.Save(stream1);
        stream1.Position = 0;
        OpmlDocument document1 = new();
        document1.Load(stream1);

        // Act - Second round trip
        using MemoryStream stream2 = new();
        document1.Save(stream2);
        stream2.Position = 0;
        OpmlDocument document2 = new();
        document2.Load(stream2);

        // Assert
        document2.Head.Title.ShouldBe(originalDocument.Head.Title);
        document2.Outlines.Count.ShouldBe(originalDocument.Outlines.Count);
    }

    /// <summary>
    /// Parsing, serialising and parsing again yields the same head title and the same outline text.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_ParseSerializeParse_ProducesSameDocument()
    {
        // Arrange
        OpmlDocument firstDocument = new();
        using MemoryStream firstStream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml));
        firstDocument.Load(firstStream);

        // Act - First serialize
        using MemoryStream serializeStream = new();
        firstDocument.Save(serializeStream);

        // Act - Second parse
        serializeStream.Position = 0;
        OpmlDocument secondDocument = new();
        secondDocument.Load(serializeStream);

        // Assert - Core properties match
        secondDocument.Head.Title.ShouldBe(firstDocument.Head.Title);
        secondDocument.Outlines.Count.ShouldBe(firstDocument.Outlines.Count);
        secondDocument.Outlines[0].Text.ShouldBe(firstDocument.Outlines[0].Text);
    }

    #endregion

    #region OPML-Specific Behavior Tests - Subscription List Outlines

    /// <summary>
    /// A subscription list outline is given the content type
    /// <c>rss</c> and the feed address as its <c>xmlUrl</c> attribute.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_CreateSubscriptionListOutline_CreatesRssOutline()
    {
        // Arrange & Act
        OpmlOutline outline = OpmlOutline.CreateSubscriptionListOutline(
            "Example Feed",
            "rss",
            new Uri("http://example.com/feed.rss"));

        // Assert
        outline.Text.ShouldBe("Example Feed");
        outline.ContentType.ShouldBe("rss");
        outline.IsSubscriptionListOutline.ShouldBeTrue();
        outline.Attributes.ShouldContainKey("xmlUrl");
        outline.Attributes["xmlUrl"].ShouldBe("http://example.com/feed.rss");
    }

    /// <summary>
    /// The full overload writes <c>xmlUrl</c>, <c>htmlUrl</c>, <c>version</c>,
    /// <c>title</c>, <c>description</c> and <c>language</c> as outline attributes,
    /// the site address in the trailing-slash form <see cref="Uri"/> normalises it to.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_CreateSubscriptionListOutline_WithAllParameters_SetsAllAttributes()
    {
        // Arrange & Act
        OpmlOutline outline = OpmlOutline.CreateSubscriptionListOutline(
            "Example Feed",
            "rss",
            new Uri("http://example.com/feed.rss"),
            new Uri("http://example.com"),
            "RSS2",
            "Example Feed Title",
            "A feed about examples",
            new System.Globalization.CultureInfo("en-US"));

        // Assert
        outline.Text.ShouldBe("Example Feed");
        outline.ContentType.ShouldBe("rss");
        outline.IsSubscriptionListOutline.ShouldBeTrue();
        outline.Attributes["xmlUrl"].ShouldBe("http://example.com/feed.rss");
        outline.Attributes["htmlUrl"].ShouldBe("http://example.com/");
        outline.Attributes["version"].ShouldBe("RSS2");
        outline.Attributes["title"].ShouldBe("Example Feed Title");
        outline.Attributes["description"].ShouldBe("A feed about examples");
        outline.Attributes["language"].ShouldBe("en-US");
    }

    /// <summary>
    /// An outline whose content type is <c>rss</c> reports itself as a subscription list.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_IsSubscriptionListOutline_ReturnsTrueForRssType()
    {
        // Arrange
        OpmlOutline outline = new("Test Feed")
        {
            ContentType = "rss"
        };

        // Assert
        outline.IsSubscriptionListOutline.ShouldBeTrue();
    }

    /// <summary>
    /// An outline whose content type is <c>feed</c> reports itself as a subscription list.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_IsSubscriptionListOutline_ReturnsTrueForFeedType()
    {
        // Arrange
        OpmlOutline outline = new("Test Feed")
        {
            ContentType = "feed"
        };

        // Assert
        outline.IsSubscriptionListOutline.ShouldBeTrue();
    }

    /// <summary>
    /// Subscription list outlines survive a round trip with their content type and their <c>xmlUrl</c> attribute.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_WithSubscriptionOutlines_SavesAndLoadsCorrectly()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "My Subscriptions" }
        };

        OpmlOutline feed1 = OpmlOutline.CreateSubscriptionListOutline(
            "Tech News",
            "rss",
            new Uri("http://technews.example.com/feed.rss"));

        OpmlOutline feed2 = OpmlOutline.CreateSubscriptionListOutline(
            "Science Daily",
            "rss",
            new Uri("http://science.example.com/feed.xml"));

        document.Outlines.Add(feed1);
        document.Outlines.Add(feed2);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Outlines.Count.ShouldBe(2);
        loadedDocument.Outlines[0].Text.ShouldBe("Tech News");
        loadedDocument.Outlines[0].ContentType.ShouldBe("rss");
        loadedDocument.Outlines[0].IsSubscriptionListOutline.ShouldBeTrue();
        loadedDocument.Outlines[0].Attributes["xmlUrl"].ShouldBe("http://technews.example.com/feed.rss");
    }

    #endregion

    #region OPML-Specific Behavior Tests - Include Outlines

    /// <summary>
    /// An inclusion outline pointing at an <c>.opml</c> address is given the content type <c>include</c>.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_CreateInclusionOutline_WithOpmlUrl_CreatesIncludeType()
    {
        // Arrange & Act
        OpmlOutline outline = OpmlOutline.CreateInclusionOutline(
            "External OPML",
            new Uri("http://example.com/external.opml"));

        // Assert
        outline.Text.ShouldBe("External OPML");
        outline.ContentType.ShouldBe("include");
        outline.IsInclusionOutline.ShouldBeTrue();
        outline.Attributes["url"].ShouldBe("http://example.com/external.opml");
    }

    /// <summary>
    /// An inclusion outline pointing anywhere other than an
    /// <c>.opml</c> address is given the content type <c>link</c>.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_CreateInclusionOutline_WithNonOpmlUrl_CreatesLinkType()
    {
        // Arrange & Act
        OpmlOutline outline = OpmlOutline.CreateInclusionOutline(
            "External Link",
            new Uri("http://example.com/page.html"));

        // Assert
        outline.Text.ShouldBe("External Link");
        outline.ContentType.ShouldBe("link");
        outline.IsInclusionOutline.ShouldBeTrue();
        outline.Attributes["url"].ShouldBe("http://example.com/page.html");
    }

    /// <summary>
    /// An outline whose content type is <c>include</c> reports itself as an inclusion.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_IsInclusionOutline_ReturnsTrueForIncludeType()
    {
        // Arrange
        OpmlOutline outline = new("Test Outline")
        {
            ContentType = "include"
        };

        // Assert
        outline.IsInclusionOutline.ShouldBeTrue();
    }

    /// <summary>
    /// An outline whose content type is <c>link</c> reports itself as an inclusion.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_IsInclusionOutline_ReturnsTrueForLinkType()
    {
        // Arrange
        OpmlOutline outline = new("Test Outline")
        {
            ContentType = "link"
        };

        // Assert
        outline.IsInclusionOutline.ShouldBeTrue();
    }

    #endregion

    #region OPML-Specific Behavior Tests - Outline Categories and Attributes

    /// <summary>
    /// Outline categories survive a round trip in order, a
    /// slash-delimited path such as <c>Technology/Software</c> included.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_WithCategories_SavesAndLoadsCorrectly()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Categories Test" }
        };

        OpmlOutline outline = new("Categorized Outline");
        outline.Categories.Add("Technology/Software");
        outline.Categories.Add("News");
        document.Outlines.Add(outline);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Outlines[0].Categories.Count.ShouldBe(2);
        loadedDocument.Outlines[0].Categories[0].ShouldBe("Technology/Software");
        loadedDocument.Outlines[0].Categories[1].ShouldBe("News");
    }

    /// <summary>
    /// Attributes the format does not define are written out and read back verbatim.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_WithCustomAttributes_SavesAndLoadsCorrectly()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Attributes Test" }
        };

        OpmlOutline outline = new("Custom Attributes Outline");
        outline.Attributes.Add("customAttr1", "value1");
        outline.Attributes.Add("customAttr2", "value2");
        document.Outlines.Add(outline);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Outlines[0].Attributes.ShouldContainKey("customAttr1");
        loadedDocument.Outlines[0].Attributes["customAttr1"].ShouldBe("value1");
        loadedDocument.Outlines[0].Attributes.ShouldContainKey("customAttr2");
        loadedDocument.Outlines[0].Attributes["customAttr2"].ShouldBe("value2");
    }

    /// <summary>
    /// The commented flag on an outline survives a round trip.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_WithIsCommented_SavesAndLoadsCorrectly()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Comment Test" }
        };

        OpmlOutline outline = new("Commented Outline")
        {
            IsCommented = true
        };
        document.Outlines.Add(outline);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Outlines[0].IsCommented.ShouldBeTrue();
    }

    /// <summary>
    /// The breakpoint flag on an outline survives a round trip.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_WithHasBreakpoint_SavesAndLoadsCorrectly()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Breakpoint Test" }
        };

        OpmlOutline outline = new("Breakpoint Outline")
        {
            HasBreakpoint = true
        };
        document.Outlines.Add(outline);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Outlines[0].HasBreakpoint.ShouldBeTrue();
    }

    /// <summary>
    /// An outline created date is written as a <c>created</c>
    /// attribute whose month is spelled <c>Jun</c>, not numbered.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_WithCreatedOnDate_SerializesCreatedAttribute()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Created Date Test" }
        };

        DateTime createdDate = new(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        OpmlOutline outline = new("Dated Outline")
        {
            CreatedOn = createdDate
        };
        document.Outlines.Add(outline);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert - verify the created attribute is included in the serialized XML
        xml.ShouldContain("created=\"");
        xml.ShouldContain("2024");
        xml.ShouldContain("Jun");
    }

    /// <summary>
    /// An outline created date is read back exactly as assigned.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_CreatedOnDate_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        DateTime createdDate = new(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        OpmlOutline outline = new("Dated Outline")
        {
            CreatedOn = createdDate
        };

        // Assert
        outline.CreatedOn.ShouldBe(createdDate);
    }

    #endregion

    #region Format and Version Tests

    /// <summary>
    /// A document reports <c>SyndicationContentFormat.Opml</c> as the format it implements.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_Format_ReturnsOpml()
    {
        // Arrange
        OpmlDocument document = new();

        // Assert
        document.Format.ShouldBe(SyndicationContentFormat.Opml);
    }

    /// <summary>
    /// A document reports version <c>2.0</c>, which is the version it writes rather than one it was told.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_Version_Returns2_0()
    {
        // Arrange
        OpmlDocument document = new();

        // Assert
        document.Version.ShouldBe(new Version(2, 0));
    }

    /// <summary>
    /// The head points by default at the OPML 2.0 specification, at <c>https://opml.org/spec2.opml</c>.
    /// </summary>
    [TestMethod]
    public void OpmlHead_Documentation_ReturnsOpmlSpecUrl()
    {
        // Arrange
        OpmlDocument document = new();

        // Assert
        document.Head.Documentation.ShouldBe(new Uri("https://opml.org/spec2.opml"));
    }

    #endregion

    #region CreateNavigator Tests

    /// <summary>
    /// The navigator a document creates is rooted on an <c>opml</c> element.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_CreateNavigator_ReturnsValidNavigator()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Navigator Test" }
        };
        document.Outlines.Add(new OpmlOutline("Test Outline"));

        // Act
        XPathNavigator navigator = document.CreateNavigator();

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("opml");
    }

    #endregion

    #region Extensions Tests

    /// <summary>
    /// A document carrying no extensions says so, and its extension collection is empty rather than null.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        OpmlDocument document = new();

        // Assert
        document.HasExtensions.ShouldBeFalse();
        document.Extensions.Count.ShouldBe(0);
    }

    /// <summary>
    /// A head carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void OpmlHead_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        OpmlDocument document = new();

        // Assert
        document.Head.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// An outline carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void OpmlOutline_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        OpmlOutline outline = new("Test");

        // Assert
        outline.HasExtensions.ShouldBeFalse();
    }

    #endregion

    #region Async Operations Tests

    /// <summary>
    /// Loading over a caller-supplied client populates the head and outlines, and raises <c>Loaded</c>.
    /// </summary>
    [TestMethod]
    public async Task OpmlDocument_LoadAsync_LoadsDocumentCorrectly()
    {
        // Arrange
        OpmlDocument document = new();
        bool eventRaised = false;
        document.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(
            new Uri("http://example.com/subscriptions.opml"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue();
        document.Head.Title.ShouldBe("Test OPML");
        document.Outlines.Count.ShouldBe(1);
    }

    /// <summary>
    /// The static create returns a document already populated from the response body.
    /// </summary>
    [TestMethod]
    public async Task OpmlDocument_CreateAsync_CreatesAndLoadsNewDocument()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new(handler);

        // Act
        OpmlDocument document = await OpmlDocument.CreateAsync(
            new Uri("http://example.com/subscriptions.opml"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        document.ShouldNotBeNull();
        document.Head.Title.ShouldBe("Test OPML");
        document.Format.ShouldBe(SyndicationContentFormat.Opml);
    }

    /// <summary>
    /// The <c>Loaded</c> event reports the URI the document was fetched from.
    /// </summary>
    [TestMethod]
    public async Task OpmlDocument_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        OpmlDocument document = new();
        Uri? sourceFromEvent = null;

        document.Loaded += (_, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient httpClient = new(handler);
        Uri requestUri = new("http://example.com/subscriptions.opml");

        // Act
        await document.LoadAsync(requestUri, httpClient, cancellationToken: TestContext!.CancellationToken);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    #endregion

    #region Save Tests

    /// <summary>
    /// Saving writes an XML declaration and an <c>opml</c> element
    /// carrying <c>version="2.0"</c>, a head with the title, and a body.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_Save_ProducesValidXml()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "Save Test" }
        };
        document.Outlines.Add(new OpmlOutline("Test Outline"));

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<opml");
        xml.ShouldContain("version=\"2.0\"");
        xml.ShouldContain("<head>");
        xml.ShouldContain("<title>Save Test</title>");
        xml.ShouldContain("<body>");
        xml.ShouldContain("<outline");
    }

    /// <summary>
    /// Saving through a caller-configured <see cref="XmlWriter"/> writes the same document as saving to a stream.
    /// </summary>
    [TestMethod]
    public void OpmlDocument_SaveWithXmlWriter_ProducesValidXml()
    {
        // Arrange
        OpmlDocument document = new()
        {
            Head = { Title = "XmlWriter Test" }
        };
        document.Outlines.Add(new OpmlOutline("Test Outline"));

        // Act
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            Indent = true,
            OmitXmlDeclaration = false
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            document.Save(writer);
        }

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert
        xml.ShouldContain("<opml");
        xml.ShouldContain("<title>XmlWriter Test</title>");
    }

    #endregion

    #region Helper Methods

    private static OpmlDocument CreateCompleteDocument()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "Complete OPML Document",
                CreatedOn = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                ModifiedOn = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                Owner = new OpmlOwner("Test Owner", "owner@example.com")
            }
        };

        // Add category folder with feeds
        OpmlOutline techFolder = new("Technology");
        techFolder.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline(
            "Tech News",
            "rss",
            new Uri("http://technews.example.com/feed.rss")));
        techFolder.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline(
            "Programming Blog",
            "rss",
            new Uri("http://programming.example.com/feed.xml")));

        OpmlOutline newsFolder = new("News");
        newsFolder.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline(
            "World News",
            "rss",
            new Uri("http://worldnews.example.com/rss")));

        document.Outlines.Add(techFolder);
        document.Outlines.Add(newsFolder);

        // Add standalone feed
        document.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline(
            "Personal Blog",
            "rss",
            new Uri("http://personalblog.example.com/feed")));

        return document;
    }

    #endregion
}