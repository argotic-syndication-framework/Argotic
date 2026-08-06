using System.Xml;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Contains the code examples for the <see cref="Syndication.Sitemap"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="Syndication.Sitemap"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class SitemapExample
{
    /// <summary>
    /// Provides example code for the Sitemap class.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Daily,
            Priority = 1.0m
        });
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/about"),
            LastModified = new DateTime(2024, 1, 15),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 0.8m
        });
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code for the Sitemap.CreateAsync(Uri) method.
    /// </summary>
    [RequiresNetwork]
    public static async Task LoadUriExampleAsync()
    {
        Syndication.Sitemap sitemap = await Syndication.Sitemap.CreateAsync(
            new Uri("https://endjin.com/sitemap.xml")).ConfigureAwait(false);
        foreach (SitemapUrl url in sitemap.Urls)
        {
            Console.WriteLine($"URL: {url.Location}");
        }
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.Sitemap);
        sitemap.Load(stream);
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/page1"),
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.7m
        });
        using Stream stream = new MemoryStream();
        sitemap.Save(stream);
        ExampleOutput.ShowSaved("Sitemap");
    }

    /// <summary>
    /// Provides example code demonstrating all change frequencies.
    /// </summary>
    public static void ChangeFrequencyExample()
    {
        Syndication.Sitemap sitemap = new();

        // Homepage - changes frequently
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/"),
            ChangeFrequency = SitemapChangeFrequency.Always
        });

        // News page - daily updates
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/news"),
            ChangeFrequency = SitemapChangeFrequency.Daily
        });

        // Blog archive - updated weekly
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/blog"),
            ChangeFrequency = SitemapChangeFrequency.Weekly
        });

        // About page - rarely changes
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/about"),
            ChangeFrequency = SitemapChangeFrequency.Yearly
        });

        // Archive page - never changes
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/archive/2020"),
            ChangeFrequency = SitemapChangeFrequency.Never
        });

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code demonstrating priority values.
    /// </summary>
    public static void PriorityExample()
    {
        Syndication.Sitemap sitemap = new();

        // Homepage - highest priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/"),
            Priority = 1.0m
        });

        // Main category pages - high priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/products"),
            Priority = 0.9m
        });

        // Product pages - medium-high priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/products/featured-item"),
            Priority = 0.8m
        });

        // Blog posts - medium priority (default)
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/blog/recent-post"),
            Priority = 0.5m
        });

        // Legal pages - low priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/terms"),
            Priority = 0.2m
        });

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method.
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        Syndication.Sitemap sitemap = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.Sitemap);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        sitemap.Load(reader);

        foreach (SitemapUrl url in sitemap.Urls)
        {
            Console.WriteLine($"URL: {url.Location}");
            if (url.LastModified.HasValue)
            {
                Console.WriteLine($"  Last Modified: {url.LastModified.Value:yyyy-MM-dd}");
            }
        }
    }

    /// <summary>
    /// Provides example code for saving with XmlWriter.
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://www.example.com/"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Daily,
            Priority = 1.0m
        });

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        sitemap.Save(writer);

        ExampleOutput.ShowSaved("Sitemap");
    }
}