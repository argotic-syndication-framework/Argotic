/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created SiteSummaryUpdateSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="SiteSummaryUpdateSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class SiteSummaryUpdateSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the period over which the feed format is updated.
        /// </summary>
        private SiteSummaryUpdatePeriod extensionUpdatePeriod   = SiteSummaryUpdatePeriod.None;
        /// <summary>
        /// Private member to hold the frequency of updates in relation to the update period.
        /// </summary>
        private int extensionUpdateFrequency                    = Int32.MinValue;
        /// <summary>
        /// Private member to hold a base date to be used in concert with period and frequency to calculate the publishing schedule.
        /// </summary>
        private DateTime extensionUpdateBase                    = DateTime.MinValue;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SiteSummaryUpdateSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> class.
        /// </summary>
        public SiteSummaryUpdateSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Base
        /// <summary>
        /// Gets or sets the base date to be used in concert with period and frequency to calculate the publishing schedule.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> that represents the base date to be used in concert with period and frequency to calculate the publishing schedule. 
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates no base date was specified.
        /// </value>
        /// <remarks>
        ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
        /// </remarks>
        public DateTime Base
        {
            get
            {
                return extensionUpdateBase;
            }

            set
            {
                extensionUpdateBase = value;
            }
        }
        #endregion

        #region Frequency
        /// <summary>
        /// Gets or sets the frequency of updates in relation to the update period.
        /// </summary>
        /// <value>The frequency of updates in relation to the update period.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <b>1</b>.</exception>
        public int Frequency
        {
            get
            {
                return extensionUpdateFrequency;
            }
            
            set
            {
                Guard.ArgumentNotLessThan(value, "value", 1);
                extensionUpdateFrequency = value;
            }
        }
        #endregion

        #region Period
        /// <summary>
        /// Gets or sets the period over which the feed format is updated.
        /// </summary>
        /// <value>
        ///     A <see cref="SiteSummaryUpdatePeriod"/> enumeration value that indicates the period over which the feed format is updated. 
        ///     The default value is <see cref="SiteSummaryUpdatePeriod.None"/>, which indicates that no update period was specified.
        /// </value>
        public SiteSummaryUpdatePeriod Period
        {
            get
            {
                return extensionUpdatePeriod;
            }

            set
            {
                extensionUpdatePeriod = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SiteSummaryUpdateSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
            if(source.HasChildren)
            {
                XPathNavigator updatePeriodNavigator    = source.SelectSingleNode("sy:updatePeriod", manager);
                XPathNavigator updateFrequencyNavigator = source.SelectSingleNode("sy:updateFrequency", manager);
                XPathNavigator updateBaseNavigator      = source.SelectSingleNode("sy:updateBase", manager);

                if (updatePeriodNavigator != null && !String.IsNullOrEmpty(updatePeriodNavigator.Value))
                {
                    SiteSummaryUpdatePeriod period  = SiteSummaryUpdateSyndicationExtension.PeriodByName(updatePeriodNavigator.Value);
                    if (period != SiteSummaryUpdatePeriod.None)
                    {
                        this.Period = period;
                        wasLoaded   = true;
                    }
                }

                if (updateFrequencyNavigator != null)
                {
                    int frequency;
                    if (Int32.TryParse(updateFrequencyNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out frequency))
                    {
                        this.Frequency  = frequency;
                        wasLoaded       = true;
                    }
                }

                if (updateBaseNavigator != null)
                {
                    DateTime updateBase;
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updateBaseNavigator.Value, out updateBase))
                    {
                        this.Base   = updateBase;
                        wasLoaded   = true;
                    }
                }
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
            if(this.Period != SiteSummaryUpdatePeriod.None)
            {
                writer.WriteElementString("updatePeriod", xmlNamespace, SiteSummaryUpdateSyndicationExtension.PeriodAsString(this.Period));
            }

            if(this.Frequency != Int32.MinValue)
            {
                writer.WriteElementString("updateFrequency", xmlNamespace, this.Frequency.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if(this.Base != DateTime.MinValue)
            {
                writer.WriteElementString("updateBase", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.Base));
            }
        }
        #endregion
    }
}
