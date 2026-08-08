namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

/// <summary>
/// Covers <see cref="OpmlDocument"/> member by member: the head, the outline tree, and a save followed by a load.
/// </summary>
[TestClass]
public class OpmlDocumentTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A newly constructed document has an outline collection, and that collection is empty.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesEmptyDocument()
    {
        OpmlDocument document = new();

        document.ShouldNotBeNull();
        document.Outlines.ShouldNotBeNull();
        document.Outlines.Count.ShouldBe(0);
    }

    /// <summary>
    /// A title assigned to the head through the document initialiser is read back unchanged.
    /// </summary>
    [TestMethod]
    public void Head_CanSetTitle()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "Test OPML Document"
            }
        };

        document.Head.Title.ShouldBe("Test OPML Document");
    }

    /// <summary>
    /// The head keeps both the creation date and the modification date it is given.
    /// </summary>
    [TestMethod]
    public void Head_CanSetDates()
    {
        DateTime createdOn = new(2024, 1, 1, 12, 0, 0);
        DateTime modifiedOn = new(2024, 1, 15, 12, 0, 0);

        OpmlDocument document = new()
        {
            Head =
            {
                CreatedOn = createdOn,
                ModifiedOn = modifiedOn
            }
        };

        document.Head.CreatedOn.ShouldBe(createdOn);
        document.Head.ModifiedOn.ShouldBe(modifiedOn);
    }

    /// <summary>
    /// An owner constructed from a name and an email address exposes both through the head.
    /// </summary>
    [TestMethod]
    public void Head_CanSetOwner()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Owner = new OpmlOwner("John Doe", "john@example.com")
            }
        };

        document.Head.Owner.Name.ShouldBe("John Doe");
        document.Head.Owner.EmailAddress.ShouldBe("john@example.com");
    }

    /// <summary>
    /// An outline added to the document appears in the outline collection with its text intact.
    /// </summary>
    [TestMethod]
    public void AddOutline_AddsOutlineCorrectly()
    {
        OpmlDocument document = new();
        OpmlOutline outline = new("Test Outline");

        document.Outlines.Add(outline);

        document.Outlines.Count.ShouldBe(1);
        document.Outlines.First().Text.ShouldBe("Test Outline");
    }

    /// <summary>
    /// An outline holds its child outlines in the order they were added.
    /// </summary>
    [TestMethod]
    public void Outline_CanHaveNestedOutlines()
    {
        OpmlDocument document = new();
        OpmlOutline containerOutline = new("Feeds");
        containerOutline.Outlines.Add(new OpmlOutline("Child 1"));
        containerOutline.Outlines.Add(new OpmlOutline("Child 2"));

        document.Outlines.Add(containerOutline);

        document.Outlines.First().Outlines.Count.ShouldBe(2);
        document.Outlines.First().Outlines[0].Text.ShouldBe("Child 1");
        document.Outlines.First().Outlines[1].Text.ShouldBe("Child 2");
    }

    /// <summary>
    /// An outline built from a feed address carries the text it was given and reports itself as a subscription list.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionListOutline_CreatesCorrectOutline()
    {
        OpmlOutline outline = OpmlOutline.CreateSubscriptionListOutline(
            "My Feed",
            "rss",
            new Uri("http://example.com/feed.rss"));

        outline.Text.ShouldBe("My Feed");
        outline.IsSubscriptionListOutline.ShouldBeTrue();
    }

    /// <summary>
    /// Loading a minimal OPML 2.0 stream populates the head title and the single outline in the body.
    /// </summary>
    [TestMethod]
    public void Load_MinimalOpml_LoadsCorrectly()
    {
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml));
        OpmlDocument document = new();
        document.Load(stream);

        document.Head.Title.ShouldBe("Test OPML");
        document.Outlines.Count.ShouldBe(1);
        document.Outlines.First().Text.ShouldBe("Test Outline");
    }

    /// <summary>
    /// Saving writes an XML declaration and the <c>opml</c>, <c>head</c>,
    /// <c>title</c>, <c>body</c> and <c>outline</c> elements.
    /// </summary>
    [TestMethod]
    public void Save_ProducesValidXml()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "Test OPML"
            }
        };
        document.Outlines.Add(new OpmlOutline("Test Outline"));

        using MemoryStream stream = new();
        document.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<opml");
        xml.ShouldContain("<head>");
        xml.ShouldContain("<title>Test OPML</title>");
        xml.ShouldContain("<body>");
        xml.ShouldContain("<outline");
    }

    /// <summary>
    /// A document saved to a stream and loaded back keeps its head title and its outline count.
    /// </summary>
    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a document
        OpmlDocument originalDocument = new()
        {
            Head =
            {
                Title = "Test OPML"
            }
        };
        originalDocument.Outlines.Add(new OpmlOutline("Outline 1"));
        originalDocument.Outlines.Add(new OpmlOutline("Outline 2"));

        // Save to stream
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Load from stream
        stream.Position = 0;
        OpmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Verify data preserved
        loadedDocument.Head.Title.ShouldBe(originalDocument.Head.Title);
        loadedDocument.Outlines.Count.ShouldBe(originalDocument.Outlines.Count);
    }
}