using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Attaches a media file to an item with <see cref="RssEnclosure"/>.
/// </summary>
internal static class RssEnclosureExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssEnclosure"/> it holds.
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

        RssItem item = new()
        {
            Title = "Seventh Heaven! Ryan Hurls Another No Hitter",
            Link = new Uri("http://dallas.example.com/1991/05/02/nolan.htm"),
            Description = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0."
        };

        item.Enclosures.Add(new RssEnclosure(24_986_239L, "audio/mpeg", new Uri("http://dallas.example.com/joebob_050689.mp3")));

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssEnclosure(item.Enclosures[0]);
    }
}