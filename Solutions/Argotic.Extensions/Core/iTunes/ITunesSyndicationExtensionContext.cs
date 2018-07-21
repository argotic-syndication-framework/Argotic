/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created ITunesSyndicationExtensionContext Class
07/22/2008  brian.kuhn  Fixed issue 10626
11/15/2009  Ilja Khaprov Fixed issue 13840
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="ITunesSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class ITunesSyndicationExtensionContext
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the name of the artist of the podcast.
        /// </summary>
        private string extensionAuthor                                      = String.Empty;
        /// <summary>
        /// Private member to hold a value indicating if the podcast is blocked from appearing in the iTunes Podcast directory.
        /// </summary>
        private bool extensionIsBlocked;
        /// <summary>
        /// Private member to hold the categorization taxonomy applied to the podcast.
        /// </summary>
        private Collection<ITunesCategory> extensionCategories;
        /// <summary>
        /// Private member to hold the total duration of the podcast.
        /// </summary>
        private TimeSpan extensionDuration                                  = TimeSpan.MinValue;
        /// <summary>
        /// Private member to hold a value indicating if the podcast contains explicit material.
        /// </summary>
        private ITunesExplicitMaterial extensionExplicitMaterialDesignation = ITunesExplicitMaterial.None;
        /// <summary>
        /// Private member to hold a URL that points to the album artwork for the podcast.
        /// </summary>
        private Uri extensionImage;
        /// <summary>
        ///  Private member to hold keywords that describe the podcast.
        /// </summary>
        private Collection<string> extensionKeywords;
        /// <summary>
        /// Private member to hold the URL where the podcast feed is has been relocated to.
        /// </summary>
        private Uri extensionNewFeedUrl;
        /// <summary>
        /// Private member to hold information that can be used to contact the owner of the podcast.
        /// </summary>
        private ITunesOwner extensionOwner;
        /// <summary>
        /// Private member to hold a brief synopsis of the podcast.
        /// </summary>
        private string extensionSubtitle                                    = String.Empty;
        /// <summary>
        /// Private member to hold the full description of the podcast.
        /// </summary>
        private string extensionSummary                                     = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region ITunesSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="ITunesSyndicationExtensionContext"/> class.
        /// </summary>
        public ITunesSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Author
        /// <summary>
        /// Gets or sets the name of the artist of this podcast.
        /// </summary>
        /// <value>The name of the artist of this podcast.</value>
        public string Author
        {
            get
            {
                return extensionAuthor;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    extensionAuthor = String.Empty;
                }
                else
                {
                    extensionAuthor = value.Trim();
                }
            }
        }
        #endregion

        #region Categories
        /// <summary>
        /// Gets the categories to which this podcast belongs.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="ITunesCategory"/> objects that represent the categories to which this podcast belongs. The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<ITunesCategory> Categories
        {
            get
            {
                if (extensionCategories == null)
                {
                    extensionCategories = new Collection<ITunesCategory>();
                }
                return extensionCategories;
            }
        }
        #endregion

        #region Duration
        /// <summary>
        /// Gets or sets the total duration of this podcast.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> that represents total duration of this podcast. The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no duration was specified.</value>
        public TimeSpan Duration
        {
            get
            {
                return extensionDuration;
            }

            set
            {
                extensionDuration = value;
            }
        }
        #endregion

        #region ExplicitMaterial
        /// <summary>
        /// Gets or sets the explicit language or adult content advisory information for this podcast.
        /// </summary>
        /// <value>
        ///     An <see cref="ITunesExplicitMaterial"/> enumeration value that indicates whether the podcast contains explicit material. 
        ///     The default value is <see cref="ITunesExplicitMaterial.None"/>.
        /// </value>
        public ITunesExplicitMaterial ExplicitMaterial
        {
            get
            {
                return extensionExplicitMaterialDesignation;
            }

            set
            {
                extensionExplicitMaterialDesignation = value;
            }
        }
        #endregion

        #region Image
        /// <summary>
        /// Gets or sets a URL that points to the album artwork for this podcast.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents a URL that points to the album artwork for this podcast.</value>
        /// <remarks>
        ///     iTunes recommends the use of square images that are at least 600 by 600 pixels. 
        ///     iTunes supports images in <i>JPEG</i> and <i>PNG</i> formats. 
        ///     The URL <b>must</b> end in ".jpg" or ".png".
        /// </remarks>
        public Uri Image
        {
            get
            {
                return extensionImage;
            }

            set
            {
                extensionImage = value;
            }
        }
        #endregion

        #region IsBlocked
        /// <summary>
        /// Gets or sets a value indicating if this podcast is blocked from appearing in the iTunes Podcast directory.
        /// </summary>
        /// <value><b>true</b> if this podcast is blocked from appearing in the iTunes Podcast directory; otherwise <b>false</b>. The default value is <b>false</b>.</value>
        public bool IsBlocked
        {
            get
            {
                return extensionIsBlocked;
            }

            set
            {
                extensionIsBlocked = value;
            }
        }
        #endregion

        #region Keywords
        /// <summary>
        /// Gets the search keywords for this podcast.
        /// </summary>
        /// <value>A <see cref="Collection{T}"/> collection of strings that allows users to search on a maximum of 12 text keywords.</value>
        public Collection<string> Keywords
        {
            get
            {
                if (extensionKeywords == null)
                {
                    extensionKeywords = new Collection<string>();
                }
                return extensionKeywords;
            }
        }
        #endregion

        #region NewFeedUrl
        /// <summary>
        /// Gets or sets the URL where this podcast feed has been relocated to.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL where this podcast feed has been relocated to.</value>
        /// <remarks>
        ///     It is recommended that you should maintain the old feed for 48 hours before retiring it. At that point, iTunes will have updated the directory with the new feed URL.
        /// </remarks>
        public Uri NewFeedUrl
        {
            get
            {
                return extensionNewFeedUrl;
            }

            set
            {
                extensionNewFeedUrl = value;
            }
        }
        #endregion

        #region Owner
        /// <summary>
        /// Gets or sets information that can be used to contact the owner of this podcast.
        /// </summary>
        /// <value>
        ///     A <see cref="ITunesOwner"/> object that represents information that can be used to contact the owner of this podcast. 
        ///     The default value is a <b>null</b> reference.
        /// </value>
        public ITunesOwner Owner
        {
            get
            {
                return extensionOwner;
            }

            set
            {
                extensionOwner = value;
            }
        }
        #endregion

        #region Subtitle
        /// <summary>
        /// Gets or sets a brief synopsis of this podcast.
        /// </summary>
        /// <value>A brief synopsis of this podcast.</value>
        /// <remarks>
        ///     It is recommended that the subtitle is only a few words long.
        /// </remarks>
        public string Subtitle
        {
            get
            {
                return extensionSubtitle;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionSubtitle = String.Empty;
                }
                else
                {
                    extensionSubtitle = value.Trim();
                }
            }
        }
        #endregion

        #region Summary
        /// <summary>
        /// Gets or sets the full description of this podcast.
        /// </summary>
        /// <value>The full description of this podcast.</value>
        public string Summary
        {
            get
            {
                return extensionSummary;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    extensionSummary = String.Empty;
                }
                else
                {
                    extensionSummary = value.Trim();
                }
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
            if(this.LoadCommon(source, manager))
            {
                wasLoaded   = true;
            }

            if (this.LoadOptionals(source, manager))
            {
                wasLoaded   = true;
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
            if (this.NewFeedUrl != null)
            {
                writer.WriteElementString("new-feed-url", xmlNamespace, this.NewFeedUrl.ToString());
            }

            if (!String.IsNullOrEmpty(this.Subtitle))
            {
                writer.WriteElementString("subtitle", xmlNamespace, this.Subtitle);
            }

            if (!String.IsNullOrEmpty(this.Author))
            {
                writer.WriteElementString("author", xmlNamespace, this.Author);
            }

            if (!String.IsNullOrEmpty(this.Summary))
            {
                writer.WriteElementString("summary", xmlNamespace, this.Summary);
            }

            if (this.Owner != null)
            {
                this.Owner.WriteTo(writer);
            }

            if (this.Image != null)
            {
                writer.WriteStartElement("image", xmlNamespace);
                writer.WriteAttributeString("href", this.Image.ToString());
                writer.WriteEndElement();
            }

            if (this.Duration != TimeSpan.MinValue)
            {
                string hours    = this.Duration.Hours < 10 ? String.Concat("0", this.Duration.Hours.ToString(NumberFormatInfo.InvariantInfo)) : this.Duration.Hours.ToString(NumberFormatInfo.InvariantInfo);
                string minutes  = this.Duration.Minutes < 10 ? String.Concat("0", this.Duration.Minutes.ToString(NumberFormatInfo.InvariantInfo)) : this.Duration.Minutes.ToString(NumberFormatInfo.InvariantInfo);
                string seconds  = this.Duration.Seconds < 10 ? String.Concat("0", this.Duration.Seconds.ToString(NumberFormatInfo.InvariantInfo)) : this.Duration.Seconds.ToString(NumberFormatInfo.InvariantInfo);
                string duration = String.Format(null, "{0}:{1}:{2}", hours, minutes, seconds);

                writer.WriteElementString("duration", xmlNamespace, duration);
            }

            if (this.Keywords.Count > 0)
            {
                string[] keywords   = new string[this.Keywords.Count];
                this.Keywords.CopyTo(keywords, 0);

                writer.WriteElementString("keywords", xmlNamespace, String.Join(",", keywords));
            }

            if (this.ExplicitMaterial != ITunesExplicitMaterial.None)
            {
                writer.WriteElementString("explicit", xmlNamespace, ITunesSyndicationExtension.ExplicitMaterialAsString(this.ExplicitMaterial));
            }

            if (this.IsBlocked)
            {
                writer.WriteElementString("block", xmlNamespace, "yes");
            }

            if (this.Categories.Count > 0)
            {
                foreach(ITunesCategory category in this.Categories)
                {
                    category.WriteTo(writer);
                }
            }
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region LoadCommon(XPathNavigator source, XmlNamespaceManager manager)
        /// <summary>
        /// Initializes the common syndication extension information using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadCommon(XPathNavigator source, XmlNamespaceManager manager)
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
                XPathNavigator authorNavigator      = source.SelectSingleNode("itunes:author", manager);
                XPathNavigator keywordsNavigator    = source.SelectSingleNode("itunes:keywords", manager);
                XPathNavigator newFeedUrlNavigator  = source.SelectSingleNode("itunes:new-feed-url", manager);
                XPathNavigator ownerNavigator       = source.SelectSingleNode("itunes:owner", manager);
                XPathNavigator subtitleNavigator    = source.SelectSingleNode("itunes:subtitle", manager);
                XPathNavigator summaryNavigator     = source.SelectSingleNode("itunes:summary", manager);

                XPathNodeIterator categoryIterator  = source.Select("itunes:category", manager);

                if (authorNavigator != null && !String.IsNullOrEmpty(authorNavigator.Value))
                {
                    this.Author = authorNavigator.Value;
                    wasLoaded   = true;
                }

                if (keywordsNavigator != null && !String.IsNullOrEmpty(keywordsNavigator.Value))
                {
                    if(keywordsNavigator.Value.Contains(","))
                    {
                        string[] keywords   = keywordsNavigator.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string keyword in keywords)
                        {
                            this.Keywords.Add(keyword);
                            wasLoaded   = true;
                        }
                    }
                    else
                    {
                        this.Keywords.Add(keywordsNavigator.Value);
                        wasLoaded   = true;
                    }
                }

                if (newFeedUrlNavigator != null)
                {
                    Uri newFeedUrl;
                    if (Uri.TryCreate(newFeedUrlNavigator.Value, UriKind.RelativeOrAbsolute, out newFeedUrl))
                    {
                        this.NewFeedUrl = newFeedUrl;
                        wasLoaded       = true;
                    }
                }

                if (ownerNavigator != null)
                {
                    ITunesOwner owner   = new ITunesOwner();
                    if (owner.Load(ownerNavigator))
                    {
                        this.Owner  = owner;
                        wasLoaded   = true;
                    }
                }

                if (subtitleNavigator != null && !String.IsNullOrEmpty(subtitleNavigator.Value))
                {
                    this.Subtitle   = subtitleNavigator.Value;
                    wasLoaded       = true;
                }

                if (summaryNavigator != null && !String.IsNullOrEmpty(summaryNavigator.Value))
                {
                    this.Summary    = summaryNavigator.Value;
                    wasLoaded       = true;
                }

                if (categoryIterator != null && categoryIterator.Count > 0)
                {
                    while (categoryIterator.MoveNext())
                    {
                        ITunesCategory category = new ITunesCategory();
                        if (category.Load(categoryIterator.Current))
                        {
                            this.Categories.Add(category);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
        /// <summary>
        /// Initializes the optional syndication extension information using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
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
                XPathNavigator blockNavigator       = source.SelectSingleNode("itunes:block", manager);
                XPathNavigator imageNavigator       = source.SelectSingleNode("itunes:image", manager);
                XPathNavigator durationNavigator    = source.SelectSingleNode("itunes:duration", manager);
                XPathNavigator explicitNavigator    = source.SelectSingleNode("itunes:explicit", manager);

                if (blockNavigator != null && !String.IsNullOrEmpty(blockNavigator.Value))
                {
                    if(String.Compare(blockNavigator.Value, "yes", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.IsBlocked  = true;
                        wasLoaded       = true;
                    }
                    else if (String.Compare(blockNavigator.Value, "no", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.IsBlocked  = false;
                        wasLoaded       = true;
                    }
                }

                if (imageNavigator != null && imageNavigator.HasAttributes)
                {
                    string hrefAttribute    = imageNavigator.GetAttribute("href", String.Empty);
                    if (!String.IsNullOrEmpty(hrefAttribute))
                    {
                        Uri image;

                        if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out image))
                        {
                            this.Image  = image;
                            wasLoaded   = true;
                        }
                    }
                }

                if (durationNavigator != null && !String.IsNullOrEmpty(durationNavigator.Value))
                {
                    TimeSpan duration   = ITunesSyndicationExtensionContext.ParseDuration(durationNavigator.Value);
                    if (duration != TimeSpan.MinValue)
                    {
                        this.Duration   = duration;
                        wasLoaded       = true;
                    }
                }

                if (explicitNavigator != null && !String.IsNullOrEmpty(explicitNavigator.Value))
                {
                    ITunesExplicitMaterial explicitMaterial = ITunesSyndicationExtension.ExplicitMaterialByName(explicitNavigator.Value.Trim());
                    if (explicitMaterial != ITunesExplicitMaterial.None)
                    {
                        this.ExplicitMaterial   = explicitMaterial;
                        wasLoaded               = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region ParseDuration(string value)
        /// <summary>
        /// Returns the <see cref="TimeSpan"/> represented by the supplied duration value.
        /// </summary>
        /// <param name="value">The ITunes duration representation of the podcast duration.</param>
        /// <returns>A <see cref="TimeSpan"/> that represents the duration. If unable to determine duration, returns <see cref="TimeSpan.MinValue"/>.</returns>
        /// <remarks>Value can be formatted as an integer, HH:MM:SS, H:MM:SS, MM:SS, or M:SS.</remarks>
        private static TimeSpan ParseDuration(string value)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            TimeSpan timeSpan   = TimeSpan.MinValue;

            //------------------------------------------------------------
            //	Parse duration representation
            //------------------------------------------------------------
            if (!value.Contains(":"))
            {
                int totalSeconds;
                if (Int32.TryParse(value, out totalSeconds))
                {
                    timeSpan    = new TimeSpan(0, 0, totalSeconds);
                }
                else
                {
                    TimeSpan duration;
                    if (TimeSpan.TryParse(value, out duration))
                    {
                        timeSpan    = duration;
                    }
                }
            }
            else
            {
                string[] durationParts  = value.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (durationParts.Length == 2)
                {
                    int minutes;
                    int seconds;

                    if (Int32.TryParse(durationParts[0], out minutes) && Int32.TryParse(durationParts[1], out seconds))
                    {
                        timeSpan    = new TimeSpan(0, minutes, seconds);
                    }
                }
                else if (durationParts.Length >= 3)
                {
                    int hours;
                    int minutes;
                    int seconds;

                    string hoursValue   = durationParts[0];
                    string minutesValue = durationParts[1];
                    string secondsValue = durationParts[2];

                    if (Int32.TryParse(hoursValue, out hours) && Int32.TryParse(minutesValue, out minutes) && Int32.TryParse(secondsValue, out seconds))
                    {
                        timeSpan    = new TimeSpan(hours, minutes, seconds);
                    }
                    else
                    {
                        TimeSpan duration;
                        if (TimeSpan.TryParse(value, out duration))
                        {
                            timeSpan    = duration;
                        }
                    }
                }
            }

            return timeSpan;
        }
        #endregion
    }
}
