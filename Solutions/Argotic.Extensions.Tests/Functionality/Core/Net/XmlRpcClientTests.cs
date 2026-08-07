using System.Text;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

/// <summary>
/// Covers what an <c>XmlRpcClient</c> carries once constructed, and what of that a caller can change.
/// </summary>
[TestClass]
public class XmlRpcClientTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A client built without a host has none, rather than a placeholder.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        XmlRpcClient client = new();

        client.ShouldNotBeNull();
        client.Host.ShouldBeNull();
    }

    /// <summary>
    /// The URI a client is constructed with becomes its host.
    /// </summary>
    [TestMethod]
    public void Constructor_WithHost_SetsHost()
    {
        Uri host = new("http://example.com/xmlrpc");
        XmlRpcClient client = new(host);

        client.Host.ShouldBe(host);
    }

    /// <summary>
    /// The host can be replaced after construction.
    /// </summary>
    [TestMethod]
    public void Host_CanBeSet()
    {
        XmlRpcClient client = new();
        Uri host = new("http://example.com/xmlrpc");

        client.Host = host;

        client.Host.ShouldBe(host);
    }

    /// <summary>
    /// A client identifies itself as <c>Argotic-Syndication-Framework/</c> and a version, without being asked to.
    /// </summary>
    [TestMethod]
    public void UserAgent_IsNotEmpty()
    {
        XmlRpcClient client = new();

        client.UserAgent.ShouldNotBeNullOrEmpty();
        client.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

    /// <summary>
    /// A client waits 15 seconds unless told otherwise.
    /// </summary>
    [TestMethod]
    public void Timeout_DefaultValue_Is15Seconds()
    {
        XmlRpcClient client = new();

        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// The timeout can be replaced after construction.
    /// </summary>
    [TestMethod]
    public void Timeout_CanBeSet()
    {
        XmlRpcClient client = new();
        TimeSpan timeout = TimeSpan.FromSeconds(30);

        client.Timeout = timeout;

        client.Timeout.ShouldBe(timeout);
    }
}

/// <summary>
/// Covers the method name, parameters and encoding an <c>XmlRpcMessage</c> carries.
/// </summary>
[TestClass]
public class XmlRpcMessageTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The method name a message is constructed with is the one it carries.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMethodName_SetsMethodName()
    {
        XmlRpcMessage message = new("pingback.ping");

        message.MethodName.ShouldBe("pingback.ping");
    }

    /// <summary>
    /// Scalar parameters of three different types can be added to one message, and all three are kept.
    /// </summary>
    [TestMethod]
    public void Parameters_CanAddScalarValues()
    {
        XmlRpcMessage message = new("test.method");
        message.Parameters.Add(new XmlRpcScalarValue("string value"));
        message.Parameters.Add(new XmlRpcScalarValue(42));
        message.Parameters.Add(new XmlRpcScalarValue(true));

        message.Parameters.Count.ShouldBe(3);
    }

    /// <summary>
    /// A message is encoded as UTF-8 unless another encoding is set.
    /// </summary>
    [TestMethod]
    public void Encoding_DefaultValue_IsUtf8()
    {
        XmlRpcMessage message = new("test.method");

        message.Encoding.ShouldBe(Encoding.UTF8);
    }

    /// <summary>
    /// The encoding can be chosen through an object initialiser, here ASCII.
    /// </summary>
    [TestMethod]
    public void Encoding_CanBeSet()
    {
        XmlRpcMessage message = new("test.method")
        {
            Encoding = Encoding.ASCII
        };

        message.Encoding.ShouldBe(Encoding.ASCII);
    }
}

/// <summary>
/// Covers the CLR value and the <c>XmlRpcScalarValueType</c> each constructor overload infers.
/// </summary>
[TestClass]
public class XmlRpcScalarValueTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A string argument is kept verbatim and typed <c>String</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithString_SetsValue()
    {
        XmlRpcScalarValue value = new("test string");

        value.Value.ShouldBe("test string");
        value.ValueType.ShouldBe(XmlRpcScalarValueType.String);
    }

    /// <summary>
    /// An <c>int</c> argument is kept verbatim and typed <c>Integer</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithInteger_SetsValue()
    {
        XmlRpcScalarValue value = new(42);

        value.Value.ShouldBe(42);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Integer);
    }

    /// <summary>
    /// A <c>bool</c> argument is kept verbatim and typed <c>Boolean</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBoolean_SetsValue()
    {
        XmlRpcScalarValue value = new(true);

        value.Value.ShouldBe(true);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Boolean);
    }

    /// <summary>
    /// A <c>double</c> argument is kept verbatim and typed <c>Double</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDouble_SetsValue()
    {
        XmlRpcScalarValue value = new(3.14);

        value.Value.ShouldBe(3.14);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Double);
    }

    /// <summary>
    /// A <c>DateTime</c> argument is kept verbatim and typed <c>DateTime</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDateTime_SetsValue()
    {
        DateTime dateTime = new(2024, 1, 15, 12, 0, 0);
        XmlRpcScalarValue value = new(dateTime);

        value.Value.ShouldBe(dateTime);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.DateTime);
    }

    /// <summary>
    /// A byte array is kept verbatim and typed <c>Base64</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBase64_SetsValue()
    {
        byte[] bytes = [1, 2, 3, 4, 5];
        XmlRpcScalarValue value = new(bytes);

        value.Value.ShouldBe(bytes);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Base64);
    }
}

/// <summary>
/// Covers the default state, equality and ordering contracts of <c>XmlRpcResponse</c>.
/// </summary>
[TestClass]
public class XmlRpcResponseTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A default-constructed response carries no fault.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesFaultlessResponse()
    {
        XmlRpcResponse response = new();

        response.Fault.ShouldBeNull();
    }

    /// <summary>
    /// Two default-constructed responses are equal.
    /// </summary>
    [TestMethod]
    public void Equals_SameResponse_ReturnsTrue()
    {
        XmlRpcResponse response1 = new();
        XmlRpcResponse response2 = new();

        response1.Equals(response2).ShouldBeTrue();
    }

    /// <summary>
    /// A default-constructed response hashes to a non-zero value.
    /// </summary>
    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        XmlRpcResponse response = new();

        int hash = response.GetHashCode();

        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// Two default-constructed responses compare as <c>0</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_SameResponse_ReturnsZero()
    {
        XmlRpcResponse response1 = new();
        XmlRpcResponse response2 = new();

        int result = response1.CompareTo(response2);

        result.ShouldBe(0);
    }
}