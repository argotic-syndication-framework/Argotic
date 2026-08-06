# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] - .NET 10 Release

### Breaking Changes

- **An Atom document of the wrong shape is refused rather than silently ignored.** RFC 4287 §2
  defines two document types: a feed document rooted at `<feed>`, read by `AtomFeed`, and a
  stand-alone entry document rooted at `<entry>`, read by `AtomEntry`. Handing either type the other
  document produced a default-constructed object — empty title, empty id, `DateTime.MinValue` — and
  reported success. `SyndicationContentFormat.Atom` covered both shapes, so the format check that
  rejects every other mismatched pairing compared `Atom` against `Atom` and passed
- **`SyndicationContentFormat.AtomEntryDocument` is new, and a stand-alone entry document now reports
  it** where it previously reported `Atom`. The detector always distinguished the two roots and threw
  the answer away; keeping it is what lets the format check do its job, and lets a caller of
  `SyndicationDiscoveryUtility.SyndicationContentFormatGet` know which type to construct. Existing
  enum values are unchanged. `AtomEntry.Format` and `AtomEntryResource.Format` return the new value
- **`GenericSyndicationFeed` refuses a format it cannot represent.** `Load` was three `else if` arms
  with no final `else`, so APML, BlogML, RSD, a sitemap, an Atom Publishing document or a stand-alone
  entry document fell through to the `Loaded` event — leaving a default-constructed instance and
  announcing that a load had succeeded. It now raises `FormatException`; it abstracts over Atom feed,
  RSS and OPML documents only
- **`SyndicationResourceLoadSettings.CharacterEncoding` is now `Encoding?` and defaults to `null`.**
  It defaulted to `Encoding.UTF8` and rejected `null`, so one value had to mean both "decode as UTF-8"
  and "work it out" — and the two load paths read it in opposite directions. `Load(Stream, settings)`
  honoured it and forced UTF-8 over a document correctly declaring `iso-8859-1`; `LoadAsync` compared it
  by reference against the singleton and sniffed instead, so an equivalent `new UTF8Encoding(false)`
  behaved oppositely to a value that decodes identically. **`null` now means "determine it from the
  byte-order mark or the XML declaration", which is what a default settings object asks for, and naming
  an encoding means it is used.** A caller relying on the old behaviour — UTF-8 forced over a lying
  feed — sets the property explicitly. Not binary-breaking: reference-type nullability is metadata
- **`SyndicationResourceLoadSettings.Timeout` is now `TimeSpan?`.** It still defaults to 100 seconds;
  `null` means no deadline of the library's own, leaving the caller's own `CancellationToken` as the
  only bound. `TimeSpan.Zero` is not a way to spell that — it cancels immediately. **Binary-breaking**:
  `get_Timeout` returns a different CLR type
- **Conditional GET now answers with the status code, not a heuristic.** `ConditionalGetAsync`
  compared the response's `Last-Modified` against the one it sent — a comparison only reachable once
  the origin had already declined to send a 304 — and then fell back to asking whether the response
  "looked like it had content" via `Content-Length` and `Content-Type`. A chunked 200 with no
  `Content-Type` answered no to both, so its body was downloaded, discarded, and reported to the caller
  as unmodified. The fallback was also gated on `== HttpStatusCode.OK` while the success check admits
  every 2xx, so a 203 or 206 was discarded without reaching it. **304 now means unmodified and any other
  success means modified.** A caller who depended on the old behaviour was depending on data loss
- **`SyndicationRequestOptions.ApplyTo` throws on a value it cannot send.** Every setter was a `Try`
  whose result was discarded, so a malformed `Accept` produced a request with no `Accept` header and
  the caller learned about it, if at all, as a `406`. A relative `Referer` was worse than dropped:
  `Uri.TryCreate(..., Absolute, ...)` accepts `/relative/path` on Linux and yields
  `file:///relative/path`, which was then sent to a remote origin. `Accept`, `User-Agent` and `Referer`
  now raise `FormatException`; only absolute `http`/`https` referers are accepted. An empty `Referer`
  still means "do not send one"
