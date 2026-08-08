using Argotic.Syndication;

namespace Argotic.Examples.Core.Opml;

/// <summary>
/// Builds the nested <see cref="OpmlOutline"/> tree that is the whole of an OPML document's body.
/// </summary>
internal static class OpmlOutlineExample
{
    /// <summary>
    /// Builds the containing <see cref="OpmlDocument"/> and prints the <see cref="OpmlOutline"/> it holds.
    /// </summary>
    public static void ClassExample()
    {
        OpmlDocument document = new()
        {
            Head =
            {
                Title = "endjin blogroll",
                CreatedOn = new DateTime(2005, 6, 18, 12, 11, 52),
                ModifiedOn = new DateTime(2005, 7, 2, 21, 42, 48),
                Owner = new OpmlOwner("John Doe", "hello@endjin.com"),
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