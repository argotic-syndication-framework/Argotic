using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Declares news-article metadata with <see cref="SitemapNewsExtension"/>, then reads it back out of a saved sitemap.
/// </summary>
internal static class SitemapNewsExtensionExample
{
    /// <summary>
    /// Attaches a <see cref="SitemapNewsExtension"/> to a <see cref="SitemapUrl"/> and prints the sitemap.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases"),
            LastModified = DateTime.UtcNow
        };

        SitemapNewsPublication publication = new("endjin.com", "en");

        SitemapNewsExtension newsExtension = new()
        {
            Publication = publication,
            PublicationDate = DateTime.UtcNow,
            Title = "Writing Effective Copilot Instructions for Complex Codebases"
        };

        url.Extensions.Add(newsExtension);
        sitemap.Urls.Add(url);

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Reads the news extension back out of a sitemap loaded from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapNews);
        sitemap.Load(stream);

        foreach (SitemapUrl url in sitemap.Urls)
        {
            if (url.FindExtension(SitemapNewsExtension.MatchByType) is SitemapNewsExtension newsExtension)
            {
                Console.WriteLine($"URL: {url.Location}");
                ExampleOutput.ShowSitemapNewsExtension(newsExtension);
            }
        }
    }

    /// <summary>
    /// Saves a sitemap carrying the news extension, and shows the namespace it declares.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
            LastModified = DateTime.UtcNow
        };

        SitemapNewsExtension newsExtension = new()
        {
            Publication = new SitemapNewsPublication("endjin.com", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "Optimising DAX: The Formula Engine and Storage Engine"
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
            Location = new Uri("https://endjin.com/en/what-we-think/talks/rxdotnet-v7-0-released"),
            LastModified = DateTime.UtcNow
        };
        englishUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("endjin.com", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "Rx.NET v7.0 Released - it could save you 95MB!"
        });
        sitemap.Urls.Add(englishUrl);

        // German article
        SitemapUrl germanUrl = new()
        {
            Location = new Uri("https://endjin.com/de/what-we-think/talks/rxdotnet-v7-0-released"),
            LastModified = DateTime.UtcNow
        };
        germanUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("endjin.com", "de"),
            PublicationDate = DateTime.UtcNow,
            Title = "Rx.NET v7.0 veroeffentlicht - das spart bis zu 95 MB!"
        });
        sitemap.Urls.Add(germanUrl);

        // French article
        SitemapUrl frenchUrl = new()
        {
            Location = new Uri("https://endjin.com/fr/what-we-think/talks/rxdotnet-v7-0-released"),
            LastModified = DateTime.UtcNow
        };
        frenchUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("endjin.com", "fr"),
            PublicationDate = DateTime.UtcNow,
            Title = "Rx.NET v7.0 publie - jusqu'a 95 Mo economises !"
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
            Location = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
            LastModified = DateTime.UtcNow
        };
        techUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("Azure Weekly", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "The GenAI Reality Check: New Instrument, Same Orchestra"
        });
        sitemap.Urls.Add(techUrl);

        // Business publication
        SitemapUrl businessUrl = new()
        {
            Location = new Uri("https://endjin.com/blog/cloud-ai-slas-are-not-what-you-think"),
            LastModified = DateTime.UtcNow
        };
        businessUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("endjin.com", "en"),
            PublicationDate = DateTime.UtcNow,
            Title = "Cloud AI SLAs are not what you think"
        });
        sitemap.Urls.Add(businessUrl);

        Console.WriteLine("News sitemap with multiple publications (Azure Weekly, endjin.com).");
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
                Location = new Uri($"https://endjin.com/blog/asyncapi-code-generation-with-corvus-part-{i + 1}"),
                LastModified = DateTime.UtcNow.AddHours(-i)
            };

            url.Extensions.Add(new SitemapNewsExtension
            {
                Publication = new SitemapNewsPublication("endjin.com", "en"),
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
            Location = new Uri("https://endjin.com/blog/trying-out-wsl-containers"),
            LastModified = DateTime.UtcNow.AddHours(-12)
        };
        recentUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("endjin.com", "en"),
            // Google News requires articles to be published within the last 2 days
            // This article was published 12 hours ago, meeting the freshness requirement
            PublicationDate = DateTime.UtcNow.AddHours(-12),
            Title = "Trying out WSL containers"
        });
        sitemap.Urls.Add(recentUrl);

        // Another recent article - published 6 hours ago
        SitemapUrl veryRecentUrl = new()
        {
            Location = new Uri("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases"),
            LastModified = DateTime.UtcNow.AddHours(-6)
        };
        veryRecentUrl.Extensions.Add(new SitemapNewsExtension
        {
            Publication = new SitemapNewsPublication("endjin.com", "en"),
            PublicationDate = DateTime.UtcNow.AddHours(-6),
            Title = "Writing Effective Copilot Instructions for Complex Codebases"
        });
        sitemap.Urls.Add(veryRecentUrl);

        Console.WriteLine("News sitemap with fresh articles (published within 2-day window).");
        Console.WriteLine("Note: Google News requires articles to be added within 2 days of publication.");
        ExampleOutput.ShowSitemap(sitemap);
    }
}