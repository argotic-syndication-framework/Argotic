using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// How often a page in a sitemap is likely to change.
/// </summary>
/// <remarks>
///     <para>
///         <i>This is a hint, not a command.</i> The protocol is explicit that crawlers "may crawl pages
///         marked <c>hourly</c> less frequently than that, and they may crawl pages marked <c>yearly</c>
///         more frequently than that", and that they "may periodically crawl pages marked <c>never</c> so
///         that they can handle unexpected changes to those pages". A sitemap cannot be used to schedule a
///         crawler, only to inform one.
///     </para>
///     <para>
///         The seven members are the seven values the protocol permits; there is no member for an absent
///         element. <see cref="SitemapUrl.ChangeFrequency"/> is nullable for that reason. Beware that the
///         zero value of this enumeration is <see cref="Always"/>, so a default-initialised
///         <see cref="SitemapChangeFrequency"/> claims the most aggressive frequency there is rather than
///         no frequency at all.
///     </para>
/// </remarks>
public enum SitemapChangeFrequency
{
    /// <summary>
    /// The document changes each time it is accessed. Reserved by the protocol for exactly that case, not as a way to ask for frequent crawling.
    /// </summary>
    [EnumerationMetadata(DisplayName = "always", AlternateValue = "always")]
    Always = 0,

    /// <summary>
    /// The document changes hourly.
    /// </summary>
    [EnumerationMetadata(DisplayName = "hourly", AlternateValue = "hourly")]
    Hourly = 1,

    /// <summary>
    /// The document changes daily.
    /// </summary>
    [EnumerationMetadata(DisplayName = "daily", AlternateValue = "daily")]
    Daily = 2,

    /// <summary>
    /// The document changes weekly.
    /// </summary>
    [EnumerationMetadata(DisplayName = "weekly", AlternateValue = "weekly")]
    Weekly = 3,

    /// <summary>
    /// The document changes monthly.
    /// </summary>
    [EnumerationMetadata(DisplayName = "monthly", AlternateValue = "monthly")]
    Monthly = 4,

    /// <summary>
    /// The document changes yearly.
    /// </summary>
    [EnumerationMetadata(DisplayName = "yearly", AlternateValue = "yearly")]
    Yearly = 5,

    /// <summary>
    /// The document is archived and will not change. Crawlers may still revisit it periodically.
    /// </summary>
    [EnumerationMetadata(DisplayName = "never", AlternateValue = "never")]
    Never = 6
}