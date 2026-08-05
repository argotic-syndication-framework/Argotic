using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins how much of a response this library downloads, and when.
/// </summary>
/// <remarks>
///     <para>
///     Every HTTP path here uses <see cref="HttpCompletionOption.ResponseContentRead"/>, which is the
///     default, so the whole body is buffered inside <c>SendAsync</c> before any Argotic code sees a
///     stream. That is invisible to a handler — a handler returns before the buffering happens — and it
///     had no test. The observable is <see cref="ControllableHttpContent.WasRead"/>, set by
///     <c>LoadIntoBufferAsync</c>, which runs under content-read completion and not under headers-read.
///     </para>
///     <para>
///     These are the rows a switch to headers-read inverts, and each one names the phase that inverts
///     it. Asserting on elapsed time instead would be flaky in the direction that gets tests deleted.
///     </para>
/// </remarks>
[TestClass]
public sealed class HttpBufferingCharacterisationTests
{
    private static readonly Uri Source = new("http://characterisation.invalid/feed.xml");

    private const string Feed = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0"><channel><title>Buffered</title><link>http://example.com/</link>
        <description>d</description></channel></rss>
        """;

    /// <summary>
    /// C1 — the body is fully buffered before the request completes.
    /// </summary>
    /// <remarks>
    ///     Deterministic, not timed: <c>WasRead</c> is set by <c>LoadIntoBufferAsync</c> exactly once
    ///     under content-read completion, and never under headers-read. Inverted at Phase 5.
    /// </remarks>
    [TestMethod]
    public async Task TheWholeBody_IsBufferedBeforeTheRequestCompletes()
    {
        ControllableHttpContent content = new(Encoding.UTF8.GetBytes(Feed));
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse()));
        using HttpClient client = new(handler, disposeHandler: false);

        await SyndicationEncodingUtility.CreateSafeNavigatorAsync(Source, client, null, null, TestContext.CancellationTokenSource.Token);

        content.WasRead.ShouldBeTrue("PINS TODAY: the body is drained inside SendAsync. Inverted at Phase 5.");
        content.BytesRead.ShouldBe(content.Length);
    }

    /// <summary>
    /// C5 — a successful response is reported as existing whether or not it declared a length.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>This corrects the modernisation plan.</b> The plan states that <c>UriExistsAsync</c>'s
    ///     <c>contentLength &gt; 0</c> test "returns <c>false</c> for every gzip'd 200 through the
    ///     shared client", and schedules a fix. Measured, it does not: buffering happens first, and a
    ///     buffered <see cref="HttpContent"/> reports its <i>buffer's</i> length from
    ///     <c>Headers.ContentLength</c> whatever the origin sent. Under the completion option this
    ///     library uses today, <c>ContentLength</c> is never null and the predicate is correct.
    ///     </para>
    ///     <para>
    ///     <b>The defect is one Phase 5 would introduce, not one it would repair</b>, and that changes
    ///     what this row is for. It is an <i>invariant</i>, not something to invert: switching
    ///     <c>UriExistsAsync</c> to headers-read without also changing the predicate in the same commit
    ///     turns this green test red. See <see cref="ContentLengthIsOnlyNull_UnderHeadersReadCompletion"/>
    ///     for the mechanism.
    ///     </para>
    /// </remarks>
    /// <param name="declareLength">Whether the response declares <c>Content-Length</c>.</param>
    [TestMethod]
    [DataRow(true, DisplayName = "Content-Length declared")]
    [DataRow(false, DisplayName = "Content-Length absent, as on any decompressed or chunked response")]
    public async Task ASuccessfulResponse_IsReportedAsExisting(bool declareLength)
    {
        ControllableHttpContent content = new(Encoding.UTF8.GetBytes(Feed), declareLength);
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse()));
        using HttpClient client = new(handler, disposeHandler: false);

        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(Source, client, TestContext.CancellationTokenSource.Token);

        exists.ShouldBeTrue("INVARIANT, not a pin. Phase 5 must keep this true while switching to headers-read.");
    }

    /// <summary>
    /// The mechanism behind C5, isolated: <c>ContentLength</c> is only ever null under headers-read.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Buffering populates the length. <c>HttpContent</c> reports its buffered length in preference
    ///     to consulting <c>TryComputeLength</c>, so under <see cref="HttpCompletionOption.ResponseContentRead"/>
    ///     a response that declared nothing still reports a length — which is precisely why
    ///     <c>UriExistsAsync</c> works today on decompressed and chunked responses.
    ///     </para>
    ///     <para>
    ///     Under <see cref="HttpCompletionOption.ResponseHeadersRead"/> nothing has been buffered, so an
    ///     undeclared length really is null. This test is the tripwire: it is the one place the two
    ///     completion options are compared side by side, and it makes the Phase 5 ordering constraint a
    ///     fact in the suite rather than a sentence in a plan.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public async Task ContentLengthIsOnlyNull_UnderHeadersReadCompletion()
    {
        static async Task<long?> LengthSeenBy(HttpCompletionOption option, bool declareLength)
        {
            ControllableHttpContent content = new(Encoding.UTF8.GetBytes("<r>hello</r>"), declareLength);
            using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse()));
            using HttpClient client = new(handler, disposeHandler: false);
            using HttpRequestMessage request = new(HttpMethod.Get, Source);
            using HttpResponseMessage response = await client.SendAsync(request, option, CancellationToken.None);
            return response.Content.Headers.ContentLength;
        }

        (await LengthSeenBy(HttpCompletionOption.ResponseContentRead, declareLength: true)).ShouldNotBeNull();
        (await LengthSeenBy(HttpCompletionOption.ResponseContentRead, declareLength: false))
            .ShouldNotBeNull("buffering supplies the length the origin did not");

        (await LengthSeenBy(HttpCompletionOption.ResponseHeadersRead, declareLength: true)).ShouldNotBeNull();
        (await LengthSeenBy(HttpCompletionOption.ResponseHeadersRead, declareLength: false))
            .ShouldBeNull("nothing has been buffered, so an undeclared length stays unknown");
    }

    /// <summary>
    /// C9 — a twelve-mebibyte feed loads today, with no cap of any kind.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Phase 5 introduces an 8 MiB default for feeds, so this becomes a throw and a caller wanting
    ///     the old behaviour must ask for it. Pinning it first is what makes that a breaking change with
    ///     a recorded before, rather than a surprise in someone's newsletter pipeline.
    ///     </para>
    ///     <para>
    ///     Generated rather than written as a literal: this repository keeps fixtures as raw string
    ///     literals in <c>FeedTestData</c>, and twelve mebibytes is where that convention stops being
    ///     sensible. It is also <c>[DoNotParallelize]</c>, because method-level parallelism plus a
    ///     24 MiB string plus an <c>XPathDocument</c> is a lot of headroom to ask of every other test
    ///     running beside it.
    ///     </para>
    /// </remarks>
    [TestMethod]
    [DoNotParallelize]
    public async Task ATwelveMebibyteFeed_LoadsWithNoCap()
    {
        byte[] body = GenerateFeedOfAtLeast(12 * 1024 * 1024);
        body.Length.ShouldBeGreaterThan(12 * 1024 * 1024);

        ControllableHttpContent content = new(body);
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse()));
        using HttpClient client = new(handler, disposeHandler: false);

        Argotic.Syndication.RssFeed feed = await Argotic.Syndication.RssFeed.CreateAsync(
            Source, client, null, null, TestContext.CancellationTokenSource.Token);

        feed.Channel.Items.Count.ShouldBeGreaterThan(0,
            "PINS TODAY: nothing bounds the size of a downloaded feed. Inverted at Phase 5.");
    }

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    private static byte[] GenerateFeedOfAtLeast(int minimumBytes)
    {
        StringBuilder builder = new(minimumBytes + 4_096);
        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<rss version=\"2.0\"><channel>\n");
        builder.Append("<title>Large</title><link>http://example.com/</link><description>d</description>\n");

        for (int i = 0; builder.Length < minimumBytes; i++)
        {
            builder.Append("<item><title>Item ").Append(i).Append("</title><link>http://example.com/")
                   .Append(i).Append("</link><description>")
                   .Append('x', 512).Append("</description></item>\n");
        }

        builder.Append("</channel></rss>");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}