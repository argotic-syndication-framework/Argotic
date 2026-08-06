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
///         The <see cref="Atom10SyndicationResourceAdapter"/> serves as a bridge between an <see cref="AtomFeed"/> or <see cref="AtomEntry"/> and an XML data source.
///         The <see cref="Atom10SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(AtomFeed)"/> or <see cref="Fill(AtomEntry)"/>, which changes the data
///         in the <see cref="AtomFeed"/> or <see cref="AtomEntry"/> to match the data in the data source.
///     </para>
///     <para>
///         This syndication resource adapter is designed to fill <see cref="AtomFeed"/> or <see cref="AtomEntry"/> objects using
///         a <see cref="XPathNavigator"/> that represents XML data that conforms to the Atom 1.0 specification.</para>
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public Atom10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Modifies the <see cref="AtomEntry"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="AtomEntry"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
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
    /// Modifies the <see cref="AtomFeed"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="AtomFeed"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
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
    /// Modifies the <see cref="AtomEntry"/> to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
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
    /// Modifies the <see cref="AtomEntry"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
    /// Modifies the <see cref="AtomEntry"/> optional entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
    /// Modifies the <see cref="AtomFeed"/> collection entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
            int counter = 0;
            while (entryIterator.MoveNext())
            {
                XPathNavigator? entryNode = entryIterator.Current;
                if (entryNode is null)
                {
                    continue;
                }

                AtomEntry entry = new();
                counter++;

                Atom10SyndicationResourceAdapter.FillEntry(entry, entryNode, manager, settings);

                if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                {
                    break;
                }

                feed.Entries.Add(entry);
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
    /// Modifies the <see cref="AtomFeed"/> optional entities to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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