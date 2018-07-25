namespace Argotic.Extensions.Core
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class DublinCoreMetadataTermsSyndicationExtensionContext
    {
        /// <summary>
        /// Private member to hold the summary of the resource.
        /// </summary>
        private string extensionAbstract = string.Empty;

        /// <summary>
        /// Private member to hold information about who can access the resource or an indication of its security status.
        /// </summary>
        private string extensionAccessRights = string.Empty;

        /// <summary>
        /// Private member to hold the method by which items are added to a collection.
        /// </summary>
        private string extensionAccrualMethod = string.Empty;

        /// <summary>
        /// Private member to hold the frequency with which items are added to a collection.
        /// </summary>
        private string extensionAccrualPeriodicity = string.Empty;

        /// <summary>
        /// Private member to hold the policy governing the addition of items to a collection.
        /// </summary>
        private string extensionAccrualPolicy = string.Empty;

        /// <summary>
        /// Private member to hold an alternative name for the resource.
        /// </summary>
        private string extensionAlternativeTitle = string.Empty;

        /// <summary>
        /// Private member to hold the class of entity for whom the resource is intended or useful.
        /// </summary>
        private string extensionAudience = string.Empty;

        /// <summary>
        /// Private member to hold the date (often a range) that the resource became or will become available.
        /// </summary>
        private string extensionDateAvailable = string.Empty;

        /// <summary>
        /// Private member to hold the bibliographic reference for the resource.
        /// </summary>
        private string extensionBibliographicCitation = string.Empty;

        /// <summary>
        /// Private member to hold the established standard to which the described resource conforms.
        /// </summary>
        private string extensionConformsTo = string.Empty;

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
        /// Private member to hold the class of entity, defined in terms of progression through an educational or training context, for which the described resource is intended.
        /// </summary>
        private string extensionAudienceEducationLevel = string.Empty;

        /// <summary>
        /// Private member to hold the size or duration of the resource.
        /// </summary>
        private string extensionExtent = string.Empty;

        /// <summary>
        /// Private member to hold the file format, physical medium, or dimensions of the resource.
        /// </summary>
        private string extensionFormat = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is substantially the same as the pre-existing described resource, but in another format.
        /// </summary>
        private string extensionHasFormat = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is included either physically or logically in the described resource.
        /// </summary>
        private string extensionHasPart = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is a version, edition, or adaptation of the described resource.
        /// </summary>
        private string extensionHasVersion = string.Empty;

        /// <summary>
        /// Private member to hold an unambiguous reference to the resource within a given context.
        /// </summary>
        private string extensionIdentifier = string.Empty;

        /// <summary>
        /// Private member to hold the process, used to engender knowledge, attitudes and skills, that the described resource is designed to support.
        /// </summary>
        private string extensionInstructionalMethod = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is substantially the same as the described resource, but in another format.
        /// </summary>
        private string extensionIsFormatOf = string.Empty;

        /// <summary>
        /// Private member to hold a related resource in which the described resource is physically or logically included.
        /// </summary>
        private string extensionIsPartOf = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that references, cites, or otherwise points to the described resource.
        /// </summary>
        private string extensionIsReferencedBy = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that supplants, displaces, or supersedes the described resource.
        /// </summary>
        private string extensionIsReplacedBy = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that requires the described resource to support its function, delivery, or coherence.
        /// </summary>
        private string extensionIsRequiredBy = string.Empty;

        /// <summary>
        /// Private member to hold a related resource of which the described resource is a version, edition, or adaptation.
        /// </summary>
        private string extensionIsVersionOf = string.Empty;

        /// <summary>
        /// Private member to hold the legal document giving official permission to do something with the resource.
        /// </summary>
        private string extensionLicense = string.Empty;

        /// <summary>
        /// Private member to hold the entity that mediates access to the resource and for whom the resource is intended or useful.
        /// </summary>
        private string extensionMediator = string.Empty;

        /// <summary>
        /// Private member to hold the material or physical carrier of the resource.
        /// </summary>
        private string extensionMedium = string.Empty;

        /// <summary>
        /// Private member to hold the statement of any changes in ownership and custody of the resource since its creation that are significant for its authenticity, integrity, and interpretation.
        /// </summary>
        private string extensionProvenance = string.Empty;

        /// <summary>
        /// Private member to hold the entity responsible for making the resource available.
        /// </summary>
        private string extensionPublisher = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is referenced, cited, or otherwise pointed to by the described resource.
        /// </summary>
        private string extensionReferences = string.Empty;

        /// <summary>
        /// Private member to hold a related resource.
        /// </summary>
        private string extensionRelation = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is supplanted, displaced, or superseded by the described resource.
        /// </summary>
        private string extensionReplaces = string.Empty;

        /// <summary>
        /// Private member to hold a related resource that is required by the described resource to support its function, delivery, or coherence.
        /// </summary>
        private string extensionRequires = string.Empty;

        /// <summary>
        /// Private member to hold information about rights held in and over the resource.
        /// </summary>
        private string extensionRights = string.Empty;

        /// <summary>
        /// Private member to hold the person or organization owning or managing rights over the resource.
        /// </summary>
        private string extensionRightsHolder = string.Empty;

        /// <summary>
        /// Private member to hold a related resource from which the described resource is derived.
        /// </summary>
        private string extensionSource = string.Empty;

        /// <summary>
        /// Private member to hold the spatial characteristics of the resource.
        /// </summary>
        private string extensionSpatialCoverage = string.Empty;

        /// <summary>
        /// Private member to hold the topic of the resource.
        /// </summary>
        private string extensionSubject = string.Empty;

        /// <summary>
        /// Private member to hold the list of sub-units of the resource.
        /// </summary>
        private string extensionTableOfContents = string.Empty;

        /// <summary>
        /// Private member to hold the temporal characteristics of the resource.
        /// </summary>
        private string extensionTemporalCoverage = string.Empty;

        /// <summary>
        /// Private member to hold the name given to the resource.
        /// </summary>
        private string extensionTitle = string.Empty;

        /// <summary>
        /// Private member to hold the date (often a range) of validity of the resource.
        /// </summary>
        private string extensionDateValid = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> class.
        /// </summary>
        public DublinCoreMetadataTermsSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets the summary of the resource.
        /// </summary>
        /// <value>The summary of the resource.</value>
        public string Abstract
        {
            get
            {
                return this.extensionAbstract;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAbstract = string.Empty;
                }
                else
                {
                    this.extensionAbstract = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets information about who can access the resource or an indication of its security status.
        /// </summary>
        /// <value>Information about who can access the resource or an indication of its security status.</value>
        /// <remarks>
        ///     <para><see cref="AccessRights"/> may include information regarding access or restrictions based on privacy, security, or other policies.</para>
        ///     <para>
        ///         The <see cref="AccessRights"/> property represents a statement about the intellectual property rights (IPR) held in or over a resource,
        ///         a legal document giving official permission to do something with a resource, or a statement about access rights.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/RightsStatement</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-RightsStatement">http://dublincore.org/documents/dcmi-terms/#classes-RightsStatement</a> for further information.
        ///     </para>
        /// </remarks>
        public string AccessRights
        {
            get
            {
                return this.extensionAccessRights;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAccessRights = string.Empty;
                }
                else
                {
                    this.extensionAccessRights = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the method by which items are added to a collection.
        /// </summary>
        /// <value>The method by which items are added to a collection.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AccrualMethod"/> property represents the method by which resources are added to a collection.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/MethodOfAccrual</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-MethodOfAccrual">http://dublincore.org/documents/dcmi-terms/#classes-MethodOfAccrual</a> for further information.
        ///     </para>
        /// </remarks>
        public string AccrualMethod
        {
            get
            {
                return this.extensionAccrualMethod;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAccrualMethod = string.Empty;
                }
                else
                {
                    this.extensionAccrualMethod = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the frequency with which items are added to a collection.
        /// </summary>
        /// <value>The frequency with which items are added to a collection.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AccrualPeriodicity"/> property represents the rate at which something recurs.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Frequency</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Frequency">http://dublincore.org/documents/dcmi-terms/#classes-Frequency</a> for further information.
        ///     </para>
        /// </remarks>
        public string AccrualPeriodicity
        {
            get
            {
                return this.extensionAccrualPeriodicity;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAccrualPeriodicity = string.Empty;
                }
                else
                {
                    this.extensionAccrualPeriodicity = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets thepolicy governing the addition of items to a collection.
        /// </summary>
        /// <value>The policy governing the addition of items to a collection.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AccrualPolicy"/> property represents the plan or course of action by an authority, intended to influence and determine decisions, actions, and other matters.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Policy</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Policy">http://dublincore.org/documents/dcmi-terms/#classes-Policy</a> for further information.
        ///     </para>
        /// </remarks>
        public string AccrualPolicy
        {
            get
            {
                return this.extensionAccrualPolicy;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAccrualPolicy = string.Empty;
                }
                else
                {
                    this.extensionAccrualPolicy = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the alternative name for the resource.
        /// </summary>
        /// <value>The alternative name for the resource.</value>
        /// <remarks>
        ///     The distinction between titles and alternative titles is application-specific.
        /// </remarks>
        public string AlternativeTitle
        {
            get
            {
                return this.extensionAlternativeTitle;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAlternativeTitle = string.Empty;
                }
                else
                {
                    this.extensionAlternativeTitle = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the class of entity for whom the resource is intended or useful.
        /// </summary>
        /// <value>The class of entity for whom the resource is intended or useful.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="Audience"/> property represents a group of resources that act or have the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/AgentClass</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-AgentClass">http://dublincore.org/documents/dcmi-terms/#classes-AgentClass</a> for further information.
        ///     </para>
        /// </remarks>
        public string Audience
        {
            get
            {
                return this.extensionAudience;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAudience = string.Empty;
                }
                else
                {
                    this.extensionAudience = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the class of entity, defined in terms of progression through an educational or training context, for which the described resource is intended.
        /// </summary>
        /// <value>The class of entity, defined in terms of progression through an educational or training context, for which the described resource is intended.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AudienceEducationLevel"/> property represents a group of resources that act or have the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/AgentClass</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-AgentClass">http://dublincore.org/documents/dcmi-terms/#classes-AgentClass</a> for further information.
        ///     </para>
        /// </remarks>
        public string AudienceEducationLevel
        {
            get
            {
                return this.extensionAudienceEducationLevel;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionAudienceEducationLevel = string.Empty;
                }
                else
                {
                    this.extensionAudienceEducationLevel = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bibliographic reference for the resource.
        /// </summary>
        /// <value>The bibliographic reference for the resource.</value>
        /// <remarks>
        ///     Recommended practice is to include sufficient bibliographic detail to identify the resource as unambiguously as possible.
        /// </remarks>
        public string BibliographicCitation
        {
            get
            {
                return this.extensionBibliographicCitation;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionBibliographicCitation = string.Empty;
                }
                else
                {
                    this.extensionBibliographicCitation = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the established standard to which the described resource conforms.
        /// </summary>
        /// <value>The established standard to which the described resource conforms.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="ConformsTo"/> property represents a basis for comparison; a reference point against which other things can be evaluated.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Standard</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Standard">http://dublincore.org/documents/dcmi-terms/#classes-Standard</a> for further information.
        ///     </para>
        /// </remarks>
        public string ConformsTo
        {
            get
            {
                return this.extensionConformsTo;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionConformsTo = string.Empty;
                }
                else
                {
                    this.extensionConformsTo = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity responsible for making contributions to the resource.
        /// </summary>
        /// <value>The entity responsible for making contributions to the resource.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="Contributor"/> property represents a resource that acts or has the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Agent</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Agent">http://dublincore.org/documents/dcmi-terms/#classes-Agent</a> for further information.
        ///     </para>
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
        ///         Recommended best practice is to use a controlled vocabulary such as the <a href="http://www.getty.edu/research/tools/vocabulary/tgn/index.html">Thesaurus of Geographic Names</a>.
        ///         Where appropriate, named places or time periods can be used in preference to numeric identifiers such as sets of coordinates or date ranges.
        ///     </para>
        ///     <para>
        ///         The <see cref="Coverage"/> property represents a location, period of time, or jurisdiction.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/LocationPeriodOrJurisdiction</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-LocationPeriodOrJurisdiction">http://dublincore.org/documents/dcmi-terms/#classes-LocationPeriodOrJurisdiction</a> for further information.
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
        ///     <para>Examples of a <see cref="Creator"/> include a person, an organization, or a service. Typically, the name of a <see cref="Creator"/> should be used to indicate the entity.</para>
        ///     <para>
        ///         The <see cref="Creator"/> property represents a resource that acts or has the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Agent</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Agent">http://dublincore.org/documents/dcmi-terms/#classes-Agent</a> for further information.
        ///     </para>
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
        ///     Date may be used to express temporal information at any level of granularity. The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime Date { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date of acceptance of the resource.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the acceptance date of the resource.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no acceptance date was provided.
        /// </value>
        /// <remarks>
        ///     <para>The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).</para>
        ///     <para>
        ///         Examples of resources to which a <see cref="DateAccepted">Date Accepted</see> may be relevant are a thesis (accepted by a university department) or an article (accepted by a journal).
        ///     </para>
        /// </remarks>
        public DateTime DateAccepted { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date that the resource became or will become available.
        /// </summary>
        /// <value>The date (often a range) that the resource became or will become available.</value>
        /// <remarks>
        ///     When representing a date-time, it is recommended that the  <a href="http://www.ietf.org/rfc/rfc3339.txt">RFC #3339: Date and Time on the Internet (Timestamps)</a> format is used.
        ///     The value of this property can represent either a single date-time that indicates the period in time the resource became available, or it may represent a delimited date range
        ///     that indicates the start and end dates that the resource is or will be available.
        /// </remarks>
        public string DateAvailable
        {
            get
            {
                return this.extensionDateAvailable;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionDateAvailable = string.Empty;
                }
                else
                {
                    this.extensionDateAvailable = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the date of copyright of the resource.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the copyright date of the resource.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no copyright date was provided.
        /// </value>
        /// <remarks>
        ///     <para>The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).</para>
        /// </remarks>
        public DateTime DateCopyrighted { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date of creation of the resource.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates creation date of the resource.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime DateCreated { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date of issuance of the resource.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the formal issuance (e.g., publication) date of the resource.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no issuance date was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime DateIssued { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date on which the resource was changed.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the date on which the resource was changed.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date was provided.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime DateModified { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date of submission of the resource.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that indicates the submission date of the resource.
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no submission date was provided.
        /// </value>
        /// <remarks>
        ///     <para>The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).</para>
        ///     <para>
        ///         Examples of resources to which a <see cref="DateSubmitted">Date Submitted</see> may be relevant are a thesis (accepted by a university department) or an article (accepted by a journal).
        ///     </para>
        /// </remarks>
        public DateTime DateSubmitted { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the date that the resource is valid for.
        /// </summary>
        /// <value>The date (often a range) of validity of a resource.</value>
        /// <remarks>
        ///     When representing a date-time, it is recommended that the  <a href="http://www.ietf.org/rfc/rfc3339.txt">RFC #3339: Date and Time on the Internet (Timestamps)</a> format is used.
        ///     The value of this property can represent either a single date-time that indicates the period in time the resource is valid, or it may represent a delimited date range
        ///     that indicates the start and end dates that the resource is or will be valid.
        /// </remarks>
        public string DateValid
        {
            get
            {
                return this.extensionDateValid;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionDateValid = string.Empty;
                }
                else
                {
                    this.extensionDateValid = value.Trim();
                }
            }
        }

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
        /// Gets or sets the size or duration of the resource.
        /// </summary>
        /// <value>The size or duration of the resource.</value>
        /// <remarks>
        ///     <para>Examples include a number of pages, a specification of length, width, and breadth, or a period in hours, minutes, and seconds.</para>
        ///     <para>
        ///         The <see cref="Extent"/> property represents a dimension or extent, or a time taken to play or execute.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/SizeOrDuration</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-SizeOrDuration">http://dublincore.org/documents/dcmi-terms/#classes-SizeOrDuration</a> for further information.
        ///     </para>
        /// </remarks>
        public string Extent
        {
            get
            {
                return this.extensionExtent;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionExtent = string.Empty;
                }
                else
                {
                    this.extensionExtent = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the file format, physical medium, or dimensions of the resource.
        /// </summary>
        /// <value>The file format, physical medium, or dimensions of the resource.</value>
        /// <remarks>
        ///     <para>
        ///         Examples of dimensions include size and duration. Recommended best practice is to use a controlled vocabulary such as
        ///         the list of <a href="http://www.iana.org/assignments/media-types/">Internet Media Types</a>.
        ///     </para>
        ///     <para>
        ///         The <see cref="Format"/> property represents a media type or extent.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/MediaTypeOrExtent</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-MediaTypeOrExtent">http://dublincore.org/documents/dcmi-terms/#classes-MediaTypeOrExtent</a> for further information.
        ///     </para>
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
        /// Gets or sets a related resource that is substantially the same as the pre-existing described resource, but in another format.
        /// </summary>
        /// <value>A related resource that is substantially the same as the pre-existing described resource, but in another format.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string HasFormat
        {
            get
            {
                return this.extensionHasFormat;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionHasFormat = string.Empty;
                }
                else
                {
                    this.extensionHasFormat = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that is included either physically or logically in the described resource.
        /// </summary>
        /// <value>A related resource that is included either physically or logically in the described resource.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string HasPart
        {
            get
            {
                return this.extensionHasPart;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionHasPart = string.Empty;
                }
                else
                {
                    this.extensionHasPart = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that is a version, edition, or adaptation of the described resource.
        /// </summary>
        /// <value>A related resource that is a version, edition, or adaptation of the described resource.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string HasVersion
        {
            get
            {
                return this.extensionHasVersion;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionHasVersion = string.Empty;
                }
                else
                {
                    this.extensionHasVersion = value.Trim();
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
        /// Gets or sets the process, used to engender knowledge, attitudes and skills, that the described resource is designed to support.
        /// </summary>
        /// <value>The process, used to engender knowledge, attitudes and skills, that the described resource is designed to support.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="InstructionalMethod"/> property represents a process that is used to engender knowledge, attitudes, and skills.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/MethodOfInstruction</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-MethodOfInstruction">http://dublincore.org/documents/dcmi-terms/#classes-MethodOfInstruction</a> for further information.
        ///     </para>
        /// </remarks>
        public string InstructionalMethod
        {
            get
            {
                return this.extensionInstructionalMethod;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionInstructionalMethod = string.Empty;
                }
                else
                {
                    this.extensionInstructionalMethod = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that is substantially the same as the described resource, but in another format.
        /// </summary>
        /// <value>A related resource that is substantially the same as the described resource, but in another format.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string IsFormatOf
        {
            get
            {
                return this.extensionIsFormatOf;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIsFormatOf = string.Empty;
                }
                else
                {
                    this.extensionIsFormatOf = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource in which the described resource is physically or logically included.
        /// </summary>
        /// <value>A related resource in which the described resource is physically or logically included.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string IsPartOf
        {
            get
            {
                return this.extensionIsPartOf;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIsPartOf = string.Empty;
                }
                else
                {
                    this.extensionIsPartOf = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that references, cites, or otherwise points to the described resource.
        /// </summary>
        /// <value>A related resource that references, cites, or otherwise points to the described resource.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string IsReferencedBy
        {
            get
            {
                return this.extensionIsReferencedBy;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIsReferencedBy = string.Empty;
                }
                else
                {
                    this.extensionIsReferencedBy = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that supplants, displaces, or supersedes the described resource.
        /// </summary>
        /// <value>A related resource that supplants, displaces, or supersedes the described resource.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string IsReplacedBy
        {
            get
            {
                return this.extensionIsReplacedBy;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIsReplacedBy = string.Empty;
                }
                else
                {
                    this.extensionIsReplacedBy = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that requires the described resource to support its function, delivery, or coherence.
        /// </summary>
        /// <value>A related resource that requires the described resource to support its function, delivery, or coherence.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string IsRequiredBy
        {
            get
            {
                return this.extensionIsRequiredBy;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIsRequiredBy = string.Empty;
                }
                else
                {
                    this.extensionIsRequiredBy = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource of which the described resource is a version, edition, or adaptation.
        /// </summary>
        /// <value>A related resource of which the described resource is a version, edition, or adaptation.</value>
        /// <remarks>
        ///     This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        /// </remarks>
        public string IsVersionOf
        {
            get
            {
                return this.extensionIsVersionOf;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionIsVersionOf = string.Empty;
                }
                else
                {
                    this.extensionIsVersionOf = value.Trim();
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
        /// Gets or sets the legal document giving official permission to do something with the resource.
        /// </summary>
        /// <value>The legal document giving official permission to do something with the resource.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="License"/> property represents a legal document giving official permission to do something with a resource.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/LicenseDocument</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-LicenseDocument">http://dublincore.org/documents/dcmi-terms/#classes-LicenseDocument</a> for further information.
        ///     </para>
        /// </remarks>
        public string License
        {
            get
            {
                return this.extensionLicense;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionLicense = string.Empty;
                }
                else
                {
                    this.extensionLicense = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity that mediates access to the resource and for whom the resource is intended or useful.
        /// </summary>
        /// <value>The entity that mediates access to the resource and for whom the resource is intended or useful.</value>
        /// <remarks>
        ///     <para>In an educational context, a mediator might be a parent, teacher, teaching assistant, or care-giver.</para>
        ///     <para>
        ///         The <see cref="Mediator"/> property represents a resource that acts or has the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Agent</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Agent">http://dublincore.org/documents/dcmi-terms/#classes-Agent</a> for further information.
        ///     </para>
        /// </remarks>
        public string Mediator
        {
            get
            {
                return this.extensionMediator;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionMediator = string.Empty;
                }
                else
                {
                    this.extensionMediator = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the material or physical carrier of the resource.
        /// </summary>
        /// <value>The material or physical carrier of the resource.</value>
        /// <remarks>
        ///     <para>Examples include paper, canvas, or DVD.</para>
        ///     <para>
        ///         The <see cref="Medium"/> property represents a physical material or carrier.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/PhysicalMedium</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-PhysicalMedium">http://dublincore.org/documents/dcmi-terms/#classes-PhysicalMedium</a> for further information.
        ///     </para>
        /// </remarks>
        public string Medium
        {
            get
            {
                return this.extensionMedium;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionMedium = string.Empty;
                }
                else
                {
                    this.extensionMedium = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the statement of any changes in ownership and custody of the resource since its creation that are significant for its authenticity, integrity, and interpretation.
        /// </summary>
        /// <value>The statement of any changes in ownership and custody of the resource since its creation that are significant for its authenticity, integrity, and interpretation.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="Provenance"/> property represents a statement of any changes in ownership and custody of a resource
        ///         since its creation that are significant for its authenticity, integrity, and interpretation.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/ProvenanceStatement</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-ProvenanceStatement">http://dublincore.org/documents/dcmi-terms/#classes-ProvenanceStatement</a> for further information.
        ///     </para>
        /// </remarks>
        public string Provenance
        {
            get
            {
                return this.extensionProvenance;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionProvenance = string.Empty;
                }
                else
                {
                    this.extensionProvenance = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity responsible for making the resource available.
        /// </summary>
        /// <value>The entity responsible for making the resource available.</value>
        /// <remarks>
        ///     <para>Examples of a <see cref="Publisher"/> include a person, an organization, or a service. Typically, the name of a <see cref="Publisher"/> should be used to indicate the entity.</para>
        ///     <para>
        ///         The <see cref="Publisher"/> property represents a resource that acts or has the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Agent</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Agent">http://dublincore.org/documents/dcmi-terms/#classes-Agent</a> for further information.
        ///     </para>
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
        /// Gets or sets a related resource that is referenced, cited, or otherwise pointed to by the described resource.
        /// </summary>
        /// <value>A related resource that is referenced, cited, or otherwise pointed to by the described resource.</value>
        /// <remarks>
        ///     <para>
        ///         This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        ///     </para>
        /// </remarks>
        public string References
        {
            get
            {
                return this.extensionReferences;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionReferences = string.Empty;
                }
                else
                {
                    this.extensionReferences = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource.
        /// </summary>
        /// <value>A related resource.</value>
        /// <remarks>
        ///     <para>Recommended best practice is to identify the related resource by means of a string conforming to a formal identification system.</para>
        ///     <para>
        ///         This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        ///     </para>
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
        /// Gets or sets a related resource that is supplanted, displaced, or superseded by the described resource.
        /// </summary>
        /// <value>A related resource that is supplanted, displaced, or superseded by the described resource.</value>
        /// <remarks>
        ///     <para>
        ///         This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        ///     </para>
        /// </remarks>
        public string Replaces
        {
            get
            {
                return this.extensionReplaces;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionReplaces = string.Empty;
                }
                else
                {
                    this.extensionReplaces = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource that is required by the described resource to support its function, delivery, or coherence.
        /// </summary>
        /// <value>A related resource that is required by the described resource to support its function, delivery, or coherence.</value>
        /// <remarks>
        ///     <para>
        ///         This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        ///     </para>
        /// </remarks>
        public string Requires
        {
            get
            {
                return this.extensionRequires;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionRequires = string.Empty;
                }
                else
                {
                    this.extensionRequires = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets information about rights held in and over the resource.
        /// </summary>
        /// <value>Information about rights held in and over the resource.</value>
        /// <remarks>
        ///     <para>Typically, rights information includes a statement about various property rights associated with the resource, including intellectual property rights.</para>
        ///     <para>
        ///         The <see cref="Rights"/> property represents a statement about the intellectual property rights (IPR) held in or over a resource,
        ///         a legal document giving official permission to do something with a resource, or a statement about access rights.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/RightsStatement</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-RightsStatement">http://dublincore.org/documents/dcmi-terms/#classes-RightsStatement</a> for further information.
        ///     </para>
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
        /// Gets or sets the person or organization owning or managing rights over the resource.
        /// </summary>
        /// <value>The person or organization owning or managing rights over the resource.</value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="RightsHolder"/> property represents a resource that acts or has the power to act.
        ///     </para>
        ///     <para>
        ///         The resource described by this term is an instance of <b>http://purl.org/dc/terms/Agent</b>.
        ///         See <a href="http://dublincore.org/documents/dcmi-terms/#classes-Agent">http://dublincore.org/documents/dcmi-terms/#classes-Agent</a> for further information.
        ///     </para>
        /// </remarks>
        public string RightsHolder
        {
            get
            {
                return this.extensionRightsHolder;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionRightsHolder = string.Empty;
                }
                else
                {
                    this.extensionRightsHolder = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets a related resource from which the described resource is derived.
        /// </summary>
        /// <value>A related resource from which the described resource is derived.</value>
        /// <remarks>
        ///     <para>
        ///         The described resource may be derived from the related resource in whole or in part.
        ///         Recommended best practice is to identify the related resource by means of a string conforming to a formal identification system.
        ///     </para>
        ///     <para>
        ///         This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        ///     </para>
        /// </remarks>
        public string Source
        {
            get
            {
                return this.extensionSource;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionSource = string.Empty;
                }
                else
                {
                    this.extensionSource = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spatial characteristics of the resource.
        /// </summary>
        /// <value>The spatial characteristics of the resource.</value>
        public string SpatialCoverage
        {
            get
            {
                return this.extensionSpatialCoverage;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionSpatialCoverage = string.Empty;
                }
                else
                {
                    this.extensionSpatialCoverage = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the topic of the resource.
        /// </summary>
        /// <value>The topic of the resource.</value>
        /// <remarks>
        ///     <para>
        ///         Typically, the subject will be represented using keywords, key phrases, or classification codes.
        ///         Recommended best practice is to use a controlled vocabulary.
        ///         To describe the spatial or temporal topic of the resource, use the <see cref="Coverage"/> property.
        ///     </para>
        ///     <para>
        ///         This term is intended to be used with non-literal values as defined in the <a href="http://dublincore.org/documents/abstract-model/">DCMI Abstract Model</a>.
        ///     </para>
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
        /// Gets or sets the list of sub-units of the resource.
        /// </summary>
        /// <value>The list of sub-units of the resource.</value>
        public string TableOfContents
        {
            get
            {
                return this.extensionTableOfContents;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionTableOfContents = string.Empty;
                }
                else
                {
                    this.extensionTableOfContents = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the temporal characteristics of the resource.
        /// </summary>
        /// <value>The temporal characteristics of the resource.</value>
        public string TemporalCoverage
        {
            get
            {
                return this.extensionTemporalCoverage;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionTemporalCoverage = string.Empty;
                }
                else
                {
                    this.extensionTemporalCoverage = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name given to the resource.
        /// </summary>
        /// <value>The name given to the resource.</value>
        /// <remarks>
        ///     In current practice, this term is used primarily with literal values; however, there are important uses with non-literal values as well.
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
        ///     <para>To describe the file format, physical medium, or dimensions of the resource, use the <see cref="Format"/> property.</para>
        /// </remarks>
        public DublinCoreTypeVocabularies TypeVocabulary { get; set; } = DublinCoreTypeVocabularies.None;

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            wasLoaded = this.LoadGroup1(source, manager);
            if (this.LoadGroup2(source, manager))
            {
                wasLoaded = true;
            }

            if (this.LoadGroup3(source, manager))
            {
                wasLoaded = true;
            }

            if (this.LoadGroup4(source, manager))
            {
                wasLoaded = true;
            }

            if (this.LoadGroup5(source, manager))
            {
                wasLoaded = true;
            }

            if (this.LoadGroup6(source, manager))
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
            this.WriteGroup1(writer, xmlNamespace);
            this.WriteGroup2(writer, xmlNamespace);
            this.WriteGroup3(writer, xmlNamespace);
            this.WriteGroup4(writer, xmlNamespace);
            this.WriteGroup5(writer, xmlNamespace);
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadGroup1(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator abstractNavigator = source.SelectSingleNode("dcterms:abstract", manager);
                XPathNavigator accessRightsNavigator = source.SelectSingleNode("dcterms:accessRights", manager);
                XPathNavigator accrualMethodNavigator = source.SelectSingleNode("dcterms:accrualMethod", manager);
                XPathNavigator accrualPeriodicityNavigator = source.SelectSingleNode("dcterms:accrualPeriodicity", manager);
                XPathNavigator accrualPolicyNavigator = source.SelectSingleNode("dcterms:accrualPolicy", manager);
                XPathNavigator alternativeNavigator = source.SelectSingleNode("dcterms:alternative", manager);
                XPathNavigator audienceNavigator = source.SelectSingleNode("dcterms:audience", manager);
                XPathNavigator availableNavigator = source.SelectSingleNode("dcterms:available", manager);
                XPathNavigator bibliographicCitationNavigator = source.SelectSingleNode("dcterms:bibliographicCitation", manager);
                XPathNavigator conformsToNavigator = source.SelectSingleNode("dcterms:conformsTo", manager);

                if (abstractNavigator != null && !string.IsNullOrEmpty(abstractNavigator.Value))
                {
                    this.Abstract = abstractNavigator.Value;
                    wasLoaded = true;
                }

                if (accessRightsNavigator != null && !string.IsNullOrEmpty(accessRightsNavigator.Value))
                {
                    this.AccessRights = accessRightsNavigator.Value;
                    wasLoaded = true;
                }

                if (accrualMethodNavigator != null && !string.IsNullOrEmpty(accrualMethodNavigator.Value))
                {
                    this.AccrualMethod = accrualMethodNavigator.Value;
                    wasLoaded = true;
                }

                if (accrualPeriodicityNavigator != null && !string.IsNullOrEmpty(accrualPeriodicityNavigator.Value))
                {
                    this.AccrualPeriodicity = accrualPeriodicityNavigator.Value;
                    wasLoaded = true;
                }

                if (accrualPolicyNavigator != null && !string.IsNullOrEmpty(accrualPolicyNavigator.Value))
                {
                    this.AccrualPolicy = accrualPolicyNavigator.Value;
                    wasLoaded = true;
                }

                if (alternativeNavigator != null && !string.IsNullOrEmpty(alternativeNavigator.Value))
                {
                    this.AlternativeTitle = alternativeNavigator.Value;
                    wasLoaded = true;
                }

                if (audienceNavigator != null && !string.IsNullOrEmpty(audienceNavigator.Value))
                {
                    this.Audience = audienceNavigator.Value;
                    wasLoaded = true;
                }

                if (availableNavigator != null && !string.IsNullOrEmpty(availableNavigator.Value))
                {
                    this.DateAvailable = availableNavigator.Value;
                    wasLoaded = true;
                }

                if (bibliographicCitationNavigator != null && !string.IsNullOrEmpty(bibliographicCitationNavigator.Value))
                {
                    this.BibliographicCitation = bibliographicCitationNavigator.Value;
                    wasLoaded = true;
                }

                if (conformsToNavigator != null && !string.IsNullOrEmpty(conformsToNavigator.Value))
                {
                    this.ConformsTo = conformsToNavigator.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadGroup2(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator contributorNavigator = source.SelectSingleNode("dcterms:contributor", manager);
                XPathNavigator coverageNavigator = source.SelectSingleNode("dcterms:coverage", manager);
                XPathNavigator createdNavigator = source.SelectSingleNode("dcterms:created", manager);
                XPathNavigator creatorNavigator = source.SelectSingleNode("dcterms:creator", manager);
                XPathNavigator dateNavigator = source.SelectSingleNode("dcterms:date", manager);
                XPathNavigator dateAcceptedNavigator = source.SelectSingleNode("dcterms:dateAccepted", manager);
                XPathNavigator dateCopyrightedNavigator = source.SelectSingleNode("dcterms:dateCopyrighted", manager);
                XPathNavigator dateSubmittedNavigator = source.SelectSingleNode("dcterms:dateSubmitted", manager);
                XPathNavigator descriptionNavigator = source.SelectSingleNode("dcterms:description", manager);
                XPathNavigator educationLevelNavigator = source.SelectSingleNode("dcterms:educationLevel", manager);

                if (contributorNavigator != null && !string.IsNullOrEmpty(contributorNavigator.Value))
                {
                    this.Contributor = contributorNavigator.Value;
                    wasLoaded = true;
                }

                if (coverageNavigator != null && !string.IsNullOrEmpty(coverageNavigator.Value))
                {
                    this.Coverage = coverageNavigator.Value;
                    wasLoaded = true;
                }

                if (createdNavigator != null)
                {
                    DateTime createdOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(createdNavigator.Value, out createdOn))
                    {
                        this.DateCreated = createdOn;
                        wasLoaded = true;
                    }
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

                if (dateAcceptedNavigator != null)
                {
                    DateTime dateAccepted;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateAcceptedNavigator.Value, out dateAccepted))
                    {
                        this.DateAccepted = dateAccepted;
                        wasLoaded = true;
                    }
                }

                if (dateCopyrightedNavigator != null)
                {
                    DateTime dateCopyrighted;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCopyrightedNavigator.Value, out dateCopyrighted))
                    {
                        this.DateCopyrighted = dateCopyrighted;
                        wasLoaded = true;
                    }
                }

                if (dateSubmittedNavigator != null)
                {
                    DateTime dateSubmitted;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateSubmittedNavigator.Value, out dateSubmitted))
                    {
                        this.DateSubmitted = dateSubmitted;
                        wasLoaded = true;
                    }
                }

                if (descriptionNavigator != null && !string.IsNullOrEmpty(descriptionNavigator.Value))
                {
                    this.Description = descriptionNavigator.Value;
                    wasLoaded = true;
                }

                if (educationLevelNavigator != null && !string.IsNullOrEmpty(educationLevelNavigator.Value))
                {
                    this.AudienceEducationLevel = educationLevelNavigator.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadGroup3(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator extentNavigator = source.SelectSingleNode("dcterms:extent", manager);
                XPathNavigator formatNavigator = source.SelectSingleNode("dcterms:format", manager);
                XPathNavigator hasFormatNavigator = source.SelectSingleNode("dcterms:hasFormat", manager);
                XPathNavigator hasPartNavigator = source.SelectSingleNode("dcterms:hasPart", manager);
                XPathNavigator hasVersionNavigator = source.SelectSingleNode("dcterms:hasVersion", manager);
                XPathNavigator identifierNavigator = source.SelectSingleNode("dcterms:identifier", manager);
                XPathNavigator instructionalMethodNavigator = source.SelectSingleNode("dcterms:instructionalMethod", manager);
                XPathNavigator isFormatOfNavigator = source.SelectSingleNode("dcterms:isFormatOf", manager);
                XPathNavigator isPartOfNavigator = source.SelectSingleNode("dcterms:isPartOf", manager);
                XPathNavigator isReferencedByNavigator = source.SelectSingleNode("dcterms:isReferencedBy", manager);

                if (extentNavigator != null && !string.IsNullOrEmpty(extentNavigator.Value))
                {
                    this.Extent = extentNavigator.Value;
                    wasLoaded = true;
                }

                if (formatNavigator != null && !string.IsNullOrEmpty(formatNavigator.Value))
                {
                    this.Format = formatNavigator.Value;
                    wasLoaded = true;
                }

                if (hasFormatNavigator != null && !string.IsNullOrEmpty(hasFormatNavigator.Value))
                {
                    this.HasFormat = hasFormatNavigator.Value;
                    wasLoaded = true;
                }

                if (hasPartNavigator != null && !string.IsNullOrEmpty(hasPartNavigator.Value))
                {
                    this.HasPart = hasPartNavigator.Value;
                    wasLoaded = true;
                }

                if (hasVersionNavigator != null && !string.IsNullOrEmpty(hasVersionNavigator.Value))
                {
                    this.HasVersion = hasVersionNavigator.Value;
                    wasLoaded = true;
                }

                if (identifierNavigator != null && !string.IsNullOrEmpty(identifierNavigator.Value))
                {
                    this.Identifier = identifierNavigator.Value;
                    wasLoaded = true;
                }

                if (instructionalMethodNavigator != null && !string.IsNullOrEmpty(instructionalMethodNavigator.Value))
                {
                    this.InstructionalMethod = instructionalMethodNavigator.Value;
                    wasLoaded = true;
                }

                if (isFormatOfNavigator != null && !string.IsNullOrEmpty(isFormatOfNavigator.Value))
                {
                    this.IsFormatOf = isFormatOfNavigator.Value;
                    wasLoaded = true;
                }

                if (isPartOfNavigator != null && !string.IsNullOrEmpty(isPartOfNavigator.Value))
                {
                    this.IsPartOf = isPartOfNavigator.Value;
                    wasLoaded = true;
                }

                if (isReferencedByNavigator != null && !string.IsNullOrEmpty(isReferencedByNavigator.Value))
                {
                    this.IsReferencedBy = isReferencedByNavigator.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadGroup4(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator isReplacedByNavigator = source.SelectSingleNode("dcterms:isReplacedBy", manager);
                XPathNavigator isRequiredByNavigator = source.SelectSingleNode("dcterms:isRequiredBy", manager);
                XPathNavigator issuedNavigator = source.SelectSingleNode("dcterms:issued", manager);
                XPathNavigator isVersionOfNavigator = source.SelectSingleNode("dcterms:isVersionOf", manager);
                XPathNavigator languageNavigator = source.SelectSingleNode("dcterms:language", manager);
                XPathNavigator licenseNavigator = source.SelectSingleNode("dcterms:license", manager);
                XPathNavigator mediatorNavigator = source.SelectSingleNode("dcterms:mediator", manager);
                XPathNavigator mediumNavigator = source.SelectSingleNode("dcterms:medium", manager);
                XPathNavigator modifiedNavigator = source.SelectSingleNode("dcterms:modified", manager);
                XPathNavigator provenanceNavigator = source.SelectSingleNode("dcterms:provenance", manager);

                if (isReplacedByNavigator != null && !string.IsNullOrEmpty(isReplacedByNavigator.Value))
                {
                    this.IsReplacedBy = isReplacedByNavigator.Value;
                    wasLoaded = true;
                }

                if (isRequiredByNavigator != null && !string.IsNullOrEmpty(isRequiredByNavigator.Value))
                {
                    this.IsRequiredBy = isRequiredByNavigator.Value;
                    wasLoaded = true;
                }

                if (issuedNavigator != null)
                {
                    DateTime issuedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(issuedNavigator.Value, out issuedOn))
                    {
                        this.DateIssued = issuedOn;
                        wasLoaded = true;
                    }
                }

                if (isVersionOfNavigator != null && !string.IsNullOrEmpty(isVersionOfNavigator.Value))
                {
                    this.IsVersionOf = isVersionOfNavigator.Value;
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
                        System.Diagnostics.Trace.TraceWarning("DublinCoreMetadataTermsSyndicationExtensionContext unable to determine CultureInfo with a name of {0}.", languageNavigator.Value);
                    }
                }

                if (licenseNavigator != null && !string.IsNullOrEmpty(licenseNavigator.Value))
                {
                    this.License = licenseNavigator.Value;
                    wasLoaded = true;
                }

                if (mediatorNavigator != null && !string.IsNullOrEmpty(mediatorNavigator.Value))
                {
                    this.Mediator = mediatorNavigator.Value;
                    wasLoaded = true;
                }

                if (mediumNavigator != null && !string.IsNullOrEmpty(mediumNavigator.Value))
                {
                    this.Medium = mediumNavigator.Value;
                    wasLoaded = true;
                }

                if (modifiedNavigator != null)
                {
                    DateTime modifiedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out modifiedOn))
                    {
                        this.DateModified = modifiedOn;
                        wasLoaded = true;
                    }
                }

                if (provenanceNavigator != null && !string.IsNullOrEmpty(provenanceNavigator.Value))
                {
                    this.Provenance = provenanceNavigator.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadGroup5(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator publisherNavigator = source.SelectSingleNode("dcterms:publisher", manager);
                XPathNavigator referencesNavigator = source.SelectSingleNode("dcterms:references", manager);
                XPathNavigator relationNavigator = source.SelectSingleNode("dcterms:relation", manager);
                XPathNavigator replacesNavigator = source.SelectSingleNode("dcterms:replaces", manager);
                XPathNavigator requiresNavigator = source.SelectSingleNode("dcterms:requires", manager);
                XPathNavigator rightsNavigator = source.SelectSingleNode("dcterms:rights", manager);
                XPathNavigator rightsHolderNavigator = source.SelectSingleNode("dcterms:rightsHolder", manager);
                XPathNavigator sourceNavigator = source.SelectSingleNode("dcterms:source", manager);
                XPathNavigator spatialNavigator = source.SelectSingleNode("dcterms:spatial", manager);
                XPathNavigator subjectNavigator = source.SelectSingleNode("dcterms:subject", manager);

                if (publisherNavigator != null && !string.IsNullOrEmpty(publisherNavigator.Value))
                {
                    this.Publisher = publisherNavigator.Value;
                    wasLoaded = true;
                }

                if (referencesNavigator != null && !string.IsNullOrEmpty(referencesNavigator.Value))
                {
                    this.References = referencesNavigator.Value;
                    wasLoaded = true;
                }

                if (relationNavigator != null && !string.IsNullOrEmpty(relationNavigator.Value))
                {
                    this.Relation = relationNavigator.Value;
                    wasLoaded = true;
                }

                if (replacesNavigator != null && !string.IsNullOrEmpty(replacesNavigator.Value))
                {
                    this.Replaces = replacesNavigator.Value;
                    wasLoaded = true;
                }

                if (requiresNavigator != null && !string.IsNullOrEmpty(requiresNavigator.Value))
                {
                    this.Requires = requiresNavigator.Value;
                    wasLoaded = true;
                }

                if (rightsNavigator != null && !string.IsNullOrEmpty(rightsNavigator.Value))
                {
                    this.Rights = rightsNavigator.Value;
                    wasLoaded = true;
                }

                if (rightsHolderNavigator != null && !string.IsNullOrEmpty(rightsHolderNavigator.Value))
                {
                    this.RightsHolder = rightsHolderNavigator.Value;
                    wasLoaded = true;
                }

                if (sourceNavigator != null && !string.IsNullOrEmpty(sourceNavigator.Value))
                {
                    this.Source = sourceNavigator.Value;
                    wasLoaded = true;
                }

                if (spatialNavigator != null && !string.IsNullOrEmpty(spatialNavigator.Value))
                {
                    this.SpatialCoverage = spatialNavigator.Value;
                    wasLoaded = true;
                }

                if (subjectNavigator != null && !string.IsNullOrEmpty(subjectNavigator.Value))
                {
                    this.Subject = subjectNavigator.Value;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadGroup6(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator tableOfContentsNavigator = source.SelectSingleNode("dcterms:tableOfContents", manager);
                XPathNavigator temporalNavigator = source.SelectSingleNode("dcterms:temporal", manager);
                XPathNavigator titleNavigator = source.SelectSingleNode("dcterms:title", manager);
                XPathNavigator typeNavigator = source.SelectSingleNode("dcterms:type", manager);
                XPathNavigator validNavigator = source.SelectSingleNode("dcterms:valid", manager);

                if (tableOfContentsNavigator != null && !string.IsNullOrEmpty(tableOfContentsNavigator.Value))
                {
                    this.TableOfContents = tableOfContentsNavigator.Value;
                    wasLoaded = true;
                }

                if (temporalNavigator != null && !string.IsNullOrEmpty(temporalNavigator.Value))
                {
                    this.TemporalCoverage = temporalNavigator.Value;
                    wasLoaded = true;
                }

                if (titleNavigator != null && !string.IsNullOrEmpty(titleNavigator.Value))
                {
                    this.Title = titleNavigator.Value;
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

                if (validNavigator != null && !string.IsNullOrEmpty(validNavigator.Value))
                {
                    this.DateValid = validNavigator.Value;
                    wasLoaded = true;
                }
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
        private void WriteGroup1(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (!string.IsNullOrEmpty(this.Abstract))
            {
                writer.WriteElementString("abstract", xmlNamespace, this.Abstract);
            }

            if (!string.IsNullOrEmpty(this.AccessRights))
            {
                writer.WriteElementString("accessRights", xmlNamespace, this.AccessRights);
            }

            if (!string.IsNullOrEmpty(this.AccrualMethod))
            {
                writer.WriteElementString("accrualMethod", xmlNamespace, this.AccrualMethod);
            }

            if (!string.IsNullOrEmpty(this.AccrualPeriodicity))
            {
                writer.WriteElementString("accrualPeriodicity", xmlNamespace, this.AccrualPeriodicity);
            }

            if (!string.IsNullOrEmpty(this.AccrualPolicy))
            {
                writer.WriteElementString("accrualPolicy", xmlNamespace, this.AccrualPolicy);
            }

            if (!string.IsNullOrEmpty(this.AlternativeTitle))
            {
                writer.WriteElementString("alternative", xmlNamespace, this.AlternativeTitle);
            }

            if (!string.IsNullOrEmpty(this.Audience))
            {
                writer.WriteElementString("audience", xmlNamespace, this.Audience);
            }

            if (!string.IsNullOrEmpty(this.DateAvailable))
            {
                writer.WriteElementString("available", xmlNamespace, this.DateAvailable);
            }

            if (!string.IsNullOrEmpty(this.BibliographicCitation))
            {
                writer.WriteElementString("bibliographicCitation", xmlNamespace, this.BibliographicCitation);
            }

            if (!string.IsNullOrEmpty(this.ConformsTo))
            {
                writer.WriteElementString("conformsTo", xmlNamespace, this.ConformsTo);
            }
        }

        /// <summary>
        /// Writes the current context to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        private void WriteGroup2(XmlWriter writer, string xmlNamespace)
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

            if (this.DateCreated != DateTime.MinValue)
            {
                writer.WriteElementString("created", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateCreated));
            }

            if (!string.IsNullOrEmpty(this.Creator))
            {
                writer.WriteElementString("creator", xmlNamespace, this.Creator);
            }

            if (this.Date != DateTime.MinValue)
            {
                writer.WriteElementString("date", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.Date));
            }

            if (this.DateAccepted != DateTime.MinValue)
            {
                writer.WriteElementString("dateAccepted", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateAccepted));
            }

            if (this.DateCopyrighted != DateTime.MinValue)
            {
                writer.WriteElementString("dateCopyrighted", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateCopyrighted));
            }

            if (this.DateSubmitted != DateTime.MinValue)
            {
                writer.WriteElementString("dateSubmitted", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateSubmitted));
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                writer.WriteElementString("description", xmlNamespace, this.Description);
            }

            if (!string.IsNullOrEmpty(this.AudienceEducationLevel))
            {
                writer.WriteElementString("educationLevel", xmlNamespace, this.AudienceEducationLevel);
            }
        }

        /// <summary>
        /// Writes the current context to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        private void WriteGroup3(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (!string.IsNullOrEmpty(this.Extent))
            {
                writer.WriteElementString("extent", xmlNamespace, this.Extent);
            }

            if (!string.IsNullOrEmpty(this.Format))
            {
                writer.WriteElementString("format", xmlNamespace, this.Format);
            }

            if (!string.IsNullOrEmpty(this.HasFormat))
            {
                writer.WriteElementString("hasFormat", xmlNamespace, this.HasFormat);
            }

            if (!string.IsNullOrEmpty(this.HasPart))
            {
                writer.WriteElementString("hasPart", xmlNamespace, this.HasPart);
            }

            if (!string.IsNullOrEmpty(this.HasVersion))
            {
                writer.WriteElementString("hasVersion", xmlNamespace, this.HasVersion);
            }

            if (!string.IsNullOrEmpty(this.Identifier))
            {
                writer.WriteElementString("identifier", xmlNamespace, this.Identifier);
            }

            if (!string.IsNullOrEmpty(this.InstructionalMethod))
            {
                writer.WriteElementString("instructionalMethod", xmlNamespace, this.InstructionalMethod);
            }

            if (!string.IsNullOrEmpty(this.IsFormatOf))
            {
                writer.WriteElementString("isFormatOf", xmlNamespace, this.IsFormatOf);
            }

            if (!string.IsNullOrEmpty(this.IsPartOf))
            {
                writer.WriteElementString("isPartOf", xmlNamespace, this.IsPartOf);
            }

            if (!string.IsNullOrEmpty(this.IsReferencedBy))
            {
                writer.WriteElementString("isReferencedBy", xmlNamespace, this.IsReferencedBy);
            }
        }

        /// <summary>
        /// Writes the current context to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        private void WriteGroup4(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (!string.IsNullOrEmpty(this.IsReplacedBy))
            {
                writer.WriteElementString("isReplacedBy", xmlNamespace, this.IsReplacedBy);
            }

            if (!string.IsNullOrEmpty(this.IsRequiredBy))
            {
                writer.WriteElementString("isRequiredBy", xmlNamespace, this.IsRequiredBy);
            }

            if (this.DateIssued != DateTime.MinValue)
            {
                writer.WriteElementString("issued", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateIssued));
            }

            if (!string.IsNullOrEmpty(this.IsVersionOf))
            {
                writer.WriteElementString("isVersionOf", xmlNamespace, this.IsVersionOf);
            }

            if (this.Language != null)
            {
                writer.WriteElementString("language", xmlNamespace, this.Language.Name);
            }

            if (!string.IsNullOrEmpty(this.License))
            {
                writer.WriteElementString("license", xmlNamespace, this.License);
            }

            if (!string.IsNullOrEmpty(this.Mediator))
            {
                writer.WriteElementString("mediator", xmlNamespace, this.Mediator);
            }

            if (!string.IsNullOrEmpty(this.Medium))
            {
                writer.WriteElementString("medium", xmlNamespace, this.Medium);
            }

            if (this.DateModified != DateTime.MinValue)
            {
                writer.WriteElementString("modified", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateModified));
            }

            if (!string.IsNullOrEmpty(this.Provenance))
            {
                writer.WriteElementString("provenance", xmlNamespace, this.Provenance);
            }
        }

        /// <summary>
        /// Writes the current context to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
        /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        private void WriteGroup5(XmlWriter writer, string xmlNamespace)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (!string.IsNullOrEmpty(this.Publisher))
            {
                writer.WriteElementString("publisher", xmlNamespace, this.Publisher);
            }

            if (!string.IsNullOrEmpty(this.References))
            {
                writer.WriteElementString("references", xmlNamespace, this.References);
            }

            if (!string.IsNullOrEmpty(this.Relation))
            {
                writer.WriteElementString("relation", xmlNamespace, this.Relation);
            }

            if (!string.IsNullOrEmpty(this.Replaces))
            {
                writer.WriteElementString("replaces", xmlNamespace, this.Replaces);
            }

            if (!string.IsNullOrEmpty(this.Requires))
            {
                writer.WriteElementString("requires", xmlNamespace, this.Requires);
            }

            if (!string.IsNullOrEmpty(this.Rights))
            {
                writer.WriteElementString("rights", xmlNamespace, this.Rights);
            }

            if (!string.IsNullOrEmpty(this.RightsHolder))
            {
                writer.WriteElementString("rightsHolder", xmlNamespace, this.RightsHolder);
            }

            if (!string.IsNullOrEmpty(this.Source))
            {
                writer.WriteElementString("source", xmlNamespace, this.Source);
            }

            if (!string.IsNullOrEmpty(this.SpatialCoverage))
            {
                writer.WriteElementString("spatial", xmlNamespace, this.SpatialCoverage);
            }

            if (!string.IsNullOrEmpty(this.Subject))
            {
                writer.WriteElementString("subject", xmlNamespace, this.Subject);
            }

            if (!string.IsNullOrEmpty(this.TableOfContents))
            {
                writer.WriteElementString("tableOfContents", xmlNamespace, this.TableOfContents);
            }

            if (!string.IsNullOrEmpty(this.TemporalCoverage))
            {
                writer.WriteElementString("temporal", xmlNamespace, this.TemporalCoverage);
            }

            if (!string.IsNullOrEmpty(this.Title))
            {
                writer.WriteElementString("title", xmlNamespace, this.Title);
            }

            if (this.TypeVocabulary != DublinCoreTypeVocabularies.None)
            {
                writer.WriteElementString("type", xmlNamespace, DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(this.TypeVocabulary));
            }

            if (!string.IsNullOrEmpty(this.DateValid))
            {
                writer.WriteElementString("valid", xmlNamespace, this.DateValid);
            }
        }
    }
}