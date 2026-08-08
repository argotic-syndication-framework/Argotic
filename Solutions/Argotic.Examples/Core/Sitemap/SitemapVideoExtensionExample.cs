using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Examples.Core.Sitemap;

/// <summary>
/// Declares video metadata with <see cref="SitemapVideoExtension"/>, then reads it back out of a saved sitemap.
/// </summary>
internal static class SitemapVideoExtensionExample
{
    /// <summary>
    /// Attaches a <see cref="SitemapVideoExtension"/> to a <see cref="SitemapUrl"/> and prints the sitemap.
    /// </summary>
    public static void ClassExample()
    {
        Syndication.Sitemap sitemap = new();

        // Create a URL entry with video extension
        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/rxdotnet-v7-0-released"),
            LastModified = DateTime.UtcNow,
            ChangeFrequency = SitemapChangeFrequency.Weekly,
            Priority = 0.8m
        };

        // Create the video extension
        SitemapVideoExtension videoExtension = new();

        // Create a video with required properties
        SitemapVideo video = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg"),
            title: "Rx.NET v7.0 Released - it could save you 95MB!",
            description: "Ian Griffiths announces Rx.NET 7.0, and how moving UI framework support into separate packages cuts 95MB from a self-contained deployment.")
        {
            // Add optional properties
            ContentLocation = new Uri("https://www.youtube.com/embed/gJlP1vcrxD8"),
            Duration = 300, // 5 minutes
            PublicationDate = new DateTime(2025, 6, 15),
            Rating = 4.5m,
            ViewCount = 12_500,
            FamilyFriendly = true,
            Uploader = "endjin",
            UploaderInfo = new Uri("https://endjin.com/who-we-are/")
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
    /// Reads the video extension back out of a sitemap loaded from a <see cref="Stream"/>.
    /// </summary>
    public static void LoadStreamExample()
    {
        Syndication.Sitemap sitemap = new();
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.SitemapVideo);
        sitemap.Load(stream);

        // Find URLs with video extensions
        foreach (SitemapUrl url in sitemap.Urls)
        {
            if (url.FindExtension(SitemapVideoExtension.MatchByType) is SitemapVideoExtension videoExtension && videoExtension.Videos.Count > 0)
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
    /// Saves a sitemap carrying the video extension, and shows the namespace it declares.
    /// </summary>
    public static void SaveStreamExample()
    {
        Syndication.Sitemap sitemap = new();

        SitemapUrl url = new()
        {
            Location = new Uri("https://endjin.com/what-we-think/talks/auditing-uk-energy-policy-without-a-cluster")
        };

        SitemapVideoExtension videoExtension = new();
        SitemapVideo video = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg"),
            title: "Auditing UK energy policy without a cluster",
            description: "Barry Smart audits twenty years of fragmented government energy data on a single laptop, using DuckDB where PySpark had failed to scale.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/Ub7Zuf_lLms"),
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
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg"),
            title: "Rx.NET v7.0: web and mobile",
            description: "Served to web browsers and mobile devices, but not to TV platforms.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/gJlP1vcrxD8"),
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
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg"),
            title: "Auditing UK energy policy: not on TV",
            description: "Denied on TV platforms, which is how a relationship of deny reads.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/Ub7Zuf_lLms"),
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
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg"),
            title: "Rx.NET v7.0: English-language markets",
            description: "Allowed in the countries listed, and nowhere else.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/gJlP1vcrxD8"),
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
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg"),
            title: "Auditing UK energy policy: restricted regions",
            description: "Denied in the countries listed, and allowed everywhere else. Allow and deny are not complements, and a reader must not treat them as such.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/Ub7Zuf_lLms"),
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
            Location = new Uri("https://endjin.com/what-we-do/data-and-analytics/data-platforms/duckdb/"),
            LastModified = DateTime.UtcNow
        };

        SitemapVideoExtension videoExtension = new();

        // Video 1: Product Overview
        SitemapVideo overviewVideo = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg"),
            title: "Rx.NET v7.0 Released",
            description: "The Rx.NET 7.0 release, its three breaking changes, and the deployment size they buy back.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/gJlP1vcrxD8"),
            Duration = 180,
            PublicationDate = new DateTime(2025, 1, 1)
        };
        overviewVideo.Tags.Add("overview");
        overviewVideo.Tags.Add("product");

        // Video 2: Installation Guide
        SitemapVideo installVideo = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg"),
            title: "Auditing UK energy policy without a cluster",
            description: "A laptop, a duck, and twenty years of wind: what replaced the monolithic data warehouse.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/Ub7Zuf_lLms"),
            Duration = 420,
            PublicationDate = new DateTime(2025, 1, 15)
        };
        installVideo.Tags.Add("installation");
        installVideo.Tags.Add("tutorial");

        // Video 3: Tips and Tricks
        SitemapVideo tipsVideo = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg"),
            title: "Reactive Extensions for .NET: status and plans for .NET 10",
            description: "Where Rx.NET stands on .NET 10, and the shape of the work ahead.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/gJlP1vcrxD8"),
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
            Location = new Uri("https://endjin.com/what-we-think/talks/"),
            ChangeFrequency = SitemapChangeFrequency.Always
        };

        SitemapVideoExtension videoExtension = new();

        SitemapVideo liveVideo = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/rx-dotnet-v7-0-released.jpg"),
            title: "endjin live: Rx.NET office hours",
            description: "A live stream on the endjin YouTube channel. video:live is yes only while it is actually live, which is the point of the element.")
        {
            PlayerLocation = new Uri("https://www.youtube.com/endjin"),
            Live = true,
            PublicationDate = DateTime.UtcNow,
            Uploader = "Example Broadcasting",
            UploaderInfo = new Uri("https://www.youtube.com/endjin")
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
            Location = new Uri("https://endjin.com/what-we-do/briefings/azure-data-strategy/")
        };

        SitemapVideoExtension videoExtension = new();

        SitemapVideo premiumVideo = new(
            thumbnailLocation: new Uri("https://res.cloudinary.com/endjin/image/upload/f_auto/q_80/assets/images/talks/duckcon-07-2026-auditing-uk-energy-policy-without-a-cluster.jpg"),
            title: "Azure Data Strategy briefing",
            description: "A recorded briefing behind a registration wall, so requires_subscription is yes.")
        {
            ContentLocation = new Uri("https://www.youtube.com/embed/Ub7Zuf_lLms"),
            RequiresSubscription = true,
            Duration = 3600, // 1 hour
            Rating = 4.8m,
            ViewCount = 50_000,
            PublicationDate = new DateTime(2025, 3, 1),
            FamilyFriendly = true,
            Uploader = "endjin",
            UploaderInfo = new Uri("https://endjin.com/what-we-do/briefings/")
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