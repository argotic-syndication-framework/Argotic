using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="OpmlOutline"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="OpmlOutline"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
public static class OpmlOutlineExample
{
    /// <summary>
    /// Provides example code for the OpmlOutline class.
    /// </summary>
    public static void ClassExample()
    {
        OpmlDocument document   = new OpmlDocument
        {
            Head =
            {
                Title = "Example OPML List",
                CreatedOn = new DateTime(2005, 6, 18, 12, 11, 52),
                ModifiedOn = new DateTime(2005, 7, 2, 21, 42, 48),
                Owner = new OpmlOwner("John Doe", "john.doe@example.com"),
                VerticalScrollState = 1,
                Window = new OpmlWindow(61, 304, 562, 842)
            }
        };

        // Create outline that contains child outlines
        OpmlOutline containerOutline    = new OpmlOutline("Feeds");
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Argotic", "rss", new Uri("http://www.codeplex.com/Argotic/Project/ProjectRss.aspx")));
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Google News", "feed", new Uri("http://news.google.com/?output=atom")));
        document.AddOutline(containerOutline);
    }
}