using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Declares the images on a page with <see cref="SitemapImageExtension"/>, then reads them back out of a saved sitemap.
/// </summary>
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
            Location = new Uri("https://www.example.com/gallery/photo-collection"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        // Create the image extension and add an image
        SitemapImageExtension imageExtension = new();
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/photo1.jpg")));

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
            Location = new Uri("https://www.example.com/products/widget"),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 0.7m
        };

        SitemapImageExtension imageExtension = new();
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/widget-front.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/widget-side.jpg")));
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

        // Create a URL for a gallery page with multiple images
        SitemapUrl galleryUrl = new()
        {
            Location = new Uri("https://www.example.com/gallery/summer-vacation"),
            LastModified = new DateTime(2025, 8, 15),
            ChangeFrequency = SitemapChangeFrequency.Never,
            Priority = 0.6m
        };

        // Create extension with multiple images
        SitemapImageExtension imageExtension = new();
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/summer/beach-sunset.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/summer/mountain-view.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/summer/lake-reflection.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/summer/forest-trail.jpg")));
        imageExtension.Images.Add(new SitemapImage(new Uri("https://www.example.com/images/summer/city-skyline.jpg")));

        galleryUrl.Extensions.Add(imageExtension);
        sitemap.Urls.Add(galleryUrl);

        Console.WriteLine($"Gallery page with {imageExtension.Images.Count} images:");
        ExampleOutput.ShowSitemapImageExtension(imageExtension);
    }

    /// <summary>
    /// Provides example code demonstrating product images for e-commerce.
    /// </summary>
    public static void EcommerceExample()
    {
        Syndication.Sitemap sitemap = new();

        // Product 1: Laptop
        SitemapUrl laptopUrl = new()
        {
            Location = new Uri("https://www.example.com/products/laptop-pro-15"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.9m
        };

        SitemapImageExtension laptopImages = new();
        laptopImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/laptop-pro-15-front.jpg")));
        laptopImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/laptop-pro-15-open.jpg")));
        laptopImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/laptop-pro-15-side.jpg")));
        laptopImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/laptop-pro-15-ports.jpg")));

        laptopUrl.Extensions.Add(laptopImages);
        sitemap.Urls.Add(laptopUrl);

        // Product 2: Headphones
        SitemapUrl headphonesUrl = new()
        {
            Location = new Uri("https://www.example.com/products/wireless-headphones"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        SitemapImageExtension headphonesImages = new();
        headphonesImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/headphones-black.jpg")));
        headphonesImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/headphones-white.jpg")));
        headphonesImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/headphones-case.jpg")));

        headphonesUrl.Extensions.Add(headphonesImages);
        sitemap.Urls.Add(headphonesUrl);

        // Product 3: Smartphone
        SitemapUrl phoneUrl = new()
        {
            Location = new Uri("https://www.example.com/products/smartphone-x"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.9m
        };

        SitemapImageExtension phoneImages = new();
        phoneImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/phone-front.jpg")));
        phoneImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/phone-back.jpg")));
        phoneImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/phone-camera.jpg")));
        phoneImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/phone-display.jpg")));
        phoneImages.Images.Add(new SitemapImage(new Uri("https://www.example.com/products/images/phone-box.jpg")));

        phoneUrl.Extensions.Add(phoneImages);
        sitemap.Urls.Add(phoneUrl);

        Console.WriteLine("E-commerce product sitemap:");
        ExampleOutput.ShowSitemap(sitemap);

        // Show image details for each product
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
    public static void CdnImagesExample()
    {
        Syndication.Sitemap sitemap = new();

        // Page with images served from a CDN subdomain
        SitemapUrl articleUrl = new()
        {
            Location = new Uri("https://www.example.com/blog/photography-tips"),
            LastModified = new DateTime(2025, 12, 1),
            ChangeFrequency = SitemapChangeFrequency.Yearly,
            Priority = 0.7m
        };

        // Images are hosted on a CDN subdomain
        SitemapImageExtension cdnImages = new();
        cdnImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/blog/photography/camera-basics.jpg")));
        cdnImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/blog/photography/lighting-setup.jpg")));
        cdnImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/blog/photography/composition-rules.jpg")));

        articleUrl.Extensions.Add(cdnImages);
        sitemap.Urls.Add(articleUrl);

        // Another page with mixed CDN images
        SitemapUrl tutorialUrl = new()
        {
            Location = new Uri("https://www.example.com/tutorials/photo-editing"),
            LastModified = new DateTime(2025, 11, 15),
            ChangeFrequency = SitemapChangeFrequency.Monthly,
            Priority = 0.6m
        };

        SitemapImageExtension tutorialImages = new();
        tutorialImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/tutorials/editing/before-after.jpg")));
        tutorialImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/tutorials/editing/color-correction.jpg")));
        tutorialImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/tutorials/editing/retouching-demo.jpg")));
        tutorialImages.Images.Add(new SitemapImage(new Uri("https://cdn.example.com/tutorials/editing/final-result.jpg")));

        tutorialUrl.Extensions.Add(tutorialImages);
        sitemap.Urls.Add(tutorialUrl);

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