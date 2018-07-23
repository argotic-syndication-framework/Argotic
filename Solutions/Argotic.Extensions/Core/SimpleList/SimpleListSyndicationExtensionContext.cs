namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="SimpleListSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class SimpleListSyndicationExtensionContext
    {
        /// <summary>
        /// Private member to hold a value indicating if the feed is intended to be consumed as a list.
        /// </summary>
        private bool extensionTreatAsList;

        /// <summary>
        /// Private member to hold information that allows the client to group or filter on the values of feed properties.
        /// </summary>
        private Collection<SimpleListGroup> extensionGroups;

        /// <summary>
        /// Private member to hold information that allows the client to sort on the values of feed properties.
        /// </summary>
        private Collection<SimpleListSort> extensionSorts;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleListSyndicationExtensionContext"/> class.
        /// </summary>
        public SimpleListSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets information that allows the client to group or filter on the values of feed properties.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="SimpleListGroup"/> objects that represent information that allows the client to group or filter on the values of feed properties.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<SimpleListGroup> Grouping
        {
            get
            {
                if (this.extensionGroups == null)
                {
                    this.extensionGroups = new Collection<SimpleListGroup>();
                }

                return this.extensionGroups;
            }
        }

        /// <summary>
        /// Gets information that allows the client to sort on the values of feed properties.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="SimpleListSort"/> objects that represent information that allows the client to sort on the values of feed properties.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<SimpleListSort> Sorting
        {
            get
            {
                if (this.extensionSorts == null)
                {
                    this.extensionSorts = new Collection<SimpleListSort>();
                }

                return this.extensionSorts;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if this feed is intended to be consumed as a list.
        /// </summary>
        /// <value><b>true</b> if the syndication feed is intended to be consumed as a list; otherwise false.</value>
        /// <remarks>
        ///     This property allows the publisher of a feed document to indicate to the consumers of the feed that the feed is intended to be consumed as a list,
        ///     and as such is the primary means for feed consumers to identify lists.
        /// </remarks>
        public bool TreatAsList
        {
            get
            {
                return this.extensionTreatAsList;
            }

            set
            {
                this.extensionTreatAsList = value;
            }
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SimpleListSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="SimpleListSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator treatAsNavigator = source.SelectSingleNode("cf:treatAs", manager);
                XPathNavigator listInformationNavigator = source.SelectSingleNode("cf:listinfo", manager);

                if (treatAsNavigator != null && string.Compare(treatAsNavigator.Value, "list", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.TreatAsList = true;
                    wasLoaded = true;
                }

                if (listInformationNavigator != null && listInformationNavigator.HasChildren)
                {
                    XPathNodeIterator sortIterator = source.Select("cf:sort", manager);
                    XPathNodeIterator groupIterator = source.Select("cf:group", manager);

                    if (sortIterator != null && sortIterator.Count > 0)
                    {
                        while (sortIterator.MoveNext())
                        {
                            SimpleListSort sort = new SimpleListSort();
                            if (sort.Load(sortIterator.Current))
                            {
                                this.Sorting.Add(sort);
                                wasLoaded = true;
                            }
                        }
                    }

                    if (groupIterator != null && groupIterator.Count > 0)
                    {
                        while (groupIterator.MoveNext())
                        {
                            SimpleListGroup group = new SimpleListGroup();
                            if (group.Load(groupIterator.Current))
                            {
                                this.Grouping.Add(group);
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
            if (this.TreatAsList)
            {
                writer.WriteElementString("treatAs", xmlNamespace, "list");
            }

            if (this.Grouping.Count > 0 || this.Sorting.Count > 0)
            {
                writer.WriteStartElement("listinfo", xmlNamespace);

                foreach (SimpleListSort sort in this.Sorting)
                {
                    sort.WriteTo(writer);
                }

                foreach (SimpleListGroup group in this.Grouping)
                {
                    group.WriteTo(writer);
                }

                writer.WriteEndElement();
            }
        }
    }
}