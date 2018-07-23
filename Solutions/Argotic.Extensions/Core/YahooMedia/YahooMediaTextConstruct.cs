namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents human-readable text.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is a generic representation for the <i>media:title</i> and <i>media:description</i> elements in the Yahoo media specificaton.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class YahooMediaTextConstruct : IComparable
    {
        /// <summary>
        /// Private member to hold the entity encoding utilized by the human-readable text.
        /// </summary>
        private YahooMediaTextConstructType textConstructType = YahooMediaTextConstructType.None;

        /// <summary>
        /// Private member to hold the content of the human-readable text.
        /// </summary>
        private string textConstructContent = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaTextConstruct"/> class.
        /// </summary>
        public YahooMediaTextConstruct()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaTextConstruct"/> class using the supplied text.
        /// </summary>
        /// <param name="text">The content of this human-readable text.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        public YahooMediaTextConstruct(string text)
        {
            this.Content = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaTextConstruct"/> class using the supplied text.
        /// </summary>
        /// <param name="text">The content of this human-readable text.</param>
        /// <param name="type">An <see cref="YahooMediaTextConstruct"/> enumeration value that represents the entity encoding utilized by this human-readable text.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
        public YahooMediaTextConstruct(string text, YahooMediaTextConstructType type)
            : this(text)
        {
            this.TextType = type;
        }

        /// <summary>
        /// Gets or sets the content of this human-readable text.
        /// </summary>
        /// <value>The content of this human-readable text.</value>
        /// <remarks>
        ///     All HTML <b>must</b> be entity-encoded.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string Content
        {
            get
            {
                return this.textConstructContent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                this.textConstructContent = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the entity encoding utilized by this human-readable text.
        /// </summary>
        /// <value>
        ///     An <see cref="YahooMediaTextConstruct"/> enumeration value that represents the entity encoding utilized by this human-readable text.
        ///     The default value is <see cref="YahooMediaTextConstructType.None"/>.
        /// </value>
        /// <remarks>
        ///     If no entity encoding is specified, a default value of <see cref="YahooMediaTextConstructType.Plain"/> can be assumed.
        /// </remarks>
        public YahooMediaTextConstructType TextType
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
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaTextConstruct first, YahooMediaTextConstruct second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return true;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(YahooMediaTextConstruct first, YahooMediaTextConstruct second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaTextConstruct first, YahooMediaTextConstruct second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(YahooMediaTextConstruct first, YahooMediaTextConstruct second)
        {
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Returns the entity encoding type identifier for the supplied <see cref="YahooMediaTextConstructType"/>.
        /// </summary>
        /// <param name="type">The <see cref="YahooMediaTextConstructType"/> to get the entity encoding type identifier for.</param>
        /// <returns>The entity encoding type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        public static string TextTypeAsString(YahooMediaTextConstructType type)
        {
            string name = string.Empty;
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaTextConstructType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaTextConstructType))
                {
                    YahooMediaTextConstructType constructType = (YahooMediaTextConstructType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (constructType == type)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                            name = enumerationMetadata.AlternateValue;
                            break;
                        }
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Returns the <see cref="YahooMediaTextConstructType"/> enumeration value that corresponds to the specified entity encoding type name.
        /// </summary>
        /// <param name="name">The name of the entity encoding type.</param>
        /// <returns>A <see cref="YahooMediaTextConstructType"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaTextConstructType.None</b>.</returns>
        /// <remarks>This method disregards case of specified entity encoding type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaTextConstructType TextTypeByName(string name)
        {
            YahooMediaTextConstructType constructType = YahooMediaTextConstructType.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaTextConstructType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaTextConstructType))
                {
                    YahooMediaTextConstructType type = (YahooMediaTextConstructType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
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
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Loads this <see cref="YahooMediaTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaTextConstruct"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaTextConstruct"/>.
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
                    YahooMediaTextConstructType type = YahooMediaTextConstruct.TextTypeByName(typeAttribute);
                    if (type != YahooMediaTextConstructType.None)
                    {
                        this.TextType = type;
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
        /// Saves the current <see cref="YahooMediaTextConstruct"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <param name="elementName">The local name of the text construct being written.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer, string elementName)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            writer.WriteStartElement(elementName, extension.XmlNamespace);

            if (this.TextType != YahooMediaTextConstructType.None)
            {
                writer.WriteAttributeString("type", YahooMediaTextConstruct.TextTypeAsString(this.TextType));
            }

            if (!string.IsNullOrEmpty(this.Content))
            {
                writer.WriteString(this.Content);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaTextConstruct"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaTextConstruct"/>.</returns>
        /// <remarks>
        ///     This method returns the XML representation for the current instance. A <i>generic</i> element name is used.
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
                    this.WriteTo(writer, "generic");
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
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

            YahooMediaTextConstruct value = obj as YahooMediaTextConstruct;

            if (value != null)
            {
                int result = string.Compare(this.Content, value.Content, StringComparison.OrdinalIgnoreCase);
                result = result | this.TextType.CompareTo(value.TextType);

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is YahooMediaTextConstruct))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }
    }
}