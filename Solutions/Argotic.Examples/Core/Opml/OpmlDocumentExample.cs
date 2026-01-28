using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Opml;

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
                CreatedOn = new DateTime(2005, 6, 18, 12, 11, 52),
                ModifiedOn = new DateTime(2005, 7, 2, 21, 42, 48),
                Owner = new OpmlOwner("John Doe", "john.doe@example.com"),
                VerticalScrollState = 1,
                Window = new OpmlWindow(61, 304, 562, 842)
            }
        };

        OpmlOutline containerOutline = new("Feeds");
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("endjin", "rss", new Uri("https://endjin.com/atom.xml")));
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("endjin", "feed", new Uri("https://endjin.com/rss.xml")));
        document.Outlines.Add(containerOutline);
        ExampleOutput.ShowOpmlDocument(document);
    }
    /// <summary>
    /// Provides example code for the OpmlDocument.CreateAsync(Uri) method
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        // Note: This example would normally load from a URL
        // For demonstration, we load from a local sample file
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.OpmlDocument);
        OpmlDocument document = new();
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
        ExampleOutput.ShowOpmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri) method with event notification
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        OpmlDocument document = new();

        document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.OpmlDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);
        ExampleOutput.ShowOpmlDocument(document);
    }

    /// <summary>
    /// Handles the <see cref="OpmlDocument.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void ResourceLoadedCallback(object? sender, SyndicationResourceLoadedEventArgs e)
    {
        // Process the loaded document using e.Data or e.Source
        if (e.Source != null)
        {
            // Process the source URI
        }
    }
    /// <summary>
    /// Provides example code for the Load(IXPathNavigable) method
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        using XmlReader xmlReader = XmlReader.Create(SampleDataPath.OpmlDocument.FullPath, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        OpmlDocument document = new();
        document.Load(source);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
        ExampleOutput.ShowOpmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        OpmlDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.OpmlDocument);
        document.Load(stream);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
        ExampleOutput.ShowOpmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        OpmlDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.OpmlDocument);
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
        ExampleOutput.ShowOpmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        OpmlDocument document = new();

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.OpmlDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (OpmlOutline outline in document.Outlines)
        {
            if (outline.IsSubscriptionListOutline)
            {
                //  Process outline information
            }
        }
        ExampleOutput.ShowOpmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        OpmlDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        document.Save(stream);
        ExampleOutput.ShowSaved("OpmlDocument");
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        OpmlDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        document.Save(writer);
        ExampleOutput.ShowSaved("OpmlDocument");
    }
}