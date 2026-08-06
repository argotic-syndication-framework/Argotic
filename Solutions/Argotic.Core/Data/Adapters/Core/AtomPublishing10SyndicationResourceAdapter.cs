using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Publishing;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public AtomPublishing10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Modifies the <see cref="AtomCategoryDocument"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="AtomCategoryDocument"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    public void Fill(AtomCategoryDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? documentNavigator = this.Navigator.SelectChildElement("app", "categories", manager);
        if (documentNavigator is not null)
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
                        if (string.Equals(fixedAttribute, "yes", StringComparison.OrdinalIgnoreCase))
                        {
                            resource.IsFixed = true;
                        }
                        else if (string.Equals(fixedAttribute, "no", StringComparison.OrdinalIgnoreCase))
                        {
                            resource.IsFixed = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(schemeAttribute))
                    {
                        if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri? scheme))
                        {
                            resource.Scheme = scheme;
                        }
                    }

                    if (!string.IsNullOrEmpty(hrefAttribute))
                    {
                        if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out Uri? href))
                        {
                            resource.Uri = href;
                        }
                    }
                }

                if (documentNavigator.HasChildren)
                {
                    XPathNodeIterator categoryIterator = documentNavigator.SelectChildElements("atom", "category", manager);

                    if (categoryIterator is { Count: > 0 })
                    {
                        while (categoryIterator.MoveNext())
                        {
                            XPathNavigator? categoryNode = categoryIterator.Current;
                            if (categoryNode is null)
                            {
                                continue;
                            }

                            AtomCategory category = new();
                            if (category.Load(categoryNode, this.Settings))
                            {
                                resource.Categories.Add(category);
                            }
                        }
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new(documentNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Modifies the <see cref="AtomServiceDocument"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="AtomServiceDocument"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    public void Fill(AtomServiceDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? documentNavigator = this.Navigator.SelectChildElement("app", "service", manager);
        if (documentNavigator is not null)
        {
            AtomUtility.FillCommonObjectAttributes(resource, documentNavigator);

            if (documentNavigator.HasChildren)
            {
                XPathNodeIterator workspaceIterator = documentNavigator.SelectChildElements("app", "workspace", manager);

                if (workspaceIterator is { Count: > 0 })
                {
                    while (workspaceIterator.MoveNext())
                    {
                        XPathNavigator? workspaceNode = workspaceIterator.Current;
                        if (workspaceNode is null)
                        {
                            continue;
                        }

                        AtomWorkspace workspace = new();
                        if (workspace.Load(workspaceNode, this.Settings))
                        {
                            resource.Workspaces.Add(workspace);
                        }
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new(documentNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }
}