namespace Argotic.Extensions.Core;

/// <summary>
/// Whether an accompanying platform or country list is an allow-list or a deny-list.
/// </summary>
/// <remarks>
///     The <c>relationship</c> attribute of <c>video:platform</c> and <c>video:restriction</c>. It carries
///     the whole polarity of the restriction, so a list read without it is not merely incomplete but
///     potentially backwards. Parsing is deliberately asymmetric: only the literal <c>allow</c> yields
///     <see cref="Allow"/>, and everything else — including a typo — yields <see cref="Deny"/>, so an
///     unreadable attribute errs towards withholding the video rather than publishing it.
/// </remarks>
/// <seealso cref="SitemapVideo.PlatformRelationship"/>
/// <seealso cref="SitemapVideo.RestrictionRelationship"/>
public enum SitemapVideoRelationship
{
    /// <summary>
    /// The listed platforms or countries are the only ones permitted. Written as <c>allow</c>.
    /// </summary>
    Allow,

    /// <summary>
    /// The listed platforms or countries are excluded; everywhere else is permitted. Written as <c>deny</c>.
    /// </summary>
    Deny
}