using System.Text;
using System.Xml.XPath;

using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net;

/// <summary>
/// Covers what a <c>TrackbackClient</c> carries once constructed, and what of that a caller can change.
/// </summary>
[TestClass]
public class TrackbackClientTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A client built without a host has none, rather than a placeholder.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        TrackbackClient client = new();

        client.ShouldNotBeNull();
        client.Host.ShouldBeNull();
    }

    /// <summary>
    /// The URI a client is constructed with becomes its host.
    /// </summary>
    [TestMethod]
    public void Constructor_WithHost_SetsHost()
    {
        Uri host = new("http://example.com/trackback/1");
        TrackbackClient client = new(host);

        client.Host.ShouldBe(host);
    }

    /// <summary>
    /// The host can be replaced after construction.
    /// </summary>
    [TestMethod]
    public void Host_CanBeSet()
    {
        TrackbackClient client = new();
        Uri host = new("http://example.com/trackback/1");

        client.Host = host;

        client.Host.ShouldBe(host);
    }

    /// <summary>
    /// A client identifies itself as <c>Argotic-Syndication-Framework/</c> and a version, without being asked to.
    /// </summary>
    [TestMethod]
    public void UserAgent_IsNotEmpty()
    {
        TrackbackClient client = new();

        client.UserAgent.ShouldNotBeNullOrEmpty();
        client.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

    /// <summary>
    /// A client waits 15 seconds unless told otherwise.
    /// </summary>
    [TestMethod]
    public void Timeout_DefaultValue_Is15Seconds()
    {
        TrackbackClient client = new();

        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// The timeout can be replaced after construction.
    /// </summary>
    [TestMethod]
    public void Timeout_CanBeSet()
    {
        TrackbackClient client = new();
        TimeSpan timeout = TimeSpan.FromSeconds(30);

        client.Timeout = timeout;

        client.Timeout.ShouldBe(timeout);
    }
}

/// <summary>
/// Covers what a <c>TrackbackMessage</c> holds once it has been built.
/// </summary>
[TestClass]
public class TrackbackMessageTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The permalink a message is constructed with is the one it carries.
    /// </summary>
    [TestMethod]
    public void Constructor_WithPermalink_SetsPermalink()
    {
        Uri permalink = new("http://www.bar.com/post/123");
        TrackbackMessage message = new(permalink);

        message.Permalink.ShouldBe(permalink);
    }

    /// <summary>
    /// Title, excerpt, weblog name and encoding are all settable through an object initialiser and read back unchanged.
    /// </summary>
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

    /// <summary>
    /// A message is encoded as UTF-8 unless another encoding is set.
    /// </summary>
    [TestMethod]
    public void Encoding_DefaultValue_IsUtf8()
    {
        TrackbackMessage message = new(new Uri("http://example.com"));

        message.Encoding.ShouldBe(Encoding.UTF8);
    }
}

