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
                Title = "endjin blog",
                Link = new Uri("https://endjin.com"),
                Description = "Technical writing from endjin on .NET, data, analytics and AI"
            }
        };

        feed.Channel.Categories.Add(new RssCategory("Media"));
        feed.Channel.Categories.Add(new RssCategory("News/Newspapers/Regional/United_States/Texas", "dmoz"));

        RssItem item = new()
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
            Description = "Moving UI framework support out of System.Reactive into separate packages cuts up to 95MB from a self-contained deployment.",
            Author = "hello@endjin.com (Barry Smart)"
        };

        item.Categories.Add(new RssCategory("sports"));
        item.Categories.Add(new RssCategory("2026/Rx.NET", "rec.sports.baseball"));

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssCategory(feed.Channel.Categories[0]);
    }
}