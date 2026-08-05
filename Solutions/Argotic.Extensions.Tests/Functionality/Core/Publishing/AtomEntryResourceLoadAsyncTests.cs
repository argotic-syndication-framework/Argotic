using System.Diagnostics.CodeAnalysis;
using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Publishing;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Publishing;

/// <summary>
/// Covers the <see cref="AtomEntryResource"/> load path that has never executed.
/// </summary>
/// <remarks>
///     <para>
///     §2.19 records <c>AtomEntryResource</c>'s <c>LoadAsync</c> state machine at <b>0.0% line and 0 of
///     4 branches</b>, and the type overall at 42.7% — the worst of anything this work touches. Nothing
///     in the suite has ever fetched a publishing resource.
///     </para>
///     <para>
///     That matters now rather than later, because <c>LoadAsync(Uri, HttpClient, …)</c> is one of the
///     three members Phase 4 converts from <c>new</c> to <c>override</c>. Re-slotting a method that no
///     test executes is a change whose effect nothing can observe.
///     </para>
/// </remarks>
[TestClass]
public sealed class AtomEntryResourceLoadAsyncTests
{
    private static readonly Uri Source = new("http://publishing.invalid/entry.atom");

    private static readonly DateTime ExpectedEditedOn = new(2024, 2, 20, 8, 30, 0, DateTimeKind.Utc);

    private const string PublishingEntry = """
        <?xml version="1.0" encoding="utf-8"?>
        <entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
          <id>urn:uuid:0a1b2c3d</id>
          <title>A fetched draft</title>
          <updated>2024-01-15T12:00:00Z</updated>
          <app:edited>2024-02-20T08:30:00Z</app:edited>
          <app:control><app:draft>yes</app:draft></app:control>
        </entry>
        """;

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Fetching into an existing instance through the concrete type populates the publishing members.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task LoadAsyncThroughTheConcreteType_PopulatesThePublishingMembers()
    {
        AtomEntryResource entry = new();
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(PublishingEntry);
        using HttpClient client = new(handler, disposeHandler: false);

        await entry.LoadAsync(Source, client, cancellationToken: TestContext.CancellationTokenSource.Token);

        entry.EditedOn.ShouldBe(ExpectedEditedOn);
        entry.IsDraft.ShouldBeTrue();
    }

    /// <summary>
    /// The static factory returns a populated <see cref="AtomEntryResource"/>.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task CreateAsyncThroughTheConcreteType_ReturnsAPopulatedResource()
    {
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(PublishingEntry);
        using HttpClient client = new(handler, disposeHandler: false);

        AtomEntryResource entry = await AtomEntryResource.CreateAsync(
            Source, client, cancellationToken: TestContext.CancellationTokenSource.Token);

        entry.EditedOn.ShouldBe(ExpectedEditedOn);
        entry.IsDraft.ShouldBeTrue();
    }

    /// <summary>
    /// Fetching through an <see cref="ISyndicationResource"/> reference drops the publishing state.
    /// </summary>
    /// <remarks>
    ///     The asynchronous counterpart of the shadowing pins, and the one that had no coverage at all.
    ///     The interface map is fixed at <c>AtomEntry</c>, so the shadowed <c>LoadAsync</c> never runs
    ///     and <c>LoadAtomPublishingExtensions</c> is never called. Inverted at Phase 4.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "The interface reference is the subject of the test. Narrowing it to AtomEntryResource would bind the shadowed member and the test would assert nothing.")]
    [TestMethod]
    public async Task LoadAsyncThroughAnInterfaceReference_SkipsThePublishingExtensions()
    {
        ISyndicationResource viaInterface = new AtomEntryResource();
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(PublishingEntry);
        using HttpClient client = new(handler, disposeHandler: false);

        await viaInterface.LoadAsync(Source, client, cancellationToken: TestContext.CancellationTokenSource.Token);

        AtomEntryResource entry = (AtomEntryResource)viaInterface;
        entry.HasExtensions.ShouldBeTrue("the extensions did arrive; they were simply not projected");
        entry.EditedOn.ShouldBe(DateTime.MinValue, "PINS TODAY. Inverted at Phase 4.");
        entry.IsDraft.ShouldBeFalse("PINS TODAY. Inverted at Phase 4.");
    }

    /// <summary>
    /// A failing fetch surfaces as an HTTP exception rather than an empty resource.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AFetchThatFails_SurfacesAsAnHttpException()
    {
        AtomEntryResource entry = new();
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithNotFound();
        using HttpClient client = new(handler, disposeHandler: false);

        await Should.ThrowAsync<HttpRequestException>(async () =>
            await entry.LoadAsync(Source, client, cancellationToken: TestContext.CancellationTokenSource.Token));
    }

    /// <summary>
    /// A fetched entry can be saved and read back with its publishing state intact.
    /// </summary>
    /// <remarks>
    ///     Uses the one save overload that reaches the synthesis path today. After Phase 4 every save
    ///     overload does, and this stays green either way — which is what makes it a control rather than
    ///     a pin.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AFetchedEntry_RoundTripsItsPublishingState()
    {
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(PublishingEntry);
        using HttpClient client = new(handler, disposeHandler: false);

        AtomEntryResource fetched = await AtomEntryResource.CreateAsync(
            Source, client, cancellationToken: TestContext.CancellationTokenSource.Token);

        StringBuilder builder = new();
        using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(builder))
        {
            fetched.Save(writer, new SyndicationResourceSaveSettings());
        }

        // Reloaded through a reader rather than by encoding the string to bytes. XmlWriter.Create over a
        // StringBuilder emits `encoding="utf-16"`, because that is what a StringBuilder is, so handing
        // those characters over as UTF-8 bytes produces a document that lies about itself - and the
        // loader believes the declaration, correctly. That is a property of the test, not of the entry.
        AtomEntryResource reloaded = new();
        using StringReader stringReader = new(builder.ToString());
        using System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stringReader);
        reloaded.Load(reader);

        reloaded.EditedOn.ShouldBe(ExpectedEditedOn);
        reloaded.IsDraft.ShouldBeTrue();
    }
}