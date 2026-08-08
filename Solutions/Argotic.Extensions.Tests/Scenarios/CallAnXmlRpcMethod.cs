using System.Globalization;
using System.Net;

using Argotic.Net;
namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Exercises the XML-RPC protocol over the wire, one test per value type on the response path.
/// </summary>
/// <remarks>
///     <para>
///     <c>XmlRpcClient.TryParseValue</c> dispatches on nine element names and only the <c>i4</c> branch
///     had ever executed. <c>int</c>, <c>boolean</c>, <c>string</c>, <c>double</c>,
///     <c>dateTime.iso8601</c>, <c>base64</c>, <c>struct</c> and <c>array</c> were all cold, as were
///     <c>SendAsync</c>, <c>SendRequestAsync</c>, <c>XmlRpcResponse.CreateAsync</c> and
///     <c>XmlRpcResponse.Load</c>. <c>XmlRpcStructureValue</c> was at zero for the whole type.
///     </para>
///     <para>
///     The existing client tests never construct an <see cref="HttpClient"/>; they set properties and
///     read them back.
///     </para>
/// </remarks>
[TestClass]
public sealed class CallAnXmlRpcMethod : IDisposable
{
    private static readonly Uri Host = new("http://example.com/xmlrpc");

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
    /// Gets or sets the MSTest context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Gets the scalar wire forms and the value each should decode to.
    /// </summary>
    public static IEnumerable<object[]> ScalarValues =>
    [
        ["i4", "<i4>42</i4>", XmlRpcScalarValueType.Integer, 42],
        ["int", "<int>42</int>", XmlRpcScalarValueType.Integer, 42],
        ["boolean true", "<boolean>1</boolean>", XmlRpcScalarValueType.Boolean, true],
        ["boolean false", "<boolean>0</boolean>", XmlRpcScalarValueType.Boolean, false],
        ["string", "<string>a value</string>", XmlRpcScalarValueType.String, "a value"],
        ["double", "<double>3.25</double>", XmlRpcScalarValueType.Double, 3.25d],
    ];

    /// <summary>
    /// Each scalar wire form decodes to the expected value and type.
    /// </summary>
    /// <param name="description">A label for the row.</param>
    /// <param name="fragment">The value element as it appears on the wire.</param>
    /// <param name="expectedType">The scalar type the value should decode to.</param>
    /// <param name="expectedValue">The value the element should decode to.</param>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    [DynamicData(nameof(ScalarValues))]
    public async Task AScalarResponse_DecodesToTheExpectedValue(
        string description,
        string fragment,
        XmlRpcScalarValueType expectedType,
        object expectedValue)
    {
        description.ShouldNotBeNullOrEmpty();
        XmlRpcClient client = this.CreateClient(MethodResponse(fragment));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        response.Fault.ShouldBeNull();
        XmlRpcScalarValue scalar = response.Parameter.ShouldBeOfType<XmlRpcScalarValue>();
        scalar.ValueType.ShouldBe(expectedType);
        scalar.Value.ShouldBe(expectedValue);
    }

    /// <summary>
    /// The boolean parser accepts the spelled forms as well as the numeric ones.
    /// </summary>
    /// <param name="wireValue">The value carried by the boolean element.</param>
    /// <param name="expected">The value it should decode to.</param>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    [DataRow("1", true)]
    [DataRow("true", true)]
    [DataRow("True", true)]
    [DataRow("0", false)]
    [DataRow("false", false)]
    [DataRow("False", false)]
    public async Task ABooleanResponse_AcceptsBothTheNumericAndSpelledForms(string wireValue, bool expected)
    {
        XmlRpcClient client = this.CreateClient(MethodResponse($"<boolean>{wireValue}</boolean>"));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        response.Parameter.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(expected);
    }

    /// <summary>
    /// A <c>dateTime.iso8601</c> element decodes to the instant it names, typed as a date rather than
    /// left as its text.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ADateTimeResponse_DecodesToADateTime()
    {
        XmlRpcClient client = this.CreateClient(
            MethodResponse("<dateTime.iso8601>2024-01-20T12:30:45Z</dateTime.iso8601>"));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        XmlRpcScalarValue scalar = response.Parameter.ShouldBeOfType<XmlRpcScalarValue>();
        scalar.ValueType.ShouldBe(XmlRpcScalarValueType.DateTime);
        DateTime decoded = ((DateTime)scalar.Value!).ToUniversalTime();
        decoded.ShouldBe(new DateTime(2024, 1, 20, 12, 30, 45, DateTimeKind.Utc));
    }

    /// <summary>
    /// A base64 payload decodes to its bytes.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task ABase64Response_DecodesToItsBytes()
    {
        byte[] payload = Encoding.UTF8.GetBytes("Argotic");
        XmlRpcClient client = this.CreateClient(
            MethodResponse($"<base64>{Convert.ToBase64String(payload)}</base64>"));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        XmlRpcScalarValue scalar = response.Parameter.ShouldBeOfType<XmlRpcScalarValue>();
        scalar.ValueType.ShouldBe(XmlRpcScalarValueType.Base64);
        ((byte[])scalar.Value!).ShouldBe(payload);
    }

