using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.Rsd;

/// <summary>
/// Demonstrates the whole <see cref="RsdDocument"/> surface: describing a blog's APIs by hand, then the <c>Load</c>, <c>LoadAsync</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     Every resource type in the library exposes this same set of overloads, so what is shown here for
///     <see cref="RsdDocument"/> reads across to the other formats unchanged. <c>CreateAsync</c> is the one-call
///     form; <c>LoadAsync</c> on an instance is the form that lets you subscribe to <c>Loaded</c> first.
/// </remarks>
internal static class RsdDocumentExample
{
    /// <summary>
    /// Builds a complete <see cref="RsdDocument"/> by hand and prints it.
    /// </summary>
    public static void ClassExample()
    {
        RsdDocument document = new()
        {
            EngineName = "Blog Munging CMS",
            EngineLink = new Uri("http://www.blogmunging.com/"),
            Homepage = new Uri("http://www.userdomain.com/")
        };

        document.Interfaces.Add(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
        document.Interfaces.Add(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
        document.Interfaces.Add(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
        document.Interfaces.Add(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

        RsdApplicationInterface conversantApi = new("Conversant", new Uri("http://example.com/xml/rpc/url"), false, string.Empty)
        {
            Documentation = new Uri("http://www.conversant.com/docs/api/"),
            Notes = "Additional explanation here."
        };
        conversantApi.Settings.Add("service-specific-setting", "a value");
        conversantApi.Settings.Add("another-setting", "another value");
        document.Interfaces.Add(conversantApi);
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Creates an <see cref="RsdDocument"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RsdDocument);
        RsdDocument document = new();
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (RsdApplicationInterface api in document.Interfaces)
        {
            if (api.IsPreferred)
            {
                //  Perform some processing on the application programming interface
                break;
            }
        }
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Subscribes to <c>Loaded</c> before loading, so the handler sees the resource the moment it is parsed.
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        RsdDocument document = new();

        document.Loaded += ResourceLoadedCallback;

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RsdDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Handles the <see cref="RsdDocument.Loaded"/> event.
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
    /// Loads an <see cref="RsdDocument"/> from an <see cref="IXPathNavigable"/>.
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        using XmlReader xmlReader = XmlReader.Create(SampleDataPath.RsdDocument.FullPath, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        RsdDocument document = new();
        document.Load(source);

        foreach (RsdApplicationInterface api in document.Interfaces)
        {
            if (api.IsPreferred)
            {
                //  Perform some processing on the application programming interface
                break;
            }
        }
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Loads an <see cref="RsdDocument"/> from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        RsdDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RsdDocument);
        document.Load(stream);

        foreach (RsdApplicationInterface api in document.Interfaces)
        {
            if (api.IsPreferred)
            {
                //  Perform some processing on the application programming interface
                break;
            }
        }
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Loads an <see cref="RsdDocument"/> from an <see cref="XmlReader"/>.
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        RsdDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RsdDocument);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        document.Load(reader);

        foreach (RsdApplicationInterface api in document.Interfaces)
        {
            if (api.IsPreferred)
            {
                //  Perform some processing on the application programming interface
                break;
            }
        }
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Loads an <see cref="RsdDocument"/> from a <see cref="Uri"/>, and shows where a caller-supplied <c>HttpClient</c> goes.
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        RsdDocument document = new();

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RsdDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (RsdApplicationInterface api in document.Interfaces)
        {
            if (api.IsPreferred)
            {
                //  Perform some processing on the application programming interface
                break;
            }
        }
        ExampleOutput.ShowRsdDocument(document);
    }

    /// <summary>
    /// Saves an <see cref="RsdDocument"/> to a <see cref="Stream"/>.
    /// </summary>
    public static void SaveStreamExample()
    {
        RsdDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        document.Save(stream);
        ExampleOutput.ShowSaved("RsdDocument");
    }

    /// <summary>
    /// Saves an <see cref="RsdDocument"/> through an <see cref="XmlWriter"/>, with indentation turned on.
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        RsdDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        document.Save(writer);
        ExampleOutput.ShowSaved("RsdDocument");
    }
}