using System.Diagnostics.CodeAnalysis;

using Argotic.Common;
using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Measures what <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> actually saves.
/// </summary>
/// <remarks>
/// <para>
/// The setting is honoured in sixteen parse loops across the adapters and had no benchmark at all,
/// despite being the largest lever the API offers a consumer. A newsletter that wants the newest ten
/// items of a five-hundred-item archive feed sets it and assumes it pays for ten.
/// </para>
/// <para>
/// It does not, and this measures the gap. The item loop breaks after the limit, but the whole
/// document is still buffered, decoded, sanitised and turned into an <c>XPathDocument</c> before the
/// first item is looked at, so there is a floor below which the setting cannot help. If that floor
/// dominates, the remedy is a streaming early-exit in the load path, not a settings change - and this
/// is the measurement that would say so.
/// </para>
/// <para>
/// Every arm passes a non-null settings object, including the unlimited one.
/// <c>RssFeed.Load(Stream, settings)</c> takes a different branch when settings are null - it skips
/// <c>GetStreamBytes</c> and <c>GetXmlEncoding</c> - so comparing a limited load against
/// <c>Load(stream)</c> would measure the limit and the encoding sniff together. That is exactly the
/// confound this harness carried in <see cref="ParsePipelineBenchmarks"/>.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "rss", "settings")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class RetrievalLimitBenchmarks
{
    private byte[] document = [];

    /// <summary>
    /// Gets or sets the retrieval limit under test. Zero means no limit.
    /// </summary>
    [Params(0, 5, 10, 25, 50)]
    public int RetrievalLimit { get; set; }

    /// <summary>
    /// Builds a five-hundred-item extension-bearing feed, the shape the setting exists for.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.document = FeedCorpus.GenerateRssWithExtensionsUtf8(500);

    /// <summary>
    /// Loads the feed under the configured retrieval limit.
    /// </summary>
    /// <returns>The parsed feed.</returns>
    [Benchmark(Description = "Load(Stream, settings) at the configured RetrievalLimit")]
    public RssFeed LoadWithLimit()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream, new SyndicationResourceLoadSettings { RetrievalLimit = this.RetrievalLimit });
        return feed;
    }
}