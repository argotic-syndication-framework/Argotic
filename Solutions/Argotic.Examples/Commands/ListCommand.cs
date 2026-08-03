using Spectre.Console;
using Spectre.Console.Cli;

namespace Argotic.Examples.Commands;

/// <summary>
/// Lists all available examples.
/// </summary>
internal sealed class ListCommand : Command<ListSettings>
{
    protected override int Execute(CommandContext context, ListSettings settings, CancellationToken cancellationToken)
    {
        ExampleRegistry.Initialize();

        IReadOnlyList<ExampleCategory> categories = ExampleRegistry.Categories;
        if (!string.IsNullOrEmpty(settings.Category))
        {
            ExampleCategory? matchedCategory = categories.FirstOrDefault(c =>
                c.Key.Equals(settings.Category, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals(settings.Category, StringComparison.OrdinalIgnoreCase));

            if (matchedCategory == null)
            {
                AnsiConsole.MarkupLine($"[red]Unknown category:[/] {settings.Category}");
                AnsiConsole.MarkupLine("[dim]Available categories:[/]");
                foreach (ExampleCategory cat in categories)
                {
                    AnsiConsole.MarkupLine($"  [blue]{cat.Key}[/] - {cat.Name}");
                }
                return 1;
            }

            categories = [matchedCategory];
        }

        Table table = new();
        table.AddColumn("Category");
        table.AddColumn("Example Name");
        table.AddColumn("Async");

        int totalCount = 0;

        foreach (ExampleCategory category in categories)
        {
            IReadOnlyList<ExampleInfo> examples = ExampleRegistry.GetExamples(category.Key);
            foreach (ExampleInfo example in examples)
            {
                table.AddRow(
                    $"[blue]{category.Key}[/]",
                    example.Name,
                    example.IsAsync ? "[cyan]Yes[/]" : "[dim]No[/]");
                totalCount++;
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Total: {totalCount} examples[/]");

        return 0;
    }
}