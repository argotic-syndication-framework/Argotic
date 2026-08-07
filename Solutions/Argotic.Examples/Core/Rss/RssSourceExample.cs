using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Attributes an item to the feed it was republished from, using <see cref="RssSource"/>.
/// </summary>
internal static class RssSourceExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssSource"/> it holds.
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
            Description = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.",
            Source = new RssSource(new Uri("http://la.example.com/rss.xml"), "Los Angeles Herald-Examiner")
        };

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssSource(item.Source);
    }
}