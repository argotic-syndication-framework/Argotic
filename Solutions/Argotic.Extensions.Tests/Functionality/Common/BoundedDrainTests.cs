namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the bounded drain and the first two sites converted to use it.
/// </summary>
[TestClass]
public sealed class BoundedDrainTests
{
    private static readonly Uri Source = new("http://drain.invalid/page.html");

    private const string PageWithPingbackLink =
        """<html><head><link rel="pingback" href="http://drain.invalid/xmlrpc" /></head><body>x</body></html>""";

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// N2 — a pingback answered from a header never reads the body.
    /// </summary>
    /// <param name="pageSize">The size of the body the origin would have sent.</param>
    /// <remarks>
    ///     <para>
    ///     The most clearly wasteful thing the discovery code did: it asked whether a page supports
    ///     pingback, got the answer from an <c>X-Pingback</c> header, and had already downloaded the
    ///     whole page to get there. Under content-read completion the body is buffered inside
    ///     <c>SendAsync</c> before any Argotic code runs, so the waste was invisible from the call site.
    ///     </para>
    ///     <para>
    ///     Parameterised by size to make the point that the saving scales with the page rather than
    ///     being a constant: <c>WasRead</c> is false either way, and it would have been true either way
    ///     before.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    [DataRow(64, DisplayName = "a small page")]
    [DataRow(512 * 1024, DisplayName = "half a megabyte")]
    public async Task APingbackAnsweredFromAHeader_NeverReadsTheBody(int pageSize)
    {
        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(new string('x', pageSize)));
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage response = content.InAResponse("text/html");
            response.Headers.Add("X-Pingback", "http://drain.invalid/xmlrpc");
            return Task.FromResult(response);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        bool enabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        enabled.ShouldBeTrue();
        content.WasRead.ShouldBeFalse("the header answered the question; the body is never touched");
        content.BytesRead.ShouldBe(0);
    }

    /// <summary>
    /// The fall-through still works: no header means the body is read and parsed.
    /// </summary>
    /// <remarks>
    ///     Without this, the row above is equally consistent with "pingback detection stopped reading
    ///     bodies at all", which would be a regression wearing the same test result.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task APingbackWithNoHeader_StillFindsTheLinkInTheBody()
    {
        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(PageWithPingbackLink));
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        Uri? server = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        server.ShouldNotBeNull();
        content.WasRead.ShouldBeTrue("no header, so the body genuinely has to be read");
    }

    /// <summary>
    /// Asking whether a URI exists no longer downloads what is behind it.
    /// </summary>
    /// <param name="declareLength">Whether the response declares <c>Content-Length</c>.</param>
    /// <remarks>
    ///     <para>
    ///     <c>UriExistsAsync</c> never reads the body — it only ever looked at the status and the
    ///     length — but under content-read completion the body was buffered inside <c>SendAsync</c>
    ///     before the method got a chance not to read it. Asking "does this feed still exist?" cost a
    ///     full download of the feed.
    ///     </para>
    ///     <para>
    ///     The undeclared row is the one that matters: it is every decompressed and every chunked
    ///     response, and it is the row that would have started answering "does not exist" had the
    ///     completion option changed without the predicate changing with it.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    [DataRow(true, DisplayName = "Content-Length declared")]
    [DataRow(false, DisplayName = "Content-Length absent")]
    public async Task AskingWhetherAUriExists_NeverReadsTheBody(bool declareLength)
    {
        using ControllableHttpContent content = new(
            Encoding.UTF8.GetBytes(new string('x', 256 * 1024)), declareLength);
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        exists.ShouldBeTrue("INVARIANT: a successful response means the URI exists, declared length or not");
        content.WasRead.ShouldBeFalse("nothing here ever needed the body");
        content.BytesRead.ShouldBe(0);
    }

    /// <summary>
    /// An empty body still means the URI does not exist.
    /// </summary>
    /// <remarks>
    ///     The one thing the old <c>&gt; 0</c> predicate got right, and the reason the new one is
    ///     <c>!= 0</c> rather than dropped altogether: a declared length of zero is a real answer, where
    ///     an absent length is the absence of one.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AZeroLengthBody_StillMeansTheUriDoesNotExist()
    {
        using ControllableHttpContent content = new([]);
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        exists.ShouldBeFalse();
    }

    /// <summary>
    /// N1 — a declared length over the cap is refused without opening the body stream.
    /// </summary>
    /// <remarks>
    ///     The cheap half of the defence, and the one that only works on an identity-encoded response.
    ///     <c>BytesRead == 0</c> is the assertion that matters: refusing after reading would have spent
    ///     the memory the cap exists to save.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ADeclaredLengthOverTheCap_IsRefusedWithoutReadingTheBody()
    {
        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(new string('x', 4 * 1024 * 1024)));
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        SyndicationContentTooLargeException thrown = await Should.ThrowAsync<SyndicationContentTooLargeException>(
            async () => await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
                Source, client, TestContext.CancellationTokenSource.Token));

        content.WasRead.ShouldBeFalse("refused before the body stream was opened");
        content.BytesRead.ShouldBe(0);
        thrown.MaxBytes.ShouldBe(SyndicationContentLengthLimits.Discovery);
        thrown.DeclaredLength.ShouldBe(4L * 1024 * 1024);
    }

    /// <summary>
    /// N1b — a body that declares no length is stopped part-way through, not after.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>This is the case that actually matters.</b> Automatic decompression strips
    ///     <c>Content-Length</c> from every response it decompresses, so on a compressing origin the
    ///     declared-length check never fires and the streaming counter is the only bound there is.
    ///     </para>
    ///     <para>
    ///     <c>BytesRead</c> being both non-zero and well under the total is the whole assertion: it says
    ///     the read started and was abandoned, rather than completing and being judged afterwards.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnUndeclaredBodyOverTheCap_IsAbandonedPartWayThrough()
    {
        using ControllableHttpContent content = new(
            Encoding.UTF8.GetBytes(new string('x', 4 * 1024 * 1024)), declareLength: false);
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        SyndicationContentTooLargeException thrown = await Should.ThrowAsync<SyndicationContentTooLargeException>(
            async () => await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
                Source, client, TestContext.CancellationTokenSource.Token));

        thrown.DeclaredLength.ShouldBeNull("nothing was declared, which is why the counter had to do it");
        thrown.MaxBytes.ShouldBe(SyndicationContentLengthLimits.Discovery);
        content.BytesRead.ShouldBeLessThan(content.Length, "the read was abandoned rather than completed");
    }

    /// <summary>
    /// A body inside the cap is read in full and produces the same answer as before.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ABodyInsideTheCap_IsReadInFull()
    {
        byte[] page = Encoding.UTF8.GetBytes(PageWithPingbackLink);
        using ControllableHttpContent content = new(page);
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse("text/html")));
        using HttpClient client = new(handler, disposeHandler: false);

        Uri? server = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        server.ShouldNotBeNull();
        content.BytesRead.ShouldBe(page.Length);
    }

    /// <summary>
    /// Detecting a feed's format reads its head rather than the whole feed.
    /// </summary>
    /// <remarks>
    ///     Format detection needs the document's first element. It used to download a five-megabyte
    ///     feed to learn the word <c>rss</c> — everything after the first element was read only because
    ///     nothing stopped it.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task DetectingAFeedsFormat_ReadsOnlyItsHead()
    {
        StringBuilder builder = new();
        builder.Append("""<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel>""");
        builder.Append("<title>Large</title>");
        builder.Append("<description>").Append('x', 5 * 1024 * 1024).Append("</description>");
        builder.Append("</channel></rss>");

        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(builder.ToString()));
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse()));
        using HttpClient client = new(handler, disposeHandler: false);

        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        format.ShouldBe(SyndicationContentFormat.Rss);
        content.BytesRead.ShouldBeLessThanOrEqualTo(64 * 1024, "the first element is all it needs");
        content.BytesRead.ShouldBeLessThan(content.Length / 10, "and that is a small fraction of the feed");
    }

    /// <summary>
    /// A prolog longer than the probe window reports an undetermined format rather than throwing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     A deliberate degrade, and the one behaviour change the bound brings. Truncating at 64 KiB can
    ///     cut a document mid-declaration, and the parser rightly objects; both overloads document
    ///     <see cref="SyndicationContentFormat.None"/> as meaning "unable to determine the format", so
    ///     answering that is in contract.
    ///     </para>
    ///     <para>
    ///     It is also the shape the bound exists for: a prolog over 64 KiB is a DTD bomb, and
    ///     <c>DtdProcessing.Parse</c> would otherwise have parsed it straight off the socket.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task APrologLongerThanTheProbeWindow_ReportsAnUndeterminedFormat()
    {
        StringBuilder builder = new();
        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!DOCTYPE rss [\n");
        for (int i = 0; builder.Length < 96 * 1024; i++)
        {
            builder.Append("  <!ENTITY padding").Append(i).Append(" \"").Append('y', 256).Append("\">\n");
        }

        builder.Append("]>\n<rss version=\"2.0\"><channel><title>x</title></channel></rss>");

        using ControllableHttpContent content = new(Encoding.UTF8.GetBytes(builder.ToString()));
        using MockHttpMessageHandler handler = new((_, _) => Task.FromResult(content.InAResponse()));
        using HttpClient client = new(handler, disposeHandler: false);

        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(
            Source, client, TestContext.CancellationTokenSource.Token);

        format.ShouldBe(SyndicationContentFormat.None,
            "in contract: both overloads document None as 'unable to determine'");
        content.BytesRead.ShouldBeLessThanOrEqualTo(64 * 1024, "and it stopped reading rather than parsing on");
    }

    /// <summary>
    /// The exception is catchable as an <see cref="HttpRequestException"/>.
    /// </summary>
    /// <remarks>
    ///     This library's own documentation tells callers to catch <see cref="HttpRequestException"/>
    ///     around a load, so an exception outside that hierarchy would escape every handler written
    ///     against the documented advice. Deriving from <see cref="IOException"/> instead — the obvious
    ///     alternative — would have done exactly that.
    /// </remarks>
    [TestMethod]
    public void TheExceptionIsCatchableAsAnHttpRequestException()
    {
        SyndicationContentTooLargeException thrown = new(1_024, 2_048);

        thrown.ShouldBeAssignableTo<HttpRequestException>();
        thrown.HttpRequestError.ShouldBe(HttpRequestError.ConfigurationLimitExceeded);
        thrown.Message.ShouldContain("2048");
        thrown.Message.ShouldContain("No part of the body was read");

        new SyndicationContentTooLargeException(1_024, null)
            .Message.ShouldContain("declared no length");
    }

    /// <summary>
    /// A conditional GET has read nothing when it returns.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Written the other way round, asserting that everything had already been downloaded, because
    ///     that is what it did. <c>ConditionalGetAsync</c> sent under the default completion option, so
    ///     the whole body was copied into memory inside <c>SendAsync</c> — before any Argotic code ran
    ///     and before the caller saw a header. Unbounded <i>and</i> eager, which is the worse of the two:
    ///     a caller who read <c>ContentLength</c>, decided the body was too large and disposed had
    ///     already paid for all of it.
    ///     </para>
    ///     <para>
    ///     What kept it that way was the modification heuristic. It read <c>ContentLength</c> and
    ///     <c>ContentType</c> off the response to decide whether anything had changed, and under
    ///     headers-read <c>ContentLength</c> is null for every chunked reply — so completing on headers
    ///     would have converted a real body into a discarded one on exactly the responses least likely
    ///     to declare a length. Deleting the heuristic removed the constraint, not by intending to.
    ///     </para>
    ///     <para>
    ///     So this asserts the opposite of what it used to, on the same fixture: eight megabytes past
    ///     the feed limit are offered, and <b>none</b> of them are read. There is still no cap here and
    ///     there should not be — the type hands out a stream. It is now genuinely a stream.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AConditionalGetHasReadNothingWhenItReturns()
    {
        byte[] body = Encoding.UTF8.GetBytes(new string('x', (int)SyndicationContentLengthLimits.Feed + 4_096));

        using ControllableHttpContent content = new(body, declareLength: false);
        using MockHttpMessageHandler handler = new((_, _) =>
        {
            HttpResponseMessage response = content.InAResponse("application/xml");
            response.Content.Headers.LastModified = DateTimeOffset.UtcNow;
            return Task.FromResult(response);
        });
        using HttpClient client = new(handler, disposeHandler: false);

        using ConditionalGetResult result = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            Source, DateTime.UtcNow.AddDays(-1), null, client, this.TestContext.CancellationTokenSource.Token);

        result.WasModified.ShouldBeTrue();
        content.WasRead.ShouldBeFalse("INVERTED: the body is the caller's to read, or not");
        result.ContentLength.ShouldBe(
            -1, "INVERTED: was the buffered length; a response declaring none now reports none");

        // And it is all still there to be read, which is the point of not having read it.
        using Stream stream = await result.GetResponseStreamAsync(this.TestContext.CancellationTokenSource.Token);
        using MemoryStream drained = new();
        await stream.CopyToAsync(drained, this.TestContext.CancellationTokenSource.Token);
        drained.Length.ShouldBe(body.Length);
        content.WasRead.ShouldBeTrue("and reading it is what reads it");
    }
}