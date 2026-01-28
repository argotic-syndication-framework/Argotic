using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Specifies the change frequency of a URL in a sitemap.
/// </summary>
/// <remarks>
///     <para>
///         The value "always" should be used to describe documents that change each time they are accessed.
///         The value "never" should be used to describe archived URLs.
///     </para>
///     <para>
///         Please note that the value of this tag is considered a hint and not a command.
///         Even though search engine crawlers may consider this information when making decisions,
///         they may crawl pages marked "hourly" less frequently than that, and they may crawl pages marked "yearly" more frequently than that.
///     </para>
/// </remarks>
[Serializable]
public enum SitemapChangeFrequency
{
    /// <summary>
    /// The document changes each time it is accessed.
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
    /// The document is an archived URL that will never change.
    /// </summary>
    [EnumerationMetadata(DisplayName = "never", AlternateValue = "never")]
    Never = 6
}