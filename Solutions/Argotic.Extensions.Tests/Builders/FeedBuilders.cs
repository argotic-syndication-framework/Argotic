using Argotic.Syndication;

namespace Argotic.Extensions.Tests.Builders;

/// <summary>
/// Builds an <see cref="RssItem"/> a test can state the interesting part of and leave the rest alone.
/// </summary>
/// <remarks>
///     <para>
///     A new builder already holds a valid item — title <c>Test Item</c>, link
///     <c>http://example.com/item</c> — so <see cref="Build"/> on its own returns something an
///     <c>RssFeed</c> will accept. Each <c>With…</c> call then names one thing the test actually cares
///     about, and a reader can tell at a glance which fields are load-bearing and which are scenery.
///     </para>
///     <para>
///     Scalar setters replace; collection setters (<see cref="WithCategory"/>,
///     <see cref="WithEnclosure"/>, <see cref="WithExtension(ISyndicationExtension)"/>) append, so
///     calling one twice gives two elements.
///     </para>
///     <para>
///     Every call returns the same instance rather than a copy. Chain from a fresh builder per item;
///     do not fork one part-way and expect two independent items.
///     </para>
/// </remarks>
public class RssItemBuilder
{
    private readonly RssItem item;

    /// <summary>
    /// Initializes a new instance of the <see cref="RssItemBuilder"/> class, holding a valid item.
    /// </summary>
    public RssItemBuilder()
    {
        item = new RssItem
        {
            Title = "Test Item",
            Link = new Uri("http://example.com/item")
        };
    }

    /// <summary>
    /// Replaces the item's title.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithTitle(string title)
    {
        item.Title = title;
        return this;
    }

    /// <summary>
    /// Replaces the item's link.
    /// </summary>
    /// <param name="link">An absolute URL.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithLink(string link)
    {
        item.Link = new Uri(link);
        return this;
    }

    /// <summary>
    /// Sets the item's description.
    /// </summary>
    /// <param name="description">The description. RSS allows markup here, escaped or in a CDATA section.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithDescription(string description)
    {
        item.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the item's author.
    /// </summary>
    /// <param name="author">An email address, which is what RSS 2.0 defines this element to carry.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithAuthor(string author)
    {
        item.Author = author;
        return this;
    }

    /// <summary>
    /// Sets the item's publication date.
    /// </summary>
    /// <param name="publicationDate">
    ///     The date. Give it an explicit <see cref="DateTimeKind"/> — this devcontainer runs in UTC, so a
    ///     kind-dependent defect is invisible here if the value is left unspecified.
    /// </param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithPublicationDate(DateTime publicationDate)
    {
        item.PublicationDate = publicationDate;
        return this;
    }

    /// <summary>
    /// Sets the item's guid.
    /// </summary>
    /// <param name="guidValue">The identifier: a URL when it is a permanent link; anything opaque when it is not.</param>
    /// <param name="isPermanentLink">
    ///     <see langword="true"/> if the value is a URL that resolves. The default matches RSS 2.0, which
    ///     defines an absent <c>isPermaLink</c> as <see langword="true"/>.
    /// </param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithGuid(string guidValue, bool isPermanentLink = true)
    {
        item.Guid = new RssGuid(guidValue, isPermanentLink);
        return this;
    }

    /// <summary>
    /// Adds a category to the item.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <param name="domain">The taxonomy the name belongs to, or <see langword="null"/> to leave it unqualified.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithCategory(string category, string? domain = null)
    {
        RssCategory rssCategory = new() { Value = category };
        if (domain is not null)
            rssCategory.Domain = domain;
        item.Categories.Add(rssCategory);
        return this;
    }

    /// <summary>
    /// Adds an enclosure to the item.
    /// </summary>
    /// <param name="url">The media file's absolute URL.</param>
    /// <param name="type">The media type, such as <c>audio/mpeg</c>.</param>
    /// <param name="length">The size in bytes. Publishers write <c>0</c> when the size is not known.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithEnclosure(string url, string type, long length)
    {
        item.Enclosures.Add(new RssEnclosure(length, type, new Uri(url)));
        return this;
    }

    /// <summary>
    /// Sets the URL of the item's comment page.
    /// </summary>
    /// <param name="commentsUrl">An absolute URL.</param>
    /// <returns>The same builder.</returns>
    public RssItemBuilder WithComments(string commentsUrl)
    {
        item.Comments = new Uri(commentsUrl);
        return this;
    }

    /// <summary>
    /// Attaches an extension the caller has already populated.
    /// </summary>
    /// <param name="extension">The extension, with its context filled in.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>
    ///     Use this overload whenever the extension's <i>data</i> matters. The generic overload attaches
    ///     an empty one.
    /// </remarks>
    public RssItemBuilder WithExtension(ISyndicationExtension extension)
    {
        item.Extensions.Add(extension);
        return this;
    }

    /// <summary>
    /// Attaches a default-constructed extension of the given type.
    /// </summary>
    /// <typeparam name="TExtension">The extension type to attach.</typeparam>
    /// <returns>The same builder.</returns>
    /// <remarks>
    ///     The extension carries no data, so this is for asking whether an extension is <i>present</i> —
    ///     attachment, discovery, ordering — not what it holds.
    /// </remarks>
    public RssItemBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        item.Extensions.Add(new TExtension());
        return this;
    }

