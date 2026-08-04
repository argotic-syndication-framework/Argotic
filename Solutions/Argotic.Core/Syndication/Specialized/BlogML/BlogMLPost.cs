using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents information that describes a web log entry.
/// </summary>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the BlogMLPost class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLPostExample.cs" 
///             region="BlogMLPost" 
///         />
///     </code>
/// </example>
[Serializable]
public class BlogMLPost : IBlogMLCommonObject, IComparable<BlogMLPost>, IEquatable<BlogMLPost>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{

    /// <summary>
    /// Private member to hold the title of the web log entity.
    /// </summary>
    private BlogMLTextConstruct commonObjectBaseTitle = new();

    /// <summary>
    /// Private member to hold the textual content of the post.
    /// </summary>
    private BlogMLTextConstruct postContent = new();

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
    ///     An <see cref="BlogMLApprovalStatus"/> enumeration value that represents whether this web log entity was approved to be publicly available.
    ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified.
    /// </value>
    public BlogMLApprovalStatus ApprovalStatus { get; set; } = BlogMLApprovalStatus.None;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was created.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates an instant in time associated with an event early in the life cycle of this web log entity.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the unique identifier of this web log entity.
    /// </summary>
    /// <value>An identification string for this web log entity. The default value is an <b>empty</b> string, which indicated that no identifier was specified.</value>
    public string Id
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was last modified.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this web log entity was modified in a way the publisher considers significant.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime LastModifiedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the title of this web log entity.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> object that represents the title of this web log entity.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public BlogMLTextConstruct Title
    {
        get => commonObjectBaseTitle;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            commonObjectBaseTitle = value;
        }
    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the attachments for this post.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="BlogMLAttachment"/> objects that represent the attachments for this post.</value>
    public IList<BlogMLAttachment> Attachments { get; } = [];

    /// <summary>
    /// Gets the authors of this post.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of strings that represent references to the authors of this post.</value>
    /// <remarks>
    ///     The authors referenced by this collection <i>should</i> be located in the post's parent document <see cref="BlogMLDocument.Authors"/> collection.
    /// </remarks>
    public IList<string> Authors { get; } = [];

    /// <summary>
    /// Gets the categories for this post.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of strings that represent references to the categories for this post.</value>
    /// <remarks>
    ///     The categories referenced by this collection <i>should</i> be located in the post's parent document <see cref="BlogMLDocument.Categories"/> collection.
    /// </remarks>
    public IList<string> Categories { get; } = [];

    /// <summary>
    /// Gets the comments for this post.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="BlogMLComment"/> objects that represent the comments for this post.</value>
    public IList<BlogMLComment> Comments { get; } = [];

