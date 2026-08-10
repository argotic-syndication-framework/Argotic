namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// The news and video extensions write their dates in UTC, with the literal <c>Z</c> designator.
/// </summary>
/// <remarks>
///     <para>
///     These writers used the format string <c>yyyy-MM-ddTHH:mm:sszzz</c>. On .NET 10, <c>zzz</c>
///     writes <c>+00:00</c> for a <c>Utc</c> kind on every machine — the runtime special-cases the
///     kind — but it stamps the <b>machine's</b> offset on a <c>Local</c> or <c>Unspecified</c> wall
///     clock. The load paths assume UTC for offset-less input, so an <c>Unspecified</c> value written
///     on a non-UTC machine came back as a different instant. Issue 177 records the agreed fix: every
///     kind is normalised to UTC and written with <c>Z</c>.
///     </para>
///     <para>
///     The exact-text assertions are not vacuous on a UTC build agent: <c>zzz</c> renders
///     <c>+00:00</c>, never <c>Z</c>, so the old shape differs from the required one whatever the
///     machine's timezone. The <c>Local</c>-kind tests assert shape and round-trip instant only,
///     because the exact text of a converted local wall clock is machine-dependent by construction.
///     </para>
/// </remarks>
[TestClass]
public class SitemapExtensionDateWritingTests
{
    /// <summary>
    /// A news publication date of Utc or Unspecified kind keeps its wall clock and gains the Z designator.
    /// </summary>
    /// <param name="kind">The kind under test. Both kinds mean UTC to this library.</param>
    [TestMethod]
    [DataRow(DateTimeKind.Utc)]
    [DataRow(DateTimeKind.Unspecified)]
    public void AUtcOrUnspecifiedKindNewsPublicationDate_IsWrittenWithALiteralZ(DateTimeKind kind)
    {
        Syndication.Sitemap sitemap = SitemapWithNews(new DateTime(2026, 8, 7, 9, 0, 0, kind));

        string xml = Save(sitemap);

        xml.ShouldContain(
            "<news:publication_date>2026-08-07T09:00:00Z</news:publication_date>",
            customMessage: "the writer must not shift the wall clock and must use the Z designator");
        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);
    }

    /// <summary>
    /// A news publication date of Local kind converts to UTC, writes with Z, and keeps its instant.
    /// </summary>
    [TestMethod]
    public void ALocalKindNewsPublicationDate_IsWrittenAsUtcWithALiteralZ()
    {
        DateTime local = new(2026, 8, 7, 9, 0, 0, DateTimeKind.Local);
        Syndication.Sitemap sitemap = SitemapWithNews(local);

        string xml = Save(sitemap);

        xml.ShouldContain("Z</news:publication_date>", Case.Sensitive);
        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);

        SitemapNewsExtension read = Reload(sitemap).Urls[0].Extensions.OfType<SitemapNewsExtension>()
            .Single();
        read.PublicationDate.ShouldBe(
            local.ToUniversalTime(),
            "a Local kind must convert to the same instant in UTC, on any machine");
    }

    /// <summary>
    /// A video publication date of Utc or Unspecified kind keeps its wall clock and gains the Z designator.
    /// </summary>
    /// <param name="kind">The kind under test. Both kinds mean UTC to this library.</param>
    [TestMethod]
    [DataRow(DateTimeKind.Utc)]
    [DataRow(DateTimeKind.Unspecified)]
    public void AUtcOrUnspecifiedKindVideoPublicationDate_IsWrittenWithALiteralZ(DateTimeKind kind)
    {
        Syndication.Sitemap sitemap = SitemapWithVideo(video =>
            video.PublicationDate = new DateTime(2026, 8, 7, 9, 0, 0, kind));

        string xml = Save(sitemap);

        xml.ShouldContain(
            "<video:publication_date>2026-08-07T09:00:00Z</video:publication_date>",
            customMessage: "the writer must not shift the wall clock and must use the Z designator");
        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);
    }

    /// <summary>
    /// A video publication date of Local kind converts to UTC, writes with Z, and keeps its instant.
    /// </summary>
    [TestMethod]
    public void ALocalKindVideoPublicationDate_IsWrittenAsUtcWithALiteralZ()
    {
        DateTime local = new(2026, 8, 7, 9, 0, 0, DateTimeKind.Local);
        Syndication.Sitemap sitemap = SitemapWithVideo(video => video.PublicationDate = local);

        string xml = Save(sitemap);

        xml.ShouldContain("Z</video:publication_date>", Case.Sensitive);
        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);

        ReadVideo(sitemap).PublicationDate.ShouldBe(
            local.ToUniversalTime(),
            "a Local kind must convert to the same instant in UTC, on any machine");
    }

    /// <summary>
    /// A video expiration date of Utc or Unspecified kind keeps its wall clock and gains the Z designator.
    /// </summary>
    /// <param name="kind">The kind under test. Both kinds mean UTC to this library.</param>
    [TestMethod]
    [DataRow(DateTimeKind.Utc)]
    [DataRow(DateTimeKind.Unspecified)]
    public void AUtcOrUnspecifiedKindVideoExpirationDate_IsWrittenWithALiteralZ(DateTimeKind kind)
    {
        Syndication.Sitemap sitemap = SitemapWithVideo(video =>
            video.ExpirationDate = new DateTime(2026, 8, 7, 9, 0, 0, kind));

        string xml = Save(sitemap);

        xml.ShouldContain(
            "<video:expiration_date>2026-08-07T09:00:00Z</video:expiration_date>",
            customMessage: "the writer must not shift the wall clock and must use the Z designator");
        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);
    }

    /// <summary>
    /// A video expiration date of Local kind converts to UTC, writes with Z, and keeps its instant.
    /// </summary>
    [TestMethod]
    public void ALocalKindVideoExpirationDate_IsWrittenAsUtcWithALiteralZ()
    {
        DateTime local = new(2026, 8, 7, 9, 0, 0, DateTimeKind.Local);
        Syndication.Sitemap sitemap = SitemapWithVideo(video => video.ExpirationDate = local);

        string xml = Save(sitemap);

        xml.ShouldContain("Z</video:expiration_date>", Case.Sensitive);
        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);

        ReadVideo(sitemap).ExpirationDate.ShouldBe(
            local.ToUniversalTime(),
            "a Local kind must convert to the same instant in UTC, on any machine");
    }

    private static Syndication.Sitemap SitemapWithNews(DateTime publicationDate)
    {
        SitemapNewsExtension news = new()
        {
            Title = "A breaking story",
            PublicationDate = publicationDate,
            Publication = new SitemapNewsPublication { Name = "The Example Times", Language = "en" },
        };

        SitemapUrl url = new(new Uri("https://example.com/news/story"));
        url.Extensions.Add(news);

        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(url);
        return sitemap;
    }

    private static Syndication.Sitemap SitemapWithVideo(Action<SitemapVideo> configure)
    {
        SitemapVideo video = new()
        {
            Title = "An example video",
            Description = "A description of the example video",
            ThumbnailLocation = new Uri("https://example.com/thumb.jpg"),
            ContentLocation = new Uri("https://example.com/video.mp4"),
        };
        configure(video);

        SitemapVideoExtension extension = new();
        extension.Videos.Add(video);

        SitemapUrl url = new(new Uri("https://example.com/watch/1"));
        url.Extensions.Add(extension);

        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(url);
        return sitemap;
    }

    private static SitemapVideo ReadVideo(Syndication.Sitemap sitemap) =>
        Reload(sitemap).Urls[0].Extensions.OfType<SitemapVideoExtension>().Single().Videos[0];

    private static string Save(Syndication.Sitemap sitemap)
    {
        using MemoryStream stream = new();
        sitemap.Save(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static Syndication.Sitemap Reload(Syndication.Sitemap sitemap)
    {
        using MemoryStream stream = new();
        sitemap.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        Syndication.Sitemap read = new();
        read.Load(stream);
        return read;
    }
}