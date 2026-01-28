using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Unit tests for <see cref="SitemapVideo"/>.
/// </summary>
[TestClass]
public class SitemapVideoTests
{
    #region Test Data

    private static readonly Uri TestThumbnailUri = new("https://example.com/thumbnail.jpg");
    private const string TestTitle = "Test Video Title";
    private const string TestDescription = "Test video description for unit testing.";

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_CreatesEmptyVideo()
    {
        // Arrange & Act
        SitemapVideo video = new();

        // Assert
        video.ThumbnailLocation.ShouldBeNull();
        video.Title.ShouldBe(string.Empty);
        video.Description.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Constructor_WithRequiredProperties_SetsThemCorrectly()
    {
        // Arrange & Act
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Assert
        video.ThumbnailLocation.ShouldBe(TestThumbnailUri);
        video.Title.ShouldBe(TestTitle);
        video.Description.ShouldBe(TestDescription);
    }

    [TestMethod]
    public void Constructor_WithNullThumbnail_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new SitemapVideo(null!, TestTitle, TestDescription));
    }

    [TestMethod]
    public void Constructor_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, null!, TestDescription));
    }

    [TestMethod]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, string.Empty, TestDescription));
    }

    [TestMethod]
    public void Constructor_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, TestTitle, null!));
    }

    [TestMethod]
    public void Constructor_WithEmptyDescription_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, TestTitle, string.Empty));
    }

    #endregion

    #region Title Validation Tests

    [TestMethod]
    public void Title_TruncatesToMaxTitleLength()
    {
        // Arrange
        string longTitle = new('A', 150);
        SitemapVideo video = new();

        // Act
        video.Title = longTitle;

        // Assert
        video.Title.Length.ShouldBe(SitemapVideo.MaxTitleLength);
    }

    [TestMethod]
    public void Title_TrimsWhitespace()
    {
        // Arrange
        string titleWithWhitespace = "  Test Title  ";
        SitemapVideo video = new();

        // Act
        video.Title = titleWithWhitespace;

        // Assert
        video.Title.ShouldBe("Test Title");
    }

    [TestMethod]
    public void MaxTitleLength_EqualsOneHundred()
    {
        // Assert
        SitemapVideo.MaxTitleLength.ShouldBe(100);
    }

    #endregion

    #region Description Validation Tests

    [TestMethod]
    public void Description_TruncatesToMaxDescriptionLength()
    {
        // Arrange
        string longDescription = new('D', 3000);
        SitemapVideo video = new();

        // Act
        video.Description = longDescription;

        // Assert
        video.Description.Length.ShouldBe(SitemapVideo.MaxDescriptionLength);
    }

    [TestMethod]
    public void Description_TrimsWhitespace()
    {
        // Arrange
        string descriptionWithWhitespace = "  Test Description  ";
        SitemapVideo video = new();

        // Act
        video.Description = descriptionWithWhitespace;

        // Assert
        video.Description.ShouldBe("Test Description");
    }

    [TestMethod]
    public void MaxDescriptionLength_EqualsTwoThousandFortyEight()
    {
        // Assert
        SitemapVideo.MaxDescriptionLength.ShouldBe(2048);
    }

    #endregion

    #region Duration and Rating Constant Tests

    [TestMethod]
    public void MinDuration_EqualsOne()
    {
        // Assert
        SitemapVideo.MinDuration.ShouldBe(1);
    }

    [TestMethod]
    public void MaxDuration_EqualsTwentyEightThousandEightHundred()
    {
        // Assert
        SitemapVideo.MaxDuration.ShouldBe(28800);
    }

    [TestMethod]
    public void MinRating_EqualsZeroPointZero()
    {
        // Assert
        SitemapVideo.MinRating.ShouldBe(0.0m);
    }

    [TestMethod]
    public void MaxRating_EqualsFivePointZero()
    {
        // Assert
        SitemapVideo.MaxRating.ShouldBe(5.0m);
    }

    #endregion

    #region Tag Tests

    [TestMethod]
    public void MaxTagCount_EqualsThirtyTwo()
    {
        // Assert
        SitemapVideo.MaxTagCount.ShouldBe(32);
    }

    [TestMethod]
    public void Tags_ReturnsEmptyListByDefault()
    {
        // Arrange
        SitemapVideo video = new();

        // Act & Assert
        video.Tags.ShouldNotBeNull();
        video.Tags.ShouldBeEmpty();
    }

    [TestMethod]
    public void Tags_CanAddTags()
    {
        // Arrange
        SitemapVideo video = new();

        // Act
        video.Tags.Add("tag1");
        video.Tags.Add("tag2");

        // Assert
        video.Tags.Count.ShouldBe(2);
        video.Tags.ShouldContain("tag1");
        video.Tags.ShouldContain("tag2");
    }

    #endregion

    #region Boolean Property Default Tests

    [TestMethod]
    public void FamilyFriendly_DefaultsToTrue()
    {
        // Arrange
        SitemapVideo video = new();

        // Assert
        video.FamilyFriendly.ShouldBeTrue();
    }

    [TestMethod]
    public void RequiresSubscription_DefaultsToFalse()
    {
        // Arrange
        SitemapVideo video = new();

        // Assert
        video.RequiresSubscription.ShouldBeFalse();
    }

    [TestMethod]
    public void Live_DefaultsToFalse()
    {
        // Arrange
        SitemapVideo video = new();

        // Assert
        video.Live.ShouldBeFalse();
    }

    #endregion

    #region Uploader Tests

    [TestMethod]
    public void Uploader_TruncatesToMaxUploaderLength()
    {
        // Arrange
        string longUploader = new('U', 300);
        SitemapVideo video = new();

        // Act
        video.Uploader = longUploader;

        // Assert
        video.Uploader.Length.ShouldBe(SitemapVideo.MaxUploaderLength);
    }

    [TestMethod]
    public void Uploader_TrimsWhitespace()
    {
        // Arrange
        string uploaderWithWhitespace = "  Test Uploader  ";
        SitemapVideo video = new();

        // Act
        video.Uploader = uploaderWithWhitespace;

        // Assert
        video.Uploader.ShouldBe("Test Uploader");
    }

    [TestMethod]
    public void MaxUploaderLength_EqualsTwoHundredFiftyFive()
    {
        // Assert
        SitemapVideo.MaxUploaderLength.ShouldBe(255);
    }

    [TestMethod]
    public void Uploader_NullValue_SetsEmptyString()
    {
        // Arrange
        SitemapVideo video = new() { Uploader = "Initial Value" };

        // Act
        video.Uploader = null!;

        // Assert
        video.Uploader.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Uploader_EmptyValue_SetsEmptyString()
    {
        // Arrange
        SitemapVideo video = new() { Uploader = "Initial Value" };

        // Act
        video.Uploader = string.Empty;

        // Assert
        video.Uploader.ShouldBe(string.Empty);
    }

    #endregion

    #region Equality and Comparison Tests

    [TestMethod]
    public void Equals_ReturnsTrueForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video1.Equals(video2).ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_ReturnsFalseForDifferentVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "Different Title", TestDescription);

        // Act & Assert
        video1.Equals(video2).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_ReturnsFalseForNonSitemapVideoObject()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);
        object other = "Not a SitemapVideo";

        // Act & Assert
        video.Equals(other).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_ReturnsFalseForNull()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video.Equals(null).ShouldBeFalse();
    }

    [TestMethod]
    public void CompareTo_ReturnsZeroForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video1.CompareTo(video2).ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_ReturnsNonZeroForDifferentVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, "AAA Title", TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "ZZZ Title", TestDescription);

        // Act & Assert
        video1.CompareTo(video2).ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_ReturnsOneForNull()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video.CompareTo(null).ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_ThrowsArgumentExceptionForInvalidType()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        Should.Throw<ArgumentException>(() => video.CompareTo("Not a SitemapVideo"));
    }

    [TestMethod]
    public void OperatorEquals_ReturnsTrueForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 == video2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_ReturnsTrueForBothNull()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = null!;

        // Act & Assert
        (video1 == video2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_ReturnsFalseWhenFirstIsNull()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 == video2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorEquals_ReturnsFalseWhenSecondIsNull()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = null!;

        // Act & Assert
        (video1 == video2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorNotEquals_ReturnsFalseForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 != video2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorNotEquals_ReturnsTrueForDifferentVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "Different Title", TestDescription);

        // Act & Assert
        (video1 != video2).ShouldBeTrue();
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsTitle()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video.ToString().ShouldBe(TestTitle);
    }

    [TestMethod]
    public void ToString_ReturnsEmptyStringWhenTitleIsEmpty()
    {
        // Arrange
        SitemapVideo video = new();

        // Act & Assert
        video.ToString().ShouldBe(string.Empty);
    }

    #endregion

    #region GetHashCode Tests

    [TestMethod]
    public void GetHashCode_ReturnsSameValueForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video1.GetHashCode().ShouldBe(video2.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_ReturnsDifferentValueForDifferentVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "Different Title", TestDescription);

        // Act & Assert
        video1.GetHashCode().ShouldNotBe(video2.GetHashCode());
    }

    #endregion

    #region Comparison Operator Tests

    [TestMethod]
    public void OperatorLessThan_WorksCorrectly()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, "AAA", TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "ZZZ", TestDescription);

        // Act & Assert
        (video1 < video2).ShouldBeTrue();
        (video2 < video1).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_WorksCorrectly()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, "ZZZ", TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "AAA", TestDescription);

        // Act & Assert
        (video1 > video2).ShouldBeTrue();
        (video2 > video1).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_WorksCorrectly()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video3 = new(TestThumbnailUri, "ZZZ", TestDescription);

        // Act & Assert
        (video1 <= video2).ShouldBeTrue();
        (video1 <= video3).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_WorksCorrectly()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video3 = new(TestThumbnailUri, "AAA", TestDescription);

        // Act & Assert
        (video1 >= video2).ShouldBeTrue();
        (video1 >= video3).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 < video2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_BothNull_ReturnsFalse()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = null!;

        // Act & Assert
        (video1 < video2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 > video2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_FirstNull_ReturnsTrue()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 <= video2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_FirstNull_SecondNull_ReturnsTrue()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = null!;

        // Act & Assert
        (video1 >= video2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_FirstNull_SecondNotNull_ReturnsFalse()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 >= video2).ShouldBeFalse();
    }

    #endregion

    #region ThumbnailLocation Property Tests

    [TestMethod]
    public void ThumbnailLocation_SetterThrowsOnNull()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => video.ThumbnailLocation = null!);
    }

    [TestMethod]
    public void ThumbnailLocation_SetterWorksCorrectly()
    {
        // Arrange
        SitemapVideo video = new();
        Uri newUri = new("https://example.com/new-thumbnail.jpg");

        // Act
        video.ThumbnailLocation = newUri;

        // Assert
        video.ThumbnailLocation.ShouldBe(newUri);
    }

    #endregion
}