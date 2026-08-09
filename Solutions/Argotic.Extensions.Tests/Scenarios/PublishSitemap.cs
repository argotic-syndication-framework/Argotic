namespace Argotic.Extensions.Tests.Scenarios;

/// <summary>
/// Building a sitemap, writing it, and reading it back.
/// </summary>
/// <remarks>
///     <para>
///     Emitting <c>sitemap.xml</c> is the primary reason to reach for a sitemap library, and
///     <c>Sitemap.Save</c> was at zero coverage on all four overloads - no test had ever written one.
///     Every <c>WriteTo</c> in the sitemap extension family was cold as a direct consequence.
///     </para>
///     <para>
///     Sixteen files and 228 tests pointed at Sitemap before this, and the <see cref="Sitemap"/> type
///     itself was instantiated in one of them: the rest parse fixture strings with a raw
///     <c>XPathDocument</c> and assert with <c>SelectSingleNode</c>, which tests
///     <c>System.Xml.XPath</c> rather than this library.
///     </para>
/// </remarks>
[TestClass]
public class PublishSitemap
{
    /// <summary>
    /// A sitemap carrying all four Google extensions survives being written and read back.
    /// </summary>
    [TestMethod]
    public void ASitemapCarryingEveryExtension_SurvivesBeingWrittenAndReadBack()
    {
        Sitemap written = BuildSitemap();

        Sitemap read = SaveAndReload(written);

        read.Urls.Count.ShouldBe(2);

        SitemapUrl article = read.Urls[0];
        article.Location.ShouldBe(new Uri("https://example.com/news/story"));
        article.ChangeFrequency.ShouldBe(SitemapChangeFrequency.Daily);
        article.Priority.ShouldBe(0.8m);

        SitemapNewsExtension news = FindExtension<SitemapNewsExtension>(article);
        news.Title.ShouldBe("A breaking story");
        news.Publication.ShouldNotBeNull();
        news.Publication.Name.ShouldBe("The Example Times");
        news.Publication.Language.ShouldBe("en");
        news.PublicationDate.ShouldBe(new DateTime(2024, 1, 20, 9, 0, 0, DateTimeKind.Utc));

        SitemapImageExtension images = FindExtension<SitemapImageExtension>(article);
        images.Images.Count.ShouldBe(2);
        images.Images[0].Location.ShouldBe(new Uri("https://example.com/img/hero.jpg"));
        images.Images[1].Location.ShouldBe(new Uri("https://example.com/img/inline.jpg"));

        SitemapUrl watch = read.Urls[1];
        watch.Location.ShouldBe(new Uri("https://example.com/watch/1"));

        SitemapVideoExtension videos = FindExtension<SitemapVideoExtension>(watch);
        videos.Videos.Count.ShouldBe(1);
        SitemapVideo video = videos.Videos[0];
        video.Title.ShouldBe("An example video");
        video.Description.ShouldBe("A description of the example video");
        video.ThumbnailLocation.ShouldBe(new Uri("https://example.com/thumb.jpg"));
        video.ContentLocation.ShouldBe(new Uri("https://example.com/video.mp4"));
        video.Duration.ShouldBe(600);
        video.FamilyFriendly.ShouldBeTrue();
        video.Tags.ShouldBe(["example", "documentation"]);

        SitemapHreflangExtension hreflang = FindExtension<SitemapHreflangExtension>(watch);
        hreflang.Links.Count.ShouldBe(2);
        hreflang.Links[0].Hreflang.ShouldBe("en");
        hreflang.Links[0].Href.ShouldBe(new Uri("https://example.com/watch/1"));
        hreflang.Links[1].Hreflang.ShouldBe("fr");
        hreflang.Links[1].Href.ShouldBe(new Uri("https://example.fr/regarder/1"));
    }

