using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Gives an item a permanent identifier with <see cref="RssGuid"/>.
/// </summary>
internal static class RssGuidExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssGuid"/> it holds.
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
            Guid = new RssGuid("http://dallas.example.com/1983/05/06/joebob.htm")
        };

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssGuid(item.Guid);
    }
}