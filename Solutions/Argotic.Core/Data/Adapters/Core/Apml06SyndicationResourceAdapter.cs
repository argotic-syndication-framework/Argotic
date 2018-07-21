/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/07/2007	brian.kuhn	Created Apml06SyndicationResourceAdapter Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;

namespace Argotic.Data.Adapters
{
    /// <summary>
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="ApmlDocument"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Apml06SyndicationResourceAdapter"/> serves as a bridge between a <see cref="ApmlDocument"/> and an XML data source. 
    ///         The <see cref="Apml06SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(ApmlDocument)"/>, which changes the data 
    ///         in the <see cref="ApmlDocument"/> to match the data in the data source.
    ///     </para>
    ///     <para>This syndication resource adapter is designed to fill <see cref="ApmlDocument"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the APML 0.6 specification.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Apml")]
    public class Apml06SyndicationResourceAdapter : SyndicationResourceAdapter
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region Apml06SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes a new instance of the <see cref="Apml06SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="ApmlDocument"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="ApmlDocument"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Apml06SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
        {
            //------------------------------------------------------------
            //	Initialization and argument validation handled by base class
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Fill(ApmlDocument resource)
        /// <summary>
        /// Modifies the <see cref="ApmlDocument"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="ApmlDocument"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(ApmlDocument resource)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Create namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager     = ApmlUtility.CreateNamespaceManager(this.Navigator.NameTable);

            //------------------------------------------------------------
            //	Attempt to fill syndication resource
            //------------------------------------------------------------
            XPathNavigator headNavigator    = this.Navigator.SelectSingleNode("apml:APML/apml:Head", manager);
            if (headNavigator != null)
            {
                resource.Head.Load(headNavigator, this.Settings);
            }

            XPathNavigator bodyNavigator    = this.Navigator.SelectSingleNode("apml:APML/apml:Body", manager);
            if (bodyNavigator != null)
            {
                if (bodyNavigator.HasAttributes)
                {
                    string defaultProfileAttribute  = bodyNavigator.GetAttribute("defaultprofile", String.Empty);
                    if (!String.IsNullOrEmpty(defaultProfileAttribute))
                    {
                        resource.DefaultProfileName = defaultProfileAttribute;
                    }
                }

                XPathNodeIterator profileIterator   = bodyNavigator.Select("apml:Profile", manager);
                if (profileIterator != null && profileIterator.Count > 0)
                {
                    int counter = 0;
                    while (profileIterator.MoveNext())
                    {
                        ApmlProfile profile = new ApmlProfile();
                        counter++;

                        if (profile.Load(profileIterator.Current, this.Settings))
                        {
                            if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                            {
                                break;
                            }

                            ((Collection<ApmlProfile>)resource.Profiles).Add(profile);
                        }
                    }
                }

                XPathNodeIterator applicationIterator   = bodyNavigator.Select("apml:Applications/apml:Application", manager);
                if (applicationIterator != null && applicationIterator.Count > 0)
                {
                    while (applicationIterator.MoveNext())
                    {
                        ApmlApplication application = new ApmlApplication();
                        if (application.Load(applicationIterator.Current, this.Settings))
                        {
                            resource.Applications.Add(application);
                        }
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(this.Navigator.SelectSingleNode("apml:APML", manager), this.Settings);
            adapter.Fill(resource, manager);
        }
        #endregion
    }
}
