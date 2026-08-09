using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides methods for extracting peer-to-peer auto-discovery and resource information from syndicated content. This class cannot be inherited.
/// </summary>
public static partial class SyndicationDiscoveryUtility
{
    /// <summary>
    /// How much of a response is read when detecting its syndication format.
    /// </summary>
    /// <remarks>
    ///     Format detection needs the document's first element. Everything after that was read only
    ///     because nothing stopped it, and on a five-megabyte feed that is five megabytes to learn one
    ///     word.
    /// </remarks>
    private const int FormatDetectionProbeLength = 64 * 1024;

    /// <summary>
    /// Creates the framework user agent string with defensive null handling.
    /// </summary>
    private static string CreateFrameworkUserAgent()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(SyndicationDiscoveryUtility));
        var version = assembly?.GetName().Version?.ToString(4) ?? "unknown";
        return $"Argotic-Syndication-Framework/{version}";
    }

    /// <summary>
    /// Gets the raw user agent string used by the framework when sending web requests.
    /// </summary>
    /// <value>
    ///     <c>Argotic-Syndication-Framework/</c> followed by this assembly's four-part version, or
    ///     <c>unknown</c> in the version's place where reflection cannot supply one — a single-file or
    ///     trimmed deployment, for instance.
    /// </value>
    /// <remarks>
    ///     Set on every request this framework makes. A caller wanting a different one supplies
    ///     <see cref="SyndicationRequestOptions.UserAgent"/>, which replaces this rather than appending
    ///     to it — some origins rate-limit or block on the agent string, and identifying as the library
    ///     rather than as the application is often the wrong side of that rule.
    /// </remarks>
    public static string FrameworkUserAgent { get; } = CreateFrameworkUserAgent();

    /// <summary>
    /// Returns the <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified format name.
    /// </summary>
    /// <param name="name">The format's alternate value — which for this enumeration is a document's root element name, such as <c>rss</c> or <c>feed</c>. Matched without regard to case.</param>
    /// <returns>The matching <see cref="SyndicationContentFormat"/>; otherwise, <see cref="SyndicationContentFormat.None"/>, which is also the answer for a <see langword="null"/> or empty name.</returns>
    public static SyndicationContentFormat SyndicationContentFormatByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, SyndicationContentFormat.None);

    /// <summary>
    /// Asynchronously returns the <see cref="SyndicationContentFormat"/> of the syndicated resource located at the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">The <see cref="Uri"/> of the syndication resource to determine syndication content format for.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="SyndicationContentFormat"/>
    ///     enumeration value indicating the format of the syndicated resource. If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<SyndicationContentFormat> SyndicationContentFormatGetAsync(
        Uri source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        // The deadline is hoisted here rather than left inside SendHttpRequestAsync(Uri, options, ct),
        // which creates a token source, calls CancelAfter and disposes it before handing the response
        // back. That was harmless while the body arrived already buffered; under headers-read the body
        // is read afterwards, with the caller's own token, so the documented 100-second bound would have
        // covered the headers and nothing else. Seven other shared-client entry points in this file
        // already have this shape.
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await SyndicationContentFormatGetAsync(source, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns the <see cref="SyndicationContentFormat"/> of the syndicated resource located at the specified <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">The <see cref="Uri"/> of the syndication resource to determine syndication content format for.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="SyndicationContentFormat"/>
    ///     enumeration value indicating the format of the syndicated resource. If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Only the first 64 KiB of the response is read, because the answer is the document's root
    ///         element name and nothing after it can change that. A document whose prolog alone exceeds
    ///         that window — the shape a DTD bomb takes — is truncated mid-declaration and reported as
    ///         <see cref="SyndicationContentFormat.None"/>, which is in contract: <c>None</c> means
    ///         "unable to determine".
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<SyndicationContentFormat> SyndicationContentFormatGetAsync(
        Uri source,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
            source, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // SyndicationContentFormatGet(Stream) builds an XmlReader and calls MoveToContent, which is
        // synchronous - and with DtdProcessing.Parse a large internal subset would make it an unbounded
        // sync-over-socket read. It needs the first element, so it gets the first 64 KiB and nothing
        // more; this used to download a five-megabyte feed to read the word "rss".
        using PooledContentBuffer head = await SyndicationEncodingUtility.ReadContentPrefixAsync(
            response, FormatDetectionProbeLength, cancellationToken).ConfigureAwait(false);
        using Stream stream = head.AsStream();

        try
        {
            return SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);
        }
        catch (System.Xml.XmlException)
        {
            // A prolog longer than the probe window truncates mid-declaration. Both overloads document
            // None as meaning "unable to determine", so this is in contract rather than a new failure -
            // and a document whose prolog exceeds 64 KiB is the DTD-bomb shape the bound exists for. The
            // synchronous overload still propagates, because its caller supplied the whole stream.
            return SyndicationContentFormat.None;
        }
    }

    /// <summary>
    /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource represented by the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that represents the XML data for the syndicated resource.</param>
    /// <returns>
    ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
    ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    public static SyndicationContentFormat SyndicationContentFormatGet(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // The shared factory rather than settings of its own. These previously left DtdProcessing at
        // the .NET default of Prohibit, while the loader parses internal DTD subsets deliberately so
        // that entities a feed declares resolve - meaning a feed Load accepted was one whose format
        // detection threw.
        using XmlReader reader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        return SyndicationDiscoveryUtility.SyndicationContentFormatGet(reader);
    }

    /// <summary>
    /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource represented by the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="XmlReader"/> that represents the XML data for the syndicated resource.</param>
    /// <returns>
    ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
    ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public static SyndicationContentFormat SyndicationContentFormatGet(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        // Only the root element's name is needed, so the reader is advanced to it rather than a whole
        // DOM being built. Microsoft's XML performance guidance is explicit that the DOM reads the
        // entire document into memory - typically three to four times its size on disk - and that
        // MoveToContent is the way to skip to what you actually want.
        string rootElementName = reader.MoveToContent() == XmlNodeType.Element
            ? reader.LocalName
            : string.Empty;

        return EnumerationMetadataAttribute.GetEnumByAlternateValueMapping<SyndicationContentFormat>()
            .GetValueOrDefault(rootElementName, SyndicationContentFormat.None);
    }

    /// <summary>
    /// Returns the <see cref="SyndicationContentFormat"/> of the syndicated resource represented by the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="navigator">A <see cref="XPathNavigator"/> that represents the XML data for the syndicated resource.</param>
    /// <returns>
    ///     A <see cref="SyndicationContentFormat"/> enumeration value indicating the format of the syndicated resource.
    ///     If unable to determine format, returns <see cref="SyndicationContentFormat.None"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    public static SyndicationContentFormat SyndicationContentFormatGet(XPathNavigator navigator)
    {
        ArgumentNullException.ThrowIfNull(navigator);

        XPathNavigator source = navigator.CreateNavigator();
        if (string.IsNullOrEmpty(source.LocalName))
        {
            source.MoveToRoot();
            source.MoveToChild(XPathNodeType.Element);
        }

        string rootElementName = source.LocalName;

        return EnumerationMetadataAttribute.GetEnumByAlternateValueMapping<SyndicationContentFormat>()
            .GetValueOrDefault(rootElementName, SyndicationContentFormat.None);
    }

    /// <summary>
    /// Returns a <see cref="Dictionary{TKey, TValue}"/> of the HTML attribute name/value pairs for the supplied content.
    /// </summary>
    /// <param name="content">The HTML content to parse.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of the HTML attribute name/value pairs extracted the supplied <paramref name="content"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="content"/> is an empty string.</exception>
    private static Dictionary<string, string> ExtractHtmlAttributes(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
        MatchCollection matches = AttributeRegex().Matches(content);

        foreach (Match match in matches)
        {
            if (match.Groups is { Count: > 0 })
            {
                string name = match.Groups[1].Value;
                string value;
                if (!string.IsNullOrEmpty(name))
                {
                    value = match.Groups[2].Value;
                }
                else
                {
                    name = match.Groups[3].Value;
                    value = match.Groups[4].Value;
                }

                name = name.Trim();
                value = value.Trim();

                attributes.TryAdd(name, value);
            }
        }

        return attributes;
    }

    /// <summary>
    /// Returns a collection of <see cref="Uri"/> instances that represent HTML header links and/or anchor tags in the supplied HTML markup.
    /// </summary>
    /// <param name="content">The HTML markup to parse.</param>
    /// <returns>Every <c>href</c> found, in document order within each group: the <c>link</c> elements first, then the anchors.</returns>
    /// <remarks>
    ///     A relative <c>href</c> is returned relative — nothing here knows the address the markup came
    ///     from — and duplicates are not removed, so the same target appearing in both a <c>link</c> and
    ///     an anchor appears twice.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<Uri> ExtractUrls(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        List<Uri> results = [];
        MatchCollection links = LinkRegex().Matches(content);

        foreach (Match link in links)
        {
            var linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

            if (linkAttributes.TryGetValue("HREF", out string? href))
            {
                if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out Uri? uri))
                {
                    results.Add(uri);
                }
            }
        }

        MatchCollection anchors = AnchorRegex().Matches(content);

        foreach (Match anchor in anchors)
        {
            var anchorAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(anchor.Value);

            if (anchorAttributes.TryGetValue("HREF", out string? href))
            {
                if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out Uri? uri))
                {
                    results.Add(uri);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the source <see cref="Uri"/> references the target <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the source web resource that will be searched.</param>
    /// <param name="target">A <see cref="Uri"/> that represents the target web resource being searched for.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="source"/>
    ///     contains at least one link to the <paramref name="target"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> SourceReferencesTargetAsync(
        Uri source,
        Uri target,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await SourceReferencesTargetAsync(source, target, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the source <see cref="Uri"/> references the target <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the source web resource that will be searched.</param>
    /// <param name="target">A <see cref="Uri"/> that represents the target web resource being searched for.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="source"/>
    ///     contains at least one link to the <paramref name="target"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///     and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///     This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> SourceReferencesTargetAsync(
        Uri source,
        Uri target,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
            source, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using StreamReader reader = new(body.AsStream());
        string content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        IList<Uri> links = SyndicationDiscoveryUtility.ExtractUrls(content);

        if (links is { Count: > 0 })
        {
            foreach (Uri link in links)
            {
                if (Uri.Compare(link, target, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> exists.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="uri"/> exists; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     This method will return <see langword="false"/> if the <paramref name="uri"/> is <see langword="null"/> or the <paramref name="uri"/> is otherwise inaccessible.
    ///     Requests are subject to a 100-second default time-out; a timed-out request is treated as inaccessible.
    /// </remarks>
    public static async Task<bool> UriExistsAsync(
        Uri uri,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);

        try
        {
            return await UriExistsAsync(uri, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> exists using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="uri"/> exists; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method will return <see langword="false"/> if the <paramref name="uri"/> is <see langword="null"/> or the <paramref name="uri"/> is otherwise inaccessible.
    ///     </para>
    ///     <para>
    ///         The request is a <c>GET</c> that completes on the headers, so the body is never read.
    ///         Success plus a declared <c>Content-Length</c> that is not exactly zero counts as existing;
    ///         a response declaring no length at all — every chunked and every decompressed one — counts
    ///         as existing too, because absence of a declaration says nothing about the body.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public static async Task<bool> UriExistsAsync(
        Uri uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        if (uri is null)
        {
            return false;
        }

        try
        {
            // The body is never read here, so headers-read is straightforward - but the predicate
            // has to change in the same breath. Under content-read the body was buffered before this
            // line ran, and a buffered HttpContent reports its BUFFER's length whatever the origin
            // sent, so ContentLength was never null and `> 0` was correct. Under headers-read nothing
            // has been buffered, so a response that declared no length - every decompressed and every
            // chunked one - reports null and would answer 'does not exist'. Splitting these two
            // changes across commits introduces the bug the second one fixes.
            using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
                uri, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            long contentLength = response.Content.Headers.ContentLength ?? -1;
            return response.IsSuccessStatusCode && contentLength != 0;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/> and entity tag.
    /// </summary>
    /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
    /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
    /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ConditionalGetResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public static async Task<ConditionalGetResult> ConditionalGetAsync(
        Uri source,
        DateTime lastModified,
        string? entityTag,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await ConditionalGetAsync(source, lastModified, entityTag, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously performs a conditional get operation against the supplied <see cref="Uri"/> using the specified <see cref="DateTime"/>, entity tag, and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
    /// <param name="lastModified">The <c>Last-Modified</c> the origin last reported, sent as <c>If-Modified-Since</c>. A <see cref="DateTimeKind.Unspecified"/> value is read as UTC, which is what HTTP validator times are.</param>
    /// <param name="entityTag">The entity tag the origin last reported, sent as <c>If-None-Match</c>. Pass it exactly as received, quotes included, or <see langword="null"/> to send none.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task whose result reports whether the origin sent a body, and carries the validators to send next time.</returns>
    /// <remarks>
    ///     <para>
    ///         A <c>304</c> arrives as a result rather than as an exception, which is what distinguishes
    ///         this from every other fetch in the library. Prefer the
    ///         <see cref="ConditionalGetAsync(Uri, SyndicationValidators, HttpClient, SyndicationRequestOptions, CancellationToken)"/>
    ///         overload: it can carry request options, and a <see cref="SyndicationValidators"/> lets a
    ///         resource never yet fetched say so, rather than needing a <see cref="DateTime"/> invented
    ///         for it.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="HttpRequestException">The response status code does not indicate success or a lack of modification.</exception>
    public static Task<ConditionalGetResult> ConditionalGetAsync(
        Uri source,
        DateTime lastModified,
        string? entityTag,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        // HTTP validator times are UTC; treat an Unspecified kind as UTC so a caller who read a
        // Last-Modified out of a previous response sends back the instant they were given.
        DateTimeOffset lastModifiedOffset = lastModified.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(lastModified, TimeSpan.Zero)
            : new DateTimeOffset(lastModified);

        return ConditionalGetAsync(
            source, new SyndicationValidators(lastModifiedOffset, entityTag), httpClient, null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously performs a conditional get operation against the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">The <see cref="Uri"/> to perform a conditional GET operation against.</param>
    /// <param name="validators">The cache validators held for the <paramref name="source"/>.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="requestOptions">Request-level options — Accept, User-Agent, Referer, custom headers. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ConditionalGetResult"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     The overload the others delegate to, and the only one that can carry a
    ///     <see cref="SyndicationRequestOptions"/> — conditional GET was previously the one fetch in
    ///     this library that could not be given an <c>Accept</c> header or a custom <c>User-Agent</c>,
    ///     because it built its request by hand instead of through
    ///     <see cref="SyndicationEncodingUtility.CreateHttpRequestMessage"/>.
    ///     </para>
    ///     <para>
    ///     Passing <see cref="SyndicationValidators.None"/> makes the request unconditional, which is
    ///     the correct thing to do for a resource never yet fetched — and is why
    ///     <see cref="SyndicationValidators"/> models both halves as nullable rather than requiring a
    ///     <see cref="DateTime"/> to be invented.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="validators"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="HttpRequestException">The response status code does not indicate success or a lack of modification.</exception>
    public static async Task<ConditionalGetResult> ConditionalGetAsync(
        Uri source,
        SyndicationValidators validators,
        HttpClient httpClient,
        SyndicationRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(validators);
        ArgumentNullException.ThrowIfNull(httpClient);

        // CreateHttpRequestMessage sets the framework User-Agent and applies the caller's options. The
        // line that used to set it here as well was the second of two spellings, and the one that
        // could not be overridden.
        using HttpRequestMessage request = SyndicationEncodingUtility.CreateHttpRequestMessage(source, requestOptions);
        validators.ApplyTo(request);

        // Headers-read, which this could not be until the modification heuristic was deleted. That
        // heuristic read ContentLength and ContentType off the response to decide whether anything had
        // changed, and under headers-read ContentLength is null for any chunked reply - so completing
        // on headers would have turned every chunked 200 into a discarded body. With the decision
        // reduced to the status code, nothing here needs the body, and the caller gets to decide
        // whether to pay for it.
        HttpResponseMessage response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            // Read the validators before disposing. A 304 has no body but it does carry an ETag and a
            // Last-Modified, and those are precisely what the caller needs for their next poll -
            // throwing them away meant re-sending a stale tag until the resource changed.
            ConditionalGetResult unmodified = new(response);
            response.Dispose();
            return unmodified;
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                response.Dispose();
            }
        }

        // Anything still here is a success that is not a 304, and that is the whole decision. The
        // server was asked with If-Modified-Since and If-None-Match; 304 is how it says "no", and a
        // body is how it says "yes". Re-deciding from the headers second-guessed an answer we had.
        //
        // The heuristic that used to sit here compared the response's Last-Modified against the one
        // we sent - a comparison that can only be true when the server already declined to send a
        // 304 - and then fell back to "does this look like it has content", asking ContentLength and
        // ContentType. A chunked 200 with no Content-Type answers no to both, so it was disposed and
        // reported to the caller as unmodified: a real body, silently discarded, indistinguishable
        // from a cache hit. The fallback was also gated on == HttpStatusCode.OK while the success
        // check above admits every 2xx, so a 203 or a 206 was thrown away without even reaching it.
        return new ConditionalGetResult(response, wasModified: true);
    }

    /// <summary>
    /// Extracts auto-discoverable syndication endpoints from the supplied HTML markup.
    /// </summary>
    /// <param name="content">The HTML markup to parse.</param>
    /// <returns>
    ///     A collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints contained within the <paramref name="content"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         A <c>link</c> qualifies only when it carries <c>href</c>, <c>rel</c> and <c>type</c>, and
    ///         its <c>rel</c> is <c>alternate</c>. Requiring <c>type</c> is what keeps a
    ///         <c>rel="alternate"</c> that is a translation rather than a feed out of the results; the
    ///         cost is that a feed link omitting <c>type</c> is missed.
    ///     </para>
    ///     <para>
    ///         A relative <c>href</c> is stored as it appears and cannot be fetched. Prefer
    ///         <see cref="ExtractDiscoverableSyndicationEndpoints(string, Uri?)"/>, which resolves one.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.rssboard.org/rss-autodiscovery">https://www.rssboard.org/rss-autodiscovery</a> for
    ///         further information about the auto-discovery of syndicated content.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<DiscoverableSyndicationEndpoint> ExtractDiscoverableSyndicationEndpoints(string content)
        => ExtractDiscoverableSyndicationEndpoints(content, baseUri: null);

    /// <summary>
    /// Extracts auto-discoverable syndication endpoints, resolving relative hrefs against a base URI.
    /// </summary>
    /// <param name="content">The HTML markup to parse.</param>
    /// <param name="baseUri">
    ///     The address the markup was retrieved from, used to resolve relative hrefs. When
    ///     <see langword="null"/>, a relative href is stored as it appears.
    /// </param>
    /// <returns>A collection of the endpoints found.</returns>
    /// <remarks>
    ///     <para>
    ///     <c>href="/feed.xml"</c> is the commonest form an auto-discovery link takes, and without a
    ///     base URI it produced an endpoint whose <c>Source</c> was relative — which
    ///     <see cref="DiscoverableSyndicationEndpoint.CreateNavigatorAsync(HttpClient, CancellationToken)"/>
    ///     cannot fetch. It hands the address to <c>HttpClient</c>, which rejects a relative one.
    ///     </para>
    ///     <para>
    ///     The overload without a base URI is kept and unchanged, because a caller parsing markup they
    ///     already hold has no address to resolve against and did not necessarily want one invented.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<DiscoverableSyndicationEndpoint> ExtractDiscoverableSyndicationEndpoints(string content, Uri? baseUri)
    {
        List<DiscoverableSyndicationEndpoint> results = [];
        ArgumentException.ThrowIfNullOrEmpty(content);

        MatchCollection links = LinkRegex().Matches(content);

        foreach (Match link in links)
        {
            var linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

            if (linkAttributes.TryGetValue("HREF", out string? href) &&
                linkAttributes.TryGetValue("REL", out string? rel) &&
                linkAttributes.TryGetValue("TYPE", out string? type))
            {
                if (string.Equals(rel, "alternate", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out Uri? url))
                    {
                        if (!url.IsAbsoluteUri && baseUri is not null && Uri.TryCreate(baseUri, url, out Uri? absolute))
                        {
                            url = absolute;
                        }

                        DiscoverableSyndicationEndpoint endpoint = new()
                        {
                            Source = url
                        };
                        if (!string.IsNullOrEmpty(type))
                        {
                            endpoint.ContentType = type;
                        }

                        if (linkAttributes.TryGetValue("TITLE", out string? title) && !string.IsNullOrEmpty(title))
                        {
                            endpoint.Title = title;
                        }

                        results.Add(endpoint);
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Extracts auto-discoverable syndication endpoints from the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that represents the HTML markup to parse.</param>
    /// <returns>
    ///     A collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints contained within the <paramref name="stream"/>.
    /// </returns>
    /// <remarks>
    ///     See <a href="https://www.rssboard.org/rss-autodiscovery">https://www.rssboard.org/rss-autodiscovery</a> for
    ///     further information about the auto-discovery of syndicated content.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    public static IList<DiscoverableSyndicationEndpoint> ExtractDiscoverableSyndicationEndpoints(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using StreamReader reader = new(stream);
        return SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(reader.ReadToEnd());
    }

    /// <summary>
    /// Asynchronously returns a collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints for the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">A <see cref="Uri"/> that represents the URL of the web resource to parse.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of <see cref="DiscoverableSyndicationEndpoint"/>
    ///     objects that represent auto-discoverable syndicated content endpoints for the web resource located at the <paramref name="uri"/>.
    /// </returns>
    /// <remarks>
    ///     See <a href="https://www.rssboard.org/rss-autodiscovery">https://www.rssboard.org/rss-autodiscovery</a> for
    ///     further information about the auto-discovery of syndicated content.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<IList<DiscoverableSyndicationEndpoint>> LocateDiscoverableSyndicationEndpointsAsync(
        Uri uri,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await LocateDiscoverableSyndicationEndpointsAsync(uri, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns a collection of <see cref="DiscoverableSyndicationEndpoint"/> objects that represent auto-discoverable syndicated content endpoints for the supplied <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">A <see cref="Uri"/> that represents the URL of the web resource to parse.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of <see cref="DiscoverableSyndicationEndpoint"/>
    ///     objects that represent auto-discoverable syndicated content endpoints for the web resource located at the <paramref name="uri"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         See <a href="https://www.rssboard.org/rss-autodiscovery">https://www.rssboard.org/rss-autodiscovery</a> for
    ///         further information about the auto-discovery of syndicated content.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<IList<DiscoverableSyndicationEndpoint>> LocateDiscoverableSyndicationEndpointsAsync(
        Uri uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
            uri, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // The Extract overload below takes a Stream and reads it with ReadToEnd, synchronously. Under
        // headers-read that stream is the socket, so it is drained here first - asynchronously, and
        // under a bound - and the synchronous reader gets memory instead.
        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using StreamReader reader = new(body.AsStream());
        string markup = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        // Resolved against the address the response actually came from rather than the one that was
        // asked for, so a redirect is honoured: a page moved from example.com to www.example.com
        // must resolve its relative links against where it ended up.
        return SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(
            markup, response.RequestMessage?.RequestUri ?? uri);
    }

    /// <summary>
    /// Extracts a <see cref="HtmlAnchor"/> that represents a pingback auto-discovery link from the supplied HTML markup.
    /// </summary>
    /// <param name="content">The HTML markup to parse.</param>
    /// <returns>
    ///     The first pingback auto-discovery link the markup declares, or <see langword="null"/> if it
    ///     declares none the caller could contact.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Pingback enabled resources that utilize the link mechanism will contain a
    ///         &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
    ///         The <c>href</c> must be an absolute <c>http</c> or <c>https</c> URI: a relative one is
    ///         skipped rather than resolved, because a pingback server address is one the publisher
    ///         states outright, and any other scheme is skipped because the ping is an XML-RPC
    ///         <c>POST</c> and nothing else can carry it.
    ///     </para>
    ///     <para>
    ///         The scheme test is not fussiness. <see cref="Uri.TryCreate(string, UriKind, out Uri)"/>
    ///         with <see cref="UriKind.Absolute"/> succeeds on Unix for a leading slash, so
    ///         <c>href="/xmlrpc.php"</c> resolved to <c>file:///xmlrpc.php</c> and was returned as a
    ///         discovered remote endpoint, while the same markup was refused on Windows — discovery
    ///         accepting a different set of documents depending on the operating system. A
    ///         protocol-relative <c>//example.com/rpc</c>, which is ordinary in real HTML, became
    ///         <c>file://example.com/rpc</c>.
    ///     </para>
    ///     <para>
    ///         Where the markup declares several, the <b>first</b> wins. The specification says
    ///         "Pages MUST NOT include more than one such element" and defines no client behaviour for a
    ///         page that does, so this is a choice among undefined behaviours: first-wins is how HTML
    ///         <c>&lt;link&gt;</c> relations are conventionally resolved, and it bounds the work a
    ///         hostile page can extract from the scan.
    ///     </para>
    ///     <para>
    ///         The <see cref="HtmlAnchor"/> that is returned will have an <see cref="HtmlAnchor.HRef"/> that points to the
    ///         absolute URI of the pingback XML-RPC server, and a <c>rel</c> entry of <c>pingback</c>.
    ///         <see cref="HtmlAnchor.Title"/> and a <c>type</c> entry are filled in when the markup supplied them.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.hixie.ch/specs/pingback/pingback">https://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="content"/> is an empty string.</exception>
    public static HtmlAnchor? ExtractPingbackNotificationServer(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        MatchCollection links = LinkRegex().Matches(content);

        foreach (Match link in links)
        {
            var linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

            if (linkAttributes.TryGetValue("HREF", out string? href) &&
                linkAttributes.TryGetValue("REL", out string? rel) &&
                string.Equals(rel, "pingback", StringComparison.OrdinalIgnoreCase) &&
                Uri.TryCreate(href, UriKind.Absolute, out Uri? uri) &&
                (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
            {
                HtmlAnchor pingbackAnchor = new()
                {
                    HRef = href,
                };
                pingbackAnchor.Attributes.Add("rel", rel);

                if (linkAttributes.TryGetValue("TYPE", out string? type) && !string.IsNullOrEmpty(type))
                {
                    pingbackAnchor.Attributes.Add("type", type);
                }

                if (linkAttributes.TryGetValue("TITLE", out string? title) && !string.IsNullOrEmpty(title))
                {
                    pingbackAnchor.Title = title;
                }

                return pingbackAnchor;
            }
        }

        return null;
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> is a pingback enabled web resource.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="uri"/>
    ///     is pingback enabled; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <c>rel</c> attribute value of <c>pingback</c>
    ///         or an HTTP header named <c>X-Pingback</c>. A web resource is considered pingback enabled if it utilizes
    ///         either or both of these mechanisms.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.hixie.ch/specs/pingback/pingback">https://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> IsPingbackEnabledAsync(
        Uri uri,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await IsPingbackEnabledAsync(uri, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> is a pingback enabled web resource using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="uri"/>
    ///     is pingback enabled; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <c>rel</c> attribute value of <c>pingback</c>
    ///         or an HTTP header named <c>X-Pingback</c>. A web resource is considered pingback enabled if it utilizes
    ///         either or both of these mechanisms.
    ///     </para>
    ///     <para>
    ///         The header is checked first and the body is downloaded only if it is absent, so a
    ///         header-advertising origin costs one round trip and no page. The body, when it is read, is
    ///         bounded by <see cref="SyndicationContentLengthLimits.Discovery"/>.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.hixie.ch/specs/pingback/pingback">https://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> IsPingbackEnabledAsync(
        Uri uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(httpClient);

        // Headers-read: the common case answers from a header and never touches the body, so downloading
        // a whole page to read one header was pure waste. The fall-through now drains explicitly.
        using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
            uri, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        if (response.Headers.TryGetValues("X-Pingback", out var values))
        {
            foreach (var value in values)
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out Uri? pingbackXmlRpcServer))
                {
                    return true;
                }
            }
        }

        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using StreamReader reader = new(body.AsStream());
        string content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        HtmlAnchor? link = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(content);

        return link is not null;
    }

    /// <summary>
    /// Asynchronously returns a <see cref="Uri"/> that represents a pingback XML-RPC server endpoint using the Pingback server auto-discovery mechanisms for the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> of a web resource to perform pingback auto-discovery against.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="Uri"/> that represents
    ///     the absolute URI of the pingback XML-RPC server. If pingback server auto-discovery fails, returns <see langword="null"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <c>rel</c> attribute value of <c>pingback</c>
    ///         or an HTTP header named <c>X-Pingback</c>.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.hixie.ch/specs/pingback/pingback">https://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<Uri?> LocatePingbackNotificationServerAsync(
        Uri uri,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await LocatePingbackNotificationServerAsync(uri, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns a <see cref="Uri"/> that represents a pingback XML-RPC server endpoint using the Pingback server auto-discovery mechanisms for the supplied <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> of a web resource to perform pingback auto-discovery against.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="Uri"/> that represents
    ///     the absolute URI of the pingback XML-RPC server. If pingback server auto-discovery fails, returns <see langword="null"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <c>rel</c> attribute value of <c>pingback</c>
    ///         or an HTTP header named <c>X-Pingback</c>. The header is preferred: it is checked first,
    ///         and the page is downloaded only if it is absent.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.hixie.ch/specs/pingback/pingback">https://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<Uri?> LocatePingbackNotificationServerAsync(
        Uri uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(httpClient);

        // Headers-read: the common case answers from a header and never touches the body, so downloading
        // a whole page to read one header was pure waste. The fall-through now drains explicitly.
        using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
            uri, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        if (response.Headers.TryGetValues("X-Pingback", out var values))
        {
            foreach (var value in values)
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out Uri? url))
                {
                    return url;
                }
            }
        }

        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using StreamReader reader = new(body.AsStream());
        string content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        HtmlAnchor? link = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(content);

        if (link is not null && Uri.TryCreate(link.HRef, UriKind.Absolute, out Uri? href))
        {
            return href;
        }

        return null;
    }

    /// <summary>
    /// Extracts embedded RDF Trackback discovery meta-data from the supplied HTML markup.
    /// </summary>
    /// <param name="content">The HTML markup to parse.</param>
    /// <returns>
    ///     One entry per embedded <c>rdf:RDF</c> island that named a <c>trackback:ping</c>. Islands that
    ///     named none are dropped, so the collection can be shorter than the number of islands present,
    ///     and every <see cref="TrackbackDiscoveryMetadata.PingUrl"/> in it is populated.
    /// </returns>
    /// <remarks>
    ///     Islands are matched in the raw markup rather than in a parsed document, so one hidden inside
    ///     an HTML comment is found just the same.
    ///     See <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a> for
    ///     further information about the auto-discovery of Trackback ping URLs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<TrackbackDiscoveryMetadata> ExtractTrackbackNotificationServers(string content)
    {
        List<TrackbackDiscoveryMetadata> results = [];
        ArgumentException.ThrowIfNullOrEmpty(content);

        XmlNamespaceManager manager = new(new NameTable());

        manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        manager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
        manager.AddNamespace("trackback", "http://madskills.com/public/xml/rss/module/trackback/");

        MatchCollection embeddedRdfs = RdfRegex().Matches(content);

        foreach (Match embeddedRdf in embeddedRdfs)
        {
            using StringReader stringReader = new(embeddedRdf.Value);
            using XmlReader xmlReader = XmlReader.Create(stringReader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
            XPathDocument document = new(xmlReader);
            XPathNavigator navigator = document.CreateNavigator();

            TrackbackDiscoveryMetadata trackbackMetadata = new();
            if (trackbackMetadata.Load(navigator))
            {
                results.Add(trackbackMetadata);
            }
        }

        return results;
    }

    /// <summary>
    /// Extracts embedded RDF Trackback discovery meta-data from the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that represents the HTML markup to parse.</param>
    /// <returns>
    ///     A collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent embedded Trackback ping URLs contained within the <paramref name="stream"/>.
    /// </returns>
    /// <remarks>
    ///     See <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a> for
    ///     further information about the auto-discovery of Trackback ping URLs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    public static IList<TrackbackDiscoveryMetadata> ExtractTrackbackNotificationServers(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using StreamReader reader = new(stream);
        return SyndicationDiscoveryUtility.ExtractTrackbackNotificationServers(reader.ReadToEnd());
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> is a trackback enabled web resource.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="uri"/>
    ///     is trackback enabled; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The auto-discovery mechanism for trackback utilizes embedded RDF meta-data elements within the web resource.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a> for
    ///         further information about the auto-discovery of Trackback ping URLs.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> IsTrackbackEnabledAsync(
        Uri uri,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        IList<TrackbackDiscoveryMetadata> endpoints = await LocateTrackbackNotificationServersAsync(uri, cancellationToken).ConfigureAwait(false);
        return endpoints.Count > 0;
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> is a trackback enabled web resource using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if the <paramref name="uri"/>
    ///     is trackback enabled; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The auto-discovery mechanism for trackback utilizes embedded RDF meta-data elements within the web resource.
    ///     </para>
    ///     <para>
    ///         See <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a> for
    ///         further information about the auto-discovery of Trackback ping URLs.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<bool> IsTrackbackEnabledAsync(
        Uri uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(httpClient);

        IList<TrackbackDiscoveryMetadata> endpoints = await LocateTrackbackNotificationServersAsync(uri, httpClient, cancellationToken).ConfigureAwait(false);
        return endpoints.Count > 0;
    }

    /// <summary>
    /// Asynchronously returns a collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent trackback ping URL endpoints for the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">A <see cref="Uri"/> that represents the URL of the web resource to parse.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of <see cref="TrackbackDiscoveryMetadata"/>
    ///     objects that represent embedded Trackback ping URLs for the web resource located at the <paramref name="uri"/>.
    /// </returns>
    /// <remarks>
    ///     See <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a> for
    ///     further information about the auto-discovery of Trackback ping URLs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<IList<TrackbackDiscoveryMetadata>> LocateTrackbackNotificationServersAsync(
        Uri uri,
        CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SyndicationEncodingUtility.DefaultRequestTimeout);
        return await LocateTrackbackNotificationServersAsync(uri, SyndicationEncodingUtility.SharedHttpClient, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously returns a collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent trackback ping URL endpoints for the supplied <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="uri">A <see cref="Uri"/> that represents the URL of the web resource to parse.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of <see cref="TrackbackDiscoveryMetadata"/>
    ///     objects that represent embedded Trackback ping URLs for the web resource located at the <paramref name="uri"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         See <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a> for
    ///         further information about the auto-discovery of Trackback ping URLs.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<IList<TrackbackDiscoveryMetadata>> LocateTrackbackNotificationServersAsync(
        Uri uri,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpResponseMessage response = await SyndicationEncodingUtility.SendHttpRequestAsync(
            uri, httpClient, null, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // The Extract overload below takes a Stream and reads it with ReadToEnd, synchronously. Under
        // headers-read that stream is the socket, so it is drained here first - asynchronously, and
        // under a bound - and the synchronous reader gets memory instead.
        using PooledContentBuffer body = await SyndicationEncodingUtility.ReadContentAsync(
            response, SyndicationContentLengthLimits.Discovery, cancellationToken).ConfigureAwait(false);
        using Stream stream = body.AsStream();
        return SyndicationDiscoveryUtility.ExtractTrackbackNotificationServers(stream);
    }
    /// <summary>
    /// Matches an HTML attribute, quoted or bare.
    /// </summary>
    /// <returns>The generated matcher.</returns>
    /// <remarks>
    ///     Source-generated rather than constructed per call. This one mattered most of the six:
    ///     <see cref="ExtractHtmlAttributes"/> built it on every invocation and <see cref="ExtractUrls"/>
    ///     invokes that once per matched <c>&lt;link&gt;</c> and once per matched <c>&lt;a&gt;</c>, so a
    ///     page with 50 links and 200 anchors parsed this pattern 250 times to answer one question.
    /// </remarks>
    [GeneratedRegex("""([a-zA-Z]+)=["']([^"']+)["']|([a-zA-Z]+)=([^"'>\r\n\t ]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex AttributeRegex();

    /// <summary>
    /// Matches the opening of an HTML <c>link</c> element.
    /// </summary>
    /// <returns>The generated matcher.</returns>
    /// <remarks>
    ///     One matcher for what were three identical per-call constructions, in
    ///     <see cref="ExtractUrls"/>, <see cref="ExtractDiscoverableSyndicationEndpoints(string, Uri?)"/>
    ///     and <see cref="ExtractPingbackNotificationServer"/>.
    /// </remarks>
    [GeneratedRegex("<link[^>]+", RegexOptions.IgnoreCase)]
    private static partial Regex LinkRegex();

    /// <summary>
    /// Matches the opening of an HTML anchor element.
    /// </summary>
    /// <returns>The generated matcher.</returns>
    [GeneratedRegex("<a[^>]+", RegexOptions.IgnoreCase)]
    private static partial Regex AnchorRegex();

    /// <summary>
    /// Matches an RDF document embedded in HTML markup.
    /// </summary>
    /// <returns>The generated matcher.</returns>
    [GeneratedRegex(@"<rdf:RDF\b[^>]*>(.*?)</rdf:RDF>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex RdfRegex();

}