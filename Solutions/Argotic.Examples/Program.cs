using Argotic.Examples.Commands;
using Spectre.Console.Cli;

CommandApp app = new();

app.Configure(config =>
{
    config.SetApplicationName("argotic-examples");

    config.AddCommand<ListCommand>("list")
        .WithDescription("List all available examples")
        .WithExample("list")
        .WithExample("list", "--category", "Rss");

    config.AddCommand<RunCommand>("run")
        .WithDescription("Run a specific example by name")
        .WithExample("run", "Rss Feed - Class")
        .WithExample("run", "AtomFeed");

    config.AddCommand<RunAllCommand>("run-all")
        .WithDescription("Run all examples sequentially")
        .WithExample("run-all")
        .WithExample("run-all", "--category", "Atom")
        .WithExample("run-all", "--skip-network")
        .WithExample("run-all", "--json");

    config.AddCommand<InteractiveCommand>("interactive")
        .WithDescription("Launch the interactive example browser");
});

// Default to interactive mode if no arguments provided
if (args.Length == 0)
{
    return await app.RunAsync(["interactive"]).ConfigureAwait(false);
}

return await app.RunAsync(args).ConfigureAwait(false);