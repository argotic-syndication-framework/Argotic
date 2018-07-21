/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
12/06/2007	brian.kuhn	Created Rss10SyndicationResourceAdapter Class
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
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="RssFeed"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Rss10SyndicationResourceAdapter"/> serves as a bridge between a <see cref="RssFeed"/> and an XML data source. 
    ///         The <see cref="Rss10SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(RssFeed)"/>, which changes the data 
    ///         in the <see cref="RssFeed"/> to match the data in the data source.
    ///     </para>
    ///     <para>This syndication resource adapter is designed to fill <see cref="RssFeed"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the RSS 1.0 specification.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public class Rss10SyndicationResourceAdapter : SyndicationResourceAdapter
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region Rss10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes a new instance of the <see cref="Rss10SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Rss10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
        {
            //------------------------------------------------------------
            //	Initialization and argument validation handled by base class
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Fill(RssFeed resource)
        /// <summary>
        /// Modifies the <see cref="RssFeed"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(RssFeed resource)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Create namespace resolver
            //------------------------------------------------------------
            XmlNamespaceManager manager     = new XmlNamespaceManager(this.Navigator.NameTable);
            manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            manager.AddNamespace("rss", "http://purl.org/rss/1.0/");

            //------------------------------------------------------------
            //	Attempt to fill syndication resource
            //------------------------------------------------------------
            XPathNavigator channelNavigator     = this.Navigator.SelectSingleNode("rdf:RDF/rss:channel", manager);
            if (channelNavigator != null)
            {
                Rss10SyndicationResourceAdapter.FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
            }

            XPathNavigator imageNavigator       = this.Navigator.SelectSingleNode("rdf:RDF/rss:image", manager);
            if (imageNavigator != null)
            {
                resource.Channel.Image          = new RssImage();
                Rss10SyndicationResourceAdapter.FillImage(resource.Channel.Image, imageNavigator, manager, this.Settings);
            }

            XPathNavigator textInputNavigator   = this.Navigator.SelectSingleNode("rdf:RDF/rss:textinput", manager);
            if (textInputNavigator != null)
            {
                resource.Channel.TextInput      = new RssTextInput();
                Rss10SyndicationResourceAdapter.FillTextInput(resource.Channel.TextInput, textInputNavigator, manager, this.Settings);
            }

            XPathNodeIterator itemIterator      = this.Navigator.Select("rdf:RDF/rss:item", manager);
            if (itemIterator != null && itemIterator.Count > 0)
            {
                int counter = 0;
                while (itemIterator.MoveNext())
                {
                    RssItem item = new RssItem();
                    counter++;

                    if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                    {
                        break;
                    }

                    XPathNavigator itemTitleNavigator       = itemIterator.Current.SelectSingleNode("rss:title", manager);
                    XPathNavigator itemLinkNavigator        = itemIterator.Current.SelectSingleNode("rss:link", manager);
                    XPathNavigator itemDescriptionNavigator = itemIterator.Current.SelectSingleNode("rss:description", manager);

                    if (itemTitleNavigator != null)
                    {
                        item.Title          = itemTitleNavigator.Value;
                    }

                    if (itemLinkNavigator != null)
                    {
                        Uri link;
                        if (Uri.TryCreate(itemLinkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                        {
                            item.Link       = link;
                        }
                    }

                    if (itemDescriptionNavigator != null)
                    {
                        item.Description    = itemDescriptionNavigator.Value;
                    }

                    SyndicationExtensionAdapter itemExtensionAdapter    = new SyndicationExtensionAdapter(itemIterator.Current, this.Settings);
                    itemExtensionAdapter.Fill(item, manager);

                    ((Collection<RssItem>)resource.Channel.Items).Add(item);
                }
            }

            SyndicationExtensionAdapter feedExtensionAdapter    = new SyndicationExtensionAdapter(this.Navigator.SelectSingleNode("rdf:RDF", manager), this.Settings);
            feedExtensionAdapter.Fill(resource, manager);
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region FillChannel(RssChannel channel, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes the supplied <see cref="RssChannel"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillChannel(RssChannel channel, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(channel, "channel");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Load required channel information
            //------------------------------------------------------------
            XPathNavigator descriptionNavigator = navigator.SelectSingleNode("rss:description", manager);
            XPathNavigator linkNavigator        = navigator.SelectSingleNode("rss:link", manager);
            XPathNavigator titleNavigator       = navigator.SelectSingleNode("rss:title", manager);

            if (descriptionNavigator != null && !String.IsNullOrEmpty(descriptionNavigator.Value))
            {
                channel.Description = descriptionNavigator.Value;
            }

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    channel.Link    = link;
                }
            }

            if (titleNavigator != null && !String.IsNullOrEmpty(titleNavigator.Value))
            {
                channel.Title       = titleNavigator.Value;
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(navigator, settings);
            adapter.Fill(channel);
        }
        #endregion

        #region FillImage(RssImage image, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes the supplied <see cref="RssImage"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the image XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="image"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillImage(RssImage image, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(image, "image");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Attempt to extract image information
            //------------------------------------------------------------
            XPathNavigator linkNavigator    = navigator.SelectSingleNode("rss:link", manager);
            XPathNavigator titleNavigator   = navigator.SelectSingleNode("rss:title", manager);
            XPathNavigator urlNavigator     = navigator.SelectSingleNode("rss:url", manager);

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    image.Link      = link;
                }
            }

            if (titleNavigator != null)
            {
                if (!String.IsNullOrEmpty(titleNavigator.Value))
                {
                    image.Title     = titleNavigator.Value;
                }
            }

            if (urlNavigator != null)
            {
                Uri url;
                if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out url))
                {
                    image.Url       = url;
                }
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(navigator, settings);
            adapter.Fill(image);
        }
        #endregion

        #region FillTextInput(RssTextInput textInput, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        /// <summary>
        /// Initializes the supplied <see cref="RssTextInput"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the text input XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="textInput"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillTextInput(RssTextInput textInput, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(textInput, "textInput");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            //------------------------------------------------------------
            //	Attempt to extract text input information
            //------------------------------------------------------------
            XPathNavigator descriptionNavigator = navigator.SelectSingleNode("rss:description", manager);
            XPathNavigator linkNavigator        = navigator.SelectSingleNode("rss:link", manager);
            XPathNavigator nameNavigator        = navigator.SelectSingleNode("rss:name", manager);
            XPathNavigator titleNavigator       = navigator.SelectSingleNode("rss:title", manager);

            if (descriptionNavigator != null)
            {
                if (!String.IsNullOrEmpty(descriptionNavigator.Value))
                {
                    textInput.Description   = descriptionNavigator.Value;
                }
            }

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    textInput.Link          = link;
                }
            }

            if (nameNavigator != null)
            {
                if (!String.IsNullOrEmpty(nameNavigator.Value))
                {
                    textInput.Name          = nameNavigator.Value;
                }
            }

            if (titleNavigator != null)
            {
                if (!String.IsNullOrEmpty(titleNavigator.Value))
                {
                    textInput.Title         = titleNavigator.Value;
                }
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(navigator, settings);
            adapter.Fill(textInput);
        }
        #endregion
    }
}
