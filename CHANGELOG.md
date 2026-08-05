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
- **HTTP stack**: Retrieval moved from `WebRequest`/`HttpWebRequest` to `HttpClient`. Failed requests now raise
  `HttpRequestException` rather than `WebException`, and cancellation surfaces as `OperationCanceledException`
  rather than a timeout-flavoured `WebException`
- **Binary serialization**: Removed `[Serializable]` from all 179 types, and the three `[NonSerialized]`
  field annotations that accompanied them. `Type.IsSerializable` now returns `false` for every Argotic
  type. Nothing in the library implemented `ISerializable` or used `SerializationInfo`, and the only
  runtime consumer of the attribute — `BinaryFormatter` — was removed from the platform in .NET 9
  (`SYSLIB0011`/`SYSLIB0050`). `IXmlSerializable`, `XmlSerializer` and the `Save`/`Load` XML round-trip
  are unaffected: they never depended on `[Serializable]`

### New Features

- **Sitemap 0.9**: Added support for Sitemap 0.9 protocol
- **Google Video Sitemap 1.1**: Improved specification implementation
- **IComparisonOperators**: New interface with extension-based comparison operators (`<`, `<=`, `>`, `>=`)
- **Dependency Injection**: Added `ServiceCollectionExtensions` for DI registration
- **Options Pattern**: New `TrackbackClientOptions` and `XmlRpcClientOptions` classes
- **Examples CLI**: Interactive/non-interactive CLI demonstrating API usage
- **HashCodeUtility**: New helper producing hash code components that agree with the framework's
  case-insensitive comparison semantics

### Fixed

#### Feed retrieval

- **Character encoding**: The encoding declared in a feed's XML declaration is honoured again when loading from a
  `Stream` or `Uri`. Content was being decoded as UTF-8 regardless of the declaration, corrupting non-UTF-8 feeds
  with replacement characters and failing outright on BOM-less UTF-16
- **HTTP error responses**: 4xx/5xx responses now raise `HttpRequestException` instead of having their body parsed
  as feed content. A well-formed XML error page was previously loaded as an empty or incorrect feed with no error
- **Request time-outs**: Requests issued through the shared `HttpClient` are bounded at 100 seconds. The shared
  client is configured with an infinite time-out, so discovery calls such as `UriExistsAsync`,
  `ConditionalGetAsync` and `LocateDiscoverableSyndicationEndpointsAsync` could hang indefinitely against a host
  that accepted a connection but never responded
- **Default load time-out**: `SyndicationResourceLoadSettings.Timeout` now defaults to 100 seconds, matching the
  effective time-out of the previous `HttpWebRequest` pipeline, rather than 15 seconds
- **User-Agent**: A `SyndicationRequestOptions.UserAgent` value now replaces the framework User-Agent instead of
  being appended to it, matching the documented "or null to use the default" semantics
- **Conditional GET**: `ConditionalGetAsync` interprets an `Unspecified`-kind `lastModified` as UTC for both the
  `If-Modified-Since` header it sends and the `Last-Modified` value it compares against. The two previously
  disagreed by the host's UTC offset, so updated content could be reported as unmodified, or unchanged feeds
  re-downloaded on every poll

#### XML parsing

- **Internal DTD subsets**: Feeds that declare entities in an internal DTD subset load again. DTD declarations were
  being discarded, so any entity they defined became undeclared and the whole feed failed to parse

#### Comparison and equality

- **`CompareTo`**: Member comparisons are no longer combined with bitwise OR. Combining a positive and a negative
  result produced a negative value in both directions, so `a < b` and `b < a` could both be true and `List<T>.Sort`
  produced arbitrary order or threw `InvalidOperationException`. Ordering is now decided by the first member that
  differs, across all affected types
- **`GetHashCode`**: Hash codes now agree with the case-insensitive comparisons that `Equals` is defined in terms
  of. Instances that compared as equal could return different hash codes, so `HashSet<T>.Contains` missed them and
  a `Dictionary<TKey, TValue>` could hold two equal keys
- **`ComparisonUtility.CompareSequence`**: Returns the first non-zero element comparison instead of the bitwise OR
  of every element comparison

#### Sitemaps

- **Extension scoping**: `image`, `news`, `video` and `xhtml` extension data binds to the `<url>` element it
  appears under. Every URL previously received every matching element in the document, and re-saving wrote the
  duplicated data under each `<url>` as well as directly under `<urlset>`
- **Date-only `<lastmod>`**: Values such as `2024-01-15` keep their stated calendar day and are exposed as UTC.
  They were converted to the host's local zone, moving the modification date onto the neighbouring day
- **`RetrievalLimit`**: `Sitemap` and `SitemapIndex` honour `SyndicationResourceLoadSettings.RetrievalLimit` on
  their public load paths, which previously read every entry regardless of the configured limit

#### Atom Publishing

- **`AtomEntryResource` load overloads**: `Load(IXPathNavigable)`, `Load(Stream)` and `Load(XmlReader)` populate
  `EditedOn` and `IsDraft`. Only two overloads were wrapped, so the others silently dropped the publishing state
- **`app:draft`**: The draft flag is read from the `app:control` element that `WriteTo` emits, so an entry's draft
  state survives a load/save round trip
- **Duplicate namespace declarations**: Extensions that share an XML prefix - such as the Atom Publishing control
  and edited extensions - emit a single declaration. Saving an entry that used both produced a duplicate
  `xmlns:app` attribute and threw `XmlException`

#### Dates

- **RFC-822 offsets**: Dates carrying a lowercase `gmt` offset, such as `Mon, 01 Jan 2024 12:00:00 gmt+02:00`,
  parse correctly. A case-sensitivity mismatch truncated the first three characters of the value, so the date
  failed to parse and the item's publication date was silently dropped

#### Trackback

- **RDF autodiscovery**: Embedded `<rdf:RDF>` discovery blocks are matched again. The pattern used a non-verbatim
  string in which `\b` was a literal backspace character rather than a word boundary, so autodiscovery returned no
  endpoints for any page and `IsTrackbackEnabledAsync` always returned `false`

### Security Fixes

- **XXE Prevention**: Resolves CA5372. XML parsing refuses to resolve external entities (`XmlResolver` is `null`)
  and bounds entity expansion via `MaxCharactersFromEntities`. The internal DTD subset is parsed rather than
  ignored, so feeds that declare their own entities continue to load without reopening the external-entity vector

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

- Microsoft.Extensions.Options 10.0.10
- Microsoft.Extensions.DependencyInjection.Abstractions 10.0.10
- Spectre.Console 0.57.2 (Examples project)
- MSTest.Sdk 4.3.3
- Shouldly 4.3.0

### Removed

- `Guard.cs` utility class
- Legacy configuration provider classes
- `WebRequestOptions` class
- Message sent event args classes