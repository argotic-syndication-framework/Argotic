/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/07/2007	brian.kuhn	Created Opml20SyndicationResourceAdapter Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters
{
    /// <summary>
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="OpmlDocument"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Opml20SyndicationResourceAdapter"/> serves as a bridge between a <see cref="OpmlDocument"/> and an XML data source. 
    ///         The <see cref="Opml20SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(OpmlDocument)"/>, which changes the data 
    ///         in the <see cref="OpmlDocument"/> to match the data in the data source.
    ///     </para>
    ///     <para>This syndication resource adapter is designed to fill <see cref="OpmlDocument"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the OPML 2.0 specification.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
    public class Opml20SyndicationResourceAdapter : SyndicationResourceAdapter
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region Opml20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes a new instance of the <see cref="Opml20SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication document information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="OpmlDocument"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="OpmlDocument"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Opml20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
        {
            //------------------------------------------------------------
            //	Initialization and argument validation handled by base class
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Fill(OpmlDocument resource)
        /// <summary>
        /// Modifies the <see cref="OpmlDocument"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="OpmlDocument"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(OpmlDocument resource)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Create namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager     = new XmlNamespaceManager(this.Navigator.NameTable);

            //------------------------------------------------------------
            //	Attempt to fill syndication resource
            //------------------------------------------------------------
            XPathNavigator documentNavigator    = this.Navigator.SelectSingleNode("opml", manager);
            if (documentNavigator != null)
            {
                XPathNavigator headNavigator    = documentNavigator.SelectSingleNode("head", manager);
                if (headNavigator != null)
                {
                    resource.Head.Load(headNavigator, this.Settings);
                }

                XPathNodeIterator outlineIterator   = documentNavigator.Select("body/outline", manager);
                if (outlineIterator != null && outlineIterator.Count > 0)
                {
                    int counter = 0;
                    while (outlineIterator.MoveNext())
                    {
                        OpmlOutline outline = new OpmlOutline();
                        counter++;

                        if (outline.Load(outlineIterator.Current, this.Settings))
                        {
                            if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                            {
                                break;
                            }

                            ((Collection<OpmlOutline>)resource.Outlines).Add(outline);
                        }
                    }
                }

                SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(documentNavigator, this.Settings);
                adapter.Fill(resource, manager);
            }
        }
        #endregion
    }
}