    /// <summary>
    /// A malformed base64 payload is rejected rather than throwing out of the parse.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AMalformedBase64Response_YieldsNoParameter()
    {
        XmlRpcClient client = this.CreateClient(MethodResponse("<base64>not valid base64!!</base64>"));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        response.Parameter.ShouldBeNull();
    }

    /// <summary>
    /// A struct response decodes to its named members.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AStructResponse_DecodesToItsNamedMembers()
    {
        XmlRpcClient client = this.CreateClient(MethodResponse(
            """
            <struct>
              <member><name>title</name><value><string>A post</string></value></member>
              <member><name>views</name><value><i4>17</i4></value></member>
            </struct>
            """));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        XmlRpcStructureValue structure = response.Parameter.ShouldBeOfType<XmlRpcStructureValue>();
        structure.Members.Count.ShouldBe(2);
        structure["title"].ShouldNotBeNull().Value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe("A post");
        structure["views"].ShouldNotBeNull().Value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(17);
    }

    /// <summary>
    /// The structure indexer reports nothing for a name it does not hold.
    /// </summary>
    /// <remarks>
    ///     Pins a behaviour nothing had observed: the setter for an unknown name is a silent no-op
    ///     rather than an add or a throw, so the getter continues to report null.
    /// </remarks>
    [TestMethod]
    public void AStructure_IndexedByAnUnknownName_ReportsNothingAndDoesNotAdd()
    {
        XmlRpcStructureValue structure = new();
        structure.Members.Add(new XmlRpcStructureMember("known", new XmlRpcScalarValue("value")));

        structure["absent"].ShouldBeNull();

        structure["absent"] = new XmlRpcStructureMember("absent", new XmlRpcScalarValue("ignored"));

        structure["absent"].ShouldBeNull();
        structure.Members.Count.ShouldBe(1);
    }

    /// <summary>
    /// An array response decodes to its ordered values.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AnArrayResponse_DecodesToItsOrderedValues()
    {
        XmlRpcClient client = this.CreateClient(MethodResponse(
            """
            <array>
              <data>
                <value><i4>1</i4></value>
                <value><string>two</string></value>
              </data>
            </array>
            """));

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        XmlRpcArrayValue array = response.Parameter.ShouldBeOfType<XmlRpcArrayValue>();
        array.Values.Count.ShouldBe(2);
        array.Values[0].ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(1);
        array.Values[1].ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe("two");
    }

    /// <summary>
    /// A fault response populates the fault rather than the parameter.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AFaultResponse_PopulatesTheFault()
    {
        XmlRpcClient client = this.CreateClient(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <methodResponse>
              <fault>
                <value>
                  <struct>
                    <member><name>faultCode</name><value><i4>4</i4></value></member>
                    <member><name>faultString</name><value><string>Too many parameters</string></value></member>
                  </struct>
                </value>
              </fault>
            </methodResponse>
            """);

        XmlRpcResponse response = await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token);

        response.Parameter.ShouldBeNull();
        XmlRpcStructureValue fault = response.Fault.ShouldNotBeNull();
        fault["faultCode"].ShouldNotBeNull().Value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe(4);
        fault["faultString"].ShouldNotBeNull().Value.ShouldBeOfType<XmlRpcScalarValue>().Value.ShouldBe("Too many parameters");
    }

    /// <summary>
    /// The request carries a well-formed method call naming the method and its parameters.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task SendingAMessage_PostsAWellFormedMethodCall()
    {
        string? body = null;
        XmlRpcClient client = this.CreateClient(
            MethodResponse("<i4>1</i4>"),
            captureBody: value => body = value);

        XmlRpcMessage message = new("demo.sum");
        message.Parameters.Add(new XmlRpcScalarValue(2));
        message.Parameters.Add(new XmlRpcScalarValue(3));

        await client.SendAsync(message, TestContext.CancellationTokenSource.Token);

        body.ShouldNotBeNull();
        body.ShouldContain("<methodCall>");
        body.ShouldContain("<methodName>demo.sum</methodName>");
        body.ShouldContain("2");
        body.ShouldContain("3");
    }

    /// <summary>
    /// A response whose content type is not text/xml is rejected.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AResponseWithAnUnexpectedContentType_IsRejected()
    {
        XmlRpcClient client = this.CreateClient("<html>not xml-rpc</html>", contentType: "text/html");

        await Should.ThrowAsync<ArgumentException>(async () => await client.SendAsync(
            new XmlRpcMessage("demo.method"),
            TestContext.CancellationTokenSource.Token));
    }

    private static string MethodResponse(string valueFragment) =>
        string.Format(
            CultureInfo.InvariantCulture,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <methodResponse><params><param><value>{0}</value></param></params></methodResponse>
            """,
            valueFragment);

    private XmlRpcClient CreateClient(
        string responseBody,
        string contentType = "text/xml",
        Action<string>? captureBody = null)
    {
        MockHttpMessageHandler handler = new(async (request, cancellationToken) =>
        {
            if (captureBody is not null && request.Content is not null)
            {
                captureBody(await request.Content.ReadAsStringAsync(cancellationToken));
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, contentType),
            };
        });

        HttpClient httpClient = new(handler, disposeHandler: false);
        this.disposables.Add(handler);
        this.disposables.Add(httpClient);

        return new XmlRpcClient(Host, httpClient);
    }
}