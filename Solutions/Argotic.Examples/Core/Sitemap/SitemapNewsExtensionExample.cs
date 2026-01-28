using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Contains the code examples for the <see cref="SitemapNewsExtension"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="SitemapNewsExtension"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class SitemapNewsExtensionExample
{
    /// <summary>
    /// Provides example code for the SitemapNewsExtension class.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/news/breaking-story"),
            LastModified = DateTime.UtcNow
        };

        SitemapNewsPublication publication = new("Example News", "en");

        SitemapNewsExtension newsExtension = new()
        {
            Publication = publication,
            PublicationDate = DateTime.UtcNow,
            Title = "Breaking: Major Technology Announcement"
        };

        url.Extensions.Add(newsExtension);
        sitemap.Urls.Add(url);

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code for loading a sitemap with news extensions from a stream.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapNews);
        sitemap.Load(stream);

        foreach (SitemapUrl url in sitemap.Urls)
        {
            SitemapNewsExtension? newsExtension = url.FindExtension(SitemapNewsExtension.MatchByType) as SitemapNewsExtension;
            if (newsExtension != null)
            {
                Console.WriteLine($"URL: {url.Location}");
                ExampleOutput.ShowSitemapNewsExtension(newsExtension);
            }
        }
    }

    /// <summary>
    /// Provides example code for saving a sitemap with news extensions to a stream.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/news/article-123"),
            LastModified = DateTime.UtcNow
        };

        SitemapNewsExtension newsExtension = new()
        {
            Publication = new SitemapNewsPublication("Tech Daily", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "New Product Launch Announced"
        };

        url.Extensions.Add(newsExtension);
        sitemap.Urls.Add(url);

        using Stream stream = new MemoryStream();
        sitemap.Save(stream);

        ExampleOutput.ShowSaved("Sitemap with News Extension");
    }

    /// <summary>
    /// Provides example code demonstrating multi-language news articles.
    /// </summary>
    public static void MultilingualExample()
    {
        Syndication.Sitemap sitemap = new();

        // English article
        SitemapUrl englishUrl = new()
        {
            Location = new Uri("https://www.example.com/en/news/global-summit"),
            LastModified = DateTime.UtcNow
        };
        englishUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("World News Network", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "Global Leaders Meet at Climate Summit"
        });
        sitemap.Urls.Add(englishUrl);

        // German article
        SitemapUrl germanUrl = new()
        {
            Location = new Uri("https://www.example.com/de/news/global-summit"),
            LastModified = DateTime.UtcNow
        };
        germanUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Welt Nachrichten Netzwerk", "de"),
            PublicationDate = DateTime.UtcNow,
            Title = "Weltweite Fuhrer treffen sich beim Klimagipfel"
        });
        sitemap.Urls.Add(germanUrl);

        // French article
        SitemapUrl frenchUrl = new()
        {
            Location = new Uri("https://www.example.com/fr/news/global-summit"),
            LastModified = DateTime.UtcNow
        };
        frenchUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Reseau Mondial d'Actualites", "fr"),
            PublicationDate = DateTime.UtcNow,
            Title = "Les dirigeants mondiaux se reunissent au sommet sur le climat"
        });
        sitemap.Urls.Add(frenchUrl);

        Console.WriteLine("Multilingual news sitemap created with articles in English, German, and French.");
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code demonstrating different publication configurations.
    /// </summary>
    public static void PublicationConfigurationExample()
    {
        Syndication.Sitemap sitemap = new();

        // Tech publication
        SitemapUrl techUrl = new()
        {
            Location = new Uri("https://www.example.com/tech/ai-breakthrough"),
            LastModified = DateTime.UtcNow
        };
        techUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Tech Weekly", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "Revolutionary AI System Unveiled"
        });
        sitemap.Urls.Add(techUrl);

        // Business publication
        SitemapUrl businessUrl = new()
        {
            Location = new Uri("https://www.example.com/business/market-update"),
            LastModified = DateTime.UtcNow
        };
        businessUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Business Daily", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "Stock Markets Reach New Highs"
        });
        sitemap.Urls.Add(businessUrl);

        Console.WriteLine("News sitemap with multiple publications (Tech Weekly, Business Daily).");
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code demonstrating a news site with multiple articles.
    /// </summary>
    public static void NewsSiteExample()
    {
        Syndication.Sitemap sitemap = new();

        string[] headlines =
        [
            "Breaking: Major Policy Change Announced",
            "Sports: Championship Finals Preview",
            "Entertainment: Award Show Highlights",
            "Science: New Discovery in Space Exploration",
            "Health: Breakthrough in Medical Research"
        ];

        for (int i = 0; i < headlines.Length; i++)
        {
            SitemapUrl url = new()
            {
                Location = new Uri($"https://www.example.com/news/article-{i + 1}"),
                LastModified = DateTime.UtcNow.AddHours(-i)
            };

            url.Extensions.Add(new SitemapNewsExtension
            {
                Publication = new SitemapNewsPublication("Daily News", "en"),
                PublicationDate = DateTime.UtcNow.AddHours(-i),
                Title = headlines[i]
            });

            sitemap.Urls.Add(url);
        }

        Console.WriteLine($"News sitemap created with {sitemap.Urls.Count} articles.");
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code demonstrating Google News 2-day freshness requirement.
    /// </summary>
    /// <remarks>
    /// Google News requires that articles be added to the sitemap within 2 days of publication.
    /// Articles older than 2 days should not be included in the news sitemap as they will be ignored
    /// by Google News indexing. Use a regular sitemap for older content.
    /// </remarks>
    public static void ArticleFreshnessExample()
    {
        Syndication.Sitemap sitemap = new();

        // Article published 12 hours ago - within freshness window
        SitemapUrl recentUrl = new()
        {
            Location = new Uri("https://www.example.com/news/fresh-article"),
            LastModified = DateTime.UtcNow.AddHours(-12)
        };
        recentUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Breaking News Daily", "en"),
            // Google News requires articles to be published within the last 2 days
            // This article was published 12 hours ago, meeting the freshness requirement
            PublicationDate = DateTime.UtcNow.AddHours(-12),
            Title = "Just In: Latest Market Analysis"
        });
        sitemap.Urls.Add(recentUrl);

        // Another recent article - published 6 hours ago
        SitemapUrl veryRecentUrl = new()
        {
            Location = new Uri("https://www.example.com/news/very-fresh-article"),
            LastModified = DateTime.UtcNow.AddHours(-6)
        };
        veryRecentUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Breaking News Daily", "en"),
            PublicationDate = DateTime.UtcNow.AddHours(-6),
            Title = "Breaking: Emergency Response Update"
        });
        sitemap.Urls.Add(veryRecentUrl);

        Console.WriteLine("News sitemap with fresh articles (published within 2-day window).");
        Console.WriteLine("Note: Google News requires articles to be added within 2 days of publication.");
        ExampleOutput.ShowSitemap(sitemap);
    }
}