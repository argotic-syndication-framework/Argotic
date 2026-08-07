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
///     One adapter, two documents. The sitemaps.org protocol defines a URL set rooted at <c>urlset</c> and
///     an index of sitemaps rooted at <c>sitemapindex</c>, in the same
///     <c>http://www.sitemaps.org/schemas/sitemap/0.9</c> namespace and distinguished only by that root
///     name. Each overload looks for its own root and does nothing if it is not there, so handing an index
///     to <see cref="Fill(Sitemap)"/> yields an empty sitemap rather than an error.
///     </para>
///     <para>
///     The namespace is bound from <c>SitemapUtility</c>'s constant, never from what the document declares.
///     A sitemap in some other namespace is a different format and is not read as though it were this one.
///     </para>
///     <para>
///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> is tested <i>before</i> the entry is
///     parsed here, so the work stops at the limit. The Atom and BlogML adapters test after parsing and
///     throw one parsed item away; on a sitemap, where a single file may legitimately carry 50,000 URLs,
///     that difference is the difference between reading the file and reading a prefix of it.
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Sitemap09SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings)
        : base(navigator, settings)
    {
    }

    /// <summary>
    /// Enumerates the <c>url</c> children of <c>urlset</c>, then attaches the syndication extensions found on <c>urlset</c>.
    /// </summary>
    /// <param name="resource">The <see cref="Sitemap"/> to be filled.</param>
    /// <remarks>
    ///     A <c>url</c> whose <c>Load</c> returns <see langword="false"/> is dropped but still counts against
    ///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/>, because the counter advances before
    ///     the parse. Google's news, image, video and hreflang extensions are attached per URL by
    ///     <see cref="SitemapUrl"/>, not here; this call attaches only what is declared at document level.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(Sitemap resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? urlsetNavigator = this.Navigator.SelectChildElement("sm", "urlset", manager);

        if (urlsetNavigator is not null)
        {
            XPathNodeIterator urlIterator = urlsetNavigator.SelectChildElements("sm", "url", manager);

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
    /// Enumerates the <c>sitemap</c> children of <c>sitemapindex</c>, then attaches the syndication extensions found on <c>sitemapindex</c>.
    /// </summary>
    /// <param name="resource">The <see cref="SitemapIndex"/> to be filled.</param>
    /// <remarks>
    ///     <see cref="SitemapIndexEntry"/> is loaded without <see cref="SyndicationResourceAdapter.Settings"/>
    ///     and takes no overload that would accept them, because unlike <see cref="SitemapUrl"/> it is not an
    ///     extensible object: an index entry is a location and a last-modified date, with nowhere for an
    ///     extension to attach. The settings still govern how many entries are read.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(SitemapIndex resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? sitemapindexNavigator = this.Navigator.SelectChildElement("sm", "sitemapindex", manager);

        if (sitemapindexNavigator is not null)
        {
            XPathNodeIterator sitemapIterator = sitemapindexNavigator.SelectChildElements("sm", "sitemap", manager);

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