/// <summary>
/// Covers the construction, equality and ordering contracts of <c>TrackbackResponse</c>.
/// </summary>
[TestClass]
public class TrackbackResponseTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// Reads a <c>response</c> element out of the supplied markup.
    /// </summary>
    /// <param name="xml">A Trackback <c>response</c> document.</param>
    /// <returns>A navigator positioned on the <c>response</c> element.</returns>
    private static XPathNavigator Response(string xml) =>
        new XPathDocument(new StringReader(xml)).CreateNavigator()!.SelectSingleNode("//response")!;

    /// <summary>
    /// A default-constructed response reports no error and carries no error message.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesSuccessResponse()
    {
        TrackbackResponse response = new();

        response.HasError.ShouldBeFalse();
        response.ErrorMessage.ShouldBeNullOrEmpty();
    }

    /// <summary>
    /// The error message a response is constructed with is the one it reports.
    /// </summary>
    [TestMethod]
    public void Constructor_WithErrorMessage_CreatesErrorResponse()
    {
        TrackbackResponse response = new("Ping failed");

        response.ErrorMessage.ShouldBe("Ping failed");
    }

    /// <summary>
    /// Constructing a response with an error message makes it report the error.
    /// </summary>
    /// <remarks>
    ///     The method name always asserted the right thing; the body did not. The constructor set
    ///     <c>ErrorMessage</c> and left <c>HasError</c> at <see langword="false"/>, so an object built
    ///     to represent a rejection reported success, and a comment in the body explained the surrender
    ///     rather than the behaviour.
    /// </remarks>
    [TestMethod]
    public void HasError_WhenErrorMessageSet_ReturnsTrue()
    {
        TrackbackResponse response = new("Error occurred");

        response.ErrorMessage.ShouldNotBeNullOrEmpty();
        response.HasError.ShouldBeTrue("INVERTED: an instance carrying an error message has an error");
    }

    /// <summary>
    /// A rejection with no explanatory message round-trips as a rejection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>WriteTo</c> derived the <c>error</c> element from <c>ErrorMessage</c> rather than from
    ///     <c>HasError</c>, so a response that had read <c>&lt;error&gt;1&lt;/error&gt;</c> correctly
    ///     was written back saying it had succeeded. The <c>message</c> element is optional in the
    ///     protocol, and a server that rejects a ping without explaining itself is entirely ordinary.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ARejectionWithNoMessage_RoundTripsAsARejection()
    {
        TrackbackResponse response = new();
        response.Load(Response("<response><error>1</error></response>")).ShouldBeTrue();
        response.HasError.ShouldBeTrue("the object reads the wire form correctly");

        string written = response.ToString();
        written.ShouldContain("<error>1</error>", Case.Sensitive, "INVERTED: a rejection stays a rejection");
        written.ShouldNotContain("<message", Case.Sensitive, "and no message is invented for one that had none");
    }

    /// <summary>
    /// A rejection carrying a message writes both elements.
    /// </summary>
    /// <remarks>
    ///     The control on the row above: the path that always worked, and the one every existing test
    ///     took. Without it, "branch on <c>HasError</c>" is indistinguishable from "stop writing
    ///     messages".
    /// </remarks>
    [TestMethod]
    public void ARejectionWithAMessage_WritesBothElements()
    {
        TrackbackResponse response = new();
        response.Load(Response("<response><error>1</error><message>Spam</message></response>")).ShouldBeTrue();

        string written = response.ToString();
        written.ShouldContain("<error>1</error>", Case.Sensitive);
        written.ShouldContain("<message>Spam</message>", Case.Sensitive);
    }

    /// <summary>
    /// An acceptance writes <c>error</c> <c>0</c> and no message.
    /// </summary>
    /// <remarks>
    ///     The other control. A fix that branched on <c>HasError</c> but forgot the <c>message</c>
    ///     guard would emit an empty <c>message</c> element on every successful ping.
    /// </remarks>
    [TestMethod]
    public void AnAcceptance_WritesErrorZeroAndNoMessage()
    {
        TrackbackResponse response = new();

        string written = response.ToString();
        written.ShouldContain("<error>0</error>", Case.Sensitive);
        written.ShouldNotContain("<message", Case.Sensitive);
    }

    /// <summary>
    /// A response constructed from an error message serialises as a rejection carrying it.
    /// </summary>
    /// <remarks>
    ///     The mirror of <see cref="ARejectionWithNoMessage_RoundTripsAsARejection"/>. Object state and
    ///     wire form disagreed in opposite directions on the two construction paths, so a fix to either
    ///     one alone would leave the other inconsistent.
    /// </remarks>
    [TestMethod]
    public void AConstructedRejection_SerialisesAsOne()
    {
        TrackbackResponse response = new("Ping refused");

        string written = response.ToString();
        written.ShouldContain("<error>1</error>", Case.Sensitive);
        written.ShouldContain("<message>Ping refused</message>", Case.Sensitive);
    }


    /// <summary>
    /// Neither <see langword="null"/> nor an empty string is accepted as an error message; both are refused with an <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_NullOrEmpty_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new TrackbackResponse(null!));
        Should.Throw<ArgumentException>(() => new TrackbackResponse(string.Empty));
    }

    /// <summary>
    /// Two default-constructed responses are equal.
    /// </summary>
    [TestMethod]
    public void Equals_SameResponse_ReturnsTrue()
    {
        TrackbackResponse response1 = new();
        TrackbackResponse response2 = new();

        response1.Equals(response2).ShouldBeTrue();
    }

    /// <summary>
    /// A default-constructed response hashes to a non-zero value.
    /// </summary>
    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        TrackbackResponse response = new();

        int hash = response.GetHashCode();

        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// Two default-constructed responses compare as <c>0</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_SameResponse_ReturnsZero()
    {
        TrackbackResponse response1 = new();
        TrackbackResponse response2 = new();

        int result = response1.CompareTo(response2);

        result.ShouldBe(0);
    }
}