using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Declares the images on a page with <see cref="SitemapImageExtension"/>, then reads them back out of a saved sitemap.
/// </summary>
/// <remarks>
///     The URLs are endjin's own: pages on endjin.com and the Cloudinary-hosted images they carry.
///     endjin's real <c>sitemap-news.xml</c> declares the image namespace alongside the news one, so a
///     page carrying both extensions is the ordinary case rather than a contrived one.
/// </remarks>
internal static class SitemapImageExtensionExample
{
    /// <summary>
    /// Attaches a <see cref="SitemapImageExtension"/> to a <see cref="SitemapUrl"/> and prints the sitemap.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();

        // Create a URL with an image extension
        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/blog/writing-effective-copilot-instructions-for-complex-codebases"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        // Create the image extension and add an image
        SitemapImageExtension imageExtension = new();
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/06/writing-effective-copilot-instructions-for-complex-codebases.png")));

        // Attach the extension to the URL
        url.Extensions.Add(imageExtension);

        sitemap.Urls.Add(url);

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Reads the image extension back out of a sitemap loaded from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapImage);
        sitemap.Load(stream);

        // Find image extensions on URLs
        foreach (SitemapUrl url in sitemap.Urls)
        {
            ISyndicationExtension? extension = url.FindExtension(SitemapImageExtension.MatchByType);
            if (extension is SitemapImageExtension imageExtension && imageExtension.Images.Count > 0)
            {
                Console.WriteLine($"URL: {url.Location}");
                ExampleOutput.ShowSitemapImageExtension(imageExtension);
            }
        }
    }

    /// <summary>
    /// Saves a sitemap carrying the image extension, and shows the namespace it declares.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        // Create a URL with images
        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 0.7m
        };

        SitemapImageExtension imageExtension = new();
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_40/assets/images/talks/rx-dotnet-v7-0-released.jpg")));
        url.Extensions.Add(imageExtension);

        sitemap.Urls.Add(url);

        using Stream stream = new MemoryStream();
        sitemap.Save(stream);

        ExampleOutput.ShowSaved("Sitemap with images");
    }

    /// <summary>
    /// Provides example code demonstrating multiple images per page.
    /// </summary>
    public static void MultipleImagesExample()
    {
        Syndication.Sitemap sitemap = new();

        // An index page listing several talks, each contributing its own title card
        SitemapUrl talksIndexUrl = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/"),
            LastModified = new DateTime(2026, 7, 29),
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.6m
        };

        // Create extension with multiple images
        SitemapImageExtension imageExtension = new();
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/06/writing-effective-copilot-instructions-for-complex-codebases.png")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/05/optimising-dax-formula-engine-and-storage-engine.png")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png")));

        talksIndexUrl.Extensions.Add(imageExtension);
        sitemap.Urls.Add(talksIndexUrl);

        Console.WriteLine($"Talks index page with {imageExtension.Images.Count} images:");
        ExampleOutput.ShowSitemapImageExtension(imageExtension);
    }

    /// <summary>
    /// Provides example code demonstrating several pages, each declaring its own set of images.
    /// </summary>
    public static void PerPageImagesExample()
    {
        Syndication.Sitemap sitemap = new();

        // Page 1: the Rx.NET talk
        SitemapUrl rxUrl = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.9m
        };

        SitemapImageExtension rxImages = new();
        rxImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg")));
        rxImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_40/assets/images/talks/rx-dotnet-v7-0-released.jpg")));

        rxUrl.Extensions.Add(rxImages);
        sitemap.Urls.Add(rxUrl);

        // Page 2: the DuckDB talk
        SitemapUrl duckUrl = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/auditing-uk-energy-policy-without-a-cluster"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        SitemapImageExtension duckImages = new();
        duckImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg")));

        duckUrl.Extensions.Add(duckImages);
        sitemap.Urls.Add(duckUrl);

        // Page 3: the DAX post
        SitemapUrl daxUrl = new()
        {
            Location = new Uri("https://endjin.com/blog/optimising-dax-formula-engine-and-storage-engine"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.9m
        };

        SitemapImageExtension daxImages = new();
        daxImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/05/optimising-dax-formula-engine-and-storage-engine.png")));
        daxImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/05/optimising-dax-why-cardinality-matters.png")));

        daxUrl.Extensions.Add(daxImages);
        sitemap.Urls.Add(daxUrl);

        Console.WriteLine("Per-page image sitemap:");
        ExampleOutput.ShowSitemap(sitemap);

        // Show image details for each page
        foreach (SitemapUrl url in sitemap.Urls)
        {
            ISyndicationExtension? extension = url.FindExtension(SitemapImageExtension.MatchByType);
            if (extension is SitemapImageExtension imageExtension)
            {
                Console.WriteLine($"\n{url.Location}:");
                ExampleOutput.ShowSitemapImageExtension(imageExtension);
            }
        }
    }

    /// <summary>
    /// Provides example code demonstrating CDN-hosted images.
    /// </summary>
    /// <remarks>
    ///     Not a hypothetical: endjin serves its images from Cloudinary and its audio from Azure Blob
    ///     Storage, both on hosts other than endjin.com. An image sitemap is allowed to point off-site,
    ///     and a reader that assumed same-origin would drop every image on this page.
    /// </remarks>
    public static void CdnImagesExample()
    {
        Syndication.Sitemap sitemap = new();

        // Page with images served from the Cloudinary CDN rather than endjin.com
        SitemapUrl articleUrl = new()
        {
            Location = new Uri("https://endjin.com/blog/genai-reality-check-new-instrument-same-orchestra"),
            LastModified = new DateTime(2026, 5, 14),
            ChangeFrequency = SitemapChangeFrequency.Yearly,
            Priority = 0.7m
        };

        SitemapImageExtension cdnImages = new();
        cdnImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/01/the-genai-reality-check-new-instrument-same-orchestra.png")));
        cdnImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/01/the-genai-reality-check-new-instrument-same-orchestra.png")));
        cdnImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/open-graph/og-endjin.png")));

        articleUrl.Extensions.Add(cdnImages);
        sitemap.Urls.Add(articleUrl);

        // Another page, images from the same CDN
        SitemapUrl seriesUrl = new()
        {
            Location = new Uri("https://endjin.com/blog/asyncapi-code-generation-with-corvus-custom-transports"),
            LastModified = new DateTime(2026, 8, 3),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 0.6m
        };

        SitemapImageExtension seriesImages = new();
        seriesImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/06/asyncapi-code-generation-with-corvus-part-08.png")));
        seriesImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/05/optimising-dax-formula-engine-and-storage-engine.png")));
        seriesImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/05/optimising-dax-why-cardinality-matters.png")));
        seriesImages.Images.Add(new SitemapImage(new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/blog/2026/06/asyncapi-code-generation-with-corvus-part-08.png")));

        seriesUrl.Extensions.Add(seriesImages);
        sitemap.Urls.Add(seriesUrl);

        Console.WriteLine("Sitemap with CDN-hosted images:");
        ExampleOutput.ShowSitemap(sitemap);

        foreach (SitemapUrl url in sitemap.Urls)
        {
            ISyndicationExtension? extension = url.FindExtension(SitemapImageExtension.MatchByType);
            if (extension is SitemapImageExtension imageExtension)
            {
                Console.WriteLine($"\nImages for {url.Location}:");
                foreach (SitemapImage image in imageExtension.Images)
                {
                    Console.WriteLine($"  CDN URL: {image.Location}");
                }
            }
        }
    }
}