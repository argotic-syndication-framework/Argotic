using BenchmarkDotNet.Running;

namespace Argotic.Benchmarks;

/// <summary>
/// Entry point for the Argotic benchmark harness.
/// </summary>
/// <remarks>
/// Run everything:
/// <code>dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --job Short</code>
/// Run one class:
/// <code>dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --filter *FeedLoad* --job Short</code>
/// Run one category:
/// <code>dotnet run -c Release --project Solutions/Argotic.Benchmarks -- --anyCategories fidelity --job Short</code>
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
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}