using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers the two enumerations a video's platform restriction is built from:
/// <see cref="SitemapVideoPlatform"/>, whose members are combinable flags, and
/// <see cref="SitemapVideoRelationship"/>, which supplies the allow-or-deny sense.
/// </summary>
[TestClass]
public class SitemapVideoPlatformTests
{
    #region SitemapVideoPlatform Enum Tests

    /// <summary>
    /// <c>None</c> is <c>0</c>, so an unset platform restriction combines with anything without changing it.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_None_HasValueZero() =>
        // Assert
        ((int)SitemapVideoPlatform.None).ShouldBe(0);

    /// <summary>
    /// <c>Web</c> is <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_Web_HasValueOne() =>
        // Assert
        ((int)SitemapVideoPlatform.Web).ShouldBe(1);

    /// <summary>
    /// <c>Mobile</c> is <c>2</c>.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_Mobile_HasValueTwo() =>
        // Assert
        ((int)SitemapVideoPlatform.Mobile).ShouldBe(2);

    /// <summary>
    /// <c>Tv</c> is <c>4</c>.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_Tv_HasValueFour() =>
        // Assert
        ((int)SitemapVideoPlatform.Tv).ShouldBe(4);

    /// <summary>
    /// The enumeration carries exactly one <see cref="FlagsAttribute"/>, which is what makes a combined
    /// value legal and formattable as a list of names.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_IsFlagsEnum()
    {
        // Arrange
        var type = typeof(SitemapVideoPlatform);

        // Assert
        type.GetCustomAttributes(typeof(FlagsAttribute), false).Length.ShouldBe(1);
    }

    /// <summary>
    /// <c>Web | Mobile</c> reports both of those flags set and <c>Tv</c> clear.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_CanCombineWebAndMobile()
    {
        // Arrange
        SitemapVideoPlatform combined = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile;

        // Assert
        combined.HasFlag(SitemapVideoPlatform.Web).ShouldBeTrue();
        combined.HasFlag(SitemapVideoPlatform.Mobile).ShouldBeTrue();
        combined.HasFlag(SitemapVideoPlatform.Tv).ShouldBeFalse();
    }

    /// <summary>
    /// All three platform flags together are <c>7</c>, and each reads back as set.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_CanCombineAllPlatforms()
    {
        // Arrange
        SitemapVideoPlatform allPlatforms = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile | SitemapVideoPlatform.Tv;

        // Assert
        allPlatforms.HasFlag(SitemapVideoPlatform.Web).ShouldBeTrue();
        allPlatforms.HasFlag(SitemapVideoPlatform.Mobile).ShouldBeTrue();
        allPlatforms.HasFlag(SitemapVideoPlatform.Tv).ShouldBeTrue();
        ((int)allPlatforms).ShouldBe(7);
    }

    /// <summary>
    /// A combination cast to <see cref="int"/> and back is the same combination, so the numeric form is
    /// lossless.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_CombinedValue_CanBeParsedBack()
    {
        // Arrange
        SitemapVideoPlatform combined = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile;
        int numericValue = (int)combined;

        // Act
        SitemapVideoPlatform parsed = (SitemapVideoPlatform)numericValue;

        // Assert
        parsed.ShouldBe(combined);
        parsed.HasFlag(SitemapVideoPlatform.Web).ShouldBeTrue();
        parsed.HasFlag(SitemapVideoPlatform.Mobile).ShouldBeTrue();
    }

    /// <summary>
    /// <c>None</c> reports every platform flag clear.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_None_DoesNotHaveAnyPlatformFlags()
    {
        // Arrange
        SitemapVideoPlatform none = SitemapVideoPlatform.None;

        // Assert
        none.HasFlag(SitemapVideoPlatform.Web).ShouldBeFalse();
        none.HasFlag(SitemapVideoPlatform.Mobile).ShouldBeFalse();
        none.HasFlag(SitemapVideoPlatform.Tv).ShouldBeFalse();
    }

    /// <summary>
    /// Masking a flag out of a combination leaves the remaining flag set and the removed one clear.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_BitwiseOperations_WorkCorrectly()
    {
        // Arrange
        SitemapVideoPlatform webAndMobile = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile;

        // Act - Remove Mobile
        SitemapVideoPlatform webOnly = webAndMobile & ~SitemapVideoPlatform.Mobile;

        // Assert
        webOnly.ShouldBe(SitemapVideoPlatform.Web);
        webOnly.HasFlag(SitemapVideoPlatform.Mobile).ShouldBeFalse();
    }

