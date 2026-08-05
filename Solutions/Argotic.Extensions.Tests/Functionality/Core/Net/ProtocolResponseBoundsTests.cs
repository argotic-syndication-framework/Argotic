using System.Net;
using System.Net.Http.Headers;
using System.Text;

using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Net;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

/// <summary>
/// Covers how much of a protocol response the two response readers will accept.
/// </summary>
/// <remarks>
///     <para>
///     <see cref="TrackbackResponse.CreateAsync(HttpResponseMessage, CancellationToken)"/> and
///     <see cref="XmlRpcResponse.CreateAsync(HttpResponseMessage, CancellationToken)"/> are public and
///     take a response the caller supplies. Both then build an <see cref="System.Xml.XmlReader"/> over
///     it and read <b>synchronously</b>.
///     </para>
///     <para>
///     Their own callers inside this library use content-read, so the stream they see is already a
///     buffer and nothing blocks. A caller who obtained the response themselves under
///     <see cref="HttpCompletionOption.ResponseHeadersRead"/> hands them the socket instead. These
///     tests pin the bound that makes that case finite rather than the caller's problem.
///     </para>
/// </remarks>
[TestClass]
public sealed class ProtocolResponseBoundsTests
{
    private const string TrackbackBody =
        """<?xml version="1.0" encoding="utf-8"?><response><error>0</error></response>""";

    private const string XmlRpcBody =
        """<?xml version="1.0" encoding="utf-8"?><methodResponse><params><param><value><i4>7</i4></value></param></params></methodResponse>""";

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// A trackback response of ordinary size is read in full.
    /// </summary>
    /// <remarks>
    ///     The control. Without it, the refusals below are equally consistent with "the reader stopped
    ///     working", and the eleven scenario tests that would also catch that run against a client
    ///     rather than against this method directly.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnOrdinaryTrackbackResponse_IsReadInFull()
    {
        using HttpResponseMessage response = Respond(TrackbackBody);

        TrackbackResponse result = await TrackbackResponse.CreateAsync(
            response, this.TestContext.CancellationTokenSource.Token);

        result.HasError.ShouldBeFalse();
    }

    /// <summary>
    /// A trackback response declaring more than the discovery allowance is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnOversizedTrackbackResponse_IsRefused()
    {
        using HttpResponseMessage response = Respond(Oversized("response"));

        SyndicationContentTooLargeException thrown =
            await Should.ThrowAsync<SyndicationContentTooLargeException>(async () =>
                await TrackbackResponse.CreateAsync(response, this.TestContext.CancellationTokenSource.Token));

        thrown.MaxBytes.ShouldBe(SyndicationContentLengthLimits.Discovery);
    }

    /// <summary>
    /// An XML-RPC response of ordinary size is read in full.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnOrdinaryXmlRpcResponse_IsReadInFull()
    {
        using HttpResponseMessage response = Respond(XmlRpcBody);

        XmlRpcResponse result = await XmlRpcResponse.CreateAsync(
            response, this.TestContext.CancellationTokenSource.Token);

        result.Fault.ShouldBeNull();
    }

    /// <summary>
    /// An XML-RPC response declaring more than the discovery allowance is refused.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnOversizedXmlRpcResponse_IsRefused()
    {
        using HttpResponseMessage response = Respond(Oversized("methodResponse"));

        SyndicationContentTooLargeException thrown =
            await Should.ThrowAsync<SyndicationContentTooLargeException>(async () =>
                await XmlRpcResponse.CreateAsync(response, this.TestContext.CancellationTokenSource.Token));

        thrown.MaxBytes.ShouldBe(SyndicationContentLengthLimits.Discovery);
    }

    /// <summary>
    /// A response that declares no length is stopped part-way through rather than at the end.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The two refusals above are satisfied by reading the whole body and measuring it afterwards,
    ///     because a <see cref="ByteArrayContent"/> declares its length up front. Chunked transfer
    ///     encoding does not, and that is the shape the bound exists for: the size is not knowable
    ///     until it has already arrived.
    ///     </para>
    ///     <para>
    ///     So this asserts on <see cref="ControllableHttpContent.BytesRead"/>, not on the exception.
    ///     A six-megabyte body against a two-megabyte cap must leave roughly four megabytes unread.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnUndeclaredBody_StopsBeingReadAtTheCap()
    {
        byte[] body = Encoding.UTF8.GetBytes(Oversized("response", SyndicationContentLengthLimits.Discovery * 3));

        using ControllableHttpContent content = new(body, declareLength: false);
        using HttpResponseMessage response = content.InAResponse("text/xml");

        await Should.ThrowAsync<SyndicationContentTooLargeException>(async () =>
            await TrackbackResponse.CreateAsync(response, this.TestContext.CancellationTokenSource.Token));

        content.BytesRead.ShouldBeLessThan(
            SyndicationContentLengthLimits.Discovery + (256 * 1024),
            "the read must stop at the cap, not at the end of the body");

        // The control on the control: without this the assertion above also passes if nothing was
        // read at all, which is what a declared-length check that fired by mistake would look like.
        content.BytesRead.ShouldBeGreaterThan(SyndicationContentLengthLimits.Discovery);
    }

    private static string Oversized(string element, long size = 0)
    {
        // The padding sits in an attribute rather than in element text so the document stays well
        // formed at any length; what is under test is the size, not the parser.
        long length = size == 0 ? SyndicationContentLengthLimits.Discovery + 1024 : size;
        string padding = new('x', (int)length);
        return $"""<?xml version="1.0" encoding="utf-8"?><{element}><pad note="{padding}" /></{element}>""";
    }

    private static HttpResponseMessage Respond(string body)
    {
        // Constructed directly rather than through MockHttpMessageHandler: these two methods take a
        // response, so the wire is not part of what is under test.
        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)),
        };

        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
        return response;
    }
}