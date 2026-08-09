using System.Xml;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Demonstrates the <see cref="Syndication.Sitemap"/> surface: listing URLs by hand, then the <c>Load</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
internal static class SitemapExample
{
    /// <summary>
    /// Builds a <see cref="Syndication.Sitemap"/> listing two URLs and prints it.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Daily,
            Priority = 1.0m
        });
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/who-we-are/"),
            LastModified = new DateTime(2024, 1, 15),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 0.8m
        });
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Creates a <see cref="Syndication.Sitemap"/> from a <see cref="Uri"/> in a single call.
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
    /// Loads a <see cref="Syndication.Sitemap"/> from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.Sitemap);
        sitemap.Load(stream);
        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Saves a <see cref="Syndication.Sitemap"/> to a <see cref="Stream"/>.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/what-we-do/pricing/"),
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
            Location = new Uri("https://endjin.com/"),
            ChangeFrequency = SitemapChangeFrequency.Always
        });

        // News page - daily updates
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/what-we-think/"),
            ChangeFrequency = SitemapChangeFrequency.Daily
        });

        // Blog archive - updated weekly
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/blog/"),
            ChangeFrequency = SitemapChangeFrequency.Weekly
        });

        // About page - rarely changes
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/who-we-are/"),
            ChangeFrequency = SitemapChangeFrequency.Yearly
        });

        // Archive page - never changes
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/blog/page/2"),
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
            Location = new Uri("https://endjin.com/"),
            Priority = 1.0m
        });

        // Main category pages - high priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/what-we-do/"),
            Priority = 0.9m
        });

        // Product pages - medium-high priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/what-we-do/cloud-native-app-dev/microsoft-azure/"),
            Priority = 0.8m
        });

        // Blog posts - medium priority (default)
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases"),
            Priority = 0.5m
        });

        // Legal pages - low priority
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/contact-us/"),
            Priority = 0.2m
        });

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Loads a <see cref="Syndication.Sitemap"/> from an <see cref="XmlReader"/>.
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
    /// Saves a <see cref="Syndication.Sitemap"/> through an <see cref="XmlWriter"/>.
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        Syndication.Sitemap sitemap = new();
        sitemap.Urls.Add(new SitemapUrl
        {
            Location = new Uri("https://endjin.com/"),
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