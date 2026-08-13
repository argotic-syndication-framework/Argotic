using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds the extension-bearing sitemap benchmark inputs: the Google video, news and image
/// extensions and the xhtml hreflang annotation.
/// </summary>
/// <remarks>
/// <para>
/// The four sitemap extension families total 930 lines with zero benchmark coverage —
/// <c>SitemapVideo</c> alone is 519 (<c>.endjin/build-warnings.md</c> §19) — and the §13.5
/// schema-order defect shipped in exactly this territory. Element mixes are calibrated against
/// the real publisher documents in the corpus manifest: Yoast's video sitemap (14 distinct
/// children, optional ones genuinely optional <i>within</i> the document), BBC/NYT/Guardian news
/// sitemaps (required-six, +keywords on roughly half, +keywords+genres respectively), Yoast's
/// image sitemap (only <c>image:image</c>/<c>image:loc</c> appear in the wild — caption, title,
/// license and geo_location were deprecated by Google in 2022 and appear in zero live documents,
/// so emitting them would flatter a parser on input that no longer exists), and GitLab's hreflang
/// pages (empty <c>xhtml:link</c> elements with all payload in attributes — the opposite parse
/// profile from every other family).
/// </para>
/// <para>
/// Video child order follows the committed <c>sitemap_video.xml</c>, whose comment records that
/// the <c>xsd:sequence</c> is stricter than it looks and that a file with <c>tag</c> last was
/// rejected by Google's own schema. The non-standard <c>video:width</c>/<c>video:height</c> that
/// Yoast emits on ~99% of entries ride along, because a real-world parser meets them and must
/// skip them.
/// </para>
/// </remarks>
internal static class SitemapExtensionCorpus
{
    /// <summary>
    /// Generates a video sitemap with one <c>video:video</c> per URL in the Yoast shape.
    /// </summary>
    /// <param name="urlCount">The number of <c>url</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    ///     Optionality cycles inside the document at the Yoast census ratios: <c>player_loc</c>
    ///     on ~97% of entries, <c>view_count</c> on two in three, <c>content_loc</c> on one in
    ///     three, and <c>platform</c>/<c>restriction</c> alternating so both
    ///     <c>SitemapVideoPlatform</c> and the restriction relationship get exercised.
    /// </remarks>
    public static byte[] GenerateVideoSitemapUtf8(int urlCount)
    {
        StringBuilder builder = new(capacity: 1024 + (urlCount * 1024));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"\n");
        builder.Append("        xmlns:video=\"http://www.google.com/schemas/sitemap-video/1.1\">\n");

        for (int i = 0; i < urlCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <url>\n");
            builder.Append("    <loc>https://example.com/videos/talk").Append(ordinal).Append("</loc>\n");
            builder.Append("    <video:video>\n");
            builder.Append("      <video:thumbnail_loc>https://example.com/thumbs/talk").Append(ordinal).Append(".jpg</video:thumbnail_loc>\n");
            builder.Append("      <video:title>Talk ").Append(ordinal).Append(": measuring what you meant to measure</video:title>\n");
            builder.Append("      <video:description>A synthetic talk description long enough to resemble the field's real use.</video:description>\n");

            if (i % 3 == 0)
            {
                builder.Append("      <video:content_loc>https://example.com/media/talk").Append(ordinal).Append(".mp4</video:content_loc>\n");
            }

            if (i % 33 != 32)
            {
                builder.Append("      <video:player_loc>https://example.com/embed/talk").Append(ordinal).Append("</video:player_loc>\n");
            }

            // Non-standard but near-universal in the Yoast corpus; a real parser meets and skips them.
            builder.Append("      <video:width>1280</video:width>\n");
            builder.Append("      <video:height>720</video:height>\n");
            builder.Append("      <video:duration>").Append(((i % 50) + 60).ToString(CultureInfo.InvariantCulture)).Append("</video:duration>\n");

            if (i % 3 != 2)
            {
                builder.Append("      <video:view_count>").Append(((i * 37) % 10_000).ToString(CultureInfo.InvariantCulture)).Append("</video:view_count>\n");
            }

            builder.Append("      <video:publication_date>2026-07-29T09:00:00Z</video:publication_date>\n");
            builder.Append("      <video:tag>benchmarks</video:tag>\n");
            builder.Append("      <video:tag>.NET</video:tag>\n");
            builder.Append("      <video:tag>syndication</video:tag>\n");
            builder.Append("      <video:family_friendly>yes</video:family_friendly>\n");

            if (i % 2 == 0)
            {
                builder.Append("      <video:restriction relationship=\"allow\">GB IE US</video:restriction>\n");
            }

            builder.Append("      <video:uploader info=\"https://example.com/channel\">Example Channel</video:uploader>\n");

            if (i % 2 == 1)
            {
                builder.Append("      <video:platform relationship=\"allow\">web mobile</video:platform>\n");
            }

            builder.Append("      <video:live>no</video:live>\n");
            builder.Append("    </video:video>\n");
            builder.Append("  </url>\n");
        }