    /// <summary>
    /// Gets or sets the content of this post.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> that represents the content of this post.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public BlogMLTextConstruct Content
    {
        get => postContent;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            postContent = value;
        }
    }

    /// <summary>
    /// Gets or sets the excerpt of this post.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> that represents an excerpt of this post.</value>
    public BlogMLTextConstruct Excerpt { get; set; }

    /// <summary>
    /// Gets a value indicating if this post has an excerpt.
    /// </summary>
    /// <value><b>true</b> if this post's <see cref="Excerpt"/> is not null; Otherwise, <b>false</b>.</value>
    public bool HasExcerpt => this.Excerpt != null;

    /// <summary>
    /// Gets or sets the name of this post.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> that represents the name of this post.</value>
    public BlogMLTextConstruct Name { get; set; }

    /// <summary>
    /// Gets or sets the type of web log entry this post represents.
    /// </summary>
    /// <value>
    ///     An <see cref="BlogMLPostType"/> enumeration value that represents the type of web log entry this post represents.
    ///     The default value is <see cref="BlogMLPostType.None"/>.
    /// </value>
    public BlogMLPostType PostType { get; set; } = BlogMLPostType.None;

    /// <summary>
    /// Gets the trackbacks for this post.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="BlogMLTrackback"/> objects that represent the trackbacks for this post.</value>
    public IList<BlogMLTrackback> Trackbacks { get; } = [];

    /// <summary>
    /// Gets or sets the URL of this post.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of this post.</value>
    public Uri Url { get; set; }

    /// <summary>
    /// Gets or sets the views of this post.
    /// </summary>
    /// <value>The views of this post.</value>
    public string Views
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Returns the post type identifier for the supplied <see cref="BlogMLPostType"/>.
    /// </summary>
    /// <param name="type">The <see cref="BlogMLPostType"/> to get the post type identifier for.</param>
    /// <returns>The post type identifier for the supplied <paramref name="type"/>, Otherwise, returns an empty string.</returns>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the PostTypeAsString method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLPostExample.cs"
    ///             region="PostTypeAsString(BlogMLPostType type)"
    ///         />
    ///     </code>
    /// </example>
    public static string PostTypeAsString(BlogMLPostType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="BlogMLPostType"/> enumeration value that corresponds to the specified post type name.
    /// </summary>
    /// <param name="name">The name of the post type.</param>
    /// <returns>A <see cref="BlogMLPostType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>BlogMLPostType.None</b>.</returns>
    /// <remarks>This method disregards case of specified post type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the PostTypeByName method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLPostExample.cs"
    ///             region="PostTypeByName(string name)"
    ///         />
    ///     </code>
    /// </example>
    public static BlogMLPostType PostTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, BlogMLPostType.None);

    /// <summary>
    /// Loads this <see cref="BlogMLPost"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLPost"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
            XPathNavigator? contentNavigator = source.SelectSingleNode("blog:content", manager);
            XPathNavigator? postNameNavigator = source.SelectSingleNode("blog:post-name", manager);
            XPathNavigator? excerptNavigator = source.SelectSingleNode("blog:excerpt", manager);

            if (contentNavigator != null)
            {
                BlogMLTextConstruct content = new();
                if (content.Load(contentNavigator))
                {
                    this.Content = content;
                    wasLoaded = true;
                }
            }

            if (postNameNavigator != null)
            {
                BlogMLTextConstruct name = new();
                if (name.Load(postNameNavigator))
                {
                    this.Name = name;
                    wasLoaded = true;
                }
            }

            if (excerptNavigator != null)
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
    /// Loads this <see cref="ApmlApplication"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
            XPathNavigator? contentNavigator = source.SelectSingleNode("blog:content", manager);
            XPathNavigator? postNameNavigator = source.SelectSingleNode("blog:post-name", manager);
            XPathNavigator? excerptNavigator = source.SelectSingleNode("blog:excerpt", manager);

            if (contentNavigator != null)
            {
                BlogMLTextConstruct content = new();
                if (content.Load(contentNavigator, settings))
                {
                    this.Content = content;
                    wasLoaded = true;
                }
            }

            if (postNameNavigator != null)
            {
                BlogMLTextConstruct name = new();
                if (name.Load(postNameNavigator, settings))
                {
                    this.Name = name;
                    wasLoaded = true;
                }
            }

            if (excerptNavigator != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("post", BlogMLUtility.BlogMLNamespace);
        BlogMLUtility.WriteCommonObjectAttributes(this, writer);

        if (this.Url != null)
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
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="post"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
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
                if (categoriesNode == null)
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
                if (commentsNode == null)
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
                if (trackbacksNode == null)
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
                if (attachmentsNode == null)
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
                if (authorsNode == null)
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
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLPost"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="post"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
                if (categoriesNode == null)
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
                if (commentsNode == null)
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
                if (trackbacksNode == null)
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
                if (attachmentsNode == null)
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
                if (authorsNode == null)
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

        if (this.Excerpt != null)
        {
            if (result == 0) result = this.Excerpt.CompareTo(other.Excerpt);
        }
        else if (other.Excerpt != null)
        {
            if (result == 0) result = -1;
        }

        if (this.Name != null)
        {
            if (result == 0) result = this.Name.CompareTo(other.Name);
        }
        else if (other.Name != null)
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
    /// <returns><b>true</b> if the specified <see cref="BlogMLPost"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is BlogMLPost other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.PostType), HashCodeUtility.Component(this.Url), HashCodeUtility.Component(this.Views), HashCodeUtility.Component(this.ApprovalStatus), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.LastModifiedOn));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(BlogMLPost? first, BlogMLPost? second)
    {
        return !(first == second);
    }

}