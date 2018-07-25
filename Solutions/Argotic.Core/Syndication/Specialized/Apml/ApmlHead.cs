namespace Argotic.Syndication.Specialized
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
    /// Represents the basic administrative information of an <see cref="ApmlDocument"/>.
    /// </summary>
    /// <seealso cref="ApmlDocument.Head"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Apml")]
    [Serializable]
    public class ApmlHead : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold a value that credits the software that created the document.
        /// </summary>
        private string headGenerator = string.Format(
            null,
            "Argotic Syndication Framework {0}, http://www.codeplex.com/Argotic",
            System.Reflection.Assembly.GetAssembly(typeof(ApmlHead)).GetName().Version.ToString(4));

        /// <summary>
        /// Private member to hold the title of the document.
        /// </summary>
        private string headTitle = string.Empty;

        /// <summary>
        /// Private member to hold email address of the owner of the document.
        /// </summary>
        private string headUserEmailAddress = string.Empty;

        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlHead"/> class.
        /// </summary>
        public ApmlHead()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApmlHead"/> class using the supplied title and creation date.
        /// </summary>
        /// <param name="title">The title of this document.</param>
        /// <param name="utcCreatedOn">A <see cref="DateTime"/> object that indicates when this document was created.</param>
        public ApmlHead(string title, DateTime utcCreatedOn)
        {
            this.CreatedOn = utcCreatedOn;
            this.Title = title;
        }

        /// <summary>
        /// Gets or sets a date-time indicating when this document was created.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that indicates when this document was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.</value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime CreatedOn { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the email address of the owner of this document.
        /// </summary>
        /// <value>The email address of the owner of this document.</value>
        public string EmailAddress
        {
            get
            {
                return this.headUserEmailAddress;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.headUserEmailAddress = string.Empty;
                }
                else
                {
                    this.headUserEmailAddress = value.Trim();
                }
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
        /// Gets or sets a value that credits the software that created this document.
        /// </summary>
        /// <value>A value that credits the software that created this document. The default value is an agent that describes this syndication framework.</value>
        public string Generator
        {
            get
            {
                return this.headGenerator;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.headGenerator = string.Empty;
                }
                else
                {
                    this.headGenerator = value.Trim();
                }
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
        /// Gets or sets the title of this document.
        /// </summary>
        /// <value>The title of this document.</value>
        public string Title
        {
            get
            {
                return this.headTitle;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.headTitle = string.Empty;
                }
                else
                {
                    this.headTitle = value.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(ApmlHead first, ApmlHead second)
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
        public static bool operator >(ApmlHead first, ApmlHead second)
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
        public static bool operator !=(ApmlHead first, ApmlHead second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(ApmlHead first, ApmlHead second)
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

            ApmlHead value = obj as ApmlHead;

            if (value != null)
            {
                int result = this.CreatedOn.CompareTo(value.CreatedOn);
                result = result | string.Compare(
                             this.EmailAddress,
                             value.EmailAddress,
                             StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Generator, value.Generator, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);

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
            if (!(obj is ApmlHead))
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
        /// Loads this <see cref="ApmlHead"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="ApmlHead"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlHead"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);
            XPathNavigator titleNavigator = source.SelectSingleNode("apml:Title", manager);
            XPathNavigator generatorNavigator = source.SelectSingleNode("apml:Generator", manager);
            XPathNavigator userEmailNavigator = source.SelectSingleNode("apml:UserEmail", manager);
            XPathNavigator dateCreatedNavigator = source.SelectSingleNode("apml:DateCreated", manager);

            if (titleNavigator != null)
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }

            if (generatorNavigator != null)
            {
                this.Generator = generatorNavigator.Value;
                wasLoaded = true;
            }

            if (userEmailNavigator != null)
            {
                this.EmailAddress = userEmailNavigator.Value;
                wasLoaded = true;
            }

            if (dateCreatedNavigator != null)
            {
                DateTime createdOn;
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedNavigator.Value, out createdOn))
                {
                    this.CreatedOn = createdOn;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="ApmlHead"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="ApmlHead"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlHead"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="ApmlHead"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ApmlHead"/>.</returns>
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
        /// Saves the current <see cref="ApmlHead"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("Head", ApmlUtility.ApmlNamespace);

            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.WriteElementString("Title", ApmlUtility.ApmlNamespace, this.Title);
            }

            if (!string.IsNullOrEmpty(this.Generator))
            {
                writer.WriteElementString("Generator", ApmlUtility.ApmlNamespace, this.Generator);
            }

            if (!string.IsNullOrEmpty(this.EmailAddress))
            {
                writer.WriteElementString("UserEmail", ApmlUtility.ApmlNamespace, this.EmailAddress);
            }

            if (this.CreatedOn != DateTime.MinValue)
            {
                writer.WriteElementString(
                    "DateCreated",
                    ApmlUtility.ApmlNamespace,
                    SyndicationDateTimeUtility.ToRfc3339DateTime(this.CreatedOn));
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}