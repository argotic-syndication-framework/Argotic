using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Represents metadata associated with a <see cref="ISyndicationResource">syndication resource</see>.
/// </summary>
[Serializable]
public class SyndicationResourceMetadata : IComparable<SyndicationResourceMetadata>, IEquatable<SyndicationResourceMetadata>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the syndication content format that the syndication resource conforms to.
    /// </summary>
    private SyndicationContentFormat resourceFormat = SyndicationContentFormat.None;

    /// <summary>
    /// Private member to hold the XML namespaces declared in the syndication resource's root element.
    /// </summary>
    private readonly Dictionary<string, string> resourceNamespaces = [];

    /// <summary>
    /// Private member to hold the version of the syndication specification that the resource conforms to.
    /// </summary>
    private Version? resourceVersion;

    /// <summary>
    /// Private member to hold a XPath navigator that can be used to navigate the root element of the syndication resource.
    /// </summary>
    [NonSerialized]
    private XPathNavigator? resourceRootNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceMetadata"/> class using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="navigator">The <see cref="XPathNavigator"/> to extract the syndication resource meta-data from.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    public SyndicationResourceMetadata(XPathNavigator navigator)
    {
        ArgumentNullException.ThrowIfNull(navigator);

        this.Load(navigator);
    }

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that the syndication resource conforms to.
    /// </summary>
    /// <value>
    ///     A <see cref="SyndicationContentFormat"/> enumeration value that indicates the syndication specification the resource conforms to.
    ///     If the syndication content format is unable to be determined, returns <see cref="SyndicationContentFormat.None"/>.
    /// </value>
    public SyndicationContentFormat Format
    {
        get => resourceFormat;
        protected set => resourceFormat = value;
    }

    /// <summary>
    /// Gets a dictionary of the XML namespaces declared in the syndication resource.
    /// </summary>
    /// <value>A dictionary of the resource's XML namespaces, keyed off of the namespace prefix. If no XML namespaces are declared on the root element of the resource, returns an empty dictionary.</value>
    public Dictionary<string, string> Namespaces => resourceNamespaces;

    /// <summary>
    /// Gets a read-only <see cref="XPathNavigator"/> object that can be used to navigate the root element of the syndication resource.
    /// </summary>
    /// <value>A read-only <see cref="XPathNavigator"/> object that can be used to navigate the root element of the syndication resource.</value>
    public XPathNavigator? Resource => resourceRootNode;

    /// <summary>
    /// Gets the <see cref="Version"/> of the syndication specification that the resource conforms to.
    /// </summary>
    /// <value>The version number of the syndication specification that the resource conforms to. If format version is unable to be determined, returns <b>null</b>.</value>
    public Version? Version => resourceVersion;

    /// <summary>
    /// Returns a <see cref="Version"/> object for the value of the XML attribute in <paramref name="navigator"/> with a local name specified by <paramref name="name"/>.
    /// </summary>
    /// <param name="navigator">The <see cref="XPathNavigator"/> to extract the XML attribute value from.</param>
    /// <param name="name">The name of the attribute to parse in the <paramref name="navigator"/>.</param>
    /// <returns>The <see cref="Version"/> represented by the value of the specified XML attribute. If unable to determine version, returns <b>null</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    protected static Version? GetVersionFromAttribute(XPathNavigator navigator, string name)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentException.ThrowIfNullOrEmpty(name);

        string value = navigator.GetAttribute(name, string.Empty);

        return Version.TryParse(value, out var version) ? version : null;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Attention Profiling Markup Language (APML) formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Attention Profiling Markup Language (APML) formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseApmlResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("apml", "http://www.apml.org/apml-0.6");

        version = null;
        if ((navigator = resource.SelectSingleNode("APML", manager)) is not null || (navigator = resource.SelectSingleNode("apml:APML", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.apml.org/apml-0.6"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(0, 6);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Atom formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Atom formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseAtomResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
        manager.AddNamespace("atom03", "http://purl.org/atom/ns#");

        version = null;
        if ((navigator = resource.SelectSingleNode("feed", manager)) is not null || (navigator = resource.SelectSingleNode("atom:feed", manager)) is not null || (navigator = resource.SelectSingleNode("atom03:feed", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.w3.org/2005/Atom"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(1, 0);
            }
            else if (namespaces.ContainsValue("http://purl.org/atom/ns#"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(0, 3);
            }
        }
        else if ((navigator = resource.SelectSingleNode("entry", manager)) is not null || (navigator = resource.SelectSingleNode("atom:entry", manager)) is not null || (navigator = resource.SelectSingleNode("atom03:entry", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.w3.org/2005/Atom"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(1, 0);
            }
            else if (namespaces.ContainsValue("http://purl.org/atom/ns#"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(0, 3);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Atom Publishing Protocol category document formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Atom formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseAtomPublishingCategoriesResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
        manager.AddNamespace("atom03", "http://purl.org/atom/ns#");
        manager.AddNamespace("app", "http://www.w3.org/2007/app");

        version = null;
        if ((navigator = resource.SelectSingleNode("categories", manager)) is not null || (navigator = resource.SelectSingleNode("app:categories", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.w3.org/2007/app"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(1, 0);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Atom Publishing Protocol service document formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Atom formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseAtomPublishingServiceResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
        manager.AddNamespace("atom03", "http://purl.org/atom/ns#");
        manager.AddNamespace("app", "http://www.w3.org/2007/app");

        version = null;
        if ((navigator = resource.SelectSingleNode("service", manager)) is not null || (navigator = resource.SelectSingleNode("app:service", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.w3.org/2007/app"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(1, 0);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a  Web Log Markup Language (BlogML) formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a  Web Log Markup Language (BlogML) formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseBlogMLResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("blogML", "http://www.blogml.com/2006/09/BlogML");

        version = null;
        if ((navigator = resource.SelectSingleNode("blog", manager)) is not null || (navigator = resource.SelectSingleNode("blogML:blog", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.blogml.com/2006/09/BlogML"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(2, 0);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Microsummary Generator formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Microsummary Generator formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseMicroSummaryGeneratorResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("micro", "http://www.mozilla.org/microsummaries/0.1");

        version = null;
        if ((navigator = resource.SelectSingleNode("generator", manager)) is not null || (navigator = resource.SelectSingleNode("micro:generator", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.mozilla.org/microsummaries/0.1"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(0, 1);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a News Markup Language (NewsML) formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a News Markup Language (NewsML) formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseNewsMLResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        version = null;
        if ((navigator = resource.SelectSingleNode("NewsML")) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");

            resourceConformsToFormat = true;
            version ??= new Version(2, 0);
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a OpenSearch Description formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a OpenSearch Description formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseOpenSearchDescriptionResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("search", "http://a9.com/-/spec/opensearch/1.1/");

        version = null;
        if ((navigator = resource.SelectSingleNode("OpenSearchDescription", manager)) is not null || (navigator = resource.SelectSingleNode("search:OpenSearchDescription", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://a9.com/-/spec/opensearch/1.1/"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(1, 1);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a  Outline Processor Markup Language (OPML) formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a  Outline Processor Markup Language (OPML) formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseOpmlResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        version = null;
        if ((navigator = resource.SelectSingleNode("opml")) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");

            resourceConformsToFormat = true;
            version ??= new Version(2, 0);
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Really Simple Discovery (RSD) formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Really Simple Discovery (RSD) formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseRsdResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("rsd", "http://archipelago.phrasewise.com/rsd");

        version = null;
        if ((navigator = resource.SelectSingleNode("rsd", manager)) is not null || (navigator = resource.SelectSingleNode("rsd:rsd", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://archipelago.phrasewise.com/rsd"))
            {
                resourceConformsToFormat = true;
                version ??= new Version(1, 0);
            }
            else if (string.Equals(navigator.Name, "rsd", StringComparison.OrdinalIgnoreCase) && version is not null)
            {
                //  Most web log software actually fails to provide the default XML namespace per RSD spec, so this is a hack/compromise
                resourceConformsToFormat = true;
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Really Simple Syndication (RSS) formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Really Simple Syndication (RSS) formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseRssResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        manager.AddNamespace("rss09", "http://my.netscape.com/rdf/simple/0.9/");
        manager.AddNamespace("rss10", "http://purl.org/rss/1.0/");

        version = null;
        if ((navigator = resource.SelectSingleNode("rss", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");

            resourceConformsToFormat = true;
            version ??= new Version(2, 0);
        }
        else if ((navigator = resource.SelectSingleNode("rdf:RDF", manager)) is not null)
        {
            version = SyndicationResourceMetadata.GetVersionFromAttribute(navigator, "version");
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://purl.org/rss/1.0/"))
            {
                resourceConformsToFormat = true;
                version = new Version(1, 0);
            }
            else if (namespaces.ContainsValue("http://my.netscape.com/rdf/simple/0.9/"))
            {
                resourceConformsToFormat = true;
                version = new Version(0, 9);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Sitemap 0.9 formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Sitemap 0.9 formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseSitemapResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("sm", "http://www.sitemaps.org/schemas/sitemap/0.9");

        version = null;
        if ((navigator = resource.SelectSingleNode("urlset", manager)) is not null || (navigator = resource.SelectSingleNode("sm:urlset", manager)) is not null)
        {
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.sitemaps.org/schemas/sitemap/0.9"))
            {
                resourceConformsToFormat = true;
                version = new Version(0, 9);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Determines if the specified <see cref="XPathNavigator"/> represents a Sitemap Index 0.9 formatted syndication resource.
    /// </summary>
    /// <param name="resource">A <see cref="XPathNavigator"/> that represents the syndication resource to attempt to parse.</param>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that can be used to navigate the root element of the syndication resource. This parameter is passed uninitialized.</param>
    /// <param name="version">The version of the syndication specification that the resource conforms to. This parameter is passed uninitialized.</param>
    /// <returns><b>true</b> if <paramref name="resource"/> represents a Sitemap Index 0.9 formatted syndication resource; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    protected static bool TryParseSitemapIndexResource(XPathNavigator resource, out XPathNavigator? navigator, out Version? version)
    {
        bool resourceConformsToFormat = false;

        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(resource.NameTable);
        manager.AddNamespace("sm", "http://www.sitemaps.org/schemas/sitemap/0.9");

        version = null;
        if ((navigator = resource.SelectSingleNode("sitemapindex", manager)) is not null || (navigator = resource.SelectSingleNode("sm:sitemapindex", manager)) is not null)
        {
            Dictionary<string, string> namespaces = (Dictionary<string, string>)navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            if (namespaces.ContainsValue("http://www.sitemaps.org/schemas/sitemap/0.9"))
            {
                resourceConformsToFormat = true;
                version = new Version(0, 9);
            }
        }

        return resourceConformsToFormat;
    }

    /// <summary>
    /// Extracts the content format, version, and XML namespaces for a syndication resource from the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="resource">The <see cref="XPathNavigator"/> to extract the syndication resource meta-data from.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    private void Load(XPathNavigator resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        Dictionary<string, string> namespaces = (Dictionary<string, string>)resource.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
        foreach (string prefix in namespaces.Keys)
        {
            resourceNamespaces.Add(prefix, namespaces[prefix]);
        }

        resourceVersion = SyndicationResourceMetadata.GetVersionFromAttribute(resource, "version");

        if (SyndicationResourceMetadata.TryParseApmlResource(resource, out XPathNavigator? navigator, out Version? version))
        {
            resourceFormat = SyndicationContentFormat.Apml;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseAtomResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.Atom;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseAtomPublishingCategoriesResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.AtomCategoryDocument;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseAtomPublishingServiceResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.AtomServiceDocument;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseBlogMLResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.BlogML;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseMicroSummaryGeneratorResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.MicroSummaryGenerator;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseNewsMLResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.NewsML;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseOpenSearchDescriptionResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.OpenSearchDescription;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseOpmlResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.Opml;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseRsdResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.Rsd;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseRssResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.Rss;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseSitemapResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.Sitemap;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else if (SyndicationResourceMetadata.TryParseSitemapIndexResource(resource, out navigator, out version))
        {
            resourceFormat = SyndicationContentFormat.SitemapIndex;
            resourceRootNode = navigator;
            resourceVersion = version;
        }
        else
        {
            resourceFormat = SyndicationContentFormat.None;
            resourceRootNode = null;
            resourceVersion = null;
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceMetadata"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationResourceMetadata"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
    /// </remarks>
    public override string ToString()
    {
        string format = this.Format.ToString();
        string version = this.Version?.ToString() ?? string.Empty;
        string namespaces = this.Namespaces?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;
        string resource = this.Resource?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;

        return $"[SyndicationResourceMetadata(Format = \"{format}\", Version = \"{version}\", Namespaces = \"{namespaces}\", Resource = \"{resource}\")]";
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SyndicationResourceMetadata? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Format.CompareTo(other.Format);

        if (this.Version is not null)
        {
            if (result == 0) result = Comparer<Version>.Default.Compare(this.Version, other.Version);
        }
        else if (other.Version is not null)
        {
            if (result == 0) result = -1;
        }

        if (this.Namespaces is not null && other.Namespaces is not null)
        {
            if (result == 0) result = ComparisonUtility.CompareSequence(this.Namespaces, other.Namespaces, StringComparison.Ordinal);
        }
        else if (this.Namespaces is not null && other.Namespaces is null)
        {
            if (result == 0) result = 1;
        }
        else if (this.Namespaces is null && other.Namespaces is not null)
        {
            if (result == 0) result = -1;
        }

        if (result == 0) result = (this.Resource, other.Resource) switch
        {
            (XPathNavigator source, XPathNavigator target) => string.Compare(source.OuterXml, target.OuterXml, StringComparison.OrdinalIgnoreCase),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyndicationResourceMetadata"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SyndicationResourceMetadata"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SyndicationResourceMetadata"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SyndicationResourceMetadata? other)
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
    public override bool Equals(object? obj) => obj is SyndicationResourceMetadata other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Format), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.Resource?.OuterXml));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SyndicationResourceMetadata? first, SyndicationResourceMetadata? second)
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
    public static bool operator !=(SyndicationResourceMetadata? first, SyndicationResourceMetadata? second) => !(first == second);
}