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
///     Four tests covered <c>ConditionalGetAsync</c> before today, and both of the ones about
///     modification take the primary branch — the response carries a genuinely newer
///     <c>Last-Modified</c>. The fallback underneath it, which is where the interesting behaviour is,
///     had never run. §2.19 records the method at 12 of 18 branches.
///     </para>
///     <para>
///     Phase 7 replaces the whole decision with "304 means unmodified, any 2xx means modified". These
///     rows are what that replacement has to be measured against, and two of them are worse than the
///     plan describes.
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
    ///     entirely from the fallback, which asks whether the response has a body or a content type.
    ///     Almost every response does. Inverted at Phase 7 — where the answer stays <c>true</c> but for
    ///     a reason that is actually about the status code.
    /// </remarks>
    [TestMethod]
    public async Task A200WithNoLastModified_IsReportedAsModified()
    {
        using HttpClient client = Responding(response => { });

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue("PINS TODAY: decided by the fallback, not by the comparison.");
    }

    /// <summary>
    /// C3b — a 200 whose <c>Last-Modified</c> is <i>older</i> than the one we sent is still modified.
    /// </summary>
    /// <remarks>
    ///     This is the row that shows the comparison is dead code. The server has said, in effect,
    ///     "this representation is older than the one you already have", and the fallback overrides it
    ///     because the response carries a content type.
    /// </remarks>
    [TestMethod]
    public async Task A200WithAnOlderLastModified_IsStillReportedAsModified()
    {
        using HttpClient client = Responding(response =>
            response.Content.Headers.LastModified = SentValidator.AddDays(-30));

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue(
            "PINS TODAY: the Last-Modified comparison never decides anything, because the fallback "
            + "fires on any response carrying a content type. Inverted at Phase 7.");
    }

    /// <summary>
    /// C3c — a 200 carrying neither a content type nor a length is reported unmodified, and its body
    /// is <b>discarded</b>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Not in the plan, and worse than what is. The plan describes the heuristic as "nonsense"
    ///     because the comparison is dead; the malignant half is here. When both fallback conditions
    ///     are false the response is disposed and the caller is told nothing changed — so a real 200
    ///     with a real body is thrown away and reported as a cache hit.
    ///     </para>
    ///     <para>
    ///     A chunked response with no <c>Content-Type</c> is exactly this shape. Phase 7 deletes the
    ///     block, and this becomes a modified result with its body intact.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public async Task A200WithNoContentTypeAndNoLength_IsReportedUnmodifiedAndItsBodyDiscarded()
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

        result.WasModified.ShouldBeFalse(
            "PINS TODAY: a real 200 with a real body is reported as unmodified and discarded. "
            + "Inverted at Phase 7.");
        result.GetResponseStream().ShouldBe(Stream.Null, "the body is gone");
    }

    /// <summary>
    /// C3d — a 2xx that is not exactly 200 is reported unmodified, and its body is discarded too.
    /// </summary>
    /// <remarks>
    ///     Also not in the plan. The success check earlier in the method lets 203 through, but the
    ///     fallback is gated on <c>== HttpStatusCode.OK</c>, so a 203 or a 206 with no newer
    ///     <c>Last-Modified</c> falls straight past it. <c>IsSuccessStatusCode</c> is the predicate
    ///     that was meant.
    /// </remarks>
    [TestMethod]
    public async Task A203_IsReportedUnmodifiedAndItsBodyDiscarded()
    {
        using HttpClient client = Responding(
            response => response.StatusCode = HttpStatusCode.NonAuthoritativeInformation);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeFalse(
            "PINS TODAY: the fallback is gated on == OK, so every other 2xx is discarded. "
            + "Inverted at Phase 7.");
    }

    /// <summary>
    /// C4 — a 304 discards the validators the server sent with it.
    /// </summary>
    /// <remarks>
    ///     RFC 9110 requires a 304 to carry the <c>ETag</c> a 200 would have carried, and servers
    ///     legitimately rotate one on a 304. Discarding it means a polling caller re-sends a stale
    ///     validator for as long as it keeps polling. Inverted at Phase 7.
    /// </remarks>
    [TestMethod]
    public async Task A304_DiscardsTheValidatorsTheServerSent()
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

        result.WasModified.ShouldBeFalse();
        result.ETag.ShouldBeNull("PINS TODAY: the rotated tag is thrown away. Inverted at Phase 7.");
        result.LastModified.ShouldBeNull("PINS TODAY: so is the date. Inverted at Phase 7.");
        result.StatusCode.ShouldBeNull("PINS TODAY: a 304 is indistinguishable from a heuristic decision. Inverted at Phase 7.");
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