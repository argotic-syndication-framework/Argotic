using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core;

/// <summary>
/// Reads a feed without knowing whether it is RSS or Atom, through <see cref="GenericSyndicationFeed"/>.
/// </summary>
/// <remarks>
///     The wrapper exposes only what both formats agree on — title, description, categories, items — and
///     hands back the underlying <c>RssFeed</c> or <c>AtomFeed</c> through <c>Resource</c> when you need
///     the rest.
/// </remarks>
internal static class GenericSyndicationFeedExample
{
    /// <summary>
    /// Walks a feed's categories and items without knowing which format produced them.
    /// </summary>
    [RequiresNetwork]
    public static async Task ClassExampleAsync()
    {
        GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(new Uri("https://endjin.com/rss.xml")).ConfigureAwait(false);

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

        ExampleOutput.ShowGenericFeed(feed);
    }

    /// <summary>
    /// Creates a <see cref="GenericSyndicationFeed"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    [RequiresNetwork]
    public static async Task CreateExampleAsync()
    {
        GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(new Uri("https://endjin.com/rss.xml")).ConfigureAwait(false);

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

        ExampleOutput.ShowGenericFeed(feed);
    }

    /// <summary>
    /// Loads a <see cref="GenericSyndicationFeed"/> from a <see cref="Uri"/>, and shows where a caller-supplied <see cref="HttpClient"/> goes.
    /// </summary>
    [RequiresNetwork]
    public static async Task LoadUriExampleAsync()
    {
        GenericSyndicationFeed feed = new();
        Uri source = new("https://endjin.com/rss.xml");

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

        ExampleOutput.ShowGenericFeed(feed);
    }
}