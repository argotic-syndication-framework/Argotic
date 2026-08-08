using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads a <see cref="CreativeCommonsSyndicationExtension"/> out of a feed the framework has already parsed, then writes the feed back out.
/// </summary>
/// <remarks>
///     Nothing registers the extension. Reflection over the assembly's exported types produces the
///     candidates; those whose namespace or prefix is bound on the document are asked whether they are
///     present; and each that says yes is attached to the entity that carried its elements — so the only
///     call a consumer makes is <c>FindExtension(MatchByType)</c>. The namespace declarations written
///     back on save are derived the same way, from the extensions actually present.
/// </remarks>
internal static class CreativeCommonsSyndicationExtensionExample
{
    /// <summary>
    /// Loads a feed carrying the extension, finds it on the channel and on the items, then writes the feed back out.
    /// </summary>
    public static void ClassExample()
    {
        // Framework auto-discovers supported extensions based on XML namespace attributes (xmlns) defined on root of resource
        RssFeed feed = new();
        using (Stream inputStream = SampleDataPath.OpenRead(SampleDataPath.RssFeedWithExtensions))
        {
            feed.Load(inputStream);
        }

        ExampleOutput.ShowLoaded("RssFeed", feed.Channel.Title);

        // Extensible framework entities provide properties/methods to determine if entity is extended and predicate based searching against available extensions
        if (feed.Channel.HasExtensions)
        {
            if (feed.Channel.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) is CreativeCommonsSyndicationExtension channelExtension)
            {
                ExampleOutput.ShowCreativeCommonsExtension(channelExtension);
            }
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions)
            {
                if (item.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) is CreativeCommonsSyndicationExtension itemExtension)
                {
                    // Process extension for current item
                }
            }
        }

        int count = feed.Channel.Items.Count(i => i.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) is not null);
        ExampleOutput.ShowItemsWithExtension(count, feed.Channel.Items.Count, "CreativeCommons");

        // By default the framework will automatically determine what XML namespace attributes (xmlns) to write
        // on the root of the resource based on the extensions applied to extensible parent and child entities
        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Builds a <see cref="CreativeCommonsSyndicationExtension"/> from scratch and reads it back.
    /// </summary>
    /// <remarks>
    ///     Licences are a list, not a single value: a channel can offer several and an item can narrow them.
    /// </remarks>
    public static void AuthorExample()
    {
        RssFeed feed = new();
        feed.Channel.Title = "endjin blog";
        feed.Channel.Link = new Uri("https://endjin.com/blog/");
        feed.Channel.Description = "Technical writing from endjin on .NET, data, analytics and AI.";

        RssItem item = new()
        {
            Title = "Rx.NET v7.0 Released - it could save you 95MB!",
            Link = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            Description = "Moving UI framework support out of System.Reactive can cut 95MB from a deployment.",
        };

        CreativeCommonsSyndicationExtension channelLicence = new();
        channelLicence.Context.Licenses.Add(new Uri("http://creativecommons.org/licenses/by/4.0/"));
        feed.Channel.Extensions.Add(channelLicence);

        CreativeCommonsSyndicationExtension itemLicence = new();
        itemLicence.Context.Licenses.Add(new Uri("http://creativecommons.org/licenses/by-nc/4.0/"));
        item.Extensions.Add(itemLicence);

        feed.Channel.Items.Add(item);

        using MemoryStream saved = new();
        feed.Save(saved);
        ExampleOutput.ShowSaved("RssFeed");

        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);

        RssItem readItem = reloaded.Channel.Items[0];
        if (readItem.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) is CreativeCommonsSyndicationExtension readBack)
        {
            ExampleOutput.ShowCreativeCommonsExtension(readBack);
        }
    }
}