using System.Globalization;
namespace Argotic.Extensions.Tests.Functionality.Core.Rss;

/// <summary>
/// Covers building an <see cref="RssFeed"/> through the object model rather than by parsing one —
/// channel metadata, cloud, image, items, enclosures and the skip schedules — and writing it out.
/// </summary>
[TestClass]
public class RssFeedConstructionTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The title, link and description assigned to a channel are read back exactly as assigned.
    /// </summary>
    [TestMethod]
    public void Construction_SetsBasicProperties_Correctly()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "Test Feed",
                Link = new Uri("http://example.com"),
                Description = "Test description"
            }
        };

        feed.Channel.Title.ShouldBe("Test Feed");
        feed.Channel.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Description.ShouldBe("Test description");
    }

    /// <summary>
    /// Channel categories are held in the order they were added, each keeping the optional domain it was given.
    /// </summary>
    [TestMethod]
    public void Construction_WithCategories_AddsCorrectly()
    {
        RssFeed feed = new();
        feed.Channel.Categories.Add(new RssCategory("Media"));
        feed.Channel.Categories.Add(new RssCategory("News/Newspapers", "dmoz"));

        feed.Channel.Categories.Count.ShouldBe(2);
        feed.Channel.Categories[0].Value.ShouldBe("Media");
        feed.Channel.Categories[1].Value.ShouldBe("News/Newspapers");
        feed.Channel.Categories[1].Domain.ShouldBe("dmoz");
    }

    /// <summary>
    /// A cloud keeps the domain, path, port, protocol and registration procedure it was constructed from.
    /// </summary>
    [TestMethod]
    public void Construction_WithCloud_SetsCorrectly()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Cloud = new RssCloud("server.example.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify")
            }
        };

        feed.Channel.Cloud.ShouldNotBeNull();
        feed.Channel.Cloud.Domain.ShouldBe("server.example.com");
        feed.Channel.Cloud.Path.ShouldBe("/rpc");
        feed.Channel.Cloud.Port.ShouldBe(80);
        feed.Channel.Cloud.Protocol.ShouldBe(RssCloudProtocol.XmlRpc);
        feed.Channel.Cloud.RegisterProcedure.ShouldBe("cloud.notify");
    }

    /// <summary>
    /// A channel image keeps its link, title and URL, and the description, height and width set afterwards.
    /// </summary>
    [TestMethod]
    public void Construction_WithImage_SetsCorrectly()
    {
        RssFeed feed = new();
        RssImage image = new(
            new Uri("http://example.com"),
            "Test Image",
            new Uri("http://example.com/image.gif"))
        {
            Description = "Test image description",
            Height = 32,
            Width = 96
        };
        feed.Channel.Image = image;

        feed.Channel.Image.ShouldNotBeNull();
        feed.Channel.Image.Link.ShouldBe(new Uri("http://example.com"));
        feed.Channel.Image.Title.ShouldBe("Test Image");
        feed.Channel.Image.Url.ShouldBe(new Uri("http://example.com/image.gif"));
        feed.Channel.Image.Description.ShouldBe("Test image description");
        feed.Channel.Image.Height.ShouldBe(32);
        feed.Channel.Image.Width.ShouldBe(96);
    }

    /// <summary>
    /// An item added to a channel keeps its title, link, description, author, categories and GUID.
    /// </summary>
    [TestMethod]
    public void Construction_WithItem_SetsCorrectly()
    {
        RssFeed feed = new();
        RssItem item = new()
        {
            Title = "Test Item",
            Link = new Uri("http://example.com/item"),
            Description = "Test item description",
            Author = "test@example.com (Test Author)"
        };

        item.Categories.Add(new RssCategory("category1"));
        item.Comments = new Uri("http://example.com/comments");
        item.Guid = new RssGuid("http://example.com/item");
        item.PublicationDate = new DateTime(2024, 1, 1, 12, 0, 0);

        feed.Channel.Items.Add(item);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem addedItem = feed.Channel.Items.First();
        addedItem.Title.ShouldBe("Test Item");
        addedItem.Link.ShouldBe(new Uri("http://example.com/item"));
        addedItem.Description.ShouldBe("Test item description");
        addedItem.Author.ShouldBe("test@example.com (Test Author)");
        addedItem.Categories.Count.ShouldBe(1);
        addedItem.Guid!.Value.ShouldBe("http://example.com/item");
    }

    /// <summary>
    /// An enclosure keeps the length, content type and URL it was constructed from.
    /// </summary>
    [TestMethod]
    public void Construction_WithEnclosure_SetsCorrectly()
    {
        RssFeed feed = new();
        RssItem item = new()
        {
            Title = "Test Item With Enclosure"
        };

        item.Enclosures.Add(new RssEnclosure(12_345L, "audio/mpeg", new Uri("http://example.com/audio.mp3")));
        feed.Channel.Items.Add(item);

        RssEnclosure enclosure = feed.Channel.Items.First().Enclosures.First();
        enclosure.Length.ShouldBe(12_345L);
        enclosure.ContentType.ShouldBe("audio/mpeg");
        enclosure.Url.ShouldBe(new Uri("http://example.com/audio.mp3"));
    }

    /// <summary>
    /// The channel holds every day added to its skip-days collection.
    /// </summary>
    [TestMethod]
    public void Construction_WithSkipDays_SetsCorrectly()
    {
        RssFeed feed = new();
        feed.Channel.SkipDays.Add(DayOfWeek.Saturday);
        feed.Channel.SkipDays.Add(DayOfWeek.Sunday);

        feed.Channel.SkipDays.Count.ShouldBe(2);
        feed.Channel.SkipDays.ShouldContain(DayOfWeek.Saturday);
        feed.Channel.SkipDays.ShouldContain(DayOfWeek.Sunday);
    }

    /// <summary>
    /// The channel holds every hour added to its skip-hours collection, including <c>0</c> and <c>23</c>.
    /// </summary>
    [TestMethod]
    public void Construction_WithSkipHours_SetsCorrectly()
    {
        RssFeed feed = new();
        feed.Channel.SkipHours.Add(0);
        feed.Channel.SkipHours.Add(1);
        feed.Channel.SkipHours.Add(23);

        feed.Channel.SkipHours.Count.ShouldBe(3);
        feed.Channel.SkipHours.ShouldContain(0);
        feed.Channel.SkipHours.ShouldContain(1);
        feed.Channel.SkipHours.ShouldContain(23);
    }

    /// <summary>
    /// A fully populated feed saved to a stream and loaded back keeps its channel title, description and item count.
    /// </summary>
    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a feed
        RssFeed originalFeed = CreateCompleteFeed();

        // Save to stream
        using MemoryStream stream = new();
        originalFeed.Save(stream);

        // Load from stream
        stream.Position = 0;
        RssFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Verify data preserved
        loadedFeed.Channel.Title.ShouldBe(originalFeed.Channel.Title);
        loadedFeed.Channel.Description.ShouldBe(originalFeed.Channel.Description);
        loadedFeed.Channel.Items.Count.ShouldBe(originalFeed.Channel.Items.Count);
    }

    /// <summary>
    /// Saving writes an XML declaration and the <c>rss</c>, <c>channel</c>, <c>title</c> and <c>item</c> elements.
    /// </summary>
    [TestMethod]
    public void Save_ProducesValidXml()
    {
        RssFeed feed = CreateCompleteFeed();

        using MemoryStream stream = new();
        feed.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<rss");
        xml.ShouldContain("<channel>");
        xml.ShouldContain("<title>Test Feed</title>");
        xml.ShouldContain("<item>");
    }

    private static RssFeed CreateCompleteFeed()
    {
        RssFeed feed = new()
        {
            Channel =
            {
                Title = "Test Feed",
                Link = new Uri("http://example.com"),
                Description = "Test feed description",
                Copyright = "Copyright 2024",
                Generator = "Test Generator v1.0",
                Language = new CultureInfo("en-US"),
                LastBuildDate = new DateTime(2024, 1, 15, 12, 0, 0),
                ManagingEditor = "editor@example.com",
                PublicationDate = new DateTime(2024, 1, 1),
                TimeToLive = 60,
                Webmaster = "webmaster@example.com"
            }
        };

        RssItem item = new()
        {
            Title = "Test Item",
            Link = new Uri("http://example.com/item"),
            Description = "Test item description",
            PublicationDate = new DateTime(2024, 1, 10)
        };
        feed.Channel.Items.Add(item);

        return feed;
    }
}