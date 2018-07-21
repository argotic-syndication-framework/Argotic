/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/04/2008	brian.kuhn	Created FeedSynchronizationRelatedInformationType Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the type of feed being related.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Publishers will generally include, in a feed, only the most recent modifications, additions, and deletions within some reasonable time window. 
    ///         These feeds are referred to as <i>partial feeds</i>, whereas feeds containing the complete set of items are referred to as <i>complete feeds</i>.
    ///     </para>
    /// </remarks>
    /// <seealso cref="FeedSynchronizationRelatedInformation.RelationType"/>
    /// <seealso cref="FeedSynchronizationRelatedInformation"/>
    [Serializable()]
    public enum FeedSynchronizationRelatedInformationType
    {
        /// <summary>
        /// No relation type specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None        = 0,

        /// <summary>
        /// The <see cref="FeedSynchronizationRelatedInformation.Link"/> points to a feed whose contents are being incorporated into this feed by the publisher.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Aggregated", AlternateValue = "aggregated")]
        Aggregated  = 1,

        /// <summary>
        /// The <see cref="FeedSynchronizationRelatedInformation.Link"/> points to a feed that contains the complete collection of items for this feed.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Complete", AlternateValue = "complete")]
        Complete    = 2
    }
}