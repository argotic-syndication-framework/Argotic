using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the explicit language or adult content advisory information of an iTunes podcast.
/// </summary>
/// <remarks>
///     Three members rather than a <see cref="bool"/> because the element has never had two states.
///     Apple's original vocabulary was <c>yes</c> / <c>no</c> / <c>clean</c> and its current one is
///     <c>true</c> / <c>false</c>; the booleans are a respelling of <see cref="Yes"/> and
///     <see cref="No"/>, and <see cref="Clean"/> is the third answer neither of them covers.
///     <see cref="ITunesSyndicationExtension.ExplicitMaterialByName"/> reads all five spellings.
/// </remarks>
/// <seealso cref="ITunesSyndicationExtensionContext.ExplicitMaterial"/>
public enum ITunesExplicitMaterial
{
    /// <summary>
    /// The publisher gave no advisory. This is the absent case, not a declaration that the content is clean.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// The podcast is declared free of explicit language and adult content, using the third value of Apple's original vocabulary. Apple's current one has no equivalent, and 889 of the 4,770 advisories in the 136-document corpus still use it.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Clean", AlternateValue = "clean")]
    Clean = 1,

    /// <summary>
    /// The publisher declares the podcast free of explicit language and adult content. Spelled <c>no</c> by Apple's original vocabulary and <c>false</c> by its current one; both read as this.
    /// </summary>
    [EnumerationMetadata(DisplayName = "No", AlternateValue = "no")]
    No = 2,

    /// <summary>
    /// The publisher declares that the podcast contains explicit language or adult content. Spelled <c>yes</c> by Apple's original vocabulary and <c>true</c> by its current one; both read as this.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Yes", AlternateValue = "yes")]
    Yes = 3
}