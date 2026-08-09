namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Covers what happens when an Atom feed document and an Atom entry document are confused for one another.
/// </summary>
/// <remarks>
///     <para>
///     RFC 4287 defines two document types: a feed document, rooted at <c>&lt;feed&gt;</c>, and a
///     stand-alone entry document, rooted at <c>&lt;entry&gt;</c>. <see cref="AtomFeed"/> reads the
///     first and <see cref="AtomEntry"/> the second, and handing either the other one is an easy
///     mistake — an entry document is rare enough that most Atom URLs are feeds.
///     </para>
///     <para>
///     <b>The format gate cannot catch it.</b> <c>SyndicationResourceMetadata</c> tests for both roots
///     and reports <c>SyndicationContentFormat.Atom</c> for either, so the check in
///     <c>SyndicationResourceAdapter</c> — which rejects every other mismatched pairing, including an
///     Atom category document handed to an entry — compares <c>Atom</c> against <c>Atom</c> and passes.
///     <c>Atom</c> is the only format value in the enum covering two document shapes; the two Atom
///     Publishing document types each got their own.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomDocumentShapeTests
{
    /// <summary>
    /// A feed document handed to an entry is refused.
    /// </summary>
    /// <remarks>
    ///     It used to produce a default-constructed <see cref="AtomEntry"/> — empty title, empty id,
    ///     <see cref="DateTime.MinValue"/> — and say nothing. The adapter looked for an
    ///     <c>&lt;entry&gt;</c> child of the document root, found <c>&lt;feed&gt;</c> instead, and its
    ///     <c>if (entryNavigator is not null)</c> guard skipped the whole fill.
    /// </remarks>
    [TestMethod]
    public void AFeedDocumentHandedToAnEntry_IsRefused()
    {
        AtomEntry entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        FormatException thrown = Should.Throw<FormatException>(() => entry.Load(stream));

        thrown.Message.ShouldContain("AtomEntryDocument", Case.Sensitive);
        thrown.Message.ShouldContain("<feed> root", Case.Sensitive);
    }

    /// <summary>
    /// An entry document handed to a feed is refused.
    /// </summary>
    /// <remarks>
    ///     The same hole in the other direction, and equally silent before: <c>Fill(AtomFeed)</c> has
    ///     the identical shape. Both directions matter, because a caller who sniffs the format first
    ///     gets <c>Atom</c> either way and has nothing to dispatch on.
    /// </remarks>
    [TestMethod]
    public void AnEntryDocumentHandedToAFeed_IsRefused()
    {
        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtomEntry));

        FormatException thrown = Should.Throw<FormatException>(() => feed.Load(stream));

        thrown.Message.ShouldContain("<entry> root", Case.Sensitive);
        thrown.Message.ShouldContain("AtomFeed", Case.Sensitive);
    }

    /// <summary>
    /// The Atom 0.3 adapter behaves the same way.
    /// </summary>
    /// <remarks>
    ///     A separate adapter with its own copy of the guard, reached by the same dispatch. Without
    ///     this row the fix is equally consistent with "1.0 was fixed and 0.3 was forgotten".
    /// </remarks>
    [TestMethod]
    public void TheLegacyAdapterRefusesTheSameMismatch()
    {
        AtomEntry entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.Atom03Feed));

        Should.Throw<FormatException>(() => entry.Load(stream));
    }

    /// <summary>
    /// Sniffing an entry document now tells a caller which type to construct.
    /// </summary>
    /// <remarks>
    ///     The reason the fix is a format value rather than a guard in the adapter.
    ///     <c>SyndicationContentFormatGet</c> exists so a caller can decide what to build; for these
    ///     two document shapes it returned <c>Atom</c> either way, so it could not.
    /// </remarks>
    [TestMethod]
    public void SniffingTellsTheTwoAtomDocumentShapesApart()
    {
        using MemoryStream feed = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));
        using MemoryStream entry = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtomEntry));

        SyndicationDiscoveryUtility.SyndicationContentFormatGet(feed)
            .ShouldBe(SyndicationContentFormat.Atom);
        SyndicationDiscoveryUtility.SyndicationContentFormatGet(entry)
            .ShouldBe(SyndicationContentFormat.AtomEntryDocument, "was Atom, which named no type to build");
    }

    /// <summary>
    /// A format the generic feed cannot represent is refused rather than silently ignored.
    /// </summary>
    /// <remarks>
    ///     <c>GenericSyndicationFeed.Load</c> was a chain of three <c>else if</c> arms with no final
    ///     <c>else</c>, so any other format fell through to the <c>Loaded</c> event -- leaving a
    ///     default-constructed instance and announcing that a load had succeeded. Found while
    ///     verifying the Atom fix, and the same defect a third time.
    /// </remarks>
    [TestMethod]
    public void AFormatTheGenericFeedCannotRepresent_IsRefused()
    {
        Argotic.Syndication.GenericSyndicationFeed generic = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtomEntry));

        Should.Throw<FormatException>(() => generic.Load(stream))
            .Message.ShouldContain("AtomEntryDocument");
    }
    /// <summary>
    /// Each type still reads the document it is for.
    /// </summary>
    /// <remarks>
    ///     The control, and the assertion that would fail if the guard had simply been made to throw
    ///     unconditionally. Without it, the three rows above are equally consistent with
    ///     "<c>Load</c> now rejects everything".
    /// </remarks>
    [TestMethod]
    public void TheMatchingDocumentStillLoads()
    {
        AtomFeed feed = new();
        using MemoryStream feedStream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));
        feed.Load(feedStream);
        feed.Title.ShouldNotBeNull();
        feed.Title.Content.ShouldBe("Test Feed");

        AtomEntry entry = new();
        using MemoryStream entryStream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtomEntry));
        entry.Load(entryStream);
        entry.Title.ShouldNotBeNull();
        entry.Title.Content.ShouldBe("Test Entry");
    }
}