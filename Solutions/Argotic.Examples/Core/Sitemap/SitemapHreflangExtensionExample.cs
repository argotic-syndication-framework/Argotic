using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Declares alternate-language versions of a page with <see cref="SitemapHreflangExtension"/>, then reads them back.
/// </summary>
/// <remarks>
///     The fourth of the Google sitemap extensions, and the odd one out: it has no namespace of its
///     own. The annotations are <c>xhtml:link</c> elements in the XHTML namespace, which is why the
///     extension binds the <c>xhtml</c> prefix rather than something sitemap-shaped.
/// </remarks>
internal static class SitemapHreflangExtensionExample
{
    /// <summary>
    /// Attaches a <see cref="SitemapHreflangExtension"/> to a <see cref="SitemapUrl"/> and prints the sitemap.
    /// </summary>
    /// <remarks>
    ///     The annotation is reciprocal: Google requires every url in a set to list every alternate,
    ///     including itself. A page that lists only the others is not a partially correct annotation,
    ///     it is an ignored one.
    /// </remarks>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            LastModified = new DateTime(2026, 7, 29),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
        };

        SitemapHreflangExtension hreflang = new();
        hreflang.Links.Add(new SitemapHreflangLink("x-default", new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released")));
        hreflang.Links.Add(new SitemapHreflangLink("en-gb", new Uri("https://endjin.com/en/what-we-think/talks/rxdotnet-v7-0-released")));
        hreflang.Links.Add(new SitemapHreflangLink("de", new Uri("https://endjin.com/de/what-we-think/talks/rxdotnet-v7-0-released")));

        url.Extensions.Add(hreflang);
        sitemap.Urls.Add(url);

        ExampleOutput.ShowSitemap(sitemap);
        ExampleOutput.ShowSitemapHreflangExtension(hreflang);
    }

    /// <summary>
    /// Reads the hreflang extension back out of a sitemap loaded from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapHreflang);
        sitemap.Load(stream);

        foreach (SitemapUrl url in sitemap.Urls)
        {
            if (url.FindExtension(SitemapHreflangExtension.MatchByType) is SitemapHreflangExtension hreflang)
            {
                Console.WriteLine($"URL: {url.Location}");
                ExampleOutput.ShowSitemapHreflangExtension(hreflang);
            }
        }
    }

    /// <summary>
    /// Saves a sitemap carrying the hreflang extension and reads it back, showing that <c>x-default</c> survives.
    /// </summary>
    /// <remarks>
    ///     <c>x-default</c> is a legal <c>hreflang</c> value and is not a language tag. A reader that
    ///     parses every value as a culture fails on a document Google considers correct, so the round
    ///     trip is worth demonstrating rather than assuming.
    /// </remarks>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
        };

        SitemapHreflangExtension hreflang = new();
        hreflang.Links.Add(new SitemapHreflangLink("x-default", new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine")));
        hreflang.Links.Add(new SitemapHreflangLink("en-gb", new Uri("https://endjin.com/en/blog/optimising-dax-formula-engine-and-storage-engine")));

        url.Extensions.Add(hreflang);
        sitemap.Urls.Add(url);

        using MemoryStream stream = new();
        sitemap.Save(stream);
        ExampleOutput.ShowSaved("Sitemap with hreflang");

        stream.Seek(0, SeekOrigin.Begin);
        Syndication.Sitemap reloaded = new();
        reloaded.Load(stream);

        foreach (SitemapUrl readUrl in reloaded.Urls)
        {
            if (readUrl.FindExtension(SitemapHreflangExtension.MatchByType) is SitemapHreflangExtension readBack)
            {
                ExampleOutput.ShowSitemapHreflangExtension(readBack);
            }
        }
    }
}