# Changelog

All notable changes to this project are in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Entries that start with **BREAKING** change behaviour or an API that callers use. Read those first.

## [Unreleased]

The next release is 4000.0.2.

### Added

- **`SyndicationResourceSaveSettings.WriteXsiSchemaLocation`.** This opt-in setting makes `Sitemap.Save`
  and `SitemapIndex.Save` write the `xmlns:xsi` declaration and the `xsi:schemaLocation` attribute on
  the root element. The default value is `false`, and the default output does not change. A `urlset`
  root pairs the core namespace with `sitemap.xsd`. A `sitemapindex` root pairs the same namespace with
  `siteindex.xsd`. One more pair follows for each declared extension namespace with a published schema:
  news, image and video. A namespace with no known schema location is skipped, such as the `xhtml`
  namespace of the hreflang extension. Consumers that validate a sitemap use the attribute as a
  validation hint. (Issue 177.)

### Fixed

- **Sitemap extension dates write in UTC, with the `Z` designator.** `news:publication_date`,
  `video:publication_date` and `video:expiration_date` normalise their value to UTC before the format
  step. A `Local` kind converts to the same instant. An `Unspecified` kind is a claim of UTC and keeps
  its wall clock. Before, the format specifier `zzz` stamped the offset of the host machine on a `Local`
  or `Unspecified` wall clock. The load path assumes UTC, and thus an `Unspecified` value written on a
  non-UTC host came back as a different instant. A `Utc` kind was safe on each host, but it wrote the
  non-canonical form `+00:00`. The output is now identical on every machine. (Issue 177.)

## [4000.0.1] - 2026-08-10

The .NET 10 release. The version changes from 3001.0.0 to 4000.0.1. The tag `4000.0.0` exists, but its
NuGet publication failed in CI. `4000.0.1` is the shipped package, and it has no library change on top
of `4000.0.0`.

### Added

#### Syndication extensions

- **Podcasting 2.0** (`https://podcastindex.org/namespace/1.0`). This is a new extension family. A survey
  of 1,934 live feeds from the Apple directory found the namespace in 1,200 of them. The extension reads
  `locked`, `guid`, `medium`, `podping`, `txt`, `funding`, `person`, `season`, `episode`, `transcript`,
  `chapters` and `license`. Before this release, a load and then a save removed all of these elements.
  The `txt` element with `purpose="applepodcastsverify"` is the most important one. Publishers send the
  Apple ownership token in two forms. Approximately one half use `podcast:txt`, and the other half use
  `itunes:applepodcastsverify`. A library that reads only one form loses the other half.
- **GeoRSS** (OGC 17-002r1). This is a new extension family. It reads and writes both encodings, Simple
  and GML. It supports the four geometries: point, line, box and polygon. It also supports `elev`,
  `floor`, `radius`, `featurename`, `featuretypetag` and `relationshiptag`.
- **Google sitemap extensions.** `SitemapNewsExtension`, `SitemapImageExtension` and
  `SitemapHreflangExtension` are new. They join the video extension. The hreflang extension is different
  from the other three. Its annotations are `xhtml:link` elements in the XHTML namespace, and thus it
  binds the `xhtml` prefix. The value `x-default` is legal, but it is not a language tag.

#### Feed retrieval

- **`SyndicationResourceReader.LoadIfModifiedAsync<TResource>`.** This method gets and parses a resource
  only when the origin reports a change. This is the usual task for a consumer that polls. Both results
  are successes. A 304 response is a result, and not the `HttpRequestException` that `LoadAsync` gives
  for the same request. `ConditionalLoadResult<T>.WasModified` has `[MemberNotNullWhen]`. Thus a test of
  that property gives access to `Resource` without a null check.
- **`SyndicationValidators`.** This type holds the `Last-Modified` and `ETag` pair. The two values always
  travel together, and a caller must send them back without a change. `SyndicationValidators.None` makes
  a request unconditional. This type is not part of `SyndicationRequestOptions`. Each path that uses
  `SyndicationRequestOptions` calls `EnsureSuccessStatusCode`, and 304 is not a success code.
