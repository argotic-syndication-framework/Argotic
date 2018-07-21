/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created PheedSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="PheedSyndicationExtension"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pheed")]
    [Serializable()]
    public class PheedSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold a thumbnail sized version of the photograph.
        /// </summary>
        private Uri extensionThumbnail;
        /// <summary>
        /// Private member to hold a larger or original version of the photograph.
        /// </summary>
        private Uri extensionImageSource;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region PheedSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="PheedSyndicationExtensionContext"/> class.
        /// </summary>
        public PheedSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region PheedSyndicationExtensionContext(Uri source, Uri thumbnail)
        /// <summary>
        /// Initializes a new instance of the <see cref="PheedSyndicationExtensionContext"/> class using the supplied parameters.
        /// </summary>
        /// <param name="source">>A <see cref="Uri"/> that represents a URL to the original version of this photograph.</param>
        /// <param name="thumbnail">A <see cref="Uri"/> that represents a URL to a thumbnail sized version of this photograph.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="thumbnail"/> is a null reference (Nothing in Visual Basic).</exception>
        public PheedSyndicationExtensionContext(Uri source, Uri thumbnail)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Source     = source;
            this.Thumbnail  = thumbnail;
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Source
        /// <summary>
        /// Gets or sets the original version of this photograph.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a URL to the original version of this photograph.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Source
        {
            get
            {
                return extensionImageSource;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionImageSource = value;
            }
        }
        #endregion

        #region Thumbnail
        /// <summary>
        /// Gets or sets the thumbnail sized version of this photograph.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a URL to a thumbnail sized version of this photograph.</value>
        /// <remarks>
        ///     The maximum size of the longest dimension <b>must be</b> 120 pixels.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Thumbnail
        {
            get
            {
                return extensionThumbnail;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionThumbnail = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="PheedSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="PheedSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator thumbnailNavigator   = source.SelectSingleNode("photo:thumbnail", manager);
                XPathNavigator imageSourceNavigator = source.SelectSingleNode("photo:imgsrc", manager);

                if (thumbnailNavigator != null)
                {
                    Uri thumbnail;
                    if (Uri.TryCreate(thumbnailNavigator.Value, UriKind.RelativeOrAbsolute, out thumbnail))
                    {
                        this.Thumbnail  = thumbnail;
                        wasLoaded       = true;
                    }
                }

                if (imageSourceNavigator != null)
                {
                    Uri original;
                    if (Uri.TryCreate(imageSourceNavigator.Value, UriKind.RelativeOrAbsolute, out original))
                    {
                        this.Source = original;
                        wasLoaded   = true;
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
            writer.WriteElementString("thumbnail", xmlNamespace, this.Thumbnail != null ? this.Thumbnail.ToString() : String.Empty);
            writer.WriteElementString("imgsrc", xmlNamespace, this.Source != null ? this.Source.ToString() : String.Empty);
        }
        #endregion
    }
}
