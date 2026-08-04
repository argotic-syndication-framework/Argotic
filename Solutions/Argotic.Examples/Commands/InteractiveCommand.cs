using Spectre.Console.Cli;

namespace Argotic.Examples.Commands;

/// <summary>
/// Launches the interactive example browser.
/// </summary>
internal sealed class InteractiveCommand : AsyncCommand
{
    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        await ExampleRunner.RunAsync().ConfigureAwait(false);
        return 0;
    }
}