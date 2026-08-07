using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Carries an entry's body inline with <see cref="AtomContent"/>, including the XHTML content type.
/// </summary>
internal static class AtomContentExample
{
    /// <summary>
    /// Builds the containing <see cref="AtomFeed"/> and prints the <see cref="AtomContent"/> it holds.
    /// </summary>
    public static void ClassExample()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("Example Feed"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2)
        };

        feed.Links.Add(new AtomLink(new Uri("http://example.org/")));
        feed.Links.Add(new AtomLink(new Uri("/feed"), "self"));

        feed.Authors.Add(new AtomPersonConstruct("John Doe"));

        //  Define the complete content of the entry
        AtomContent content = new("Powered by <b>Argotic</b>!", "xhtml");

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Atom-Powered Robots Run Amok"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2),
            Summary = new AtomTextConstruct("Some text."),
            Content = content
        };

        feed.Entries.Add(entry);

        ExampleOutput.ShowAtomContent(content);
    }
}