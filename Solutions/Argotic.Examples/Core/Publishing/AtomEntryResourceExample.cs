using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Publishing;

/// <summary>
/// Demonstrates <see cref="AtomEntryResource"/>: an Atom entry as a member of a Publishing collection.
/// </summary>
/// <remarks>
///     <para>
///     An <see cref="AtomEntryResource"/> is an <see cref="AtomEntry"/> that also knows the two things
///     a collection member has and a syndicated entry does not: whether it is a draft, and when the
///     server last edited it. Those are <c>app:control/app:draft</c> and <c>app:edited</c>, surfaced
///     here as ordinary properties rather than as extensions to go looking for.
///     </para>
///     <para>
///     It derives from <see cref="AtomEntry"/>, which is load-bearing rather than incidental: the
///     dispatcher's Atom arm pattern-matches on <see cref="AtomEntry"/>, so an entry resource is filled
///     by the same adapters as any other entry and picks up the publishing state on top.
///     </para>
/// </remarks>
internal static class AtomEntryResourceExample
{
    /// <summary>
    /// Builds a draft entry resource and prints its publishing state.
    /// </summary>
    public static void ClassExample()
    {
        AtomEntryResource entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:7c3e9b52-4f6a-4e8d-b1c7-5d2a8f0e6b33")),
            Title = new AtomTextConstruct("Rx.NET v8.0 planning"),
            UpdatedOn = new DateTime(2026, 7, 30, 11, 0, 0, DateTimeKind.Utc),
            Summary = new AtomTextConstruct("An outline of what a v8.0 might contain, pending community feedback."),

            // The two members a collection member has that a syndicated entry does not.
            IsDraft = true,
            EditedOn = new DateTime(2026, 7, 30, 11, 5, 0, DateTimeKind.Utc),
        };

        entry.Authors.Add(new AtomPersonConstruct("Ian Griffiths"));

        ExampleOutput.ShowAtomEntry(entry);
        Console.WriteLine($"  Draft:     {entry.IsDraft}");
        Console.WriteLine($"  Edited on: {entry.EditedOn:yyyy-MM-dd HH:mm:ss}");
    }

    /// <summary>
    /// Loads a stand-alone entry document as an entry resource.
    /// </summary>
    /// <remarks>
    ///     The sample entry document declares no <c>app:</c> namespace, so this shows the ordinary
    ///     case: a member with no publishing state at all. RFC 5023 section 9.2 says a member with no
    ///     <c>app:control</c> is not a draft, so <c>IsDraft</c> reading false here is the specified
    ///     answer rather than a missing value.
    /// </remarks>
    public static void LoadStreamExample()
    {
        AtomEntryResource entry = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.AtomEntryDocument);
        entry.Load(stream);

        ExampleOutput.ShowAtomEntry(entry);
        Console.WriteLine($"  Draft: {entry.IsDraft} (no app:control in the document)");
    }

    /// <summary>
    /// Saves a draft entry resource and reads the draft state back.
    /// </summary>
    public static void SaveStreamExample()
    {
        AtomEntryResource entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:8d4f0c63-5a7b-4f9e-c2d8-6e3b9a1f7c44")),
            Title = new AtomTextConstruct("Auditing UK energy policy without a cluster"),
            UpdatedOn = new DateTime(2026, 6, 29, 6, 0, 0, DateTimeKind.Utc),
            IsDraft = true,
        };

        using MemoryStream stream = new();
        entry.Save(stream);
        ExampleOutput.ShowSaved("AtomEntryResource (draft)");

        stream.Seek(0, SeekOrigin.Begin);
        AtomEntryResource reloaded = new();
        reloaded.Load(stream);

        // Whether the draft flag survives a round trip is the only question that matters here: losing
        // it publishes something the author had not finished.
        Console.WriteLine($"  Draft after round trip: {reloaded.IsDraft}");
    }
}