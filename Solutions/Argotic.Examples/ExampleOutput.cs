using Argotic.Extensions.Core;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;
using Spectre.Console;

namespace Argotic.Examples;

/// <summary>
/// Helper class for displaying meaningful output from examples.
/// </summary>
public static class ExampleOutput
{
    /// <summary>
    /// Displays information about an RSS feed.
    /// </summary>
    public static void ShowRssFeed(RssFeed feed)
    {
        AnsiConsole.MarkupLine($"  [dim]RSS Feed:[/] [blue]{Markup.Escape(feed.Channel.Title)}[/]");
        AnsiConsole.MarkupLine($"  [dim]Link:[/] {Markup.Escape(feed.Channel.Link?.ToString() ?? "")}");
        AnsiConsole.MarkupLine($"  [dim]Items:[/] {feed.Channel.Items.Count}");
        foreach (RssItem item in feed.Channel.Items.Take(3))
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(item.Title)}");
        }
        if (feed.Channel.Items.Count > 3)
        {
            AnsiConsole.MarkupLine($"    [dim]... and {feed.Channel.Items.Count - 3} more[/]");
        }
    }

    /// <summary>
    /// Displays information about an Atom feed.
    /// </summary>
    public static void ShowAtomFeed(AtomFeed feed)
    {
        AnsiConsole.MarkupLine($"  [dim]Atom Feed:[/] [blue]{Markup.Escape(feed.Title?.Content ?? "")}[/]");
        AnsiConsole.MarkupLine($"  [dim]Entries:[/] {feed.Entries.Count}");
        foreach (AtomEntry entry in feed.Entries.Take(3))
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(entry.Title?.Content ?? "")}");
        }
        if (feed.Entries.Count > 3)
        {
            AnsiConsole.MarkupLine($"    [dim]... and {feed.Entries.Count - 3} more[/]");
        }
    }

    /// <summary>
    /// Displays information about an Atom entry.
    /// </summary>
    public static void ShowAtomEntry(AtomEntry entry)
    {
        AnsiConsole.MarkupLine($"  [dim]Atom Entry:[/] [blue]{Markup.Escape(entry.Title?.Content ?? "")}[/]");
        AnsiConsole.MarkupLine($"  [dim]Updated:[/] {entry.UpdatedOn}");
        if (entry.Summary != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Summary:[/] {Markup.Escape(Truncate(entry.Summary.Content, 80))}");
        }
    }

    /// <summary>
    /// Displays information about an OPML document.
    /// </summary>
    public static void ShowOpmlDocument(OpmlDocument document)
    {
        AnsiConsole.MarkupLine($"  [dim]OPML Document:[/] [blue]{Markup.Escape(document.Head.Title)}[/]");
        AnsiConsole.MarkupLine($"  [dim]Outlines:[/] {CountOutlines(document.Outlines)}");
        foreach (OpmlOutline outline in document.Outlines.Take(3))
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(outline.Text)}");
        }
    }

    /// <summary>
    /// Displays information about an APML document.
    /// </summary>
    public static void ShowApmlDocument(ApmlDocument document)
    {
        AnsiConsole.MarkupLine($"  [dim]APML Document:[/] [blue]{Markup.Escape(document.Head.Title)}[/]");
        AnsiConsole.MarkupLine($"  [dim]Profiles:[/] {document.Profiles.Count}");
        foreach (ApmlProfile profile in document.Profiles)
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(profile.Name)}");
        }
    }

    /// <summary>
    /// Displays information about a BlogML document.
    /// </summary>
    public static void ShowBlogMLDocument(BlogMLDocument document)
    {
        AnsiConsole.MarkupLine($"  [dim]BlogML Document:[/] [blue]{Markup.Escape(document.Title?.Content ?? "")}[/]");
        AnsiConsole.MarkupLine($"  [dim]Posts:[/] {document.Posts.Count}");
        AnsiConsole.MarkupLine($"  [dim]Categories:[/] {document.Categories.Count}");
        AnsiConsole.MarkupLine($"  [dim]Authors:[/] {document.Authors.Count}");
    }

    /// <summary>
    /// Displays information about an RSD document.
    /// </summary>
    public static void ShowRsdDocument(RsdDocument document)
    {
        AnsiConsole.MarkupLine($"  [dim]RSD Document:[/] [blue]{Markup.Escape(document.EngineName)}[/]");
        AnsiConsole.MarkupLine($"  [dim]Interfaces:[/] {document.Interfaces.Count}");
        foreach (RsdApplicationInterface api in document.Interfaces)
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(api.Name)}{(api.IsPreferred ? " [green](preferred)[/]" : "")}");
        }
    }

    /// <summary>
    /// Displays information about a generic syndication feed.
    /// </summary>
    public static void ShowGenericFeed(GenericSyndicationFeed feed)
    {
        AnsiConsole.MarkupLine($"  [dim]Feed Title:[/] [blue]{Markup.Escape(feed.Title)}[/]");
        AnsiConsole.MarkupLine($"  [dim]Format:[/] {feed.Format}");
        AnsiConsole.MarkupLine($"  [dim]Items:[/] {feed.Items.Count}");
        foreach (GenericSyndicationItem item in feed.Items.Take(3))
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(item.Title)}");
        }
    }

    /// <summary>
    /// Displays a message indicating an object was created.
    /// </summary>
    public static void ShowCreated(string typeName, string? name = null)
    {
        if (name != null)
        {
            AnsiConsole.MarkupLine($"  [green]Created[/] [dim]{Markup.Escape(typeName)}:[/] {Markup.Escape(name)}");
        }
        else
        {
            AnsiConsole.MarkupLine($"  [green]Created[/] [dim]{Markup.Escape(typeName)}[/]");
        }
    }

    /// <summary>
    /// Displays a message indicating an object was loaded.
    /// </summary>
    public static void ShowLoaded(string typeName, string? details = null)
    {
        if (details != null)
        {
            AnsiConsole.MarkupLine($"  [green]Loaded[/] [dim]{Markup.Escape(typeName)}:[/] {Markup.Escape(details)}");
        }
        else
        {
            AnsiConsole.MarkupLine($"  [green]Loaded[/] [dim]{Markup.Escape(typeName)}[/]");
        }
    }

    /// <summary>
    /// Displays a message indicating an object was saved.
    /// </summary>
    public static void ShowSaved(string typeName)
    {
        AnsiConsole.MarkupLine($"  [green]Saved[/] [dim]{Markup.Escape(typeName)}[/] to stream");
    }

    private static int CountOutlines(IEnumerable<OpmlOutline> outlines)
    {
        int count = 0;
        foreach (OpmlOutline outline in outlines)
        {
            count++;
            count += CountOutlines(outline.Outlines);
        }
        return count;
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..(maxLength - 3)] + "...";
    }

    // ========================================
    // RSS Element Display Methods
    // ========================================

    /// <summary>
    /// Displays information about an RSS channel.
    /// </summary>
    public static void ShowRssChannel(RssChannel channel)
    {
        AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(channel.Title)}");
        if (channel.Link != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Link:[/] {Markup.Escape(channel.Link.ToString())}");
        }
        AnsiConsole.MarkupLine($"  [dim]Description:[/] {Markup.Escape(Truncate(channel.Description, 60))}");
    }

    /// <summary>
    /// Displays information about an RSS item.
    /// </summary>
    public static void ShowRssItem(RssItem item)
    {
        AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(item.Title)}");
        if (!string.IsNullOrEmpty(item.Author))
        {
            AnsiConsole.MarkupLine($"  [dim]Author:[/] {Markup.Escape(item.Author)}");
        }
        if (item.Link != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Link:[/] {Markup.Escape(item.Link.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about an RSS category.
    /// </summary>
    public static void ShowRssCategory(RssCategory category)
    {
        AnsiConsole.MarkupLine($"  [dim]Value:[/] {Markup.Escape(category.Value)}");
        if (!string.IsNullOrEmpty(category.Domain))
        {
            AnsiConsole.MarkupLine($"  [dim]Domain:[/] {Markup.Escape(category.Domain)}");
        }
    }

    /// <summary>
    /// Displays information about an RSS cloud.
    /// </summary>
    public static void ShowRssCloud(RssCloud cloud)
    {
        AnsiConsole.MarkupLine($"  [dim]Domain:[/] {Markup.Escape(cloud.Domain)}");
        AnsiConsole.MarkupLine($"  [dim]Port:[/] {cloud.Port}");
        AnsiConsole.MarkupLine($"  [dim]Protocol:[/] {cloud.Protocol}");
    }

    /// <summary>
    /// Displays information about an RSS enclosure.
    /// </summary>
    public static void ShowRssEnclosure(RssEnclosure enclosure)
    {
        if (enclosure.Url != null)
        {
            AnsiConsole.MarkupLine($"  [dim]URL:[/] {Markup.Escape(enclosure.Url.ToString())}");
        }
        AnsiConsole.MarkupLine($"  [dim]Type:[/] {Markup.Escape(enclosure.ContentType)}");
        AnsiConsole.MarkupLine($"  [dim]Length:[/] {enclosure.Length} bytes");
    }

    /// <summary>
    /// Displays information about an RSS GUID.
    /// </summary>
    public static void ShowRssGuid(RssGuid guid)
    {
        AnsiConsole.MarkupLine($"  [dim]Value:[/] {Markup.Escape(guid.Value)}");
        AnsiConsole.MarkupLine($"  [dim]IsPermaLink:[/] {guid.IsPermanentLink}");
    }

    /// <summary>
    /// Displays information about an RSS image.
    /// </summary>
    public static void ShowRssImage(RssImage image)
    {
        if (image.Url != null)
        {
            AnsiConsole.MarkupLine($"  [dim]URL:[/] {Markup.Escape(image.Url.ToString())}");
        }
        AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(image.Title)}");
    }

    /// <summary>
    /// Displays information about an RSS text input.
    /// </summary>
    public static void ShowRssTextInput(RssTextInput textInput)
    {
        AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(textInput.Title)}");
        AnsiConsole.MarkupLine($"  [dim]Name:[/] {Markup.Escape(textInput.Name)}");
    }

    /// <summary>
    /// Displays information about an RSS source.
    /// </summary>
    public static void ShowRssSource(RssSource source)
    {
        AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(source.Title)}");
        if (source.Url != null)
        {
            AnsiConsole.MarkupLine($"  [dim]URL:[/] {Markup.Escape(source.Url.ToString())}");
        }
    }

    // ========================================
    // Atom Element Display Methods
    // ========================================

    /// <summary>
    /// Displays information about an Atom category.
    /// </summary>
    public static void ShowAtomCategory(AtomCategory category)
    {
        AnsiConsole.MarkupLine($"  [dim]Term:[/] {Markup.Escape(category.Term)}");
        if (!string.IsNullOrEmpty(category.Label))
        {
            AnsiConsole.MarkupLine($"  [dim]Label:[/] {Markup.Escape(category.Label)}");
        }
        if (category.Scheme != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Scheme:[/] {Markup.Escape(category.Scheme.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about an Atom link.
    /// </summary>
    public static void ShowAtomLink(AtomLink link)
    {
        if (link.Uri != null)
        {
            AnsiConsole.MarkupLine($"  [dim]URI:[/] {Markup.Escape(link.Uri.ToString())}");
        }
        if (!string.IsNullOrEmpty(link.Relation))
        {
            AnsiConsole.MarkupLine($"  [dim]Relation:[/] {Markup.Escape(link.Relation)}");
        }
    }

    /// <summary>
    /// Displays information about an Atom content.
    /// </summary>
    public static void ShowAtomContent(AtomContent content)
    {
        AnsiConsole.MarkupLine($"  [dim]Content Type:[/] {content.ContentType}");
        if (!string.IsNullOrEmpty(content.Content))
        {
            AnsiConsole.MarkupLine($"  [dim]Content:[/] {Markup.Escape(Truncate(content.Content, 60))}");
        }
    }

    /// <summary>
    /// Displays information about an Atom generator.
    /// </summary>
    public static void ShowAtomGenerator(AtomGenerator generator)
    {
        AnsiConsole.MarkupLine($"  [dim]Name:[/] {Markup.Escape(generator.Content)}");
        if (!string.IsNullOrEmpty(generator.Version))
        {
            AnsiConsole.MarkupLine($"  [dim]Version:[/] {Markup.Escape(generator.Version)}");
        }
    }

    /// <summary>
    /// Displays information about an Atom icon.
    /// </summary>
    public static void ShowAtomIcon(AtomIcon icon)
    {
        if (icon.Uri != null)
        {
            AnsiConsole.MarkupLine($"  [dim]URI:[/] {Markup.Escape(icon.Uri.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about an Atom logo.
    /// </summary>
    public static void ShowAtomLogo(AtomLogo logo)
    {
        if (logo.Uri != null)
        {
            AnsiConsole.MarkupLine($"  [dim]URI:[/] {Markup.Escape(logo.Uri.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about an Atom ID.
    /// </summary>
    public static void ShowAtomId(AtomId id)
    {
        if (id.Uri != null)
        {
            AnsiConsole.MarkupLine($"  [dim]ID:[/] {Markup.Escape(id.Uri.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about an Atom person construct.
    /// </summary>
    public static void ShowAtomPersonConstruct(AtomPersonConstruct person)
    {
        AnsiConsole.MarkupLine($"  [dim]Name:[/] {Markup.Escape(person.Name)}");
        if (!string.IsNullOrEmpty(person.EmailAddress))
        {
            AnsiConsole.MarkupLine($"  [dim]Email:[/] {Markup.Escape(person.EmailAddress)}");
        }
    }

    /// <summary>
    /// Displays information about an Atom source.
    /// </summary>
    public static void ShowAtomSource(AtomSource source)
    {
        if (source.Title != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(source.Title.Content)}");
        }
    }

    /// <summary>
    /// Displays information about an Atom text construct.
    /// </summary>
    public static void ShowAtomTextConstruct(AtomTextConstruct text)
    {
        AnsiConsole.MarkupLine($"  [dim]Text Type:[/] {text.TextType}");
        if (!string.IsNullOrEmpty(text.Content))
        {
            AnsiConsole.MarkupLine($"  [dim]Content:[/] {Markup.Escape(Truncate(text.Content, 60))}");
        }
    }

    // ========================================
    // APML Element Display Methods
    // ========================================

    /// <summary>
    /// Displays information about an APML application.
    /// </summary>
    public static void ShowApmlApplication(ApmlApplication application)
    {
        AnsiConsole.MarkupLine($"  [dim]Name:[/] {Markup.Escape(application.Name)}");
    }

    /// <summary>
    /// Displays information about an APML author.
    /// </summary>
    public static void ShowApmlAuthor(ApmlAuthor author)
    {
        AnsiConsole.MarkupLine($"  [dim]Key:[/] {Markup.Escape(author.Key)}");
        AnsiConsole.MarkupLine($"  [dim]Value:[/] {author.Value}");
    }

    /// <summary>
    /// Displays information about an APML concept.
    /// </summary>
    public static void ShowApmlConcept(ApmlConcept concept)
    {
        AnsiConsole.MarkupLine($"  [dim]Key:[/] {Markup.Escape(concept.Key)}");
        AnsiConsole.MarkupLine($"  [dim]Value:[/] {concept.Value}");
    }

    /// <summary>
    /// Displays information about an APML profile.
    /// </summary>
    public static void ShowApmlProfile(ApmlProfile profile)
    {
        AnsiConsole.MarkupLine($"  [dim]Name:[/] {Markup.Escape(profile.Name)}");
    }

    /// <summary>
    /// Displays information about an APML source.
    /// </summary>
    public static void ShowApmlSource(ApmlSource source)
    {
        AnsiConsole.MarkupLine($"  [dim]Key:[/] {Markup.Escape(source.Key)}");
        AnsiConsole.MarkupLine($"  [dim]Value:[/] {source.Value}");
    }

    // ========================================
    // OPML Element Display Methods
    // ========================================

    /// <summary>
    /// Displays information about an OPML outline.
    /// </summary>
    public static void ShowOpmlOutline(OpmlOutline outline)
    {
        AnsiConsole.MarkupLine($"  [dim]Text:[/] {Markup.Escape(outline.Text)}");
        if (!string.IsNullOrEmpty(outline.ContentType))
        {
            AnsiConsole.MarkupLine($"  [dim]Type:[/] {Markup.Escape(outline.ContentType)}");
        }
        if (outline.Outlines.Count > 0)
        {
            AnsiConsole.MarkupLine($"  [dim]Sub-outlines:[/] {outline.Outlines.Count}");
        }
    }

    // ========================================
    // BlogML Element Display Methods
    // ========================================

    /// <summary>
    /// Displays information about a BlogML post.
    /// </summary>
    public static void ShowBlogMLPost(BlogMLPost post)
    {
        if (post.Title != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(post.Title.Content)}");
        }
        AnsiConsole.MarkupLine($"  [dim]Type:[/] {post.PostType}");
    }

    /// <summary>
    /// Displays information about a BlogML text construct.
    /// </summary>
    public static void ShowBlogMLTextConstruct(BlogMLTextConstruct text)
    {
        AnsiConsole.MarkupLine($"  [dim]Content Type:[/] {text.ContentType}");
        if (!string.IsNullOrEmpty(text.Content))
        {
            AnsiConsole.MarkupLine($"  [dim]Content:[/] {Markup.Escape(Truncate(text.Content, 60))}");
        }
    }

    // ========================================
    // RSD Element Display Methods
    // ========================================

    /// <summary>
    /// Displays information about an RSD application interface.
    /// </summary>
    public static void ShowRsdApplicationInterface(RsdApplicationInterface api)
    {
        AnsiConsole.MarkupLine($"  [dim]Name:[/] {Markup.Escape(api.Name)}");
        if (api.Link != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Link:[/] {Markup.Escape(api.Link.ToString())}");
        }
        AnsiConsole.MarkupLine($"  [dim]Preferred:[/] {api.IsPreferred}");
    }

    // ========================================
    // Network Client Display Methods
    // ========================================

    /// <summary>
    /// Displays information about a Trackback client.
    /// </summary>
    public static void ShowTrackbackClient(Uri host, string weblogName, string title)
    {
        if (host != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Host:[/] {Markup.Escape(host.ToString())}");
        }
        if (!string.IsNullOrEmpty(weblogName))
        {
            AnsiConsole.MarkupLine($"  [dim]Weblog Name:[/] {Markup.Escape(weblogName)}");
        }
        if (!string.IsNullOrEmpty(title))
        {
            AnsiConsole.MarkupLine($"  [dim]Title:[/] {Markup.Escape(title)}");
        }
    }

    /// <summary>
    /// Displays information about an XML-RPC client.
    /// </summary>
    public static void ShowXmlRpcClient(Uri host, string methodName)
    {
        if (host != null)
        {
            AnsiConsole.MarkupLine($"  [dim]Host:[/] {Markup.Escape(host.ToString())}");
        }
        if (!string.IsNullOrEmpty(methodName))
        {
            AnsiConsole.MarkupLine($"  [dim]Method Name:[/] {Markup.Escape(methodName)}");
        }
    }

    // ========================================
    // Extension Display Methods
    // ========================================

    /// <summary>
    /// Displays information about a Dublin Core Element Set extension.
    /// </summary>
    public static void ShowDublinCoreExtension(DublinCoreElementSetSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Dublin Core Extension:[/]");
        if (!string.IsNullOrEmpty(ext.Context.Creator))
        {
            AnsiConsole.MarkupLine($"    [dim]Creator:[/] {Markup.Escape(ext.Context.Creator)}");
        }
        if (!string.IsNullOrEmpty(ext.Context.Subject))
        {
            AnsiConsole.MarkupLine($"    [dim]Subject:[/] {Markup.Escape(ext.Context.Subject)}");
        }
        if (!string.IsNullOrEmpty(ext.Context.Title))
        {
            AnsiConsole.MarkupLine($"    [dim]Title:[/] {Markup.Escape(ext.Context.Title)}");
        }
    }

    /// <summary>
    /// Displays information about a Dublin Core Metadata Terms extension.
    /// </summary>
    public static void ShowDublinCoreMetadataTermsExtension(DublinCoreMetadataTermsSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Dublin Core Metadata Terms Extension:[/]");
        if (ext.Context.DateCreated != DateTime.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Created:[/] {ext.Context.DateCreated}");
        }
        if (ext.Context.DateModified != DateTime.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Modified:[/] {ext.Context.DateModified}");
        }
    }

    /// <summary>
    /// Displays information about an iTunes extension.
    /// </summary>
    public static void ShowITunesExtension(ITunesSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]iTunes Extension:[/]");
        if (!string.IsNullOrEmpty(ext.Context.Author))
        {
            AnsiConsole.MarkupLine($"    [dim]Author:[/] {Markup.Escape(ext.Context.Author)}");
        }
        if (ext.Context.Categories.Count > 0)
        {
            AnsiConsole.MarkupLine($"    [dim]Categories:[/] {ext.Context.Categories.Count}");
        }
        if (ext.Context.Duration != TimeSpan.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Duration:[/] {ext.Context.Duration}");
        }
    }

    /// <summary>
    /// Displays information about a Yahoo Media extension.
    /// </summary>
    public static void ShowYahooMediaExtension(YahooMediaSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Yahoo Media Extension:[/]");
        if (ext.Context.Contents.Count > 0)
        {
            AnsiConsole.MarkupLine($"    [dim]Contents:[/] {ext.Context.Contents.Count}");
        }
        if (ext.Context.Thumbnails.Count > 0)
        {
            AnsiConsole.MarkupLine($"    [dim]Thumbnails:[/] {ext.Context.Thumbnails.Count}");
        }
        if (ext.Context.Groups.Count > 0)
        {
            AnsiConsole.MarkupLine($"    [dim]Groups:[/] {ext.Context.Groups.Count}");
        }
    }

    /// <summary>
    /// Displays information about a Creative Commons extension.
    /// </summary>
    public static void ShowCreativeCommonsExtension(CreativeCommonsSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Creative Commons Extension:[/]");
        AnsiConsole.MarkupLine($"    [dim]Licenses:[/] {ext.Context.Licenses.Count}");
        foreach (Uri license in ext.Context.Licenses.Take(2))
        {
            AnsiConsole.MarkupLine($"      - {Markup.Escape(license.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about a Basic Geocoding extension.
    /// </summary>
    public static void ShowBasicGeocodingExtension(BasicGeocodingSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Basic Geocoding Extension:[/]");
        if (ext.Context.Latitude != decimal.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Latitude:[/] {ext.Context.Latitude}");
        }
        if (ext.Context.Longitude != decimal.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Longitude:[/] {ext.Context.Longitude}");
        }
    }

    /// <summary>
    /// Displays information about a Blog Channel extension.
    /// </summary>
    public static void ShowBlogChannelExtension(BlogChannelSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Blog Channel Extension:[/]");
        if (ext.Context.BlogRoll != null)
        {
            AnsiConsole.MarkupLine($"    [dim]BlogRoll:[/] {Markup.Escape(ext.Context.BlogRoll.ToString())}");
        }
        if (ext.Context.MySubscriptions != null)
        {
            AnsiConsole.MarkupLine($"    [dim]MySubscriptions:[/] {Markup.Escape(ext.Context.MySubscriptions.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about a Feed History extension.
    /// </summary>
    public static void ShowFeedHistoryExtension(FeedHistorySyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Feed History Extension:[/]");
        AnsiConsole.MarkupLine($"    [dim]Is Archive:[/] {ext.Context.IsArchive}");
        AnsiConsole.MarkupLine($"    [dim]Is Complete:[/] {ext.Context.IsComplete}");
    }

    /// <summary>
    /// Displays information about a Feed Rank extension.
    /// </summary>
    public static void ShowFeedRankExtension(FeedRankSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Feed Rank Extension:[/]");
        if (ext.Context.Value != decimal.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Rank:[/] {ext.Context.Value}");
        }
    }

    /// <summary>
    /// Displays information about a Feed Synchronization extension.
    /// </summary>
    public static void ShowFeedSyncExtension(FeedSynchronizationSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Feed Sync Extension:[/]");
        if (ext.Context.Sharing != null)
        {
            AnsiConsole.MarkupLine($"    [dim]Since:[/] {ext.Context.Sharing.Since}");
            AnsiConsole.MarkupLine($"    [dim]Until:[/] {ext.Context.Sharing.Until}");
        }
    }

    /// <summary>
    /// Displays information about a LiveJournal extension.
    /// </summary>
    public static void ShowLiveJournalExtension(LiveJournalSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]LiveJournal Extension:[/]");
        if (ext.Context.Mood != null && !string.IsNullOrEmpty(ext.Context.Mood.Content))
        {
            AnsiConsole.MarkupLine($"    [dim]Mood:[/] {Markup.Escape(ext.Context.Mood.Content)}");
        }
        if (!string.IsNullOrEmpty(ext.Context.Music))
        {
            AnsiConsole.MarkupLine($"    [dim]Music:[/] {Markup.Escape(ext.Context.Music)}");
        }
    }

    /// <summary>
    /// Displays information about a Pheed extension.
    /// </summary>
    public static void ShowPheedExtension(PheedSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Pheed Extension:[/]");
        if (ext.Context.Thumbnail != null)
        {
            AnsiConsole.MarkupLine($"    [dim]Thumbnail:[/] {Markup.Escape(ext.Context.Thumbnail.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about a Pingback extension.
    /// </summary>
    public static void ShowPingbackExtension(PingbackSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Pingback Extension:[/]");
        if (ext.Context.Server != null)
        {
            AnsiConsole.MarkupLine($"    [dim]Server:[/] {Markup.Escape(ext.Context.Server.ToString())}");
        }
    }

    /// <summary>
    /// Displays information about a Simple List extension.
    /// </summary>
    public static void ShowSimpleListExtension(SimpleListSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Simple List Extension:[/]");
        AnsiConsole.MarkupLine($"    [dim]TreatAsList:[/] {ext.Context.TreatAsList}");
        if (ext.Context.Sorting.Count > 0)
        {
            AnsiConsole.MarkupLine($"    [dim]Sort Fields:[/] {ext.Context.Sorting.Count}");
        }
    }

    /// <summary>
    /// Displays information about a Site Summary Content extension.
    /// </summary>
    public static void ShowContentExtension(SiteSummaryContentSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Content Extension:[/]");
        if (!string.IsNullOrEmpty(ext.Context.Encoded))
        {
            AnsiConsole.MarkupLine($"    [dim]Encoded:[/] {Markup.Escape(Truncate(ext.Context.Encoded, 60))}");
        }
    }

    /// <summary>
    /// Displays information about a Site Summary Slash extension.
    /// </summary>
    public static void ShowSlashExtension(SiteSummarySlashSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Slash Extension:[/]");
        if (ext.Context.Comments != int.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Comments:[/] {ext.Context.Comments}");
        }
        if (!string.IsNullOrEmpty(ext.Context.Section))
        {
            AnsiConsole.MarkupLine($"    [dim]Section:[/] {Markup.Escape(ext.Context.Section)}");
        }
    }

    /// <summary>
    /// Displays information about a Site Summary Update extension.
    /// </summary>
    public static void ShowUpdateExtension(SiteSummaryUpdateSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Update Extension:[/]");
        if (ext.Context.Period != SiteSummaryUpdatePeriod.None)
        {
            AnsiConsole.MarkupLine($"    [dim]Period:[/] {ext.Context.Period}");
        }
        if (ext.Context.Frequency != int.MinValue)
        {
            AnsiConsole.MarkupLine($"    [dim]Frequency:[/] {ext.Context.Frequency}");
        }
    }

    /// <summary>
    /// Displays information about a Trackback extension.
    /// </summary>
    public static void ShowTrackbackExtension(TrackbackSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]Trackback Extension:[/]");
        if (ext.Context.Ping != null)
        {
            AnsiConsole.MarkupLine($"    [dim]Ping URL:[/] {Markup.Escape(ext.Context.Ping.ToString())}");
        }
        if (ext.Context.Abouts.Count > 0)
        {
            AnsiConsole.MarkupLine($"    [dim]About URLs:[/] {ext.Context.Abouts.Count}");
        }
    }

    /// <summary>
    /// Displays information about a Well-Formed Web Comments extension.
    /// </summary>
    public static void ShowWellFormedWebCommentsExtension(WellFormedWebCommentsSyndicationExtension ext)
    {
        AnsiConsole.MarkupLine("  [blue]WFW Comments Extension:[/]");
        if (ext.Context.Comments != null)
        {
            AnsiConsole.MarkupLine($"    [dim]Comment URL:[/] {Markup.Escape(ext.Context.Comments.ToString())}");
        }
        if (ext.Context.CommentsFeed != null)
        {
            AnsiConsole.MarkupLine($"    [dim]Comment RSS:[/] {Markup.Escape(ext.Context.CommentsFeed.ToString())}");
        }
    }

    /// <summary>
    /// Displays a summary of extensions found on an extensible object.
    /// </summary>
    public static void ShowExtensionsSummary(int extensionCount, string entityType = "entity")
    {
        if (extensionCount > 0)
        {
            AnsiConsole.MarkupLine($"  [dim]Extensions on {entityType}:[/] {extensionCount}");
        }
    }

    /// <summary>
    /// Displays information about items with a specific extension type.
    /// </summary>
    public static void ShowItemsWithExtension(int count, int total, string extensionName)
    {
        AnsiConsole.MarkupLine($"  [dim]Items with {extensionName}:[/] {count}/{total}");
    }

    // ========================================
    // Sitemap Display Methods
    // ========================================

    /// <summary>
    /// Displays information about a Sitemap.
    /// </summary>
    public static void ShowSitemap(Syndication.Sitemap sitemap)
    {
        AnsiConsole.MarkupLine($"  [dim]Sitemap URLs:[/] [blue]{sitemap.Urls.Count}[/]");
        foreach (SitemapUrl url in sitemap.Urls.Take(5))
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(url.Location?.ToString() ?? "")}");
            if (url.LastModified.HasValue)
            {
                AnsiConsole.MarkupLine($"      [dim]Last Modified:[/] {url.LastModified.Value:yyyy-MM-dd}");
            }
            if (url.ChangeFrequency.HasValue)
            {
                AnsiConsole.MarkupLine($"      [dim]Change Frequency:[/] {url.ChangeFrequency.Value}");
            }
            if (url.Priority.HasValue)
            {
                AnsiConsole.MarkupLine($"      [dim]Priority:[/] {url.Priority.Value}");
            }
        }
        if (sitemap.Urls.Count > 5)
        {
            AnsiConsole.MarkupLine($"    [dim]... and {sitemap.Urls.Count - 5} more[/]");
        }
    }

    /// <summary>
    /// Displays information about a SitemapIndex.
    /// </summary>
    public static void ShowSitemapIndex(SitemapIndex index)
    {
        AnsiConsole.MarkupLine($"  [dim]Sitemap Index:[/] [blue]{index.Sitemaps.Count} sitemaps[/]");
        foreach (SitemapIndexEntry entry in index.Sitemaps.Take(5))
        {
            AnsiConsole.MarkupLine($"    - {Markup.Escape(entry.Location?.ToString() ?? "")}");
            if (entry.LastModified.HasValue)
            {
                AnsiConsole.MarkupLine($"      [dim]Last Modified:[/] {entry.LastModified.Value:yyyy-MM-dd HH:mm:ss}");
            }
        }
        if (index.Sitemaps.Count > 5)
        {
            AnsiConsole.MarkupLine($"    [dim]... and {index.Sitemaps.Count - 5} more[/]");
        }
    }
}