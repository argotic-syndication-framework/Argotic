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
///     Reads the two document types RFC 5023 defines: a service document rooted at <c>app:service</c>, and a
///     category document rooted at <c>app:categories</c>. Both live in <c>http://www.w3.org/2007/app</c>,
///     while the <c>atom:category</c> elements inside a category document remain in the Atom namespace — so
///     both prefixes must resolve, which is why the manager comes from <c>AtomUtility</c> rather than being
///     built here.
///     </para>
///     <para>
///     A category document is the one resource in this library that appears in two positions. Stand-alone,
///     it is a document whose root is <c>app:categories</c>. Nested, it is an <c>app:categories</c> element
///     inside a collection in a service document, and <see cref="AtomMemberResources"/> hands this adapter a
///     navigator already positioned on it — where a child selector finds nothing. Hence the second arm in
///     <see cref="Fill(AtomCategoryDocument)"/>, which accepts the navigator itself.
///     </para>
///     <para>
///     RFC 5023 §7.2.1 also allows an out-of-line category document: an <c>app:categories</c> element with
///     an <c>href</c> and no children, naming where the real list lives. Fetching it is the caller's
///     business; this adapter's job is to not lose the <c>href</c> while reading a childless element.
///     </para>
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public AtomPublishing10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Reads an <c>app:categories</c> element — its <c>fixed</c>, <c>scheme</c> and <c>href</c> attributes, its <c>atom:category</c> children, and its syndication extensions — whether it is the document root or the navigator's own position.
    /// </summary>
    /// <param name="resource">The <see cref="AtomCategoryDocument"/> to be filled.</param>
    /// <remarks>
    ///     <para>
    ///     <c>fixed</c> is read as the literal <c>yes</c> or <c>no</c> RFC 5023 specifies, case-insensitively;
    ///     any other value leaves <see cref="AtomCategoryDocument.IsFixed"/> alone rather than guessing. Since
    ///     that property is a plain <see cref="bool"/> defaulting to <see langword="false"/>, an absent or
    ///     unreadable attribute is indistinguishable from <c>fixed="no"</c> once the load has finished.
    ///     </para>
    ///     <para>
    ///     <c>scheme</c> is inherited, as RFC 5023 §7.2.1 requires: a child that declares none is given the
    ///     parent's. The inheritance is materialised onto each <see cref="AtomCategory"/> rather than left
    ///     implicit, so it survives into anything the document is later saved as — at the cost of the saved
    ///     document restating the scheme on every child. That is the same trade the <c>xml:base</c> handling
    ///     makes, and it says the same thing the source document said.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(AtomCategoryDocument resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

        // The second arm is the nested case: AtomMemberResources hands AtomCategoryDocument.Load a
        // navigator positioned ON the app:categories element, where there is no child of that name.
        XPathNavigator? documentNavigator = this.Navigator.SelectChildElement("app", "categories", manager)
            ?? AtomPublishing10SyndicationResourceAdapter.SelfIfCategories(this.Navigator);

        if (documentNavigator is not null)
        {
            AtomUtility.FillCommonObjectAttributes(resource, documentNavigator);

            // Deliberately NOT inside a HasChildren guard. An out-of-line categories element is by
            // definition childless -- its whole purpose is the href saying where the real list lives --
            // so guarding the attribute read on children dropped fixed, scheme and href from precisely
            // the document whose attributes are the only thing it carries.
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
                            // RFC 5023 section 7.2.1: an atom:category child with no scheme attribute
                            // inherits its app:categories parent's. AtomCategory.Load reads only the
                            // attribute in front of it, so the inheritance has to be applied here -- and
                            // without it a bare term arrives with no vocabulary to be a term in.
                            category.Scheme ??= resource.Scheme;

                            resource.Categories.Add(category);
                        }
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new(documentNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Returns the supplied navigator when it is itself positioned on an <c>app:categories</c> element.
    /// </summary>
    /// <param name="navigator">A <see cref="XPathNavigator"/> to test.</param>
    /// <returns>The <paramref name="navigator"/> if it is an <c>app:categories</c> element; otherwise <see langword="null"/>.</returns>
    /// <remarks>
    ///     Restricted to <see cref="XPathNodeType.Element"/>, so a stand-alone category document — which
    ///     arrives here as a <see cref="XPathNodeType.Root"/> and is found by the child selector — takes
    ///     the same path it always did.
    /// </remarks>
    private static XPathNavigator? SelfIfCategories(XPathNavigator navigator) =>
        navigator.NodeType == XPathNodeType.Element
        && string.Equals(navigator.LocalName, "categories", StringComparison.Ordinal)
        && string.Equals(navigator.NamespaceURI, "http://www.w3.org/2007/app", StringComparison.Ordinal)
            ? navigator
            : null;

    /// <summary>
    /// Reads the <c>app:workspace</c> children of <c>app:service</c>, and the service document's own syndication extensions.
    /// </summary>
    /// <param name="resource">The <see cref="AtomServiceDocument"/> to be filled.</param>
    /// <remarks>
    ///     The walk below this point re-enters this adapter: a workspace holds collections, a collection may
    ///     hold an <c>app:categories</c> element, and that element comes back through
    ///     <see cref="Fill(AtomCategoryDocument)"/> by its second arm.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
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