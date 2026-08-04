using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication.Specialized;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogML;

/// <summary>
/// Behavior-driven tests for <see cref="BlogMLDocument"/> covering creation, parsing, and round-trip scenarios.
/// </summary>
[TestClass]
public class BlogMLDocumentBehaviorTests
{
    public TestContext? TestContext { get; set; }

    #region Test Data

    /// <summary>
    /// A minimal valid BlogML document.
    /// </summary>
    private const string MinimalBlogML = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com">
            <title type="text">Test Blog</title>
            <sub-title type="text">A test blog</sub-title>
        </blog>
        """;

    /// <summary>
    /// A BlogML document with a single post.
    /// </summary>
    private const string BlogMLWithPost = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com" date-created="2024-01-15T10:00:00Z">
            <title type="text">Test Blog</title>
            <sub-title type="text">A test blog</sub-title>
            <posts>
                <post id="1" date-created="2024-01-15T10:00:00Z" date-modified="2024-01-15T10:00:00Z" approved="true" post-url="/posts/first-post" type="normal">
                    <title type="text">First Post</title>
                    <content type="html"><![CDATA[<p>This is the content.</p>]]></content>
                </post>
            </posts>
        </blog>
        """;

    /// <summary>
    /// A BlogML document with posts containing comments.
    /// </summary>
    private const string BlogMLWithComments = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com">
            <title type="text">Test Blog</title>
            <posts>
                <post id="1" date-created="2024-01-15T10:00:00Z" approved="true">
                    <title type="text">Post with Comments</title>
                    <content type="text">Post content</content>
                    <comments>
                        <comment id="101" date-created="2024-01-16T10:00:00Z" user-name="John Doe" user-email="john@example.com" approved="true">
                            <title type="text">re: Post with Comments</title>
                            <content type="text">Great post!</content>
                        </comment>
                        <comment id="102" date-created="2024-01-17T10:00:00Z" user-name="Jane Smith" approved="true">
                            <title type="text">re: Post with Comments</title>
                            <content type="text">I agree!</content>
                        </comment>
                    </comments>
                </post>
            </posts>
        </blog>
        """;

    /// <summary>
    /// A BlogML document with categories.
    /// </summary>
    private const string BlogMLWithCategories = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com">
            <title type="text">Test Blog</title>
            <categories>
                <category id="cat1" date-created="2024-01-01T00:00:00Z" approved="true">
                    <title type="text">Technology</title>
                </category>
                <category id="cat2" date-created="2024-01-01T00:00:00Z" approved="true" parentref="cat1">
                    <title type="text">Programming</title>
                </category>
            </categories>
            <posts>
                <post id="1" date-created="2024-01-15T10:00:00Z" approved="true">
                    <title type="text">Programming Post</title>
                    <content type="text">Content</content>
                    <categories>
                        <category ref="cat2" />
                    </categories>
                </post>
            </posts>
        </blog>
        """;

