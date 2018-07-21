/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
12/11/2007	brian.kuhn	Created BlogMLContentType Class
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Syndication.Specialized
{
    /// <summary>
    /// Represents the entity encoding utilized by textual content constructs.
    /// </summary>
    [Serializable()]
    public enum BlogMLContentType
    {
        /// <summary>
        /// No content type specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Indicates that the textual content is base-64 encoded.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Base64", AlternateValue = "base64")]
        Base64  = 1,

        /// <summary>
        /// Indicates that the textual content is Hyper-Text Markup Language (HTML) encoded.
        /// </summary>
        [EnumerationMetadata(DisplayName = "HTML", AlternateValue = "html")]
        Html    = 2,

        /// <summary>
        /// Indicates that the textual content is not encoded per a specific entity scheme.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Text", AlternateValue = "text")]
        Text    = 3,

        /// <summary>
        /// Indicates that the textual content is Extensible Hyper-Text Markup Language (XHTML) encoded.
        /// </summary>
        [EnumerationMetadata(DisplayName = "XHTML", AlternateValue = "xhtml")]
        Xhtml   = 4
    }
}