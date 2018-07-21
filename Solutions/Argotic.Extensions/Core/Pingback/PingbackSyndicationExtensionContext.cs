/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created PingbackSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="PingbackSyndicationExtension"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pingback")]
    [Serializable()]
    public class PingbackSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the URL of the Pingback server.
        /// </summary>
        private Uri extensionServer;
        /// <summary>
        /// Private member to hold the value that should be used as the target in a ping.
        /// </summary>
        private Uri extensionTarget;
        /// <summary>
        /// Private member to hold the targets that were pinged in reference.
        /// </summary>
        private Collection<Uri> extensionAbouts;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region PingbackSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="PingbackSyndicationExtensionContext"/> class.
        /// </summary>
        public PingbackSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Abouts
        /// <summary>
        /// Gets the targets that were pinged in reference.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="Uri"/> objects that represent targets that were pinged in reference. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Abouts")]
        public Collection<Uri> Abouts
        {
            get
            {
                if (extensionAbouts == null)
                {
                    extensionAbouts = new Collection<Uri>();
                }
                return extensionAbouts;
            }
        }
        #endregion

        #region Server
        /// <summary>
        /// Gets or sets the URL of the Pingback server.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the Pingback server.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Server
        {
            get
            {
                return extensionServer;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionServer = value;
            }
        }
        #endregion

        #region Target
        /// <summary>
        /// Gets or sets the value that should be used as the <i>targetURI</i> in a ping.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the value that should be used as the <i>targetURI</i> in a ping.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Target
        {
            get
            {
                return extensionTarget;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionTarget = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="PingbackSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="PingbackSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator serverNavigator  = source.SelectSingleNode("pingback:server", manager);
                XPathNavigator targetNavigator  = source.SelectSingleNode("pingback:target", manager);
                XPathNodeIterator aboutIterator = source.Select("pingback:about", manager);

                if (serverNavigator != null)
                {
                    Uri server;
                    if (Uri.TryCreate(serverNavigator.Value, UriKind.RelativeOrAbsolute, out server))
                    {
                        this.Server = server;
                        wasLoaded   = true;
                    }
                }

                if (targetNavigator != null)
                {
                    Uri target;
                    if (Uri.TryCreate(targetNavigator.Value, UriKind.RelativeOrAbsolute, out target))
                    {
                        this.Target = target;
                        wasLoaded   = true;
                    }
                }

                if (aboutIterator != null && aboutIterator.Count > 0)
                {
                    while (aboutIterator.MoveNext())
                    {
                        Uri about;
                        if (Uri.TryCreate(aboutIterator.Current.Value, UriKind.RelativeOrAbsolute, out about))
                        {
                            this.Abouts.Add(about);
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
            writer.WriteElementString("server", xmlNamespace, this.Server != null ? this.Server.ToString() : String.Empty);
            writer.WriteElementString("target", xmlNamespace, this.Target != null ? this.Target.ToString() : String.Empty);

            foreach(Uri about in this.Abouts)
            {
                if (about != null)
                {
                    writer.WriteElementString("about", xmlNamespace, about.ToString());
                }
            }
        }
        #endregion
    }
}