- **A 304 response gives back its validators.** An origin can change its `ETag` on a not-modified
  response. Before this release, the library discarded all data from a 304. A caller that polls thus
  sent the first validator again and again, and revalidated against a value the origin no longer knew.
  `ConditionalGetResult` also reports `StatusCode` for a 304. Before, it reported `null`.
- **`ConditionalGetAsync(Uri, SyndicationValidators, HttpClient, SyndicationRequestOptions?,
  CancellationToken)`.** This overload accepts request options. This method was the only fetch in the
  library that made its request directly. Thus it was the only one that could not accept an `Accept`
  header or a custom `User-Agent`.
- **`SyndicationEncodingUtility.DefaultRequestTimeout` is public.** The shared `HttpClient` uses
  `Timeout.InfiniteTimeSpan`, and each deadline comes from a `CancellationTokenSource`. Before this
  release, a caller could not read the deadline that applied to them.
- **`SyndicationResourceLoadSettings.MaxResponseContentLength`.** This property bounds the quantity of an
  HTTP response that a load accepts. The count is in decompressed bytes. The default value is `null`,
  which means the format default for the type, and not "no limit". Thus a caller who makes a settings
  object for a different reason keeps the allowance for their document type.
  `SyndicationResourceLoadSettings.Unbounded` asks for no limit. `SyndicationContentLengthLimits` gives
  the default for each format:
  - 8 MiB for a feed
  - 64 MiB for a sitemap or a full-site export
  - 2 MiB for a discovery fetch
- **`SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(SocketsHttpHandler)`.** This method applies
  the handler settings that the shared client uses. A caller who makes their own `HttpClient`, or who
  configures one with `IHttpClientFactory`, gets the same pipeline. The caller does not have to know the
  contents of that pipeline.

#### Other

- **`AddTrackbackClient` and `AddXmlRpcClient`** register typed clients for `IHttpClientFactory`. A client
  from the container uses the handler that the container made, and not the process-wide singleton.
  `AddArgoticSyndicationClient` registers a named client for the resource types. Get it with
  `IHttpClientFactory.CreateClient(ArgoticHttpClients.Syndication)`, then give it to any `LoadAsync` or
  `CreateAsync` overload that accepts an `HttpClient`.
- **Sitemap 0.9 support.**
- **`IComparisonOperators`.** This new interface gives the comparison operators `<`, `<=`, `>` and `>=`
  as extension members.
- **`ServiceCollectionExtensions`** for dependency injection registration.
- **`TrackbackClientOptions` and `XmlRpcClientOptions`** for the options pattern.
- **Examples CLI.** This is an interactive and non-interactive command-line application. It shows how to
  use the API.
- **`HashCodeUtility`.** This helper makes hash code components that agree with the case-insensitive
  comparison rules of the framework.

### Changed

#### Namespaces and types

- **BREAKING. `Argotic.Syndication.Specialized` no longer exists.** `ApmlDocument`, `BlogMLDocument` and
  `RsdDocument` are now in `Argotic.Syndication`. All other formats were already in that namespace.
  Change the `using` statement. No type and no member changed.
- **BREAKING. `SyndicationResourceAdapter` is sealed, and it is no longer the base class of the
  adapters.** It was the dispatcher and the base class at the same time. Thus each of the 14 version
  adapters had a public `Fill(ISyndicationResource, SyndicationContentFormat)` method that no code
  called. The navigator and the settings moved to the new `SyndicationResourceAdapterBase`. The 14
  adapters are sealed.
- **BREAKING. The 15 adapter constructors accept a non-nullable `SyndicationResourceLoadSettings`.** The
  parameter was nullable, but each constructor threw an exception for a null value. Thus the annotation
  was the opposite of the behaviour. Nullable annotations are part of the API.
- **BREAKING. `Atom03SyndicationResourceAdapter.CreateNamespaceManager` is private.** It was `protected
  static` on a type that is now sealed.
