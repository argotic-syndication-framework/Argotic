using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Preserves the metadata of the feed an entry was copied from, using <see cref="AtomSource"/>.
/// </summary>
internal static class AtomSourceExample
{
    /// <summary>
    /// Builds the containing <see cref="AtomFeed"/> and prints the <see cref="AtomSource"/> it holds.
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
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2),
            Summary = new AtomTextConstruct("Some text.")
        };

        //  Entry was copied from another feed, so preserve source meta-data
        AtomSource source = new()
        {
            Id = new AtomId(new Uri("http://example2.org/")),
            Title = new AtomTextConstruct("Fourty-Two"),
            UpdatedOn = new DateTime(2003, 11, 13, 18, 30, 2),
            Rights = new AtomTextConstruct("© 2026 endjin limited")
        };
        entry.Source = source;

        feed.Entries.Add(entry);

        ExampleOutput.ShowAtomSource(source);
    }
}