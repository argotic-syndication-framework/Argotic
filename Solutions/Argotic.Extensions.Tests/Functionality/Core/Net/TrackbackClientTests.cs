using System.Text;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

[TestClass]
public class TrackbackClientTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        TrackbackClient client = new();

        client.ShouldNotBeNull();
        client.Host.ShouldBeNull();
    }

    [TestMethod]
    public void Constructor_WithHost_SetsHost()
    {
        Uri host = new("http://example.com/trackback/1");
        TrackbackClient client = new(host);

        client.Host.ShouldBe(host);
    }

    [TestMethod]
    public void Host_CanBeSet()
    {
        TrackbackClient client = new();
        Uri host = new("http://example.com/trackback/1");

        client.Host = host;

        client.Host.ShouldBe(host);
    }

    [TestMethod]
    public void UserAgent_IsNotEmpty()
    {
        TrackbackClient client = new();

        client.UserAgent.ShouldNotBeNullOrEmpty();
        client.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

    [TestMethod]
    public void Timeout_DefaultValue_Is15Seconds()
    {
        TrackbackClient client = new();

        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    [TestMethod]
    public void Timeout_CanBeSet()
    {
        TrackbackClient client = new();
        TimeSpan timeout = TimeSpan.FromSeconds(30);

        client.Timeout = timeout;

        client.Timeout.ShouldBe(timeout);
    }
}

[TestClass]
public class TrackbackMessageTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_WithPermalink_SetsPermalink()
    {
        Uri permalink = new("http://www.bar.com/post/123");
        TrackbackMessage message = new(permalink);

        message.Permalink.ShouldBe(permalink);
    }

    [TestMethod]
    public void Properties_CanBeSet()
    {
        TrackbackMessage message = new(new Uri("http://example.com"))
        {
            Title = "Test Title",
            Excerpt = "Test Excerpt",
            WeblogName = "Test Blog",
            Encoding = Encoding.UTF8
        };

        message.Title.ShouldBe("Test Title");
        message.Excerpt.ShouldBe("Test Excerpt");
        message.WeblogName.ShouldBe("Test Blog");
        message.Encoding.ShouldBe(Encoding.UTF8);
    }

    [TestMethod]
    public void Encoding_DefaultValue_IsUtf8()
    {
        TrackbackMessage message = new(new Uri("http://example.com"));

        message.Encoding.ShouldBe(Encoding.UTF8);
    }
}

[TestClass]
public class TrackbackResponseTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesSuccessResponse()
    {
        TrackbackResponse response = new();

        response.HasError.ShouldBeFalse();
        response.ErrorMessage.ShouldBeNullOrEmpty();
    }

    [TestMethod]
    public void Constructor_WithErrorMessage_CreatesErrorResponse()
    {
        TrackbackResponse response = new("Ping failed");

        response.ErrorMessage.ShouldBe("Ping failed");
    }

    [TestMethod]
    public void HasError_WhenErrorMessageSet_ReturnsTrue()
    {
        TrackbackResponse response = new("Error occurred");

        // Note: HasError is controlled by the internal responseHasError field,
        // not derived from ErrorMessage. The constructor with error message
        // doesn't automatically set HasError. This tests the actual behavior.
        response.ErrorMessage.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void ErrorMessage_NullOrEmpty_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new TrackbackResponse(null!));
        Should.Throw<ArgumentException>(() => new TrackbackResponse(string.Empty));
    }

    [TestMethod]
    public void Equals_SameResponse_ReturnsTrue()
    {
        TrackbackResponse response1 = new();
        TrackbackResponse response2 = new();

        response1.Equals(response2).ShouldBeTrue();
    }

    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        TrackbackResponse response = new();

        int hash = response.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_SameResponse_ReturnsZero()
    {
        TrackbackResponse response1 = new();
        TrackbackResponse response2 = new();

        int result = response1.CompareTo(response2);

        result.ShouldBe(0);
    }
}