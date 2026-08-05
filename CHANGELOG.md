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

### Changed

Behaviour changes that fix no defect and break no documented contract, but that a caller could notice.

- **The shared `HttpClient` negotiates Brotli and keeps no cookies.** It advertised only gzip and
  deflate, declining the smallest encoding most origins offer; and `SocketsHttpHandler.UseCookies`
  defaults to `true`, so a `Set-Cookie` from any origin was replayed on the next request to that host.
  On a process-wide singleton that is per-domain session state accumulating for the lifetime of the
  application, with no API to inspect or clear it
- **`SyndicationEncodingUtility.CreateSafeNavigator(Stream)` streams rather than buffering.** It read
  the whole document into a `byte[]`, decoded that to a string, sanitised it into a second string, and
  parsed the result; it now reads a bounded head, detects the encoding from it, and decodes the
  remainder as it goes. Allocation for a 706 KiB feed falls 68%. Two visible consequences: an empty
  stream produces `XmlException` ("Root element is missing.") rather than `ArgumentException` naming
  `content` — the parameter of a private helper three calls down, which the caller never supplied — and
  the stream is consumed lazily, so a parse failure part-way through leaves it part-way through rather
  than drained. This overload still does not close the stream
- **`SyndicationEncodingUtility.CreateSafeNavigator(Stream, Encoding)` no longer closes the supplied
  stream.** It wrapped it in a `StreamReader` it owned and disposed, which closed the stream as a side
  effect — while the single-argument overload, which reads identically at the call site, did not.
  Nothing documented the difference. Callers who relied on it to dispose their stream must now do so
  themselves. An empty stream produces `XmlException` rather than `ArgumentException` naming `xml`, and
  a byte-order mark still takes precedence over the supplied encoding, as it always did
- **`SyndicationEncodingUtility.CreateSafeNavigator(TextReader)` filters as it reads.** It previously
  drained the reader to a string, sanitised that into a second string, and parsed the result; it now
  drops invalid characters incrementally, so a document is no longer held twice. Two visible
  consequences: an empty reader produces `XmlException` ("Root element is missing.") rather than
  `ArgumentException` naming `xml` — a parameter that overload does not have and the caller never
  supplied — and the reader is consumed lazily, so a parse failure part-way through leaves it part-way
  through rather than drained. The reader is still never disposed by this method.

### New Features

- **Response size caps**: `SyndicationResourceLoadSettings.MaxResponseContentLength` bounds how much
  of an HTTP response a load will accept, counted in decompressed bytes. `null` — the default —
  means the loading type's format default rather than no limit, so a caller who constructs a settings
  object for an unrelated reason does not silently lose the allowance their document type is entitled
  to. `SyndicationResourceLoadSettings.Unbounded` asks for no limit, and
  `SyndicationContentLengthLimits` publishes the per-format defaults: 8 MiB for a feed, 64 MiB for a
  sitemap or a whole-site export, 2 MiB for a discovery fetch
- **`SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(SocketsHttpHandler)`**: applies the handler
  settings the shared client uses, so a caller building their own `HttpClient` or configuring one
  through `IHttpClientFactory` gets the same pipeline without having to know what it consists of
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

- **`AtomEntryResource.CreateAsync(Uri, SyndicationResourceLoadSettings, CancellationToken)`** is new and
  returns an `AtomEntryResource`. `AtomEntryResource.CreateAsync(uri, settings)` previously bound the
  inherited `AtomEntry.CreateAsync` and returned an `AtomEntry`, silently dropping the Atom Publishing
  members the caller asked for by naming the derived type. The `CancellationToken` parameter of
  `CreateAsync(Uri, CancellationToken)` no longer has a default value, which is what keeps
  `CreateAsync(uri)` unambiguous; `CreateAsync(uri, cancellationToken)` still compiles
- **Six redundant `AtomEntryResource` members removed** - `Load(IXPathNavigable)`, `Load(Stream)`,
  `Load(Stream, SyndicationResourceLoadSettings)`, `Load(XmlReader)`,
  `Load(XmlReader, SyndicationResourceLoadSettings)` and `LoadAsync(Uri, CancellationToken)`. Source is
  unaffected: the inherited `AtomEntry` members have identical signatures and now dispatch correctly
  through the virtual funnel. Binary-breaking - a caller compiled against 3001.0.0 must be recompiled
- **Publishing state on a constructed entry**: `AtomEntryResource.Save(Stream)`, `Save(XmlWriter)` and
  `CreateNavigator()` now write `app:edited` and `app:control/app:draft` from `EditedOn` and `IsDraft`.
  Only `Save(XmlWriter, SyndicationResourceSaveSettings)` did, so an entry built in code and saved by
  any other route lost its entire Atom Publishing state, silently. An entry that had been *loaded* was
  unaffected, because loading places the extension objects directly into `Extensions` - which is why a
  load-then-save round trip could not detect this
- **Publishing state through a base or interface reference**: an `AtomEntryResource` held as
  `ISyndicationResource` or `AtomEntry` now populates `EditedOn` and `IsDraft` on every load overload,
  synchronous and asynchronous. `AtomEntry.Load(IXPathNavigable, SyndicationResourceLoadSettings)`,
  `LoadAsync(Uri, HttpClient, ...)` and `Save(XmlWriter, SyndicationResourceSaveSettings)` are now
  `virtual`, and the corresponding `AtomEntryResource` members `override` rather than shadow them. The
  interface map was fixed at `AtomEntry`, so a caller not using the concrete type got the publishing
  members silently dropped
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

- **`SyndicationEncodingUtility.GetXmlEncoding(Stream)`** — source- and binary-breaking. It read the
  *entire* stream to find forty bytes and left it consumed, so a stream-shaped sniff could not promise
  non-consumption and could not be used twice. `GetXmlEncoding(byte[])` still accepts a whole document
  and is still unbounded; `CreateSafeNavigator(Stream)` now sniffs a bounded head internally
- **Internal `SyndicationEncodingUtility.GetStreamBytes`** — the load path no longer buffers a whole
  document before parsing it, so nothing called it. Binary-breaking only for the three assemblies
  holding an `InternalsVisibleTo` grant, all of which are in this repository
- **`SyndicationEncodingUtility.EncodeInvalidXmlHexadecimalCharacters(string)`** — source- and
  binary-breaking. It had no caller anywhere in the library and could not have had a working one: its
  pattern relied on `\xD800` meaning U+D800, where .NET regex reads `\x` as exactly two hex digits, so
  `\xD800` denoted the range `'0'`–`'ß'`. Nearly every letter matched, and each match was passed to
  `Convert.ToUInt32(value, 16)` — which threw `FormatException` on `"Hello"` and silently rewrote
  `"abc"` to `"101112"`. Use `RemoveInvalidXmlHexadecimalCharacters` instead
- `Guard.cs` utility class
- Legacy configuration provider classes
- `WebRequestOptions` class
- Message sent event args classes