    /// <summary>
    /// The three platform values are successive powers of two — <c>1</c>, <c>2</c> and <c>4</c> — so no
    /// combination collides with a single member.
    /// </summary>
    [TestMethod]
    public void SitemapVideoPlatform_ValuesArePowersOfTwo()
    {
        // Assert - Each value should be a power of 2 (except None which is 0)
        ((int)SitemapVideoPlatform.Web).ShouldBe(1);    // 2^0
        ((int)SitemapVideoPlatform.Mobile).ShouldBe(2); // 2^1
        ((int)SitemapVideoPlatform.Tv).ShouldBe(4);     // 2^2
    }

    #endregion

    #region SitemapVideoRelationship Enum Tests

    /// <summary>
    /// <c>Allow</c> is a member of the relationship enumeration.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_Allow_Exists()
    {
        // Arrange & Act
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Allow;

        // Assert
        relationship.ShouldBe(SitemapVideoRelationship.Allow);
    }

    /// <summary>
    /// <c>Deny</c> is a member of the relationship enumeration.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_Deny_Exists()
    {
        // Arrange & Act
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Deny;

        // Assert
        relationship.ShouldBe(SitemapVideoRelationship.Deny);
    }

    /// <summary>
    /// The relationship enumeration has exactly two members, so a restriction is allow or deny and
    /// nothing else.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_HasTwoValues()
    {
        // Arrange
        var values = Enum.GetValues<SitemapVideoRelationship>();

        // Assert
        values.Length.ShouldBe(2);
    }

    /// <summary>
    /// <c>Allow</c> and <c>Deny</c> are distinct values.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_AllowAndDeny_AreDifferent() =>
        // Assert
        SitemapVideoRelationship.Allow.ShouldNotBe(SitemapVideoRelationship.Deny);

    /// <summary>
    /// <c>Allow</c> takes the allow arm of a switch over the enumeration rather than falling to the discard.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_CanBeUsedInSwitch()
    {
        // Arrange
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Allow;

        // Act
        string result = relationship switch
        {
            SitemapVideoRelationship.Allow => "allow",
            SitemapVideoRelationship.Deny => "deny",
            _ => "unknown"
        };

        // Assert
        result.ShouldBe("allow");
    }

    /// <summary>
    /// <c>Deny</c> takes the deny arm of the same switch.
    /// </summary>
    [TestMethod]
    public void SitemapVideoRelationship_DenyCanBeUsedInSwitch()
    {
        // Arrange
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Deny;

        // Act
        string result = relationship switch
        {
            SitemapVideoRelationship.Allow => "allow",
            SitemapVideoRelationship.Deny => "deny",
            _ => "unknown"
        };

        // Assert
        result.ShouldBe("deny");
    }

    #endregion

    #region Combined Usage Tests

    /// <summary>
    /// Read the way a consumer would read them, <c>Web | Mobile</c> under <c>Allow</c> permits web and
    /// does not permit TV.
    /// </summary>
    [TestMethod]
    public void CombinedUsage_PlatformWithRelationship_WorksTogether()
    {
        // This test demonstrates how the enums would be used together
        // in a video sitemap extension

        // Arrange
        SitemapVideoPlatform platforms = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile;
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Allow;

        // Act - Simulate determining if a platform is allowed
        bool isWebAllowed = platforms.HasFlag(SitemapVideoPlatform.Web) && relationship == SitemapVideoRelationship.Allow;
        bool isTvAllowed = platforms.HasFlag(SitemapVideoPlatform.Tv) && relationship == SitemapVideoRelationship.Allow;

        // Assert
        isWebAllowed.ShouldBeTrue();
        isTvAllowed.ShouldBeFalse();
    }

    /// <summary>
    /// <c>Web</c> under <c>Deny</c> reads as the web platform being denied.
    /// </summary>
    [TestMethod]
    public void CombinedUsage_DenyRelationship_WorksCorrectly()
    {
        // Arrange
        SitemapVideoPlatform platforms = SitemapVideoPlatform.Web;
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Deny;

        // Act - If relationship is Deny, the platform should be denied, not allowed
        bool isWebDenied = platforms.HasFlag(SitemapVideoPlatform.Web) && relationship == SitemapVideoRelationship.Deny;

        // Assert
        isWebDenied.ShouldBeTrue();
    }

    #endregion
}