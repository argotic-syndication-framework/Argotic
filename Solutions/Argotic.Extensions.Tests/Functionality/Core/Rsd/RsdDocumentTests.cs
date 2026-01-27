using Argotic.Syndication.Specialized;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Rsd;

[TestClass]
public class RsdDocumentTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesEmptyDocument()
    {
        RsdDocument document = new RsdDocument();

        document.ShouldNotBeNull();
        document.Interfaces.Count.ShouldBe(0);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        RsdDocument document = new RsdDocument
        {
            EngineName = "Test CMS",
            EngineLink = new Uri("http://example.com/cms"),
            Homepage = new Uri("http://example.com/")
        };

        document.EngineName.ShouldBe("Test CMS");
        document.EngineLink.ShouldBe(new Uri("http://example.com/cms"));
        document.Homepage.ShouldBe(new Uri("http://example.com/"));
    }

    [TestMethod]
    public void Interfaces_Add_AddsInterfaceCorrectly()
    {
        RsdDocument document = new RsdDocument();
        RsdApplicationInterface api = new RsdApplicationInterface(
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

    [TestMethod]
    public void Interface_CanHaveOptionalProperties()
    {
        RsdApplicationInterface api = new RsdApplicationInterface(
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

    [TestMethod]
    public void Save_ProducesValidXml()
    {
        RsdDocument document = new RsdDocument
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

        using MemoryStream stream = new MemoryStream();
        document.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<rsd");
        xml.ShouldContain("<service>");
        xml.ShouldContain("<engineName>Test CMS</engineName>");
        xml.ShouldContain("<api");
    }

    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a document
        RsdDocument originalDocument = new RsdDocument
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
        using MemoryStream stream = new MemoryStream();
        originalDocument.Save(stream);

        // Load from stream
        stream.Position = 0;
        RsdDocument loadedDocument = new RsdDocument();
        loadedDocument.Load(stream);

        // Verify data preserved
        loadedDocument.EngineName.ShouldBe(originalDocument.EngineName);
        loadedDocument.Interfaces.Count.ShouldBe(originalDocument.Interfaces.Count);
    }
}
