using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Contains the code examples for the <see cref="AtomEntry"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="AtomEntry"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class AtomEntryExample
{
    /// <summary>
    /// Provides example code for the AtomEntry class.
    /// </summary>
    public static void ClassExample()
    {
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Atom Entry Document"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2)
        };

        entry.Authors.Add(new AtomPersonConstruct("John Doe"));
        entry.Links.Add(new AtomLink(new Uri("/blog/1234"), "alternate"));
        entry.Summary = new AtomTextConstruct("A stand-alone Atom Entry Document.");

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Provides example code for the AtomEntry.CreateAsync(Uri) method
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        AtomEntry entry = await AtomEntry.CreateAsync(new Uri("https://endjin.com/atom.xml")).ConfigureAwait(false);

        if (entry.PublishedOn >= DateTime.Today)
        {
            //  Perform some processing on the entry
        }

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri) method with event notification
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        AtomEntry entry = new();

        entry.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(EntryLoadedCallback);

        await entry.LoadAsync(new Uri("https://endjin.com/atom.xml")).ConfigureAwait(false);

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Handles the <see cref="AtomFeed.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void EntryLoadedCallback(object? sender, SyndicationResourceLoadedEventArgs e)
    {
        // Process the loaded entry using e.Data or e.Source
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
        using XmlReader xmlReader = XmlReader.Create(SampleDataPath.AtomEntryDocument.FullPath, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        AtomEntry entry = new();
        entry.Load(source);

        if (entry.UpdatedOn >= DateTime.Today)
        {
            //  Perform some processing on the entry
        }

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomEntry entry = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomEntryDocument);
        entry.Load(stream);

        if (entry.UpdatedOn >= DateTime.Today)
        {
            //  Perform some processing on the entry
        }

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        AtomEntry entry = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomEntryDocument);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        entry.Load(reader);

        if (entry.UpdatedOn >= DateTime.Today)
        {
            //  Perform some processing on the entry
        }

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        AtomEntry entry = new();
        Uri source = new("https://endjin.com/atom.xml");

        // For simple case (no credentials):
        await entry.LoadAsync(source).ConfigureAwait(false);

        // Or for credentials:
        // var handler = new SocketsHttpHandler { Credentials = CredentialCache.DefaultNetworkCredentials };
        // using var httpClient = new HttpClient(handler);
        // await entry.LoadAsync(source, httpClient);

        if (entry.UpdatedOn >= DateTime.Today)
        {
            //  Perform some processing on the entry
        }

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        AtomEntry entry = new();

        //  Modify entry state using public properties and methods

        using Stream stream = new MemoryStream();
        entry.Save(stream);

        ExampleOutput.ShowSaved("AtomEntry");
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        AtomEntry entry = new();

        //  Modify entry state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        entry.Save(writer);

        ExampleOutput.ShowSaved("AtomEntry");
    }
}