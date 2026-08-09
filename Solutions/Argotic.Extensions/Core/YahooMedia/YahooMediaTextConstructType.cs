using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the entity encoding utilized by human-readable text constructs.
/// </summary>
/// <seealso cref="YahooMediaTextConstruct"/>
public enum YahooMediaTextConstructType
{
    /// <summary>
    /// No entity-encoding type specified. The specification's default when the attribute is absent is <see cref="Plain"/>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Indicates that the human-readable text is Hyper-Text Markup Language (HTML) encoded.
    /// </summary>
    [EnumerationMetadata(DisplayName = "HTML", AlternateValue = "html")]
    Html = 1,

    /// <summary>
    /// Indicates that the human-readable text is not encoded per a specific entity scheme.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Plain Text", AlternateValue = "plain")]
    Plain = 2
}