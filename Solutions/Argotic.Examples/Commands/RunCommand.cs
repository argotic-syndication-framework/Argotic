using Spectre.Console;
using Spectre.Console.Cli;

namespace Argotic.Examples.Commands;

/// <summary>
/// Runs a specific example by name.
/// </summary>
internal sealed class RunCommand : AsyncCommand<RunSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, RunSettings settings, CancellationToken cancellationToken)
    {
        ExampleRegistry.Initialize();

        List<ExampleCategory> categories = [.. ExampleRegistry.Categories];

        if (!string.IsNullOrEmpty(settings.Category))
        {
            categories = [.. categories.Where(c => c.Key.Equals(settings.Category, StringComparison.OrdinalIgnoreCase))];

            if (categories.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]Unknown category:[/] {Markup.Escape(settings.Category)}");
                AnsiConsole.MarkupLine($"[dim]Available: {string.Join(", ", ExampleRegistry.Categories.Select(c => c.Key))}[/]");
                return 1;
            }
        }

        // Collect every match rather than stopping at the first. The same example name occurs in
        // several categories - "Document - Load Stream" exists in four - so stopping early silently
        // runs whichever category happens to be registered first.
        List<(string Category, ExampleInfo Example)> matches = [];

        foreach (ExampleCategory category in categories)
        {
            foreach (ExampleInfo example in ExampleRegistry.GetExamples(category.Key))
            {
                if (example.Name.Contains(settings.Name, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add((category.Key, example));
                }
            }
        }

        // An exact name match beats a partial one, so "Feed - Class" does not report itself as
        // ambiguous against "Feed - Class Async".
        List<(string Category, ExampleInfo Example)> exact =
            [.. matches.Where(m => m.Example.Name.Equals(settings.Name, StringComparison.OrdinalIgnoreCase))];

        List<(string Category, ExampleInfo Example)> selected = exact.Count > 0 ? exact : matches;

        if (selected.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]Example not found:[/] {Markup.Escape(settings.Name)}");
            AnsiConsole.MarkupLine("[dim]Use 'list' command to see available examples.[/]");
            return 1;
        }

        if (selected.Count > 1)
        {
            AnsiConsole.MarkupLine($"[yellow]'{Markup.Escape(settings.Name)}' matches {selected.Count} examples:[/]");
            foreach ((string category, ExampleInfo example) in selected)
            {
                AnsiConsole.MarkupLine($"  [dim]{category}[/]  {Markup.Escape(example.Name)}");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Narrow it with --category, e.g. --category " + selected[0].Category + "[/]");
            return 1;
        }

        (string matchedCategory, ExampleInfo matchedExample) = selected[0];

        AnsiConsole.MarkupLine($"[blue]Running:[/] {Markup.Escape(matchedExample.Name)}");
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