/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
03/20/2007	brian.kuhn	Created TrackbackSyndicationExtensionExample Class
****************************************************************************/
using System;
using System.IO;

using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="TrackbackSyndicationExtension"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="TrackbackSyndicationExtension"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public static class TrackbackSyndicationExtensionExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the TrackbackSyndicationExtension class.
        /// </summary>
        public static void ClassExample()
        {
            #region TrackbackSyndicationExtension
            // Framework auto-discovers supported extensions based on XML namespace attributes (xmlns) defined on root of resource
            RssFeed feed    = RssFeed.Create(new Uri("http://www.example.com/feed.aspx?format=rss"));

            // Extensible framework entities provide properties/methods to determine if entity is extended and predicate based seaching against available extensions
            if(feed.Channel.HasExtensions)
            {
                TrackbackSyndicationExtension channelExtension = feed.Channel.FindExtension(TrackbackSyndicationExtension.MatchByType) as TrackbackSyndicationExtension;
                if (channelExtension != null)
                {
                    // Process channel extension
                }
            }

            foreach(RssItem item in feed.Channel.Items)
            {
                if (item.HasExtensions)
                {
                    TrackbackSyndicationExtension itemExtension = item.FindExtension(TrackbackSyndicationExtension.MatchByType) as TrackbackSyndicationExtension;
                    if (itemExtension != null)
                    {
                        // Process extension for current item
                    }
                }
            }

            // By default the framework will automatically determine what XML namespace attributes (xmlns) to write 
            // on the root of the resource based on the extensions applied to extensible parent and child entities
            using(FileStream stream = new FileStream("Feed.xml", FileMode.Create, FileAccess.Write))
            {
                feed.Save(stream);
            }
            #endregion
        }
    }
}
