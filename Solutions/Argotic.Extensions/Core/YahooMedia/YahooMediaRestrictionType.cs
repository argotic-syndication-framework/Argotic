/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/07/2008	brian.kuhn	Created YahooMediaRestrictionType Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Indicates the type of media that a restriction applies to.
    /// </summary>
    /// <seealso cref="YahooMediaRestriction"/>
    [Serializable()]
    public enum YahooMediaRestrictionType
    {
        /// <summary>
        /// No restriction type specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Indicates that restrictions are be placed based on country code.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Country", AlternateValue = "country")]
        Country = 1,

        /// <summary>
        /// Indicates that restrictions are be placed based on URI.
        /// </summary>
        [EnumerationMetadata(DisplayName = "URI", AlternateValue = "uri")]
        Uri    = 2
    }
}