﻿namespace Argotic.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;

    /// <summary>
    /// Represents information about the meta-data and contents associated to an <see cref="RssFeed"/>.
    /// </summary>
    /// <seealso cref="RssFeed"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the RssChannel class.">
    ///         <code source="..\..\Argotic.Examples\Core\Rss\RssChannelExample.cs" region="RssChannel" />
    ///     </code>
    /// </example>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public class RssChannel : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold categories or tags to which the channel belongs.
        /// </summary>
        private Collection<RssCategory> channelCategories;

        /// <summary>
        /// Private member to hold the human-readable copyright statement that applies to the feed.
        /// </summary>
        private string channelCopyrightNotice = string.Empty;

        /// <summary>
        /// Private member to hold character data that provides a human-readable characterization or summary of the feed.
        /// </summary>
        private string channelDescription = string.Empty;

        /// <summary>
        /// Private member to hold a value that credits the software that created the feed.
        /// </summary>
        private string channelGenerator = string.Format(
            null,
            "Argotic Syndication Framework {0}, http://www.codeplex.com/Argotic",
            System.Reflection.Assembly.GetAssembly(typeof(RssChannel)).GetName().Version.ToString(4));

        /// <summary>
        /// Private member to hold the collection of items that comprise the distinct content published in the feed.
        /// </summary>
        private IEnumerable<RssItem> channelItems;

        /// <summary>
        /// Private member to hold the URL of the web site associated with the feed.
        /// </summary>
        private Uri channelLink;

        /// <summary>
        /// Private member to hold the e-mail address of the person to contact regarding the editorial content of the feed.
        /// </summary>
        private string channelManagingEditor = string.Empty;

        /// <summary>
        /// Private member to hold an advisory label for the content in a feed.
        /// </summary>
        private string channelRating = string.Empty;

        /// <summary>
        /// Private member to hold the days of the week during which the feed is not updated.
        /// </summary>
        private Collection<DayOfWeek> channelSkipDays;

        /// <summary>
        /// Private member to hold the hours of the day during which the feed is not updated.
        /// </summary>
        private Collection<int> channelSkipHours;

        /// <summary>
        /// Private member to hold character data that provides the name of the feed.
        /// </summary>
        private string channelTitle = string.Empty;

        /// <summary>
        /// Private member to hold the e-mail address of the person to contact about technical issues regarding the feed.
        /// </summary>
        private string channelWebmaster = string.Empty;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RssChannel"/> class.
        /// </summary>
        public RssChannel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssChannel"/> class using the supplied link, title, and description.
        /// </summary>
        /// <param name="link">A <see cref="Uri"/> that represents the URL of the web site associated with this feed.</param>
        /// <param name="title">Character data that provides the name of this feed.</param>
        /// <param name="description">Character data that provides a human-readable characterization or summary of this feed.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="title"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="description"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="description"/> is an empty string.</exception>
        public RssChannel(Uri link, string title, string description)
        {
            this.Link = link;
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Gets the URL of the RSS specification implemented by the software that created this feed.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the RSS specification implemented by the software that created this feed.</value>
        public static Uri Documentation { get; } = new Uri("http://www.rssboard.org/rss-specification");

        /// <summary>
        /// Gets the categories or tags to which this channel belongs.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="RssCategory"/> objects that represent the categories to which this channel belongs. The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<RssCategory> Categories
        {
            get
            {
                return this.channelCategories ?? (this.channelCategories = new Collection<RssCategory>());
            }
        }

        /// <summary>
        /// Gets or sets the meta-data clients can use to register to be notified of updates to this feed.
        /// </summary>
        /// <value>
        ///     A <see cref="RssCloud"/> object that represents the meta-data clients can use for monitoring
        ///     updates to this feed using a web service that implements the RssCloud application programming interface.
        ///     The default value is a <b>null</b> reference.
        /// </value>
        public RssCloud Cloud { get; set; }

        /// <summary>
        /// Gets or sets the human-readable copyright statement that applies to this feed.
        /// </summary>
        /// <value>The human-readable copyright statement that applies to this feed.</value>
        /// <remarks>
        ///     When a feed lacks a copyright element, aggregators <i>should not</i> assume that is in the public domain and can be republished and redistributed without restriction.
        /// </remarks>
        public string Copyright
        {
            get
            {
                return this.channelCopyrightNotice;
            }

            set
            {
                this.channelCopyrightNotice = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets character data that provides a human-readable characterization or summary of this feed.
        /// </summary>
        /// <value>Character data that provides a human-readable characterization or summary of this feed.</value>
        /// <remarks>
        ///     The description character data <b>must</b> be suitable for presentation as HTML.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Description
        {
            get
            {
                return this.channelDescription;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.channelDescription = value.Trim();
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
                return this.objectSyndicationExtensions ?? (this.objectSyndicationExtensions = new Collection<ISyndicationExtension>());
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.objectSyndicationExtensions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that credits the software that created this feed.
        /// </summary>
        /// <value>A value that credits the software that created this feed. The default value is an agent that describes this syndication framework.</value>
        public string Generator
        {
            get
            {
                return this.channelGenerator;
            }

            set
            {
                this.channelGenerator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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
        /// Gets or sets the graphical logo for this feed.
        /// </summary>
        /// <value>
        ///     A <see cref="RssImage"/> object that represents the graphical logo for this feed. The default value is a <b>null</b> reference.
        /// </value>
        public RssImage Image { get; set; }

        /// <summary>
        /// Gets or sets the distinct content published in this feed.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="RssItem"/> objects that represent distinct content published in this feed.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="RssItem"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<RssItem> Items
        {
            get
            {
                return this.channelItems ?? (this.channelItems = new Collection<RssItem>());
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.channelItems = value;
            }
        }

        /// <summary>
        /// Gets or sets the natural language employed in this feed.
        /// </summary>
        /// <value>A <see cref="CultureInfo"/> object that represents the natural language employed in this feed. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     The language <b>must</b> be identified using one of the <a href="http://www.rssboard.org/rss-language-codes">RSS language codes</a>
        ///     or a <a href="http://www.w3.org/TR/REC-html40/struct/dirlang.html#langcodes">W3C language code</a>.
        /// </remarks>
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the last date and time the content of this feed was updated.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> object that represents the last date and time the content of this feed was updated.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no last build date was specified.
        /// </value>
        public DateTime LastBuildDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the URL of the web site associated with this feed.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the web site associated with this feed.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Link
        {
            get
            {
                return this.channelLink;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.channelLink = value;
            }
        }

        /// <summary>
        /// Gets or sets the e-mail address of the person to contact regarding the editorial content of this feed.
        /// </summary>
        /// <value>The e-mail address of the person to contact regarding the editorial content of this feed.</value>
        /// <remarks>
        ///     <para>
        ///         There is no requirement to follow a specific format for email addresses. Publishers can format addresses according to the RFC 2822 Address Specification,
        ///         the RFC 2368 guidelines for mailto links, or some other scheme. The recommended format for e-mail addresses is <i>username@hostname.tld (Real Name)</i>.
        ///     </para>
        /// </remarks>
        public string ManagingEditor
        {
            get
            {
                return this.channelManagingEditor;
            }

            set
            {
                this.channelManagingEditor = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the publication date and time of this feed's content.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> object that represents the publication date and time of this feed's content.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no publication date was specified.
        /// </value>
        /// <remarks>
        ///     Publishers of daily, weekly or monthly periodicals can use this element to associate feed items with the date they most recently went to press.
        /// </remarks>
        public DateTime PublicationDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets an advisory label for the content in this feed.
        /// </summary>
        /// <value>A string value, formatted according to the specification for the Platform for Internet Content Selection (PICS), that supplies an advisory label for the content in this feed.</value>
        /// <remarks>
        ///     <para>
        ///         For further information on the <b>Platform for Internet Content Selection (PICS)</b> advisory label formatting specification,
        ///         see <a href="http://www.w3.org/TR/REC-PICS-labels#General">http://www.w3.org/TR/REC-PICS-labels#General</a>.
        ///     </para>
        /// </remarks>
        public string Rating
        {
            get
            {
                return this.channelRating;
            }

            set
            {
                this.channelRating = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a URL that describes the feed itself.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a URL that points to where this feed can be retrieved from.</value>
        /// <remarks>
        ///     <para>
        ///         Identifying a feed's URL within the feed makes it more portable, self-contained, and easier to cache.
        ///         For these reasons, a feed <i>should</i> provide a value for <see cref="SelfLink"/> that is used for this purpose.
        ///     </para>
        ///     <para>
        ///         Identifying a self referential link is achieved by including a <i>atom:link</i> element within the channel.
        ///         See <a href="http://www.rssboard.org/rss-profile#namespace-elements-atom-link">RSS Profile</a> for more information.
        ///     </para>
        /// </remarks>
        public Uri SelfLink { get; set; }

        /// <summary>
        /// Gets the days of the week during which this feed is not updated.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="DayOfWeek"/> enumeration values that indicate the days of the week during which this feed is not updated.</value>
        /// <remarks>
        ///     <see cref="DayOfWeek"/> enumeration values within this collection <b>must not</b> be duplicated.
        /// </remarks>
        public Collection<DayOfWeek> SkipDays
        {
            get
            {
                return this.channelSkipDays ?? (this.channelSkipDays = new Collection<DayOfWeek>());
            }
        }

        /// <summary>
        /// Gets the hours of the day during which this feed is not updated.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="int"/> objects that indicate the hours of the day during which this feed is not updated.</value>
        /// <remarks>
        ///     Values from 0 to 23 are permitted, with 0 representing midnight. Integer values within this collection <b>must not</b> be duplicated.
        /// </remarks>
        public Collection<int> SkipHours
        {
            get
            {
                return this.channelSkipHours ?? (this.channelSkipHours = new Collection<int>());
            }
        }

        /// <summary>
        /// Gets or sets a form to submit a text query to this feed's publisher over the Common Gateway Interface (CGI).
        /// </summary>
        /// <value>
        ///     A <see cref="TextInput"/> object that represents a form to submit a text query to this feed's publisher over the Common Gateway Interface (CGI).
        ///     The default value is a <b>null</b> reference.
        /// </value>
        public RssTextInput TextInput { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of minutes to cache the data before a client should request it again.
        /// </summary>
        /// <value>
        ///     The maximum number of minutes to cache the data before an aggregator should request it again.
        ///     The default value is <see cref="int.MinValue"/>, which indicates no time-to-live was specified.
        /// </value>
        /// <remarks>
        ///     Aggregators that support this property <i>should</i> treat it as a publisher's suggestion of a feed's update frequency, not a hard rule.
        /// </remarks>
        public int TimeToLive { get; set; } = int.MinValue;

        /// <summary>
        /// Gets or sets character data that provides the name of this feed.
        /// </summary>
        /// <value>Character data that provides the name of this feed.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Title
        {
            get
            {
                return this.channelTitle;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.channelTitle = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the e-mail address of the person to contact about technical issues regarding this feed.
        /// </summary>
        /// <value>The e-mail address of the person to contact about technical issues regarding this feed.</value>
        /// <remarks>
        ///     <para>
        ///         There is no requirement to follow a specific format for email addresses. Publishers can format addresses according to the RFC 2822 Address Specification,
        ///         the RFC 2368 guidelines for mailto links, or some other scheme. The recommended format for e-mail addresses is <i>username@hostname.tld (Real Name)</i>.
        ///     </para>
        /// </remarks>
        public string Webmaster
        {
            get
            {
                return this.channelWebmaster;
            }

            set
            {
                this.channelWebmaster = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RssItem"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to get or set.</param>
        /// <returns>The <see cref="RssItem"/> at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="RssChannel.Items"/>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public RssItem this[int index]
        {
            get
            {
                return ((Collection<RssItem>)this.Items)[index];
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                ((Collection<RssItem>)this.Items)[index] = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(RssChannel first, RssChannel second)
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
        public static bool operator >(RssChannel first, RssChannel second)
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
        public static bool operator !=(RssChannel first, RssChannel second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(RssChannel first, RssChannel second)
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
        /// Compares two specified <see cref="Collection{RssItem}"/> collections.
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
        public static int CompareSequence(Collection<RssItem> source, Collection<RssItem> target)
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
        /// Adds the supplied <see cref="RssItem"/> to the current instance's <see cref="Items"/> collection.
        /// </summary>
        /// <param name="item">The <see cref="RssItem"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="RssItem"/> was added to the <see cref="Items"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="item"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddItem(RssItem item)
        {
            bool wasAdded = false;

            Guard.ArgumentNotNull(item, "item");
            ((Collection<RssItem>)this.Items).Add(item);
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

            RssChannel value = obj as RssChannel;

            if (value != null)
            {
                int result = string.Compare(this.Copyright, value.Copyright, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Generator, value.Generator, StringComparison.OrdinalIgnoreCase);
                result = result | this.LastBuildDate.CompareTo(value.LastBuildDate);
                result = result | Uri.Compare(this.Link, value.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(
                             this.ManagingEditor,
                             value.ManagingEditor,
                             StringComparison.OrdinalIgnoreCase);
                result = result | this.PublicationDate.CompareTo(value.PublicationDate);
                result = result | string.Compare(this.Rating, value.Rating, StringComparison.OrdinalIgnoreCase);
                result = result | this.TimeToLive.CompareTo(value.TimeToLive);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Webmaster, value.Webmaster, StringComparison.OrdinalIgnoreCase);

                if (this.Cloud != null)
                {
                    result = result | this.Cloud.CompareTo(value.Cloud);
                }
                else if (this.Cloud == null && value.Cloud != null)
                {
                    result = result | -1;
                }

                if (this.Image != null)
                {
                    result = result | this.Image.CompareTo(value.Image);
                }
                else if (this.Image == null && value.Image != null)
                {
                    result = result | -1;
                }

                if (this.Language != null)
                {
                    if (value.Language != null)
                    {
                        result = result | string.Compare(this.Language.Name, value.Language.Name, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        result = result | 1;
                    }
                }
                else if (this.Language == null && value.Language != null)
                {
                    result = result | -1;
                }

                if (this.TextInput != null)
                {
                    result = result | this.TextInput.CompareTo(value.TextInput);
                }
                else if (this.TextInput == null && value.TextInput != null)
                {
                    result = result | -1;
                }

                result = result | RssFeed.CompareSequence(this.Categories, value.Categories);
                result = result | CompareSequence((Collection<RssItem>)this.Items, (Collection<RssItem>)value.Items);
                result = result | ComparisonUtility.CompareSequence(this.SkipDays, value.SkipDays);
                result = result | ComparisonUtility.CompareSequence(this.SkipHours, value.SkipHours);

                return result;
            }

            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is RssChannel))
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
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Loads this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            return this.Load(source, new SyndicationResourceLoadSettings());
        }

        /// <summary>
        /// Loads this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            XmlNamespaceManager manager = new XmlNamespaceManager(source.NameTable);
            manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            XPathNavigator descriptionNavigator = source.SelectSingleNode("description", manager);
            XPathNavigator linkNavigator = source.SelectSingleNode("link", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("title", manager);

            if (descriptionNavigator != null && !string.IsNullOrEmpty(descriptionNavigator.Value))
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

            if (titleNavigator != null && !string.IsNullOrEmpty(titleNavigator.Value))
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }

            if (this.LoadOptionals(source, manager, settings))
            {
                wasLoaded = true;
            }

            if (this.LoadCollections(source, manager, settings))
            {
                wasLoaded = true;
            }

            if (this.LoadProfile(source, manager, settings))
            {
                wasLoaded = true;
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
        /// Removes the supplied <see cref="RssItem"/> from the current instance's <see cref="Items"/> collection.
        /// </summary>
        /// <param name="item">The <see cref="RssItem"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="RssItem"/> was removed from the <see cref="Items"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Items"/> collection of the current instance does not contain the specified <see cref="RssItem"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="item"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveItem(RssItem item)
        {
            bool wasRemoved = false;

            Guard.ArgumentNotNull(item, "item");

            if (((Collection<RssItem>)this.Items).Contains(item))
            {
                ((Collection<RssItem>)this.Items).Remove(item);
                wasRemoved = true;
            }

            return wasRemoved;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="RssChannel"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="RssChannel"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, Indent = true, OmitXmlDeclaration = true };

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
        /// Saves the current <see cref="RssChannel"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("channel");
            writer.WriteElementString("title", this.Title);
            writer.WriteElementString("link", this.Link != null ? this.Link.ToString() : string.Empty);
            writer.WriteElementString("description", this.Description);

            if (this.Cloud != null)
            {
                this.Cloud.WriteTo(writer);
            }

            if (!string.IsNullOrEmpty(this.Copyright))
            {
                writer.WriteElementString("copyright", this.Copyright);
            }

            writer.WriteElementString("docs", Documentation.ToString());

            if (!string.IsNullOrEmpty(this.Generator))
            {
                writer.WriteElementString("generator", this.Generator);
            }

            if (this.Image != null)
            {
                this.Image.WriteTo(writer);
            }

            if (this.Language != null)
            {
                writer.WriteElementString("language", this.Language.Name);
            }

            if (this.LastBuildDate != DateTime.MinValue)
            {
                writer.WriteElementString(
                    "lastBuildDate",
                    SyndicationDateTimeUtility.ToRfc822DateTime(this.LastBuildDate));
            }

            if (!string.IsNullOrEmpty(this.ManagingEditor))
            {
                writer.WriteElementString("managingEditor", this.ManagingEditor);
            }

            if (this.PublicationDate != DateTime.MinValue)
            {
                writer.WriteElementString("pubDate", SyndicationDateTimeUtility.ToRfc822DateTime(this.PublicationDate));
            }

            if (!string.IsNullOrEmpty(this.Rating))
            {
                writer.WriteElementString("rating", this.Rating);
            }

            if (this.TextInput != null)
            {
                this.TextInput.WriteTo(writer);
            }

            if (this.TimeToLive != int.MinValue)
            {
                writer.WriteElementString("ttl", this.TimeToLive.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (!string.IsNullOrEmpty(this.Webmaster))
            {
                writer.WriteElementString("webMaster", this.Webmaster);
            }

            if (this.SkipDays.Count > 0)
            {
                writer.WriteStartElement("skipDays");

                foreach (DayOfWeek day in this.SkipDays)
                {
                    writer.WriteElementString("day", day.ToString());
                }

                writer.WriteEndElement();
            }

            if (this.SkipHours.Count > 0)
            {
                writer.WriteStartElement("skipHours");

                foreach (int hour in this.SkipHours)
                {
                    writer.WriteElementString("hour", hour.ToString(NumberFormatInfo.InvariantInfo));
                }

                writer.WriteEndElement();
            }

            foreach (RssCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }

            if (this.SelfLink != null)
            {
                writer.WriteStartElement("link", "http://www.w3.org/2005/Atom");
                writer.WriteAttributeString("href", this.SelfLink.ToString());
                writer.WriteAttributeString("rel", "self");
                writer.WriteAttributeString("type", "application/rss+xml");
                writer.WriteEndElement();
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            foreach (RssItem item in this.Items)
            {
                item.WriteTo(writer);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads the collection elements of this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     <para>
        ///         This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
        ///     </para>
        ///     <para>
        ///         The number of <see cref="RssChannel.Items"/> that are loaded is limited based on the <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadCollections(
            XPathNavigator source,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");
            XPathNodeIterator categoryIterator = source.Select("category", manager);
            XPathNodeIterator skipDaysIterator = source.Select("skipDays/day", manager);
            XPathNodeIterator skipHoursIterator = source.Select("skipHours/hour", manager);
            XPathNodeIterator itemIterator = source.Select("item", manager);

            if (categoryIterator != null && categoryIterator.Count > 0)
            {
                while (categoryIterator.MoveNext())
                {
                    RssCategory category = new RssCategory();

                    if (category.Load(categoryIterator.Current, settings))
                    {
                        this.Categories.Add(category);
                        wasLoaded = true;
                    }
                }
            }

            if (skipDaysIterator != null && skipDaysIterator.Count > 0)
            {
                while (skipDaysIterator.MoveNext())
                {
                    if (!string.IsNullOrEmpty(skipDaysIterator.Current.Value))
                    {
                        try
                        {
                            DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), skipDaysIterator.Current.Value, true);

                            if (!this.SkipDays.Contains(day))
                            {
                                this.SkipDays.Add(day);
                                wasLoaded = true;
                            }
                        }
                        catch (ArgumentException)
                        {
                            System.Diagnostics.Trace.TraceWarning("RssChannel unable to determine DayOfWeek with a name of {0}.", skipDaysIterator.Current.Value);
                        }
                    }
                }
            }

            if (skipHoursIterator != null && skipHoursIterator.Count > 0)
            {
                while (skipHoursIterator.MoveNext())
                {
                    if (int.TryParse(skipHoursIterator.Current.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var hour))
                    {
                        if (!this.SkipHours.Contains(hour) && hour >= 0 && hour <= 23)
                        {
                            this.SkipHours.Add(hour);
                            wasLoaded = true;
                        }
                        else
                        {
                            System.Diagnostics.Trace.TraceWarning("RssChannel unable to add duplicate or out-of-range skip hour with a value of {0}.", hour);
                        }
                    }
                }
            }

            if (itemIterator != null && itemIterator.Count > 0)
            {
                int counter = 0;
                while (itemIterator.MoveNext())
                {
                    RssItem item = new RssItem();
                    counter++;

                    if (item.Load(itemIterator.Current, settings))
                    {
                        if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                        {
                            break;
                        }

                        ((Collection<RssItem>)this.Items).Add(item);
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads the optional elements of this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator cloudNavigator = source.SelectSingleNode("cloud", manager);
            XPathNavigator copyrightNavigator = source.SelectSingleNode("copyright", manager);
            XPathNavigator generatorNavigator = source.SelectSingleNode("generator", manager);
            XPathNavigator imageNavigator = source.SelectSingleNode("image", manager);
            XPathNavigator languageNavigator = source.SelectSingleNode("language", manager);
            XPathNavigator lastBuildDateNavigator = source.SelectSingleNode("lastBuildDate", manager);
            XPathNavigator managingEditorNavigator = source.SelectSingleNode("managingEditor", manager);
            XPathNavigator publicationNavigator = source.SelectSingleNode("pubDate", manager);
            XPathNavigator ratingNavigator = source.SelectSingleNode("rating", manager);
            XPathNavigator textInputNavigator = source.SelectSingleNode("textInput", manager);
            XPathNavigator timeToLiveNavigator = source.SelectSingleNode("ttl", manager);
            XPathNavigator webMasterNavigator = source.SelectSingleNode("webMaster", manager);

            if (cloudNavigator != null)
            {
                RssCloud cloud = new RssCloud();

                if (cloud.Load(cloudNavigator, settings))
                {
                    this.Cloud = cloud;
                    wasLoaded = true;
                }
            }

            if (copyrightNavigator != null)
            {
                this.Copyright = copyrightNavigator.Value;
                wasLoaded = true;
            }

            if (generatorNavigator != null)
            {
                this.Generator = generatorNavigator.Value;
                wasLoaded = true;
            }

            if (imageNavigator != null)
            {
                RssImage image = new RssImage();

                if (image.Load(imageNavigator, settings))
                {
                    this.Image = image;
                    wasLoaded = true;
                }
            }

            if (languageNavigator != null && !string.IsNullOrEmpty(languageNavigator.Value))
            {
                try
                {
                    CultureInfo language = new CultureInfo(languageNavigator.Value);
                    this.Language = language;
                    wasLoaded = true;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "RssChannel unable to determine CultureInfo with a name of {0}.",
                        languageNavigator.Value);
                }
            }

            if (lastBuildDateNavigator != null)
            {
                DateTime lastBuildDate;

                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(lastBuildDateNavigator.Value, out lastBuildDate))
                {
                    this.LastBuildDate = lastBuildDate;
                    wasLoaded = true;
                }
            }

            if (managingEditorNavigator != null)
            {
                this.ManagingEditor = managingEditorNavigator.Value;
                wasLoaded = true;
            }

            if (publicationNavigator != null)
            {
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out var publicationDate))
                {
                    this.PublicationDate = publicationDate;
                    wasLoaded = true;
                }
            }

            if (ratingNavigator != null)
            {
                this.Rating = ratingNavigator.Value;
                wasLoaded = true;
            }

            if (textInputNavigator != null)
            {
                RssTextInput textInput = new RssTextInput();
                if (textInput.Load(textInputNavigator, settings))
                {
                    this.TextInput = textInput;
                    wasLoaded = true;
                }
            }

            if (timeToLiveNavigator != null)
            {
                if (int.TryParse(timeToLiveNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var timeToLive))
                {
                    this.TimeToLive = timeToLive;
                    wasLoaded = true;
                }
            }

            if (webMasterNavigator != null)
            {
                this.Webmaster = webMasterNavigator.Value;
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads the optional RSS Profile elements of this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadProfile(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNodeIterator atomLinkIterator = source.Select("atom:link", manager);

            if (atomLinkIterator != null && atomLinkIterator.Count > 0)
            {
                while (atomLinkIterator.MoveNext())
                {
                    if (atomLinkIterator.Current.HasAttributes)
                    {
                        string relAttribute = atomLinkIterator.Current.GetAttribute("rel", string.Empty);

                        if (string.Compare(relAttribute, "self", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string hrefAttribute = atomLinkIterator.Current.GetAttribute("href", string.Empty);

                            if (!string.IsNullOrEmpty(hrefAttribute))
                            {
                                if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out var atomLink))
                                {
                                    this.SelfLink = atomLink;
                                    wasLoaded = true;
                                }
                            }

                            break;
                        }
                    }
                }
            }

            return wasLoaded;
        }
    }
}