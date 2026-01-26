using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="RssGuid"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RssGuid"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class RssGuidExample
{
    /// <summary>
    /// Provides example code for the RssGuid class.
    /// </summary>
    public static void ClassExample()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "Dallas Times-Herald",
                Link = new("http://dallas.example.com"),
                Description = "Current headlines from the Dallas Times-Herald newspaper"
            }
        };

        RssItem item = new()
        {
            Title = "Seventh Heaven! Ryan Hurls Another No Hitter",
            Link = new("http://dallas.example.com/1991/05/02/nolan.htm"),
            Description = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.",
            Guid = new("http://dallas.example.com/1983/05/06/joebob.htm")
        };

        feed.Channel.AddItem(item);
    }
}