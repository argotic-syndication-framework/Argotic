/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/09/2007	brian.kuhn	Created IYahooMediaCommonObjectEntities Interface
****************************************************************************/
using System;
using System.Collections.ObjectModel;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Allows an object to implement common Yahoo media entities by representing a set of properties, methods, indexers and events common to Yahoo media syndication resources.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///          The following properties are optional and may appear as sub-elements of syndication entities, <see cref="YahooMediaContent"/> and/or <see cref="YahooMediaGroup"/>.
    ///     </para>
    ///     <para>
    ///         When a property appears at a shallow level, such as syndication entities, it means that the property should be applied to every media object within its scope. 
    ///         Duplicated properties appearing at deeper levels of the document tree have higher priority over other levels. For example, <see cref="YahooMediaContent"/> level 
    ///         properties are favored over syndication entity level properties.
    ///     </para>
    ///     <para>
    ///         The priority level is listed from strongest to weakest: <see cref="YahooMediaContent"/>, <see cref="YahooMediaGroup"/>, syndication item entity, syndication feed.
    ///     </para>
    /// </remarks>
    /// <seealso cref="YahooMediaContent"/>
    /// <seealso cref="YahooMediaGroup"/>
    /// <seealso cref="YahooMediaSyndicationExtensionContext"/>
    interface IYahooMediaCommonObjectEntities
    {
        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Categories
        /// <summary>
        /// Gets a taxonomy that gives an indication of the type of content for the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCategory"/> objects that represent a taxonomy that gives an indication to the type of content for the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        Collection<YahooMediaCategory> Categories
        {
            get;
        }
        #endregion

        #region Copyright
        /// <summary>
        /// Gets or sets the copyright information for the media object.
        /// </summary>
        /// <value>A <see cref="YahooMediaCopyright"/> that represents the copyright information for the media object.</value>
        /// <remarks>
        ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
        /// </remarks>
        YahooMediaCopyright Copyright
        {
            get;
            set;
        }
        #endregion

        #region Credits
        /// <summary>
        /// Gets the entities that contributed to the creation of the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaCredit"/> objects that represent the entities that contributed to the creation of the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Current entities can include people, companies, locations, etc. Specific entities can have multiple roles, 
        ///     and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
        /// </remarks>
        Collection<YahooMediaCredit> Credits
        {
            get;
        }
        #endregion

        #region Description
        /// <summary>
        /// Gets or sets the description of the media object.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents a short description of the media object.</value>
        /// <remarks>
        ///     Media object descriptions are typically a sentence in length.
        /// </remarks>
        YahooMediaTextConstruct Description
        {
            get;
            set;
        }
        #endregion

        #region Hashes
        /// <summary>
        /// Gets the hash digests for the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaHash"/> objects that represent the hash digests for the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     When assigning multiple hashes, each <see cref="YahooMediaHash"/> <b>must</b> have a different <see cref="YahooMediaHash.Algorithm"/>.
        /// </remarks>
        Collection<YahooMediaHash> Hashes
        {
            get;
        }
        #endregion

        #region Keywords
        /// <summary>
        /// Gets the relevant keywords that describe the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="String"/> objects that represent the relevant keywords that describe the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Media objects are typically assigned maximum of ten keywords or phrases.
        /// </remarks>
        Collection<string> Keywords
        {
            get;
        }
        #endregion

        #region Player
        /// <summary>
        /// Gets or sets a web browser media player console the media object can be accessed through.
        /// </summary>
        /// <value>A <see cref="YahooMediaPlayer"/> that represents a web browser media player console the media object can be accessed through.</value>
        YahooMediaPlayer Player
        {
            get;
            set;
        }
        #endregion

        #region Ratings
        /// <summary>
        /// Gets the permissible audiences for the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRating"/> objects that represent the permissible audiences for the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If there are no ratings specified, it can be assumed that no restrictions are necessary.
        /// </remarks>
        Collection<YahooMediaRating> Ratings
        {
            get;
        }
        #endregion

        #region Restrictions
        /// <summary>
        /// Gets the restrictions to be placed on aggregators that are rendering the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaRestriction"/> objects that represent restrictions to be placed on aggregators that are rendering the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        Collection<YahooMediaRestriction> Restrictions
        {
            get;
        }
        #endregion

        #region TextSeries
        /// <summary>
        /// Gets the text transcript, closed captioning, or lyrics for the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaText"/> objects that represent text transcript, closed captioning, or lyrics for the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     Many of these <see cref="YahooMediaText"/> objects are permitted to provide a time series of text. 
        ///     In such cases, it is encouraged, but not required, that the <see cref="YahooMediaText"/> objects be grouped by language and appear in time sequence order based on the start time. 
        ///     <see cref="YahooMediaText"/> objects can have overlapping start and end times.
        /// </remarks>
        Collection<YahooMediaText> TextSeries
        {
            get;
        }
        #endregion

        #region Thumbnails
        /// <summary>
        /// Gets the representative images for the media object.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="YahooMediaThumbnail"/> objects that represent images that are representative of the media object. 
        ///     The default value is an <i>empty</i> collection.
        /// </value>
        /// <remarks>
        ///     If multiple thumbnails are included, and time coding is not at play, it is assumed that the images are in order of importance.
        /// </remarks>
        Collection<YahooMediaThumbnail> Thumbnails
        {
            get;
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets or sets the title of the media object.
        /// </summary>
        /// <value>A <see cref="YahooMediaTextConstruct"/> that represents the title of the media object.</value>
        YahooMediaTextConstruct Title
        {
            get;
            set;
        }
        #endregion
    }
}
