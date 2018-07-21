/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/04/2008	brian.kuhn	Created FeedSynchronizationTombstoneStatus Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents whether an item has been deleted and represents a tombstone.
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    /// <seealso cref="FeedSynchronizationItem.TombstoneStatus"/>
    /// <seealso cref="FeedSynchronizationItem"/>
    [Serializable()]
    public enum FeedSynchronizationTombstoneStatus
    {
        /// <summary>
        /// No deletion status specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// The item has been deleted.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Deleted", AlternateValue = "true")]
        Deleted  = 1,

        /// <summary>
        /// The item has not been deleted.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Present", AlternateValue = "false")]
        Present = 2
    }
}