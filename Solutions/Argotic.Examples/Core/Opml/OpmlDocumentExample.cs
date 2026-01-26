using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="OpmlDocument"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="OpmlDocument"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class OpmlDocumentExample
{
    /// <summary>
    /// Provides example code for the OpmlDocument class.
    /// </summary>
    public static void ClassExample()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "Example OPML List",
                CreatedOn = new(2005, 6, 18, 12, 11, 52),
                ModifiedOn = new(2005, 7, 2, 21, 42, 48),
                Owner = new("John Doe", "john.doe@example.com"),
                VerticalScrollState = 1,
                Window = new(61, 304, 562, 842)
            }
        };

        OpmlOutline containerOutline = new("Feeds");
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Argotic", "rss", new("http://www.codeplex.com/Argotic/Project/ProjectRss.aspx")));
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Google News", "feed", new("http://news.google.com/?output=atom")));
        document.AddOutline(containerOutline);
    }
    /// <summary>
    /// Provides example code for the OpmlDocument.Create(Uri) method
    /// </summary>
    public static void CreateExample()
    {
        OpmlDocument document = OpmlDocument.Create(new("http://blog.oppositionallydefiant.com/opml.axd"));

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri, Object) method
    /// </summary>
    public static void LoadAsyncExample()
    {
        OpmlDocument document = new();

        document.Loaded += new(ResourceLoadedCallback);

        document.LoadAsync(new("http://blog.oppositionallydefiant.com/opml.axd"), null);
    }

    /// <summary>
    /// Handles the <see cref="OpmlDocument.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void ResourceLoadedCallback(object sender, SyndicationResourceLoadedEventArgs e)
    {
        if (e.State != null)
        {
        }
    }
    /// <summary>
    /// Provides example code for the Load(IXPathNavigable) method
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        XPathDocument source = new("http://blog.oppositionallydefiant.com/opml.axd");

        OpmlDocument document = new();
        document.Load(source);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        OpmlDocument document = new();

        using Stream stream = new FileStream("OpmlDocument.xml", FileMode.Open, FileAccess.Read);
        document.Load(stream);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        OpmlDocument document = new();

        using Stream stream = new FileStream("OpmlDocument.xml", FileMode.Open, FileAccess.Read);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        document.Load(reader);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
    /// </summary>
    public static void LoadUriExample()
    {
        OpmlDocument document = new();
        Uri source = new("http://blog.oppositionallydefiant.com/opml.axd");

        document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        OpmlDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new FileStream("OpmlDocument.xml", FileMode.Create, FileAccess.Write);
        document.Save(stream);
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        OpmlDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new FileStream("OpmlDocument.xml", FileMode.Create, FileAccess.Write);
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        document.Save(writer);
    }
}