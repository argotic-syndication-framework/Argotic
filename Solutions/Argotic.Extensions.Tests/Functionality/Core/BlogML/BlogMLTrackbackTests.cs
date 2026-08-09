namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

/// <summary>
/// Unit tests for <see cref="BlogMLTrackback"/> that verify trackback parsing, serialization, and comparison.
/// </summary>
[TestClass]
public class BlogMLTrackbackTests
{
    #region Constructor Tests

    /// <summary>
    /// A newly constructed trackback has an empty identifier, a non-null title, no approval status, and both timestamps at <c>DateTime.MinValue</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesValidInstance()
    {
        // Arrange & Act
        BlogMLTrackback trackback = new();

        // Assert
        trackback.ShouldNotBeNull();
        trackback.Id.ShouldBe(string.Empty);
        trackback.Title.ShouldNotBeNull();
        trackback.ApprovalStatus.ShouldBe(BlogMLApprovalStatus.None);
        trackback.CreatedOn.ShouldBe(DateTime.MinValue);
        trackback.LastModifiedOn.ShouldBe(DateTime.MinValue);
        trackback.Extensions.ShouldNotBeNull();
        trackback.HasExtensions.ShouldBeFalse();
    }

    #endregion

    #region Load Tests

    /// <summary>
    /// A fully attributed trackback element loads its identifier, URL, both timestamps, the title element, and <c>approved="true"</c> as <see cref="BlogMLApprovalStatus.Approved"/>.
    /// </summary>
    [TestMethod]
    public void Load_ValidXml_PopulatesProperties()
    {
        // Arrange
        const string trackbackXml = """
            <trackback xmlns="http://www.blogml.com/2006/09/BlogML"
                       id="tb1"
                       date-created="2025-01-16T10:00:00Z"
                       date-modified="2025-01-16T12:00:00Z"
                       approved="true"
                       url="http://other.example.com/post">
                <title type="text">Trackback Title</title>
            </trackback>
            """;

        BlogMLTrackback trackback = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(trackbackXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act
        bool wasLoaded = trackback.Load(navigator);

        // Assert
        wasLoaded.ShouldBeTrue();
        trackback.Id.ShouldBe("tb1");
        trackback.Url.ShouldBe(new Uri("http://other.example.com/post"));
        trackback.ApprovalStatus.ShouldBe(BlogMLApprovalStatus.Approved);
        trackback.CreatedOn.Year.ShouldBe(2025);
        trackback.LastModifiedOn.Year.ShouldBe(2025);
        trackback.Title.Content.ShouldBe("Trackback Title");
    }

    /// <summary>
    /// A trackback element carrying nothing but <c>url</c> still loads, and the URL survives.
    /// </summary>
    [TestMethod]
    public void Load_MinimalXml_PopulatesUrl()
    {
        // Arrange
        const string trackbackXml = """
            <trackback xmlns="http://www.blogml.com/2006/09/BlogML" url="http://example.com/trackback"/>
            """;

        BlogMLTrackback trackback = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(trackbackXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act
        bool wasLoaded = trackback.Load(navigator);

        // Assert
        wasLoaded.ShouldBeTrue();
        trackback.Url.ShouldBe(new Uri("http://example.com/trackback"));
    }

    /// <summary>
    /// The load overload taking <see cref="SyndicationResourceLoadSettings"/> reads the same attributes, and maps <c>approved="false"</c> to <see cref="BlogMLApprovalStatus.NotApproved"/>.
    /// </summary>
    [TestMethod]
    public void Load_WithSettings_LoadsTrackback()
    {
        // Arrange
        const string trackbackXml = """
            <trackback xmlns="http://www.blogml.com/2006/09/BlogML"
                       id="tb2"
                       date-created="2025-01-17T10:00:00Z"
                       approved="false"
                       url="http://another.example.com/post">
                <title type="text">Another Trackback</title>
            </trackback>
            """;

        BlogMLTrackback trackback = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(trackbackXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();
        SyndicationResourceLoadSettings settings = new();

        // Act
        bool wasLoaded = trackback.Load(navigator, settings);

        // Assert
        wasLoaded.ShouldBeTrue();
        trackback.Id.ShouldBe("tb2");
        trackback.ApprovalStatus.ShouldBe(BlogMLApprovalStatus.NotApproved);
        trackback.Url.ShouldBe(new Uri("http://another.example.com/post"));
    }

    /// <summary>
    /// Loading from a <see langword="null"/> navigator throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Load(null!));
    }

    /// <summary>
    /// <see langword="null"/> load settings throw <see cref="ArgumentNullException"/> even when the navigator is valid.
    /// </summary>
    [TestMethod]
    public void Load_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        const string trackbackXml = """
            <trackback xmlns="http://www.blogml.com/2006/09/BlogML" url="http://example.com/trackback"/>
            """;

        BlogMLTrackback trackback = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(trackbackXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToFirstChild();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Load(navigator, null!));
    }

    #endregion

    #region WriteTo Tests

    /// <summary>
    /// Writing a trackback emits <c>id</c>, <c>url</c>, <c>date-created</c>, <c>approved="true"</c> and a nested title element.
    /// </summary>
    [TestMethod]
    public void WriteTo_ValidTrackback_WritesCorrectXml()
    {
        // Arrange
        BlogMLTrackback trackback = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/trackback"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2025, 1, 16, 10, 0, 0, DateTimeKind.Utc),
            Title = new BlogMLTextConstruct("Trackback Title")
        };

        // Act
        using MemoryStream stream = new();
        XmlWriterSettings writerSettings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true
        };

