using System.Diagnostics.CodeAnalysis;
using System.Net;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Measures the workload this library is actually deployed for: polling many feeds, repeatedly.
/// </summary>
/// <remarks>
/// <para>
/// Argotic is used in production to gather the Azure Weekly and Power BI Weekly newsletters, which
/// means fetching a set of feeds on a schedule and extracting their items. Not one of the twenty-five
/// public <c>CreateAsync(Uri, …)</c> overloads had a benchmark, so the composite a consumer actually
/// invokes - fetch, decode, parse, walk - had never been priced as a unit.
/// </para>
/// <para>
/// The number this exists to produce is allocation per feed per poll. That is what sizes the
/// memory limit of the function or container the poller runs in, and it is the only figure here that
/// informs a deployment decision rather than a refactoring one.
/// </para>
/// <para>
/// The transport is a stub handler rather than a loopback socket, deliberately. A real socket adds
/// scheduler and kernel noise to a measurement whose subject is the parse, and BenchmarkDotNet cannot
/// separate the two. What is measured here is everything the library does with a response once the
/// bytes are available, which is the part this repository controls.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "polling", "workload")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class PollingSteadyStateBenchmarks : IDisposable
{
    private static readonly Uri Source = new("http://benchmark.invalid/feed.xml");

    private HttpClient client = null!;
    private StubHandler handler = null!;

    /// <summary>
    /// Disposes the stubbed transport.
    /// </summary>
    /// <remarks>
    ///     The full pattern rather than a sealed type with a simple <c>Dispose</c>: BenchmarkDotNet
    ///     rejects a sealed benchmark class outright - "Declaring type must be unsealed" - and does so
    ///     at validation time, after <c>--list</c> has already reported the benchmark as present.
    /// </remarks>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the stubbed transport.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.client?.Dispose();
            this.handler?.Dispose();
        }
    }

    /// <summary>
    /// Gets or sets the number of feeds fetched in one polling cycle.
    /// </summary>
    [Params(1, 25, 100)]
    public int FeedsPerCycle { get; set; }

    /// <summary>
    /// Builds the stubbed client over a twenty-item extension-bearing feed.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        byte[] body = FeedCorpus.GenerateRssWithExtensionsUtf8(20);
        this.handler = new StubHandler(body);
        this.client = new HttpClient(this.handler, disposeHandler: false);
    }

    /// <summary>
    /// Disposes the stubbed transport between parameter values.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup() => this.Dispose();

    /// <summary>
    /// Fetches and parses every feed in one polling cycle, reusing a single client.
    /// </summary>
    /// <returns>The total item count, so the work cannot be elided.</returns>
    [Benchmark(Description = "one polling cycle: CreateAsync per feed, shared client")]
    public async Task<int> PollEveryFeedOnce()
    {
        int items = 0;
        for (int i = 0; i < this.FeedsPerCycle; i++)
        {
            RssFeed feed = await RssFeed.CreateAsync(Source, this.client).ConfigureAwait(false);
            items += feed.Channel.Items.Count;
        }

        return items;
    }

    /// <summary>
    /// The same cycle through the format-agnostic entry point.
    /// </summary>
    /// <remarks>
    /// A poller consuming arbitrary subscriber-supplied URLs does not know the format in advance, so
    /// this is the honest shape for that workload. The delta against the typed arm is the price of
    /// not knowing.
    /// </remarks>
    /// <returns>The total item count, so the work cannot be elided.</returns>
    [Benchmark(Description = "one polling cycle: GenericSyndicationFeed, format unknown")]
    public async Task<int> PollEveryFeedOnceWithoutKnowingTheFormat()
    {
        int items = 0;
        for (int i = 0; i < this.FeedsPerCycle; i++)
        {
            GenericSyndicationFeed feed = await GenericSyndicationFeed.CreateAsync(Source, this.client).ConfigureAwait(false);
            items += feed.Items.Count;
        }

        return items;
    }

    /// <summary>
    /// Returns a fixed body for every request, so the measurement is the library rather than the network.
    /// </summary>
    private sealed class StubHandler(byte[] body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(body),
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/rss+xml");

            return Task.FromResult(response);
        }
    }
}