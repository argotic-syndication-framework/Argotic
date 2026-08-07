using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers <see cref="SitemapVideo"/>: the three values Google requires of every entry, the length and
/// count limits the properties enforce on assignment, the published duration and rating bounds, the
/// defaults of the boolean elements, and the equality, ordering and hashing contracts.
/// </summary>
/// <remarks>
///     Two neighbours carry what this file does not:
///     <c>SitemapVideoComparisonBreadthTests</c> pins the breadth of the comparison and its agreement with
///     the hash, and <c>SitemapVideoRangeEnforcementTests</c> pins the enforcement of the duration and
///     rating bounds.
/// </remarks>
[TestClass]
public class SitemapVideoTests
{
    #region Test Data

    private static readonly Uri TestThumbnailUri = new("https://example.com/thumbnail.jpg");
    private const string TestTitle = "Test Video Title";
    private const string TestDescription = "Test video description for unit testing.";

    #endregion

    #region Constructor Tests

    /// <summary>
    /// A video constructed with no arguments has no thumbnail and an <i>empty</i> title and description,
    /// not nulls.
    /// </summary>
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

    /// <summary>
    /// The three values Google requires of every entry — thumbnail, title and description — are stored
    /// as given.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> thumbnail location throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullThumbnail_ThrowsArgumentNullException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new SitemapVideo(null!, TestTitle, TestDescription));

    /// <summary>
    /// A <see langword="null"/> title is rejected; the guard is <c>ArgumentException.ThrowIfNullOrEmpty</c>,
    /// so what surfaces is its <see cref="ArgumentNullException"/> branch.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullTitle_ThrowsArgumentException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, null!, TestDescription));

    /// <summary>
    /// An <i>empty</i> title throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, string.Empty, TestDescription));

    /// <summary>
    /// A <see langword="null"/> description is rejected by the same guard, and so by the same
    /// <see cref="ArgumentNullException"/> branch.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullDescription_ThrowsArgumentException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, TestTitle, null!));

    /// <summary>
    /// An <i>empty</i> description throws <see cref="ArgumentException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyDescription_ThrowsArgumentException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => new SitemapVideo(TestThumbnailUri, TestTitle, string.Empty));

    #endregion

    #region Title Validation Tests

    /// <summary>
    /// A 150-character title is cut to <c>MaxTitleLength</c> characters on assignment.
    /// </summary>
    /// <remarks>
    ///     The setter truncates rather than throwing, so a publisher's over-long title still produces a
    ///     document Google will accept.
    /// </remarks>
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

    /// <summary>
    /// Leading and trailing whitespace is stripped from a title on assignment.
    /// </summary>
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

    /// <summary>
    /// The title limit is <c>100</c> characters.
    /// </summary>
    /// <remarks>
    ///     That is the <c>maxLength</c> facet the Video Sitemap 1.1 XSD puts on <c>video:title</c>.
    ///     Google's prose documentation states no title limit at all, so the schema is the stricter of the
    ///     two sources and the one this library follows.
    /// </remarks>
    [TestMethod]
    public void MaxTitleLength_EqualsOneHundred() =>
        // Assert
        SitemapVideo.MaxTitleLength.ShouldBe(100);

    #endregion

    #region Description Validation Tests

    /// <summary>
    /// A 3,000-character description is cut to <c>MaxDescriptionLength</c> characters on assignment.
    /// </summary>
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

    /// <summary>
    /// Leading and trailing whitespace is stripped from a description on assignment.
    /// </summary>
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

    /// <summary>
    /// The description limit is <c>2048</c> characters.
    /// </summary>
    /// <remarks>
    ///     The XSD and Google's documentation agree on this one: "Maximum 2048 characters."
    /// </remarks>
    [TestMethod]
    public void MaxDescriptionLength_EqualsTwoThousandFortyEight() =>
        // Assert
        SitemapVideo.MaxDescriptionLength.ShouldBe(2048);

    #endregion

    #region Duration and Rating Constant Tests

    /// <summary>
    /// The shortest duration Google accepts is <c>1</c> second.
    /// </summary>
    [TestMethod]
    public void MinDuration_EqualsOne() =>
        // Assert
        SitemapVideo.MinDuration.ShouldBe(1);

