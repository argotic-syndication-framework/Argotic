using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="AtomPersonConstruct"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="AtomPersonConstruct"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class AtomPersonConstructExample
{
    /// <summary>
    /// Provides example code for the AtomPersonConstruct class.
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
        feed.Links.Add(new(new("/feed"), "self"));

        //  Identify the author of the feed
        feed.Authors.Add(new("John Doe"));

        //  Identify the contributors to the feed
        feed.Contributors.Add(new("Jane Doe"));

        AtomPersonConstruct contributor = new()
        {
            EmailAddress = "some.person@example.org",
            Name = "Some Person",
            Uri = new("http://example.org/somePerson")
        };
        feed.Contributors.Add(contributor);

        AtomEntry entry = new()
        {
            Id = new(new("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new("Atom-Powered Robots Run Amok"),
            UpdatedOn = new(2003, 12, 13, 18, 30, 2),
            Summary = new("Some text.")
        };

        //  Identify the author of the entry
        entry.Authors.Add(new("Jane Doe"));

        feed.AddEntry(entry);
    }
}