namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// The opt-in <c>xsi:schemaLocation</c> output on the two sitemap roots.
/// </summary>
/// <remarks>
///     <para>
///     Issue 177: consumers that validate sitemaps against the XSD schemas want the attribute as a
///     validation hint. The option lives on <see cref="SyndicationResourceSaveSettings"/>, defaults to
///     off, and must not change the default output. A <c>urlset</c> root pairs the core namespace with
///     <c>sitemap.xsd</c>; a <c>sitemapindex</c> root pairs the same namespace with
///     <c>siteindex.xsd</c> — same namespace, different schema, so the root decides the pair.
///     </para>
///     <para>
///     Pairs are written only for namespaces with a known schema location: the core namespace and the
///     Google news, image and video extensions. The hreflang extension's <c>xhtml</c> namespace has no
///     sitemap schema, so it is declared but not paired. The XSD locations keep the <c>http</c> scheme
///     of the protocol's own examples; the integration tier fetches the same files over <c>https</c>.
///     </para>
///     <para>
///     The conformance test here asserts the attribute is present before it validates. Without that,
///     the test would pass against a writer that never wrote the attribute at all.
///     </para>
/// </remarks>
[TestClass]
public class SitemapSchemaLocationTests
{
    private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    private const string CorePair = "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd";

    private const string IndexPair = "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd";

    private const string NewsPair = "http://www.google.com/schemas/sitemap-news/0.9 http://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd";

    private const string ImagePair = "http://www.google.com/schemas/sitemap-image/1.1 http://www.google.com/schemas/sitemap-image/1.1/sitemap-image.xsd";

    private const string VideoPair = "http://www.google.com/schemas/sitemap-video/1.1 http://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd";

    /// <summary>
    /// A plain sitemap saved with the option on declares xsi and writes the core pair.
    /// </summary>
    [TestMethod]
    public void APlainSitemapWithTheOptionOn_WritesTheCoreSchemaLocationPair()
    {
        string xml = Save(PlainSitemap(), OptionOn());

        xml.ShouldContain($"xmlns:xsi=\"{XsiNamespace}\"");
        SchemaLocationValue(xml).ShouldBe(CorePair);
    }

    /// <summary>
    /// A sitemap carrying news and image extensions writes three pairs, core first.
    /// </summary>
    [TestMethod]
    public void ASitemapCarryingNewsAndImages_WritesThreePairs()
    {
        string xml = Save(SitemapWithNewsAndImages(), OptionOn());

        SchemaLocationValue(xml).ShouldBe(
            $"{CorePair} {NewsPair} {ImagePair}",
            "the pairs must follow the extension declaration order, after the core pair");
    }

    /// <summary>
    /// A sitemap carrying the video extension writes two pairs.
    /// </summary>
    [TestMethod]
    public void ASitemapCarryingVideo_WritesTwoPairs()
    {
        string xml = Save(SitemapWithVideo(), OptionOn());

        SchemaLocationValue(xml).ShouldBe($"{CorePair} {VideoPair}");
    }

    /// <summary>
    /// An empty sitemap with pre-registered extensions writes the registered pairs.
    /// </summary>
    /// <remarks>
    ///     Auto-detection finds nothing here — there are no urls — so the pair can come only from
    ///     <see cref="SyndicationResourceSaveSettings.SupportedExtensions"/>.
    /// </remarks>
    [TestMethod]
    public void AnEmptySitemapWithPreRegisteredExtensions_WritesTheRegisteredPairs()
    {
        SyndicationResourceSaveSettings settings = OptionOn();
        settings.SupportedExtensions.Add(typeof(SitemapNewsExtension));

        string xml = Save(new Syndication.Sitemap(), settings);

        SchemaLocationValue(xml).ShouldBe($"{CorePair} {NewsPair}");
    }

    /// <summary>
    /// The hreflang extension's xhtml namespace is declared but not paired.
    /// </summary>
    [TestMethod]
    public void ASitemapCarryingHreflangAnnotations_SkipsTheXhtmlNamespace()
    {
        string xml = Save(SitemapWithHreflang(), OptionOn());

        xml.ShouldContain(
            "xmlns:xhtml=\"http://www.w3.org/1999/xhtml\"",
            customMessage: "the namespace declaration itself must not change");
        SchemaLocationValue(xml).ShouldBe(
            CorePair,
            "no sitemap schema exists for the xhtml namespace, so it has no pair");
    }

    /// <summary>
    /// A sitemap index saved with the option on pairs the namespace with siteindex.xsd, not sitemap.xsd.
    /// </summary>
    [TestMethod]
    public void ASitemapIndexWithTheOptionOn_WritesTheSiteindexSchemaLocation()
    {
        SitemapIndex index = new();
        index.Sitemaps.Add(new SitemapIndexEntry { Location = new Uri("https://example.com/sitemap.xml") });

        string xml = Save(index, OptionOn());

        xml.ShouldContain($"xmlns:xsi=\"{XsiNamespace}\"");
        SchemaLocationValue(xml).ShouldBe(
            IndexPair,
            "a sitemapindex root validates against siteindex.xsd, not sitemap.xsd");
    }

    /// <summary>
    /// The attribute value alternates namespace and schema tokens, whatever is attached.
    /// </summary>
    [TestMethod]
    public void TheSchemaLocationValue_AlternatesNamespaceAndXsdTokens()
    {
        string xml = Save(SitemapWithEveryExtension(), OptionOn());

        string[] tokens = SchemaLocationValue(xml)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        tokens.Length.ShouldBeGreaterThan(0);
        (tokens.Length % 2).ShouldBe(0, "the value must hold whole namespace/schema pairs");
        for (int i = 1; i < tokens.Length; i += 2)
        {
            tokens[i].ShouldEndWith(".xsd", customMessage: $"token {i} must be a schema location");
        }
    }

