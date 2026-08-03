using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds the benchmark inputs.
/// </summary>
/// <remarks>
/// <para>
/// Two provenances, deliberately. Synthetic documents are the only way to get a scaling curve —
/// the repository's real sample documents are all small (15 files, 76 KB total) and cannot show
/// how cost grows with feed size. Real documents are the only way to know the synthetic ones are
/// not a fiction.
/// </para>
/// <para>
/// The synthetic generator therefore models its element mix on
/// <c>Argotic.Examples/SampleData/RssFeed.xml</c> rather than emitting minimal stubs: channel
/// metadata, image, categories, and per-item author/categories/enclosure/guid/source. A feed of
/// bare title-and-link items would measure a parser that Argotic never meets in production, and
/// would flatter every result.
/// </para>
/// </remarks>
internal static class FeedCorpus
{
    /// <summary>
    /// Generates an RSS 2.0 document with <paramref name="itemCount"/> items, using the element
    /// mix of the repository's real sample feed.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes, as it would arrive over the wire.</returns>
    public static byte[] GenerateRssUtf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 1024 + (itemCount * 512));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<rss version=\"2.0\">\n  <channel>\n");
        builder.Append("    <title>Synthetic Benchmark Feed</title>\n");
        builder.Append("    <link>https://example.com/</link>\n");
        builder.Append("    <description>A synthetic feed whose element mix mirrors the repository's real RSS sample.</description>\n");
        builder.Append("    <language>en-us</language>\n");
        builder.Append("    <copyright>Copyright 2026 Example Corporation</copyright>\n");
        builder.Append("    <managingEditor>editor@example.com (Jane Editor)</managingEditor>\n");
        builder.Append("    <webMaster>webmaster@example.com (John Admin)</webMaster>\n");
        builder.Append("    <pubDate>Mon, 01 Jan 2024 08:00:00 GMT</pubDate>\n");
        builder.Append("    <lastBuildDate>Mon, 01 Jan 2024 12:00:00 GMT</lastBuildDate>\n");
        builder.Append("    <category domain=\"https://example.com/categories\">Technology</category>\n");
        builder.Append("    <category>Software Development</category>\n");
        builder.Append("    <generator>Argotic Benchmarks</generator>\n");
        builder.Append("    <docs>https://www.rssboard.org/rss-specification</docs>\n");
        builder.Append("    <ttl>60</ttl>\n");
        builder.Append("    <image>\n");
        builder.Append("      <url>https://example.com/images/logo.png</url>\n");
        builder.Append("      <title>Synthetic Benchmark Feed</title>\n");
        builder.Append("      <link>https://example.com/</link>\n");
        builder.Append("      <width>144</width>\n");
        builder.Append("      <height>144</height>\n");
        builder.Append("      <description>The logo for the synthetic benchmark feed</description>\n");
        builder.Append("    </image>\n");

        for (int i = 0; i < itemCount; i++)
        {
            AppendItem(builder, i);
        }

        builder.Append("  </channel>\n</rss>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Reads one of the repository's real sample documents, linked into the benchmark output.
    /// </summary>
    /// <param name="fileName">The sample file name, e.g. <c>RssFeed.xml</c>.</param>
    /// <returns>The document's bytes exactly as they sit on disk, BOM and all.</returns>
    public static byte[] ReadRealSample(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "SampleData", fileName);
        return File.ReadAllBytes(path);
    }

    private static void AppendItem(StringBuilder builder, int index)
    {
        // The real sample varies its items: some carry an enclosure, some a source element, and
        // guid alternates between permalink and urn forms. Cycling through those shapes keeps the
        // synthetic parse work representative instead of measuring one repeated element pattern.
        int shape = index % 3;
        string ordinal = index.ToString(CultureInfo.InvariantCulture);

        builder.Append("    <item>\n");
        builder.Append("      <title>Synthetic Article ").Append(ordinal).Append("</title>\n");
        builder.Append("      <link>https://example.com/article").Append(ordinal).Append("</link>\n");
        builder.Append("      <description>Body text for synthetic article ").Append(ordinal)
               .Append(", long enough to resemble a real feed entry rather than a stub.</description>\n");
        builder.Append("      <author>author").Append(ordinal).Append("@example.com (Alice Writer)</author>\n");
        builder.Append("      <category>Podcasts</category>\n");
        builder.Append("      <category domain=\"https://example.com/tags\">Audio</category>\n");

        if (shape != 2)
        {
            builder.Append("      <category>Tutorial</category>\n");
        }

        builder.Append("      <comments>https://example.com/article").Append(ordinal).Append("#comments</comments>\n");

        if (shape == 0)
        {
            builder.Append("      <enclosure url=\"https://example.com/media/episode").Append(ordinal)
                   .Append(".mp3\" length=\"12345678\" type=\"audio/mpeg\"/>\n");
        }
        else if (shape == 1)
        {
            builder.Append("      <enclosure url=\"https://example.com/media/tutorial").Append(ordinal)
                   .Append(".mp4\" length=\"98765432\" type=\"video/mp4\"/>\n");
        }

        builder.Append(shape == 1
            ? "      <guid isPermaLink=\"false\">urn:uuid:synthetic-item-" + ordinal + "</guid>\n"
            : "      <guid isPermaLink=\"true\">https://example.com/article" + ordinal + "</guid>\n");

        builder.Append("      <pubDate>Mon, 01 Jan 2024 10:00:00 GMT</pubDate>\n");

        if (shape == 0)
        {
            builder.Append("      <source url=\"https://original-source.com/feed.rss\">Original Source Feed</source>\n");
        }

        builder.Append("    </item>\n");
    }
}
