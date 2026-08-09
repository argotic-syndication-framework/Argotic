using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Categorises an Atom feed and one of its entries with <see cref="AtomCategory"/>, including the scheme that gives a term its meaning.
/// </summary>
internal static class AtomCategoryExample
{
    /// <summary>
    /// Builds the containing <see cref="AtomFeed"/> and prints the <see cref="AtomCategory"/> it holds.
    /// </summary>
    public static void ClassExample()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("Example Feed"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2)
        };

        feed.Links.Add(new AtomLink(new Uri("https://endjin.com/")));
        feed.Links.Add(new AtomLink(new Uri("/feed"), "self"));

        feed.Authors.Add(new AtomPersonConstruct("John Doe"));

        // Categorize the feed
        feed.Categories.Add(new AtomCategory("sports"));

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Atom-Powered Robots Run Amok"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2),
            Summary = new AtomTextConstruct("Some text.")
        };

        //  Categorize the feed entry
        AtomCategory entryCategory = new()
        {
            Label = "Baseball",
            Scheme = new Uri("https://endjin.com/tags"),
            Term = "baseball"
        };

        entry.Categories.Add(entryCategory);

        feed.Entries.Add(entry);

        ExampleOutput.ShowAtomCategory(entryCategory);
    }
}