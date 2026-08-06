using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Argotic.Common;
using Argotic.Extensions.Core;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Utilities;

/// <summary>
/// Answers whether the <see cref="FrozenDictionary{TKey, TValue}"/> modernisation actually paid,
/// against what it actually replaced.
/// </summary>
/// <remarks>
/// <para>
/// The premise that prompted this was "we moved to FrozenDictionary but never verified it helped".
/// An audit of all 14 declarations first established two facts that decide what these arms have to
/// be, and both contradict the obvious experiment:
/// </para>
/// <list type="number">
///   <item><description><b>Nothing is built per call.</b> Every declaration is <c>static readonly</c>,
///   or a <c>static readonly</c> field of a generic type closed over 22 enums. Construction cost is
///   paid once per process and is not what a caller pays, so an arm that measures
///   <c>ToFrozenDictionary()</c> would be measuring something no workload reaches.</description></item>
///   <item><description><b>The predecessor was not a <c>Dictionary</c>.</b> The enum mappings replaced
///   a <em>per-call reflection scan</em> — <c>GetFields</c>, then <c>GetCustomAttribute</c> on each.
///   Benchmarking Frozen against Dictionary answers a question nobody asked; the honest baseline is
///   the reflection it displaced, which is the <c>Reflection…</c> arm below.</description></item>
/// </list>
/// <para>
/// So there are two separate questions here and they have different answers. <b>Was the move worth
/// making?</b> — compare against reflection. <b>Is Frozen the right structure now?</b> — compare
/// against <see cref="Dictionary{TKey, TValue}"/>, a <c>switch</c>, and a linear scan, at the sizes
/// that actually occur. 50 of the 56 live instances hold <b>8 entries or fewer</b> and 30 hold three
/// or fewer, which is precisely the region where a hash structure has no room to win.
/// </para>
/// <para>
/// Hit position is a parameter because it is the whole argument: a linear scan beats a hash lookup on
/// the first key and loses on the last, and a miss is the case a real feed hits most — the audit's
/// census of 627 live feeds found <c>geo:</c> declared by 121 and used by 5.
/// </para>
/// </remarks>
[BenchmarkCategory("utilities", "frozen")]
[SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "BenchmarkDotNet discovers benchmark types by reflection over the assembly's public types and generates a separate runner assembly that calls into them; an internal benchmark class is silently not discovered.")]
public class FrozenLookupBenchmarks
{
    // The real shape: four members, one of them an absence marker, three matchable tokens. This is
    // the modal size in the library - 30 of 56 live instances hold three entries or fewer.
    private static readonly string[] Tokens = ["full", "nonstop", "sample"];
    private static readonly YahooMediaExpression[] Values =
        [YahooMediaExpression.Full, YahooMediaExpression.Nonstop, YahooMediaExpression.Sample];

    private static readonly FrozenDictionary<string, YahooMediaExpression> Frozen =
        Tokens.Zip(Values).ToFrozenDictionary(p => p.First, p => p.Second, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, YahooMediaExpression> Hashed =
        Tokens.Zip(Values).ToDictionary(p => p.First, p => p.Second, StringComparer.OrdinalIgnoreCase);

    private string probe = "full";

    /// <summary>
    /// Gets or sets which key is looked up: the first, the last, or one that is not present.
    /// </summary>
    /// <remarks>
    ///     A miss is not an edge case here. Extension probing asks "is this token one of mine?" far
    ///     more often than it gets a hit, so an arm measuring only hits would price the structure for
    ///     the rarer half of its work.
    /// </remarks>
    [Params("first", "last", "miss")]
    public string HitPosition { get; set; } = "first";

    /// <summary>
    /// Selects the probe key for the configured hit position.
    /// </summary>
    [GlobalSetup]
    public void Setup() => this.probe = this.HitPosition switch
    {
        "first" => "full",
        "last" => "sample",
        _ => "widescreen",
    };

    /// <summary>
    /// What the library does today.
    /// </summary>
    /// <returns>The resolved value.</returns>
    [Benchmark(Baseline = true, Description = "FrozenDictionary (as shipped)")]
    public YahooMediaExpression FrozenLookup() =>
        Frozen.GetValueOrDefault(this.probe, YahooMediaExpression.None);

    /// <summary>
    /// The same structure as an ordinary hash dictionary, to isolate what freezing buys.
    /// </summary>
    /// <returns>The resolved value.</returns>
    [Benchmark(Description = "Dictionary, same comparer")]
    public YahooMediaExpression DictionaryLookup() =>
        Hashed.GetValueOrDefault(this.probe, YahooMediaExpression.None);

    /// <summary>
    /// No structure at all: the comparison chain a three-token enum could be written as.
    /// </summary>
    /// <returns>The resolved value.</returns>
    [Benchmark(Description = "switch on the token")]
    public YahooMediaExpression SwitchLookup() => this.probe.ToLowerInvariant() switch
    {
        "full" => YahooMediaExpression.Full,
        "nonstop" => YahooMediaExpression.Nonstop,
        "sample" => YahooMediaExpression.Sample,
        _ => YahooMediaExpression.None,
    };

    /// <summary>
    /// A linear scan of the token array, which at three entries is a serious contender.
    /// </summary>
    /// <returns>The resolved value.</returns>
    [Benchmark(Description = "linear scan of a 3-entry array")]
    public YahooMediaExpression LinearScan()
    {
        for (int i = 0; i < Tokens.Length; i++)
        {
            if (string.Equals(Tokens[i], this.probe, StringComparison.OrdinalIgnoreCase))
            {
                return Values[i];
            }
        }

        return YahooMediaExpression.None;
    }

    /// <summary>
    /// The public API the extensions actually call, which routes through the cached frozen mapping.
    /// </summary>
    /// <returns>The resolved value.</returns>
    /// <remarks>
    ///     Separate from <see cref="FrozenLookup"/> because it measures the whole public path, generic
    ///     dispatch and static-cache access included, rather than a bare lookup on a local field.
    /// </remarks>
    [Benchmark(Description = "EnumerationMetadataAttribute (the public path)")]
    public YahooMediaExpression PublicPath() =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(this.probe, YahooMediaExpression.None);

    /// <summary>
    /// The predecessor: a reflection scan performed on every call.
    /// </summary>
    /// <returns>The resolved value.</returns>
    /// <remarks>
    ///     This, not <see cref="DictionaryLookup"/>, is the baseline the modernisation was measured
    ///     against — the enum mappings replaced this, so this is the arm that says whether the change
    ///     was worth making. Transcribed to match the shape the audit recovered from the history:
    ///     enumerate the public static fields, read the attribute off each, compare the alternate
    ///     value.
    /// </remarks>
    [Benchmark(Description = "per-call reflection scan (what it replaced)")]
    public YahooMediaExpression ReflectionScan()
    {
        foreach (FieldInfo field in typeof(YahooMediaExpression).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetCustomAttribute<EnumerationMetadataAttribute>() is { } metadata
                && string.Equals(metadata.AlternateValue, this.probe, StringComparison.OrdinalIgnoreCase))
            {
                return (YahooMediaExpression)field.GetValue(null)!;
            }
        }

        return YahooMediaExpression.None;
    }
}