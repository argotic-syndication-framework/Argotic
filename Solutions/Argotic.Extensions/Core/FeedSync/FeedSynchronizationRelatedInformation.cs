namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Represents information about related feeds or locations.
    /// </summary>
    /// <remarks>
    ///     In the case where a publisher’s feed has incorporated items from other feeds, it can be useful for subscribers to see more detailed information about the other feeds.
    ///     In the case of feed sharing as envisioned by the <i>FeedSync</i> specification, this class can also be used to notify subscribing feeds of the feeds of other participants
    ///     which they might also wish to subscribe to.
    /// </remarks>
    /// <seealso cref="FeedSynchronizationSyndicationExtensionContext"/>
    [Serializable]
    public class FeedSynchronizationRelatedInformation : IComparable
    {
        /// <summary>
        /// Private member to hold the URI for the related feed.
        /// </summary>
        private Uri relatedInformationLink;

        /// <summary>
        /// Private member to hold the name or description of the related feed.
        /// </summary>
        private string relatedInformationTitle = string.Empty;

        /// <summary>
        /// Private member to hold the type of the related feed.
        /// </summary>
        private FeedSynchronizationRelatedInformationType relatedInformationType = FeedSynchronizationRelatedInformationType.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class.
        /// </summary>
        public FeedSynchronizationRelatedInformation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class using the supplied <see cref="Uri"/> and <see cref="FeedSynchronizationRelatedInformationType"/>.
        /// </summary>
        /// <param name="link">A <see cref="Uri"/> that represents the URI for this related feed.</param>
        /// <param name="type">A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="type"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
        public FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type)
        {
            this.Link = link;
            this.RelationType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedSynchronizationRelatedInformation"/> class using the supplied <see cref="Uri"/> and <see cref="FeedSynchronizationRelatedInformationType"/>.
        /// </summary>
        /// <param name="link">A <see cref="Uri"/> that represents the URI for this related feed.</param>
        /// <param name="type">A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.</param>
        /// <param name="title">The name or description of this related feed.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">The <paramref name="type"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
        public FeedSynchronizationRelatedInformation(Uri link, FeedSynchronizationRelatedInformationType type, string title)
            : this(link, type)
        {
            this.Title = title;
        }

        /// <summary>
        /// Gets or sets the URI for this related feed.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URI for this related feed.</value>
        /// <remarks>
        ///     The value <b>must not</b> be a relative reference.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Link
        {
            get
            {
                return this.relatedInformationLink;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.relatedInformationLink = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the related feed.
        /// </summary>
        /// <value>
        ///     A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration values that represents the type of the related feed.
        ///     The default value is <see cref="FeedSynchronizationRelatedInformationType.None"/>, which indicates that no relation type has been specified.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Publishers will generally include, in a feed, only the most recent modifications, additions, and deletions within some reasonable time window.
        ///         These feeds are referred to as <i>partial feeds</i>, whereas feeds containing the complete set of items are referred to as <i>complete feeds</i>.
        ///     </para>
        ///     <para>
        ///         In the feed sharing context new subscribers, or existing subscribers failing to subscribe within the published feed window, will need to initially
        ///         copy a complete set of items from a publisher before being in a position to process incremental updates. As such, the specification provides for the
        ///         ability for the latter feed to reference the complete feed.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentException">The <paramref name="value"/> is equal to <see cref="FeedSynchronizationRelatedInformationType.None"/>.</exception>
        public FeedSynchronizationRelatedInformationType RelationType
        {
            get
            {
                return this.relatedInformationType;
            }

            set
            {
                if (value == FeedSynchronizationRelatedInformationType.None)
                {
                    throw new ArgumentException(string.Format(null, "The specified relation type of {0} is invalid.", value), "value");
                }

                this.relatedInformationType = value;
            }
        }

        /// <summary>
        /// Gets or sets the name or description of this related feed.
        /// </summary>
        /// <value>The name or description of this related feed.</value>
        public string Title
        {
            get
            {
                return this.relatedInformationTitle;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.relatedInformationTitle = string.Empty;
                }
                else
                {
                    this.relatedInformationTitle = value.Trim();
                }
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
        public static bool operator !=(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
        public static bool operator >(FeedSynchronizationRelatedInformation first, FeedSynchronizationRelatedInformation second)
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
        /// Returns the relation type identifier for the supplied <see cref="FeedSynchronizationRelatedInformationType"/>.
        /// </summary>
        /// <param name="type">The <see cref="FeedSynchronizationRelatedInformationType"/> to get the relation type identifier for.</param>
        /// <returns>The relation type identifier for the supplied <paramref name="type"/>, otherwise returns an empty string.</returns>
        public static string RelationTypeAsString(FeedSynchronizationRelatedInformationType type)
        {
            string name = string.Empty;
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationRelatedInformationType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationRelatedInformationType))
                {
                    FeedSynchronizationRelatedInformationType relationType = (FeedSynchronizationRelatedInformationType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (relationType == type)
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
        /// Returns the <see cref="FeedSynchronizationRelatedInformationType"/> enumeration value that corresponds to the specified relation type name.
        /// </summary>
        /// <param name="name">The name of the relation type.</param>
        /// <returns>A <see cref="FeedSynchronizationRelatedInformationType"/> enumeration value that corresponds to the specified string, otherwise returns <b>FeedSynchronizationRelatedInformationType.None</b>.</returns>
        /// <remarks>This method disregards case of specified relation type name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static FeedSynchronizationRelatedInformationType RelationTypeByName(string name)
        {
            FeedSynchronizationRelatedInformationType relationType = FeedSynchronizationRelatedInformationType.None;
            Guard.ArgumentNotNullOrEmptyString(name, "name");
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(FeedSynchronizationRelatedInformationType).GetFields())
            {
                if (fieldInfo.FieldType == typeof(FeedSynchronizationRelatedInformationType))
                {
                    FeedSynchronizationRelatedInformationType type = (FeedSynchronizationRelatedInformationType)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            relationType = type;
                            break;
                        }
                    }
                }
            }

            return relationType;
        }

        /// <summary>
        /// Loads this <see cref="FeedSynchronizationRelatedInformation"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="FeedSynchronizationRelatedInformation"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="FeedSynchronizationRelatedInformation"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            if (source.HasAttributes)
            {
                string linkAttribute = source.GetAttribute("link", string.Empty);
                string titleAttribute = source.GetAttribute("title", string.Empty);
                string typeAttribute = source.GetAttribute("type", string.Empty);

                if (!string.IsNullOrEmpty(linkAttribute))
                {
                    Uri link;
                    if (Uri.TryCreate(linkAttribute, UriKind.Absolute, out link))
                    {
                        this.Link = link;
                        wasLoaded = true;
                    }
                }

                if (!string.IsNullOrEmpty(titleAttribute))
                {
                    this.Title = titleAttribute;
                    wasLoaded = true;
                }

                if (!string.IsNullOrEmpty(typeAttribute))
                {
                    FeedSynchronizationRelatedInformationType type = FeedSynchronizationRelatedInformation.RelationTypeByName(typeAttribute);
                    if (type != FeedSynchronizationRelatedInformationType.None)
                    {
                        this.RelationType = type;
                        wasLoaded = true;
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Saves the current <see cref="FeedSynchronizationRelatedInformation"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            FeedSynchronizationSyndicationExtension extension = new FeedSynchronizationSyndicationExtension();
            writer.WriteStartElement("related", extension.XmlNamespace);

            writer.WriteAttributeString("link", extension.XmlNamespace, this.Link != null ? this.Link.ToString() : string.Empty);
            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.WriteAttributeString("title", extension.XmlNamespace, this.Title);
            }

            writer.WriteAttributeString("type", extension.XmlNamespace, FeedSynchronizationRelatedInformation.RelationTypeAsString(this.RelationType));

            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="FeedSynchronizationRelatedInformation"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="FeedSynchronizationRelatedInformation"/>.</returns>
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

            FeedSynchronizationRelatedInformation value = obj as FeedSynchronizationRelatedInformation;

            if (value != null)
            {
                int result = Uri.Compare(this.Link, value.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);
                result = result | this.RelationType.CompareTo(value.RelationType);

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
            if (!(obj is FeedSynchronizationRelatedInformation))
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