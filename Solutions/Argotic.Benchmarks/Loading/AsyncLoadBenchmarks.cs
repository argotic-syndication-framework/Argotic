using System.Diagnostics.CodeAnalysis;
using System.Net;

using Argotic.Publishing;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices the per-format async surface: <c>CreateAsync</c> — and through it <c>LoadAsync</c>,
/// which every <c>CreateAsync</c> delegates to — for the eleven resource types whose async state
/// machines had never executed under a benchmark.
/// </summary>
/// <remarks>
/// <para>
/// The §2.29 HTTP modernisation gave every resource type a four-member async surface, and §19
/// found every one of those state machines at zero benchmark coverage except the two the polling
/// class exercises (<c>RssFeed</c> and <c>GenericSyndicationFeed</c>, deliberately absent here
/// for that reason). Arm a is the sync anchor: the a→b delta on the same document is the
/// async-over-sync overhead — state machine, response plumbing, content buffering — with no
/// network in it.
/// </para>
/// <para>
/// Transport is a stub <see cref="HttpMessageHandler"/>, not a loopback socket, on the decision
/// <c>PollingSteadyStateBenchmarks</c> records: "A real socket adds scheduler and kernel noise to
/// a measurement whose subject is the parse, and BenchmarkDotNet cannot separate the two." The
/// stub's per-request <see cref="ByteArrayContent"/> allocation is part of the measured
/// composite, as in that precedent. Bodies are the committed samples, so every arm parses a real
/// document.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "async", "gapfill")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class AsyncLoadBenchmarks : IDisposable
{
    private static readonly Uri Source = new("http://stub.invalid/document.xml");

    private SampleServingHandler handler = new();
    private HttpClient client = new(new SampleServingHandler());
    private byte[] atomFeedBytes = [];
    private byte[] atomEntryBytes = [];
    private byte[] opmlBytes = [];
    private byte[] apmlBytes = [];
    private byte[] blogmlBytes = [];
    private byte[] rsdBytes = [];
    private byte[] sitemapBytes = [];
    private byte[] sitemapIndexBytes = [];
    private byte[] serviceBytes = [];
    private byte[] categoryBytes = [];
    private bool disposed;

    /// <summary>
    /// Reads every sample body once, builds the stub client, and sync-parses each body so a
    /// malformed or misrouted sample fails here rather than measuring an exception.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.handler = new SampleServingHandler();
        this.client = new HttpClient(this.handler, disposeHandler: false);
        this.atomFeedBytes = FeedCorpus.ReadRealSample("AtomFeed.xml");
        this.atomEntryBytes = FeedCorpus.ReadRealSample("AtomEntryDocument.xml");
        this.opmlBytes = FeedCorpus.ReadRealSample("OpmlDocument.xml");
        this.apmlBytes = FeedCorpus.ReadRealSample("ApmlDocument.xml");
        this.blogmlBytes = FeedCorpus.ReadRealSample("BlogMLDocument.xml");
        this.rsdBytes = FeedCorpus.ReadRealSample("RsdDocument.xml");
        this.sitemapBytes = FeedCorpus.ReadRealSample("sitemap.xml");
        this.sitemapIndexBytes = FeedCorpus.ReadRealSample("sitemap_index.xml");
        this.serviceBytes = FeedCorpus.ReadRealSample("AtomServiceDocument.xml");
        this.categoryBytes = FeedCorpus.ReadRealSample("AtomCategoryDocument.xml");

        AtomFeed sanity = new();
        using (MemoryStream stream = new(this.atomFeedBytes, writable: false))
        {
            sanity.Load(stream);
        }

        if (sanity.Entries.Count == 0)
        {
            throw new InvalidOperationException("Corpus guard failed: the Atom sample parsed to zero entries.");
        }
    }

    /// <summary>
    /// Releases the stub transport after the run.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup() => this.Dispose();

    /// <summary>
    /// Parses the Atom sample synchronously — the anchor the async arms are read against.
    /// </summary>
    /// <returns>The number of entries parsed, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "a. AtomFeed.Load (sync anchor)")]
    public int LoadAtomSync()
    {
        AtomFeed feed = new();
        using MemoryStream stream = new(this.atomFeedBytes, writable: false);
        feed.Load(stream);
        return feed.Entries.Count;
    }

    /// <summary>
    /// Creates an Atom feed through the async surface.
    /// </summary>
    /// <returns>The number of entries parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "b. AtomFeed.CreateAsync")]
    public async Task<int> CreateAtomFeed()
    {
        this.handler.Body = this.atomFeedBytes;
        AtomFeed feed = await AtomFeed.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return feed.Entries.Count;
    }

    /// <summary>
    /// Creates a stand-alone Atom entry through the async surface.
    /// </summary>
    /// <returns>One when an entry parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "c. AtomEntry.CreateAsync")]
    public async Task<int> CreateAtomEntry()
    {
        this.handler.Body = this.atomEntryBytes;
        AtomEntry entry = await AtomEntry.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return entry.Title is null ? 0 : 1;
    }

    /// <summary>
    /// Creates an OPML document through the async surface.
    /// </summary>
    /// <returns>The number of outlines parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "d. OpmlDocument.CreateAsync")]
    public async Task<int> CreateOpml()
    {
        this.handler.Body = this.opmlBytes;
        OpmlDocument document = await OpmlDocument.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return document.Outlines.Count;
    }

    /// <summary>
    /// Creates an APML document through the async surface.
    /// </summary>
    /// <returns>The number of profiles parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "e. ApmlDocument.CreateAsync")]
    public async Task<int> CreateApml()
    {
        this.handler.Body = this.apmlBytes;
        ApmlDocument document = await ApmlDocument.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return document.Profiles.Count;
    }

    /// <summary>
    /// Creates a BlogML document through the async surface.
    /// </summary>
    /// <returns>The number of posts parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "f. BlogMLDocument.CreateAsync")]
    public async Task<int> CreateBlogml()
    {
        this.handler.Body = this.blogmlBytes;
        BlogMLDocument document = await BlogMLDocument.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return document.Posts.Count;
    }

    /// <summary>
    /// Creates an RSD document through the async surface.
    /// </summary>
    /// <returns>The number of APIs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "g. RsdDocument.CreateAsync")]
    public async Task<int> CreateRsd()
    {
        this.handler.Body = this.rsdBytes;
        RsdDocument document = await RsdDocument.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return document.Interfaces.Count;
    }

    /// <summary>
    /// Creates a sitemap through the async surface.
    /// </summary>
    /// <returns>The number of URLs parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "h. Sitemap.CreateAsync")]
    public async Task<int> CreateSitemap()
    {
        this.handler.Body = this.sitemapBytes;
        Sitemap sitemap = await Sitemap.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return sitemap.Urls.Count;
    }

    /// <summary>
    /// Creates a sitemap index through the async surface.
    /// </summary>
    /// <returns>The number of entries parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "i. SitemapIndex.CreateAsync")]
    public async Task<int> CreateSitemapIndex()
    {
        this.handler.Body = this.sitemapIndexBytes;
        SitemapIndex index = await SitemapIndex.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return index.Sitemaps.Count;
    }

    /// <summary>
    /// Creates an Atom Publishing service document through the async surface.
    /// </summary>
    /// <returns>The number of workspaces parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "j. AtomServiceDocument.CreateAsync")]
    public async Task<int> CreateServiceDocument()
    {
        this.handler.Body = this.serviceBytes;
        AtomServiceDocument document = await AtomServiceDocument.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return document.Workspaces.Count;
    }

    /// <summary>
    /// Creates an Atom Publishing category document through the async surface.
    /// </summary>
    /// <returns>The number of categories parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "k. AtomCategoryDocument.CreateAsync")]
    public async Task<int> CreateCategoryDocument()
    {
        this.handler.Body = this.categoryBytes;
        AtomCategoryDocument document = await AtomCategoryDocument.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return document.Categories.Count;
    }

    /// <summary>
    /// Creates a publishing-aware Atom entry resource through the async surface.
    /// </summary>
    /// <returns>One when an entry parsed, so the work cannot be elided.</returns>
    [Benchmark(Description = "l. AtomEntryResource.CreateAsync")]
    public async Task<int> CreateEntryResource()
    {
        this.handler.Body = this.atomEntryBytes;
        AtomEntryResource resource = await AtomEntryResource.CreateAsync(Source, this.client, settings: null, requestOptions: null, cancellationToken: default).ConfigureAwait(false);
        return resource.Title is null ? 0 : 1;
    }

    /// <summary>
    /// Releases the stub client and handler.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the stub client and handler.
    /// </summary>
    /// <param name="disposing">Whether managed state should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.client.Dispose();
            this.handler.Dispose();
        }

        this.disposed = true;
    }

    /// <summary>
    /// Returns the currently assigned sample body for every request, so the measurement is the
    /// library rather than the network.
    /// </summary>
    private sealed class SampleServingHandler : HttpMessageHandler
    {
        public byte[] Body { get; set; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(this.Body),
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

            return Task.FromResult(response);
        }
    }
}