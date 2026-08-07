namespace Argotic.Extensions.Core;

/// <summary>
/// Names the external system a <see cref="SitemapVideoId"/> draws its value from.
/// </summary>
/// <remarks>
///     These are the systems the 1.1 schema enumerates, and the set is closed: a <c>type</c> attribute
///     naming anything else reads as <see cref="None"/> and is dropped rather than preserved. Because the
///     schema fixes the list, a system added to the format later would need a new member here before this
///     library could round-trip it.
/// </remarks>
/// <seealso cref="SitemapVideoId.Type"/>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Google Video Sitemap 1.1 XSD Specification</seealso>
public enum SitemapVideoIdType
{
    /// <summary>
    /// No identifier type specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// A Tribune Media Services (TMS) series identifier.
    /// Maps to "tms:series" in the sitemap.
    /// </summary>
    TmsSeries = 1,

    /// <summary>
    /// A Tribune Media Services (TMS) program identifier.
    /// Maps to "tms:program" in the sitemap.
    /// </summary>
    TmsProgram = 2,

    /// <summary>
    /// A Rovi series identifier.
    /// Maps to "rovi:series" in the sitemap.
    /// </summary>
    RoviSeries = 3,

    /// <summary>
    /// A Rovi program identifier.
    /// Maps to "rovi:program" in the sitemap.
    /// </summary>
    RoviProgram = 4,

    /// <summary>
    /// A Freebase identifier.
    /// Maps to "freebase" in the sitemap.
    /// </summary>
    Freebase = 5,

    /// <summary>
    /// A URL-based identifier.
    /// Maps to "url" in the sitemap.
    /// </summary>
    Url = 6
}