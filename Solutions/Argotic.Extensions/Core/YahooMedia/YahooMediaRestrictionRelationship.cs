/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/07/2008	brian.kuhn	Created YahooMediaRestrictionRelationship Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Indicates the type of relationship that a restriction represents.
    /// </summary>
    /// <seealso cref="YahooMediaRestriction"/>
    [Serializable()]
    public enum YahooMediaRestrictionRelationship
    {
        /// <summary>
        /// No restriction relationship specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Indicates that the type of relationship is permissive.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Allow", AlternateValue = "allow")]
        Allow    = 1,

        /// <summary>
        /// Indicates that the type of relationship is restrictive.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Deny", AlternateValue = "deny")]
        Deny    = 2
    }
}