    /// <summary>
    /// Returns the item under construction.
    /// </summary>
    /// <returns>The item itself, not a copy — later <c>With…</c> calls on this builder would still reach it.</returns>
    public RssItem Build() => item;
}

/// <summary>
/// Builds an <see cref="AtomEntry"/> a test can state the interesting part of and leave the rest alone.
/// </summary>
/// <remarks>
///     <para>
///     The counterpart to <see cref="RssItemBuilder"/>, and it follows the same rules: a new builder
///     already holds a valid entry, scalar setters replace, collection setters append, and every call
///     returns the same instance.
///     </para>
///     <para>
///     <b>The default identity and timestamp are fresh each time.</b> Atom requires an <c>id</c> and an
///     <c>updated</c>, so the constructor supplies a new <see cref="Guid"/> URN and
///     <see cref="DateTime.UtcNow"/>. Neither is reproducible between runs — a test that asserts on
///     either, or that compares two entries for equality, must set them with <see cref="WithId(Uri)"/>
///     and <see cref="WithUpdatedOn"/> first.
///     </para>
/// </remarks>
public class AtomEntryBuilder
{
    private readonly AtomEntry entry;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntryBuilder"/> class, holding a valid entry
    /// with a freshly generated identifier and the current UTC time.
    /// </summary>
    public AtomEntryBuilder()
    {
        entry = new AtomEntry(
            new AtomId(new Uri("urn:uuid:" + Guid.NewGuid().ToString())),
            new AtomTextConstruct("Test Entry"),
            DateTime.UtcNow
        );
    }

    /// <summary>
    /// Replaces the entry's identifier, given as text.
    /// </summary>
    /// <param name="id">An absolute IRI. Atom identifiers are commonly <c>urn:uuid:</c> or <c>tag:</c> URIs rather than links.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithId(string id)
    {
        entry.Id = new AtomId(new Uri(id));
        return this;
    }

    /// <summary>
    /// Replaces the entry's identifier.
    /// </summary>
    /// <param name="id">An absolute URI.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithId(Uri id)
    {
        entry.Id = new AtomId(id);
        return this;
    }

    /// <summary>
    /// Replaces the entry's title.
    /// </summary>
    /// <param name="title">The title text. Written as a text construct, so it is emitted escaped.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithTitle(string title)
    {
        entry.Title = new AtomTextConstruct(title);
        return this;
    }

    /// <summary>
    /// Sets the entry's summary.
    /// </summary>
    /// <param name="summary">A short description of the entry, as distinct from its body.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithSummary(string summary)
    {
        entry.Summary = new AtomTextConstruct(summary);
        return this;
    }

    /// <summary>
    /// Sets the entry's content — the post body itself.
    /// </summary>
    /// <param name="content">The body.</param>
    /// <param name="type">
    ///     The content type: <c>text</c>, <c>html</c>, <c>xhtml</c>, or a media type. Leave it
    ///     <see langword="null"/> to accept whatever <c>AtomContent</c> defaults to; each of the three
    ///     named values takes a different branch of the load and save paths.
    /// </param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithContent(string content, string? type = null)
    {
        entry.Content = new AtomContent(content);
        if (type is not null)
            entry.Content.ContentType = type;
        return this;
    }

