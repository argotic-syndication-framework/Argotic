/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created BlogChannelSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="BlogChannelSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class BlogChannelSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the URL of an OPML file containing the blogroll for the web site.
        /// </summary>
        private Uri extensionBlogRoll;
        /// <summary>
        /// Private member to hold the URL of an OPML file containing the author's feed subscriptions.
        /// </summary>
        private Uri extensionMySubscriptions;
        /// <summary>
        /// Private member to hold the URL of a weblog that the author is promoting.
        /// </summary>
        private Uri extensionBlink;
        /// <summary>
        /// Private member to hold the URL of the site's changes file.
        /// </summary>
        private Uri extensionChanges;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region BlogChannelSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogChannelSyndicationExtensionContext"/> class.
        /// </summary>
        public BlogChannelSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Blink
        /// <summary>
        /// Gets or sets the URL of a weblog that the author is promoting.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of a weblog that the author is promoting.</value>
        public Uri Blink
        {
            get
            {
                return extensionBlink;
            }

            set
            {
                extensionBlink = value;
            }
        }
        #endregion

        #region BlogRoll
        /// <summary>
        /// Gets or sets the URL of an OPML file containing the blogroll for the web site.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of an OPML file containing the blogroll for the web site.</value>
        public Uri BlogRoll
        {
            get
            {
                return extensionBlogRoll;
            }

            set
            {
                extensionBlogRoll = value;
            }
        }
        #endregion

        #region Changes
        /// <summary>
        /// Gets or sets the URL the web site's change tracking endpoint.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL the web site's change tracking endpoint.</value>
        /// <remarks>
        ///     When a feed that contains this element updates, it pings a server that updates this file. 
        ///     The presence of this element indicates to aggregators that they only have to read the changes file to see if this feed has updated. 
        ///     If several feeds point to the same changes file, aggregators have to do less polling, resulting in better use of server bandwidth and faster scans. 
        ///     See <a href="http://www.xmlrpc.com/weblogsComForRss">http://www.xmlrpc.com/weblogsComForRss</a> for technical details.
        /// </remarks>
        public Uri Changes
        {
            get
            {
                return extensionChanges;
            }

            set
            {
                extensionChanges = value;
            }
        }
        #endregion

        #region MySubscriptions
        /// <summary>
        /// Gets or sets the URL of an OPML file containing the author's feed subscriptions.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of an OPML file containing the author's feed subscriptions.</value>
        public Uri MySubscriptions
        {
            get
            {
                return extensionMySubscriptions;
            }

            set
            {
                extensionMySubscriptions = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="BlogChannelSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="BlogChannelSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator blogRollNavigator        = source.SelectSingleNode("blogChannel:blogRoll", manager);
                XPathNavigator mySubscriptionsNavigator = source.SelectSingleNode("blogChannel:mySubscriptions", manager);
                XPathNavigator blinkNavigator           = source.SelectSingleNode("blogChannel:blink", manager);
                XPathNavigator changesNavigator         = source.SelectSingleNode("blogChannel:changes", manager);

                if (blogRollNavigator != null)
                {
                    Uri blogRoll;
                    if (Uri.TryCreate(blogRollNavigator.Value, UriKind.RelativeOrAbsolute, out blogRoll))
                    {
                        this.BlogRoll   = blogRoll;
                        wasLoaded       = true;
                    }
                }

                if (mySubscriptionsNavigator != null)
                {
                    Uri mySubscriptions;
                    if (Uri.TryCreate(mySubscriptionsNavigator.Value, UriKind.RelativeOrAbsolute, out mySubscriptions))
                    {
                        this.MySubscriptions    = mySubscriptions;
                        wasLoaded               = true;
                    }
                }

                if (blinkNavigator != null)
                {
                    Uri blink;
                    if (Uri.TryCreate(blinkNavigator.Value, UriKind.RelativeOrAbsolute, out blink))
                    {
                        this.Blink  = blink;
                        wasLoaded   = true;
                    }
                }

                if (changesNavigator != null)
                {
                    Uri changes;
                    if (Uri.TryCreate(changesNavigator.Value, UriKind.RelativeOrAbsolute, out changes))
                    {
                        this.Changes    = changes;
                        wasLoaded       = true;
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
            if(this.BlogRoll != null)
            {
                writer.WriteElementString("blogRoll", xmlNamespace, this.BlogRoll.ToString());
            }

            if (this.MySubscriptions != null)
            {
                writer.WriteElementString("mySubscriptions", xmlNamespace, this.MySubscriptions.ToString());
            }

            if (this.Blink != null)
            {
                writer.WriteElementString("blink", xmlNamespace, this.Blink.ToString());
            }

            if (this.Changes != null)
            {
                writer.WriteElementString("changes", xmlNamespace, this.Changes.ToString());
            }
        }
        #endregion
    }
}
