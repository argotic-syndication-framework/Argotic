﻿namespace Argotic.Data.Adapters
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;
    using Argotic.Publishing;
    using Argotic.Syndication;

    /// <summary>
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="AtomServiceDocument"/> or <see cref="AtomCategoryDocument"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomPublishing10SyndicationResourceAdapter"/> serves as a bridge between a <see cref="AtomServiceDocument"/> and an XML data source.
    ///         The <see cref="AtomPublishing10SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(AtomServiceDocument)"/> or <see cref="Fill(AtomCategoryDocument)"/>, which changes the data
    ///         in the <see cref="AtomServiceDocument"/> or <see cref="AtomCategoryDocument"/> to match the data in the data source.
    ///     </para>
    ///     <para>This syndication resource adapter is designed to fill <see cref="AtomServiceDocument"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the Atom Publishing Protocol 1.0 specification.</para>
    /// </remarks>
    public class AtomPublishing10SyndicationResourceAdapter : SyndicationResourceAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomPublishing10SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomServiceDocument"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="AtomServiceDocument"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomPublishing10SyndicationResourceAdapter(
            XPathNavigator navigator,
            SyndicationResourceLoadSettings settings)
            : base(navigator, settings)
        {
        }

        /// <summary>
        /// Modifies the <see cref="AtomCategoryDocument"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="AtomCategoryDocument"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(AtomCategoryDocument resource)
        {
            Guard.ArgumentNotNull(resource, "resource");

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

            XPathNavigator documentNavigator = this.Navigator.SelectSingleNode("app:categories", manager);
            if (documentNavigator != null)
            {
                AtomUtility.FillCommonObjectAttributes(resource, documentNavigator);

                if (documentNavigator.HasChildren)
                {
                    if (documentNavigator.HasAttributes)
                    {
                        string fixedAttribute = documentNavigator.GetAttribute("fixed", string.Empty);
                        string schemeAttribute = documentNavigator.GetAttribute("scheme", string.Empty);
                        string hrefAttribute = documentNavigator.GetAttribute("href", string.Empty);

                        if (!string.IsNullOrEmpty(fixedAttribute))
                        {
                            if (string.Compare(fixedAttribute, "yes", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                resource.IsFixed = true;
                            }
                            else if (string.Compare(fixedAttribute, "no", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                resource.IsFixed = false;
                            }
                        }

                        if (!string.IsNullOrEmpty(schemeAttribute))
                        {
                            Uri scheme;
                            if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out scheme))
                            {
                                resource.Scheme = scheme;
                            }
                        }

                        if (!string.IsNullOrEmpty(hrefAttribute))
                        {
                            Uri href;
                            if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out href))
                            {
                                resource.Uri = href;
                            }
                        }
                    }

                    if (documentNavigator.HasChildren)
                    {
                        XPathNodeIterator categoryIterator = documentNavigator.Select("atom:category", manager);

                        if (categoryIterator != null && categoryIterator.Count > 0)
                        {
                            while (categoryIterator.MoveNext())
                            {
                                AtomCategory category = new AtomCategory();
                                if (category.Load(categoryIterator.Current, this.Settings))
                                {
                                    resource.AddCategory(category);
                                }
                            }
                        }
                    }
                }

                SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(documentNavigator, this.Settings);
                adapter.Fill(resource, manager);
            }
        }

        /// <summary>
        /// Modifies the <see cref="AtomServiceDocument"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="AtomServiceDocument"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(AtomServiceDocument resource)
        {
            Guard.ArgumentNotNull(resource, "resource");

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

            XPathNavigator documentNavigator = this.Navigator.SelectSingleNode("app:service", manager);
            if (documentNavigator != null)
            {
                AtomUtility.FillCommonObjectAttributes(resource, documentNavigator);

                if (documentNavigator.HasChildren)
                {
                    XPathNodeIterator workspaceIterator = documentNavigator.Select("app:workspace", manager);

                    if (workspaceIterator != null && workspaceIterator.Count > 0)
                    {
                        while (workspaceIterator.MoveNext())
                        {
                            AtomWorkspace workspace = new AtomWorkspace();
                            if (workspace.Load(workspaceIterator.Current, this.Settings))
                            {
                                resource.AddWorkspace(workspace);
                            }
                        }
                    }
                }

                SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(documentNavigator, this.Settings);
                adapter.Fill(resource, manager);
            }
        }
    }
}