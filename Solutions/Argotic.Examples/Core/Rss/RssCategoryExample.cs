using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Categorises a channel and an item with <see cref="RssCategory"/>, with and without a taxonomy domain.
/// </summary>
internal static class RssCategoryExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssCategory"/> it holds.
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

        RssItem item = new()
        {
            Title = "Seventh Heaven! Ryan Hurls Another No Hitter",
            Link = new Uri("http://dallas.example.com/1991/05/02/nolan.htm"),
            Description = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.",
            Author = "jbb@dallas.example.com (Joe Bob Briggs)"
        };

        item.Categories.Add(new RssCategory("sports"));
        item.Categories.Add(new RssCategory("1991/Texas Rangers", "rec.sports.baseball"));

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssCategory(feed.Channel.Categories[0]);
    }
}