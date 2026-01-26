using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="RssItem"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="RssItem"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class RssItemExample
{
    /// <summary>
    /// Provides example code for the RssItem class.
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
            Author = "jbb@dallas.example.com (Joe Bob Briggs)"
        };

        item.Categories.Add(new("sports"));
        item.Categories.Add(new("1991/Texas Rangers", "rec.sports.baseball"));

        item.Comments = new("http://dallas.example.com/feedback/1983/06/joebob.htm");
        item.Enclosures.Add(new(24986239L, "audio/mpeg", new("http://dallas.example.com/joebob_050689.mp3")));
        item.Guid = new("http://dallas.example.com/1983/05/06/joebob.htm");
        item.PublicationDate = new(2007, 10, 5, 9, 0, 0);
        item.Source = new(new("http://la.example.com/rss.xml"), "Los Angeles Herald-Examiner");

        feed.Channel.AddItem(item);
    }
}