    /// <summary>
    /// A video identifier survives the round trip, in every identifier type the schema defines.
    /// </summary>
    /// <remarks>
    ///     <c>SitemapVideoId.StringToType</c> and <c>TypeToString</c> are the mapping for all seven
    ///     <see cref="SitemapVideoIdType"/> values and neither had ever mapped one, in either direction.
    ///     No fixture in the repository contains a <c>video:id</c> element.
    /// </remarks>
    /// <param name="identifierType">The identifier type under test.</param>
    [TestMethod]
    [DataRow(SitemapVideoIdType.TmsSeries)]
    [DataRow(SitemapVideoIdType.TmsProgram)]
    [DataRow(SitemapVideoIdType.RoviSeries)]
    [DataRow(SitemapVideoIdType.RoviProgram)]
    [DataRow(SitemapVideoIdType.Freebase)]
    [DataRow(SitemapVideoIdType.Url)]
    public void AVideoIdentifier_SurvivesTheRoundTrip(SitemapVideoIdType identifierType)
    {
        SitemapVideo video = new()
        {
            Title = "An example video",
            Description = "A description",
            ThumbnailLocation = new Uri("https://example.com/thumb.jpg"),
            ContentLocation = new Uri("https://example.com/video.mp4"),
        };
        video.Identifiers.Add(new SitemapVideoId { Value = "identifier-value", Type = identifierType });

        SitemapVideoExtension extension = new();
        extension.Videos.Add(video);

        SitemapUrl url = new(new Uri("https://example.com/watch/1"));
        url.Extensions.Add(extension);

        Sitemap sitemap = new();
        sitemap.Urls.Add(url);

        Sitemap read = SaveAndReload(sitemap);

        SitemapVideo readVideo = FindExtension<SitemapVideoExtension>(read.Urls[0]).Videos[0];
        readVideo.Identifiers.Count.ShouldBe(1);
        readVideo.Identifiers[0].Value.ShouldBe("identifier-value");
        readVideo.Identifiers[0].Type.ShouldBe(identifierType);
    }

    /// <summary>
    /// A video content segment survives the round trip.
    /// </summary>
    [TestMethod]
    public void AVideoContentSegment_SurvivesTheRoundTrip()
    {
        SitemapVideo video = new()
        {
            Title = "An example video",
            Description = "A description",
            ThumbnailLocation = new Uri("https://example.com/thumb.jpg"),
            ContentLocation = new Uri("https://example.com/video.mp4"),
        };
        video.ContentSegments.Add(new SitemapVideoSegment
        {
            Location = new Uri("https://example.com/video-part-1.mp4"),
            Duration = 300,
        });

        SitemapVideoExtension extension = new();
        extension.Videos.Add(video);

        SitemapUrl url = new(new Uri("https://example.com/watch/1"));
        url.Extensions.Add(extension);

        Sitemap sitemap = new();
        sitemap.Urls.Add(url);

        Sitemap read = SaveAndReload(sitemap);

        SitemapVideo readVideo = FindExtension<SitemapVideoExtension>(read.Urls[0]).Videos[0];
        readVideo.ContentSegments.Count.ShouldBe(1);
        readVideo.ContentSegments[0].Location.ShouldBe(new Uri("https://example.com/video-part-1.mp4"));
        readVideo.ContentSegments[0].Duration.ShouldBe(300);
    }

