namespace Argotic.Extensions.Core
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Extends syndication specifications to provide a meta-data term resource description vocabulary.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="DublinCoreMetadataTermsSyndicationExtension"/> extends syndicated content to specify all metadata terms maintained by the Dublin Core Metadata Initiative.
    ///         This syndication extension conforms to the <b>Dublin Core Metadata Initiative (DCMI) Metadata Terms</b> 1.0 specification, which can be found
    ///         at <a href="http://dublincore.org/documents/dcmi-terms/">http://dublincore.org/documents/dcmi-terms/</a>.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the DublinCoreMetadataTermsSyndicationExtension class.">
    ///         <code source="..\..\Argotic.Examples\Extensions\Core\DublinCoreMetadataTermsSyndicationExtensionExample.cs" region="DublinCoreMetadataTermsSyndicationExtension" />
    ///     </code>
    /// </example>
    [Serializable]
    public class DublinCoreMetadataTermsSyndicationExtension : SyndicationExtension, IComparable
    {
        /// <summary>
        /// Private member to hold specific information about the extension.
        /// </summary>
        private DublinCoreMetadataTermsSyndicationExtensionContext extensionContext = new DublinCoreMetadataTermsSyndicationExtensionContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="DublinCoreMetadataTermsSyndicationExtension"/> class.
        /// </summary>
        public DublinCoreMetadataTermsSyndicationExtension()
            : base("dcterms", "http://purl.org/dc/terms/", new Version("1.0"), new Uri("http://dublincore.org/documents/dcmi-terms/"), "Dublin Core Metadata Terms", "Extends syndication feeds to provide a meta-data term resource description vocabulary.")
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> object associated with this extension.
        /// </summary>
        /// <value>A <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
        /// <remarks>
        ///     The <b>Context</b> encapsulates all of the syndication extension information that can be retrieved or written to an extended syndication entity.
        ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
        ///     are defined for the custom syndication extension.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public DublinCoreMetadataTermsSyndicationExtensionContext Context
        {
            get
            {
                return this.extensionContext;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.extensionContext = value;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(DublinCoreMetadataTermsSyndicationExtension first, DublinCoreMetadataTermsSyndicationExtension second)
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
        public static bool operator !=(DublinCoreMetadataTermsSyndicationExtension first, DublinCoreMetadataTermsSyndicationExtension second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(DublinCoreMetadataTermsSyndicationExtension first, DublinCoreMetadataTermsSyndicationExtension second)
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
        public static bool operator >(DublinCoreMetadataTermsSyndicationExtension first, DublinCoreMetadataTermsSyndicationExtension second)
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
        /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
        /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
        /// </summary>
        /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
        /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool MatchByType(ISyndicationExtension extension)
        {
            Guard.ArgumentNotNull(extension, "extension");

            return extension.GetType() == typeof(DublinCoreMetadataTermsSyndicationExtension);
        }

        /// <summary>
        /// Returns the type vocabulary identifier for the supplied <see cref="DublinCoreTypeVocabularies"/>.
        /// </summary>
        /// <param name="vocabulary">The <see cref="DublinCoreTypeVocabularies"/> to get the type vocabulary identifier for.</param>
        /// <returns>The type vocabulary identifier for the supplied <paramref name="vocabulary"/>, otherwise returns an empty string.</returns>
        public static string TypeVocabularyAsString(DublinCoreTypeVocabularies vocabulary)
        {
            string name = string.Empty;

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(DublinCoreTypeVocabularies).GetFields())
            {
                if (fieldInfo.FieldType == typeof(DublinCoreTypeVocabularies))
                {
                    DublinCoreTypeVocabularies typeVocabulary = (DublinCoreTypeVocabularies)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (typeVocabulary == vocabulary)
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
        /// Returns the <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified type vocabulary name.
        /// </summary>
        /// <param name="name">The name of the type vocabulary.</param>
        /// <returns>A <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified string, otherwise returns <b>DublinCoreTypeVocabularies.None</b>.</returns>
        /// <remarks>This method disregards case of specified type vocabulary name.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
        public static DublinCoreTypeVocabularies TypeVocabularyByName(string name)
        {
            DublinCoreTypeVocabularies typeVocabulary = DublinCoreTypeVocabularies.None;

            Guard.ArgumentNotNullOrEmptyString(name, "name");

            foreach (System.Reflection.FieldInfo fieldInfo in typeof(DublinCoreTypeVocabularies).GetFields())
            {
                if (fieldInfo.FieldType == typeof(DublinCoreTypeVocabularies))
                {
                    DublinCoreTypeVocabularies vocabulary = (DublinCoreTypeVocabularies)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        if (string.Compare(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            typeVocabulary = vocabulary;
                            break;
                        }
                    }
                }
            }

            return typeVocabulary;
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
        /// </summary>
        /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(IXPathNavigable source)
        {
            bool wasLoaded = false;

            Guard.ArgumentNotNull(source, "source");

            XPathNavigator navigator = source.CreateNavigator();
            wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
            SyndicationExtensionLoadedEventArgs args = new SyndicationExtensionLoadedEventArgs(source, this);
            this.OnExtensionLoaded(args);

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference (Nothing in Visual Basic).</exception>
        public override bool Load(XmlReader reader)
        {
            Guard.ArgumentNotNull(reader, "reader");

            XPathDocument document = new XPathDocument(reader);

            return this.Load(document.CreateNavigator());
        }

        /// <summary>
        /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public override void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");

            this.Context.WriteTo(writer, this.XmlNamespace);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.</returns>
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

            DublinCoreMetadataTermsSyndicationExtension value = obj as DublinCoreMetadataTermsSyndicationExtension;

            if (value != null)
            {
                int result = string.Compare(this.Description, value.Description, StringComparison.OrdinalIgnoreCase);
                result = result | Uri.Compare(this.Documentation, value.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Name, value.Name, StringComparison.OrdinalIgnoreCase);
                result = result | this.Version.CompareTo(value.Version);
                result = result | string.Compare(this.XmlNamespace, value.XmlNamespace, StringComparison.Ordinal);
                result = result | string.Compare(this.XmlPrefix, value.XmlPrefix, StringComparison.Ordinal);
                result = result | string.Compare(this.Context.Abstract, value.Context.Abstract, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.AccessRights, value.Context.AccessRights, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.AccrualMethod, value.Context.AccrualMethod, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.AccrualPeriodicity, value.Context.AccrualPeriodicity, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.AccrualPolicy, value.Context.AccrualPolicy, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.AlternativeTitle, value.Context.AlternativeTitle, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Audience, value.Context.Audience, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.AudienceEducationLevel, value.Context.AudienceEducationLevel, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.BibliographicCitation, value.Context.BibliographicCitation, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.ConformsTo, value.Context.ConformsTo, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Contributor, value.Context.Contributor, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Coverage, value.Context.Coverage, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Creator, value.Context.Creator, StringComparison.OrdinalIgnoreCase);
                result = result | this.Context.Date.CompareTo(value.Context.Date);
                result = result | this.Context.DateAccepted.CompareTo(value.Context.DateAccepted);
                result = result | string.Compare(this.Context.DateAvailable, value.Context.DateAvailable, StringComparison.OrdinalIgnoreCase);
                result = result | this.Context.DateCopyrighted.CompareTo(value.Context.DateCopyrighted);
                result = result | this.Context.DateCreated.CompareTo(value.Context.DateCreated);
                result = result | this.Context.DateIssued.CompareTo(value.Context.DateIssued);
                result = result | this.Context.DateModified.CompareTo(value.Context.DateModified);
                result = result | this.Context.DateSubmitted.CompareTo(value.Context.DateSubmitted);
                result = result | string.Compare(this.Context.DateValid, value.Context.DateValid, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Description, value.Context.Description, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Extent, value.Context.Extent, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Format, value.Context.Format, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.HasFormat, value.Context.HasFormat, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.HasPart, value.Context.HasPart, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.HasVersion, value.Context.HasVersion, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Identifier, value.Context.Identifier, StringComparison.Ordinal);
                result = result | string.Compare(this.Context.InstructionalMethod, value.Context.InstructionalMethod, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.IsFormatOf, value.Context.IsFormatOf, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.IsPartOf, value.Context.IsPartOf, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.IsReferencedBy, value.Context.IsReferencedBy, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.IsReplacedBy, value.Context.IsReplacedBy, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.IsRequiredBy, value.Context.IsRequiredBy, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.IsVersionOf, value.Context.IsVersionOf, StringComparison.OrdinalIgnoreCase);

                if (this.Context.Language != null)
                {
                    if (value.Context.Language != null)
                    {
                        result = result | string.Compare(this.Context.Language.Name, value.Context.Language.Name, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        result = result | 1;
                    }
                }
                else if (this.Context.Language == null && value.Context.Language != null)
                {
                    result = result | -1;
                }

                result = result | string.Compare(this.Context.License, value.Context.License, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Mediator, value.Context.Mediator, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Medium, value.Context.Medium, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Provenance, value.Context.Provenance, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Publisher, value.Context.Publisher, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.References, value.Context.References, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Relation, value.Context.Relation, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Replaces, value.Context.Replaces, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Requires, value.Context.Requires, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Rights, value.Context.Rights, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.RightsHolder, value.Context.RightsHolder, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Source, value.Context.Source, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.SpatialCoverage, value.Context.SpatialCoverage, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Subject, value.Context.Subject, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.TableOfContents, value.Context.TableOfContents, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.TemporalCoverage, value.Context.TemporalCoverage, StringComparison.OrdinalIgnoreCase);
                result = result | string.Compare(this.Context.Title, value.Context.Title, StringComparison.OrdinalIgnoreCase);
                result = result | this.Context.TypeVocabulary.CompareTo(value.Context.TypeVocabulary);

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
            if (!(obj is DublinCoreMetadataTermsSyndicationExtension))
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