namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// Provides test data for feed loading tests.
/// </summary>
public static class FeedTestData
{
    /// <summary>
    /// A minimal valid RSS 2.0 feed.
    /// </summary>
    public const string MinimalRss = """
                                     <?xml version="1.0" encoding="UTF-8"?>
                                     <rss version="2.0">
                                         <channel>
                                             <title>Test Feed</title>
                                             <link>http://example.com</link>
                                             <description>A test feed</description>
                                         </channel>
                                     </rss>
                                     """;

    /// <summary>
    /// A minimal valid Atom 1.0 feed.
    /// </summary>
    public const string MinimalAtom = """
                                      <?xml version="1.0" encoding="UTF-8"?>
                                      <feed xmlns="http://www.w3.org/2005/Atom">
                                          <title>Test Feed</title>
                                          <id>urn:uuid:12345678-1234-1234-1234-123456789012</id>
                                          <updated>2024-01-01T00:00:00Z</updated>
                                      </feed>
                                      """;

    /// <summary>
    /// A minimal valid Atom entry.
    /// </summary>
    public const string MinimalAtomEntry = """
                                           <?xml version="1.0" encoding="UTF-8"?>
                                           <entry xmlns="http://www.w3.org/2005/Atom">
                                               <title>Test Entry</title>
                                               <id>urn:uuid:12345678-1234-1234-1234-123456789013</id>
                                               <updated>2024-01-01T00:00:00Z</updated>
                                           </entry>
                                           """;

    /// <summary>
    /// A minimal valid OPML document.
    /// </summary>
    public const string MinimalOpml = """
                                      <?xml version="1.0" encoding="UTF-8"?>
                                      <opml version="2.0">
                                          <head>
                                              <title>Test OPML</title>
                                          </head>
                                          <body>
                                              <outline text="Test Outline"/>
                                          </body>
                                      </opml>
                                      """;

    /// <summary>
    /// A malformed XML document.
    /// </summary>
    public const string MalformedXml = """
                                       <?xml version="1.0" encoding="UTF-8"?>
                                       <rss version="2.0">
                                           <channel>
                                               <title>Unclosed Tag
                                           </channel>
                                       </rss>
                                       """;

    /// <summary>
    /// HTML page with RSS link for auto-discovery tests.
    /// </summary>
    public const string HtmlWithRssLink = """
                                          <!DOCTYPE html>
                                          <html>
                                          <head>
                                              <title>Test Page</title>
                                              <link rel="alternate" type="application/rss+xml" title="RSS Feed" href="http://example.com/feed.rss" />
                                          </head>
                                          <body>
                                              <p>Test content</p>
                                          </body>
                                          </html>
                                          """;

    /// <summary>
    /// HTML page with pingback link for pingback discovery tests.
    /// </summary>
    public const string HtmlWithPingbackLink = """
                                               <!DOCTYPE html>
                                               <html>
                                               <head>
                                                   <title>Test Page</title>
                                                   <link rel="pingback" href="http://example.com/xmlrpc.php" />
                                               </head>
                                               <body>
                                                   <p>Test content</p>
                                               </body>
                                               </html>
                                               """;

    /// <summary>
    /// HTML page with embedded trackback RDF for trackback discovery tests.
    /// </summary>
    public const string HtmlWithTrackbackRdf = """
                                               <!DOCTYPE html>
                                               <html>
                                               <head>
                                                   <title>Test Page</title>
                                               </head>
                                               <body>
                                                   <p>Test content</p>
                                                   <!--
                                                   <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                                                            xmlns:dc="http://purl.org/dc/elements/1.1/"
                                                            xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/">
                                                   <rdf:Description
                                                       rdf:about="http://example.com/post/1"
                                                       dc:identifier="http://example.com/post/1"
                                                       dc:title="Test Post"
                                                       trackback:ping="http://example.com/trackback/1" />
                                                   </rdf:RDF>
                                                   -->
                                               </body>
                                               </html>
                                               """;

    /// <summary>
    /// HTML page containing a link to a target URL for SourceReferencesTarget tests.
    /// </summary>
    public const string HtmlWithTargetLink = """
                                             <!DOCTYPE html>
                                             <html>
                                             <head>
                                                 <title>Test Page</title>
                                             </head>
                                             <body>
                                                 <p>Check out <a href="http://example.com/target">this link</a>!</p>
                                             </body>
                                             </html>
                                             """;

    /// <summary>
    /// RSS feed with items for GenericSyndicationFeed tests.
    /// </summary>
    public const string RssWithItems = """
                                       <?xml version="1.0" encoding="UTF-8"?>
                                       <rss version="2.0">
                                           <channel>
                                               <title>Test Feed</title>
                                               <link>http://example.com</link>
                                               <description>A test feed</description>
                                               <category>Technology</category>
                                               <item>
                                                   <title>Recent Item</title>
                                                   <link>http://example.com/recent</link>
                                                   <description>A recent item</description>
                                                   <pubDate>Mon, 20 Jan 2025 12:00:00 GMT</pubDate>
                                                   <category>Tech</category>
                                                   <category>News</category>
                                               </item>
                                               <item>
                                                   <title>Old Item</title>
                                                   <link>http://example.com/old</link>
                                                   <description>An old item</description>
                                                   <pubDate>Mon, 01 Jan 2024 12:00:00 GMT</pubDate>
                                                   <category>Archive</category>
                                               </item>
                                           </channel>
                                       </rss>
                                       """;

    /// <summary>
    /// Atom feed with entries for GenericSyndicationFeed tests.
    /// </summary>
    public const string AtomWithEntries = """
                                          <?xml version="1.0" encoding="UTF-8"?>
                                          <feed xmlns="http://www.w3.org/2005/Atom">
                                              <title>Test Feed</title>
                                              <id>urn:uuid:12345678-1234-1234-1234-123456789012</id>
                                              <updated>2025-01-20T12:00:00Z</updated>
                                              <category term="Technology" />
                                              <entry>
                                                  <title>Recent Entry</title>
                                                  <id>urn:uuid:entry-1</id>
                                                  <updated>2025-01-20T12:00:00Z</updated>
                                                  <published>2025-01-20T12:00:00Z</published>
                                                  <summary>A recent entry</summary>
                                                  <category term="Tech" />
                                                  <category term="News" />
                                              </entry>
                                              <entry>
                                                  <title>Old Entry</title>
                                                  <id>urn:uuid:entry-2</id>
                                                  <updated>2024-01-01T12:00:00Z</updated>
                                                  <published>2024-01-01T12:00:00Z</published>
                                                  <summary>An old entry</summary>
                                                  <category term="Archive" />
                                              </entry>
                                          </feed>
                                          """;
}