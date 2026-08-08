using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads and writes the <c>app:edited</c> element with <see cref="AtomPublishingEditedSyndicationExtension"/>.
/// </summary>
/// <remarks>
///     <c>app:edited</c> is not a second <c>atom:updated</c>. RFC 5023 section 10.1 draws the line
///     precisely: <c>atom:updated</c> is when the author considers the content to have changed
///     significantly, and is theirs to set; <c>app:edited</c> is when the server last altered the
///     member for any reason, including edits the author would call trivial. A client sorting a
///     collection by recency wants the second, and a reader that conflates the two silently reorders it.
/// </remarks>
internal static class AtomPublishingEditedSyndicationExtensionExample
{
    /// <summary>
    /// Finds the extension on the entries of an <see cref="AtomFeed"/> and compares it with <c>atom:updated</c>.
    /// </summary>
    public static void ClassExample()
    {
        AtomFeed feed = new();
        using (Stream inputStream = SampleDataPath.OpenRead(SampleDataPath.AtomFeedWithExtensions))
        {
            feed.Load(inputStream);
        }

        ExampleOutput.ShowLoaded("AtomFeed", feed.Title?.Content ?? string.Empty);

        foreach (AtomEntry entry in feed.Entries)
        {
            if (entry.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) is AtomPublishingEditedSyndicationExtension edited)
            {
                Console.WriteLine($"Entry: {entry.Title?.Content}");
                ExampleOutput.ShowAtomPublishingEditedExtension(edited);
                Console.WriteLine($"    atom:updated: {entry.UpdatedOn:yyyy-MM-dd HH:mm:ss}");
            }
        }

        int count = feed.Entries.Count(e => e.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) is not null);
        ExampleOutput.ShowItemsWithExtension(count, feed.Entries.Count, "app:edited");

        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("AtomFeed");
    }

    /// <summary>
    /// Stamps a new entry with a server edit time distinct from its author-set update time, then reads both back.
    /// </summary>
    public static void AuthorExample()
    {
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:6b2d8a71-2e5f-4c1b-8d4f-3a7c9f1e5b22")),
            Title = new AtomTextConstruct("Rx.NET v7.0 Released - it could save you 95MB!"),

            // The author's own claim about the content.
            UpdatedOn = new DateTime(2026, 7, 29, 10, 0, 0, DateTimeKind.Utc),
            Summary = new AtomTextConstruct("Moving UI framework support out of System.Reactive can cut 95MB from a deployment."),
        };

        AtomPublishingEditedSyndicationExtension edited = new();

        // Half an hour later the server touched the member -- a typo fix, say. atom:updated does not
        // move for that, and app:edited does. The two dates differing is the whole point.
        edited.Context.EditedOn = new DateTime(2026, 7, 29, 10, 30, 0, DateTimeKind.Utc);
        entry.Extensions.Add(edited);

        using MemoryStream saved = new();
        entry.Save(saved);
        ExampleOutput.ShowSaved("AtomEntry with app:edited");

        saved.Seek(0, SeekOrigin.Begin);
        AtomEntry reloaded = new();
        reloaded.Load(saved);

        Console.WriteLine($"  atom:updated: {reloaded.UpdatedOn:yyyy-MM-dd HH:mm:ss}");
        if (reloaded.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) is AtomPublishingEditedSyndicationExtension readBack)
        {
            ExampleOutput.ShowAtomPublishingEditedExtension(readBack);
        }
    }
}