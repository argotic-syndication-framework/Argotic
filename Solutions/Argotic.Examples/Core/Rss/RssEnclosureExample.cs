/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created RssEnclosureExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RssEnclosure"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssEnclosure"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public static class RssEnclosureExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RssEnclosure class.
        /// </summary>
        public static void ClassExample()
        {
            #region RssEnclosure
            RssFeed feed    = new RssFeed();

            feed.Channel.Title          = "Dallas Times-Herald";
            feed.Channel.Link           = new Uri("http://dallas.example.com");
            feed.Channel.Description    = "Current headlines from the Dallas Times-Herald newspaper";

            RssItem item        = new RssItem();
            item.Title          = "Seventh Heaven! Ryan Hurls Another No Hitter";
            item.Link           = new Uri("http://dallas.example.com/1991/05/02/nolan.htm");
            item.Description    = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.";

            item.Enclosures.Add(new RssEnclosure(24986239L, "audio/mpeg", new Uri("http://dallas.example.com/joebob_050689.mp3")));

            feed.Channel.AddItem(item);
            #endregion
        }
    }
}
