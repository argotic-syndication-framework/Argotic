using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the approval status of a web log entity.
/// </summary>
/// <remarks>
///     Whether the entity was published or was held back by moderation. It matters most on comments and
///     trackbacks, where an export carries the moderation queue alongside what was live, and an importer that
///     ignores this republishes both.
/// </remarks>
public enum BlogMLApprovalStatus
{
    /// <summary>
    /// No approval status specified. Nothing is written for it, and nothing should be inferred from it.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Indicates that the web log entity is approved.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Approved", AlternateValue = "true")]
    Approved = 1,

    /// <summary>
    /// Indicates that the web log entity is not approved.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Not Approved", AlternateValue = "false")]
    NotApproved = 2
}