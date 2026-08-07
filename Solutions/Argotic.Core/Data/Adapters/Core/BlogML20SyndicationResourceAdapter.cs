using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="BlogMLDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///     BlogML is a blog <i>export</i> format, not a syndication feed: a single <c>blog</c> root in
///     <c>http://www.blogml.com/2006/09/BlogML</c> carrying the whole site — every author, category and
///     post, and inside each post its comments, trackbacks and attachments — rather than a recent window of
///     it. The consequence shows up here as scale, which is why
///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> caps authors and categories as well as
///     posts, each against its own budget. Extended properties are a dictionary rather than a collection of
///     entities and are still read in full.
///     </para>
///     <para>
///     Element names use hyphens where the rest of the library uses camel case — <c>sub-title</c>,
///     <c>date-created</c>, <c>root-url</c>, <c>extended-properties</c> — so a selector copied from another
///     adapter and adjusted by eye will silently match nothing.
///     </para>
///     <para>
///     Dates are XML Schema <c>dateTime</c>, and are read with the RFC 3339 parser, whose accepted grammar
///     is close enough for the values real exporters emit. The RSS adapters use the RFC 822 parser instead;
///     the two are not interchangeable.
///     </para>
/// </remarks>
public sealed class BlogML20SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogML20SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="BlogMLDocument"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="BlogMLDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public BlogML20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Reads the <c>blog</c> root: its <c>date-created</c> and <c>root-url</c> attributes, its title and sub-title, its four collections, and its syndication extensions.
    /// </summary>
    /// <param name="resource">The <see cref="BlogMLDocument"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(BlogMLDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? blogNavigator = this.Navigator.SelectChildElement("blog", "blog", manager);
        if (blogNavigator is not null)
        {
            if (blogNavigator.HasAttributes)
            {
                string dateCreatedAttribute = blogNavigator.GetAttribute("date-created", string.Empty);
                string rootUrlAttribute = blogNavigator.GetAttribute("root-url", string.Empty);

                if (!string.IsNullOrEmpty(dateCreatedAttribute))
                {
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedAttribute, out DateTime createdOn))
                    {
                        resource.GeneratedOn = createdOn;
                    }
                }

                if (!string.IsNullOrEmpty(rootUrlAttribute))
                {
                    if (Uri.TryCreate(rootUrlAttribute, UriKind.RelativeOrAbsolute, out Uri? rootUrl))
                    {
                        resource.RootUrl = rootUrl;
                    }
                }
            }

            if (blogNavigator.HasChildren)
            {
                XPathNavigator? titleNavigator = blogNavigator.SelectChildElement("blog", "title", manager);
                XPathNavigator? subtitleNavigator = blogNavigator.SelectChildElement("blog", "sub-title", manager);

                if (titleNavigator is not null)
                {
                    BlogMLTextConstruct title = new();
                    if (title.Load(titleNavigator))
                    {
                        resource.Title = title;
                    }
                }

                if (subtitleNavigator is not null)
                {
                    BlogMLTextConstruct subtitle = new();
                    if (subtitle.Load(subtitleNavigator))
                    {
                        resource.Subtitle = subtitle;
                    }
                }

                BlogML20SyndicationResourceAdapter.FillDocumentCollections(resource, blogNavigator, manager, this.Settings);
            }

            SyndicationExtensionAdapter adapter = new(blogNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Reads the four collections under <c>blog</c> — <c>authors/author</c>, <c>extended-properties/property</c>, <c>categories/category</c> and <c>posts/post</c> — recursing into each post's comments, trackbacks and attachments.
    /// </summary>
    /// <param name="document">The <see cref="BlogMLDocument"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLDocument"/>.
    ///     </para>
    ///     <para>
    ///     Extended properties are name/value attribute pairs, and the first spelling of a name wins: a
    ///     repeated <c>name</c> is dropped rather than overwriting, because the dictionary is guarded with
    ///     <c>ContainsKey</c> instead of an indexer assignment. A property with a name and no value is kept,
    ///     with an empty string for the value.
    ///     </para>
    ///     <para>
    ///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> is tested at the top of each of the
    ///     three entity loops, against the number of entities that loop has <i>kept</i>. A post that fails to
    ///     load therefore costs nothing against the budget, and the post that would trip it is never parsed —
    ///     which on this format means never walking its comments, trackbacks and attachments either.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillDocumentCollections(BlogMLDocument document, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorsIterator = source.Select("blog:authors/blog:author", manager);
        XPathNodeIterator extendedPropertiesIterator = source.Select("blog:extended-properties/blog:property", manager);
        XPathNodeIterator categoriesIterator = source.Select("blog:categories/blog:category", manager);
        XPathNodeIterator postsIterator = source.Select("blog:posts/blog:post", manager);

        if (authorsIterator is { Count: > 0 })
        {
            int addedAuthors = 0;
            while (authorsIterator.MoveNext())
            {
                if (settings.RetrievalLimit != 0 && addedAuthors >= settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? authorsNode = authorsIterator.Current;
                if (authorsNode is null)
                {
                    continue;
                }

                BlogMLAuthor author = new();
                if (author.Load(authorsNode, settings))
                {
                    document.Authors.Add(author);
                    addedAuthors++;
                }
            }
        }

        if (extendedPropertiesIterator is { Count: > 0 })
        {
            while (extendedPropertiesIterator.MoveNext())
            {
                XPathNavigator? extendedPropertiesNode = extendedPropertiesIterator.Current;
                if (extendedPropertiesNode is null)
                {
                    continue;
                }

                if (extendedPropertiesNode.HasAttributes)
                {
                    string propertyName = extendedPropertiesNode.GetAttribute("name", string.Empty);
                    string propertyValue = extendedPropertiesNode.GetAttribute("value", string.Empty);

                    if (!string.IsNullOrEmpty(propertyName) && !document.ExtendedProperties.ContainsKey(propertyName))
                    {
                        document.ExtendedProperties.Add(propertyName, propertyValue);
                    }
                }
            }
        }

        if (categoriesIterator is { Count: > 0 })
        {
            int addedCategories = 0;
            while (categoriesIterator.MoveNext())
            {
                if (settings.RetrievalLimit != 0 && addedCategories >= settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? categoriesNode = categoriesIterator.Current;
                if (categoriesNode is null)
                {
                    continue;
                }

                BlogMLCategory category = new();
                if (category.Load(categoriesNode, settings))
                {
                    document.Categories.Add(category);
                    addedCategories++;
                }
            }
        }

        if (postsIterator is { Count: > 0 })
        {
            int addedPosts = 0;
            while (postsIterator.MoveNext())
            {
                if (settings.RetrievalLimit != 0 && addedPosts >= settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? postsNode = postsIterator.Current;
                if (postsNode is null)
                {
                    continue;
                }

                BlogMLPost post = new();
                if (post.Load(postsNode, settings))
                {
                    document.Posts.Add(post);
                    addedPosts++;
                }
            }
        }
    }
}