- **BREAKING. All 119 comparable types implement `IComparable<T>`** in place of the non-generic
  `IComparable`.
- **BREAKING. The target framework is `net10.0` only.** Support for .NET Standard 2.0, .NET Standard 2.1,
  .NET 8 and .NET 9 stopped.

#### Load behaviour

- **BREAKING. The library refuses a declared version that no adapter reads.** Before this release,
  `<rss version="0.93">` passed the format check, matched no version arm, and gave back an empty feed
  and a `Loaded` event. There was no diagnostic. RSS, OPML, APML and RSD now throw `FormatException`. The
  message gives the version in the document and the versions that the library reads. Atom and the Atom
  Publishing Protocol are different, because their specifications define no version attribute. These two
  formats route by namespace. RFC 4287 defines a version attribute only on `atom:generator`, and §6.3
  does not permit an error for foreign markup.
- **BREAKING. Version routing accepts Build and Revision components.** Before this release,
  `<rss version="2.0.1">` reached no adapter, because `System.Version` equality compares all four
  components. `new Version("2.0")` is not equal to `new Version("2.0.1")`. The document now routes to the
  2.0 adapter. Patch versions of the other formats also route correctly.
- **BREAKING. A resource of the wrong runtime type causes `ArgumentException`.** Before, seven casts in
  the dispatcher threw `InvalidCastException`, which no documentation recorded. The Atom arm and the Atom
  Publishing arm used pattern matching with no `else`, and thus did nothing at all. The message now gives
  the type of the resource and the type that reads the format.
- **BREAKING. A format that the library detects but does not read causes `FormatException`.** `NewsML`,
  `MicroSummaryGenerator` and `OpenSearchDescription` fell through the outer switch of the dispatcher.
  The library gave back an empty resource and reported success.
- **BREAKING. `Sitemap` and `SitemapIndex` check the format of the document.** Both types used a private
  walk with no format check. Thus `Sitemap.Load` of an RSS document was a success with zero URLs. At the
  same time, the XML documentation promised a `FormatException` on 22 members that could not throw one.
  Three more cases change. An un-namespaced `urlset`, a `urlset` below the document root, and a navigator
  on an element instead of the document node loaded before. The library refuses all three now.
- **BREAKING. The library refuses an Atom document of the wrong shape.** RFC 4287 §2 defines two document
  types. A feed document has a `<feed>` root, and `AtomFeed` reads it. A stand-alone entry document has an
  `<entry>` root, and `AtomEntry` reads it. Before this release, each type accepted the other document and
  gave back a default object with an empty title, an empty id and `DateTime.MinValue`. It reported success.
  `SyndicationContentFormat.Atom` covered both shapes. Thus the format check compared `Atom` against
  `Atom`, and it passed.
- **BREAKING. `SyndicationContentFormat.AtomEntryDocument` is new.** A stand-alone entry document reports
  this value. Before, it reported `Atom`. The detector always knew the two roots apart, but it discarded
  the answer. The new value lets the format check do its work. It also lets a caller of
  `SyndicationDiscoveryUtility.SyndicationContentFormatGet` know which type to make. The other enumeration
  values do not change. `AtomEntry.Format` and `AtomEntryResource.Format` give back the new value.
- **BREAKING. `GenericSyndicationFeed` refuses a format that it cannot represent.** `Load` had three
  `else if` arms and no final `else`. Thus APML, BlogML, RSD, a sitemap, an Atom Publishing document and
  a stand-alone entry document reached the `Loaded` event. The caller received a default object and a
  report of success. The method now throws `FormatException`. This type represents an Atom feed, an RSS
  feed and an OPML document only.
