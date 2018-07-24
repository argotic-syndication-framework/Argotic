namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="FeedHistorySyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class FeedHistorySyndicationExtensionContext
    {

        /// <summary>
        /// Private member to hold a collection of relations for linked feed documents that together contain the entries of a logical feed.
        /// </summary>
        private Collection<FeedHistoryLinkRelation> extensionLinkRelations;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedHistorySyndicationExtensionContext"/> class.
        /// </summary>
        public FeedHistorySyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating the feed is a set of linked feed documents that together contain the entries of a logical feed, without any guarantees about the stability of the documents' contents.
        /// </summary>
        /// <value><b>true</b> if feed is a set of linked feed documents that together contain the entries of a logical feed; otherwise returns <b>false</b>.</value>
        public bool IsArchive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating the feed contains all of the entries of a logical feed; any entry not actually in the feed document should not be considered to be part of that feed.
        /// </summary>
        /// <value><b>true</b> if feed contains all of the entries of a logical feed; otherwise returns <b>false</b>.</value>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="FeedHistoryLinkRelation"/> objects that represent the relationships between feed documents.
        /// </summary>
        /// <value>A collection of <see cref="FeedHistoryLinkRelation"/> objects that represent the relationships between feed documents.</value>
        public Collection<FeedHistoryLinkRelation> Relations
        {
            get
            {
                if (this.extensionLinkRelations == null)
                {
                    this.extensionLinkRelations = new Collection<FeedHistoryLinkRelation>();
                }

                return this.extensionLinkRelations;
            }
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="FeedHistorySyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="FeedHistorySyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            if (string.IsNullOrEmpty(manager.LookupNamespace("atom")))
            {
                manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            }

            if (source.HasChildren)
            {
                XPathNavigator archiveNavigator = source.SelectSingleNode("fh:archive", manager);
                XPathNavigator completeNavigator = source.SelectSingleNode("fh:complete", manager);
                XPathNodeIterator linkIterator = source.Select("atom:link", manager);

                if (archiveNavigator != null)
                {
                    this.IsArchive = true;
                    wasLoaded = true;
                }

                if (completeNavigator != null)
                {
                    this.IsComplete = true;
                    wasLoaded = true;
                }

                if (linkIterator != null && linkIterator.Count > 0)
                {
                    while (linkIterator.MoveNext())
                    {
                        string relAttribute = linkIterator.Current.GetAttribute("rel", string.Empty);

                        if (!string.IsNullOrEmpty(relAttribute) && FeedHistorySyndicationExtension.LinkRelationTypeByName(relAttribute) != FeedHistoryLinkRelationType.None)
                        {
                            FeedHistoryLinkRelation relation = new FeedHistoryLinkRelation();
                            if (relation.Load(linkIterator.Current))
                            {
                                this.Relations.Add(relation);
                                wasLoaded = true;
                            }
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Writes the current context to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        public void WriteTo(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (this.IsArchive)
            {
                writer.WriteElementString("archive", xmlNamespace, string.Empty);
            }

            if (this.IsComplete)
            {
                writer.WriteElementString("complete", xmlNamespace, string.Empty);
            }

            foreach (FeedHistoryLinkRelation relation in this.Relations)
            {
                relation.WriteTo(writer);
            }
        }
    }
}