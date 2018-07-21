/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/05/2008	brian.kuhn	Created SiteSummaryUpdatePeriod Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents the period over which the feed format is updated.
    /// </summary>
    /// <seealso cref="SiteSummaryUpdateSyndicationExtensionContext.Period"/>
    /// <seealso cref="SiteSummaryUpdateSyndicationExtensionContext"/>
    [Serializable()]
    public enum SiteSummaryUpdatePeriod
    {
        /// <summary>
        /// No update period specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None    = 0,

        /// <summary>
        /// Feed format is updated on a daily basis.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Daily", AlternateValue = "daily")]
        Daily   = 1,

        /// <summary>
        /// Feed format is updated on an hourly basis.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Hourly", AlternateValue = "hourly")]
        Hourly  = 2,

        /// <summary>
        /// Feed format is updated on a monthly basis.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Monthly", AlternateValue = "monthly")]
        Monthly = 3,

        /// <summary>
        /// Feed format is updated on a weekly basis.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Weekly", AlternateValue = "weekly")]
        Weekly  = 4,

        /// <summary>
        /// Feed format is updated on a yearly basis.
        /// </summary>
        [EnumerationMetadata(DisplayName = "Yearly", AlternateValue = "yearly")]
        Yearly  = 5
    }
}