- **BREAKING. `SyndicationResourceLoadSettings.CharacterEncoding` is `Encoding?`, and the default is
  `null`.** The default was `Encoding.UTF8`, and the property refused `null`. Thus one value had to mean
  both "decode as UTF-8" and "detect the encoding". The two load paths read the property in opposite
  directions. `Load(Stream, settings)` obeyed it, and forced UTF-8 on a document that correctly declared
  `iso-8859-1`. `LoadAsync` compared it by reference against the singleton, and detected the encoding
  instead. Thus an equivalent `new UTF8Encoding(false)` behaved differently from a value that decodes the
  same way. `null` now means "detect the encoding from the byte-order mark or the XML declaration", which
  is what a default settings object asks for. The library uses a named encoding. A caller who needs the old
  behaviour sets the property. This change is not binary-breaking, because nullability of a reference
  type is metadata.
- **BREAKING. `SyndicationResourceLoadSettings.Timeout` is `TimeSpan?`.** The default is still 100
  seconds. `null` means that the library applies no deadline, and the `CancellationToken` of the caller
  is the only bound. Do not use `TimeSpan.Zero` for this purpose, because it cancels immediately. This
  change is binary-breaking, because `get_Timeout` gives back a different CLR type.
- **BREAKING. Conditional GET uses the status code.** Before, `ConditionalGetAsync` compared the
  `Last-Modified` of the response against the one that it sent. The code could reach that comparison only
  after the origin refused to send a 304. It then asked whether the response "looked like it had content"
  from `Content-Length` and `Content-Type`. A chunked 200 response with no `Content-Type` failed both
  tests. Thus the library downloaded the body, discarded it, and told the caller that the resource was
  unmodified. The fallback also tested for `HttpStatusCode.OK` only, but the success check accepts each
  2xx code. Thus the library discarded a 203 or a 206 response before it reached the fallback. A 304 now
  means unmodified, and each other success code means modified. A caller who depended on the old
  behaviour depended on data loss.
- **BREAKING. `SyndicationRequestOptions.ApplyTo` throws an exception for a value that it cannot send.**
  Each setter was a `Try` method, and the code discarded the result. Thus a malformed `Accept` value made
  a request with no `Accept` header. The caller learned about the problem only as a 406 response, or not
  at all. A relative `Referer` value was worse. `Uri.TryCreate(..., Absolute, ...)` accepts
  `/relative/path` on Linux and makes `file:///relative/path`, and the library sent that value to a
  remote origin. `Accept`, `User-Agent` and `Referer` now throw `FormatException`. The library accepts
  only an absolute `http` or `https` referer. An empty `Referer` still means "do not send one".
- **BREAKING. A download has a size limit by default.** Each `LoadAsync(Uri, ...)` and
  `CreateAsync(Uri, ...)` method refuses a response that is larger than the limit for its format. The
  limits are 8 MiB for a feed, and 64 MiB for a sitemap or a BlogML export. Before this release, there
  was no limit. A caller who needs a larger document sets
  `SyndicationResourceLoadSettings.MaxResponseContentLength`, or
  `SyndicationResourceLoadSettings.Unbounded` for the old behaviour. Note that `RetrievalLimit` does not
  bound the download, because the library applies it after the parse. To read a large archive feed, also
  increase the size limit.
- **BREAKING. The HTTP stack uses `HttpClient`.** It used `WebRequest` and `HttpWebRequest`. A failed
  request now throws `HttpRequestException` in place of `WebException`. Cancellation now throws
  `OperationCanceledException` in place of a `WebException` with a timeout status.

#### Other behaviour

The changes in this group fix no defect and break no documented contract. A caller can see them.

- **Discovery reads only the data that it needs.** `IsPingbackEnabledAsync`,
  `LocatePingbackNotificationServerAsync`, `UriExistsAsync`, `SourceReferencesTargetAsync`,
  `LocateDiscoverableSyndicationEndpointsAsync`, `LocateTrackbackNotificationServersAsync` and the two
  `SyndicationContentFormatGetAsync` overloads no longer buffer the full response first. A pingback
  answer from an `X-Pingback` header reads no body. Format detection reads the first 64 KiB, and not the
  full feed. Each of these methods has a 2 MiB limit. Note one result of this change. A document with a
  prolog longer than 64 KiB now reports `SyndicationContentFormat.None`. The documentation defines that
  value as "unable to determine". Before, the library parsed the full document from the socket.
