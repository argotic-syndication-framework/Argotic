using System.Text;
using Argotic.Net;

namespace Argotic.Examples.Core.Net;

/// <summary>
/// Configures an <see cref="XmlRpcClient"/> and builds the <c>pingback.ping</c> notification it would post.
/// </summary>
/// <remarks>
///     Nothing is sent. The endpoint is a placeholder, so the example stops at a configured client and a
///     well-formed message; the <c>SendAsync</c> call is left commented out beside them.
/// </remarks>
internal static class XmlRpcClientExample
{
    /// <summary>
    /// Configures an <see cref="XmlRpcClient"/> and the <c>pingback.ping</c> <see cref="XmlRpcMessage"/> it would post.
    /// </summary>
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
        message.Parameters.Add(new XmlRpcScalarValue("https://endjin.com/who-we-are/#barry-smart"));    // sourceURI
        message.Parameters.Add(new XmlRpcScalarValue("http://bob.example.net/#foo"));       // targetURI

        // Note: In a real application, you would send the message:
        // XmlRpcResponse response = await client.SendAsync(message).ConfigureAwait(false);

        // For demonstration, we just verify the client and message are configured correctly
        if (client.Host is not null && message.MethodName is not null)
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

        ExampleOutput.ShowXmlRpcClient(client.Host!, message.MethodName!);
    }
}