using System.Text;
using System.Xml;
using System.Xml.XPath;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

/// <summary>
/// Covers the three transports the RssCloud interface defines — <c>xml-rpc</c>, <c>soap</c> and
/// <c>http-post</c> — against the <see cref="RssCloudProtocol"/> members that model them.
/// </summary>
/// <remarks>
///     <c>protocol</c> is not decoration: it tells a subscriber how to call
///     <see cref="RssCloud.RegisterProcedure"/>. A value that fails to round-trip is a subscriber
///     registering over a transport the publisher never offered.
/// </remarks>
[TestClass]
public class RssCloudProtocolTests
{
    private const string CloudElement = """
        <cloud domain="rpc.example.com" port="80" path="/rsscloud/pleaseNotify" registerProcedure="" protocol="http-post"/>
        """;

    private static XPathNavigator NavigatorFor(string xml)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        XPathDocument document = new(stream);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToChild(XPathNodeType.Element);

        return navigator;
    }

    /// <summary>
    /// <c>http-post</c> resolves to the member that models it.
    /// </summary>
    [TestMethod]
    public void CloudProtocolByName_WithHttpPost_ReturnsHttpPost()
    {
        // Act & Assert
        RssCloud.CloudProtocolByName("http-post").ShouldBe(RssCloudProtocol.HttpPost);
        RssCloud.CloudProtocolAsString(RssCloudProtocol.HttpPost).ShouldBe("http-post");
    }

    /// <summary>
    /// Loading <c>protocol="http-post"</c> sets the transport the document declared, rather than leaving
    /// the property at its <see cref="RssCloudProtocol.XmlRpc"/> default.
    /// </summary>
    [TestMethod]
    public void Load_WithHttpPostProtocol_SetsHttpPost()
    {
        // Arrange
        RssCloud cloud = new();

        // Act
        cloud.Load(NavigatorFor(CloudElement));

        // Assert
        cloud.Domain.ShouldBe("rpc.example.com");
        cloud.Protocol.ShouldBe(RssCloudProtocol.HttpPost);
    }

    /// <summary>
    /// Saving that cloud writes <c>protocol="http-post"</c> back.
    /// </summary>
    /// <remarks>
    ///     The harm this closes was silent: a re-saved feed was well formed and said <c>xml-rpc</c>, so
    ///     every subscriber following it registered over a transport the publisher never offered.
    /// </remarks>
    [TestMethod]
    public void WriteTo_AfterLoadingHttpPost_PreservesTheTransport()
    {
        // Arrange
        RssCloud cloud = new();
        cloud.Load(NavigatorFor(CloudElement));

        // Act
        using StringWriter stringWriter = new();
        using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment }))
        {
            cloud.WriteTo(writer);
            writer.Flush();
        }

        // Assert
        stringWriter.ToString().ShouldContain("protocol=\"http-post\"", Case.Sensitive);
    }

    /// <summary>
    /// The two transports that are modelled round-trip through the lookup and back to the attribute value.
    /// </summary>
    /// <remarks>
    ///     A guard: adding a member must not disturb the two that already resolve.
    /// </remarks>
    [TestMethod]
    public void CloudProtocol_ForTheModelledTransports_RoundTripsThroughTheLookup()
    {
        // Act & Assert
        RssCloud.CloudProtocolByName("xml-rpc").ShouldBe(RssCloudProtocol.XmlRpc);
        RssCloud.CloudProtocolByName("soap").ShouldBe(RssCloudProtocol.Soap);
        RssCloud.CloudProtocolAsString(RssCloudProtocol.XmlRpc).ShouldBe("xml-rpc");
        RssCloud.CloudProtocolAsString(RssCloudProtocol.Soap).ShouldBe("soap");
    }
}