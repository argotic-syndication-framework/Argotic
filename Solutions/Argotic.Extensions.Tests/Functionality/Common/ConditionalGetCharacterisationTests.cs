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
    /// <see cref="EntityTagHeaderValue.ToString"/> emits the <c>W/</c> prefix for a weak tag.
    /// </summary>
    /// <remarks>
    ///     The framework claim the <c>ETag</c> projection rests on, pinned rather than assumed. The
    ///     reference documentation for <c>ToString</c> says only "a string that represents the current
    ///     object" — it does not promise the weakness indicator — so reading <c>ToString()</c> instead
    ///     of <c>Tag</c> is a fix only if this holds. Green on both sides of that change by
    ///     construction: it asserts about the BCL, not about this library.
    /// </remarks>
    [TestMethod]
    public void EntityTagHeaderValueToString_EmitsTheWeaknessPrefix()
    {
        new EntityTagHeaderValue("\"abc123\"", isWeak: true).ToString().ShouldBe("W/\"abc123\"");
        new EntityTagHeaderValue("\"abc123\"", isWeak: false).ToString().ShouldBe("\"abc123\"");
    }

    /// <summary>
    /// A weak entity tag on a <c>200</c> keeps its <c>W/</c> prefix.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>SyndicationValidators</c> states the invariant this restores: a validator "must be sent
    ///     exactly as received, quotes and any <c>W/</c> prefix included". The projection read
    ///     <c>EntityTagHeaderValue.Tag</c>, which is only the opaque quoted string — weakness lives on
    ///     the separate <c>IsWeak</c> — so <c>W/"v1"</c> arrived and <c>"v1"</c> came out.
    ///     </para>
    ///     <para>
    ///     The consequence is narrower than it looks and is worth stating so it is not overclaimed.
    ///     <c>If-None-Match</c> uses the weak comparison function, under which <c>"v1"</c> and
    ///     <c>W/"v1"</c> match, so revalidation returned <c>304</c> either way. What was actually
    ///     broken is information loss on a public property, a wrong value for <c>If-Match</c> and
    ///     <c>If-Range</c>, which compare strictly, and the library contradicting its own documented
    ///     invariant.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public async Task AWeakEntityTagOnA200_KeepsItsWeaknessPrefix()
    {
        using HttpClient client = Responding(
            response => response.Headers.ETag = new EntityTagHeaderValue("\"v1\"", isWeak: true));

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.ETag.ShouldBe("W/\"v1\"", "INVERTED: the weakness indicator is part of the validator");
    }

    /// <summary>
    /// A weak entity tag on a <c>304</c> keeps its <c>W/</c> prefix too.
    /// </summary>
    /// <remarks>
    ///     The second projection site. <c>ConditionalGetResult</c> has two constructors and both read
    ///     the header, so a fix applied to one of them would leave the polling path — the one this API
    ///     exists for — still lossy, and the 200 test above would not notice.
    /// </remarks>
    [TestMethod]
    public async Task AWeakEntityTagOnA304_KeepsItsWeaknessPrefix()
    {
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage response = new(HttpStatusCode.NotModified);
            response.Headers.ETag = new EntityTagHeaderValue("\"rotated-v2\"", isWeak: true);
            return Task.FromResult(response);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, "W/\"v1\"", client, TestContext.CancellationTokenSource.Token);

        result.ETag.ShouldBe("W/\"rotated-v2\"", "INVERTED: the next poll re-sends what the origin sent");
    }

    /// <summary>
    /// A strong entity tag is unchanged by the weak-tag fix.
    /// </summary>
    /// <remarks>
    ///     The control. Every entity tag elsewhere in this suite is strong, so without this row the two
    ///     tests above are equally consistent with "the projection now prefixes everything".
    /// </remarks>
    [TestMethod]
    public async Task AStrongEntityTag_IsCarriedThroughUnchanged()
    {
        using HttpClient client = Responding(
            response => response.Headers.ETag = new EntityTagHeaderValue("\"v1\""));

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, null, client, TestContext.CancellationTokenSource.Token);

        result.ETag.ShouldBe("\"v1\"", "INVARIANT: a strong tag has no prefix to keep");
    }

    /// <summary>
    /// C10 — an unquoted entity tag is refused rather than dropped.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>IfNoneMatch.TryParseAdd</c> returned a bool that nothing checked. An entity tag has to be
    ///     quoted to be valid, so a caller who stored <c>abc123</c> rather than <c>"abc123"</c> — which
    ///     is what happens when a tag is round-tripped through a database column or a JSON field
    ///     without care — got a full download every poll and no indication why.
    ///     </para>
    ///     <para>
    ///     The silence was the defect. Degrading to an unconditional GET does not fail: it succeeds,
    ///     returns a body, and is indistinguishable from a resource that genuinely changed. Nothing
    ///     downstream can tell the difference, so nothing downstream can report it.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public async Task AnUnquotedEntityTag_IsRefusedRatherThanDropped()
    {
        bool reached = false;
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            reached = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
        });
        using HttpClient client = new(handler, disposeHandler: false);

        await Should.ThrowAsync<FormatException>(async () => await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SentValidator, "abc123", client, TestContext.CancellationTokenSource.Token));

        reached.ShouldBeFalse("INVERTED: the request is not sent at all, rather than sent unconditionally");
    }

    /// <summary>
    /// The framework User-Agent is emitted exactly once.
    /// </summary>
    /// <remarks>
    ///     Conditional GET built its request by hand and set the User-Agent itself, while every other
    ///     fetch in the library goes through <c>CreateHttpRequestMessage</c>, which also sets it. Now
    ///     that this path routes through that method, a surviving copy of the old line would append a
    ///     second product token — and <c>SyndicationRequestOptions.ApplyTo</c> clears the collection
    ///     before setting a custom one, so the duplicate would be invisible to any test that supplies
    ///     options. This one supplies none.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task TheFrameworkUserAgentIsSentExactlyOnce()
    {
        HttpRequestMessage? sent = null;
        using MockHttpMessageHandler handler = new((request, _) =>
        {
            sent = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, SyndicationValidators.None, client, null, TestContext.CancellationTokenSource.Token);

        sent.ShouldNotBeNull();
        sent.Headers.UserAgent.ToString().ShouldBe(SyndicationDiscoveryUtility.FrameworkUserAgent);
    }

    /// <summary>
    /// Conditional GET can now carry the request options every other fetch could.
    /// </summary>
    /// <remarks>
    ///     It was the one fetch in this library that could not be given an <c>Accept</c> header, because
    ///     it constructed its own request instead of going through the shared builder. Nothing about
    ///     conditional GET made that true; it was simply the path the options plumbing never reached.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RequestOptionsReachTheConditionalRequest()
    {
        HttpRequestMessage? sent = null;
        using MockHttpMessageHandler handler = new((request, _) =>
        {
            sent = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source,
            new SyndicationValidators(SentValidator, "\"v1\""),
            client,
            new SyndicationRequestOptions { Accept = "application/rss+xml", UserAgent = "Mine/1.0" },
            TestContext.CancellationTokenSource.Token);

        sent.ShouldNotBeNull();
        sent.Headers.Accept.ToString().ShouldBe("application/rss+xml");
        sent.Headers.UserAgent.ToString().ShouldBe("Mine/1.0");
        sent.Headers.IfNoneMatch.ToString().ShouldBe("\"v1\"");
        sent.Headers.IfModifiedSince.ShouldNotBeNull();
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