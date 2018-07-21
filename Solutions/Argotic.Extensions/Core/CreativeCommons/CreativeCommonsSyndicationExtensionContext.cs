/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created CreativeCommonsSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="CreativeCommonsSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class CreativeCommonsSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold a collection of URI's that represent creative commons licenses that apply to published content.
        /// </summary>
        private Collection<Uri> extensionLicenses;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region CreativeCommonsSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="CreativeCommonsSyndicationExtensionContext"/> class.
        /// </summary>
        public CreativeCommonsSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Licenses
        /// <summary>
        /// Gets or sets the creative commons licenses that apply to the published content.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of <see cref="Uri"/> objects that represent the creative commons licenses that apply to the published content.</value>
        /// <remarks>
        ///     See <a href="http://creativecommons.org/licenses/">http://creativecommons.org/licenses/</a> for a listing of the current Creative Commons licenses.
        /// </remarks>
        public Collection<Uri> Licenses
        {
            get
            {
                if (extensionLicenses == null)
                {
                    extensionLicenses = new Collection<Uri>();
                }
                return extensionLicenses;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="CreativeCommonsSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="CreativeCommonsSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNodeIterator licenseIterator   = source.Select("creativeCommons:license", manager);
                if (licenseIterator != null && licenseIterator.Count > 0)
                {
                    while (licenseIterator.MoveNext())
                    {
                        Uri license;
                        if (Uri.TryCreate(licenseIterator.Current.Value, UriKind.RelativeOrAbsolute, out license))
                        {
                            this.Licenses.Add(license);
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
            if(this.Licenses.Count > 0)
            {
                foreach(Uri license in this.Licenses)
                {
                    writer.WriteElementString("license", xmlNamespace, license.ToString());
                }
            }
        }
        #endregion
    }
}
