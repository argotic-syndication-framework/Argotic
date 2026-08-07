using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Pins the breadth of <see cref="SitemapVideo"/>'s comparison, and the agreement between that comparison
/// and its hash code.
/// </summary>
/// <remarks>
///     <para>
///         The comparison used to fold three of the type's twenty-two members — <see cref="SitemapVideo.Title"/>,
///         <see cref="SitemapVideo.ThumbnailLocation"/> and <see cref="SitemapVideo.Description"/>. The hash
///         folded the same three, so the dictionary contract held; what did not hold was the meaning. Two
///         videos agreeing on those three and differing in content location, duration, tags and identifiers
///         reported themselves equal, so a <see cref="HashSet{T}"/> or a <c>Distinct()</c> silently discarded
///         one of them.
///     </para>
///     <para>
///         Both were widened together, which is the only safe way to do it: widening the comparison alone
///         reintroduces the collection-by-reference defect of <c>docs/build-warnings.md</c> §4.3 verbatim.
///         <see cref="TwoVideosBuiltFromIdenticalDataIncludingCollections_AreEqualAndHashAlike"/> is the guard
///         for that, and is green on both sides of the change.
///     </para>
/// </remarks>
[TestClass]
public class SitemapVideoComparisonBreadthTests
{
    private static readonly Uri Thumbnail = new("https://example.com/thumbnail.jpg");

    /// <summary>
    /// Two videos agreeing on title, thumbnail and description but differing in every optional member are
    /// not equal, and the one carrying no optional members sorts before the one that does.
    /// </summary>
    [TestMethod]
    public void TwoVideosDifferingInEveryOptionalMember_AreUnequalAndOrderedByTheFirstOptionalMember()
    {
        SitemapVideo first = Build();
        SitemapVideo second = BuildWithEveryOptionalMember();

        first.Equals(second).ShouldBeFalse();

        // The three required members agree, so the first optional one decides: ContentLocation is absent on
        // the bare video, and CompareLocation treats an absent location as the lesser.
        first.CompareTo(second).ShouldBeLessThan(0);
        second.CompareTo(first).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// A hash set therefore keeps two videos that differ only in the file they point at, rather than
    /// discarding one.
    /// </summary>
    [TestMethod]
    public void AHashSetOfVideosDifferingOnlyInContentLocation_KeepsBoth()
    {
        SitemapVideo first = Build();
        SitemapVideo second = Build();
        second.ContentLocation = new Uri("https://example.com/other.mp4");

        HashSet<SitemapVideo> set = [first, second];

        set.Count.ShouldBe(2);
    }

    /// <summary>
    /// Each of the three collections is folded element by element rather than by reference, so two videos
    /// differing in one tag, one identifier or one segment are unequal.
    /// </summary>
    [TestMethod]
    public void TwoVideosDifferingInOneCollectionElement_AreUnequal()
    {
        SitemapVideo differentTag = BuildWithEveryOptionalMember();
        differentTag.Tags[1] = "third";

        SitemapVideo differentIdentifier = BuildWithEveryOptionalMember();
        differentIdentifier.Identifiers[0] = new SitemapVideoId("54321", SitemapVideoIdType.TmsProgram);

        SitemapVideo differentSegment = BuildWithEveryOptionalMember();
        differentSegment.ContentSegments[0] = new SitemapVideoSegment(new Uri("https://example.com/segment-2.mp4"), 300);

        SitemapVideo baseline = BuildWithEveryOptionalMember();

        baseline.Equals(differentTag).ShouldBeFalse("tags are compared");
        baseline.Equals(differentIdentifier).ShouldBeFalse("identifiers are compared");
        baseline.Equals(differentSegment).ShouldBeFalse("content segments are compared");
    }

    /// <summary>
    /// Two videos built from identical data, collections included, are equal and hash equally. This is the
    /// guard that catches a comparison widened without its hash: it is green on both sides of the change.
    /// </summary>
    [TestMethod]
    public void TwoVideosBuiltFromIdenticalDataIncludingCollections_AreEqualAndHashAlike()
    {
        SitemapVideo first = BuildWithEveryOptionalMember();
        SitemapVideo second = BuildWithEveryOptionalMember();

        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// <see cref="SitemapVideo.Title"/> stays the first comparand, so the ordering the type published over
    /// its required fields is unchanged.
    /// </summary>
    [TestMethod]
    public void TitleStillDominatesTheOrdering()
    {
        SitemapVideo lesser = new(Thumbnail, "AAA", "A description") { Duration = 28_800 };
        SitemapVideo greater = new(Thumbnail, "ZZZ", "A description") { Duration = 1 };

        lesser.CompareTo(greater).ShouldBeLessThan(0);
        greater.CompareTo(lesser).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// <see cref="Uri.Compare"/> over <see cref="UriComponents.AbsoluteUri"/> neither throws on a relative
    /// URI nor stops distinguishing one, which is what lets the four <see cref="Uri"/> members be compared
    /// the same way when <c>SitemapVideo.Load</c> accepts <see cref="UriKind.RelativeOrAbsolute"/>.
    /// </summary>
    [TestMethod]
    public void ComparingRelativeUris_NeitherThrowsNorCollapses()
    {
        Uri relative = new("/video.mp4", UriKind.Relative);

        Uri.Compare(relative, new Uri("/other.mp4", UriKind.Relative), UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase).ShouldNotBe(0);
        Uri.Compare(relative, new Uri("/video.mp4", UriKind.Relative), UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase).ShouldBe(0);
        Uri.Compare(relative, new Uri("/VIDEO.MP4", UriKind.Relative), UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase).ShouldBe(0);
    }

    /// <summary>
    /// Builds a video carrying only the three required values.
    /// </summary>
    /// <returns>The video.</returns>
    private static SitemapVideo Build() => new(Thumbnail, "A Title", "A description");

    /// <summary>
    /// Builds the same three required values with every optional member populated.
    /// </summary>
    /// <returns>The video.</returns>
    private static SitemapVideo BuildWithEveryOptionalMember()
    {
        SitemapVideo video = new(Thumbnail, "A Title", "A description")
        {
            ContentLocation = new Uri("https://example.com/video.mp4"),
            PlayerLocation = new Uri("https://example.com/player"),
            Duration = 600,
            ExpirationDate = new DateTime(2030, 11, 5, 19, 20, 30, DateTimeKind.Utc),
            Rating = 4.1m,
            ViewCount = 12345,
            PublicationDate = new DateTime(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc),
            FamilyFriendly = false,
            RequiresSubscription = true,
            Live = true,
            Uploader = "Someone",
            UploaderInfo = new Uri("https://example.com/uploader"),
            Platform = SitemapVideoPlatform.Web,
            PlatformRelationship = SitemapVideoRelationship.Allow,
            Restriction = "GB",
            RestrictionRelationship = SitemapVideoRelationship.Deny,
        };

        video.Tags.Add("first");
        video.Tags.Add("second");
        video.Identifiers.Add(new SitemapVideoId("12345", SitemapVideoIdType.TmsProgram));
        video.ContentSegments.Add(new SitemapVideoSegment(new Uri("https://example.com/segment-1.mp4"), 300));

        return video;
    }
}