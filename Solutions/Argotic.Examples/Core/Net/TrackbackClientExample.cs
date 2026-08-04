using System.Text;
using Argotic.Net;

namespace Argotic.Examples.Core.Net;

/// <summary>
/// Contains the code examples for the <see cref="TrackbackClient"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="TrackbackClient"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class TrackbackClientExample
{
    /// <summary>
    /// Provides example code for the TrackbackClient class.
    /// </summary>
    /// <remarks>
    /// This example demonstrates how to configure and use the TrackbackClient.
    /// Note: This example does not make actual network calls since it uses placeholder URLs.
    /// In a real application, you would use actual trackback server endpoints.
    /// </remarks>
    public static void ClassExample()
    {
        // Initialize the Trackback peer-to-peer notification protocol client
        TrackbackClient client = new()
        {
            Host = new Uri("http://www.example.com/trackback/5")
        };

        // Construct the trackback message to be sent
        TrackbackMessage message = new(new Uri("http://www.bar.com/"))
        {
            Encoding = Encoding.UTF8,
            WeblogName = "Foo",
            Title = "Foo Bar",
            Excerpt = "My Excerpt"
        };

        // Note: In a real application, you would send the message:
        // TrackbackResponse response = await client.SendAsync(message).ConfigureAwait(false);

        // For demonstration, we just verify the client and message are configured correctly
        if (client.Host != null && message.Permalink != null)
        {
            // Client is configured and ready to send
            // Verify response to the trackback ping
            // if (response != null && response.HasError)
            // {
            //     // Use the TrackbackResponse.ErrorMessage property to determine the reason the trackback ping failed
            // }
        }

        ExampleOutput.ShowTrackbackClient(client.Host!, message.WeblogName, message.Title);
    }
}