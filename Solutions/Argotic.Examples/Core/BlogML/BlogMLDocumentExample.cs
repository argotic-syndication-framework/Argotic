using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.BlogML;

/// <summary>
/// Demonstrates the whole <see cref="BlogMLDocument"/> surface: building a blog export by hand, then the <c>Load</c>, <c>LoadAsync</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     Every resource type in the library exposes this same set of overloads, so what is shown here for
///     <see cref="BlogMLDocument"/> reads across to the other formats unchanged. <c>CreateAsync</c> is the one-call
///     form; <c>LoadAsync</c> on an instance is the form that lets you subscribe to <c>Loaded</c> first.
/// </remarks>
internal static class BlogMLDocumentExample
{
    /// <summary>
    /// Builds a complete <see cref="BlogMLDocument"/> by hand and prints it.
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

        BlogMLPost post = new()
        {
            Id = "34",
            CreatedOn = new DateTime(2006, 9, 5, 3, 19, 0),
            LastModifiedOn = new DateTime(2006, 9, 5, 3, 19, 0),
            ApprovalStatus = BlogMLApprovalStatus.Approved,
            Url = new Uri("/blogs/archive/2006/09/05/Sample-Blog-Post.aspx"),
            PostType = BlogMLPostType.Normal,
            Views = "0",
            Title = new BlogMLTextConstruct("Writing Effective Copilot Instructions for Complex Codebases"),
            Content = new BlogMLTextConstruct("<p>This is <b>HTML encoded</b> content.&nbsp;</p>", BlogMLContentType.Html),
            Name = new BlogMLTextConstruct("Writing Effective Copilot Instructions for Complex Codebases")
        };

        post.Categories.Add("1018");
        post.Categories.Add("1020");

        post.Authors.Add("2100");

        BlogMLComment comment = new()
        {
            Id = "35",
            CreatedOn = new DateTime(2006, 9, 5, 11, 36, 50),
            LastModifiedOn = new DateTime(2006, 9, 5, 11, 36, 50),
            Title = new BlogMLTextConstruct("re: Writing Effective Copilot Instructions for Complex Codebases"),
            Content = new BlogMLTextConstruct("This is a test comment.")
        };
        post.Comments.Add(comment);

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Creates a <see cref="BlogMLDocument"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.BlogMLDocument);
        BlogMLDocument document = new();
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Subscribes to <c>Loaded</c> before loading, so the handler sees the resource the moment it is parsed.
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        BlogMLDocument document = new();

        document.Loaded += ResourceLoadedCallback;

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.BlogMLDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Handles the <see cref="BlogMLDocument.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void ResourceLoadedCallback(object? sender, SyndicationResourceLoadedEventArgs e)
    {
        // Process the loaded document using e.Data or e.Source
        if (e.Source is not null)
        {
            // Process the source URI
        }
    }

    /// <summary>
    /// Loads a <see cref="BlogMLDocument"/> from an <see cref="IXPathNavigable"/>.
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        using XmlReader xmlReader = XmlReader.Create(SampleDataPath.BlogMLDocument.FullPath, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        BlogMLDocument document = new();
        document.Load(source);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Loads a <see cref="BlogMLDocument"/> from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        BlogMLDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.BlogMLDocument);
        document.Load(stream);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Loads a <see cref="BlogMLDocument"/> from an <see cref="XmlReader"/>.
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        BlogMLDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.BlogMLDocument);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        document.Load(reader);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Loads a <see cref="BlogMLDocument"/> from a <see cref="Uri"/>, and shows where a caller-supplied <c>HttpClient</c> goes.
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        BlogMLDocument document = new();

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.BlogMLDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }

        ExampleOutput.ShowBlogMLDocument(document);
    }

    /// <summary>
    /// Saves a <see cref="BlogMLDocument"/> to a <see cref="Stream"/>.
    /// </summary>
    public static void SaveStreamExample()
    {
        BlogMLDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        document.Save(stream);

        ExampleOutput.ShowSaved("BlogMLDocument");
    }

    /// <summary>
    /// Saves a <see cref="BlogMLDocument"/> through an <see cref="XmlWriter"/>, with indentation turned on.
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        BlogMLDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        document.Save(writer);

        ExampleOutput.ShowSaved("BlogMLDocument");
    }
}