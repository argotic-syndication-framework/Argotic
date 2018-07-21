/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created FeedRankSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="FeedRankSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class FeedRankSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the permanent, universally unique identifier for the ranking scheme.
        /// </summary>
        private Uri extensionScheme;
        /// <summary>
        /// Private member to hold the permanent, universally unique identifier for the ranking domain.
        /// </summary>
        private Uri extensionDomain;
        /// <summary>
        /// Private member to hold the language sensitive, human-readable label for the rank.
        /// </summary>
        private string extensionLabel   = String.Empty;
        /// <summary>
        /// Private member to hold the decimal value of the rank.
        /// </summary>
        private decimal extensionValue  = Decimal.MinValue;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region FeedRankSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedRankSyndicationExtensionContext"/> class.
        /// </summary>
        public FeedRankSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region FeedRankSyndicationExtensionContext(Uri scheme, decimal value)
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedRankSyndicationExtensionContext"/> class using the supplied schem and value.
        /// </summary>
        /// <param name="scheme">The <see cref="Uri"/> that describes the permanent, universally unique identifier for the ranking scheme.</param>
        /// <param name="value">The <see cref="Decimal"/> value of the rank.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="scheme"/> is a null reference (Nothing in Visual Basic).</exception>
        public FeedRankSyndicationExtensionContext(Uri scheme, decimal value)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Scheme = scheme;
            this.Value  = value;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Domain
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking domain.
        /// </summary>
        /// <value>The <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking domain.</value>
        public Uri Domain
        {
            get
            {
                return extensionDomain;
            }

            set
            {
                extensionDomain = value;
            }
        }
        #endregion

        #region Label
        /// <summary>
        /// Gets or sets the language sensitive, human-readable label for this rank.
        /// </summary>
        /// <value>The language sensitive, human-readable label for this rank.</value>
        public string Label
        {
            get
            {
                return extensionLabel;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionLabel = String.Empty;
                }
                else
                {
                    extensionLabel = value.Trim();
                }
            }
        }
        #endregion

        #region Scheme
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking scheme.
        /// </summary>
        /// <value>The <see cref="Uri"/> that describes the permanent, universally unique identifier for this ranking scheme.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Scheme
        {
            get
            {
                return extensionScheme;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionScheme = value;
            }
        }
        #endregion

        #region Value
        /// <summary>
        /// Gets or sets the value of this rank.
        /// </summary>
        /// <value>The <see cref="Decimal"/> value of this rank. The default value is <see cref="Decimal.MinValue"/>, which indicates that no ranking value was specified.</value>
        public decimal Value
        {
            get
            {
                return extensionValue;
            }

            set
            {
                extensionValue = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="FeedRankSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="FeedRankSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
            if (source.HasChildren)
            {
                XPathNavigator rankNavigator    = source.SelectSingleNode("re:rank", manager);
                if (rankNavigator != null)
                {
                    if (rankNavigator.HasAttributes)
                    {
                        string schemeAttribute  = rankNavigator.GetAttribute("scheme", String.Empty);
                        string domainAttribute  = rankNavigator.GetAttribute("domain", String.Empty);
                        string labelAttribute   = rankNavigator.GetAttribute("label", String.Empty);

                        if (!String.IsNullOrEmpty(schemeAttribute))
                        {
                            Uri scheme;
                            if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out scheme))
                            {
                                this.Scheme = scheme;
                                wasLoaded   = true;
                            }
                        }

                        if (!String.IsNullOrEmpty(domainAttribute))
                        {
                            Uri domain;
                            if (Uri.TryCreate(domainAttribute, UriKind.RelativeOrAbsolute, out domain))
                            {
                                this.Domain = domain;
                                wasLoaded   = true;
                            }
                        }

                        if (!String.IsNullOrEmpty(labelAttribute))
                        {
                            this.Label  = labelAttribute;
                            wasLoaded   = true;
                        }
                    }

                    if (!String.IsNullOrEmpty(rankNavigator.Value))
                    {
                        decimal value;
                        if (Decimal.TryParse(rankNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out value))
                        {
                            this.Value  = value;
                            wasLoaded   = true;
                        }
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
            writer.WriteStartElement("rank", xmlNamespace);

            writer.WriteAttributeString("scheme", xmlNamespace, this.Scheme != null ? this.Scheme.ToString() : String.Empty);

            if(this.Domain != null)
            {
                writer.WriteAttributeString("domain", xmlNamespace, this.Domain.ToString());
            }

            if (!String.IsNullOrEmpty(this.Label))
            {
                writer.WriteAttributeString("label", this.Label);
            }

            if (this.Value != Decimal.MinValue)
            {
                writer.WriteString(this.Value.ToString(NumberFormatInfo.InvariantInfo));
            }

            writer.WriteEndElement();
        }
        #endregion
    }
}
