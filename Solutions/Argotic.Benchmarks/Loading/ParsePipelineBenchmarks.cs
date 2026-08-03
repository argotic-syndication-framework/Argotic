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
/// Most measured points are nested public calls, so the stages between them fall out by
/// subtraction:
/// </para>
/// <list type="bullet">
///   <item><description>XPathDocument build ≈ <c>CreateSafeNavigator(string)</c> − <c>RemoveInvalidXmlHexadecimalCharacters</c></description></item>
///   <item><description>decode ≈ <c>CreateSafeNavigator(Stream)</c> − <c>CreateSafeNavigator(string)</c> − <c>GetXmlEncoding</c> − <c>GetStreamBytes</c></description></item>
///   <item><description>object-model walk ≈ <c>Load(Stream)</c> − <c>CreateSafeNavigator(Stream)</c></description></item>
/// </list>
/// <para>
/// <c>GetStreamBytes</c> is measured directly. It is <c>internal</c> rather than public, and the
/// harness reaches it through an <c>InternalsVisibleTo</c> grant added deliberately for this
/// purpose — the alternative, inferring it by subtraction, buried the repository's most-allocating
/// buffering step inside a residual.
/// </para>
/// <para>
/// Each nested measured point strictly contains the ones inside it, so the subtractions are well
/// defined. That containment is also the built-in honesty check: if a nested call ever measures
/// larger than the one containing it, the decomposition is wrong and no conclusion may be drawn
/// from it.
/// </para>
/// </remarks>
[BenchmarkCategory("pipeline", "rss")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class ParsePipelineBenchmarks
{
    private byte[] document = [];
    private byte[] documentWithExtensions = [];
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
        this.documentWithExtensions = FeedCorpus.GenerateRssWithExtensionsUtf8(this.ItemCount);
        this.decoded = Encoding.UTF8.GetString(this.document);
    }

    /// <summary>
    /// Buffering the whole stream into a byte array — the load path's first act.
    /// </summary>
    /// <returns>The buffered bytes.</returns>
    /// <remarks>
    /// Reached via <c>InternalsVisibleTo</c>; see the class remarks for why this stage is measured
    /// directly rather than inferred.
    /// </remarks>
    [Benchmark(Description = "a. GetStreamBytes")]
    public byte[] BufferStream()
    {
        using MemoryStream stream = new(this.document, writable: false);
        return SyndicationEncodingUtility.GetStreamBytes(stream);
    }

    /// <summary>
    /// Encoding detection over the raw bytes.
    /// </summary>
    /// <returns>The detected encoding.</returns>
    [Benchmark(Description = "b. GetXmlEncoding(bytes)")]
    public Encoding DetectEncoding()
    {
        return SyndicationEncodingUtility.GetXmlEncoding(this.document);
    }

    /// <summary>
    /// Invalid-character stripping, which produces a second full copy of the document.
    /// </summary>
    /// <returns>The sanitised document.</returns>
    [Benchmark(Description = "c. RemoveInvalidXmlHexadecimalCharacters")]
    public string Sanitise()
    {
        return SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.decoded);
    }

    /// <summary>
    /// Sanitising plus XPathDocument construction. Contains <see cref="Sanitise"/>.
    /// </summary>
    /// <returns>A navigator over the parsed document.</returns>
    [Benchmark(Description = "d. CreateSafeNavigator(string)")]
    public XPathNavigator NavigatorFromString()
    {
        return SyndicationEncodingUtility.CreateSafeNavigator(this.decoded);
    }

    /// <summary>
    /// The whole navigator construction from a stream: buffer, sniff, decode, sanitise, build.
    /// Contains <see cref="NavigatorFromString"/> and <see cref="DetectEncoding"/>.
    /// </summary>
    /// <returns>A navigator over the parsed document.</returns>
    [Benchmark(Description = "e. CreateSafeNavigator(Stream)")]
    public XPathNavigator NavigatorFromStream()
    {
        using MemoryStream stream = new(this.document, writable: false);
        return SyndicationEncodingUtility.CreateSafeNavigator(stream);
    }

    /// <summary>
    /// The whole public load path. Contains every point above plus the object-model walk.
    /// </summary>
    /// <returns>The parsed feed.</returns>
    [Benchmark(Baseline = true, Description = "f. whole Load(Stream)")]
    public RssFeed WholeLoad()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// The whole load, on a feed that DECLARES extension namespaces and carries extension elements.
    /// </summary>
    /// <returns>The parsed feed.</returns>
    /// <remarks>
    /// <para>
    /// The load-bearing measurement for any change to extension detection, and the one the
    /// extension-free benchmark cannot substitute for. A change that filters extension work by
    /// declared namespace does no work at all on the extension-free document, so measuring only
    /// that document guarantees a favourable number regardless of the change's real merit.
    /// </para>
    /// <para>
    /// Here the namespaces match, the extensions are genuinely needed, and the work is genuinely
    /// performed — so a change that helps only the easy case shows up as no improvement.
    /// </para>
    /// </remarks>
    [Benchmark(Description = "h. Load(Stream), feed WITH extension namespaces")]
    public RssFeed WholeLoadWithExtensions()
    {
        using MemoryStream stream = new(this.documentWithExtensions, writable: false);
        RssFeed feed = new();
        feed.Load(stream);
        return feed;
    }

    /// <summary>
    /// The same with extension auto-detection switched off.
    /// </summary>
    /// <returns>The parsed feed.</returns>
    /// <remarks>
    /// <para>
    /// The one experiment that can localise the object-model walk without a profiler.
    /// <c>SyndicationResourceLoadSettings.AutoDetectExtensions</c> defaults to <see langword="true"/>,
    /// and it gates the entire per-entity extension-detection path. Everything else about the two
    /// loads is identical.
    /// </para>
    /// <para>
    /// This is falsifiable in both directions, which is why it is worth running. If auto-detection
    /// is the dominant cost, this benchmark collapses relative to
    /// <see cref="WholeLoad"/> and the mechanism is named. If it lands close to
    /// <see cref="WholeLoad"/>, the hypothesis is dead and the cost is in the element walk itself —
    /// a result that would send the investigation somewhere else entirely.
    /// </para>
    /// <para>
    /// Note the two are not behaviourally equivalent: with detection off, extension data is not
    /// populated. This measures where time goes, and is not a proposed fix.
    /// </para>
    /// </remarks>
    [Benchmark(Description = "g. Load(Stream), AutoDetectExtensions=false")]
    public RssFeed WholeLoadWithoutExtensionDetection()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream, new SyndicationResourceLoadSettings { AutoDetectExtensions = false });
        return feed;
    }
}