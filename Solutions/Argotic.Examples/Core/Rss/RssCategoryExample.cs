/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created RssCategoryExample Class
****************************************************************************/
using System;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RssCategory"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssCategory"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public static class RssCategoryExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RssCategory class.
        /// </summary>
        public static void ClassExample()
        {
            #region RssCategory
            RssFeed feed    = new RssFeed();

            feed.Channel.Title          = "Dallas Times-Herald";
            feed.Channel.Link           = new Uri("http://dallas.example.com");
            feed.Channel.Description    = "Current headlines from the Dallas Times-Herald newspaper";

            feed.Channel.Categories.Add(new RssCategory("Media"));
            feed.Channel.Categories.Add(new RssCategory("News/Newspapers/Regional/United_States/Texas", "dmoz"));

            RssItem item        = new RssItem();
            item.Title          = "Seventh Heaven! Ryan Hurls Another No Hitter";
            item.Link           = new Uri("http://dallas.example.com/1991/05/02/nolan.htm");
            item.Description    = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.";
            item.Author         = "jbb@dallas.example.com (Joe Bob Briggs)";

            item.Categories.Add(new RssCategory("sports"));
            item.Categories.Add(new RssCategory("1991/Texas Rangers", "rec.sports.baseball"));

            feed.Channel.AddItem(item);
            #endregion
        }
    }
}
