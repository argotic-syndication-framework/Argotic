namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the type of video identifier used in a sitemap video extension.
/// </summary>
/// <remarks>
///     <para>
///         This enumeration is used to specify the type of identifier for a video in a sitemap video extension.
///         The identifier type indicates the source or format of the video ID.
///     </para>
/// </remarks>
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