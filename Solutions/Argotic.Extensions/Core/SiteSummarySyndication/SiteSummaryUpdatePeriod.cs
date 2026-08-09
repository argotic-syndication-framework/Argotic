using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// The unit of time a feed's update frequency is counted against.
/// </summary>
/// <remarks>
///     The five periods the Syndication module defines, plus <see cref="None"/> for their absence. The
///     member on its own is only half the schedule — <see cref="SiteSummaryUpdateSyndicationExtensionContext.Frequency"/>
///     supplies the count, and <c>Hourly</c> with a frequency of 4 means four times an hour.
///     <para>
///     <b>The underlying values are alphabetical, not chronological.</b> <see cref="Daily"/> is 1 and
///     <see cref="Hourly"/> is 2, so ordering by the numeric value sorts a day before an hour. Never
///     derive a duration by comparing members; map them explicitly.
///     </para>
/// </remarks>
/// <seealso cref="SiteSummaryUpdateSyndicationExtensionContext.Period"/>
/// <seealso cref="SiteSummaryUpdateSyndicationExtensionContext"/>
public enum SiteSummaryUpdatePeriod
{
    /// <summary>
    /// No update period. Suppresses the element on write, and is what an unrecognised value reads as.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The feed is updated daily. Written as <c>daily</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Daily", AlternateValue = "daily")]
    Daily = 1,

    /// <summary>
    /// The feed is updated hourly. Written as <c>hourly</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Hourly", AlternateValue = "hourly")]
    Hourly = 2,

    /// <summary>
    /// The feed is updated monthly. Written as <c>monthly</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Monthly", AlternateValue = "monthly")]
    Monthly = 3,

    /// <summary>
    /// The feed is updated weekly. Written as <c>weekly</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Weekly", AlternateValue = "weekly")]
    Weekly = 4,

    /// <summary>
    /// The feed is updated yearly. Written as <c>yearly</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Yearly", AlternateValue = "yearly")]
    Yearly = 5
}