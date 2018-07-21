/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
12/11/2007	brian.kuhn	Created RssCloudProtocol Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Syndication
{
    /// <summary>
    /// Represents the message format utilized by a web service that implements the RssCloud application programming interface.
    /// </summary>
    /// <seealso cref="RssCloud"/>
    /// <remarks>
    ///     For more information about the RssCloud application programming interface, see <a href="http://www.rssboard.org/rsscloud-interface">http://www.rssboard.org/rsscloud-interface</a>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    [Serializable()]
    public enum RssCloudProtocol
    {
        /// <summary>
        /// No cloud protocol specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None = 0,

        /// <summary>
        /// The cloud notification web service utilizes SOAP 1.1 message exchange. 
        /// </summary>
        [EnumerationMetadata(DisplayName = "SOAP 1.1", AlternateValue = "soap")]
        Soap = 1,

        /// <summary>
        /// The cloud notification web service utilizes XML-RPC message exchange. 
        /// </summary>
        [EnumerationMetadata(DisplayName = "XML-RPC", AlternateValue = "xml-rpc")]
        XmlRpc = 2
    }
}