using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication.Specialized;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

/// <summary>
/// Unit tests for <see cref="BlogMLTrackback"/> that verify trackback parsing, serialization, and comparison.
/// </summary>
[TestClass]
public class BlogMLTrackbackTests
{
    #region Constructor Tests

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

    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Load(null!));
    }

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

    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.WriteTo(null!));
    }

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

    [TestMethod]
    public void CompareTo_DifferentUrls_ReturnsNonZero()
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
        int result = trackback1.CompareTo(trackback2);

        // Assert
        result.ShouldNotBe(0);
    }

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

    [TestMethod]
    public void CompareTo_DifferentTrackback_ReturnsNonZero()
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
        int result = trackback1.CompareTo(trackback2);

        // Assert
        result.ShouldNotBe(0);
    }

    #endregion

    #region Operators Comparison Tests

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

    [TestMethod]
    public void Url_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Url = null!);
    }

    [TestMethod]
    public void Title_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLTrackback trackback = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => trackback.Title = null!);
    }

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

    [TestMethod]
    public void GetHashCode_ReturnsValue()
    {
        // Arrange
        BlogMLTrackback trackback = new()
        {
            Id = "tb1",
            Url = new Uri("http://example.com/post")
        };

        // Act
        int hashCode = trackback.GetHashCode();

        // Assert
        hashCode.ShouldNotBe(0);
    }

    #endregion

    #region FindExtension Tests

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