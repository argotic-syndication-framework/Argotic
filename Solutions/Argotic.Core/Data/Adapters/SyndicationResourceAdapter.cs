using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Publishing;
using Argotic.Syndication;

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
///     Version routing answers every input. For the formats whose specifications define a version
///     attribute — RSS, OPML, APML and RSD — a declared version no adapter reads is refused with a
///     <see cref="FormatException"/> naming the version found and the versions read, and the comparison
///     ignores build and revision components, so a document declaring <c>2.0.1</c> reaches the 2.0
///     adapter. Atom, the Atom Publishing Protocol and BlogML define no version attribute at all, so a
///     version found on those documents is foreign markup and the namespace decides the route — RFC 4287
///     §6.3 forbids refusing a document over markup a processor does not recognise.
///     </para>
///     <para>
///     Two of the routes are deliberately many-to-one. OPML 1.0, 1.1 and 2.0 all reach
///     <see cref="Opml20SyndicationResourceAdapter"/>, because the head/body shape did not change across
///     them; an Atom feed document and a stand-alone Atom entry document both reach the adapter for their
///     version, which then picks the overload by the resource's runtime type.
///     </para>
/// </remarks>
public sealed class SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="ISyndicationResource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        : base(navigator, settings)
    {
    }

    /// <summary>
    /// Verifies that the data source is the format the caller expects, then routes it to the adapter for that format and version.
    /// </summary>
    /// <param name="resource">The <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="format">The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that the <paramref name="resource"/> is expected to conform to.</param>
    /// <remarks>
    ///     Three refusals guard the routing. The document must be the format the caller asked for; the
    ///     <paramref name="resource"/> must be of the runtime type that reads that format; and, for the
    ///     formats whose specifications define a version attribute, the version the document declares must
    ///     be one an adapter reads. A format the detection recognises but no adapter reads — NewsML,
    ///     say — is refused rather than ignored.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="format"/> is equal to <see cref="SyndicationContentFormat.None"/>, or the <paramref name="resource"/> runtime type does not read the <paramref name="format"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="resource"/> data does not conform to the specified <paramref name="format"/>, declares a version no adapter reads, or is a format this library detects but does not read.</exception>
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

                this.FillApmlResource(ResolveResource<ApmlDocument>(resource, format), resourceMetadata);
                break;

            case SyndicationContentFormat.Atom:

            // A stand-alone entry document is filled by the same adapters; what the distinct format
            // value buys is the check above, which refuses a feed handed to an AtomEntry and an
            // entry document handed to an AtomFeed. Both used to pass it and yield an empty object.
            case SyndicationContentFormat.AtomEntryDocument:

                this.FillAtomResource(resource, format, resourceMetadata);
                break;

            case SyndicationContentFormat.AtomCategoryDocument:

                this.FillAtomPublishingResource(ResolveResource<AtomCategoryDocument>(resource, format));
                break;

            case SyndicationContentFormat.AtomServiceDocument:

                this.FillAtomPublishingResource(ResolveResource<AtomServiceDocument>(resource, format));
                break;

            case SyndicationContentFormat.BlogML:

                this.FillBlogMLResource(ResolveResource<BlogMLDocument>(resource, format));
                break;

            case SyndicationContentFormat.Opml:

                this.FillOpmlResource(ResolveResource<OpmlDocument>(resource, format), resourceMetadata);
                break;

            case SyndicationContentFormat.Rsd:

                this.FillRsdResource(ResolveResource<RsdDocument>(resource, format), resourceMetadata);
                break;

            case SyndicationContentFormat.Rss:

                this.FillRssResource(ResolveResource<RssFeed>(resource, format), resourceMetadata);
                break;

            case SyndicationContentFormat.Sitemap:

                this.FillSitemapResource(ResolveResource<Sitemap>(resource, format), resourceMetadata);
                break;

            case SyndicationContentFormat.SitemapIndex:

                this.FillSitemapIndexResource(ResolveResource<SitemapIndex>(resource, format), resourceMetadata);
                break;

            default:

                // MicroSummaryGenerator, NewsML and OpenSearchDescription are detected so that the
                // mismatch message above can name them, but no adapter reads them. Falling through
                // here used to hand back an untouched resource and no diagnostic.
                throw new FormatException(
                    $"The supplied syndication resource has a content format of {format}, which this library detects but does not read.");
        }
    }

    /// <summary>
    /// Routes an Attention Profiling Markup Language (APML) document to <see cref="Apml06SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="document">The Attention Profiling Markup Language (APML) document to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="document"/>.</param>
    /// <exception cref="FormatException">The document declares a version this library does not read. APML's schema requires the version attribute.</exception>
    private void FillApmlResource(ApmlDocument document, SyndicationResourceMetadata resourceMetadata)
    {
        switch (resourceMetadata.Version)
        {
            case { Major: 0, Minor: 6 }:
                Apml06SyndicationResourceAdapter apml06Adapter = new(this.Navigator, this.Settings);
                apml06Adapter.Fill(document);
                break;

            default:
                throw UnreadDeclaredVersion(SyndicationContentFormat.Apml, resourceMetadata, "0.6");
        }
    }

    /// <summary>
    /// Routes an Atom document to <see cref="Atom10SyndicationResourceAdapter"/> or <see cref="Atom03SyndicationResourceAdapter"/>, and to the feed or entry overload by the resource's runtime type.
    /// </summary>
    /// <param name="resource">The Atom <see cref="ISyndicationResource"/> to be filled.</param>
    /// <param name="format">The Atom document shape the caller asked for, named in the refusal when <paramref name="resource"/> is neither a feed nor an entry.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="resource"/>.</param>
    /// <remarks>
    ///     Atom 1.0 and Atom 0.3 are not two versions of one grammar; they are different formats sharing a
    ///     name, with different namespaces and different element vocabularies. That is why the version test
    ///     selects between two whole adapters rather than a flag inside one — and why an unrecognised
    ///     declared version falls back to the namespace: RFC 4287 defines no version attribute on a feed
    ///     or an entry, so the value is foreign markup, and the namespace is what actually names the
    ///     format, exactly as it does when no version is declared at all.
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="resource"/> is neither an <see cref="AtomFeed"/> nor an <see cref="AtomEntry"/>.</exception>
    private void FillAtomResource(ISyndicationResource resource, SyndicationContentFormat format, SyndicationResourceMetadata resourceMetadata)
    {
        bool readsAsAtom10 = resourceMetadata.Version switch
        {
            { Major: 1, Minor: 0 } => true,
            { Major: 0, Minor: 3 } => false,
            _ => resourceMetadata.Resource is { } root
                && ((Dictionary<string, string>)root.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml)).ContainsValue(AtomUtility.AtomNamespace),
        };

        if (readsAsAtom10)
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
            else
            {
                throw WrongResourceType(resource, format, "AtomFeed or AtomEntry");
            }
        }
        else
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
            else
            {
                throw WrongResourceType(resource, format, "AtomFeed or AtomEntry");
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
    /// Routes an Atom Publishing Protocol category document to <see cref="AtomPublishing10SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="categoryDocument">The Atom Publishing Protocol category document to be filled.</param>
    /// <remarks>
    ///     No version is consulted: RFC 5023 defines no version attribute on a category or service
    ///     document, so the protocol namespace the detection has already required is the whole answer,
    ///     and a version attribute found on the document is foreign markup.
    /// </remarks>
    private void FillAtomPublishingResource(AtomCategoryDocument categoryDocument)
    {
        AtomPublishing10SyndicationResourceAdapter atomPublishing10Adapter = new(this.Navigator, this.Settings);
        atomPublishing10Adapter.Fill(categoryDocument);
    }

    /// <summary>
    /// Routes an Atom Publishing Protocol service document to <see cref="AtomPublishing10SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="serviceDocument">The Atom Publishing Protocol service document to be filled.</param>
    /// <remarks>
    ///     No version is consulted, for the same reason as the category overload.
    /// </remarks>
    private void FillAtomPublishingResource(AtomServiceDocument serviceDocument)
    {
        AtomPublishing10SyndicationResourceAdapter atomPublishing10Adapter = new(this.Navigator, this.Settings);
        atomPublishing10Adapter.Fill(serviceDocument);
    }

    /// <summary>
    /// Routes a BlogML document to <see cref="BlogML20SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="document">The BlogML document to be filled.</param>
    /// <remarks>
    ///     No version is consulted: the BlogML 2.0 schema declares no version attribute on the
    ///     <c>blog</c> root and admits no attribute wildcard — <c>BlogMLDocument.Save</c> records the
    ///     same fact from the writing side — so the dated namespace the detection has already required
    ///     names the version, and an attribute claiming otherwise is noise.
    /// </remarks>
    private void FillBlogMLResource(BlogMLDocument document)
    {
        BlogML20SyndicationResourceAdapter blogML20Adapter = new(this.Navigator, this.Settings);
        blogML20Adapter.Fill(document);
    }

    /// <summary>
    /// Routes an Outline Processor Markup Language (OPML) document of version 1.0, 1.1 or 2.0 to <see cref="Opml20SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="document">The Outline Processor Markup Language (OPML) document to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="document"/>.</param>
    /// <remarks>
    ///     Three labels, one destination. The head/body/outline shape is common to all three OPML versions,
    ///     so the version is used only to decide that the document is OPML at all. The labels are kept
    ///     apart rather than collapsed into one pattern because a version-specific adapter would slot into
    ///     one of them.
    /// </remarks>
    /// <exception cref="FormatException">The document declares a version this library does not read. OPML's specification requires the version attribute.</exception>
    private void FillOpmlResource(OpmlDocument document, SyndicationResourceMetadata resourceMetadata)
    {
        switch (resourceMetadata.Version)
        {
            case { Major: 2, Minor: 0 }:
            case { Major: 1, Minor: 1 }:
            case { Major: 1, Minor: 0 }:
                Opml20SyndicationResourceAdapter opml20Adapter = new(this.Navigator, this.Settings);
                opml20Adapter.Fill(document);
                break;

            default:
                throw UnreadDeclaredVersion(SyndicationContentFormat.Opml, resourceMetadata, "2.0, 1.1 and 1.0");
        }
    }

    /// <summary>
    /// Routes a Really Simple Discovery (RSD) document to <see cref="Rsd10SyndicationResourceAdapter"/> or <see cref="Rsd06SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="document">The Really Simple Discovery (RSD) document to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="document"/>.</param>
    /// <exception cref="FormatException">The document declares a version this library does not read.</exception>
    private void FillRsdResource(RsdDocument document, SyndicationResourceMetadata resourceMetadata)
    {
        switch (resourceMetadata.Version)
        {
            case { Major: 1, Minor: 0 }:
                Rsd10SyndicationResourceAdapter rsd10Adapter = new(this.Navigator, this.Settings);
                rsd10Adapter.Fill(document);
                break;

            case { Major: 0, Minor: 6 }:
                Rsd06SyndicationResourceAdapter rsd06Adapter = new(this.Navigator, this.Settings);
                rsd06Adapter.Fill(document);
                break;

            default:
                throw UnreadDeclaredVersion(SyndicationContentFormat.Rsd, resourceMetadata, "1.0 and 0.6");
        }
    }

    /// <summary>
    /// Routes a Really Simple Syndication (RSS) feed to the adapter for its version, of which there are five.
    /// </summary>
    /// <param name="feed">The Really Simple Syndication (RSS) feed to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="feed"/>.</param>
    /// <remarks>
    ///     The five destinations do not form a version ladder. RSS 0.90 and RSS 1.0 are RDF documents rooted
    ///     at <c>rdf:RDF</c>, in unrelated namespaces; RSS 0.91, 0.92 and 2.0 are plain XML rooted at
    ///     <c>rss</c> with no namespace at all. They share a name and a target object model, and almost
    ///     nothing else, which is why the adapters share no parsing code. Matching on major and minor
    ///     alone is deliberate: the RSS Advisory Board publishes the current specification as 2.0.11, and
    ///     a feed declaring a patch component is the same format, not an unknown one.
    /// </remarks>
    /// <exception cref="FormatException">The feed declares a version this library does not read. RSS's specification requires the version attribute.</exception>
    private void FillRssResource(RssFeed feed, SyndicationResourceMetadata resourceMetadata)
    {
        switch (resourceMetadata.Version)
        {
            case { Major: 2, Minor: 0 }:
                Rss20SyndicationResourceAdapter rss20Adapter = new(this.Navigator, this.Settings);
                rss20Adapter.Fill(feed);
                break;

            case { Major: 1, Minor: 0 }:
                Rss10SyndicationResourceAdapter rss10Adapter = new(this.Navigator, this.Settings);
                rss10Adapter.Fill(feed);
                break;

            case { Major: 0, Minor: 92 }:
                Rss092SyndicationResourceAdapter rss092Adapter = new(this.Navigator, this.Settings);
                rss092Adapter.Fill(feed);
                break;

            case { Major: 0, Minor: 91 }:
                Rss091SyndicationResourceAdapter rss091Adapter = new(this.Navigator, this.Settings);
                rss091Adapter.Fill(feed);
                break;

            case { Major: 0, Minor: 9 }:
                Rss090SyndicationResourceAdapter rss090Adapter = new(this.Navigator, this.Settings);
                rss090Adapter.Fill(feed);
                break;

            default:
                throw UnreadDeclaredVersion(SyndicationContentFormat.Rss, resourceMetadata, "2.0, 1.0, 0.92, 0.91 and 0.9");
        }
    }

    /// <summary>
    /// Routes a Sitemap document to <see cref="Sitemap09SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="sitemap">The Sitemap to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="sitemap"/>.</param>
    /// <exception cref="FormatException">The document declares a version this library does not read. Unreachable today — detection assigns 0.9 from the namespace and never reads an attribute — and kept so that a detector change fails loud rather than silent.</exception>
    private void FillSitemapResource(Sitemap sitemap, SyndicationResourceMetadata resourceMetadata)
    {
        switch (resourceMetadata.Version)
        {
            case { Major: 0, Minor: 9 }:
                Sitemap09SyndicationResourceAdapter adapter = new(this.Navigator, this.Settings);
                adapter.Fill(sitemap);
                break;

            default:
                throw UnreadDeclaredVersion(SyndicationContentFormat.Sitemap, resourceMetadata, "0.9");
        }
    }

    /// <summary>
    /// Routes a Sitemap index document to the <see cref="SitemapIndex"/> overload of <see cref="Sitemap09SyndicationResourceAdapter"/>.
    /// </summary>
    /// <param name="sitemapIndex">The Sitemap index to be filled.</param>
    /// <param name="resourceMetadata">A <see cref="SyndicationResourceMetadata"/> object that represents the meta-data describing the <paramref name="sitemapIndex"/>.</param>
    /// <exception cref="FormatException">The document declares a version this library does not read. Unreachable today, for the same reason as the Sitemap overload.</exception>
    private void FillSitemapIndexResource(SitemapIndex sitemapIndex, SyndicationResourceMetadata resourceMetadata)
    {
        switch (resourceMetadata.Version)
        {
            case { Major: 0, Minor: 9 }:
                Sitemap09SyndicationResourceAdapter adapter = new(this.Navigator, this.Settings);
                adapter.Fill(sitemapIndex);
                break;

            default:
                throw UnreadDeclaredVersion(SyndicationContentFormat.SitemapIndex, resourceMetadata, "0.9");
        }
    }

    /// <summary>
    /// Creates the refusal for a resource whose runtime type does not read the requested format.
    /// </summary>
    /// <param name="resource">The resource of the wrong runtime type.</param>
    /// <param name="format">The format the caller asked for.</param>
    /// <param name="expected">The name of the resource type — or types — that read <paramref name="format"/>.</param>
    /// <returns>The <see cref="ArgumentException"/> to throw.</returns>
    private static ArgumentException WrongResourceType(ISyndicationResource resource, SyndicationContentFormat format, string expected) =>
        new($"The supplied resource is a {resource.GetType().Name}, which does not read {format} documents; {format} is read by {expected}.", nameof(resource));

    /// <summary>
    /// Returns <paramref name="resource"/> as <typeparamref name="TResource"/>, or refuses it by naming both types.
    /// </summary>
    /// <typeparam name="TResource">The resource type that reads <paramref name="format"/>.</typeparam>
    /// <param name="resource">The resource the caller supplied.</param>
    /// <param name="format">The format the caller asked for.</param>
    /// <returns>The <paramref name="resource"/>, typed.</returns>
    /// <exception cref="ArgumentException">The <paramref name="resource"/> is not a <typeparamref name="TResource"/>.</exception>
    private static TResource ResolveResource<TResource>(ISyndicationResource resource, SyndicationContentFormat format)
        where TResource : class, ISyndicationResource =>
        resource as TResource ?? throw WrongResourceType(resource, format, typeof(TResource).Name);

    /// <summary>
    /// Creates the refusal for a document declaring a version no adapter reads.
    /// </summary>
    /// <param name="format">The format the document conforms to.</param>
    /// <param name="resourceMetadata">The meta-data carrying the version the document declared.</param>
    /// <param name="versionsRead">The versions the library reads for <paramref name="format"/>, as prose.</param>
    /// <returns>The <see cref="FormatException"/> to throw.</returns>
    private static FormatException UnreadDeclaredVersion(SyndicationContentFormat format, SyndicationResourceMetadata resourceMetadata, string versionsRead) =>
        new($"The supplied document declares {format} version {resourceMetadata.Version?.ToString() ?? "none"}, which this library does not read; the versions read are {versionsRead}.");
}