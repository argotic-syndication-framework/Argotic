using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Extensions.Core;

/// <summary>
/// Reads a <see cref="SimpleListSyndicationExtension"/> out of a feed the framework has already parsed, then writes the feed back out.
/// </summary>
/// <remarks>
///     Nothing registers the extension. Reflection over the assembly's exported types produces the
///     candidates; those whose namespace or prefix is bound on the document are asked whether they are
///     present; and each that says yes is attached to the entity that carried its elements — so the only
///     call a consumer makes is <c>FindExtension(MatchByType)</c>. The namespace declarations written
///     back on save are derived the same way, from the extensions actually present.
/// </remarks>
internal static class SimpleListSyndicationExtensionExample
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
            if (feed.Channel.FindExtension(SimpleListSyndicationExtension.MatchByType) is SimpleListSyndicationExtension channelExtension)
            {
                ExampleOutput.ShowSimpleListExtension(channelExtension);
            }
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            if (item.HasExtensions)
            {
                if (item.FindExtension(SimpleListSyndicationExtension.MatchByType) is SimpleListSyndicationExtension itemExtension)
                {
                    // Process extension for current item
                }
            }
        }

        // By default the framework will automatically determine what XML namespace attributes (xmlns) to write
        // on the root of the resource based on the extensions applied to extensible parent and child entities
        using MemoryStream stream = new();
        feed.Save(stream);

        ExampleOutput.ShowSaved("RssFeed");
    }

    /// <summary>
    /// Builds a <see cref="SimpleListSyndicationExtension"/> from scratch and reads it back.
    /// </summary>
    /// <remarks>
    ///     This is the extension the sample corpus used to declare and never use, so it attached to nothing
    ///     and its example printed only "Loaded" and "Saved". A feed that says treatAs=list is asking a
    ///     reader to present it as an ordered list rather than a stream, and the sort and group entries say
    ///     on which fields.
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

        SimpleListSyndicationExtension list = new();
        list.Context.TreatAsList = true;

        list.Context.Sorting.Add(new SimpleListSort
        {
            Element = "pubDate",
            Label = "Date published",
            DataType = SimpleListDataType.Date,
            IsDefault = true,
        });
        list.Context.Sorting.Add(new SimpleListSort
        {
            Element = "title",
            Label = "Title",
        });

        list.Context.Grouping.Add(new SimpleListGroup
        {
            Namespace = new Uri("http://purl.org/dc/elements/1.1/"),
            Element = "subject",
            Label = "Topic",
        });

        feed.Channel.Extensions.Add(list);

        feed.Channel.Items.Add(item);

        using MemoryStream saved = new();
        feed.Save(saved);
        ExampleOutput.ShowSaved("RssFeed");

        saved.Seek(0, SeekOrigin.Begin);
        RssFeed reloaded = new();
        reloaded.Load(saved);


        if (reloaded.Channel.FindExtension(SimpleListSyndicationExtension.MatchByType) is SimpleListSyndicationExtension readBack)
        {
            ExampleOutput.ShowSimpleListExtension(readBack);
        }
    }
}