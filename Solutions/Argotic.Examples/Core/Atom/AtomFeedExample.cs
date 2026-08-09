using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Demonstrates the whole <see cref="AtomFeed"/> surface: building one by hand, then the <c>Load</c>, <c>LoadAsync</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     Every resource type in the library exposes this same set of overloads, so what is shown here for
///     <see cref="AtomFeed"/> reads across to the other formats unchanged. <c>CreateAsync</c> is the one-call
///     form; <c>LoadAsync</c> on an instance is the form that lets you subscribe to <c>Loaded</c> first.
/// </remarks>
internal static class AtomFeedExample
{
    /// <summary>
    /// Builds a complete <see cref="AtomFeed"/> by hand and prints it.
    /// </summary>
    public static void ClassExample()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("Example Feed"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2)
        };

        feed.Links.Add(new AtomLink(new Uri("https://endjin.com/")));
        feed.Links.Add(new AtomLink(new Uri("/feed"), "self"));

        feed.Authors.Add(new AtomPersonConstruct("John Doe"));

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Atom-Powered Robots Run Amok"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2),
            Summary = new AtomTextConstruct("Some text.")
        };

        feed.Entries.Add(entry);

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Creates an <see cref="AtomFeed"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    [RequiresNetwork]
    public static async Task CreateExampleAsync()
    {
        AtomFeed feed = await AtomFeed.CreateAsync(new Uri("https://endjin.com/atom.xml")).ConfigureAwait(false);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Subscribes to <c>Loaded</c> before loading, so the handler sees the resource the moment it is parsed.
    /// </summary>
    [RequiresNetwork]
    public static async Task LoadAsyncExampleAsync()
    {
        AtomFeed feed = new();

        feed.Loaded += FeedLoadedCallback;

        await feed.LoadAsync(new Uri("https://endjin.com/atom.xml")).ConfigureAwait(false);

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Handles the <see cref="AtomFeed.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void FeedLoadedCallback(object? sender, SyndicationResourceLoadedEventArgs e)
    {
        // Process the loaded feed using e.Data or e.Source
        if (e.Source is not null)
        {
            // Process the source URI
        }
    }

    /// <summary>
    /// Loads an <see cref="AtomFeed"/> from an <see cref="IXPathNavigable"/>.
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        using XmlReader xmlReader = XmlReader.Create(SampleDataPath.AtomFeed.FullPath, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        AtomFeed feed = new();
        feed.Load(source);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Loads an <see cref="AtomFeed"/> from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomFeed feed = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomFeed);
        feed.Load(stream);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Loads an <see cref="AtomFeed"/> from an <see cref="XmlReader"/>.
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        AtomFeed feed = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomFeed);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        feed.Load(reader);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Loads an <see cref="AtomFeed"/> from a <see cref="Uri"/>, and shows where a caller-supplied <see cref="HttpClient"/> goes.
    /// </summary>
    [RequiresNetwork]
    public static async Task LoadUriExampleAsync()
    {
        AtomFeed feed = new();
        Uri source = new("https://endjin.com/atom.xml");

        // For simple case (no credentials):
        await feed.LoadAsync(source).ConfigureAwait(false);

        // Or for credentials:
        // var handler = new SocketsHttpHandler { Credentials = CredentialCache.DefaultNetworkCredentials };
        // using var httpClient = new HttpClient(handler);
        // await feed.LoadAsync(source, httpClient);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }

        ExampleOutput.ShowAtomFeed(feed);
    }

    /// <summary>
    /// Saves an <see cref="AtomFeed"/> to a <see cref="Stream"/>.
    /// </summary>
    public static void SaveStreamExample()
    {
        AtomFeed feed = new();

        //  Modify feed state using public properties and methods

        using Stream stream = new MemoryStream();
        feed.Save(stream);

        ExampleOutput.ShowSaved("AtomFeed");
    }

    /// <summary>
    /// Saves an <see cref="AtomFeed"/> through an <see cref="XmlWriter"/>, with indentation turned on.
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        AtomFeed feed = new();

        //  Modify feed state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        feed.Save(writer);

        ExampleOutput.ShowSaved("AtomFeed");
    }
}