    /// <summary>
    /// Output written with the option on still conforms to the sitemaps.org schemas.
    /// </summary>
    /// <remarks>
    ///     The validator never processes <c>xsi:schemaLocation</c> — <c>ProcessSchemaLocation</c> is
    ///     off in <see cref="ConformanceSchemas"/> — but the XSD specification permits the four
    ///     <c>xsi:</c> attributes on any element without an <c>anyAttribute</c> declaration. This test
    ///     proves that claim against the real validator rather than asserting it in prose. The presence
    ///     assertion first keeps the test able to fail against a writer that writes nothing.
    /// </remarks>
    [TestMethod]
    public void ASitemapWithTheSchemaLocationOn_StillConformsToTheSitemapsOrgSchema()
    {
        string plain = Save(PlainSitemap(), OptionOn());
        string hreflang = Save(SitemapWithHreflang(), OptionOn());

        SitemapIndex index = new();
        index.Sitemaps.Add(new SitemapIndexEntry { Location = new Uri("https://example.com/sitemap.xml") });
        string indexXml = Save(index, OptionOn());

        SchemaLocationValue(plain).ShouldNotBeEmpty("the option is on, so the attribute must be present");
        SchemaLocationValue(hreflang).ShouldNotBeEmpty();
        SchemaLocationValue(indexXml).ShouldNotBeEmpty();

        ConformanceSchemas.Describe(plain, ConformanceSchemas.Sitemap)
            .ShouldBeEmpty("the xsi attributes made a plain sitemap invalid");
        ConformanceSchemas.Describe(hreflang, ConformanceSchemas.Sitemap)
            .ShouldBeEmpty("the xsi attributes made an hreflang sitemap invalid");
        ConformanceSchemas.Describe(indexXml, ConformanceSchemas.Sitemap)
            .ShouldBeEmpty("the xsi attributes made a sitemap index invalid");
    }

    /// <summary>
    /// The default output carries no xsi attribute and no xsi declaration.
    /// </summary>
    /// <remarks>
    ///     The pin on "the default output does not change". Proven able to fail by a scratch run with
    ///     the default flipped to <see langword="true"/>; see the engineering log for issue 177.
    /// </remarks>
    [TestMethod]
    public void ASitemapSavedWithDefaults_WritesNoXsiAttributes()
    {
        string xml = Save(SitemapWithEveryExtension(), new SyndicationResourceSaveSettings());

        xml.ShouldNotContain("xsi");
        xml.ShouldNotContain("schemaLocation");
    }

    private static SyndicationResourceSaveSettings OptionOn() => new() { WriteXsiSchemaLocation = true };

    private static Syndication.Sitemap PlainSitemap()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl(new Uri("https://example.com/")));
        return sitemap;
    }

    private static Syndication.Sitemap SitemapWithNewsAndImages()
    {
        SitemapNewsExtension news = new()
        {
            Title = "A breaking story",
            PublicationDate = new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc),
            Publication = new SitemapNewsPublication { Name = "The Example Times", Language = "en" },
        };

        SitemapImageExtension images = new();
        images.Images.Add(new SitemapImage { Location = new Uri("https://example.com/img/hero.jpg") });

        SitemapUrl url = new(new Uri("https://example.com/news/story"));
        url.Extensions.Add(news);
        url.Extensions.Add(images);

        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(url);
        return sitemap;
    }

    private static Syndication.Sitemap SitemapWithVideo()
    {
        SitemapVideo video = new()
        {
            Title = "An example video",
            Description = "A description of the example video",
            ThumbnailLocation = new Uri("https://example.com/thumb.jpg"),
            ContentLocation = new Uri("https://example.com/video.mp4"),
        };

        SitemapVideoExtension extension = new();
        extension.Videos.Add(video);

        SitemapUrl url = new(new Uri("https://example.com/watch/1"));
        url.Extensions.Add(extension);

        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(url);
        return sitemap;
    }

    private static Syndication.Sitemap SitemapWithHreflang()
    {
        SitemapHreflangExtension hreflang = new();
        hreflang.Links.Add(new SitemapHreflangLink { Hreflang = "en", Href = new Uri("https://example.com/") });
        hreflang.Links.Add(new SitemapHreflangLink { Hreflang = "fr", Href = new Uri("https://example.fr/") });

        SitemapUrl url = new(new Uri("https://example.com/"));
        url.Extensions.Add(hreflang);

        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(url);
        return sitemap;
    }

    private static Syndication.Sitemap SitemapWithEveryExtension()
    {
        Syndication.Sitemap sitemap = SitemapWithNewsAndImages();
        sitemap.Urls.Add(SitemapWithVideo().Urls[0]);
        sitemap.Urls.Add(SitemapWithHreflang().Urls[0]);
        return sitemap;
    }

    private static string Save(Syndication.Sitemap sitemap, SyndicationResourceSaveSettings settings)
    {
        using MemoryStream stream = new();
        sitemap.Save(stream, settings);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string Save(SitemapIndex index, SyndicationResourceSaveSettings settings)
    {
        using MemoryStream stream = new();
        index.Save(stream, settings);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string SchemaLocationValue(string xml)
    {
        using StringReader reader = new(xml.TrimStart('﻿'));
        XPathDocument document = new(reader);
        XPathNavigator navigator = document.CreateNavigator();
        navigator.MoveToChild(XPathNodeType.Element).ShouldBeTrue("the document has no root element");
        return navigator.GetAttribute("schemaLocation", XsiNamespace);
    }
}