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
    /// Represents a discrete entity within an <see cref="OpmlDocument"/>.
    /// </summary>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the OpmlOutline class.">
    ///         <code source="..\..\Argotic.Examples\Core\Opml\OpmlOutlineExample.cs" region="OpmlOutline" />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
    [Serializable]
    public class OpmlOutline : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Private member to hold a collection of key/value pairs that represent custom attributes applied to the outline.
        /// </summary>
        private Dictionary<string, string> outlineAttributes;

        /// <summary>
        /// Private member to hold a collection that describes the categorization taxonomy applied to the outline.
        /// </summary>
        private Collection<string> outlineCategories;

        /// <summary>
        /// Private member to hold a collection of outlines that are children of the outline.
        /// </summary>
        private Collection<OpmlOutline> outlineSubordinateOutlines;

        /// <summary>
        /// Private member to hold the textual content of the outline.
        /// </summary>
        private string outlineText = string.Empty;

        /// <summary>
        /// Private member to hold a value indicating how the outline's attributes are interpreted.
        /// </summary>
        private string outlineType = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlOutline"/> class.
        /// </summary>
        public OpmlOutline()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpmlOutline"/> class using the supplied text.
        /// </summary>
        /// <param name="text">The textual content of this outline.</param>
        /// <remarks>
        ///     Textual values <i>may</i> contain encoded HTML markup.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        public OpmlOutline(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Gets a collection of key/value string pairs that represent custom attributes applied to this outline.
        /// </summary>
        /// <value>A <see cref="Dictionary{T, T}"/> of strings that represent custom attributes applied to this outline.</value>
        /// <remarks>
        ///     The attributes <b>text</b>, <b>type</b>, <b>isComment</b>, <b>isBreakpoint</b>, <b>created</b>, and <b>category</b> are treated as special
        ///     within the OPML specification. Use the class properties that represent these attributes instead of adding them to this collection.
        /// </remarks>
        public Dictionary<string, string> Attributes
        {
            get
            {
                return this.outlineAttributes ?? (this.outlineAttributes = new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Gets a collection that describes the categorization taxonomy applied to this outline.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> of strings that represent the categorization taxonomy applied to this outline.</value>
        /// <remarks>
        ///     Categories are represented as slash-delimited strings, in the format defined by the <a href="http://cyber.law.harvard.edu/rss/rss.html#ltcategorygtSubelementOfLtitemgt">RSS 2.0 category element</a>.
        ///     To represent a <i>tag</i>, the category string should contain <u>no</u> slashes.
        /// </remarks>
        public Collection<string> Categories
        {
            get
            {
                return this.outlineCategories ?? (this.outlineCategories = new Collection<string>());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how this outline's attributes should be interpreted.
        /// </summary>
        /// <value>A value indicating how this outline's attributes should be interpreted.</value>
        public string ContentType
        {
            get
            {
                return this.outlineType;
            }

            set
            {
                this.outlineType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a date-time indicating when this outline was created.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates when this outline was created.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime CreatedOn { get; set; } = DateTime.MinValue;

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
        /// Gets or sets a value indicating whether gets or sets a value indicating if a breakpoint is set on this outline.
        /// </summary>
        /// <value><b>true</b> if a breakpoint is set on this outline; otherwise <b>false</b>.</value>
        /// <remarks>
        ///     This property is mainly necessary for outlines used to edit scripts. If it's not present, the value is <b>false</b>.
        /// </remarks>
        public bool HasBreakpoint { get; set; }

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
        /// Gets or sets a value indicating whether this outline is commented.
        /// </summary>
        /// <value><b>true</b> if this outline is commented; otherwise <b>false</b>.</value>
        /// <remarks>
        ///     By convention if an outline is commented, all subordinate outlines are considered to also be commented. If it's not present, the value is <b>false</b>.
        /// </remarks>
        public bool IsCommented { get; set; }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if this outline represents an inclusion.
        /// </summary>
        /// <value><b>true</b> if the <see cref="ContentType"/> is <i>include</i> or <i>link</i>; otherwise <b>false</b>.</value>
        /// <seealso cref="OpmlOutline.CreateInclusionOutline(string, Uri)"/>
        public bool IsInclusionOutline
        {
            get
            {
                return string.Compare(this.ContentType, "include", StringComparison.OrdinalIgnoreCase) == 0
                       || string.Compare(this.ContentType, "link", StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if this outline represents a subscription list.
        /// </summary>
        /// <value><b>true</b> if the <see cref="ContentType"/> is <i>rss</i> or <i>feed</i>; otherwise <b>false</b>.</value>
        /// <seealso cref="OpmlOutline.CreateSubscriptionListOutline(string, string, Uri)"/>
        public bool IsSubscriptionListOutline
        {
            get
            {
                return string.Compare(this.ContentType, "rss", StringComparison.OrdinalIgnoreCase) == 0
                       || string.Compare(this.ContentType, "feed", StringComparison.OrdinalIgnoreCase) == 0;
            }
        }

        /// <summary>
        /// Gets a collection of outlines that are children of this outline.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> of <see cref="OpmlOutline"/> objects that represent the children of this outline.</value>
        public Collection<OpmlOutline> Outlines
        {
            get
            {
                return this.outlineSubordinateOutlines ?? (this.outlineSubordinateOutlines = new Collection<OpmlOutline>());
            }
        }

        /// <summary>
        /// Gets or sets the textual content of this outline.
        /// </summary>
        /// <value>The textual content of this outline.</value>
        /// <remarks>
        ///     Textual values <i>may</i> contain encoded HTML markup.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Text
        {
            get
            {
                return this.outlineText;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.outlineText = value.Trim();
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(OpmlOutline first, OpmlOutline second)
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
        public static bool operator >(OpmlOutline first, OpmlOutline second)
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
        public static bool operator !=(OpmlOutline first, OpmlOutline second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(OpmlOutline first, OpmlOutline second)
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
        /// Compares two specified <see cref="Collection{OpmlOutline}"/> collections.
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
        public static int CompareSequence(Collection<OpmlOutline> source, Collection<OpmlOutline> target)
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
        /// Creates a new <see cref="OpmlOutline"/> that represents an inclusion outline using the supplied parameters.
        /// </summary>
        /// <param name="text">The textual content of the outline.</param>
        /// <param name="url">A <see cref="Uri"/> that represents an http address.</param>
        /// <returns>A new <see cref="OpmlOutline"/> object that represents an inclusion outline, initialized using the supplied parameters.</returns>
        /// <remarks>
        ///     <para>
        ///         When a outline is expanded in an outliner, if the <paramref name="url"/> ends with <i>.opml</i>, the outline expands in place. This is called <b>inclusion</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="url"/> does not end with <i>.opml</i>, the link is assumed to point to something that can be displayed in a web browser.
        ///     </para>
        ///     <para>The difference between <b>link</b> and <b>include</b> is that <i>link</i> may point to something that is displayed in a web browser, and <i>include</i> always points to an OPML file.</para>
        ///     <para>
        ///         This method will create an <see cref="OpmlOutline"/> with a <see cref="ContentType"/> of <b>include</b> if the <paramref name="url"/> ends with <i>.opml</i>,
        ///         otherwise the <see cref="ContentType"/> will have a value of <b>link</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        public static OpmlOutline CreateInclusionOutline(string text, Uri url)
        {
            OpmlOutline outline = new OpmlOutline();

            Guard.ArgumentNotNullOrEmptyString(text, "text");
            Guard.ArgumentNotNull(url, "url");

            outline.Text = text;
            outline.ContentType = url.ToString().EndsWith(".opml", StringComparison.OrdinalIgnoreCase) ? "include" : "link";
            outline.Attributes.Add("url", url.ToString());

            return outline;
        }

        /// <summary>
        /// Creates a new <see cref="OpmlOutline"/> that represents a subscription list outline using the supplied parameters.
        /// </summary>
        /// <param name="text">The textual content of the outline.</param>
        /// <param name="type">The syndication format of the feed being pointed to. Permissible values include <i>rss</i> or <i>feed</i>.</param>
        /// <param name="xmlUrl">A <see cref="Uri"/> that represents the http address of the feed.</param>
        /// <returns>A new <see cref="OpmlOutline"/> object that represents a subscription list outline, initialized using the supplied parameters.</returns>
        /// <remarks>
        ///     <para>
        ///         A subscription list is a possibly multiple-level list of subscriptions to feeds. Each sub-element of the body of the OPML document
        ///         is a node of type <i>rss</i> or an outline element that contains nodes of type <i>rss</i>.
        ///     </para>
        ///     <para>
        ///         Today, most subscription lists are a flat sequence of <i>rss</i> nodes, but some aggregators allow categorized subscription lists
        ///         that are arbitrarily structured. A validator may flag these files, warning that some processors may not understand and preserve the structure.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlUrl"/> is a null reference (Nothing in Visual Basic).</exception>
        public static OpmlOutline CreateSubscriptionListOutline(string text, string type, Uri xmlUrl)
        {
            return CreateSubscriptionListOutline(text, type, xmlUrl, null, string.Empty, string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Creates a new <see cref="OpmlOutline"/> that represents a subscription list outline using the supplied parameters.
        /// </summary>
        /// <param name="text">The textual content of the outline.</param>
        /// <param name="type">The syndication format of the feed being pointed to. Permissible values include <i>rss</i> or <i>feed</i>.</param>
        /// <param name="xmlUrl">A <see cref="Uri"/> that represents the http address of the feed.</param>
        /// <param name="htmlUrl">A <see cref="Uri"/> that represents the web site that hosts the feed. This value can be <b>null</b>.</param>
        /// <param name="version">
        ///     The version of the syndication format for the feed that's being pointed to.
        ///     Permissible values include <i>RSS</i>, <i>RSS1</i>, <i>scriptingNews</i>, or a custom version identifier for the feed.
        ///     This value can be an empty string.
        /// </param>
        /// <param name="title">The title of the feed. This value can be an empty string.</param>
        /// <param name="description">The description of the feed. This value can be an empty string.</param>
        /// <param name="language">A <see cref="CultureInfo"/> that represents the natural or formal language in which the feed is written. This value can be <b>null</b>.</param>
        /// <returns>A new <see cref="OpmlOutline"/> object that represents a subscription list outline, initialized using the supplied parameters.</returns>
        /// <remarks>
        ///     <para>
        ///         A subscription list is a possibly multiple-level list of subscriptions to feeds. Each sub-element of the body of the OPML document
        ///         is a node of type <i>rss</i> or an outline element that contains nodes of type <i>rss</i>.
        ///     </para>
        ///     <para>
        ///         Today, most subscription lists are a flat sequence of <i>rss</i> nodes, but some aggregators allow categorized subscription lists
        ///         that are arbitrarily structured. A validator may flag these files, warning that some processors may not understand and preserve the structure.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlUrl"/> is a null reference (Nothing in Visual Basic).</exception>
        public static OpmlOutline CreateSubscriptionListOutline(string text, string type, Uri xmlUrl, Uri htmlUrl, string version, string title, string description, CultureInfo language)
        {
            OpmlOutline outline = new OpmlOutline();
            Guard.ArgumentNotNullOrEmptyString(text, "text");
            Guard.ArgumentNotNullOrEmptyString(type, "type");
            Guard.ArgumentNotNull(xmlUrl, "xmlUrl");

            outline.Text = text;
            outline.ContentType = type;
            outline.Attributes.Add("xmlUrl", xmlUrl.ToString());

            if (htmlUrl != null)
            {
                outline.Attributes.Add("htmlUrl", htmlUrl.ToString());
            }

            if (!string.IsNullOrEmpty(version))
            {
                outline.Attributes.Add("version", version.Trim());
            }

            if (!string.IsNullOrEmpty(title))
            {
                outline.Attributes.Add("title", title.Trim());
            }

            if (!string.IsNullOrEmpty(description))
            {
                outline.Attributes.Add("description", description.Trim());
            }

            if (language != null)
            {
                outline.Attributes.Add("language", language.Name);
            }

            return outline;
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

            OpmlOutline value = obj as OpmlOutline;

            if (value != null)
            {
                int result = string.Compare(this.ContentType, value.ContentType, StringComparison.OrdinalIgnoreCase);
                result = result | this.CreatedOn.CompareTo(value.CreatedOn);
                result = result | this.HasBreakpoint.CompareTo(value.HasBreakpoint);
                result = result | this.IsCommented.CompareTo(value.IsCommented);
                result = result | string.Compare(this.Text, value.Text, StringComparison.OrdinalIgnoreCase);

                result = result | ComparisonUtility.CompareSequence(this.Attributes, value.Attributes, StringComparison.OrdinalIgnoreCase);
                result = result | ComparisonUtility.CompareSequence(this.Categories, value.Categories, StringComparison.OrdinalIgnoreCase);
                result = result | CompareSequence(this.Outlines, value.Outlines);

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
            if (!(obj is OpmlOutline))
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
        /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.HasAttributes)
            {
                XPathNavigator attributesNavigator = source.CreateNavigator();

                if (attributesNavigator.MoveToFirstAttribute())
                {
                    if (this.LoadAttribute(attributesNavigator))
                    {
                        wasLoaded = true;
                    }

                    while (attributesNavigator.MoveToNextAttribute())
                    {
                        if (this.LoadAttribute(attributesNavigator))
                        {
                            wasLoaded = true;
                        }
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNodeIterator outlinesIterator = source.Select("outline");

                if (outlinesIterator != null && outlinesIterator.Count > 0)
                {
                    while (outlinesIterator.MoveNext())
                    {
                        OpmlOutline outline = new OpmlOutline();

                        if (outline.Load(outlinesIterator.Current))
                        {
                            this.Outlines.Add(outline);
                            wasLoaded = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="OpmlOutline"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.HasAttributes)
            {
                XPathNavigator attributesNavigator = source.CreateNavigator();

                if (attributesNavigator.MoveToFirstAttribute())
                {
                    if (this.LoadAttribute(attributesNavigator))
                    {
                        wasLoaded = true;
                    }

                    while (attributesNavigator.MoveToNextAttribute())
                    {
                        if (this.LoadAttribute(attributesNavigator))
                        {
                            wasLoaded = true;
                        }
                    }
                }
            }

            if (source.HasChildren)
            {
                XPathNodeIterator outlinesIterator = source.Select("outline");

                if (outlinesIterator != null && outlinesIterator.Count > 0)
                {
                    while (outlinesIterator.MoveNext())
                    {
                        OpmlOutline outline = new OpmlOutline();

                        if (outline.Load(outlinesIterator.Current, settings))
                        {
                            this.Outlines.Add(outline);
                            wasLoaded = true;
                        }
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
        /// Returns a <see cref="string"/> that represents the current <see cref="OpmlOutline"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="OpmlOutline"/>.</returns>
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
        /// Saves the current <see cref="OpmlOutline"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            writer.WriteStartElement("outline");
            writer.WriteAttributeString("text", this.Text);

            if (!string.IsNullOrEmpty(this.ContentType))
            {
                writer.WriteAttributeString("type", this.ContentType);
            }

            if (this.IsCommented)
            {
                writer.WriteAttributeString("isComment", "true");
            }

            if (this.HasBreakpoint)
            {
                writer.WriteAttributeString("isBreakpoint", "true");
            }

            if (this.CreatedOn != DateTime.MinValue)
            {
                writer.WriteAttributeString("created", SyndicationDateTimeUtility.ToRfc822DateTime(this.CreatedOn));
            }

            if (this.Categories.Count > 0)
            {
                string[] categories = new string[this.Categories.Count];
                this.Categories.CopyTo(categories, 0);

                writer.WriteAttributeString("category", string.Join(",", categories));
            }

            if (this.Attributes.Count > 0)
            {
                foreach (string name in this.Attributes.Keys)
                {
                    writer.WriteAttributeString(name, this.Attributes[name]);
                }
            }

            foreach (OpmlOutline outline in this.Outlines)
            {
                outline.WriteTo(writer);
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads this <see cref="OpmlOutline"/> using attributes defined on the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <returns><b>true</b> if the <see cref="OpmlOutline"/> was initialized using the supplied <paramref name="attribute"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="attribute"/> to be positioned on the XML element that represents a <see cref="OpmlOutline"/> attribute.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="attribute"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadAttribute(XPathNavigator attribute)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(attribute, "attribute");

            if (string.IsNullOrEmpty(attribute.Value))
            {
                return false;
            }

            if (string.Compare(attribute.Name, "text", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.Text = attribute.Value;
                wasLoaded = true;
            }
            else if (string.Compare(attribute.Name, "type", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.ContentType = attribute.Value;
                wasLoaded = true;
            }
            else if (string.Compare(attribute.Name, "isComment", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (bool.TryParse(attribute.Value, out var isComment))
                {
                    this.IsCommented = isComment;
                    wasLoaded = true;
                }
            }
            else if (string.Compare(attribute.Name, "isBreakpoint", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (bool.TryParse(attribute.Value, out var isBreakpoint))
                {
                    this.HasBreakpoint = isBreakpoint;
                    wasLoaded = true;
                }
            }
            else if (string.Compare(attribute.Name, "created", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(attribute.Value, out var created))
                {
                    this.CreatedOn = created;
                    wasLoaded = true;
                }
            }
            else if (string.Compare(attribute.Name, "category", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (attribute.Value.Contains(","))
                {
                    string[] categories = attribute.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string category in categories)
                    {
                        this.Categories.Add(category);
                    }
                }
                else
                {
                    this.Categories.Add(attribute.Value);
                }

                wasLoaded = true;
            }
            else
            {
                if (!this.Attributes.ContainsKey(attribute.Name))
                {
                    this.Attributes.Add(attribute.Name, attribute.Value);
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }
    }
}