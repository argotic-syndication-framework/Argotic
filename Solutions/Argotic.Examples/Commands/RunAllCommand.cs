using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Argotic.Examples.Commands;

/// <summary>
/// Runs all examples sequentially.
/// </summary>
internal sealed class RunAllCommand : AsyncCommand<RunAllSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected override async Task<int> ExecuteAsync(CommandContext context, RunAllSettings settings, CancellationToken cancellationToken)
    {
        ExampleRegistry.Initialize();

        List<ExampleResult> results = [];
        List<ExampleCategory> categories = [.. ExampleRegistry.Categories];

        if (!string.IsNullOrEmpty(settings.Category))
        {
            ExampleCategory? matchedCategory = categories.FirstOrDefault(c =>
                c.Key.Equals(settings.Category, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals(settings.Category, StringComparison.OrdinalIgnoreCase));

            if (matchedCategory is null)
            {
                AnsiConsole.MarkupLine($"[red]Unknown category:[/] {settings.Category}");
                return 1;
            }

            categories = [matchedCategory];
        }

        List<(string Category, ExampleInfo Example)> allExamples = [];

        foreach (ExampleCategory category in categories)
        {
            IReadOnlyList<ExampleInfo> examples = ExampleRegistry.GetExamples(category.Key);
            foreach (ExampleInfo example in examples)
            {
                // Declared on the method by [RequiresNetwork], not inferred from the name. The keyword
                // filter this replaces was wrong in both directions: twelve local-file examples named
                // "Create" or "Load Uri" were excluded for nothing, and one live fetch whose name
                // matched no keyword ran inside the offline gate at every commit.
                if (settings.SkipNetwork && example.RequiresNetwork)
                {
                    continue;
                }
                allExamples.Add((category.Key, example));
            }
        }

        if (!settings.JsonOutput)
        {
            AnsiConsole.MarkupLine($"[blue]Running {allExamples.Count} examples...[/]");
            AnsiConsole.WriteLine();
        }

        int passed = 0;
        int failed = 0;
        int skipped = 0;

        foreach ((string category, ExampleInfo example) in allExamples)
        {
            ExampleResult result = new()
            {
                Name = example.Name,
                Category = category,
                MethodName = example.MethodName
            };

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                if (!settings.JsonOutput)
                {
                    AnsiConsole.Markup($"  [dim]{category}[/] {example.Name}... ");
                }

                await example.RunAsync().ConfigureAwait(false);

                stopwatch.Stop();
                result.Status = "Passed";
                result.DurationMs = stopwatch.ElapsedMilliseconds;
                passed++;

                if (!settings.JsonOutput)
                {
                    AnsiConsole.MarkupLine($"[green]OK[/] [dim]({stopwatch.ElapsedMilliseconds}ms)[/]");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Status = "Failed";
                result.Error = ex.Message;
                result.ErrorType = ex.GetType().Name;
                result.DurationMs = stopwatch.ElapsedMilliseconds;
                failed++;

                if (!settings.JsonOutput)
                {
                    AnsiConsole.MarkupLine($"[red]FAILED[/]");
                    AnsiConsole.MarkupLine($"    [red]{Markup.Escape(ex.GetType().Name)}:[/] {Markup.Escape(ex.Message)}");
                }

                if (!settings.ContinueOnError)
                {
                    results.Add(result);
                    break;
                }
            }

            results.Add(result);
        }

        if (settings.JsonOutput)
        {
            var output = new
            {
                Summary = new { Passed = passed, Failed = failed, Skipped = skipped, Total = results.Count },
                Results = results
            };
            Console.WriteLine(JsonSerializer.Serialize(output, JsonOptions));
        }
        else
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Summary:[/]");
            AnsiConsole.MarkupLine($"  [green]Passed:[/] {passed}");
            AnsiConsole.MarkupLine($"  [red]Failed:[/] {failed}");
            if (skipped > 0)
            {
                AnsiConsole.MarkupLine($"  [yellow]Skipped:[/] {skipped}");
            }
            AnsiConsole.MarkupLine($"  [dim]Total:[/] {results.Count}");
        }

        return failed > 0 ? 1 : 0;
    }

    private sealed class ExampleResult
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
        public string? ErrorType { get; set; }
        public long DurationMs { get; set; }
    }
}