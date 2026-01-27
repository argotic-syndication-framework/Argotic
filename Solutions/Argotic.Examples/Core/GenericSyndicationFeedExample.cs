using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core;

/// <summary>
/// Contains the code examples for the <see cref="GenericSyndicationFeed"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="GenericSyndicationFeed"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class GenericSyndicationFeedExample
{
    /// <summary>
    /// Provides example code for the GenericSyndicationFeed class.
    /// </summary>
    public static async Task ClassExampleAsync()
    {
        GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(new Uri("http://feeds.feedburner.com/OppositionallyDefiant")).ConfigureAwait(false);

        foreach (GenericSyndicationCategory category in feed.Categories)
        {
            if (string.Equals(category.Term, ".NET", StringComparison.OrdinalIgnoreCase))
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
                if (string.Equals(category.Term, "WCF", StringComparison.OrdinalIgnoreCase))
                {
                    //  Process item category
                }
            }
        }

        if (feed.Format == SyndicationContentFormat.Rss)
        {
            if (feed.Resource is RssFeed)
            {
                //  Process RSS format specific information
            }
        }
    }

    /// <summary>
    /// Provides example code for the GenericSyndicationFeed.CreateAsync(Uri) method
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(new Uri("http://feeds.feedburner.com/OppositionallyDefiant")).ConfigureAwait(false);

        foreach (GenericSyndicationItem item in feed.Items)
        {
            if (item.PublishedOn > DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process generic item's published in the last week
            }

            foreach (GenericSyndicationCategory category in item.Categories)
            {
                if (string.Equals(category.Term, "WCF", StringComparison.OrdinalIgnoreCase))
                {
                    //  Process item category
                }
            }
        }
    }
    /// <summary>
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        GenericSyndicationFeed feed = new();
        Uri source = new("http://feeds.feedburner.com/OppositionallyDefiant");

        // For simple case (no credentials):
        await feed.LoadAsync(source).ConfigureAwait(false);

        // Or for credentials:
        // var handler = new SocketsHttpHandler { Credentials = CredentialCache.DefaultNetworkCredentials };
        // using var httpClient = new HttpClient(handler);
        // await feed.LoadAsync(source, httpClient);

        foreach (GenericSyndicationItem item in feed.Items)
        {
            if (item.PublishedOn > DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)))
            {
                //  Process generic item's published in the last week
            }

            foreach (GenericSyndicationCategory category in item.Categories)
            {
                if (string.Equals(category.Term, "WCF", StringComparison.OrdinalIgnoreCase))
                {
                    //  Process item category
                }
            }
        }
    }
}
