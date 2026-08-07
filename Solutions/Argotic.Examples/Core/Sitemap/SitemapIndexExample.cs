using System.Xml;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Demonstrates the <see cref="SitemapIndex"/> surface: pointing at child sitemaps, then the <c>Load</c>, <c>CreateAsync</c> and <c>Save</c> overloads.
/// </summary>
/// <remarks>
///     An index exists because the protocol caps a single sitemap at 50,000 URLs; the index is how a
///     larger site is split across several files and still announced as one.
/// </remarks>
internal static class SitemapIndexExample
{
    /// <summary>
    /// Builds a <see cref="SitemapIndex"/> pointing at three child sitemaps and prints it.
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
    /// Loads a <see cref="SitemapIndex"/> from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        SitemapIndex index = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapIndex);
        index.Load(stream);
        ExampleOutput.ShowSitemapIndex(index);
    }

    /// <summary>
    /// Saves a <see cref="SitemapIndex"/> to a <see cref="Stream"/>.
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
    /// Creates a <see cref="SitemapIndex"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    [RequiresNetwork]
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
    /// Loads a <see cref="SitemapIndex"/> from an <see cref="XmlReader"/>.
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
    /// Saves a <see cref="SitemapIndex"/> through an <see cref="XmlWriter"/>.
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