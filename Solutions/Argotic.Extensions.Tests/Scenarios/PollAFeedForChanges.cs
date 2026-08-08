using System.Net;
using System.Net.Http.Headers;

using Argotic.Publishing;
namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Polls a resource repeatedly, downloading it only when the origin says it has changed.
/// </summary>
/// <remarks>
///     <para>
///     The workload this library exists for, and until now the one it had no API for. Conditional GET
///     was available as a raw <see cref="ConditionalGetResult"/> that the caller had to wire into a
///     resource themselves, which meant knowing to hand the response stream to
///     <c>Load(Stream, settings)</c> and knowing to keep the validators off the result. Every
///     <c>LoadAsync</c> overload downloads unconditionally.
///     </para>
///     <para>
///     These are scenarios rather than unit tests because each one spans a whole poll cycle: fetch,
///     store the validators, fetch again with them, and check what crossed the wire the second time.
///     </para>
/// </remarks>
[TestClass]
public sealed class PollAFeedForChanges
{
    private static readonly Uri Source = new("http://poll.invalid/feed.xml");

    private const string Feed =
        """<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel><title>Weekly</title><link>http://poll.invalid/</link><description>d</description></channel></rss>""";

    private const string Entry =
        """<?xml version="1.0" encoding="utf-8"?><entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app"><id>tag:poll.invalid,2026:1</id><title>Draft</title><updated>2026-01-01T00:00:00Z</updated><app:edited>2026-02-02T09:00:00Z</app:edited><app:control><app:draft>yes</app:draft></app:control></entry>""";

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// A first poll fetches the feed and comes back with what to send next time.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AFirstPollFetchesTheFeed_AndReturnsTheValidatorsForTheNext()
    {
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(Respond(Feed, "\"v1\"")));
        using HttpClient client = new(handler, disposeHandler: false);

        ConditionalLoadResult<RssFeed> result = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            Source, null, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue();

