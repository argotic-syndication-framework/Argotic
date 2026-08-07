using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the remote-procedure-call transport a subscriber uses to register with an <see cref="RssCloud"/>.
/// </summary>
/// <seealso cref="RssCloud.Protocol"/>
/// <remarks>
///     The RssCloud interface defines three transports, identified in the <c>protocol</c> attribute as
///     <c>xml-rpc</c>, <c>soap</c> and <c>http-post</c>. This enumeration covers the first two only; there
///     is no member for REST-style HTTP POST. The interface is specified at
///     <a href="https://www.rssboard.org/rsscloud-interface">https://www.rssboard.org/rsscloud-interface</a>.
/// </remarks>
public enum RssCloudProtocol
{
    /// <summary>
    /// No cloud protocol specified. Also the result of parsing a <c>protocol</c> attribute this enumeration does not model, such as <c>http-post</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The cloud notification web service exchanges SOAP 1.1 messages. Written as <c>protocol="soap"</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "SOAP 1.1", AlternateValue = "soap")]
    Soap = 1,

    /// <summary>
    /// The cloud notification web service exchanges XML-RPC messages. Written as <c>protocol="xml-rpc"</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "XML-RPC", AlternateValue = "xml-rpc")]
    XmlRpc = 2
}