using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="BlogMLDocument"/>.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="BlogML20SyndicationResourceAdapter"/> serves as a bridge between a <see cref="BlogMLDocument"/> and an XML data source.
///         The <see cref="BlogML20SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(BlogMLDocument)"/>, which changes the data
///         in the <see cref="BlogMLDocument"/> to match the data in the data source.
///     </para>
///     <para>This syndication resource adapter is designed to fill <see cref="BlogMLDocument"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the BlogML 2.0 specification.</para>
/// </remarks>
public class BlogML20SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogML20SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="BlogMLDocument"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="BlogMLDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public BlogML20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Modifies the <see cref="BlogMLDocument"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="BlogMLDocument"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
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
    /// Modifies the <see cref="BlogMLDocument"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="document">The <see cref="BlogMLDocument"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLDocument"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="document"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
            while (authorsIterator.MoveNext())
            {
                XPathNavigator? authorsNode = authorsIterator.Current;
                if (authorsNode is null)
                {
                    continue;
                }

                BlogMLAuthor author = new();
                if (author.Load(authorsNode, settings))
                {
                    document.Authors.Add(author);
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
            while (categoriesIterator.MoveNext())
            {
                XPathNavigator? categoriesNode = categoriesIterator.Current;
                if (categoriesNode is null)
                {
                    continue;
                }

                BlogMLCategory category = new();
                if (category.Load(categoriesNode, settings))
                {
                    document.Categories.Add(category);
                }
            }
        }

        if (postsIterator is { Count: > 0 })
        {
            int counter = 0;
            while (postsIterator.MoveNext())
            {
                XPathNavigator? postsNode = postsIterator.Current;
                if (postsNode is null)
                {
                    continue;
                }

                BlogMLPost post = new();
                counter++;

                if (post.Load(postsNode, settings))
                {
                    if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                    {
                        break;
                    }

                    document.Posts.Add(post);
                }
            }
        }
    }
}