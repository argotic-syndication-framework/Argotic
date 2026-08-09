namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Fetching a single Atom 0.3 entry document and reading what it says.
/// </summary>
/// <remarks>
///     <para>
///     A stand-alone entry document — an <c>entry</c> element as the root of its own file, rather than an
///     entry inside a feed — is what an Atom 0.3-era editing endpoint returned for one post.
///     <see cref="AtomEntry"/> has always advertised that it reads one, and until now it could not read a
///     0.3 one at all: the walk it routed to built its namespace manager against the Atom 1.0 namespace
///     and then looked for an entry the document had spelled in the 0.3 namespace, so every such load
///     ended in a <see cref="FormatException"/> whose message said the document was not entry-rooted.
///     </para>
///     <para>
///     The consumer's path is what is exercised here — <c>Load</c> from a stream, then <c>Save</c> — not
///     the adapter, because a defect that only the adapter's own tests can see is a defect nobody
///     reported.
///     </para>
/// </remarks>
[TestClass]
public class ReadAStandaloneAtom03EntryDocument
{
    private const string Atom03EntryDocument = """
        <?xml version="1.0" encoding="UTF-8"?>
        <entry xmlns="http://purl.org/atom/ns#">
            <title>The Entry This Library Could Not Read</title>
            <id>urn:uuid:8f2b4c1e-0a5d-4c3f-9c2a-2b7d6e5f4a31</id>
            <modified>2025-01-20T12:00:00Z</modified>
            <issued>2025-01-19T09:30:00Z</issued>
            <link rel="alternate" type="text/html" href="http://example.com/entry"/>
            <author>
                <name>Entry Author</name>
                <url>http://example.com/author</url>
            </author>
            <summary mode="escaped">A short summary.</summary>
        </entry>
        """;

    /// <summary>
    /// An Atom 0.3 entry document loads its title, its identifier and the rest of what it carries.
    /// </summary>
    [TestMethod]
    public void AnAtom03EntryDocument_LoadsItsTitleAndId()
    {
        // Arrange
        AtomEntry entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(Atom03EntryDocument));

        // Act
        entry.Load(stream);

        // Assert
        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("The Entry This Library Could Not Read");
        entry.Id.ShouldNotBeNull();
        entry.Id.Uri.ShouldBe(new Uri("urn:uuid:8f2b4c1e-0a5d-4c3f-9c2a-2b7d6e5f4a31"));
        entry.UpdatedOn.ShouldBe(new DateTime(2025, 1, 20, 12, 0, 0, DateTimeKind.Utc));
        entry.PublishedOn.ShouldBe(new DateTime(2025, 1, 19, 9, 30, 0, DateTimeKind.Utc));
        entry.Authors.Count.ShouldBe(1);
        entry.Authors[0].Name.ShouldBe("Entry Author");
        entry.Summary.ShouldNotBeNull();
        entry.Summary.Content.ShouldBe("A short summary.");
    }

    /// <summary>
    /// Saving what was read emits an Atom 1.0 entry document, because that is the only Atom this library
    /// writes.
    /// </summary>
    /// <remarks>
    ///     A 0.3 document is read and a 1.0 document is written: <c>modified</c> comes back as
    ///     <c>updated</c> and <c>issued</c> as <c>published</c>. The conversion is the point of reading 0.3
    ///     at all, and it is only reachable now that the load works.
    /// </remarks>
    [TestMethod]
    public void AnAtom03EntryDocument_SavesAsAnAtom10EntryDocument()
    {
        // Arrange
        AtomEntry entry = new();
        using MemoryStream source = new(Encoding.UTF8.GetBytes(Atom03EntryDocument));
        entry.Load(source);

        // Act
        using MemoryStream destination = new();
        entry.Save(destination);
        string written = Encoding.UTF8.GetString(destination.ToArray());

        // Assert
        written.ShouldContain("http://www.w3.org/2005/Atom", Case.Sensitive);
        written.ShouldContain("<updated>", Case.Sensitive);
        written.ShouldContain("<published>", Case.Sensitive);
        written.ShouldContain("The Entry This Library Could Not Read", Case.Sensitive);
    }

    /// <summary>
    /// The same entry, reloaded from what was written, is the entry that was saved.
    /// </summary>
    [TestMethod]
    public void AnAtom03EntryDocument_SurvivesBeingSavedAndReadBackAsAtom10()
    {
        // Arrange
        AtomEntry original = new();
        using MemoryStream source = new(Encoding.UTF8.GetBytes(Atom03EntryDocument));
        original.Load(source);

        using MemoryStream destination = new();
        original.Save(destination);
        destination.Position = 0;

        // Act
        AtomEntry reloaded = new();
        reloaded.Load(destination);

        // Assert
        reloaded.Title.ShouldNotBeNull();
        reloaded.Title.Content.ShouldBe(original.Title!.Content);
        reloaded.Id.ShouldNotBeNull();
        reloaded.Id.Uri.ShouldBe(original.Id!.Uri);
        reloaded.UpdatedOn.ShouldBe(original.UpdatedOn);
        reloaded.Authors.Count.ShouldBe(1);
        reloaded.Authors[0].Name.ShouldBe("Entry Author");
    }
}