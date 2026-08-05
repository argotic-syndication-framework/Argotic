using System.Diagnostics.CodeAnalysis;
using System.Xml.XPath;

using Argotic.Common;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Loads the same document from a seekable stream and from the shape a socket actually delivers.
/// </summary>
/// <remarks>
/// <para>
/// <c>SyndicationEncodingUtility.GetStreamBytes</c> branches on <c>Stream.CanSeek</c>: a seekable
/// stream is sized and filled with one <c>ReadExactly</c>, a non-seekable one falls into
/// <c>stream.CopyTo</c>. <b>Every benchmark in this harness passes a <c>MemoryStream</c>, and the
/// test suite reports that branch at 2 of 4 — the <c>CopyTo</c> arm has never executed anywhere in
/// this repository.</b> It is also the arm that runs against a live network stream, and the one a
/// switch to <c>ResponseHeadersRead</c> would make the common case.
/// </para>
/// <para>
/// The one-byte arm is not a pathology for its own sake. It forces every possible chunk boundary,
/// which is where a streaming decode or a bounded head read gets its arithmetic wrong; and a real
/// network read genuinely does return far less than asked for, which is why a head read must use
/// <c>ReadAtLeast</c> rather than a single <c>Read</c>. The 4,096-byte arm is the realistic middle,
/// and the gap between the three is the price of not being able to seek.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "stream", "shape")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class StreamShapeBenchmarks
{
    private byte[] document = [];

    /// <summary>
    /// Gets or sets the number of items in the document under test.
    /// </summary>
    [Params(100, 1_000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Generates the document once per parameter value.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.document = FeedCorpus.GenerateRssUtf8(this.ItemCount);

    /// <summary>
    /// Parses from a seekable stream — the shape every other benchmark uses.
    /// </summary>
    /// <returns>The navigator, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "seekable MemoryStream")]
    public XPathNavigator Seekable()
    {
        using MemoryStream stream = new(this.document, writable: false);
        return SyndicationEncodingUtility.CreateSafeNavigator(stream);
    }

    /// <summary>
    /// Parses from a non-seekable stream returning up to 4,096 bytes per read.
    /// </summary>
    /// <returns>The navigator, so the work cannot be elided.</returns>
    [Benchmark(Description = "non-seekable, 4,096 bytes per read")]
    public XPathNavigator NonSeekableChunked()
    {
        using ShortReadStream stream = new(this.document, 4_096);
        return SyndicationEncodingUtility.CreateSafeNavigator(stream);
    }

    /// <summary>
    /// Parses from a non-seekable stream returning one byte per read.
    /// </summary>
    /// <returns>The navigator, so the work cannot be elided.</returns>
    [Benchmark(Description = "non-seekable, one byte per read")]
    public XPathNavigator NonSeekableOneByte()
    {
        using ShortReadStream stream = new(this.document, 1);
        return SyndicationEncodingUtility.CreateSafeNavigator(stream);
    }
}