    /// <summary>
    /// The longest duration Google accepts is <c>28800</c> seconds.
    /// </summary>
    /// <remarks>
    ///     Eight hours. This constant and the rating pair are the numeric facets of the Video Sitemap 1.1
    ///     schema, and they are enforced: the setters throw outside the range and <c>Load</c> skips a source
    ///     value outside it. <c>SitemapVideoRangeEnforcementTests</c> pins that; these four assert only the
    ///     published values, which is what a consumer compiles against.
    /// </remarks>
    [TestMethod]
    public void MaxDuration_EqualsTwentyEightThousandEightHundred() =>
        // Assert
        SitemapVideo.MaxDuration.ShouldBe(28_800);

    /// <summary>
    /// The lowest rating Google accepts is <c>0.0</c>.
    /// </summary>
    [TestMethod]
    public void MinRating_EqualsZeroPointZero() =>
        // Assert
        SitemapVideo.MinRating.ShouldBe(0.0m);

    /// <summary>
    /// The highest rating Google accepts is <c>5.0</c>.
    /// </summary>
    [TestMethod]
    public void MaxRating_EqualsFivePointZero() =>
        // Assert
        SitemapVideo.MaxRating.ShouldBe(5.0m);

    #endregion

    #region Tag Tests

    /// <summary>
    /// At most <c>32</c> tags are carried.
    /// </summary>
    /// <remarks>
    ///     Google: "A maximum of 32 tags is permitted per video." Unlike the duration and rating constants
    ///     this one is enforced — reading and writing both stop once 32 tags have been seen.
    /// </remarks>
    [TestMethod]
    public void MaxTagCount_EqualsThirtyTwo() =>
        // Assert
        SitemapVideo.MaxTagCount.ShouldBe(32);

    /// <summary>
    /// A new video exposes an empty tag collection rather than <see langword="null"/>, so a caller can
    /// add to it without a null check.
    /// </summary>
    [TestMethod]
    public void Tags_ReturnsEmptyListByDefault()
    {
        // Arrange
        SitemapVideo video = new();

        // Act & Assert
        video.Tags.ShouldNotBeNull();
        video.Tags.ShouldBeEmpty();
    }

    /// <summary>
    /// Tags added to the collection are held in the order they were added.
    /// </summary>
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

    /// <summary>
    /// A video is family friendly unless it says otherwise, matching the sense of Google's
    /// <c>family_friendly</c> element.
    /// </summary>
    [TestMethod]
    public void FamilyFriendly_DefaultsToTrue()
    {
        // Arrange
        SitemapVideo video = new();

        // Assert
        video.FamilyFriendly.ShouldBeTrue();
    }

    /// <summary>
    /// A video does not require a subscription unless it says so.
    /// </summary>
    [TestMethod]
    public void RequiresSubscription_DefaultsToFalse()
    {
        // Arrange
        SitemapVideo video = new();

        // Assert
        video.RequiresSubscription.ShouldBeFalse();
    }

    /// <summary>
    /// A video is not a live stream unless it says so.
    /// </summary>
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

    /// <summary>
    /// A 300-character uploader name is cut to <c>MaxUploaderLength</c> characters on assignment.
    /// </summary>
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

    /// <summary>
    /// Leading and trailing whitespace is stripped from an uploader name on assignment.
    /// </summary>
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

    /// <summary>
    /// The uploader-name limit is <c>255</c> characters.
    /// </summary>
    /// <remarks>
    ///     Google: "The string value can be a maximum of 255 characters."
    /// </remarks>
    [TestMethod]
    public void MaxUploaderLength_EqualsTwoHundredFiftyFive() =>
        // Assert
        SitemapVideo.MaxUploaderLength.ShouldBe(255);

    /// <summary>
    /// Assigning <see langword="null"/> to the uploader clears it to an <i>empty</i> string rather than
    /// throwing, so the element is simply omitted when the video is written.
    /// </summary>
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

    /// <summary>
    /// Assigning an <i>empty</i> string to the uploader clears a name that was already there.
    /// </summary>
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

