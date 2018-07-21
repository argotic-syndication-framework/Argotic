/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created TrackbackSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="TrackbackSyndicationExtension"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    [Serializable()]
    public class TrackbackSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the item's TrackBack URL.
        /// </summary>
        private Uri extensionPing;
        /// <summary>
        /// Private member to hold the TRackbackURLs that were pinged in reference.
        /// </summary>
        private Collection<Uri> extensionAbouts;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region TrackbackSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackSyndicationExtensionContext"/> class.
        /// </summary>
        public TrackbackSyndicationExtensionContext()
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
        /// Gets the trackbacks that were pinged in reference.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="Uri"/> objects that represent trackbacks that were pinged in reference. 
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

        #region Ping
        /// <summary>
        /// Gets or sets the TrackBack URL.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the item's TrackBack URL.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Ping
        {
            get
            {
                return extensionPing;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionPing = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="TrackbackSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="TrackbackSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator pingNavigator    = source.SelectSingleNode("trackback:ping", manager);
                XPathNodeIterator aboutIterator = source.Select("trackback:about", manager);

                if (pingNavigator != null)
                {
                    Uri ping;
                    if (Uri.TryCreate(pingNavigator.Value, UriKind.RelativeOrAbsolute, out ping))
                    {
                        this.Ping   = ping;
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
            writer.WriteElementString("ping", xmlNamespace, this.Ping != null ? this.Ping.ToString() : String.Empty);

            foreach (Uri about in this.Abouts)
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
