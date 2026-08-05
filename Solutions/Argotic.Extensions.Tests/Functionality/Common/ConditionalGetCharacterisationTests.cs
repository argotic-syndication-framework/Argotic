using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins what conditional GET decides, and what it throws away deciding it.
/// </summary>
/// <remarks>
///     <para>
///     Four tests covered <c>ConditionalGetAsync</c> before these, and both of the ones about
///     modification took the primary branch — the response carrying a genuinely newer
///     <c>Last-Modified</c>. The fallback underneath it, which is where the interesting behaviour was,
///     had never run. §2.19 records the method at 12 of 18 branches.
///     </para>
///     <para>
///     The whole decision is now "304 means unmodified, any 2xx means modified". These rows are what
///     that replacement was measured against, and two of them were worse than the plan described:
///     C3c and C3d are not confusion about what changed, they are a response body being downloaded,
///     discarded, and reported to the caller as a cache hit.
///     </para>
///     <para>
///     C3a and C3b did not invert and were never expected to. They arrived at the right answer
///     through the wrong reasoning, and a test that passes before and after is what tells you the
///     replacement did not change more than it meant to.
///     </para>
/// </remarks>
[TestClass]
public sealed class ConditionalGetCharacterisationTests
{
    private static readonly Uri Source = new("http://conditional.invalid/feed.xml");

