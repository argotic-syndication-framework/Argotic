using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Publishing;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

/// <summary>
/// The Atom Publishing state that <see cref="AtomEntryResource"/> used to lose, and no longer does.
/// </summary>
/// <remarks>
///     <para>
///     <b>Every entry here is constructed in code. None of them is loaded, and that is the whole
///     point.</b> The suite also contains a load-then-save round trip asserting the output carries
///     <c>edited</c> and <c>draft</c>, and it was green throughout — because <c>Load</c> puts the
///     extension objects into <c>Extensions</c> directly, so the base <c>Save</c> wrote them out
///     without ever consulting <c>EditedOn</c> or <c>IsDraft</c>. The synthesis step that reads those
///     properties was what was broken, and a round trip from a loaded document could not reach it.
///     </para>
///     <para>
///     §2.19 stated it flatly at the time: <c>AtomEntryResource.Save(XmlWriter, settings)</c> was at
///     <b>0.0% line and 0 of 8 branches</b>. The member that should have written the state had never
///     executed.
///     </para>
///     <para>
///     The cause was that <see cref="AtomEntryResource"/> declared nine members <c>new</c> rather than
///     <c>override</c>, because no corresponding <see cref="AtomEntry"/> member was <c>virtual</c>.
///     Shadowing binds at compile time from the static type of the reference, so every base overload
///     funnelled past every shadow. Three base members are now <c>virtual</c> and three shadows are
///     <c>override</c>s; these five tests are what say it worked, and the fifth is what says the other
///     four did not merely move together.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomEntryResourceShadowingTests
{
    private const string AppNamespace = "http://www.w3.org/2007/app";

    private const string PublishingEntry = """
        <?xml version="1.0" encoding="utf-8"?>
        <entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
          <id>urn:uuid:0a1b2c3d</id>
          <title>A draft entry</title>
          <updated>2024-01-15T12:00:00Z</updated>
          <app:edited>2024-02-20T08:30:00Z</app:edited>
          <app:control><app:draft>yes</app:draft></app:control>
        </entry>
        """;

    private static readonly DateTime EditedOn = new(2024, 2, 20, 8, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// C6.1 — the concretely-typed <c>Save(Stream)</c> writes both publishing elements.
    /// </summary>
    /// <remarks>
    ///     No base reference anywhere in this test. <c>AtomEntry.Save(Stream)</c> forwards to
    ///     <c>Save(Stream, settings)</c>, which calls the non-virtual <c>Save(XmlWriter, settings)</c> on
    ///     itself — so the shadow never runs even though the caller is holding the derived type.
    /// </remarks>
    [TestMethod]
    public void SavingAConstructedEntryToAStream_WritesThePublishingState()
    {
        AtomEntryResource entry = ConstructedDraft();

        using MemoryStream stream = new();
        entry.Save(stream);

        (bool edited, bool draft) = Probe(Encoding.UTF8.GetString(stream.ToArray()));
        edited.ShouldBeTrue("INVERTED at Phase 4: the shadow now runs through the virtual base call.");
        draft.ShouldBeTrue("INVERTED at Phase 4: app:control/app:draft too, which the plan did not say was lost.");
    }

    /// <summary>
    /// C6.2 — so does <c>Save(XmlWriter)</c>, the overload without settings.
    /// </summary>
    [TestMethod]
    public void SavingAConstructedEntryToAWriter_WritesThePublishingState()
    {
        AtomEntryResource entry = ConstructedDraft();

        StringBuilder builder = new();
        using (XmlWriter writer = XmlWriter.Create(builder))
        {
            entry.Save(writer);
        }

        (bool edited, bool draft) = Probe(builder.ToString());
        edited.ShouldBeTrue("INVERTED at Phase 4.");
        draft.ShouldBeTrue("INVERTED at Phase 4.");
    }

    /// <summary>
    /// C6.3 — and so does the four-argument save reached through the interface.
    /// </summary>
    /// <remarks>
    ///     Distinct from the row above: this one proves the <i>interface map</i> is fixed at
    ///     <see cref="AtomEntry"/>, rather than proving anything about an internal funnel.
    /// </remarks>
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "The interface reference is the subject of the test. Narrowing it to AtomEntryResource would bind the shadowed member and the test would assert nothing.")]
    [TestMethod]
    public void SavingThroughAnInterfaceReference_WritesThePublishingState()
    {
        ISyndicationResource entry = ConstructedDraft();

        StringBuilder builder = new();
        using (XmlWriter writer = XmlWriter.Create(builder))
        {
            entry.Save(writer, new SyndicationResourceSaveSettings());
        }

        (bool edited, bool draft) = Probe(builder.ToString());
        edited.ShouldBeTrue("INVERTED at Phase 4.");
        draft.ShouldBeTrue("INVERTED at Phase 4.");
    }

    /// <summary>
    /// C6.4 — loading through an interface reference now populates the publishing members.
    /// </summary>
    /// <remarks>
    ///     The <c>HasExtensions</c> assertion is what makes the negative one mean something. Without it,
    ///     <c>EditedOn == DateTime.MinValue</c> is equally consistent with "the document failed to
    ///     parse"; with it, the extensions demonstrably arrived and simply were not projected onto the
    ///     properties.
    /// </remarks>
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "The interface reference is the subject of the test. Narrowing it to AtomEntryResource would bind the shadowed member and the test would assert nothing.")]
    [TestMethod]
    public void LoadingThroughAnInterfaceReference_PopulatesThePublishingMembers()
    {
        ISyndicationResource viaInterface = new AtomEntryResource();
        using (MemoryStream stream = new(Encoding.UTF8.GetBytes(PublishingEntry), writable: false))
        {
            viaInterface.Load(stream);
        }

        AtomEntryResource entry = (AtomEntryResource)viaInterface;
        entry.HasExtensions.ShouldBeTrue("the extensions did arrive; they were simply not projected");
        entry.EditedOn.ShouldBe(EditedOn, "INVERTED at Phase 4: the interface map now reaches the override.");
        entry.IsDraft.ShouldBeTrue("INVERTED at Phase 4.");
    }

    /// <summary>
    /// C6.4, control — the concretely-typed load populates them correctly.
    /// </summary>
    [TestMethod]
    public void LoadingThroughTheConcreteType_PopulatesThePublishingMembers()
    {
        AtomEntryResource entry = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(PublishingEntry), writable: false);
        entry.Load(stream);

        entry.EditedOn.ShouldBe(EditedOn);
        entry.IsDraft.ShouldBeTrue();
    }

    /// <summary>
    /// C6.5, the control that must <b>not</b> invert.
    /// </summary>
    /// <remarks>
    ///     The other four rows all assert that something is absent, and after Phase 4 they all assert it
    ///     is present. If the change accidentally broke the shadow as well — or if the extension itself
    ///     stopped emitting — all four would still flip together and read as success. This row is the
    ///     one that is green before and green after, and it is the only member that reaches the
    ///     synthesis path today.
    /// </remarks>
    [TestMethod]
    public void SavingThroughTheShadowedOverload_KeepsThePublishingState()
    {
        AtomEntryResource entry = ConstructedDraft();

        StringBuilder builder = new();
        using (XmlWriter writer = XmlWriter.Create(builder))
        {
            entry.Save(writer, new SyndicationResourceSaveSettings());
        }

        (bool edited, bool draft) = Probe(builder.ToString());
        edited.ShouldBeTrue("INVARIANT: this is the one overload that works, before and after Phase 4.");
        draft.ShouldBeTrue("INVARIANT.");
    }

    private static AtomEntryResource ConstructedDraft() => new(
        new AtomId(new Uri("urn:uuid:0a1b2c3d")),
        new AtomTextConstruct("A draft entry"),
        new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
        EditedOn,
        isDraft: true);

    private static (bool HasEdited, bool HasDraft) Probe(string xml)
    {
        // Save(Stream) writes a UTF-8 byte-order mark, and decoding those bytes to a string leaves
        // U+FEFF as the first character, which XPathDocument rejects as content before the root
        // element. Reading through a StreamReader would strip it; this is the explicit equivalent, and
        // is here rather than hidden because "the save path emits a BOM" is a fact worth stating.
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(xml.TrimStart('\uFEFF'));
        XmlNamespaceManager namespaces = new(navigator.NameTable);
        namespaces.AddNamespace("app", AppNamespace);

        return (navigator.SelectSingleNode("//app:edited", namespaces) is not null,
                navigator.SelectSingleNode("//app:control/app:draft", namespaces) is not null);
    }
}