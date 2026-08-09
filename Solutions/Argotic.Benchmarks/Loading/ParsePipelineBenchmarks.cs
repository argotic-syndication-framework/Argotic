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
/// The decomposition this class was built on no longer exists, and pretending otherwise would be
/// worse than losing it. The stream path used to buffer the document, sniff it, decode it to a
/// string and sanitise that into a second string, so each stage was a public call nested inside the
/// next and the stages between them fell out by subtraction. It now reads a bounded head and streams
/// the rest, and there is no intermediate to measure.
/// </para>
/// <para>
/// What survives, and the only subtraction still admissible:
/// </para>
/// <list type="bullet">
///   <item><description>object-model walk ≈ <c>Load(Stream)</c> − <c>CreateSafeNavigator(Stream)</c></description></item>
/// </list>
/// <para>
/// Containment still holds for <c>sniff ⊂ CreateSafeNavigator(Stream) ⊂ Load(Stream)</c>, and that is
/// the honesty check: a nested call measuring larger than the one containing it means the
/// decomposition is wrong and no conclusion may be drawn from it.
/// </para>
/// <para>
/// The following relations no longer hold, and are recorded here so that they are not inferred from
/// the shape of the arm list:
/// </para>
/// <list type="bullet">
///   <item><description>
///   <c>c ⊄ d</c> — <c>CreateSafeNavigator(string)</c> no longer calls the sanitiser as a separate
///   pass.
///   </description></item>
///   <item><description>
///   <c>d ⊄ e</c> — the stream path never materialises a string, so the string arm is not inside it.
///   </description></item>
///   <item><description>
///   <c>b ⊄ e</c> — <c>GetXmlEncoding(bytes)</c> sniffs the whole array; the stream path sniffs a
///   bounded head.
///   </description></item>
/// </list>
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
    /// Sniffing the declaration from a bounded head — the load path's first act.
    /// </summary>
    /// <returns>The sniffed encoding.</returns>
    /// <remarks>
    /// Replaces the <c>GetStreamBytes</c> arm, which measured buffering the whole document and which
    /// no longer exists. Reached via <c>InternalsVisibleTo</c>: this is the innermost measured point
    /// and the only stage still strictly inside <c>CreateSafeNavigator(Stream)</c>, so inferring it by
    /// subtraction would bury it in a residual.
    /// </remarks>
    [Benchmark(Description = "a. sniff the 512-byte head")]
    public Encoding SniffHead()
        => SyndicationEncodingUtility.SniffXmlEncoding(this.document.AsSpan(0, 512), out _);

    /// <summary>
    /// Encoding detection over the raw bytes.
    /// </summary>
    /// <returns>The detected encoding.</returns>
    [Benchmark(Description = "b. GetXmlEncoding(bytes)")]
    public Encoding DetectEncoding() => SyndicationEncodingUtility.GetXmlEncoding(this.document);

    /// <summary>
    /// Invalid-character stripping.
    /// </summary>
    /// <returns>The sanitised document.</returns>
    /// <remarks>
    /// This no longer produces a second copy of the document, and the corpus cannot make it do so.
    /// <c>IndexOfInvalidXmlCharacter</c> scans first and returns the original instance when the
    /// document is clean, and every document this harness generates is clean - so what is measured
    /// here is the scan, at zero allocation. The rebuild branch is unreachable from any corpus in the
    /// repository, which is a gap in the corpus rather than a property of the method.
    /// </remarks>
    [Benchmark(Description = "c. RemoveInvalidXmlHexadecimalCharacters (clean input: scan only)")]
    public string Sanitise() => SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.decoded);

    /// <summary>
    /// Sanitising plus XPathDocument construction. Contains <see cref="Sanitise"/>.
    /// </summary>
    /// <returns>A navigator over the parsed document.</returns>
    [Benchmark(Description = "d. CreateSafeNavigator(string)")]
    public XPathNavigator NavigatorFromString() => SyndicationEncodingUtility.CreateSafeNavigator(this.decoded);

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
    /// Performs the whole load on a feed that both declares extension namespaces and carries extension
    /// elements.
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
    /// The same load as <see cref="WholeLoad"/>, but passing settings. The control for
    /// <see cref="WholeLoadWithoutExtensionDetection"/>.
    /// </summary>
    /// <returns>The parsed feed.</returns>
    /// <remarks>
    /// <para>
    /// This arm existed because the obvious comparison was confounded. <c>RssFeed.Load(Stream, settings)</c>
    /// branched on <c>settings is not null</c>: with settings it called
    /// <c>CreateSafeNavigator(stream, settings.CharacterEncoding)</c>, which supplied the encoding and
    /// so skipped detection entirely. <see cref="WholeLoad"/> passed null and paid for it;
    /// <see cref="WholeLoadWithoutExtensionDetection"/> passed an object and did not. Comparing them
    /// measured two changes at once — at 1000 items the skipped stages were about a tenth of the
    /// reported delta.
    /// </para>
    /// <para>
    /// That confound is gone. <c>CharacterEncoding</c> is nullable and defaults to null, so a
    /// default settings object no longer names an encoding and both arms sniff. <c>f</c> and <c>g</c>
    /// are now directly comparable for the first time.
    /// </para>
    /// <para>
    /// The arm stays, with its job inverted: it used to be the control that made <c>g</c> readable,
    /// and it is now the control that proves the settings branch costs nothing. <c>f</c> and
    /// <c>f2</c> must agree on allocation to the byte. If they diverge, a settings object has
    /// silently acquired a cost again, and every <c>f</c>-versus-<c>g</c> reading in this file is
    /// measuring that instead of what it claims to.
    /// </para>
    /// <para>
    /// Measured: 146,440 B / 969,968 B / 9,092,368 B at 10, 100 and 1000 items — equal in both
    /// arms at every size, not merely close. Read the allocation column and nothing else here.
    /// The same run timed these two provably identical code paths at a ratio of 1.384 under one job
    /// configuration and 0.908 under another, which is why only allocation is quoted from this class.
    /// </para>
    /// </remarks>
    [Benchmark(Description = "f2. Load(Stream, settings), AutoDetectExtensions=true")]
    public RssFeed WholeLoadWithSettings()
    {
        using MemoryStream stream = new(this.document, writable: false);
        RssFeed feed = new();
        feed.Load(stream, new SyndicationResourceLoadSettings { AutoDetectExtensions = true });
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
    /// and it gates the entire per-entity extension-detection path. Its control is
    /// <see cref="WholeLoadWithSettings"/>, not <see cref="WholeLoad"/>.
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