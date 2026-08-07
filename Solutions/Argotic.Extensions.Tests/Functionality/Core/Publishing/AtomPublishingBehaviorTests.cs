using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Publishing;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

/// <summary>
/// Covers the Atom Publishing Protocol object model — <see cref="AtomAcceptedMediaRange"/>,
/// <see cref="AtomWorkspace"/>, <see cref="AtomMemberResources"/>, <see cref="AtomServiceDocument"/>,
/// <see cref="AtomCategoryDocument"/> and <see cref="AtomEntryResource"/> — both as constructed in code
/// and as parsed from XML.
/// </summary>
[TestClass]
public class AtomPublishingBehaviorTests
{
    #region AtomAcceptedMediaRange Tests

    /// <summary>
    /// A newly constructed media range has an empty range and neither a base URI nor a language.
    /// </summary>
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

    /// <summary>
    /// The range passed to the constructor is the range the instance reports.
    /// </summary>
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

    /// <summary>
    /// Assigning a range trims the surrounding whitespace, so an indented <c>accept</c> element does not yield an unusable media type.
    /// </summary>
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

    /// <summary>
    /// The entry media range is spelled <c>application/atom+xml;type=entry</c> — no space, and the parameter lower-case.
    /// </summary>
    [TestMethod]
    public void AtomAcceptedMediaRange_AtomEntryMediaRange_ReturnsCorrectValue()
    {
        // Act
        string range = AtomAcceptedMediaRange.AtomEntryMediaRange;

        // Assert
        range.ShouldBe("application/atom+xml;type=entry");
    }

    /// <summary>
    /// The feed media range is spelled <c>application/atom+xml;type=feed</c> — no space, and the parameter lower-case.
    /// </summary>
    [TestMethod]
    public void AtomAcceptedMediaRange_AtomFeedMediaRange_ReturnsCorrectValue()
    {
        // Act
        string range = AtomAcceptedMediaRange.AtomFeedMediaRange;

        // Assert
        range.ShouldBe("application/atom+xml;type=feed");
    }

    /// <summary>
    /// The text content of an <c>app:accept</c> element becomes the media range.
    /// </summary>
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

    /// <summary>
    /// <c>ToString</c> renders the media range as an <c>accept</c> element wrapping the range as text.
    /// </summary>
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

    /// <summary>
    /// Comparing a media range against <see langword="null"/> returns <c>1</c>, so <see langword="null"/> sorts first.
    /// </summary>
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

    /// <summary>
    /// Two media ranges carrying the same range compare equal.
    /// </summary>
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

    /// <summary>
    /// Media ranges compare by value, not by reference, under <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void AtomAcceptedMediaRange_Equals_WithSameRange_ReturnsTrue()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/png");

        // Act & Assert
        range1.Equals(range2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>==</c> agrees with <c>Equals</c> for two media ranges carrying the same range.
    /// </summary>
    [TestMethod]
    public void AtomAcceptedMediaRange_EqualityOperator_Works()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/png");

        // Act & Assert
        (range1 == range2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>!=</c> separates media ranges carrying different ranges.
    /// </summary>
    [TestMethod]
    public void AtomAcceptedMediaRange_InequalityOperator_Works()
    {
        // Arrange
        var range1 = new AtomAcceptedMediaRange("image/png");
        var range2 = new AtomAcceptedMediaRange("image/jpeg");

        // Act & Assert
        (range1 != range2).ShouldBeTrue();
    }

    /// <summary>
    /// A media range with no syndication extensions reports <c>HasExtensions</c> as <see langword="false"/>.
    /// </summary>
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

    /// <summary>
    /// A newly constructed workspace has a non-null title and an empty, non-null collection list.
    /// </summary>
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

    /// <summary>
    /// The <see cref="AtomTextConstruct"/> passed to the constructor becomes the workspace title.
    /// </summary>
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

    /// <summary>
    /// The two-argument constructor takes the collections as well as the title, and the collections keep their URIs.
    /// </summary>
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

    /// <summary>
    /// The workspace indexer reads through to the collection at that position.
    /// </summary>
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

    /// <summary>
    /// The workspace indexer writes through, replacing the collection at that position.
    /// </summary>
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

    /// <summary>
    /// A workspace element reads its title from the Atom-namespaced <c>atom:title</c> child, not an app-namespaced one.
    /// </summary>
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

    /// <summary>
    /// Comparing a workspace against <see langword="null"/> returns <c>1</c>, so <see langword="null"/> sorts first.
    /// </summary>
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

    /// <summary>
    /// Workspaces compare by value, so two built from the same title are equal.
    /// </summary>
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

    /// <summary>
    /// A newly constructed service document has an empty, non-null workspace list.
    /// </summary>
    [TestMethod]
    public void AtomServiceDocument_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var doc = new AtomServiceDocument();

        // Assert
        doc.Workspaces.ShouldNotBeNull();
        doc.Workspaces.Count.ShouldBe(0);
    }

    /// <summary>
    /// The workspaces passed to the constructor are the workspaces the document reports.
    /// </summary>
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

    /// <summary>
    /// An <i>empty</i> workspace list throws <see cref="ArgumentOutOfRangeException"/>: a service document must carry at least one workspace.
    /// </summary>
    [TestMethod]
    public void AtomServiceDocument_Constructor_WithEmptyWorkspaces_ThrowsException()
    {
        // Arrange
        List<AtomWorkspace> workspaces = [];

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => new AtomServiceDocument(workspaces));
    }

    /// <summary>
    /// The service document indexer reads through to the workspace at that position.
    /// </summary>
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

    /// <summary>
    /// A service document reports its format as <see cref="SyndicationContentFormat.AtomServiceDocument"/>.
    /// </summary>
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

