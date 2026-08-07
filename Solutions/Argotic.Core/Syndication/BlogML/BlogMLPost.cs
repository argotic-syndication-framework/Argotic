using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents information that describes a web log entry.
/// </summary>
/// <remarks>
///     One post and everything hanging off it — its body, comments, trackbacks and attachments — which is
///     what makes a <see cref="BlogMLDocument"/> a complete backup rather than a feed. Its authors and
///     categories, by contrast, are stored as identifier references into the document's tables rather than
///     inline.
/// </remarks>
/// <seealso cref="BlogMLDocument.Posts"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLPostExample.cs" language="cs" title="The following code example demonstrates the usage of the BlogMLPost class." />
/// </example>
public class BlogMLPost : IBlogMLCommonObject, IComparable<BlogMLPost>, IEquatable<BlogMLPost>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLPost"/> class.
    /// </summary>
    public BlogMLPost()
    {
    }

    /// <summary>
    /// Gets or sets the approval status of this web log entity.
    /// </summary>
    /// <value>
    ///     The <c>approved</c> attribute, read and written as <c>true</c> or <c>false</c>.
    ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified, and suppresses the attribute on save.
    /// </value>
    public BlogMLApprovalStatus ApprovalStatus { get; set; } = BlogMLApprovalStatus.None;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was created.
    /// </summary>
    /// <value>
    ///     The <c>date-created</c> attribute.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; BlogML dates are written as RFC 3339. A value that is not
    ///     RFC 3339 is retried under the invariant culture, which is how exports from engines that emitted
    ///     ordinary .NET date strings still load. That retry does not adjust to universal time, so a
    ///     non-conforming value carrying an offset comes back converted to the reading machine's local time —
    ///     invisible on a UTC host, wrong everywhere else.
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the unique identifier of this web log entity.
    /// </summary>
    /// <value>An identification string for this web log entity, or an <i>empty</i> string if none was specified.</value>
    public string Id
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was last modified.
    /// </summary>
    /// <value>
    ///     The <c>date-modified</c> attribute — the last change the publisher considered significant.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; the parsing caveat on <c>date-created</c> applies here too.
    /// </remarks>
    public DateTime LastModifiedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the title of this web log entity.
    /// </summary>
    /// <value>The <c>title</c> element. Never <see langword="null"/> — a new instance starts with an empty text construct, and the setter rejects null.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public BlogMLTextConstruct Title
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the attachments for this post.
    /// </summary>
    /// <remarks>
    ///     Files that belong to the post — images, media — either referenced by URL or carried inline as
    ///     base-64. Inline attachments are what make a BlogML export self-contained, and what make it large.
    /// </remarks>
    public IList<BlogMLAttachment> Attachments { get; } = [];

    /// <summary>
    /// Gets the authors of this post.
    /// </summary>
    /// <value><see cref="BlogMLAuthor.Id"/> strings, not author objects.</value>
    /// <remarks>
    ///     These are references into <see cref="BlogMLDocument.Authors"/>, resolved by the caller. Nothing
    ///     checks that a referenced author exists in the parent document, and a post can be moved between
    ///     documents leaving the references dangling.
    /// </remarks>
    public IList<string> Authors { get; } = [];

    /// <summary>
    /// Gets the categories for this post.
    /// </summary>
    /// <value><see cref="BlogMLCategory.Id"/> strings, not category objects.</value>
    /// <remarks>
    ///     References into <see cref="BlogMLDocument.Categories"/>, on the same terms as
    ///     <see cref="Authors"/> and with the same absence of checking.
    /// </remarks>
    public IList<string> Categories { get; } = [];

    /// <summary>
    /// Gets the comments for this post.
    /// </summary>
    /// <remarks>
    ///     Comments are stored in full inside the post, not referenced. A comment carries its own
    ///     <see cref="BlogMLComment.ApprovalStatus"/>, so an export includes the moderation queue as well as
    ///     what was published.
    /// </remarks>
    public IList<BlogMLComment> Comments { get; } = [];

    /// <summary>
    /// Gets or sets the content of this post.
    /// </summary>
    /// <value>The <c>content</c> element — the post body. Never <see langword="null"/> — a new post starts with an empty text construct, and the setter rejects null.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public BlogMLTextConstruct Content
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets or sets the excerpt of this post.
    /// </summary>
    /// <value>The <c>excerpt</c> element, or <see langword="null"/> if the post has none.</value>
    public BlogMLTextConstruct? Excerpt { get; set; }

    /// <summary>
    /// Gets a value indicating if this post has an excerpt.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Excerpt"/> is not <see langword="null"/>; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     This is what the post's <c>hasexcerpt</c> attribute is written from, and it is written on every
    ///     save, whether or not the post has one.
    /// </remarks>
    public bool HasExcerpt => this.Excerpt is not null;

    /// <summary>
    /// Gets or sets the name of this post.
    /// </summary>
    /// <value>The <c>post-name</c> element — the URL slug, as against the display <c>title</c> — or <see langword="null"/> if none was specified.</value>
    public BlogMLTextConstruct? Name { get; set; }

    /// <summary>
    /// Gets or sets the type of web log entry this post represents.
    /// </summary>
    /// <value>
    ///     The <c>type</c> attribute, distinguishing a standing page (<see cref="BlogMLPostType.Article"/>)
    ///     from a dated entry (<see cref="BlogMLPostType.Normal"/>).
    ///     The default value is <see cref="BlogMLPostType.None"/>, which suppresses the attribute on save.
    /// </value>
    public BlogMLPostType PostType { get; set; } = BlogMLPostType.None;

    /// <summary>
    /// Gets the trackbacks for this post.
    /// </summary>
    /// <remarks>
    ///     Inbound links recorded by the trackback protocol, stored in full inside the post. Like comments,
    ///     each carries its own approval status.
    /// </remarks>
    public IList<BlogMLTrackback> Trackbacks { get; } = [];

    /// <summary>
    /// Gets or sets the URL of this post.
    /// </summary>
    /// <value>The <c>post-url</c> attribute, which may be relative to <see cref="BlogMLDocument.RootUrl"/>, or <see langword="null"/> if none was specified.</value>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the views of this post.
    /// </summary>
    /// <value>The <c>views</c> attribute — a view count — or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     A string rather than a number, because the attribute is not constrained to one and engines have put
    ///     other things in it. Parse it yourself, and be ready for it not to parse.
    /// </remarks>
    public string Views
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Returns the post type identifier for the supplied <see cref="BlogMLPostType"/>.
    /// </summary>
    /// <param name="type">The <see cref="BlogMLPostType"/> to get the post type identifier for.</param>
    /// <returns>The identifier written to the <c>type</c> attribute, such as <c>article</c>; an <i>empty</i> string for <see cref="BlogMLPostType.None"/> or an undefined value.</returns>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLPostExample.cs" language="cs" title="The following code example demonstrates the usage of the PostTypeAsString method." />
    /// </example>
    public static string PostTypeAsString(BlogMLPostType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="BlogMLPostType"/> enumeration value that corresponds to the specified post type name.
    /// </summary>
    /// <param name="name">The name of the post type, as it appears in the <c>type</c> attribute.</param>
    /// <returns>The matching <see cref="BlogMLPostType"/>, or <see cref="BlogMLPostType.None"/> if <paramref name="name"/> matches nothing.</returns>
    /// <remarks>The comparison disregards case. An unrecognised name is not an error and is not preserved: it becomes <see cref="BlogMLPostType.None"/> and is dropped on save.</remarks>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLPostExample.cs" language="cs" title="The following code example demonstrates the usage of the PostTypeByName method." />
    /// </example>
    public static BlogMLPostType PostTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, BlogMLPostType.None);

    /// <summary>
    /// Loads this <see cref="BlogMLPost"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="BlogMLPost"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);
        if (BlogMLUtility.FillCommonObject(this, source))
        {
            wasLoaded = true;
        }
        if (source.HasAttributes)
        {
            string postUrlAttribute = source.GetAttribute("post-url", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string viewsAttribute = source.GetAttribute("views", string.Empty);

            if (!string.IsNullOrEmpty(postUrlAttribute))
            {
                if (Uri.TryCreate(postUrlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                BlogMLPostType type = BlogMLPost.PostTypeByName(typeAttribute);
                if (type != BlogMLPostType.None)
                {
                    this.PostType = type;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(viewsAttribute))
            {
                this.Views = viewsAttribute;
                wasLoaded = true;
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? contentNavigator = source.SelectChildElement("blog", "content", manager);
            XPathNavigator? postNameNavigator = source.SelectChildElement("blog", "post-name", manager);
            XPathNavigator? excerptNavigator = source.SelectChildElement("blog", "excerpt", manager);

            if (contentNavigator is not null)
            {
                BlogMLTextConstruct content = new();
                if (content.Load(contentNavigator))
                {
                    this.Content = content;
                    wasLoaded = true;
                }
            }

            if (postNameNavigator is not null)
            {
                BlogMLTextConstruct name = new();
                if (name.Load(postNameNavigator))
                {
                    this.Name = name;
                    wasLoaded = true;
                }
            }

            if (excerptNavigator is not null)
            {
                BlogMLTextConstruct excerpt = new();
                if (excerpt.Load(excerptNavigator))
                {
                    this.Excerpt = excerpt;
                    wasLoaded = true;
                }
            }

            if (BlogMLPost.FillPostCollections(this, source, manager))
            {
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="BlogMLPost"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="BlogMLPost"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);
        if (BlogMLUtility.FillCommonObject(this, source, settings))
        {
            wasLoaded = true;
        }
        if (source.HasAttributes)
        {
            string postUrlAttribute = source.GetAttribute("post-url", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string viewsAttribute = source.GetAttribute("views", string.Empty);

            if (!string.IsNullOrEmpty(postUrlAttribute))
            {
                if (Uri.TryCreate(postUrlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                BlogMLPostType type = BlogMLPost.PostTypeByName(typeAttribute);
                if (type != BlogMLPostType.None)
                {
                    this.PostType = type;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(viewsAttribute))
            {
                this.Views = viewsAttribute;
                wasLoaded = true;
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? contentNavigator = source.SelectChildElement("blog", "content", manager);
            XPathNavigator? postNameNavigator = source.SelectChildElement("blog", "post-name", manager);
            XPathNavigator? excerptNavigator = source.SelectChildElement("blog", "excerpt", manager);

            if (contentNavigator is not null)
            {
                BlogMLTextConstruct content = new();
                if (content.Load(contentNavigator, settings))
                {
                    this.Content = content;
                    wasLoaded = true;
                }
            }

            if (postNameNavigator is not null)
            {
                BlogMLTextConstruct name = new();
                if (name.Load(postNameNavigator, settings))
                {
                    this.Name = name;
                    wasLoaded = true;
                }
            }

            if (excerptNavigator is not null)
            {
                BlogMLTextConstruct excerpt = new();
                if (excerpt.Load(excerptNavigator, settings))
                {
                    this.Excerpt = excerpt;
                    wasLoaded = true;
                }
            }

            if (BlogMLPost.FillPostCollections(this, source, manager, settings))
            {
                wasLoaded = true;
            }
        }
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="BlogMLPost"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("post", BlogMLUtility.BlogMLNamespace);
        BlogMLUtility.WriteCommonObjectAttributes(this, writer);

        if (this.Url is not null)
        {
            writer.WriteAttributeString("post-url", this.Url.ToString());
        }

        if (this.PostType != BlogMLPostType.None)
        {
            writer.WriteAttributeString("type", BlogMLPost.PostTypeAsString(this.PostType));
        }

        writer.WriteAttributeString("hasexcerpt", this.HasExcerpt ? "true" : "false");

        if (!string.IsNullOrEmpty(this.Views))
        {
            writer.WriteAttributeString("views", this.Views);
        }

        BlogMLUtility.WriteCommonObjectElements(this, writer);

        this.Content.WriteTo(writer, "content");

        this.Name?.WriteTo(writer, "post-name");

        this.Excerpt?.WriteTo(writer, "excerpt");

        if (this.Categories.Count > 0)
        {
            writer.WriteStartElement("categories", BlogMLUtility.BlogMLNamespace);
            foreach (string category in this.Categories)
            {
                writer.WriteStartElement("category", BlogMLUtility.BlogMLNamespace);
                writer.WriteAttributeString("ref", category);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        if (this.Comments.Count > 0)
        {
            writer.WriteStartElement("comments", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLComment comment in this.Comments)
            {
                comment.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (this.Trackbacks.Count > 0)
        {
            writer.WriteStartElement("trackbacks", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLTrackback trackback in this.Trackbacks)
            {
                trackback.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (this.Attachments.Count > 0)
        {
            writer.WriteStartElement("attachments", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLAttachment attachment in this.Attachments)
            {
                attachment.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (this.Authors.Count > 0)
        {
            writer.WriteStartElement("authors", BlogMLUtility.BlogMLNamespace);
            foreach (string author in this.Authors)
            {
                writer.WriteStartElement("author", BlogMLUtility.BlogMLNamespace);
                writer.WriteAttributeString("ref", author);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Modifies the <see cref="BlogMLPost"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="post">The <see cref="BlogMLPost"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <returns><see langword="true"/> if at least one category, comment, trackback, attachment or author reference was read; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="post"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    private static bool FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(post);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        XPathNodeIterator categoriesIterator = source.Select("blog:categories/blog:category", manager);
        XPathNodeIterator commentsIterator = source.Select("blog:comments/blog:comment", manager);
        XPathNodeIterator trackbacksIterator = source.Select("blog:trackbacks/blog:trackback", manager);
        XPathNodeIterator attachmentsIterator = source.Select("blog:attachments/blog:attachment", manager);
        XPathNodeIterator authorsIterator = source.Select("blog:authors/blog:author", manager);

        if (categoriesIterator is { Count: > 0 })
        {
            while (categoriesIterator.MoveNext())
            {
                XPathNavigator? categoriesNode = categoriesIterator.Current;
                if (categoriesNode is null)
                {
                    continue;
                }

                string referenceId = categoriesNode.GetAttribute("ref", string.Empty);
                if (!string.IsNullOrEmpty(referenceId))
                {
                    post.Categories.Add(referenceId);
                    wasLoaded = true;
                }
            }
        }

        if (commentsIterator is { Count: > 0 })
        {
            while (commentsIterator.MoveNext())
            {
                XPathNavigator? commentsNode = commentsIterator.Current;
                if (commentsNode is null)
                {
                    continue;
                }

                BlogMLComment comment = new();
                if (comment.Load(commentsNode))
                {
                    post.Comments.Add(comment);
                    wasLoaded = true;
                }
            }
        }

        if (trackbacksIterator is { Count: > 0 })
        {
            while (trackbacksIterator.MoveNext())
            {
                XPathNavigator? trackbacksNode = trackbacksIterator.Current;
                if (trackbacksNode is null)
                {
                    continue;
                }

                BlogMLTrackback trackback = new();
                if (trackback.Load(trackbacksNode))
                {
                    post.Trackbacks.Add(trackback);
                    wasLoaded = true;
                }
            }
        }

        if (attachmentsIterator is { Count: > 0 })
        {
            while (attachmentsIterator.MoveNext())
            {
                XPathNavigator? attachmentsNode = attachmentsIterator.Current;
                if (attachmentsNode is null)
                {
                    continue;
                }

                BlogMLAttachment attachment = new();
                if (attachment.Load(attachmentsNode))
                {
                    post.Attachments.Add(attachment);
                    wasLoaded = true;
                }
            }
        }

        if (authorsIterator is { Count: > 0 })
        {
            while (authorsIterator.MoveNext())
            {
                XPathNavigator? authorsNode = authorsIterator.Current;
                if (authorsNode is null)
                {
                    continue;
                }

                string referenceId = authorsNode.GetAttribute("ref", string.Empty);
                if (!string.IsNullOrEmpty(referenceId))
                {
                    post.Authors.Add(referenceId);
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Modifies the <see cref="BlogMLPost"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="post">The <see cref="BlogMLPost"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if at least one category, comment, trackback, attachment or author reference was read; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="post"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static bool FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(post);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);
        XPathNodeIterator categoriesIterator = source.Select("blog:categories/blog:category", manager);
        XPathNodeIterator commentsIterator = source.Select("blog:comments/blog:comment", manager);
        XPathNodeIterator trackbacksIterator = source.Select("blog:trackbacks/blog:trackback", manager);
        XPathNodeIterator attachmentsIterator = source.Select("blog:attachments/blog:attachment", manager);
        XPathNodeIterator authorsIterator = source.Select("blog:authors/blog:author", manager);

        if (categoriesIterator is { Count: > 0 })
        {
            while (categoriesIterator.MoveNext())
            {
                XPathNavigator? categoriesNode = categoriesIterator.Current;
                if (categoriesNode is null)
                {
                    continue;
                }

                string referenceId = categoriesNode.GetAttribute("ref", string.Empty);
                if (!string.IsNullOrEmpty(referenceId))
                {
                    post.Categories.Add(referenceId);
                    wasLoaded = true;
                }
            }
        }

        if (commentsIterator is { Count: > 0 })
        {
            while (commentsIterator.MoveNext())
            {
                XPathNavigator? commentsNode = commentsIterator.Current;
                if (commentsNode is null)
                {
                    continue;
                }

                BlogMLComment comment = new();
                if (comment.Load(commentsNode, settings))
                {
                    post.Comments.Add(comment);
                    wasLoaded = true;
                }
            }
        }

        if (trackbacksIterator is { Count: > 0 })
        {
            while (trackbacksIterator.MoveNext())
            {
                XPathNavigator? trackbacksNode = trackbacksIterator.Current;
                if (trackbacksNode is null)
                {
                    continue;
                }

                BlogMLTrackback trackback = new();
                if (trackback.Load(trackbacksNode, settings))
                {
                    post.Trackbacks.Add(trackback);
                    wasLoaded = true;
                }
            }
        }

        if (attachmentsIterator is { Count: > 0 })
        {
            while (attachmentsIterator.MoveNext())
            {
                XPathNavigator? attachmentsNode = attachmentsIterator.Current;
                if (attachmentsNode is null)
                {
                    continue;
                }

                BlogMLAttachment attachment = new();
                if (attachment.Load(attachmentsNode, settings))
                {
                    post.Attachments.Add(attachment);
                    wasLoaded = true;
                }
            }
        }

        if (authorsIterator is { Count: > 0 })
        {
            while (authorsIterator.MoveNext())
            {
                XPathNavigator? authorsNode = authorsIterator.Current;
                if (authorsNode is null)
                {
                    continue;
                }

                string referenceId = authorsNode.GetAttribute("ref", string.Empty);
                if (!string.IsNullOrEmpty(referenceId))
                {
                    post.Authors.Add(referenceId);
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="BlogMLPost"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="BlogMLPost"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(BlogMLPost? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Attachments, other.Attachments);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Authors, other.Authors, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Categories, other.Categories, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Comments, other.Comments);
        if (result == 0) result = this.Content.CompareTo(other.Content);

        if (this.Excerpt is not null)
        {
            if (result == 0) result = this.Excerpt.CompareTo(other.Excerpt);
        }
        else if (other.Excerpt is not null)
        {
            if (result == 0) result = -1;
        }

        if (this.Name is not null)
        {
            if (result == 0) result = this.Name.CompareTo(other.Name);
        }
        else if (other.Name is not null)
        {
            if (result == 0) result = -1;
        }

        if (result == 0) result = this.PostType.CompareTo(other.PostType);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Trackbacks, other.Trackbacks);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Views, other.Views, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = BlogMLUtility.CompareCommonObjects(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLPost"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLPost"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="BlogMLPost"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(BlogMLPost? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is BlogMLPost other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.PostType), HashCodeUtility.Component(this.Url), HashCodeUtility.Component(this.Views), HashCodeUtility.Component(this.ApprovalStatus), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.LastModifiedOn));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(BlogMLPost? first, BlogMLPost? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(BlogMLPost? first, BlogMLPost? second) => !(first == second);

}