    /// <summary>
    /// Two videos built from the same thumbnail, title and description are equal.
    /// </summary>
    [TestMethod]
    public void Equals_ReturnsTrueForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video1.Equals(video2).ShouldBeTrue();
    }

    /// <summary>
    /// A difference in title alone is enough to make two videos unequal.
    /// </summary>
    [TestMethod]
    public void Equals_ReturnsFalseForDifferentVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "Different Title", TestDescription);

        // Act & Assert
        video1.Equals(video2).ShouldBeFalse();
    }

    /// <summary>
    /// A video is never equal to an object of another type, here a <see cref="string"/>.
    /// </summary>
    [TestMethod]
    public void Equals_ReturnsFalseForNonSitemapVideoObject()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);
        object other = "Not a SitemapVideo";

        // Act & Assert
        video.Equals(other).ShouldBeFalse();
    }

    /// <summary>
    /// A video is never equal to <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void Equals_ReturnsFalseForNull()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video.Equals(null).ShouldBeFalse();
    }

    /// <summary>
    /// Two videos with the same thumbnail, title and description compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_ReturnsZeroForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video1.CompareTo(video2).ShouldBe(0);
    }

    /// <summary>
    /// Two videos differing only in title do not compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_ReturnsNonZeroForDifferentVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, "AAA Title", TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, "ZZZ Title", TestDescription);

        // Act & Assert
        video1.CompareTo(video2).ShouldNotBe(0);
    }

    /// <summary>
    /// A video compares greater than <see langword="null"/>, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_ReturnsOneForNull()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video.CompareTo(null).ShouldBe(1);
    }

    /// <summary>
    /// Two videos differing in every required value do not compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_DifferentVideos_ReturnsNonZero()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(new Uri("http://example.com/other-thumb.jpg"), "Other Title", "Other Description");

        // Act
        int result = video1.CompareTo(video2);

        // Assert
        result.ShouldNotBe(0);
    }

    /// <summary>
    /// The equality operator agrees with <c>Equals</c> for two identically built videos.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_ReturnsTrueForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 == video2).ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> references compare equal under the operator rather than dereferencing.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_ReturnsTrueForBothNull()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = null!;

        // Act & Assert
        (video1 == video2).ShouldBeTrue();
    }

    /// <summary>
    /// A <see langword="null"/> left operand is not equal to a video, and the operator does not throw.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_ReturnsFalseWhenFirstIsNull()
    {
        // Arrange
        SitemapVideo video1 = null!;
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 == video2).ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> right operand is not equal to a video, and the operator does not throw.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_ReturnsFalseWhenSecondIsNull()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = null!;

        // Act & Assert
        (video1 == video2).ShouldBeFalse();
    }

    /// <summary>
    /// The inequality operator is the negation of the equality operator for two identically built videos.
    /// </summary>
    [TestMethod]
    public void OperatorNotEquals_ReturnsFalseForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        (video1 != video2).ShouldBeFalse();
    }

    /// <summary>
    /// The inequality operator reports videos differing in title as different.
    /// </summary>
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

    /// <summary>
    /// A video renders as its title alone, with no element name or punctuation around it.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsTitle()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video.ToString().ShouldBe(TestTitle);
    }

    /// <summary>
    /// A video with no title renders as an <i>empty</i> string rather than throwing or naming the type.
    /// </summary>
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

    /// <summary>
    /// Equal videos hash equally, which is the contract a hash set relies on.
    /// </summary>
    [TestMethod]
    public void GetHashCode_ReturnsSameValueForEqualVideos()
    {
        // Arrange
        SitemapVideo video1 = new(TestThumbnailUri, TestTitle, TestDescription);
        SitemapVideo video2 = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        video1.GetHashCode().ShouldBe(video2.GetHashCode());
    }

    /// <summary>
    /// Videos differing only in title hash differently, so the title is part of the hash and not just of
    /// <c>Equals</c>.
    /// </summary>
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

    #region ThumbnailLocation Property Tests

    /// <summary>
    /// Assigning <see langword="null"/> to the thumbnail location throws
    /// <see cref="ArgumentNullException"/>, so a video that has one cannot lose it.
    /// </summary>
    [TestMethod]
    public void ThumbnailLocation_SetterThrowsOnNull()
    {
        // Arrange
        SitemapVideo video = new(TestThumbnailUri, TestTitle, TestDescription);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => video.ThumbnailLocation = null!);
    }

    /// <summary>
    /// A thumbnail location assigned after construction is stored unchanged.
    /// </summary>
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