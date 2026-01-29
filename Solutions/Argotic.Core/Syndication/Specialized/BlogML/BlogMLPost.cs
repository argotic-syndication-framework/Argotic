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
public class BlogMLPost : IBlogMLCommonObject, IComparable<BlogMLPost>, IEquatable<BlogMLPost>, IExtensibleSyndicationObject, IXmlWritable
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
        get
        {
            return commonObjectBaseTitle;
        }

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
        get
        {
            return postContent;
        }

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
    /// Compares two specified <see cref="IList{BlogMLAttachment}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<BlogMLAttachment> source, IList<BlogMLAttachment> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified <see cref="IList{BlogMLAuthor}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<BlogMLAuthor> source, IList<BlogMLAuthor> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified <see cref="IList{BlogMLComment}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<BlogMLComment> source, IList<BlogMLComment> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified <see cref="IList{BlogMLTrackback}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<BlogMLTrackback> source, IList<BlogMLTrackback> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

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
    public static string PostTypeAsString(BlogMLPostType type)
    {
        string name = string.Empty;
        foreach (System.Reflection.FieldInfo fieldInfo in typeof(BlogMLPostType).GetFields())
        {
            if (fieldInfo.FieldType == typeof(BlogMLPostType))
            {
                BlogMLPostType postType = (BlogMLPostType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                if (postType == type)
                {
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes is { Length: > 0 })
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        name = enumerationMetadata.AlternateValue;
                        break;
                    }
                }
            }
        }

        return name;
    }

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
    public static BlogMLPostType PostTypeByName(string name)
    {
        BlogMLPostType postType = BlogMLPostType.None;
        ArgumentException.ThrowIfNullOrEmpty(name);
        foreach (System.Reflection.FieldInfo fieldInfo in typeof(BlogMLPostType).GetFields())
        {
            if (fieldInfo.FieldType == typeof(BlogMLPostType))
            {
                BlogMLPostType type = (BlogMLPostType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                if (customAttributes is { Length: > 0 })
                {
                    EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                    if (string.Equals(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase))
                    {
                        postType = type;
                        break;
                    }
                }
            }
        }

        return postType;
    }
    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
    }
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
                if (Uri.TryCreate(postUrlAttribute, UriKind.RelativeOrAbsolute, out Uri url))
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
            XPathNavigator contentNavigator = source.SelectSingleNode("blog:content", manager);
            XPathNavigator postNameNavigator = source.SelectSingleNode("blog:post-name", manager);
            XPathNavigator excerptNavigator = source.SelectSingleNode("blog:excerpt", manager);

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
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
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
                if (Uri.TryCreate(postUrlAttribute, UriKind.RelativeOrAbsolute, out Uri url))
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
            XPathNavigator contentNavigator = source.SelectSingleNode("blog:content", manager);
            XPathNavigator postNameNavigator = source.SelectSingleNode("blog:post-name", manager);
            XPathNavigator excerptNavigator = source.SelectSingleNode("blog:excerpt", manager);

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
                string referenceId = categoriesIterator.Current.GetAttribute("ref", string.Empty);
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
                BlogMLComment comment = new();
                if (comment.Load(commentsIterator.Current))
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
                BlogMLTrackback trackback = new();
                if (trackback.Load(trackbacksIterator.Current))
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
                BlogMLAttachment attachment = new();
                if (attachment.Load(attachmentsIterator.Current))
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
                string referenceId = authorsIterator.Current.GetAttribute("ref", string.Empty);
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
    private static bool FillPostCollections(BlogMLPost post, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
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
                string referenceId = categoriesIterator.Current.GetAttribute("ref", string.Empty);
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
                BlogMLComment comment = new();
                if (comment.Load(commentsIterator.Current, settings))
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
                BlogMLTrackback trackback = new();
                if (trackback.Load(trackbacksIterator.Current, settings))
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
                BlogMLAttachment attachment = new();
                if (attachment.Load(attachmentsIterator.Current, settings))
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
                string referenceId = authorsIterator.Current.GetAttribute("ref", string.Empty);
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
    /// Returns a <see cref="String"/> that represents the current <see cref="BlogMLPost"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="BlogMLPost"/>.</returns>
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

        int result = BlogMLPost.CompareSequence(this.Attachments, other.Attachments);
        result |= ComparisonUtility.CompareSequence(this.Authors, other.Authors, StringComparison.OrdinalIgnoreCase);
        result |= ComparisonUtility.CompareSequence(this.Categories, other.Categories, StringComparison.OrdinalIgnoreCase);
        result |= BlogMLPost.CompareSequence(this.Comments, other.Comments);
        result |= this.Content.CompareTo(other.Content);

        if (this.Excerpt != null)
        {
            result |= this.Excerpt.CompareTo(other.Excerpt);
        }
        else if (other.Excerpt != null)
        {
            result |= -1;
        }

        if (this.Name != null)
        {
            result |= this.Name.CompareTo(other.Name);
        }
        else if (other.Name != null)
        {
            result |= -1;
        }

        result |= this.PostType.CompareTo(other.PostType);
        result |= BlogMLPost.CompareSequence(this.Trackbacks, other.Trackbacks);
        result |= Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Views, other.Views, StringComparison.OrdinalIgnoreCase);

        result |= BlogMLUtility.CompareCommonObjects(this, other);

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
        return HashCode.Combine(this.Content, this.PostType, this.Url, this.Views, this.ApprovalStatus, this.CreatedOn, this.Id, this.LastModifiedOn);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(BlogMLPost first, BlogMLPost second)
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
    public static bool operator !=(BlogMLPost first, BlogMLPost second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(BlogMLPost first, BlogMLPost second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(BlogMLPost first, BlogMLPost second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(BlogMLPost first, BlogMLPost second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(BlogMLPost first, BlogMLPost second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}