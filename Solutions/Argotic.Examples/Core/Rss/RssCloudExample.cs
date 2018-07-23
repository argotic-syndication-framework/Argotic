namespace Argotic.Examples
{
    using System;

    using Argotic.Syndication;

    /// <summary>
    /// Contains the code examples for the <see cref="RssCloud"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssCloud"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rss")]
    public static class RssCloudExample
    {
        /// <summary>
        /// Provides example code for the RssCloud class.
        /// </summary>
        public static void ClassExample()
        {
            RssFeed feed = new RssFeed();

            feed.Channel.Title = "Dallas Times-Herald";
            feed.Channel.Link = new Uri("http://dallas.example.com");
            feed.Channel.Description = "Current headlines from the Dallas Times-Herald newspaper";

            feed.Channel.Cloud = new RssCloud(
                "server.example.com",
                "/rpc",
                80,
                RssCloudProtocol.XmlRpc,
                "cloud.notify");
        }

        /// <summary>
        /// Provides example code for the RssCloud.CloudProtocolAsString(RssCloudProtocol) method.
        /// </summary>
        public static void ProtocolAsStringExample()
        {
            string protocol = RssCloud.CloudProtocolAsString(RssCloudProtocol.XmlRpc); // xml-rpc

            if (string.Compare(protocol, "xml-rpc", StringComparison.OrdinalIgnoreCase) == 0)
            {
            }
        }

        /// <summary>
        /// Provides example code for the RssCloud.CloudProtocolByName(string) method.
        /// </summary>
        public static void ProtocolByNameExample()
        {
            RssCloudProtocol protocol = RssCloud.CloudProtocolByName("xml-rpc");

            if (protocol == RssCloudProtocol.XmlRpc)
            {
            }
        }
    }
}