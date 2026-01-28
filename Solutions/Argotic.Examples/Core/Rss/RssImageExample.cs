using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Contains the code examples for the <see cref="RssImage"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RssImage"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class RssImageExample
{
    /// <summary>
    /// Provides example code for the RssImage class.
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

        RssImage image = new(new Uri("http://dallas.example.com"), "Dallas Times-Herald", new Uri("http://dallas.example.com/masthead.gif"))
        {
            Description = "Read the Dallas Times-Herald",
            Height = 32,
            Width = 96
        };
        feed.Channel.Image = image;

        ExampleOutput.ShowRssImage(image);
    }
}