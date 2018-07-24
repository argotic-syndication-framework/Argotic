namespace Argotic.Extensions.Core
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="DublinCoreElementSetSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class DublinCoreElementSetSyndicationExtensionContext
    {
        /// <summary>
        /// Private member to hold the entity responsible for making contributions to the resource.
        /// </summary>
        private string extensionContributor = string.Empty;

        /// <summary>
        /// Private member to hold the spatial or temporal topic of the resource, the spatial applicability of the resource, or the jurisdiction under which the resource is relevant.
        /// </summary>
        private string extensionCoverage = string.Empty;

        /// <summary>
        /// Private member to hold the entity primarily responsible for making the resource.
        /// </summary>
        private string extensionCreator = string.Empty;

        /// <summary>
        /// Private member to hold an account of the resource.
        /// </summary>
        private string extensionDescription = string.Empty;

        /// <summary>
        /// Private member to hold the file format, physical medium, or dimensions of the resource.
        /// </summary>
        private string extensionFormat = string.Empty;

        /// <summary>
        /// Private member to hold an unambiguous reference to the resource within a given context.
        /// </summary>
        private string extensionIdentifier = string.Empty;

        /// <summary>
        /// Private member to hold the entity responsible for making the resource available.
        /// </summary>
        private string extensionPublisher = string.Empty;

        /// <summary>
        /// Private member to hold a related resource.
        /// </summary>
        private string extensionRelation = string.Empty;

        /// <summary>
        /// Private member to hold information about rights held in and over the resource.
        /// </summary>
        private string extensionRights = string.Empty;

        /// <summary>
        /// Private member to hold a related resource from which the described resource is derived.
        /// </summary>
        private string extesionSource = string.Empty;

        /// <summary>
        /// Private member to hold the topic of the resource.
        /// </summary>
        private string extensionSubject = string.Empty;

        /// <summary>
        /// Private member to hold the name given to the resource.
        /// </summary>
        private string extensionTitle = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> class.
        /// </summary>
        public DublinCoreElementSetSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets the entity responsible for making contributions to the resource.
        /// </summary>
        /// <value>The entity responsible for making contributions to the resource.</value>
        /// <remarks>
        ///     Examples of a Contributor include a person, an organization, or a service. Typically, the name of a <see cref="Contributor"/> should be used to indicate the entity.
        /// </remarks>
        public string Contributor
        {
            get
            {
                return this.extensionContributor;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionContributor = string.Empty;
                }
                else
                {
                    this.extensionContributor = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spatial or temporal topic of the resource, the spatial applicability of the resource, or the jurisdiction under which the resource is relevant.
        /// </summary>
        /// <value>The spatial or temporal topic of the resource, the spatial applicability of the resource, or the jurisdiction under which the resource is relevant.</value>
        /// <remarks>
        ///     <para>
        ///         Spatial topic and spatial applicability may be a named place or a location specified by its geographic coordinates.
        ///         Temporal topic may be a named period, date, or date range.
        ///         A jurisdiction may be a named administrative entity or a geographic place to which the resource applies.
        ///     </para>
        ///     <para>
        ///         Recommended best practice is to use a controlled vocabulary such as the <a href="http://www.getty.edu/research/tools/vocabulary/tgn/index.html">Thesaurus of Geographic Names</a>.
        ///         Where appropriate, named places or time periods can be used in preference to numeric identifiers such as sets of coordinates or date ranges.
        ///     </para>
        /// </remarks>
        public string Coverage
        {
            get
            {
                return this.extensionCoverage;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionCoverage = string.Empty;
                }
                else
                {
                    this.extensionCoverage = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity primarily responsible for making the resource.
        /// </summary>
        /// <value>The entity primarily responsible for making the resource.</value>
        /// <remarks>
        ///     Examples of a <see cref="Creator"/> include a person, an organization, or a service. Typically, the name of a <see cref="Creator"/> should be used to indicate the entity.
        /// </remarks>
        public string Creator
        {
            get
            {
                return this.extensionCreator;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionCreator = string.Empty;
                }
                else
                {
                    this.extensionCreator = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a point or period of time associated with an event in the lifecycle of the resource.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates a point or period of time associated with an event in the lifecycle of the resource.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no date was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime Date { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets an account of the resource.
        /// </summary>
        /// <value>An account of the resource.</value>
        /// <remarks>
        ///     <see cref="Description"/> may include but is not limited to: an abstract, a table of contents, a graphical representation, or a free-text account of the resource.
        /// </remarks>
        public string Description
        {
            get
            {
                return this.extensionDescription;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionDescription = string.Empty;
                }
                else
                {
                    this.extensionDescription = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the file format, physical medium, or dimensions of the resource.
        /// </summary>
        /// <value>The file format, physical medium, or dimensions of the resource.</value>
        /// <remarks>
        ///     Examples of dimensions include size and duration.
        ///     Recommended best practice is to use a controlled vocabulary such as the list of <a href="http://www.iana.org/assignments/media-types/">Internet Media Types</a>.
        /// </remarks>
        public string Format
        {
            get
            {
                return this.extensionFormat;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionFormat = string.Empty;
                }
                else
                {
                    this.extensionFormat = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets an unambiguous reference to the resource within a given context.
        /// </summary>
        /// <value>An unambiguous reference to the resource within a given context.</value>
        /// <remarks>
        ///     Recommended best practice is to identify the resource by means of a string conforming to a formal identification system.
        /// </remarks>
        public string Identifier
        {
            get
            {
                return this.extensionIdentifier;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIdentifier = string.Empty;
                }
                else
                {
                    this.extensionIdentifier = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the language of the resource.
        /// </summary>
        /// <value>A <see cref="CultureInfo"/> object that represents the language of the resource. The default value is a <b>null</b> reference, which indicates that no language was specified.</value>
        /// <remarks>
        ///     Recommended best practice is to use a controlled vocabulary such as <a href="http://www.ietf.org/rfc/rfc4646.txt">RFC 4646</a>.
        ///     This framework conforms to this best practice by utilizing the <see cref="CultureInfo"/> class to represent the language of a resource.
        /// </remarks>
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the entity responsible for making the resource available.
        /// </summary>
        /// <value>The entity responsible for making the resource available.</value>
        /// <remarks>
        ///     Examples of a <see cref="Publisher"/> include a person, an organization, or a service. Typically, the name of a <see cref="Publisher"/> should be used to indicate the entity.
        /// </remarks>
        public string Publisher
        {
            get
            {
                return this.extensionPublisher;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionPublisher = string.Empty;
                }
                else
                {
                    this.extensionPublisher = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource.
        /// </summary>
        /// <value>A related resource.</value>
        /// <remarks>
        ///     Recommended best practice is to identify the related resource by means of a string conforming to a formal identification system.
        /// </remarks>
        public string Relation
        {
            get
            {
                return this.extensionRelation;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionRelation = string.Empty;
                }
                else
                {
                    this.extensionRelation = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets information about rights held in and over the resource.
        /// </summary>
        /// <value>Information about rights held in and over the resource.</value>
        /// <remarks>
        ///     Typically, rights information includes a statement about various property rights associated with the resource, including intellectual property rights.
        /// </remarks>
        public string Rights
        {
            get
            {
                return this.extensionRights;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionRights = string.Empty;
                }
                else
                {
                    this.extensionRights = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource from which the described resource is derived.
        /// </summary>
        /// <value>A related resource from which the described resource is derived.</value>
        /// <remarks>
        ///     The described resource may be derived from the related resource in whole or in part.
        ///     Recommended best practice is to identify the related resource by means of a string conforming to a formal identification system.
        /// </remarks>
        public string Source
        {
            get
            {
                return this.extesionSource;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extesionSource = string.Empty;
                }
                else
                {
                    this.extesionSource = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the topic of the resource.
        /// </summary>
        /// <value>The topic of the resource.</value>
        /// <remarks>
        ///     Typically, the subject will be represented using keywords, key phrases, or classification codes.
        ///     Recommended best practice is to use a controlled vocabulary.
        ///     To describe the spatial or temporal topic of the resource, use the <see cref="Coverage"/> element.
        /// </remarks>
        public string Subject
        {
            get
            {
                return this.extensionSubject;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionSubject = string.Empty;
                }
                else
                {
                    this.extensionSubject = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name given to the resource.
        /// </summary>
        /// <value>The name given to the resource.</value>
        /// <remarks>
        ///     Typically, a <see cref="Title"/> will be a name by which the resource is formally known.
        /// </remarks>
        public string Title
        {
            get
            {
                return this.extensionTitle;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionTitle = string.Empty;
                }
                else
                {
                    this.extensionTitle = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the nature or genre of the resource.
        /// </summary>
        /// <value>
        ///     An <see cref="DublinCoreTypeVocabularies"/> enumeration value that represents the nature or genre of the resource.
        ///     The default value is <see cref="DublinCoreTypeVocabularies.None"/>, which indicates that no nature or genre was specified.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Recommended best practice is to use a controlled vocabulary such as the <a href="http://dublincore.org/documents/dcmi-type-vocabulary/">DCMI Type Vocabulary</a>.
        ///         This framework conforms to this best practice by providing the <see cref="DublinCoreTypeVocabularies"/> enumeration for specifiying the nature or genre of a resource.
        ///     </para>
        ///     <para>To describe the file format, physical medium, or dimensions of the resource, use <see cref="Format"/>.</para>
        /// </remarks>
        public DublinCoreTypeVocabularies TypeVocabulary { get; set; } = DublinCoreTypeVocabularies.None;

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreElementSetSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            wasLoaded = this.LoadCommon(source, manager);

            if (this.LoadOptionals(source, manager))
            {
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Writes the current context to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        public void WriteTo(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (!string.IsNullOrEmpty(this.Contributor))
            {
                writer.WriteElementString("contributor", xmlNamespace, this.Contributor);
            }

            if (!string.IsNullOrEmpty(this.Coverage))
            {
                writer.WriteElementString("coverage", xmlNamespace, this.Coverage);
            }

            if (!string.IsNullOrEmpty(this.Creator))
            {
                writer.WriteElementString("creator", xmlNamespace, this.Creator);
            }

            if (this.Date != DateTime.MinValue)
            {
                writer.WriteElementString("date", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.Date));
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                writer.WriteElementString("description", xmlNamespace, this.Description);
            }

            if (!string.IsNullOrEmpty(this.Format))
            {
                writer.WriteElementString("format", xmlNamespace, this.Format);
            }

            if (!string.IsNullOrEmpty(this.Identifier))
            {
                writer.WriteElementString("identifier", xmlNamespace, this.Identifier);
            }

            if (this.Language != null)
            {
                writer.WriteElementString("language", xmlNamespace, this.Language.Name);
            }

            if (!string.IsNullOrEmpty(this.Publisher))
            {
                writer.WriteElementString("publisher", xmlNamespace, this.Publisher);
            }

            if (!string.IsNullOrEmpty(this.Relation))
            {
                writer.WriteElementString("relation", xmlNamespace, this.Relation);
            }

            if (!string.IsNullOrEmpty(this.Rights))
            {
                writer.WriteElementString("rights", xmlNamespace, this.Rights);
            }

            if (!string.IsNullOrEmpty(this.Source))
            {
                writer.WriteElementString("source", xmlNamespace, this.Source);
            }

            if (!string.IsNullOrEmpty(this.Subject))
            {
                writer.WriteElementString("subject", xmlNamespace, this.Subject);
            }

            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.WriteElementString("title", xmlNamespace, this.Title);
            }

            if (this.TypeVocabulary != DublinCoreTypeVocabularies.None)
            {
                writer.WriteElementString("type", xmlNamespace, DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(this.TypeVocabulary));
            }
        }

        /// <summary>
        /// Initializes the syndication extension context common elements using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreElementSetSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadCommon(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            XPathNavigator contributorNavigator = source.SelectSingleNode("dc:contributor", manager);
            XPathNavigator creatorNavigator = source.SelectSingleNode("dc:creator", manager);
            XPathNavigator dateNavigator = source.SelectSingleNode("dc:date", manager);
            XPathNavigator descriptionNavigator = source.SelectSingleNode("dc:description", manager);
            XPathNavigator languageNavigator = source.SelectSingleNode("dc:language", manager);
            XPathNavigator publisherNavigator = source.SelectSingleNode("dc:publisher", manager);
            XPathNavigator rightsNavigator = source.SelectSingleNode("dc:rights", manager);
            XPathNavigator titleNavigator = source.SelectSingleNode("dc:title", manager);

            if (contributorNavigator != null && !string.IsNullOrEmpty(contributorNavigator.Value))
            {
                this.Contributor = contributorNavigator.Value;
                wasLoaded = true;
            }

            if (creatorNavigator != null && !string.IsNullOrEmpty(creatorNavigator.Value))
            {
                this.Creator = creatorNavigator.Value;
                wasLoaded = true;
            }

            if (dateNavigator != null)
            {
                DateTime date;
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateNavigator.Value, out date))
                {
                    this.Date = date;
                    wasLoaded = true;
                }
            }

            if (descriptionNavigator != null && !string.IsNullOrEmpty(descriptionNavigator.Value))
            {
                this.Description = descriptionNavigator.Value;
                wasLoaded = true;
            }

            if (languageNavigator != null && !string.IsNullOrEmpty(languageNavigator.Value))
            {
                try
                {
                    CultureInfo language = new CultureInfo(languageNavigator.Value);
                    this.Language = language;
                    wasLoaded = true;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning("DublinCoreElementSetSyndicationExtensionContext unable to determine CultureInfo with a name of {0}.", languageNavigator.Value);
                }
            }

            if (publisherNavigator != null && !string.IsNullOrEmpty(publisherNavigator.Value))
            {
                this.Publisher = publisherNavigator.Value;
                wasLoaded = true;
            }

            if (rightsNavigator != null && !string.IsNullOrEmpty(rightsNavigator.Value))
            {
                this.Rights = rightsNavigator.Value;
                wasLoaded = true;
            }

            if (titleNavigator != null && !string.IsNullOrEmpty(titleNavigator.Value))
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension context optional elements using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreElementSetSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            XPathNavigator coverageNavigator = source.SelectSingleNode("dc:coverage", manager);
            XPathNavigator formatNavigator = source.SelectSingleNode("dc:format", manager);
            XPathNavigator identifierNavigator = source.SelectSingleNode("dc:identifier", manager);
            XPathNavigator relationNavigator = source.SelectSingleNode("dc:relation", manager);
            XPathNavigator sourceNavigator = source.SelectSingleNode("dc:source", manager);
            XPathNavigator subjectNavigator = source.SelectSingleNode("dc:subject", manager);
            XPathNavigator typeNavigator = source.SelectSingleNode("dc:type", manager);

            if (coverageNavigator != null && !string.IsNullOrEmpty(coverageNavigator.Value))
            {
                this.Coverage = coverageNavigator.Value;
                wasLoaded = true;
            }

            if (formatNavigator != null && !string.IsNullOrEmpty(formatNavigator.Value))
            {
                this.Format = formatNavigator.Value;
                wasLoaded = true;
            }

            if (identifierNavigator != null && !string.IsNullOrEmpty(identifierNavigator.Value))
            {
                this.Identifier = identifierNavigator.Value;
                wasLoaded = true;
            }

            if (relationNavigator != null && !string.IsNullOrEmpty(relationNavigator.Value))
            {
                this.Relation = relationNavigator.Value;
                wasLoaded = true;
            }

            if (sourceNavigator != null && !string.IsNullOrEmpty(sourceNavigator.Value))
            {
                this.Source = sourceNavigator.Value;
                wasLoaded = true;
            }

            if (subjectNavigator != null && !string.IsNullOrEmpty(subjectNavigator.Value))
            {
                this.Subject = subjectNavigator.Value;
                wasLoaded = true;
            }

            if (typeNavigator != null && !string.IsNullOrEmpty(typeNavigator.Value))
            {
                DublinCoreTypeVocabularies typeVocabulary = DublinCoreElementSetSyndicationExtension.TypeVocabularyByName(typeNavigator.Value);
                if (typeVocabulary != DublinCoreTypeVocabularies.None)
                {
                    this.TypeVocabulary = typeVocabulary;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }
    }
}