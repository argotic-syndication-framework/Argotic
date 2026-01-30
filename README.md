[![Build Status](https://github.com/argotic-syndication-framework/Argotic/actions/workflows/build.yml/badge.svg)](https://github.com/argotic-syndication-framework/Argotic/actions/workflows/build.yml)
[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/argotic-syndication-framework/argotic/master/LICENSE)
[![IMM](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/total?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/total?cache=false)


The Argotic Syndication Framework was originally created by Brian Kuhn in 2007. Argotic is one of the most powerful and extensible web content syndication frameworks available to .NET developers, supporting [RSS](http://www.rssboard.org/rss-specification), [Atom](http://www.atomenabled.org/developers/syndication/atom-format-spec.php), [OPML](http://www.opml.org/spec2), [APML](http://apml.pbwiki.com), [BlogML](http://blogml.org), [RSD](http://cyber.law.harvard.edu/blogs/gems/tech/rsd.html), and [Sitemap](https://www.sitemaps.org/protocol.html). 

The project had become dormant, but has been brought back to life by [endjin](https://endjin.com), as Argotic is used to produce the [Azure Weekly Newsletter](https://azureweekly.info), [Microsoft Fabric Weekly Newsletter](https://fabricweekly.info) and [Power BI Weekly Newsletter](https://powerbiweekly.info).

The project has been updated to .NET 10 with comprehensive C# modernization, including collection expressions, pattern matching, file-scoped namespaces, and nullable reference types. The codebase has been refactored to follow modern .NET idioms and best practices. There are *many* breaking changes.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

> **Note:** This release drops support for .NET Standard 2.0/2.1, .NET 8, and .NET 9. If you need to target earlier frameworks, please use a previous version of the packages.

## Installation

Install the NuGet packages:

```bash
# Core syndication library
dotnet add package Argotic.Core

# Common utilities and interfaces
dotnet add package Argotic.Common

# Syndication extensions (iTunes, Dublin Core, Yahoo Media, etc.)
dotnet add package Argotic.Extensions
```

Or via the Package Manager Console:

```powershell
Install-Package Argotic.Core
Install-Package Argotic.Common
Install-Package Argotic.Extensions
```

See the [wiki](https://argotic-syndication-framework.github.io/Argotic) for detailed documentation and the [CHANGELOG](CHANGELOG.md) for a complete list of changes in this release.

## Quick Start Examples

### Reading an RSS Feed

```csharp
using Argotic.Syndication;

// Load feed from a URL
RssFeed feed = await RssFeed.CreateAsync(new Uri("https://endjin.com/rss.xml"));

// Access feed metadata
Console.WriteLine($"Feed: {feed.Channel.Title}");
Console.WriteLine($"Description: {feed.Channel.Description}");

// Iterate through items
foreach (RssItem item in feed.Channel.Items)
{
    Console.WriteLine($"- {item.Title}");
    Console.WriteLine($"  Link: {item.Link}");
    Console.WriteLine($"  Published: {item.PublicationDate}");
}
```

### Creating an RSS Feed

```csharp
using Argotic.Syndication;

// Create a new feed
RssFeed feed = new()
{
    Channel =
    {
        Title = "endjin blog",
        Link = new Uri("https://endjin.com/blog"),
        Description = "Latest posts from the endjin blog"
    }
};

// Add items
feed.Channel.Items.Add(new RssItem
{
    Title = "Polars Workloads on Microsoft Fabric",
    Link = new Uri("https://endjin.com/blog/2026/01/polars-workloads-on-microsoft-fabric"),
    Description = "A technical guide demonstrating how to leverage Polars within Microsoft Fabric for efficient data transformation.",
    PublicationDate = DateTime.UtcNow
});

feed.Channel.Items.Add(new RssItem
{
    Title = "Practical Polars: Code Examples for Everyday Data Tasks",
    Link = new Uri("https://endjin.com/blog/2026/01/practical-polars-code-examples-everyday-data-tasks"),
    Description = "A hands-on guide featuring concrete code examples for common data workflows using Python Polars.",
    PublicationDate = DateTime.UtcNow.AddDays(-1)
});

// Save to a stream or file
using FileStream stream = File.Create("feed.xml");
feed.Save(stream);
```

### Creating a Sitemap

```csharp
using Argotic.Syndication;

// Create a new sitemap
Sitemap sitemap = new();

sitemap.Urls.Add(new SitemapUrl
{
    Location = new Uri("https://endjin.com/"),
    LastModified = DateTime.UtcNow,
    ChangeFrequency = SitemapChangeFrequency.Daily,
    Priority = 1.0m
});

sitemap.Urls.Add(new SitemapUrl
{
    Location = new Uri("https://endjin.com/what-we-do"),
    ChangeFrequency = SitemapChangeFrequency.Monthly,
    Priority = 0.8m
});

sitemap.Urls.Add(new SitemapUrl
{
    Location = new Uri("https://endjin.com/who-we-are"),
    ChangeFrequency = SitemapChangeFrequency.Monthly,
    Priority = 0.8m
});

// Save the sitemap
using FileStream stream = File.Create("sitemap.xml");
sitemap.Save(stream);
```

## Building

This project uses [ZeroFailed](https://github.com/zerofailed/ZeroFailed), a PowerShell-based build orchestration framework built on [InvokeBuild](https://github.com/nightroman/Invoke-Build).

### Prerequisites

- PowerShell 7.0 or later
- .NET 10 SDK

### Build Commands

```powershell
# Full build (compile, test, package)
./build.ps1

# Clean build (removes bin/obj folders first)
./build.ps1 -Clean

# Run specific tasks
./build.ps1 -Tasks Build      # Compile only
./build.ps1 -Tasks Test       # Run tests with code coverage
./build.ps1 -Tasks Package    # Create NuGet packages

# Release build
./build.ps1 -Configuration Release

# Verbose output
./build.ps1 -LogLevel detailed
```

### Build Output

- **NuGet packages**: `_packages/`
- **Code coverage reports**: `_codeCoverage/`
- **Test results**: `Solutions/Argotic.Extensions.Tests/TestResults/`

### Direct .NET Commands

You can also use standard .NET CLI commands:

```bash
# Build the solution
dotnet build Solutions/Argotic.slnx

# Run tests
dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj

# Run tests with coverage
dotnet test --project Solutions/Argotic.Extensions.Tests/Argotic.Extensions.Tests.csproj --coverage --coverage-output-format cobertura
```

## Examples

The `Argotic.Examples` project is an interactive CLI that demonstrates all features of the Argotic framework with 66 runnable examples.

### Quick Start

Use the PowerShell script to build and run all examples:

```powershell
# Run all examples
./run-all-examples.ps1

# Run examples in a specific category
./run-all-examples.ps1 -Category Rss

# Skip examples requiring network access
./run-all-examples.ps1 -SkipNetwork

# Output results as JSON (for CI/CD)
./run-all-examples.ps1 -JsonOutput
```

### Running Examples with .NET CLI

```bash
# Interactive mode (default) - browse and run examples via menu
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj

# List all available examples
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- list

# List examples in a specific category
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- list --category Rss

# Run a specific example
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- run "Rss Feed - Class"

# Run all examples (batch mode)
dotnet run --project Solutions/Argotic.Examples/Argotic.Examples.csproj -- run-all
```

### Example Categories

| Category       | Description                                                         |
|----------------|---------------------------------------------------------------------|
| **Atom**       | Atom 1.0 feed and entry creation, loading, and serialization        |
| **RSS**        | RSS 2.0 feed creation, loading, and serialization                   |
| **OPML**       | Outline Processor Markup Language documents                         |
| **APML**       | Attention Profiling Markup Language                                 |
| **BlogML**     | Blog content import/export format                                   |
| **RSD**        | Really Simple Discovery                                             |
| **Sitemap**    | Sitemap 0.9 with video, image, and news extensions                  |
| **Extensions** | 20+ syndication extensions (iTunes, Dublin Core, Yahoo Media, etc.) |
| **Network**    | Trackback and XML-RPC client usage                                  |
| **Generic**    | Format-agnostic syndication feed handling                           |

*ar·got·ic* (_ahr-got-ik_)
A specialized idiomatic vocabulary peculiar to a particular class or group of people.

## Licenses

[![GitHub license](https://img.shields.io/badge/License-Apache%202-blue.svg)](https://raw.githubusercontent.com/argotic-syndication-framework/argotic/master/LICENSE)

Argotic Syndication Framework is available under the Apache 2.0 open source license.

For any licensing questions, please email [&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;](&#109;&#97;&#105;&#108;&#116;&#111;&#58;&#108;&#105;&#99;&#101;&#110;&#115;&#105;&#110;&#103;&#64;&#101;&#110;&#100;&#106;&#105;&#110;&#46;&#99;&#111;&#109;)

## Project Sponsor

This project is sponsored by [endjin](https://endjin.com), a UK based, fully-remote, Consultancy which specializes in Data & Analytics, AI, and Cloud Native App Dev.

We help small teams achieve big things.

For more information about our products and services, or for commercial support of this project, please [contact us](https://endjin.com/contact-us). 

We produce three free weekly newsletters; [Azure Weekly](https://azureweekly.info) for all things about the Microsoft Azure Platform,  [Fabric Weekly](https://fabricweekly.info) for all things about the Microsoft Fabric and [Power BI Weekly](https://powerbiweekly.info).

Keep up with everything that's going on at endjin via our [blog](https://blogs.endjin.com/), watch our talks and tutorials on our [YouTube Channel](https://www.youtube.com/endjin), follow us on [Bluesky](https://bsky.app/profile/endjin.com), or [LinkedIn](https://www.linkedin.com/company/1671851/).

Our other Open Source projects can be found on [our website](https://endjin.com/open-source).

## Code of conduct

This project has adopted a code of conduct adapted from the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behaviour in our community. This code of conduct has been [adopted by many other projects](http://contributor-covenant.org/adopters/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;](&#109;&#097;&#105;&#108;&#116;&#111;:&#104;&#101;&#108;&#108;&#111;&#064;&#101;&#110;&#100;&#106;&#105;&#110;&#046;&#099;&#111;&#109;) with any additional questions or comments.

## IP Maturity Model (IMM)

The [IP Maturity Model](https://github.com/endjin/Endjin.Ip.Maturity.Matrix) is endjin's IP quality framework; it defines a [configurable set of rules](https://github.com/endjin/Endjin.Ip.Maturity.Matrix.RuleDefinitions), which are committed into the [root of a repo](imm.yaml), and a [Azure Function HttpTrigger](https://github.com/endjin/Endjin.Ip.Maturity.Matrix/tree/master/Solutions/Endjin.Ip.Maturity.Matrix.Host) which can evaluate the ruleset, and render an svg badge for display in repo's `readme.md`.

This approach is based on our 15+ years experience of delivering complex, high performance, bleeding-edge projects, and due diligence assessments of 3rd party systems. For detailed information about the ruleset see the [IP Maturity Model repo](https://github.com/endjin/Endjin.Ip.Maturity.Matrix).

## IMM for Argotic

[![Shared Engineering Standards](https://endimmfuncdev.azurewebsites.net/api/imm/github/endjin/Stacker/rule/74e29f9b-6dca-4161-8fdd-b468a1eb185d?nocache=true)](https://endimmfuncdev.azurewebsites.net/api/imm/github/endjin/Stacker/rule/74e29f9b-6dca-4161-8fdd-b468a1eb185d?cache=false)

[![Coding Standards](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f6f6490f-9493-4dc3-a674-15584fa951d8?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f6f6490f-9493-4dc3-a674-15584fa951d8?cache=false)

[![Executable Specifications](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/bb49fb94-6ab5-40c3-a6da-dfd2e9bc4b00?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/bb49fb94-6ab5-40c3-a6da-dfd2e9bc4b00?cache=false)

[![Code Coverage](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/0449cadc-0078-4094-b019-520d75cc6cbb?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/0449cadc-0078-4094-b019-520d75cc6cbb?cache=false)

[![Benchmarks](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/64ed80dc-d354-45a9-9a56-c32437306afa?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/64ed80dc-d354-45a9-9a56-c32437306afa?cache=false)

[![Reference Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/2a7fc206-d578-41b0-85f6-a28b6b0fec5f?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/2a7fc206-d578-41b0-85f6-a28b6b0fec5f?cache=false)

[![Design & Implementation Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f026d5a2-ce1a-4e04-af15-5a35792b164b?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/f026d5a2-ce1a-4e04-af15-5a35792b164b?cache=false)

[![How-to Documentation](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/145f2e3d-bb05-4ced-989b-7fb218fc6705?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/145f2e3d-bb05-4ced-989b-7fb218fc6705?cache=false)

[![Date of Last IP Review](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/da4ed776-0365-4d8a-a297-c4e91a14d646?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/da4ed776-0365-4d8a-a297-c4e91a14d646?cache=false)

[![Framework Version](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/6c0402b3-f0e3-4bd7-83fe-04bb6dca7924?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/6c0402b3-f0e3-4bd7-83fe-04bb6dca7924?cache=false)

[![Associated Work Items](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/79b8ff50-7378-4f29-b07c-bcd80746bfd4?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/79b8ff50-7378-4f29-b07c-bcd80746bfd4?cache=false)

[![Source Code Availability](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/30e1b40b-b27d-4631-b38d-3172426593ca?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/30e1b40b-b27d-4631-b38d-3172426593ca?cache=false)

[![License](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/d96b5bdc-62c7-47b6-bcc4-de31127c08b7?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/d96b5bdc-62c7-47b6-bcc4-de31127c08b7?cache=false)

[![Production Use](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/87ee2c3e-b17a-4939-b969-2c9c034d05d7?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/87ee2c3e-b17a-4939-b969-2c9c034d05d7?cache=false)

[![Insights](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/71a02488-2dc9-4d25-94fa-8c2346169f8b?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/71a02488-2dc9-4d25-94fa-8c2346169f8b?cache=false)

[![Packaging](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/547fd9f5-9caf-449f-82d9-4fba9e7ce13a?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/547fd9f5-9caf-449f-82d9-4fba9e7ce13a?cache=false)

[![Deployment](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/edea4593-d2dd-485b-bc1b-aaaf18f098f9?cache=false)](https://endimmfuncdev.azurewebsites.net/api/imm/github/argotic-syndication-framework/argotic/rule/edea4593-d2dd-485b-bc1b-aaaf18f098f9?cache=false)