    private static readonly DateTime SentValidator = new(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// C3a — a 200 with no <c>Last-Modified</c> at all is reported as modified.
    /// </summary>
    /// <remarks>
    ///     The primary comparison cannot be true here: an absent header becomes
    ///     <see cref="DateTimeOffset.MinValue"/>, which is not newer than anything. The result comes
    ///     entirely from the fallback, which asked whether the response had a body or a content type.
    ///     Almost every response does. The answer is unchanged and the reasoning is not: it is now the
    ///     status code, which is what the server was answering with all along.
    /// </remarks>
    [TestMethod]
    public async Task A200WithNoLastModified_IsReportedAsModified()
    {
        using HttpClient client = Responding(response => { });

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue("INVARIANT: right answer before and after, for different reasons.");
    }

    /// <summary>
    /// C3b — a 200 whose <c>Last-Modified</c> is <i>older</i> than the one we sent is still modified.
    /// </summary>
    /// <remarks>
    ///     The row that showed the comparison was dead code. The server said, in effect, "this
    ///     representation is older than the one you already have", and the fallback overrode it because
    ///     the response carried a content type. Still modified, now because a 200 is a 200.
    /// </remarks>
    [TestMethod]
    public async Task A200WithAnOlderLastModified_IsStillReportedAsModified()
    {
        using HttpClient client = Responding(response =>
            response.Content.Headers.LastModified = SentValidator.AddDays(-30));

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue(
            "INVARIANT: the Last-Modified comparison never decided anything, and is now gone.");
    }

    /// <summary>
    /// C3c — a 200 carrying neither a content type nor a length is delivered, body intact.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The worst of the defects this phase removes, and one the plan did not describe. The
    ///     heuristic asked "does this look like it has content" via <c>ContentLength</c> and
    ///     <c>ContentType</c>; when both answered no the response was disposed and the caller told
    ///     nothing had changed. A real 200 with a real body, thrown away and reported as a cache hit.
    ///     </para>
    ///     <para>
    ///     A chunked response with no <c>Content-Type</c> is exactly this shape — which is to say a
    ///     perfectly ordinary one.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public async Task A200WithNoContentTypeAndNoLength_IsDeliveredWithItsBodyIntact()
    {
        using HttpClient client = Responding(
            response =>
            {
                response.Content.Headers.ContentType = null;
                response.Content.Headers.ContentLength = null;
            },
            declareLength: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue(
            "INVERTED: a 200 is a 200, whatever its headers do or do not say about its body");

        using Stream body = await result.GetResponseStreamAsync(TestContext.CancellationTokenSource.Token);
        using StreamReader reader = new(body);
        string delivered = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);
        delivered.ShouldNotBeEmpty("INVERTED: and the body reaches the caller rather than the GC");
    }

    /// <summary>
    /// C3d — a 2xx that is not exactly 200 is delivered like any other success.
    /// </summary>
    /// <remarks>
    ///     Also not in the plan. The success check admits every 2xx, but the fallback beneath it was
    ///     gated on <c>== HttpStatusCode.OK</c>, so a 203 or a 206 with no newer <c>Last-Modified</c>
    ///     fell straight past it and was discarded. Two predicates for one question, disagreeing.
    /// </remarks>
    [TestMethod]
    public async Task A203_IsDeliveredLikeAnyOtherSuccess()
    {
        using HttpClient client = Responding(
            response => response.StatusCode = HttpStatusCode.NonAuthoritativeInformation);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue(
            "INVERTED: one predicate now decides, and it is the status code the server chose");
        result.StatusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
    }

    /// <summary>
    /// C4 — a 304 keeps the validators the server sent with it.
    /// </summary>
    /// <remarks>
    ///     RFC 9110 requires a 304 to carry the <c>ETag</c> a 200 would have carried, and servers
    ///     legitimately rotate one on a 304. Discarding it meant a polling caller re-sent a stale
    ///     validator for as long as it kept polling — the one request shape conditional GET exists
    ///     to make cheap was the one it got wrong.
    /// </remarks>
    [TestMethod]
    public async Task A304_KeepsTheValidatorsTheServerSent()
    {
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage response = new(HttpStatusCode.NotModified);
            response.Headers.ETag = new EntityTagHeaderValue("\"rotated-v2\"");
            response.Content.Headers.LastModified = SentValidator.AddDays(1);
            return Task.FromResult(response);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, "\"stale-v1\"", client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeFalse("INVARIANT: a 304 still means unmodified");
        result.ETag.ShouldBe("\"rotated-v2\"", "INVERTED: the rotated tag survives, so the next poll sends it");
        result.LastModified.ShouldBe(SentValidator.AddDays(1), "INVERTED: and so does the date");
        result.StatusCode.ShouldBe(HttpStatusCode.NotModified, "INVERTED: a 304 is now distinguishable from a heuristic decision");

        // The body accessor is untouched: a 304 has no body, and the validators arriving does not
        // change that. Asserted here rather than trusted, because the new constructor could easily
        // have retained the response to read them from.
        (await result.GetResponseStreamAsync(TestContext.CancellationTokenSource.Token))
            .ShouldBeSameAs(Stream.Null);
    }

    /// <summary>
    /// C10 — an unquoted entity tag is silently dropped, degrading the request to an unconditional GET.
    /// </summary>
    /// <remarks>
    ///     <c>IfNoneMatch.TryParseAdd</c> returns a bool that nothing checks. An entity tag has to be
    ///     quoted to be valid, so a caller who stores <c>abc123</c> rather than <c>"abc123"</c> — which
    ///     is what happens when a tag is round-tripped through a database column or a JSON field
    ///     without care — gets a full download every poll and no indication why.
    /// </remarks>
    [TestMethod]
    public async Task AnUnquotedEntityTag_IsSilentlyDroppedAndTheRequestBecomesUnconditional()
    {
        HttpRequestMessage? sent = null;
        using MockHttpMessageHandler handler = new((request, _) =>
        {
            sent = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, "abc123", client, TestContext.CancellationTokenSource.Token);

        sent.ShouldNotBeNull();
        sent.Headers.IfNoneMatch.Count.ShouldBe(0,
            "PINS TODAY: the tag is dropped without a word and the poll costs a full download.");
        sent.Headers.IfModifiedSince.ShouldNotBeNull("the date validator still goes, so it is not wholly unconditional");
    }

    /// <summary>
    /// C10, control — a properly quoted entity tag does reach the wire.
    /// </summary>
    [TestMethod]
    public async Task AQuotedEntityTag_IsSent()
    {
        HttpRequestMessage? sent = null;
        using MockHttpMessageHandler handler = new((request, _) =>
        {
            sent = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, "\"abc123\"", client, TestContext.CancellationTokenSource.Token);

        sent.ShouldNotBeNull();
        sent.Headers.IfNoneMatch.Single().Tag.ShouldBe("\"abc123\"");
    }

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership transfers twice, and the analyzer can follow neither hop. The handler is owned by the HttpClient built over it with disposeHandler: true, which every caller disposes; the content is owned by the HttpResponseMessage it is attached to, which the library disposes on the caller's behalf.")]
    private static HttpClient Responding(Action<HttpResponseMessage> configure, bool declareLength = true)
    {
        MockHttpMessageHandler handler = new((_, _) =>
        {
            ControllableHttpContent content = new(Encoding.UTF8.GetBytes("<r>body</r>"), declareLength);
            HttpResponseMessage response = content.InAResponse();
            configure(response);
            return Task.FromResult(response);
        });

        return new HttpClient(handler, disposeHandler: true);
    }
}