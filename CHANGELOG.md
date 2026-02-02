# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] - .NET 10 Release

### Breaking Changes

- Update the version from 3001.0.0 to 4000.0.0
- **Target Framework**: Now targets .NET 10 only (dropped .NET Standard 2.0/2.1, .NET 8, .NET 9)
- **Configuration**: Removed legacy `System.Configuration` classes:
  - `PrivilegedConfigurationManager`
  - `SyndicationResourceProvider` and related classes
  - `TrackbackClientSection`, `XmlRpcClientSection`
  - `WebRequestOptions`
- **IComparable**: All 119 types now implement `IComparable<T>` instead of non-generic `IComparable`
- **Guard class**: Removed custom `Guard` utility; use built-in `ArgumentNullException.ThrowIfNull()` etc.
- **Event handlers removed**: `TrackbackMessageSentEventArgs`, `XmlRpcMessageSentEventArgs`

### New Features

- **Sitemap 0.9**: Added support for Sitemap 0.9 protocol
- **Google Video Sitemap 1.1**: Improved specification implementation
- **IComparisonOperators**: New interface with extension-based comparison operators (`<`, `<=`, `>`, `>=`)
- **Dependency Injection**: Added `ServiceCollectionExtensions` for DI registration
- **Options Pattern**: New `TrackbackClientOptions` and `XmlRpcClientOptions` classes
- **Examples CLI**: Interactive/non-interactive CLI demonstrating API usage

### Security Fixes

- **XXE Prevention**: Fixed CA5372 warnings - all `XPathDocument` usage now prevents XML External Entity attacks

### Code Quality

- **C# Modernization** (C# 7-14 features):
  - File-scoped namespaces
  - Collection expressions (`[]`)
  - Pattern matching
  - Target-typed new expressions
  - Nullable reference types improvements
  - Range operators
  - `nameof()` expressions
  - Auto-properties
  - Expression-bodied members
- **Analyzer Fixes**: ~200+ code analysis warnings resolved:
  - CA1854/CA1864: Dictionary optimizations
  - CA1868: Collection.Remove optimizations
  - CA2000: IDisposable fixes
  - CA1307/CA1867: String method improvements
  - CA2251: String comparison fixes
- **IEquatable<T>**: Implemented across all comparable types
- **Test Framework**: Migrated to Microsoft Testing Platform (MTP) with Shouldly assertions
  - Significantly increased the number of unit tests and code coverage.

### Dependencies

- Microsoft.Extensions.Options 10.0.2
- Microsoft.Extensions.DependencyInjection.Abstractions 10.0.2
- Spectre.Console 0.54.0 (Examples project)
- MSTest.Sdk 4.0.2
- Shouldly 4.3.0

### Removed

- `Guard.cs` utility class
- Legacy configuration provider classes
- `WebRequestOptions` class
- Message sent event args classes