using System.Xml.XPath;

using Argotic.Common;
using Argotic.Publishing;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="ISyndicationResource"/>.
/// </summary>
/// <remarks>
///     <para>
///     This is the dispatcher, not a parser. <see cref="Fill(ISyndicationResource, SyndicationContentFormat)"/>
///     sniffs the document with <see cref="SyndicationResourceMetadata"/>, refuses it when the format found is
///     not the format the caller's resource type reads, and then hands the same navigator to the adapter for
///     that format <i>and that version</i>. Every element walk happens in the derived adapter.
///     </para>
///     <para>
///     Version routing is a chain of equality tests with no fallback arm, and that is observable behaviour:
///     a document whose format matches but whose version no adapter claims — RSS 0.93, say, or APML 0.5 —
///     fills nothing and raises nothing. The caller is handed an empty resource with no diagnostic.
///     </para>
///     <para>
///     Two of the routes are deliberately many-to-one. OPML 1.0, 1.1 and 2.0 all reach
///     <see cref="Opml20SyndicationResourceAdapter"/>, because the head/body shape did not change across
///     them; an Atom feed document and a stand-alone Atom entry document both reach the adapter for their
///     version, which then picks the overload by the resource's runtime type.
///     </para>
/// </remarks>
public class SyndicationResourceAdapter
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="ISyndicationResource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);

        Navigator = navigator;
        Settings = settings;
    }

    /// <summary>
    /// Gets the <see cref="XPathNavigator"/> used to fill a syndication resource.
    /// </summary>
    /// <value>The navigator supplied to the constructor. Derived adapters expect it to be positioned on the document root, not on the format's root element.</value>
    public XPathNavigator Navigator { get; }

    /// <summary>
    /// Gets the <see cref="SyndicationResourceLoadSettings"/> used to configure the fill of a syndication resource.
    /// </summary>
    public SyndicationResourceLoadSettings Settings { get; } = new();

    /// <summary>
    /// Verifies that the data source is the format the caller expects, then routes it to the adapter for that format and version.
    /// </summary>
    /// <param name="resource">The <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="format">The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that the <paramref name="resource"/> is expected to conform to.</param>
    /// <remarks>
    ///     The format check is the only validation performed here. Once past it, whether anything is read
    ///     depends on the document's version matching one the routing recognises; an unrecognised version
    ///     leaves <paramref name="resource"/> untouched and reports nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="format"/> is equal to <see cref="SyndicationContentFormat.None"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="resource"/> data does not conform to the specified <paramref name="format"/>.</exception>
    public void Fill(ISyndicationResource resource, SyndicationContentFormat format)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (format == SyndicationContentFormat.None)
        {
            throw new ArgumentException($"The specified syndication content format of {format} is invalid.", nameof(format));
        }

        SyndicationResourceMetadata resourceMetadata = new(this.Navigator);

        if (format != resourceMetadata.Format)
        {
            // Atom's two document shapes are the pairing most often confused, because both are "an
            // Atom document" and only one of them is common on the open web. Naming the root elements
            // says what the format names alone do not.
            string detail = IsAtomShapeMismatch(format, resourceMetadata.Format)
                ? " An Atom feed document has a <feed> root and is read by AtomFeed; a stand-alone Atom entry document has an <entry> root and is read by AtomEntry."
                : string.Empty;

            throw new FormatException(
                $"The supplied syndication resource has a content format of {resourceMetadata.Format}, which does not match the expected content format of {format}.{detail}");
        }

        switch (format)
        {
            case SyndicationContentFormat.Apml:

                this.FillApmlResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.Atom:

            // A stand-alone entry document is filled by the same adapters; what the distinct format
            // value buys is the check above, which now refuses a feed handed to an AtomEntry and an
            // entry document handed to an AtomFeed. Both used to pass it and yield an empty object.
            case SyndicationContentFormat.AtomEntryDocument:

                this.FillAtomResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.AtomCategoryDocument:

                this.FillAtomPublishingResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.AtomServiceDocument:

                this.FillAtomPublishingResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.BlogML:

                this.FillBlogMLResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.Opml:

                this.FillOpmlResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.Rsd:

                this.FillRsdResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.Rss:

                this.FillRssResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.Sitemap:

                this.FillSitemapResource(resource, resourceMetadata);
                break;

            case SyndicationContentFormat.SitemapIndex:

                this.FillSitemapIndexResource(resource, resourceMetadata);
                break;
        }
    }

    /// <summary>
    /// Routes an Attention Profiling Markup Language (APML) document to <see cref="Apml06SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The Attention Profiling Markup Language (APML) <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillApmlResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        ApmlDocument apmlDocument = (ApmlDocument)resource;

        if (resourceMetadata.Version == new Version("0.6"))
        {
            Apml06SyndicationResourceAdapter apml06Adapter = new(this.Navigator, this.Settings);
            apml06Adapter.Fill(apmlDocument);
        }
    }

    /// <summary>
    /// Routes an Atom document to <see cref="Atom10SyndicationResourceAdapter"/> or <see cref="Atom03SyndicationResourceAdapter"/>, and to the feed or entry overload by the resource's runtime type.
    /// </summary>
    /// <param name="resource">The Atom <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <remarks>
    ///     Atom 1.0 and Atom 0.3 are not two versions of one grammar; they are different formats sharing a
    ///     name, with different namespaces and different element vocabularies. That is why the version test
    ///     selects between two whole adapters rather than a flag inside one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillAtomResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        if (resourceMetadata.Version == new Version("1.0"))
        {
            Atom10SyndicationResourceAdapter atom10Adapter = new(this.Navigator, this.Settings);
            if (resource is AtomFeed atomFeed)
            {
                atom10Adapter.Fill(atomFeed);
            }
            else if (resource is AtomEntry atomEntry)
            {
                atom10Adapter.Fill(atomEntry);
            }
        }

        if (resourceMetadata.Version == new Version("0.3"))
        {
            Atom03SyndicationResourceAdapter atom03Adapter = new(this.Navigator, this.Settings);
            if (resource is AtomFeed atomFeed)
            {
                atom03Adapter.Fill(atomFeed);
            }
            else if (resource is AtomEntry atomEntry)
            {
                atom03Adapter.Fill(atomEntry);
            }
        }
    }

    /// <summary>
    /// Determines whether a format mismatch is an Atom feed document confused for an entry document, or the reverse.
    /// </summary>
    /// <param name="expected">The format the resource type reads.</param>
    /// <param name="detected">The format the supplied document actually is.</param>
    /// <returns><see langword="true"/> if the two are Atom's two document shapes; otherwise, <see langword="false"/>.</returns>
    private static bool IsAtomShapeMismatch(SyndicationContentFormat expected, SyndicationContentFormat detected) =>
        (expected == SyndicationContentFormat.Atom && detected == SyndicationContentFormat.AtomEntryDocument)
        || (expected == SyndicationContentFormat.AtomEntryDocument && detected == SyndicationContentFormat.Atom);

    /// <summary>
    /// Routes an Atom Publishing Protocol service or category document to <see cref="AtomPublishing10SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The Atom Publishing Protocol <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillAtomPublishingResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        if (resourceMetadata.Version == new Version("1.0"))
        {
            AtomPublishing10SyndicationResourceAdapter atomPublishing10Adapter = new(this.Navigator, this.Settings);
            if (resource is AtomCategoryDocument categoryDocument)
            {
                atomPublishing10Adapter.Fill(categoryDocument);
            }
            else if (resource is AtomServiceDocument serviceDocument)
            {
                atomPublishing10Adapter.Fill(serviceDocument);
            }
        }
    }

    /// <summary>
    /// Routes a BlogML document to <see cref="BlogML20SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The BlogML <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillBlogMLResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        BlogMLDocument blogMLDocument = (BlogMLDocument)resource;
        BlogML20SyndicationResourceAdapter blogML20Adapter = new(this.Navigator, this.Settings);

        if (resourceMetadata.Version == new Version("2.0"))
        {
            blogML20Adapter.Fill(blogMLDocument);
        }
    }

    /// <summary>
    /// Routes an Outline Processor Markup Language (OPML) document of version 1.0, 1.1 or 2.0 to <see cref="Opml20SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The Outline Processor Markup Language (OPML) <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <remarks>
    ///     Three arms, one destination. The head/body/outline shape is common to all three OPML versions,
    ///     so the version is used only to decide that the document is OPML at all. The arms are kept apart
    ///     rather than collapsed because a version-specific adapter would slot into one of them.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillOpmlResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        OpmlDocument opmlDocument = (OpmlDocument)resource;
        Opml20SyndicationResourceAdapter opml20Adapter = new(this.Navigator, this.Settings);

        if (resourceMetadata.Version == new Version("2.0"))
        {
            opml20Adapter.Fill(opmlDocument);
        }

        if (resourceMetadata.Version == new Version("1.1"))
        {
            opml20Adapter.Fill(opmlDocument);
        }

        if (resourceMetadata.Version == new Version("1.0"))
        {
            opml20Adapter.Fill(opmlDocument);
        }
    }

    /// <summary>
    /// Routes a Really Simple Discovery (RSD) document to <see cref="Rsd10SyndicationResourceAdapter"/> or <see cref="Rsd06SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The Really Simple Discovery (RSD) <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillRsdResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        RsdDocument rsdDocument = (RsdDocument)resource;

        if (resourceMetadata.Version == new Version("1.0"))
        {
            Rsd10SyndicationResourceAdapter rsd10Adapter = new(this.Navigator, this.Settings);
            rsd10Adapter.Fill(rsdDocument);
        }

        if (resourceMetadata.Version == new Version("0.6"))
        {
            Rsd06SyndicationResourceAdapter rsd06Adapter = new(this.Navigator, this.Settings);
            rsd06Adapter.Fill(rsdDocument);
        }
    }

    /// <summary>
    /// Routes a Really Simple Syndication (RSS) feed to the adapter for its version, of which there are five.
    /// </summary>
    /// <param name="resource">The Really Simple Syndication (RSS) <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <remarks>
    ///     The five destinations do not form a version ladder. RSS 0.90 and RSS 1.0 are RDF documents rooted
    ///     at <c>rdf:RDF</c>, in unrelated namespaces; RSS 0.91, 0.92 and 2.0 are plain XML rooted at
    ///     <c>rss</c> with no namespace at all. They share a name and a target object model, and almost
    ///     nothing else, which is why the adapters share no parsing code.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillRssResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        RssFeed rssFeed = (RssFeed)resource;

        if (resourceMetadata.Version == new Version("2.0"))
        {
            Rss20SyndicationResourceAdapter rss20Adapter = new(this.Navigator, this.Settings);
            rss20Adapter.Fill(rssFeed);
        }

        if (resourceMetadata.Version == new Version("1.0"))
        {
            Rss10SyndicationResourceAdapter rss10Adapter = new(this.Navigator, this.Settings);
            rss10Adapter.Fill(rssFeed);
        }

        if (resourceMetadata.Version == new Version("0.92"))
        {
            Rss092SyndicationResourceAdapter rss092Adapter = new(this.Navigator, this.Settings);
            rss092Adapter.Fill(rssFeed);
        }

        if (resourceMetadata.Version == new Version("0.91"))
        {
            Rss091SyndicationResourceAdapter rss091Adapter = new(this.Navigator, this.Settings);
            rss091Adapter.Fill(rssFeed);
        }

        if (resourceMetadata.Version == new Version("0.9"))
        {
            Rss090SyndicationResourceAdapter rss090Adapter = new(this.Navigator, this.Settings);
            rss090Adapter.Fill(rssFeed);
        }
    }

    /// <summary>
    /// Routes a Sitemap document to <see cref="Sitemap09SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The Sitemap <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillSitemapResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        Sitemap sitemap = (Sitemap)resource;

        if (resourceMetadata.Version == new Version("0.9"))
        {
            Sitemap09SyndicationResourceAdapter adapter = new(this.Navigator, this.Settings);
            adapter.Fill(sitemap);
        }
    }

    /// <summary>
    /// Routes a Sitemap index document to the <see cref="SitemapIndex"/> overload of <see cref="Sitemap09SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="resource">The Sitemap Index <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resourceMetadata"/> is <see langword="null"/>.</exception>
    private void FillSitemapIndexResource(ISyndicationResource resource, SyndicationResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(resourceMetadata);

        SitemapIndex sitemapIndex = (SitemapIndex)resource;

        if (resourceMetadata.Version == new Version("0.9"))
        {
            Sitemap09SyndicationResourceAdapter adapter = new(this.Navigator, this.Settings);
            adapter.Fill(sitemapIndex);
        }
    }
}