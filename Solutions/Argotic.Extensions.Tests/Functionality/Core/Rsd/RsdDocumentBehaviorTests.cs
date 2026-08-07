using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication.Specialized;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Rsd;

/// <summary>
/// Covers <see cref="RsdDocument"/> end to end: declaring application interfaces, parsing an RSD
/// 1.0 document, and what survives a save and reload — the settings block included.
/// </summary>
[TestClass]
public class RsdDocumentBehaviorTests
{
    private const string MinimalRsd = """
        <?xml version="1.0"?>
        <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
            <service>
                <engineName>Test Engine</engineName>
                <engineLink>http://example.com</engineLink>
                <homePageLink>http://example.com/blog</homePageLink>
                <apis>
                    <api name="MetaWeblog" preferred="true" apiLink="http://example.com/xmlrpc.php" blogID="1"/>
                </apis>
            </service>
        </rsd>
        """;

    private const string RsdWithMultipleApis = """
        <?xml version="1.0"?>
        <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
            <service>
                <engineName>Blog Platform</engineName>
                <engineLink>http://blogplatform.example.com</engineLink>
                <homePageLink>http://myblog.example.com</homePageLink>
                <apis>
                    <api name="MetaWeblog" preferred="true" apiLink="http://example.com/xmlrpc" blogID="123"/>
                    <api name="Blogger" preferred="false" apiLink="http://example.com/blogger" blogID="123"/>
                    <api name="Atom" preferred="false" apiLink="http://example.com/atom" blogID="123"/>
                </apis>
            </service>
        </rsd>
        """;

    // A settings block nested directly under api, which is where RSD 1.0 puts it.
    private const string RsdWithApiSettings = """
        <?xml version="1.0"?>
        <rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
            <service>
                <engineName>Advanced CMS</engineName>
                <engineLink>http://cms.example.com</engineLink>
                <homePageLink>http://mysite.example.com</homePageLink>
                <apis>
                    <api name="Conversant" preferred="true" apiLink="http://example.com/api" blogID="">
                        <settings>
                            <docs>http://example.com/docs/api</docs>
                            <notes>Additional configuration notes</notes>
                            <setting name="auth-type">oauth2</setting>
                            <setting name="api-version">2.0</setting>
                        </settings>
                    </api>
                </apis>
            </service>
        </rsd>
        """;

    public TestContext? TestContext { get; set; }

    #region Document Creation Tests

    /// <summary>
    /// A default-constructed document has an interface collection
    /// and an extension collection, both empty rather than null.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenCreatedWithDefaultConstructor_HasEmptyCollections()
    {
        // Arrange & Act
        RsdDocument document = new();

        // Assert
        document.Interfaces.ShouldNotBeNull();
        document.Interfaces.ShouldBeEmpty();
        document.Extensions.ShouldNotBeNull();
        document.Extensions.ShouldBeEmpty();
    }

    /// <summary>
    /// The engine name, the engine link and the homepage are read back exactly as assigned.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenServicePropertiesSet_ContainsCorrectValues()
    {
        // Arrange & Act
        RsdDocument document = new()
        {
            EngineName = "Blog Munging CMS",
            EngineLink = new Uri("http://www.blogmunging.com/"),
            Homepage = new Uri("http://www.userdomain.com/")
        };

        // Assert
        document.EngineName.ShouldBe("Blog Munging CMS");
        document.EngineLink.ShouldBe(new Uri("http://www.blogmunging.com/"));
        document.Homepage.ShouldBe(new Uri("http://www.userdomain.com/"));
    }

    /// <summary>
    /// An interface added to the document appears in the collection with its name and preferred flag.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenApiInterfaceAdded_ContainsInterface()
    {
        // Arrange
        RsdDocument document = new();
        RsdApplicationInterface api = new(
            "MetaWeblog",
            new Uri("http://example.com/xmlrpc"),
            true,
            "123abc");

        // Act
        document.Interfaces.Add(api);

        // Assert
        document.Interfaces.Count.ShouldBe(1);
        document.Interfaces[0].Name.ShouldBe("MetaWeblog");
        document.Interfaces[0].IsPreferred.ShouldBeTrue();
    }

