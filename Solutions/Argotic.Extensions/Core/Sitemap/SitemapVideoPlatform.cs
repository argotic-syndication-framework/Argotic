namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the platforms on which a video can be played.
/// </summary>
/// <remarks>
///     <para>
///         This enumeration is used to specify which platforms are allowed or denied for video playback
///         in a sitemap video extension. Multiple platforms can be combined using bitwise operations.
///     </para>
/// </remarks>
[Flags]
[Serializable]
public enum SitemapVideoPlatform
{
    /// <summary>
    /// No platform specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// The video can be played on traditional desktop and laptop computers.
    /// </summary>
    Web = 1,

    /// <summary>
    /// The video can be played on mobile devices such as phones and tablets.
    /// </summary>
    Mobile = 2,

    /// <summary>
    /// The video can be played on TV platforms.
    /// </summary>
    Tv = 4
}