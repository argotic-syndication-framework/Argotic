using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Builders;

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
        if (domain is not null)
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
        if (type is not null)
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
        if (email is not null)
            author.EmailAddress = email;
        if (uri is not null)
            author.Uri = new Uri(uri);
        entry.Authors.Add(author);
        return this;
    }

    public AtomEntryBuilder WithContributor(string name, string? email = null, string? uri = null)
    {
        AtomPersonConstruct contributor = new(name);
        if (email is not null)
            contributor.EmailAddress = email;
        if (uri is not null)
            contributor.Uri = new Uri(uri);
        entry.Contributors.Add(contributor);
        return this;
    }

    public AtomEntryBuilder WithCategory(string term, string? scheme = null, string? label = null)
    {
        AtomCategory category = new(term);
        if (scheme is not null)
            category.Scheme = new Uri(scheme);
        if (label is not null)
            category.Label = label;
        entry.Categories.Add(category);
        return this;
    }

    public AtomEntryBuilder WithLink(string href, string? rel = null, string? type = null, string? title = null)
    {
        AtomLink link = new(new Uri(href));
        if (rel is not null)
            link.Relation = rel;
        if (type is not null)
            link.ContentType = type;
        if (title is not null)
            link.Title = title;
        entry.Links.Add(link);
        return this;
    }

    public AtomEntryBuilder WithAlternateLink(string href, string? type = null) => WithLink(href, "alternate", type ?? "text/html");

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