    /// <summary>
    /// A BlogML document with authors.
    /// </summary>
    private const string BlogMLWithAuthors = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com">
            <title type="text">Test Blog</title>
            <authors>
                <author id="author1" date-created="2024-01-01T00:00:00Z" approved="true" email="admin@example.com">
                    <title type="text">Admin</title>
                </author>
                <author id="author2" date-created="2024-01-01T00:00:00Z" approved="true" email="writer@example.com">
                    <title type="text">Writer</title>
                </author>
            </authors>
            <posts>
                <post id="1" date-created="2024-01-15T10:00:00Z" approved="true">
                    <title type="text">Post by Admin</title>
                    <content type="text">Content</content>
                    <authors>
                        <author ref="author1" />
                    </authors>
                </post>
            </posts>
        </blog>
        """;

    /// <summary>
    /// A complete BlogML document with all elements.
    /// </summary>
    private const string CompleteBlogML = """
        <?xml version="1.0" encoding="UTF-8"?>
        <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com" date-created="2024-01-01T00:00:00Z">
            <title type="text">Complete Blog</title>
            <sub-title type="text">A complete test blog</sub-title>
            <authors>
                <author id="author1" date-created="2024-01-01T00:00:00Z" approved="true" email="admin@example.com">
                    <title type="text">Admin</title>
                </author>
            </authors>
            <extended-properties>
                <property name="CommentModeration" value="Anonymous" />
                <property name="SendTrackback" value="yes" />
            </extended-properties>
            <categories>
                <category id="cat1" date-created="2024-01-01T00:00:00Z" approved="true">
                    <title type="text">Technology</title>
                </category>
            </categories>
            <posts>
                <post id="1" date-created="2024-01-15T10:00:00Z" date-modified="2024-01-15T12:00:00Z" approved="true" post-url="/posts/first" type="normal" views="100">
                    <title type="text">First Post</title>
                    <content type="html"><![CDATA[<p>HTML content</p>]]></content>
                    <post-name type="text">first-post</post-name>
                    <excerpt type="text">This is an excerpt</excerpt>
                    <categories>
                        <category ref="cat1" />
                    </categories>
                    <authors>
                        <author ref="author1" />
                    </authors>
                    <comments>
                        <comment id="c1" date-created="2024-01-16T10:00:00Z" user-name="Commenter" approved="true">
                            <title type="text">re: First Post</title>
                            <content type="text">Nice post!</content>
                        </comment>
                    </comments>
                </post>
            </posts>
        </blog>
        """;

    #endregion

    #region Document Creation Tests

    [TestMethod]
    public void BlogMLDocument_WhenCreatedWithRootUrl_ContainsCorrectRootUrl()
    {
        // Arrange & Act
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("http://example.com")
        };

        // Assert
        document.RootUrl.ShouldBe(new Uri("http://example.com"));
    }

    [TestMethod]
    public void BlogMLDocument_WhenCreatedWithTitle_ContainsCorrectTitle()
    {
        // Arrange & Act
        BlogMLDocument document = new()
        {
            Title = new BlogMLTextConstruct("My Blog")
        };

        // Assert
        document.Title.ShouldNotBeNull();
        document.Title.Content.ShouldBe("My Blog");
    }

    [TestMethod]
    public void BlogMLDocument_WhenCreatedWithSubtitle_ContainsCorrectSubtitle()
    {
        // Arrange & Act
        BlogMLDocument document = new()
        {
            Subtitle = new BlogMLTextConstruct("A test blog about testing")
        };

        // Assert
        document.Subtitle.ShouldNotBeNull();
        document.Subtitle.Content.ShouldBe("A test blog about testing");
    }

    [TestMethod]
    public void BlogMLDocument_WhenCreatedWithGeneratedOn_ContainsCorrectDate()
    {
        // Arrange
        DateTime generatedOn = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        BlogMLDocument document = new()
        {
            GeneratedOn = generatedOn
        };

        // Assert
        document.GeneratedOn.ShouldBe(generatedOn);
    }

    [TestMethod]
    public void BlogMLDocument_WhenPostsAdded_ContainsAllPosts()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogMLPost post1 = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("First Post"),
            Content = new BlogMLTextConstruct("<p>Content 1</p>", BlogMLContentType.Html)
        };
        BlogMLPost post2 = new()
        {
            Id = "2",
            Title = new BlogMLTextConstruct("Second Post"),
            Content = new BlogMLTextConstruct("<p>Content 2</p>", BlogMLContentType.Html)
        };

        // Act
        document.Posts.Add(post1);
        document.Posts.Add(post2);

        // Assert
        document.Posts.Count.ShouldBe(2);
        document.Posts[0].Id.ShouldBe("1");
        document.Posts[0].Title.Content.ShouldBe("First Post");
        document.Posts[1].Id.ShouldBe("2");
        document.Posts[1].Title.Content.ShouldBe("Second Post");
    }

    [TestMethod]
    public void BlogMLDocument_WhenPostWithContentAdded_PreservesContentType()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("HTML Post"),
            Content = new BlogMLTextConstruct("<p>HTML <b>formatted</b> content</p>", BlogMLContentType.Html)
        };

        // Act
        document.Posts.Add(post);

        // Assert
        document.Posts[0].Content.ContentType.ShouldBe(BlogMLContentType.Html);
        document.Posts[0].Content.Content.ShouldBe("<p>HTML <b>formatted</b> content</p>");
    }

    [TestMethod]
    public void BlogMLDocument_WhenCategoriesAdded_ContainsAllCategories()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogMLCategory category1 = new()
        {
            Id = "cat1",
            Title = new BlogMLTextConstruct("Technology"),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        BlogMLCategory category2 = new()
        {
            Id = "cat2",
            Title = new BlogMLTextConstruct("Science"),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };

        // Act
        document.Categories.Add(category1);
        document.Categories.Add(category2);

        // Assert
        document.Categories.Count.ShouldBe(2);
        document.Categories[0].Id.ShouldBe("cat1");
        document.Categories[0].Title.Content.ShouldBe("Technology");
        document.Categories[1].Id.ShouldBe("cat2");
        document.Categories[1].Title.Content.ShouldBe("Science");
    }

    [TestMethod]
    public void BlogMLDocument_WhenAuthorsAdded_ContainsAllAuthors()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogMLAuthor author1 = new()
        {
            Id = "auth1",
            Title = new BlogMLTextConstruct("Admin"),
            EmailAddress = "admin@example.com",
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        BlogMLAuthor author2 = new()
        {
            Id = "auth2",
            Title = new BlogMLTextConstruct("Writer"),
            EmailAddress = "writer@example.com",
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };

        // Act
        document.Authors.Add(author1);
        document.Authors.Add(author2);

        // Assert
        document.Authors.Count.ShouldBe(2);
        document.Authors[0].Id.ShouldBe("auth1");
        document.Authors[0].EmailAddress.ShouldBe("admin@example.com");
        document.Authors[1].Id.ShouldBe("auth2");
        document.Authors[1].EmailAddress.ShouldBe("writer@example.com");
    }

    [TestMethod]
    public void BlogMLDocument_WhenPostWithCommentsAdded_ContainsAllComments()
    {
        // Arrange
        BlogMLDocument document = new();
        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Post with Comments"),
            Content = new BlogMLTextConstruct("Content")
        };

        BlogMLComment comment1 = new()
        {
            Id = "c1",
            Title = new BlogMLTextConstruct("First Comment"),
            Content = new BlogMLTextConstruct("Great post!"),
            UserName = "John Doe",
            UserEmailAddress = "john@example.com"
        };

        BlogMLComment comment2 = new()
        {
            Id = "c2",
            Title = new BlogMLTextConstruct("Second Comment"),
            Content = new BlogMLTextConstruct("I agree!"),
            UserName = "Jane Smith"
        };

        // Act
        post.Comments.Add(comment1);
        post.Comments.Add(comment2);
        document.Posts.Add(post);

        // Assert
        document.Posts[0].Comments.Count.ShouldBe(2);
        document.Posts[0].Comments[0].UserName.ShouldBe("John Doe");
        document.Posts[0].Comments[0].Content.Content.ShouldBe("Great post!");
        document.Posts[0].Comments[1].UserName.ShouldBe("Jane Smith");
    }

    #endregion

    #region Document Parsing Tests

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromMinimalXml_PopulatesBasicProperties()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(MinimalBlogML));
        document.Load(stream);

        // Assert
        document.RootUrl.ShouldBe(new Uri("http://example.com"));
        document.Title.ShouldNotBeNull();
        document.Title.Content.ShouldBe("Test Blog");
        document.Subtitle.ShouldNotBeNull();
        document.Subtitle.Content.ShouldBe("A test blog");
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromXmlWithPost_PopulatesPost()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(BlogMLWithPost));
        document.Load(stream);

        // Assert
        document.Posts.Count.ShouldBe(1);
        document.Posts[0].Id.ShouldBe("1");
        document.Posts[0].Title.Content.ShouldBe("First Post");
        document.Posts[0].Content.Content.ShouldBe("<p>This is the content.</p>");
        document.Posts[0].Content.ContentType.ShouldBe(BlogMLContentType.Html);
        document.Posts[0].ApprovalStatus.ShouldBe(BlogMLApprovalStatus.Approved);
        document.Posts[0].PostType.ShouldBe(BlogMLPostType.Normal);
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromXmlWithComments_PopulatesComments()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(BlogMLWithComments));
        document.Load(stream);

        // Assert
        document.Posts.Count.ShouldBe(1);
        document.Posts[0].Comments.Count.ShouldBe(2);

        BlogMLComment firstComment = document.Posts[0].Comments[0];
        firstComment.Id.ShouldBe("101");
        firstComment.UserName.ShouldBe("John Doe");
        firstComment.UserEmailAddress.ShouldBe("john@example.com");
        firstComment.Content.Content.ShouldBe("Great post!");

        BlogMLComment secondComment = document.Posts[0].Comments[1];
        secondComment.Id.ShouldBe("102");
        secondComment.UserName.ShouldBe("Jane Smith");
        secondComment.Content.Content.ShouldBe("I agree!");
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromXmlWithCategories_PopulatesCategories()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(BlogMLWithCategories));
        document.Load(stream);

        // Assert
        document.Categories.Count.ShouldBe(2);
        document.Categories[0].Id.ShouldBe("cat1");
        document.Categories[0].Title.Content.ShouldBe("Technology");
        document.Categories[1].Id.ShouldBe("cat2");
        document.Categories[1].Title.Content.ShouldBe("Programming");
        document.Categories[1].ParentId.ShouldBe("cat1");

        // Post should reference category
        document.Posts[0].Categories.Count.ShouldBe(1);
        document.Posts[0].Categories[0].ShouldBe("cat2");
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromXmlWithAuthors_PopulatesAuthors()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(BlogMLWithAuthors));
        document.Load(stream);

        // Assert
        document.Authors.Count.ShouldBe(2);
        document.Authors[0].Id.ShouldBe("author1");
        document.Authors[0].Title.Content.ShouldBe("Admin");
        document.Authors[0].EmailAddress.ShouldBe("admin@example.com");
        document.Authors[1].Id.ShouldBe("author2");
        document.Authors[1].Title.Content.ShouldBe("Writer");

        // Post should reference author
        document.Posts[0].Authors.Count.ShouldBe(1);
        document.Posts[0].Authors[0].ShouldBe("author1");
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromCompleteBlogML_PopulatesAllProperties()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(CompleteBlogML));
        document.Load(stream);

        // Assert
        document.RootUrl.ShouldBe(new Uri("http://example.com"));
        document.Title.Content.ShouldBe("Complete Blog");
        document.Subtitle!.Content.ShouldBe("A complete test blog");
        document.Authors.Count.ShouldBe(1);
        document.Categories.Count.ShouldBe(1);
        document.Posts.Count.ShouldBe(1);
        document.ExtendedProperties.Count.ShouldBe(2);
        document.ExtendedProperties["CommentModeration"].ShouldBe("Anonymous");
        document.ExtendedProperties["SendTrackback"].ShouldBe("yes");

        BlogMLPost post = document.Posts[0];
        post.Views.ShouldBe("100");
        post.Excerpt.ShouldNotBeNull();
        post.Excerpt.Content.ShouldBe("This is an excerpt");
        post.Name.ShouldNotBeNull();
        post.Name.Content.ShouldBe("first-post");
        post.Comments.Count.ShouldBe(1);
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromStream_RaisesLoadedEvent()
    {
        // Arrange
        BlogMLDocument document = new();
        bool eventRaised = false;
        SyndicationResourceLoadedEventArgs? eventArgs = null;

        document.Loaded += (sender, args) =>
        {
            eventRaised = true;
            eventArgs = args;
        };

        // Act
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(MinimalBlogML));
        document.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue();
        eventArgs.ShouldNotBeNull();
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromXmlReader_PopulatesProperties()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(MinimalBlogML));
        document.Load(reader);

        // Assert
        document.Title.Content.ShouldBe("Test Blog");
        document.RootUrl.ShouldBe(new Uri("http://example.com"));
    }

    [TestMethod]
    public void BlogMLDocument_WhenLoadedFromMalformedXml_ThrowsXmlException()
    {
        // Arrange
        const string malformedXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <blog xmlns="http://www.blogml.com/2006/09/BlogML">
                <title>Unclosed Tag
            </blog>
            """;

