using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads and writes the <c>app:control</c> element with <see cref="AtomPublishingControlSyndicationExtension"/>.
/// </summary>
/// <remarks>
///     <para>
///     RFC 5023 gives a member exactly one piece of publication state: <c>app:draft</c>, meaning the
///     server should not make this entry publicly visible. It is a small extension carrying a decision
///     nothing else in the document records, and a client that drops it on a round trip publishes
///     something its author had not finished.
///     </para>
///     <para>
///     Absence is not the same as <c>no</c>. RFC 5023 section 9.2 says a member with no
///     <c>app:control</c> is treated as not a draft, so an unset extension and an extension set to
///     false mean the same thing to a server — but only the second says so out loud.
///     </para>
/// </remarks>
internal static class AtomPublishingControlSyndicationExtensionExample
{
    /// <summary>
    /// Finds the extension on the entries of an <see cref="AtomFeed"/> and reports which are drafts.
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
            if (entry.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) is AtomPublishingControlSyndicationExtension control)
            {
                Console.WriteLine($"Entry: {entry.Title?.Content}");
                ExampleOutput.ShowAtomPublishingControlExtension(control);
            }
        }

        int drafts = feed.Entries.Count(e =>
            e.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) is AtomPublishingControlSyndicationExtension c && c.Context.IsDraft);

        Console.WriteLine($"Draft entries: {drafts}/{feed.Entries.Count}");

        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("AtomFeed");
    }

    /// <summary>
    /// Marks a new entry as a draft, saves it, and reads the state back.
    /// </summary>
    public static void AuthorExample()
    {
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:5a1c7f60-1f4d-4d0a-9c3e-2f6b8e0d4a11")),
            Title = new AtomTextConstruct("Rx.NET v8.0 planning"),
            UpdatedOn = new DateTime(2026, 7, 30, 11, 0, 0, DateTimeKind.Utc),
            Summary = new AtomTextConstruct("Not yet published: an outline pending community feedback."),
        };

        AtomPublishingControlSyndicationExtension control = new();
        control.Context.IsDraft = true;
        entry.Extensions.Add(control);

        using MemoryStream saved = new();
        entry.Save(saved);
        ExampleOutput.ShowSaved("AtomEntry marked as draft");

        // Round trip, because "did the draft flag survive" is the only question worth asking of an
        // extension whose whole job is to stop a server publishing something.
        saved.Seek(0, SeekOrigin.Begin);
        AtomEntry reloaded = new();
        reloaded.Load(saved);

        if (reloaded.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) is AtomPublishingControlSyndicationExtension readBack)
        {
            ExampleOutput.ShowAtomPublishingControlExtension(readBack);
        }
    }
}