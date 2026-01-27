using System.Collections.ObjectModel;
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
///         The <see cref="Atom03SyndicationResourceAdapter"/> serves as a bridge between an <see cref="AtomFeed"/> or <see cref="AtomEntry"/> and an XML data source.
///         The <see cref="Atom03SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(AtomFeed)"/> or <see cref="Fill(AtomEntry)"/>, which changes the data
///         in the <see cref="AtomFeed"/> or <see cref="AtomEntry"/> to match the data in the data source.
///     </para>
///     <para>
///         This syndication resource adapter is designed to fill <see cref="AtomFeed"/> or <see cref="AtomEntry"/> objects using
///         a <see cref="XPathNavigator"/> that represents XML data that conforms to the Atom 0.3 specification.</para>
/// </remarks>
public class Atom03SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Atom03SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="AtomFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public Atom03SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Atom syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference.</exception>
    protected static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);

        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("atom", !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : "http://purl.org/atom/ns#");
        manager.AddNamespace("xhtml", AtomUtility.XhtmlNamespace);

        return manager;
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

        XPathNavigator entryNavigator = this.Navigator.SelectSingleNode("atom:entry", manager);

        if (entryNavigator != null)
        {
            Atom03SyndicationResourceAdapter.FillEntry(resource, entryNavigator, manager, this.Settings);
        }
    }

    /// <summary>
    /// Modifies the <see cref="AtomFeed"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="AtomFeed"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    public void Fill(AtomFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = Atom03SyndicationResourceAdapter.CreateNamespaceManager(this.Navigator.NameTable);

        XPathNavigator feedNavigator = this.Navigator.SelectSingleNode("atom:feed", manager);

        if (feedNavigator != null)
        {
            AtomUtility.FillCommonObjectAttributes(resource, feedNavigator);

            XPathNavigator idNavigator = feedNavigator.SelectSingleNode("atom:id", manager);
            XPathNavigator titleNavigator = feedNavigator.SelectSingleNode("atom:title", manager);
            XPathNavigator modifiedNavigator = feedNavigator.SelectSingleNode("atom:modified", manager);

            if (idNavigator != null)
            {
                resource.Id = new();
                resource.Id.Load(idNavigator, this.Settings);
            }

            if (titleNavigator != null)
            {
                resource.Title = Atom03SyndicationResourceAdapter.CreateTextContent(titleNavigator, manager, this.Settings);
            }

            if (modifiedNavigator != null)
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out DateTime updatedOn))
                {
                    resource.UpdatedOn = updatedOn;
                }
            }

            Atom03SyndicationResourceAdapter.FillFeedOptionals(resource, feedNavigator, manager, this.Settings);
            Atom03SyndicationResourceAdapter.FillFeedCollections(resource, feedNavigator, manager, this.Settings);

            SyndicationExtensionAdapter adapter = new(feedNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Creates a <see cref="AtomContent"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomContent"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 Content construct.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static AtomContent CreateContent(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        AtomContent content = new();
        string modeAttribute = string.Empty;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(content, source);

        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            modeAttribute = source.GetAttribute("mode", string.Empty);

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                content.ContentType = typeAttribute;
            }
        }

        if (string.Equals(modeAttribute, "xml", StringComparison.OrdinalIgnoreCase))
        {
            XPathNavigator xhtmlDivNavigator = source.SelectSingleNode("xhtml:div", manager);
            if (xhtmlDivNavigator != null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                content.Content = xhtmlDivNavigator.Value;
            }
            else if (!string.IsNullOrEmpty(source.Value))
            {
                content.Content = source.Value;
            }
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            content.Content = source.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(content, manager);

        return content;
    }

    /// <summary>
    /// Creates a <see cref="AtomGenerator"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomGenerator"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 generator element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static AtomGenerator CreateGenerator(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        AtomGenerator generator = new();

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(generator, source);

        if (source.HasAttributes)
        {
            string urlAttribute = source.GetAttribute("url", string.Empty);
            string versionAttribute = source.GetAttribute("version", string.Empty);

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    generator.Uri = uri;
                }
            }

            if (!string.IsNullOrEmpty(versionAttribute))
            {
                generator.Version = versionAttribute;
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            generator.Content = source.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(generator, manager);

        return generator;
    }

    /// <summary>
    /// Creates a <see cref="AtomPersonConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomPersonConstruct"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 Person construct.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static AtomPersonConstruct CreatePerson(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        AtomPersonConstruct person = new();

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(person, source);

        XPathNavigator nameNavigator = source.SelectSingleNode("atom:name", manager);
        XPathNavigator urlNavigator = source.SelectSingleNode("atom:url", manager);
        XPathNavigator emailNavigator = source.SelectSingleNode("atom:email", manager);

        if (nameNavigator != null)
        {
            person.Name = nameNavigator.Value;
        }

        if (urlNavigator != null)
        {
            if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                person.Uri = uri;
            }
        }

        if (emailNavigator != null)
        {
            person.EmailAddress = emailNavigator.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(person, manager);

        return person;
    }

    /// <summary>
    /// Creates a <see cref="AtomTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <returns>A <see cref="AtomTextConstruct"/> instance initialized using the supplied <paramref name="source"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a Atom 0.3 Content construct.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static AtomTextConstruct CreateTextContent(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        AtomTextConstruct content = new();

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(content, source);

        if (source.HasAttributes)
        {
            string modeAttribute = source.GetAttribute("mode", string.Empty);
            if (!string.IsNullOrEmpty(modeAttribute))
            {
                if (string.Equals(modeAttribute, "base64", StringComparison.OrdinalIgnoreCase))
                {
                    content.TextType = AtomTextConstructType.Text;
                }
                else if (string.Equals(modeAttribute, "escaped", StringComparison.OrdinalIgnoreCase))
                {
                    content.TextType = AtomTextConstructType.Html;
                }
                else if (string.Equals(modeAttribute, "xml", StringComparison.OrdinalIgnoreCase))
                {
                    content.TextType = AtomTextConstructType.Xhtml;
                }
                else
                {
                    content.TextType = AtomTextConstructType.Text;
                }
            }
        }

        if (content.TextType == AtomTextConstructType.Xhtml)
        {
            XPathNavigator xhtmlDivNavigator = source.SelectSingleNode("xhtml:div", manager);
            if (xhtmlDivNavigator != null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                content.Content = xhtmlDivNavigator.Value;
            }
            else if (!string.IsNullOrEmpty(source.Value))
            {
                content.Content = source.Value;
            }
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            content.Content = source.Value;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(content, manager);

        return content;
    }

    /// <summary>
    /// Modifies the <see cref="AtomEntry"/> to match the supplied <see cref="XPathNavigator"/> data source.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents an Atom 0.3 element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static void FillEntry(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        AtomUtility.FillCommonObjectAttributes(entry, source);

        XPathNavigator idNavigator = source.SelectSingleNode("atom:id", manager);
        XPathNavigator titleNavigator = source.SelectSingleNode("atom:title", manager);
        XPathNavigator modifiedNavigator = source.SelectSingleNode("atom:modified", manager);

        if (idNavigator != null)
        {
            entry.Id = new();
            entry.Id.Load(idNavigator, settings);
        }

        if (titleNavigator != null)
        {
            entry.Title = Atom03SyndicationResourceAdapter.CreateTextContent(titleNavigator, manager, settings);
        }

        if (modifiedNavigator != null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out DateTime updatedOn))
            {
                entry.UpdatedOn = updatedOn;
            }
        }

        Atom03SyndicationResourceAdapter.FillEntryOptionals(entry, source, manager, settings);
        Atom03SyndicationResourceAdapter.FillEntryCollections(entry, source, manager, settings);

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
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents an Atom 0.3 element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static void FillEntryCollections(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorIterator = source.Select("atom:author", manager);
        XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
        XPathNodeIterator linkIterator = source.Select("atom:link", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                AtomPersonConstruct author = Atom03SyndicationResourceAdapter.CreatePerson(authorIterator.Current, manager, settings);
                entry.Authors.Add(author);
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                AtomPersonConstruct contributor = Atom03SyndicationResourceAdapter.CreatePerson(contributorIterator.Current, manager, settings);
                entry.Contributors.Add(contributor);
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                AtomLink link = new();
                if (link.Load(linkIterator.Current, settings))
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
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents an Atom 0.3 element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static void FillEntryOptionals(AtomEntry entry, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator contentNavigator = source.SelectSingleNode("atom:content", manager);
        XPathNavigator createdNavigator = source.SelectSingleNode("atom:created", manager);
        XPathNavigator summaryNavigator = source.SelectSingleNode("atom:summary", manager);

        if (contentNavigator != null)
        {
            entry.Content = Atom03SyndicationResourceAdapter.CreateContent(contentNavigator, manager, settings);
        }

        if (createdNavigator != null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(createdNavigator.Value, out DateTime publishedOn))
            {
                entry.PublishedOn = publishedOn;
            }
        }

        if (summaryNavigator != null)
        {
            entry.Summary = Atom03SyndicationResourceAdapter.CreateTextContent(summaryNavigator, manager, settings);
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
    private static void FillFeedCollections(AtomFeed feed, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator authorIterator = source.Select("atom:author", manager);
        XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
        XPathNodeIterator linkIterator = source.Select("atom:link", manager);
        XPathNodeIterator entryIterator = source.Select("atom:entry", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                AtomPersonConstruct author = Atom03SyndicationResourceAdapter.CreatePerson(authorIterator.Current, manager, settings);
                feed.Authors.Add(author);
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                AtomPersonConstruct contributor = Atom03SyndicationResourceAdapter.CreatePerson(contributorIterator.Current, manager, settings);
                feed.Contributors.Add(contributor);
            }
        }

        if (entryIterator is { Count: > 0 })
        {
            int counter = 0;
            while (entryIterator.MoveNext())
            {
                AtomEntry entry = new();
                counter++;

                Atom03SyndicationResourceAdapter.FillEntry(entry, entryIterator.Current, manager, settings);

                if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                {
                    break;
                }

                ((Collection<AtomEntry>)feed.Entries).Add(entry);
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                AtomLink link = new();
                if (link.Load(linkIterator.Current, settings))
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
    private static void FillFeedOptionals(AtomFeed feed, XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(feed);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator generatorNavigator = source.SelectSingleNode("atom:generator", manager);
        XPathNavigator copyrightNavigator = source.SelectSingleNode("atom:copyright", manager);
        XPathNavigator taglineNavigator = source.SelectSingleNode("atom:tagline", manager);

        if (generatorNavigator != null)
        {
            feed.Generator = Atom03SyndicationResourceAdapter.CreateGenerator(generatorNavigator, manager, settings);
        }

        if (copyrightNavigator != null)
        {
            feed.Rights = Atom03SyndicationResourceAdapter.CreateTextContent(copyrightNavigator, manager, settings);
        }

        if (taglineNavigator != null)
        {
            feed.Subtitle = Atom03SyndicationResourceAdapter.CreateTextContent(taglineNavigator, manager, settings);
        }
    }
}