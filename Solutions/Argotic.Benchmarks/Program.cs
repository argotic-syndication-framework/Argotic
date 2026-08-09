using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Argotic.Benchmarks;

/// <summary>
/// Entry point for the Argotic benchmark harness.
/// </summary>
/// <remarks>
/// Run everything:
/// <code>dotnet run -c Release --project Solutions/Argotic.Benchmarks</code>
/// Run one class:
/// <code>dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --filter *FeedLoad*</code>
/// Run one category:
/// <code>dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --anyCategories fidelity</code>
/// None of these names a job, deliberately. <see cref="BenchmarkConfig"/> pins <c>MediumRun</c>, and
/// <c>--job Short</c> overrides it — which is fine for reading the allocation column and not fine for
/// reading a time, because short-job error bars on this hardware equal or exceed their means.
/// Benchmarks must be run in Release; BenchmarkDotNet rejects an unoptimised assembly, correctly.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dispatches to BenchmarkDotNet's switcher so individual classes, categories or filters can
    /// be selected from the command line — the harness is run in stages, not as one long job.
    /// </summary>
    /// <param name="args">BenchmarkDotNet command-line arguments.</param>
    public static void Main(string[] args)
    {
        // The shared config supplies the diagnosers so every class reports the same columns;
        // command-line arguments layer on top of it, so --job, --filter and --profiler still work.
        IConfig config = new BenchmarkConfig();
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}