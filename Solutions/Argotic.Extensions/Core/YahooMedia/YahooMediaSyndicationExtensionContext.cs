/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/23/2008	brian.kuhn	Created YahooMediaSyndicationExtensionContext Class
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Encapsulates specific information about an individual <see cref="YahooMediaSyndicationExtension"/>.
    /// </summary>
    [Serializable()]
    public class YahooMediaSyndicationExtensionContext : IYahooMediaCommonObjectEntities
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the collection of items that comprise the distinct content published in the feed.
        /// </summary>
        private IEnumerable<YahooMediaContent> extensionContents;
        /// <summary>
        /// Private member to hold the collection of media objects that are effectively the same content, yet different representations.
        /// </summary>
        private IEnumerable<YahooMediaGroup> extensionGroups;
        /// <summary>
        /// Private member to hold the permissible audiences for the syndication entity.
        /// </summary>
        private Collection<YahooMediaRating> mediaObjectRatings;
        /// <summary>
        /// Private member to hold the title of the syndication entity.
        /// </summary>
        private YahooMediaTextConstruct mediaObjectTitle;
        /// <summary>
        /// Private member to hold a short description of the syndication entity.
        /// </summary>
        private YahooMediaTextConstruct mediaObjectDescription;
        /// <summary>
        /// Private member to hold the relevant keywords that describe the syndication entity.
        /// </summary>
        private Collection<string> mediaObjectKeywords;
        /// <summary>
        /// Private member to hold the representative images for the syndication entity.
        /// </summary>
        private Collection<YahooMediaThumbnail> mediaObjectThumbnails;
        /// <summary>
        /// Private member to hold a taxonomy that gives an indication of the type of content for the syndication entity.
        /// </summary>
        private Collection<YahooMediaCategory> mediaObjectCategories;
        /// <summary>
        /// Private member to hold the hash digests for the syndication entity.
        /// </summary>
        private Collection<YahooMediaHash> mediaObjectHashes;
        /// <summary>
        /// Private member to hold a web browser media player console the syndication entity can be accessed through.
        /// </summary>
        private YahooMediaPlayer mediaObjectPlayer;
        /// <summary>
        /// Private member to hold the entities that contributed to the creation of the syndication entity.
        /// </summary>
        private Collection<YahooMediaCredit> mediaObjectCredits;
        /// <summary>
        /// Private member to hold the copyright information for the syndication entity.
        /// </summary>
        private YahooMediaCopyright mediaObjectCopyright;
        /// <summary>
        /// Private member to hold the text transcript, closed captioning, or lyrics for the syndication entity.
        /// </summary>
        private Collection<YahooMediaText> mediaObjectTextSeries;
        /// <summary>
        /// Private member to hold the restrictions to be placed on aggregators that are rendering the syndication entity.
        /// </summary>
        private Collection<YahooMediaRestriction> mediaObjectRestrictions;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region YahooMediaSyndicationExtensionContext()
        /// <summary>
        /// Initializes a new instance of the <see cref="YahooMediaSyndicationExtensionContext"/> class.
        /// </summary>
        public YahooMediaSyndicationExtensionContext()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Contents
        /// <summary>
        /// Gets or sets the publishable media objects.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="YahooMediaContent"/> objects that represent publishable media objects.</value>
        /// <remarks>
        ///     <para>
        ///         The sequence of <see cref="YahooMediaContent"/> objects within a syndication entity implies the order of presentation.
        ///     </para>
        ///     <para>
        ///         This <see cref="IEnumerable{T}"/> collection of <see cref="YahooMediaContent"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<YahooMediaContent> Contents
        {
            get
            {
                if (extensionContents == null)
                {
                    extensionContents = new Collection<YahooMediaContent>();
                }
                return extensionContents;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionContents = value;
            }
        }
        #endregion

        #region Groups
        /// <summary>
        /// Gets or sets the media objects that are effectively the same content, yet different representations.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="YahooMediaGroup"/> objects that represent effectively the same content, yet different representations.</value>
        /// <remarks>
        ///     <para>
        ///         Media objects that are not the same content should not be included in the same <see cref="YahooMediaGroup"/>. 
        ///         The sequence of <see cref="YahooMediaContent"/> objects within a <see cref="YahooMediaGroup"/> implies the order of presentation.
        ///     </para>
        ///     <para>
        ///         This <see cref="IEnumerable{T}"/> collection of <see cref="YahooMediaGroup"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<YahooMediaGroup> Groups
        {
            get
            {
                if (extensionGroups == null)
                {
                    extensionGroups = new Collection<YahooMediaGroup>();
                }
                return extensionGroups;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                extensionGroups = value;
            }
        }
        #endregion

        //============================================================
        //	SECONDARY PUBLIC PROPERTIES
        //============================================================
        #region Categories
        /// <summary>
        /// Gets a taxonomy that gives an indication of the type of content for this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCategory"/> objects that represent a taxonomy that gives an indication to the type of content for this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<YahooMediaCategory> Categories
        {
            get
            {
                if (mediaObjectCategories == null)
                {
                    mediaObjectCategories = new Collection<YahooMediaCategory>();
                }
                return mediaObjectCategories;
            }
        }
        #endregion

        #region Copyright
        /// <summary>
        /// Gets or sets the copyright information for this syndication entity.
        /// </summary>
        /// <value>A <see cref="YahooMediaCopyright"/> that represents the copyright information for this syndication entity.</value>
        /// <remarks>
        ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
        /// </remarks>
        public YahooMediaCopyright Copyright
        {
            get
            {
                return mediaObjectCopyright;
            }

            set
            {
                mediaObjectCopyright = value;
            }
        }
        #endregion

        #region Credits
        /// <summary>
        /// Gets the entities that contributed to the creation of this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCredit"/> objects that represent the entities that contributed to the creation of this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Current entities can include people, companies, locations, etc. Specific entities can have multiple roles, 
        ///     and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
        /// </remarks>
        public Collection<YahooMediaCredit> Credits
        {
            get
            {
                if (mediaObjectCredits == null)
                {
                    mediaObjectCredits = new Collection<YahooMediaCredit>();
                }
                return mediaObjectCredits;
            }
        }
        #endregion

        #region Description
        /// <summary>
        /// Gets or sets the description of this syndication entity.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents a short description of this syndication entity.</value>
        /// <remarks>
        ///     Media object descriptions are typically a sentence in length.
        /// </remarks>
        public YahooMediaTextConstruct Description
        {
            get
            {
                return mediaObjectDescription;
            }

            set
            {
                mediaObjectDescription = value;
            }
        }
        #endregion

        #region Hashes
        /// <summary>
        /// Gets the hash digests for this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaHash"/> objects that represent the hash digests for this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     When assigning multiple hashes, each <see cref="YahooMediaHash"/> <b>must</b> have a different <see cref="YahooMediaHash.Algorithm"/>.
        /// </remarks>
        public Collection<YahooMediaHash> Hashes
        {
            get
            {
                if (mediaObjectHashes == null)
                {
                    mediaObjectHashes = new Collection<YahooMediaHash>();
                }
                return mediaObjectHashes;
            }
        }
        #endregion

        #region Keywords
        /// <summary>
        /// Gets the relevant keywords that describe this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="String"/> objects that represent the relevant keywords that describe this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Media objects are typically assigned maximum of ten keywords or phrases.
        /// </remarks>
        public Collection<string> Keywords
        {
            get
            {
                if (mediaObjectKeywords == null)
                {
                    mediaObjectKeywords = new Collection<string>();
                }
                return mediaObjectKeywords;
            }
        }
        #endregion

        #region Player
        /// <summary>
        /// Gets or sets a web browser media player console this syndication entity can be accessed through.
        /// </summary>
        /// <value>A <see cref="YahooMediaPlayer"/> that represents a web browser media player console this syndication entity can be accessed through.</value>
        public YahooMediaPlayer Player
        {
            get
            {
                return mediaObjectPlayer;
            }

            set
            {
                mediaObjectPlayer = value;
            }
        }
        #endregion

        #region Ratings
        /// <summary>
        /// Gets the permissible audiences for this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRating"/> objects that represent the permissible audiences for this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If there are no ratings specified, it can be assumed that no restrictions are necessary.
        /// </remarks>
        public Collection<YahooMediaRating> Ratings
        {
            get
            {
                if (mediaObjectRatings == null)
                {
                    mediaObjectRatings = new Collection<YahooMediaRating>();
                }
                return mediaObjectRatings;
            }
        }
        #endregion

        #region Restrictions
        /// <summary>
        /// Gets the restrictions to be placed on aggregators that are rendering this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRestriction"/> objects that represent restrictions to be placed on aggregators that are rendering this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        public Collection<YahooMediaRestriction> Restrictions
        {
            get
            {
                if (mediaObjectRestrictions == null)
                {
                    mediaObjectRestrictions = new Collection<YahooMediaRestriction>();
                }
                return mediaObjectRestrictions;
            }
        }
        #endregion

        #region TextSeries
        /// <summary>
        /// Gets the text transcript, closed captioning, or lyrics for this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaText"/> objects that represent text transcript, closed captioning, or lyrics for this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Many of these <see cref="YahooMediaText"/> objects are permitted to provide a time series of text. 
        ///     In such cases, it is encouraged, but not required, that the <see cref="YahooMediaText"/> objects be grouped by language and appear in time sequence order based on the start time. 
        ///     <see cref="YahooMediaText"/> objects can have overlapping start and end times.
        /// </remarks>
        public Collection<YahooMediaText> TextSeries
        {
            get
            {
                if (mediaObjectTextSeries == null)
                {
                    mediaObjectTextSeries = new Collection<YahooMediaText>();
                }
                return mediaObjectTextSeries;
            }
        }
        #endregion

        #region Thumbnails
        /// <summary>
        /// Gets the representative images for this syndication entity.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaThumbnail"/> objects that represent images that are representative of this syndication entity. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If multiple thumbnails are included, and time coding is not at play, it is assumed that the images are in order of importance.
        /// </remarks>
        public Collection<YahooMediaThumbnail> Thumbnails
        {
            get
            {
                if (mediaObjectThumbnails == null)
                {
                    mediaObjectThumbnails = new Collection<YahooMediaThumbnail>();
                }
                return mediaObjectThumbnails;
            }
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets or sets the title of this syndication entity.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents the title of this syndication entity.</value>
        public YahooMediaTextConstruct Title
        {
            get
            {
                return mediaObjectTitle;
            }

            set
            {
                mediaObjectTitle = value;
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
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="YahooMediaSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
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
                XPathNodeIterator contentIterator   = source.Select("media:content", manager);
                XPathNodeIterator groupIterator     = source.Select("media:group", manager);

                if (contentIterator != null && contentIterator.Count > 0)
                {
                    while (contentIterator.MoveNext())
                    {
                        YahooMediaContent content   = new YahooMediaContent();
                        if (content.Load(contentIterator.Current))
                        {
                            this.AddContent(content);
                            wasLoaded   = true;
                        }
                    }
                }

                if (groupIterator != null && groupIterator.Count > 0)
                {
                    while (groupIterator.MoveNext())
                    {
                        YahooMediaGroup group   = new YahooMediaGroup();
                        if (group.Load(groupIterator.Current))
                        {
                            this.AddGroup(group);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            if (YahooMediaUtility.FillCommonObjectEntities(this, source))
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
            foreach(YahooMediaContent content in this.Contents)
            {
                content.WriteTo(writer);
            }

            foreach (YahooMediaGroup group in this.Groups)
            {
                group.WriteTo(writer);
            }

            YahooMediaUtility.WriteCommonObjectEntities(this, writer);
        }
        #endregion

        //============================================================
        //	UTILITY METHODS
        //============================================================
        #region AddContent(YahooMediaContent content)
        /// <summary>
        /// Adds the supplied <see cref="YahooMediaContent"/> to the current instance's <see cref="Contents"/> collection.
        /// </summary>
        /// <param name="content">The <see cref="YahooMediaContent"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaContent"/> was added to the <see cref="Contents"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddContent(YahooMediaContent content)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasAdded   = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(content, "content");

            //------------------------------------------------------------
            //	Add media object to collection
            //------------------------------------------------------------
            ((Collection<YahooMediaContent>)this.Contents).Add(content);
            wasAdded    = true;

            return wasAdded;
        }
        #endregion

        #region AddGroup(YahooMediaGroup group)
        /// <summary>
        /// Adds the supplied <see cref="YahooMediaGroup"/> to the current instance's <see cref="Groups"/> collection.
        /// </summary>
        /// <param name="group">The <see cref="YahooMediaGroup"/> to be added.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaGroup"/> was added to the <see cref="Groups"/> collection, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="group"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool AddGroup(YahooMediaGroup group)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasAdded = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(group, "group");

            //------------------------------------------------------------
            //	Add media group to collection
            //------------------------------------------------------------
            ((Collection<YahooMediaGroup>)this.Groups).Add(group);
            wasAdded = true;

            return wasAdded;
        }
        #endregion

        #region RemoveContent(YahooMediaContent content)
        /// <summary>
        /// Removes the supplied <see cref="YahooMediaContent"/> from the current instance's <see cref="Contents"/> collection.
        /// </summary>
        /// <param name="content">The <see cref="YahooMediaContent"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaContent"/> was removed from the <see cref="Contents"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Contents"/> collection of the current instance does not contain the specified <see cref="YahooMediaContent"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveContent(YahooMediaContent content)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasRemoved = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(content, "content");

            //------------------------------------------------------------
            //	Remove media object from collection
            //------------------------------------------------------------
            if (((Collection<YahooMediaContent>)this.Contents).Contains(content))
            {
                ((Collection<YahooMediaContent>)this.Contents).Remove(content);
                wasRemoved  = true;
            }

            return wasRemoved;
        }
        #endregion

        #region RemoveGroup(YahooMediaGroup content)
        /// <summary>
        /// Removes the supplied <see cref="YahooMediaGroup"/> from the current instance's <see cref="Groups"/> collection.
        /// </summary>
        /// <param name="group">The <see cref="YahooMediaGroup"/> to be removed.</param>
        /// <returns><b>true</b> if the <see cref="YahooMediaGroup"/> was removed from the <see cref="Groups"/> collection, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     If the <see cref="Groups"/> collection of the current instance does not contain the specified <see cref="YahooMediaGroup"/>, will return <b>false</b>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="group"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool RemoveGroup(YahooMediaGroup group)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasRemoved = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(group, "group");

            //------------------------------------------------------------
            //	Remove media group from collection
            //------------------------------------------------------------
            if (((Collection<YahooMediaGroup>)this.Groups).Contains(group))
            {
                ((Collection<YahooMediaGroup>)this.Groups).Remove(group);
                wasRemoved = true;
            }

            return wasRemoved;
        }
        #endregion
    }
}
