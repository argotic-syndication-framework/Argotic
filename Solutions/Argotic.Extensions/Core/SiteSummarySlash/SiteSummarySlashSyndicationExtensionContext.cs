/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created SiteSummarySlashSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="SiteSummarySlashSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class SiteSummarySlashSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the section name.
        /// </summary>
        private string extensionSection     = String.Empty;
        /// <summary>
        /// Private member to hold the department name.
        /// </summary>
        private string extensionDepartment  = String.Empty;
        /// <summary>
        /// Private member to hold the number of comments.
        /// </summary>
        private int extensionComments       = Int32.MinValue;
        /// <summary>
        /// Private member to hold the hit parade identifiers.
        /// </summary>
        private Collection<int> extensionHitParade;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SiteSummarySlashSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSummarySlashSyndicationExtensionContext"/> class.
        /// </summary>
        public SiteSummarySlashSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Comments
        /// <summary>
        /// Gets or sets the number of comments.
        /// </summary>
        /// <value>The number of comments. The default value is <see cref="Int32.MinValue"/>, which indicates that no comment count was specified.</value>
        public int Comments
        {
            get
            {
                return extensionComments;
            }

            set
            {
                extensionComments = value;
            }
        }
        #endregion

        #region Department
        /// <summary>
        /// Gets or sets the name of the department.
        /// </summary>
        /// <value>The name of the department.</value>
        public string Department
        {
            get
            {
                return extensionDepartment;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    extensionDepartment = String.Empty;
                }
                else
                {
                    extensionDepartment = value.Trim();
                }
            }
        }
        #endregion

        #region HitParade
        /// <summary>
        /// Gets the hit parade identifiers.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="Int32"/> objects that represent the hit parade identifiers. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<int> HitParade
        {
            get
            {
                if (extensionHitParade == null)
                {
                    extensionHitParade = new Collection<int>();
                }
                return extensionHitParade;
            }
        }
        #endregion

        #region Section
        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <value>The name of the section.</value>
        public string Section
        {
            get
            {
                return extensionSection;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionSection = String.Empty;
                }
                else
                {
                    extensionSection = value.Trim();
                }
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SiteSummarySlashSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="SiteSummarySlashSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator sectionNavigator     = source.SelectSingleNode("slash:section", manager);
                XPathNavigator departmentNavigator  = source.SelectSingleNode("slash:department", manager);
                XPathNavigator commentsNavigator    = source.SelectSingleNode("slash:comments", manager);
                XPathNavigator hitParadeNavigator   = source.SelectSingleNode("slash:hit_parade", manager);

                if (sectionNavigator != null && !String.IsNullOrEmpty(sectionNavigator.Value))
                {
                    this.Section    = sectionNavigator.Value;
                    wasLoaded       = true;
                }

                if (departmentNavigator != null && !String.IsNullOrEmpty(departmentNavigator.Value))
                {
                    this.Department = departmentNavigator.Value;
                    wasLoaded       = true;
                }

                if (commentsNavigator != null)
                {
                    int comments;
                    if (Int32.TryParse(commentsNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out comments))
                    {
                        this.Comments   = comments;
                        wasLoaded       = true;
                    }
                }

                if (hitParadeNavigator != null && !String.IsNullOrEmpty(hitParadeNavigator.Value))
                {
                    if (hitParadeNavigator.Value.Contains(","))
                    {
                        string[] identifiers    = hitParadeNavigator.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (identifiers != null && identifiers.Length > 0)
                        {
                            foreach(string identifier in identifiers)
                            {
                                int paradeId;
                                if (Int32.TryParse(identifier, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out paradeId))
                                {
                                    this.HitParade.Add(paradeId);
                                    wasLoaded   = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        int hitParade;
                        if (Int32.TryParse(hitParadeNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out hitParade))
                        {
                            this.HitParade.Add(hitParade);
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
            if(!String.IsNullOrEmpty(this.Section))
            {
                writer.WriteStartElement("section", xmlNamespace);
                writer.WriteCData(this.Section);
                writer.WriteEndElement();
            }

            if (!String.IsNullOrEmpty(this.Department))
            {
                writer.WriteStartElement("department", xmlNamespace);
                writer.WriteCData(this.Department);
                writer.WriteEndElement();
            }

            if(this.Comments != Int32.MinValue)
            {
                writer.WriteElementString("comments", xmlNamespace, this.Comments.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
            }

            if(this.HitParade.Count > 0)
            {
                string[] hitParade = new string[this.HitParade.Count];
                for (int i = 0; i < this.HitParade.Count; i++)
                {
                    hitParade[i]    = this.HitParade[i].ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                }

                writer.WriteElementString("hit_parade", xmlNamespace, String.Join(",", hitParade));
            }
        }
        #endregion
    }
}
