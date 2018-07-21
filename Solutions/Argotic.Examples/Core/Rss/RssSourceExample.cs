/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created RssSourceExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RssSource"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssSource"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public static class RssSourceExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RssSource class.
        /// </summary>
        public static void ClassExample()
        {
            #region RssSource
            RssFeed feed    = new RssFeed();

            feed.Channel.Title          = "Dallas Times-Herald";
            feed.Channel.Link           = new Uri("http://dallas.example.com");
            feed.Channel.Description    = "Current headlines from the Dallas Times-Herald newspaper";

            RssItem item        = new RssItem();
            item.Title          = "Seventh Heaven! Ryan Hurls Another No Hitter";
            item.Link           = new Uri("http://dallas.example.com/1991/05/02/nolan.htm");
            item.Description    = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.";

            item.Source         = new RssSource(new Uri("http://la.example.com/rss.xml"), "Los Angeles Herald-Examiner");

            feed.Channel.AddItem(item);
            #endregion
        }
    }
}
