﻿namespace Argotic.Data.Adapters
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
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Atom10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
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

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(this.Navigator.NameTable);

            XPathNavigator feedNavigator = this.Navigator.SelectSingleNode("atom:feed", manager);

            if (feedNavigator != null)
            {
                AtomUtility.FillCommonObjectAttributes(resource, feedNavigator);

                XPathNavigator idNavigator = feedNavigator.SelectSingleNode("atom:id", manager);
                XPathNavigator titleNavigator = feedNavigator.SelectSingleNode("atom:title", manager);
                XPathNavigator updatedNavigator = feedNavigator.SelectSingleNode("atom:updated", manager);

                if (idNavigator != null)
                {
                    resource.Id = new AtomId();
                    resource.Id.Load(idNavigator, this.Settings);
                }

                if (titleNavigator != null)
                {
                    resource.Title = new AtomTextConstruct();
                    resource.Title.Load(titleNavigator, this.Settings);
                }

                if (updatedNavigator != null)
                {
                    DateTime updatedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedNavigator.Value, out updatedOn))
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
        /// Modifies the <see cref="AtomEntry"/> to match the supplied <see cref="XPathNavigator"/> data source.
        /// </summary>
        /// <param name="entry">The <see cref="AtomEntry"/> to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the fill operation.</param>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
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
            XPathNavigator updatedNavigator = source.SelectSingleNode("atom:updated", manager);

            if (idNavigator != null)
            {
                entry.Id = new AtomId();
                entry.Id.Load(idNavigator, settings);
            }

            if (titleNavigator != null)
            {
                entry.Title = new AtomTextConstruct();
                entry.Title.Load(titleNavigator, settings);
            }

            if (updatedNavigator != null)
            {
                DateTime updatedOn;
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedNavigator.Value, out updatedOn))
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
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
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
            XPathNodeIterator categoryIterator = source.Select("atom:category", manager);
            XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
            XPathNodeIterator linkIterator = source.Select("atom:link", manager);

            if (authorIterator != null && authorIterator.Count > 0)
            {
                while (authorIterator.MoveNext())
                {
                    AtomPersonConstruct author = new AtomPersonConstruct();
                    if (author.Load(authorIterator.Current, settings))
                    {
                        entry.Authors.Add(author);
                    }
                }
            }

            if (categoryIterator != null && categoryIterator.Count > 0)
            {
                while (categoryIterator.MoveNext())
                {
                    AtomCategory category = new AtomCategory();
                    if (category.Load(categoryIterator.Current, settings))
                    {
                        entry.Categories.Add(category);
                    }
                }
            }

            if (contributorIterator != null && contributorIterator.Count > 0)
            {
                while (contributorIterator.MoveNext())
                {
                    AtomPersonConstruct contributor = new AtomPersonConstruct();
                    if (contributor.Load(contributorIterator.Current, settings))
                    {
                        entry.Contributors.Add(contributor);
                    }
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
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomEntry"/>.
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
            XPathNavigator publishedNavigator = source.SelectSingleNode("atom:published", manager);
            XPathNavigator rightsNavigator = source.SelectSingleNode("atom:rights", manager);
            XPathNavigator sourceNavigator = source.SelectSingleNode("atom:source", manager);
            XPathNavigator summaryNavigator = source.SelectSingleNode("atom:summary", manager);

            if (contentNavigator != null)
            {
                entry.Content = new AtomContent();
                entry.Content.Load(contentNavigator, settings);
            }

            if (publishedNavigator != null)
            {
                DateTime publishedOn;
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(publishedNavigator.Value, out publishedOn))
                {
                    entry.PublishedOn = publishedOn;
                }
            }

            if (rightsNavigator != null)
            {
                entry.Rights = new AtomTextConstruct();
                entry.Rights.Load(rightsNavigator, settings);
            }

            if (sourceNavigator != null)
            {
                entry.Source = new AtomSource();
                entry.Source.Load(sourceNavigator, settings);
            }

            if (summaryNavigator != null)
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
            XPathNodeIterator categoryIterator = source.Select("atom:category", manager);
            XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
            XPathNodeIterator linkIterator = source.Select("atom:link", manager);
            XPathNodeIterator entryIterator = source.Select("atom:entry", manager);

            if (authorIterator != null && authorIterator.Count > 0)
            {
                while (authorIterator.MoveNext())
                {
                    AtomPersonConstruct author = new AtomPersonConstruct();
                    if (author.Load(authorIterator.Current, settings))
                    {
                        feed.Authors.Add(author);
                    }
                }
            }

            if (categoryIterator != null && categoryIterator.Count > 0)
            {
                while (categoryIterator.MoveNext())
                {
                    AtomCategory category = new AtomCategory();
                    if (category.Load(categoryIterator.Current, settings))
                    {
                        feed.Categories.Add(category);
                    }
                }
            }

            if (contributorIterator != null && contributorIterator.Count > 0)
            {
                while (contributorIterator.MoveNext())
                {
                    AtomPersonConstruct contributor = new AtomPersonConstruct();
                    if (contributor.Load(contributorIterator.Current, settings))
                    {
                        feed.Contributors.Add(contributor);
                    }
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
            XPathNavigator iconNavigator = source.SelectSingleNode("atom:icon", manager);
            XPathNavigator logoNavigator = source.SelectSingleNode("atom:logo", manager);
            XPathNavigator rightsNavigator = source.SelectSingleNode("atom:rights", manager);
            XPathNavigator subtitleNavigator = source.SelectSingleNode("atom:subtitle", manager);

            if (generatorNavigator != null)
            {
                feed.Generator = new AtomGenerator();
                feed.Generator.Load(generatorNavigator, settings);
            }

            if (iconNavigator != null)
            {
                feed.Icon = new AtomIcon();
                feed.Icon.Load(iconNavigator, settings);
            }

            if (logoNavigator != null)
            {
                feed.Logo = new AtomLogo();
                feed.Logo.Load(logoNavigator, settings);
            }

            if (rightsNavigator != null)
            {
                feed.Rights = new AtomTextConstruct();
                feed.Rights.Load(rightsNavigator, settings);
            }

            if (subtitleNavigator != null)
            {
                feed.Subtitle = new AtomTextConstruct();
                feed.Subtitle.Load(subtitleNavigator, settings);
            }
        }
    }
}