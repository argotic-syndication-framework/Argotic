using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Builders;

/// <summary>
/// Fluent builder for creating RssFeed instances for testing.
/// </summary>
public class RssFeedBuilder
{
    private readonly RssFeed feed;

    public RssFeedBuilder()
    {
        feed = new RssFeed(new Uri("http://example.com/feed"), "Test Feed")
        {
            Channel =
            {
                Description = "Test feed description"
            }
        };
    }

    public RssFeedBuilder WithTitle(string title)
    {
        feed.Channel.Title = title;
        return this;
    }

    public RssFeedBuilder WithLink(string link)
    {
        feed.Channel.Link = new Uri(link);
        return this;
    }

    public RssFeedBuilder WithDescription(string description)
    {
        feed.Channel.Description = description;
        return this;
    }

    public RssFeedBuilder WithItem(Action<RssItem> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        RssItem item = new()
        {
            Title = $"Item {feed.Channel.Items.Count + 1}",
            Link = new Uri($"http://example.com/item{feed.Channel.Items.Count + 1}")
        };
        configure(item);
        feed.Channel.Items.Add(item);
        return this;
    }

    public RssFeedBuilder WithItem(string title, string link, string? description = null)
    {
        RssItem item = new()
        {
            Title = title,
            Link = new Uri(link)
        };
        if (description != null)
            item.Description = description;
        feed.Channel.Items.Add(item);
        return this;
    }

    public RssFeedBuilder WithExtension(ISyndicationExtension extension)
    {
        feed.Extensions.Add(extension);
        return this;
    }

    public RssFeedBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        feed.Extensions.Add(new TExtension());
        return this;
    }

    public RssFeedBuilder WithChannelExtension(ISyndicationExtension extension)
    {
        feed.Channel.Extensions.Add(extension);
        return this;
    }

    public RssFeedBuilder WithChannelExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        feed.Channel.Extensions.Add(new TExtension());
        return this;
    }

    public RssFeed Build() => feed;
}

/// <summary>
/// Fluent builder for creating RssItem instances for testing.
/// </summary>
public class RssItemBuilder
{
    private readonly RssItem item;

    public RssItemBuilder()
    {
        item = new RssItem
        {
            Title = "Test Item",
            Link = new Uri("http://example.com/item")
        };
    }

    public RssItemBuilder WithTitle(string title)
    {
        item.Title = title;
        return this;
    }

    public RssItemBuilder WithLink(string link)
    {
        item.Link = new Uri(link);
        return this;
    }

    public RssItemBuilder WithDescription(string description)
    {
        item.Description = description;
        return this;
    }

    public RssItemBuilder WithAuthor(string author)
    {
        item.Author = author;
        return this;
    }

    public RssItemBuilder WithPublicationDate(DateTime publicationDate)
    {
        item.PublicationDate = publicationDate;
        return this;
    }

    public RssItemBuilder WithGuid(string guidValue, bool isPermanentLink = true)
    {
        item.Guid = new RssGuid(guidValue, isPermanentLink);
        return this;
    }

    public RssItemBuilder WithCategory(string category, string? domain = null)
    {
        RssCategory rssCategory = new() { Value = category };
        if (domain != null)
            rssCategory.Domain = domain;
        item.Categories.Add(rssCategory);
        return this;
    }

    public RssItemBuilder WithEnclosure(string url, string type, long length)
    {
        item.Enclosures.Add(new RssEnclosure(length, type, new Uri(url)));
        return this;
    }

    public RssItemBuilder WithComments(string commentsUrl)
    {
        item.Comments = new Uri(commentsUrl);
        return this;
    }

    public RssItemBuilder WithExtension(ISyndicationExtension extension)
    {
        item.Extensions.Add(extension);
        return this;
    }

    public RssItemBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        item.Extensions.Add(new TExtension());
        return this;
    }

    public RssItem Build() => item;
}

/// <summary>
/// Fluent builder for creating AtomFeed instances for testing.
/// </summary>
public class AtomFeedBuilder
{
    private readonly AtomFeed feed;