    /// <summary>
    /// Interfaces are held in the order they were added.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenMultipleApisAdded_ContainsAllApis()
    {
        // Arrange
        RsdDocument document = new()
        {
            EngineName = "Test CMS",
            EngineLink = new Uri("http://example.com/cms"),
            Homepage = new Uri("http://example.com/")
        };

        // Act
        document.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xmlrpc"), true, "123"));
        document.Interfaces.Add(new RsdApplicationInterface("Blogger", new Uri("http://example.com/blogger"), false, "123"));
        document.Interfaces.Add(new RsdApplicationInterface("Atom", new Uri("http://example.com/atom"), false, "123"));

        // Assert
        document.Interfaces.Count.ShouldBe(3);
        document.Interfaces[0].Name.ShouldBe("MetaWeblog");
        document.Interfaces[1].Name.ShouldBe("Blogger");
        document.Interfaces[2].Name.ShouldBe("Atom");
    }

    /// <summary>
    /// The document indexer reads the interface at a position in the collection.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenIndexerUsed_ReturnsCorrectInterface()
    {
        // Arrange
        RsdDocument document = new();
        document.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xmlrpc"), true, "1"));
        document.Interfaces.Add(new RsdApplicationInterface("Blogger", new Uri("http://example.com/blogger"), false, "2"));

        // Act
        RsdApplicationInterface firstApi = document[0];
        RsdApplicationInterface secondApi = document[1];

        // Assert
        firstApi.Name.ShouldBe("MetaWeblog");
        secondApi.Name.ShouldBe("Blogger");
    }

    /// <summary>
    /// An interface carries its four constructor arguments alongside
    /// the documentation, notes and named settings set afterwards.
    /// </summary>
    [TestMethod]
    public void RsdApplicationInterface_WhenCreatedWithOptionalProperties_ContainsAllProperties()
    {
        // Arrange & Act
        RsdApplicationInterface api = new("Conversant", new Uri("http://example.com/api"), false, "123")
        {
            Documentation = new Uri("http://example.com/docs/"),
            Notes = "Additional API notes here."
        };
        api.Settings.Add("auth-type", "oauth2");
        api.Settings.Add("api-version", "2.0");

        // Assert
        api.Name.ShouldBe("Conversant");
        api.Link.ShouldBe(new Uri("http://example.com/api"));
        api.IsPreferred.ShouldBeFalse();
        api.WeblogId.ShouldBe("123");
        api.Documentation.ShouldBe(new Uri("http://example.com/docs/"));
        api.Notes.ShouldBe("Additional API notes here.");
        api.Settings.Count.ShouldBe(2);
        api.Settings["auth-type"].ShouldBe("oauth2");
        api.Settings["api-version"].ShouldBe("2.0");
    }

    #endregion

    #region Document Parsing Tests

    /// <summary>
    /// Loading an RSD 1.0 document populates the engine name, the
    /// engine link and the homepage from the <c>service</c> element.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenLoadedFromXml_PopulatesServiceProperties()
    {
        // Arrange
        RsdDocument document = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(MinimalRsd));
        document.Load(reader);

