using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.Rsd;

/// <summary>
/// Contains the code examples for the <see cref="RsdDocument"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RsdDocument"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class RsdDocumentExample
{
    /// <summary>
    /// Provides example code for the RsdDocument class.
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
    /// Provides example code for the RsdDocument.CreateAsync(Uri) method
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
    /// Provides example code for the LoadAsync(Uri) method with event notification
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        RsdDocument document = new();

        document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

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
    /// Provides example code for the Load(IXPathNavigable) method
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
    /// Provides example code for the Load(Stream) method
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
    /// Provides example code for the Load(XmlReader) method
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
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
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
    /// Provides example code for the Save(Stream) method
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
    /// Provides example code for the Save(XmlWriter) method
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