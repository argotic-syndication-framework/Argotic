using Argotic.Common;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents the permissible types of a web log post.
/// </summary>
public enum BlogMLPostType
{
    /// <summary>
    /// No post type specified. Nothing is written for it, and a post read with an unrecognised type lands here.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Indicates that the post represents an article — a standing page, such as an "about" page, rather than a dated entry.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Article", AlternateValue = "article")]
    Article = 1,

    /// <summary>
    /// Indicates that the post represents an ordinary dated web log entry.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Normal", AlternateValue = "normal")]
    Normal = 2
}