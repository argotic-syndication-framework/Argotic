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
                Title = "Dallas Times-Herald",
                Link = new Uri("http://dallas.example.com"),
                Description = "Current headlines from the Dallas Times-Herald newspaper"
            }
        };

        RssImage image = new(new Uri("http://dallas.example.com"), "Dallas Times-Herald", new Uri("http://dallas.example.com/masthead.gif"))
        {
            Description = "Read the Dallas Times-Herald",
            Height = 32,
            Width = 96
        };
        feed.Channel.Image = image;

        ExampleOutput.ShowRssImage(image);
    }
}