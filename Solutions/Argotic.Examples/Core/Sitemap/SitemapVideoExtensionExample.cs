using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Contains the code examples for the <see cref="SitemapVideoExtension"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="SitemapVideoExtension"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
public static class SitemapVideoExtensionExample
{
    /// <summary>
    /// Provides example code for the SitemapVideoExtension class.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();

        // Create a URL entry with video extension
        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/videos/awesome-video"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        // Create the video extension
        SitemapVideoExtension videoExtension = new();

        // Create a video with required properties
        SitemapVideo video = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbnails/awesome-video.jpg"),
            title: "Awesome Product Demo Video",
            description: "Watch our amazing product demo showcasing all the features and benefits of our new product line.")
        {
            // Add optional properties
            ContentLocation = new Uri("https://www.example.com/videos/awesome-video.mp4"),
            Duration = 300, // 5 minutes
            PublicationDate = new DateTime(2025, 6, 15),
            Rating = 4.5m,
            ViewCount = 12500,
            FamilyFriendly = true,
            Uploader = "Example Company",
            UploaderInfo = new Uri("https://www.example.com/about-us")
        };

        // Add tags
        video.Tags.Add("product");
        video.Tags.Add("demo");
        video.Tags.Add("tutorial");
        video.Tags.Add("how-to");

        videoExtension.Videos.Add(video);
        url.Extensions.Add(videoExtension);
        sitemap.Urls.Add(url);

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code for loading a sitemap with video extensions from a stream.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapVideo);
        sitemap.Load(stream);

        // Find URLs with video extensions
        foreach (SitemapUrl url in sitemap.Urls)
        {
            SitemapVideoExtension? videoExtension = url.FindExtension(SitemapVideoExtension.MatchByType) as SitemapVideoExtension;
            if (videoExtension != null && videoExtension.Videos.Count > 0)
            {
                Console.WriteLine($"URL: {url.Location}");
                foreach (SitemapVideo video in videoExtension.Videos)
                {
                    Console.WriteLine($"  Video: {video.Title}");
                    Console.WriteLine($"    Thumbnail: {video.ThumbnailLocation}");
                    Console.WriteLine($"    Description: {video.Description[..Math.Min(50, video.Description.Length)]}...");
                    if (video.Duration.HasValue)
                    {
                        Console.WriteLine($"    Duration: {video.Duration.Value} seconds");
                    }
                }
            }
        }

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code for saving a sitemap with video extension to a stream.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/videos/tutorial")
        };

        SitemapVideoExtension videoExtension = new();
        SitemapVideo video = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/tutorial.jpg"),
            title: "Getting Started Tutorial",
            description: "Learn the basics of our platform in this comprehensive tutorial.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/tutorial.mp4"),
            Duration = 600
        };

        videoExtension.Videos.Add(video);
        url.Extensions.Add(videoExtension);
        sitemap.Urls.Add(url);

        using Stream stream = new MemoryStream();
        sitemap.Save(stream);

        ExampleOutput.ShowSaved("Sitemap with Video Extension");
    }

    /// <summary>
    /// Provides example code demonstrating platform restriction (allow/deny).
    /// </summary>
    public static void PlatformRestrictionExample()
    {
        // Example 1: Allow video only on Web and Mobile platforms
        SitemapVideo webMobileVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/web-mobile-video.jpg"),
            title: "Web and Mobile Optimized Video",
            description: "This video is optimized for web browsers and mobile devices.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/web-mobile.mp4"),
            Platform = SitemapVideoPlatform.Web | SitemapVideoPlatform.Mobile,
            PlatformRelationship = SitemapVideoRelationship.Allow
        };

        Console.WriteLine("Video 1: Web and Mobile Only (Allow)");
        Console.WriteLine($"  Title: {webMobileVideo.Title}");
        Console.WriteLine($"  Platform: {webMobileVideo.Platform}");
        Console.WriteLine($"  Relationship: {webMobileVideo.PlatformRelationship}");
        Console.WriteLine();

        // Example 2: Deny video on TV platform
        SitemapVideo noTvVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/no-tv-video.jpg"),
            title: "Not Available on TV",
            description: "This video is not available on TV platforms due to licensing restrictions.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/no-tv.mp4"),
            Platform = SitemapVideoPlatform.Tv,
            PlatformRelationship = SitemapVideoRelationship.Deny
        };

        Console.WriteLine("Video 2: TV Platform Denied");
        Console.WriteLine($"  Title: {noTvVideo.Title}");
        Console.WriteLine($"  Platform: {noTvVideo.Platform}");
        Console.WriteLine($"  Relationship: {noTvVideo.PlatformRelationship}");
    }

    /// <summary>
    /// Provides example code demonstrating country restrictions.
    /// </summary>
    public static void CountryRestrictionExample()
    {
        // Example 1: Allow video only in US, Canada, and Great Britain
        SitemapVideo allowedCountriesVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/english-video.jpg"),
            title: "English-Speaking Markets Video",
            description: "This video is available in English-speaking markets only.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/english-markets.mp4"),
            Restriction = "US CA GB",
            RestrictionRelationship = SitemapVideoRelationship.Allow
        };

        Console.WriteLine("Video 1: Allowed in US, CA, GB");
        Console.WriteLine($"  Title: {allowedCountriesVideo.Title}");
        Console.WriteLine($"  Restriction: {allowedCountriesVideo.Restriction}");
        Console.WriteLine($"  Relationship: {allowedCountriesVideo.RestrictionRelationship}");
        Console.WriteLine();

        // Example 2: Deny video in specific countries
        SitemapVideo deniedCountriesVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/restricted-video.jpg"),
            title: "Geographically Restricted Video",
            description: "This video is not available in certain regions due to licensing.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/restricted.mp4"),
            Restriction = "CN RU KP",
            RestrictionRelationship = SitemapVideoRelationship.Deny
        };

        Console.WriteLine("Video 2: Denied in CN, RU, KP");
        Console.WriteLine($"  Title: {deniedCountriesVideo.Title}");
        Console.WriteLine($"  Restriction: {deniedCountriesVideo.Restriction}");
        Console.WriteLine($"  Relationship: {deniedCountriesVideo.RestrictionRelationship}");
    }

    /// <summary>
    /// Provides example code demonstrating multiple videos per URL.
    /// </summary>
    public static void MultipleVideosExample()
    {
        Syndication.Sitemap sitemap = new();

        // Create a URL entry that has multiple related videos
        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/product/widget"),
            LastModified = DateTime.UtcNow
        };

        SitemapVideoExtension videoExtension = new();

        // Video 1: Product Overview
        SitemapVideo overviewVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/widget-overview.jpg"),
            title: "Widget Product Overview",
            description: "A comprehensive overview of our Widget product and its key features.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/widget-overview.mp4"),
            Duration = 180,
            PublicationDate = new DateTime(2025, 1, 1)
        };
        overviewVideo.Tags.Add("overview");
        overviewVideo.Tags.Add("product");

        // Video 2: Installation Guide
        SitemapVideo installVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/widget-install.jpg"),
            title: "Widget Installation Guide",
            description: "Step-by-step instructions for installing and setting up your Widget.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/widget-install.mp4"),
            Duration = 420,
            PublicationDate = new DateTime(2025, 1, 15)
        };
        installVideo.Tags.Add("installation");
        installVideo.Tags.Add("tutorial");

        // Video 3: Tips and Tricks
        SitemapVideo tipsVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/widget-tips.jpg"),
            title: "Widget Tips and Tricks",
            description: "Expert tips and tricks to get the most out of your Widget.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/widget-tips.mp4"),
            Duration = 540,
            PublicationDate = new DateTime(2025, 2, 1)
        };
        tipsVideo.Tags.Add("tips");
        tipsVideo.Tags.Add("advanced");

        videoExtension.Videos.Add(overviewVideo);
        videoExtension.Videos.Add(installVideo);
        videoExtension.Videos.Add(tipsVideo);

        url.Extensions.Add(videoExtension);
        sitemap.Urls.Add(url);

        Console.WriteLine($"URL: {url.Location}");
        Console.WriteLine($"  Number of videos: {videoExtension.Videos.Count}");
        foreach (SitemapVideo video in videoExtension.Videos)
        {
            Console.WriteLine($"    - {video.Title} ({video.Duration} seconds)");
        }

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code demonstrating a live stream video.
    /// </summary>
    public static void LiveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/live/event-stream"),
            ChangeFrequency = SitemapChangeFrequency.Always
        };

        SitemapVideoExtension videoExtension = new();

        SitemapVideo liveVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/live-event.jpg"),
            title: "Live Event Stream",
            description: "Watch our live event streaming now! Join us for this exciting broadcast.")
        {
            PlayerLocation = new Uri("https://www.example.com/player/live-event"),
            Live = true,
            PublicationDate = DateTime.UtcNow,
            Uploader = "Example Broadcasting",
            UploaderInfo = new Uri("https://www.example.com/broadcasting")
        };
        liveVideo.Tags.Add("live");
        liveVideo.Tags.Add("event");
        liveVideo.Tags.Add("streaming");

        videoExtension.Videos.Add(liveVideo);
        url.Extensions.Add(videoExtension);
        sitemap.Urls.Add(url);

        Console.WriteLine($"Live Video: {liveVideo.Title}");
        Console.WriteLine($"  Is Live: {liveVideo.Live}");
        Console.WriteLine($"  Player: {liveVideo.PlayerLocation}");
        Console.WriteLine($"  Uploader: {liveVideo.Uploader}");

        ExampleOutput.ShowSitemap(sitemap);
    }

    /// <summary>
    /// Provides example code demonstrating a video that requires subscription.
    /// </summary>
    public static void SubscriptionRequiredExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://www.example.com/premium/exclusive-content")
        };

        SitemapVideoExtension videoExtension = new();

        SitemapVideo premiumVideo = new(
            thumbnailLocation: new Uri("https://www.example.com/thumbs/premium-content.jpg"),
            title: "Premium Exclusive Content",
            description: "This premium video content is available exclusively to our subscribers.")
        {
            ContentLocation = new Uri("https://www.example.com/videos/premium-exclusive.mp4"),
            RequiresSubscription = true,
            Duration = 3600, // 1 hour
            Rating = 4.8m,
            ViewCount = 50000,
            PublicationDate = new DateTime(2025, 3, 1),
            FamilyFriendly = true,
            Uploader = "Example Premium",
            UploaderInfo = new Uri("https://www.example.com/premium")
        };
        premiumVideo.Tags.Add("premium");
        premiumVideo.Tags.Add("exclusive");
        premiumVideo.Tags.Add("subscriber-only");

        videoExtension.Videos.Add(premiumVideo);
        url.Extensions.Add(videoExtension);
        sitemap.Urls.Add(url);

        Console.WriteLine($"Premium Video: {premiumVideo.Title}");
        Console.WriteLine($"  Requires Subscription: {premiumVideo.RequiresSubscription}");
        Console.WriteLine($"  Duration: {premiumVideo.Duration} seconds ({premiumVideo.Duration / 60} minutes)");
        Console.WriteLine($"  Rating: {premiumVideo.Rating}");
        Console.WriteLine($"  View Count: {premiumVideo.ViewCount}");

        ExampleOutput.ShowSitemap(sitemap);
    }
}