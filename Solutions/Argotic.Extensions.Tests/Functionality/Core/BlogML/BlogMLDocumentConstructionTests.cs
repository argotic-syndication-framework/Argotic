using Argotic.Syndication.Specialized;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

[TestClass]
public class BlogMLDocumentConstructionTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Construction_SetsBasicProperties_Correctly()
    {
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("/blogs/default.aspx", UriKind.Relative),
            GeneratedOn = new DateTime(2024, 1, 15, 12, 0, 0),
            Title = new BlogMLTextConstruct("Test BlogML Document"),
            Subtitle = new BlogMLTextConstruct("This is a test blog")
        };

        document.RootUrl.ShouldBe(new Uri("/blogs/default.aspx", UriKind.Relative));
        document.GeneratedOn.ShouldBe(new DateTime(2024, 1, 15, 12, 0, 0));
        document.Title.Content.ShouldBe("Test BlogML Document");
        document.Subtitle.Content.ShouldBe("This is a test blog");
    }

    [TestMethod]
    public void Construction_WithAuthor_AddsCorrectly()
    {
        BlogMLDocument document = new();
        BlogMLAuthor author = new()
        {
            Id = "1",
            CreatedOn = new DateTime(2024, 1, 1),
            LastModifiedOn = new DateTime(2024, 1, 15),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            EmailAddress = "test@example.com",
            Title = new BlogMLTextConstruct("Test Author")
        };
        document.Authors.Add(author);

        document.Authors.Count.ShouldBe(1);
        document.Authors[0].Id.ShouldBe("1");
        document.Authors[0].EmailAddress.ShouldBe("test@example.com");
        document.Authors[0].ApprovalStatus.ShouldBe(BlogMLApprovalStatus.Approved);
    }

    [TestMethod]
    public void Construction_WithCategories_AddsCorrectly()
    {
        BlogMLDocument document = new();
        BlogMLCategory category = new()
        {
            Id = "101",
            CreatedOn = new DateTime(2024, 1, 1),
            LastModifiedOn = new DateTime(2024, 1, 1),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Description = "Test Category",
            ParentId = "0",
            Title = new BlogMLTextConstruct("Category 1")
        };
        document.Categories.Add(category);

        document.Categories.Count.ShouldBe(1);
        document.Categories[0].Id.ShouldBe("101");
        document.Categories[0].Description.ShouldBe("Test Category");
        document.Categories[0].Title.Content.ShouldBe("Category 1");
    }

    [TestMethod]
    public void Construction_WithExtendedProperties_AddsCorrectly()
    {
        BlogMLDocument document = new();
        document.ExtendedProperties.Add("CommentModeration", "Anonymous");
        document.ExtendedProperties.Add("SendTrackback", "yes");

        document.ExtendedProperties.Count.ShouldBe(2);
        document.ExtendedProperties["CommentModeration"].ShouldBe("Anonymous");
        document.ExtendedProperties["SendTrackback"].ShouldBe("yes");
    }

    [TestMethod]
    public void Construction_WithPost_SetsCorrectly()
    {
        BlogMLDocument document = new();
        BlogMLPost post = new()
        {
            Id = "1",
            CreatedOn = new DateTime(2024, 1, 10),
            LastModifiedOn = new DateTime(2024, 1, 15),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Url = new Uri("/blogs/archive/2024/01/10/test-post.aspx", UriKind.Relative),
            PostType = BlogMLPostType.Normal,
            Views = "100",
            Title = new BlogMLTextConstruct("Test Blog Post"),
            Content = new BlogMLTextConstruct("<p>Test content</p>", BlogMLContentType.Html),
            Name = new BlogMLTextConstruct("Test Blog Post")
        };

        document.Posts.Add(post);

        document.Posts.Count().ShouldBe(1);
        document.Posts[0].Id.ShouldBe("1");
        document.Posts[0].Title.Content.ShouldBe("Test Blog Post");
        document.Posts[0].Content.Content.ShouldBe("<p>Test content</p>");
        document.Posts[0].Content.ContentType.ShouldBe(BlogMLContentType.Html);
        document.Posts[0].PostType.ShouldBe(BlogMLPostType.Normal);
    }

    [TestMethod]
    public void Construction_PostWithCategories_AddsCorrectly()
    {
        BlogMLDocument document = new();

        // Add categories to document first
        document.Categories.Add(new BlogMLCategory { Id = "101", Title = new BlogMLTextConstruct("Category 1") });
        document.Categories.Add(new BlogMLCategory { Id = "102", Title = new BlogMLTextConstruct("Category 2") });

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Test Post")
        };
        post.Categories.Add("101");
        post.Categories.Add("102");

        document.Posts.Add(post);

        document.Posts[0].Categories.Count.ShouldBe(2);
        document.Posts[0].Categories.ShouldContain("101");
        document.Posts[0].Categories.ShouldContain("102");
    }

    [TestMethod]
    public void Construction_PostWithComments_AddsCorrectly()
    {
        BlogMLDocument document = new();

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Test Post")
        };

        BlogMLComment comment = new()
        {
            Id = "1001",
            CreatedOn = new DateTime(2024, 1, 12),
            LastModifiedOn = new DateTime(2024, 1, 12),
            Title = new BlogMLTextConstruct("re: Test Post"),
            Content = new BlogMLTextConstruct("This is a test comment.")
        };
        post.Comments.Add(comment);

        document.Posts.Add(post);

        document.Posts[0].Comments.Count.ShouldBe(1);
        document.Posts[0].Comments[0].Title.Content.ShouldBe("re: Test Post");
        document.Posts[0].Comments[0].Content.Content.ShouldBe("This is a test comment.");
    }

    [TestMethod]
    public void Construction_PostWithAuthors_AddsCorrectly()
    {
        BlogMLDocument document = new();

        // Add author to document
        document.Authors.Add(new BlogMLAuthor { Id = "1", Title = new BlogMLTextConstruct("Author 1") });

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Test Post")
        };
        post.Authors.Add("1");

        document.Posts.Add(post);

        document.Posts[0].Authors.Count.ShouldBe(1);
        document.Posts[0].Authors.ShouldContain("1");
    }

    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a document
        BlogMLDocument originalDocument = CreateCompleteDocument();

        // Save to stream
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Load from stream
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Verify data preserved
        loadedDocument.Title.Content.ShouldBe(originalDocument.Title.Content);
        loadedDocument.Authors.Count.ShouldBe(originalDocument.Authors.Count);
        loadedDocument.Categories.Count.ShouldBe(originalDocument.Categories.Count);
        loadedDocument.Posts.Count().ShouldBe(originalDocument.Posts.Count());
    }

    [TestMethod]
    public void Save_ProducesValidXml()
    {
        BlogMLDocument document = CreateCompleteDocument();

        using MemoryStream stream = new();
        document.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<blog");
        xml.ShouldContain("xmlns=\"http://www.blogml.com/2006/09/BlogML\"");
        xml.ShouldContain("<title");
        xml.ShouldContain("<post");
    }

    private static BlogMLDocument CreateCompleteDocument()
    {
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("/blogs/default.aspx", UriKind.Relative),
            GeneratedOn = new DateTime(2024, 1, 15, 12, 0, 0),
            Title = new BlogMLTextConstruct("Test Blog"),
            Subtitle = new BlogMLTextConstruct("Test blog description")
        };

        document.Authors.Add(new BlogMLAuthor
        {
            Id = "1",
            CreatedOn = new DateTime(2024, 1, 1),
            LastModifiedOn = new DateTime(2024, 1, 15),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            EmailAddress = "test@example.com",
            Title = new BlogMLTextConstruct("Test Author")
        });

        document.Categories.Add(new BlogMLCategory
        {
            Id = "101",
            CreatedOn = new DateTime(2024, 1, 1),
            LastModifiedOn = new DateTime(2024, 1, 1),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Title = new BlogMLTextConstruct("Test Category")
        });

        BlogMLPost post = new()
        {
            Id = "1",
            CreatedOn = new DateTime(2024, 1, 10),
            LastModifiedOn = new DateTime(2024, 1, 10),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Title = new BlogMLTextConstruct("Test Post"),
            Content = new BlogMLTextConstruct("Test content", BlogMLContentType.Text)
        };
        post.Authors.Add("1");
        post.Categories.Add("101");

        document.Posts.Add(post);

        return document;
    }
}