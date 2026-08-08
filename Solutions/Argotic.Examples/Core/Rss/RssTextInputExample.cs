using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Adds an <see cref="RssTextInput"/> form to a channel.
/// </summary>
internal static class RssTextInputExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssTextInput"/> it holds.
    /// </summary>
    public static void ClassExample()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "endjin blog",
                Link = new Uri("https://endjin.com"),
                Description = "Technical writing from endjin on .NET, data, analytics and AI",
                TextInput = new RssTextInput("What software are you using?", new Uri("https://endjin.com/search"), "query", "TextInput Inquiry")
            }
        };

        ExampleOutput.ShowRssTextInput(feed.Channel.TextInput);
    }
}