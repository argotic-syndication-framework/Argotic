using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Contains the code examples for the <see cref="RssTextInput"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RssTextInput"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class RssTextInputExample
{
    /// <summary>
    /// Provides example code for the RssTextInput class.
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