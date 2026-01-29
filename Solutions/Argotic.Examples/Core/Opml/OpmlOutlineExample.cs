using Argotic.Syndication;

namespace Argotic.Examples.Core.Opml;

/// <summary>
/// Contains the code examples for the <see cref="OpmlOutline"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="OpmlOutline"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class OpmlOutlineExample
{
    /// <summary>
    /// Provides example code for the OpmlOutline class.
    /// </summary>
    public static void ClassExample()
    {
        OpmlDocument document = new()
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
        OpmlOutline containerOutline = new("Feeds");
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("endjin", "rss", new Uri("https://endjin.com/atom.xml")));
        containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("endjin", "feed", new Uri("https://endjin.com/rss.xml")));
        document.Outlines.Add(containerOutline);

        ExampleOutput.ShowOpmlOutline(containerOutline);
    }
}