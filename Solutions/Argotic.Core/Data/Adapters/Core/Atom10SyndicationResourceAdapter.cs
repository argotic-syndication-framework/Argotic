using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill an <see cref="AtomFeed"/> or <see cref="AtomEntry"/>.
/// </summary>
/// <remarks>
///     <para>
///     Reads the Atom 1.0 of RFC 4287 — namespace <c>http://www.w3.org/2005/Atom</c> — and only that.
///     Atom 0.3 documents are a different format with a different namespace and are read by
///     <see cref="Atom03SyndicationResourceAdapter"/>; nothing here falls back to it.
///     </para>
///     <para>
///     RFC 4287 defines two document types, and this adapter has one overload for each. Handed the wrong
///     one, <see cref="Fill(AtomFeed)"/> and <see cref="Fill(AtomEntry)"/> throw
///     <see cref="FormatException"/> rather than returning an empty object, because both roots report the
///     same content format and the mismatch cannot be caught upstream.
///     </para>
///     <para>
///     The three elements RFC 4287 §4.1.1 requires of a feed — <c>id</c>, <c>title</c>, <c>updated</c> — are
///     read where present and never insisted on. A feed missing all three loads successfully with those
///     properties left null. Conformance is not this layer's job; parsing what publishers actually emit is.
///     Dates go through <c>TryParseRfc3339DateTime</c>, and a value that fails to parse leaves the property
///     at its default rather than failing the load.
///     </para>
///     <para>
///     The walk is split three ways — the required elements, then <c>*Optionals</c> for the single-valued
///     optional ones, then <c>*Collections</c> for the repeatable ones — and split again between feed and
///     entry. The <c>id</c>/<c>title</c>/<c>updated</c> block is therefore written out twice, once in
///     <see cref="Fill(AtomFeed)"/> and once in <c>FillEntry</c>. They are not shared, and a correction to
///     one is not a correction to the other.
///     </para>
/// </remarks>
public class Atom10SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Atom10SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Atom10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Fills the entry from the <c>entry</c> root of a stand-alone Atom entry document.
    /// </summary>
    /// <param name="resource">The <see cref="AtomEntry"/> to be filled.</param>
    /// <remarks>
    ///     For the <c>entry</c> elements <i>inside</i> a feed document, see <see cref="Fill(AtomFeed)"/>;
    ///     both routes converge on the same private walk, so a stand-alone entry and an in-feed entry are
    ///     read identically.
    /// </remarks>
    /// <exception cref="FormatException">The document has no <c>entry</c> root — most often because it is a feed document, which <see cref="Fill(AtomFeed)"/> reads.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(AtomEntry resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? entryNavigator = this.Navigator.SelectChildElement("atom", "entry", manager);

        if (entryNavigator is null)
        {
            throw new FormatException(AtomUtility.WrongDocumentShape("entry", "feed"));
        }

        Atom10SyndicationResourceAdapter.FillEntry(resource, entryNavigator, manager, this.Settings);
    }

    /// <summary>
    /// Fills the feed from the <c>feed</c> root: its common attributes, its <c>id</c>, <c>title</c> and <c>updated</c>, then its optional elements, its collections including every entry, and its syndication extensions.
    /// </summary>
    /// <param name="resource">The <see cref="AtomFeed"/> to be filled.</param>
    /// <exception cref="FormatException">The document has no <c>feed</c> root — most often because it is a stand-alone entry document, which <see cref="Fill(AtomEntry)"/> reads.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(AtomFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator? feedNavigator = this.Navigator.SelectChildElement("atom", "feed", manager);

        if (feedNavigator is null)
        {
            throw new FormatException(AtomUtility.WrongDocumentShape("feed", "entry"));
        }

        AtomUtility.FillCommonObjectAttributes(resource, feedNavigator);

        XPathNavigator? idNavigator = feedNavigator.SelectChildElement("atom", "id", manager);
        XPathNavigator? titleNavigator = feedNavigator.SelectChildElement("atom", "title", manager);
        XPathNavigator? updatedNavigator = feedNavigator.SelectChildElement("atom", "updated", manager);

        if (idNavigator is not null)
        {
            resource.Id = new AtomId();
            resource.Id.Load(idNavigator, this.Settings);
        }

        if (titleNavigator is not null)
        {
            resource.Title = new AtomTextConstruct();
            resource.Title.Load(titleNavigator, this.Settings);
        }

        if (updatedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedNavigator.Value, out DateTime updatedOn))
            {
                resource.UpdatedOn = updatedOn;
            }
        }

        Atom10SyndicationResourceAdapter.FillFeedOptionals(resource, feedNavigator, manager, this.Settings);
        Atom10SyndicationResourceAdapter.FillFeedCollections(resource, feedNavigator, manager, this.Settings);

        SyndicationExtensionAdapter adapter = new(feedNavigator, this.Settings);
        adapter.Fill(resource, manager);
    }

    /// <summary>
    /// Reads one <c>entry</c> subtree in full: common attributes, <c>id</c>, <c>title</c>, <c>updated</c>, the optional elements, the collections, and the entry's own syndication extensions.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    private static void FillEntry(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(entry, source);

        XPathNavigator? idNavigator = source.SelectChildElement("atom", "id", manager);
        XPathNavigator? titleNavigator = source.SelectChildElement("atom", "title", manager);
        XPathNavigator? updatedNavigator = source.SelectChildElement("atom", "updated", manager);

        if (idNavigator is not null)
        {
            entry.Id = new AtomId();
            entry.Id.Load(idNavigator, settings);
        }

        if (titleNavigator is not null)
        {
            entry.Title = new AtomTextConstruct();
            entry.Title.Load(titleNavigator, settings);
        }

        if (updatedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedNavigator.Value, out DateTime updatedOn))
            {
                entry.UpdatedOn = updatedOn;
            }
        }

        Atom10SyndicationResourceAdapter.FillEntryOptionals(entry, source, manager, settings);
        Atom10SyndicationResourceAdapter.FillEntryCollections(entry, source, manager, settings);

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(entry, manager);
    }

    /// <summary>
    /// Reads the repeatable children of an <c>entry</c> — <c>author</c>, <c>category</c>, <c>contributor</c> and <c>link</c>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillEntryCollections(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorIterator = source.SelectChildElements("atom", "author", manager);
        XPathNodeIterator categoryIterator = source.SelectChildElements("atom", "category", manager);
        XPathNodeIterator contributorIterator = source.SelectChildElements("atom", "contributor", manager);
        XPathNodeIterator linkIterator = source.SelectChildElements("atom", "link", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                XPathNavigator? authorNode = authorIterator.Current;
                if (authorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct author = new();
                if (author.Load(authorNode, settings))
                {
                    entry.Authors.Add(author);
                }
            }
        }

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
                if (category.Load(categoryNode, settings))
                {
                    entry.Categories.Add(category);
                }
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                XPathNavigator? contributorNode = contributorIterator.Current;
                if (contributorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct contributor = new();
                if (contributor.Load(contributorNode, settings))
                {
                    entry.Contributors.Add(contributor);
                }
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                XPathNavigator? linkNode = linkIterator.Current;
                if (linkNode is null)
                {
                    continue;
                }

                AtomLink link = new();
                if (link.Load(linkNode, settings))
                {
                    entry.Links.Add(link);
                }
            }
        }
    }

    /// <summary>
    /// Reads the single-valued optional children of an <c>entry</c> — <c>content</c>, <c>published</c>, <c>rights</c>, <c>source</c> and <c>summary</c>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillEntryOptionals(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? contentNavigator = source.SelectChildElement("atom", "content", manager);
        XPathNavigator? publishedNavigator = source.SelectChildElement("atom", "published", manager);
        XPathNavigator? rightsNavigator = source.SelectChildElement("atom", "rights", manager);
        XPathNavigator? sourceNavigator = source.SelectChildElement("atom", "source", manager);
        XPathNavigator? summaryNavigator = source.SelectChildElement("atom", "summary", manager);

        if (contentNavigator is not null)
        {
            entry.Content = new AtomContent();
            entry.Content.Load(contentNavigator, settings);
        }

        if (publishedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(publishedNavigator.Value, out DateTime publishedOn))
            {
                entry.PublishedOn = publishedOn;
            }
        }

        if (rightsNavigator is not null)
        {
            entry.Rights = new AtomTextConstruct();
            entry.Rights.Load(rightsNavigator, settings);
        }

        if (sourceNavigator is not null)
        {
            entry.Source = new AtomSource();
            entry.Source.Load(sourceNavigator, settings);
        }

        if (summaryNavigator is not null)
        {
            entry.Summary = new AtomTextConstruct();
            entry.Summary.Load(summaryNavigator, settings);
        }
    }

    /// <summary>
    /// Reads the repeatable children of a <c>feed</c> — <c>author</c>, <c>category</c>, <c>contributor</c>, <c>link</c> and <c>entry</c> — recursing into each entry's own subtree and extensions.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    ///     </para>
    ///     <para>
    ///     This is where a parse spends its time: every entry is walked in full and probed for every
    ///     supported extension. <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> is tested at
    ///     the top of the loop, against the number of entries <i>kept</i>, so capping a 500-entry feed at
    ///     10 costs 10 entry parses.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillFeedCollections(AtomFeed feed, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorIterator = source.SelectChildElements("atom", "author", manager);
        XPathNodeIterator categoryIterator = source.SelectChildElements("atom", "category", manager);
        XPathNodeIterator contributorIterator = source.SelectChildElements("atom", "contributor", manager);
        XPathNodeIterator linkIterator = source.SelectChildElements("atom", "link", manager);
        XPathNodeIterator entryIterator = source.SelectChildElements("atom", "entry", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                XPathNavigator? authorNode = authorIterator.Current;
                if (authorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct author = new();
                if (author.Load(authorNode, settings))
                {
                    feed.Authors.Add(author);
                }
            }
        }

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
                if (category.Load(categoryNode, settings))
                {
                    feed.Categories.Add(category);
                }
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                XPathNavigator? contributorNode = contributorIterator.Current;
                if (contributorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct contributor = new();
                if (contributor.Load(contributorNode, settings))
                {
                    feed.Contributors.Add(contributor);
                }
            }
        }

        if (entryIterator is { Count: > 0 })
        {
            int added = 0;
            while (entryIterator.MoveNext())
            {
                if (settings.RetrievalLimit != 0 && added >= settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? entryNode = entryIterator.Current;
                if (entryNode is null)
                {
                    continue;
                }

                AtomEntry entry = new();
                Atom10SyndicationResourceAdapter.FillEntry(entry, entryNode, manager, settings);

                feed.Entries.Add(entry);
                added++;
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                XPathNavigator? linkNode = linkIterator.Current;
                if (linkNode is null)
                {
                    continue;
                }

                AtomLink link = new();
                if (link.Load(linkNode, settings))
                {
                    feed.Links.Add(link);
                }
            }
        }
    }

    /// <summary>
    /// Reads the single-valued optional children of a <c>feed</c> — <c>generator</c>, <c>icon</c>, <c>logo</c>, <c>rights</c> and <c>subtitle</c>.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillFeedOptionals(AtomFeed feed, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? generatorNavigator = source.SelectChildElement("atom", "generator", manager);
        XPathNavigator? iconNavigator = source.SelectChildElement("atom", "icon", manager);
        XPathNavigator? logoNavigator = source.SelectChildElement("atom", "logo", manager);
        XPathNavigator? rightsNavigator = source.SelectChildElement("atom", "rights", manager);
        XPathNavigator? subtitleNavigator = source.SelectChildElement("atom", "subtitle", manager);

        if (generatorNavigator is not null)
        {
            feed.Generator = new AtomGenerator();
            feed.Generator.Load(generatorNavigator, settings);
        }

        if (iconNavigator is not null)
        {
            feed.Icon = new AtomIcon();
            feed.Icon.Load(iconNavigator, settings);
        }

        if (logoNavigator is not null)
        {
            feed.Logo = new AtomLogo();
            feed.Logo.Load(logoNavigator, settings);
        }

        if (rightsNavigator is not null)
        {
            feed.Rights = new AtomTextConstruct();
            feed.Rights.Load(rightsNavigator, settings);
        }

        if (subtitleNavigator is not null)
        {
            feed.Subtitle = new AtomTextConstruct();
            feed.Subtitle.Load(subtitleNavigator, settings);
        }
    }
}