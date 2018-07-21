/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created LiveJournalSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="LiveJournalSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class LiveJournalSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the current music.
        /// </summary>
        private string extensionMusic   = String.Empty;
        /// <summary>
        /// Private member to hold the current mood.
        /// </summary>
        private LiveJournalMood extensionMood;
        /// <summary>
        /// Private member to hold the access level. 
        /// </summary>
        private LiveJournalSecurity extensionSecurity;
        /// <summary>
        /// Private member to hold a value indicating if entry has been preformatted.
        /// </summary>
        private bool extensionIsPreformatted;
        /// <summary>
        /// Private member to hold the associated user picture.
        /// </summary>
        private LiveJournalUserPicture extensionUserPicture;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region LiveJournalSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSyndicationExtensionContext"/> class.
        /// </summary>
        public LiveJournalSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region IsPreformatted
        /// <summary>
        /// Gets or sets a value indicating if the author has requested that the entry HTML be displayed without modification.
        /// </summary>
        /// <value><b>true</b> if the author has requested that the entry HTML be displayed without modification; otherwise <b>false</b>.</value>
        /// <remarks>
        ///     If <b>false</b>, newlines within the entry must be expanded into visual newlines (using the <i>br</i> tag) to be displayed properly.
        /// </remarks>
        public bool IsPreformatted
        {
            get
            {
                return extensionIsPreformatted;
            }

            set
            {
                extensionIsPreformatted = value;
            }
        }
        #endregion

        #region Mood
        /// <summary>
        /// Gets or sets the current mood.
        /// </summary>
        /// <value>A <see cref="LiveJournalMood"/> object that represents the current mood.</value>
        public LiveJournalMood Mood
        {
            get
            {
                return extensionMood;
            }

            set
            {
                extensionMood = value;
            }
        }
        #endregion

        #region Music
        /// <summary>
        /// Gets or sets the current music.
        /// </summary>
        /// <value>Textual or entity encoded content that represents the current music.</value>
        /// <remarks>
        ///     There is no standard format for presenting broken-down information such as artist or album.
        /// </remarks>
        public string Music
        {
            get
            {
                return extensionMusic;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    extensionMusic = String.Empty;
                }
                else
                {
                    extensionMusic = value.Trim();
                }
            }
        }
        #endregion

        #region Security
        /// <summary>
        /// Gets or sets the access level.
        /// </summary>
        /// <value>A <see cref="LiveJournalSecurity"/> object that represents the access level.</value>
        /// <remarks>
        ///     If absent, the entry is assumed to be <see cref="LiveJournalSecurityType.Public">publicly</see> accessible. 
        ///     All feeds requested without authentication will <b>only</b> contain public entries.
        /// </remarks>
        public LiveJournalSecurity Security
        {
            get
            {
                return extensionSecurity;
            }

            set
            {
                extensionSecurity = value;
            }
        }
        #endregion

        #region UserPicture
        /// <summary>
        /// Gets or sets theassociated user picture.
        /// </summary>
        /// <value>A <see cref="LiveJournalUserPicture"/> object that represents the associated user picture.</value>
        /// <remarks>
        ///     If omitted, the LiveJournal entry uses the feed-level default picture.
        /// </remarks>
        public LiveJournalUserPicture UserPicture
        {
            get
            {
                return extensionUserPicture;
            }

            set
            {
                extensionUserPicture = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="LiveJournalSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="LiveJournalSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNavigator musicNavigator           = source.SelectSingleNode("lj:music", manager);
                XPathNavigator moodNavigator            = source.SelectSingleNode("lj:mood", manager);
                XPathNavigator securityNavigator        = source.SelectSingleNode("lj:security", manager);
                XPathNavigator userPictureNavigator     = source.SelectSingleNode("lj:userpic", manager);
                XPathNavigator preformattedNavigator    = source.SelectSingleNode("lj:preformatted", manager);

                if (musicNavigator != null && !String.IsNullOrEmpty(musicNavigator.Value))
                {
                    this.Music  = musicNavigator.Value;
                    wasLoaded   = true;
                }

                if (moodNavigator != null)
                {
                    LiveJournalMood mood    = new LiveJournalMood();
                    if (mood.Load(moodNavigator))
                    {
                        this.Mood   = mood;
                        wasLoaded   = true;
                    }
                }

                if (securityNavigator != null)
                {
                    LiveJournalSecurity security    = new LiveJournalSecurity();
                    if (security.Load(securityNavigator))
                    {
                        this.Security   = security;
                        wasLoaded       = true;
                    }
                }

                if (userPictureNavigator != null)
                {
                    LiveJournalUserPicture userPicture  = new LiveJournalUserPicture();
                    if (userPicture.Load(userPictureNavigator))
                    {
                        this.UserPicture    = userPicture;
                        wasLoaded           = true;
                    }
                }

                if (preformattedNavigator != null)
                {
                    this.IsPreformatted = true;
                    wasLoaded           = true;
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
            if (!String.IsNullOrEmpty(this.Music))
            {
                writer.WriteStartElement("music", xmlNamespace);
                writer.WriteCData(this.Music);
                writer.WriteEndElement();
            }

            if(this.Mood != null)
            {
                this.Mood.WriteTo(writer);
            }

            if (this.Security != null)
            {
                this.Security.WriteTo(writer);
            }

            if (this.IsPreformatted)
            {
                writer.WriteElementString("preformatted", xmlNamespace, String.Empty);
            }
        }
        #endregion
    }
}
