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
                Title = "endjin blog",
                Link = new Uri("https://endjin.com"),
                Description = "Technical writing from endjin on .NET, data, analytics and AI"
            }
        };

        RssItem item = new()
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
            Description = "Moving UI framework support out of System.Reactive into separate packages cuts up to 95MB from a self-contained deployment.",
            Source = new RssSource(new Uri("https://endjin.com/rss.xml"), "Los Angeles Herald-Examiner")
        };

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssSource(item.Source);
    }
}