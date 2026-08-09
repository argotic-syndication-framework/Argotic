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
            Location = new Uri("https://endjin.com/sitemap.xml"),
            LastModified = DateTime.UtcNow
        });
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://endjin.com/sitemap-news.xml"),
            LastModified = new DateTime(2024, 1, 14)
        });
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://endjin.com/sitemap-video.xml"),
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
            Location = new Uri("https://endjin.com/sitemap.xml"),
            LastModified = DateTime.UtcNow
        });
        index.Sitemaps.Add(new SitemapIndexEntry
        {
            Location = new Uri("https://endjin.com/sitemap-news.xml"),
            LastModified = DateTime.UtcNow.AddDays(-1)
        });
        using Stream stream = new MemoryStream();
        index.Save(stream);
        ExampleOutput.ShowSaved("SitemapIndex");
    }

    /// <summary>
    /// Creates a <see cref="SitemapIndex"/> from a <see cref="Uri"/> in a single call.
    /// </summary>
    /// <remarks>
    ///     The index is served from loopback rather than fetched from a live origin, and the reason is
    ///     worth stating: this example used to fetch <c>endjin.com/sitemap.xml</c>, which is a
    ///     <c>urlset</c>, not a <c>sitemapindex</c>. endjin's <c>robots.txt</c> announces three sitemaps
    ///     and all three are <c>urlset</c> documents, so there was no URL that would have made the call
    ///     correct. It passed for as long as <see cref="SitemapIndex"/> accepted anything and returned an
    ///     empty index; once the load began checking the declared format against the document, it threw.
    ///     Serving the right shape locally is what makes the example demonstrate what it claims — and
    ///     because loopback is not the network, it now runs in the offline gate that would have caught
    ///     the mismatch on the day it appeared. The three locations are endjin's real sitemaps.
    /// </remarks>
    public static async Task LoadUriExampleAsync()
    {
        using LoopbackHost host = LoopbackHost.ServingXml(
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <sitemap>
                <loc>https://endjin.com/sitemap.xml</loc>
                <lastmod>2026-08-07</lastmod>
              </sitemap>
              <sitemap>
                <loc>https://endjin.com/sitemap-news.xml</loc>
                <lastmod>2026-08-07</lastmod>
              </sitemap>
              <sitemap>
                <loc>https://endjin.com/sitemap-video.xml</loc>
                <lastmod>2026-07-29</lastmod>
              </sitemap>
            </sitemapindex>
            """);

        SitemapIndex index = await SitemapIndex.CreateAsync(host.Uri).ConfigureAwait(false);
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
            Location = new Uri("https://endjin.com/sitemap.xml"),
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