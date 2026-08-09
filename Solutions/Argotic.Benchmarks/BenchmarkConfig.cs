using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;

namespace Argotic.Benchmarks;

/// <summary>
/// The harness-wide BenchmarkDotNet configuration.
/// </summary>
/// <remarks>
/// <para>
/// Diagnosers live here rather than as per-class attributes so every benchmark reports the same
/// columns. A diagnoser attached to some classes and not others produces a results table with
/// holes in it, and a hole reads as "no allocations" rather than "not measured".
/// </para>
/// <para><strong>Always on</strong> — cheap enough that the measurement is not distorted:</para>
/// <list type="bullet">
///   <item><description><see cref="MemoryDiagnoser"/> — managed allocation and Gen0/1/2 counts.
///   Half of this investigation is memory, and allocation reproduces to within 0.1% across runs
///   where timing on this machine does not.</description></item>
///   <item><description><see cref="ExceptionDiagnoser"/> — exceptions thrown per operation.
///   Throw/catch used as control flow inside a parser is a classic silent cost that no timing
///   number explains on its own; if this column is non-zero it is a finding in itself.</description></item>
///   <item><description><see cref="ThreadingDiagnoser"/> — lock contention and completed work
///   items. Feed parsing should be entirely single-threaded, so any contention here would be
///   unexpected, and unexpected is exactly what this run is looking for.</description></item>
/// </list>
/// <para>
/// <strong>Available but opt-in</strong>, because each materially slows or distorts a run and
/// none is needed to answer "where does the cost go":
/// </para>
/// <list type="bullet">
///   <item><description><c>EventPipeProfiler</c> — pass <c>--profiler EP</c>. Emits a .nettrace
///   for attributing cost below the public API seam. This is the tool for identifying a mechanism
///   once a stage has been localised, which is why it is wired for use but not switched on by
///   default.</description></item>
///   <item><description><c>PerfCollectProfiler</c> — pass <c>--profiler perf</c>. Linux-native
///   sampling; needs <c>perf</c> installed and elevated permissions, which a dev container
///   typically lacks. Expect it to be unavailable here rather than silently empty.</description></item>
///   <item><description><c>DisassemblyDiagnoser</c> — pass <c>--disasm</c>. Answers questions
///   about codegen. Premature while the open question is algorithmic shape rather than
///   instruction selection.</description></item>
/// </list>
/// <para>
/// Deliberately absent: <c>NativeMemoryProfiler</c> (the parse path is managed; native allocation
/// is not where this cost lives), and the Windows-only diagnosers from
/// BenchmarkDotNet.Diagnostics.Windows — this repository targets net10.0 and runs its builds on
/// Linux, so an ETW-based diagnoser would report nothing.
/// </para>
/// <para>
/// Also deliberately absent: an orderer. BenchmarkDotNet can sort a summary from fastest to slowest,
/// but several classes here document an arm order that carries meaning — position in a dispatch
/// chain, or successive stages of a pipeline whose adjacent differences are the measurement — and
/// re-sorting the table would destroy the property those classes were built on.
/// </para>
/// <para>
/// Tiered compilation is left at its default, and the reason is worth recording because the common
/// advice is the opposite. Disabling it does reduce dispersion: measured over the eighteen cases of
/// <c>FrozenLookupBenchmarks</c>, <c>DOTNET_TieredCompilation=0</c> tightened seventeen of them and
/// halved the mean error, from 13.3% of the mean to 6.5%. It reduces dispersion by changing what is
/// measured. The same run reported the frozen-dictionary lookup at 8.83 ns against 1.61 ns by
/// default, and a three-entry linear scan at 2.97 ns against 0.52 ns — between 1.3 and 7.4 times the
/// default across the eighteen. The error bars narrow because slower code jitters proportionally
/// less, and the result would be a table quoting several times the real cost with better-looking
/// confidence intervals to support it.
/// </para>
/// <para>
/// The mechanism is dynamic profile-guided optimisation, which is implemented through tiered
/// compilation: methods are instrumented in the first tier and recompiled in the second using the
/// collected profile. Disabling tiering discards the profile rather than skipping a warmup phase.
/// A third run isolated this — tiering left on with <c>DOTNET_TieredPGO=0</c> reproduced most of the
/// slowdown, placing the same two arms at 5.18 ns and 2.41 ns — which identifies profile-guided
/// optimisation as the dominant component and leaves a smaller residue attributable to tiering
/// itself. Since the library runs under the default configuration, that is the configuration these
/// benchmarks measure.
/// </para>
/// </remarks>
internal sealed class BenchmarkConfig : ManualConfig
{
    /// <summary>
    /// Initialises a new instance of the <see cref="BenchmarkConfig"/> class.
    /// </summary>
    public BenchmarkConfig()
    {
        // Build ON the defaults, not instead of them. A bare ManualConfig replaces BenchmarkDotNet's
        // loggers, columns and exporters wholesale, and the symptom is an empty results table with
        // only "No loggers defined" on stderr - a run that appears to have measured nothing.
        this.Add(DefaultConfig.Instance);

        // Pin the job. The committed artifacts were produced under three different job configurations
        // across eight classes, which makes any cross-class ratio invalid, and ShortRun is what
        // produced an error bar of +/-312% on a mean. A caller can still override with --job.
        this.AddJob(Job.MediumRun);

        this.AddDiagnoser(MemoryDiagnoser.Default);
        this.AddDiagnoser(ExceptionDiagnoser.Default);
        this.AddDiagnoser(ThreadingDiagnoser.Default);

        // The default table reports mean, error and standard deviation only, which describes a
        // distribution adequately when it is symmetric and misleads when it is not. Timing on this
        // machine is not reliably symmetric: thermal throttling truncates a run at one end, so the mean
        // moves while the median does not. These three columns make that visible instead of leaving it
        // to be inferred from an error bar.
        this.AddColumn(StatisticColumn.Median);
        this.AddColumn(StatisticColumn.Min);
        this.AddColumn(StatisticColumn.Max);

        // The default exporters emit Markdown, HTML and CSV, all of which are for reading. None can be
        // consumed by a regression detector. The full compressed JSON document is the format both
        // BenchmarkDotNet.Analyser and the github-action-benchmark action read, so emitting it is what
        // makes a future comparison against a stored baseline possible at all.
        this.AddExporter(JsonExporter.FullCompressed);
    }
}