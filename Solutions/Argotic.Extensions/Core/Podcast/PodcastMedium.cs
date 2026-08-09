using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents what the content of a podcast feed <i>is</i>, as opposed to what it is about.
/// </summary>
/// <remarks>
///     <para>
///     Podcasting 2.0's <c>podcast:medium</c>. A category says what a feed is about; this says what it
///     is, so an application can change its behaviour — resetting playback speed to 1× and adjusting
///     equalisation for <see cref="Music"/>, for instance.
///     </para>
///     <para>
///     The specification also defines a "list" counterpart for every value, spelled by suffixing the
///     letter <c>L</c> — <c>podcastL</c>, <c>musicL</c>, <c>audiobookL</c>. That is deliberately
///     <b>not</b> twenty enumeration members: it is this enumeration plus
///     <see cref="PodcastSyndicationExtensionContext.MediumIsList"/>, which is how the specification
///     itself describes it and which cannot drift out of step the way two parallel lists would.
///     </para>
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.Medium"/>
public enum PodcastMedium
{
    /// <summary>
    /// No medium specified.
    /// </summary>
    /// <remarks>
    ///     Distinct from <see cref="Podcast"/>. The specification says a feed with no <c>medium</c>
    ///     element is to be treated as a podcast, but that is a consumer's inference and not something
    ///     the publisher wrote — so it is not recorded as though it were.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// A podcast show. This is the value assumed when no medium is given.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Podcast", AlternateValue = "podcast")]
    Podcast = 1,

    /// <summary>
    /// Music organised into an album, each item a song within it.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Music", AlternateValue = "music")]
    Music = 2,

    /// <summary>
    /// A more visual experience than a podcast, akin to a dedicated video channel.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Video", AlternateValue = "video")]
    Video = 3,

    /// <summary>
    /// Video with one item per feed, as distinct from a video channel.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Film", AlternateValue = "film")]
    Film = 4,

    /// <summary>
    /// Audio with one item per feed, or items representing chapters of a book.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Audiobook", AlternateValue = "audiobook")]
    Audiobook = 5,

    /// <summary>
    /// Curated written articles.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Newsletter", AlternateValue = "newsletter")]
    Newsletter = 6,

    /// <summary>
    /// Informally written articles.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Blog", AlternateValue = "blog")]
    Blog = 7,

    /// <summary>
    /// A feed linking to the other feeds a publisher owns.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Publisher", AlternateValue = "publisher")]
    Publisher = 8,

    /// <summary>
    /// Training material, each item a lesson.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Course", AlternateValue = "course")]
    Course = 9,

    /// <summary>
    /// A feed of mixed content types.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Mixed", AlternateValue = "mixed")]
    Mixed = 10,
}