using System.Globalization;
using System.Text;

namespace Argotic.Benchmarks;

/// <summary>
/// Builds the legacy-format benchmark inputs: the RSS versions before 2.0, Atom 0.3 and RSD 0.6.
/// </summary>
/// <remarks>
/// <para>
/// These formats had zero benchmark coverage while their adapters total 1,520 lines
/// (<c>.endjin/build-warnings.md</c> §19) — every load benchmark parsed RSS 2.0 or Atom 1.0, so
/// the version-dispatch cost and the RDF-shaped parse had no price at all. Element mixes are
/// modelled on the test fixtures in <c>FeedTestData.cs</c> (which the benchmarks project cannot
/// reference) and on the real 1999–2005 documents in the gitignored perf-spike corpus: BBC's RSS
/// 0.91 ships ISO-8859-1 with a DOCTYPE, Slashdot's RSS 0.90 carries no version attribute and is
/// recognised by namespace alone, and RSS 1.0 aggregators grow an <c>rdf:Seq</c> manifest in
/// lockstep with their items.
/// </para>
/// <para>
/// Deliberately absent: a <c>version="0.94"</c> generator, even though one real feed shipped that
/// string. The dispatch supports 2.0, 1.0, 0.92, 0.91 and 0.9 and throws for anything else, so
/// the arm would measure an exception rather than a load, and a parse-verify guard on it could
/// never pass.
/// </para>
/// </remarks>
internal static class LegacyFeedCorpus
{
    /// <summary>
    /// Generates an RSS 0.90 document: RDF root, Netscape namespace, and no version attribute —
    /// the format is recognised by namespace sniffing alone.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateRss090Utf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 768 + (itemCount * 128));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"\n");
        builder.Append("         xmlns=\"http://my.netscape.com/rdf/simple/0.9/\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Legacy Benchmark Feed 0.90</title>\n");
        builder.Append("    <link>http://example.com</link>\n");
        builder.Append("    <description>A synthetic RSS 0.90 feed in the Netscape My.Netscape.Com shape.</description>\n");
        builder.Append("  </channel>\n");
        builder.Append("  <image>\n");
        builder.Append("    <title>Feed Image</title>\n");
        builder.Append("    <url>http://example.com/image.png</url>\n");
        builder.Append("    <link>http://example.com</link>\n");
        builder.Append("  </image>\n");

        for (int i = 0; i < itemCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <item>\n");
            builder.Append("    <title>Headline ").Append(ordinal).Append("</title>\n");
            builder.Append("    <link>http://example.com/item").Append(ordinal).Append("</link>\n");
            builder.Append("  </item>\n");
        }

