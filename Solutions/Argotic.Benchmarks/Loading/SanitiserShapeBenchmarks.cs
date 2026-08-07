using System.Diagnostics.CodeAnalysis;
using System.Text;

using Argotic.Common;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Compares the document shapes the sanitiser treats completely differently.
/// </summary>
/// <remarks>
/// <para>
/// Three of these four arms could not be written before <see cref="FeedCorpus"/> gained the
/// generators that produce them, which is the point: the harness measured one shape and called it the
/// baseline.
/// </para>
/// <para>
/// Astral characters break the fast path. A <c>SearchValues&lt;char&gt;</c> built from the
/// complement of <c>XmlConvert.IsXmlChar</c> necessarily contains the whole of <c>[D800-DFFF]</c> — a
/// surrogate is not a valid XML character on its own — so <c>IndexOfAny</c> fires on every emoji and
/// the vectorised scan hands off to the char-by-char pair rule. A 7-bit "clean" baseline therefore
/// measures the path a real feed with an emoji in its title never takes.
/// </para>
/// <para>
/// The all-invalid arms price a loop that correctness tests reach and no benchmark did. A
/// streaming sanitiser must never return zero characters while input remains, because
/// <c>TextReader.Read</c> documents zero as end of input and <c>XmlTextReaderImpl</c> treats it that
/// way — a document with a long NUL run would be silently truncated. A 0–1% dirt sweep never produces
/// a wholly-dropped chunk, so the loop-again path has no measured cost. Two sizes, an order of
/// magnitude apart: the pass condition is that cost grows linearly in the NUL count. This is
/// coverage of an unmeasured branch, not a guard against a quadratic — no plausible implementation is
/// quadratic.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "sanitiser", "fidelity")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SanitiserShapeBenchmarks
{
    private string cleanAscii = string.Empty;
    private string astral = string.Empty;
    private string allInvalidSmall = string.Empty;
    private string allInvalidLarge = string.Empty;

    /// <summary>
    /// Builds the four shapes once.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.cleanAscii = Encoding.UTF8.GetString(FeedCorpus.GenerateRssUtf8(100));
        this.astral = Encoding.UTF8.GetString(FeedCorpus.GenerateRssWithAstralUtf8(100));
        this.allInvalidSmall = Encoding.UTF8.GetString(FeedCorpus.GenerateAllInvalidDocument(20_000));
        this.allInvalidLarge = Encoding.UTF8.GetString(FeedCorpus.GenerateAllInvalidDocument(200_000));
    }

    /// <summary>
    /// The 7-bit baseline every other benchmark in this harness has been using.
    /// </summary>
    /// <returns>The sanitised string, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "clean, 7-bit ASCII")]
    public string CleanAscii() => SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.cleanAscii);

    /// <summary>
    /// The same document with an astral character in every title and description.
    /// </summary>
    /// <returns>The sanitised string, so the work cannot be elided.</returns>
    [Benchmark(Description = "clean, with astral characters")]
    public string Astral() => SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.astral);

    /// <summary>
    /// Twenty thousand NULs between the root tags.
    /// </summary>
    /// <returns>The sanitised string, so the work cannot be elided.</returns>
    [Benchmark(Description = "all-invalid, 20,000 NULs")]
    public string AllInvalidSmall() => SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.allInvalidSmall);

    /// <summary>
    /// Two hundred thousand NULs, ten times the smaller arm.
    /// </summary>
    /// <returns>The sanitised string, so the work cannot be elided.</returns>
    [Benchmark(Description = "all-invalid, 200,000 NULs")]
    public string AllInvalidLarge() => SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.allInvalidLarge);
}