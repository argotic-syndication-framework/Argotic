using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds the extension-focused benchmark inputs: the modern podcast feed, the
/// declared-but-unused production shape, and the deliberately-maximal every-family document.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FeedCorpus.GenerateRssWithExtensionsUtf8"/> emits six families' simplest elements,
/// and that single choice left nine families and every rich element mix without a benchmark
/// (<c>.endjin/build-warnings.md</c> §19). The three generators here close that from opposite
/// directions: census-ratio realism for the family 62% of live feeds declare, the ~96% production
/// shape in which namespaces are declared but never used, and a ceiling document that exercises
/// every collection-bearing element the extension families model.
/// </para>
/// <para>
/// Element mixes cite their sources: the committed <c>SampleData/PodcastFeed.xml</c> and the
/// per-element census in <c>.endjin/build-warnings.md</c> §2.50 (podcast), the 627-feed
/// declared-vs-used census in the corpus manifest (declared-only), and the scenario-test fixtures
/// (YouTube's <c>media:group</c>, the FeedSync spec example, the LiveJournal userpic) for the
/// maximal arm.
/// </para>
/// </remarks>
internal static class ExtensionFeedCorpus
{
    /// <summary>
    /// Generates a Podcasting 2.0 feed with iTunes at genuine podcast density.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    /// <para>
    ///     Channel elements follow §2.50's adoption order (locked, guid, medium, podping, txt,
    ///     funding, person); the per-item <c>podcast:transcript</c> cycles the three MIME types
    ///     that dominate the 396-feed validation — <c>text/vtt</c>, <c>text/plain</c> and the
    ///     unregistered <c>application/srt</c> — because a corpus that only emits the registered
    ///     types would flatter any type-keyed handling. <c>podcast:license</c> carries a
    ///     copyright-notice value rather than an SPDX identifier, which is what publishers
    ///     actually put there. The namespace is the https:// form, matched exactly: a feed
    ///     declaring http:// silently yields no extension at all, which is precisely the drift
    ///     the consuming benchmark's guard exists to catch.
    /// </para>
    /// <para>
    ///     iTunes rides along at The Daily's measured density (23,525 elements over 2,939 items —
    ///     roughly eight per item plus the channel set), including the nested
    ///     <c>itunes:category</c> and <c>itunes:owner</c> that light the two element classes
    ///     nothing else reaches.
    /// </para>
    /// </remarks>
    public static byte[] GeneratePodcastFeedUtf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 2048 + (itemCount * 1536));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<rss version=\"2.0\"\n");
        builder.Append("     xmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\n");
        builder.Append("     xmlns:podcast=\"https://podcastindex.org/namespace/1.0\"\n");
        builder.Append("     xmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\n");
        builder.Append("     xmlns:media=\"http://search.yahoo.com/mrss/\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Synthetic Podcast Feed</title>\n");
        builder.Append("    <link>https://example.com/podcast</link>\n");
        builder.Append("    <description>A synthetic Podcasting 2.0 feed at census-ratio element density.</description>\n");
        builder.Append("    <language>en-us</language>\n");
        builder.Append("    <podcast:locked owner=\"owner@example.com\">yes</podcast:locked>\n");
        builder.Append("    <podcast:guid>917393e3-1b1e-5cef-ace4-edaa54e1f810</podcast:guid>\n");
        builder.Append("    <podcast:medium>podcast</podcast:medium>\n");
        builder.Append("    <podcast:podping usesPodping=\"true\"/>\n");
        builder.Append("    <podcast:txt purpose=\"applepodcastsverify\">e6cfaae0-9496-11f0-a272-f9e230f88be0</podcast:txt>\n");
        builder.Append("    <podcast:funding url=\"https://example.com/support\">Support the show</podcast:funding>\n");
        builder.Append("    <podcast:person role=\"host\" href=\"https://example.com/hosts/alice\">Alice Host</podcast:person>\n");
        builder.Append("    <podcast:person role=\"guest\" href=\"https://example.com/guests/bob\">Bob Guest</podcast:person>\n");
        builder.Append("    <itunes:author>Example Productions</itunes:author>\n");
        builder.Append("    <itunes:category text=\"Technology\">\n");
        builder.Append("      <itunes:category text=\"Tech News\"/>\n");
        builder.Append("    </itunes:category>\n");
        builder.Append("    <itunes:owner>\n");
        builder.Append("      <itunes:name>Example Productions</itunes:name>\n");
        builder.Append("      <itunes:email>owner@example.com</itunes:email>\n");
        builder.Append("    </itunes:owner>\n");
        builder.Append("    <itunes:type>episodic</itunes:type>\n");
        builder.Append("    <itunes:new-feed-url>https://example.com/podcast/feed.xml</itunes:new-feed-url>\n");
        builder.Append("    <itunes:explicit>false</itunes:explicit>\n");
        builder.Append("    <itunes:image href=\"https://example.com/artwork.png\"/>\n");

        for (int i = 0; i < itemCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            string transcriptType = (i % 3) switch
            {
                0 => "text/vtt",
                1 => "text/plain",
                _ => "application/srt",
            };

            builder.Append("    <item>\n");
            builder.Append("      <title>Episode ").Append(ordinal).Append("</title>\n");
            builder.Append("      <link>https://example.com/podcast/episode").Append(ordinal).Append("</link>\n");
            builder.Append("      <description>Show notes for episode ").Append(ordinal).Append(".</description>\n");
            builder.Append("      <guid isPermaLink=\"false\">episode-").Append(ordinal).Append("</guid>\n");
            builder.Append("      <pubDate>Mon, 01 Jan 2024 10:00:00 GMT</pubDate>\n");
            builder.Append("      <enclosure url=\"https://example.com/media/episode").Append(ordinal)
                   .Append(".mp3\" length=\"36588921\" type=\"audio/mpeg\"/>\n");
            builder.Append("      <podcast:season name=\"Season One\">1</podcast:season>\n");
            builder.Append(i % 4 == 3
                ? "      <podcast:episode display=\"Bonus\">" + ordinal + ".1</podcast:episode>\n"
                : "      <podcast:episode display=\"Ep. " + ordinal + "\">" + ordinal + "</podcast:episode>\n");
            builder.Append("      <podcast:transcript url=\"https://example.com/podcast/episode").Append(ordinal)
                   .Append("/transcript\" type=\"").Append(transcriptType).Append("\" language=\"en\" rel=\"captions\"/>\n");
            builder.Append("      <podcast:person role=\"host\" href=\"https://example.com/hosts/alice\">Alice Host</podcast:person>\n");

            if (i % 8 == 0)
            {
                builder.Append("      <podcast:chapters url=\"https://example.com/podcast/episode").Append(ordinal)
                       .Append("/chapters.json\" type=\"application/json+chapters\"/>\n");
            }

            if (i % 16 == 0)
            {
                // The value publishers actually write: a copyright notice, not an SPDX identifier.
                builder.Append("      <podcast:license url=\"https://example.com/license\">Copyright 2026 Example Productions</podcast:license>\n");
            }

            builder.Append("      <itunes:title>Episode ").Append(ordinal).Append("</itunes:title>\n");
            builder.Append("      <itunes:episodeType>full</itunes:episodeType>\n");
            builder.Append("      <itunes:duration>00:42:").Append((i % 60).ToString("D2", CultureInfo.InvariantCulture)).Append("</itunes:duration>\n");
            builder.Append("      <itunes:explicit>false</itunes:explicit>\n");
            builder.Append("      <itunes:author>Example Productions</itunes:author>\n");
            builder.Append("      <itunes:subtitle>A short teaser for episode ").Append(ordinal).Append("</itunes:subtitle>\n");
            builder.Append("      <itunes:summary>A longer summary of what episode ").Append(ordinal).Append(" covers.</itunes:summary>\n");
            builder.Append("      <itunes:image href=\"https://example.com/artwork/episode").Append(ordinal).Append(".png\"/>\n");
            builder.Append("      <content:encoded><![CDATA[<p>Full show notes for episode ").Append(ordinal)
                   .Append(" with <a href=\"https://example.com\">links</a>.</p>]]></content:encoded>\n");
            builder.Append("      <media:thumbnail url=\"https://example.com/thumbs/episode").Append(ordinal).Append(".jpg\"/>\n");
            builder.Append("    </item>\n");
        }

        builder.Append("  </channel>\n</rss>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates the shape most production loads actually take: nine extension namespaces
    /// declared, five families used.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    ///     The 627-feed census: <c>dc</c> declared by 404 and used by 382, <c>content</c> 401/321,
    ///     <c>sy</c> 296/293, <c>wfw</c> 306/225, <c>slash</c> 298/225 — and <c>geo</c>
    ///     <b>121/5</b>, <c>georss</c> <b>144/4</b>, <c>creativeCommons</c> 28/1. WordPress
    ///     stamps the declarations into every response whether or not a plugin ever writes an
    ///     element, and detection answers on the prefix alone, so the unused declarations force
    ///     extension loads that find nothing on every entity. This generator emits exactly that:
    ///     the five used families' everyday elements, and not one element of the other four.
    /// </remarks>
    public static byte[] GenerateDeclaredOnlyProductionUtf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 1536 + (itemCount * 640));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<rss version=\"2.0\"\n");
        builder.Append("     xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n");
        builder.Append("     xmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\n");
        builder.Append("     xmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\n");
        builder.Append("     xmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\n");
        builder.Append("     xmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\n");
        builder.Append("     xmlns:media=\"http://search.yahoo.com/mrss/\"\n");
        builder.Append("     xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\"\n");
        builder.Append("     xmlns:georss=\"http://www.georss.org/georss\"\n");
        builder.Append("     xmlns:creativeCommons=\"http://backend.userland.com/creativeCommonsRssModule\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Production Weblog</title>\n");
        builder.Append("    <link>https://example.com/blog</link>\n");
        builder.Append("    <description>A synthetic feed in the WordPress declared-but-unused shape.</description>\n");
        builder.Append("    <language>en-us</language>\n");
        builder.Append("    <sy:updatePeriod>hourly</sy:updatePeriod>\n");
        builder.Append("    <sy:updateFrequency>1</sy:updateFrequency>\n");

        for (int i = 0; i < itemCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("    <item>\n");
            builder.Append("      <title>Post ").Append(ordinal).Append("</title>\n");
            builder.Append("      <link>https://example.com/blog/post").Append(ordinal).Append("</link>\n");
            builder.Append("      <description>Excerpt of post ").Append(ordinal).Append(".</description>\n");
            builder.Append("      <pubDate>Mon, 01 Jan 2024 10:00:00 GMT</pubDate>\n");
            builder.Append("      <guid isPermaLink=\"true\">https://example.com/blog/post").Append(ordinal).Append("</guid>\n");
            builder.Append("      <dc:creator>Blog Author</dc:creator>\n");
            builder.Append("      <content:encoded><![CDATA[<p>The full body of post ").Append(ordinal)
                   .Append(" with <strong>markup</strong>.</p>]]></content:encoded>\n");
            builder.Append("      <wfw:commentRss>https://example.com/blog/post").Append(ordinal).Append("/comments/feed</wfw:commentRss>\n");
            builder.Append("      <slash:comments>7</slash:comments>\n");
            builder.Append("    </item>\n");
        }

        builder.Append("  </channel>\n</rss>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates the ceiling document: every framework family the RSS surface can carry, with
    /// every collection-bearing element populated.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    /// <para>
    ///     Deliberately a fiction — no real feed looks like this, and no number measured over it
    ///     may be quoted as production-relevant. It exists so that every extension element class
    ///     has a price at all. Three honesty labels carried over from the corpus manifest:
    ///     FeedSync, SimpleList and FeedRank exist in the wild only as their spec examples, so
    ///     those are the shapes emitted; LiveJournal is emitted under Argotic's 2.0 namespace URI
    ///     because real LiveJournal feeds use the 1.0 URI this library does not match — the arm
    ///     prices code paths, not live traffic; and <c>media:group</c> is the YouTube shape, the
    ///     only real publisher of the element.
    /// </para>
    /// <para>
    ///     Podcasting 2.0 is deliberately absent: it has its own census-ratio generator, and a
    ///     second copy here would price the same paths twice.
    /// </para>
    /// </remarks>
    public static byte[] GenerateMaximalExtensionFeedUtf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 4096 + (itemCount * 2560));

        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        builder.Append("<rss version=\"2.0\"\n");
        builder.Append("     xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n");
        builder.Append("     xmlns:dcterms=\"http://purl.org/dc/terms/\"\n");
        builder.Append("     xmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\n");
        builder.Append("     xmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\n");
        builder.Append("     xmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\n");
        builder.Append("     xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\"\n");
        builder.Append("     xmlns:georss=\"http://www.georss.org/georss\"\n");
        builder.Append("     xmlns:blogChannel=\"http://backend.userland.com/blogChannelModule\"\n");
        builder.Append("     xmlns:creativeCommons=\"http://backend.userland.com/creativeCommonsRssModule\"\n");
        builder.Append("     xmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\n");
        builder.Append("     xmlns:media=\"http://search.yahoo.com/mrss/\"\n");
        builder.Append("     xmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\n");
        builder.Append("     xmlns:trackback=\"http://madskills.com/public/xml/rss/module/trackback/\"\n");
        builder.Append("     xmlns:pingback=\"http://madskills.com/public/xml/rss/module/pingback/\"\n");
        builder.Append("     xmlns:lj=\"http://livejournal.org/rss/lj/2.0/\"\n");
        builder.Append("     xmlns:sx=\"http://feedsync.org/2007/feedsync\"\n");
        builder.Append("     xmlns:cf=\"http://www.microsoft.com/schemas/rss/core/2005\"\n");
        builder.Append("     xmlns:fh=\"http://purl.org/syndication/history/1.0\"\n");
        builder.Append("     xmlns:re=\"http://purl.org/atompub/rank/1.0\"\n");
        builder.Append("     xmlns:photo=\"http://www.pheed.com/pheed/\"\n");
        builder.Append("     xmlns:atom=\"http://www.w3.org/2005/Atom\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Maximal Extension Feed</title>\n");
        builder.Append("    <link>https://example.com/</link>\n");
        builder.Append("    <description>The ceiling document: every family, every collection-bearing element.</description>\n");
        builder.Append("    <language>en-us</language>\n");
        builder.Append("    <sy:updatePeriod>hourly</sy:updatePeriod>\n");
        builder.Append("    <sy:updateFrequency>1</sy:updateFrequency>\n");
        builder.Append("    <sy:updateBase>2024-01-01T00:00:00Z</sy:updateBase>\n");
        builder.Append("    <blogChannel:blogRoll>https://example.com/blogroll.opml</blogChannel:blogRoll>\n");
        builder.Append("    <blogChannel:blink>https://example.com/recommended</blogChannel:blink>\n");
        builder.Append("    <creativeCommons:license>https://creativecommons.org/licenses/by/4.0/</creativeCommons:license>\n");
        builder.Append("    <fh:archive/>\n");
        builder.Append("    <atom:link rel=\"current\" href=\"https://example.com/feed.xml\"/>\n");
        builder.Append("    <atom:link rel=\"prev-archive\" href=\"https://example.com/archive/2023-12.xml\"/>\n");
        builder.Append("    <cf:treatAs>list</cf:treatAs>\n");
        builder.Append("    <cf:listinfo>\n");
        builder.Append("      <cf:sort ns=\"\" element=\"pubDate\" label=\"Date\" data-type=\"date\" default=\"true\"/>\n");
        builder.Append("      <cf:sort ns=\"http://purl.org/dc/elements/1.1/\" element=\"creator\" label=\"Author\" data-type=\"text\"/>\n");
        builder.Append("      <cf:group ns=\"\" element=\"category\" label=\"Category\"/>\n");
        builder.Append("    </cf:listinfo>\n");
        builder.Append("    <sx:sharing since=\"2024-01-01T00:00:00Z\" until=\"2024-02-01T00:00:00Z\">\n");
        builder.Append("      <sx:related link=\"https://example.com/feed-complete.xml\" type=\"complete\"/>\n");
        builder.Append("    </sx:sharing>\n");
        builder.Append("    <itunes:author>Example Productions</itunes:author>\n");
        builder.Append("    <itunes:category text=\"Technology\">\n");
        builder.Append("      <itunes:category text=\"Tech News\"/>\n");
        builder.Append("    </itunes:category>\n");
        builder.Append("    <itunes:owner>\n");
        builder.Append("      <itunes:name>Example Productions</itunes:name>\n");
        builder.Append("      <itunes:email>owner@example.com</itunes:email>\n");
        builder.Append("    </itunes:owner>\n");

        for (int i = 0; i < itemCount; i++)
        {
            AppendMaximalItem(builder, i);
        }

        builder.Append("  </channel>\n</rss>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static void AppendMaximalItem(StringBuilder builder, int index)
    {
        string ordinal = index.ToString(CultureInfo.InvariantCulture);

        builder.Append("    <item>\n");
        builder.Append("      <title>Maximal Article ").Append(ordinal).Append("</title>\n");
        builder.Append("      <link>https://example.com/article").Append(ordinal).Append("</link>\n");
        builder.Append("      <description>Body text for maximal article ").Append(ordinal).Append(".</description>\n");
        builder.Append("      <pubDate>Mon, 01 Jan 2024 10:00:00 GMT</pubDate>\n");
        builder.Append("      <guid isPermaLink=\"true\">https://example.com/article").Append(ordinal).Append("</guid>\n");

        // The everyday families, on every item.
        builder.Append("      <dc:creator>Article Author ").Append(ordinal).Append("</dc:creator>\n");
        builder.Append("      <dc:subject>technology</dc:subject>\n");
        builder.Append("      <dc:date>2024-01-01T10:00:00Z</dc:date>\n");
        builder.Append("      <dcterms:abstract>An abstract of article ").Append(ordinal).Append("</dcterms:abstract>\n");
        builder.Append("      <dcterms:modified>2024-01-02T10:00:00Z</dcterms:modified>\n");
        builder.Append("      <content:encoded><![CDATA[<p>Full content of article ").Append(ordinal).Append(".</p>]]></content:encoded>\n");
        builder.Append("      <slash:section>articles</slash:section>\n");
        builder.Append("      <slash:department>benchmarking</slash:department>\n");
        builder.Append("      <slash:comments>42</slash:comments>\n");
        builder.Append("      <slash:hit_parade>42,30,20,10,5,2,1</slash:hit_parade>\n");
        builder.Append("      <wfw:comment>https://example.com/article").Append(ordinal).Append("/comment</wfw:comment>\n");
        builder.Append("      <wfw:commentRss>https://example.com/article").Append(ordinal).Append("/comments/feed</wfw:commentRss>\n");
        builder.Append("      <trackback:ping>https://example.com/article").Append(ordinal).Append("/trackback</trackback:ping>\n");
        builder.Append("      <trackback:about>https://example.com/article").Append(ordinal).Append("/trackbacks</trackback:about>\n");
        builder.Append("      <pingback:server>https://example.com/pingback</pingback:server>\n");
        builder.Append("      <pingback:target>https://example.com/article").Append(ordinal).Append("</pingback:target>\n");
        builder.Append("      <geo:lat>51.5074</geo:lat>\n");
        builder.Append("      <geo:long>-0.1278</geo:long>\n");
        builder.Append("      <georss:point>51.5074 -0.1278</georss:point>\n");
        builder.Append("      <re:rank scheme=\"http://example.com/rank-scheme\" domain=\"https://example.com/\">").Append(ordinal).Append("</re:rank>\n");
        builder.Append("      <photo:thumbnail>https://example.com/photo/thumb").Append(ordinal).Append(".jpg</photo:thumbnail>\n");
        builder.Append("      <photo:imgsrc>https://example.com/photo/full").Append(ordinal).Append(".jpg</photo:imgsrc>\n");
        builder.Append("      <lj:security>public</lj:security>\n");
        builder.Append("      <lj:mood>productive</lj:mood>\n");
        builder.Append("      <lj:music>Around the World</lj:music>\n");
        builder.Append("      <lj:userpic><url>https://example.com/userpic.jpg</url><keyword>coding</keyword><width>100</width><height>100</height></lj:userpic>\n");
        builder.Append("      <media:thumbnail url=\"https://example.com/thumb").Append(ordinal).Append(".jpg\"/>\n");
        builder.Append("      <sx:sync id=\"article-").Append(ordinal).Append("\" updates=\"3\">\n");
        builder.Append("        <sx:history sequence=\"3\" when=\"2024-01-03T10:00:00Z\" by=\"editor\"/>\n");
        builder.Append("        <sx:history sequence=\"2\" when=\"2024-01-02T10:00:00Z\" by=\"author\"/>\n");
        builder.Append("        <sx:history sequence=\"1\" when=\"2024-01-01T10:00:00Z\" by=\"author\"/>\n");
        builder.Append("      </sx:sync>\n");

        // The geometry showcase, once per document: the GeoRSS shapes beyond a point.
        if (index == 0)
        {
            builder.Append("      <georss:line>45.256 -110.45 46.46 -109.48 43.84 -109.86</georss:line>\n");
            builder.Append("      <georss:polygon>45.256 -110.45 46.46 -109.48 43.84 -109.86 45.256 -110.45</georss:polygon>\n");
            builder.Append("      <georss:box>42.943 -71.032 43.039 -69.856</georss:box>\n");
            builder.Append("      <georss:featurename>Example Valley</georss:featurename>\n");
            builder.Append("      <georss:elev>313</georss:elev>\n");
        }

        // The Yahoo Media long tail, once per document - the census finds each of these "once or
        // twice across all eight" real documents.
        if (index == 1)
        {
            builder.Append("      <media:hash algo=\"md5\">dfdec888b72151965a34b4b59e290372</media:hash>\n");
            builder.Append("      <media:player url=\"https://example.com/player?article=1\" height=\"390\" width=\"640\"/>\n");
            builder.Append("      <media:category scheme=\"http://search.yahoo.com/mrss/category_schema\">technology/benchmarks</media:category>\n");
            builder.Append("      <media:copyright url=\"https://example.com/copyright\">2026 Example Corporation</media:copyright>\n");
            builder.Append("      <media:credit role=\"author\" scheme=\"urn:ebu\">Alice Author</media:credit>\n");
            builder.Append("      <media:rating scheme=\"urn:simple\">nonadult</media:rating>\n");
            builder.Append("      <media:restriction relationship=\"allow\" type=\"country\">gb us</media:restriction>\n");
            builder.Append("      <media:text type=\"plain\">A caption for the media object.</media:text>\n");
        }

        // The YouTube media:group shape, once per document - the only real publisher of the element.
        if (index == 2)
        {
            builder.Append("      <media:group>\n");
            builder.Append("        <media:title>A Video</media:title>\n");
            builder.Append("        <media:content url=\"https://example.com/v/abc\" type=\"video/mp4\" width=\"640\" height=\"390\"/>\n");
            builder.Append("        <media:content url=\"https://example.com/v/abc-hd\" type=\"video/mp4\" width=\"1280\" height=\"720\"/>\n");
            builder.Append("        <media:content url=\"https://example.com/a/abc\" type=\"audio/mp4\"/>\n");
            builder.Append("        <media:thumbnail url=\"https://example.com/vi/abc/hqdefault.jpg\" width=\"480\" height=\"360\"/>\n");
            builder.Append("        <media:description>A description of the video.</media:description>\n");
            builder.Append("      </media:group>\n");
        }

        builder.Append("    </item>\n");
    }
}