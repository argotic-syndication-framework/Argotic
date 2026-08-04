using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Contains the code examples for the <see cref="RssFeed"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RssFeed"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class RssFeedExample
{
    /// <summary>
    /// Provides example code for the RssFeed class.
    /// </summary>
    public static void ClassExample()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "Dallas Times-Herald",
                Link = new Uri("http://dallas.example.com"),
                Description = "Current headlines from the Dallas Times-Herald newspaper"
            }
        };

        feed.Channel.Categories.Add(new RssCategory("Media"));
        feed.Channel.Categories.Add(new RssCategory("News/Newspapers/Regional/United_States/Texas", "dmoz"));

        feed.Channel.Cloud = new RssCloud("server.example.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify");
        feed.Channel.Copyright = "Copyright 2007 Dallas Times-Herald";
        feed.Channel.Generator = "Microsoft Spaces v1.1";

        RssImage image = new(new Uri("http://dallas.example.com"), "Dallas Times-Herald", new Uri("http://dallas.example.com/masthead.gif"))
        {
            Description = "Read the Dallas Times-Herald",
            Height = 32,
            Width = 96
        };
        feed.Channel.Image = image;

        feed.Channel.Language = new CultureInfo("en-US");
        feed.Channel.LastBuildDate = new DateTime(2007, 10, 14, 17, 17, 44);
        feed.Channel.ManagingEditor = "jlehrer@dallas.example.com (Jim Lehrer)";
        feed.Channel.PublicationDate = new DateTime(2007, 10, 14, 5, 0, 0);
        feed.Channel.Rating = """(PICS-1.1 "http://www.rsac.org/ratingsv01.html" l by "webmaster@example.com" on "2007.01.29T10:09-0800" r (n 0 s 0 v 0 l 0))""";

        feed.Channel.SkipDays.Add(DayOfWeek.Saturday);
        feed.Channel.SkipDays.Add(DayOfWeek.Sunday);

        feed.Channel.SkipHours.Add(0);
        feed.Channel.SkipHours.Add(1);
        feed.Channel.SkipHours.Add(2);
        feed.Channel.SkipHours.Add(22);
        feed.Channel.SkipHours.Add(23);

        feed.Channel.TextInput = new RssTextInput("What software are you using?", new Uri("https://example.com/search"), "query", "TextInput Inquiry");
        feed.Channel.TimeToLive = 60;
        feed.Channel.Webmaster = "helpdesk@dallas.example.com";

        RssItem item = new()
        {
            Title = "Seventh Heaven! Ryan Hurls Another No Hitter",
            Link = new Uri("http://dallas.example.com/1991/05/02/nolan.htm"),
            Description = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.",
            Author = "jbb@dallas.example.com (Joe Bob Briggs)"
        };

        item.Categories.Add(new RssCategory("sports"));
        item.Categories.Add(new RssCategory("1991/Texas Rangers", "rec.sports.baseball"));

        item.Comments = new Uri("http://dallas.example.com/feedback/1983/06/joebob.htm");
        item.Enclosures.Add(new RssEnclosure(24_986_239L, "audio/mpeg", new Uri("http://dallas.example.com/joebob_050689.mp3")));
        item.Guid = new RssGuid("http://dallas.example.com/1983/05/06/joebob.htm");
        item.PublicationDate = new DateTime(2007, 10, 5, 9, 0, 0);
        item.Source = new RssSource(new Uri("http://la.example.com/rss.xml"), "Los Angeles Herald-Examiner");

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssFeed(feed);
    }

    /// <summary>
    /// Provides example code for the RssFeed.CreateAsync(Uri) method
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        RssFeed feed = await RssFeed.CreateAsync(new Uri("https://endjin.com/rss.xml")).ConfigureAwait(false);

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process channel items published in the last week
            }
        }
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri) method with event notification
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        RssFeed feed = new();

        feed.Loaded += FeedLoadedCallback;

        await feed.LoadAsync(new Uri("https://endjin.com/rss.xml")).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the <see cref="RssFeed.Loaded"/> event.
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
    /// Provides example code for the Load(IXPathNavigable) method
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        using XmlReader xmlReader = XmlReader.Create("https://endjin.com/rss.xml", SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        RssFeed feed = new();
        feed.Load(source);

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process channel items published in the last week
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        RssFeed feed = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RssFeed);
        feed.Load(stream);

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process channel items published in the last week
            }
        }

        ExampleOutput.ShowRssFeed(feed);
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        RssFeed feed = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.RssFeed);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        feed.Load(reader);

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process channel items published in the last week
            }
        }
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        RssFeed feed = new();
        Uri source = new("https://endjin.com/rss.xml");

        // For simple case (no credentials):
        await feed.LoadAsync(source).ConfigureAwait(false);

        // Or for credentials:
        // var handler = new SocketsHttpHandler { Credentials = CredentialCache.DefaultNetworkCredentials };
        // using var httpClient = new HttpClient(handler);
        // await feed.LoadAsync(source, httpClient);

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process channel items published in the last week
            }
        }
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        RssFeed feed = new();

        //  Modify feed state using public properties and methods

        using Stream stream = new MemoryStream();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        RssFeed feed = new();

        //  Modify feed state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        feed.Save(writer);

        ExampleOutput.ShowSaved("RssFeed");
    }
}