using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;

using Shouldly;

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
}