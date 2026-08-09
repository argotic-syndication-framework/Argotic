using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Indicates what a restriction's entity list is a list of.
/// </summary>
/// <remarks>
///     These are the three values the maintained specification permits on the <c>type</c> attribute:
///     <see cref="Country"/>, <see cref="Uri"/> and <see cref="Sharing"/>. <see cref="None"/> is not one of
///     them — it stands for the attribute being absent, which the specification allows only for the reserved
///     entities <c>all</c> and <c>none</c>.
/// </remarks>
/// <seealso cref="YahooMediaRestriction"/>
/// <seealso href="https://www.rssboard.org/media-rss">Media RSS Specification</seealso>
public enum YahooMediaRestrictionType
{
    /// <summary>
    /// No restriction type specified. Legal only for the reserved entities <c>all</c> and <c>none</c>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The entities are ISO 3166 country codes.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Country", AlternateValue = "country")]
    Country = 1,

    /// <summary>
    /// The entities are distributor URIs.
    /// </summary>
    [EnumerationMetadata(DisplayName = "URI", AlternateValue = "uri")]
    Uri = 2,

    /// <summary>
    /// The restriction is on sharing rather than on an entity list.
    /// </summary>
    /// <remarks>
    ///     The specification's own gloss: <c>deny</c> means the content cannot be shared — via embed tags,
    ///     for example. The entity list carries the reserved <c>all</c> or <c>none</c> alongside it rather
    ///     than a list of countries or distributors.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "Sharing", AlternateValue = "sharing")]
    Sharing = 3
}