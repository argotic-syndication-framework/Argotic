namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the relationship type for platform or restriction specifications in a sitemap video extension.
/// </summary>
/// <remarks>
///     <para>
///         This enumeration is used to indicate whether the associated platforms or country restrictions
///         should be allowed or denied access to the video.
///     </para>
/// </remarks>
public enum SitemapVideoRelationship
{
    /// <summary>
    /// The specified platforms or countries are allowed to access the video.
    /// </summary>
    Allow,

    /// <summary>
    /// The specified platforms or countries are denied access to the video.
    /// </summary>
    Deny
}