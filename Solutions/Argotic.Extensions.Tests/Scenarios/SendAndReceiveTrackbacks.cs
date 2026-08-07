using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Net;

using Shouldly;

namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Exercises the Trackback protocol over the wire, in both directions.
/// </summary>
/// <remarks>
///     Every method that touches the wire was at zero coverage: <c>TrackbackClient.SendAsync</c>,
///     <c>SendRequestAsync</c>, <c>TrackbackResponse.CreateAsync</c>, <c>TrackbackMessage.WriteTo</c>
///     and <c>TrackbackMessage.Load(NameValueCollection)</c>. The existing client tests are property
///     bags that never construct an <see cref="HttpClient"/>. <see cref="MockHttpMessageHandler"/>
///     already existed; it had simply never been pointed at this subsystem.
/// </remarks>
[TestClass]
public sealed class SendAndReceiveTrackbacks : IDisposable
{
    private static readonly Uri Host = new("http://example.com/trackback/ping");

    private readonly List<IDisposable> disposables = [];

    /// <summary>
    /// Disposes the handlers and clients created by the tests.
    /// </summary>
    public void Dispose()
    {
        foreach (IDisposable disposable in this.disposables)
        {
            disposable.Dispose();
        }

        this.disposables.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Sending a message posts a form-encoded body to the configured host.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SendingAMessage_PostsAFormEncodedBodyToTheHost()
    {
        (TrackbackClient client, RequestRecord record) = this.CreateClient(SuccessResponse);

        TrackbackMessage message = new(new Uri("http://example.com/post/1"))
        {
            Title = "A post",
            WeblogName = "A weblog",
            Excerpt = "An excerpt",
        };

        await client.SendAsync(message, TestContext.CancellationTokenSource.Token);

        record.Method.ShouldBe(HttpMethod.Post);
        record.RequestUri.ShouldBe(Host);
        record.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/x-www-form-urlencoded");
        record.UserAgent.ShouldNotBeNullOrEmpty();
        record.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");

        NameValueCollection form = HttpUtility.ParseQueryString(record.Body!);
        form["url"].ShouldBe("http://example.com/post/1");
        form["title"].ShouldBe("A post");
        form["blog_name"].ShouldBe("A weblog");
        form["excerpt"].ShouldBe("An excerpt");
    }

    /// <summary>
    /// A permalink containing reserved characters survives the round trip to a receiver.
    /// </summary>
    /// <remarks>
    ///     This is the case that matters. <c>title</c>, <c>blog_name</c> and <c>excerpt</c> are passed
    ///     through <see cref="HttpUtility.UrlEncode(string)"/>; <c>url</c> was written raw. A permalink
    ///     carrying <c>&amp;</c> or <c>=</c> - an ordinary query string - therefore split into extra form
    ///     fields and arrived truncated, which no test had ever executed.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SendingAMessage_WithReservedCharactersInThePermalink_RoundTripsIntact()
    {
        (TrackbackClient client, RequestRecord record) = this.CreateClient(SuccessResponse);

        Uri permalink = new("http://example.com/post?id=1&ref=weekly");
        TrackbackMessage sent = new(permalink) { Title = "Ampersands & equals" };

        await client.SendAsync(sent, TestContext.CancellationTokenSource.Token);

        // Parse the body the way a receiving endpoint would, then load it back.
        NameValueCollection form = HttpUtility.ParseQueryString(record.Body!);
        TrackbackMessage received = new();
        received.Load(form).ShouldBeTrue();

        received.Permalink.ShouldBe(permalink);
        received.Title.ShouldBe("Ampersands & equals");
    }

    /// <summary>
    /// Only populated fields are written to the body.
    /// </summary>
    [TestMethod]
    public void WriteTo_WithOnlyAPermalink_EmitsOnlyTheUrlField()
    {
        TrackbackMessage message = new(new Uri("http://example.com/post/1"));

        string body = WriteBody(message);

        HttpUtility.ParseQueryString(body).AllKeys.ShouldBe(["url"]);
    }

    /// <summary>
    /// The receiving side reads all four Trackback fields from a form post, and ignores a parameter it
    /// does not recognise rather than failing on it.
    /// </summary>
    [TestMethod]
    public void Load_FromRequestParameters_PopulatesEveryRecognisedField()
    {
        NameValueCollection parameters = new()
        {
            { "url", "http://example.com/post/1" },
            { "title", "A post" },
            { "blog_name", "A weblog" },
            { "excerpt", "An excerpt" },
            { "unrecognised", "ignored" },
        };

        TrackbackMessage message = new();

        message.Load(parameters).ShouldBeTrue();
        message.Permalink.ShouldBe(new Uri("http://example.com/post/1"));
        message.Title.ShouldBe("A post");
        message.WeblogName.ShouldBe("A weblog");
        message.Excerpt.ShouldBe("An excerpt");
    }

    /// <summary>
    /// Parameter names are matched without regard to case.
    /// </summary>
    [TestMethod]
    public void Load_FromRequestParameters_MatchesNamesCaseInsensitively()
    {
        NameValueCollection parameters = new() { { "URL", "http://example.com/post/1" }, { "Blog_Name", "A weblog" } };

        TrackbackMessage message = new();

        message.Load(parameters).ShouldBeTrue();
        message.Permalink.ShouldBe(new Uri("http://example.com/post/1"));
        message.WeblogName.ShouldBe("A weblog");
    }

    /// <summary>
    /// A parameter collection with nothing recognisable reports that nothing was loaded.
    /// </summary>
    [TestMethod]
    public void Load_FromRequestParameters_WithNothingRecognisable_ReturnsFalse()
    {
        NameValueCollection parameters = new() { { "irrelevant", "value" } };

        new TrackbackMessage().Load(parameters).ShouldBeFalse();
    }

    /// <summary>
    /// A success response from the server reports no error.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AServerSuccessResponse_ReportsNoError()
    {
        (TrackbackClient client, _) = this.CreateClient(SuccessResponse);

        TrackbackResponse response = await client.SendAsync(
            new TrackbackMessage(new Uri("http://example.com/post/1")),
            TestContext.CancellationTokenSource.Token);

        response.HasError.ShouldBeFalse();
    }

    /// <summary>
    /// An error response from the server carries the server's message.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AServerErrorResponse_CarriesTheServerMessage()
    {
        (TrackbackClient client, _) = this.CreateClient(
            """<?xml version="1.0" encoding="utf-8"?><response><error>1</error><message>The trackback was rejected</message></response>""");

        TrackbackResponse response = await client.SendAsync(
            new TrackbackMessage(new Uri("http://example.com/post/1")),
            TestContext.CancellationTokenSource.Token);

        response.HasError.ShouldBeTrue();
        response.ErrorMessage.ShouldBe("The trackback was rejected");
    }

    /// <summary>
    /// A response whose content type is not text/xml is rejected.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AResponseWithAnUnexpectedContentType_IsRejected()
    {
        (TrackbackClient client, _) = this.CreateClient("<html>not a trackback response</html>", "text/html");

        await Should.ThrowAsync<ArgumentException>(async () => await client.SendAsync(
            new TrackbackMessage(new Uri("http://example.com/post/1")),
            TestContext.CancellationTokenSource.Token));
    }

    /// <summary>
    /// Sending without a configured host fails before any request is made.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SendingWithoutAHost_ThrowsBeforeIssuingARequest()
    {
        bool requestIssued = false;
        MockHttpMessageHandler handler = new((_, _) =>
        {
            requestIssued = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        HttpClient httpClient = new(handler, disposeHandler: false);
        this.disposables.Add(handler);
        this.disposables.Add(httpClient);
        TrackbackClient client = new(httpClient);

        await Should.ThrowAsync<InvalidOperationException>(async () => await client.SendAsync(
            new TrackbackMessage(new Uri("http://example.com/post/1")),
            TestContext.CancellationTokenSource.Token));

        requestIssued.ShouldBeFalse();
    }

    /// <summary>
    /// Gets or sets the MSTest context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    private const string SuccessResponse =
        """<?xml version="1.0" encoding="utf-8"?><response><error>0</error></response>""";

    /// <summary>
    /// The request body carries no byte-order mark.
    /// </summary>
    /// <remarks>
    ///     <see cref="Encoding.UTF8"/> emits a preamble and <see cref="StreamWriter"/> writes it ahead of
    ///     the first field, so the body began with EF BB BF and the first form parameter was named
    ///     "﻿url". Argotic's own receiver matches "url" with
    ///     <see cref="StringComparison.OrdinalIgnoreCase"/> and would have rejected it.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SendingAMessage_WritesABodyWithNoByteOrderMark()
    {
        (TrackbackClient client, RequestRecord record) = this.CreateClient(SuccessResponse);

        await client.SendAsync(
            new TrackbackMessage(new Uri("http://example.com/post/1")),
            TestContext.CancellationTokenSource.Token);

        record.RawBody.ShouldNotBeNull();
        record.RawBody.Length.ShouldBeGreaterThan(3);
        record.RawBody.Take(3).ShouldNotBe(new byte[] { 0xEF, 0xBB, 0xBF });
        record.RawBody[0].ShouldBe((byte)'u');
    }

    private static string WriteBody(TrackbackMessage message)
    {
        // A writer with no preamble, so this exercises WriteTo's own output rather than the
        // encoding's byte-order mark. The client's handling of that is asserted separately.
        using MemoryStream stream = new();
        using (StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true))
        {
            message.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private (TrackbackClient Client, RequestRecord Record) CreateClient(string responseBody, string contentType = "text/xml")
    {
        RequestRecord record = new();
        MockHttpMessageHandler handler = new(async (request, cancellationToken) =>
        {
            record.Method = request.Method;
            record.RequestUri = request.RequestUri;
            record.UserAgent = request.Headers.UserAgent.ToString();
            record.ContentType = request.Content?.Headers.ContentType;
            record.Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            record.RawBody = request.Content is null
                ? null
                : await request.Content.ReadAsByteArrayAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, contentType),
            };
        });

        HttpClient httpClient = new(handler, disposeHandler: false);
        this.disposables.Add(handler);
        this.disposables.Add(httpClient);

        return (new TrackbackClient(Host, httpClient), record);
    }

    private sealed class RequestRecord
    {
        public HttpMethod? Method { get; set; }

        public Uri? RequestUri { get; set; }

        public string? UserAgent { get; set; }

        public MediaTypeHeaderValue? ContentType { get; set; }

        public string? Body { get; set; }

        /// <summary>
        /// Gets or sets the body as it went on the wire. <see cref="HttpContent.ReadAsStringAsync()"/>
        /// strips a byte-order mark while decoding, so <see cref="Body"/> cannot see one.
        /// </summary>
        public byte[]? RawBody { get; set; }
    }
}