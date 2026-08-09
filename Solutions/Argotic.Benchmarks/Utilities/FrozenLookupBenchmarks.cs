using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Argotic.Common;
using Argotic.Extensions.Core;
using BenchmarkDotNet.Attributes;

namespace Argotic.Benchmarks.Utilities;

/// <summary>
/// Measures <see cref="FrozenDictionary{TKey, TValue}"/> lookup against the reflection scan it
/// replaced and against the alternative structures available at the sizes this library uses.
/// </summary>
/// <remarks>
/// <para>
/// Two properties of the 14 declarations in this library determine which arms are meaningful, and
/// both rule out the obvious experiment.
/// </para>
/// <list type="number">
///   <item><description>
///   No mapping is constructed per call. Every declaration is <c>static readonly</c>, or a
///   <c>static readonly</c> field of a generic type closed over 22 enumerations. Construction is paid
///   once per process, so an arm measuring <c>ToFrozenDictionary()</c> would measure a cost no
///   workload reaches.
///   </description></item>
///   <item><description>
///   The predecessor was not a <see cref="Dictionary{TKey, TValue}"/>. The enumeration mappings
///   replaced a per-call reflection scan of <c>GetFields</c> followed by <c>GetCustomAttribute</c> on
///   each field. The baseline that establishes whether the change paid is therefore the reflection
///   arm below, not the dictionary arm.
///   </description></item>
/// </list>
/// <para>
/// The arms answer two distinct questions. Whether the change paid is answered against reflection.
/// Whether <see cref="FrozenDictionary{TKey, TValue}"/> remains the correct structure is answered
/// against <see cref="Dictionary{TKey, TValue}"/>, a <c>switch</c> and a linear scan, at the observed
/// sizes: 50 of the 56 live instances hold eight entries or fewer and 30 hold three or fewer, which
/// is the region in which a hash structure has the least opportunity to win.
/// </para>
/// <para>
/// Hit position is parameterised because it determines the outcome. A linear scan is faster than a
/// hash lookup on the first key and slower on the last, and a miss is the most frequent case in
/// practice: across 627 surveyed live feeds the <c>geo:</c> prefix was declared by 121 and used by 5.
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
    ///     A miss is the common case rather than an edge case. Extension probing tests whether a token
    ///     belongs to a given extension far more often than it matches, so an arm measuring only hits
    ///     would price the structure for the rarer half of its work.
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
    /// Looks the key up in the frozen mapping, as the library does.
    /// </summary>
    /// <returns>The resolved value.</returns>
    [Benchmark(Baseline = true, Description = "FrozenDictionary (as shipped)")]
    public YahooMediaExpression FrozenLookup() =>
        Frozen.GetValueOrDefault(this.probe, YahooMediaExpression.None);

    /// <summary>
    /// Looks the key up in an ordinary hash dictionary, isolating the contribution of freezing.
    /// </summary>
    /// <returns>The resolved value.</returns>
    [Benchmark(Description = "Dictionary, same comparer")]
    public YahooMediaExpression DictionaryLookup() =>
        Hashed.GetValueOrDefault(this.probe, YahooMediaExpression.None);

    /// <summary>
    /// Resolves the key through a comparison chain, using no lookup structure at all.
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
    /// Resolves the key by scanning the token array, which at three entries is a viable alternative.
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
    /// Resolves the key through the public API the extensions call, which routes through the cached
    /// frozen mapping.
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
    /// Resolves the key by the per-call reflection scan that the frozen mapping replaced.
    /// </summary>
    /// <returns>The resolved value.</returns>
    /// <remarks>
    ///     This arm, not <see cref="DictionaryLookup"/>, is the baseline that establishes whether the
    ///     change was worth making, because this is what the enumeration mappings replaced. It
    ///     reproduces the original sequence: enumerate the public static fields, read the attribute
    ///     from each, and compare its alternate value.
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