        BlogMLDocument document = new();

        // Act & Assert
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(malformedXml));
        Should.Throw<XmlException>(() => document.Load(stream));
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloaded_PreservesBasicProperties()
    {
        // Arrange
        BlogMLDocument originalDocument = new()
        {
            RootUrl = new Uri("http://example.com/blog"),
            GeneratedOn = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Title = new BlogMLTextConstruct("Round Trip Blog"),
            Subtitle = new BlogMLTextConstruct("Testing round-trip serialization")
        };

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.RootUrl.ShouldBe(originalDocument.RootUrl);
        loadedDocument.Title.Content.ShouldBe(originalDocument.Title.Content);
        loadedDocument.Subtitle.ShouldNotBeNull();
        loadedDocument.Subtitle.Content.ShouldBe(originalDocument.Subtitle.Content);
    }

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloaded_PreservesPosts()
    {
        // Arrange
        BlogMLDocument originalDocument = CreateDocumentWithPosts();

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Posts.Count.ShouldBe(originalDocument.Posts.Count);
        for (int i = 0; i < originalDocument.Posts.Count; i++)
        {
            loadedDocument.Posts[i].Id.ShouldBe(originalDocument.Posts[i].Id);
            loadedDocument.Posts[i].Title.Content.ShouldBe(originalDocument.Posts[i].Title.Content);
            loadedDocument.Posts[i].Content.Content.ShouldBe(originalDocument.Posts[i].Content.Content);
            loadedDocument.Posts[i].Content.ContentType.ShouldBe(originalDocument.Posts[i].Content.ContentType);
        }
    }

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloaded_PreservesPostsWithComments()
    {
        // Arrange
        BlogMLDocument originalDocument = new()
        {
            RootUrl = new Uri("http://example.com"),
            Title = new BlogMLTextConstruct("Comments Blog")
        };

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Post with Comments"),
            Content = new BlogMLTextConstruct("Post content"),
            CreatedOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };

        BlogMLComment comment = new()
        {
            Id = "c1",
            Title = new BlogMLTextConstruct("Comment Title"),
            Content = new BlogMLTextConstruct("Comment content"),
            UserName = "Commenter",
            UserEmailAddress = "commenter@example.com",
            CreatedOn = new DateTime(2024, 1, 16, 10, 0, 0, DateTimeKind.Utc),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        post.Comments.Add(comment);
        originalDocument.Posts.Add(post);

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Posts.Count.ShouldBe(1);
        loadedDocument.Posts[0].Comments.Count.ShouldBe(1);

        BlogMLComment loadedComment = loadedDocument.Posts[0].Comments[0];
        loadedComment.Id.ShouldBe("c1");
        loadedComment.Title.Content.ShouldBe("Comment Title");
        loadedComment.Content.Content.ShouldBe("Comment content");
        loadedComment.UserName.ShouldBe("Commenter");
        loadedComment.UserEmailAddress.ShouldBe("commenter@example.com");
    }

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloaded_PreservesCategories()
    {
        // Arrange
        BlogMLDocument originalDocument = new()
        {
            RootUrl = new Uri("http://example.com"),
            Title = new BlogMLTextConstruct("Categories Blog")
        };

        originalDocument.Categories.Add(new BlogMLCategory
        {
            Id = "cat1",
            Title = new BlogMLTextConstruct("Technology"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        originalDocument.Categories.Add(new BlogMLCategory
        {
            Id = "cat2",
            Title = new BlogMLTextConstruct("Programming"),
            ParentId = "cat1",
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Tech Post"),
            Content = new BlogMLTextConstruct("Content"),
            CreatedOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        post.Categories.Add("cat1");
        post.Categories.Add("cat2");
        originalDocument.Posts.Add(post);

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Categories.Count.ShouldBe(2);
        loadedDocument.Categories[0].Id.ShouldBe("cat1");
        loadedDocument.Categories[0].Title.Content.ShouldBe("Technology");
        loadedDocument.Categories[1].Id.ShouldBe("cat2");
        loadedDocument.Categories[1].Title.Content.ShouldBe("Programming");
        loadedDocument.Categories[1].ParentId.ShouldBe("cat1");

        loadedDocument.Posts[0].Categories.Count.ShouldBe(2);
        loadedDocument.Posts[0].Categories.ShouldContain("cat1");
        loadedDocument.Posts[0].Categories.ShouldContain("cat2");
    }

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloaded_PreservesAuthors()
    {
        // Arrange
        BlogMLDocument originalDocument = new()
        {
            RootUrl = new Uri("http://example.com"),
            Title = new BlogMLTextConstruct("Authors Blog")
        };

        originalDocument.Authors.Add(new BlogMLAuthor
        {
            Id = "auth1",
            Title = new BlogMLTextConstruct("Admin"),
            EmailAddress = "admin@example.com",
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Admin Post"),
            Content = new BlogMLTextConstruct("Content"),
            CreatedOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        post.Authors.Add("auth1");
        originalDocument.Posts.Add(post);

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Authors.Count.ShouldBe(1);
        loadedDocument.Authors[0].Id.ShouldBe("auth1");
        loadedDocument.Authors[0].Title.Content.ShouldBe("Admin");
        loadedDocument.Authors[0].EmailAddress.ShouldBe("admin@example.com");

        loadedDocument.Posts[0].Authors.Count.ShouldBe(1);
        loadedDocument.Posts[0].Authors[0].ShouldBe("auth1");
    }

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloaded_PreservesExtendedProperties()
    {
        // Arrange
        BlogMLDocument originalDocument = new()
        {
            RootUrl = new Uri("http://example.com"),
            Title = new BlogMLTextConstruct("Extended Props Blog")
        };

        originalDocument.ExtendedProperties.Add("Property1", "Value1");
        originalDocument.ExtendedProperties.Add("Property2", "Value2");

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        BlogMLDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.ExtendedProperties.Count.ShouldBe(2);
        loadedDocument.ExtendedProperties["Property1"].ShouldBe("Value1");
        loadedDocument.ExtendedProperties["Property2"].ShouldBe("Value2");
    }

    [TestMethod]
    public void BlogMLDocument_ParseSerializeParse_ProducesSameDocument()
    {
        // Arrange
        BlogMLDocument firstDocument = new();
        using MemoryStream firstStream = new(Encoding.UTF8.GetBytes(CompleteBlogML));
        firstDocument.Load(firstStream);

        // Act - First serialize
        using MemoryStream serializeStream = new();
        firstDocument.Save(serializeStream);

        // Act - Second parse
        serializeStream.Position = 0;
        BlogMLDocument secondDocument = new();
        secondDocument.Load(serializeStream);

        // Assert - Core properties match
        secondDocument.RootUrl.ShouldBe(firstDocument.RootUrl);
        secondDocument.Title.Content.ShouldBe(firstDocument.Title.Content);
        secondDocument.Subtitle!.Content.ShouldBe(firstDocument.Subtitle!.Content);
        secondDocument.Authors.Count.ShouldBe(firstDocument.Authors.Count);
        secondDocument.Categories.Count.ShouldBe(firstDocument.Categories.Count);
        secondDocument.Posts.Count.ShouldBe(firstDocument.Posts.Count);
        secondDocument.ExtendedProperties.Count.ShouldBe(firstDocument.ExtendedProperties.Count);

        // Assert - Post details match
        for (int i = 0; i < firstDocument.Posts.Count; i++)
        {
            secondDocument.Posts[i].Id.ShouldBe(firstDocument.Posts[i].Id);
            secondDocument.Posts[i].Title.Content.ShouldBe(firstDocument.Posts[i].Title.Content);
            secondDocument.Posts[i].Content.Content.ShouldBe(firstDocument.Posts[i].Content.Content);
            secondDocument.Posts[i].Comments.Count.ShouldBe(firstDocument.Posts[i].Comments.Count);
        }
    }

    [TestMethod]
    public void BlogMLDocument_WhenSavedAndReloadedTwice_MaintainsDataIntegrity()
    {
        // Arrange
        BlogMLDocument originalDocument = CreateCompleteDocument();

        // Act - First round trip
        using MemoryStream stream1 = new();
        originalDocument.Save(stream1);
        stream1.Position = 0;
        BlogMLDocument document1 = new();
        document1.Load(stream1);

        // Act - Second round trip
        using MemoryStream stream2 = new();
        document1.Save(stream2);
        stream2.Position = 0;
        BlogMLDocument document2 = new();
        document2.Load(stream2);

        // Assert
        document2.Title.Content.ShouldBe(originalDocument.Title.Content);
        document2.Authors.Count.ShouldBe(originalDocument.Authors.Count);
        document2.Categories.Count.ShouldBe(originalDocument.Categories.Count);
        document2.Posts.Count.ShouldBe(originalDocument.Posts.Count);
        document2.Posts[0].Comments.Count.ShouldBe(originalDocument.Posts[0].Comments.Count);
    }

    #endregion

    #region Format and Version Tests

    [TestMethod]
    public void BlogMLDocument_Format_ReturnsBlogML()
    {
        // Arrange
        BlogMLDocument document = new();

        // Assert
        document.Format.ShouldBe(SyndicationContentFormat.BlogML);
    }

    [TestMethod]
    public void BlogMLDocument_Version_Returns2Point0()
    {
        // Arrange
        BlogMLDocument document = new();

        // Assert
        document.Version.Major.ShouldBe(2);
        document.Version.Minor.ShouldBe(0);
    }

    #endregion

    #region Async Operations Tests

    [TestMethod]
    public async Task BlogMLDocument_LoadAsync_LoadsDocumentCorrectly()
    {
        // Arrange
        BlogMLDocument document = new();
        bool eventRaised = false;
        document.Loaded += (sender, args) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalBlogML);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(
            new Uri("http://example.com/blog.xml"),
            httpClient,
            cancellationToken: TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        eventRaised.ShouldBeTrue();
        document.Title.Content.ShouldBe("Test Blog");
        document.RootUrl.ShouldBe(new Uri("http://example.com"));
    }

    [TestMethod]
    public async Task BlogMLDocument_CreateAsync_CreatesAndLoadsNewDocument()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(BlogMLWithPost);
        using HttpClient httpClient = new(handler);

        // Act
        BlogMLDocument document = await BlogMLDocument.CreateAsync(
            new Uri("http://example.com/blog.xml"),
            httpClient,
            cancellationToken: TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        document.ShouldNotBeNull();
        document.Title.Content.ShouldBe("Test Blog");
        document.Posts.Count.ShouldBe(1);
        document.Format.ShouldBe(SyndicationContentFormat.BlogML);
    }

    [TestMethod]
    public async Task BlogMLDocument_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        BlogMLDocument document = new();
        Uri? sourceFromEvent = null;

        document.Loaded += (sender, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalBlogML);
        using HttpClient httpClient = new(handler);
        Uri requestUri = new("http://example.com/blog.xml");

        // Act
        await document.LoadAsync(requestUri, httpClient, cancellationToken: TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    #endregion

    #region Additional Behavior Tests

    [TestMethod]
    public void BlogMLDocument_CreateNavigator_ReturnsValidNavigator()
    {
        // Arrange
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("http://example.com"),
            Title = new BlogMLTextConstruct("Navigator Test Blog")
        };

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Test Post"),
            Content = new BlogMLTextConstruct("Content"),
            CreatedOn = DateTime.UtcNow,
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        document.Posts.Add(post);

        // Act
        XPathNavigator navigator = document.CreateNavigator();

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("blog");
    }

    [TestMethod]
    public void BlogMLDocument_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        BlogMLDocument document = new()
        {
            Title = new BlogMLTextConstruct("Test Blog")
        };

        // Assert
        document.HasExtensions.ShouldBeFalse();
    }

    [TestMethod]
    public void BlogMLDocument_FindExtension_ReturnsNullWhenNoMatch()
    {
        // Arrange
        BlogMLDocument document = new()
        {
            Title = new BlogMLTextConstruct("Test Blog")
        };

        // Act
        ISyndicationExtension? extension = document.FindExtension(ext => ext.XmlNamespace == "http://nonexistent.example.com");

        // Assert
        extension.ShouldBeNull();
    }

    [TestMethod]
    public void BlogMLDocument_SetTitleToNull_ThrowsArgumentNullException()
    {
        // Arrange
        BlogMLDocument document = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => document.Title = null!);
    }

    [TestMethod]
    public void BlogMLDocument_Save_ProducesValidXml()
    {
        // Arrange
        BlogMLDocument document = CreateCompleteDocument();

        // Act
        using MemoryStream stream = new();
        document.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<blog");
        xml.ShouldContain("xmlns=\"http://www.blogml.com/2006/09/BlogML\"");
        xml.ShouldContain("<title");
        xml.ShouldContain("<post");
        xml.ShouldContain("<comment");
    }

    #endregion

    #region Helper Methods

    private static BlogMLDocument CreateDocumentWithPosts()
    {
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("http://example.com"),
            GeneratedOn = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Title = new BlogMLTextConstruct("Posts Blog")
        };

        for (int i = 1; i <= 3; i++)
        {
            document.Posts.Add(new BlogMLPost
            {
                Id = i.ToString(),
                Title = new BlogMLTextConstruct($"Post {i}"),
                Content = new BlogMLTextConstruct($"<p>Content {i}</p>", BlogMLContentType.Html),
                CreatedOn = new DateTime(2024, 1, i, 10, 0, 0, DateTimeKind.Utc),
                ApprovalStatus = BlogMLApprovalStatus.Approved
            });
        }

        return document;
    }

    private static BlogMLDocument CreateCompleteDocument()
    {
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("http://example.com/blog"),
            GeneratedOn = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Title = new BlogMLTextConstruct("Complete Test Blog"),
            Subtitle = new BlogMLTextConstruct("A complete blog for testing")
        };

        document.Authors.Add(new BlogMLAuthor
        {
            Id = "auth1",
            Title = new BlogMLTextConstruct("Admin"),
            EmailAddress = "admin@example.com",
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        document.Categories.Add(new BlogMLCategory
        {
            Id = "cat1",
            Title = new BlogMLTextConstruct("Technology"),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        document.ExtendedProperties.Add("TestProperty", "TestValue");

        BlogMLPost post = new()
        {
            Id = "1",
            Title = new BlogMLTextConstruct("Complete Post"),
            Content = new BlogMLTextConstruct("<p>Complete content</p>", BlogMLContentType.Html),
            CreatedOn = new DateTime(2024, 1, 10, 10, 0, 0, DateTimeKind.Utc),
            LastModifiedOn = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            PostType = BlogMLPostType.Normal,
            Views = "50"
        };
        post.Authors.Add("auth1");
        post.Categories.Add("cat1");

        BlogMLComment comment = new()
        {
            Id = "c1",
            Title = new BlogMLTextConstruct("Comment"),
            Content = new BlogMLTextConstruct("Great post!"),
            UserName = "Commenter",
            UserEmailAddress = "commenter@example.com",
            CreatedOn = new DateTime(2024, 1, 11, 10, 0, 0, DateTimeKind.Utc),
            ApprovalStatus = BlogMLApprovalStatus.Approved
        };
        post.Comments.Add(comment);

        document.Posts.Add(post);

        return document;
    }

    #endregion
}