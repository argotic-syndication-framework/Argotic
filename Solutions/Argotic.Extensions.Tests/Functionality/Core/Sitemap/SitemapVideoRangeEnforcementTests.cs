namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Pins the enforcement of the five published duration and rating bounds, under the rule that a guard
/// throws for programmatic assignment while a loader skips an unusable value.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="SitemapVideo.MinDuration"/>, <see cref="SitemapVideo.MaxDuration"/>,
///         <see cref="SitemapVideo.MinRating"/>, <see cref="SitemapVideo.MaxRating"/> and
///         <see cref="SitemapVideoSegment.MaxDuration"/> are the <c>minInclusive</c>/<c>maxInclusive</c>
///         facets of the Video Sitemap 1.1 schema. They were published as <c>public const</c> and consulted
///         nowhere, while the four <i>string</i> facets from the same schema were enforced.
///     </para>
///     <para>
///         Deleting a <c>const</c> is a source <i>and</i> binary break, because it is inlined into every
///         consumer assembly ever compiled against it — so the answer for dead public constants is to make
///         them live, not to remove them.
///     </para>
///     <para>
///         A feed is untrusted remote input, so <c>Load</c> skips an out-of-range value rather than
///         rejecting the document; a caller assigning one has made a programming error, so the setter
///         throws. That is the same split the <c>sy:updateFrequency</c> decision took.
///     </para>
/// </remarks>
[TestClass]
public class SitemapVideoRangeEnforcementTests
{
    private static readonly Uri Thumbnail = new("https://example.com/thumbnail.jpg");

    /// <summary>
    /// A duration above <see cref="SitemapVideo.MaxDuration"/> throws.
    /// </summary>
    [TestMethod]
    public void AssigningADurationAboveTheMaximum_Throws()
    {
        SitemapVideo video = new(Thumbnail, "A Title", "A description");

        Should.Throw<ArgumentOutOfRangeException>(() => video.Duration = SitemapVideo.MaxDuration + 1);
    }

    /// <summary>
    /// A duration below <see cref="SitemapVideo.MinDuration"/> throws, negative durations included.
    /// </summary>
    [TestMethod]
    public void AssigningADurationBelowTheMinimum_Throws()
    {
        SitemapVideo video = new(Thumbnail, "A Title", "A description");

        Should.Throw<ArgumentOutOfRangeException>(() => video.Duration = 0);
        Should.Throw<ArgumentOutOfRangeException>(() => video.Duration = -1);
    }

    /// <summary>
    /// Both ends of the permitted range, and <see langword="null"/>, are still accepted.
    /// </summary>
    [TestMethod]
    public void AssigningADurationInsideTheRange_IsAccepted()
    {
        SitemapVideo video = new(Thumbnail, "A Title", "A description") { Duration = SitemapVideo.MinDuration };
        video.Duration.ShouldBe(SitemapVideo.MinDuration);

        video.Duration = SitemapVideo.MaxDuration;
        video.Duration.ShouldBe(SitemapVideo.MaxDuration);

        video.Duration = null;
        video.Duration.ShouldBeNull();
    }

    /// <summary>
    /// A rating outside <see cref="SitemapVideo.MinRating"/>..<see cref="SitemapVideo.MaxRating"/> throws.
    /// </summary>
    [TestMethod]
    public void AssigningARatingOutsideTheRange_Throws()
    {
        SitemapVideo video = new(Thumbnail, "A Title", "A description");

        Should.Throw<ArgumentOutOfRangeException>(() => video.Rating = SitemapVideo.MaxRating + 0.1m);
        Should.Throw<ArgumentOutOfRangeException>(() => video.Rating = SitemapVideo.MinRating - 0.1m);
    }

    /// <summary>
    /// Both ends of the permitted rating range, and <see langword="null"/>, are still accepted.
    /// </summary>
    [TestMethod]
    public void AssigningARatingInsideTheRange_IsAccepted()
    {
        SitemapVideo video = new(Thumbnail, "A Title", "A description") { Rating = SitemapVideo.MinRating };
        video.Rating.ShouldBe(SitemapVideo.MinRating);

        video.Rating = SitemapVideo.MaxRating;
        video.Rating.ShouldBe(SitemapVideo.MaxRating);

        video.Rating = null;
        video.Rating.ShouldBeNull();
    }

    /// <summary>
    /// A segment duration above <see cref="SitemapVideoSegment.MaxDuration"/> throws.
    /// </summary>
    /// <remarks>
    ///     The segment publishes only the upper facet, so only the upper facet is enforced.
    /// </remarks>
    [TestMethod]
    public void AssigningASegmentDurationAboveTheMaximum_Throws()
    {
        SitemapVideoSegment segment = new(new Uri("https://example.com/segment.mp4"));

        Should.Throw<ArgumentOutOfRangeException>(() => segment.Duration = SitemapVideoSegment.MaxDuration + 1);
    }

    /// <summary>
    /// The maximum itself, and <see langword="null"/>, are still accepted by a segment.
    /// </summary>
    [TestMethod]
    public void AssigningASegmentDurationAtTheMaximum_IsAccepted()
    {
        SitemapVideoSegment segment = new(new Uri("https://example.com/segment.mp4")) { Duration = SitemapVideoSegment.MaxDuration };
        segment.Duration.ShouldBe(SitemapVideoSegment.MaxDuration);

        segment.Duration = null;
        segment.Duration.ShouldBeNull();
    }

    /// <summary>
    /// A document carrying out-of-range values still loads; the unusable values are skipped and everything
    /// around them survives.
    /// </summary>
    [TestMethod]
    public void ADocumentWithOutOfRangeValues_LoadsWithoutThem()
    {
        SitemapVideo video = LoadSingleVideo();

        video.Title.ShouldBe("Example Video");
        video.Duration.ShouldBeNull();
        video.Rating.ShouldBeNull();
        video.ContentSegments.Single().Duration.ShouldBeNull();
        video.ContentSegments.Single().Location.ShouldBe(new Uri("https://example.com/segment.mp4"));
    }

    /// <summary>
    /// Loads the one video of the out-of-range fixture.
    /// </summary>
    /// <returns>The video.</returns>
    private static SitemapVideo LoadSingleVideo()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                    xmlns:video="http://www.google.com/schemas/sitemap-video/1.1">
              <url>
                <loc>https://example.com/videos/1</loc>
                <video:video>
                  <video:thumbnail_loc>https://example.com/thumb.jpg</video:thumbnail_loc>
                  <video:title>Example Video</video:title>
                  <video:description>A sample video description</video:description>
                  <video:duration>99999</video:duration>
                  <video:rating>9.9</video:rating>
                  <video:content_segment_loc duration="88888">https://example.com/segment.mp4</video:content_segment_loc>
                </video:video>
              </url>
            </urlset>
            """;

        Argotic.Syndication.Sitemap sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        sitemap.Load(stream);

        SitemapUrl url = sitemap.Urls.Single();
        SitemapVideoExtension extension = url.Extensions.OfType<SitemapVideoExtension>().Single();
        return extension.Videos.Single();
    }
}