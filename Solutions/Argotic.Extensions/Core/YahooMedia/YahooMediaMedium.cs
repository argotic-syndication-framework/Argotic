using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the broad kind of a media object.
/// </summary>
/// <remarks>
///     The five values the Media RSS <c>medium</c> attribute defines. It says only what sort of thing this is;
///     <see cref="YahooMediaContent.ContentType"/> carries the media type that says exactly what it is.
/// </remarks>
/// <seealso cref="YahooMediaContent.Medium"/>
public enum YahooMediaMedium
{
    /// <summary>
    /// No object medium specified, or one this library does not recognise.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The media object represents a resource primarily intended to be heard.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Audio", AlternateValue = "audio")]
    Audio = 1,

    /// <summary>
    /// The media object represents a resource consisting primarily of words for reading.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Document", AlternateValue = "document")]
    Document = 2,

    /// <summary>
    /// The media object represents a computer program in source or compiled form.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Executable", AlternateValue = "executable")]
    Executable = 3,

    /// <summary>
    /// The media object represents a visual representation.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Image", AlternateValue = "image")]
    Image = 4,

    /// <summary>
    /// The media object represents a series of visual representations imparting an impression of motion when shown in succession.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Video", AlternateValue = "video")]
    Video = 5
}