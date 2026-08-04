using Spectre.Console;

namespace Argotic.Examples;

/// <summary>
/// Main runner for the Argotic Examples CLI application.
/// Provides an interactive menu system for exploring and running examples.
/// </summary>
internal sealed class ExampleRunner
{
    private const string ExitChoice = "[grey]Exit[/]";
    private const string BackChoice = "[grey]<< Back[/]";

    /// <summary>
    /// Runs the interactive example browser.
    /// </summary>
    public static async Task RunAsync()
    {
        ExampleRegistry.Initialize();

        AnsiConsole.Write(
            new FigletText("Argotic")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]Web Content Syndication Framework[/]");
        AnsiConsole.MarkupLine("[dim]RSS, Atom, OPML, APML, RSD, BlogML[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            ExampleCategory? category = ShowMainMenu();

            if (category == null)
            {
                AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
                break;
            }

            await ShowCategoryMenuAsync(category).ConfigureAwait(false);
        }
    }

    private static ExampleCategory? ShowMainMenu()
    {
        List<string> choices =
        [
            .. ExampleRegistry.Categories
                        .Select(c => $"[blue]{c.Name}[/] - {c.Description}")
,
            ExitChoice,
        ];

        string selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Select a category:[/]")
                .PageSize(15)
                .HighlightStyle(Style.Parse("blue bold"))
                .AddChoices(choices));

        if (selection == ExitChoice)
        {
            return null;
        }

        int index = choices.IndexOf(selection);
        return ExampleRegistry.Categories[index];
    }

    private static async Task ShowCategoryMenuAsync(ExampleCategory category)
    {
        while (true)
        {
            IReadOnlyDictionary<string, IReadOnlyList<ExampleInfo>> examplesByClass = ExampleRegistry.GetExamplesByClass(category.Key);

            if (examplesByClass.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]No examples found for {category.Name}[/]");
                return;
            }

            // Build menu with class groupings
            List<string> choices = [BackChoice];
            List<KeyValuePair<string, IReadOnlyList<ExampleInfo>>> orderedClasses = [.. examplesByClass.OrderBy(kvp => kvp.Key)];

            foreach ((string classNameItem, IReadOnlyList<ExampleInfo> examplesItem) in orderedClasses)
            {
                choices.Add($"[blue]{classNameItem}[/] ({examplesItem.Count} examples)");
            }

            string selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[green]{category.Name} Examples:[/]")
                    .PageSize(15)
                    .HighlightStyle(Style.Parse("blue bold"))
                    .AddChoices(choices));

            if (selection == BackChoice)
            {
                return;
            }

            // Extract class name from selection
            int classIndex = choices.IndexOf(selection) - 1; // -1 for BackChoice
            KeyValuePair<string, IReadOnlyList<ExampleInfo>> selectedClass = orderedClasses[classIndex];
            string className = selectedClass.Key;
            IReadOnlyList<ExampleInfo> classExamples = selectedClass.Value;

            await ShowClassExamplesAsync(className, classExamples).ConfigureAwait(false);
        }
    }

    private static async Task ShowClassExamplesAsync(string className, IReadOnlyList<ExampleInfo> examples)
    {
        while (true)
        {
            List<string> choices =
            [
                BackChoice,
                .. examples.Select(e =>
                    e.IsAsync
                        ? $"[cyan]{e.Name}[/] [dim](async)[/]"
                        : $"[cyan]{e.Name}[/]"),
            ];

            string selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[green]{className} Methods:[/]")
                    .PageSize(15)
                    .HighlightStyle(Style.Parse("cyan bold"))
                    .AddChoices(choices));

            if (selection == BackChoice)
            {
                return;
            }

            int exampleIndex = choices.IndexOf(selection) - 1; // -1 for BackChoice
            ExampleInfo example = examples[exampleIndex];

            await RunExampleAsync(example).ConfigureAwait(false);
        }
    }

    private static async Task RunExampleAsync(ExampleInfo example)
    {
        AnsiConsole.WriteLine();

        Panel panel = new(example.Description)
        {
            Header = new PanelHeader($" {example.Name} ", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("blue")
        };
        AnsiConsole.Write(panel);

        AnsiConsole.WriteLine();

        bool confirmed = await AnsiConsole.ConfirmAsync("Run this example?", defaultValue: true).ConfigureAwait(false);

        if (!confirmed)
        {
            return;
        }

        AnsiConsole.WriteLine();

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync($"Running {example.Name}...", async ctx =>
                {
                    await example.RunAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);

            AnsiConsole.MarkupLine("[green]Example completed successfully![/]");
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"[red]Network error:[/] {Markup.Escape(ex.Message)}");
            AnsiConsole.MarkupLine("[dim]This example requires network access to external feeds.[/]");
        }
        catch (TaskCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Request timed out.[/]");
            AnsiConsole.MarkupLine("[dim]The remote server did not respond in time.[/]");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[yellow]File not found:[/] {Markup.Escape(ex.FileName ?? ex.Message)}");
            AnsiConsole.MarkupLine("[dim]This example requires a local file that doesn't exist.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");

            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[dim]Inner: {Markup.Escape(ex.InnerException.Message)}[/]");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
        AnsiConsole.WriteLine();
    }
}