    /// <summary>
    /// A service document reports version <c>1.0</c>.
    /// </summary>
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

    /// <summary>
    /// Loading a service document reaches its workspace, that workspace's collection, and the collection's own title.
    /// </summary>
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

    /// <summary>
    /// A newly constructed collection has no URI, but a non-null title and non-null accept and category lists.
    /// </summary>
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

    /// <summary>
    /// The URI and title passed to the constructor are the ones the collection reports.
    /// </summary>
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

    /// <summary>
    /// A collection's accept list takes media ranges, and the well-known entry range arrives as <c>application/atom+xml;type=entry</c>.
    /// </summary>
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

    /// <summary>
    /// Encoding a title for a <c>Slug</c> header percent-escapes the reserved characters and the non-ASCII
    /// ones as their UTF-8 bytes, and leaves the spaces as spaces rather than as the <c>+</c> that form
    /// encoding would produce.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The previous assertion was <c>ShouldNotBeNullOrEmpty</c> over the input
    ///         <c>"My First Blog Post!"</c>. Two things were wrong with that. The assertion is satisfied
    ///         by an identity implementation; and the input is itself a <i>fixed point</i> of the method —
    ///         <c>HttpUtility.UrlEncode</c> leaves <c>!</c> unescaped, and the only other transformation
    ///         is spaces to <c>+</c> and back again — so even an exact assertion on that input could not
    ///         have distinguished <c>SlugEncode</c> from <c>return characterSequence;</c>.
    ///     </para>
    ///     <para>
    ///         The escapes are lowercase, which is what <c>HttpUtility.UrlEncode</c> emits and what
    ///         distinguishes it from <c>Uri.EscapeDataString</c>. That is pinned deliberately: the two
    ///         differ in case and in which characters they treat as reserved, so a swap would be a
    ///         behaviour change for every server that compares slugs byte for byte.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AtomMemberResources_SlugEncode_ReturnsEncodedString()
    {
        // Arrange
        string input = "Café & Crème?";

        // Act
        string encoded = AtomMemberResources.SlugEncode(input);

        // Assert
        encoded.ShouldBe("Caf%c3%a9 %26 Cr%c3%a8me%3f");
        encoded.ShouldNotBe(input, "an identity implementation must not satisfy this test");
        encoded.ShouldNotContain("+", Case.Sensitive);
        AtomMemberResources.SlugDecode(encoded).ShouldBe(input);
    }

    /// <summary>
    /// Decoding a <c>Slug</c> header turns its percent-escapes back into the characters they stand for, so <c>%20</c> becomes a space.
    /// </summary>
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

    /// <summary>
    /// A collection element loads its <c>href</c> as the URI, its <c>atom:title</c>, and its <c>accept</c> children.
    /// </summary>
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

    /// <summary>
    /// Comparing a collection against <see langword="null"/> returns <c>1</c>, so <see langword="null"/> sorts first.
    /// </summary>
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

    /// <summary>
    /// A newly constructed category document has no categories, is not fixed, and has no scheme.
    /// </summary>
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

    /// <summary>
    /// A category added to the document keeps its term.
    /// </summary>
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

    /// <summary>
    /// The fixed flag — whether the listed categories are the only permitted ones — is settable and readable.
    /// </summary>
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

    /// <summary>
    /// The default scheme, which categories without one inherit, round-trips through the property.
    /// </summary>
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

    /// <summary>
    /// A category document loads <c>fixed="yes"</c> as <see langword="true"/>, its <c>scheme</c> as a URI, and both <c>atom:category</c> children.
    /// </summary>
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

    /// <summary>
    /// A category document reports its format as <see cref="SyndicationContentFormat.AtomCategoryDocument"/>.
    /// </summary>
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

    /// <summary>
    /// Comparing a category document against <see langword="null"/> returns <c>1</c>, so <see langword="null"/> sorts first.
    /// </summary>
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

    /// <summary>
    /// A newly constructed entry resource has no edit timestamp and is not a draft.
    /// </summary>
    [TestMethod]
    public void AtomEntryResource_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var resource = new AtomEntryResource();

        // Assert
        resource.EditedOn.ShouldBe(DateTime.MinValue);
        resource.IsDraft.ShouldBeFalse();
    }

    /// <summary>
    /// The identifier, title and update timestamp passed to the constructor reach the inherited Atom entry members.
    /// </summary>
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

    /// <summary>
    /// The <c>app:edited</c> timestamp round-trips through the property.
    /// </summary>
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

    /// <summary>
    /// The <c>app:control/app:draft</c> flag round-trips through the property.
    /// </summary>
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

    /// <summary>
    /// An entry resource reports its format as <see cref="SyndicationContentFormat.AtomEntryDocument"/>, not as a feed.
    /// </summary>
    /// <remarks>
    ///     This was <c>Atom</c>. An <see cref="AtomEntryResource"/> is an Atom Publishing member resource
    ///     — a stand-alone entry document — and reporting the same format as a feed is what let the two be
    ///     confused.
    /// </remarks>
    [TestMethod]
    public void AtomEntryResource_Format_ReturnsAtomEntryDocument()
    {
        // Arrange
        var resource = new AtomEntryResource();

        // Act
        var format = resource.Format;

        // Assert
        // Was Atom. An AtomEntryResource is an Atom Publishing member resource -- a stand-alone entry
        // document -- and reporting the same format as a feed is what let the two be confused.
        format.ShouldBe(SyndicationContentFormat.AtomEntryDocument);
    }

    #endregion

    #region Roundtrip Tests

    /// <summary>
    /// A service document written to a stream and read back keeps its workspace title and its collection's title.
    /// </summary>
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

    /// <summary>
    /// A category document written to a stream and read back keeps its fixed flag, its scheme and both categories.
    /// </summary>
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