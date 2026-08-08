using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Supplies the human-readable titles and summaries of a feed and its entries with <see cref="AtomTextConstruct"/>.
/// </summary>
internal static class AtomTextConstructExample
{
    /// <summary>
    /// Builds the containing <see cref="AtomFeed"/> and prints the <see cref="AtomTextConstruct"/> it holds.
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

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Atom-Powered Robots Run Amok"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2)
        };

        //  Provide summary as entity escaped html
        AtomTextConstruct summary = new()
        {
            Content = "Rx.NET 7.0 &lt;b&gt;saves 95MB&lt;/b&gt;!",
            TextType = AtomTextConstructType.Html
        };
        entry.Summary = summary;

        feed.Entries.Add(entry);

        ExampleOutput.ShowAtomTextConstruct(summary);
    }
}