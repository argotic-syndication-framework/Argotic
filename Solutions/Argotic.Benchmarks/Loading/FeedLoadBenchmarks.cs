using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Argotic.Syndication;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Measures RSS feed parsing across the public load surfaces, at three feed sizes.
/// </summary>
/// <remarks>
/// The size ladder is the point. With a single size a result is just a number; across
/// 10 → 100 → 1000 items it becomes a scaling curve, and a curve is falsifiable — cost that
/// grows super-linearly with feed size is a defect regardless of whether anyone has declared a
/// performance target. This repository has no such target, so the curve is the only honest
/// definition of "issue" available.
/// </remarks>
[BenchmarkCategory("load", "rss", "synthetic")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class FeedLoadBenchmarks
{
    private byte[] document = [];

    /// <summary>
    /// Gets or sets the number of items in the feed under test.
    /// </summary>
    /// <remarks>Spans two orders of magnitude — enough to separate linear from super-linear.</remarks>
    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates the document once per parameter set, so the measured work is parsing rather
    /// than string building.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.document = FeedCorpus.GenerateRssUtf8(this.ItemCount);
    }

    /// <summary>
    /// Parses via <see cref="RssFeed.Load(Stream)"/> — the surface a caller uses for a response
    /// body or a file, and the one that performs Argotic's own encoding detection.
    /// </summary>
    /// <returns>The parsed feed, returned so the JIT cannot elide the work.</returns>
    [Benchmark(Baseline = true, Description = "Load(Stream)")]
    public RssFeed LoadFromStream()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// Parses via <see cref="RssFeed.Load(XmlReader)"/> — the surface that bypasses Argotic's
    /// encoding detection, so the delta against <see cref="LoadFromStream"/> isolates that cost.
    /// </summary>
    /// <returns>The parsed feed, returned so the JIT cannot elide the work.</returns>
    [Benchmark(Description = "Load(XmlReader)")]
    public RssFeed LoadFromXmlReader()
    {
        using MemoryStream stream = new(this.document, writable: false);
        using XmlReader reader = XmlReader.Create(stream);
        RssFeed feed = new();
        feed.Load(reader);
        return feed;
    }
}