using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the expressed version of a media object.
/// </summary>
/// <remarks>
///     <para>
///     The alternate values are the three tokens the Media RSS specification defines for
///     <c>media:content/@expression</c>: <c>sample</c>, <c>full</c> and <c>nonstop</c>.
///     </para>
///     <para>
///     <b>Every member used to declare <c>AlternateValue = ""</c></b>, which broke the attribute in
///     both directions and silently. <c>ExpressionByName</c> resolves through
///     <see cref="EnumerationMetadataAttribute.GetEnumByAlternateValue{TEnum}"/>, so with no distinct
///     token to match it returned <see cref="None"/> for every input — and <c>YahooMediaContent.Load</c>
///     assigns only when the result is not <see cref="None"/>, so a conformant
///     <c>expression="full"</c> was read and discarded. The write side emitted <c>expression=""</c>
///     for the same reason. A sweep of all 23 metadata-bearing enums in the product assemblies found
///     this to be the only one affected.
///     </para>
/// </remarks>
/// <seealso cref="YahooMediaTextConstruct"/>
public enum YahooMediaExpression
{
    /// <summary>
    /// No media expression specified.
    /// </summary>
    /// <remarks>
    ///     The empty alternate value is deliberate here and only here: this member means "the document
    ///     did not say", so it has no token to match and must not be written back out.
    /// </remarks>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The media object represents the full version.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Full", AlternateValue = "full")]
    Full = 1,

    /// <summary>
    /// The media object represents a continuous stream.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Nonstop", AlternateValue = "nonstop")]
    Nonstop = 2,

    /// <summary>
    /// The media object represents a sample version.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Sample", AlternateValue = "sample")]
    Sample = 3
}