- **The shared `HttpClient` accepts Brotli, and it keeps no cookies.** It offered gzip and deflate only,
  and thus refused the smallest encoding that most origins offer. `SocketsHttpHandler.UseCookies` has a
  default of `true`. Thus the client sent a `Set-Cookie` value from any origin again on the next request
  to that host. On a process-wide singleton, that state stays for the life of the application, and there
  is no API to read it or to clear it.
- **`SyndicationEncodingUtility.CreateSafeNavigator(Stream)` streams the document.** It read the full
  document into a `byte[]`. It then decoded that array to a string, cleaned that string into a second
  string, and parsed the result. It now reads a bounded head, detects the encoding from the head, and
  decodes the remainder as it goes. For a 706 KiB feed, allocation decreases by 68%. There are two
  results. An empty stream now causes `XmlException` with the message "Root element is missing.". Before,
  it caused `ArgumentException` for the parameter `content`. That parameter belongs to a private helper
  method, and the caller never supplied it. The stream is also read lazily. Thus a parse failure leaves
  the stream at that point, and not at the end. This overload still does not close the stream.
- **`SyndicationEncodingUtility.CreateSafeNavigator(Stream, Encoding)` does not close the stream.** It
  put the stream into a `StreamReader` that it owned and disposed, and thus closed the stream. The
  single-argument overload did not do this, and no documentation recorded the difference. A caller who
  depended on this behaviour must now dispose the stream. An empty stream causes `XmlException` in place
  of `ArgumentException` for the parameter `xml`. A byte-order mark still has precedence over the supplied
  encoding, as before.
- **`SyndicationEncodingUtility.CreateSafeNavigator(TextReader)` filters as it reads.** It read the full
  reader into a string, cleaned that string into a second string, and then parsed the result. It now
  removes invalid characters as it reads, and thus does not hold the document two times. There are two
  results. An empty reader now causes `XmlException` with the message "Root element is missing.".
  Before, it caused `ArgumentException` for the parameter `xml`. This overload does not have that
  parameter, and the caller never supplied it. The reader is also read lazily. Thus a parse failure leaves the reader at that point. This
  method still does not dispose the reader.

#### Quality and tooling

- **Conformance validation is a test gate, in two tiers.** The offline tier validates the output of the
  library, and the sample corpus, against embedded sitemaps.org and APML schemas. The tier with
  `[TestCategory("Integration")]` uses live services. It gets Google's sitemap extension schemas at test
  time. Each of those files has an "All Rights Reserved" notice, and thus the repository holds no copy. It also uses the W3C Feed Validator, which is the standard checker for RSS and Atom,
  because .NET cannot validate either format against a schema. An integration test does not pass unless
  it reaches its service. An unavailable service gives an inconclusive result, and only a document that
  the service refuses causes a failure.
- **The documentation does not refer to files that the repository does not hold.** XML documentation
  comments in `Argotic.Common` and `Argotic.Extensions` referred to an engineering log outside version
  control. Those comments are in the NuGet packages. Thus a consumer saw a path that does not exist for
  them. The text keeps the facts, but not the references.
- **`ARCHITECTURE.md`** is new. It records the load pipeline, format dispatch, extension discovery, the
  network layer and the test strategy, with diagrams. It also records the changes of the .NET 10
  modernisation, and the parts that did not change.
- **C# 7 to C# 14 features.** The code now uses:
  - file-scoped namespaces
  - collection expressions
  - pattern matching
  - target-typed `new` expressions
  - range operators and `nameof` expressions
  - auto-properties and expression-bodied members
- **This release resolves more than 200 code analysis warnings.** These include:
  - CA1854 and CA1864, for dictionaries
  - CA1868, for `Collection.Remove`
  - CA2000, for `IDisposable`
  - CA1307 and CA1867, for string methods
  - CA2251, for string comparison
