/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/30/2008	brian.kuhn	Created FeedHistoryLinkRelationType Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the link relation type of a web resource.
    /// </summary>
    /// <seealso cref="FeedHistoryLinkRelation.RelationType"/>
    [Serializable()]
    public enum FeedHistoryLinkRelationType
    {
        /// <summary>
        /// No link relation type specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None            = 0,

        /// <summary>
        /// Refers to a document containing the most recent entries.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Current", AlternateValue = "current")]
        Current         = 1,

        /// <summary>
        /// Refers to the furthest preceding document in a series of documents.
        /// </summary>
        [EnumerationMetadata(DisplayName = "First", AlternateValue = "first")]
        First           = 1,

        /// <summary>
        /// Refers to the furthest following document in a series of documents.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Last", AlternateValue = "last")]
        Last            = 2,

        /// <summary>
        /// Refers to the immediately following document in a series of documents.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Next", AlternateValue = "next")]
        Next            = 3,

        /// <summary>
        /// Refers to the immediately following archive document.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Next Archive", AlternateValue = "next-archive")]
        NextArchive     = 3,

        /// <summary>
        /// Refers to the immediately preceding document in a series of documents.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Previous", AlternateValue = "previous")]
        Previous        = 4,

        /// <summary>
        /// Refers to the immediately preceding archive document.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Previous Archive", AlternateValue = "prev-archive")]
        PreviousArchive = 5
    }
}