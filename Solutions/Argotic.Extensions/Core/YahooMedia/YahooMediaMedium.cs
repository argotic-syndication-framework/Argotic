/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/08/2008	brian.kuhn	Created YahooMediaMedium Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the type of a media object .
    /// </summary>
    /// <seealso cref="YahooMediaTextConstruct"/>
    [Serializable()]
    public enum YahooMediaMedium
    {
        /// <summary>
        /// No object medium specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None        = 0,

        /// <summary>
        /// The media object represents a resource primarily intended to be heard.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Audio", AlternateValue = "audio")]
        Audio       = 1,

        /// <summary>
        /// The media object represents a resource consisting primarily of words for reading.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Document", AlternateValue = "document")]
        Document    = 2,

        /// <summary>
        /// The media object represents a computer program in source or compiled form.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Executable", AlternateValue = "executable")]
        Executable  = 3,

        /// <summary>
        /// The media object represents a visual representation.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Image", AlternateValue = "image")]
        Image       = 4,

        /// <summary>
        /// The media object represents a series of visual representations imparting an impression of motion when shown in succession.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Video", AlternateValue = "video")]
        Video       = 5
    }
}