        builder.Append("  <textinput>\n");
        builder.Append("    <title>Search</title>\n");
        builder.Append("    <description>Search the feed</description>\n");
        builder.Append("    <name>query</name>\n");
        builder.Append("    <link>http://example.com/search</link>\n");
        builder.Append("  </textinput>\n");
        builder.Append("</rdf:RDF>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates a full RSS 0.91 document: rating, skipHours/skipDays, textInput and a
    /// width/height image — the complete optional-element surface of the format.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateRss091Utf8(int itemCount)
        => Encoding.UTF8.GetBytes(BuildRss091Document(itemCount, withDoctype: false, latin1Accents: false));

    /// <summary>
    /// Generates the same RSS 0.91 document as it actually travelled in 1999: ISO-8859-1 bytes, a
    /// PUBLIC Netscape DOCTYPE, and Latin-1 accented characters in the text.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as ISO-8859-1 bytes.</returns>
    /// <remarks>
    ///     The corpus's real 0.91 feeds (BBC, Wired, xml.com) are ISO-8859-1 and carry SYSTEM or
    ///     PUBLIC DOCTYPEs; a UTF-8-only corpus can never reach the encoding-detection and
    ///     DTD-handling paths those documents exercise on every load.
    /// </remarks>
    public static byte[] GenerateRss091Iso88591WithDoctype(int itemCount)
        => Encoding.GetEncoding("ISO-8859-1").GetBytes(BuildRss091Document(itemCount, withDoctype: true, latin1Accents: true));

    /// <summary>
    /// Generates a full RSS 0.92 document: everything 0.91 carried plus <c>cloud</c>, and per-item
    /// <c>enclosure</c>/<c>category</c>/<c>source</c> cycling the way real items vary.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateRss092Utf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 1536 + (itemCount * 384));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<rss version=\"0.92\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Legacy Benchmark Feed 0.92</title>\n");
        builder.Append("    <link>http://example.com</link>\n");
        builder.Append("    <description>A synthetic RSS 0.92 feed with the cloud and enclosure surface.</description>\n");
        builder.Append("    <language>en-us</language>\n");
        builder.Append("    <copyright>Copyright 2026 Example Corporation</copyright>\n");
        builder.Append("    <managingEditor>editor@example.com</managingEditor>\n");
        builder.Append("    <webMaster>webmaster@example.com</webMaster>\n");
        builder.Append("    <pubDate>Mon, 01 Jan 2024 08:00:00 GMT</pubDate>\n");
        builder.Append("    <lastBuildDate>Mon, 01 Jan 2024 12:00:00 GMT</lastBuildDate>\n");
        builder.Append("    <cloud domain=\"rpc.example.com\" port=\"80\" path=\"/RPC2\" registerProcedure=\"pingMe\" protocol=\"soap\"/>\n");
        builder.Append("    <image>\n");
        builder.Append("      <title>Feed Image</title>\n");
        builder.Append("      <url>http://example.com/image.png</url>\n");
        builder.Append("      <link>http://example.com</link>\n");
        builder.Append("      <width>88</width>\n");
        builder.Append("      <height>31</height>\n");
        builder.Append("    </image>\n");
        builder.Append("    <skipHours>\n      <hour>1</hour>\n      <hour>2</hour>\n    </skipHours>\n");
        builder.Append("    <skipDays>\n      <day>Saturday</day>\n      <day>Sunday</day>\n    </skipDays>\n");

        for (int i = 0; i < itemCount; i++)
        {
            int shape = i % 3;
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("    <item>\n");
            builder.Append("      <title>Article ").Append(ordinal).Append("</title>\n");
            builder.Append("      <link>http://example.com/item").Append(ordinal).Append("</link>\n");
            builder.Append("      <description>Body text for article ").Append(ordinal).Append(".</description>\n");

            if (shape == 0)
            {
                builder.Append("      <enclosure url=\"http://example.com/media/episode").Append(ordinal)
                       .Append(".mp3\" length=\"12345678\" type=\"audio/mpeg\"/>\n");
            }

            builder.Append("      <category>Technology</category>\n");

            if (shape != 2)
            {
                builder.Append("      <category domain=\"http://example.com/categories\">News</category>\n");
            }

            if (shape == 1)
            {
                builder.Append("      <source url=\"http://other.example.com/feed.xml\">Other Feed</source>\n");
            }

            builder.Append("    </item>\n");
        }

        builder.Append("  </channel>\n</rss>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates an RSS 1.0 (RDF) document whose <c>rdf:Seq</c> manifest grows with the item
    /// count, with Dublin Core at its native RSS 1.0 density and the slash quartet on one item in
    /// ten — the Slashdot shape.
    /// </summary>
    /// <param name="itemCount">The number of <c>item</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    /// <remarks>
    ///     The RDF bookkeeping is the point: every item is listed twice (once as an
    ///     <c>rdf:li</c> in the channel manifest, once as the item itself), which is the parse
    ///     shape no RSS 2.0 corpus can price. Dublin Core rides along because RSS 1.0 is its
    ///     native habitat — the corpus's aggregated RDF feeds carry <c>dc:</c> on every item.
    /// </remarks>
    public static byte[] GenerateRss10Utf8(int itemCount)
    {
        StringBuilder builder = new(capacity: 1536 + (itemCount * 512));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"\n");
        builder.Append("         xmlns=\"http://purl.org/rss/1.0/\"\n");
        builder.Append("         xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n");
        builder.Append("         xmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\n");
        builder.Append("         xmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\">\n");
        builder.Append("  <channel rdf:about=\"http://example.com\">\n");
        builder.Append("    <title>Legacy Benchmark Feed 1.0</title>\n");
        builder.Append("    <link>http://example.com</link>\n");
        builder.Append("    <description>A synthetic RSS 1.0 feed in the aggregated RDF shape.</description>\n");
        builder.Append("    <dc:publisher>Example Corporation</dc:publisher>\n");
        builder.Append("    <items>\n      <rdf:Seq>\n");

        for (int i = 0; i < itemCount; i++)
        {
            builder.Append("        <rdf:li resource=\"http://example.com/item")
                   .Append(i.ToString(CultureInfo.InvariantCulture)).Append("\"/>\n");
        }

        builder.Append("      </rdf:Seq>\n    </items>\n");
        builder.Append("  </channel>\n");
        builder.Append("  <image rdf:about=\"http://example.com/image.png\">\n");
        builder.Append("    <title>Feed Image</title>\n");
        builder.Append("    <url>http://example.com/image.png</url>\n");
        builder.Append("    <link>http://example.com</link>\n");
        builder.Append("  </image>\n");

        for (int i = 0; i < itemCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <item rdf:about=\"http://example.com/item").Append(ordinal).Append("\">\n");
            builder.Append("    <title>Aggregated Article ").Append(ordinal).Append("</title>\n");
            builder.Append("    <link>http://example.com/item").Append(ordinal).Append("</link>\n");
            builder.Append("    <description>Body text for aggregated article ").Append(ordinal).Append(".</description>\n");
            builder.Append("    <dc:creator>Author ").Append(ordinal).Append("</dc:creator>\n");
            builder.Append("    <dc:creator>Second Author</dc:creator>\n");
            builder.Append("    <dc:date>2024-01-01T10:00:00Z</dc:date>\n");
            builder.Append("    <dc:identifier>http://example.com/item").Append(ordinal).Append("</dc:identifier>\n");
            builder.Append("    <dc:source>http://origin.example.com/feed</dc:source>\n");
            builder.Append("    <content:encoded><![CDATA[<p>Full content of aggregated article ").Append(ordinal)
                   .Append(".</p>]]></content:encoded>\n");

            if (i % 10 == 0)
            {
                builder.Append("    <slash:section>articles</slash:section>\n");
                builder.Append("    <slash:department>benchmarking</slash:department>\n");
                builder.Append("    <slash:comments>42</slash:comments>\n");
                builder.Append("    <slash:hit_parade>42,30,20,10,5,2,1</slash:hit_parade>\n");
            }

            builder.Append("  </item>\n");
        }

        builder.Append("</rdf:RDF>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates an Atom 0.3 document: the <c>modified</c>/<c>created</c>/<c>issued</c> date
    /// triple, <c>tagline</c>, and entries alternating escaped HTML content with inline XHTML —
    /// the two content modes real 0.3 publishers used.
    /// </summary>
    /// <param name="entryCount">The number of <c>entry</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateAtom03Utf8(int entryCount)
    {
        StringBuilder builder = new(capacity: 1024 + (entryCount * 512));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<feed version=\"0.3\" xmlns=\"http://purl.org/atom/ns#\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">\n");
        builder.Append("  <title mode=\"escaped\">Legacy Benchmark Feed (Atom 0.3)</title>\n");
        builder.Append("  <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>\n");
        builder.Append("  <modified>2024-01-01T12:00:00Z</modified>\n");
        builder.Append("  <link rel=\"alternate\" type=\"text/html\" href=\"http://example.com\"/>\n");
        builder.Append("  <tagline>A synthetic Atom 0.3 feed in the Blogger-era shape.</tagline>\n");
        builder.Append("  <copyright>Copyright 2026 Example Corporation</copyright>\n");
        builder.Append("  <generator url=\"http://example.com/generator\" version=\"1.0\">Argotic Benchmarks</generator>\n");
        builder.Append("  <author>\n    <name>Feed Author</name>\n    <url>http://example.com/author</url>\n    <email>author@example.com</email>\n  </author>\n");
        builder.Append("  <contributor>\n    <name>Feed Contributor</name>\n  </contributor>\n");

        for (int i = 0; i < entryCount; i++)
        {
            int shape = i % 3;
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("  <entry>\n");
            builder.Append("    <title mode=\"escaped\">Entry ").Append(ordinal).Append("</title>\n");
            builder.Append("    <id>urn:uuid:entry-").Append(ordinal).Append("</id>\n");
            builder.Append("    <modified>2024-01-01T10:00:00Z</modified>\n");
            builder.Append("    <created>2023-12-31T10:00:00Z</created>\n");
            builder.Append("    <issued>2024-01-01T09:00:00Z</issued>\n");
            builder.Append("    <link rel=\"alternate\" type=\"text/html\" href=\"http://example.com/entry").Append(ordinal).Append("\"/>\n");

            if (shape != 2)
            {
                builder.Append("    <author>\n      <name>Entry Author ").Append(ordinal).Append("</name>\n    </author>\n");
            }

            builder.Append("    <summary mode=\"escaped\">Summary for entry ").Append(ordinal).Append(".</summary>\n");
            builder.Append(shape == 1
                ? "    <content type=\"text/html\" mode=\"xml\">\n      <xhtml:div>Entry content in <xhtml:strong>XHTML</xhtml:strong>.</xhtml:div>\n    </content>\n"
                : "    <content type=\"text/html\" mode=\"escaped\">&lt;p&gt;Escaped HTML content for entry " + ordinal + ".&lt;/p&gt;</content>\n");
            builder.Append("  </entry>\n");
        }

        builder.Append("</feed>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Generates an RSD 0.6 document with the requested number of APIs, cycling the engines a
    /// real discovery document lists and carrying <c>settings</c> on every fourth API.
    /// </summary>
    /// <param name="apiCount">The number of <c>api</c> elements to emit.</param>
    /// <returns>The generated document as UTF-8 bytes.</returns>
    public static byte[] GenerateRsd06Utf8(int apiCount)
    {
        StringBuilder builder = new(capacity: 512 + (apiCount * 224));

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        builder.Append("<rsd version=\"0.6\" xmlns=\"http://archipelago.phrasewise.com/rsd\">\n");
        builder.Append("  <service>\n");
        builder.Append("    <engineName>Example Blog Engine</engineName>\n");
        builder.Append("    <engineLink>http://example.com/engine</engineLink>\n");
        builder.Append("    <homePageLink>http://example.com</homePageLink>\n");
        builder.Append("    <apis>\n");

        for (int i = 0; i < apiCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            string name = (i % 3) switch
            {
                0 => "MetaWeblog",
                1 => "Blogger",
                _ => "WordPress",
            };

            builder.Append("      <api name=\"").Append(name).Append("\" preferred=\"").Append(i == 0 ? "true" : "false")
                   .Append("\" apiLink=\"http://example.com/xmlrpc").Append(ordinal).Append("\" blogID=\"").Append(ordinal).Append('"');

            if (i % 4 == 0)
            {
                builder.Append(">\n        <settings>\n");
                builder.Append("          <setting name=\"docs\">http://www.xmlrpc.com/metaWeblogApi</setting>\n");
                builder.Append("          <setting name=\"notes\">Synthetic API entry ").Append(ordinal).Append("</setting>\n");
                builder.Append("        </settings>\n      </api>\n");
            }
            else
            {
                builder.Append("/>\n");
            }
        }

        builder.Append("    </apis>\n  </service>\n</rsd>\n");

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string BuildRss091Document(int itemCount, bool withDoctype, bool latin1Accents)
    {
        // The accented strings stay within Latin-1 so the ISO-8859-1 arm round-trips its encoding
        // rather than silently substituting.
        string editor = latin1Accents ? "rédacteur@example.com (René Éditeur)" : "editor@example.com";
        string titleSuffix = latin1Accents ? " — édition française" : string.Empty;

        StringBuilder builder = new(capacity: 1536 + (itemCount * 256));

        builder.Append("<?xml version=\"1.0\" encoding=\"").Append(latin1Accents ? "iso-8859-1" : "UTF-8").Append("\"?>\n");

        if (withDoctype)
        {
            builder.Append("<!DOCTYPE rss PUBLIC \"-//Netscape Communications//DTD RSS 0.91//EN\"\n");
            builder.Append("  \"http://my.netscape.com/publish/formats/rss-0.91.dtd\">\n");
        }

        builder.Append("<rss version=\"0.91\">\n");
        builder.Append("  <channel>\n");
        builder.Append("    <title>Legacy Benchmark Feed 0.91").Append(titleSuffix).Append("</title>\n");
        builder.Append("    <link>http://example.com</link>\n");
        builder.Append("    <description>A synthetic RSS 0.91 feed with the full optional-element surface.</description>\n");
        builder.Append("    <language>").Append(latin1Accents ? "fr" : "en-us").Append("</language>\n");
        builder.Append("    <copyright>Copyright 2026 Example Corporation</copyright>\n");
        builder.Append("    <managingEditor>").Append(editor).Append("</managingEditor>\n");
        builder.Append("    <webMaster>webmaster@example.com</webMaster>\n");
        builder.Append("    <rating>(PICS-1.1 \"http://www.classify.org/safesurf/\" 1 r (SS~~000 1))</rating>\n");
        builder.Append("    <pubDate>Mon, 01 Jan 2024 08:00:00 GMT</pubDate>\n");
        builder.Append("    <lastBuildDate>Mon, 01 Jan 2024 12:00:00 GMT</lastBuildDate>\n");
        builder.Append("    <image>\n");
        builder.Append("      <title>Feed Image</title>\n");
        builder.Append("      <url>http://example.com/image.png</url>\n");
        builder.Append("      <link>http://example.com</link>\n");
        builder.Append("      <width>88</width>\n");
        builder.Append("      <height>31</height>\n");
        builder.Append("      <description>Logo for the feed</description>\n");
        builder.Append("    </image>\n");
        builder.Append("    <textInput>\n");
        builder.Append("      <title>Search</title>\n");
        builder.Append("      <description>Search the feed</description>\n");
        builder.Append("      <name>query</name>\n");
        builder.Append("      <link>http://example.com/search</link>\n");
        builder.Append("    </textInput>\n");
        builder.Append("    <skipHours>\n      <hour>1</hour>\n      <hour>2</hour>\n    </skipHours>\n");
        builder.Append("    <skipDays>\n      <day>Saturday</day>\n      <day>Sunday</day>\n    </skipDays>\n");

        for (int i = 0; i < itemCount; i++)
        {
            string ordinal = i.ToString(CultureInfo.InvariantCulture);
            builder.Append("    <item>\n");
            builder.Append("      <title>").Append(latin1Accents ? "Dépêche " : "Bulletin ").Append(ordinal).Append("</title>\n");
            builder.Append("      <link>http://example.com/item").Append(ordinal).Append("</link>\n");
            builder.Append("      <description>").Append(latin1Accents ? "Résumé de la dépêche " : "Body text for bulletin ")
                   .Append(ordinal).Append(".</description>\n");
            builder.Append("    </item>\n");
        }

        builder.Append("  </channel>\n</rss>\n");

        return builder.ToString();
    }
}