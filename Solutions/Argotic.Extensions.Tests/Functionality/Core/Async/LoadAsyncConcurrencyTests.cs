namespace Argotic.Extensions.Tests.Functionality.Core.Async;

/// <summary>
/// Covers loads that overlap, and asserts that no resource ends up holding another's state.
/// </summary>
/// <remarks>
///     The class guards a fixed defect in which load state was held in a static field. Each test
///     therefore asserts a per-instance outcome — every resource's own <c>Loaded</c> event and its
///     own parsed title — rather than merely that the calls completed.
/// </remarks>
[TestClass]
public class LoadAsyncConcurrencyTests
{
    /// <summary>
    /// Five RSS feeds loaded in turn from separate streams each raise their own <c>Loaded</c> event and each keep their own parsed title.
    /// </summary>
    [TestMethod]
    public void RssFeed_MultipleConcurrentLoads_DoNotInterfere()
    {
        // Arrange - Create multiple feeds
        RssFeed[] feeds = new RssFeed[5];
        bool[] results = new bool[5];

        for (int i = 0; i < 5; i++)
        {
            feeds[i] = new RssFeed();
            int index = i;
            feeds[i].Loaded += (_, _) => results[index] = true;
        }

        // Act - Load all feeds from streams
        for (int i = 0; i < 5; i++)
        {
            using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));
            feeds[i].Load(stream);
        }

        // Assert - All feeds should have loaded successfully
        for (int i = 0; i < 5; i++)
        {
            results[i].ShouldBeTrue($"Feed {i} should have loaded successfully");
            feeds[i].Channel.Title.ShouldBe("Test Feed", $"Feed {i} should have correct title");
        }
    }

    /// <summary>
    /// Five Atom feeds loaded in turn from separate streams each raise their own <c>Loaded</c> event and each keep their own parsed title.
    /// </summary>
    [TestMethod]
    public void AtomFeed_MultipleConcurrentLoads_DoNotInterfere()
    {
        // Arrange - Create multiple feeds
        AtomFeed[] feeds = new AtomFeed[5];
        bool[] results = new bool[5];

        for (int i = 0; i < 5; i++)
        {
            feeds[i] = new AtomFeed();
            int index = i;
            feeds[i].Loaded += (_, _) => results[index] = true;
        }

        // Act - Load all feeds from streams
        for (int i = 0; i < 5; i++)
        {
            using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));
            feeds[i].Load(stream);
        }

        // Assert - All feeds should have loaded successfully
        for (int i = 0; i < 5; i++)
        {
            results[i].ShouldBeTrue($"Feed {i} should have loaded successfully");
            feeds[i].Title!.Content.ShouldBe("Test Feed", $"Feed {i} should have correct title");
        }
    }

    /// <summary>
    /// An RSS feed, an Atom feed, an OPML document and a generic feed loaded in turn each raise their own <c>Loaded</c> event and keep their own parsed title.
    /// </summary>
    [TestMethod]
    public void MixedFeeds_MultipleConcurrentLoads_DoNotInterfere()
    {
        // Arrange - Create different types of feeds
        RssFeed rssFeed = new();
        AtomFeed atomFeed = new();
        OpmlDocument opmlDoc = new();
        Syndication.GenericSyndicationFeed genericFeed = new();

        bool rssLoaded = false, atomLoaded = false, opmlLoaded = false, genericLoaded = false;

        rssFeed.Loaded += (_, _) => rssLoaded = true;
        atomFeed.Loaded += (_, _) => atomLoaded = true;
        opmlDoc.Loaded += (_, _) => opmlLoaded = true;
        genericFeed.Loaded += (_, _) => genericLoaded = true;

        // Act - Load all feeds
        using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss)))
        {
            rssFeed.Load(stream);
        }

        using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom)))
        {
            atomFeed.Load(stream);
        }

        using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml)))
        {
            opmlDoc.Load(stream);
        }

        using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(FeedTestData.MinimalRss)))
        {
            genericFeed.Load(stream);
        }

        // Assert - All should have loaded successfully
        rssLoaded.ShouldBeTrue("RSS feed should have loaded");
        atomLoaded.ShouldBeTrue("Atom feed should have loaded");
        opmlLoaded.ShouldBeTrue("OPML document should have loaded");
        genericLoaded.ShouldBeTrue("Generic feed should have loaded");

        rssFeed.Channel.Title.ShouldBe("Test Feed");
        atomFeed.Title!.Content.ShouldBe("Test Feed");
        opmlDoc.Head.Title.ShouldBe("Test OPML");
        genericFeed.Title.ShouldBe("Test Feed");
    }

    /// <summary>
    /// Five RSS feeds loaded at once through a single <c>HttpClient</c> under <c>Task.WhenAll</c> each raise their own <c>Loaded</c> event and each keep their own parsed title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task RssFeed_MultipleConcurrentLoadAsync_DoNotInterfere()
    {
        // Arrange - Create multiple feeds
        RssFeed[] feeds = new RssFeed[5];
        bool[] results = new bool[5];

        for (int i = 0; i < 5; i++)
        {
            feeds[i] = new RssFeed();
            int index = i;
            feeds[i].Loaded += (_, _) => results[index] = true;
        }

        // Act - Load all feeds concurrently using Task.WhenAll
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient httpClient = new(handler);

        Task[] loadTasks = new Task[5];
        for (int i = 0; i < 5; i++)
        {
            loadTasks[i] = feeds[i].LoadAsync(new Uri($"http://example.com/feed{i}.xml"), httpClient, cancellationToken: TestContext.CancellationToken);
        }

        await Task.WhenAll(loadTasks);

        // Assert - All feeds should have loaded successfully
        for (int i = 0; i < 5; i++)
        {
            results[i].ShouldBeTrue($"Feed {i} should have loaded successfully");
            feeds[i].Channel.Title.ShouldBe("Test Feed", $"Feed {i} should have correct title");
        }
    }

    /// <summary>
    /// Five Atom feeds loaded at once through a single <c>HttpClient</c> under <c>Task.WhenAll</c> each raise their own <c>Loaded</c> event and each keep their own parsed title.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AtomFeed_MultipleConcurrentLoadAsync_DoNotInterfere()
    {
        // Arrange - Create multiple feeds
        AtomFeed[] feeds = new AtomFeed[5];
        bool[] results = new bool[5];

        for (int i = 0; i < 5; i++)
        {
            feeds[i] = new AtomFeed();
            int index = i;
            feeds[i].Loaded += (_, _) => results[index] = true;
        }

        // Act - Load all feeds concurrently using Task.WhenAll
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient httpClient = new(handler);

        Task[] loadTasks = new Task[5];
        for (int i = 0; i < 5; i++)
        {
            loadTasks[i] = feeds[i].LoadAsync(new Uri($"http://example.com/feed{i}.xml"), httpClient, cancellationToken: TestContext.CancellationToken);
        }

        await Task.WhenAll(loadTasks);

        // Assert - All feeds should have loaded successfully
        for (int i = 0; i < 5; i++)
        {
            results[i].ShouldBeTrue($"Feed {i} should have loaded successfully");
            feeds[i].Title!.Content.ShouldBe("Test Feed", $"Feed {i} should have correct title");
        }
    }

    /// <summary>
    /// Four resources of different formats loaded at once, each through its own handler, all finish holding their own content.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task MixedFeeds_ConcurrentLoadAsync_AllLoadSuccessfully()
    {
        // Arrange - Create different types of feeds
        RssFeed rssFeed = new();
        AtomFeed atomFeed = new();
        OpmlDocument opmlDoc = new();
        Syndication.GenericSyndicationFeed genericFeed = new();

        bool rssLoaded = false, atomLoaded = false, opmlLoaded = false, genericLoaded = false;

        rssFeed.Loaded += (_, _) => rssLoaded = true;
        atomFeed.Loaded += (_, _) => atomLoaded = true;
        opmlDoc.Loaded += (_, _) => opmlLoaded = true;
        genericFeed.Loaded += (_, _) => genericLoaded = true;

        // Act - Load all feeds concurrently
        using MockHttpMessageHandler rssHandler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient rssClient = new(rssHandler);

        using MockHttpMessageHandler atomHandler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalAtom);
        using HttpClient atomClient = new(atomHandler);

        using MockHttpMessageHandler opmlHandler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalOpml);
        using HttpClient opmlClient = new(opmlHandler);

        using MockHttpMessageHandler genericHandler = MockHttpMessageHandler.WithContent(FeedTestData.MinimalRss);
        using HttpClient genericClient = new(genericHandler);

        await Task.WhenAll(
            rssFeed.LoadAsync(new Uri("http://example.com/rss.xml"), rssClient, cancellationToken: TestContext.CancellationToken),
            atomFeed.LoadAsync(new Uri("http://example.com/atom.xml"), atomClient, cancellationToken: TestContext.CancellationToken),
            opmlDoc.LoadAsync(new Uri("http://example.com/opml.xml"), opmlClient, cancellationToken: TestContext.CancellationToken),
            genericFeed.LoadAsync(new Uri("http://example.com/generic.xml"), genericClient, cancellationToken: TestContext.CancellationToken));

        // Assert - All should have loaded successfully
        rssLoaded.ShouldBeTrue("RSS feed should have loaded");
        atomLoaded.ShouldBeTrue("Atom feed should have loaded");
        opmlLoaded.ShouldBeTrue("OPML document should have loaded");
        genericLoaded.ShouldBeTrue("Generic feed should have loaded");

        rssFeed.Channel.Title.ShouldBe("Test Feed");
        atomFeed.Title!.Content.ShouldBe("Test Feed");
        opmlDoc.Head.Title.ShouldBe("Test OPML");
        genericFeed.Title.ShouldBe("Test Feed");
    }

    public TestContext TestContext { get; set; }
}