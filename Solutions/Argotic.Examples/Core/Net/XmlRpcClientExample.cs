/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
03/19/2007	brian.kuhn	Created XmlRpcClientExample Class
****************************************************************************/
using System;
using System.Text;

using Argotic.Net;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="XmlRpcClient"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="XmlRpcClient"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class XmlRpcClientExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the XmlRpcClient class.
        /// </summary>
        public static void ClassExample()
        {
            #region XmlRpcClient
            // Initialize the XML-RPC client
            XmlRpcClient client = new XmlRpcClient();
            client.Host         = new Uri("http://bob.example.net/xmlrpcserver");

            // Construct a Pingback peer-to-peer notification XML-RPC message
            XmlRpcMessage message   = new XmlRpcMessage("pingback.ping");
            message.Encoding        = Encoding.UTF8;
            message.Parameters.Add(new XmlRpcScalarValue("http://alice.example.org/#p123"));    // sourceURI
            message.Parameters.Add(new XmlRpcScalarValue("http://bob.example.net/#foo"));       // targetURI

            // Send a synchronous pingback ping
            XmlRpcResponse response = client.Send(message);

            // Verify response to the trackback ping
            if (response != null)
            {
                if (response.Fault != null)
                {
                    XmlRpcStructureMember faultCode     = response.Fault["faultCode"];
                    XmlRpcStructureMember faultMessage  = response.Fault["faultString"];

                    if (faultCode != null && faultMessage != null)
                    {
                        // Handle the pingback ping error condition that occurred
                    }
                }
                else
                {
                    XmlRpcScalarValue successInformation    = response.Parameter as XmlRpcScalarValue;
                    if (successInformation != null)
                    {
                        // Pingback request was successful, return should be a string containing information the server deems useful.
                    }
                }
            }
            #endregion
        }
    }
}