    public AtomFeedBuilder()
    {
        feed = new AtomFeed(
            new AtomId(new Uri("urn:uuid:" + Guid.NewGuid().ToString())),
            new AtomTextConstruct("Test Feed"),
            DateTime.UtcNow
        );
    }

    public AtomFeedBuilder WithId(string id)
    {
        feed.Id = new AtomId(new Uri(id));
        return this;
    }

    public AtomFeedBuilder WithId(Uri id)
    {
        feed.Id = new AtomId(id);
        return this;
    }

    public AtomFeedBuilder WithTitle(string title)
    {
        feed.Title = new AtomTextConstruct(title);
        return this;
    }

    public AtomFeedBuilder WithSubtitle(string subtitle)
    {
        feed.Subtitle = new AtomTextConstruct(subtitle);
        return this;
    }

    public AtomFeedBuilder WithUpdatedOn(DateTime updatedOn)
    {
        feed.UpdatedOn = updatedOn;
        return this;
    }

    public AtomFeedBuilder WithRights(string rights)
    {
        feed.Rights = new AtomTextConstruct(rights);
        return this;
    }

    public AtomFeedBuilder WithAuthor(string name, string? email = null, string? uri = null)
    {
        AtomPersonConstruct author = new(name);
        if (email != null)
            author.EmailAddress = email;
        if (uri != null)
            author.Uri = new Uri(uri);
        feed.Authors.Add(author);
        return this;
    }

    public AtomFeedBuilder WithContributor(string name, string? email = null, string? uri = null)
    {
        AtomPersonConstruct contributor = new(name);
        if (email != null)
            contributor.EmailAddress = email;
        if (uri != null)
            contributor.Uri = new Uri(uri);
        feed.Contributors.Add(contributor);
        return this;
    }

    public AtomFeedBuilder WithCategory(string term, string? scheme = null, string? label = null)
    {
        AtomCategory category = new(term);
        if (scheme != null)
            category.Scheme = new Uri(scheme);
        if (label != null)
            category.Label = label;
        feed.Categories.Add(category);
        return this;
    }

    public AtomFeedBuilder WithLink(string href, string? rel = null, string? type = null, string? title = null)
    {
        AtomLink link = new(new Uri(href));
        if (rel != null)
            link.Relation = rel;
        if (type != null)
            link.ContentType = type;
        if (title != null)
            link.Title = title;
        feed.Links.Add(link);
        return this;
    }

    public AtomFeedBuilder WithSelfLink(string href)
    {
        return WithLink(href, "self", "application/atom+xml");
    }

    public AtomFeedBuilder WithAlternateLink(string href, string? type = null)
    {
        return WithLink(href, "alternate", type ?? "text/html");
    }

    public AtomFeedBuilder WithEntry(Action<AtomEntryBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AtomEntryBuilder builder = new();
        configure(builder);
        feed.Entries.Add(builder.Build());
        return this;
    }

    public AtomFeedBuilder WithEntry(string title, string id, DateTime updatedOn)
    {
        AtomEntry entry = new(
            new AtomId(new Uri(id)),
            new AtomTextConstruct(title),
            updatedOn
        );
        feed.Entries.Add(entry);
        return this;
    }

    public AtomFeedBuilder WithExtension(ISyndicationExtension extension)
    {
        feed.Extensions.Add(extension);
        return this;
    }

    public AtomFeedBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        feed.Extensions.Add(new TExtension());
        return this;
    }

    public AtomFeed Build() => feed;
}

/// <summary>
/// Fluent builder for creating AtomEntry instances for testing.
/// </summary>
public class AtomEntryBuilder
{
    private readonly AtomEntry entry;

    public AtomEntryBuilder()
    {
        entry = new AtomEntry(
            new AtomId(new Uri("urn:uuid:" + Guid.NewGuid().ToString())),
            new AtomTextConstruct("Test Entry"),
            DateTime.UtcNow
        );
    }

    public AtomEntryBuilder WithId(string id)
    {
        entry.Id = new AtomId(new Uri(id));
        return this;
    }

    public AtomEntryBuilder WithId(Uri id)
    {
        entry.Id = new AtomId(id);
        return this;
    }

