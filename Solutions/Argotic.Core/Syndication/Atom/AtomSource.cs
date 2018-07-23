namespace Argotic.Syndication
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
    /// Represents the meta-data of the source feed that an <see cref="AtomEntry"/> was copied from.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an <see cref="AtomEntry"/> is copied from one feed into another feed, then the source feed's metadata (all child elements of feed other than the entry elements) <i>may</i> be preserved
    ///         within the copied entry by specifying an <see cref="AtomSource"/>, if it is not already present in the entry, and including some or all of the source feed's meta-data elements as the
    ///         source's children. Such metadata <i>should</i> be preserved if the source <see cref="AtomFeed">feed</see> contains any of the child elements author, contributor, rights, or category
    ///         and those child elements are not present in the source <see cref="AtomEntry">entry</see>.
    ///     </para>
    ///     <para>
    ///         The <see cref="AtomSource"/> is designed to allow the aggregation of entries from different feeds while retaining information about an entry's source feed.
    ///         For this reason, Atom Processors that are performing such aggregation <i>should</i> include at least the required feed-level meta-data elements
    ///         (<see cref="AtomFeed.Id">id</see>, <see cref="AtomFeed.Title">title</see>, and <see cref="AtomFeed.UpdatedOn">updated</see>) in the <see cref="AtomSource"/>.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the AtomSource class.">
    ///         <code source="..\..\Argotic.Examples\Core\Atom\AtomSourceExample.cs" region="AtomSource" />
    ///     </code>
    /// </example>
    [Serializable]
    public class AtomSource : IAtomCommonObjectAttributes, IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Private member to hold the collection of authors of the source.
        /// </summary>
        private Collection<AtomPersonConstruct> sourceAuthors;

        /// <summary>
        /// Private member to hold the collection of categories associated with the source.
        /// </summary>
        private Collection<AtomCategory> sourceCategories;

        /// <summary>
        /// Private member to hold the collection of contributors of the source.
        /// </summary>
        private Collection<AtomPersonConstruct> sourceContributors;

        /// <summary>
        /// Private member to hold eferences from the source to one or more Web resources.
        /// </summary>
        private Collection<AtomLink> sourceLinks;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomSource"/> class.
        /// </summary>
        public AtomSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomSource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
        /// </summary>
        /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this source.</param>
        /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this source.</param>
        /// <param name="utcUpdatedOn">
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this source was modified in a way the publisher considers significant.
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </param>
        public AtomSource(AtomId id, AtomTextConstruct title, DateTime utcUpdatedOn)
        {
            this.Id = id;
            this.Title = title;
            this.UpdatedOn = utcUpdatedOn;
        }

        /// <summary>
        /// Gets the authors of this source.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the authors of this source.</value>
        public Collection<AtomPersonConstruct> Authors
        {
            get
            {
                return this.sourceAuthors ?? (this.sourceAuthors = new Collection<AtomPersonConstruct>());
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
        /// Gets the categories associated with this source.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomCategory"/> objects that represent the categories associated with this source.</value>
        public Collection<AtomCategory> Categories
        {
            get
            {
                return this.sourceCategories ?? (this.sourceCategories = new Collection<AtomCategory>());
            }
        }

        /// <summary>
        /// Gets the entities who contributed to this source.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the entities who contributed to this source.</value>
        public Collection<AtomPersonConstruct> Contributors
        {
            get
            {
                return this.sourceContributors ?? (this.sourceContributors = new Collection<AtomPersonConstruct>());
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
        /// Gets or sets the agent used to generate this source.
        /// </summary>
        /// <value>A <see cref="AtomGenerator"/> object that represents the agent used to generate this source. The default value is a <b>null</b> reference.</value>
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
        /// Gets or sets an image that provides iconic visual identification for this source.
        /// </summary>
        /// <value>A <see cref="AtomIcon"/> object that represents an image that provides iconic visual identification for this source. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     The image <i>should</i> have an aspect ratio of one (horizontal) to one (vertical) and <i>should</i> be suitable for presentation at a small size.
        /// </remarks>
        public AtomIcon Icon { get; set; }

        /// <summary>
        /// Gets or sets a permanent, universally unique identifier for this source.
        /// </summary>
        /// <value>A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this source.</value>
        public AtomId Id { get; set; }

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
        /// Gets references from this source to one or more Web resources.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="AtomLink"/> objects that represent references from this source to one or more Web resources.</value>
        public Collection<AtomLink> Links
        {
            get
            {
                return this.sourceLinks ?? (this.sourceLinks = new Collection<AtomLink>());
            }
        }

        /// <summary>
        /// Gets or sets an image that provides visual identification for this source.
        /// </summary>
        /// <value>A <see cref="AtomLogo"/> object that represents an image that provides visual identification for this source. The default value is a <b>null</b> reference.</value>
        /// <remarks>
        ///     The image <i>should</i> have an aspect ratio of 2 (horizontal) to 1 (vertical).
        /// </remarks>
        public AtomLogo Logo { get; set; }

        /// <summary>
        /// Gets or sets information about rights held in and over this source.
        /// </summary>
        /// <value>A <see cref="AtomTextConstruct"/> object that represents information about rights held in and over this source.</value>
        /// <remarks>
        ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
        /// </remarks>
        public AtomTextConstruct Rights { get; set; }

        /// <summary>
        /// Gets or sets information that conveys a human-readable description or subtitle for this source.
        /// </summary>
        /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable description or subtitle for this source.</value>
        public AtomTextConstruct Subtitle { get; set; }

        /// <summary>
        /// Gets or sets information that conveys a human-readable title for this source.
        /// </summary>
        /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this source.</value>
        public AtomTextConstruct Title { get; set; }

        /// <summary>
        /// Gets or sets a date-time indicating the most recent instant in time when this source was modified in a way the publisher considers significant.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this source was modified in a way the publisher considers significant.
        ///     Publishers <i>may</i> change the value of this element over time. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update time was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(AtomSource first, AtomSource second)
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
        public static bool operator >(AtomSource first, AtomSource second)
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
        public static bool operator !=(AtomSource first, AtomSource second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(AtomSource first, AtomSource second)
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

            AtomSource value = obj as AtomSource;

            if (value != null)
            {
                int result = AtomFeed.CompareSequence(this.Authors, value.Authors);
                result = result | AtomFeed.CompareSequence(this.Categories, value.Categories);
                result = result | AtomFeed.CompareSequence(this.Contributors, value.Contributors);

                if (this.Generator != null)
                {
                    result = result | this.Generator.CompareTo(value.Generator);
                }
                else if (value.Generator != null)
                {
                    result = result | -1;
                }

                if (this.Icon != null)
                {
                    result = result | this.Icon.CompareTo(value.Icon);
                }
                else if (value.Icon != null)
                {
                    result = result | -1;
                }

                if (this.Id != null)
                {
                    result = result | this.Id.CompareTo(value.Id);
                }
                else if (value.Id != null)
                {
                    result = result | -1;
                }

                result = result | AtomFeed.CompareSequence(this.Links, value.Links);

                if (this.Logo != null)
                {
                    result = result | this.Logo.CompareTo(value.Logo);
                }
                else if (value.Logo != null)
                {
                    result = result | -1;
                }

                if (this.Rights != null)
                {
                    result = result | this.Rights.CompareTo(value.Rights);
                }
                else if (value.Rights != null)
                {
                    result = result | -1;
                }

                if (this.Subtitle != null)
                {
                    result = result | this.Subtitle.CompareTo(value.Subtitle);
                }
                else if (value.Subtitle != null)
                {
                    result = result | -1;
                }

                if (this.Title != null)
                {
                    result = result | this.Title.CompareTo(value.Title);
                }
                else if (value.Title != null)
                {
                    result = result | -1;
                }

                result = result | this.UpdatedOn.CompareTo(value.UpdatedOn);

                result = result | AtomUtility.CompareCommonObjectAttributes(this, value);

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
            if (!(obj is AtomSource))
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
        /// Loads this <see cref="AtomSource"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="AtomSource"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");

            XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);

            if (AtomUtility.FillCommonObjectAttributes(this, source))
            {
                wasLoaded = true;
            }

            XPathNavigator idNavigator = source.SelectSingleNode("atom:id", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("atom:title", manager);
            XPathNavigator updatedNavigator = source.SelectSingleNode("atom:updated", manager);

            if (idNavigator != null)
            {
                this.Id = new AtomId();

                if (this.Id.Load(idNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (titleNavigator != null)
            {
                this.Title = new AtomTextConstruct();

                if (this.Title.Load(titleNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (updatedNavigator != null)
            {
                DateTime updatedOn;

                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedNavigator.Value, out updatedOn))
                {
                    this.UpdatedOn = updatedOn;
                    wasLoaded = true;
                }
            }

            if (this.LoadOptionals(source, manager))
            {
                wasLoaded = true;
            }

            if (this.LoadCollections(source, manager))
            {
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="AtomSource"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="AtomSource"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");

            wasLoaded = this.Load(source);
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
        /// Returns a <see cref="string"/> that represents the current <see cref="AtomSource"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AtomSource"/>.</returns>
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
        /// Saves the current <see cref="AtomSource"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("source", AtomUtility.AtomNamespace);
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

            foreach (AtomLink link in this.Links)
            {
                link.WriteTo(writer);
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads this <see cref="AtomSource"/> collection elements using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <returns><b>true</b> if the <see cref="AtomSource"/> collection entities were initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadCollections(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            XPathNodeIterator authorIterator = source.Select("atom:author", manager);
            XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
            XPathNodeIterator categoryIterator = source.Select("atom:category", manager);
            XPathNodeIterator linkIterator = source.Select("atom:link", manager);

            if (authorIterator != null && authorIterator.Count > 0)
            {
                while (authorIterator.MoveNext())
                {
                    AtomPersonConstruct author = new AtomPersonConstruct();

                    if (author.Load(authorIterator.Current))
                    {
                        this.Authors.Add(author);
                        wasLoaded = true;
                    }
                }
            }

            if (categoryIterator != null && categoryIterator.Count > 0)
            {
                while (categoryIterator.MoveNext())
                {
                    AtomCategory category = new AtomCategory();

                    if (category.Load(categoryIterator.Current))
                    {
                        this.Categories.Add(category);
                        wasLoaded = true;
                    }
                }
            }

            if (contributorIterator != null && contributorIterator.Count > 0)
            {
                while (contributorIterator.MoveNext())
                {
                    AtomPersonConstruct contributor = new AtomPersonConstruct();

                    if (contributor.Load(contributorIterator.Current))
                    {
                        this.Contributors.Add(contributor);
                        wasLoaded = true;
                    }
                }
            }

            if (linkIterator != null && linkIterator.Count > 0)
            {
                while (linkIterator.MoveNext())
                {
                    AtomLink link = new AtomLink();

                    if (link.Load(linkIterator.Current))
                    {
                        this.Links.Add(link);
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="AtomSource"/> optional elements using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <returns><b>true</b> if the <see cref="AtomSource"/> optional entities were initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            XPathNavigator generatorNavigator = source.SelectSingleNode("atom:generator", manager);
            XPathNavigator iconNavigator = source.SelectSingleNode("atom:icon", manager);
            XPathNavigator logoNavigator = source.SelectSingleNode("atom:logo", manager);
            XPathNavigator rightsNavigator = source.SelectSingleNode("atom:rights", manager);
            XPathNavigator subtitleNavigator = source.SelectSingleNode("atom:subtitle", manager);

            if (generatorNavigator != null)
            {
                this.Generator = new AtomGenerator();

                if (this.Generator.Load(generatorNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (iconNavigator != null)
            {
                this.Icon = new AtomIcon();

                if (this.Icon.Load(iconNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (logoNavigator != null)
            {
                this.Logo = new AtomLogo();

                if (this.Logo.Load(logoNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (rightsNavigator != null)
            {
                this.Rights = new AtomTextConstruct();

                if (this.Rights.Load(rightsNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (subtitleNavigator != null)
            {
                this.Subtitle = new AtomTextConstruct();

                if (this.Subtitle.Load(subtitleNavigator))
                {
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }
    }
}