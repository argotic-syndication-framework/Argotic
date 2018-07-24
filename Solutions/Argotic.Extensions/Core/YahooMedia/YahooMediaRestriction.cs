﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents a means of allowing restrictions to be placed on the aggregator rendering the media in the feed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Currently, restrictions are based on <b>distributor</b> or <b>country code</b>.
    ///         A <see cref="YahooMediaRestriction"/> is purely informational and no obligation can be assumed or implied.
    ///         Only one <see cref="YahooMediaRestriction"/> object of the same type can be applied to a media object, all others will be ignored.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class YahooMediaRestriction : IComparable
    {

        /// <summary>
        /// Private member to hold the entities the restriction applies to.
        /// </summary>
        private Collection<string> restrictionEntities;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaRestriction"/> class.
        /// </summary>
        public YahooMediaRestriction()
        {
        }

        /// <summary>
        /// Gets the entities this restriction applies to.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="string"/> objects that represent the entities this restriction applies to.
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         To allow a producer to explicitly declare their intentions, two literal entity names are reserved: <b>all</b> and <b>none</b>. These literals can <u>only</u> be used once.
        ///     </para>
        ///     <para>
        ///         When the restriction <see cref="EntityType"/> is <see cref="YahooMediaRestrictionType.Uri"/> the elements in this collection should represent distributor <see cref="Uri">Uri's</see>.
        ///     </para>
        ///     <para>
        ///         When the restriction <see cref="EntityType"/> is <see cref="YahooMediaRestrictionType.Country"/> the elements in this collection should represent country codes.
        ///         See <a href="http://www.iso.org/iso/country_codes/iso_3166_code_lists/english_country_names_and_code_elements.htm">ISO 3166</a> for a listing of the permissible country codes.
        ///     </para>
        /// </remarks>
        public Collection<string> Entities
        {
            get
            {
                if (this.restrictionEntities == null)
                {
                    this.restrictionEntities = new Collection<string>();
                }

                return this.restrictionEntities;
            }
        }

        /// <summary>
        /// Gets or sets the type of media that this restriction applies to.
        /// </summary>
        /// <value>A <see cref="YahooMediaRestrictionType"/> enumeration value that indicates the type of media that this restriction applies to.</value>
        public YahooMediaRestrictionType EntityType { get; set; } = YahooMediaRestrictionType.None;

        /// <summary>
        /// Gets or sets the type of relationship that this restriction represents.
        /// </summary>
        /// <value>A <see cref="YahooMediaRestrictionRelationship"/> enumeration value that indicates the type of relationship that this restriction represents.</value>
        public YahooMediaRestrictionRelationship Relationship { get; set; } = YahooMediaRestrictionRelationship.None;

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(YahooMediaRestriction first, YahooMediaRestriction second)
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
        public static bool operator !=(YahooMediaRestriction first, YahooMediaRestriction second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(YahooMediaRestriction first, YahooMediaRestriction second)
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
        public static bool operator >(YahooMediaRestriction first, YahooMediaRestriction second)
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
        /// Returns the relationship identifier for the supplied <see cref="YahooMediaRestrictionRelationship"/>.
        /// </summary>
        /// <param name="relationship">The <see cref="YahooMediaRestrictionRelationship"/> to get the relationship identifier for.</param>
        /// <returns>The relationship identifier for the supplied <paramref name="relationship"/>, otherwise returns an empty string.</returns>
        public static string RelationshipAsString(YahooMediaRestrictionRelationship relationship)
        {
            string name = string.Empty;
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaRestrictionRelationship).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaRestrictionRelationship))
                {
                    YahooMediaRestrictionRelationship restrictionRelationship = (YahooMediaRestrictionRelationship)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (restrictionRelationship == relationship)
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
        /// Returns the <see cref="YahooMediaRestrictionRelationship"/> enumeration value that corresponds to the specified relationship name.
        /// </summary>
        /// <param name="name">The name of the relationship.</param>
        /// <returns>A <see cref="YahooMediaRestrictionRelationship"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaRestrictionRelationship.None</b>.</returns>
        /// <remarks>This method disregards case of specified relationship name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaRestrictionRelationship RelationshipByName(string name)
        {
            YahooMediaRestrictionRelationship restrictionRelationship = YahooMediaRestrictionRelationship.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaRestrictionRelationship).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaRestrictionRelationship))
                {
                    YahooMediaRestrictionRelationship relationship = (YahooMediaRestrictionRelationship)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            restrictionRelationship = relationship;
                            break;
                        }
                    }
                }
            }

            return restrictionRelationship;
        }

        /// <summary>
        /// Returns the restriction type identifier for the supplied <see cref="YahooMediaRestrictionType"/>.
        /// </summary>
        /// <param name="type">The <see cref="YahooMediaRestrictionType"/> to get the restriction type identifier for.</param>
        /// <returns>The restriction type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        public static string RestrictionTypeAsString(YahooMediaRestrictionType type)
        {
            string name = string.Empty;
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaRestrictionType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaRestrictionType))
                {
                    YahooMediaRestrictionType restrictionType = (YahooMediaRestrictionType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (restrictionType == type)
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
        /// Returns the <see cref="YahooMediaRestrictionType"/> enumeration value that corresponds to the specified restriction type name.
        /// </summary>
        /// <param name="name">The name of the restriction type.</param>
        /// <returns>A <see cref="YahooMediaRestrictionType"/> enumeration value that corresponds to the specified string, otherwise returns <b>YahooMediaRestrictionType.None</b>.</returns>
        /// <remarks>This method disregards case of specified restriction type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static YahooMediaRestrictionType RestrictionTypeByName(string name)
        {
            YahooMediaRestrictionType restrictionType = YahooMediaRestrictionType.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(YahooMediaRestrictionType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(YahooMediaRestrictionType))
                {
                    YahooMediaRestrictionType type = (YahooMediaRestrictionType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            restrictionType = type;
                            break;
                        }
                    }
                }
            }

            return restrictionType;
        }

        /// <summary>
        /// Loads this <see cref="YahooMediaRestriction"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaRestriction"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaRestriction"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string relationshipAttribute = source.GetAttribute("relationship", string.Empty);
                string typeAttribute = source.GetAttribute("type", string.Empty);

                if (!string.IsNullOrEmpty(relationshipAttribute))
                {
                    YahooMediaRestrictionRelationship relationship = YahooMediaRestriction.RelationshipByName(relationshipAttribute);
                    if (relationship != YahooMediaRestrictionRelationship.None)
                    {
                        this.Relationship = relationship;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    YahooMediaRestrictionType type = YahooMediaRestriction.RestrictionTypeByName(typeAttribute);
                    if (type != YahooMediaRestrictionType.None)
                    {
                        this.EntityType = type;
                        wasLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(source.Value))
            {
                if (source.Value.Contains(" "))
                {
                    string[] entities = source.Value.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (entities.Length > 0)
                    {
                        foreach (string entity in entities)
                        {
                            this.Entities.Add(entity);
                        }

                        wasLoaded = true;
                    }
                }
                else
                {
                    this.Entities.Add(source.Value);
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="YahooMediaRestriction"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            YahooMediaSyndicationExtension extension = new YahooMediaSyndicationExtension();
            writer.WriteStartElement("restriction", extension.XmlNamespace);

            if (this.Relationship != YahooMediaRestrictionRelationship.None)
            {
                writer.WriteAttributeString("relationship", YahooMediaRestriction.RelationshipAsString(this.Relationship));
            }

            if (this.EntityType != YahooMediaRestrictionType.None)
            {
                writer.WriteAttributeString("type", YahooMediaRestriction.RestrictionTypeAsString(this.EntityType));
            }

            if (this.Entities.Count > 0)
            {
                string[] entities = new string[this.Entities.Count];
                this.Entities.CopyTo(entities, 0);

                writer.WriteString(string.Join(" ", entities));
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaRestriction"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaRestriction"/>.</returns>
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

            YahooMediaRestriction value = obj as YahooMediaRestriction;

            if (value != null)
            {
                int result = ComparisonUtility.CompareSequence(this.Entities, value.Entities, StringComparison.Ordinal);
                result = result | this.EntityType.CompareTo(value.EntityType);
                result = result | this.Relationship.CompareTo(value.Relationship);

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
            if (!(obj is YahooMediaRestriction))
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