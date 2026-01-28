using System.ComponentModel;
using Spectre.Console.Cli;

namespace Argotic.Examples.Commands;

/// <summary>
/// Base settings shared by all commands.
/// </summary>
public class CommandSettings : Spectre.Console.Cli.CommandSettings
{
}

/// <summary>
/// Settings for the list command.
/// </summary>
public sealed class ListSettings : CommandSettings
{
    [Description("Filter examples by category (e.g., Rss, Atom, Extensions)")]
    [CommandOption("-c|--category")]
    public string? Category { get; init; }
}

/// <summary>
/// Settings for the run command.
/// </summary>
public sealed class RunSettings : CommandSettings
{
    [Description("The name of the example to run (use 'list' to see available examples)")]
    [CommandArgument(0, "<name>")]
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Settings for the run-all command.
/// </summary>
public sealed class RunAllSettings : CommandSettings
{
    [Description("Filter examples by category (e.g., Rss, Atom, Extensions)")]
    [CommandOption("-c|--category")]
    public string? Category { get; init; }

    [Description("Skip examples that require network access")]
    [CommandOption("--skip-network")]
    public bool SkipNetwork { get; init; }

    [Description("Output results in JSON format")]
    [CommandOption("--json")]
    public bool JsonOutput { get; init; }

    [Description("Continue running after failures")]
    [CommandOption("--continue-on-error")]
    [DefaultValue(true)]
    public bool ContinueOnError { get; init; } = true;
}