    public AtomEntryBuilder WithTitle(string title)
    {
        entry.Title = new AtomTextConstruct(title);
        return this;
    }

    public AtomEntryBuilder WithSummary(string summary)
    {
        entry.Summary = new AtomTextConstruct(summary);
        return this;
    }

    public AtomEntryBuilder WithContent(string content, string? type = null)
    {
        entry.Content = new AtomContent(content);
        if (type != null)
            entry.Content.ContentType = type;
        return this;
    }

    public AtomEntryBuilder WithUpdatedOn(DateTime updatedOn)
    {
        entry.UpdatedOn = updatedOn;
        return this;
    }

    public AtomEntryBuilder WithPublishedOn(DateTime publishedOn)
    {
        entry.PublishedOn = publishedOn;
        return this;
    }

    public AtomEntryBuilder WithRights(string rights)
    {
        entry.Rights = new AtomTextConstruct(rights);
        return this;
    }

    public AtomEntryBuilder WithAuthor(string name, string? email = null, string? uri = null)
    {
        AtomPersonConstruct author = new(name);
        if (email != null)
            author.EmailAddress = email;
        if (uri != null)
            author.Uri = new Uri(uri);
        entry.Authors.Add(author);
        return this;
    }

    public AtomEntryBuilder WithContributor(string name, string? email = null, string? uri = null)
    {
        AtomPersonConstruct contributor = new(name);
        if (email != null)
            contributor.EmailAddress = email;
        if (uri != null)
            contributor.Uri = new Uri(uri);
        entry.Contributors.Add(contributor);
        return this;
    }

    public AtomEntryBuilder WithCategory(string term, string? scheme = null, string? label = null)
    {
        AtomCategory category = new(term);
        if (scheme != null)
            category.Scheme = new Uri(scheme);
        if (label != null)
            category.Label = label;
        entry.Categories.Add(category);
        return this;
    }

    public AtomEntryBuilder WithLink(string href, string? rel = null, string? type = null, string? title = null)
    {
        AtomLink link = new(new Uri(href));
        if (rel != null)
            link.Relation = rel;
        if (type != null)
            link.ContentType = type;
        if (title != null)
            link.Title = title;
        entry.Links.Add(link);
        return this;
    }

    public AtomEntryBuilder WithAlternateLink(string href, string? type = null)
    {
        return WithLink(href, "alternate", type ?? "text/html");
    }

    public AtomEntryBuilder WithExtension(ISyndicationExtension extension)
    {
        entry.Extensions.Add(extension);
        return this;
    }

    public AtomEntryBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        entry.Extensions.Add(new TExtension());
        return this;
    }

    public AtomEntry Build() => entry;
}

/// <summary>
/// Fluent builder for creating OpmlDocument instances for testing.
/// </summary>
public class OpmlDocumentBuilder
{
    private readonly OpmlDocument document;

    public OpmlDocumentBuilder()
    {
        document = new OpmlDocument
        {
            Head =
            {
                Title = "Test OPML Document"
            }
        };
    }

    public OpmlDocumentBuilder WithTitle(string title)
    {
        document.Head.Title = title;
        return this;
    }

    public OpmlDocumentBuilder WithCreatedOn(DateTime createdOn)
    {
        document.Head.CreatedOn = createdOn;
        return this;
    }

    public OpmlDocumentBuilder WithModifiedOn(DateTime modifiedOn)
    {
        document.Head.ModifiedOn = modifiedOn;
        return this;
    }

    public OpmlDocumentBuilder WithOwnerName(string name)
    {
        document.Head.Owner ??= new OpmlOwner();
        document.Head.Owner.Name = name;
        return this;
    }

    public OpmlDocumentBuilder WithOwnerEmail(string email)
    {
        document.Head.Owner ??= new OpmlOwner();
        document.Head.Owner.EmailAddress = email;
        return this;
    }

    public OpmlDocumentBuilder WithHead(Action<OpmlHead> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(document.Head);
        return this;
    }

    public OpmlDocumentBuilder WithOutline(string text)
    {
        document.Outlines.Add(new OpmlOutline(text));
        return this;
    }

