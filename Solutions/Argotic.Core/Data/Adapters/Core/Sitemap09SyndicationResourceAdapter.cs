using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="Sitemap"/> or <see cref="SitemapIndex"/>.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="Sitemap09SyndicationResourceAdapter"/> serves as a bridge between a <see cref="Sitemap"/> or <see cref="SitemapIndex"/> and an XML data source.
///         The <see cref="Sitemap09SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(Sitemap)"/> or <see cref="Fill(SitemapIndex)"/>, which changes the data
///         in the <see cref="Sitemap"/> or <see cref="SitemapIndex"/> to match the data in the data source.
///     </para>
///     <para>
///         This syndication resource adapter is designed to fill <see cref="Sitemap"/> or <see cref="SitemapIndex"/> objects using
///         a <see cref="XPathNavigator"/> that represents XML data that conforms to the Sitemap 0.9 specification.
///     </para>
/// </remarks>
public class Sitemap09SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Sitemap09SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="Sitemap"/> or <see cref="SitemapIndex"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="Sitemap"/> or <see cref="SitemapIndex"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public Sitemap09SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings)
        : base(navigator, settings)
    {
    }

    /// <summary>
    /// Modifies the <see cref="Sitemap"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="Sitemap"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    public void Fill(Sitemap resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? urlsetNavigator = this.Navigator.SelectChildElement("sm", "urlset", manager);

        if (urlsetNavigator is not null)
        {
            XPathNodeIterator urlIterator = urlsetNavigator.Select("sm:url", manager);

            if (urlIterator is { Count: > 0 })
            {
                int counter = 0;
                while (urlIterator.MoveNext())
                {
                    XPathNavigator? urlNode = urlIterator.Current;
                    if (urlNode is null)
                    {
                        continue;
                    }

                    counter++;

                    if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                    {
                        break;
                    }

                    SitemapUrl url = new();
                    if (url.Load(urlNode, this.Settings))
                    {
                        resource.Urls.Add(url);
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new(urlsetNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Modifies the <see cref="SitemapIndex"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="SitemapIndex"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    public void Fill(SitemapIndex resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? sitemapindexNavigator = this.Navigator.SelectChildElement("sm", "sitemapindex", manager);

        if (sitemapindexNavigator is not null)
        {
            XPathNodeIterator sitemapIterator = sitemapindexNavigator.Select("sm:sitemap", manager);

            if (sitemapIterator is { Count: > 0 })
            {
                int counter = 0;
                while (sitemapIterator.MoveNext())
                {
                    XPathNavigator? sitemapNode = sitemapIterator.Current;
                    if (sitemapNode is null)
                    {
                        continue;
                    }

                    counter++;

                    if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                    {
                        break;
                    }

                    SitemapIndexEntry entry = new();
                    if (entry.Load(sitemapNode))
                    {
                        resource.Sitemaps.Add(entry);
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new(sitemapindexNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }
}