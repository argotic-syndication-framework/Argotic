﻿namespace Argotic.Extensions.Core
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="LiveJournalSyndicationExtension"/>.
    /// </summary>
    [Serializable]
    public class LiveJournalSyndicationExtensionContext
    {
        /// <summary>
        /// Private member to hold the current music.
        /// </summary>
        private string extensionMusic = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalSyndicationExtensionContext"/> class.
        /// </summary>
        public LiveJournalSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if the author has requested that the entry HTML be displayed without modification.
        /// </summary>
        /// <value><b>true</b> if the author has requested that the entry HTML be displayed without modification; otherwise <b>false</b>.</value>
        /// <remarks>
        ///     If <b>false</b>, newlines within the entry must be expanded into visual newlines (using the <i>br</i> tag) to be displayed properly.
        /// </remarks>
        public bool IsPreformatted { get; set; }

        /// <summary>
        /// Gets or sets the current mood.
        /// </summary>
        /// <value>A <see cref="LiveJournalMood"/> object that represents the current mood.</value>
        public LiveJournalMood Mood { get; set; }

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
                return this.extensionMusic;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.extensionMusic = string.Empty;
                }
                else
                {
                    this.extensionMusic = value.Trim();
                }
            }
        }

        /// <summary>
        /// Gets or sets the access level.
        /// </summary>
        /// <value>A <see cref="LiveJournalSecurity"/> object that represents the access level.</value>
        /// <remarks>
        ///     If absent, the entry is assumed to be <see cref="LiveJournalSecurityType.Public">publicly</see> accessible.
        ///     All feeds requested without authentication will <b>only</b> contain public entries.
        /// </remarks>
        public LiveJournalSecurity Security { get; set; }

        /// <summary>
        /// Gets or sets theassociated user picture.
        /// </summary>
        /// <value>A <see cref="LiveJournalUserPicture"/> object that represents the associated user picture.</value>
        /// <remarks>
        ///     If omitted, the LiveJournal entry uses the feed-level default picture.
        /// </remarks>
        public LiveJournalUserPicture UserPicture { get; set; }

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
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            if (source.HasChildren)
            {
                XPathNavigator musicNavigator = source.SelectSingleNode("lj:music", manager);
                XPathNavigator moodNavigator = source.SelectSingleNode("lj:mood", manager);
                XPathNavigator securityNavigator = source.SelectSingleNode("lj:security", manager);
                XPathNavigator userPictureNavigator = source.SelectSingleNode("lj:userpic", manager);
                XPathNavigator preformattedNavigator = source.SelectSingleNode("lj:preformatted", manager);

                if (musicNavigator != null && !string.IsNullOrEmpty(musicNavigator.Value))
                {
                    this.Music = musicNavigator.Value;
                    wasLoaded = true;
                }

                if (moodNavigator != null)
                {
                    LiveJournalMood mood = new LiveJournalMood();
                    if (mood.Load(moodNavigator))
                    {
                        this.Mood = mood;
                        wasLoaded = true;
                    }
                }

                if (securityNavigator != null)
                {
                    LiveJournalSecurity security = new LiveJournalSecurity();
                    if (security.Load(securityNavigator))
                    {
                        this.Security = security;
                        wasLoaded = true;
                    }
                }

                if (userPictureNavigator != null)
                {
                    LiveJournalUserPicture userPicture = new LiveJournalUserPicture();
                    if (userPicture.Load(userPictureNavigator))
                    {
                        this.UserPicture = userPicture;
                        wasLoaded = true;
                    }
                }

                if (preformattedNavigator != null)
                {
                    this.IsPreformatted = true;
                    wasLoaded = true;
                }
            }

            return wasLoaded;
        }

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
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");
            if (!string.IsNullOrEmpty(this.Music))
            {
                writer.WriteStartElement("music", xmlNamespace);
                writer.WriteCData(this.Music);
                writer.WriteEndElement();
            }

            if (this.Mood != null)
            {
                this.Mood.WriteTo(writer);
            }

            if (this.Security != null)
            {
                this.Security.WriteTo(writer);
            }

            if (this.IsPreformatted)
            {
                writer.WriteElementString("preformatted", xmlNamespace, string.Empty);
            }
        }
    }
}