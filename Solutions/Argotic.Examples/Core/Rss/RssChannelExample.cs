using System.Globalization;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Populates an <see cref="RssChannel"/> with the optional elements RSS 2.0 allows — cloud, image, text input, skip days and skip hours among them.
/// </summary>
internal static class RssChannelExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <c>RssChannel</c> it holds.
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

        ExampleOutput.ShowRssChannel(feed.Channel);
    }
}