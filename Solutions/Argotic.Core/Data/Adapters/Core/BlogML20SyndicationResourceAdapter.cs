/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/17/2008	brian.kuhn	Created BlogML20SyndicationResourceAdapter Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;

namespace Argotic.Data.Adapters
{
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
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region BlogML20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogML20SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="BlogMLDocument"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="BlogMLDocument"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public BlogML20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
        {
            //------------------------------------------------------------
            //	Initialization and argument validation handled by base class
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Fill(BlogMLDocument resource)
        /// <summary>
        /// Modifies the <see cref="BlogMLDocument"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="BlogMLDocument"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(BlogMLDocument resource)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Create namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager     = BlogMLUtility.CreateNamespaceManager(this.Navigator.NameTable);

            //------------------------------------------------------------
            //	Attempt to fill syndication resource
            //------------------------------------------------------------
            XPathNavigator blogNavigator = this.Navigator.SelectSingleNode("blog:blog", manager);
            if (blogNavigator != null)
            {
                if(blogNavigator.HasAttributes)
                {
                    string dateCreatedAttribute = blogNavigator.GetAttribute("date-created", String.Empty);
                    string rootUrlAttribute     = blogNavigator.GetAttribute("root-url", String.Empty);

                    if (!String.IsNullOrEmpty(dateCreatedAttribute))
                    {
                        DateTime createdOn;
                        if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedAttribute, out createdOn))
                        {
                            resource.GeneratedOn    = createdOn;
                        }
                    }

                    if (!String.IsNullOrEmpty(rootUrlAttribute))
                    {
                        Uri rootUrl;
                        if (Uri.TryCreate(rootUrlAttribute, UriKind.RelativeOrAbsolute, out rootUrl))
                        {
                            resource.RootUrl    = rootUrl;
                        }
                    }
                }

                if (blogNavigator.HasChildren)
                {
                    XPathNavigator titleNavigator       = blogNavigator.SelectSingleNode("blog:title", manager);
                    XPathNavigator subtitleNavigator    = blogNavigator.SelectSingleNode("blog:sub-title", manager);

                    if (titleNavigator != null)
                    {
                        BlogMLTextConstruct title   = new BlogMLTextConstruct();
                        if (title.Load(titleNavigator))
                        {
                            resource.Title          = title;
                        }
                    }

                    if (subtitleNavigator != null)
                    {
                        BlogMLTextConstruct subtitle    = new BlogMLTextConstruct();
                        if (subtitle.Load(subtitleNavigator))
                        {
                            resource.Subtitle           = subtitle;
                        }
                    }

                    BlogML20SyndicationResourceAdapter.FillDocumentCollections(resource, blogNavigator, manager, this.Settings);
                }

                SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(blogNavigator, this.Settings);
                adapter.Fill(resource, manager);
            }
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region FillDocumentCollections(BlogMLDocument document, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
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
        /// <exception cref="ArgumentNullException">The <paramref name="document"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillDocumentCollections(BlogMLDocument document, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(document, "document");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Attempt to extract syndication information
            //------------------------------------------------------------
            XPathNodeIterator authorsIterator               = source.Select("blog:authors/blog:author", manager);
            XPathNodeIterator extendedPropertiesIterator    = source.Select("blog:extended-properties/blog:property", manager);
            XPathNodeIterator categoriesIterator            = source.Select("blog:categories/blog:category", manager);
            XPathNodeIterator postsIterator                 = source.Select("blog:posts/blog:post", manager);

            if (authorsIterator != null && authorsIterator.Count > 0)
            {
                while (authorsIterator.MoveNext())
                {
                    BlogMLAuthor author = new BlogMLAuthor();
                    if (author.Load(authorsIterator.Current, settings))
                    {
                        document.Authors.Add(author);
                    }
                }
            }

            if (extendedPropertiesIterator != null && extendedPropertiesIterator.Count > 0)
            {
                while (extendedPropertiesIterator.MoveNext())
                {
                    if (extendedPropertiesIterator.Current.HasAttributes)
                    {
                        string propertyName     = extendedPropertiesIterator.Current.GetAttribute("name", String.Empty);
                        string propertyValue    = extendedPropertiesIterator.Current.GetAttribute("value", String.Empty);

                        if (!String.IsNullOrEmpty(propertyName) && !document.ExtendedProperties.ContainsKey(propertyName))
                        {
                            document.ExtendedProperties.Add(propertyName, propertyValue);
                        }
                    }
                }
            }

            if (categoriesIterator != null && categoriesIterator.Count > 0)
            {
                while (categoriesIterator.MoveNext())
                {
                    BlogMLCategory category = new BlogMLCategory();
                    if (category.Load(categoriesIterator.Current, settings))
                    {
                        document.Categories.Add(category);
                    }
                }
            }

            if (postsIterator != null && postsIterator.Count > 0)
            {
                int counter = 0;
                while (postsIterator.MoveNext())
                {
                    BlogMLPost post = new BlogMLPost();
                    counter++;

                    if (post.Load(postsIterator.Current, settings))
                    {
                        if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                        {
                            break;
                        }

                        ((Collection<BlogMLPost>)document.Posts).Add(post);
                    }
                }
            }
        }
        #endregion
    }
}
