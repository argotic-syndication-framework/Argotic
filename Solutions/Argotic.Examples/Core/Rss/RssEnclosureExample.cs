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
                Title = "endjin blog",
                Link = new Uri("https://endjin.com"),
                Description = "Technical writing from endjin on .NET, data, analytics and AI"
            }
        };

        RssItem item = new()
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
            Description = "Moving UI framework support out of System.Reactive into separate packages cuts up to 95MB from a self-contained deployment."
        };

        item.Enclosures.Add(new RssEnclosure(24_986_239L, "audio/mpeg", new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check-new-Instrument-same-orchestra.mp3")));

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssEnclosure(item.Enclosures[0]);
    }
}