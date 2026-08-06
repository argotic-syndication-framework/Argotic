using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using Argotic.Net;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Protocols;

/// <summary>
/// Decomposes <see cref="TrackbackClient.SendAsync(TrackbackMessage, CancellationToken)"/> into
/// encode, round trip and parse.
/// </summary>
/// <remarks>
/// <para>
/// The other half of <c>Argotic.Net</c>, at the same 0.000 coverage and with the same absence of any
/// measurement. A Trackback ping is the cheapest thing this library does over the wire — a
/// form-encoded body of at most four fields, and a response of one or two XML elements — which makes
/// it the right place to find out what the fixed cost of using one of these clients actually is.
/// </para>
/// <para>
/// The interesting variable is not size but <b>escaping</b>. <c>TrackbackMessage.WriteTo</c> passes
/// every field through <c>HttpUtility.UrlEncode</c>, and that call has two
/// very different costs: a string containing only unreserved ASCII is returned as-is, while a string
/// containing anything else is rebuilt byte by byte into percent-triples. A title in English and a
/// title in Japanese are the same field taking different paths, and both are ordinary. The arms are
/// therefore chosen by <em>character class</em>, not by length.
/// </para>
/// <para>
/// The arms nest: <c>encode ⊂ send</c>, and <c>parse ⊂ send</c>. The residual is
/// <see cref="HttpClient"/> plus the per-call linked <see cref="CancellationTokenSource"/> that
/// <c>SendAsync</c> creates to enforce its own timeout — the same plumbing
/// <see cref="XmlRpcClientBenchmarks"/> isolates, measured here against a far smaller payload, so
/// the two together say whether that plumbing is a constant or a proportion.
/// </para>
/// <para>
/// <b>The transport is a stub handler and no socket is opened</b>, following
/// <c>PollingSteadyStateBenchmarks</c>: the subject is the encoder and the parser, and a real socket
/// would add scheduler noise BenchmarkDotNet cannot subtract. It also means nothing here can reach a
/// network by construction.
/// </para>
/// <para>
/// <b>No <c>[Params]</c>.</b> The arms differ in which fields are present and in what alphabet those
/// fields use, and neither is a numeric axis every arm could vary along; a class-scoped length
/// parameter would leave the minimal arm and the parse arm identical at every value, which is the
/// defect <c>docs/build-warnings.md</c> §D0 records.
/// </para>
/// </remarks>
[BenchmarkCategory("protocols", "trackback", "client")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class TrackbackClientBenchmarks : IDisposable
{
    /// <summary>
    /// A successful Trackback response: the one-element document a server returns for an accepted ping.
    /// </summary>
    private const string SuccessResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><response><error>0</error></response>";

    /// <summary>
    /// A rejected ping, which carries an error flag and a human-readable message.
    /// </summary>
    private const string ErrorResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><response><error>1</error><message>The entry you are trying to ping does not accept Trackback pings.</message></response>";

    private static readonly Uri Host = new("http://benchmark.invalid/trackback/ping");

    private HttpClient client = null!;
    private StubHandler handler = null!;
    private TrackbackClient trackbackClient = null!;
    private TrackbackMessage minimalPing = null!;
    private TrackbackMessage fullPing = null!;
    private TrackbackMessage nonAsciiPing = null!;
    private byte[] successResponseBody = [];
    private byte[] errorResponseBody = [];

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
    /// Builds the stubbed client and the three message shapes.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.successResponseBody = Encoding.UTF8.GetBytes(SuccessResponse);
        this.errorResponseBody = Encoding.UTF8.GetBytes(ErrorResponse);

        this.handler = new StubHandler(this.successResponseBody);
        this.client = new HttpClient(this.handler, disposeHandler: false);
        this.trackbackClient = new TrackbackClient(Host, this.client);

        // The permalink alone is the only required field, and it is what a minimal client sends.
        this.minimalPing = new TrackbackMessage(new Uri("https://example.com/2024/01/01/an-article"));

        // The permalink carries a query string on purpose. It is form-encoded like every other field
        // — a regression that wrote it raw split it into extra form fields at the first ampersand —
        // so a corpus of bare permalinks would measure the one shape that cannot show the escaping.
        this.fullPing = new TrackbackMessage(new Uri("https://example.com/2024/01/01/an-article?id=1&ref=weekly"))
        {
            Title = "An Article About Syndication",
            WeblogName = "The Example Weblog",
            Excerpt = "A short excerpt of the article, of the length a real Trackback ping carries, which is a sentence or two rather than the whole entry.",
        };

        this.nonAsciiPing = new TrackbackMessage(new Uri("https://example.com/2024/01/01/an-article"))
        {
            Title = "シンジケーションに関する記事",
            WeblogName = "Пример блога",
            Excerpt = "Un court extrait de l'article — accentué, ponctué d'em-dashes, et d'une longueur ordinaire pour un ping Trackback réel.",
        };
    }

    /// <summary>
    /// Disposes the stubbed transport once the class is finished with.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup() => this.Dispose();

    /// <summary>
    /// Form-encoding the full message only: no HTTP, no response.
    /// </summary>
    /// <returns>The number of bytes written, so the work cannot be elided.</returns>
    /// <remarks>
    ///     The innermost measured point, written the way <c>TrackbackClient</c> writes it — a
    ///     <see cref="StreamWriter"/> at the message's own encoding. Contained in every send arm.
    ///     Note that the client then <em>strips</em> the byte-order mark this writer emits, because a
    ///     form body must not carry one; that strip is charged to the send arms and not to this one.
    /// </remarks>
    [Benchmark(Description = "a. encode the form body only (ASCII fields)")]
    public int EncodeFullMessage()
    {
        using MemoryStream stream = new();
        using (StreamWriter writer = new(stream, this.fullPing.Encoding, leaveOpen: true))
        {
            this.fullPing.WriteTo(writer);
        }

        return (int)stream.Length;
    }

    /// <summary>
    /// Form-encoding the same message with non-ASCII field content.
    /// </summary>
    /// <returns>The number of bytes written, so the work cannot be elided.</returns>
    /// <remarks>
    ///     The same three fields at comparable length, in alphabets that force
    ///     <c>HttpUtility.UrlEncode</c> down its rebuild path: every
    ///     character becomes a percent-triple rather than being returned unchanged. The delta against
    ///     <see cref="EncodeFullMessage"/> is the whole price of escaping, and it is a price paid by
    ///     any publisher outside the ASCII range — which is most of them.
    /// </remarks>
    [Benchmark(Description = "b. encode the form body only (non-ASCII fields)")]
    public int EncodeNonAsciiMessage()
    {
        using MemoryStream stream = new();
        using (StreamWriter writer = new(stream, this.nonAsciiPing.Encoding, leaveOpen: true))
        {
            this.nonAsciiPing.WriteTo(writer);
        }

        return (int)stream.Length;
    }

    /// <summary>
    /// Parsing an error response only: no request, no HTTP.
    /// </summary>
    /// <returns>The parsed response.</returns>
    /// <remarks>
    ///     <para>
    ///     The error document rather than the success one, because it is the larger of the two and it
    ///     is the branch of <c>TrackbackResponse.Load</c> that populates <c>ErrorMessage</c> — the
    ///     success document exercises neither. Both branches were at zero coverage.
    ///     </para>
    ///     <para>
    ///     The response is constructed inside the measured region and has to be:
    ///     <c>SyndicationEncodingUtility.ReadContentAsync</c> disposes the content stream, so a
    ///     response hoisted into <c>[GlobalSetup]</c> would throw on the second invocation.
    ///     </para>
    /// </remarks>
    [Benchmark(Description = "c. TrackbackResponse.CreateAsync over an error body")]
    public async Task<TrackbackResponse> ParseErrorResponse()
    {
        using HttpResponseMessage response = StubHandler.CreateResponse(this.errorResponseBody);
        return await TrackbackResponse.CreateAsync(response).ConfigureAwait(false);
    }

    /// <summary>
    /// The whole ping with only the required permalink: encode, POST, parse.
    /// </summary>
    /// <returns>The response.</returns>
    /// <remarks>
    ///     The floor. Everything above this in the send arms is field content, and everything below
    ///     the encode arm in this one is <see cref="HttpClient"/> and the client's own timeout
    ///     plumbing — which, on a body this small, is very likely to be most of it. That would be the
    ///     finding: the cost of a Trackback ping is not the Trackback.
    /// </remarks>
    [Benchmark(Baseline = true, Description = "d. SendAsync, permalink only")]
    public Task<TrackbackResponse> SendMinimalPing() => this.trackbackClient.SendAsync(this.minimalPing);

    /// <summary>
    /// The whole ping with all four fields populated in ASCII.
    /// </summary>
    /// <returns>The response.</returns>
    [Benchmark(Description = "e. SendAsync, all four fields (ASCII)")]
    public Task<TrackbackResponse> SendFullPing() => this.trackbackClient.SendAsync(this.fullPing);

    /// <summary>
    /// The whole ping with all four fields populated outside the ASCII range.
    /// </summary>
    /// <returns>The response.</returns>
    /// <remarks>
    ///     Its delta against <see cref="SendFullPing"/> must match the delta between the two encode
    ///     arms. If it does not, the escaping is costing something outside <c>WriteTo</c> — in the
    ///     preamble strip, or in <see cref="ByteArrayContent"/> — and the encode arms are not
    ///     measuring what they claim to.
    /// </remarks>
    [Benchmark(Description = "f. SendAsync, all four fields (non-ASCII)")]
    public Task<TrackbackResponse> SendNonAsciiPing() => this.trackbackClient.SendAsync(this.nonAsciiPing);

    /// <summary>
    /// Returns a fixed body for every request, so the measurement is the library rather than a network.
    /// </summary>
    /// <param name="body">The response body to return.</param>
    private sealed class StubHandler(byte[] body) : HttpMessageHandler
    {
        /// <summary>
        /// Wraps a body in the response a Trackback server would return.
        /// </summary>
        /// <param name="content">The response body.</param>
        /// <returns>A 200 response carrying <paramref name="content"/> as <c>text/xml</c>.</returns>
        /// <remarks>
        ///     <c>text/xml</c> is not decoration. <c>TrackbackResponse.CreateAsync</c> rejects any
        ///     other media type with an <see cref="ArgumentException"/>, so a stub that returned
        ///     <c>application/xml</c> would measure the throw path and report it as a parse.
        /// </remarks>
        public static HttpResponseMessage CreateResponse(byte[] content)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(CreateResponse(body));
    }
}