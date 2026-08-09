namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers parse paths that no other test executed.
/// </summary>
/// <remarks>
///     <para>
///         Mapping code coverage onto the child-selection call sites showed 81 of them were reached by
///         no test. These are the four largest clusters: every field of a sitemap video extension, of
///         an Atom entry's source element, and of the RSS image and text-input elements.
///     </para>
///     <para>
///         The existing sitemap fixture carried 6 of the video extension's 16 fields and no test
///         loaded it end to end, so ten of those parse branches had never run at all.
///     </para>
/// </remarks>
[TestClass]
public class UncoveredParsePathTests
{
    /// <summary>
    /// All sixteen fields of a <c>video:video</c> element survive a sitemap load, down to the boolean
    /// <c>yes</c>/<c>no</c> flags and the two elements read for their <c>relationship</c> attribute.
    /// </summary>
    /// <remarks>
    ///     Ten of these branches had never run: the existing fixture carried six of the sixteen, and nothing
    ///     loaded it end to end.
    /// </remarks>
    [TestMethod]
    public void SitemapVideoExtension_EveryField_IsParsed()
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
                  <video:content_loc>https://example.com/video.mp4</video:content_loc>
                  <video:player_loc>https://example.com/player.swf</video:player_loc>
                  <video:duration>600</video:duration>
                  <video:expiration_date>2030-11-05T19:20:30+08:00</video:expiration_date>
                  <video:rating>4.1</video:rating>
                  <video:view_count>12345</video:view_count>
                  <video:publication_date>2024-01-15T08:30:00+00:00</video:publication_date>
                  <video:family_friendly>yes</video:family_friendly>
                  <video:requires_subscription>no</video:requires_subscription>
                  <video:live>no</video:live>
                  <video:uploader>Example Uploader</video:uploader>
                  <video:platform relationship="allow">web mobile</video:platform>
                  <video:restriction relationship="deny">CA</video:restriction>
                </video:video>
              </url>
            </urlset>
            """;

        Sitemap sitemap = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        sitemap.Load(stream);

        SitemapUrl url = sitemap.Urls.Single();
        SitemapVideoExtension extension = url.Extensions.OfType<SitemapVideoExtension>().Single();
        SitemapVideo video = extension.Videos.Single();

        video.ThumbnailLocation.ShouldBe(new Uri("https://example.com/thumb.jpg"));
        video.Title.ShouldBe("Example Video");
        video.Description.ShouldBe("A sample video description");
        video.ContentLocation.ShouldBe(new Uri("https://example.com/video.mp4"));
        video.PlayerLocation.ShouldBe(new Uri("https://example.com/player.swf"));
        video.Duration.ShouldBe(600);
        video.ExpirationDate.ShouldNotBeNull();
        video.Rating.ShouldBe(4.1m);
        video.ViewCount.ShouldBe(12345);
        video.PublicationDate.ShouldNotBeNull();
        video.FamilyFriendly.ShouldBeTrue();
        video.RequiresSubscription.ShouldBeFalse();
        video.Live.ShouldBeFalse();
        video.Uploader.ShouldBe("Example Uploader");
        video.Platform.ShouldNotBeNull();
        video.Restriction.ShouldBe("CA");
    }

    /// <summary>
    /// Every child of an Atom entry's <c>source</c> element is read: its identifier, title, subtitle,
    /// rights, update time, icon, logo and generator.
    /// </summary>
    [TestMethod]
    public void AtomEntrySource_EveryField_IsParsed()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>Feed</title>
              <id>urn:uuid:feed</id>
              <updated>2024-01-15T08:30:00Z</updated>
              <entry>
                <title>Entry</title>
                <id>urn:uuid:entry</id>
                <updated>2024-01-15T09:30:00Z</updated>
                <source>
                  <id>urn:uuid:source</id>
                  <title>Source Title</title>
                  <subtitle>Source Subtitle</subtitle>
                  <rights>Source Rights</rights>
                  <updated>2024-01-14T07:00:00Z</updated>
                  <icon>https://example.com/icon.png</icon>
                  <logo>https://example.com/logo.png</logo>
                  <generator uri="https://example.com/gen" version="1.0">Example Generator</generator>
                </source>
              </entry>
            </feed>
            """;

        AtomFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        AtomSource source = feed.Entries.Single().Source.ShouldNotBeNull();
        source.Id.ShouldNotBeNull();
        source.Title.ShouldNotBeNull();
        source.Title.Content.ShouldBe("Source Title");
        source.Subtitle.ShouldNotBeNull();
        source.Subtitle.Content.ShouldBe("Source Subtitle");
        source.Rights.ShouldNotBeNull();
        source.Rights.Content.ShouldBe("Source Rights");
        source.Icon.ShouldNotBeNull();
        source.Logo.ShouldNotBeNull();
        source.Generator.ShouldNotBeNull();
        source.Generator.Content.ShouldBe("Example Generator");
        source.UpdatedOn.ShouldNotBe(DateTime.MinValue);
    }

    /// <summary>
    /// <c>RssItem.Load(navigator, settings)</c> fills all eight children of an <c>item</c> and reports
    /// success.
    /// </summary>
    /// <remarks>
    ///     The inline note gives the reason this needed writing: both <c>Load</c> overloads are public API
    ///     that no adapter calls, so nothing in the suite reached either one's child selections.
    /// </remarks>
    [TestMethod]
    public void RssItemLoadWithSettings_EveryField_IsParsed()
    {
        // Both RssItem.Load overloads are public API that no adapter calls, so neither one's eight
        // child selections were exercised by anything. Each is covered here.
        const string xml = """
            <item>
              <title>Item Title</title>
              <link>https://example.com/item</link>
              <description>Item description</description>
              <author>author@example.com</author>
              <comments>https://example.com/item/comments</comments>
              <guid isPermaLink="false">urn:uuid:item-1</guid>
              <pubDate>Mon, 15 Jan 2024 08:30:00 GMT</pubDate>
              <source url="https://example.com/feed.xml">Example Feed</source>
            </item>
            """;
        XPathDocument document = new(new StringReader(xml));
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToFirstChild();

        RssItem item = new();
        item.Load(navigator, new SyndicationResourceLoadSettings()).ShouldBeTrue();

        item.Title.ShouldBe("Item Title");
        item.Link.ShouldBe(new Uri("https://example.com/item"));
        item.Description.ShouldBe("Item description");
        item.Author.ShouldBe("author@example.com");
        item.Comments.ShouldBe(new Uri("https://example.com/item/comments"));
        item.Guid.ShouldNotBeNull();
        item.Guid.Value.ShouldBe("urn:uuid:item-1");
        item.PublicationDate.ShouldNotBe(DateTime.MinValue);
        item.Source.ShouldNotBeNull();
    }

    /// <summary>
    /// The settings-free overload reads the same eight children to the same values, so omitting the settings
    /// is not a different parse.
    /// </summary>
    [TestMethod]
    public void RssItemLoadWithoutSettings_EveryField_IsParsed()
    {
        const string xml = """
            <item>
              <title>Item Title</title>
              <link>https://example.com/item</link>
              <description>Item description</description>
              <author>author@example.com</author>
              <comments>https://example.com/item/comments</comments>
              <guid isPermaLink="false">urn:uuid:item-1</guid>
              <pubDate>Mon, 15 Jan 2024 08:30:00 GMT</pubDate>
              <source url="https://example.com/feed.xml">Example Feed</source>
            </item>
            """;
        XPathDocument document = new(new StringReader(xml));
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToFirstChild();

        RssItem item = new();
        item.Load(navigator).ShouldBeTrue();

        item.Title.ShouldBe("Item Title");
        item.Link.ShouldBe(new Uri("https://example.com/item"));
        item.Description.ShouldBe("Item description");
        item.Author.ShouldBe("author@example.com");
        item.Comments.ShouldBe(new Uri("https://example.com/item/comments"));
        item.Guid.ShouldNotBeNull();
        item.Guid.Value.ShouldBe("urn:uuid:item-1");
        item.PublicationDate.ShouldNotBe(DateTime.MinValue);
        item.Source.ShouldNotBeNull();
    }

    /// <summary>
    /// A channel's <c>image</c> yields all six of its fields, including the numeric <c>width</c> and
    /// <c>height</c>, and its <c>textInput</c> all four of its own.
    /// </summary>
    [TestMethod]
    public void RssChannelImageAndTextInput_EveryField_IsParsed()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0">
              <channel>
                <title>Feed</title>
                <link>https://example.com</link>
                <description>Feed description</description>
                <image>
                  <url>https://example.com/logo.png</url>
                  <title>Image Title</title>
                  <link>https://example.com/home</link>
                  <description>Image description</description>
                  <width>88</width>
                  <height>31</height>
                </image>
                <textInput>
                  <title>Search</title>
                  <description>Search this feed</description>
                  <name>q</name>
                  <link>https://example.com/search</link>
                </textInput>
              </channel>
            </rss>
            """;

        RssFeed feed = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        feed.Load(stream);

        RssImage image = feed.Channel.Image.ShouldNotBeNull();
        image.Url.ShouldBe(new Uri("https://example.com/logo.png"));
        image.Title.ShouldBe("Image Title");
        image.Link.ShouldBe(new Uri("https://example.com/home"));
        image.Description.ShouldBe("Image description");
        image.Width.ShouldBe(88);
        image.Height.ShouldBe(31);

        RssTextInput textInput = feed.Channel.TextInput.ShouldNotBeNull();
        textInput.Title.ShouldBe("Search");
        textInput.Description.ShouldBe("Search this feed");
        textInput.Name.ShouldBe("q");
        textInput.Link.ShouldBe(new Uri("https://example.com/search"));
    }
}