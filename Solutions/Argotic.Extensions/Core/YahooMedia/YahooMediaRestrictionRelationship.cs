using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Indicates the type of relationship that a restriction represents.
/// </summary>
/// <seealso cref="YahooMediaRestriction"/>
public enum YahooMediaRestrictionRelationship
{
    /// <summary>
    /// No restriction relationship specified.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The entity list is the set that is permitted; everything else is denied.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Allow", AlternateValue = "allow")]
    Allow = 1,

    /// <summary>
    /// The entity list is the set that is denied; everything else is permitted.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Deny", AlternateValue = "deny")]
    Deny = 2
}