﻿namespace Argotic.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Data.Adapters;
    using Argotic.Extensions;

    /// <summary>
    /// Represents an Atom syndication feed, including metadata about the feed, and some or all of the entries associated with it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Atom is an an XML-based Web content and metadata syndication format that describes lists of related information known as <i>feeds</i>.
    ///         Feeds are composed of a number of items, known as <i>entries</i>, each with an extensible set of attached metadata.
    ///     </para>
    ///     <para>
    ///         This implementation conforms to the Atom 1.0 specification, which can be found
    ///         at <a href="http://www.atomenabled.org/developers/syndication/atom-format-spec.php">http://www.atomenabled.org/developers/syndication/atom-format-spec.php</a>.
    ///     </para>
    ///     <para>
    ///         If multiple <see cref="AtomEntry"/> objects with the same <see cref="AtomEntry.Id"/> value appear in an Atom Feed Document, they represent the same entry.
    ///         Their <see cref="AtomEntry.UpdatedOn"/> timestamps <i>should</i> be different. If an Atom Feed Document contains multiple entries with the same <see cref="AtomEntry.Id"/>,
    ///         Atom Processors <u>may</u> choose to display all of them or some subset of them. One typical behavior would be to display only the entry with the latest <see cref="AtomEntry.UpdatedOn"/> timestamp.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the AtomFeed class.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="AtomFeed" />
    ///     </code>
    /// </example>
    [Serializable]
    public class AtomFeed : ISyndicationResource, IAtomCommonObjectAttributes, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold HTTP web request used by asynchronous load operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;

        /// <summary>
        /// Private member to hold the syndication format for this syndication resource.
        /// </summary>
        private static SyndicationContentFormat feedFormat = SyndicationContentFormat.Atom;

        /// <summary>
        /// Private member to hold the version of the syndication format for this syndication resource conforms to.
        /// </summary>
        private static Version feedVersion = new Version(1, 0);

        /// <summary>
        /// Private member to hold the collection of authors of the feed.
        /// </summary>
        private Collection<AtomPersonConstruct> feedAuthors;

        /// <summary>
        /// Private member to hold the collection of categories associated with the feed.
        /// </summary>
        private Collection<AtomCategory> feedCategories;

        /// <summary>
        /// Private member to hold the collection of contributors of the feed.
        /// </summary>
        private Collection<AtomPersonConstruct> feedContributors;

        /// <summary>
        /// Private member to hold the collection of entries that comprise the distinct content published in the feed.
        /// </summary>
        private IEnumerable<AtomEntry> feedEntries;

        /// <summary>
        /// Private member to hold a permanent, universally unique identifier for the feed.
        /// </summary>
        private AtomId feedId;

        /// <summary>
        /// Private member to hold eferences from the feed to one or more Web resources.
        /// </summary>
        private Collection<AtomLink> feedLinks;

        /// <summary>
        /// Private member to hold information that conveys a human-readable title for the feed.
        /// </summary>
        private AtomTextConstruct feedTitle;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomFeed"/> class.
        /// </summary>
        public AtomFeed()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomFeed"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
        /// </summary>
        /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this feed.</param>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this feed.</param>
        /// <param name="utcUpdatedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this feed was modified in a way the publisher considers significant.
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomFeed(AtomId id, AtomTextConstruct title, DateTime utcUpdatedOn)
        {
            this.Id = id;
            this.Title = title;
            this.UpdatedOn = utcUpdatedOn;
        }

        /// <summary>
        /// Occurs when the syndication resource state has been changed by a load operation.
        /// </summary>
        /// <seealso cref="AtomFeed.Load(IXPathNavigable)"/>
        /// <seealso cref="AtomFeed.Load(XmlReader)"/>
        public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

        /// <summary>
        /// Gets the authors of this feed.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the authors of this feed.</value>
        /// <remarks>
        ///     <para>A <see cref="AtomFeed"/> <b>must</b> contain one or more authors, unless all of the feeds's child <see cref="AtomEntry"/> objects contain at least one author.</para>
        ///     <para>
        ///         The <see cref="Authors"/> are considered to apply to any <see cref="AtomEntry"/> contained in this feed if the entry does not contain any authors and the entry's source does contain any authors.
        ///     </para>
        /// </remarks>
        public Collection<AtomPersonConstruct> Authors
        {
            get
            {
                if (this.feedAuthors == null)
                {
                    this.feedAuthors = new Collection<AtomPersonConstruct>();
                }

                return this.feedAuthors;
            }
        }

        /// <summary>
        /// Gets or sets the base URI other than the base URI of the document or external entity.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>,
        ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
        /// </remarks>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// Gets the categories associated with this feed.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomCategory"/> objects that represent the categories associated with this feed.</value>
        public Collection<AtomCategory> Categories
        {
            get
            {
                if (this.feedCategories == null)
                {
                    this.feedCategories = new Collection<AtomCategory>();
                }

                return this.feedCategories;
            }
        }

        /// <summary>
        /// Gets the entities who contributed to this feed.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the entities who contributed to this feed.</value>
        public Collection<AtomPersonConstruct> Contributors
        {
            get
            {
                if (this.feedContributors == null)
                {
                    this.feedContributors = new Collection<AtomPersonConstruct>();
                }

                return this.feedContributors;
            }
        }

        /// <summary>
        /// Gets or sets the distinct content published in this feed.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="AtomEntry"/> objects that represent distinct content published in this feed.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="AtomEntry"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<AtomEntry> Entries
        {
            get
            {
                if (this.feedEntries == null)
                {
                    this.feedEntries = new Collection<AtomEntry>();
                }

                return this.feedEntries;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.feedEntries = value;
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
        /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
        /// </summary>
        /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication resource implements.</value>
        public SyndicationContentFormat Format => feedFormat;

        /// <summary>
        /// Gets or sets the agent used to generate this feed.
        /// </summary>
        /// <value>A <see cref="AtomGenerator"/> object that represents the agent used to generate this feed. The default value is a <b>null</b> reference.</value>
        public AtomGenerator Generator { get; set; }

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
        /// Gets or sets an image that provides iconic visual identification for this feed.
        /// </summary>
        /// <value>A <see cref="AtomIcon"/> object that represents an image that provides iconic visual identification for this feed. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     The image <i>should</i> have an aspect ratio of one (horizontal) to one (vertical) and <i>should</i> be suitable for presentation at a small size.
        /// </remarks>
        public AtomIcon Icon { get; set; }

        /// <summary>
        /// Gets or sets a permanent, universally unique identifier for this feed.
        /// </summary>
        /// <value>A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this feed.</value>
        /// <remarks>
        ///     <para>
        ///         When an <i>Atom Document</i> is relocated, migrated, syndicated, republished, exported, or imported, the content of its universally unique identifier <b>must not</b> change.
        ///         Put another way, an <see cref="AtomId"/> pertains to all instantiations of a particular <see cref="AtomFeed"/>; revisions retain the same
        ///         content in their <see cref="AtomId"/> properties. It is suggested that the<see cref="AtomId"/> be stored along with the associated resource.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomId Id
        {
            get
            {
                return this.feedId;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.feedId = value;
            }
        }

        /// <summary>
        /// Gets or sets the natural or formal language in which the content is written.
        /// </summary>
        /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     <para>
        ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
        ///     </para>
        /// </remarks>
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets references from this feed to one or more Web resources.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomLink"/> objects that represent references from this feed to one or more Web resources.</value>
        /// <remarks>
        ///     <para>
        ///         A feed <i>should</i> contain one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> property of <i>self</i>.
        ///         This is the preferred URI for retrieving Atom Feed Documents representing this Atom feed.
        ///     </para>
        ///     <para>
        ///         A feed <b>must not</b> contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> property of <i>alternate</i>
        ///         that has the same combination of <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> property values.
        ///     </para>
        /// </remarks>
        public Collection<AtomLink> Links
        {
            get
            {
                if (this.feedLinks == null)
                {
                    this.feedLinks = new Collection<AtomLink>();
                }

                return this.feedLinks;
            }
        }

        /// <summary>
        /// Gets or sets an image that provides visual identification for this feed.
        /// </summary>
        /// <value>A <see cref="AtomLogo"/> object that represents an image that provides visual identification for this feed. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     The image <i>should</i> have an aspect ratio of 2 (horizontal) to 1 (vertical).
        /// </remarks>
        public AtomLogo Logo { get; set; }

        /// <summary>
        /// Gets or sets information about rights held in and over this feed.
        /// </summary>
        /// <value>A <see cref="AtomTextConstruct"/> object that represents information about rights held in and over this feed.</value>
        /// <remarks>
        ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
        /// </remarks>
        public AtomTextConstruct Rights { get; set; }

        /// <summary>
        /// Gets or sets information that conveys a human-readable description or subtitle for this feed.
        /// </summary>
        /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable description or subtitle for this feed.</value>
        public AtomTextConstruct Subtitle { get; set; }

        /// <summary>
        /// Gets or sets information that conveys a human-readable title for this feed.
        /// </summary>
        /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this feed.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomTextConstruct Title
        {
            get
            {
                return this.feedTitle;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.feedTitle = value;
            }
        }

        /// <summary>
        /// Gets or sets a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this feed was modified in a way the publisher considers significant.
        ///     Publishers <i>may</i> change the value of this element over time. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
        /// </summary>
        /// <value>The <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to. The default value is <b>2.0</b>.</value>
        public Version Version => feedVersion;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        /// <value><b>true</b> if syndication resource asynchronous load operation has been cancelled, otherwise <b>false</b>.</value>
        internal bool AsyncLoadHasBeenCancelled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        /// <value><b>true</b> if syndication resource is in the process of loading, otherwise <b>false</b>.</value>
        internal bool LoadOperationInProgress { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AtomEntry"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to get or set.</param>
        /// <returns>The <see cref="AtomEntry"/> at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="AtomFeed.Entries"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomEntry this[int index]
        {
            get
            {
                return ((Collection<AtomEntry>)this.Entries)[index];
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                ((Collection<AtomEntry>)this.Entries)[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AtomEntry"/> that has the associated identifier.
        /// </summary>
        /// <param name="id">The <see cref="AtomId.Uri"/> that uniquely identifies the entry to get or set.</param>
        /// <returns>The <see cref="AtomEntry"/> with the associated <see cref="AtomId.Uri"/>.</returns>
        /// <remarks>
        ///     If no entry exists for the specified <paramref name="id"/>, returns a <b>null</b> reference.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public AtomEntry this[string id]
        {
            get
            {
                Guard.ArgumentNotNullOrEmptyString(id, "id");

                AtomEntry result = null;
                Uri uri = new Uri(id, UriKind.RelativeOrAbsolute);

                foreach (AtomEntry entry in this.Entries)
                {
                    if (entry.Id != null)
                    {
                        Uri idUri = null;
                        if (entry.Id.BaseUri != null)
                        {
                            idUri = new Uri(entry.Id.BaseUri, entry.Id.Uri);
                        }
                        else
                        {
                            idUri = entry.Id.Uri;
                        }

                        if (Uri.Compare(
                                idUri,
                                uri,
                                UriComponents.AbsoluteUri,
                                UriFormat.Unescaped,
                                StringComparison.Ordinal) == 0)
                        {
                            result = entry;
                            break;
                        }
                    }
                }

                return result;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(id, "id");
                Guard.ArgumentNotNull(value, "value");

                Collection<AtomEntry> entries = (Collection<AtomEntry>)this.Entries;
                Uri uri = new Uri(id, UriKind.RelativeOrAbsolute);

                for (int i = 0; i < entries.Count; i++)
                {
                    AtomEntry entry = entries[i];

                    if (entry.Id != null)
                    {
                        Uri idUri = null;
                        if (entry.Id.BaseUri != null)
                        {
                            idUri = new Uri(entry.Id.BaseUri, entry.Id.Uri);
                        }
                        else
                        {
                            idUri = entry.Id.Uri;
                        }

                        if (Uri.Compare(
                                idUri,
                                uri,
                                UriComponents.AbsoluteUri,
                                UriFormat.Unescaped,
                                StringComparison.Ordinal) == 0)
                        {
                            ((Collection<AtomEntry>)this.Entries)[i] = value;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares two specified <see cref="Collection{AtomCategory}"/> collections.
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
        public static int CompareSequence(Collection<AtomCategory> source, Collection<AtomCategory> target)
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
        /// Compares two specified <see cref="Collection{AtomLink}"/> collections.
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
        public static int CompareSequence(Collection<AtomLink> source, Collection<AtomLink> target)
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
        /// Compares two specified <see cref="Collection{AtomPersonConstruct}"/> collections.
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
        public static int CompareSequence(
            Collection<AtomPersonConstruct> source,
            Collection<AtomPersonConstruct> target)
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
        /// Compares two specified <see cref="Collection{AtomTextConstruct}"/> collections.
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
        public static int CompareSequence(Collection<AtomTextConstruct> source, Collection<AtomTextConstruct> target)
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
        /// Creates a new <see cref="AtomFeed"/> instance using the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <returns>An <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="AtomFeed"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Create method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Create(Uri source)" />
        ///     </code>
        /// </example>
        public static AtomFeed Create(Uri source)
        {
            return Create(source, new WebRequestOptions());
        }

        /// <summary>
        /// Creates a new <see cref="AtomFeed"/> instance using the specified <see cref="Uri"/> and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static AtomFeed Create(Uri source, SyndicationResourceLoadSettings settings)
        {
            return Create(source, new WebRequestOptions(), settings);
        }

        /// <summary>
        /// Creates a new <see cref="AtomFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <returns>An <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="AtomFeed"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static AtomFeed Create(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            return Create(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Creates a new <see cref="AtomFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>An <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="AtomFeed"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static AtomFeed Create(Uri source, WebRequestOptions options)
        {
            return Create(source, options, null);
        }

        /// <summary>
        /// Creates a new <see cref="AtomFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static AtomFeed Create(
            Uri source,
            ICredentials credentials,
            IWebProxy proxy,
            SyndicationResourceLoadSettings settings)
        {
            return Create(source, new WebRequestOptions(credentials, proxy), settings);
        }

        /// <summary>
        /// Creates a new <see cref="AtomFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static AtomFeed Create(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            AtomFeed syndicationResource = new AtomFeed();

            Guard.ArgumentNotNull(source, "source");

            syndicationResource.Load(source, options, settings);

            return syndicationResource;
        }

        /// <summary>
        /// Adds the supplied <see cref="AtomEntry"/> to the current instance's <see cref="Entries"/> collection.
        /// </summary>
        /// <param name="entry">The <see cref="AtomEntry"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="AtomEntry"/> was added to the <see cref="Entries"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddEntry(AtomEntry entry)
        {
            bool wasAdded = false;

            Guard.ArgumentNotNull(entry, "entry");

            ((Collection<AtomEntry>)this.Entries).Add(entry);
            wasAdded = true;

            return wasAdded;
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
        /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="AtomFeed"/>.
        /// </summary>
        /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
        /// <remarks>
        ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="AtomFeed"/>.
        ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="AtomFeed"/>.
        /// </remarks>
        public XPathNavigator CreateNavigator()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.ConformanceLevel = ConformanceLevel.Document;
                settings.Indent = true;
                settings.OmitXmlDeclaration = false;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.Save(writer);
                    writer.Flush();
                }

                stream.Seek(0, SeekOrigin.Begin);

                XPathDocument document = new XPathDocument(stream);
                return document.CreateNavigator();
            }
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
        /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Load(IXPathNavigable source)" />
        ///     </code>
        /// </example>
        public void Load(IXPathNavigable source)
        {
            this.Load(source, null);
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(source, "source");

            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            XPathNavigator navigator = source.CreateNavigator();
            this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator));
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Load(Stream stream)" />
        ///     </code>
        /// </example>
        public void Load(Stream stream)
        {
            this.Load(stream, null);
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(Stream stream, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(stream, "stream");

            if (settings != null)
            {
                this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding), settings);
            }
            else
            {
                this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream), null);
            }
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Load(XmlReader reader)" />
        ///     </code>
        /// </example>
        public void Load(XmlReader reader)
        {
            this.Load(reader, null);
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(XmlReader reader, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(reader, "reader");

            this.Load(new XPathDocument(reader), settings);
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Load(Uri source, ICredentials credentials, IWebProxy proxy)" />
        ///     </code>
        /// </example>
        public void Load(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            this.Load(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Load(Uri source, WebRequestOptions options)" />
        ///     </code>
        /// </example>
        public void Load(Uri source, WebRequestOptions options)
        {
            this.Load(source, options, null);
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/>
        ///                     the character encoding of the <paramref name="source"/> will be attempt to be determined automatically, otherwise the specified character encoding will be used.
        ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(
            Uri source,
            ICredentials credentials,
            IWebProxy proxy,
            SyndicationResourceLoadSettings settings)
        {
            this.Load(source, new WebRequestOptions(credentials, proxy), settings);
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/>
        ///                     the character encoding of the <paramref name="source"/> will be attempt to be determined automatically, otherwise the specified character encoding will be used.
        ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            XPathNavigator navigator = null;

            Guard.ArgumentNotNull(source, "source");

            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            if (settings.CharacterEncoding == System.Text.Encoding.UTF8)
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
            }
            else
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
            }

            this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator, source, options));
        }

        /// <summary>
        /// Loads this <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>The <see cref="AtomFeed"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event.
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>, you must wait for the load operation to complete before
        ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="AtomFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the LoadAsync method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="LoadAsync(Uri source, Object userToken)" />
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="FeedLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)" />
        ///     </code>
        /// </example>
        public void LoadAsync(Uri source, object userToken)
        {
            this.LoadAsync(source, null, userToken);
        }

        /// <summary>
        /// Loads this <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event.
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>, you must wait for the load operation to complete before
        ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="AtomFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, object userToken)
        {
            this.LoadAsync(source, settings, new WebRequestOptions(), userToken);
        }

        /// <summary>
        /// Loads this <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event.
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>,
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="LoadAsync(Uri, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="AtomFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        public void LoadAsync(
            Uri source,
            SyndicationResourceLoadSettings settings,
            ICredentials credentials,
            IWebProxy proxy,
            object userToken)
        {
            this.LoadAsync(source, settings, new WebRequestOptions(credentials, proxy), userToken);
        }

        /// <summary>
        /// Loads this <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event.
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>,
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="LoadAsync(Uri, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="AtomFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        public void LoadAsync(
            Uri source,
            SyndicationResourceLoadSettings settings,
            WebRequestOptions options,
            object userToken)
        {
            Guard.ArgumentNotNull(source, "source");

            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            if (this.LoadOperationInProgress)
            {
                throw new InvalidOperationException();
            }

            this.LoadOperationInProgress = true;

            this.AsyncLoadHasBeenCancelled = false;

            asyncHttpWebRequest = SyndicationEncodingUtility.CreateWebRequest(source, options);
            asyncHttpWebRequest.Timeout = Convert.ToInt32(
                settings.Timeout.TotalMilliseconds,
                NumberFormatInfo.InvariantInfo);

            object[] state = new object[6] { asyncHttpWebRequest, this, source, settings, options, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncLoadCallback), state);

            ThreadPool.RegisterWaitForSingleObject(
                result.AsyncWaitHandle,
                new WaitOrTimerCallback(this.AsyncTimeoutCallback),
                state,
                settings.Timeout,
                true);
        }

        /// <summary>
        /// Cancels an asynchronous operation to load this syndication resource.
        /// </summary>
        /// <remarks>
        ///     Use the LoadAsyncCancel method to cancel a pending <see cref="LoadAsync(Uri, object)"/> operation.
        ///     If there is a load operation in progress, this method releases resources used to execute the load operation.
        ///     If there is no load operation pending, this method does nothing.
        /// </remarks>
        public void LoadAsyncCancel()
        {
            if (this.LoadOperationInProgress && !this.AsyncLoadHasBeenCancelled)
            {
                this.AsyncLoadHasBeenCancelled = true;

                asyncHttpWebRequest.Abort();
            }
        }

        /// <summary>
        /// Removes the supplied <see cref="AtomEntry"/> from the current instance's <see cref="Entries"/> collection.
        /// </summary>
        /// <param name="entry">The <see cref="AtomEntry"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="AtomEntry"/> was removed from the <see cref="Entries"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Entries"/> collection of the current instance does not contain the specified <see cref="AtomEntry"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveEntry(AtomEntry entry)
        {
            bool wasRemoved = false;

            Guard.ArgumentNotNull(entry, "entry");

            if (((Collection<AtomEntry>)this.Entries).Contains(entry))
            {
                ((Collection<AtomEntry>)this.Entries).Remove(entry);
                wasRemoved = true;
            }

            return wasRemoved;
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
        /// Saves the syndication resource to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="XmlException">The operation would not result in well formed XML for the syndication resource.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Save method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Save(Stream stream)" />
        ///     </code>
        /// </example>
        public void Save(Stream stream)
        {
            this.Save(stream, null);
        }

        /// <summary>
        /// Saves the syndication resource to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomFeed"/> instance. This value can be <b>null</b>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="XmlException">The operation would not result in well formed XML for the syndication resource.</exception>
        public void Save(Stream stream, SyndicationResourceSaveSettings settings)
        {
            Guard.ArgumentNotNull(stream, "stream");

            if (settings == null)
            {
                settings = new SyndicationResourceSaveSettings();
            }

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = false;
            writerSettings.Indent = !settings.MinimizeOutputSize;
            writerSettings.Encoding = settings.CharacterEncoding;

            using (XmlWriter writer = XmlWriter.Create(stream, writerSettings))
            {
                this.Save(writer, settings);
            }
        }

        /// <summary>
        /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="XmlException">The operation would not result in well formed XML for the syndication resource.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Save method.">
        ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" region="Save(XmlWriter writer)" />
        ///     </code>
        /// </example>
        public void Save(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            this.Save(writer, new SyndicationResourceSaveSettings());
        }

        /// <summary>
        /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomFeed"/> instance.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="XmlException">The operation would not result in well formed XML for the syndication resource.</exception>
        public void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNull(settings, "settings");

            writer.WriteStartElement("feed", AtomUtility.AtomNamespace);

            if (settings.AutoDetectExtensions)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

                if (this.Generator != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Generator, settings.SupportedExtensions);
                }

                if (this.Icon != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Icon, settings.SupportedExtensions);
                }

                if (this.Id != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Id, settings.SupportedExtensions);
                }

                if (this.Logo != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Logo, settings.SupportedExtensions);
                }

                if (this.Rights != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Rights, settings.SupportedExtensions);
                }

                if (this.Subtitle != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Subtitle, settings.SupportedExtensions);
                }

                if (this.Title != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(this.Title, settings.SupportedExtensions);
                }

                foreach (AtomPersonConstruct author in this.Authors)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(author, settings.SupportedExtensions);
                }

                foreach (AtomCategory category in this.Categories)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(category, settings.SupportedExtensions);
                }

                foreach (AtomPersonConstruct contributor in this.Contributors)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(contributor, settings.SupportedExtensions);
                }

                foreach (AtomEntry entry in this.Entries)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(entry, settings.SupportedExtensions);
                }

                foreach (AtomLink link in this.Links)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(link, settings.SupportedExtensions);
                }
            }

            SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

            AtomUtility.WriteCommonObjectAttributes(this, writer);

            if (this.Id != null)
            {
                this.Id.WriteTo(writer);
            }

            if (this.Title != null)
            {
                this.Title.WriteTo(writer, "title");
            }

            if (this.UpdatedOn != DateTime.MinValue)
            {
                writer.WriteElementString(
                    "updated",
                    AtomUtility.AtomNamespace,
                    SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
            }

            this.WriteFeedOptionals(writer);
            this.WriteFeedCollections(writer);

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            foreach (AtomEntry entry in this.Entries)
            {
                entry.Save(writer, settings);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Raises the <see cref="AtomFeed.Loaded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
        protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e)
        {
            EventHandler<SyndicationResourceLoadedEventArgs> handler = null;

            handler = this.Loaded;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Called when a corresponding asynchronous load operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncLoadCallback(IAsyncResult result)
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            XPathNavigator navigator = null;
            WebRequest httpWebRequest = null;
            AtomFeed feed = null;
            Uri source = null;
            WebRequestOptions options = null;
            SyndicationResourceLoadSettings settings = null;

            if (result.IsCompleted)
            {
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest = parameters[0] as WebRequest;
                feed = parameters[1] as AtomFeed;
                source = parameters[2] as Uri;
                settings = parameters[3] as SyndicationResourceLoadSettings;
                options = parameters[4] as WebRequestOptions;
                object userToken = parameters[5];

                if (feed != null)
                {
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);

                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (settings != null)
                        {
                            encoding = settings.CharacterEncoding;
                        }

                        using (StreamReader streamReader = new StreamReader(stream, encoding))
                        {
                            XmlReaderSettings readerSettings = new XmlReaderSettings();
                            readerSettings.IgnoreComments = true;
                            readerSettings.IgnoreWhitespace = true;
                            readerSettings.DtdProcessing = DtdProcessing.Ignore;

                            using (XmlReader reader = XmlReader.Create(streamReader, readerSettings))
                            {
                                if (encoding == System.Text.Encoding.UTF8)
                                {
                                    navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
                                }
                                else
                                {
                                    navigator = SyndicationEncodingUtility.CreateSafeNavigator(
                                        source,
                                        options,
                                        settings.CharacterEncoding);
                                }

                                SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(
                                    navigator,
                                    settings);
                                adapter.Fill(feed, SyndicationContentFormat.Atom);

                                feed.OnFeedLoaded(
                                    new SyndicationResourceLoadedEventArgs(navigator, source, options, userToken));
                            }
                        }
                    }

                    feed.LoadOperationInProgress = false;
                }
            }
        }

        /// <summary>
        /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method each time it executes.</param>
        /// <param name="timedOut"><b>true</b> if the <see cref="WaitHandle"/> timed out; <b>false</b> if it was signaled.</param>
        private void AsyncTimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }

            this.LoadOperationInProgress = false;
        }

        /// <summary>
        /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomFeed"/>.</param>
        /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="AtomFeed.Loaded"/> event.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        private void Load(
            XPathNavigator navigator,
            SyndicationResourceLoadSettings settings,
            SyndicationResourceLoadedEventArgs eventData)
        {
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(settings, "settings");
            Guard.ArgumentNotNull(eventData, "eventData");

            SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(navigator, settings);
            adapter.Fill(this, SyndicationContentFormat.Atom);

            this.OnFeedLoaded(eventData);
        }

        /// <summary>
        /// Saves the current <see cref="AtomFeed"/> collection entities to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        private void WriteFeedCollections(XmlWriter writer)
        {
            SyndicationResourceSaveSettings settings = new SyndicationResourceSaveSettings();
            settings.AutoDetectExtensions = false;

            Guard.ArgumentNotNull(writer, "writer");

            foreach (AtomLink link in this.Links)
            {
                link.WriteTo(writer);
            }

            foreach (AtomPersonConstruct author in this.Authors)
            {
                author.WriteTo(writer, "author");
            }

            foreach (AtomCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }

            foreach (AtomPersonConstruct contributor in this.Contributors)
            {
                contributor.WriteTo(writer, "contributor");
            }
        }

        /// <summary>
        /// Saves the current <see cref="AtomFeed"/> optional entities to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        private void WriteFeedOptionals(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            if (this.Generator != null)
            {
                this.Generator.WriteTo(writer);
            }

            if (this.Icon != null)
            {
                this.Icon.WriteTo(writer);
            }

            if (this.Logo != null)
            {
                this.Logo.WriteTo(writer);
            }

            if (this.Rights != null)
            {
                this.Rights.WriteTo(writer, "rights");
            }

            if (this.Subtitle != null)
            {
                this.Subtitle.WriteTo(writer, "subtitle");
            }
        }
    }
}