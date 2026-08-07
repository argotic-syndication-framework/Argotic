using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the entity encoding utilized by textual content constructs.
/// </summary>
/// <remarks>
///     A declaration about how to read a <see cref="BlogMLTextConstruct"/>'s text, not an instruction to this
///     library: nothing here decodes base-64 or sanitises markup on the strength of it.
/// </remarks>
public enum BlogMLContentType
{
    /// <summary>
    /// No content type specified. The consumer is left to guess how to read the text.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Indicates that the textual content is base-64 encoded.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Base64", AlternateValue = "base64")]
    Base64 = 1,

    /// <summary>
    /// Indicates that the textual content is Hyper-Text Markup Language (HTML) encoded.
    /// </summary>
    [EnumerationMetadata(DisplayName = "HTML", AlternateValue = "html")]
    Html = 2,

    /// <summary>
    /// Indicates that the textual content is not encoded per a specific entity scheme.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Text", AlternateValue = "text")]
    Text = 3,

    /// <summary>
    /// Indicates that the textual content is Extensible Hyper-Text Markup Language (XHTML) encoded.
    /// </summary>
    [EnumerationMetadata(DisplayName = "XHTML", AlternateValue = "xhtml")]
    Xhtml = 4
}