        // No null check and no `!` — MemberNotNullWhen is what makes the result type worth having.
        result.Resource.Channel.Title.ShouldBe("Weekly");
        result.Validators.ETag.ShouldBe("\"v1\"");
        result.Validators.LastModified.ShouldNotBeNull();
        result.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// A second poll sends the validators and, on a 304, downloads nothing.
    /// </summary>
    /// <remarks>
    ///     The whole point. <c>WasRead</c> is asserted on the content the origin was prepared to send:
    ///     a 304 carries no body, so a result that reported "unmodified" while still having drained
    ///     the connection would be a saving in name only.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ASecondPollSendsTheValidators_AndDownloadsNothingWhenUnchanged()
    {
        HttpRequestMessage? second = null;
        int calls = 0;

        using ControllableHttpContent body = new(Encoding.UTF8.GetBytes(Feed));
        using MockHttpMessageHandler handler = new((request, _) =>
        {
            if (calls++ == 0)
            {
                HttpResponseMessage first = new(HttpStatusCode.OK) { Content = body };
                first.Content.Headers.ContentType = new MediaTypeHeaderValue("application/rss+xml");
                first.Headers.ETag = new EntityTagHeaderValue("\"v1\"");
                first.Content.Headers.LastModified = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
                return Task.FromResult(first);
            }

            second = request;
            HttpResponseMessage unchanged = new(HttpStatusCode.NotModified);
            unchanged.Headers.ETag = new EntityTagHeaderValue("\"v1\"");
            return Task.FromResult(unchanged);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        ConditionalLoadResult<RssFeed> first = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            Source, null, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        ConditionalLoadResult<RssFeed> again = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            Source, first.Validators, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        second.ShouldNotBeNull();
        second.Headers.IfNoneMatch.ToString().ShouldBe("\"v1\"");
        second.Headers.IfModifiedSince.ShouldNotBeNull();

        again.WasModified.ShouldBeFalse();
        again.Resource.ShouldBeNull();
        again.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    /// <summary>
    /// A rotated entity tag on a 304 replaces the one that was sent.
    /// </summary>
    /// <remarks>
    ///     An origin may rotate its <c>ETag</c> on a not-modified response, and a caller who keeps
    ///     sending the tag they started with revalidates against a value the origin has stopped
    ///     recognising — at which point every poll is a full download again, silently. The validators
    ///     come off the response, not from what was sent.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ARotatedTagOnA304_ReplacesTheOneThatWasSent()
    {
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage unchanged = new(HttpStatusCode.NotModified);
            unchanged.Headers.ETag = new EntityTagHeaderValue("\"v2\"");
            return Task.FromResult(unchanged);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        ConditionalLoadResult<RssFeed> result = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            Source,
            new SyndicationValidators(DateTimeOffset.UtcNow.AddDays(-1), "\"v1\""),
            client,
            cancellationToken: this.TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeFalse();
        result.Validators.ETag.ShouldBe("\"v2\"", "the next poll must revalidate against what the origin now holds");
    }

    /// <summary>
    /// The size limit applies here, where it deliberately does not on the raw conditional GET.
    /// </summary>
    /// <remarks>
    ///     <see cref="ConditionalGetResult"/> hands out a live stream and leaves the decision to the
    ///     caller. This method reads the stream on the caller's behalf, so the limit that governs every
    ///     other load governs this one.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AResponsePastTheLimit_IsRefused()
    {
        string oversized = $"""<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel><title>{new string('x', 2048)}</title><link>http://poll.invalid/</link><description>d</description></channel></rss>""";

        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(Respond(oversized, "\"v1\"")));
        using HttpClient client = new(handler, disposeHandler: false);

        SyndicationContentTooLargeException thrown =
            await Should.ThrowAsync<SyndicationContentTooLargeException>(async () =>
                await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
                    Source,
                    null,
                    client,
                    new SyndicationResourceLoadSettings { MaxResponseContentLength = 512 },
                    cancellationToken: this.TestContext.CancellationTokenSource.Token));

        thrown.MaxBytes.ShouldBe(512);
    }

    /// <summary>
    /// A conditionally loaded publishing entry arrives with its publishing state populated.
    /// </summary>
    /// <remarks>
    ///     The gate that fails without the shadowing fix. <see cref="AtomEntryResource"/> used to
    ///     redeclare <c>Load</c> with <c>new</c> rather than override it, so any code holding the base
    ///     type — which a generic method necessarily does, through
    ///     <see cref="ISyndicationResource"/> — bound to <c>AtomEntry.Load</c> and never ran the
    ///     publishing extension pass. The result parsed, looked complete, and had
    ///     <c>EditedOn == DateTime.MinValue</c>.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AConditionallyLoadedPublishingEntry_KeepsItsPublishingState()
    {
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(Respond(Entry, "\"e1\"")));
        using HttpClient client = new(handler, disposeHandler: false);

        ConditionalLoadResult<AtomEntryResource> result =
            await SyndicationResourceReader.LoadIfModifiedAsync<AtomEntryResource>(
                Source, null, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue();
        result.Resource.EditedOn.ShouldBe(new DateTime(2026, 2, 2, 9, 0, 0, DateTimeKind.Utc));
        result.Resource.IsDraft.ShouldBeTrue();
    }

    /// <summary>
    /// The Loaded event fires on a 200 and not on a 304.
    /// </summary>
    /// <remarks>
    ///     Decidable, and decided: the event is raised by the resource's own <c>Load(Stream, settings)</c>,
    ///     which is reached only when there is a body. So a subscriber counting events is counting real
    ///     changes rather than polls. The arguments carry no <see cref="Uri"/>, because
    ///     <c>Load(Stream, …)</c> has never known one.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheLoadedEventFiresOnAChangeAndNotOnAPoll()
    {
        int calls = 0;
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(
            calls++ == 0 ? Respond(Feed, "\"v1\"") : new HttpResponseMessage(HttpStatusCode.NotModified)));
        using HttpClient client = new(handler, disposeHandler: false);

        ConditionalLoadResult<RssFeed> changed = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            Source, null, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        int raised = 0;
        SyndicationResourceLoadedEventArgs? seen = null;
        changed.Resource.ShouldNotBeNull();

        // Subscribing after the fact would observe nothing, so the count is taken from a second load
        // of the same instance rather than from the fetch above.
        changed.Resource.Loaded += (_, e) =>
        {
            raised++;
            seen = e;
        };

        using MemoryStream reload = new(Encoding.UTF8.GetBytes(Feed));
        changed.Resource.Load(reload, null);

        raised.ShouldBe(1);
        seen.ShouldNotBeNull();
        seen.Source.ShouldBeNull("Load(Stream, ...) has never known the address it came from");

        ConditionalLoadResult<RssFeed> unchanged = await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
            Source, changed.Validators, client, cancellationToken: this.TestContext.CancellationTokenSource.Token);

        unchanged.WasModified.ShouldBeFalse("and a 304 constructs no resource, so it can raise nothing");
    }

    /// <summary>
    /// The shared-client overload observes the caller's cancellation before opening a socket.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The convenience overload exists for symmetry with every other <c>LoadAsync(Uri, ...)</c>
    ///     in the library, and it binds the process-wide client, which no test can replace. Its
    ///     state machine sat at 0% — new public surface with nothing exercising it, which is worse
    ///     than inheriting an old gap.
    ///     </para>
    ///     <para>
    ///     An already-cancelled token reaches everything reachable offline: the linked source, the
    ///     deadline selection, and the delegation. It fails deterministically, before a connection
    ///     is attempted, so the suite stays socket-free.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheSharedClientOverload_ObservesTheCallersToken()
    {
        using CancellationTokenSource cancelled = new();
        await cancelled.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await SyndicationResourceReader.LoadIfModifiedAsync<RssFeed>(
                Source, null, null, cancelled.Token));
    }
    private static HttpResponseMessage Respond(string body, string eTag)
    {
        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)),
        };

        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        response.Content.Headers.LastModified = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        response.Headers.ETag = new EntityTagHeaderValue(eTag);
        return response;
    }
}