    public OpmlDocumentBuilder WithOutline(Action<OpmlOutlineBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        OpmlOutlineBuilder builder = new();
        configure(builder);
        document.Outlines.Add(builder.Build());
        return this;
    }

    public OpmlDocumentBuilder WithSubscriptionOutline(string text, string feedUrl, string? htmlUrl = null)
    {
        OpmlOutline outline = OpmlOutline.CreateSubscriptionListOutline(text, "rss", new Uri(feedUrl));
        if (htmlUrl != null)
        {
            outline.Attributes["htmlUrl"] = htmlUrl;
        }
        document.Outlines.Add(outline);
        return this;
    }

    public OpmlDocumentBuilder WithInclusionOutline(string text, string url)
    {
        document.Outlines.Add(OpmlOutline.CreateInclusionOutline(text, new Uri(url)));
        return this;
    }

    public OpmlDocumentBuilder WithExtension(ISyndicationExtension extension)
    {
        document.Extensions.Add(extension);
        return this;
    }

    public OpmlDocumentBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        document.Extensions.Add(new TExtension());
        return this;
    }

    public OpmlDocument Build() => document;
}

/// <summary>
/// Fluent builder for creating OpmlOutline instances for testing.
/// </summary>
public class OpmlOutlineBuilder
{
    private readonly OpmlOutline outline;

    public OpmlOutlineBuilder()
    {
        outline = new OpmlOutline("Test Outline");
    }

    public OpmlOutlineBuilder(string text)
    {
        outline = new OpmlOutline(text);
    }

    public OpmlOutlineBuilder WithText(string text)
    {
        outline.Text = text;
        return this;
    }

    public OpmlOutlineBuilder WithContentType(string contentType)
    {
        outline.ContentType = contentType;
        return this;
    }

    public OpmlOutlineBuilder WithCreatedOn(DateTime createdOn)
    {
        outline.CreatedOn = createdOn;
        return this;
    }

    public OpmlOutlineBuilder IsCommented(bool isCommented = true)
    {
        outline.IsCommented = isCommented;
        return this;
    }

    public OpmlOutlineBuilder HasBreakpoint(bool hasBreakpoint = true)
    {
        outline.HasBreakpoint = hasBreakpoint;
        return this;
    }

    public OpmlOutlineBuilder WithCategory(string category)
    {
        outline.Categories.Add(category);
        return this;
    }

    public OpmlOutlineBuilder WithAttribute(string name, string value)
    {
        outline.Attributes[name] = value;
        return this;
    }

    public OpmlOutlineBuilder WithXmlUrl(string xmlUrl)
    {
        outline.Attributes["xmlUrl"] = xmlUrl;
        return this;
    }

    public OpmlOutlineBuilder WithHtmlUrl(string htmlUrl)
    {
        outline.Attributes["htmlUrl"] = htmlUrl;
        return this;
    }

    public OpmlOutlineBuilder WithChildOutline(string text)
    {
        outline.Outlines.Add(new OpmlOutline(text));
        return this;
    }

    public OpmlOutlineBuilder WithChildOutline(Action<OpmlOutlineBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        OpmlOutlineBuilder builder = new();
        configure(builder);
        outline.Outlines.Add(builder.Build());
        return this;
    }

    public OpmlOutlineBuilder AsRssOutline(string feedUrl)
    {
        outline.ContentType = "rss";
        outline.Attributes["xmlUrl"] = feedUrl;
        return this;
    }

    public OpmlOutlineBuilder AsIncludeOutline(string includeUrl)
    {
        outline.ContentType = "include";
        outline.Attributes["url"] = includeUrl;
        return this;
    }

    public OpmlOutlineBuilder AsLinkOutline(string linkUrl)
    {
        outline.ContentType = "link";
        outline.Attributes["url"] = linkUrl;
        return this;
    }

    public OpmlOutlineBuilder WithExtension(ISyndicationExtension extension)
    {
        outline.Extensions.Add(extension);
        return this;
    }

    public OpmlOutlineBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        outline.Extensions.Add(new TExtension());
        return this;
    }

    public OpmlOutline Build() => outline;
}