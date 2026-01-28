using System.Xml;
using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Contains the code examples for the <see cref="SitemapIndex"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="SitemapIndex"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class SitemapIndexExample
{
    /// <summary>
    /// Provides example code for the SitemapIndex class.
    /// </summary>
    public static void ClassExample()
    {
        SitemapIndex index = new();
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://www.example.com/sitemap-pages.xml"),
            LastModified = DateTime.UtcNow
        });
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://www.example.com/sitemap-products.xml"),
            LastModified = new DateTime(2024, 1, 14)
        });
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://www.example.com/sitemap-blog.xml"),
            LastModified = new DateTime(2024, 1, 10)
        });
        ExampleOutput.ShowSitemapIndex(index);
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method.
    /// </summary>
    public static void LoadStreamExample()
    {
        SitemapIndex index = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapIndex);
        index.Load(stream);
        ExampleOutput.ShowSitemapIndex(index);
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method.
    /// </summary>
    public static void SaveStreamExample()
    {
        SitemapIndex index = new();
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://www.example.com/sitemap1.xml"),
            LastModified = DateTime.UtcNow
        });
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://www.example.com/sitemap2.xml"),
            LastModified = DateTime.UtcNow.AddDays(-1)
        });
        using Stream stream = new MemoryStream();
        index.Save(stream);
        ExampleOutput.ShowSaved("SitemapIndex");
    }

    /// <summary>
    /// Provides example code for the SitemapIndex.CreateAsync(Uri) method.
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        // Note: Loading endjin.com/sitemap.xml as a demonstration. In practice,
        // you would use your own sitemap index URL (e.g., https://example.com/sitemap_index.xml)
        SitemapIndex index = await SitemapIndex.CreateAsync(
            new Uri("https://endjin.com/sitemap.xml")).ConfigureAwait(false);
        foreach (SitemapIndexEntry entry in index.Sitemaps)
        {
            Console.WriteLine($"URL: {entry.Location}");
            if (entry.LastModified.HasValue)
            {
                Console.WriteLine($"  Last Modified: {entry.LastModified.Value}");
            }
        }
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method.
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        SitemapIndex index = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapIndex);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        index.Load(reader);

        foreach (SitemapIndexEntry entry in index.Sitemaps)
        {
            Console.WriteLine($"Sitemap: {entry.Location}");
        }
    }

    /// <summary>
    /// Provides example code for saving with XmlWriter.
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        SitemapIndex index = new();
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://www.example.com/sitemap-pages.xml"),
            LastModified = DateTime.UtcNow
        });

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        index.Save(writer);

        ExampleOutput.ShowSaved("SitemapIndex");
    }
}