using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Opml;

[TestClass]
public class OpmlDocumentTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesEmptyDocument()
    {
        OpmlDocument document = new();

        document.ShouldNotBeNull();
        document.Outlines.ShouldNotBeNull();
        document.Outlines.Count.ShouldBe(0);
    }

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

    [TestMethod]
    public void AddOutline_AddsOutlineCorrectly()
    {
        OpmlDocument document = new();
        OpmlOutline outline = new("Test Outline");

        document.Outlines.Add(outline);

        document.Outlines.Count.ShouldBe(1);
        document.Outlines.First().Text.ShouldBe("Test Outline");
    }

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