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
                Title = "Dallas Times-Herald",
                Link = new Uri("http://dallas.example.com"),
                Description = "Current headlines from the Dallas Times-Herald newspaper",
                TextInput = new RssTextInput("What software are you using?", new Uri("https://example.com/search"), "query", "TextInput Inquiry")
            }
        };

        ExampleOutput.ShowRssTextInput(feed.Channel.TextInput);
    }
}