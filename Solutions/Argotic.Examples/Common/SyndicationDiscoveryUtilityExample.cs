using System.Collections.ObjectModel;
using System.Net;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="SyndicationDiscoveryUtility"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="SyndicationDiscoveryUtility"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class SyndicationDiscoveryUtilityExample
{
    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.SyndicationContentFormatGet(Uri) method
    /// </summary>
    public static void SyndicationContentFormatGetExample()
    {
        Uri url = new("http://feeds.feedburner.com/HanselminutesCompleteMP3?format=xml");

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(url);

        if (format != SyndicationContentFormat.None)
        {
            // Do something based on the determined content format
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.SourceReferencesTarget(Uri, Uri) method
    /// </summary>
    public static void SourceReferencesTargetExample()
    {
        //  Certain syndication scenarios involve verifying that one web resource references or 'links' to another web resource.

        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");
        Uri target = new("http://www.wikimindmap.org/");

        if (SyndicationDiscoveryUtility.SourceReferencesTarget(source, target))
        {
            // Perform some action based on source referencing the target.
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.UriExists(Uri) method
    /// </summary>
    public static void UriExistsExample()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/");

        if (SyndicationDiscoveryUtility.UriExists(source))
        {
            // Perform some action based on source existing.
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.ConditionalGet(Uri, DateTime, string) method
    /// </summary>
    public static void ConditionalGetExample()
    {
        Uri source = new("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");

        HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(source);
        httpRequest.AllowAutoRedirect = true;
        httpRequest.KeepAlive = true;
        httpRequest.UserAgent = "Some User Agent 1.0.0.0";

        HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

        DateTime lastModified = httpResponse.LastModified;
        string? entityTag = httpResponse.Headers[HttpResponseHeader.ETag];

        /*
            Typically the consumer would store the modification date and entity tag information for the resource,
            and some amount of time passes. Consumer can now use a conditional GET operation to determine if
            the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
        */
        WebResponse conditionalResponse = SyndicationDiscoveryUtility.ConditionalGet(source, lastModified, entityTag);
        if (conditionalResponse != null)
        {
            // Web resource has been modified since last retrieval, consumer would process the new data.
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.TryConditionalGetExample(Uri, DateTime, string, out WebResponse) method
    /// </summary>
    public static void TryConditionalGetExample()
    {
        Uri source = new("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
        HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(source);
        httpRequest.AllowAutoRedirect = true;
        httpRequest.KeepAlive = true;
        httpRequest.UserAgent = "Some User Agent 1.0.0.0";

        HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

        DateTime lastModified = httpResponse.LastModified;
        string? entityTag = httpResponse.Headers[HttpResponseHeader.ETag];


        /*
            Typically the consumer would store the modification date and entity tag information for the resource,
            and some amount of time passes. Consumer can now use a conditional GET operation to determine if
            the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
        */
        if (SyndicationDiscoveryUtility.TryConditionalGet(source, lastModified, entityTag, out _))
        {
            // Web resource has been modified since last retrieval, consumer would process the new data.
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(Uri) method
    /// </summary>
    public static void LocateDiscoverableSyndicationEndpointsExample()
    {
        Uri source = new("http://www.dotnetrocks.com/");

        Collection<DiscoverableSyndicationEndpoint> endpoints = SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpoints(source);

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
    /// Provides example code for the SyndicationDiscoveryUtility.IsPingbackEnabled(Uri) method
    /// </summary>
    public static void IsPingbackEnabledExample()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        if (SyndicationDiscoveryUtility.IsPingbackEnabled(source))
        {
            //  Parse source for Pingback information
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.LocatePingbackNotificationServer(Uri)  method
    /// </summary>
    public static void LocatePingbackNotificationServerExample()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        Uri pingbackServer = SyndicationDiscoveryUtility.LocatePingbackNotificationServer(source);
        if (pingbackServer != null)
        {
            Argotic.Net.XmlRpcClient client = new(pingbackServer);
            Argotic.Net.XmlRpcMessage message = new();

            // Build the Pingback XML-RPC message to be sent

            client.Send(message);
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.IsTrackbackEnabled(Uri) method
    /// </summary>
    public static void IsTrackbackEnabledExample()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        if (SyndicationDiscoveryUtility.IsTrackbackEnabled(source))
        {
            // Parse source for Trackback information
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(Uri) method
    /// </summary>
    public static void LocateTrackbackNotificationServersExample()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        Collection<TrackbackDiscoveryMetadata> endpoints = SyndicationDiscoveryUtility.LocateTrackbackNotificationServers(source);
        foreach (TrackbackDiscoveryMetadata endpoint in endpoints)
        {
            Argotic.Net.TrackbackClient client = new(endpoint.PingUrl);
            Argotic.Net.TrackbackMessage message = new();

            //  Build Trackback url-encoded message to be sent

            client.Send(message);
        }
    }
}