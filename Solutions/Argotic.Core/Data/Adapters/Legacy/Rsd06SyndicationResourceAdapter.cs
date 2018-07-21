/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
12/06/2007	brian.kuhn	Created Rsd06SyndicationResourceAdapter Class
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
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="RsdDocument"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Rsd06SyndicationResourceAdapter"/> serves as a bridge between a <see cref="RsdDocument"/> and an XML data source. 
    ///         The <see cref="Rsd06SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(RsdDocument)"/>, which changes the data 
    ///         in the <see cref="RsdDocument"/> to match the data in the data source.
    ///     </para>
    ///     <para>This syndication resource adapter is designed to fill <see cref="RsdDocument"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the RSD 0.6 specification.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsd")]
    public class Rsd06SyndicationResourceAdapter : SyndicationResourceAdapter
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region Rsd06SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes a new instance of the <see cref="Rsd06SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RsdDocument"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RsdDocument"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Rsd06SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
        {
            //------------------------------------------------------------
            //	Initialization and argument validation handled by base class
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Fill(RsdDocument resource)
        /// <summary>
        /// Modifies the <see cref="RsdDocument"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="RsdDocument"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(RsdDocument resource)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Create namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager     = RsdUtility.CreateNamespaceManager(this.Navigator.NameTable);

            //------------------------------------------------------------
            //	Attempt to fill syndication resource
            //------------------------------------------------------------
            XPathNavigator serviceNavigator = RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd/rsd:service", manager);

            if (serviceNavigator == null)
            {
                //  dasBlog places an empty default XML namespace on the <service> element, this is a hack/compromise
                serviceNavigator    = RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd/service", manager);
            }

            if (serviceNavigator != null)
            {
                XPathNavigator engineNameNavigator      = RsdUtility.SelectSafeSingleNode(serviceNavigator, "rsd:engineName", manager);
                XPathNavigator engineLinkNavigator      = RsdUtility.SelectSafeSingleNode(serviceNavigator, "rsd:engineLink", manager);
                XPathNavigator homePageLinkNavigator    = RsdUtility.SelectSafeSingleNode(serviceNavigator, "rsd:homePageLink", manager);
                XPathNodeIterator apiIterator           = RsdUtility.SelectSafe(serviceNavigator, "rsd:apis/rsd:api", manager);

                if (engineNameNavigator != null && !String.IsNullOrEmpty(engineNameNavigator.Value))
                {
                    resource.EngineName     = engineNameNavigator.Value;
                }

                if (engineLinkNavigator != null)
                {
                    Uri link;
                    if (Uri.TryCreate(engineLinkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                    {
                        resource.EngineLink = link;
                    }
                }

                if (homePageLinkNavigator != null)
                {
                    Uri homepage;
                    if (Uri.TryCreate(homePageLinkNavigator.Value, UriKind.RelativeOrAbsolute, out homepage))
                    {
                        resource.Homepage   = homepage;
                    }
                }

                if (apiIterator != null && apiIterator.Count > 0)
                {
                    int counter = 0;
                    while (apiIterator.MoveNext())
                    {
                        RsdApplicationInterface api = new RsdApplicationInterface();
                        counter++;

                        Uri link;
                        string rpcLinkAttribute = apiIterator.Current.GetAttribute("rpcLink", String.Empty);
                        if (Uri.TryCreate(rpcLinkAttribute, UriKind.RelativeOrAbsolute, out link))
                        {
                            api.Link    = link;
                        }

                        if (api.Load(apiIterator.Current, this.Settings) || api.Link != null)
                        {
                            if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                            {
                                break;
                            }

                            ((Collection<RsdApplicationInterface>)resource.Interfaces).Add(api);
                        }
                    }
                }
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd", manager), this.Settings);
            adapter.Fill(resource, manager);
        }
        #endregion
    }
}