- **All comparable types implement `IEquatable<T>`.**
- **The test project uses Microsoft Testing Platform (MTP) with Shouldly assertions.** The number of
  tests and the code coverage both increased.

#### Dependencies

- Microsoft.Extensions.Options 10.0.10
- Microsoft.Extensions.DependencyInjection.Abstractions 10.0.10
- Spectre.Console 0.57.2, in the Examples project
- MSTest.Sdk 4.3.3
- Shouldly 4.3.0

### Removed

- **BREAKING. `SyndicationEncodingUtility.GetXmlEncoding(Stream)`.** This removal breaks source and
  binary compatibility. The method read the full stream to find forty bytes, and left the stream at the
  end. Thus a sniff of a stream could not promise to leave the stream unread, and a caller could not use
  the stream again. `GetXmlEncoding(byte[])` still accepts a full document, and it still has no limit.
  `CreateSafeNavigator(Stream)` now reads a bounded head internally.
- **BREAKING. `SyndicationEncodingUtility.EncodeInvalidXmlHexadecimalCharacters(string)`.** This removal
  breaks source and binary compatibility. No code in the library called it, and no code could have used
  it correctly. Its pattern used `\xD800` for U+D800, but .NET regular expressions read `\x` and then
  exactly two hexadecimal digits. Thus `\xD800` was the range `'0'` to `'ß'`. Almost every letter matched.
  The method gave each match to `Convert.ToUInt32(value, 16)`, which threw `FormatException` for
  `"Hello"` and changed `"abc"` to `"101112"`. Use `RemoveInvalidXmlHexadecimalCharacters` in place of it.
- **BREAKING. Binary serialization support.** This release removes the `[Serializable]` attribute from
  all 179 types, and the three `[NonSerialized]` field annotations with it. `Type.IsSerializable` now gives
  `false` for each Argotic type. No type in the library implemented `ISerializable` or used
  `SerializationInfo`. `BinaryFormatter` was the only runtime consumer of the attribute, and .NET 9
  removed it from the platform (`SYSLIB0011` and `SYSLIB0050`). `IXmlSerializable`, `XmlSerializer` and
  the `Save` and `Load` XML round trip do not change, because they never used `[Serializable]`.
- **BREAKING. The legacy `System.Configuration` classes**: `PrivilegedConfigurationManager`,
  `SyndicationResourceProvider` and its related classes, `TrackbackClientSection`, `XmlRpcClientSection`
  and `WebRequestOptions`.
- **BREAKING. The `Guard` utility class.** Use `ArgumentNullException.ThrowIfNull` and the equivalent
  methods of the framework.
- **BREAKING. `TrackbackMessageSentEventArgs` and `XmlRpcMessageSentEventArgs`.**
- **Internal `SyndicationEncodingUtility.GetStreamBytes`.** The load path no longer buffers a full
  document before the parse, and thus no code called this method. This removal breaks binary
  compatibility only for the three assemblies with an `InternalsVisibleTo` grant. All three are in this
  repository.

### Fixed

#### Writing

- **The library writes the elements of a video sitemap in the order that the schema requires.**
  `sitemap-video-1.1.xsd` declares an `xsd:sequence`, and thus the order of the child elements is part of
  the contract. `SitemapVideo` wrote `tag` last, and wrote `uploader`, `platform` and `restriction` after
  `live`. The schema puts `tag` directly after `publication_date`, `restriction` after `family_friendly`,
  and `platform` and `live` near the end. The position of `content_segment_loc` was also incorrect. Thus
  Google's schema refused each video sitemap from this library that had a tag, a restriction, a platform
  or an uploader. No round-trip test could find this defect, because the reader accepts the child
  elements in any order. A save and then a load gave the same result in both directions.

#### Feed retrieval

- **Auto-discovery with a relative link.** The library resolves an endpoint from `href="/feed.xml"`
  against the address of the page, and follows redirects. This form of link is the most common one.
  Before, the library kept the value without a change. Thus
  `DiscoverableSyndicationEndpoint.CreateNavigatorAsync` gave a relative URI to `HttpClient`, and threw
  an exception. The new `ExtractDiscoverableSyndicationEndpoints(string, Uri)` overload gives the same
  resolution to a caller who parses the markup. The overload with one argument does not change.
