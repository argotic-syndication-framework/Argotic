using System.Diagnostics.CodeAnalysis;

using Argotic.Common;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Prices the invalid-character sanitiser against input that can actually reach its rebuild loop.
/// </summary>
/// <remarks>
/// <para>
/// <c>RemoveInvalidXmlHexadecimalCharacters</c> has two costs, not one. On a clean document it scans,
/// finds nothing, and returns the original instance — 0 B allocated, which
/// <c>docs/build-warnings.md</c> records. On a dirty one it rebuilds into a <c>StringBuilder</c>.
/// <b>Every generator in <see cref="FeedCorpus"/> was clean, so only the first cost had ever been
/// measured</b> and a rewrite of the rebuild loop could be declared free on evidence that never
/// touched it.
/// </para>
/// <para>
/// The sweep is deliberately separate from <see cref="SanitiserShapeBenchmarks"/>. Mixing a
/// <c>[Params]</c> axis with arms that do not vary along it reproduces the defect
/// <c>ExtensionDetectionBenchmarks</c> carried until it was repaired: six of nine rows were the same
/// measurement replayed, differing by less than the noise.
/// </para>
/// </remarks>
[BenchmarkCategory("load", "sanitiser", "fidelity")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class SanitiserCostBenchmarks
{
    private string document = string.Empty;

    /// <summary>
    /// Gets or sets the fraction of eligible text characters that are invalid.
    /// </summary>
    /// <remarks>
    ///     Zero is the fast path — scan, find nothing, return the original instance. The other two
    ///     values both enter the rebuild loop; the gap between them says whether cost tracks dirt
    ///     density or merely the presence of dirt.
    /// </remarks>
    [Params(0.0, 0.0001, 0.01)]
    public double DirtFraction { get; set; }

    /// <summary>
    /// Builds a hundred-item document at the configured dirt density.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.document = System.Text.Encoding.UTF8.GetString(
        FeedCorpus.GenerateRssWithDirtUtf8(100, this.DirtFraction));

    /// <summary>
    /// Runs the sanitiser alone.
    /// </summary>
    /// <returns>The sanitised string, so the work cannot be elided.</returns>
    [Benchmark(Baseline = true, Description = "RemoveInvalidXmlHexadecimalCharacters")]
    public string Sanitise() => SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(this.document);

    /// <summary>
    /// Runs the sanitiser and the parse together, which is what a consumer actually pays.
    /// </summary>
    /// <returns>The navigator, so the work cannot be elided.</returns>
    [Benchmark(Description = "CreateSafeNavigator(string)")]
    public System.Xml.XPath.XPathNavigator SanitiseAndParse()
        => SyndicationEncodingUtility.CreateSafeNavigator(this.document);
}