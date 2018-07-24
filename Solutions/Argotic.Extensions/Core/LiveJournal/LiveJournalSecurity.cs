namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents the access level of a LiveJournal entry.
    /// </summary>
    /// <seealso cref="LiveJournalSyndicationExtensionContext.Security"/>
    [Serializable]
    public class LiveJournalSecurity : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class.
        /// </summary>
        public LiveJournalSecurity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class using the supplied <see cref="LiveJournalSecurityType"/>.
        /// </summary>
        /// <param name="accessType">A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type.</param>
        public LiveJournalSecurity(LiveJournalSecurityType accessType)
        {
            this.Accessibility = accessType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class using the supplied <see cref="LiveJournalSecurityType"/>.
        /// </summary>
        /// <param name="accessType">A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type.</param>
        /// <param name="mask">An integer indicating the friend-groups mask.</param>
        public LiveJournalSecurity(LiveJournalSecurityType accessType, int mask)
        {
            this.Accessibility = accessType;
            this.Mask = mask;
        }

        /// <summary>
        /// Gets or sets the accessibility type.
        /// </summary>
        /// <value>A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type. The default value is <see cref="LiveJournalSecurityType.Public"/>.</value>
        public LiveJournalSecurityType Accessibility { get; set; } = LiveJournalSecurityType.Public;

        /// <summary>
        /// Gets or sets the friend-groups mask.
        /// </summary>
        /// <value>An integer indicating the friend-groups mask used. The default value is <see cref="int.MinValue"/>, which indicates that no friend-groups mask was specified.</value>
        /// <remarks>
        ///     This property only applies if the <see cref="Accessibility"/> property is <see cref="LiveJournalSecurityType.Friends">friends</see>
        ///     and <b>only</b> if the author of the post is the same as the user who has authenticated the feed request.
        /// </remarks>
        public int Mask { get; set; } = int.MinValue;

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(LiveJournalSecurity first, LiveJournalSecurity second)
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
        public static bool operator !=(LiveJournalSecurity first, LiveJournalSecurity second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(LiveJournalSecurity first, LiveJournalSecurity second)
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
        public static bool operator >(LiveJournalSecurity first, LiveJournalSecurity second)
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
        /// Returns the access level identifier for the supplied <see cref="LiveJournalSecurityType"/>.
        /// </summary>
        /// <param name="level">The <see cref="LiveJournalSecurityType"/> to get the access level identifier for.</param>
        /// <returns>The access level identifier for the supplied <paramref name="level"/>, otherwise returns an empty string.</returns>
        public static string AccessibilityAsString(LiveJournalSecurityType level)
        {
            string name = string.Empty;
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(LiveJournalSecurityType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(LiveJournalSecurityType))
                {
                    LiveJournalSecurityType accessLevel = (LiveJournalSecurityType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (accessLevel == level)
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
        /// Returns the <see cref="LiveJournalSecurityType"/> enumeration value that corresponds to the specified access level name.
        /// </summary>
        /// <param name="name">The name of the access level.</param>
        /// <returns>A <see cref="LiveJournalSecurityType"/> enumeration value that corresponds to the specified string, otherwise returns <b>LiveJournalSecurityType.None</b>.</returns>
        /// <remarks>This method disregards case of specified access level name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static LiveJournalSecurityType AccessibilityByName(string name)
        {
            LiveJournalSecurityType accessLevel = LiveJournalSecurityType.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(LiveJournalSecurityType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(LiveJournalSecurityType))
                {
                    LiveJournalSecurityType level = (LiveJournalSecurityType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            accessLevel = level;
                            break;
                        }
                    }
                }
            }

            return accessLevel;
        }

        /// <summary>
        /// Loads this <see cref="LiveJournalSecurity"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="LiveJournalSecurity"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalSecurity"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string typeAttribute = source.GetAttribute("type", string.Empty);
                string maskAttribute = source.GetAttribute("mask", string.Empty);

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    LiveJournalSecurityType accessLevel = LiveJournalSecurity.AccessibilityByName(typeAttribute);
                    if (accessLevel != LiveJournalSecurityType.None)
                    {
                        this.Accessibility = accessLevel;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(maskAttribute))
                {
                    int mask;
                    if (int.TryParse(maskAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out mask))
                    {
                        this.Mask = mask;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="LiveJournalSecurity"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            LiveJournalSyndicationExtension extension = new LiveJournalSyndicationExtension();
            writer.WriteStartElement("security", extension.XmlNamespace);

            writer.WriteAttributeString("type", LiveJournalSecurity.AccessibilityAsString(this.Accessibility));

            if (this.Mask != int.MinValue)
            {
                writer.WriteAttributeString("mask", this.Mask.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalSecurity"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="LiveJournalSecurity"/>.</returns>
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

            LiveJournalSecurity value = obj as LiveJournalSecurity;

            if (value != null)
            {
                int result = this.Accessibility.CompareTo(value.Accessibility);
                result = this.Mask.CompareTo(value.Mask);

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
            if (!(obj is LiveJournalSecurity))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
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
    }
}