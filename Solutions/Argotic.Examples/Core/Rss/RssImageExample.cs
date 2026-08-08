using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Points a channel at its logo with <see cref="RssImage"/>.
/// </summary>
internal static class RssImageExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssImage"/> it holds.
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

        RssImage image = new(new Uri("https://endjin.com"), "endjin blog", new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png"))
        {
            Description = "Read the endjin blog",
            Height = 32,
            Width = 96
        };
        feed.Channel.Image = image;

        ExampleOutput.ShowRssImage(image);
    }
}