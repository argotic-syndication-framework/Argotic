using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds the benchmark inputs.
/// </summary>
/// <remarks>
/// <para>
/// Two provenances, deliberately. Synthetic documents are the only way to get a scaling curve —
/// the repository's real sample documents are all small (15 files, 59.8 KiB total) and cannot show
/// how cost grows with feed size. Real documents are the only way to know the synthetic ones are
/// not a fiction.
/// </para>
/// <para>
/// <b>Every generator above <see cref="GenerateRssWithDirtUtf8"/> emits pure 7-bit ASCII, free of
/// invalid XML characters, over a seekable <see cref="MemoryStream"/>.</b> That is three separate
/// blind spots, and each one guarantees a favourable measurement for a change whose whole purpose is
/// to alter the path the corpus cannot reach: the sanitiser's rebuild loop never runs, the
/// <c>SearchValues</c> fast path always fires, and <c>GetStreamBytes</c> always takes its
/// <c>CanSeek</c> branch. The last three generators exist to remove those blind spots, and they were
/// added <i>before</i> the change they measure — a benchmark authored afterwards has no "before".
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
    /// Generates an RSS 2.0 document that DECLARES extension namespaces and carries extension
    /// elements on every item, modelled on the repository's <c>RssFeedWithExtensions.xml</c>.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    /// <para>
    /// This exists because a blind prosecution panel found the extension-free corpus made an
    /// experiment unfalsifiable. Any change that filters extension work by declared namespace
    /// skips 100% of that work on a document declaring no namespaces, so measuring only
    /// <see cref="GenerateRssUtf8"/> guarantees a favourable result no matter what the change does.
    /// </para>
    /// <para>
    /// Production feeds are not extension-free — the newsletters this library serves aggregate
    /// sources carrying Dublin Core, content:encoded, slash, syndication, iTunes and Media RSS.
    /// This generator declares the same namespaces the real sample declares and emits matching
    /// elements per item, so extension work is genuinely performed and a change that helps only
    /// the extension-free case cannot hide.
    /// </para>
    /// </remarks>
    public static byte[] GenerateRssWithExtensionsUtf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 2048 + (itemCount * 1024));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<rss version=\"2.0\"\n");
        builder.Append("     xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n");
        builder.Append("     xmlns:dcterms=\"http://purl.org/dc/terms/\"\n");
        builder.Append("     xmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\n");
        builder.Append("     xmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\n");
        builder.Append("     xmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\n");
        builder.Append("     xmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\n");
        builder.Append("     xmlns:media=\"http://search.yahoo.com/mrss/\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Synthetic Extension-Bearing Feed</title>\n");
        builder.Append("    <link>https://example.com/</link>\n");
        builder.Append("    <description>A synthetic feed declaring the extension namespaces the real sample declares.</description>\n");
        builder.Append("    <language>en-us</language>\n");
        builder.Append("    <sy:updatePeriod>hourly</sy:updatePeriod>\n");
        builder.Append("    <sy:updateFrequency>1</sy:updateFrequency>\n");
        builder.Append("    <dc:publisher>Example Corporation</dc:publisher>\n");
        builder.Append("    <itunes:author>Example Author</itunes:author>\n");

        for (int i = 0; i < itemCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("    <item>\n");
            builder.Append("      <title>Extension Article ").Append(ordinal).Append("</title>\n");
            builder.Append("      <link>https://example.com/article").Append(ordinal).Append("</link>\n");
            builder.Append("      <description>Body text for extension-bearing article ").Append(ordinal).Append(".</description>\n");
            builder.Append("      <pubDate>Mon, 01 Jan 2024 10:00:00 GMT</pubDate>\n");
            builder.Append("      <dc:creator>Article Author ").Append(ordinal).Append("</dc:creator>\n");
            builder.Append("      <dc:subject>technology</dc:subject>\n");
            builder.Append("      <dc:date>2024-01-01T10:00:00Z</dc:date>\n");
            builder.Append("      <dcterms:abstract>An abstract of article ").Append(ordinal).Append("</dcterms:abstract>\n");
            builder.Append("      <content:encoded><![CDATA[<p>Full content of article ").Append(ordinal)
                   .Append(" with <strong>HTML</strong> formatting.</p>]]></content:encoded>\n");
            builder.Append("      <slash:comments>42</slash:comments>\n");
            builder.Append("      <slash:department>tech</slash:department>\n");
            builder.Append("      <itunes:duration>00:31:00</itunes:duration>\n");
            builder.Append("      <media:thumbnail url=\"https://example.com/thumb").Append(ordinal).Append(".jpg\"/>\n");
            builder.Append("    </item>\n");
        }

        builder.Append("  </channel>\n</rss>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates an Atom 1.0 document with <paramref name="entryCount"/> entries, using the
    /// element mix of the repository's real Atom sample.
    /// </summary>
    /// <param name="entryCount">The number of <c>entry</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateAtomUtf8(int entryCount)
    {
        StringBuilder builder = new(capacity: 1024 + (entryCount * 640));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<feed xmlns=\"http://www.w3.org/2005/Atom\">\n");
        builder.Append("  <title type=\"text\">Synthetic Benchmark Feed</title>\n");
        builder.Append("  <subtitle type=\"html\">A &lt;em&gt;synthetic&lt;/em&gt; Atom feed</subtitle>\n");
        builder.Append("  <link href=\"https://example.com/\" rel=\"alternate\" type=\"text/html\"/>\n");
        builder.Append("  <link href=\"https://example.com/feed.atom\" rel=\"self\" type=\"application/atom+xml\"/>\n");
        builder.Append("  <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>\n");
        builder.Append("  <updated>2024-01-01T12:00:00Z</updated>\n");
        builder.Append("  <rights type=\"text\">Copyright 2026 Example Corporation.</rights>\n");
        builder.Append("  <generator uri=\"https://github.com/argotic-syndication-framework/Argotic\" version=\"4.0\">Argotic Benchmarks</generator>\n");
        builder.Append("  <author>\n    <name>Feed Author</name>\n    <email>author@example.com</email>\n  </author>\n");

        for (int i = 0; i < entryCount; i++)
        {
            AppendAtomEntry(builder, i);
        }

        builder.Append("</feed>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates a Sitemap 0.9 <c>urlset</c> with <paramref name="urlCount"/> entries.
    /// </summary>
    /// <param name="urlCount">The number of <c>url</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateSitemapUtf8(int urlCount)
    {
        StringBuilder builder = new(capacity: 512 + (urlCount * 192));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");

        for (int i = 0; i < urlCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <url>\n");
            builder.Append("    <loc>https://www.example.com/page").Append(ordinal).Append("</loc>\n");
            builder.Append("    <lastmod>2024-01-15</lastmod>\n");
            builder.Append("    <changefreq>daily</changefreq>\n");
            builder.Append("    <priority>0.8</priority>\n");
            builder.Append("  </url>\n");
        }

        builder.Append("</urlset>\n");

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

    /// <summary>
    /// Generates the RSS document of <see cref="GenerateRssUtf8"/> with invalid XML characters
    /// substituted into its element text at a controlled density.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <param name="dirtFraction">
    ///     The fraction of <b>eligible text characters</b> — lowercase ASCII letters outside any tag —
    ///     replaced by <c>U+0001</c>. Zero leaves the document clean.
    /// </param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    /// <para>
    /// <c>SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters</c> has two completely
    /// different costs. On a clean document it scans and returns the original instance — 0 B
    /// allocated. On a dirty one it rebuilds into a <see cref="StringBuilder"/>. <b>No generator in
    /// this file emitted an invalid character, so only the first was ever measured</b>, and a rewrite
    /// of the rebuild loop could be declared free on evidence that never touched it.
    /// </para>
    /// <para>
    /// Substituting rather than inserting keeps the document byte-identical in length across the
    /// sweep, so the arms differ in dirt density and nothing else. Only lowercase letters outside a
    /// tag are eligible, which keeps markup, element names and attribute names intact — the document
    /// must still parse once the sanitiser has run over it, or the benchmark measures an exception
    /// rather than a rebuild.
    /// </para>
    /// </remarks>
    public static byte[] GenerateRssWithDirtUtf8(int itemCount, double dirtFraction)
    {
        byte[] clean = GenerateRssUtf8(itemCount);
        if (dirtFraction <= 0)
        {
            return clean;
        }

        char[] characters = Encoding.UTF8.GetString(clean).ToCharArray();
        int stride = (int)Math.Round(1.0 / dirtFraction, MidpointRounding.AwayFromZero);
        bool insideTag = false;
        int eligible = 0;

        for (int i = 0; i < characters.Length; i++)
        {
            switch (characters[i])
            {
                case '<':
                    insideTag = true;
                    continue;
                case '>':
                    insideTag = false;
                    continue;
            }

            if (insideTag || characters[i] is < 'a' or > 'z')
            {
                continue;
            }

            if (++eligible % stride == 0)
            {
                characters[i] = '\u0001';
            }
        }

        return Encoding.UTF8.GetBytes(characters);
    }

    /// <summary>
    /// Generates the degenerate document whose content is entirely invalid but for one character.
    /// </summary>
    /// <param name="nulCount">The number of <c>U+0000</c> characters between the root tags.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    ///     This is the shape a streaming sanitiser can silently truncate. A chunk consisting entirely
    ///     of dropped characters must make the reader loop and read again, never return zero, because
    ///     <c>XmlTextReaderImpl.ReadData</c> treats a zero-length read as end of file — and
    ///     <see cref="TextReader.Read(char[], int, int)"/> documents zero as meaning no characters are
    ///     left. Corrupt feeds do contain long runs of NUL. The cost of that loop-again path has never
    ///     been measured, because a 0–1% dirt sweep never produces a wholly-dropped chunk.
    /// </remarks>
    public static byte[] GenerateAllInvalidDocument(int nulCount)
        => Encoding.UTF8.GetBytes(string.Concat("<r>", new string('\0', nulCount), "x</r>"));

    /// <summary>
    /// Generates an RSS document that is valid XML throughout but carries astral characters.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    /// <para>
    /// A <c>SearchValues&lt;char&gt;</c> built from the complement of <c>XmlConvert.IsXmlChar</c>
    /// contains the whole of <c>[D800-DFFF]</c>, because a surrogate is not a valid XML character on
    /// its own. So <c>IndexOfAny</c> fires on <b>every astral character</b> and the vectorised fast
    /// path never completes over a document containing one — the char-by-char pair rule has to run.
    /// </para>
    /// <para>
    /// A 7-bit "clean" baseline therefore measures the best case and calls it the normal one. Emoji
    /// are ordinary in feed titles; this generator puts them where a real feed does, so the clean-path
    /// claim is tested against an input that can refute it.
    /// </para>
    /// </remarks>
    public static byte[] GenerateRssWithAstralUtf8(int itemCount)
    {
        byte[] clean = GenerateRssUtf8(itemCount);
        string document = Encoding.UTF8.GetString(clean);

        // One astral character per item title and one per description, which is the density of a
        // podcast or newsletter feed rather than a pathological case.
        document = document
            .Replace("<title>Synthetic Article ", "<title>\U0001F4E1 Synthetic Article ", StringComparison.Ordinal)
            .Replace("<description>Body text for synthetic article ", "<description>\U0001F4DD Body text for synthetic article ", StringComparison.Ordinal);

        return Encoding.UTF8.GetBytes(document);
    }

    private static void AppendAtomEntry(StringBuilder builder, int index)
    {
        // Mirrors the real sample's variation: content type alternates text/html, and only some
        // entries carry an entry-level author or a second category.
        int shape = index % 3;
        string ordinal = index.ToString(CultureInfo.InvariantCulture);

        builder.Append("  <entry>\n");
        builder.Append(shape == 1
            ? "    <title type=\"html\">&lt;strong&gt;Synthetic Entry " + ordinal + "&lt;/strong&gt;</title>\n"
            : "    <title type=\"text\">Synthetic Entry " + ordinal + "</title>\n");
        builder.Append("    <link href=\"https://example.com/entry").Append(ordinal).Append("\" rel=\"alternate\" type=\"text/html\"/>\n");
        builder.Append("    <link href=\"https://example.com/entry").Append(ordinal).Append("/related\" rel=\"related\"/>\n");
        builder.Append("    <id>urn:uuid:1225c695-cfb8-4ebb-aaaa-").Append(ordinal.PadLeft(12, '0')).Append("</id>\n");
        builder.Append("    <published>2024-01-01T09:00:00Z</published>\n");
        builder.Append("    <updated>2024-01-01T10:00:00Z</updated>\n");
        builder.Append("    <summary type=\"text\">Summary for synthetic entry ").Append(ordinal).Append(".</summary>\n");
        builder.Append("    <content type=\"text\">Body content for synthetic entry ").Append(ordinal)
               .Append(", long enough to resemble a real entry rather than a stub.</content>\n");

        if (shape != 2)
        {
            builder.Append("    <author>\n      <name>Entry Author ").Append(ordinal)
                   .Append("</name>\n      <email>entry").Append(ordinal).Append("@example.com</email>\n    </author>\n");
        }

        builder.Append("    <category term=\"synthetic\" label=\"Synthetic\"/>\n");

        if (shape == 0)
        {
            builder.Append("    <category term=\"example\" scheme=\"https://example.com/tags\"/>\n");
        }

        builder.Append("  </entry>\n");
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