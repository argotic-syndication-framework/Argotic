﻿namespace Argotic.Syndication.Specialized
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;

    /// <summary>
    /// Represents the author of a <see cref="ApmlSource"/>.
    /// </summary>
    /// <seealso cref="ApmlSource.Authors"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlAuthor class.">
    ///         <code source="..\..\Argotic.Examples\Core\Apml\ApmlAuthorExample.cs" region="ApmlAuthor" />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Apml")]
    [Serializable]
    public class ApmlAuthor : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the name of the entity that contributed the author.
        /// </summary>
        private string authorFrom = string.Empty;

        /// <summary>
        /// Private member to hold the unique key for the author.
        /// </summary>
        private string authorKey = string.Empty;

        /// <summary>
        /// Private member to hold the decimal score of the author.
        /// </summary>
        private decimal authorValue = decimal.MinValue;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlAuthor"/> class.
        /// </summary>
        public ApmlAuthor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlAuthor"/> class using the supplied parameters.
        /// </summary>
        /// <param name="key">The unique key for this author.</param>
        /// <param name="value">The decimal score of this author.</param>
        /// <remarks>
        ///     This constructor is meant to be used when creating an <b>explicit</b> author. Explicit data is for items that are explicitly added by a user to represent something.
        ///     For example, a user could edit their own APML file and add items they know they're interested in.
        ///     For this reason the <see cref="From"/> and <see cref="UpdatedOn"/> properties are not necessary for explicit data items, because it's a manual process.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
        public ApmlAuthor(string key, decimal value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlAuthor"/> class using the supplied parameters.
        /// </summary>
        /// <param name="key">The unique key for this author.</param>
        /// <param name="value">The decimal score of this author.</param>
        /// <param name="from">The name of the entity that contributed this author.</param>
        /// <param name="utcUpdatedOn">A <see cref="DateTime"/> object that indicates the last time this author was updated.</param>
        /// <remarks>
        ///     This constructor is meant to be used when creating an <b>implicit</b> author. Implicit data is added by machines/computers that try to make
        ///     some informed guesses about the things that you are interested in. This stuff will change over time and are added with a certain degree of confidence
        ///     that may have a decay in certain applications. For this reason it is important to keep a track of when things were added/modified.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="from"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="from"/> is an empty string.</exception>
        public ApmlAuthor(string key, decimal value, string from, DateTime utcUpdatedOn)
            : this(key, value)
        {
            Guard.ArgumentNotNullOrEmptyString(from, "from");
            this.From = from;
            this.UpdatedOn = utcUpdatedOn;
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
        /// Gets or sets the name of the entity that contributed this author.
        /// </summary>
        /// <value>The name of the entity that contributed this author. The default value is an empty string, which indicates no contributor was specified.</value>
        public string From
        {
            get
            {
                return this.authorFrom;
            }

            set
            {
                this.authorFrom = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
        /// Gets or sets the unique key for this author.
        /// </summary>
        /// <value>The unique key for this author.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Key
        {
            get
            {
                return this.authorKey;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.authorKey = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a date-time indicating the last time this author was updated.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that indicates the last time this author was updated. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update date was specified.</value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the decimal score of this author.
        /// </summary>
        /// <value>The decimal score of this author.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
        public decimal Value
        {
            get
            {
                return this.authorValue;
            }

            set
            {
                Guard.ArgumentNotLessThan(value, "value", decimal.MinusOne);
                Guard.ArgumentNotGreaterThan(value, "value", decimal.One);
                this.authorValue = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(ApmlAuthor first, ApmlAuthor second)
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
        public static bool operator >(ApmlAuthor first, ApmlAuthor second)
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
        public static bool operator !=(ApmlAuthor first, ApmlAuthor second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(ApmlAuthor first, ApmlAuthor second)
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

            ApmlAuthor value = obj as ApmlAuthor;

            if (value != null)
            {
                int result = string.Compare(this.From, value.From, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Key, value.Key, StringComparison.OrdinalIgnoreCase);
                result = result | this.UpdatedOn.CompareTo(value.UpdatedOn);
                result = result | this.Value.CompareTo(value.Value);

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
            if (!(obj is ApmlAuthor))
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
        /// Loads this <see cref="ApmlAuthor"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="ApmlAuthor"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlAuthor"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            if (source.HasAttributes)
            {
                string keyAttribute = source.GetAttribute("key", string.Empty);
                string valueAttribute = source.GetAttribute("value", string.Empty);
                string fromAttribute = source.GetAttribute("from", string.Empty);
                string updatedAttribute = source.GetAttribute("updated", string.Empty);

                if (!string.IsNullOrEmpty(keyAttribute))
                {
                    this.Key = keyAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(valueAttribute))
                {
                    if (decimal.TryParse(valueAttribute, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out var value))
                    {
                        if (value >= decimal.MinusOne && value <= decimal.One)
                        {
                            this.Value = value;
                            wasLoaded = true;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(fromAttribute))
                {
                    this.From = fromAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(updatedAttribute))
                {
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedAttribute, out var updatedOn))
                    {
                        this.UpdatedOn = updatedOn;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="ApmlAuthor"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="ApmlAuthor"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlAuthor"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="ApmlAuthor"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ApmlAuthor"/>.</returns>
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
        /// Saves the current <see cref="ApmlAuthor"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("Author", ApmlUtility.ApmlNamespace);

            writer.WriteAttributeString("key", this.Key);
            writer.WriteAttributeString("value", this.Value.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo));

            if (!string.IsNullOrEmpty(this.From))
            {
                writer.WriteAttributeString("from", this.From);
            }

            if (this.UpdatedOn != DateTime.MinValue)
            {
                writer.WriteAttributeString("updated", SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}