using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Demonstrates the whole <see cref="RssFeed"/> surface: building a channel by hand, then the <c>Load</c>, <c>LoadAsync</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     Every resource type in the library exposes this same set of overloads, so what is shown here for
///     <see cref="RssFeed"/> reads across to the other formats unchanged. <c>CreateAsync</c> is the one-call
///     form; <c>LoadAsync</c> on an instance is the form that lets you subscribe to <c>Loaded</c> first.
/// </remarks>
internal static class RssFeedExample
{
    /// <summary>
    /// Builds a complete <see cref="RssFeed"/> by hand and prints it.
    /// </summary>
    public static void ClassExample()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "endjin blog",
                Link = new Uri("https://endjin.com"),
                Description = "Technical writing from endjin on .NET, data, analytics and AI"
            }
        };

        feed.Channel.Categories.Add(new RssCategory("Media"));
        feed.Channel.Categories.Add(new RssCategory("News/Newspapers/Regional/United_States/Texas", "dmoz"));

        feed.Channel.Cloud = new RssCloud("endjin.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify");
        feed.Channel.Copyright = "Copyright 2026 endjin limited";
        feed.Channel.Generator = "Microsoft Spaces v1.1";

        RssImage image = new(new Uri("https://endjin.com"), "endjin blog", new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/endjin-logo.png"))
        {
            Description = "Read the endjin blog",
            Height = 32,
            Width = 96
        };
        feed.Channel.Image = image;

        feed.Channel.Language = new CultureInfo("en-US");
        feed.Channel.LastBuildDate = new DateTime(2007, 10, 14, 17, 17, 44);
        feed.Channel.ManagingEditor = "hello@endjin.com (Ian Griffiths)";
        feed.Channel.PublicationDate = new DateTime(2007, 10, 14, 5, 0, 0);
        feed.Channel.Rating = """(PICS-1.1 "http://www.rsac.org/ratingsv01.html" l by "hello@endjin.com" on "2007.01.29T10:09-0800" r (n 0 s 0 v 0 l 0))""";

        feed.Channel.SkipDays.Add(DayOfWeek.Saturday);
        feed.Channel.SkipDays.Add(DayOfWeek.Sunday);

        feed.Channel.SkipHours.Add(0);
        feed.Channel.SkipHours.Add(1);
        feed.Channel.SkipHours.Add(2);
        feed.Channel.SkipHours.Add(22);
        feed.Channel.SkipHours.Add(23);

        feed.Channel.TextInput = new RssTextInput("What software are you using?", new Uri("https://endjin.com/search"), "query", "TextInput Inquiry");
        feed.Channel.TimeToLive = 60;
        feed.Channel.Webmaster = "hello@endjin.com";

        RssItem item = new()
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
            Description = "Moving UI framework support out of System.Reactive into separate packages cuts up to 95MB from a self-contained deployment.",
            Author = "hello@endjin.com (Barry Smart)"
        };

        item.Categories.Add(new RssCategory("sports"));
        item.Categories.Add(new RssCategory("2026/Rx.NET", "rec.sports.baseball"));

        item.Comments = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra#comments");
        item.Enclosures.Add(new RssEnclosure(24_986_239L, "audio/mpeg", new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check-new-Instrument-same-orchestra.mp3")));
        item.Guid = new RssGuid("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra");
        item.PublicationDate = new DateTime(2007, 10, 5, 9, 0, 0);
        item.Source = new RssSource(new Uri("https://endjin.com/rss.xml"), "Los Angeles Herald-Examiner");

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssFeed(feed);
    }

    /// <summary>
    /// Creates an <see cref="RssFeed"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    [RequiresNetwork]
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
    /// Subscribes to <c>Loaded</c> before loading, so the handler sees the resource the moment it is parsed.
    /// </summary>
    [RequiresNetwork]
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
    /// Loads an <see cref="RssFeed"/> from an <see cref="IXPathNavigable"/>.
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
    /// Loads an <see cref="RssFeed"/> from a <see cref="Stream"/>.
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
    /// Loads an <see cref="RssFeed"/> from an <see cref="XmlReader"/>.
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
    /// Loads an <see cref="RssFeed"/> from a <see cref="Uri"/>, and shows where a caller-supplied <see cref="HttpClient"/> goes.
    /// </summary>
    [RequiresNetwork]
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
    /// Saves an <see cref="RssFeed"/> to a <see cref="Stream"/>.
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
    /// Saves an <see cref="RssFeed"/> through an <see cref="XmlWriter"/>, with indentation turned on.
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