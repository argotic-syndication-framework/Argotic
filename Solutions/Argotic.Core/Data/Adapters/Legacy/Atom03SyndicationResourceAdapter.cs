namespace Argotic.Data.Adapters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;
    using Argotic.Syndication;

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
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Atom03SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
            : base(navigator, settings)
        {
        }

        /// <summary>
        /// Modifies the <see cref="AtomEntry"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="AtomEntry"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(AtomEntry resource)
        {
            Guard.ArgumentNotNull(resource, "resource");

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

            XPathNavigator entryNavigator = this.Navigator.SelectSingleNode("atom:entry", manager);

            if (entryNavigator != null)
            {
                FillEntry(resource, entryNavigator, manager, this.Settings);
            }
        }

        /// <summary>
        /// Modifies the <see cref="AtomFeed"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="AtomFeed"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(AtomFeed resource)
        {
            Guard.ArgumentNotNull(resource, "resource");

            XmlNamespaceManager manager = CreateNamespaceManager(this.Navigator.NameTable);

            XPathNavigator feedNavigator = this.Navigator.SelectSingleNode("atom:feed", manager);

            if (feedNavigator != null)
            {
                AtomUtility.FillCommonObjectAttributes(resource, feedNavigator);

                XPathNavigator idNavigator = feedNavigator.SelectSingleNode("atom:id", manager);
                XPathNavigator titleNavigator = feedNavigator.SelectSingleNode("atom:title", manager);
                XPathNavigator modifiedNavigator = feedNavigator.SelectSingleNode("atom:modified", manager);

                if (idNavigator != null)
                {
                    resource.Id = new AtomId();
                    resource.Id.Load(idNavigator, this.Settings);
                }

                if (titleNavigator != null)
                {
                    resource.Title = CreateTextContent(titleNavigator, manager, this.Settings);
                }

                if (modifiedNavigator != null)
                {
                    DateTime updatedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out updatedOn))
                    {
                        resource.UpdatedOn = updatedOn;
                    }
                }

                FillFeedOptionals(resource, feedNavigator, manager, this.Settings);
                FillFeedCollections(resource, feedNavigator, manager, this.Settings);

                SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(feedNavigator, this.Settings);
                adapter.Fill(resource, manager);
            }
        }

        /// <summary>
        /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Atom syndication entities.
        /// </summary>
        /// <param name="nameTable">The table of atomized string objects.</param>
        /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference (Nothing in Visual Basic).</exception>
        protected static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
        {
            XmlNamespaceManager manager = null;

            Guard.ArgumentNotNull(nameTable, "nameTable");

            manager = new XmlNamespaceManager(nameTable);
            manager.AddNamespace(
                "atom",
                !string.IsNullOrEmpty(manager.DefaultNamespace) ? manager.DefaultNamespace : "http://purl.org/atom/ns#");
            manager.AddNamespace("xhtml", AtomUtility.XhtmlNamespace);

            return manager;
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static AtomContent CreateContent(
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            AtomContent content = new AtomContent();
            string modeAttribute = string.Empty;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

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

            if (string.Compare(modeAttribute, "xml", StringComparison.OrdinalIgnoreCase) == 0)
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

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static AtomGenerator CreateGenerator(
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            AtomGenerator generator = new AtomGenerator();

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            AtomUtility.FillCommonObjectAttributes(generator, source);

            if (source.HasAttributes)
            {
                string urlAttribute = source.GetAttribute("url", string.Empty);
                string versionAttribute = source.GetAttribute("version", string.Empty);

                if (!string.IsNullOrEmpty(urlAttribute))
                {
                    Uri uri;
                    if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out uri))
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

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static AtomPersonConstruct CreatePerson(
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            AtomPersonConstruct person = new AtomPersonConstruct();

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

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
                Uri uri;
                if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out uri))
                {
                    person.Uri = uri;
                }
            }

            if (emailNavigator != null)
            {
                person.EmailAddress = emailNavigator.Value;
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static AtomTextConstruct CreateTextContent(
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            AtomTextConstruct content = new AtomTextConstruct();

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            AtomUtility.FillCommonObjectAttributes(content, source);

            if (source.HasAttributes)
            {
                string modeAttribute = source.GetAttribute("mode", string.Empty);
                if (!string.IsNullOrEmpty(modeAttribute))
                {
                    if (string.Compare(modeAttribute, "base64", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        content.TextType = AtomTextConstructType.Text;
                    }
                    else if (string.Compare(modeAttribute, "escaped", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        content.TextType = AtomTextConstructType.Html;
                    }
                    else if (string.Compare(modeAttribute, "xml", StringComparison.OrdinalIgnoreCase) == 0)
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

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
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
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillEntry(
            AtomEntry entry,
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(entry, "entry");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            AtomUtility.FillCommonObjectAttributes(entry, source);

            XPathNavigator idNavigator = source.SelectSingleNode("atom:id", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("atom:title", manager);
            XPathNavigator modifiedNavigator = source.SelectSingleNode("atom:modified", manager);

            if (idNavigator != null)
            {
                entry.Id = new AtomId();
                entry.Id.Load(idNavigator, settings);
            }

            if (titleNavigator != null)
            {
                entry.Title = CreateTextContent(titleNavigator, manager, settings);
            }

            if (modifiedNavigator != null)
            {
                DateTime updatedOn;
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out updatedOn))
                {
                    entry.UpdatedOn = updatedOn;
                }
            }

            FillEntryOptionals(entry, source, manager, settings);
            FillEntryCollections(entry, source, manager, settings);

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
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
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillEntryCollections(
            AtomEntry entry,
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(entry, "entry");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNodeIterator authorIterator = source.Select("atom:author", manager);
            XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
            XPathNodeIterator linkIterator = source.Select("atom:link", manager);

            if (authorIterator != null && authorIterator.Count > 0)
            {
                while (authorIterator.MoveNext())
                {
                    AtomPersonConstruct author = CreatePerson(authorIterator.Current, manager, settings);
                    entry.Authors.Add(author);
                }
            }

            if (contributorIterator != null && contributorIterator.Count > 0)
            {
                while (contributorIterator.MoveNext())
                {
                    AtomPersonConstruct contributor = CreatePerson(contributorIterator.Current, manager, settings);
                    entry.Contributors.Add(contributor);
                }
            }

            if (linkIterator != null && linkIterator.Count > 0)
            {
                while (linkIterator.MoveNext())
                {
                    AtomLink link = new AtomLink();
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
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillEntryOptionals(
            AtomEntry entry,
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(entry, "entry");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator contentNavigator = source.SelectSingleNode("atom:content", manager);
            XPathNavigator createdNavigator = source.SelectSingleNode("atom:created", manager);
            XPathNavigator summaryNavigator = source.SelectSingleNode("atom:summary", manager);

            if (contentNavigator != null)
            {
                entry.Content = CreateContent(contentNavigator, manager, settings);
            }

            if (createdNavigator != null)
            {
                DateTime publishedOn;
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(createdNavigator.Value, out publishedOn))
                {
                    entry.PublishedOn = publishedOn;
                }
            }

            if (summaryNavigator != null)
            {
                entry.Summary = CreateTextContent(summaryNavigator, manager, settings);
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
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillFeedCollections(
            AtomFeed feed,
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(feed, "feed");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNodeIterator authorIterator = source.Select("atom:author", manager);
            XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
            XPathNodeIterator linkIterator = source.Select("atom:link", manager);
            XPathNodeIterator entryIterator = source.Select("atom:entry", manager);

            if (authorIterator != null && authorIterator.Count > 0)
            {
                while (authorIterator.MoveNext())
                {
                    AtomPersonConstruct author = CreatePerson(authorIterator.Current, manager, settings);
                    feed.Authors.Add(author);
                }
            }

            if (contributorIterator != null && contributorIterator.Count > 0)
            {
                while (contributorIterator.MoveNext())
                {
                    AtomPersonConstruct contributor = CreatePerson(contributorIterator.Current, manager, settings);
                    feed.Contributors.Add(contributor);
                }
            }

            if (entryIterator != null && entryIterator.Count > 0)
            {
                int counter = 0;
                while (entryIterator.MoveNext())
                {
                    AtomEntry entry = new AtomEntry();
                    counter++;

                    FillEntry(entry, entryIterator.Current, manager, settings);

                    if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                    {
                        break;
                    }

                    ((Collection<AtomEntry>)feed.Entries).Add(entry);
                }
            }

            if (linkIterator != null && linkIterator.Count > 0)
            {
                while (linkIterator.MoveNext())
                {
                    AtomLink link = new AtomLink();
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
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillFeedOptionals(
            AtomFeed feed,
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(feed, "feed");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator generatorNavigator = source.SelectSingleNode("atom:generator", manager);
            XPathNavigator copyrightNavigator = source.SelectSingleNode("atom:copyright", manager);
            XPathNavigator taglineNavigator = source.SelectSingleNode("atom:tagline", manager);

            if (generatorNavigator != null)
            {
                feed.Generator = CreateGenerator(generatorNavigator, manager, settings);
            }

            if (copyrightNavigator != null)
            {
                feed.Rights = CreateTextContent(copyrightNavigator, manager, settings);
            }

            if (taglineNavigator != null)
            {
                feed.Subtitle = CreateTextContent(taglineNavigator, manager, settings);
            }
        }
    }
}