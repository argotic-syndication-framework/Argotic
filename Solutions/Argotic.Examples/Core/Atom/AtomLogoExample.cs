using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Points a feed at its logo with <see cref="AtomLogo"/>.
/// </summary>
internal static class AtomLogoExample
{
    /// <summary>
    /// Builds the containing <see cref="AtomFeed"/> and prints the <see cref="AtomLogo"/> it holds.
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

        //  Provide visual identification for the feed
        AtomLogo logo = new(new Uri("/logo.jpg"));
        feed.Logo = logo;

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Atom-Powered Robots Run Amok"),
            UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2),
            Summary = new AtomTextConstruct("Some text.")
        };

        feed.Entries.Add(entry);

        ExampleOutput.ShowAtomLogo(logo);
    }
}