/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created RssCloudExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RssCloud"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssCloud"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public static class RssCloudExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RssCloud class.
        /// </summary>
        public static void ClassExample()
        {
            #region RssCloud
            RssFeed feed    = new RssFeed();

            feed.Channel.Title          = "Dallas Times-Herald";
            feed.Channel.Link           = new Uri("http://dallas.example.com");
            feed.Channel.Description    = "Current headlines from the Dallas Times-Herald newspaper";

            feed.Channel.Cloud          = new RssCloud("server.example.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify");
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the RssCloud.CloudProtocolAsString(RssCloudProtocol) method
        /// </summary>
        public static void ProtocolAsStringExample()
        {
            #region CloudProtocolAsString(RssCloudProtocol protocol)
            string protocol = RssCloud.CloudProtocolAsString(RssCloudProtocol.XmlRpc);    // xml-rpc

            if (String.Compare(protocol, "xml-rpc", StringComparison.OrdinalIgnoreCase) == 0)
            {
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the RssCloud.CloudProtocolByName(string) method
        /// </summary>
        public static void ProtocolByNameExample()
        {
            #region CloudProtocolByName(string name)
            RssCloudProtocol protocol   = RssCloud.CloudProtocolByName("xml-rpc");

            if (protocol == RssCloudProtocol.XmlRpc)
            {
            }
            #endregion
        }
    }
}
