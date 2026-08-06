using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents how an iTunes podcast's episodes are meant to be consumed.
/// </summary>
/// <remarks>
///     A channel-level element from Apple's 2017 revision of the podcasting specification. It tells a
///     client whether to present the newest episode first — <see cref="Episodic"/>, the default a client
///     assumes — or to present the series from its beginning, which is what <see cref="Serial"/> asks
///     for.
/// </remarks>
/// <seealso cref="ITunesSyndicationExtensionContext.PodcastType"/>
public enum ITunesPodcastType
{
    /// <summary>
    /// No podcast type specified.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Episodes stand on their own and are presented newest first.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Episodic", AlternateValue = "episodic")]
    Episodic = 1,

    /// <summary>
    /// Episodes are meant to be heard in order, and are presented oldest first.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Serial", AlternateValue = "serial")]
    Serial = 2,
}