        // Assert
        document.EngineName.ShouldBe("Test Engine");
        document.EngineLink.ShouldBe(new Uri("http://example.com"));
        document.Homepage.ShouldBe(new Uri("http://example.com/blog"));
    }

    /// <summary>
    /// An <c>api</c> element loads with its name, its <c>preferred</c>
    /// flag, its <c>apiLink</c> and its <c>blogID</c>.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenLoadedFromXml_PopulatesApiInterface()
    {
        // Arrange
        RsdDocument document = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(MinimalRsd));
        document.Load(reader);

        // Assert
        document.Interfaces.Count.ShouldBe(1);
        RsdApplicationInterface api = document.Interfaces[0];
        api.Name.ShouldBe("MetaWeblog");
        api.IsPreferred.ShouldBeTrue();
        api.Link.ShouldBe(new Uri("http://example.com/xmlrpc.php"));
        api.WeblogId.ShouldBe("1");
    }

    /// <summary>
    /// Every <c>api</c> element under <c>apis</c> loads, in document order, each with its own preferred flag.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenLoadedFromXmlWithMultipleApis_PopulatesAllInterfaces()
    {
        // Arrange
        RsdDocument document = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(RsdWithMultipleApis));
        document.Load(reader);

        // Assert
        document.Interfaces.Count.ShouldBe(3);
        document.Interfaces[0].Name.ShouldBe("MetaWeblog");
        document.Interfaces[0].IsPreferred.ShouldBeTrue();
        document.Interfaces[1].Name.ShouldBe("Blogger");
        document.Interfaces[1].IsPreferred.ShouldBeFalse();
        document.Interfaces[2].Name.ShouldBe("Atom");
        document.Interfaces[2].IsPreferred.ShouldBeFalse();
    }

    /// <summary>
    /// An <c>api</c> element carrying a <c>settings</c> block loads its name, preferred flag and link,
    /// and also the documentation link, the notes and every named setting inside that block.
    /// </summary>
    /// <remarks>
    ///     <c>RsdApplicationInterface.Load</c> used to select the settings with the XPath
    ///     <c>rsd:api/rsd:settings</c> from a navigator already positioned on the <c>api</c> element — an
    ///     <c>api</c> nested inside an <c>api</c>, which no RSD document contains. <c>docs</c>, <c>notes</c>
    ///     and every <c>setting</c> were therefore silently dropped on both RSD load paths. The step is now
    ///     <c>rsd:settings</c>, and this test is the pin that keeps it there.
    /// </remarks>
    [TestMethod]
    public void RsdDocument_WhenLoadedFromXmlWithApiSettings_ReadsDocumentationNotesAndSettings()
    {
        // Arrange
        RsdDocument document = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(RsdWithApiSettings));
        document.Load(reader);

        // Assert - Basic API properties are parsed correctly
        document.Interfaces.Count.ShouldBe(1);
        RsdApplicationInterface api = document.Interfaces[0];
        api.Name.ShouldBe("Conversant");
        api.IsPreferred.ShouldBeTrue();
        api.Link.ShouldBe(new Uri("http://example.com/api"));

        // Assert - and the settings block, which is the whole point of the element
        api.Documentation.ShouldBe(new Uri("http://example.com/docs/api"));
        api.Notes.ShouldBe("Additional configuration notes");
        api.Settings.Count.ShouldBe(2);
        api.Settings["auth-type"].ShouldBe("oauth2");
        api.Settings["api-version"].ShouldBe("2.0");
    }

    /// <summary>
    /// Loading from a stream yields the same engine name and interfaces as loading from a reader.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenLoadedFromStream_PopulatesProperties()
    {
        // Arrange
        RsdDocument document = new();

        // Act
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(MinimalRsd));
        document.Load(stream);

        // Assert
        document.EngineName.ShouldBe("Test Engine");
        document.Interfaces.Count.ShouldBe(1);
    }

    /// <summary>
    /// Loading raises <c>Loaded</c>, and the handler receives event arguments rather than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenLoaded_RaisesLoadedEvent()
    {
        // Arrange
        RsdDocument document = new();
        bool eventRaised = false;
        SyndicationResourceLoadedEventArgs? eventArgs = null;
        document.Loaded += (_, args) =>
        {
            eventRaised = true;
            eventArgs = args;
        };

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(MinimalRsd));
        document.Load(reader);

        // Assert
        eventRaised.ShouldBeTrue();
        eventArgs.ShouldNotBeNull();
    }

    /// <summary>
    /// The one interface marked <c>preferred="true"</c> is the one a search of the collection finds.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenPreferredApiRequested_CanBeFoundInCollection()
    {
        // Arrange
        RsdDocument document = new();
        using XmlReader reader = XmlReader.Create(new StringReader(RsdWithMultipleApis));
        document.Load(reader);

        // Act
        RsdApplicationInterface? preferredApi = document.Interfaces.FirstOrDefault(api => api.IsPreferred);

        // Assert
        preferredApi.ShouldNotBeNull();
        preferredApi.Name.ShouldBe("MetaWeblog");

        // In RsdWithMultipleApis the preferred interface is also the first, so FirstOrDefault(api =>
        // api.IsPreferred) and FirstOrDefault(_ => true) return the same element: the predicate is a
        // no-op on that fixture and a parser marking every interface preferred passed. The flag is
        // therefore asserted across the whole collection, where position and flag disagree.
        document.Interfaces.Count.ShouldBe(3);
        document.Interfaces.Select(api => api.IsPreferred).ShouldBe([true, false, false]);
    }

    #endregion

    #region Round-Trip Tests

    /// <summary>
    /// The engine name, the engine link and the homepage survive a save and reload.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenSavedAndReloaded_PreservesServiceProperties()
    {
        // Arrange
        RsdDocument originalDocument = new()
        {
            EngineName = "Round Trip CMS",
            EngineLink = new Uri("http://roundtrip.example.com/cms"),
            Homepage = new Uri("http://roundtrip.example.com/")
        };

        // Act
        using MemoryStream stream = new();
        originalDocument.Save(stream);
        stream.Position = 0;

        RsdDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.EngineName.ShouldBe(originalDocument.EngineName);
        loadedDocument.EngineLink.ShouldBe(originalDocument.EngineLink);
        loadedDocument.Homepage.ShouldBe(originalDocument.Homepage);
    }

    /// <summary>
    /// Both interfaces survive a save and reload, each with its name, link, preferred flag and weblog identifier.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenSavedAndReloaded_PreservesApiInterfaces()
    {
        // Arrange
        RsdDocument originalDocument = new()
        {
            EngineName = "Test CMS",
            EngineLink = new Uri("http://example.com/cms"),
            Homepage = new Uri("http://example.com/")
        };
        originalDocument.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xmlrpc"), true, "123"));
        originalDocument.Interfaces.Add(new RsdApplicationInterface("Blogger", new Uri("http://example.com/blogger"), false, "456"));

        // Act
        using MemoryStream stream = new();
        originalDocument.Save(stream);
        stream.Position = 0;

        RsdDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Interfaces.Count.ShouldBe(originalDocument.Interfaces.Count);
        for (int i = 0; i < originalDocument.Interfaces.Count; i++)
        {
            loadedDocument.Interfaces[i].Name.ShouldBe(originalDocument.Interfaces[i].Name);
            loadedDocument.Interfaces[i].Link.ShouldBe(originalDocument.Interfaces[i].Link);
            loadedDocument.Interfaces[i].IsPreferred.ShouldBe(originalDocument.Interfaces[i].IsPreferred);
            loadedDocument.Interfaces[i].WeblogId.ShouldBe(originalDocument.Interfaces[i].WeblogId);
        }
    }

    /// <summary>
    /// A second save and reload leaves the service properties and the interface count where the first one left them.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenSavedAndReloadedTwice_MaintainsDataIntegrity()
    {
        // Arrange
        RsdDocument originalDocument = CreateCompleteRsdDocument();

        // Act - First round trip
        using MemoryStream stream1 = new();
        originalDocument.Save(stream1);
        stream1.Position = 0;
        RsdDocument document1 = new();
        document1.Load(stream1);

        // Act - Second round trip
        using MemoryStream stream2 = new();
        document1.Save(stream2);
        stream2.Position = 0;
        RsdDocument document2 = new();
        document2.Load(stream2);

        // Assert
        document2.EngineName.ShouldBe(originalDocument.EngineName);
        document2.EngineLink.ShouldBe(originalDocument.EngineLink);
        document2.Homepage.ShouldBe(originalDocument.Homepage);
        document2.Interfaces.Count.ShouldBe(originalDocument.Interfaces.Count);
    }

    /// <summary>
    /// A parsed document that is written out and read back keeps its
    /// engine name and every interface, preferred flag included.
    /// </summary>
    [TestMethod]
    public void RsdDocument_WhenParsedFromXmlAndReserialized_MaintainsStructure()
    {
        // Arrange
        RsdDocument document = new();
        using XmlReader reader = XmlReader.Create(new StringReader(RsdWithMultipleApis));
        document.Load(reader);

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        RsdDocument reloadedDocument = new();
        reloadedDocument.Load(stream);

        // Assert
        // The first two used to read their expected values off `document`, which the same save/load
        // pipeline produced; the literals make a dropped engineName or a lost interface visible.
        reloadedDocument.EngineName.ShouldBe("Blog Platform");
        reloadedDocument.Interfaces.Count.ShouldBe(3);
        reloadedDocument.EngineName.ShouldBe(document.EngineName);
        reloadedDocument.Interfaces.Count.ShouldBe(document.Interfaces.Count);
        reloadedDocument.Interfaces[0].Name.ShouldBe("MetaWeblog");
        reloadedDocument.Interfaces[0].IsPreferred.ShouldBeTrue();
    }

    #endregion

    #region Format and Version Tests

    /// <summary>
    /// A document reports <c>SyndicationContentFormat.Rsd</c> as its format.
    /// </summary>
    /// <remarks>
    ///     The type used to hard-code <c>SyndicationContentFormat.Opml</c> while both of its load paths passed
    ///     <c>SyndicationContentFormat.Rsd</c> to the adapter — so the one public place a consumer could ask
    ///     what it was holding answered with a different format entirely. Nothing inside the library reads
    ///     <see cref="ISyndicationResource.Format"/>, which is why it went unnoticed: the blast radius is
    ///     consumer dispatch, and consumer dispatch is exactly what the property exists for.
    /// </remarks>
    [TestMethod]
    public void RsdDocument_Format_ReturnsRsd()
    {
        // Arrange
        RsdDocument document = new();

        // Assert
        document.Format.ShouldBe(SyndicationContentFormat.Rsd);
    }

    /// <summary>
    /// A document reports version <c>1.0</c>, which is the version it writes rather than one it was told.
    /// </summary>
    [TestMethod]
    public void RsdDocument_Version_Returns1_0()
    {
        // Arrange
        RsdDocument document = new();

        // Assert
        document.Version.ShouldBe(new Version(1, 0));
    }

    #endregion

    #region Navigator Tests

    /// <summary>
    /// A document exposes a navigator over its own XML, rooted on a node that has children.
    /// </summary>
    [TestMethod]
    public void RsdDocument_CreateNavigator_ReturnsValidNavigator()
    {
        // Arrange
        RsdDocument document = new()
        {
            EngineName = "Navigator Test CMS",
            EngineLink = new Uri("http://example.com/"),
            Homepage = new Uri("http://example.com/blog")
        };
        document.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/api"), true, "1"));

        // Act
        XPathNavigator navigator = document.CreateNavigator();

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.HasChildren.ShouldBeTrue();
    }

    #endregion

    #region Extension Tests

    /// <summary>
    /// A document carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void RsdDocument_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        RsdDocument document = new();

        // Assert
        document.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// An interface carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void RsdApplicationInterface_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        RsdApplicationInterface api = new("Test", new Uri("http://example.com/"), false, "1");

        // Assert
        api.HasExtensions.ShouldBeFalse();
    }

    #endregion

    #region Async Operations Tests

    /// <summary>
    /// Loading over a caller-supplied client populates the service
    /// properties and the interfaces, and raises <c>Loaded</c>.
    /// </summary>
    [TestMethod]
    public async Task RsdDocument_LoadAsync_LoadsDocumentCorrectly()
    {
        // Arrange
        RsdDocument document = new();
        bool eventRaised = false;
        document.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalRsd);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(
            new Uri("http://example.com/rsd.xml"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue();
        document.EngineName.ShouldBe("Test Engine");
        document.EngineLink.ShouldBe(new Uri("http://example.com"));
        document.Homepage.ShouldBe(new Uri("http://example.com/blog"));
        document.Interfaces.Count.ShouldBe(1);
    }

    /// <summary>
    /// The static create returns a document already populated from the response body, all three interfaces included.
    /// </summary>
    [TestMethod]
    public async Task RsdDocument_CreateAsync_CreatesAndLoadsNewDocument()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(RsdWithMultipleApis);
        using HttpClient httpClient = new(handler);

        // Act
        RsdDocument document = await RsdDocument.CreateAsync(
            new Uri("http://example.com/rsd.xml"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        document.ShouldNotBeNull();
        document.EngineName.ShouldBe("Blog Platform");
        document.Interfaces.Count.ShouldBe(3);
    }

    /// <summary>
    /// Loading over HTTP preserves the document order of the interfaces, as loading from a reader does.
    /// </summary>
    [TestMethod]
    public async Task RsdDocument_LoadAsync_WithMultipleApis_LoadsAllInterfaces()
    {
        // Arrange
        RsdDocument document = new();

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(RsdWithMultipleApis);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(
            new Uri("http://example.com/rsd.xml"),
            httpClient,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        document.Interfaces.Count.ShouldBe(3);
        document.Interfaces[0].Name.ShouldBe("MetaWeblog");
        document.Interfaces[0].IsPreferred.ShouldBeTrue();
        document.Interfaces[1].Name.ShouldBe("Blogger");
        document.Interfaces[2].Name.ShouldBe("Atom");
    }

    /// <summary>
    /// The <c>Loaded</c> event reports the URI the document was fetched from.
    /// </summary>
    [TestMethod]
    public async Task RsdDocument_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        RsdDocument document = new();
        Uri? sourceFromEvent = null;

        document.Loaded += (_, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalRsd);
        using HttpClient httpClient = new(handler);
        Uri requestUri = new("http://example.com/rsd.xml");

        // Act
        await document.LoadAsync(requestUri, httpClient, cancellationToken: TestContext!.CancellationToken);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    /// <summary>
    /// Passing load settings to the asynchronous load still yields a fully populated document.
    /// </summary>
    [TestMethod]
    public async Task RsdDocument_LoadAsync_WithSettings_AppliesSettings()
    {
        // Arrange
        RsdDocument document = new();
        SyndicationResourceLoadSettings settings = new()
        {
            AutoDetectExtensions = true
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalRsd);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(
            new Uri("http://example.com/rsd.xml"),
            httpClient,
            settings,
            cancellationToken: TestContext!.CancellationToken);

        // Assert
        document.EngineName.ShouldBe("Test Engine");
        document.Interfaces.Count.ShouldBe(1);
    }

    #endregion

    #region Helper Methods

    private static RsdDocument CreateCompleteRsdDocument()
    {
        RsdDocument document = new()
        {
            EngineName = "Complete CMS",
            EngineLink = new Uri("http://complete.example.com/cms"),
            Homepage = new Uri("http://complete.example.com/")
        };

        document.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://complete.example.com/xmlrpc"), true, "blog1"));
        document.Interfaces.Add(new RsdApplicationInterface("Blogger", new Uri("http://complete.example.com/blogger"), false, "blog1"));

        RsdApplicationInterface advancedApi = new("Conversant", new Uri("http://complete.example.com/conversant"), false, "")
        {
            Documentation = new Uri("http://complete.example.com/docs/"),
            Notes = "Full featured API with settings"
        };
        advancedApi.Settings.Add("feature1", "enabled");
        advancedApi.Settings.Add("feature2", "configured");
        document.Interfaces.Add(advancedApi);

        return document;
    }

    #endregion
}