- **Character encoding.** The library obeys the encoding in the XML declaration of a feed for a load from
  a `Stream` or a `Uri`. Before, it decoded the content as UTF-8 in all cases. Thus it damaged a feed in
  a different encoding with replacement characters, and it failed for UTF-16 with no byte-order mark.
- **HTTP error responses.** A 4xx or 5xx response throws `HttpRequestException`. Before, the library
  parsed the body as feed content. Thus a well-formed XML error page became an empty or incorrect feed,
  with no error.
- **Request timeouts.** A request through the shared `HttpClient` has a limit of 100 seconds. The shared
  client has an infinite timeout. Thus discovery methods such as `UriExistsAsync`, `ConditionalGetAsync`
  and `LocateDiscoverableSyndicationEndpointsAsync` could wait without a limit for a host that accepted
  a connection but sent no response.
- **Default load timeout.** The default of `SyndicationResourceLoadSettings.Timeout` is 100 seconds, and
  not 15 seconds. The new value agrees with the effective timeout of the previous `HttpWebRequest`
  pipeline.
- **User-Agent.** A `SyndicationRequestOptions.UserAgent` value replaces the User-Agent of the framework.
  Before, the library added the value to it. The new behaviour agrees with the documented rule, "or null
  to use the default".
- **Conditional GET.** `ConditionalGetAsync` reads a `lastModified` value of kind `Unspecified` as UTC.
  It does this for the `If-Modified-Since` header that it sends, and for the `Last-Modified` value that
  it compares. The two disagreed by the UTC offset of the host. Thus the library could report new content
  as unmodified, or download an unchanged feed on each poll.

#### XML parsing

- **Internal DTD subsets.** A feed that declares entities in an internal DTD subset loads correctly. The
  library discarded the DTD declarations. Thus each entity became undeclared, and the parse of the full
  feed failed.

#### Comparison and equality

- **`CompareTo`.** The library no longer combines member comparisons with a bitwise OR. That operation
  gave a negative result in both directions for a positive and a negative input. Thus `a < b` and
  `b < a` could both be true, and `List<T>.Sort` gave an arbitrary order or threw
  `InvalidOperationException`. The first member that differs now decides the order, in each affected
  type.
- **`GetHashCode`.** Hash codes agree with the case-insensitive comparisons that define `Equals`. Two
  equal instances could give different hash codes. Thus `HashSet<T>.Contains` did not find them, and a
  `Dictionary<TKey, TValue>` could hold two equal keys.
- **`ComparisonUtility.CompareSequence`.** This method gives the first non-zero element comparison, and
  not the bitwise OR of each element comparison.

#### Sitemaps

- **Extension scope.** The `image`, `news`, `video` and `xhtml` extension data binds to the `<url>`
  element that holds it. Before, each URL received each matching element in the document. A save then
  wrote the duplicate data under each `<url>`, and also directly under `<urlset>`.
- **A date-only `<lastmod>`.** A value such as `2024-01-15` keeps its calendar day, and the library gives
  it as UTC. Before, the library changed it to the local zone of the host, and thus moved the date to the
  adjacent day.
- **`RetrievalLimit`.** `Sitemap` and `SitemapIndex` obey
  `SyndicationResourceLoadSettings.RetrievalLimit` on their public load paths. Before, they read each
  entry and ignored the limit.

#### Atom Publishing

- **`AtomEntryResource.CreateAsync(Uri, SyndicationResourceLoadSettings, CancellationToken)`** is new,
  and it gives back an `AtomEntryResource`. Before, `AtomEntryResource.CreateAsync(uri, settings)` bound
  the inherited `AtomEntry.CreateAsync` and gave back an `AtomEntry`. Thus it discarded the Atom
  Publishing members that the caller asked for by the name of the derived type. The `CancellationToken`
  parameter of `CreateAsync(Uri, CancellationToken)` no longer has a default value, which keeps
  `CreateAsync(uri)` unambiguous. `CreateAsync(uri, cancellationToken)` still compiles.
