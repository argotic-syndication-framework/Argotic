<#
This example demonstrates a software build process using the 'ZeroFailed.Build.DotNet' extension
to provide the features needed when building a .NET solutions.
#>

$zerofailedExtensions = @(
    @{
        # References the extension from its GitHub repository. If not already installed, use latest version from 'main' will be downloaded.
        Name = "ZeroFailed.Build.DotNet"
        GitRepository = "https://github.com/zerofailed/ZeroFailed.Build.DotNet"
        GitRef = "main"
    }
    @{
        Name = "ZeroFailed.Build.GitHub"
        GitRepository = "https://github.com/zerofailed/ZeroFailed.Build.GitHub"
        GitRef = "main"
    }
)

# Load the tasks and process
. ZeroFailed.tasks -ZfPath $here/.zf

#
# Build process configuration
#
#
# Build process control options
#
$SkipInit = $false
$SkipVersion = $false
$SkipBuild = $false
$CleanBuild = $Clean
$SkipTest = $false
$SkipTestReport = $false
$SkipAnalysis = $false
$SkipPackage = $false
$SkipPublish = $false

$SolutionToBuild = (Resolve-Path (Join-Path $here "./Solutions/Argotic.slnx")).Path
$ProjectsToPublish = @()
$NugetPublishSource = property ZF_NUGET_PUBLISH_SOURCE "$here/_local-nuget-feed"
$IncludeAssembliesInCodeCoverage = "Argotic.*"
$ExcludeAssembliesInCodeCoverage = "Argotic.*.Tests*"
$ExcludeFilesInCodeCoverage = @(
    '*.g.cs'
    '_\**'  # required for CI build where sourcelink is enabled
)

# The SiteMonitoring tests fetch endjin.com's published sitemaps and validate them against Google's
# schemas. They check a website, not this library, so a content problem there must not fail a build
# here - nobody can fix it from this repository, and a red build nobody can fix is a red build
# everybody learns to ignore.
#
# This is not hypothetical. The category was added precisely so this could not happen, and then the
# pipeline was never told about it: runs 31250504215 and 31261773537 both failed on the single test
# APublishedSitemap_ConformsToTheSchemasItDeclares, because 17 of the 100 video:description elements
# endjin.com serves exceed the 2,048 characters sitemap-video-1.1.xsd permits.
#
# Everything else in the Integration tier stays in, because it validates what Argotic writes and what
# Argotic reads. Unreachable services report Inconclusive rather than failing, so third-party downtime
# does not break the build either.
#
# Counts, from 'dotnet test --solution … --list-tests' on 2026-08-08: 3,552 total, 3,549 here,
# 23 Integration, 3 SiteMonitoring.
$AdditionalTestArgs = @("--filter", "TestCategory!=SiteMonitoring")

task . FullBuild

#
# Build Process Extensibility Points - uncomment and implement as required
#

# task RunFirst {}
# task PreInit {}
# task PostInit {}
# task PreVersion {}
# task PostVersion {}
# task PreBuild {}

# The single-file samples in Solutions/Samples are .NET 10 file-based apps. They are not in
# Argotic.slnx and no csproj compiles them, so all four of the other quality gates are blind to
# them: a sample that stopped compiling against a changed public API would stay broken until
# somebody happened to run it by hand.
#
# ./build.ps1 is the entire CI surface -- .github/workflows/build.yml has two steps and neither is
# a dotnet command -- so a runner nothing calls from here is a runner CI never calls. That is the
# shape CLAUDE.md warns about: a guard no runner honours is a comment, not a control, and this
# repository has already shipped one (the SiteMonitoring category, designed to keep endjin.com's
# content out of the build and then never wired into the pipeline it was written for).
#
# PostBuild rather than PreTest: build.process.ps1 orders Build as Init,Version,PreBuild,BuildCore,
# PostBuild, and compile.tasks.ps1 attaches BuildSolution -After BuildCore. So by the time this
# runs the solution is already compiled in $Configuration, which is the precondition the samples
# need -- they are run with -p:BuildProjectReferences=false so that 8 of them in parallel do not
# collide over the same bin directories. It also fails before the ~60-second test run rather than
# after it.
#
# Measured cold at throttle 8: about 20 seconds for 21 samples.
task PostBuild {
    exec { pwsh -NoProfile -File "$here/run-samples.ps1" -Configuration $Configuration -SkipBuild }
}
# task PreTest {}
# task PostTest {}
# task PreTestReport {}
# task PostTestReport {}
# task PreAnalysis {}
# task PostAnalysis {}
# task PrePackage {}
# task PostPackage {}
# task PrePublish {}
# task PostPublish {}
# task RunLast {}