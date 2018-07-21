/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/16/2008	brian.kuhn	Created BlogMLPostType Class
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Syndication.Specialized
{
    /// <summary>
    /// Represents the permissible types of a web log post.
    /// </summary>
    [Serializable()]
    public enum BlogMLPostType
    {
        /// <summary>
        /// No post type specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Indicates that the post represents an article.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Article", AlternateValue = "article")]
        Article = 1,

        /// <summary>
        /// Indicates that the post represents web log entry.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Normal", AlternateValue = "normal")]
        Normal  = 2
    }
}