using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Builds a <see cref="SitemapUrl"/> through both the property initialiser and the constructor overloads.
/// </summary>
internal static class SitemapUrlExample
{
    /// <summary>
    /// Builds a <see cref="SitemapUrl"/> through the property initialiser and prints its parts.
    /// </summary>
    public static void ClassExample()
    {
        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-do/data-and-analytics/data-platforms/duckdb/"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        Console.WriteLine($"URL: {url.Location}");
        Console.WriteLine($"Last Modified: {url.LastModified}");
        Console.WriteLine($"Change Frequency: {url.ChangeFrequency}");
        Console.WriteLine($"Priority: {url.Priority}");

        ExampleOutput.ShowCreated("SitemapUrl", url.Location?.ToString());
    }

    /// <summary>
    /// Provides example code demonstrating the ChangeFrequency property.
    /// </summary>
    public static void ChangeFrequencyExample()
    {
        // Demonstrate different change frequencies for various content types
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
    /// Provides example code demonstrating the Priority property.
    /// </summary>
    public static void PriorityExample()
    {
        // Demonstrate priority values for different pages
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
    /// Provides example code demonstrating a fully configured SitemapUrl.
    /// </summary>
    public static void FullConfigurationExample()
    {
        // Create a fully configured URL entry with all properties
        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-do/pricing/"),
            LastModified = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc),
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.9m
        };

        // Display the result
        Console.WriteLine("Fully configured SitemapUrl:");
        Console.WriteLine($"  Location: {url.Location}");
        Console.WriteLine($"  Last Modified: {url.LastModified}");
        Console.WriteLine($"  Change Frequency: {url.ChangeFrequency}");
        Console.WriteLine($"  Priority: {url.Priority}");
        Console.WriteLine($"  Extensions: {url.Extensions.Count}");

        ExampleOutput.ShowCreated("SitemapUrl", url.Location?.ToString());
    }

    /// <summary>
    /// Builds the same <see cref="SitemapUrl"/> through each constructor overload instead.
    /// </summary>
    public static void ConstructorExample()
    {
        // Default constructor
        SitemapUrl url1 = new();
        url1.Location = new Uri("https://endjin.com/what-we-do/pricing/");

        // Constructor with location
        SitemapUrl url2 = new(new Uri("https://endjin.com/who-we-help/"));

        // Constructor with location and last modified date
        SitemapUrl url3 = new(new Uri("https://endjin.com/page3"), DateTime.UtcNow);

        Console.WriteLine("Created three SitemapUrl instances:");
        Console.WriteLine($"  URL 1: {url1.Location}");
        Console.WriteLine($"  URL 2: {url2.Location}");
        Console.WriteLine($"  URL 3: {url3.Location} (Last Modified: {url3.LastModified})");

        ExampleOutput.ShowCreated("SitemapUrl", "3 instances");
    }

    /// <summary>
    /// Provides example code demonstrating SitemapUrl comparison.
    /// </summary>
    public static void ComparisonExample()
    {
        SitemapUrl url1 = new()
        {
            Location = new Uri("https://endjin.com/what-we-do/pricing/"),
            Priority = 0.8m
        };

        SitemapUrl url2 = new()
        {
            Location = new Uri("https://endjin.com/what-we-do/pricing/"),
            Priority = 0.8m
        };

        SitemapUrl url3 = new()
        {
            Location = new Uri("https://endjin.com/who-we-help/"),
            Priority = 0.9m
        };

        Console.WriteLine("SitemapUrl comparison examples:");
        Console.WriteLine($"  url1 == url2: {url1 == url2}");
        Console.WriteLine($"  url1 == url3: {url1 == url3}");
        Console.WriteLine($"  url1.Equals(url2): {url1.Equals(url2)}");
        Console.WriteLine($"  url1.CompareTo(url3): {url1.CompareTo(url3)}");
    }
}