        using (XmlWriter writer = XmlWriter.Create(stream, writerSettings))
        {
            trackback.WriteTo(writer);
        }

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert
        xml.ShouldContain("trackback");
        xml.ShouldContain("id=\"tb1\"");
        xml.ShouldContain("url=\"http://example.com/trackback\"");
        xml.ShouldContain("approved=\"true\"");
        xml.ShouldContain("date-created=");
        xml.ShouldContain("<title");
    }

    /// <summary>
    /// Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.WriteTo(null!));
    }

    /// <summary>
    /// <c>ToString</c> renders the trackback as its XML element, carrying the <c>url</c> attribute.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        BlogMLTrackback trackback = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/trackback")
        };

        // Act
        string result = trackback.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("trackback");
        result.ShouldContain("url=\"http://example.com/trackback\"");
    }

    #endregion

    #region Comparison Tests

    /// <summary>
    /// Two trackbacks agreeing on identifier, URL, approval status and creation time compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_SameUrl_ReturnsZero()
    {
        // Arrange
        BlogMLTrackback trackback1 = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2025, 1, 16, 10, 0, 0, DateTimeKind.Utc)
        };

        BlogMLTrackback trackback2 = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2025, 1, 16, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        int result = trackback1.CompareTo(trackback2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Trackbacks differing only in URL are ordered by URL, antisymmetrically: <c>post1</c> sorts before
    /// <c>post2</c>, and reversing the operands reverses the sign.
    /// </summary>
    [TestMethod]
    public void CompareTo_TrackbacksDifferingOnlyInUrl_OrdersByUrl()
    {
        // Arrange
        BlogMLTrackback trackback1 = new()
        {
            Url = new Uri("http://example.com/post1")
        };

        BlogMLTrackback trackback2 = new()
        {
            Url = new Uri("http://example.com/post2")
        };

        // Act
        int forward = trackback1.CompareTo(trackback2);
        int reverse = trackback2.CompareTo(trackback1);

        // Assert
        // Url is the first comparand, compared as an absolute URI under OrdinalIgnoreCase.
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Comparing a trackback against <see langword="null"/> returns <c>1</c>, so <see langword="null"/> sorts first.
    /// </summary>
    [TestMethod]
    public void CompareTo_NullObject_ReturnsPositive()
    {
        // Arrange
        BlogMLTrackback trackback = new()
        {
            Url = new Uri("http://example.com/post")
        };

        // Act
        int result = trackback.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// When both the identifier and the URL differ, the URL decides, antisymmetrically: <c>post1</c> sorts
    /// before <c>post2</c> whichever way round the pair is compared.
    /// </summary>
    [TestMethod]
    public void CompareTo_TrackbacksDifferingInIdentifierAndUrl_OrdersByUrlFirst()
    {
        // Arrange
        BlogMLTrackback trackback1 = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post1")
        };
        BlogMLTrackback trackback2 = new()
        {
            Id = "tb2",
            Url = new Uri("http://example.com/post2")
        };

        // Act
        int forward = trackback1.CompareTo(trackback2);
        int reverse = trackback2.CompareTo(trackback1);

        // Assert
        // Url is compared before the common BlogML members, so the identifier is never reached here.
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Operators Comparison Tests

    /// <summary>
    /// <c>==</c>, <c>!=</c> and <c>Equals</c> agree on value equality, and <c>Equals</c> rejects both <see langword="null"/> and an unrelated type.
    /// </summary>
    [TestMethod]
    public void Operators_Comparison_WorkCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback1 = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post1"),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };

        BlogMLTrackback trackback2 = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post1"),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };

        BlogMLTrackback trackback3 = new()
        {
            Id = "tb2",
            Url = new Uri("http://example.com/post2"),
            ApprovalStatus = BlogMLApprovalStatus.NotApproved
        };

        // Act & Assert - Equality
        (trackback1 == trackback2).ShouldBeTrue();
        (trackback1 != trackback3).ShouldBeTrue();

        // Act & Assert - Equals method
        trackback1.Equals(trackback2).ShouldBeTrue();
        trackback1.Equals(trackback3).ShouldBeFalse();
        trackback1.Equals(null).ShouldBeFalse();
        trackback1.Equals("not a trackback").ShouldBeFalse();
    }

    /// <summary>
    /// <c>==</c> against a <see langword="null"/> left operand is <see langword="false"/>, and the <c>is null</c> pattern agrees with each reference's state.
    /// </summary>
    [TestMethod]
    public void Operator_Equality_WithNulls_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new() { Url = new Uri("http://example.com/post") };
        BlogMLTrackback? nullTrackback = null;

        // Act & Assert
        (nullTrackback is null).ShouldBeTrue();
        (trackback is null).ShouldBeFalse();
        (null == trackback).ShouldBeFalse();
    }

    /// <summary>
    /// <c>&lt;</c> orders trackbacks by URL, so <c>/a</c> precedes <c>/z</c>, and <see langword="null"/> precedes any instance.
    /// </summary>
    [TestMethod]
    public void Operator_LessThan_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback1 = new() { Url = new Uri("http://example.com/a") };
        BlogMLTrackback trackback2 = new() { Url = new Uri("http://example.com/z") };
        BlogMLTrackback? nullTrackback = null;

        // Act & Assert
        (trackback1 < trackback2).ShouldBeTrue();
        (nullTrackback < trackback1).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;</c> orders trackbacks by URL, so <c>/z</c> follows <c>/a</c>, and <see langword="null"/> is never greater than an instance.
    /// </summary>
    [TestMethod]
    public void Operator_GreaterThan_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback1 = new() { Url = new Uri("http://example.com/z") };
        BlogMLTrackback trackback2 = new() { Url = new Uri("http://example.com/a") };
        BlogMLTrackback? nullTrackback = null;

        // Act & Assert
        (trackback1 > trackback2).ShouldBeTrue();
        (nullTrackback > trackback1).ShouldBeFalse();
    }

    /// <summary>
    /// <c>&lt;=</c> holds for two equal trackbacks and for <see langword="null"/> against an instance.
    /// </summary>
    [TestMethod]
    public void Operator_LessThanOrEqual_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback1 = new() { Url = new Uri("http://example.com/post") };
        BlogMLTrackback trackback2 = new() { Url = new Uri("http://example.com/post") };
        BlogMLTrackback? nullTrackback = null;

        // Act & Assert
        (trackback1 <= trackback2).ShouldBeTrue();
        (nullTrackback <= trackback1).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> holds for two equal trackbacks.
    /// </summary>
    [TestMethod]
    public void Operator_GreaterThanOrEqual_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback1 = new() { Url = new Uri("http://example.com/post") };
        BlogMLTrackback trackback2 = new() { Url = new Uri("http://example.com/post") };

        // Act & Assert
        (trackback1 >= trackback2).ShouldBeTrue();
    }

    #endregion

    #region Property Tests

    /// <summary>
    /// Assigning an identifier trims the surrounding whitespace.
    /// </summary>
    [TestMethod]
    public void Id_SetAndGet_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act
        trackback.Id = "  test-id  ";

        // Assert
        trackback.Id.ShouldBe("test-id");
    }

    /// <summary>
    /// Assigning a <see langword="null"/> identifier yields an empty string rather than throwing.
    /// </summary>
    [TestMethod]
    public void Id_SetToNull_ReturnsEmptyString()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act
        trackback.Id = null!;

        // Assert
        trackback.Id.ShouldBe(string.Empty);
    }

    /// <summary>
    /// The URL is required: assigning <see langword="null"/> throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Url_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Url = null!);
    }

    /// <summary>
    /// The title is required: assigning <see langword="null"/> throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Title_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Title = null!);
    }

    /// <summary>
    /// An assigned <see cref="BlogMLTextConstruct"/> title keeps its content.
    /// </summary>
    [TestMethod]
    public void Title_SetValidValue_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new();
        BlogMLTextConstruct title = new("Test Title");

        // Act
        trackback.Title = title;

        // Assert
        trackback.Title.Content.ShouldBe("Test Title");
    }

    /// <summary>
    /// Two separately built trackbacks carrying identical data are equal, hash equally, and hash stably —
    /// the contract a <see cref="Dictionary{TKey, TValue}"/> relies on.
    /// </summary>
    [TestMethod]
    public void GetHashCode_EqualInstances_AgreeAndAreStable()
    {
        // Arrange
        // Every member the comparison folds is populated, so the assertion covers all six of them rather
        // than agreeing on a pair of defaults.
        BlogMLTrackback first = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2025, 1, 16, 10, 0, 0, DateTimeKind.Utc),
            LastModifiedOn = new DateTime(2025, 1, 17, 11, 30, 0, DateTimeKind.Utc),
            Title = new BlogMLTextConstruct("Trackback Title")
        };

        BlogMLTrackback second = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2025, 1, 16, 10, 0, 0, DateTimeKind.Utc),
            LastModifiedOn = new DateTime(2025, 1, 17, 11, 30, 0, DateTimeKind.Utc),
            Title = new BlogMLTextConstruct("Trackback Title")
        };

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    #endregion

    #region FindExtension Tests

    /// <summary>
    /// Searching a trackback that carries no extensions returns <see langword="null"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void FindExtension_WithPredicate_ReturnsCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act
        ISyndicationExtension? result = trackback.FindExtension(ext => ext.XmlNamespace == "http://nonexistent.example.com");

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// A <see langword="null"/> match predicate throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void FindExtension_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.FindExtension(null!));
    }

    #endregion

    #region Integration Tests with BlogML Document

    /// <summary>
    /// Loading a whole BlogML document reaches the trackbacks on its post, in document order, and keeps their approval status distinct.
    /// </summary>
    [TestMethod]
    public void Load_FromBlogMLDocument_ParsesTrackbacks()
    {
        // Arrange
        BlogMLDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.BlogMLWithTrackbacks));

        // Act
        document.Load(stream);

        // Assert
        document.Posts.Count.ShouldBe(1);
        document.Posts[0].Trackbacks.Count.ShouldBe(2);

        // First trackback
        BlogMLTrackback trackback1 = document.Posts[0].Trackbacks[0];
        trackback1.Id.ShouldBe("tb1");
        trackback1.Url.ShouldBe(new Uri("http://other.example.com/post"));
        trackback1.ApprovalStatus.ShouldBe(BlogMLApprovalStatus.Approved);
        trackback1.CreatedOn.Year.ShouldBe(2025);

        // Second trackback
        BlogMLTrackback trackback2 = document.Posts[0].Trackbacks[1];
        trackback2.Id.ShouldBe("tb2");
        trackback2.Url.ShouldBe(new Uri("http://another.example.com/post"));
        trackback2.ApprovalStatus.ShouldBe(BlogMLApprovalStatus.NotApproved);
        trackback2.Title.Content.ShouldBe("Trackback Title");
    }

    #endregion

    #region IBlogMLCommonObject Tests

    /// <summary>
    /// The <c>IBlogMLCommonObject</c> approval status round-trips through the property.
    /// </summary>
    [TestMethod]
    public void ApprovalStatus_SetAndGet_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act
        trackback.ApprovalStatus = BlogMLApprovalStatus.Approved;

        // Assert
        trackback.ApprovalStatus.ShouldBe(BlogMLApprovalStatus.Approved);
    }

    /// <summary>
    /// The creation timestamp round-trips through the property with its <c>DateTimeKind</c> intact.
    /// </summary>
    [TestMethod]
    public void CreatedOn_SetAndGet_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new();
        DateTime createdOn = new(2025, 1, 16, 10, 0, 0, DateTimeKind.Utc);

        // Act
        trackback.CreatedOn = createdOn;

        // Assert
        trackback.CreatedOn.ShouldBe(createdOn);
    }

    /// <summary>
    /// The last-modified timestamp round-trips through the property with its <c>DateTimeKind</c> intact.
    /// </summary>
    [TestMethod]
    public void LastModifiedOn_SetAndGet_WorksCorrectly()
    {
        // Arrange
        BlogMLTrackback trackback = new();
        DateTime modifiedOn = new(2025, 1, 17, 12, 0, 0, DateTimeKind.Utc);

        // Act
        trackback.LastModifiedOn = modifiedOn;

        // Assert
        trackback.LastModifiedOn.ShouldBe(modifiedOn);
    }

    #endregion
}