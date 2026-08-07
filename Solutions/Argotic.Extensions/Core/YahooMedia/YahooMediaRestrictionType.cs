using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Indicates what a restriction's entity list is a list of.
/// </summary>
/// <remarks>
///     The maintained specification defines a third value, <c>sharing</c>, which this enumeration does not
///     model; such a restriction reads as <see cref="None"/> and loses its type on save.
/// </remarks>
/// <seealso cref="YahooMediaRestriction"/>
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
    Uri = 2
}