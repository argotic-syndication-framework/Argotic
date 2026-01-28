using Spectre.Console;
using Spectre.Console.Cli;

namespace Argotic.Examples.Commands;

/// <summary>
/// Runs a specific example by name.
/// </summary>
public sealed class RunCommand : AsyncCommand<RunSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RunSettings settings, CancellationToken cancellationToken)
    {
        ExampleRegistry.Initialize();

        // Find the example by name (case-insensitive, partial match)
        ExampleInfo? matchedExample = null;
        string? matchedCategory = null;

        foreach (ExampleCategory category in ExampleRegistry.Categories)
        {
            IReadOnlyList<ExampleInfo> examples = ExampleRegistry.GetExamples(category.Key);
            ExampleInfo? example = examples.FirstOrDefault(e =>
                e.Name.Equals(settings.Name, StringComparison.OrdinalIgnoreCase) ||
                e.Name.Contains(settings.Name, StringComparison.OrdinalIgnoreCase));

            if (example != null)
            {
                matchedExample = example;
                matchedCategory = category.Key;
                break;
            }
        }

        if (matchedExample == null)
        {
            AnsiConsole.MarkupLine($"[red]Example not found:[/] {settings.Name}");
            AnsiConsole.MarkupLine("[dim]Use 'list' command to see available examples.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[blue]Running:[/] {matchedExample.Name}");
        AnsiConsole.MarkupLine($"[dim]Category: {matchedCategory}[/]");
        AnsiConsole.WriteLine();

        try
        {
            await matchedExample.RunAsync().ConfigureAwait(false);
            AnsiConsole.MarkupLine("[green]Completed successfully.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[dim]Inner: {Markup.Escape(ex.InnerException.Message)}[/]");
            }
            return 1;
        }
    }
}