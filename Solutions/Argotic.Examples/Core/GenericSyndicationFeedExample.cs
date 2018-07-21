/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/12/2007	brian.kuhn	Created GenericSyndicationFeedExample Class
****************************************************************************/
using System;
using System.Net;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="GenericSyndicationFeed"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="GenericSyndicationFeed"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class GenericSyndicationFeedExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the GenericSyndicationFeed class.
        /// </summary>
        public static void ClassExample()
        {
            #region GenericSyndicationFeed
            GenericSyndicationFeed feed = GenericSyndicationFeed.Create(new Uri("http://feeds.feedburner.com/OppositionallyDefiant"));

            foreach(GenericSyndicationCategory category in feed.Categories)
            {
                if (String.Compare(category.Term, ".NET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    //  Process feed category
                }
            }

            // Enumerate through syndicated content
            foreach (GenericSyndicationItem item in feed.Items)
            {
                if (item.PublishedOn > DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)))
                {
                    //  Process generic item's published in the last week
                }

                foreach (GenericSyndicationCategory category in item.Categories)
                {
                    if (String.Compare(category.Term, "WCF", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //  Process item category
                    }
                }
            }

            if(feed.Format == SyndicationContentFormat.Rss)
            {
                RssFeed rssFeed = feed.Resource as RssFeed;
                if (rssFeed != null)
                {
                    //  Process RSS format specific information
                }
            }
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the GenericSyndicationFeed.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            #region Create(Uri source)
            GenericSyndicationFeed feed = GenericSyndicationFeed.Create(new Uri("http://feeds.feedburner.com/OppositionallyDefiant"));

            foreach (GenericSyndicationItem item in feed.Items)
            {
                if (item.PublishedOn > DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)))
                {
                    //  Process generic item's published in the last week
                }

                foreach (GenericSyndicationCategory category in item.Categories)
                {
                    if (String.Compare(category.Term, "WCF", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //  Process item category
                    }
                }
            }
            #endregion
        }

        //============================================================
        //	INSTANCE METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
        /// </summary>
        public static void LoadUriExample()
        {
            #region Load(Uri source, ICredentials credentials, IWebProxy proxy)
            GenericSyndicationFeed feed = new GenericSyndicationFeed();
            Uri source                  = new Uri("http://feeds.feedburner.com/OppositionallyDefiant");

            feed.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (GenericSyndicationItem item in feed.Items)
            {
                if (item.PublishedOn > DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)))
                {
                    //  Process generic item's published in the last week
                }

                foreach (GenericSyndicationCategory category in item.Categories)
                {
                    if (String.Compare(category.Term, "WCF", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //  Process item category
                    }
                }
            }
            #endregion
        }
    }
}
