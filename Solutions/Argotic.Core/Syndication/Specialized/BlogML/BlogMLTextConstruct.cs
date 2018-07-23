namespace Argotic.Syndication.Specialized
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;

    /// <summary>
    /// Represents machine or human readable text.
    /// </summary>
    [Serializable]
    public class BlogMLTextConstruct : IComparable, IExtensibleSyndicationObject
    {
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        /// <summary>
        /// Private member to hold the content of the text.
        /// </summary>
        private string textConstructContent = string.Empty;

        /// <summary>
        /// Private member to hold a value indicating if the text construct escapes content using a CDATA block.
        /// </summary>
        private bool textConstructEscapesContent = true;

        /// <summary>
        /// Private member to hold entity encoding utilized by the text.
        /// </summary>
        private BlogMLContentType textConstructType = BlogMLContentType.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogMLTextConstruct"/> class.
        /// </summary>
        public BlogMLTextConstruct()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogMLTextConstruct"/> class using the supplied textual content.
        /// </summary>
        /// <param name="content">The textual content.</param>
        /// <remarks>
        ///     This constructor assumes the supplied <paramref name="content"/> is not encoded per a specific entity scheme.
        ///     The textual content will be escaped using a <i>CDATA</i> block.
        /// </remarks>
        public BlogMLTextConstruct(string content)
        {
            this.Content = content;
            this.ContentType = BlogMLContentType.Text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogMLTextConstruct"/> class using the supplied content and entity encoding scheme.
        /// </summary>
        /// <param name="content">The entity encoded content.</param>
        /// <param name="encoding">An <see cref="BlogMLContentType"/> enumeration value that represents the entity encoding utilized by the <paramref name="content"/>.</param>
        /// <remarks>
        ///     This constructor assumes the supplied <paramref name="content"/> is encoded per the specified <paramref name="encoding"/> scheme.
        ///     The textual content will be escaped using a <i>CDATA</i> block.
        /// </remarks>
        public BlogMLTextConstruct(string content, BlogMLContentType encoding)
        {
            this.Content = content;
            this.ContentType = encoding;
        }

        /// <summary>
        /// Gets or sets the content of this text.
        /// </summary>
        /// <value>The content of this text.</value>
        public string Content
        {
            get
            {
                return this.textConstructContent;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.textConstructContent = string.Empty;
                }
                else
                {
                    this.textConstructContent = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity encoding utilized by this text.
        /// </summary>
        /// <value>
        ///     An <see cref="BlogMLContentType"/> enumeration value that represents the entity encoding utilized by this text.
        ///     The default value is <see cref="BlogMLContentType.None"/>.
        /// </value>
        public BlogMLContentType ContentType
        {
            get
            {
                return this.textConstructType;
            }

            set
            {
                this.textConstructType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if the content of this text is escaped using a CDATA block.
        /// </summary>
        /// <value><b>true</b> if the content of this text will be escaped using a CDATA block section; otherwise <b>false</b>. The default value is <b>true</b>.</value>
        /// <remarks>
        ///     <i>CDATA</i> sections are used to escape blocks of text containing characters which would otherwise be recognized as markup.
        ///     All tags and entity references are ignored by an XML processor that treats them just like any character data.
        ///     <i>CDATA</i> blocks should be used when you want to include large blocks of special characters as character data,
        ///     but you do not want to have to use entity references all the time.
        /// </remarks>
        public bool EscapeContent
        {
            get
            {
                return this.textConstructEscapesContent;
            }

            set
            {
                this.textConstructEscapesContent = value;
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
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(BlogMLTextConstruct first, BlogMLTextConstruct second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }
            else if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator >(BlogMLTextConstruct first, BlogMLTextConstruct second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
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
        public static bool operator !=(BlogMLTextConstruct first, BlogMLTextConstruct second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(BlogMLTextConstruct first, BlogMLTextConstruct second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Returns the text construct identifier for the supplied <see cref="BlogMLContentType"/>.
        /// </summary>
        /// <param name="type">The <see cref="BlogMLContentType"/> to get the text construct identifier for.</param>
        /// <returns>The text construct identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the ConstructTypeAsString method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\BlogML\BlogMLTextConstructExample.cs"
        ///             region="ConstructTypeAsString(BlogMLContentType type)"
        ///         />
        ///     </code>
        /// </example>
        public static string ConstructTypeAsString(BlogMLContentType type)
        {
            string name = string.Empty;
            foreach (FieldInfo fieldInfo in typeof(BlogMLContentType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(BlogMLContentType))
                {
                    BlogMLContentType constructType = (BlogMLContentType)Enum.Parse(
                        fieldInfo.FieldType,
                        fieldInfo.Name);

                    if (constructType == type)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(
                            typeof(EnumerationMetadataAttribute),
                            false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata =
                                customAttributes[0] as EnumerationMetadataAttribute;

                            name = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Returns the <see cref="BlogMLContentType"/> enumeration value that corresponds to the specified text construct type name.
        /// </summary>
        /// <param name="name">The name of the text construct type.</param>
        /// <returns>A <see cref="BlogMLContentType"/> enumeration value that corresponds to the specified string, otherwise returns <b>BlogMLContentType.None</b>.</returns>
        /// <remarks>This method disregards case of specified text construct type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the ConstructTypeByName method.">
        ///         <code
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\BlogML\BlogMLTextConstructExample.cs"
        ///             region="ConstructTypeByName(string name)"
        ///         />
        ///     </code>
        /// </example>
        public static BlogMLContentType ConstructTypeByName(string name)
        {
            BlogMLContentType constructType = BlogMLContentType.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (FieldInfo fieldInfo in typeof(BlogMLContentType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(BlogMLContentType))
                {
                    BlogMLContentType type = (BlogMLContentType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(
                        typeof(EnumerationMetadataAttribute),
                        false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata =
                            customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase)
                            == 0)
                        {
                            constructType = type;
                            break;
                        }
                    }
                }
            }

            return constructType;
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

            BlogMLTextConstruct value = obj as BlogMLTextConstruct;

            if (value != null)
            {
                int result = string.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result = result | this.ContentType.CompareTo(value.ContentType);

                return result;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        null,
                        "obj is not of type {0}, type was found to be '{1}'.",
                        this.GetType().FullName,
                        obj.GetType().FullName),
                    "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BlogMLTextConstruct))
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
        /// Loads this <see cref="BlogMLTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="BlogMLTextConstruct"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLTextConstruct"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string typeAttribute = source.GetAttribute("type", string.Empty);
                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    BlogMLContentType type = ConstructTypeByName(typeAttribute);
                    if (type != BlogMLContentType.None)
                    {
                        this.ContentType = type;
                        wasLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.Value))
            {
                this.Content = source.Value;
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="ApmlApplication"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
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
        /// Returns a <see cref="string"/> that represents the current <see cref="BlogMLTextConstruct"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="BlogMLTextConstruct"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance, with a generic element name of <i>TextConstruct</i>.
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
                    this.WriteTo(writer, "TextConstruct");
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Saves the current <see cref="BlogMLTextConstruct"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <param name="elementName">The local name of the text construct being written.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is an empty string.</exception>
        public void WriteTo(XmlWriter writer, string elementName)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(elementName, "elementName");
            writer.WriteStartElement(elementName, BlogMLUtility.BlogMLNamespace);

            if (this.ContentType != BlogMLContentType.None)
            {
                writer.WriteAttributeString("type", ConstructTypeAsString(this.ContentType));
            }

            if (!string.IsNullOrEmpty(this.Content))
            {
                if (this.EscapeContent)
                {
                    writer.WriteCData(this.Content);
                }
                else
                {
                    writer.WriteString(this.Content);
                }
            }

            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }
    }
}