namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

/// <summary>
/// Covers <see cref="ApmlDocument"/> member by member: head metadata, profiles and
/// the concepts, sources and authors they hold, and a save followed by a load.
/// </summary>
[TestClass]
public class ApmlDocumentTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A newly constructed document holds no profiles.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesEmptyDocument()
    {
        ApmlDocument document = new();

        document.ShouldNotBeNull();
        document.Profiles.Count.ShouldBe(0);
    }

    /// <summary>
    /// The head keeps the title, generator, email address and creation date it is given.
    /// </summary>
    [TestMethod]
    public void Head_CanSetProperties()
    {
        DateTime createdOn = new(2024, 1, 1, 12, 0, 0);
        ApmlDocument document = new()
        {
            Head =
            {
                Title = "Test APML Document",
                Generator = "Test Generator",
                EmailAddress = "test@example.com",
                CreatedOn = createdOn
            }
        };

        document.Head.Title.ShouldBe("Test APML Document");
        document.Head.Generator.ShouldBe("Test Generator");
        document.Head.EmailAddress.ShouldBe("test@example.com");
        document.Head.CreatedOn.ShouldBe(createdOn);
    }

    /// <summary>
    /// The name of the default profile is read back exactly as assigned.
    /// </summary>
    [TestMethod]
    public void DefaultProfileName_CanBeSet()
    {
        ApmlDocument document = new()
        {
            DefaultProfileName = "Work"
        };

        document.DefaultProfileName.ShouldBe("Work");
    }

    /// <summary>
    /// A profile added to the document appears in the profile collection under its own name.
    /// </summary>
    [TestMethod]
    public void AddProfile_AddsProfileCorrectly()
    {
        ApmlDocument document = new();
        ApmlProfile profile = new()
        {
            Name = "Home"
        };

        document.Profiles.Add(profile);

        document.Profiles.Count.ShouldBe(1);
        document.Profiles.First().Name.ShouldBe("Home");
    }

    /// <summary>
    /// An implicit concept keeps the key and the weight it was constructed from, here <c>0.99</c>.
    /// </summary>
    [TestMethod]
    public void Profile_CanHaveImplicitConcepts()
    {
        ApmlProfile profile = new()
        {
            Name = "Test"
        };

        profile.ImplicitConcepts.Add(new ApmlConcept(
            "attention",
            0.99m,
            "GatheringTool.com",
            new DateTime(2024, 1, 1)));

        profile.ImplicitConcepts.Count.ShouldBe(1);
        profile.ImplicitConcepts[0].Key.ShouldBe("attention");
        profile.ImplicitConcepts[0].Value.ShouldBe(0.99m);
    }

    /// <summary>
    /// An explicit concept keeps the key and the weight it was constructed from, here <c>0.99</c>.
    /// </summary>
    [TestMethod]
    public void Profile_CanHaveExplicitConcepts()
    {
        ApmlProfile profile = new()
        {
            Name = "Test"
        };

        profile.ExplicitConcepts.Add(new ApmlConcept("direct attention", 0.99m));

        profile.ExplicitConcepts.Count.ShouldBe(1);
        profile.ExplicitConcepts[0].Key.ShouldBe("direct attention");
        profile.ExplicitConcepts[0].Value.ShouldBe(0.99m);
    }

    /// <summary>
    /// A source keeps its key and name, and holds the authors added to it.
    /// </summary>
    [TestMethod]
    public void Source_CanHaveAuthors()
    {
        ApmlSource source = new()
        {
            Key = "http://feeds.example.com/feed",
            Name = "Example Feed",
            Value = 1.0m,
            MimeType = "application/rss+xml"
        };
        source.Authors.Add(new ApmlAuthor("Test Author", 0.5m, "Source", new DateTime(2024, 1, 1)));

        source.Key.ShouldBe("http://feeds.example.com/feed");
        source.Name.ShouldBe("Example Feed");
        source.Authors.Count.ShouldBe(1);
        source.Authors[0].Key.ShouldBe("Test Author");
    }

    /// <summary>
    /// Saving writes the element names in the capitalisation APML 0.6
    /// defines: <c>APML</c>, <c>Head</c>, <c>Title</c> and <c>Body</c>.
    /// </summary>
    [TestMethod]
    public void Save_ProducesValidXml()
    {
        ApmlDocument document = new()
        {
            DefaultProfileName = "Work",
            Head =
            {
                Title = "Test APML"
            }
        };
        ApmlProfile profile = new() { Name = "Work" };
        profile.ExplicitConcepts.Add(new ApmlConcept("test", 0.5m));
        document.Profiles.Add(profile);

        using MemoryStream stream = new();
        document.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<APML");
        xml.ShouldContain("<Head>");
        xml.ShouldContain("<Title>Test APML</Title>");
        xml.ShouldContain("<Body");
    }

    /// <summary>
    /// A document saved to a stream and loaded back keeps its head title and its profile count.
    /// </summary>
    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a document
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "Work",
            Head =
            {
                Title = "Test APML"
            }
        };
        ApmlProfile profile = new() { Name = "Work" };
        profile.ExplicitConcepts.Add(new ApmlConcept("test", 0.5m));
        originalDocument.Profiles.Add(profile);

        // Save to stream
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Load from stream
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Verify data preserved
        loadedDocument.Head.Title.ShouldBe(originalDocument.Head.Title);
        loadedDocument.Profiles.Count.ShouldBe(originalDocument.Profiles.Count);
    }
}