using System.Text;
using Argotic.Net;

namespace Argotic.Examples.Core.Net;

/// <summary>
/// Configures a <see cref="TrackbackClient"/> and the url-encoded <see cref="TrackbackMessage"/> it would post.
/// </summary>
/// <remarks>
///     Nothing is sent. The endpoint is a placeholder, so the example stops at a configured client and a
///     well-formed message; the <c>SendAsync</c> call is left commented out beside them.
/// </remarks>
internal static class TrackbackClientExample
{
    /// <summary>
    /// Configures a <see cref="TrackbackClient"/> and the <see cref="TrackbackMessage"/> it would post.
    /// </summary>
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
        if (client.Host is not null && message.Permalink is not null)
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