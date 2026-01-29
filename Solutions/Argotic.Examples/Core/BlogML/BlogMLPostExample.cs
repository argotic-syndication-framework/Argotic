using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.BlogML;

/// <summary>
/// Contains the code examples for the <see cref="BlogMLPost"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="BlogMLPost"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class BlogMLPostExample
{
    /// <summary>
    /// Provides example code for the BlogMLPost class.
    /// </summary>
    public static void ClassExample()
    {
        BlogMLDocument document = new()
        {
            RootUrl = new Uri("/blogs/default.aspx"),
            GeneratedOn = new DateTime(2006, 9, 5, 18, 22, 10),
            Title = new BlogMLTextConstruct("BlogML 2.0 Example"),
            Subtitle = new BlogMLTextConstruct("This is some sample blog content for BlogML 2.0")
        };

        BlogMLAuthor administrator = new()
        {
            Id = "2100",
            CreatedOn = new DateTime(2006, 8, 10, 8, 44, 35),
            LastModifiedOn = new DateTime(2006, 9, 4, 13, 46, 38),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            EmailAddress = "someone@domain.com",
            Title = new BlogMLTextConstruct("admin")
        };
        document.Authors.Add(administrator);

        document.ExtendedProperties.Add("CommentModeration", "Anonymous");
        document.ExtendedProperties.Add("SendTrackback", "yes");

        BlogMLCategory category1 = new()
        {
            Id = "1018",
            CreatedOn = new DateTime(2006, 9, 5, 17, 54, 58),
            LastModifiedOn = new DateTime(2006, 9, 5, 17, 54, 58),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Description = "Sample Category 1",
            ParentId = "0",
            Title = new BlogMLTextConstruct("Category 1")
        };
        document.Categories.Add(category1);

        BlogMLCategory category2 = new()
        {
            Id = "1019",
            CreatedOn = new DateTime(2006, 9, 5, 17, 54, 59),
            LastModifiedOn = new DateTime(2006, 9, 5, 17, 54, 59),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Description = "Sample Category 2",
            ParentId = "0",
            Title = new BlogMLTextConstruct("Category 2")
        };
        document.Categories.Add(category2);

        BlogMLCategory category3 = new()
        {
            Id = "1020",
            CreatedOn = new DateTime(2006, 9, 5, 17, 55, 0),
            LastModifiedOn = new DateTime(2006, 9, 5, 17, 55, 0),
            ApprovalStatus = BlogMLApprovalStatus.NotApproved,
            Description = "Sample Category 3",
            ParentId = "0",
            Title = new BlogMLTextConstruct("Category 3")
        };
        document.Categories.Add(category3);

        //  Create a blog entry
        BlogMLPost post = new()
        {
            Id = "34",
            CreatedOn = new DateTime(2006, 9, 5, 3, 19, 0),
            LastModifiedOn = new DateTime(2006, 9, 5, 3, 19, 0),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Url = new Uri("/blogs/archive/2006/09/05/Sample-Blog-Post.aspx"),
            PostType = BlogMLPostType.Normal,
            Views = "0",
            Title = new BlogMLTextConstruct("Sample Blog Post"),
            Content = new BlogMLTextConstruct("<p>This is <b>HTML encoded</b> content.&nbsp;</p>", BlogMLContentType.Html),
            Name = new BlogMLTextConstruct("Sample Blog Post")
        };

        post.Categories.Add("1018");
        post.Categories.Add("1020");

        post.Authors.Add("2100");

        BlogMLComment comment = new()
        {
            Id = "35",
            CreatedOn = new DateTime(2006, 9, 5, 11, 36, 50),
            LastModifiedOn = new DateTime(2006, 9, 5, 11, 36, 50),
            Title = new BlogMLTextConstruct("re: Sample Blog Post"),
            Content = new BlogMLTextConstruct("This is a test comment.")
        };
        post.Comments.Add(comment);

        ExampleOutput.ShowBlogMLPost(post);
    }
    /// <summary>
    /// Provides example code for the BlogMLPost.PostTypeAsString(BlogMLPostType) method
    /// </summary>
    public static void PostTypeAsStringExample()
    {
        string postType = BlogMLPost.PostTypeAsString(BlogMLPostType.Normal);   // normal

        if (string.Equals(postType, "normal", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    /// <summary>
    /// Provides example code for the BlogMLPost.PostTypeByName(string) method
    /// </summary>
    public static void PostTypeByNameExample()
    {
        BlogMLPostType postType = BlogMLPost.PostTypeByName("normal");

        if (postType == BlogMLPostType.Normal)
        {
        }
    }
}