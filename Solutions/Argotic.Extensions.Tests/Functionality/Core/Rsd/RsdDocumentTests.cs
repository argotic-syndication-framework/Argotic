using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Rsd;

/// <summary>
/// Covers <see cref="RsdDocument"/> member by member: the service properties, the
/// application interfaces a blog client chooses between, and a save followed by a load.
/// </summary>
[TestClass]
public class RsdDocumentTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A newly constructed document declares no application interfaces.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesEmptyDocument()
    {
        RsdDocument document = new();

        document.ShouldNotBeNull();
        document.Interfaces.Count.ShouldBe(0);
    }

    /// <summary>
    /// The engine name, the engine link and the homepage are read back exactly as assigned.
    /// </summary>
    [TestMethod]
    public void Properties_CanBeSet()
    {
        RsdDocument document = new()
        {
            EngineName = "Test CMS",
            EngineLink = new Uri("http://example.com/cms"),
            Homepage = new Uri("http://example.com/")
        };

        document.EngineName.ShouldBe("Test CMS");
        document.EngineLink.ShouldBe(new Uri("http://example.com/cms"));
        document.Homepage.ShouldBe(new Uri("http://example.com/"));
    }

    /// <summary>
    /// An interface added to the document keeps its name, link, preferred flag and weblog identifier.
    /// </summary>
    [TestMethod]
    public void Interfaces_Add_AddsInterfaceCorrectly()
    {
        RsdDocument document = new();
        RsdApplicationInterface api = new(
            "MetaWeblog",
            new Uri("http://example.com/xml/rpc"),
            true,
            "123abc");

        document.Interfaces.Add(api);

        document.Interfaces.Count.ShouldBe(1);
        RsdApplicationInterface addedApi = document.Interfaces.First();
        addedApi.Name.ShouldBe("MetaWeblog");
        addedApi.Link.ShouldBe(new Uri("http://example.com/xml/rpc"));
        addedApi.IsPreferred.ShouldBeTrue();
        addedApi.WeblogId.ShouldBe("123abc");
    }

    /// <summary>
    /// Documentation, notes and named settings can be attached to an interface after it is constructed.
    /// </summary>
    [TestMethod]
    public void Interface_CanHaveOptionalProperties()
    {
        RsdApplicationInterface api = new(
            "Conversant",
            new Uri("http://example.com/xml/rpc"),
            false,
            string.Empty)
        {
            Documentation = new Uri("http://example.com/docs/"),
            Notes = "Additional notes here."
        };
        api.Settings.Add("custom-setting", "custom-value");

        api.Documentation.ShouldBe(new Uri("http://example.com/docs/"));
        api.Notes.ShouldBe("Additional notes here.");
        api.Settings["custom-setting"].ShouldBe("custom-value");
    }

    /// <summary>
    /// Saving writes an XML declaration and the <c>rsd</c>,
    /// <c>service</c>, <c>engineName</c> and <c>api</c> elements.
    /// </summary>
    [TestMethod]
    public void Save_ProducesValidXml()
    {
        RsdDocument document = new()
        {
            EngineName = "Test CMS",
            EngineLink = new Uri("http://example.com/cms"),
            Homepage = new Uri("http://example.com/")
        };
        document.Interfaces.Add(new RsdApplicationInterface(
            "MetaWeblog",
            new Uri("http://example.com/xml/rpc"),
            true,
            "123"));

        using MemoryStream stream = new();
        document.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<rsd");
        xml.ShouldContain("<service>");
        xml.ShouldContain("<engineName>Test CMS</engineName>");
        xml.ShouldContain("<api");
    }

    /// <summary>
    /// A document saved to a stream and loaded back keeps its engine name and its interface count.
    /// </summary>
    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a document
        RsdDocument originalDocument = new()
        {
            EngineName = "Test CMS",
            EngineLink = new Uri("http://example.com/cms"),
            Homepage = new Uri("http://example.com/")
        };
        originalDocument.Interfaces.Add(new RsdApplicationInterface(
            "MetaWeblog",
            new Uri("http://example.com/xml/rpc"),
            true,
            "123"));

        // Save to stream
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Load from stream
        stream.Position = 0;
        RsdDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Verify data preserved
        loadedDocument.EngineName.ShouldBe(originalDocument.EngineName);
        loadedDocument.Interfaces.Count.ShouldBe(originalDocument.Interfaces.Count);
    }
}