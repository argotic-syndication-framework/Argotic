/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/04/2008	brian.kuhn	Created LiveJournalSecurityType Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the access level type of a LiveJournal entry.
    /// </summary>
    /// <seealso cref="LiveJournalSecurity.Accessibility"/>
    /// <seealso cref="LiveJournalSecurity"/>
    [Serializable()]
    public enum LiveJournalSecurityType
    {
        /// <summary>
        /// No access level specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// The LiveJournal entry is accessible to friends of the author. 
        /// </summary>
        [EnumerationMetadata(DisplayName = "Friends", AlternateValue = "friends")]
        Friends = 1,

        /// <summary>
        /// The LiveJournal entry is accessible to the author.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Private", AlternateValue = "private")]
        Private = 2,

        /// <summary>
        /// The LiveJournal entry is publicly accessible. 
        /// </summary>
        [EnumerationMetadata(DisplayName = "Public", AlternateValue = "public")]
        Public  = 3
    }
}