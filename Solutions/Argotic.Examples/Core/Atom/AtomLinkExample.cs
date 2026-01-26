using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="AtomLink"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="AtomLink"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class AtomLinkExample
{
    /// <summary>
    /// Provides example code for the AtomLink class.
    /// </summary>
    public static void ClassExample()
    {
        AtomFeed feed = new()
        {
            Id = new(new("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new("Example Feed"),
            UpdatedOn = new(2003, 12, 13, 18, 30, 2)
        };

        feed.Links.Add(new(new("http://example.org/")));

        //  Identify a related web resource for the feed
        feed.Links.Add(new(new("/feed"), "self"));

        feed.Authors.Add(new("John Doe"));

        AtomEntry entry = new()
        {
            Id = new(new("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new("Atom-Powered Robots Run Amok"),
            UpdatedOn = new(2003, 12, 13, 18, 30, 2),
            Summary = new("Some text.")
        };

        //  Identify a related web resource for the entry
        entry.Links.Add(new(new("/blog/1234"), "alternate"));

        feed.AddEntry(entry);
    }
}