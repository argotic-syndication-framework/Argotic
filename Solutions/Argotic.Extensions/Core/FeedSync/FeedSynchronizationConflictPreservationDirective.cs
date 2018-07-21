/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/04/2008	brian.kuhn	Created FeedSynchronizationConflictPreservationDirective Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents whether conflict preservation should be performed for a <see cref="FeedSynchronizationItem"/>.
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    /// <seealso cref="FeedSynchronizationItem.ConflictPreservation"/>
    /// <seealso cref="FeedSynchronizationItem"/>
    [Serializable()]
    public enum FeedSynchronizationConflictPreservationDirective
    {
        /// <summary>
        /// No conflict preservation processing directive specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Conflict preservation <b>must not</b> be performed for the item.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Ignore", AlternateValue = "true")]
        Ignore  = 1,

        /// <summary>
        /// Conflict preservation <b>must</b> be performed for the item.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Perform", AlternateValue = "false")]
        Perform = 2
    }
}