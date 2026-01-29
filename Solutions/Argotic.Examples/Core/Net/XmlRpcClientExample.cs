using System.Text;
using Argotic.Net;

namespace Argotic.Examples.Core.Net;

/// <summary>
/// Contains the code examples for the <see cref="XmlRpcClient"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="XmlRpcClient"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class XmlRpcClientExample
{
    /// <summary>
    /// Provides example code for the XmlRpcClient class.
    /// </summary>
    /// <remarks>
    /// This example demonstrates how to configure and use the XmlRpcClient.
    /// Note: This example does not make actual network calls since it uses placeholder URLs.
    /// In a real application, you would use actual XML-RPC server endpoints.
    /// </remarks>
    public static void ClassExample()
    {
        // Initialize the XML-RPC client
        XmlRpcClient client = new()
        {
            Host = new Uri("http://bob.example.net/xmlrpcserver")
        };

        // Construct a Pingback peer-to-peer notification XML-RPC message
        XmlRpcMessage message = new("pingback.ping")
        {
            Encoding = Encoding.UTF8
        };
        message.Parameters.Add(new XmlRpcScalarValue("http://alice.example.org/#p123"));    // sourceURI
        message.Parameters.Add(new XmlRpcScalarValue("http://bob.example.net/#foo"));       // targetURI

        // Note: In a real application, you would send the message:
        // XmlRpcResponse response = await client.SendAsync(message).ConfigureAwait(false);

        // For demonstration, we just verify the client and message are configured correctly
        if (client.Host != null && message.MethodName != null)
        {
            // Client is configured and ready to send
            // Verify response to the XML-RPC call
            // if (response?.Fault != null)
            // {
            //     XmlRpcStructureMember faultCode = response.Fault["faultCode"];
            //     XmlRpcStructureMember faultMessage = response.Fault["faultString"];
            //     // Handle the fault condition
            // }
        }

        ExampleOutput.ShowXmlRpcClient(client.Host, message.MethodName);
    }
}