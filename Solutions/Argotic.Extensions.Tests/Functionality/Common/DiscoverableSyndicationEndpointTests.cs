using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class DiscoverableSyndicationEndpointTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        DiscoverableSyndicationEndpoint endpoint = new();

        // Assert
        endpoint.ShouldNotBeNull();
        endpoint.Title.ShouldBe(string.Empty);
        endpoint.ContentType.ShouldBe(string.Empty);
        endpoint.Source.ShouldBeNull();
    }

    [TestMethod]
    public void Constructor_WithSourceAndContentType_SetsProperties()
    {
        // Arrange
        Uri source = new("http://example.com/feed.rss");
        string contentType = "application/rss+xml";

        // Act
        DiscoverableSyndicationEndpoint endpoint = new(source, contentType);

        // Assert
        endpoint.Source.ShouldBe(source);
        endpoint.ContentType.ShouldBe(contentType);
        endpoint.Title.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Constructor_WithSourceContentTypeAndTitle_SetsProperties()
    {
        // Arrange
        Uri source = new("http://example.com/feed.rss");
        string contentType = "application/rss+xml";
        string title = "Test RSS Feed";

        // Act
        DiscoverableSyndicationEndpoint endpoint = new(source, contentType, title);

        // Assert
        endpoint.Source.ShouldBe(source);
        endpoint.ContentType.ShouldBe(contentType);
        endpoint.Title.ShouldBe(title);
    }

    [TestMethod]
    public void Constructor_WithNullSource_ThrowsArgumentNullException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new DiscoverableSyndicationEndpoint(null!, "application/rss+xml"));

    [TestMethod]
    public void Constructor_WithNullContentType_ThrowsArgumentException()
    {
        // Arrange
        Uri source = new("http://example.com/feed.rss");

        // Act & Assert
        Should.Throw<ArgumentException>(() => new DiscoverableSyndicationEndpoint(source, null!));
    }

    [TestMethod]
    public void Constructor_WithEmptyContentType_ThrowsArgumentException()
    {
        // Arrange
        Uri source = new("http://example.com/feed.rss");

        // Act & Assert
        Should.Throw<ArgumentException>(() => new DiscoverableSyndicationEndpoint(source, string.Empty));
    }

    [TestMethod]
    public void ContentType_Set_SetsValue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act
        endpoint.ContentType = "application/atom+xml";

        // Assert
        endpoint.ContentType.ShouldBe("application/atom+xml");
    }

    [TestMethod]
    public void ContentType_SetWithWhitespace_TrimsValue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act
        endpoint.ContentType = "  application/atom+xml  ";

        // Assert
        endpoint.ContentType.ShouldBe("application/atom+xml");
    }

    [TestMethod]
    public void ContentType_SetNull_ThrowsArgumentException()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => endpoint.ContentType = null!);
    }

    [TestMethod]
    public void ContentType_SetEmpty_ThrowsArgumentException()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => endpoint.ContentType = string.Empty);
    }

    [TestMethod]
    public void Source_Set_SetsValue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();
        Uri source = new("http://example.com/feed.rss");

        // Act
        endpoint.Source = source;

        // Assert
        endpoint.Source.ShouldBe(source);
    }

    [TestMethod]
    public void Source_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => endpoint.Source = null!);
    }

    [TestMethod]
    public void Title_Set_SetsValue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act
        endpoint.Title = "Test Feed";

        // Assert
        endpoint.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void Title_SetWithWhitespace_TrimsValue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act
        endpoint.Title = "  Test Feed  ";

        // Assert
        endpoint.Title.ShouldBe("Test Feed");
    }

    [TestMethod]
    public void Title_SetNull_SetsEmpty()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();
        endpoint.Title = "Initial Title";

        // Act
        endpoint.Title = null!;

        // Assert
        endpoint.Title.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Title_SetEmpty_SetsEmpty()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();
        endpoint.Title = "Initial Title";

        // Act
        endpoint.Title = string.Empty;

        // Assert
        endpoint.Title.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void ContentFormat_RssContentType_ReturnsRss()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Rss);
    }

    [TestMethod]
    public void ContentFormat_AtomContentType_ReturnsAtom()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.atom"),
            "application/atom+xml");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Atom);
    }

    [TestMethod]
    public void ContentFormat_UnknownContentType_ReturnsNone()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/unknown"),
            "application/unknown");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.None);
    }

    [TestMethod]
    public void ContentFormat_EmptyContentType_ReturnsNone()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.None);
    }

    [TestMethod]
    public void ContentFormat_OpmlContentType_ReturnsOpml()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/subscriptions.opml"),
            "text/x-opml");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Opml);
    }

    [TestMethod]
    public void ToString_ReturnsXhtmlRepresentation()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml",
            "Test Feed");

        // Act
        string result = endpoint.ToString();

        // Assert
        result.ShouldContain("<link");
        result.ShouldContain("rel=\"alternate\"");
        result.ShouldContain("type=\"application/rss+xml\"");
        result.ShouldContain("title=\"Test Feed\"");
        result.ShouldContain("href=\"http://example.com/feed.rss\"");
    }

    [TestMethod]
    public void CompareTo_EqualEndpoints_ReturnsZero()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint1 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml",
            "Test Feed");

        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml",
            "Test Feed");

        // Act
        int result = endpoint1.CompareTo(endpoint2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act
        int result = endpoint.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_DifferentEndpoint_ReturnsNonZero()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint1 = new(
            new Uri("http://example.com/feed1.rss"),
            "application/rss+xml");
        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed2.rss"),
            "application/atom+xml");

        // Act
        int result = endpoint1.CompareTo(endpoint2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void Equals_SameEndpoint_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint1 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml",
            "Test Feed");

        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml",
            "Test Feed");

        // Act & Assert
        endpoint1.Equals(endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_DifferentEndpoint_ReturnsFalse()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint1 = new(
            new Uri("http://example.com/feed1.rss"),
            "application/rss+xml");

        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed2.rss"),
            "application/rss+xml");

        // Act & Assert
        endpoint1.Equals(endpoint2).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_NonDiscoverableSyndicationEndpoint_ReturnsFalse()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act & Assert
        endpoint.Equals("not an endpoint").ShouldBeFalse();
    }

    [TestMethod]
    public void GetHashCode_EqualEndpoints_ReturnSameValue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint first = new(new Uri("http://example.com/feed.rss"), "application/rss+xml");
        DiscoverableSyndicationEndpoint second = new(new Uri("http://example.com/feed.rss"), "application/rss+xml");

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    [TestMethod]
    public void OperatorEquals_EqualEndpoints_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint1 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act & Assert
        (endpoint1 == endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint? endpoint1 = null;
        DiscoverableSyndicationEndpoint? endpoint2 = null;

        // Act & Assert
        (endpoint1 == endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentEndpoints_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint1 = new(
            new Uri("http://example.com/feed1.rss"),
            "application/rss+xml");

        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed2.rss"),
            "application/rss+xml");

        // Act & Assert
        (endpoint1 != endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_NullFirst_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint? endpoint1 = null;
        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act & Assert
        (endpoint1 < endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_NullFirst_ReturnsFalse()
    {
        // Arrange
        DiscoverableSyndicationEndpoint? endpoint1 = null;
        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act & Assert
        (endpoint1 > endpoint2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_NullFirst_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint? endpoint1 = null;
        DiscoverableSyndicationEndpoint endpoint2 = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act & Assert
        (endpoint1 <= endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullBoth_ReturnsTrue()
    {
        // Arrange
        DiscoverableSyndicationEndpoint? endpoint1 = null;
        DiscoverableSyndicationEndpoint? endpoint2 = null;

        // Act & Assert
        (endpoint1 >= endpoint2).ShouldBeTrue();
    }

    [TestMethod]
    public async Task CreateNavigatorAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => endpoint.CreateNavigatorAsync());
    }

    [TestMethod]
    public async Task CreateNavigatorAsync_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.rss"),
            "application/rss+xml");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => endpoint.CreateNavigatorAsync(null!));
    }

    /// <summary>
    /// <c>application/xml</c> resolves to <see cref="SyndicationContentFormat.Sitemap"/>, never to
    /// <see cref="SyndicationContentFormat.SitemapIndex"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Two enumeration fields carry the same <c>[MimeMediaType]</c>, so one of them cannot be
    ///     reached through this property at all. That is not a defect in the lookup — a sitemap and a
    ///     sitemap index really are both served as <c>application/xml</c>, and the content type carries
    ///     nothing that could tell them apart. Distinguishing them needs the document's root element,
    ///     which is what <c>SyndicationDiscoveryUtility</c> sniffs.
    ///     </para>
    ///     <para>
    ///     What <b>is</b> worth pinning is <i>which</i> of the two wins. The mapping is built by
    ///     iterating <see cref="System.Type.GetFields()"/> and calling <c>TryAdd</c>, and the BCL does
    ///     not guarantee field order — so before the tie-break was made explicit, the answer here was a
    ///     property of the running runtime rather than of this library. This test fails if that ever
    ///     changes, which is the only reason the silent drop is safe to leave in place.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ContentFormat_ApplicationXml_ResolvesToSitemapAndNeverToSitemapIndex()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/sitemap.xml"),
            "application/xml");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Sitemap);
        format.ShouldNotBe(SyndicationContentFormat.SitemapIndex);
    }

    /// <summary>
    /// <c>application/atom+xml</c> resolves to <see cref="SyndicationContentFormat.Atom"/>, never to
    /// <see cref="SyndicationContentFormat.AtomEntryDocument"/>.
    /// </summary>
    /// <remarks>
    ///     The second of the two colliding content types, and the same reasoning. RFC 4287 gives an Atom
    ///     feed document and a stand-alone entry document the same media type, so the collision is in the
    ///     specification rather than in this mapping. The feed document wins because it is the
    ///     overwhelmingly more common one, and because that is what the property already returned.
    /// </remarks>
    [TestMethod]
    public void ContentFormat_ApplicationAtomXml_ResolvesToAtomAndNeverToAtomEntryDocument()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.atom"),
            "application/atom+xml");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Atom);
        format.ShouldNotBe(SyndicationContentFormat.AtomEntryDocument);
    }

    /// <summary>
    /// Every non-colliding content type still resolves to its own format.
    /// </summary>
    /// <remarks>
    ///     The guard on the tie-break. Making one of two colliding entries win deliberately is only
    ///     correct if it leaves the other thirteen mappings alone, and a change to how the table is built
    ///     could plausibly drop or reorder them all rather than just the two in question.
    /// </remarks>
    [TestMethod]
    [DataRow("text/x-apml", SyndicationContentFormat.Apml)]
    [DataRow("application/blog+xml", SyndicationContentFormat.BlogML)]
    [DataRow("application/x.microsummary+xml", SyndicationContentFormat.MicroSummaryGenerator)]
    [DataRow("text/vnd.IPTC.NewsML", SyndicationContentFormat.NewsML)]
    [DataRow("application/opensearchdescription+xml", SyndicationContentFormat.OpenSearchDescription)]
    [DataRow("text/x-opml", SyndicationContentFormat.Opml)]
    [DataRow("application/rsd+xml", SyndicationContentFormat.Rsd)]
    [DataRow("application/rss+xml", SyndicationContentFormat.Rss)]
    [DataRow("application/rdf+xml", SyndicationContentFormat.Rdf)]
    [DataRow("application/atomcat+xml", SyndicationContentFormat.AtomCategoryDocument)]
    [DataRow("application/atomsvc+xml", SyndicationContentFormat.AtomServiceDocument)]
    public void ContentFormat_UncollidedContentType_ResolvesToItsOwnFormat(string contentType, SyndicationContentFormat expected)
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(new Uri("http://example.com/resource"), contentType);

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(expected);
    }

    /// <summary>
    /// The content type is matched without regard to case.
    /// </summary>
    /// <remarks>
    ///     The table is built with <see cref="StringComparer.OrdinalIgnoreCase"/> and frozen with the same
    ///     comparer. Both have to be right: freezing with the default comparer while building with an
    ///     ordinal-ignore-case one is a silent way to lose case insensitivity, because the dictionary
    ///     still populates correctly and only lookups change.
    /// </remarks>
    [TestMethod]
    public void ContentFormat_ContentTypeInDifferentCase_StillResolves()
    {
        // Arrange
        DiscoverableSyndicationEndpoint endpoint = new(
            new Uri("http://example.com/feed.rss"),
            "APPLICATION/RSS+XML");

        // Act
        SyndicationContentFormat format = endpoint.ContentFormat;

        // Assert
        format.ShouldBe(SyndicationContentFormat.Rss);
    }
}