    /// <summary>
    /// Replaces the entry's last-updated timestamp.
    /// </summary>
    /// <param name="updatedOn">
    ///     The timestamp. Set this whenever the test asserts on a date — the default is
    ///     <see cref="DateTime.UtcNow"/> and differs on every run.
    /// </param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithUpdatedOn(DateTime updatedOn)
    {
        entry.UpdatedOn = updatedOn;
        return this;
    }

    /// <summary>
    /// Sets the entry's publication timestamp.
    /// </summary>
    /// <param name="publishedOn">
    ///     When the entry first appeared, which Atom keeps separate from <c>updated</c> and which is
    ///     optional.
    /// </param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithPublishedOn(DateTime publishedOn)
    {
        entry.PublishedOn = publishedOn;
        return this;
    }

    /// <summary>
    /// Sets the entry's rights statement.
    /// </summary>
    /// <param name="rights">The licensing or copyright text.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithRights(string rights)
    {
        entry.Rights = new AtomTextConstruct(rights);
        return this;
    }

    /// <summary>
    /// Adds an author to the entry.
    /// </summary>
    /// <param name="name">The author's name. The only part Atom requires.</param>
    /// <param name="email">The author's email address, or <see langword="null"/> to omit the element.</param>
    /// <param name="uri">The author's home page as an absolute URL, or <see langword="null"/> to omit it.</param>
    /// <returns>The same builder.</returns>
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

    /// <summary>
    /// Adds a contributor to the entry.
    /// </summary>
    /// <param name="name">The contributor's name.</param>
    /// <param name="email">The contributor's email address, or <see langword="null"/> to omit the element.</param>
    /// <param name="uri">The contributor's home page as an absolute URL, or <see langword="null"/> to omit it.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>
    ///     Structurally identical to <see cref="WithAuthor"/> — both are Atom person constructs — but a
    ///     different collection, which is what makes the pair worth exercising separately.
    /// </remarks>
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

    /// <summary>
    /// Adds a category to the entry.
    /// </summary>
    /// <param name="term">The category itself. The only part Atom requires.</param>
    /// <param name="scheme">An absolute URL identifying the taxonomy, or <see langword="null"/> to leave the term unqualified.</param>
    /// <param name="label">A human-readable name for the term, or <see langword="null"/> to omit it.</param>
    /// <returns>The same builder.</returns>
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

    /// <summary>
    /// Adds a link to the entry.
    /// </summary>
    /// <param name="href">The link target, as an absolute URL.</param>
    /// <param name="rel">
    ///     The relation — <c>alternate</c>, <c>self</c>, <c>edit</c>, <c>next</c> and so on — or
    ///     <see langword="null"/> to omit the attribute, which Atom defines as meaning <c>alternate</c>.
    /// </param>
    /// <param name="type">The target's media type, or <see langword="null"/> to omit it.</param>
    /// <param name="title">A human-readable title for the link, or <see langword="null"/> to omit it.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>
    ///     The relation is an ordinary string and is not validated against any registry, so this will
    ///     build the unmodelled relations real feeds carry as readily as the well-known ones.
    /// </remarks>
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

    /// <summary>
    /// Adds the link to the entry's own web page — the one a reader clicks.
    /// </summary>
    /// <param name="href">The page's absolute URL.</param>
    /// <param name="type">The media type, or <see langword="null"/> for <c>text/html</c>.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithAlternateLink(string href, string? type = null) => WithLink(href, "alternate", type ?? "text/html");

    /// <summary>
    /// Attaches an extension the caller has already populated.
    /// </summary>
    /// <param name="extension">The extension, with its context filled in.</param>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithExtension(ISyndicationExtension extension)
    {
        entry.Extensions.Add(extension);
        return this;
    }

    /// <summary>
    /// Attaches a default-constructed extension of the given type.
    /// </summary>
    /// <typeparam name="TExtension">The extension type to attach.</typeparam>
    /// <returns>The same builder.</returns>
    public AtomEntryBuilder WithExtension<TExtension>() where TExtension : ISyndicationExtension, new()
    {
        entry.Extensions.Add(new TExtension());
        return this;
    }

    /// <summary>
    /// Returns the entry under construction.
    /// </summary>
    /// <returns>The entry itself, not a copy.</returns>
    public AtomEntry Build() => entry;
}