- **This release removes six unnecessary `AtomEntryResource` members**: `Load(IXPathNavigable)`, `Load(Stream)`,
  `Load(Stream, SyndicationResourceLoadSettings)`, `Load(XmlReader)`,
  `Load(XmlReader, SyndicationResourceLoadSettings)` and `LoadAsync(Uri, CancellationToken)`. Source
  compatibility does not change, because the inherited `AtomEntry` members have the same signatures and
  now dispatch correctly through the virtual funnel. This removal breaks binary compatibility. Recompile
  a caller that was built against 3001.0.0.
- **Publishing state on a constructed entry.** `AtomEntryResource.Save(Stream)`, `Save(XmlWriter)` and
  `CreateNavigator()` write `app:edited` and `app:control/app:draft` from `EditedOn` and `IsDraft`.
  Before, only `Save(XmlWriter, SyndicationResourceSaveSettings)` did this. Thus an entry that a caller
  made in code, and then saved by a different route, lost all of its Atom Publishing state. There was no
  error. An entry from a load was correct, because a load puts the extension objects directly into
  `Extensions`. For this reason, a load and then a save could not find this defect.
- **Publishing state through a base or interface reference.** An `AtomEntryResource` that a caller holds
  as `ISyndicationResource` or as `AtomEntry` populates `EditedOn` and `IsDraft` on each load overload,
  synchronous and asynchronous. `AtomEntry.Load(IXPathNavigable, SyndicationResourceLoadSettings)`,
  `LoadAsync(Uri, HttpClient, ...)` and `Save(XmlWriter, SyndicationResourceSaveSettings)` are now
  `virtual`. The equivalent `AtomEntryResource` members use `override`, and no longer shadow them. The
  interface map pointed at `AtomEntry`. Thus a caller who did not use the concrete type lost the
  publishing members.
- **`AtomEntryResource` load overloads.** `Load(IXPathNavigable)`, `Load(Stream)` and `Load(XmlReader)`
  populate `EditedOn` and `IsDraft`. Before, only two overloads did this, and the others lost the
  publishing state.
- **`app:draft`.** The library reads the draft flag from the `app:control` element that `WriteTo` writes.
  Thus the draft state of an entry stays correct through a load and a save.
- **Duplicate namespace declarations.** Extensions that share an XML prefix, such as the Atom Publishing
  control and edited extensions, write one declaration. Before, a save of an entry that used both
  extensions made a duplicate `xmlns:app` attribute, and threw `XmlException`.

#### Dates

- **RFC 822 offsets.** A date with a lowercase `gmt` offset, such as
  `Mon, 01 Jan 2024 12:00:00 gmt+02:00`, parses correctly. A case-sensitivity error removed the first
  three characters of the value. Thus the parse failed, and the library discarded the publication date of
  the item.

#### Trackback

- **RDF autodiscovery.** The library finds an embedded `<rdf:RDF>` discovery block. The pattern used a
  non-verbatim string, in which `\b` was a backspace character and not a word boundary. Thus
  autodiscovery found no endpoints on any page, and `IsTrackbackEnabledAsync` always gave `false`.

### Security

- **XXE prevention.** This change resolves CA5372. The XML parser does not resolve external entities,
  because `XmlResolver` is `null`. It also bounds entity expansion with `MaxCharactersFromEntities`. The
  parser reads the internal DTD subset, and does not ignore it. Thus a feed that declares its own
  entities loads correctly, and the external-entity vector stays closed.

[Unreleased]: https://github.com/argotic-syndication-framework/Argotic/compare/4000.0.1...HEAD
[4000.0.1]: https://github.com/argotic-syndication-framework/Argotic/compare/3001.0.0...4000.0.1
[3001.0.0]: https://github.com/argotic-syndication-framework/Argotic/releases/tag/3001.0.0
