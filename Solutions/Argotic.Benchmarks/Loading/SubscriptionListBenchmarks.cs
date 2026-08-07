using System.Diagnostics.CodeAnalysis;

using Argotic.Syndication;

using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Loading;

/// <summary>
/// Measures the subscription-list workload: parse an OPML file, then read every feed address out of it.
/// </summary>
/// <remarks>
///     <para>
///     This is the first thing a newsletter pipeline does, and it was unmeasured. The corpus is
///     calibrated to three production lists (see <see cref="OpmlCorpus"/>), and the parameter values
///     are their real sizes rather than round numbers — 88, 299 and 478 subscriptions.
///     </para>
///     <para>
///     Two arms, because the difference between them is the question. <c>a</c> parses and stops;
///     <c>b</c> does what a caller actually does next, which is walk every outline and pull
///     <c>xmlUrl</c> out of the extensibility dictionary. If <c>b</c> costs materially more than
///     <c>a</c>, the model is making the caller pay twice for data it already parsed.
///     </para>
///     <para>
///     Read the allocation column. On this hardware two provably identical code paths have timed 48%
///     apart while allocating byte-identically, so timing here is not evidence unless allocation
///     agrees with it.
///     </para>
/// </remarks>
[BenchmarkCategory("opml")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered. The rule's premise - that an application's types are not referenced from outside the assembly - does not hold here.")]
public class SubscriptionListBenchmarks
{
    private byte[] document = [];

    /// <summary>
    /// Gets or sets the number of subscriptions in the generated list.
    /// </summary>
    /// <remarks>
    ///     The three sizes the production lists actually are, so a reader can map a row onto a real
    ///     file instead of interpolating.
    /// </remarks>
    [Params(88, 299, 478)]
    public int SubscriptionCount { get; set; }

    /// <summary>
    /// Builds the corpus once per parameter value.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.document = OpmlCorpus.GenerateSubscriptionListUtf8(this.SubscriptionCount);

    /// <summary>
    /// Parses the subscription list and stops.
    /// </summary>
    /// <returns>The parsed document, returned so nothing is optimised away.</returns>
    [Benchmark(Description = "a. OpmlDocument.Load(Stream)", Baseline = true)]
    public OpmlDocument ParseOnly()
    {
        using MemoryStream stream = new(this.document, writable: false);
        OpmlDocument opml = new();
        opml.Load(stream);
        return opml;
    }

    /// <summary>
    /// Parses the list and reads every feed address out of it — the whole of what a caller does.
    /// </summary>
    /// <returns>The number of feed addresses found, returned so nothing is optimised away.</returns>
    [Benchmark(Description = "b. Load + read every xmlUrl")]
    public int ParseAndReadEveryFeedAddress()
    {
        using MemoryStream stream = new(this.document, writable: false);
        OpmlDocument opml = new();
        opml.Load(stream);

        int found = 0;
        foreach (OpmlOutline outline in opml.Outlines)
        {
            found += Count(outline);
        }

        return found;
    }

    private static int Count(OpmlOutline outline)
    {
        int found = outline.Attributes.ContainsKey("xmlUrl") ? 1 : 0;

        foreach (OpmlOutline child in outline.Outlines)
        {
            found += Count(child);
        }

        return found;
    }
}