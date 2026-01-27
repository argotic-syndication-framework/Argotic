using Argotic.Syndication;

namespace Argotic.Examples.Core.Atom;

/// <summary>
/// Contains the code examples for the <see cref="AtomSource"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="AtomSource"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class AtomSourceExample
{
    /// <summary>
    /// Provides example code for the AtomSource class.
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
            Rights = new AtomTextConstruct("© 2003 Example, Inc.")
        };
        entry.Source = source;

        feed.Entries.Add(entry);
    }
}