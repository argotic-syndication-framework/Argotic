using Argotic.Syndication;

namespace Argotic.Examples.Core.Rss;

/// <summary>
/// Advertises a publish-subscribe endpoint with <see cref="RssCloud"/>, and converts between <c>RssCloudProtocol</c> and the string that appears in the XML.
/// </summary>
internal static class RssCloudExample
{
    /// <summary>
    /// Builds the containing <see cref="RssFeed"/> and prints the <see cref="RssCloud"/> it holds.
    /// </summary>
    public static void ClassExample()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "endjin blog",
                Link = new Uri("https://endjin.com"),
                Description = "Technical writing from endjin on .NET, data, analytics and AI",
                Cloud = new RssCloud("endjin.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify")
            }
        };

        ExampleOutput.ShowRssCloud(feed.Channel.Cloud);
    }

    /// <summary>
    /// Converts an <c>RssCloudProtocol</c> to the string that appears in the XML.
    /// </summary>
    public static void ProtocolAsStringExample()
    {
        string protocol = RssCloud.CloudProtocolAsString(RssCloudProtocol.XmlRpc);    // xml-rpc

        if (string.Equals(protocol, "xml-rpc", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    /// <summary>
    /// Converts the string that appears in the XML back to an <c>RssCloudProtocol</c>.
    /// </summary>
    public static void ProtocolByNameExample()
    {
        RssCloudProtocol protocol = RssCloud.CloudProtocolByName("xml-rpc");

        if (protocol == RssCloudProtocol.XmlRpc)
        {
        }
    }
}