using System.Collections.ObjectModel;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Common;

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
    /// Provides example code for the SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(Uri) method
    /// </summary>
    public static async Task SyndicationContentFormatGetExampleAsync()
    {
        Uri url = new("http://feeds.feedburner.com/HanselminutesCompleteMP3?format=xml");

        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(url).ConfigureAwait(false);

        if (format != SyndicationContentFormat.None)
        {
            // Do something based on the determined content format
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.SourceReferencesTargetAsync(Uri, Uri) method
    /// </summary>
    public static async Task SourceReferencesTargetExampleAsync()
    {
        //  Certain syndication scenarios involve verifying that one web resource references or 'links' to another web resource.

        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");
        Uri target = new("http://www.wikimindmap.org/");

        if (await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(source, target).ConfigureAwait(false))
        {
            // Perform some action based on source referencing the target.
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.UriExistsAsync(Uri) method
    /// </summary>
    public static async Task UriExistsExampleAsync()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/");

        if (await SyndicationDiscoveryUtility.UriExistsAsync(source).ConfigureAwait(false))
        {
            // Perform some action based on source existing.
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.ConditionalGet(Uri, DateTime, string) method
    /// </summary>
    public static async Task ConditionalGetExampleAsync()
    {
        Uri source = new("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");

        using HttpClient client = new HttpClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, source);
        request.Headers.UserAgent.ParseAdd("Some User Agent 1.0.0.0");

        using HttpResponseMessage httpResponse = await client.SendAsync(request).ConfigureAwait(false);

        DateTime lastModified = httpResponse.Content.Headers.LastModified?.DateTime ?? DateTime.MinValue;
        string? entityTag = httpResponse.Headers.ETag?.Tag;

        /*
            Typically the consumer would store the modification date and entity tag information for the resource,
            and some amount of time passes. Consumer can now use a conditional GET operation to determine if
            the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
        */
        using ConditionalGetResult conditionalResponse = await SyndicationDiscoveryUtility.ConditionalGetAsync(source, lastModified, entityTag).ConfigureAwait(false);
        if (conditionalResponse.WasModified)
        {
            // Web resource has been modified since last retrieval, consumer would process the new data.
            using Stream stream = await conditionalResponse.GetResponseStreamAsync().ConfigureAwait(false);
            // Process the stream...
        }
    }


    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(Uri) method
    /// </summary>
    public static async Task LocateDiscoverableSyndicationEndpointsExampleAsync()
    {
        Uri source = new("http://www.dotnetrocks.com/");

        Collection<DiscoverableSyndicationEndpoint> endpoints = await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(source).ConfigureAwait(false);

        foreach (DiscoverableSyndicationEndpoint endpoint in endpoints)
        {
            if (endpoint.ContentFormat == SyndicationContentFormat.Rss)
            {
                RssFeed feed = new();
                await feed.LoadAsync(endpoint.Source).ConfigureAwait(false);
                if (feed.Channel.HasExtensions)
                {
                    // Process feed extensions
                }
            }
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.IsPingbackEnabledAsync(Uri) method
    /// </summary>
    public static async Task IsPingbackEnabledExampleAsync()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        if (await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(source).ConfigureAwait(false))
        {
            //  Parse source for Pingback information
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(Uri) method
    /// </summary>
    public static async Task LocatePingbackNotificationServerExampleAsync()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        Uri? pingbackServer = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(source).ConfigureAwait(false);
        if (pingbackServer != null)
        {
            Argotic.Net.XmlRpcClient client = new(pingbackServer);
            Argotic.Net.XmlRpcMessage message = new();

            // Build the Pingback XML-RPC message to be sent

            await client.SendAsync(message).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(Uri) method
    /// </summary>
    public static async Task IsTrackbackEnabledExampleAsync()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        if (await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(source).ConfigureAwait(false))
        {
            // Parse source for Trackback information
        }
    }

    /// <summary>
    /// Provides example code for the SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(Uri) method
    /// </summary>
    public static async Task LocateTrackbackNotificationServersExampleAsync()
    {
        Uri source = new("http://blog.oppositionallydefiant.com/post/SystemIOIntuition-Leveraging-human-pattern-recognition.aspx");

        Collection<TrackbackDiscoveryMetadata> endpoints = await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(source).ConfigureAwait(false);
        foreach (TrackbackDiscoveryMetadata endpoint in endpoints)
        {
            Argotic.Net.TrackbackClient client = new(endpoint.PingUrl);
            Argotic.Net.TrackbackMessage message = new();

            //  Build Trackback url-encoded message to be sent

            await client.SendAsync(message).ConfigureAwait(false);
        }
    }
}