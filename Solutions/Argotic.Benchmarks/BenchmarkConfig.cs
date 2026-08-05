using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
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
    }
}