    /// <summary>
    /// A sitemap with no extensions writes and reads its core fields.
    /// </summary>
    [TestMethod]
    public void APlainSitemap_SurvivesBeingWrittenAndReadBack()
    {
        Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl(new Uri("https://example.com/"), new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        {
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 1.0m,
        });

        Sitemap read = SaveAndReload(sitemap);

        read.Urls.Count.ShouldBe(1);
        read.Urls[0].Location.ShouldBe(new Uri("https://example.com/"));
        read.Urls[0].ChangeFrequency.ShouldBe(SitemapChangeFrequency.Monthly);
        read.Urls[0].Priority.ShouldBe(1.0m);

        // ShouldNotBeNull passed for a writer that emitted DateTime.Now, which is the regression a
        // lastmod round trip exists to catch.
        read.Urls[0].LastModified.ShouldBe(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// An extension attached to the sitemap itself — not to a url — survives being written and read back.
    /// </summary>
    /// <remarks>
    ///     <c>Sitemap.Save</c> writes document-level extensions on the <c>urlset</c> element, and the load
    ///     now probes for them there too: the old resource-side walk probed from the document node, where
    ///     no namespace is in scope, so what was written was never found again. The four Google extensions
    ///     all attach to <see cref="SitemapUrl"/> objects and never saw this; the round trip below is the
    ///     only pin on the document-level half.
    /// </remarks>
    [TestMethod]
    public void ADocumentLevelExtension_SurvivesBeingWrittenAndReadBack()
    {
        Sitemap written = new();
        written.Urls.Add(new SitemapUrl(new Uri("https://example.com/")));
        DublinCoreElementSetSyndicationExtension extension = new() { Context = { Title = "A document-level title" } };
        written.Extensions.Add(extension);

        Sitemap read = SaveAndReload(written);

        DublinCoreElementSetSyndicationExtension? readExtension =
            read.Extensions.OfType<DublinCoreElementSetSyndicationExtension>().SingleOrDefault();
        readExtension.ShouldNotBeNull();
        readExtension.Context.Title.ShouldBe("A document-level title");
    }

    private static Sitemap BuildSitemap()
    {
        SitemapNewsExtension news = new()
        {
            Title = "A breaking story",
            PublicationDate = new DateTime(2024, 1, 20, 9, 0, 0, DateTimeKind.Utc),
            Publication = new SitemapNewsPublication { Name = "The Example Times", Language = "en" },
        };

        SitemapImageExtension images = new();
        images.Images.Add(new SitemapImage { Location = new Uri("https://example.com/img/hero.jpg") });
        images.Images.Add(new SitemapImage { Location = new Uri("https://example.com/img/inline.jpg") });

        SitemapUrl article = new(new Uri("https://example.com/news/story"))
        {
            ChangeFrequency = SitemapChangeFrequency.Daily,
            Priority = 0.8m,
        };
        article.Extensions.Add(news);
        article.Extensions.Add(images);

        SitemapVideo video = new()
        {
            Title = "An example video",
            Description = "A description of the example video",
            ThumbnailLocation = new Uri("https://example.com/thumb.jpg"),
            ContentLocation = new Uri("https://example.com/video.mp4"),
            Duration = 600,
            FamilyFriendly = true,
        };
        video.Tags.Add("example");
        video.Tags.Add("documentation");

        SitemapVideoExtension videos = new();
        videos.Videos.Add(video);

        SitemapHreflangExtension hreflang = new();
        hreflang.Links.Add(new SitemapHreflangLink { Hreflang = "en", Href = new Uri("https://example.com/watch/1") });
        hreflang.Links.Add(new SitemapHreflangLink { Hreflang = "fr", Href = new Uri("https://example.fr/regarder/1") });

        SitemapUrl watch = new(new Uri("https://example.com/watch/1"));
        watch.Extensions.Add(videos);
        watch.Extensions.Add(hreflang);

        Sitemap sitemap = new();
        sitemap.Urls.Add(article);
        sitemap.Urls.Add(watch);

        return sitemap;
    }

    private static Sitemap SaveAndReload(Sitemap sitemap)
    {
        using MemoryStream stream = new();
        sitemap.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        Sitemap read = new();
        read.Load(stream);
        return read;
    }

    private static TExtension FindExtension<TExtension>(SitemapUrl url)
        where TExtension : class, ISyndicationExtension
    {
        url.HasExtensions.ShouldBeTrue($"{url.Location} carries no extensions after the round trip");
        return url.Extensions.OfType<TExtension>().SingleOrDefault()
            .ShouldNotBeNull($"{url.Location} lost its {typeof(TExtension).Name} in the round trip");
    }
}