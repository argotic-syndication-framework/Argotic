/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created DublinCoreMetadataTermsSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class DublinCoreMetadataTermsSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the summary of the resource.
        /// </summary>
        private string extensionAbstract                    = String.Empty;
        /// <summary>
        /// Private member to hold information about who can access the resource or an indication of its security status.
        /// </summary>
        private string extensionAccessRights                = String.Empty;
        /// <summary>
        /// Private member to hold the method by which items are added to a collection.
        /// </summary>
        private string extensionAccrualMethod               = String.Empty;
        /// <summary>
        /// Private member to hold the frequency with which items are added to a collection.
        /// </summary>
        private string extensionAccrualPeriodicity          = String.Empty;
        /// <summary>
        /// Private member to hold the policy governing the addition of items to a collection.
        /// </summary>
        private string extensionAccrualPolicy               = String.Empty;
        /// <summary>
        /// Private member to hold an alternative name for the resource.
        /// </summary>
        private string extensionAlternativeTitle            = String.Empty;
        /// <summary>
        /// Private member to hold the class of entity for whom the resource is intended or useful.
        /// </summary>
        private string extensionAudience                    = String.Empty;
        /// <summary>
        /// Private member to hold the date (often a range) that the resource became or will become available.
        /// </summary>
        private string extensionDateAvailable               = String.Empty;
        /// <summary>
        /// Private member to hold the bibliographic reference for the resource.
        /// </summary>
        private string extensionBibliographicCitation       = String.Empty;
        /// <summary>
        /// Private member to hold the established standard to which the described resource conforms.
        /// </summary>
        private string extensionConformsTo                  = String.Empty;
        /// <summary>
        /// Private member to hold the entity responsible for making contributions to the resource.
        /// </summary>
        private string extensionContributor                 = String.Empty;
        /// <summary>
        /// Private member to hold the spatial or temporal topic of the resource, the spatial applicability of the resource, or the jurisdiction under which the resource is relevant.
        /// </summary>
        private string extensionCoverage                    = String.Empty;
        /// <summary>
        /// Private member to hold the date of creation of the resource.
        /// </summary>
        private DateTime extensionDateCreated               = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the entity primarily responsible for making the resource.
        /// </summary>
        private string extensionCreator                     = String.Empty;
        /// <summary>
        /// Private member to hold a point or period of time associated with an event in the lifecycle of the resource.
        /// </summary>
        private DateTime extensionDate                      = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the date of acceptance of the resource.
        /// </summary>
        private DateTime extensionDateAccepted              = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the date of copyright of the resource.
        /// </summary>
        private DateTime extensionDateCopyrighted           = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the date of submission of the resource.
        /// </summary>
        private DateTime extensionDateSubmitted             = DateTime.MinValue;
        /// <summary>
        /// Private member to hold an account of the resource.
        /// </summary>
        private string extensionDescription                 = String.Empty;
        /// <summary>
        /// Private member to hold the class of entity, defined in terms of progression through an educational or training context, for which the described resource is intended.
        /// </summary>
        private string extensionAudienceEducationLevel      = String.Empty;
        /// <summary>
        /// Private member to hold the size or duration of the resource.
        /// </summary>
        private string extensionExtent                      = String.Empty;
        /// <summary>
        /// Private member to hold the file format, physical medium, or dimensions of the resource.
        /// </summary>
        private string extensionFormat                      = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is substantially the same as the pre-existing described resource, but in another format.
        /// </summary>
        private string extensionHasFormat                   = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is included either physically or logically in the described resource.
        /// </summary>
        private string extensionHasPart                     = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is a version, edition, or adaptation of the described resource.
        /// </summary>
        private string extensionHasVersion                  = String.Empty;
        /// <summary>
        /// Private member to hold an unambiguous reference to the resource within a given context.
        /// </summary>
        private string extensionIdentifier                  = String.Empty;
        /// <summary>
        /// Private member to hold the process, used to engender knowledge, attitudes and skills, that the described resource is designed to support.
        /// </summary>
        private string extensionInstructionalMethod         = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is substantially the same as the described resource, but in another format.
        /// </summary>
        private string extensionIsFormatOf                  = String.Empty;
        /// <summary>
        /// Private member to hold a related resource in which the described resource is physically or logically included.
        /// </summary>
        private string extensionIsPartOf                    = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that references, cites, or otherwise points to the described resource.
        /// </summary>
        private string extensionIsReferencedBy              = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that supplants, displaces, or supersedes the described resource.
        /// </summary>
        private string extensionIsReplacedBy                = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that requires the described resource to support its function, delivery, or coherence.
        /// </summary>
        private string extensionIsRequiredBy                = String.Empty;
        /// <summary>
        /// Private member to hold the date of formal issuance (e.g., publication) of the resource.
        /// </summary>
        private DateTime extensionDateIssued                = DateTime.MinValue;
        /// <summary>
        /// Private member to hold a related resource of which the described resource is a version, edition, or adaptation.
        /// </summary>
        private string extensionIsVersionOf                 = String.Empty;
        /// <summary>
        /// Private member to hold the language of the resource.
        /// </summary>
        private CultureInfo extensionLanguage;
        /// <summary>
        /// Private member to hold the legal document giving official permission to do something with the resource.
        /// </summary>
        private string extensionLicense                     = String.Empty;
        /// <summary>
        /// Private member to hold the entity that mediates access to the resource and for whom the resource is intended or useful.
        /// </summary>
        private string extensionMediator                    = String.Empty;
        /// <summary>
        /// Private member to hold the material or physical carrier of the resource.
        /// </summary>
        private string extensionMedium                      = String.Empty;
        /// <summary>
        /// Private member to hold the date on which the resource was changed.
        /// </summary>
        private DateTime extensionDateModified              = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the statement of any changes in ownership and custody of the resource since its creation that are significant for its authenticity, integrity, and interpretation.
        /// </summary>
        private string extensionProvenance                  = String.Empty;
        /// <summary>
        /// Private member to hold the entity responsible for making the resource available.
        /// </summary>
        private string extensionPublisher                   = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is referenced, cited, or otherwise pointed to by the described resource.
        /// </summary>
        private string extensionReferences                  = String.Empty;
        /// <summary>
        /// Private member to hold a related resource.
        /// </summary>
        private string extensionRelation                    = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is supplanted, displaced, or superseded by the described resource.
        /// </summary>
        private string extensionReplaces                    = String.Empty;
        /// <summary>
        /// Private member to hold a related resource that is required by the described resource to support its function, delivery, or coherence.
        /// </summary>
        private string extensionRequires                    = String.Empty;
        /// <summary>
        /// Private member to hold information about rights held in and over the resource.
        /// </summary>
        private string extensionRights                      = String.Empty;
        /// <summary>
        /// Private member to hold the person or organization owning or managing rights over the resource.
        /// </summary>
        private string extensionRightsHolder                = String.Empty;
        /// <summary>
        /// Private member to hold a related resource from which the described resource is derived.
        /// </summary>
        private string extensionSource                      = String.Empty;
        /// <summary>
        /// Private member to hold the spatial characteristics of the resource.
        /// </summary>
        private string extensionSpatialCoverage             = String.Empty;
        /// <summary>
        /// Private member to hold the topic of the resource.
        /// </summary>
        private string extensionSubject                     = String.Empty;
        /// <summary>
        /// Private member to hold the list of sub-units of the resource.
        /// </summary>
        private string extensionTableOfContents             = String.Empty;
        /// <summary>
        /// Private member to hold the temporal characteristics of the resource.
        /// </summary>
        private string extensionTemporalCoverage            = String.Empty;
        /// <summary>
        /// Private member to hold the name given to the resource.
        /// </summary>
        private string extensionTitle                       = String.Empty;
        /// <summary>
        /// Private member to hold the nature or genre of the resource.
        /// </summary>
        private DublinCoreTypeVocabularies extensionType    = DublinCoreTypeVocabularies.None;
        /// <summary>
        /// Private member to hold the date (often a range) of validity of the resource.
        /// </summary>
        private string extensionDateValid                   = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region DublinCoreMetadataTermsSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> class.
        /// </summary>
        public DublinCoreMetadataTermsSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Abstract
        /// <summary>
        /// Gets or sets the summary of the resource.
        /// </summary>
        /// <value>The summary of the resource.</value>
        public string Abstract
        {
            get
            {
                return extensionAbstract;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAbstract = String.Empty;
                }
                else
                {
                    extensionAbstract = value.Trim();
                }
            }
        }
        #endregion

        #region AccessRights
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
                return extensionAccessRights;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAccessRights = String.Empty;
                }
                else
                {
                    extensionAccessRights = value.Trim();
                }
            }
        }
        #endregion

        #region AccrualMethod
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
                return extensionAccrualMethod;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAccrualMethod = String.Empty;
                }
                else
                {
                    extensionAccrualMethod = value.Trim();
                }
            }
        }
        #endregion

        #region AccrualPeriodicity
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
                return extensionAccrualPeriodicity;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAccrualPeriodicity = String.Empty;
                }
                else
                {
                    extensionAccrualPeriodicity = value.Trim();
                }
            }
        }
        #endregion

        #region AccrualPolicy
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
                return extensionAccrualPolicy;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAccrualPolicy = String.Empty;
                }
                else
                {
                    extensionAccrualPolicy = value.Trim();
                }
            }
        }
        #endregion

        #region AlternativeTitle
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
                return extensionAlternativeTitle;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAlternativeTitle = String.Empty;
                }
                else
                {
                    extensionAlternativeTitle = value.Trim();
                }
            }
        }
        #endregion

        #region Audience
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
                return extensionAudience;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAudience = String.Empty;
                }
                else
                {
                    extensionAudience = value.Trim();
                }
            }
        }
        #endregion

        #region AudienceEducationLevel
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
                return extensionAudienceEducationLevel;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionAudienceEducationLevel = String.Empty;
                }
                else
                {
                    extensionAudienceEducationLevel = value.Trim();
                }
            }
        }
        #endregion

        #region BibliographicCitation
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
                return extensionBibliographicCitation;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionBibliographicCitation = String.Empty;
                }
                else
                {
                    extensionBibliographicCitation = value.Trim();
                }
            }
        }
        #endregion

        #region ConformsTo
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
                return extensionConformsTo;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionConformsTo = String.Empty;
                }
                else
                {
                    extensionConformsTo = value.Trim();
                }
            }
        }
        #endregion

        #region Contributor
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
                return extensionContributor;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionContributor = String.Empty;
                }
                else
                {
                    extensionContributor = value.Trim();
                }
            }
        }
        #endregion

        #region Coverage
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
                return extensionCoverage;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionCoverage = String.Empty;
                }
                else
                {
                    extensionCoverage = value.Trim();
                }
            }
        }
        #endregion

        #region Creator
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
                return extensionCreator;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionCreator = String.Empty;
                }
                else
                {
                    extensionCreator = value.Trim();
                }
            }
        }
        #endregion

        #region Date
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
        public DateTime Date
        {
            get
            {
                return extensionDate;
            }

            set
            {
                extensionDate = value;
            }
        }
        #endregion

        #region DateAccepted
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
        public DateTime DateAccepted
        {
            get
            {
                return extensionDateAccepted;
            }

            set
            {
                extensionDateAccepted = value;
            }
        }
        #endregion

        #region DateAvailable
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
                return extensionDateAvailable;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionDateAvailable = String.Empty;
                }
                else
                {
                    extensionDateAvailable = value.Trim();
                }
            }
        }
        #endregion

        #region DateCopyrighted
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
        public DateTime DateCopyrighted
        {
            get
            {
                return extensionDateCopyrighted;
            }

            set
            {
                extensionDateCopyrighted = value;
            }
        }
        #endregion

        #region DateCreated
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
        public DateTime DateCreated
        {
            get
            {
                return extensionDateCreated;
            }

            set
            {
                extensionDateCreated = value;
            }
        }
        #endregion

        #region DateIssued
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
        public DateTime DateIssued
        {
            get
            {
                return extensionDateIssued;
            }

            set
            {
                extensionDateIssued = value;
            }
        }
        #endregion

        #region DateModified
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
        public DateTime DateModified
        {
            get
            {
                return extensionDateModified;
            }

            set
            {
                extensionDateModified = value;
            }
        }
        #endregion

        #region DateSubmitted
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
        public DateTime DateSubmitted
        {
            get
            {
                return extensionDateSubmitted;
            }

            set
            {
                extensionDateSubmitted = value;
            }
        }
        #endregion

        #region DateValid
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
                return extensionDateValid;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionDateValid = String.Empty;
                }
                else
                {
                    extensionDateValid = value.Trim();
                }
            }
        }
        #endregion

        #region Description
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
                return extensionDescription;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionDescription = String.Empty;
                }
                else
                {
                    extensionDescription = value.Trim();
                }
            }
        }
        #endregion

        #region Extent
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
                return extensionExtent;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionExtent = String.Empty;
                }
                else
                {
                    extensionExtent = value.Trim();
                }
            }
        }
        #endregion

        #region Format
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
                return extensionFormat;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionFormat = String.Empty;
                }
                else
                {
                    extensionFormat = value.Trim();
                }
            }
        }
        #endregion

        #region HasFormat
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
                return extensionHasFormat;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionHasFormat = String.Empty;
                }
                else
                {
                    extensionHasFormat = value.Trim();
                }
            }
        }
        #endregion

        #region HasPart
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
                return extensionHasPart;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionHasPart = String.Empty;
                }
                else
                {
                    extensionHasPart = value.Trim();
                }
            }
        }
        #endregion

        #region HasVersion
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
                return extensionHasVersion;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionHasVersion = String.Empty;
                }
                else
                {
                    extensionHasVersion = value.Trim();
                }
            }
        }
        #endregion

        #region Identifier
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
                return extensionIdentifier;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIdentifier = String.Empty;
                }
                else
                {
                    extensionIdentifier = value.Trim();
                }
            }
        }
        #endregion

        #region InstructionalMethod
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
                return extensionInstructionalMethod;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionInstructionalMethod = String.Empty;
                }
                else
                {
                    extensionInstructionalMethod = value.Trim();
                }
            }
        }
        #endregion

        #region IsFormatOf
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
                return extensionIsFormatOf;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIsFormatOf = String.Empty;
                }
                else
                {
                    extensionIsFormatOf = value.Trim();
                }
            }
        }
        #endregion

        #region IsPartOf
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
                return extensionIsPartOf;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIsPartOf = String.Empty;
                }
                else
                {
                    extensionIsPartOf = value.Trim();
                }
            }
        }
        #endregion

        #region IsReferencedBy
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
                return extensionIsReferencedBy;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIsReferencedBy = String.Empty;
                }
                else
                {
                    extensionIsReferencedBy = value.Trim();
                }
            }
        }
        #endregion

        #region IsReplacedBy
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
                return extensionIsReplacedBy;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIsReplacedBy = String.Empty;
                }
                else
                {
                    extensionIsReplacedBy = value.Trim();
                }
            }
        }
        #endregion

        #region IsRequiredBy
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
                return extensionIsRequiredBy;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIsRequiredBy = String.Empty;
                }
                else
                {
                    extensionIsRequiredBy = value.Trim();
                }
            }
        }
        #endregion

        #region IsVersionOf
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
                return extensionIsVersionOf;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionIsVersionOf = String.Empty;
                }
                else
                {
                    extensionIsVersionOf = value.Trim();
                }
            }
        }
        #endregion

        #region Language
        /// <summary>
        /// Gets or sets the language of the resource.
        /// </summary>
        /// <value>A <see cref="CultureInfo"/> object that represents the language of the resource. The default value is a <b>null</b> reference, which indicates that no language was specified.</value>
        /// <remarks>
        ///     Recommended best practice is to use a controlled vocabulary such as <a href="http://www.ietf.org/rfc/rfc4646.txt">RFC 4646</a>. 
        ///     This framework conforms to this best practice by utilizing the <see cref="CultureInfo"/> class to represent the language of a resource.
        /// </remarks>
        public CultureInfo Language
        {
            get
            {
                return extensionLanguage;
            }

            set
            {
                extensionLanguage = value;
            }
        }
        #endregion

        #region License
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
                return extensionLicense;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionLicense = String.Empty;
                }
                else
                {
                    extensionLicense = value.Trim();
                }
            }
        }
        #endregion

        #region Mediator
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
                return extensionMediator;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionMediator = String.Empty;
                }
                else
                {
                    extensionMediator = value.Trim();
                }
            }
        }
        #endregion

        #region Medium
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
                return extensionMedium;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionMedium = String.Empty;
                }
                else
                {
                    extensionMedium = value.Trim();
                }
            }
        }
        #endregion

        #region Provenance
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
                return extensionProvenance;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionProvenance = String.Empty;
                }
                else
                {
                    extensionProvenance = value.Trim();
                }
            }
        }
        #endregion

        #region Publisher
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
                return extensionPublisher;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionPublisher = String.Empty;
                }
                else
                {
                    extensionPublisher = value.Trim();
                }
            }
        }
        #endregion

        #region References
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
                return extensionReferences;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionReferences = String.Empty;
                }
                else
                {
                    extensionReferences = value.Trim();
                }
            }
        }
        #endregion

        #region Relation
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
                return extensionRelation;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionRelation = String.Empty;
                }
                else
                {
                    extensionRelation = value.Trim();
                }
            }
        }
        #endregion

        #region Replaces
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
                return extensionReplaces;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionReplaces = String.Empty;
                }
                else
                {
                    extensionReplaces = value.Trim();
                }
            }
        }
        #endregion

        #region Requires
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
                return extensionRequires;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionRequires = String.Empty;
                }
                else
                {
                    extensionRequires = value.Trim();
                }
            }
        }
        #endregion

        #region Rights
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
                return extensionRights;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionRights = String.Empty;
                }
                else
                {
                    extensionRights = value.Trim();
                }
            }
        }
        #endregion

        #region RightsHolder
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
                return extensionRightsHolder;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionRightsHolder = String.Empty;
                }
                else
                {
                    extensionRightsHolder = value.Trim();
                }
            }
        }
        #endregion

        #region Source
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
                return extensionSource;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionSource = String.Empty;
                }
                else
                {
                    extensionSource = value.Trim();
                }
            }
        }
        #endregion

        #region SpatialCoverage
        /// <summary>
        /// Gets or sets the spatial characteristics of the resource.
        /// </summary>
        /// <value>The spatial characteristics of the resource.</value>
        public string SpatialCoverage
        {
            get
            {
                return extensionSpatialCoverage;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionSpatialCoverage = String.Empty;
                }
                else
                {
                    extensionSpatialCoverage = value.Trim();
                }
            }
        }
        #endregion

        #region Subject
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
                return extensionSubject;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionSubject = String.Empty;
                }
                else
                {
                    extensionSubject = value.Trim();
                }
            }
        }
        #endregion

        #region TableOfContents
        /// <summary>
        /// Gets or sets the list of sub-units of the resource.
        /// </summary>
        /// <value>The list of sub-units of the resource.</value>
        public string TableOfContents
        {
            get
            {
                return extensionTableOfContents;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionTableOfContents = String.Empty;
                }
                else
                {
                    extensionTableOfContents = value.Trim();
                }
            }
        }
        #endregion

        #region TemporalCoverage
        /// <summary>
        /// Gets or sets the temporal characteristics of the resource.
        /// </summary>
        /// <value>The temporal characteristics of the resource.</value>
        public string TemporalCoverage
        {
            get
            {
                return extensionTemporalCoverage;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionTemporalCoverage = String.Empty;
                }
                else
                {
                    extensionTemporalCoverage = value.Trim();
                }
            }
        }
        #endregion

        #region Title
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
                return extensionTitle;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionTitle = String.Empty;
                }
                else
                {
                    extensionTitle = value.Trim();
                }
            }
        }
        #endregion

        #region TypeVocabulary
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
        public DublinCoreTypeVocabularies TypeVocabulary
        {
            get
            {
                return extensionType;
            }

            set
            {
                extensionType = value;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Load(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            wasLoaded       = this.LoadGroup1(source, manager);
            if (this.LoadGroup2(source, manager))
            {
                wasLoaded   = true;
            }
            if (this.LoadGroup3(source, manager))
            {
                wasLoaded   = true;
            }
            if (this.LoadGroup4(source, manager))
            {
                wasLoaded   = true;
            }
            if (this.LoadGroup5(source, manager))
            {
                wasLoaded   = true;
            }
            if (this.LoadGroup6(source, manager))
            {
                wasLoaded   = true;
            }

            return wasLoaded;
        }
        #endregion

        #region WriteTo(XmlWriter writer, string xmlNamespace)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            this.WriteGroup1(writer, xmlNamespace);
            this.WriteGroup2(writer, xmlNamespace);
            this.WriteGroup3(writer, xmlNamespace);
            this.WriteGroup4(writer, xmlNamespace);
            this.WriteGroup5(writer, xmlNamespace);
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region LoadGroup1(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNavigator abstractNavigator                = source.SelectSingleNode("dcterms:abstract", manager);
                XPathNavigator accessRightsNavigator            = source.SelectSingleNode("dcterms:accessRights", manager);
                XPathNavigator accrualMethodNavigator           = source.SelectSingleNode("dcterms:accrualMethod", manager);
                XPathNavigator accrualPeriodicityNavigator      = source.SelectSingleNode("dcterms:accrualPeriodicity", manager);
                XPathNavigator accrualPolicyNavigator           = source.SelectSingleNode("dcterms:accrualPolicy", manager);
                XPathNavigator alternativeNavigator             = source.SelectSingleNode("dcterms:alternative", manager);
                XPathNavigator audienceNavigator                = source.SelectSingleNode("dcterms:audience", manager);
                XPathNavigator availableNavigator               = source.SelectSingleNode("dcterms:available", manager);
                XPathNavigator bibliographicCitationNavigator   = source.SelectSingleNode("dcterms:bibliographicCitation", manager);
                XPathNavigator conformsToNavigator              = source.SelectSingleNode("dcterms:conformsTo", manager);

                if (abstractNavigator != null && !String.IsNullOrEmpty(abstractNavigator.Value))
                {
                    this.Abstract   = abstractNavigator.Value;
                    wasLoaded       = true;
                }

                if (accessRightsNavigator != null && !String.IsNullOrEmpty(accessRightsNavigator.Value))
                {
                    this.AccessRights   = accessRightsNavigator.Value;
                    wasLoaded           = true;
                }

                if (accrualMethodNavigator != null && !String.IsNullOrEmpty(accrualMethodNavigator.Value))
                {
                    this.AccrualMethod  = accrualMethodNavigator.Value;
                    wasLoaded           = true;
                }

                if (accrualPeriodicityNavigator != null && !String.IsNullOrEmpty(accrualPeriodicityNavigator.Value))
                {
                    this.AccrualPeriodicity = accrualPeriodicityNavigator.Value;
                    wasLoaded               = true;
                }

                if (accrualPolicyNavigator != null && !String.IsNullOrEmpty(accrualPolicyNavigator.Value))
                {
                    this.AccrualPolicy  = accrualPolicyNavigator.Value;
                    wasLoaded           = true;
                }

                if (alternativeNavigator != null && !String.IsNullOrEmpty(alternativeNavigator.Value))
                {
                    this.AlternativeTitle   = alternativeNavigator.Value;
                    wasLoaded               = true;
                }

                if (audienceNavigator != null && !String.IsNullOrEmpty(audienceNavigator.Value))
                {
                    this.Audience   = audienceNavigator.Value;
                    wasLoaded       = true;
                }

                if (availableNavigator != null && !String.IsNullOrEmpty(availableNavigator.Value))
                {
                    this.DateAvailable  = availableNavigator.Value;
                    wasLoaded           = true;
                }

                if (bibliographicCitationNavigator != null && !String.IsNullOrEmpty(bibliographicCitationNavigator.Value))
                {
                    this.BibliographicCitation  = bibliographicCitationNavigator.Value;
                    wasLoaded                   = true;
                }

                if (conformsToNavigator != null && !String.IsNullOrEmpty(conformsToNavigator.Value))
                {
                    this.ConformsTo = conformsToNavigator.Value;
                    wasLoaded       = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region LoadGroup2(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator contributorNavigator     = source.SelectSingleNode("dcterms:contributor", manager);
                XPathNavigator coverageNavigator        = source.SelectSingleNode("dcterms:coverage", manager);
                XPathNavigator createdNavigator         = source.SelectSingleNode("dcterms:created", manager);
                XPathNavigator creatorNavigator         = source.SelectSingleNode("dcterms:creator", manager);
                XPathNavigator dateNavigator            = source.SelectSingleNode("dcterms:date", manager);
                XPathNavigator dateAcceptedNavigator    = source.SelectSingleNode("dcterms:dateAccepted", manager);
                XPathNavigator dateCopyrightedNavigator = source.SelectSingleNode("dcterms:dateCopyrighted", manager);
                XPathNavigator dateSubmittedNavigator   = source.SelectSingleNode("dcterms:dateSubmitted", manager);
                XPathNavigator descriptionNavigator     = source.SelectSingleNode("dcterms:description", manager);
                XPathNavigator educationLevelNavigator  = source.SelectSingleNode("dcterms:educationLevel", manager);

                if (contributorNavigator != null && !String.IsNullOrEmpty(contributorNavigator.Value))
                {
                    this.Contributor    = contributorNavigator.Value;
                    wasLoaded           = true;
                }

                if (coverageNavigator != null && !String.IsNullOrEmpty(coverageNavigator.Value))
                {
                    this.Coverage   = coverageNavigator.Value;
                    wasLoaded       = true;
                }

                if (createdNavigator != null)
                {
                    DateTime createdOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(createdNavigator.Value, out createdOn))
                    {
                        this.DateCreated    = createdOn;
                        wasLoaded           = true;
                    }
                }

                if (creatorNavigator != null && !String.IsNullOrEmpty(creatorNavigator.Value))
                {
                    this.Creator    = creatorNavigator.Value;
                    wasLoaded       = true;
                }

                if (dateNavigator != null)
                {
                    DateTime date;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateNavigator.Value, out date))
                    {
                        this.Date   = date;
                        wasLoaded   = true;
                    }
                }

                if (dateAcceptedNavigator != null)
                {
                    DateTime dateAccepted;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateAcceptedNavigator.Value, out dateAccepted))
                    {
                        this.DateAccepted   = dateAccepted;
                        wasLoaded           = true;
                    }
                }

                if (dateCopyrightedNavigator != null)
                {
                    DateTime dateCopyrighted;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCopyrightedNavigator.Value, out dateCopyrighted))
                    {
                        this.DateCopyrighted    = dateCopyrighted;
                        wasLoaded               = true;
                    }
                }

                if (dateSubmittedNavigator != null)
                {
                    DateTime dateSubmitted;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateSubmittedNavigator.Value, out dateSubmitted))
                    {
                        this.DateSubmitted  = dateSubmitted;
                        wasLoaded           = true;
                    }
                }

                if (descriptionNavigator != null && !String.IsNullOrEmpty(descriptionNavigator.Value))
                {
                    this.Description    = descriptionNavigator.Value;
                    wasLoaded           = true;
                }

                if (educationLevelNavigator != null && !String.IsNullOrEmpty(educationLevelNavigator.Value))
                {
                    this.AudienceEducationLevel = educationLevelNavigator.Value;
                    wasLoaded                   = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region LoadGroup3(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator extentNavigator              = source.SelectSingleNode("dcterms:extent", manager);
                XPathNavigator formatNavigator              = source.SelectSingleNode("dcterms:format", manager);
                XPathNavigator hasFormatNavigator           = source.SelectSingleNode("dcterms:hasFormat", manager);
                XPathNavigator hasPartNavigator             = source.SelectSingleNode("dcterms:hasPart", manager);
                XPathNavigator hasVersionNavigator          = source.SelectSingleNode("dcterms:hasVersion", manager);
                XPathNavigator identifierNavigator          = source.SelectSingleNode("dcterms:identifier", manager);
                XPathNavigator instructionalMethodNavigator = source.SelectSingleNode("dcterms:instructionalMethod", manager);
                XPathNavigator isFormatOfNavigator          = source.SelectSingleNode("dcterms:isFormatOf", manager);
                XPathNavigator isPartOfNavigator            = source.SelectSingleNode("dcterms:isPartOf", manager);
                XPathNavigator isReferencedByNavigator      = source.SelectSingleNode("dcterms:isReferencedBy", manager);

                if (extentNavigator != null && !String.IsNullOrEmpty(extentNavigator.Value))
                {
                    this.Extent = extentNavigator.Value;
                    wasLoaded   = true;
                }

                if (formatNavigator != null && !String.IsNullOrEmpty(formatNavigator.Value))
                {
                    this.Format = formatNavigator.Value;
                    wasLoaded   = true;
                }

                if (hasFormatNavigator != null && !String.IsNullOrEmpty(hasFormatNavigator.Value))
                {
                    this.HasFormat  = hasFormatNavigator.Value;
                    wasLoaded       = true;
                }

                if (hasPartNavigator != null && !String.IsNullOrEmpty(hasPartNavigator.Value))
                {
                    this.HasPart    = hasPartNavigator.Value;
                    wasLoaded       = true;
                }

                if (hasVersionNavigator != null && !String.IsNullOrEmpty(hasVersionNavigator.Value))
                {
                    this.HasVersion = hasVersionNavigator.Value;
                    wasLoaded       = true;
                }

                if (identifierNavigator != null && !String.IsNullOrEmpty(identifierNavigator.Value))
                {
                    this.Identifier = identifierNavigator.Value;
                    wasLoaded       = true;
                }

                if (instructionalMethodNavigator != null && !String.IsNullOrEmpty(instructionalMethodNavigator.Value))
                {
                    this.InstructionalMethod    = instructionalMethodNavigator.Value;
                    wasLoaded                   = true;
                }

                if (isFormatOfNavigator != null && !String.IsNullOrEmpty(isFormatOfNavigator.Value))
                {
                    this.IsFormatOf = isFormatOfNavigator.Value;
                    wasLoaded       = true;
                }

                if (isPartOfNavigator != null && !String.IsNullOrEmpty(isPartOfNavigator.Value))
                {
                    this.IsPartOf   = isPartOfNavigator.Value;
                    wasLoaded       = true;
                }

                if (isReferencedByNavigator != null && !String.IsNullOrEmpty(isReferencedByNavigator.Value))
                {
                    this.IsReferencedBy = isReferencedByNavigator.Value;
                    wasLoaded           = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region LoadGroup4(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator isReplacedByNavigator    = source.SelectSingleNode("dcterms:isReplacedBy", manager);
                XPathNavigator isRequiredByNavigator    = source.SelectSingleNode("dcterms:isRequiredBy", manager);
                XPathNavigator issuedNavigator          = source.SelectSingleNode("dcterms:issued", manager);
                XPathNavigator isVersionOfNavigator     = source.SelectSingleNode("dcterms:isVersionOf", manager);
                XPathNavigator languageNavigator        = source.SelectSingleNode("dcterms:language", manager);
                XPathNavigator licenseNavigator         = source.SelectSingleNode("dcterms:license", manager);
                XPathNavigator mediatorNavigator        = source.SelectSingleNode("dcterms:mediator", manager);
                XPathNavigator mediumNavigator          = source.SelectSingleNode("dcterms:medium", manager);
                XPathNavigator modifiedNavigator        = source.SelectSingleNode("dcterms:modified", manager);
                XPathNavigator provenanceNavigator      = source.SelectSingleNode("dcterms:provenance", manager);

                if (isReplacedByNavigator != null && !String.IsNullOrEmpty(isReplacedByNavigator.Value))
                {
                    this.IsReplacedBy   = isReplacedByNavigator.Value;
                    wasLoaded           = true;
                }

                if (isRequiredByNavigator != null && !String.IsNullOrEmpty(isRequiredByNavigator.Value))
                {
                    this.IsRequiredBy   = isRequiredByNavigator.Value;
                    wasLoaded           = true;
                }

                if (issuedNavigator != null)
                {
                    DateTime issuedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(issuedNavigator.Value, out issuedOn))
                    {
                        this.DateIssued = issuedOn;
                        wasLoaded       = true;
                    }
                }

                if (isVersionOfNavigator != null && !String.IsNullOrEmpty(isVersionOfNavigator.Value))
                {
                    this.IsVersionOf    = isVersionOfNavigator.Value;
                    wasLoaded           = true;
                }

                if (languageNavigator != null && !String.IsNullOrEmpty(languageNavigator.Value))
                {
                    try
                    {
                        CultureInfo language    = new CultureInfo(languageNavigator.Value);
                        this.Language           = language;
                        wasLoaded               = true;
                    }
                    catch (ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("DublinCoreMetadataTermsSyndicationExtensionContext unable to determine CultureInfo with a name of {0}.", languageNavigator.Value);
                    }
                }

                if (licenseNavigator != null && !String.IsNullOrEmpty(licenseNavigator.Value))
                {
                    this.License    = licenseNavigator.Value;
                    wasLoaded       = true;
                }

                if (mediatorNavigator != null && !String.IsNullOrEmpty(mediatorNavigator.Value))
                {
                    this.Mediator   = mediatorNavigator.Value;
                    wasLoaded       = true;
                }

                if (mediumNavigator != null && !String.IsNullOrEmpty(mediumNavigator.Value))
                {
                    this.Medium = mediumNavigator.Value;
                    wasLoaded   = true;
                }

                if (modifiedNavigator != null)
                {
                    DateTime modifiedOn;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(modifiedNavigator.Value, out modifiedOn))
                    {
                        this.DateModified   = modifiedOn;
                        wasLoaded           = true;
                    }
                }

                if (provenanceNavigator != null && !String.IsNullOrEmpty(provenanceNavigator.Value))
                {
                    this.Provenance = provenanceNavigator.Value;
                    wasLoaded       = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region LoadGroup5(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator publisherNavigator       = source.SelectSingleNode("dcterms:publisher", manager);
                XPathNavigator referencesNavigator      = source.SelectSingleNode("dcterms:references", manager);
                XPathNavigator relationNavigator        = source.SelectSingleNode("dcterms:relation", manager);
                XPathNavigator replacesNavigator        = source.SelectSingleNode("dcterms:replaces", manager);
                XPathNavigator requiresNavigator        = source.SelectSingleNode("dcterms:requires", manager);
                XPathNavigator rightsNavigator          = source.SelectSingleNode("dcterms:rights", manager);
                XPathNavigator rightsHolderNavigator    = source.SelectSingleNode("dcterms:rightsHolder", manager);
                XPathNavigator sourceNavigator          = source.SelectSingleNode("dcterms:source", manager);
                XPathNavigator spatialNavigator         = source.SelectSingleNode("dcterms:spatial", manager);
                XPathNavigator subjectNavigator         = source.SelectSingleNode("dcterms:subject", manager);

                if (publisherNavigator != null && !String.IsNullOrEmpty(publisherNavigator.Value))
                {
                    this.Publisher  = publisherNavigator.Value;
                    wasLoaded       = true;
                }

                if (referencesNavigator != null && !String.IsNullOrEmpty(referencesNavigator.Value))
                {
                    this.References = referencesNavigator.Value;
                    wasLoaded       = true;
                }

                if (relationNavigator != null && !String.IsNullOrEmpty(relationNavigator.Value))
                {
                    this.Relation   = relationNavigator.Value;
                    wasLoaded       = true;
                }

                if (replacesNavigator != null && !String.IsNullOrEmpty(replacesNavigator.Value))
                {
                    this.Replaces   = replacesNavigator.Value;
                    wasLoaded       = true;
                }

                if (requiresNavigator != null && !String.IsNullOrEmpty(requiresNavigator.Value))
                {
                    this.Requires   = requiresNavigator.Value;
                    wasLoaded       = true;
                }

                if (rightsNavigator != null && !String.IsNullOrEmpty(rightsNavigator.Value))
                {
                    this.Rights = rightsNavigator.Value;
                    wasLoaded   = true;
                }

                if (rightsHolderNavigator != null && !String.IsNullOrEmpty(rightsHolderNavigator.Value))
                {
                    this.RightsHolder   = rightsHolderNavigator.Value;
                    wasLoaded           = true;
                }

                if (sourceNavigator != null && !String.IsNullOrEmpty(sourceNavigator.Value))
                {
                    this.Source = sourceNavigator.Value;
                    wasLoaded   = true;
                }

                if (spatialNavigator != null && !String.IsNullOrEmpty(spatialNavigator.Value))
                {
                    this.SpatialCoverage    = spatialNavigator.Value;
                    wasLoaded               = true;
                }

                if (subjectNavigator != null && !String.IsNullOrEmpty(subjectNavigator.Value))
                {
                    this.Subject    = subjectNavigator.Value;
                    wasLoaded       = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region LoadGroup6(XPathNavigator source, XmlNamespaceManager manager)
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
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");

            //------------------------------------------------------------
            //	Attempt to extract syndication extension information
            //------------------------------------------------------------
            if (source.HasChildren)
            {
                XPathNavigator tableOfContentsNavigator = source.SelectSingleNode("dcterms:tableOfContents", manager);
                XPathNavigator temporalNavigator        = source.SelectSingleNode("dcterms:temporal", manager);
                XPathNavigator titleNavigator           = source.SelectSingleNode("dcterms:title", manager);
                XPathNavigator typeNavigator            = source.SelectSingleNode("dcterms:type", manager);
                XPathNavigator validNavigator           = source.SelectSingleNode("dcterms:valid", manager);

                if (tableOfContentsNavigator != null && !String.IsNullOrEmpty(tableOfContentsNavigator.Value))
                {
                    this.TableOfContents    = tableOfContentsNavigator.Value;
                    wasLoaded               = true;
                }

                if (temporalNavigator != null && !String.IsNullOrEmpty(temporalNavigator.Value))
                {
                    this.TemporalCoverage   = temporalNavigator.Value;
                    wasLoaded               = true;
                }

                if (titleNavigator != null && !String.IsNullOrEmpty(titleNavigator.Value))
                {
                    this.Title  = titleNavigator.Value;
                    wasLoaded   = true;
                }

                if (typeNavigator != null && !String.IsNullOrEmpty(typeNavigator.Value))
                {
                    DublinCoreTypeVocabularies typeVocabulary   = DublinCoreElementSetSyndicationExtension.TypeVocabularyByName(typeNavigator.Value);
                    if (typeVocabulary != DublinCoreTypeVocabularies.None)
                    {
                        this.TypeVocabulary = typeVocabulary;
                        wasLoaded           = true;
                    }
                }

                if (validNavigator != null && !String.IsNullOrEmpty(validNavigator.Value))
                {
                    this.DateValid  = validNavigator.Value;
                    wasLoaded       = true;
                }
            }

            return wasLoaded;
        }
        #endregion

        #region WriteGroup1(XmlWriter writer, string xmlNamespace)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            if (!String.IsNullOrEmpty(this.Abstract))
            {
                writer.WriteElementString("abstract", xmlNamespace, this.Abstract);
            }

            if (!String.IsNullOrEmpty(this.AccessRights))
            {
                writer.WriteElementString("accessRights", xmlNamespace, this.AccessRights);
            }

            if (!String.IsNullOrEmpty(this.AccrualMethod))
            {
                writer.WriteElementString("accrualMethod", xmlNamespace, this.AccrualMethod);
            }

            if (!String.IsNullOrEmpty(this.AccrualPeriodicity))
            {
                writer.WriteElementString("accrualPeriodicity", xmlNamespace, this.AccrualPeriodicity);
            }

            if (!String.IsNullOrEmpty(this.AccrualPolicy))
            {
                writer.WriteElementString("accrualPolicy", xmlNamespace, this.AccrualPolicy);
            }

            if (!String.IsNullOrEmpty(this.AlternativeTitle))
            {
                writer.WriteElementString("alternative", xmlNamespace, this.AlternativeTitle);
            }

            if (!String.IsNullOrEmpty(this.Audience))
            {
                writer.WriteElementString("audience", xmlNamespace, this.Audience);
            }

            if (!String.IsNullOrEmpty(this.DateAvailable))
            {
                writer.WriteElementString("available", xmlNamespace, this.DateAvailable);
            }

            if (!String.IsNullOrEmpty(this.BibliographicCitation))
            {
                writer.WriteElementString("bibliographicCitation", xmlNamespace, this.BibliographicCitation);
            }

            if (!String.IsNullOrEmpty(this.ConformsTo))
            {
                writer.WriteElementString("conformsTo", xmlNamespace, this.ConformsTo);
            }
        }
        #endregion

        #region WriteGroup2(XmlWriter writer, string xmlNamespace)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            if (!String.IsNullOrEmpty(this.Contributor))
            {
                writer.WriteElementString("contributor", xmlNamespace, this.Contributor);
            }

            if (!String.IsNullOrEmpty(this.Coverage))
            {
                writer.WriteElementString("coverage", xmlNamespace, this.Coverage);
            }

            if (this.DateCreated != DateTime.MinValue)
            {
                writer.WriteElementString("created", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateCreated));
            }

            if (!String.IsNullOrEmpty(this.Creator))
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

            if (!String.IsNullOrEmpty(this.Description))
            {
                writer.WriteElementString("description", xmlNamespace, this.Description);
            }

            if (!String.IsNullOrEmpty(this.AudienceEducationLevel))
            {
                writer.WriteElementString("educationLevel", xmlNamespace, this.AudienceEducationLevel);
            }
        }
        #endregion

        #region WriteGroup3(XmlWriter writer, string xmlNamespace)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            if (!String.IsNullOrEmpty(this.Extent))
            {
                writer.WriteElementString("extent", xmlNamespace, this.Extent);
            }

            if (!String.IsNullOrEmpty(this.Format))
            {
                writer.WriteElementString("format", xmlNamespace, this.Format);
            }

            if (!String.IsNullOrEmpty(this.HasFormat))
            {
                writer.WriteElementString("hasFormat", xmlNamespace, this.HasFormat);
            }

            if (!String.IsNullOrEmpty(this.HasPart))
            {
                writer.WriteElementString("hasPart", xmlNamespace, this.HasPart);
            }

            if (!String.IsNullOrEmpty(this.HasVersion))
            {
                writer.WriteElementString("hasVersion", xmlNamespace, this.HasVersion);
            }

            if (!String.IsNullOrEmpty(this.Identifier))
            {
                writer.WriteElementString("identifier", xmlNamespace, this.Identifier);
            }

            if (!String.IsNullOrEmpty(this.InstructionalMethod))
            {
                writer.WriteElementString("instructionalMethod", xmlNamespace, this.InstructionalMethod);
            }

            if (!String.IsNullOrEmpty(this.IsFormatOf))
            {
                writer.WriteElementString("isFormatOf", xmlNamespace, this.IsFormatOf);
            }

            if (!String.IsNullOrEmpty(this.IsPartOf))
            {
                writer.WriteElementString("isPartOf", xmlNamespace, this.IsPartOf);
            }

            if (!String.IsNullOrEmpty(this.IsReferencedBy))
            {
                writer.WriteElementString("isReferencedBy", xmlNamespace, this.IsReferencedBy);
            }
        }
        #endregion

        #region WriteGroup4(XmlWriter writer, string xmlNamespace)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            if (!String.IsNullOrEmpty(this.IsReplacedBy))
            {
                writer.WriteElementString("isReplacedBy", xmlNamespace, this.IsReplacedBy);
            }

            if (!String.IsNullOrEmpty(this.IsRequiredBy))
            {
                writer.WriteElementString("isRequiredBy", xmlNamespace, this.IsRequiredBy);
            }

            if (this.DateIssued != DateTime.MinValue)
            {
                writer.WriteElementString("issued", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateIssued));
            }

            if (!String.IsNullOrEmpty(this.IsVersionOf))
            {
                writer.WriteElementString("isVersionOf", xmlNamespace, this.IsVersionOf);
            }

            if (this.Language != null)
            {
                writer.WriteElementString("language", xmlNamespace, this.Language.Name);
            }

            if (!String.IsNullOrEmpty(this.License))
            {
                writer.WriteElementString("license", xmlNamespace, this.License);
            }

            if (!String.IsNullOrEmpty(this.Mediator))
            {
                writer.WriteElementString("mediator", xmlNamespace, this.Mediator);
            }

            if (!String.IsNullOrEmpty(this.Medium))
            {
                writer.WriteElementString("medium", xmlNamespace, this.Medium);
            }

            if (this.DateModified != DateTime.MinValue)
            {
                writer.WriteElementString("modified", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.DateModified));
            }

            if (!String.IsNullOrEmpty(this.Provenance))
            {
                writer.WriteElementString("provenance", xmlNamespace, this.Provenance);
            }
        }
        #endregion

        #region WriteGroup5(XmlWriter writer, string xmlNamespace)
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
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

            //------------------------------------------------------------
            //	Write current extension details to the writer
            //------------------------------------------------------------
            if (!String.IsNullOrEmpty(this.Publisher))
            {
                writer.WriteElementString("publisher", xmlNamespace, this.Publisher);
            }

            if (!String.IsNullOrEmpty(this.References))
            {
                writer.WriteElementString("references", xmlNamespace, this.References);
            }

            if (!String.IsNullOrEmpty(this.Relation))
            {
                writer.WriteElementString("relation", xmlNamespace, this.Relation);
            }

            if (!String.IsNullOrEmpty(this.Replaces))
            {
                writer.WriteElementString("replaces", xmlNamespace, this.Replaces);
            }

            if (!String.IsNullOrEmpty(this.Requires))
            {
                writer.WriteElementString("requires", xmlNamespace, this.Requires);
            }

            if (!String.IsNullOrEmpty(this.Rights))
            {
                writer.WriteElementString("rights", xmlNamespace, this.Rights);
            }

            if (!String.IsNullOrEmpty(this.RightsHolder))
            {
                writer.WriteElementString("rightsHolder", xmlNamespace, this.RightsHolder);
            }

            if (!String.IsNullOrEmpty(this.Source))
            {
                writer.WriteElementString("source", xmlNamespace, this.Source);
            }

            if (!String.IsNullOrEmpty(this.SpatialCoverage))
            {
                writer.WriteElementString("spatial", xmlNamespace, this.SpatialCoverage);
            }

            if (!String.IsNullOrEmpty(this.Subject))
            {
                writer.WriteElementString("subject", xmlNamespace, this.Subject);
            }

            if (!String.IsNullOrEmpty(this.TableOfContents))
            {
                writer.WriteElementString("tableOfContents", xmlNamespace, this.TableOfContents);
            }

            if (!String.IsNullOrEmpty(this.TemporalCoverage))
            {
                writer.WriteElementString("temporal", xmlNamespace, this.TemporalCoverage);
            }

            if (!String.IsNullOrEmpty(this.Title))
            {
                writer.WriteElementString("title", xmlNamespace, this.Title);
            }

            if (this.TypeVocabulary != DublinCoreTypeVocabularies.None)
            {
                writer.WriteElementString("type", xmlNamespace, DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(this.TypeVocabulary));
            }

            if (!String.IsNullOrEmpty(this.DateValid))
            {
                writer.WriteElementString("valid", xmlNamespace, this.DateValid);
            }
        }
        #endregion
    }
}
