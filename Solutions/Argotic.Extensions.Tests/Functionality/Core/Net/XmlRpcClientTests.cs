using System.Text;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class XmlRpcClientTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        XmlRpcClient client = new();

        client.ShouldNotBeNull();
        client.Host.ShouldBeNull();
    }

    [TestMethod]
    public void Constructor_WithHost_SetsHost()
    {
        Uri host = new("http://example.com/xmlrpc");
        XmlRpcClient client = new(host);

        client.Host.ShouldBe(host);
    }

    [TestMethod]
    public void Host_CanBeSet()
    {
        XmlRpcClient client = new();
        Uri host = new("http://example.com/xmlrpc");

        client.Host = host;

        client.Host.ShouldBe(host);
    }

    [TestMethod]
    public void UserAgent_IsNotEmpty()
    {
        XmlRpcClient client = new();

        client.UserAgent.ShouldNotBeNullOrEmpty();
        client.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

    [TestMethod]
    public void Timeout_DefaultValue_Is15Seconds()
    {
        XmlRpcClient client = new();

        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    [TestMethod]
    public void Timeout_CanBeSet()
    {
        XmlRpcClient client = new();
        TimeSpan timeout = TimeSpan.FromSeconds(30);

        client.Timeout = timeout;

        client.Timeout.ShouldBe(timeout);
    }
}

[TestClass]
public class XmlRpcMessageTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_WithMethodName_SetsMethodName()
    {
        XmlRpcMessage message = new("pingback.ping");

        message.MethodName.ShouldBe("pingback.ping");
    }

    [TestMethod]
    public void Parameters_CanAddScalarValues()
    {
        XmlRpcMessage message = new("test.method");
        message.Parameters.Add(new XmlRpcScalarValue("string value"));
        message.Parameters.Add(new XmlRpcScalarValue(42));
        message.Parameters.Add(new XmlRpcScalarValue(true));

        message.Parameters.Count.ShouldBe(3);
    }

    [TestMethod]
    public void Encoding_DefaultValue_IsUtf8()
    {
        XmlRpcMessage message = new("test.method");

        message.Encoding.ShouldBe(Encoding.UTF8);
    }

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

[TestClass]
public class XmlRpcScalarValueTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_WithString_SetsValue()
    {
        XmlRpcScalarValue value = new("test string");

        value.Value.ShouldBe("test string");
        value.ValueType.ShouldBe(XmlRpcScalarValueType.String);
    }

    [TestMethod]
    public void Constructor_WithInteger_SetsValue()
    {
        XmlRpcScalarValue value = new(42);

        value.Value.ShouldBe(42);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Integer);
    }

    [TestMethod]
    public void Constructor_WithBoolean_SetsValue()
    {
        XmlRpcScalarValue value = new(true);

        value.Value.ShouldBe(true);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Boolean);
    }

    [TestMethod]
    public void Constructor_WithDouble_SetsValue()
    {
        XmlRpcScalarValue value = new(3.14);

        value.Value.ShouldBe(3.14);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Double);
    }

    [TestMethod]
    public void Constructor_WithDateTime_SetsValue()
    {
        DateTime dateTime = new(2024, 1, 15, 12, 0, 0);
        XmlRpcScalarValue value = new(dateTime);

        value.Value.ShouldBe(dateTime);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.DateTime);
    }

    [TestMethod]
    public void Constructor_WithBase64_SetsValue()
    {
        byte[] bytes = new byte[] { 1, 2, 3, 4, 5 };
        XmlRpcScalarValue value = new(bytes);

        value.Value.ShouldBe(bytes);
        value.ValueType.ShouldBe(XmlRpcScalarValueType.Base64);
    }
}

[TestClass]
public class XmlRpcResponseTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesFaultlessResponse()
    {
        XmlRpcResponse response = new();

        response.Fault.ShouldBeNull();
    }

    [TestMethod]
    public void Equals_SameResponse_ReturnsTrue()
    {
        XmlRpcResponse response1 = new();
        XmlRpcResponse response2 = new();

        response1.Equals(response2).ShouldBeTrue();
    }

    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        XmlRpcResponse response = new();

        int hash = response.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_SameResponse_ReturnsZero()
    {
        XmlRpcResponse response1 = new();
        XmlRpcResponse response2 = new();

        int result = response1.CompareTo(response2);

        result.ShouldBe(0);
    }
}