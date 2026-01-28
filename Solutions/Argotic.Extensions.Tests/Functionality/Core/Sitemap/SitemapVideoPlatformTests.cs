using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Unit tests for <see cref="SitemapVideoPlatform"/> and <see cref="SitemapVideoRelationship"/>.
/// </summary>
[TestClass]
public class SitemapVideoPlatformTests
{
    #region SitemapVideoPlatform Enum Tests

    [TestMethod]
    public void SitemapVideoPlatform_None_HasValueZero()
    {
        // Assert
        ((int)SitemapVideoPlatform.None).ShouldBe(0);
    }

    [TestMethod]
    public void SitemapVideoPlatform_Web_HasValueOne()
    {
        // Assert
        ((int)SitemapVideoPlatform.Web).ShouldBe(1);
    }

    [TestMethod]
    public void SitemapVideoPlatform_Mobile_HasValueTwo()
    {
        // Assert
        ((int)SitemapVideoPlatform.Mobile).ShouldBe(2);
    }

    [TestMethod]
    public void SitemapVideoPlatform_Tv_HasValueFour()
    {
        // Assert
        ((int)SitemapVideoPlatform.Tv).ShouldBe(4);
    }

    [TestMethod]
    public void SitemapVideoPlatform_IsFlagsEnum()
    {
        // Arrange
        var type = typeof(SitemapVideoPlatform);

        // Assert
        type.GetCustomAttributes(typeof(FlagsAttribute), false).Length.ShouldBe(1);
    }

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

    [TestMethod]
    public void SitemapVideoRelationship_Allow_Exists()
    {
        // Arrange & Act
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Allow;

        // Assert
        relationship.ShouldBe(SitemapVideoRelationship.Allow);
    }

    [TestMethod]
    public void SitemapVideoRelationship_Deny_Exists()
    {
        // Arrange & Act
        SitemapVideoRelationship relationship = SitemapVideoRelationship.Deny;

        // Assert
        relationship.ShouldBe(SitemapVideoRelationship.Deny);
    }

    [TestMethod]
    public void SitemapVideoRelationship_HasTwoValues()
    {
        // Arrange
        var values = Enum.GetValues<SitemapVideoRelationship>();

        // Assert
        values.Length.ShouldBe(2);
    }

    [TestMethod]
    public void SitemapVideoRelationship_AllowAndDeny_AreDifferent()
    {
        // Assert
        SitemapVideoRelationship.Allow.ShouldNotBe(SitemapVideoRelationship.Deny);
    }

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