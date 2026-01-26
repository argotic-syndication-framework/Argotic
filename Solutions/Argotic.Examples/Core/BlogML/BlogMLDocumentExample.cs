using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="BlogMLDocument"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="BlogMLDocument"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class BlogMLDocumentExample
{
    /// <summary>
    /// Provides example code for the BlogMLDocument class.
    /// </summary>
    public static void ClassExample()
    {
        BlogMLDocument document = new BlogMLDocument
        {
            RootUrl = new Uri("/blogs/default.aspx"),
            GeneratedOn = new DateTime(2006, 9, 5, 18, 22, 10),
            Title = new BlogMLTextConstruct("BlogML 2.0 Example"),
            Subtitle = new BlogMLTextConstruct("This is some sample blog content for BlogML 2.0")
        };

        BlogMLAuthor administrator      = new BlogMLAuthor
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

        BlogMLCategory category1    = new BlogMLCategory
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

        BlogMLCategory category2    = new BlogMLCategory
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

        BlogMLCategory category3    = new BlogMLCategory
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

        BlogMLPost post         = new BlogMLPost
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

        BlogMLComment comment   = new BlogMLComment
        {
            Id = "35",
            CreatedOn = new DateTime(2006, 9, 5, 11, 36, 50),
            LastModifiedOn = new DateTime(2006, 9, 5, 11, 36, 50),
            Title = new BlogMLTextConstruct("re: Sample Blog Post"),
            Content = new BlogMLTextConstruct("This is a test comment.")
        };
        post.Comments.Add(comment);
    }
    /// <summary>
    /// Provides example code for the BlogMLDocument.Create(Uri) method
    /// </summary>
    public static void CreateExample()
    {
        BlogMLDocument document = BlogMLDocument.Create(new Uri("http://www.example.org/blog/blogML.axd"));

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }
    }
    /// <summary>
    /// Provides example code for the LoadAsync(Uri, Object) method
    /// </summary>
    public static void LoadAsyncExample()
    {
        BlogMLDocument document   = new BlogMLDocument();

        document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

        document.LoadAsync(new Uri("http://www.example.org/blog/blogML.axd"), null);
    }

    /// <summary>
    /// Handles the <see cref="BlogMLDocument.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void ResourceLoadedCallback(object sender, SyndicationResourceLoadedEventArgs e)
    {
        if(e.State != null)
        {
        }
    }

    /// <summary>
    /// Provides example code for the Load(IXPathNavigable) method
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        XPathDocument source    = new XPathDocument("http://www.example.org/blog/blogML.axd");

        BlogMLDocument document = new BlogMLDocument();
        document.Load(source);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        BlogMLDocument document = new BlogMLDocument();

        using (Stream stream = new FileStream("BlogMLDocument.xml", FileMode.Open, FileAccess.Read))
        {
            document.Load(stream);

            foreach (BlogMLPost post in document.Posts)
            {
                if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
                {
                    //  Perform some processing on the blog post
                }
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        BlogMLDocument document = new BlogMLDocument();

        using (Stream stream = new FileStream("BlogMLDocument.xml", FileMode.Open, FileAccess.Read))
        {
            XmlReaderSettings settings  = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using(XmlReader reader = XmlReader.Create(stream, settings))
            {
                document.Load(reader);

                foreach (BlogMLPost post in document.Posts)
                {
                    if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
                    {
                        //  Perform some processing on the blog post
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
    /// </summary>
    public static void LoadUriExample()
    {
        BlogMLDocument document = new BlogMLDocument();
        Uri source              = new Uri("http://www.example.org/blog/blogML.axd");

        document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

        foreach (BlogMLPost post in document.Posts)
        {
            if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
            {
                //  Perform some processing on the blog post
            }
        }
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        BlogMLDocument document = new BlogMLDocument();

        //  Modify document state using public properties and methods

        using(Stream stream = new FileStream("BlogMLDocument.xml", FileMode.Create, FileAccess.Write))
        {
            document.Save(stream);
        }
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        BlogMLDocument document = new BlogMLDocument();

        //  Modify document state using public properties and methods

        using (Stream stream = new FileStream("BlogMLDocument.xml", FileMode.Create, FileAccess.Write))
        {
            XmlWriterSettings settings  = new XmlWriterSettings
            {
                Indent = true
            };

            using(XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                document.Save(writer);
            }
        }
    }
}