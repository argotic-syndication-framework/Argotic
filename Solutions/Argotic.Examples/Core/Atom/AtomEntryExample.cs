using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Demonstrates the whole <see cref="AtomEntry"/> surface: building one by hand, then the <c>Load</c>, <c>LoadAsync</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     The <see cref="Uri"/> overloads take a stand-alone entry document — <c>&lt;entry&gt;</c> as the
///     document element, not <c>&lt;feed&gt;</c> — which essentially nothing on the open web serves, so
///     those examples are pointed at a loopback <see cref="SampleHost"/> rather than a live origin.
/// </remarks>
internal static class AtomEntryExample
{
    /// <summary>
    /// Builds a complete <see cref="AtomEntry"/> by hand and prints it.
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
    /// Creates an <see cref="AtomEntry"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        //  The Uri overloads take an RFC 4287 §2 stand-alone entry document: <entry> as the document
        //  element. That is not a feed -- endjin.com/atom.xml is a <feed>, and pointing this at it
        //  used to return an entry with an empty title and a DateTime.MinValue timestamp, reporting
        //  success. It now raises FormatException. Stand-alone entry documents are Atom Publishing
        //  Protocol member resources and are not served on the open web, so this one is served from
        //  SampleData over loopback -- HttpClient has no file scheme, and the Uri overload is the point.
        using SampleHost host = SampleHost.Serving(SampleDataPath.AtomEntryDocument, "application/atom+xml");

        AtomEntry entry = await AtomEntry.CreateAsync(host.Uri).ConfigureAwait(false);

        if (entry.PublishedOn >= DateTime.Today)
        {
            //  Perform some processing on the entry
        }

        ExampleOutput.ShowAtomEntry(entry);
    }

    /// <summary>
    /// Subscribes to <c>Loaded</c> before loading, so the handler sees the resource the moment it is parsed.
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        //  The Uri overloads take an RFC 4287 §2 stand-alone entry document: <entry> as the document
        //  element. That is not a feed -- endjin.com/atom.xml is a <feed>, and pointing this at it
        //  used to return an entry with an empty title and a DateTime.MinValue timestamp, reporting
        //  success. It now raises FormatException. Stand-alone entry documents are Atom Publishing
        //  Protocol member resources and are not served on the open web, so this one is served from
        //  SampleData over loopback -- HttpClient has no file scheme, and the Uri overload is the point.
        using SampleHost host = SampleHost.Serving(SampleDataPath.AtomEntryDocument, "application/atom+xml");

        AtomEntry entry = new();

        entry.Loaded += EntryLoadedCallback;

        await entry.LoadAsync(host.Uri).ConfigureAwait(false);

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
    /// Loads an <see cref="AtomEntry"/> from an <see cref="IXPathNavigable"/>.
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
    /// Loads an <see cref="AtomEntry"/> from a <see cref="Stream"/>.
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
    /// Loads an <see cref="AtomEntry"/> from an <see cref="XmlReader"/>.
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
    /// Loads an <see cref="AtomEntry"/> from a <see cref="Uri"/>, and shows where a caller-supplied <see cref="HttpClient"/> goes.
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        AtomEntry entry = new();
        //  The Uri overloads take an RFC 4287 §2 stand-alone entry document: <entry> as the document
        //  element. That is not a feed -- endjin.com/atom.xml is a <feed>, and pointing this at it
        //  used to return an entry with an empty title and a DateTime.MinValue timestamp, reporting
        //  success. It now raises FormatException. Stand-alone entry documents are Atom Publishing
        //  Protocol member resources and are not served on the open web, so this one is served from
        //  SampleData over loopback -- HttpClient has no file scheme, and the Uri overload is the point.
        using SampleHost host = SampleHost.Serving(SampleDataPath.AtomEntryDocument, "application/atom+xml");

        Uri source = host.Uri;

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
    /// Saves an <see cref="AtomEntry"/> to a <see cref="Stream"/>.
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
    /// Saves an <see cref="AtomEntry"/> through an <see cref="XmlWriter"/>, with indentation turned on.
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