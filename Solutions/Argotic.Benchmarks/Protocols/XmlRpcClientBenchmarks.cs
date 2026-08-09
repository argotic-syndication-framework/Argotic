using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;

using Argotic.Net;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Protocols;

/// <summary>
/// Decomposes <see cref="XmlRpcClient.SendAsync(XmlRpcMessage, CancellationToken)"/> into serialise,
/// round trip and parse.
/// </summary>
/// <remarks>
/// <para>
/// Every wire-touching method in <c>Argotic.Net</c> is at 0.000 coverage and none has ever been
/// benchmarked. <c>SendAsync</c> is the whole of the client's public surface for doing anything: it
/// serialises a <see cref="XmlRpcMessage"/> to XML, POSTs it, and hands the response body to
/// <see cref="XmlRpcResponse.CreateAsync(HttpResponseMessage, CancellationToken)"/>. Three separate
/// bodies of code, one public call, no numbers for any of it.
/// </para>
/// <para>
/// The transport is a stub handler, and no socket is opened. This follows
/// <c>PollingSteadyStateBenchmarks</c> for the same reason: a real socket adds scheduler and kernel
/// noise to a measurement whose subject is the serialiser and the parser, and BenchmarkDotNet cannot
/// separate the two. What is measured is everything the library does with a request before it hands
/// it to <see cref="HttpClient"/> and everything it does with a response once the bytes are
/// available. It is also what keeps the harness offline: nothing here can reach a network, by
/// construction rather than by configuration.
/// </para>
/// <para>
/// The arms nest, so cost can be attributed rather than merely observed:
/// </para>
/// <list type="bullet">
///   <item><description><c>serialise ⊂ send</c> — every send arm writes the same message first.</description></item>
///   <item><description><c>parse ⊂ send (struct)</c> — the parse arm reads the same body the struct
///   send arm receives.</description></item>
///   <item><description>The residual, <c>send − serialise − parse</c>, is
///   <see cref="HttpClient"/> plus the per-call <see cref="CancellationTokenSource"/> that
///   <c>SendAsync</c> creates to enforce its own timeout. If that residual is large, the finding is
///   about the client's plumbing rather than about XML.</description></item>
/// </list>
/// <para>
/// The parse arm builds its own <see cref="HttpResponseMessage"/> inside the measured region and has
/// to: <c>SyndicationEncodingUtility.ReadContentAsync</c> disposes the content stream, so a response
/// hoisted into <c>[GlobalSetup]</c> would throw on the second invocation. The construction is a
/// header dictionary and a wrapper over an existing array, and it is charged to every invocation
/// equally, so it shifts the arm's floor without changing what its slope means.
/// </para>
/// <para>
/// No <c>[Params]</c>. Each arm is a fixed response shape — scalar, struct, fault — and a
/// class-scoped size axis would reproduce the serialise arm and the fault arm unchanged at every
/// value. Payload scaling is <see cref="XmlRpcCompositeParsingBenchmarks"/>'s job.
/// </para>
/// </remarks>
[BenchmarkCategory("protocols", "xmlrpc", "client")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class XmlRpcClientBenchmarks : IDisposable
{
    private static readonly Uri ScalarHost = new("http://benchmark.invalid/xmlrpc/scalar");

    private static readonly Uri StructHost = new("http://benchmark.invalid/xmlrpc/struct");

    private static readonly Uri FaultHost = new("http://benchmark.invalid/xmlrpc/fault");

    private HttpClient client = null!;
    private StubHandler handler = null!;
    private XmlRpcClient scalarClient = null!;
    private XmlRpcClient structClient = null!;
    private XmlRpcClient faultClient = null!;
    private XmlRpcMessage message = null!;
    private byte[] structResponseBody = [];

    /// <summary>
    /// Disposes the stubbed transport.
    /// </summary>
    /// <remarks>
    ///     The full pattern rather than a sealed type with a simple <c>Dispose</c>: BenchmarkDotNet
    ///     rejects a sealed benchmark class outright — "Declaring type must be unsealed" — and does so
    ///     at validation time, after <c>--list</c> has already reported the benchmark as present.
    /// </remarks>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the stubbed transport.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.client?.Dispose();
            this.handler?.Dispose();
        }
    }

    /// <summary>
    /// Builds the stubbed client, one <see cref="XmlRpcClient"/> per response shape, and the request.
    /// </summary>
    /// <remarks>
    ///     Three clients rather than one whose <c>Host</c> is reassigned per invocation: assigning the
    ///     property inside the measured region would put a property setter and a <c>Uri</c> comparison
    ///     into every send arm, and the stub needs the URL to know which body to return.
    /// </remarks>
    [GlobalSetup]
    public void Setup()
    {
        this.structResponseBody = XmlRpcCorpus.MethodResponseUtf8(XmlRpcCorpus.StructValue(20));

        this.handler = new StubHandler(
            XmlRpcCorpus.MethodResponseUtf8(XmlRpcCorpus.TypedScalar("i4", "1138")),
            this.structResponseBody,
            Encoding.UTF8.GetBytes(XmlRpcCorpus.FaultResponse()));

        this.client = new HttpClient(this.handler, disposeHandler: false);
        this.scalarClient = new XmlRpcClient(ScalarHost, this.client);
        this.structClient = new XmlRpcClient(StructHost, this.client);
        this.faultClient = new XmlRpcClient(FaultHost, this.client);

        // weblogUpdates.ping is the two-parameter call this library exists to make; metaWeblog calls
        // carry more, but the message writer's cost is per parameter and two is the honest floor.
        this.message = XmlRpcCorpus.MethodCall("weblogUpdates.ping", 2);
    }

    /// <summary>
    /// Disposes the stubbed transport once the class is finished with.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup() => this.Dispose();

    /// <summary>
    /// Serialising the request only: no HTTP, no response.
    /// </summary>
    /// <returns>The number of bytes written, so the work cannot be elided.</returns>
    /// <remarks>
    ///     The innermost measured point, and the same <see cref="XmlWriterSettings"/>
    ///     <c>XmlRpcClient</c> uses internally — indented, with a declaration, at the message's own
    ///     encoding. Contained in every send arm.
    /// </remarks>
    [Benchmark(Description = "a. serialise the methodCall only")]
    public int SerialiseMessage()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Document,
            Encoding = this.message.Encoding,
            Indent = true,
            OmitXmlDeclaration = false,
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.message.WriteTo(writer);
        }

        return (int)stream.Length;
    }

    /// <summary>
    /// Parsing a struct response only: no request, no HTTP.
    /// </summary>
    /// <returns>The parsed response.</returns>
    /// <remarks>
    ///     Contained in <see cref="SendStructResponse"/>. This is where the document is built —
    ///     <see cref="XmlReader"/>, <see cref="System.Xml.XPath.XPathDocument"/> and the pooled content
    ///     buffer — on top of the value walk that <see cref="XmlRpcValueParsingBenchmarks"/> measures
    ///     from a navigator that already exists.
    /// </remarks>
    [Benchmark(Description = "b. XmlRpcResponse.CreateAsync over a struct body")]
    public async Task<XmlRpcResponse> ParseStructResponse()
    {
        using HttpResponseMessage response = StubHandler.CreateResponse(this.structResponseBody);
        return await XmlRpcResponse.CreateAsync(response).ConfigureAwait(false);
    }

    /// <summary>
    /// The whole call against a scalar response: serialise, POST, parse.
    /// </summary>
    /// <returns>The response.</returns>
    /// <remarks>
    ///     The baseline. This is the smallest complete thing a consumer can do with the client, so
    ///     every other send arm's ratio reads as the price of a larger response rather than of using
    ///     the client at all.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "c. SendAsync, scalar response")]
    public Task<XmlRpcResponse> SendScalarResponse() => this.scalarClient.SendAsync(this.message);

    /// <summary>
    /// The whole call against a twenty-member struct response.
    /// </summary>
    /// <returns>The response.</returns>
    /// <remarks>
    ///     Twenty members is a <c>metaWeblog.getPost</c> reply, not a stress case. The delta against
    ///     the scalar arm is the response body's contribution; the delta against
    ///     <see cref="ParseStructResponse"/> is everything the client does around the parse.
    /// </remarks>
    [Benchmark(Description = "d. SendAsync, 20-member struct response")]
    public Task<XmlRpcResponse> SendStructResponse() => this.structClient.SendAsync(this.message);

    /// <summary>
    /// The whole call against a fault response.
    /// </summary>
    /// <returns>The response.</returns>
    /// <remarks>
    ///     <para>
    ///     A fault is not an exception in this library: <c>XmlRpcResponse.Load</c> takes a different
    ///     branch and populates <c>Fault</c> through <c>XmlRpcStructureValue.Load</c>, and the caller
    ///     is expected to check. Both the branch and the type it reaches were at zero coverage, so a
    ///     harness that only ever sends a successful call measures half of what a client actually
    ///     meets — every rate-limited or rejected ping arrives this way.
    ///     </para>
    ///     <para>
    ///     The harness's <c>ExceptionDiagnoser</c> column is the thing to read on this arm: it should
    ///     be zero. A non-zero value would mean a fault costs a throw somewhere on the path, which is
    ///     the kind of cost no timing number explains on its own.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "e. SendAsync, fault response")]
    public Task<XmlRpcResponse> SendFaultResponse() => this.faultClient.SendAsync(this.message);

    /// <summary>
    /// Returns a fixed body per request URL, so the measurement is the library rather than a network.
    /// </summary>
    /// <param name="scalarBody">The body returned for the scalar host.</param>
    /// <param name="structBody">The body returned for the struct host.</param>
    /// <param name="faultBody">The body returned for the fault host.</param>
    private sealed class StubHandler(byte[] scalarBody, byte[] structBody, byte[] faultBody) : HttpMessageHandler
    {
        /// <summary>
        /// Wraps a body in the response an XML-RPC server would return.
        /// </summary>
        /// <param name="body">The response body.</param>
        /// <returns>A 200 response carrying <paramref name="body"/> as <c>text/xml</c>.</returns>
        /// <remarks>
        ///     <c>text/xml</c> is not decoration. <c>XmlRpcResponse.CreateAsync</c> rejects any other
        ///     media type with an <see cref="ArgumentException"/>, so a stub that returned
        ///     <c>application/xml</c> would measure the throw path and report it as a parse.
        /// </remarks>
        public static HttpResponseMessage CreateResponse(byte[] body)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(body),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string path = request.RequestUri?.AbsolutePath ?? string.Empty;
            byte[] body = path.EndsWith("/struct", StringComparison.Ordinal)
                ? structBody
                : path.EndsWith("/fault", StringComparison.Ordinal)
                    ? faultBody
                    : scalarBody;

            return Task.FromResult(CreateResponse(body));
        }
    }
}