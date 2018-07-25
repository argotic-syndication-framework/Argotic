namespace Argotic.Examples
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;

    using Argotic.Common;
    using Argotic.Net;
    using Argotic.Syndication;

    /// <summary>
    /// Contains the code examples for the <see cref="SyndicationDiscoveryUtility"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="SyndicationDiscoveryUtility"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class SyndicationDiscoveryUtilityExample
    {
        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.ConditionalGet(Uri, DateTime, string) method.
        /// </summary>
        public static void ConditionalGetExample()
        {
            Uri source = new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
            DateTime lastModified;
            string entityTag;

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(source);
            httpRequest.AllowAutoRedirect = true;
            httpRequest.KeepAlive = true;
            httpRequest.UserAgent = "Some User Agent 1.0.0.0";

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            lastModified = httpResponse.LastModified;
            entityTag = httpResponse.Headers[HttpResponseHeader.ETag];

            /*
                Typically the consumer would store the modification date and entity tag information for the resource,
                and some amount of time passes. Consumer can now use a conditional GET operation to determine if
                the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
            */
            WebResponse conditionalResponse =
                SyndicationDiscoveryUtility.ConditionalGet(source, lastModified, entityTag);
            if (conditionalResponse != null)
            {
                // Web resource has been modified since last retrieval, consumer would process the new data.
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.IsPingbackEnabled(Uri) method.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Pingback")]
        public static void IsPingbackEnabledExample()
        {
            Uri source = new Uri(
                "http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            if (SyndicationDiscoveryUtility.IsPingbackEnabled(source))
            {
                // Parse source for Pingback information
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.IsTrackbackEnabled(Uri) method.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Trackback")]
        public static void IsTrackbackEnabledExample()
        {
            Uri source = new Uri(
                "http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            if (SyndicationDiscoveryUtility.IsTrackbackEnabled(source))
            {
                // Parse source for Trackback information
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(Uri) method.
        /// </summary>
        public static void LocateDiscoverableSyndicationEndpointsExample()
        {
            Uri source = new Uri("http://www.dotnetrocks.com/");
            Collection<DiscoverableSyndicationEndpoint> endpoints;

            endpoints = SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(source);

            foreach (DiscoverableSyndicationEndpoint endpoint in endpoints)
            {
                if (endpoint.ContentFormat == SyndicationContentFormat.Rss)
                {
                    RssFeed feed = RssFeed.Create(endpoint.Source);
                    if (feed.Channel.HasExtensions)
                    {
                        // Process feed extensions
                    }
                }
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.LocatePingbackNotificationServer(Uri)  method.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Pingback")]
        public static void LocatePingbackNotificationServerExample()
        {
            Uri source = new Uri(
                "http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            Uri pingbackServer = SyndicationDiscoveryUtility.LocatePingbackNotificationServer(source);
            if (pingbackServer != null)
            {
                XmlRpcClient client = new Net.XmlRpcClient(pingbackServer);
                XmlRpcMessage message = new Net.XmlRpcMessage();

                // Build the Pingback XML-RPC message to be sent
                client.Send(message);
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(Uri) method.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Trackback")]
        public static void LocateTrackbackNotificationServersExample()
        {
            Uri source = new Uri(
                "http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

            Collection<TrackbackDiscoveryMetadata> endpoints =
                SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(source);
            foreach (TrackbackDiscoveryMetadata endpoint in endpoints)
            {
                TrackbackClient client = new Net.TrackbackClient(endpoint.PingUrl);
                TrackbackMessage message = new Net.TrackbackMessage();

                // Build Trackback url-encoded message to be sent
                client.Send(message);
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.SourceReferencesTarget(Uri, Uri) method.
        /// </summary>
        public static void SourceReferencesTargetExample()
        {
            // Certain syndication scenarios involve verifying that one web resource references or 'links' to another web resource.
            Uri source = new Uri(
                "http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");
            Uri target = new Uri("http://www.wikimindmap.org/");

            if (SyndicationDiscoveryUtility.SourceReferencesTarget(source, target))
            {
                // Perform some action based on source referencing the target.
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.SyndicationContentFormatGet(Uri) method.
        /// </summary>
        public static void SyndicationContentFormatGetExample()
        {
            SyndicationContentFormat format = SyndicationContentFormat.None;
            Uri url = new Uri("http://feeds.feedburner.com/HanselminutesCompleteMP3?format=xml");

            format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(url);

            if (format != SyndicationContentFormat.None)
            {
                // Do something based on the determined content format
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.TryConditionalGetExample(Uri, DateTime, string, out WebResponse) method.
        /// </summary>
        public static void TryConditionalGetExample()
        {
            Uri source = new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
            DateTime lastModified;
            string entityTag;
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(source);
            httpRequest.AllowAutoRedirect = true;
            httpRequest.KeepAlive = true;
            httpRequest.UserAgent = "Some User Agent 1.0.0.0";

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            lastModified = httpResponse.LastModified;
            entityTag = httpResponse.Headers[HttpResponseHeader.ETag];

            /*
                Typically the consumer would store the modification date and entity tag information for the resource,
                and some amount of time passes. Consumer can now use a conditional GET operation to determine if
                the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
            */
            HttpWebResponse conditionalResponse = null;
            if (SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, out conditionalResponse))
            {
                // Web resource has been modified since last retrieval, consumer would process the new data.
            }
        }

        /// <summary>
        /// Provides example code for the SyndicationDiscoveryUtility.UriExists(Uri) method.
        /// </summary>
        public static void UriExistsExample()
        {
            Uri source = new Uri("http://blog.oppositionallydefiant.com/");

            if (SyndicationDiscoveryUtility.UriExists(source))
            {
                // Perform some action based on source existing.
            }
        }
    }
}