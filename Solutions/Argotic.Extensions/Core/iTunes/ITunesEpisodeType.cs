using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the kind of episode an iTunes podcast item is.
/// </summary>
/// <remarks>
///     Introduced by Apple's 2017 revision of the podcasting specification, alongside
///     <c>itunes:episode</c>, <c>itunes:season</c>, <c>itunes:title</c> and <c>itunes:type</c>. It is
///     one of the most widely emitted elements in real podcast feeds — <b>4,695</b> occurrences across
///     the 136-document real-world corpus, of which 4,657 are <c>full</c>, 36 <c>bonus</c> and 2
///     <c>trailer</c>.
/// </remarks>
/// <seealso cref="ITunesSyndicationExtensionContext.EpisodeType"/>
public enum ITunesEpisodeType
{
    /// <summary>
    /// No episode type specified.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// A full episode of the podcast.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Full", AlternateValue = "full")]
    Full = 1,

    /// <summary>
    /// A short piece of promotional material for the podcast, a season, or a forthcoming episode.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Trailer", AlternateValue = "trailer")]
    Trailer = 2,

    /// <summary>
    /// Extra material published outside the podcast's regular run, such as an interview or a cross-promotion.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Bonus", AlternateValue = "bonus")]
    Bonus = 3,
}