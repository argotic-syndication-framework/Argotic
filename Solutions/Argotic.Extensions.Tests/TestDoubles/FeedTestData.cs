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

    #region Legacy Format Test Data

    /// <summary>
    /// A minimal valid RSS 0.90 feed (RDF-based).
    /// </summary>
    public const string Rss090Minimal = """
                                        <?xml version="1.0" encoding="UTF-8"?>
                                        <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                                                 xmlns="http://my.netscape.com/rdf/simple/0.9/">
                                            <channel>
                                                <title>Test RSS 0.90 Feed</title>
                                                <link>http://example.com</link>
                                                <description>A test RSS 0.90 feed</description>
                                            </channel>
                                        </rdf:RDF>
                                        """;

    /// <summary>
    /// RSS 0.90 feed with items, image, and text input.
    /// </summary>
    public const string Rss090WithItems = """
                                          <?xml version="1.0" encoding="UTF-8"?>
                                          <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                                                   xmlns="http://my.netscape.com/rdf/simple/0.9/">
                                              <channel>
                                                  <title>Test RSS 0.90 Feed</title>
                                                  <link>http://example.com</link>
                                                  <description>A test RSS 0.90 feed with items</description>
                                              </channel>
                                              <image>
                                                  <title>Feed Image</title>
                                                  <url>http://example.com/image.png</url>
                                                  <link>http://example.com</link>
                                              </image>
                                              <item>
                                                  <title>First Item</title>
                                                  <link>http://example.com/item1</link>
                                              </item>
                                              <item>
                                                  <title>Second Item</title>
                                                  <link>http://example.com/item2</link>
                                              </item>
                                              <textinput>
                                                  <title>Search</title>
                                                  <description>Search the feed</description>
                                                  <name>query</name>
                                                  <link>http://example.com/search</link>
                                              </textinput>
                                          </rdf:RDF>
                                          """;

    /// <summary>
    /// A minimal valid RSS 0.91 feed.
    /// </summary>
    public const string Rss091Minimal = """
                                        <?xml version="1.0" encoding="UTF-8"?>
                                        <rss version="0.91">
                                            <channel>
                                                <title>Test RSS 0.91 Feed</title>
                                                <link>http://example.com</link>
                                                <description>A test RSS 0.91 feed</description>
                                                <language>en-us</language>
                                            </channel>
                                        </rss>
                                        """;

    /// <summary>
    /// A full RSS 0.91 feed with all optional elements.
    /// </summary>
    public const string Rss091Full = """
                                     <?xml version="1.0" encoding="UTF-8"?>
                                     <rss version="0.91">
                                         <channel>
                                             <title>Test RSS 0.91 Feed</title>
                                             <link>http://example.com</link>
                                             <description>A full RSS 0.91 feed</description>
                                             <language>en-us</language>
                                             <copyright>Copyright 2025</copyright>
                                             <managingEditor>editor@example.com</managingEditor>
                                             <webMaster>webmaster@example.com</webMaster>
                                             <rating>(PICS-1.1 "http://www.classify.org/safesurf/" 1 r (SS~~000 1))</rating>
                                             <pubDate>Mon, 20 Jan 2025 12:00:00 GMT</pubDate>
                                             <lastBuildDate>Mon, 20 Jan 2025 12:00:00 GMT</lastBuildDate>
                                             <image>
                                                 <title>Feed Image</title>
                                                 <url>http://example.com/image.png</url>
                                                 <link>http://example.com</link>
                                                 <width>88</width>
                                                 <height>31</height>
                                                 <description>Logo for the feed</description>
                                             </image>
                                             <textInput>
                                                 <title>Search</title>
                                                 <description>Search the feed</description>
                                                 <name>query</name>
                                                 <link>http://example.com/search</link>
                                             </textInput>
                                             <skipHours>
                                                 <hour>1</hour>
                                                 <hour>2</hour>
                                             </skipHours>
                                             <skipDays>
                                                 <day>Saturday</day>
                                                 <day>Sunday</day>
                                             </skipDays>
                                             <item>
                                                 <title>Test Item</title>
                                                 <link>http://example.com/item1</link>
                                                 <description>Test item description</description>
                                             </item>
                                         </channel>
                                     </rss>
                                     """;

    /// <summary>
    /// A minimal valid RSS 0.92 feed.
    /// </summary>
    public const string Rss092Minimal = """
                                        <?xml version="1.0" encoding="UTF-8"?>
                                        <rss version="0.92">
                                            <channel>
                                                <title>Test RSS 0.92 Feed</title>
                                                <link>http://example.com</link>
                                                <description>A test RSS 0.92 feed</description>
                                            </channel>
                                        </rss>
                                        """;

    /// <summary>
    /// A full RSS 0.92 feed with cloud, enclosures, categories, and source.
    /// </summary>
    public const string Rss092Full = """
                                     <?xml version="1.0" encoding="UTF-8"?>
                                     <rss version="0.92">
                                         <channel>
                                             <title>Test RSS 0.92 Feed</title>
                                             <link>http://example.com</link>
                                             <description>A full RSS 0.92 feed</description>
                                             <language>en-us</language>
                                             <copyright>Copyright 2025</copyright>
                                             <managingEditor>editor@example.com</managingEditor>
                                             <webMaster>webmaster@example.com</webMaster>
                                             <pubDate>Mon, 20 Jan 2025 12:00:00 GMT</pubDate>
                                             <lastBuildDate>Mon, 20 Jan 2025 12:00:00 GMT</lastBuildDate>
                                             <cloud domain="rpc.example.com" port="80" path="/RPC2" registerProcedure="pingMe" protocol="soap"/>
                                             <image>
                                                 <title>Feed Image</title>
                                                 <url>http://example.com/image.png</url>
                                                 <link>http://example.com</link>
                                                 <width>88</width>
                                                 <height>31</height>
                                             </image>
                                             <textInput>
                                                 <title>Search</title>
                                                 <description>Search the feed</description>
                                                 <name>query</name>
                                                 <link>http://example.com/search</link>
                                             </textInput>
                                             <skipHours>
                                                 <hour>1</hour>
                                                 <hour>2</hour>
                                             </skipHours>
                                             <skipDays>
                                                 <day>Saturday</day>
                                                 <day>Sunday</day>
                                             </skipDays>
                                             <item>
                                                 <title>Test Item with Enclosure</title>
                                                 <link>http://example.com/item1</link>
                                                 <description>Test item with enclosure</description>
                                                 <enclosure url="http://example.com/podcast.mp3" length="12345678" type="audio/mpeg"/>
                                                 <category>Technology</category>
                                                 <category domain="http://example.com/categories">News</category>
                                                 <source url="http://other.example.com/feed.xml">Other Feed</source>
                                             </item>
                                             <item>
                                                 <title>Second Item</title>
                                                 <link>http://example.com/item2</link>
                                                 <description>Second item description</description>
                                             </item>
                                         </channel>
                                     </rss>
                                     """;

    /// <summary>
    /// A minimal valid RSS 1.0 feed (RDF-based).
    /// </summary>
    public const string Rss10Minimal = """
                                       <?xml version="1.0" encoding="UTF-8"?>
                                       <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                                                xmlns="http://purl.org/rss/1.0/">
                                           <channel rdf:about="http://example.com">
                                               <title>Test RSS 1.0 Feed</title>
                                               <link>http://example.com</link>
                                               <description>A test RSS 1.0 feed</description>
                                           </channel>
                                       </rdf:RDF>
                                       """;

    /// <summary>
    /// RSS 1.0 feed with items, image, and text input (RDF-based).
    /// </summary>
    public const string Rss10WithItems = """
                                         <?xml version="1.0" encoding="UTF-8"?>
                                         <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                                                  xmlns="http://purl.org/rss/1.0/">
                                             <channel rdf:about="http://example.com">
                                                 <title>Test RSS 1.0 Feed</title>
                                                 <link>http://example.com</link>
                                                 <description>A test RSS 1.0 feed with items</description>
                                                 <items>
                                                     <rdf:Seq>
                                                         <rdf:li resource="http://example.com/item1"/>
                                                         <rdf:li resource="http://example.com/item2"/>
                                                     </rdf:Seq>
                                                 </items>
                                             </channel>
                                             <image rdf:about="http://example.com/image.png">
                                                 <title>Feed Image</title>
                                                 <url>http://example.com/image.png</url>
                                                 <link>http://example.com</link>
                                             </image>
                                             <item rdf:about="http://example.com/item1">
                                                 <title>First Item</title>
                                                 <link>http://example.com/item1</link>
                                                 <description>First item description</description>
                                             </item>
                                             <item rdf:about="http://example.com/item2">
                                                 <title>Second Item</title>
                                                 <link>http://example.com/item2</link>
                                                 <description>Second item description</description>
                                             </item>
                                             <textinput rdf:about="http://example.com/search">
                                                 <title>Search</title>
                                                 <description>Search the feed</description>
                                                 <name>query</name>
                                                 <link>http://example.com/search</link>
                                             </textinput>
                                         </rdf:RDF>
                                         """;

    /// <summary>
    /// A minimal valid Atom 0.3 feed.
    /// </summary>
    public const string Atom03Feed = """
                                     <?xml version="1.0" encoding="UTF-8"?>
                                     <feed version="0.3" xmlns="http://purl.org/atom/ns#">
                                         <title>Test Atom 0.3 Feed</title>
                                         <id>urn:uuid:12345678-1234-1234-1234-123456789012</id>
                                         <modified>2025-01-20T12:00:00Z</modified>
                                         <link rel="alternate" type="text/html" href="http://example.com"/>
                                     </feed>
                                     """;

    /// <summary>
    /// A full Atom 0.3 feed with entries and all optional elements.
    /// </summary>
    public const string Atom03FeedFull = """
                                         <?xml version="1.0" encoding="UTF-8"?>
                                         <feed version="0.3" xmlns="http://purl.org/atom/ns#" xmlns:xhtml="http://www.w3.org/1999/xhtml">
                                             <title mode="escaped">Test Atom 0.3 Feed</title>
                                             <id>urn:uuid:12345678-1234-1234-1234-123456789012</id>
                                             <modified>2025-01-20T12:00:00Z</modified>
                                             <link rel="alternate" type="text/html" href="http://example.com"/>
                                             <author>
                                                 <name>Test Author</name>
                                                 <url>http://example.com/author</url>
                                                 <email>author@example.com</email>
                                             </author>
                                             <contributor>
                                                 <name>Test Contributor</name>
                                                 <email>contributor@example.com</email>
                                             </contributor>
                                             <tagline>A tagline for the feed</tagline>
                                             <copyright>Copyright 2025</copyright>
                                             <generator url="http://example.com/generator" version="1.0">Test Generator</generator>
                                             <entry>
                                                 <title>Test Entry</title>
                                                 <id>urn:uuid:entry-1</id>
                                                 <modified>2025-01-20T12:00:00Z</modified>
                                                 <created>2025-01-19T12:00:00Z</created>
                                                 <link rel="alternate" type="text/html" href="http://example.com/entry1"/>
                                                 <author>
                                                     <name>Entry Author</name>
                                                 </author>
                                                 <summary mode="escaped">Entry summary</summary>
                                                 <content type="text/html" mode="xml">
                                                     <xhtml:div>Entry content in XHTML</xhtml:div>
                                                 </content>
                                             </entry>
                                         </feed>
                                         """;

    /// <summary>
    /// A minimal valid Atom 0.3 entry.
    /// </summary>
    public const string Atom03Entry = """
                                      <?xml version="1.0" encoding="UTF-8"?>
                                      <entry xmlns="http://purl.org/atom/ns#">
                                          <title>Test Entry</title>
                                          <id>urn:uuid:entry-1</id>
                                          <modified>2025-01-20T12:00:00Z</modified>
                                          <link rel="alternate" type="text/html" href="http://example.com/entry"/>
                                      </entry>
                                      """;

    /// <summary>
    /// A minimal valid RSD 0.6 document.
    /// </summary>
    public const string Rsd06Minimal = """
                                       <?xml version="1.0" encoding="UTF-8"?>
                                       <rsd version="0.6" xmlns="http://archipelago.phrasewise.com/rsd">
                                           <service>
                                               <engineName>Test Blog Engine</engineName>
                                               <engineLink>http://example.com/engine</engineLink>
                                               <homePageLink>http://example.com</homePageLink>
                                               <apis>
                                                   <api name="MetaWeblog" preferred="true" apiLink="http://example.com/xmlrpc.php" blogID="1"/>
                                               </apis>
                                           </service>
                                       </rsd>
                                       """;

    /// <summary>
    /// A full RSD 0.6 document with multiple APIs.
    /// </summary>
    public const string Rsd06Full = """
                                    <?xml version="1.0" encoding="UTF-8"?>
                                    <rsd version="0.6" xmlns="http://archipelago.phrasewise.com/rsd">
                                        <service>
                                            <engineName>Test Blog Engine</engineName>
                                            <engineLink>http://example.com/engine</engineLink>
                                            <homePageLink>http://example.com</homePageLink>
                                            <apis>
                                                <api name="MetaWeblog" preferred="true" apiLink="http://example.com/xmlrpc.php" blogID="1">
                                                    <settings>
                                                        <setting name="docs">http://www.xmlrpc.com/metaWeblogApi</setting>
                                                        <setting name="notes">This is the MetaWeblog API</setting>
                                                    </settings>
                                                </api>
                                                <api name="Blogger" preferred="false" apiLink="http://example.com/blogger" blogID="1"/>
                                            </apis>
                                        </service>
                                    </rsd>
                                    """;

    #endregion

    #region BlogML Test Data

    /// <summary>
    /// A BlogML document with attachments.
    /// </summary>
    public const string BlogMLWithAttachments = """
                                                <?xml version="1.0" encoding="UTF-8"?>
                                                <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com" date-created="2025-01-01T00:00:00">
                                                    <title>Test Blog</title>
                                                    <sub-title>A test blog</sub-title>
                                                    <authors>
                                                        <author id="author1" date-created="2025-01-01T00:00:00" date-modified="2025-01-01T00:00:00" approved="true">
                                                            <title>Test Author</title>
                                                        </author>
                                                    </authors>
                                                    <posts>
                                                        <post id="post1" date-created="2025-01-15T12:00:00" date-modified="2025-01-15T12:00:00" approved="true" post-url="http://example.com/post1">
                                                            <title>Test Post with Attachment</title>
                                                            <content type="html"><![CDATA[<p>Post content</p>]]></content>
                                                            <attachments>
                                                                <attachment embedded="true" mime-type="image/png" size="12345" url="http://example.com/image.png">SGVsbG8gV29ybGQ=</attachment>
                                                                <attachment embedded="false" mime-type="application/pdf" external-uri="http://example.com/doc.pdf" url="http://example.com/doc.pdf"/>
                                                            </attachments>
                                                        </post>
                                                    </posts>
                                                </blog>
                                                """;

    /// <summary>
    /// A BlogML document with trackbacks.
    /// </summary>
    public const string BlogMLWithTrackbacks = """
                                               <?xml version="1.0" encoding="UTF-8"?>
                                               <blog xmlns="http://www.blogml.com/2006/09/BlogML" root-url="http://example.com" date-created="2025-01-01T00:00:00">
                                                   <title>Test Blog</title>
                                                   <sub-title>A test blog</sub-title>
                                                   <authors>
                                                       <author id="author1" date-created="2025-01-01T00:00:00" date-modified="2025-01-01T00:00:00" approved="true">
                                                           <title>Test Author</title>
                                                       </author>
                                                   </authors>
                                                   <posts>
                                                       <post id="post1" date-created="2025-01-15T12:00:00" date-modified="2025-01-15T12:00:00" approved="true" post-url="http://example.com/post1">
                                                           <title>Test Post with Trackbacks</title>
                                                           <content type="html"><![CDATA[<p>Post content</p>]]></content>
                                                           <trackbacks>
                                                               <trackback id="tb1" date-created="2025-01-16T10:00:00" date-modified="2025-01-16T10:00:00" approved="true" url="http://other.example.com/post"/>
                                                               <trackback id="tb2" date-created="2025-01-17T10:00:00" date-modified="2025-01-17T10:00:00" approved="false" url="http://another.example.com/post">
                                                                   <title>Trackback Title</title>
                                                               </trackback>
                                                           </trackbacks>
                                                       </post>
                                                   </posts>
                                               </blog>
                                               """;

    #endregion

    #region XML-RPC Test Data

    /// <summary>
    /// XML-RPC array value.
    /// </summary>
    public const string XmlRpcArrayValue = """
                                           <?xml version="1.0"?>
                                           <value>
                                               <array>
                                                   <data>
                                                       <value><i4>1</i4></value>
                                                       <value><string>hello</string></value>
                                                       <value><boolean>1</boolean></value>
                                                       <value><double>3.14</double></value>
                                                   </data>
                                               </array>
                                           </value>
                                           """;

    /// <summary>
    /// XML-RPC struct value.
    /// </summary>
    public const string XmlRpcStructValue = """
                                            <?xml version="1.0"?>
                                            <value>
                                                <struct>
                                                    <member>
                                                        <name>title</name>
                                                        <value><string>Test Title</string></value>
                                                    </member>
                                                    <member>
                                                        <name>count</name>
                                                        <value><i4>42</i4></value>
                                                    </member>
                                                    <member>
                                                        <name>enabled</name>
                                                        <value><boolean>1</boolean></value>
                                                    </member>
                                                </struct>
                                            </value>
                                            """;

    /// <summary>
    /// XML-RPC method call message.
    /// </summary>
    public const string XmlRpcMethodCall = """
                                           <?xml version="1.0"?>
                                           <methodCall>
                                               <methodName>test.method</methodName>
                                               <params>
                                                   <param>
                                                       <value><string>parameter1</string></value>
                                                   </param>
                                                   <param>
                                                       <value><i4>123</i4></value>
                                                   </param>
                                               </params>
                                           </methodCall>
                                           """;

    /// <summary>
    /// XML-RPC method response message.
    /// </summary>
    public const string XmlRpcMethodResponse = """
                                               <?xml version="1.0"?>
                                               <methodResponse>
                                                   <params>
                                                       <param>
                                                           <value><string>success</string></value>
                                                       </param>
                                                   </params>
                                               </methodResponse>
                                               """;

    #endregion

    #region Trackback Test Data

    /// <summary>
    /// Trackback RDF embedded in HTML comments.
    /// </summary>
    public const string TrackbackRdfMetadata = """
                                               <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                                                        xmlns:dc="http://purl.org/dc/elements/1.1/"
                                                        xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/">
                                                   <rdf:Description
                                                       rdf:about="http://example.com/post/1"
                                                       dc:identifier="http://example.com/post/1"
                                                       dc:title="Test Post Title"
                                                       trackback:ping="http://example.com/trackback/1" />
                                               </rdf:RDF>
                                               """;

    /// <summary>
    /// Trackback response success.
    /// </summary>
    public const string TrackbackResponseSuccess = """
                                                   <?xml version="1.0" encoding="UTF-8"?>
                                                   <response>
                                                       <error>0</error>
                                                   </response>
                                                   """;

    /// <summary>
    /// Trackback response error.
    /// </summary>
    public const string TrackbackResponseError = """
                                                 <?xml version="1.0" encoding="UTF-8"?>
                                                 <response>
                                                     <error>1</error>
                                                     <message>The trackback has already been registered.</message>
                                                 </response>
                                                 """;

    #endregion

    #region Sitemap Test Data

    /// <summary>
    /// A minimal valid sitemap.
    /// </summary>
    public const string MinimalSitemap = """
                                         <?xml version="1.0" encoding="UTF-8"?>
                                         <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                             <url>
                                                 <loc>https://www.example.com/</loc>
                                             </url>
                                         </urlset>
                                         """;

    /// <summary>
    /// A sitemap with all optional elements.
    /// </summary>
    public const string FullSitemap = """
                                      <?xml version="1.0" encoding="UTF-8"?>
                                      <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                          <url>
                                              <loc>https://www.example.com/</loc>
                                              <lastmod>2024-01-15</lastmod>
                                              <changefreq>daily</changefreq>
                                              <priority>1.0</priority>
                                          </url>
                                          <url>
                                              <loc>https://www.example.com/about</loc>
                                              <lastmod>2024-01-10T12:00:00+00:00</lastmod>
                                              <changefreq>monthly</changefreq>
                                              <priority>0.8</priority>
                                          </url>
                                          <url>
                                              <loc>https://www.example.com/contact</loc>
                                              <lastmod>2024-01-05</lastmod>
                                              <changefreq>yearly</changefreq>
                                              <priority>0.5</priority>
                                          </url>
                                      </urlset>
                                      """;

    /// <summary>
    /// A sitemap with various changefreq values.
    /// </summary>
    public const string SitemapWithAllChangeFrequencies = """
                                                          <?xml version="1.0" encoding="UTF-8"?>
                                                          <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                                              <url>
                                                                  <loc>https://www.example.com/always</loc>
                                                                  <changefreq>always</changefreq>
                                                              </url>
                                                              <url>
                                                                  <loc>https://www.example.com/hourly</loc>
                                                                  <changefreq>hourly</changefreq>
                                                              </url>
                                                              <url>
                                                                  <loc>https://www.example.com/daily</loc>
                                                                  <changefreq>daily</changefreq>
                                                              </url>
                                                              <url>
                                                                  <loc>https://www.example.com/weekly</loc>
                                                                  <changefreq>weekly</changefreq>
                                                              </url>
                                                              <url>
                                                                  <loc>https://www.example.com/monthly</loc>
                                                                  <changefreq>monthly</changefreq>
                                                              </url>
                                                              <url>
                                                                  <loc>https://www.example.com/yearly</loc>
                                                                  <changefreq>yearly</changefreq>
                                                              </url>
                                                              <url>
                                                                  <loc>https://www.example.com/never</loc>
                                                                  <changefreq>never</changefreq>
                                                              </url>
                                                          </urlset>
                                                          """;

    /// <summary>
    /// A sitemap with URL entity escaping.
    /// </summary>
    public const string SitemapWithEscapedUrls = """
                                                 <?xml version="1.0" encoding="UTF-8"?>
                                                 <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                                     <url>
                                                         <loc>https://www.example.com/page?param1=value1&amp;param2=value2</loc>
                                                     </url>
                                                     <url>
                                                         <loc>https://www.example.com/search?q=test%20query</loc>
                                                     </url>
                                                 </urlset>
                                                 """;

    /// <summary>
    /// An empty sitemap with no URLs.
    /// </summary>
    public const string EmptySitemap = """
                                       <?xml version="1.0" encoding="UTF-8"?>
                                       <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                       </urlset>
                                       """;

    /// <summary>
    /// A sitemap with various date formats.
    /// </summary>
    public const string SitemapWithDateFormats = """
                                                 <?xml version="1.0" encoding="UTF-8"?>
                                                 <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                                     <url>
                                                         <loc>https://www.example.com/date-only</loc>
                                                         <lastmod>2024-01-15</lastmod>
                                                     </url>
                                                     <url>
                                                         <loc>https://www.example.com/datetime-utc</loc>
                                                         <lastmod>2024-01-15T10:30:00Z</lastmod>
                                                     </url>
                                                     <url>
                                                         <loc>https://www.example.com/datetime-offset</loc>
                                                         <lastmod>2024-01-15T10:30:00+05:00</lastmod>
                                                     </url>
                                                     <url>
                                                         <loc>https://www.example.com/datetime-negative-offset</loc>
                                                         <lastmod>2024-01-15T10:30:00-08:00</lastmod>
                                                     </url>
                                                 </urlset>
                                                 """;

    /// <summary>
    /// A minimal sitemap index.
    /// </summary>
    public const string MinimalSitemapIndex = """
                                              <?xml version="1.0" encoding="UTF-8"?>
                                              <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                                  <sitemap>
                                                      <loc>https://www.example.com/sitemap1.xml</loc>
                                                  </sitemap>
                                              </sitemapindex>
                                              """;

    /// <summary>
    /// A sitemap index with multiple sitemaps.
    /// </summary>
    public const string FullSitemapIndex = """
                                           <?xml version="1.0" encoding="UTF-8"?>
                                           <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                               <sitemap>
                                                   <loc>https://www.example.com/sitemap1.xml</loc>
                                                   <lastmod>2024-01-15T10:00:00Z</lastmod>
                                               </sitemap>
                                               <sitemap>
                                                   <loc>https://www.example.com/sitemap2.xml</loc>
                                                   <lastmod>2024-01-14T10:00:00Z</lastmod>
                                               </sitemap>
                                               <sitemap>
                                                   <loc>https://www.example.com/sitemap3.xml</loc>
                                               </sitemap>
                                           </sitemapindex>
                                           """;

    /// <summary>
    /// An empty sitemap index.
    /// </summary>
    public const string EmptySitemapIndex = """
                                            <?xml version="1.0" encoding="UTF-8"?>
                                            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                            </sitemapindex>
                                            """;

    /// <summary>
    /// A sitemap with image extension.
    /// </summary>
    public const string SitemapWithImageExtension = """
                                                    <?xml version="1.0" encoding="UTF-8"?>
                                                    <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                                                            xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
                                                        <url>
                                                            <loc>https://example.com/page.html</loc>
                                                            <image:image>
                                                                <image:loc>https://example.com/image1.jpg</image:loc>
                                                            </image:image>
                                                        </url>
                                                    </urlset>
                                                    """;

    /// <summary>
    /// A sitemap with multiple images per URL.
    /// </summary>
    public const string SitemapWithMultipleImages = """
                                                    <?xml version="1.0" encoding="UTF-8"?>
                                                    <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                                                            xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
                                                        <url>
                                                            <loc>https://example.com/gallery.html</loc>
                                                            <image:image>
                                                                <image:loc>https://example.com/photo1.jpg</image:loc>
                                                            </image:image>
                                                            <image:image>
                                                                <image:loc>https://example.com/photo2.jpg</image:loc>
                                                            </image:image>
                                                            <image:image>
                                                                <image:loc>https://example.com/photo3.jpg</image:loc>
                                                            </image:image>
                                                        </url>
                                                    </urlset>
                                                    """;

    /// <summary>
    /// A sitemap with video extension.
    /// </summary>
    public const string SitemapWithVideoExtension = """
                                                    <?xml version="1.0" encoding="UTF-8"?>
                                                    <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                                                            xmlns:video="http://www.google.com/schemas/sitemap-video/1.1">
                                                        <url>
                                                            <loc>https://example.com/video-page.html</loc>
                                                            <video:video>
                                                                <video:thumbnail_loc>https://example.com/thumb.jpg</video:thumbnail_loc>
                                                                <video:title>Example Video</video:title>
                                                                <video:description>A sample video description</video:description>
                                                                <video:content_loc>https://example.com/video.mp4</video:content_loc>
                                                                <video:duration>600</video:duration>
                                                                <video:publication_date>2024-01-15</video:publication_date>
                                                            </video:video>
                                                        </url>
                                                    </urlset>
                                                    """;

    /// <summary>
    /// A sitemap with news extension.
    /// </summary>
    public const string SitemapWithNewsExtension = """
                                                   <?xml version="1.0" encoding="UTF-8"?>
                                                   <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                                                           xmlns:news="http://www.google.com/schemas/sitemap-news/0.9">
                                                       <url>
                                                           <loc>https://example.com/article.html</loc>
                                                           <news:news>
                                                               <news:publication>
                                                                   <news:name>Example News</news:name>
                                                                   <news:language>en</news:language>
                                                               </news:publication>
                                                               <news:publication_date>2024-01-15</news:publication_date>
                                                               <news:title>Breaking News Article</news:title>
                                                           </news:news>
                                                       </url>
                                                   </urlset>
                                                   """;

    /// <summary>
    /// A sitemap with hreflang/xhtml:link extension.
    /// </summary>
    public const string SitemapWithHreflangExtension = """
                                                       <?xml version="1.0" encoding="UTF-8"?>
                                                       <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
                                                               xmlns:xhtml="http://www.w3.org/1999/xhtml">
                                                           <url>
                                                               <loc>https://example.com/en/page.html</loc>
                                                               <xhtml:link rel="alternate" hreflang="en" href="https://example.com/en/page.html"/>
                                                               <xhtml:link rel="alternate" hreflang="de" href="https://example.com/de/page.html"/>
                                                               <xhtml:link rel="alternate" hreflang="fr" href="https://example.com/fr/page.html"/>
                                                               <xhtml:link rel="alternate" hreflang="x-default" href="https://example.com/page.html"/>
                                                           </url>
                                                       </urlset>
                                                       """;

    /// <summary>
    /// A sitemap with priority boundary values.
    /// </summary>
    public const string SitemapWithPriorityBoundaries = """
                                                        <?xml version="1.0" encoding="UTF-8"?>
                                                        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                                                            <url>
                                                                <loc>https://www.example.com/min-priority</loc>
                                                                <priority>0.0</priority>
                                                            </url>
                                                            <url>
                                                                <loc>https://www.example.com/max-priority</loc>
                                                                <priority>1.0</priority>
                                                            </url>
                                                            <url>
                                                                <loc>https://www.example.com/default-priority</loc>
                                                                <priority>0.5</priority>
                                                            </url>
                                                        </urlset>
                                                        """;

    #endregion
}