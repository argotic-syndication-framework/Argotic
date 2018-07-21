/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
03/19/2007	brian.kuhn	Created TrackbackClientExample Class
****************************************************************************/
using System;
using System.Text;

using Argotic.Net;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="TrackbackClient"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="TrackbackClient"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public static class TrackbackClientExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the TrackbackClient class.
        /// </summary>
        public static void ClassExample()
        {
            #region TrackbackClient
            // Initialize the Trackback peer-to-peer notification protocol client
            TrackbackClient client      = new TrackbackClient();
            client.Host                 = new Uri("http://www.example.com/trackback/5");

            // Construct the trackback message to be sent
            TrackbackMessage message    = new TrackbackMessage(new Uri("http://www.bar.com/"));
            message.Encoding            = Encoding.UTF8;
            message.WeblogName          = "Foo";
            message.Title               = "Foo Bar";
            message.Excerpt             = "My Excerpt";

            // Send a synchronous trackback ping
            TrackbackResponse response  = client.Send(message);

            // Verify response to the trackback ping
            if (response != null)
            {
                if (response.HasError)
                {
                    // Use the TrackbackResponse.ErrorMessage property to determine the reason the trackback ping failed
                }
            }
            #endregion
        }
    }
}
