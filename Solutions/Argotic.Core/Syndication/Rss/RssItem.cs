﻿namespace Argotic.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;

    /// <summary>
    /// Represents distinct content published in an <see cref="RssFeed"/> such as a news article, weblog entry or some other form of discrete update.
    /// </summary>
    /// <seealso cref="RssFeed"/>
    /// <remarks>
    ///     A <see cref="RssItem"/> <b>must</b> contain either a <see cref="RssItem.Title"/> <i>or</i> <see cref="RssItem.Description"/>.
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the RssItem class.">
    ///         <code source="..\..\Argotic.Examples\Core\Rss\RssItemExample.cs" region="RssItem"/>
    ///     </code>
    /// </example>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rss")]
    public class RssItem : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the e-mail address of the person who wrote the item.
        /// </summary>
        private string itemAuthor = string.Empty;

        /// <summary>
        /// Private member to hold categories or tags to which the item belongs.
        /// </summary>
        private Collection<RssCategory> itemCategories;

        /// <summary>
        /// Private member to hold character data that contains the item's full content or a summary of its contents.
        /// </summary>
        private string itemDescription = string.Empty;

        /// <summary>
        /// Private member to hold media objects such as an audio, video, or executable file that are associated with the item.
        /// </summary>
        private Collection<RssEnclosure> itemEnclosures;

        /// <summary>
        /// Private member to hold character data that provides the item's headline.
        /// </summary>
        private string itemTitle = string.Empty;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RssItem"/> class.
        /// </summary>
        public RssItem()
        {
        }

        /// <summary>
        /// Gets or sets the e-mail address of the person who wrote this item.
        /// </summary>
        /// <value>The e-mail address of the person who wrote this item.</value>
        /// <remarks>
        ///     <para>
        ///         There is no requirement to follow a specific format for email addresses. Publishers can format addresses according to the RFC 2822 Address Specification,
        ///         the RFC 2368 guidelines for mailto links, or some other scheme. The recommended format for e-mail addresses is <i>username@hostname.tld (Real Name)</i>.
        ///     </para>
        ///     <para>
        ///         A feed published by an individual <i>should</i> omit the item <see cref="RssItem.Author">author</see>
        ///         and use the <see cref="RssChannel.ManagingEditor"/> or <see cref="RssChannel.Webmaster"/> channel properties to provide contact information.
        ///     </para>
        /// </remarks>
        public string Author
        {
            get
            {
                return this.itemAuthor;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.itemAuthor = string.Empty;
                }
                else
                {
                    this.itemAuthor = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets the categories or tags to which this item belongs.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> of <see cref="RssCategory"/> objects that represent the categories to which this item belongs. The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<RssCategory> Categories
        {
            get
            {
                if (this.itemCategories == null)
                {
                    this.itemCategories = new Collection<RssCategory>();
                }

                return this.itemCategories;
            }
        }

        /// <summary>
        /// Gets or sets the URL of a web page that contains comments received in response to this item.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of a web page that contains comments received in response to this item.</value>
        public Uri Comments { get; set; }

        /// <summary>
        /// Gets or sets character data that contains this item's full content or a summary of its contents.
        /// </summary>
        /// <value>Character data that contains this item's full content or a summary of its contents.</value>
        /// <remarks>
        ///     <para>The description <i>may</i> be empty if the item specifies a <see cref="RssItem.Title"/>.</para>
        ///     <para>
        ///         The description <b>must</b> be suitable for presentation as HTML.
        ///         HTML markup must be encoded as character data either by employing the <b>HTML entities</b> (&lt; and &gt;) <i>or</i> a <b>CDATA</b> section.
        ///     </para>
        ///     <para>
        ///         The description <i>should not</i> contain relative URLs, because the RSS format does not provide a means to identify the base URL of a document.
        ///         When a relative URL is present, an aggregator <i>may</i> attempt to resolve it to a full URL using the channel's <see cref="RssChannel.Link">link</see> as the base.
        ///     </para>
        /// </remarks>
        public string Description
        {
            get
            {
                return this.itemDescription;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.itemDescription = string.Empty;
                }
                else
                {
                    this.itemDescription = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets the media objects associated with this item.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> of <see cref="RssEnclosure"/> objects that represent the media objects such as an audio, video, or executable file that are associated with this item.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Support for the enclosure element in RSS software varies significantly because of disagreement over whether the specification permits more than one enclosure per item.
        ///         Although the original author intended to permit no more than one enclosure in each item, this limit is not explicit in the specification.
        ///         For best support in the widest number of aggregators, an item <i>should not</i> contain more than one enclosure.
        ///     </para>
        /// </remarks>
        public Collection<RssEnclosure> Enclosures
        {
            get
            {
                if (this.itemEnclosures == null)
                {
                    this.itemEnclosures = new Collection<RssEnclosure>();
                }

                return this.itemEnclosures;
            }
        }

        /// <summary>
        /// Gets or sets the syndication extensions applied to this syndication entity.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<ISyndicationExtension> Extensions
        {
            get
            {
                if (this.objectSyndicationExtensions == null)
                {
                    this.objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }

                return this.objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.objectSyndicationExtensions = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique identifier for this item.
        /// </summary>
        /// <value>
        ///     A <see cref="RssGuid"/> object that represents the unique identifier for this item. The default value is a <b>null</b> reference.
        /// </value>
        /// <remarks>
        ///     A publisher <i>should</i> provide a guid for each item.
        /// </remarks>
        public RssGuid Guid { get; set; }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
        /// </summary>
        /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, otherwise returns <b>false</b>.</value>
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets the URL of a web page associated with this item.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of a web page associated with this item.</value>
        public Uri Link { get; set; }

        /// <summary>
        /// Gets or sets the publication date and time of this item.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> object that represents the publication date and time of this item.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no publication date was specified.
        /// </value>
        /// <remarks>
        ///     The specification recommends that aggregators <i>should</i> ignore items with a publication date that occurs in the future,
        ///     providing a means for publishers to embargo an item until that date. However, it is recommended that publishers <i>should not</i>
        ///     include items in a feed until they are ready for publication.
        /// </remarks>
        public DateTime PublicationDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the source feed that this item was republished from.
        /// </summary>
        /// <value>
        ///     A <see cref="RssSource"/> object that represents the source feed that this item was republished from. The default value is a <b>null</b> reference.
        /// </value>
        public RssSource Source { get; set; }

        /// <summary>
        /// Gets or sets character data that provides this item's headline.
        /// </summary>
        /// <value>Character data that provides this item's headline.</value>
        /// <remarks>
        ///     This property is optional if the item contains a <see cref="RssItem.Description"/>.
        /// </remarks>
        public string Title
        {
            get
            {
                return this.itemTitle;
            }

            set
            {
                this.itemTitle = value.Trim();
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(RssItem first, RssItem second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(RssItem first, RssItem second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(RssItem first, RssItem second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(RssItem first, RssItem second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }

            if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Compares two specified <see cref="Collection{RssEnclosure}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<RssEnclosure> source, Collection<RssEnclosure> target)
        {
            int result = 0;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result = result | source[i].CompareTo(target[i]);
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }

        /// <summary>
        /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddExtension(ISyndicationExtension extension)
        {
            bool wasAdded = false;
            Guard.ArgumentNotNull(extension, "extension");
            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded = true;

            return wasAdded;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            RssItem value = obj as RssItem;

            if (value != null)
            {
                int result = string.Compare(this.Author, value.Author, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(
                             this.Comments,
                             value.Comments,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(
                             this.Description,
                             value.Description,
                             StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(
                             this.Link,
                             value.Link,
                             UriComponents.AbsoluteUri,
                             UriFormat.SafeUnescaped,
                             StringComparison.OrdinalIgnoreCase);
                result = result | this.PublicationDate.CompareTo(value.PublicationDate);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);

                if (this.Guid != null)
                {
                    result = result | this.Guid.CompareTo(value.Guid);
                }
                else if (this.Guid == null && value.Guid != null)
                {
                    result = result | -1;
                }

                if (this.Source != null)
                {
                    result = result | this.Source.CompareTo(value.Source);
                }
                else if (this.Source == null && value.Source != null)
                {
                    result = result | -1;
                }

                result = result | RssFeed.CompareSequence(this.Categories, value.Categories);
                result = result | CompareSequence(this.Enclosures, value.Enclosures);

                return result;
            }

            throw new ArgumentException(
                string.Format(
                    null,
                    "obj is not of type {0}, type was found to be '{1}'.",
                    this.GetType().FullName,
                    obj.GetType().FullName),
                "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RssItem))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
        /// <returns>
        ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
        /// </returns>
        /// <remarks>
        ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
        ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
        ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference (Nothing in Visual Basic).</exception>
        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            Guard.ArgumentNotNull(match, "match");
            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }

        /// <summary>
        /// Finds an extension.
        /// </summary>
        /// <typeparam name="TExtension">The extension type.</typeparam>
        /// <returns>Returns the extension.</returns>
        public TExtension FindExtension<TExtension>()
            where TExtension : ISyndicationExtension
        {
            return this.Extensions.OfType<TExtension>().FirstOrDefault();
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Loads this <see cref="RssItem"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RssItem"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssItem"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XmlNamespaceManager manager = new XmlNamespaceManager(source.NameTable);
            XPathNavigator authorNavigator = source.SelectSingleNode("author", manager);
            XPathNavigator commentsNavigator = source.SelectSingleNode("comments", manager);
            XPathNavigator descriptionNavigator = source.SelectSingleNode("description", manager);
            XPathNavigator guidNavigator = source.SelectSingleNode("guid", manager);
            XPathNavigator linkNavigator = source.SelectSingleNode("link", manager);
            XPathNavigator publicationNavigator = source.SelectSingleNode("pubDate", manager);
            XPathNavigator sourceNavigator = source.SelectSingleNode("source", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("title", manager);

            XPathNodeIterator categoryIterator = source.Select("category", manager);
            XPathNodeIterator enclosureIterator = source.Select("enclosure", manager);
            if (titleNavigator != null)
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }

            if (descriptionNavigator != null)
            {
                this.Description = descriptionNavigator.Value;
                wasLoaded = true;
            }

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    this.Link = link;
                    wasLoaded = true;
                }
            }

            if (authorNavigator != null)
            {
                this.Author = authorNavigator.Value;
                wasLoaded = true;
            }

            if (commentsNavigator != null)
            {
                Uri comments;
                if (Uri.TryCreate(commentsNavigator.Value, UriKind.RelativeOrAbsolute, out comments))
                {
                    this.Comments = comments;
                    wasLoaded = true;
                }
            }

            if (guidNavigator != null)
            {
                RssGuid guid = new RssGuid();
                if (guid.Load(guidNavigator))
                {
                    this.Guid = guid;
                    wasLoaded = true;
                }
            }

            if (publicationNavigator != null)
            {
                DateTime publicationDate;
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out publicationDate))
                {
                    this.PublicationDate = publicationDate;
                    wasLoaded = true;
                }
            }

            if (sourceNavigator != null)
            {
                RssSource sourceFeed = new RssSource();
                if (sourceFeed.Load(sourceNavigator))
                {
                    this.Source = sourceFeed;
                    wasLoaded = true;
                }
            }

            if (categoryIterator != null && categoryIterator.Count > 0)
            {
                while (categoryIterator.MoveNext())
                {
                    RssCategory category = new RssCategory();
                    if (category.Load(categoryIterator.Current))
                    {
                        this.Categories.Add(category);
                    }
                }
            }

            if (enclosureIterator != null && enclosureIterator.Count > 0)
            {
                while (enclosureIterator.MoveNext())
                {
                    RssEnclosure enclosure = new RssEnclosure();
                    if (enclosure.Load(enclosureIterator.Current))
                    {
                        this.Enclosures.Add(enclosure);
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="RssItem"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssItem"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssItem"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");
            XmlNamespaceManager manager = new XmlNamespaceManager(source.NameTable);
            XPathNavigator authorNavigator = source.SelectSingleNode("author", manager);
            XPathNavigator commentsNavigator = source.SelectSingleNode("comments", manager);
            XPathNavigator descriptionNavigator = source.SelectSingleNode("description", manager);
            XPathNavigator guidNavigator = source.SelectSingleNode("guid", manager);
            XPathNavigator linkNavigator = source.SelectSingleNode("link", manager);
            XPathNavigator publicationNavigator = source.SelectSingleNode("pubDate", manager);
            XPathNavigator sourceNavigator = source.SelectSingleNode("source", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("title", manager);

            XPathNodeIterator categoryIterator = source.Select("category", manager);
            XPathNodeIterator enclosureIterator = source.Select("enclosure", manager);
            if (titleNavigator != null)
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }

            if (descriptionNavigator != null)
            {
                this.Description = descriptionNavigator.Value;
                wasLoaded = true;
            }

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    this.Link = link;
                    wasLoaded = true;
                }
            }

            if (authorNavigator != null)
            {
                this.Author = authorNavigator.Value;
                wasLoaded = true;
            }

            if (commentsNavigator != null)
            {
                Uri comments;
                if (Uri.TryCreate(commentsNavigator.Value, UriKind.RelativeOrAbsolute, out comments))
                {
                    this.Comments = comments;
                    wasLoaded = true;
                }
            }

            if (guidNavigator != null)
            {
                RssGuid guid = new RssGuid();
                if (guid.Load(guidNavigator, settings))
                {
                    this.Guid = guid;
                    wasLoaded = true;
                }
            }

            if (publicationNavigator != null)
            {
                DateTime publicationDate;
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out publicationDate))
                {
                    this.PublicationDate = publicationDate;
                    wasLoaded = true;
                }
            }

            if (sourceNavigator != null)
            {
                RssSource sourceFeed = new RssSource();
                if (sourceFeed.Load(sourceNavigator, settings))
                {
                    this.Source = sourceFeed;
                    wasLoaded = true;
                }
            }

            if (categoryIterator != null && categoryIterator.Count > 0)
            {
                while (categoryIterator.MoveNext())
                {
                    RssCategory category = new RssCategory();
                    if (category.Load(categoryIterator.Current, settings))
                    {
                        this.Categories.Add(category);
                    }
                }
            }

            if (enclosureIterator != null && enclosureIterator.Count > 0)
            {
                while (enclosureIterator.MoveNext())
                {
                    RssEnclosure enclosure = new RssEnclosure();
                    if (enclosure.Load(enclosureIterator.Current, settings))
                    {
                        this.Enclosures.Add(enclosure);
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }

        /// <summary>
        /// Removes the supplied <see cref="ISyndicationExtension"/> from the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was removed from the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Extensions"/> collection of the current instance does not contain the specified <see cref="ISyndicationExtension"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveExtension(ISyndicationExtension extension)
        {
            bool wasRemoved = false;
            Guard.ArgumentNotNull(extension, "extension");
            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved = true;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="RssItem"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="RssItem"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.WriteTo(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Saves the current <see cref="RssItem"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("item");

            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.WriteElementString("title", this.Title);
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                writer.WriteElementString("description", this.Description);
            }

            if (this.Link != null)
            {
                writer.WriteElementString("link", this.Link.ToString());
            }

            if (!string.IsNullOrEmpty(this.Author))
            {
                writer.WriteElementString("author", this.Author);
            }

            if (this.Comments != null)
            {
                writer.WriteElementString("comments", this.Comments.ToString());
            }

            if (this.Guid != null)
            {
                this.Guid.WriteTo(writer);
            }

            if (this.PublicationDate != DateTime.MinValue)
            {
                writer.WriteElementString("pubDate", SyndicationDateTimeUtility.ToRfc822DateTime(this.PublicationDate));
            }

            if (this.Source != null)
            {
                this.Source.WriteTo(writer);
            }

            foreach (RssCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }

            foreach (RssEnclosure enclosure in this.Enclosures)
            {
                enclosure.WriteTo(writer);
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}