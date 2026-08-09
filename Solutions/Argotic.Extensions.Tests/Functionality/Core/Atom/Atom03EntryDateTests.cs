namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

/// <summary>
/// Covers how an Atom 0.3 entry's three date elements reach the object model.
/// </summary>
/// <remarks>
///     <para>
///     Atom 0.3 gives an entry three dates and does not treat them alike. <c>atom:issued</c> and
///     <c>atom:modified</c> are <b>required</b>; <c>atom:created</c> is <b>optional</b>.
///     <c>issued</c> is the entry's publication time and is the element RFC 4287 replaced with
///     <c>atom:published</c>, which is why <see cref="AtomEntry.PublishedOn"/> is where it belongs.
///     </para>
///     <para>
///     <b>The adapter read only <c>created</c>.</b> <c>Atom03SyndicationResourceAdapter</c> selected
///     <c>atom:created</c> for <c>PublishedOn</c> and referred to <c>atom:issued</c> nowhere at all, so
///     an entry carrying the conformant minimum — the two required elements and not the optional one —
///     lost its publication date silently, arriving as <see cref="DateTime.MinValue"/>.
///     </para>
///     <para>
///     Found by converting the real-world corpus into tests, and the corpus is what shows the size of
///     it. Of 70 Atom 0.3 entries across six documents: <c>intertwingly-atom03-2004.xml</c> has
///     <b>5 <c>issued</c>, 0 <c>created</c></b> across 5 entries, and
///     <c>livejournal-brad-atom03-2004.xml</c> has <b>25 <c>issued</c>, 2 <c>created</c></b> across 25
///     entries. <b>28 entries lost their date entirely</b>, and the two feeds worst affected are Sam
///     Ruby's and Brad Fitzpatrick's — that is, the people who were writing the specifications.
///     </para>
/// </remarks>
[TestClass]
public sealed class Atom03EntryDateTests
{
    private static AtomEntry LoadFirstEntry(string entryDates)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://purl.org/atom/ns#" version="0.3">
              <title>A Feed</title>
              <id>tag:example.com,1999:feed</id>
              <modified>2005-01-01T00:00:00Z</modified>
              <entry>
                <title>An Entry</title>
                <id>tag:example.com,1999:entry</id>
                {entryDates}
              </entry>
            </feed>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        AtomFeed feed = new();
        feed.Load(stream);

        return feed.Entries.Single();
    }

    private const string Issued = "<issued>2001-01-01T01:01:01Z</issued>";
    private const string Created = "<created>2002-02-02T02:02:02Z</created>";
    private const string Modified = "<modified>2003-03-03T03:03:03Z</modified>";

    /// <summary>
    /// An entry carrying only the two required date elements keeps its publication date.
    /// </summary>
    /// <remarks>
    ///     The conformant minimum, and the shape that was broken. Every entry in
    ///     <c>intertwingly-atom03-2004.xml</c> is written this way.
    /// </remarks>
    [TestMethod]
    public void AnEntryWithTheRequiredDatesOnly_KeepsItsPublicationDate()
    {
        AtomEntry entry = LoadFirstEntry(Issued + Modified);

        entry.PublishedOn.ShouldNotBe(DateTime.MinValue, "issued is required and is the publication time");
        entry.PublishedOn.ShouldBe(new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc));
        entry.UpdatedOn.ShouldBe(new DateTime(2003, 3, 3, 3, 3, 3, DateTimeKind.Utc));
    }

    /// <summary>
    /// When an entry carries both, <c>issued</c> is the publication date and <c>created</c> is not.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The three literals are deliberately three different years, so this test can tell them apart.
    ///     An earlier probe used values where <c>issued</c> and <c>created</c> named the same instant in
    ///     different offsets, and it could not have distinguished a correct mapping from a wrong one.
    ///     </para>
    ///     <para>
    ///     This is the row that changes an existing answer rather than filling in a missing one:
    ///     <c>created</c> used to win because it was the only element read. Atom 0.3 defines
    ///     <c>issued</c> as the time the entry was issued and <c>created</c> as the time it was created,
    ///     and it is <c>issued</c> that RFC 4287 carried forward as <c>published</c>.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void WhenAnEntryCarriesBoth_IssuedIsThePublicationDate()
    {
        AtomEntry entry = LoadFirstEntry(Issued + Created + Modified);

        entry.PublishedOn.ShouldBe(
            new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            "issued outranks created, which is a different event");
    }

    /// <summary>
    /// An entry carrying only the optional <c>created</c> still yields a publication date.
    /// </summary>
    /// <remarks>
    ///     The compatibility control. <c>created</c> was the only element read before, so it must keep
    ///     working when it is all an entry offers — a fix that swapped one element for the other rather
    ///     than preferring one would pass the two tests above and fail here.
    /// </remarks>
    [TestMethod]
    public void AnEntryWithOnlyTheOptionalCreated_StillYieldsAPublicationDate()
    {
        AtomEntry entry = LoadFirstEntry(Created + Modified);

        entry.PublishedOn.ShouldBe(new DateTime(2002, 2, 2, 2, 2, 2, DateTimeKind.Utc));
    }

    /// <summary>
    /// An entry with neither has no publication date, and does not fail to load.
    /// </summary>
    /// <remarks>
    ///     The boundary control. <c>PublishedOn</c> is a non-nullable <see cref="DateTime"/>, so "absent"
    ///     and "1 January 0001" are the same value — which is exactly why the tests above assert a real
    ///     instant rather than merely that the load succeeded.
    /// </remarks>
    [TestMethod]
    public void AnEntryWithNeither_LoadsWithNoPublicationDate()
    {
        AtomEntry entry = LoadFirstEntry(Modified);

        entry.PublishedOn.ShouldBe(DateTime.MinValue);
        entry.UpdatedOn.ShouldBe(new DateTime(2003, 3, 3, 3, 3, 3, DateTimeKind.Utc));
    }
}