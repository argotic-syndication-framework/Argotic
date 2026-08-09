namespace Argotic.Extensions.Core;

/// <summary>
/// The platforms named by a <c>video:platform</c> element.
/// </summary>
/// <remarks>
///     A set, not a single choice: <c>video:platform</c> holds a space-delimited list, which maps onto
///     these flags. The set on its own says nothing — <see cref="SitemapVideo.PlatformRelationship"/>
///     decides whether it is the list of platforms permitted or the list blocked, and reading one without
///     the other inverts the meaning.
/// </remarks>
/// <seealso cref="SitemapVideo.Platform"/>
[Flags]
public enum SitemapVideoPlatform
{
    /// <summary>
    /// No platform. Set when the element was present but named nothing recognisable; suppresses the element on write.
    /// </summary>
    None = 0,

    /// <summary>
    /// Desktop and laptop browsers. Written as <c>web</c>.
    /// </summary>
    Web = 1,

    /// <summary>
    /// Phones and tablets. Written as <c>mobile</c>.
    /// </summary>
    Mobile = 2,

    /// <summary>
    /// Television devices. Written as <c>tv</c>.
    /// </summary>
    Tv = 4
}