- **Downloaded resources are size-capped by default.** Every `LoadAsync(Uri, ...)` and
  `CreateAsync(Uri, ...)` now refuses a response larger than its format allows: 8 MiB for a feed,
  64 MiB for a sitemap or a BlogML export. Nothing bounded them before. A caller who genuinely needs a
  larger document sets `SyndicationResourceLoadSettings.MaxResponseContentLength`, or
  `SyndicationResourceLoadSettings.Unbounded` to restore the previous behaviour. Note that
  `RetrievalLimit` does not bound the download — it is applied after parsing — so taming a large
  archive feed with it now requires raising the cap as well
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

- **Discovery reads only what it needs.** `IsPingbackEnabledAsync`, `LocatePingbackNotificationServerAsync`,
  `UriExistsAsync`, `SourceReferencesTargetAsync`, `LocateDiscoverableSyndicationEndpointsAsync`,
  `LocateTrackbackNotificationServersAsync` and both `SyndicationContentFormatGetAsync` overloads no
  longer buffer the whole response before deciding what to do with it. A pingback answered from an
  `X-Pingback` header now reads no body at all; format detection reads the first 64 KiB rather than the
  whole feed; and every one of them is bounded at 2 MiB. **One consequence to be aware of**: a document
  whose prolog exceeds 64 KiB now reports `SyndicationContentFormat.None` — documented as "unable to
  determine" — where before it was parsed in full off the socket
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

- **Conditional loading**: `SyndicationResourceReader.LoadIfModifiedAsync<TResource>` fetches and parses
  a resource only when the origin reports it has changed, which is the workload a polling consumer
  actually has. Both outcomes are successes — a 304 arrives as a result rather than as the
  `HttpRequestException` the same request produces through `LoadAsync`. `ConditionalLoadResult<T>.WasModified`
  carries `[MemberNotNullWhen]`, so testing it reaches `Resource` with no null check
- **`SyndicationValidators`**: the `Last-Modified`/`ETag` pair as one type, since they always travel
  together and must be sent back exactly as received. `SyndicationValidators.None` makes a request
  unconditional. Deliberately not folded into `SyndicationRequestOptions`: every path that consumes
  those funnels through a fetch calling `EnsureSuccessStatusCode`, and 304 is not a success code
- **A 304 now returns the validators it carried.** An origin may rotate its `ETag` on a not-modified
  response, and the previous behaviour discarded everything about a 304 — so a polling caller re-sent
  the validator they started with indefinitely, revalidating against a value the origin had stopped
  recognising. `ConditionalGetResult` also reports `StatusCode` for a 304, where it previously reported
  `null`
- **Conditional GET accepts request options**: `ConditionalGetAsync(Uri, SyndicationValidators, HttpClient,
  SyndicationRequestOptions?, CancellationToken)`. It was the one fetch in the library that could not be
  given an `Accept` header or a custom `User-Agent`, because it built its request by hand
- **Conditional GET no longer downloads before it decides.** It completed on content, so the whole body
  was buffered before `ConditionalGetResult` existed — unbounded and eager both, and `ContentLength`
  reported the buffered length rather than what the origin declared. It now completes on headers, so
  the type streams, as its shape always suggested
- **`IHttpClientFactory` registration**: `AddTrackbackClient` and `AddXmlRpcClient` register typed
  clients, so a container-resolved client uses the handler the container built rather than the
  process-wide singleton. `AddArgoticSyndicationClient` registers a named client for the resource
  types — resolve it with `IHttpClientFactory.CreateClient(ArgoticHttpClients.Syndication)` and pass it
  to any `LoadAsync` or `CreateAsync` overload taking an `HttpClient`
- **`SyndicationEncodingUtility.DefaultRequestTimeout`** is now public. The shared `HttpClient` is
  deliberately `Timeout.InfiniteTimeSpan` — every deadline comes from a `CancellationTokenSource` — so
  there was previously no value a caller could read to discover what deadline applied to them
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

- **Auto-discovery with relative links**: an endpoint discovered from `href="/feed.xml"` — the commonest
  form such a link takes — is now resolved against the address the page was retrieved from, following
  redirects. It was stored exactly as written, so `DiscoverableSyndicationEndpoint.CreateNavigatorAsync`
  handed a relative URI to `HttpClient` and threw. A new
  `ExtractDiscoverableSyndicationEndpoints(string, Uri)` overload exposes the same resolution to callers
  parsing markup themselves; the single-argument overload is unchanged
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