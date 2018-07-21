/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
03/06/2007	brian.kuhn	Created SyndicationDiscoveryUtilityExample Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Net;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="SyndicationDiscoveryUtility"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="SyndicationDiscoveryUtility"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class SyndicationDiscoveryUtilityExample
    {
        //============================================================
        //	ENUMERATION UTILITY METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.SyndicationContentFormatGet(Uri) method
        /// </summary>
        public static void SyndicationContentFormatGetExample()
        {
            #region SyndicationContentFormatGet(Uri source)
            SyndicationContentFormat format = SyndicationContentFormat.None;
            Uri url                         = new Uri("http://feeds.feedburner.com/HanselminutesCompleteMP3?format=xml");

            format                          = SyndicationDiscoveryUtility.SyndicationContentFormatGet(url);

            if (format != SyndicationContentFormat.None)
            {
                // Do something based on the determined content format
            }
            #endregion
        }

        //============================================================
        //	GENERAL WEB RESOURCE METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.SourceReferencesTarget(Uri, Uri) method
        /// </summary>
        public static void SourceReferencesTargetExample()
        {
            #region SourceReferencesTarget(Uri source, Uri target)
            //  Certain syndication scenarios involve verifying that one web resource references or 'links' to another web resource.

            Uri source  = new Uri("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");
            Uri target  = new Uri("http://www.wikimindmap.org/");

            if (SyndicationDiscoveryUtility.SourceReferencesTarget(source, target))
            {
                // Perform some action based on source referencing the target.
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.UriExists(Uri) method
        /// </summary>
        public static void UriExistsExample()
        {
            #region UriExists(Uri uri)
            Uri source  = new Uri("http://blog.oppositionallydefiant.com/");

            if (SyndicationDiscoveryUtility.UriExists(source))
            {
                // Perform some action based on source existing.
            }
            #endregion
        }

        //============================================================
        //	CONDITIONAL GET EXAMPLES
        //============================================================
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.ConditionalGet(Uri, DateTime, string) method
        /// </summary>
        public static void ConditionalGetExample()
        {
            #region ConditionalGet(Uri source, DateTime lastModified, string entityTag)
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Uri source                      = new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
            DateTime lastModified;
            string entityTag;

            //------------------------------------------------------------
            //	Retrieve the modification date and entity tag information
            //------------------------------------------------------------
            HttpWebRequest httpRequest      = (HttpWebRequest)HttpWebRequest.Create(source);
            httpRequest.AllowAutoRedirect   = true;
            httpRequest.KeepAlive           = true;
            httpRequest.UserAgent           = "Some User Agent 1.0.0.0";

            HttpWebResponse httpResponse    = (HttpWebResponse)httpRequest.GetResponse();

            lastModified    = httpResponse.LastModified;
            entityTag       = httpResponse.Headers[HttpResponseHeader.ETag];

            /*
                Typically the consumer would store the modification date and entity tag information for the resource,
                and some amount of time passes. Consumer can now use a conditional GET operation to determine if 
                the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
            */

            //------------------------------------------------------------
            //	Determine if web resource has been modified
            //------------------------------------------------------------
            WebResponse conditionalResponse = SyndicationDiscoveryUtility.ConditionalGet(source, lastModified, entityTag);
            if (conditionalResponse != null)
            {
                // Web resource has been modified since last retrieval, consumer would process the new data.
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.TryConditionalGetExample(Uri, DateTime, string, out WebResponse) method
        /// </summary>
        public static void TryConditionalGetExample()
        {
            #region TryConditionalGet(Uri source, DateTime lastModified, string entityTag, out WebResponse response)
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            Uri source                      = new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
            DateTime lastModified;
            string entityTag;

            //------------------------------------------------------------
            //	Retrieve the modification date and entity tag information
            //------------------------------------------------------------
            HttpWebRequest httpRequest      = (HttpWebRequest)HttpWebRequest.Create(source);
            httpRequest.AllowAutoRedirect   = true;
            httpRequest.KeepAlive           = true;
            httpRequest.UserAgent           = "Some User Agent 1.0.0.0";

            HttpWebResponse httpResponse    = (HttpWebResponse)httpRequest.GetResponse();

            lastModified    = httpResponse.LastModified;
            entityTag       = httpResponse.Headers[HttpResponseHeader.ETag];

            /*
                Typically the consumer would store the modification date and entity tag information for the resource,
                and some amount of time passes. Consumer can now use a conditional GET operation to determine if 
                the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
            */

            //------------------------------------------------------------
            //	Determine if web resource has been modified
            //------------------------------------------------------------
            HttpWebResponse conditionalResponse = null;
            if (SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, out conditionalResponse))
            {
                // Web resource has been modified since last retrieval, consumer would process the new data.
            }
            #endregion
        }

        //============================================================
        //	SYNDICATED CONTENT AUTO-DISCOVERY METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(Uri) method
        /// </summary>
        public static void LocateDiscoverableSyndicationEndpointsExample()
        {
            #region LocateDiscoverableSyndicationEndpoints(Uri uri)
            Uri source  = new Uri("http://www.dotnetrocks.com/");
            Collection<DiscoverableSyndicationEndpoint> endpoints;

            endpoints   = SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(source);

            foreach(DiscoverableSyndicationEndpoint endpoint in endpoints)
            {
                if (endpoint.ContentFormat == SyndicationContentFormat.Rss)
                {
                    RssFeed feed    = RssFeed.Create(endpoint.Source);
                    if(feed.Channel.HasExtensions)
                    {
                        // Process feed extensions
                    }
                }
            }
            #endregion
        }

        //============================================================
        //	PINGBACK PEER-TO-PEER NOTIFICATION METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.IsPingbackEnabled(Uri) method
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static void IsPingbackEnabledExample()
        {
            #region IsPingbackEnabled(Uri uri)
            Uri source  = new Uri("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            if (SyndicationDiscoveryUtility.IsPingbackEnabled(source))
            {
                //  Parse source for Pingback information
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.LocatePingbackNotificationServer(Uri)  method
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
        public static void LocatePingbackNotificationServerExample()
        {
            #region LocatePingbackNotificationServer(Uri uri)
            Uri source  = new Uri("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            Uri pingbackServer  = SyndicationDiscoveryUtility.LocatePingbackNotificationServer(source);
            if (pingbackServer != null)
            {
                Argotic.Net.XmlRpcClient client     = new Argotic.Net.XmlRpcClient(pingbackServer);
                Argotic.Net.XmlRpcMessage message   = new Argotic.Net.XmlRpcMessage();

                // Build the Pingback XML-RPC message to be sent

                client.Send(message);
            }
            #endregion
        }

        //============================================================
        //	TRACKBACK PEER-TO-PEER NOTIFICATION METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.IsTrackbackEnabled(Uri) method
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static void IsTrackbackEnabledExample()
        {
            #region IsTrackbackEnabled(Uri uri)
            Uri source  = new Uri("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            if (SyndicationDiscoveryUtility.IsTrackbackEnabled(source))
            {
                // Parse source for Trackback information
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(Uri) method
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
        public static void LocateTrackbackNotificationServersExample()
        {
            #region LocateTrackbackNotificationServers(Uri uri)
            Uri source  = new Uri("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            Collection<TrackbackDiscoveryMetadata> endpoints    = SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(source);
            foreach(TrackbackDiscoveryMetadata endpoint in endpoints)
            {
                Argotic.Net.TrackbackClient client = new Argotic.Net.TrackbackClient(endpoint.PingUrl);
                Argotic.Net.TrackbackMessage message    = new Argotic.Net.TrackbackMessage();

                //  Build Trackback url-encoded message to be sent

                client.Send(message);
            }
            #endregion
        }
    }
}
