#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and run every single-file sample in Solutions/Samples, and report the results.

.DESCRIPTION
    The samples are .NET 10 file-based apps. They are not in Argotic.slnx and no csproj compiles
    them, so none of the four existing quality gates can see them -- a sample that stopped
    compiling against a changed public API would sit broken indefinitely. This script is the runner
    that makes them a control rather than a comment.

    Every sample is offline by construction: samples 01-13 open no sockets, and 14-21 serve
    themselves over an HttpListener on 127.0.0.1 with an OS-assigned port. So this needs no network
    and is safe in CI.

    Three flags are load-bearing, and each was chosen against a measured failure:

    -p:TreatWarningsAsErrors=true
        Note the property form. The CLI flag `-warnaserror` is NOT usable with a file-based app --
        it stops recognising the .cs as a program and hands it to MSBuild as XML, producing
        MSB4025.

    -p:BuildProjectReferences=false
        Every sample references Argotic.Core. Without this, each concurrent `dotnet run --file`
        rebuilds the same three projects into the same bin directories and they collide: measured
        as MSB3030 ("could not copy ... .pdb ... not found") and MSB3248 ("the process cannot
        access the file ... because it is being used by another process").

    --no-restore, after a restore pass
        Restore is the other shared resource: concurrent runs collide on the referenced projects'
        obj/project.assets.json with "The file ... already exists." Restoring first and then
        running with --no-restore removes that. The restore pass cannot be skipped -- a sample that
        has never been restored fails with NETSDK1004 and a message about NuGet rather than about
        itself, which is exactly the misleading failure a new contributor would hit.

    Measured on this repository, cold (runfile cache cleared) at -ThrottleLimit 8: about 20 seconds
    for 21 samples, over three consecutive trials with no failures. Sequential is 100.

.PARAMETER Configuration
    Debug or Release. Defaults to Debug.

.PARAMETER SkipBuild
    Do not pre-build the solution. Set this when the caller has already built it -- which the
    ZeroFailed PostBuild hook has. The referenced assemblies are checked either way.

.PARAMETER ThrottleLimit
    How many samples to run at once. Defaults to 8.

.PARAMETER NoColor
    Disable coloured output.

.EXAMPLE
    ./run-samples.ps1
    Builds the solution, then runs all samples.

.EXAMPLE
    ./run-samples.ps1 -Configuration Release -SkipBuild
    Runs the samples against an existing Release build.
#>

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$SkipBuild,
    [int]$ThrottleLimit = 8,
    [switch]$NoColor
)

$ErrorActionPreference = "Continue"
$SamplesDir = Join-Path $PSScriptRoot "Solutions/Samples"
$SolutionPath = Join-Path $PSScriptRoot "Solutions/Argotic.slnx"
$ReadmePath = Join-Path $SamplesDir "README.md"

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

# Non-recursive on purpose. A subdirectory is how a future sample that genuinely needed the network
# would be kept out of this gate, and a glob that reached into one would defeat that before it was
# ever used.
$samples = @(Get-ChildItem -Path $SamplesDir -Filter "*.cs" -File | Sort-Object Name)

if ($samples.Count -eq 0) {
    Write-ColorHost "No samples found in $SamplesDir." -Color Red
    exit 1
}

# The index in README.md is hand-maintained, and a hand-maintained list falls behind its directory.
# That has already happened once here: SampleFeeds.All fell four names behind and every guard
# iterating it skipped those four silently. Same defect class, so the same control.
$drift = @()
if (Test-Path $ReadmePath) {
    $readme = Get-Content $ReadmePath -Raw
    foreach ($sample in $samples) {
        if ($readme -notmatch [regex]::Escape($sample.Name)) {
            $drift += $sample.Name
        }
    }
} else {
    $drift += "README.md is missing entirely"
}

if (-not $SkipBuild) {
    Write-ColorHost "Building $Configuration..." -Color Cyan
    $buildOutput = dotnet build $SolutionPath --configuration $Configuration --verbosity quiet 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-ColorHost "Build failed." -Color Red
        Write-Host $buildOutput
        exit 1
    }
}

# Because the samples are told not to build their project references, they will happily run against
# whatever assemblies are already on disk -- including none at all. A green run against a library
# that was never built is exactly the kind of verification that cannot fail, so check first.
$required = @("Argotic.Common", "Argotic.Core", "Argotic.Extensions")
$missing = @($required | Where-Object {
    -not (Test-Path (Join-Path $PSScriptRoot "Solutions/$_/bin/$Configuration/net10.0/$_.dll"))
})

if ($missing.Count -gt 0) {
    Write-ColorHost "Not built in $Configuration : $($missing -join ', ')" -Color Red
    Write-ColorHost "Run without -SkipBuild, or build the solution first." -Color Red
    exit 1
}

# Phase 1. Restore parallelises safely; running does not, until this has happened.
Write-ColorHost "Restoring $($samples.Count) samples..." -Color Cyan
$restoreFailures = $samples | ForEach-Object -Parallel {
    $output = dotnet restore $_.FullName 2>&1
    if ($LASTEXITCODE -ne 0) {
        [PSCustomObject]@{ Name = $_.Name; Output = ($output | Out-String) }
    }
} -ThrottleLimit $ThrottleLimit

if ($restoreFailures) {
    foreach ($failure in $restoreFailures) {
        Write-ColorHost "  restore FAILED  $($failure.Name)" -Color Red
        Write-Host $failure.Output
    }

    exit 1
}

# Phase 2.
Write-ColorHost "Running $($samples.Count) samples in $Configuration (throttle $ThrottleLimit)..." -Color Cyan
Write-Host ""

$results = $samples | ForEach-Object -Parallel {
    $started = Get-Date
    $output = dotnet run --file $_.FullName --configuration $using:Configuration --no-restore `
        -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false 2>&1
    $code = $LASTEXITCODE

    [PSCustomObject]@{
        Name     = $_.Name
        ExitCode = $code
        Seconds  = [math]::Round(((Get-Date) - $started).TotalSeconds, 1)
        Output   = ($output | Out-String)
    }
} -ThrottleLimit $ThrottleLimit | Sort-Object Name

foreach ($result in $results) {
    if ($result.ExitCode -eq 0) {
        Write-ColorHost ("  ok    {0,-34} {1,5}s" -f $result.Name, $result.Seconds) -Color Green
    } else {
        Write-ColorHost ("  FAIL  {0,-34} {1,5}s  (exit {2})" -f $result.Name, $result.Seconds, $result.ExitCode) -Color Red
        Write-Host $result.Output
    }
}

$succeeded = @($results | Where-Object { $_.ExitCode -eq 0 }).Count
$failed = @($results | Where-Object { $_.ExitCode -ne 0 }).Count

Write-Host ""
Write-ColorHost "$($samples.Count) samples, $succeeded succeeded, $failed failed" -Color ($failed -eq 0 ? "Green" : "Red")

if ($drift.Count -gt 0) {
    Write-Host ""
    Write-ColorHost "Samples missing from Solutions/Samples/README.md:" -Color Red
    foreach ($name in $drift) {
        Write-ColorHost "  $name" -Color Red
    }
}

if ($failed -gt 0 -or $drift.Count -gt 0) {
    exit 1
}

exit 0