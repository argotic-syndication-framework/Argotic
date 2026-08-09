using Argotic.Common;
using Argotic.Syndication;
using Spectre.Console;

namespace Argotic.Examples.Common;

/// <summary>
/// Demonstrates <see cref="SyndicationDiscoveryUtility"/>: what format a URL serves, whether one page links to another, whether a URL exists, conditional GET, and locating syndication, Pingback and Trackback endpoints.
/// </summary>
/// <remarks>
///     Every method here fetches from a live origin, so all of them are marked
///     <see cref="RequiresNetworkAttribute"/> and are skipped by the offline gate.
/// </remarks>
internal static class SyndicationDiscoveryUtilityExample
{
    /// <summary>
    /// Determines which syndication format a URL serves, without parsing it fully.
    /// </summary>
    [RequiresNetwork]
    public static async Task SyndicationContentFormatGetExampleAsync()
    {
        Uri url = new("https://endjin.com/rss.xml");

        SyndicationContentFormat format = await SyndicationDiscoveryUtility.SyndicationContentFormatGetAsync(url).ConfigureAwait(false);

        if (format != SyndicationContentFormat.None)
        {
            // Do something based on the determined content format
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {url}");
        AnsiConsole.MarkupLine($"  [dim]Format:[/] {format}");
    }

    /// <summary>
    /// Determines whether one web resource links to another — the check behind Pingback and Trackback validation.
    /// </summary>
    [RequiresNetwork]
    public static async Task SourceReferencesTargetExampleAsync()
    {
        //  Certain syndication scenarios involve verifying that one web resource references or 'links' to another web resource.

        Uri source = new("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases");
        Uri target = new("https://endjin.com");

        bool references = await SyndicationDiscoveryUtility.SourceReferencesTargetAsync(source, target).ConfigureAwait(false);
        if (references)
        {
            // Perform some action based on source referencing the target.
        }

        AnsiConsole.MarkupLine($"  [dim]Source:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Target:[/] {target}");
        AnsiConsole.MarkupLine($"  [dim]References:[/] {references}");
    }

    /// <summary>
    /// Determines whether a URL resolves, without downloading its body.
    /// </summary>
    [RequiresNetwork]
    public static async Task UriExistsExampleAsync()
    {
        Uri source = new("https://endjin.com/blog/");

        bool exists = await SyndicationDiscoveryUtility.UriExistsAsync(source).ConfigureAwait(false);
        if (exists)
        {
            // Perform some action based on source existing.
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Exists:[/] {exists}");
    }

    /// <summary>
    /// Re-fetches a feed only if it changed, using the modification date and entity tag from the previous response.
    /// </summary>
    [RequiresNetwork]
    public static async Task ConditionalGetExampleAsync()
    {
        Uri source = new("https://endjin.com/rss.xml");

        using HttpClient client = new();
        using HttpRequestMessage request = new(HttpMethod.Get, source);
        request.Headers.UserAgent.ParseAdd("Some User Agent 1.0.0.0");

        using HttpResponseMessage httpResponse = await client.SendAsync(request).ConfigureAwait(false);

        DateTime lastModified = httpResponse.Content.Headers.LastModified?.DateTime ?? DateTime.MinValue;
        string? entityTag = httpResponse.Headers.ETag?.Tag;

        /*
            Typically the consumer would store the modification date and entity tag information for the resource,
            and some amount of time passes. Consumer can now use a conditional GET operation to determine if
            the web resource has changed since it was last retrieved. This minimizes bandwidth usage significantly.
        */
        using ConditionalGetResult conditionalResponse = await SyndicationDiscoveryUtility.ConditionalGetAsync(source, lastModified, entityTag!).ConfigureAwait(false);
        if (conditionalResponse.WasModified)
        {
            // Web resource has been modified since last retrieval, consumer would process the new data.
            using Stream stream = await conditionalResponse.GetResponseStreamAsync().ConfigureAwait(false);
            // Process the stream...
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Was Modified:[/] {conditionalResponse.WasModified}");
    }


    /// <summary>
    /// Finds the feeds an HTML page advertises through its <c>link</c> elements.
    /// </summary>
    [RequiresNetwork]
    public static async Task LocateDiscoverableSyndicationEndpointsExampleAsync()
    {
        Uri source = new("https://endjin.com/what-we-think/talks/");

        IList<DiscoverableSyndicationEndpoint> endpoints = await SyndicationDiscoveryUtility.LocateDiscoverableSyndicationEndpointsAsync(source).ConfigureAwait(false);

        foreach (DiscoverableSyndicationEndpoint endpoint in endpoints)
        {
            if (endpoint.ContentFormat == SyndicationContentFormat.Rss)
            {
                RssFeed feed = new();
                await feed.LoadAsync(endpoint.Source!).ConfigureAwait(false);
                if (feed.Channel.HasExtensions)
                {
                    // Process feed extensions
                }
            }
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Endpoints found:[/] {endpoints.Count}");
    }

    /// <summary>
    /// Determines whether a page accepts Pingback notifications.
    /// </summary>
    [RequiresNetwork]
    public static async Task IsPingbackEnabledExampleAsync()
    {
        Uri source = new("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases");

        bool isPingbackEnabled = await SyndicationDiscoveryUtility.IsPingbackEnabledAsync(source).ConfigureAwait(false);
        if (isPingbackEnabled)
        {
            //  Parse source for Pingback information
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Pingback Enabled:[/] {isPingbackEnabled}");
    }

    /// <summary>
    /// Finds a page's Pingback server and sends it an XML-RPC notification.
    /// </summary>
    [RequiresNetwork]
    public static async Task LocatePingbackNotificationServerExampleAsync()
    {
        Uri source = new("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases");

        Uri? pingbackServer = await SyndicationDiscoveryUtility.LocatePingbackNotificationServerAsync(source).ConfigureAwait(false);
        if (pingbackServer is not null)
        {
            Argotic.Net.XmlRpcClient client = new(pingbackServer);
            Argotic.Net.XmlRpcMessage message = new();

            // Build the Pingback XML-RPC message to be sent

            await client.SendAsync(message).ConfigureAwait(false);
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Pingback Server:[/] {pingbackServer?.ToString() ?? "Not found"}");
    }

    /// <summary>
    /// Determines whether a page accepts Trackback notifications.
    /// </summary>
    [RequiresNetwork]
    public static async Task IsTrackbackEnabledExampleAsync()
    {
        Uri source = new("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases");

        bool isTrackbackEnabled = await SyndicationDiscoveryUtility.IsTrackbackEnabledAsync(source).ConfigureAwait(false);
        if (isTrackbackEnabled)
        {
            // Parse source for Trackback information
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Trackback Enabled:[/] {isTrackbackEnabled}");
    }

    /// <summary>
    /// Finds a page's Trackback servers and sends each one a notification.
    /// </summary>
    [RequiresNetwork]
    public static async Task LocateTrackbackNotificationServersExampleAsync()
    {
        Uri source = new("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases");

        IList<TrackbackDiscoveryMetadata> endpoints = await SyndicationDiscoveryUtility.LocateTrackbackNotificationServersAsync(source).ConfigureAwait(false);
        foreach (TrackbackDiscoveryMetadata endpoint in endpoints)
        {
            Argotic.Net.TrackbackClient client = new(endpoint.PingUrl!);
            Argotic.Net.TrackbackMessage message = new();

            //  Build Trackback url-encoded message to be sent

            await client.SendAsync(message).ConfigureAwait(false);
        }

        AnsiConsole.MarkupLine($"  [dim]URL:[/] {source}");
        AnsiConsole.MarkupLine($"  [dim]Trackback Servers:[/] {endpoints.Count}");
    }
}