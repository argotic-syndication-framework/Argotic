using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Builds an <see cref="RssItem"/> carrying every optional element RSS 2.0 defines for one, and adds it to a channel.
/// </summary>
internal static class RssItemExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssItem"/> it holds.
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
            Author = "hello@endjin.com (Barry Smart)"
        };

        item.Categories.Add(new RssCategory("sports"));
        item.Categories.Add(new RssCategory("2026/Rx.NET", "rec.sports.baseball"));

        item.Comments = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra#comments");
        item.Enclosures.Add(new RssEnclosure(24_986_239L, "audio/mpeg", new Uri("https://endjincdn.blob.core.windows.net/assets/podcast/2026-05-14-the-genai-reality-check-new-Instrument-same-orchestra.mp3")));
        item.Guid = new RssGuid("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra");
        item.PublicationDate = new DateTime(2007, 10, 5, 9, 0, 0);
        item.Source = new RssSource(new Uri("https://endjin.com/rss.xml"), "Los Angeles Herald-Examiner");

        feed.Channel.Items.Add(item);

        ExampleOutput.ShowRssItem(item);
    }
}