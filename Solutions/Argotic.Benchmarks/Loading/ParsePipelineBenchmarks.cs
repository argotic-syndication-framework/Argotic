using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Decomposes <see cref="RssFeed.Load(Stream)"/> so cost can be attributed to a stage rather than
/// merely observed on the whole.
/// </summary>
/// <remarks>
/// <para>
/// The public load path runs: read the stream to a byte array, sniff the XML encoding, decode to
/// a string, strip invalid XML characters into a second string, build an
/// <see cref="XPathDocument"/>, then walk that document into Argotic's object model.
/// </para>
/// <para>
/// <strong>Only public API is measured.</strong> Argotic's byte-buffering step
/// (<c>GetStreamBytes</c>) is private, so it is deliberately NOT benchmarked directly — adding
/// <c>InternalsVisibleTo</c> or widening its accessibility would change the shipped API surface
/// in order to observe it. Instead the measured points are nested public calls, and the private
/// stages fall out by subtraction:
/// </para>
/// <list type="bullet">
///   <item><description>XPathDocument build ≈ <c>CreateSafeNavigator(string)</c> − <c>RemoveInvalidXmlHexadecimalCharacters</c></description></item>
///   <item><description>buffer + decode ≈ <c>CreateSafeNavigator(Stream)</c> − <c>CreateSafeNavigator(string)</c> − <c>GetXmlEncoding</c></description></item>
///   <item><description>object-model walk ≈ <c>Load(Stream)</c> − <c>CreateSafeNavigator(Stream)</c></description></item>
/// </list>
/// <para>
/// Each measured point strictly contains the one before it, so the subtractions are well defined.
/// The containment is also the built-in honesty check: if a nested call ever measures larger than
/// the one containing it, the decomposition is wrong and no conclusion may be drawn from it.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[BenchmarkCategory("pipeline", "rss")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class ParsePipelineBenchmarks
{
    private byte[] document = [];
    private string decoded = string.Empty;

    /// <summary>
    /// Gets or sets the number of items in the feed under test.
    /// </summary>
    [Params(10, 100, 1000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Prepares each measured point's real starting input.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.document = FeedCorpus.GenerateRssUtf8(this.ItemCount);
        this.decoded = Encoding.UTF8.GetString(this.document);
    }

    /// <summary>
    /// Innermost measured point — encoding detection over the raw bytes.
    /// </summary>
    /// <returns>The detected encoding.</returns>
    [Benchmark(Description = "a. GetXmlEncoding(bytes)")]
    public Encoding DetectEncoding()
    {
        return SyndicationEncodingUtility.GetXmlEncoding(this.document);
    }

    /// <summary>
    /// Invalid-character stripping, which produces a second full copy of the document.
    /// </summary>
    /// <returns>The sanitised document.</returns>
    [Benchmark(Description = "b. RemoveInvalidXmlHexadecimalCharacters")]
    public string Sanitise()
    {
        return SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.decoded);
    }

    /// <summary>
    /// Sanitising plus XPathDocument construction. Contains <see cref="Sanitise"/>.
    /// </summary>
    /// <returns>A navigator over the parsed document.</returns>
    [Benchmark(Description = "c. CreateSafeNavigator(string)")]
    public XPathNavigator NavigatorFromString()
    {
        return SyndicationEncodingUtility.CreateSafeNavigator(this.decoded);
    }

    /// <summary>
    /// The whole navigator construction from a stream: buffer, sniff, decode, sanitise, build.
    /// Contains <see cref="NavigatorFromString"/> and <see cref="DetectEncoding"/>.
    /// </summary>
    /// <returns>A navigator over the parsed document.</returns>
    [Benchmark(Description = "d. CreateSafeNavigator(Stream)")]
    public XPathNavigator NavigatorFromStream()
    {
        using MemoryStream stream = new(this.document, writable: false);
        return SyndicationEncodingUtility.CreateSafeNavigator(stream);
    }

    /// <summary>
    /// The whole public load path. Contains every point above plus the object-model walk.
    /// </summary>
    /// <returns>The parsed feed.</returns>
    [Benchmark(Baseline = true, Description = "e. whole Load(Stream)")]
    public RssFeed WholeLoad()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }
}
