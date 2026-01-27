using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Contains the code examples for the <see cref="AtomFeed"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="AtomFeed"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class AtomFeedExample
{
    /// <summary>
    /// Provides example code for the AtomFeed class.
    /// </summary>
    public static void ClassExample()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("Example Feed"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2)
        };

        feed.Links.Add(new AtomLink(new Uri("http://example.org/")));
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
    }
    /// <summary>
    /// Provides example code for the AtomFeed.CreateAsync(Uri) method
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        AtomFeed feed = await AtomFeed.CreateAsync(new Uri("http://news.google.com/?output=atom")).ConfigureAwait(false);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }
    }
    /// <summary>
    /// Provides example code for the LoadAsync(Uri) method with event notification
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        AtomFeed feed = new();

        feed.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(FeedLoadedCallback);

        await feed.LoadAsync(new Uri("http://news.google.com/?output=atom")).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the <see cref="AtomFeed.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void FeedLoadedCallback(object? sender, SyndicationResourceLoadedEventArgs e)
    {
        // Process the loaded feed using e.Data or e.Source
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
        using XmlReader xmlReader = XmlReader.Create("http://news.google.com/?output=atom", SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
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
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        AtomFeed feed = new();

        using Stream stream = new FileStream("AtomFeed.xml", FileMode.Open, FileAccess.Read);
        feed.Load(stream);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the feed entry
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        AtomFeed feed = new();

        using Stream stream = new FileStream("AtomFeed.xml", FileMode.Open, FileAccess.Read);
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
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        AtomFeed feed = new();
        Uri source = new("http://news.google.com/?output=atom");

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
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        AtomFeed feed = new();

        //  Modify feed state using public properties and methods

        using Stream stream = new FileStream("AtomFeed.xml", FileMode.Create, FileAccess.Write);
        feed.Save(stream);
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        AtomFeed feed = new();

        //  Modify feed state using public properties and methods

        using Stream stream = new FileStream("AtomFeed.xml", FileMode.Create, FileAccess.Write);
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        feed.Save(writer);
    }
}