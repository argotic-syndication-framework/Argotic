/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created SiteSummaryContentSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="SiteSummaryContentSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class SiteSummaryContentSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the entity-encoded or CDATA-escaped version of the content of the item.
        /// </summary>
        private string extensionEncoded = String.Empty;
        /// <summary>
        /// Private member to hold the alternative versions of the item's content.
        /// </summary>
        private Collection<SiteSummaryContentItem> extensionItems;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SiteSummaryContentSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSummaryContentSyndicationExtensionContext"/> class.
        /// </summary>
        public SiteSummaryContentSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Encoded
        /// <summary>
        /// Gets or sets an alternative version of the content of this item.
        /// </summary>
        /// <value>The alternative version of the content of this item.</value>
        /// <remarks>
        ///     The value of this property <i>may</i> be entity-encoded, but will <b>always</b> be CDATA-escaped. 
        /// </remarks>
        public string Encoded
        {
            get
            {
                return extensionEncoded;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    extensionEncoded = String.Empty;
                }
                else
                {
                    extensionEncoded = value.Trim();
                }
            }
        }
        #endregion

        #region Items
        /// <summary>
        /// Gets the alternative versions of this item's content.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="SiteSummaryContentItem"/> objects that represent multiple versions of this item's content. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     The <see cref="Encoded"/> property represents the <b>updated</b> syntax for the <see cref="SiteSummaryContentSyndicationExtension"/>. 
        ///     It is <i>recommended</i> that <see cref="Encoded"/> is utilized when defining an alternative encoding for the content of an item.
        /// </remarks>
        public Collection<SiteSummaryContentItem> Items
        {
            get
            {
                if (extensionItems == null)
                {
                    extensionItems = new Collection<SiteSummaryContentItem>();
                }
                return extensionItems;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SiteSummaryContentSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="SiteSummaryContentSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator encodedNavigator = source.SelectSingleNode("content:encoded", manager);
                XPathNavigator itemsNavigator   = source.SelectSingleNode("content:items", manager);

                if (encodedNavigator != null && !String.IsNullOrEmpty(encodedNavigator.Value))
                {
                    this.Encoded    = encodedNavigator.Value;
                    wasLoaded       = true;
                }

                if (itemsNavigator != null && itemsNavigator.HasChildren)
                {
                    XPathNodeIterator itemIterator  = itemsNavigator.Select("content:item", manager);
                    if (itemIterator != null && itemIterator.Count > 0)
                    {
                        while (itemIterator.MoveNext())
                        {
                            SiteSummaryContentItem item = new SiteSummaryContentItem();
                            if (item.Load(itemIterator.Current))
                            {
                                this.Items.Add(item);
                                wasLoaded   = true;
                            }
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
            if(!String.IsNullOrEmpty(this.Encoded))
            {
                writer.WriteStartElement("encoded", xmlNamespace);
                writer.WriteCData(this.Encoded);
                writer.WriteEndElement();
            }

            if(this.Items.Count > 0)
            {
                writer.WriteStartElement("items", xmlNamespace);
                foreach(SiteSummaryContentItem item in this.Items)
                {
                    item.WriteTo(writer);
                }
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
