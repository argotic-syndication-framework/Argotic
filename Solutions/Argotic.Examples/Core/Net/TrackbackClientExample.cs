using System.Text;

using Argotic.Net;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="TrackbackClient"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="TrackbackClient"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class TrackbackClientExample
{
    /// <summary>
    /// Provides example code for the TrackbackClient class.
    /// </summary>
    public static void ClassExample()
    {
        // Initialize the Trackback peer-to-peer notification protocol client
        TrackbackClient client = new()
        {
            Host = new("http://www.example.com/trackback/5")
        };

        // Construct the trackback message to be sent
        TrackbackMessage message = new(new("http://www.bar.com/"))
        {
            Encoding = Encoding.UTF8,
            WeblogName = "Foo",
            Title = "Foo Bar",
            Excerpt = "My Excerpt"
        };

        // Send a synchronous trackback ping
        TrackbackResponse response = client.Send(message);

        // Verify response to the trackback ping
        if (response != null)
        {
            if (response.HasError)
            {
                // Use the TrackbackResponse.ErrorMessage property to determine the reason the trackback ping failed
            }
        }
    }
}