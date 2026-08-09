#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run all Argotic examples and report results.

.DESCRIPTION
    This script builds the Argotic.Examples project and runs all examples,
    reporting the results in a summary format.

.PARAMETER Category
    Optional category to filter examples (e.g., Rss, Atom, Extensions).

.PARAMETER SkipNetwork
    Skip examples that require network access.

.PARAMETER JsonOutput
    Output results in JSON format.

.PARAMETER NoColor
    Disable colored output.

.EXAMPLE
    ./run-all-examples.ps1
    Runs all examples with default settings.

.EXAMPLE
    ./run-all-examples.ps1 -Category Rss
    Runs only RSS examples.

.EXAMPLE
    ./run-all-examples.ps1 -SkipNetwork -JsonOutput
    Runs non-network examples and outputs JSON.
#>

[CmdletBinding()]
param(
    [string]$Category,
    [switch]$SkipNetwork,
    [switch]$JsonOutput,
    [switch]$NoColor
)

$ErrorActionPreference = "Continue"
$ProjectPath = Join-Path $PSScriptRoot "Solutions/Argotic.Examples/Argotic.Examples.csproj"

function Write-ColorHost {
    param(
        [string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::White
    )
    if ($NoColor) {
        Write-Host $Message
    } else {
        Write-Host $Message -ForegroundColor $Color
    }
}

# Build the project
Write-ColorHost "Building Argotic.Examples..." -Color Cyan
$buildResult = dotnet build $ProjectPath --verbosity quiet 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-ColorHost "Build failed!" -Color Red
    Write-Host $buildResult
    exit 1
}

Write-ColorHost "Build successful." -Color Green
Write-Host ""

# Prepare arguments
$runArgs = @("run", "--project", $ProjectPath, "--", "run-all")

if ($Category) {
    $runArgs += @("--category", $Category)
}

if ($SkipNetwork) {
    $runArgs += "--skip-network"
}

if ($JsonOutput) {
    $runArgs += "--json"
}

# Run examples
Write-ColorHost "Running examples..." -Color Cyan
Write-Host ""

& dotnet @runArgs

$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-ColorHost "All examples completed successfully!" -Color Green
} else {
    Write-ColorHost "Some examples failed." -Color Yellow
}

exit $exitCode
