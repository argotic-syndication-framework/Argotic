using Argotic.Configuration;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Configuration;

[TestClass]
public class TrackbackClientOptionsTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        TrackbackClientOptions options = new();

        // Assert
        options.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        options.UserAgent.ShouldBeNull();
        options.Host.ShouldBeNull();
    }

    [TestMethod]
    public void Timeout_SetProperty_RetainsValue()
    {
        // Arrange
        TrackbackClientOptions options = new();
        TimeSpan expectedTimeout = TimeSpan.FromSeconds(30);

        // Act
        options.Timeout = expectedTimeout;

        // Assert
        options.Timeout.ShouldBe(expectedTimeout);
    }

    [TestMethod]
    public void UserAgent_SetProperty_RetainsValue()
    {
        // Arrange
        TrackbackClientOptions options = new();
        string expectedUserAgent = "TestTrackbackAgent/1.0";

        // Act
        options.UserAgent = expectedUserAgent;

        // Assert
        options.UserAgent.ShouldBe(expectedUserAgent);
    }

    [TestMethod]
    public void Host_SetProperty_RetainsValue()
    {
        // Arrange
        TrackbackClientOptions options = new();
        Uri expectedHost = new("http://example.com/trackback/1");

        // Act
        options.Host = expectedHost;

        // Assert
        options.Host.ShouldBe(expectedHost);
    }

    [TestMethod]
    public void SetAllProperties_RetainValues()
    {
        // Arrange
        TimeSpan expectedTimeout = TimeSpan.FromMinutes(1);
        string expectedUserAgent = "CompleteTrackbackAgent/2.0";
        Uri expectedHost = new("http://complete-example.com/trackback/post/123");

        // Act
        TrackbackClientOptions options = new()
        {
            Timeout = expectedTimeout,
            UserAgent = expectedUserAgent,
            Host = expectedHost
        };

        // Assert
        options.Timeout.ShouldBe(expectedTimeout);
        options.UserAgent.ShouldBe(expectedUserAgent);
        options.Host.ShouldBe(expectedHost);
    }

    [TestMethod]
    public void UserAgent_SetToNull_ReturnsNull()
    {
        // Arrange
        TrackbackClientOptions options = new()
        {
            UserAgent = "InitialAgent/1.0"
        };

        // Act
        options.UserAgent = null;

        // Assert
        options.UserAgent.ShouldBeNull();
    }

    [TestMethod]
    public void Host_SetToNull_ReturnsNull()
    {
        // Arrange
        TrackbackClientOptions options = new()
        {
            Host = new Uri("http://example.com")
        };

        // Act
        options.Host = null;

        // Assert
        options.Host.ShouldBeNull();
    }
}

[TestClass]
public class XmlRpcClientOptionsTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        XmlRpcClientOptions options = new();

        // Assert
        options.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        options.UserAgent.ShouldBeNull();
        options.Host.ShouldBeNull();
    }

    [TestMethod]
    public void Timeout_SetProperty_RetainsValue()
    {
        // Arrange
        XmlRpcClientOptions options = new();
        TimeSpan expectedTimeout = TimeSpan.FromSeconds(45);

        // Act
        options.Timeout = expectedTimeout;

        // Assert
        options.Timeout.ShouldBe(expectedTimeout);
    }

    [TestMethod]
    public void UserAgent_SetProperty_RetainsValue()
    {
        // Arrange
        XmlRpcClientOptions options = new();
        string expectedUserAgent = "TestXmlRpcAgent/1.0";

        // Act
        options.UserAgent = expectedUserAgent;

        // Assert
        options.UserAgent.ShouldBe(expectedUserAgent);
    }

    [TestMethod]
    public void Host_SetProperty_RetainsValue()
    {
        // Arrange
        XmlRpcClientOptions options = new();
        Uri expectedHost = new("http://example.com/xmlrpc");

        // Act
        options.Host = expectedHost;

        // Assert
        options.Host.ShouldBe(expectedHost);
    }

    [TestMethod]
    public void SetAllProperties_RetainValues()
    {
        // Arrange
        TimeSpan expectedTimeout = TimeSpan.FromMinutes(2);
        string expectedUserAgent = "CompleteXmlRpcAgent/2.0";
        Uri expectedHost = new("http://complete-example.com/xmlrpc/api");

        // Act
        XmlRpcClientOptions options = new()
        {
            Timeout = expectedTimeout,
            UserAgent = expectedUserAgent,
            Host = expectedHost
        };

        // Assert
        options.Timeout.ShouldBe(expectedTimeout);
        options.UserAgent.ShouldBe(expectedUserAgent);
        options.Host.ShouldBe(expectedHost);
    }

    [TestMethod]
    public void UserAgent_SetToNull_ReturnsNull()
    {
        // Arrange
        XmlRpcClientOptions options = new()
        {
            UserAgent = "InitialXmlRpcAgent/1.0"
        };

        // Act
        options.UserAgent = null;

        // Assert
        options.UserAgent.ShouldBeNull();
    }

    [TestMethod]
    public void Host_SetToNull_ReturnsNull()
    {
        // Arrange
        XmlRpcClientOptions options = new()
        {
            Host = new Uri("http://example.com/xmlrpc")
        };

        // Act
        options.Host = null;

        // Assert
        options.Host.ShouldBeNull();
    }

    [TestMethod]
    public void Timeout_ZeroValue_IsAllowed()
    {
        // Arrange
        XmlRpcClientOptions options = new();

        // Act
        options.Timeout = TimeSpan.Zero;

        // Assert
        options.Timeout.ShouldBe(TimeSpan.Zero);
    }

    [TestMethod]
    public void Timeout_NegativeValue_IsAllowed()
    {
        // This test documents that negative values are not prevented at the options level.
        // Validation should occur at the client level if needed.

        // Arrange
        XmlRpcClientOptions options = new();

        // Act
        options.Timeout = TimeSpan.FromSeconds(-1);

        // Assert
        options.Timeout.ShouldBe(TimeSpan.FromSeconds(-1));
    }
}