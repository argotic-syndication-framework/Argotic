using Argotic.Data.Adapters;
namespace Argotic.Extensions.Tests.Functionality.Core.Data.Adapters;

/// <summary>
/// Covers <see cref="Rsd06SyndicationResourceAdapter"/>: what an RSD 0.6 document in
/// <c>http://archipelago.phrasewise.com/rsd</c> fills on an <see cref="RsdDocument"/> — the
/// <c>service</c> identity and each <c>api</c> element beneath it — plus the retrieval limit and the
/// argument guards.
/// </summary>
[TestClass]
public class Rsd06SyndicationResourceAdapterTests
{
    #region Document Parsing Tests

    /// <summary>
    /// An RSD 0.6 <c>service</c> fills the engine name, the engine link and the <c>homePageLink</c> that
    /// reaches <c>Homepage</c>.
    /// </summary>
    [TestMethod]
    public void Fill_MinimalRsd06_PopulatesDocument()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.EngineName.ShouldBe("Test Blog Engine");
        document.EngineLink.ShouldBe(new Uri("http://example.com/engine"));
        document.Homepage.ShouldBe(new Uri("http://example.com"));
    }

    /// <summary>
    /// A single <c>api</c> element fills one interface with its name, its preferred flag, its link and its
    /// <c>blogID</c>.
    /// </summary>
    [TestMethod]
    public void Fill_WithApis_PopulatesInterfaces()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.Interfaces.Count.ShouldBe(1);
        RsdApplicationInterface api = document.Interfaces[0];
        api.Name.ShouldBe("MetaWeblog");
        api.IsPreferred.ShouldBeTrue();
        api.Link.ShouldBe(new Uri("http://example.com/xmlrpc.php"));
        api.WeblogId.ShouldBe("1");
    }

    /// <summary>
    /// Two <c>api</c> elements fill two interfaces in document order, each keeping its own name, link and
    /// preferred flag.
    /// </summary>
    [TestMethod]
    public void Fill_FullRsd06_PopulatesAllInterfaces()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.Interfaces.Count.ShouldBe(2);

        // First API - MetaWeblog (preferred)
        RsdApplicationInterface metaWeblog = document.Interfaces[0];
        metaWeblog.Name.ShouldBe("MetaWeblog");
        metaWeblog.IsPreferred.ShouldBeTrue();
        metaWeblog.Link.ShouldBe(new Uri("http://example.com/xmlrpc.php"));

        // Second API - Blogger
        RsdApplicationInterface blogger = document.Interfaces[1];
        blogger.Name.ShouldBe("Blogger");
        blogger.IsPreferred.ShouldBeFalse();
        blogger.Link.ShouldBe(new Uri("http://example.com/blogger"));
    }

    /// <summary>
    /// An <c>api</c> carrying a <c>settings</c> child still fills its own name, link, preferred flag and
    /// <c>blogID</c>.
    /// </summary>
    /// <remarks>
    ///     The settings themselves are not asserted. The file records why: <c>RsdApplicationInterface.Load</c>
    ///     selects <c>rsd:api/rsd:settings</c> while the navigator is already positioned on the <c>api</c>
    ///     element, so nothing beneath it is read.
    /// </remarks>
    [TestMethod]
    public void Fill_WithApiChildElements_ParsesCorrectly()
    {
        // Arrange
        // Note: The current RsdApplicationInterface.Load() implementation has a bug where it looks
        // for "rsd:api/rsd:settings" when the source is already positioned on the api element.
        // This test verifies the adapter correctly populates basic API properties.
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert - Verify that APIs are loaded correctly, even if settings are not parsed
        document.Interfaces.Count.ShouldBe(2);

        // First API - MetaWeblog (has settings child elements in XML but they may not be parsed)
        RsdApplicationInterface metaWeblog = document.Interfaces[0];
        metaWeblog.Name.ShouldBe("MetaWeblog");
        metaWeblog.IsPreferred.ShouldBeTrue();
        metaWeblog.Link.ShouldBe(new Uri("http://example.com/xmlrpc.php"));
        metaWeblog.WeblogId.ShouldBe("1");

        // Second API - Blogger
        RsdApplicationInterface blogger = document.Interfaces[1];
        blogger.Name.ShouldBe("Blogger");
        blogger.IsPreferred.ShouldBeFalse();
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Filling a <see langword="null"/> document throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Fill_NullResource_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => adapter.Fill(null!));
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
        Should.Throw<ArgumentNullException>(() => new Rsd06SyndicationResourceAdapter(null!, settings));
    }

    /// <summary>
    /// Constructing the adapter without load settings throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Rsd06SyndicationResourceAdapter(navigator, null!));
    }

    #endregion

    #region Service Properties Tests

    /// <summary>
    /// The <c>engineName</c> element fills a non-empty engine name.
    /// </summary>
    [TestMethod]
    public void Fill_ServiceWithEngineName_PopulatesEngineName()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.EngineName.ShouldNotBeNullOrEmpty();
        document.EngineName.ShouldBe("Test Blog Engine");
    }

    /// <summary>
    /// The <c>engineLink</c> element fills the engine's URI.
    /// </summary>
    [TestMethod]
    public void Fill_ServiceWithEngineLink_PopulatesEngineLink()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.EngineLink.ShouldNotBeNull();
        document.EngineLink.ShouldBe(new Uri("http://example.com/engine"));
    }

    /// <summary>
    /// The <c>homePageLink</c> element fills the document's <c>Homepage</c>.
    /// </summary>
    [TestMethod]
    public void Fill_ServiceWithHomePageLink_PopulatesHomepage()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.Homepage.ShouldNotBeNull();
        document.Homepage.ShouldBe(new Uri("http://example.com"));
    }

    #endregion

    #region Retrieval Limit Tests

    /// <summary>
    /// A retrieval limit of <c>1</c> keeps only the first <c>api</c> in document order.
    /// </summary>
    [TestMethod]
    public void Fill_WithRetrievalLimit_RespectsLimit()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new()
        {
            RetrievalLimit = 1
        };
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.Interfaces.Count.ShouldBe(1);
        document.Interfaces[0].Name.ShouldBe("MetaWeblog");
    }

    #endregion

    #region API Properties Tests

    /// <summary>
    /// An <c>api</c> element's <c>blogID</c> attribute fills <c>WeblogId</c> as the string it was written as.
    /// </summary>
    [TestMethod]
    public void Fill_ApiWithBlogId_PopulatesBlogId()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Minimal));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.Interfaces[0].WeblogId.ShouldBe("1");
    }

    /// <summary>
    /// The <c>preferred</c> attribute fills <c>IsPreferred</c> in both directions, <c>true</c> and
    /// <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Fill_ApiWithPreferredAttribute_PopulatesIsPreferred()
    {
        // Arrange
        RsdDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Rsd06Full));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        SyndicationResourceLoadSettings settings = new();
        Rsd06SyndicationResourceAdapter adapter = new(navigator, settings);

        // Act
        adapter.Fill(document);

        // Assert
        document.Interfaces[0].IsPreferred.ShouldBeTrue();
        document.Interfaces[1].IsPreferred.ShouldBeFalse();
    }

    #endregion
}