        builder.Append("</urlset>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates a news sitemap cycling the three real publisher shapes: BBC (required six only),
    /// NYT (plus keywords on roughly half), Guardian (plus keywords and genres on all).
    /// </summary>
    /// <param name="urlCount">The number of <c>url</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateNewsSitemapUtf8(int urlCount)
    {
        StringBuilder builder = new(capacity: 1024 + (urlCount * 512));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"\n");
        builder.Append("        xmlns:news=\"http://www.google.com/schemas/sitemap-news/0.9\">\n");

        for (int i = 0; i < urlCount; i++)
        {
            int shape = i % 3;
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <url>\n");
            builder.Append("    <loc>https://example.com/news/story").Append(ordinal).Append("</loc>\n");
            builder.Append("    <news:news>\n");
            builder.Append("      <news:publication>\n");
            builder.Append("        <news:name>Example News</news:name>\n");
            builder.Append("        <news:language>en</news:language>\n");
            builder.Append("      </news:publication>\n");
            builder.Append("      <news:publication_date>2026-08-07T00:00:00Z</news:publication_date>\n");
            builder.Append("      <news:title>Story ").Append(ordinal).Append(": a synthetic headline</news:title>\n");

            if (shape == 1 && i % 2 == 1)
            {
                builder.Append("      <news:keywords>syndication, benchmarks, dotnet</news:keywords>\n");
            }
            else if (shape == 2)
            {
                builder.Append("      <news:keywords>syndication, benchmarks, dotnet</news:keywords>\n");
                builder.Append("      <news:genres>Blog</news:genres>\n");
            }

            builder.Append("    </news:news>\n");
            builder.Append("  </url>\n");
        }

        builder.Append("</urlset>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates an image sitemap in the only shape the wild still uses: <c>image:image</c>
    /// wrapping a bare <c>image:loc</c>, two images per URL.
    /// </summary>
    /// <param name="urlCount">The number of <c>url</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateImageSitemapUtf8(int urlCount)
    {
        StringBuilder builder = new(capacity: 512 + (urlCount * 384));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"\n");
        builder.Append("        xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\">\n");

        for (int i = 0; i < urlCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <url>\n");
            builder.Append("    <loc>https://example.com/posts/post").Append(ordinal).Append("</loc>\n");
            builder.Append("    <image:image>\n");
            builder.Append("      <image:loc>https://example.com/images/post").Append(ordinal).Append("-hero.png</image:loc>\n");
            builder.Append("    </image:image>\n");
            builder.Append("    <image:image>\n");
            builder.Append("      <image:loc>https://example.com/images/post").Append(ordinal).Append("-diagram.png</image:loc>\n");
            builder.Append("    </image:image>\n");
            builder.Append("  </url>\n");
        }

        builder.Append("</urlset>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates a localisation sitemap in the GitLab shape: per URL, one empty
    /// <c>xhtml:link</c> per locale with all payload in attributes.
    /// </summary>
    /// <param name="urlCount">The number of <c>url</c> elements to emit.</param>
    /// <param name="localeCount">The number of alternate-locale links per URL.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateHreflangSitemapUtf8(int urlCount, int localeCount)
    {
        string[] locales = ["en", "de", "fr", "es", "ja", "pt-BR", "zh-CN", "it", "nl", "pl", "ru", "ko"];

        StringBuilder builder = new(capacity: 512 + (urlCount * (128 + (localeCount * 128))));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"\n");
        builder.Append("        xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">\n");

        for (int i = 0; i < urlCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <url>\n");
            builder.Append("    <loc>https://example.com/docs/page").Append(ordinal).Append("</loc>\n");

            for (int locale = 0; locale < localeCount; locale++)
            {
                string language = locales[locale % locales.Length];
                builder.Append("    <xhtml:link rel=\"alternate\" hreflang=\"").Append(language)
                       .Append("\" href=\"https://example.com/").Append(language).Append("/docs/page").Append(ordinal).Append("\"/>\n");
            }

            builder.Append("  </url>\n");
        }

        builder.Append("</urlset>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}