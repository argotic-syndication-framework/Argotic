using System.Globalization;
using Argotic.Extensions.Core;
using Argotic.Publishing;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins the hash code of every type whose comparison folds an absent language and the invariant
/// culture together.
/// </summary>
/// <remarks>
///     <para>
///         These comparisons normalise with <c>Language?.Name ?? string.Empty</c>. The invariant
///         culture's <see cref="CultureInfo.Name"/> <i>is</i> the empty string, so an absent language
///         and the invariant culture both become <c>""</c> and compare equal — which is right: XML 1.0
///         §2.12 gives <c>xml:lang=""</c> the meaning "no language information", the same thing the
///         attribute's absence means.
///     </para>
///     <para>
///         The hash codes did not agree. Three types folded <c>Component(this.Language)</c>, the
///         identity overload, so two different <see cref="CultureInfo"/> references hashed differently;
///         three folded <c>Component(this.Language?.Name)</c>, where <see langword="null"/> hashes to
///         <c>0</c> and <c>""</c> hashes to something else. Equal with different hash codes is a lookup
///         miss. All six now route through <c>HashCodeUtility.Component(CultureInfo)</c>, which
///         normalises the same way the comparison does.
///     </para>
///     <para>
///         This is the family §4.3 of <c>docs/build-warnings.md</c> hunted and missed: it looked for
///         collections folded by reference and whole context objects passed to the identity overload,
///         never for a <c>?? string.Empty</c> present in the comparison and absent from the hash.
///     </para>
/// </remarks>
[TestClass]
public class LanguageHashContractTests
{
    /// <summary>
    /// An absent language and the invariant culture compare equal, so they must hash equally; and a
    /// real language must still separate two instances, or the pin would pass on a type that had
    /// stopped consulting the member at all.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="build">Builds an instance carrying the supplied language.</param>
    private static void ShouldHashAnAbsentLanguageAsTheInvariantCulture<T>(Func<CultureInfo?, T> build)
        where T : class
    {
        T absent = build(null);
        T invariant = build(CultureInfo.InvariantCulture);
        T named = build(new CultureInfo("en-GB"));

        absent.Equals(invariant).ShouldBeTrue();
        absent.GetHashCode().ShouldBe(invariant.GetHashCode());

        // The control: the member is genuinely consulted, so the pin above is not vacuous.
        absent.Equals(named).ShouldBeFalse();
    }

    [TestMethod]
    public void AtomLink_HashesAnAbsentContentLanguageAsTheInvariantCulture() =>
        ShouldHashAnAbsentLanguageAsTheInvariantCulture(language => new AtomLink { ContentLanguage = language });

    [TestMethod]
    public void AtomCategoryDocument_HashesAnAbsentLanguageAsTheInvariantCulture() =>
        ShouldHashAnAbsentLanguageAsTheInvariantCulture(language => new AtomCategoryDocument { Language = language });

    [TestMethod]
    public void AtomWorkspace_HashesAnAbsentLanguageAsTheInvariantCulture() =>
        ShouldHashAnAbsentLanguageAsTheInvariantCulture(language => new AtomWorkspace { Language = language });

    [TestMethod]
    public void AtomAcceptedMediaRange_HashesAnAbsentLanguageAsTheInvariantCulture() =>
        ShouldHashAnAbsentLanguageAsTheInvariantCulture(language => new AtomAcceptedMediaRange { Language = language });

    [TestMethod]
    public void YahooMediaContent_HashesAnAbsentLanguageAsTheInvariantCulture() =>
        ShouldHashAnAbsentLanguageAsTheInvariantCulture(language => new YahooMediaContent { Language = language });

    [TestMethod]
    public void YahooMediaText_HashesAnAbsentLanguageAsTheInvariantCulture() =>
        ShouldHashAnAbsentLanguageAsTheInvariantCulture(language => new YahooMediaText { Language = language });
}