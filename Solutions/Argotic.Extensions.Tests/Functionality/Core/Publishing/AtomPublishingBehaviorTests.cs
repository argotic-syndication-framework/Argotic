using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Publishing;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

[TestClass]
public class AtomPublishingBehaviorTests
{
    #region AtomAcceptedMediaRange Tests

    [TestMethod]
    public void AtomAcceptedMediaRange_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var mediaRange = new AtomAcceptedMediaRange();

        // Assert
        mediaRange.MediaRange.ShouldBe(string.Empty);
        mediaRange.BaseUri.ShouldBeNull();
        mediaRange.Language.ShouldBeNull();
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_Constructor_WithMediaRange_SetsProperty()
    {
        // Arrange
        string range = "image/png";

        // Act
        var mediaRange = new AtomAcceptedMediaRange(range);

        // Assert
        mediaRange.MediaRange.ShouldBe(range);
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_MediaRange_TrimsWhitespace()
    {
        // Arrange
        var mediaRange = new AtomAcceptedMediaRange();

        // Act
        mediaRange.MediaRange = "  application/json  ";

        // Assert
        mediaRange.MediaRange.ShouldBe("application/json");
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_AtomEntryMediaRange_ReturnsCorrectValue()
    {
        // Act
        string range = AtomAcceptedMediaRange.AtomEntryMediaRange;

        // Assert
        range.ShouldBe("application/atom+xml;type=entry");
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_AtomFeedMediaRange_ReturnsCorrectValue()
    {
        // Act
        string range = AtomAcceptedMediaRange.AtomFeedMediaRange;

        // Assert
        range.ShouldBe("application/atom+xml;type=feed");
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_Load_LoadsFromXml()
    {
        // Arrange
        string xml = "<accept xmlns=\"http://www.w3.org/2007/app\">image/png</accept>";
        var navigator = CreateNavigator(xml);
        navigator.MoveToFirstChild();

        var mediaRange = new AtomAcceptedMediaRange();

        // Act
        bool loaded = mediaRange.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        mediaRange.MediaRange.ShouldBe("image/png");
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_WriteTo_WritesXml()
    {
        // Arrange
        var mediaRange = new AtomAcceptedMediaRange("image/png");

        // Act
        string xml = mediaRange.ToString();

        // Assert
        xml.ShouldContain("<accept");
        xml.ShouldContain("image/png");
        xml.ShouldContain("</accept>");
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var mediaRange = new AtomAcceptedMediaRange("image/png");

        // Act
        int result = mediaRange.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_CompareTo_WithEqual_ReturnsZero()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/png");

        // Act
        int result = range1.CompareTo(range2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_Equals_WithSameRange_ReturnsTrue()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/png");

        // Act & Assert
        range1.Equals(range2).ShouldBeTrue();
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_EqualityOperator_Works()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/png");

        // Act & Assert
        (range1 == range2).ShouldBeTrue();
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_InequalityOperator_Works()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/jpeg");

        // Act & Assert
        (range1 != range2).ShouldBeTrue();
    }

    [TestMethod]
    public void AtomAcceptedMediaRange_HasExtensions_WhenEmpty_ReturnsFalse()
    {
        // Arrange
        var mediaRange = new AtomAcceptedMediaRange();

        // Act & Assert
        mediaRange.HasExtensions.ShouldBeFalse();
    }

    #endregion

    #region AtomWorkspace Tests

    [TestMethod]
    public void AtomWorkspace_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var workspace = new AtomWorkspace();

        // Assert
        workspace.Title.ShouldNotBeNull();
        workspace.Collections.ShouldNotBeNull();
        workspace.Collections.Count.ShouldBe(0);
    }

    [TestMethod]
    public void AtomWorkspace_Constructor_WithTitle_SetsTitle()
    {
        // Arrange
        var title = new AtomTextConstruct("My Workspace");

        // Act
        var workspace = new AtomWorkspace(title);

        // Assert
        workspace.Title.Content.ShouldBe("My Workspace");
    }

    [TestMethod]
    public void AtomWorkspace_Constructor_WithTitleAndCollections_SetsBoth()
    {
        // Arrange
        var title = new AtomTextConstruct("Blog");
        var collection = new AtomMemberResources
        {
            Uri = new Uri("http://example.com/posts"),
            Title = new AtomTextConstruct("Posts")
        };
        List<AtomMemberResources> collections = [collection];

        // Act
        var workspace = new AtomWorkspace(title, collections);

        // Assert
        workspace.Title.Content.ShouldBe("Blog");
        workspace.Collections.Count.ShouldBe(1);
        workspace.Collections[0].Uri.ShouldBe(new Uri("http://example.com/posts"));
    }

    [TestMethod]
    public void AtomWorkspace_Indexer_GetsCollection()
    {
        // Arrange
        var workspace = new AtomWorkspace(new AtomTextConstruct("Workspace"));
        workspace.Collections.Add(new AtomMemberResources
        {
            Uri = new Uri("http://example.com/collection"),
            Title = new AtomTextConstruct("Collection")
        });

        // Act
        var collection = workspace[0];

        // Assert
        collection.Uri.ShouldBe(new Uri("http://example.com/collection"));
    }

    [TestMethod]
    public void AtomWorkspace_Indexer_SetsCollection()
    {
        // Arrange
        var workspace = new AtomWorkspace(new AtomTextConstruct("Workspace"));
        workspace.Collections.Add(new AtomMemberResources
        {
            Uri = new Uri("http://example.com/old"),
            Title = new AtomTextConstruct("Old")
        });
        var newCollection = new AtomMemberResources
        {
            Uri = new Uri("http://example.com/new"),
            Title = new AtomTextConstruct("New")
        };

        // Act
        workspace[0] = newCollection;

        // Assert
        workspace[0].Uri.ShouldBe(new Uri("http://example.com/new"));
    }

    [TestMethod]
    public void AtomWorkspace_Load_LoadsFromXml()
    {
        // Arrange
        string xml = """
            <workspace xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
                <atom:title>Main Workspace</atom:title>
            </workspace>
            """;
        var navigator = CreateNavigator(xml);
        navigator.MoveToFirstChild();
        var workspace = new AtomWorkspace();

        // Act
        bool loaded = workspace.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        workspace.Title.Content.ShouldBe("Main Workspace");
    }

    [TestMethod]
    public void AtomWorkspace_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var workspace = new AtomWorkspace(new AtomTextConstruct("Test"));

        // Act
        int result = workspace.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void AtomWorkspace_Equals_WithEqual_ReturnsTrue()
    {
        // Arrange
        var ws1 = new AtomWorkspace(new AtomTextConstruct("Test"));
        var ws2 = new AtomWorkspace(new AtomTextConstruct("Test"));

        // Act & Assert
        ws1.Equals(ws2).ShouldBeTrue();
    }

    #endregion

    #region AtomServiceDocument Tests

    [TestMethod]
    public void AtomServiceDocument_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var doc = new AtomServiceDocument();

        // Assert
        doc.Workspaces.ShouldNotBeNull();
        doc.Workspaces.Count.ShouldBe(0);
    }

    [TestMethod]
    public void AtomServiceDocument_Constructor_WithWorkspaces_SetsWorkspaces()
    {
        // Arrange
        var workspace = new AtomWorkspace(new AtomTextConstruct("Main"));
        List<AtomWorkspace> workspaces = [workspace];

        // Act
        var doc = new AtomServiceDocument(workspaces);

        // Assert
        doc.Workspaces.Count.ShouldBe(1);
        doc.Workspaces[0].Title.Content.ShouldBe("Main");
    }

    [TestMethod]
    public void AtomServiceDocument_Constructor_WithEmptyWorkspaces_ThrowsException()
    {
        // Arrange
        List<AtomWorkspace> workspaces = [];

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => new AtomServiceDocument(workspaces));
    }

    [TestMethod]
    public void AtomServiceDocument_Indexer_GetsWorkspace()
    {
        // Arrange
        var workspace = new AtomWorkspace(new AtomTextConstruct("Blog"));
        var doc = new AtomServiceDocument([workspace]);

        // Act
        var result = doc[0];

        // Assert
        result.Title.Content.ShouldBe("Blog");
    }

    [TestMethod]
    public void AtomServiceDocument_Format_ReturnsAtomServiceDocument()
    {
        // Arrange
        var doc = new AtomServiceDocument();

        // Act
        var format = doc.Format;

        // Assert
        format.ShouldBe(SyndicationContentFormat.AtomServiceDocument);
    }

    [TestMethod]
    public void AtomServiceDocument_Version_ReturnsOneZero()
    {
        // Arrange
        var doc = new AtomServiceDocument();

        // Act
        var version = doc.Version;

        // Assert
        version.Major.ShouldBe(1);
        version.Minor.ShouldBe(0);
    }

    [TestMethod]
    public void AtomServiceDocument_Load_ParsesServiceDocument()
    {
        // Arrange
        string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
                <workspace>
                    <atom:title>My Blog</atom:title>
                    <collection href="http://example.com/posts">
                        <atom:title>Blog Posts</atom:title>
                        <accept>application/atom+xml;type=entry</accept>
                    </collection>
                </workspace>
            </service>
            """;
        using var reader = XmlReader.Create(new StringReader(xml));
        var doc = new AtomServiceDocument();

        // Act
        doc.Load(reader);

        // Assert
        doc.Workspaces.Count.ShouldBe(1);
        doc.Workspaces[0].Title.Content.ShouldBe("My Blog");
        doc.Workspaces[0].Collections.Count.ShouldBe(1);
        doc.Workspaces[0].Collections[0].Title.Content.ShouldBe("Blog Posts");
    }

    #endregion

    #region AtomMemberResources Tests

    [TestMethod]
    public void AtomMemberResources_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var resources = new AtomMemberResources();

        // Assert
        resources.Uri.ShouldBeNull();
        resources.Title.ShouldNotBeNull();
        resources.Accepts.ShouldNotBeNull();
        resources.Categories.ShouldNotBeNull();
    }

    [TestMethod]
    public void AtomMemberResources_Constructor_WithUriAndTitle_SetsBoth()
    {
        // Arrange
        var uri = new Uri("http://example.com/collection");
        var title = new AtomTextConstruct("My Collection");

        // Act
        var resources = new AtomMemberResources(uri, title);

        // Assert
        resources.Uri.ShouldBe(uri);
        resources.Title.Content.ShouldBe("My Collection");
    }

    [TestMethod]
    public void AtomMemberResources_Accepts_CanAddMediaRanges()
    {
        // Arrange
        var resources = new AtomMemberResources(
            new Uri("http://example.com/collection"),
            new AtomTextConstruct("Collection"));

        // Act
        resources.Accepts.Add(new AtomAcceptedMediaRange(AtomAcceptedMediaRange.AtomEntryMediaRange));

        // Assert
        resources.Accepts.Count.ShouldBe(1);
        resources.Accepts[0].MediaRange.ShouldBe("application/atom+xml;type=entry");
    }

    [TestMethod]
    public void AtomMemberResources_SlugEncode_ReturnsEncodedString()
    {
        // Arrange
        string input = "My First Blog Post!";

        // Act
        string encoded = AtomMemberResources.SlugEncode(input);

        // Assert
        // SlugEncode returns a valid slug header value
        encoded.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void AtomMemberResources_SlugDecode_DecodesString()
    {
        // Arrange
        string encoded = "My%20First%20Blog%20Post";

        // Act
        string decoded = AtomMemberResources.SlugDecode(encoded);

        // Assert
        decoded.ShouldBe("My First Blog Post");
    }

    [TestMethod]
    public void AtomMemberResources_Load_LoadsFromXml()
    {
        // Arrange
        string xml = """
            <collection href="http://example.com/posts" xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
                <atom:title>Blog Posts</atom:title>
                <accept>application/atom+xml;type=entry</accept>
            </collection>
            """;
        var navigator = CreateNavigator(xml);
        navigator.MoveToFirstChild();
        var resources = new AtomMemberResources();

        // Act
        bool loaded = resources.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        resources.Uri.ShouldBe(new Uri("http://example.com/posts"));
        resources.Title.Content.ShouldBe("Blog Posts");
        resources.Accepts.Count.ShouldBe(1);
    }

    [TestMethod]
    public void AtomMemberResources_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var resources = new AtomMemberResources(
            new Uri("http://example.com/collection"),
            new AtomTextConstruct("Collection"));

        // Act
        int result = resources.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    #endregion

    #region AtomCategoryDocument Tests

    [TestMethod]
    public void AtomCategoryDocument_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var doc = new AtomCategoryDocument();

        // Assert
        doc.Categories.ShouldNotBeNull();
        doc.Categories.Count.ShouldBe(0);
        doc.IsFixed.ShouldBeFalse();
        doc.Scheme.ShouldBeNull();
    }

    [TestMethod]
    public void AtomCategoryDocument_AddCategory_AddsToCollection()
    {
        // Arrange
        var doc = new AtomCategoryDocument();
        var category = new AtomCategory("tech");

        // Act
        doc.Categories.Add(category);

        // Assert
        doc.Categories.Count.ShouldBe(1);
        doc.Categories[0].Term.ShouldBe("tech");
    }

    [TestMethod]
    public void AtomCategoryDocument_IsFixed_CanBeSet()
    {
        // Arrange
        var doc = new AtomCategoryDocument();

        // Act
        doc.IsFixed = true;

        // Assert
        doc.IsFixed.ShouldBeTrue();
    }

    [TestMethod]
    public void AtomCategoryDocument_Scheme_CanBeSet()
    {
        // Arrange
        var doc = new AtomCategoryDocument();
        var scheme = new Uri("http://example.com/categories");

        // Act
        doc.Scheme = scheme;

        // Assert
        doc.Scheme.ShouldBe(scheme);
    }

    [TestMethod]
    public void AtomCategoryDocument_Load_ParsesCategoryDocument()
    {
        // Arrange
        string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <categories xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom" fixed="yes" scheme="http://example.com/categories">
                <atom:category term="tech"/>
                <atom:category term="news"/>
            </categories>
            """;
        using var reader = XmlReader.Create(new StringReader(xml));
        var doc = new AtomCategoryDocument();

        // Act
        doc.Load(reader);

        // Assert
        doc.IsFixed.ShouldBeTrue();
        doc.Scheme.ShouldBe(new Uri("http://example.com/categories"));
        doc.Categories.Count.ShouldBe(2);
    }

    [TestMethod]
    public void AtomCategoryDocument_Format_ReturnsAtomCategoryDocument()
    {
        // Arrange
        var doc = new AtomCategoryDocument();

        // Act
        var format = doc.Format;

        // Assert
        format.ShouldBe(SyndicationContentFormat.AtomCategoryDocument);
    }

    [TestMethod]
    public void AtomCategoryDocument_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var doc = new AtomCategoryDocument();

        // Act
        int result = doc.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    #endregion

    #region AtomEntryResource Tests

    [TestMethod]
    public void AtomEntryResource_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var resource = new AtomEntryResource();

        // Assert
        resource.EditedOn.ShouldBe(DateTime.MinValue);
        resource.IsDraft.ShouldBeFalse();
    }

    [TestMethod]
    public void AtomEntryResource_Constructor_WithParameters_SetsProperties()
    {
        // Arrange
        var id = new AtomId(new Uri("urn:uuid:test"));
        var title = new AtomTextConstruct("Test Entry");
        var updatedOn = DateTime.UtcNow;

        // Act
        var resource = new AtomEntryResource(id, title, updatedOn);

        // Assert
        resource.Title!.Content.ShouldBe("Test Entry");
        resource.UpdatedOn.ShouldBe(updatedOn);
    }

    [TestMethod]
    public void AtomEntryResource_EditedOn_CanBeSet()
    {
        // Arrange
        var resource = new AtomEntryResource();
        var editTime = DateTime.UtcNow;

        // Act
        resource.EditedOn = editTime;

        // Assert
        resource.EditedOn.ShouldBe(editTime);
    }

    [TestMethod]
    public void AtomEntryResource_IsDraft_CanBeSet()
    {
        // Arrange
        var resource = new AtomEntryResource();

        // Act
        resource.IsDraft = true;

        // Assert
        resource.IsDraft.ShouldBeTrue();
    }

    [TestMethod]
    public void AtomEntryResource_Format_ReturnsAtom()
    {
        // Arrange
        var resource = new AtomEntryResource();

        // Act
        var format = resource.Format;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Atom);
    }

    #endregion

    #region Roundtrip Tests

    [TestMethod]
    public void AtomServiceDocument_RoundTrip_PreservesData()
    {
        // Arrange
        var workspace = new AtomWorkspace(new AtomTextConstruct("Main Workspace"));
        workspace.Collections.Add(new AtomMemberResources(
            new Uri("http://example.com/posts"),
            new AtomTextConstruct("Blog Posts")));
        var doc = new AtomServiceDocument([workspace]);

        // Act - Serialize
        using var ms = new MemoryStream();
        doc.Save(ms);
        ms.Position = 0;

        // Act - Deserialize
        var loadedDoc = new AtomServiceDocument();
        loadedDoc.Load(ms);

        // Assert
        loadedDoc.Workspaces.Count.ShouldBe(1);
        loadedDoc.Workspaces[0].Title.Content.ShouldBe("Main Workspace");
        loadedDoc.Workspaces[0].Collections.Count.ShouldBe(1);
        loadedDoc.Workspaces[0].Collections[0].Title.Content.ShouldBe("Blog Posts");
    }

    [TestMethod]
    public void AtomCategoryDocument_RoundTrip_PreservesData()
    {
        // Arrange
        var doc = new AtomCategoryDocument
        {
            IsFixed = true,
            Scheme = new Uri("http://example.com/categories")
        };
        doc.Categories.Add(new AtomCategory("tech"));
        doc.Categories.Add(new AtomCategory("news"));

        // Act - Serialize
        using var ms = new MemoryStream();
        doc.Save(ms);
        ms.Position = 0;

        // Act - Deserialize
        var loadedDoc = new AtomCategoryDocument();
        loadedDoc.Load(ms);

        // Assert
        loadedDoc.IsFixed.ShouldBeTrue();
        loadedDoc.Scheme.ShouldBe(new Uri("http://example.com/categories"));
        loadedDoc.Categories.Count.ShouldBe(2);
    }

    #endregion

    #region Helper Methods

    private static XPathNavigator CreateNavigator(string xml)
    {
        using var reader = new StringReader(xml);
        var doc = new XPathDocument(reader);
        return doc.CreateNavigator();
    }

    #endregion
}