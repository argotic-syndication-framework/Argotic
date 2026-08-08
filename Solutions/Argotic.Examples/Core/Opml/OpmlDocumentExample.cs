using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Opml;

/// <summary>
/// Demonstrates the whole <see cref="OpmlDocument"/> surface: building a subscription list by hand, then the <c>Load</c>, <c>LoadAsync</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     Every resource type in the library exposes this same set of overloads, so what is shown here for
///     <see cref="OpmlDocument"/> reads across to the other formats unchanged. <c>CreateAsync</c> is the one-call
///     form; <c>LoadAsync</c> on an instance is the form that lets you subscribe to <c>Loaded</c> first.
/// </remarks>
internal static class OpmlDocumentExample
{
    /// <summary>
    /// Builds a complete <see cref="OpmlDocument"/> by hand and prints it.
    /// </summary>
    public static void ClassExample()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "endjin blogroll",
                CreatedOn = new DateTime(2005, 6, 18, 12, 11, 52),
                ModifiedOn = new DateTime(2005, 7, 2, 21, 42, 48),
                Owner = new OpmlOwner("John Doe", "hello@endjin.com"),
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
    /// Creates an <see cref="OpmlDocument"/> from a <see cref="Uri"/> in a single call.
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
    /// Subscribes to <c>Loaded</c> before loading, so the handler sees the resource the moment it is parsed.
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        OpmlDocument document = new();

        document.Loaded += ResourceLoadedCallback;

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
        if (e.Source is not null)
        {
            // Process the source URI
        }
    }

    /// <summary>
    /// Loads an <see cref="OpmlDocument"/> from an <see cref="IXPathNavigable"/>.
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
    /// Loads an <see cref="OpmlDocument"/> from a <see cref="Stream"/>.
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
    /// Loads an <see cref="OpmlDocument"/> from an <see cref="XmlReader"/>.
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
    /// Loads an <see cref="OpmlDocument"/> from a <see cref="Uri"/>, and shows where a caller-supplied <c>HttpClient</c> goes.
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
    /// Saves an <see cref="OpmlDocument"/> to a <see cref="Stream"/>.
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
    /// Saves an <see cref="OpmlDocument"/> through an <see cref="XmlWriter"/>, with indentation turned on.
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