using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides methods for extracting peer-to-peer auto-discovery and resource information from syndicated content. This class cannot be inherited.
/// </summary>
public static class SyndicationDiscoveryUtility
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
    /// <value>A string that represents information such as the client application name, version, host operating system, and language.</value>
    public static string FrameworkUserAgent { get; } = CreateFrameworkUserAgent();

    /// <summary>
    /// Returns the <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified format name.
    /// </summary>
    /// <param name="name">The name of the syndication content format.</param>
    /// <returns>A <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>SyndicationContentFormat.None</b>.</returns>
    /// <remarks>This method disregards case of specified format name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    ///     This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///     and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///     This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    private static Dictionary<string, string> ExtractHtmlAttributes(string content)
    {
        Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
        Regex attributePattern = new("""([a-zA-Z]+)=["']([^"']+)["']|([a-zA-Z]+)=([^"'>\r\n\t ]+)""", RegexOptions.IgnoreCase);

        ArgumentException.ThrowIfNullOrEmpty(content);

        MatchCollection matches = attributePattern.Matches(content);

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
    /// <returns>A collection of <see cref="Uri"/> instances that represent HTML anchor elements and header links in the supplied HTML markup.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<Uri> ExtractUrls(string content)
    {
        List<Uri> results = [];
        Regex linkPattern = new("<link[^>]+", RegexOptions.IgnoreCase);
        Regex anchorPattern = new("<a[^>]+", RegexOptions.IgnoreCase);

        ArgumentException.ThrowIfNullOrEmpty(content);

        MatchCollection links = linkPattern.Matches(content);

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

        MatchCollection anchors = anchorPattern.Matches(content);

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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="source"/>
    ///     contains at least one link to the <paramref name="target"/>, otherwise <b>false</b>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="source"/>
    ///     contains at least one link to the <paramref name="target"/>, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///     and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///     This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="uri"/> exists, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     This method will return <b>false</b> if the <paramref name="uri"/> is a null reference or the <paramref name="uri"/> is otherwise inaccessible.
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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="uri"/> exists, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method will return <b>false</b> if the <paramref name="uri"/> is a null reference or the <paramref name="uri"/> is otherwise inaccessible.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    /// <param name="lastModified">A <see cref="DateTime"/> object that represents the date and time at which the <paramref name="source"/> was last known to be modified.</param>
    /// <param name="entityTag">The entity tag provided by the <paramref name="source"/> that is used to determine change in content.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ConditionalGetResult"/>.</returns>
    /// <remarks>
    ///     This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///     and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///     This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="HttpRequestException">The response status code does not indicate success or a lack of modification.</exception>
    public static async Task<ConditionalGetResult> ConditionalGetAsync(
        Uri source,
        DateTime lastModified,
        string? entityTag,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        // HTTP validator times are UTC; treat Unspecified-kind values as UTC so the sent header and the comparison below agree.
        DateTimeOffset lastModifiedOffset = lastModified.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(lastModified, TimeSpan.Zero)
            : new DateTimeOffset(lastModified);

        using var request = new HttpRequestMessage(HttpMethod.Get, source);
        request.Headers.UserAgent.ParseAdd(FrameworkUserAgent);
        request.Headers.IfModifiedSince = lastModifiedOffset;
        if (!string.IsNullOrEmpty(entityTag))
        {
            request.Headers.IfNoneMatch.TryParseAdd(entityTag);
        }

        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            response.Dispose();
            return new ConditionalGetResult(null, wasModified: false);
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

        // Check if actually modified by comparing Last-Modified header
        DateTimeOffset responseLastModified = response.Content.Headers.LastModified ?? DateTimeOffset.MinValue;
        bool isModified = responseLastModified > lastModifiedOffset;

        if (!isModified && response.StatusCode == HttpStatusCode.OK)
        {
            // Server may not support conditional GET properly, consider it modified if we got content
            isModified = response.Content.Headers.ContentLength > 0 ||
                         response.Content.Headers.ContentType is not null;
        }

        if (!isModified)
        {
            response.Dispose();
            return new ConditionalGetResult(null, wasModified: false);
        }

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
    ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
    ///     further information about the auto-discovery of syndicated content.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<DiscoverableSyndicationEndpoint> ExtractDiscoverableSyndicationEndpoints(string content)
    {
        List<DiscoverableSyndicationEndpoint> results = [];
        Regex linkPattern = new("<link[^>]+", RegexOptions.IgnoreCase);

        ArgumentException.ThrowIfNullOrEmpty(content);

        MatchCollection links = linkPattern.Matches(content);

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
    ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
    ///     further information about the auto-discovery of syndicated content.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
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
    ///     See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
    ///     further information about the auto-discovery of syndicated content.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
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
    ///         See <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a> for
    ///         further information about the auto-discovery of syndicated content.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
        using Stream stream = body.AsStream();
        return SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(stream);
    }

    /// <summary>
    /// Extracts a <see cref="HtmlAnchor"/> that represents a pingback auto-discovery link from the supplied HTML markup.
    /// </summary>
    /// <param name="content">The HTML markup to parse.</param>
    /// <returns>
    ///     A <see cref="HtmlAnchor"/> that represents the pingback auto-discovery link extracted from the <paramref name="content"/>.
    ///     If no pingback auto-discovery link was found, returns <b>null</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Pingback enabled resources that utilize the link mechanism will contain a
    ///         &lt;link rel="pingback" href="{Absolute URI of the pingback XML-RPC server}" /&gt; element.
    ///     </para>
    ///     <para>
    ///         The <see cref="HtmlAnchor"/> that is returned will have a <i>Href</i> property that points to the
    ///         absolute URI of the pingback XML-RPC server, and a <i>rel</i> attribute of pingback.
    ///         The <i>Title</i> property and <i>type</i> attribute will also be extracted if available.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static HtmlAnchor? ExtractPingbackNotificationServer(string content)
    {
        HtmlAnchor? pingbackAnchor = null;
        Regex linkPattern = new("<link[^>]+", RegexOptions.IgnoreCase);

        ArgumentException.ThrowIfNullOrEmpty(content);

        MatchCollection links = linkPattern.Matches(content);

        foreach (Match link in links)
        {
            var linkAttributes = SyndicationDiscoveryUtility.ExtractHtmlAttributes(link.Value);

            if (linkAttributes.TryGetValue("HREF", out string? href) &&
                linkAttributes.TryGetValue("REL", out string? rel))
            {
                if (string.Equals(rel, "pingback", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(href, UriKind.Absolute, out Uri? uri))
                    {
                        pingbackAnchor = new HtmlAnchor
                        {
                            HRef = href
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
                    }
                }
            }
        }

        return pingbackAnchor;
    }

    /// <summary>
    /// Asynchronously returns a value indicating if the supplied <see cref="Uri"/> is a pingback enabled web resource.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to validate.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="uri"/>
    ///     is pingback enabled, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
    ///         <u>or</u> an HTTP header named <b>X-Pingback</b>. A web resource is considered pingback enabled if it utilizes
    ///         either or both of these mechanisms.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="uri"/>
    ///     is pingback enabled, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
    ///         <u>or</u> an HTTP header named <b>X-Pingback</b>. A web resource is considered pingback enabled if it utilizes
    ///         either or both of these mechanisms.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    ///     the absolute URI of the pingback XML-RPC server. If pingback server auto-discovery fails, returns <b>null</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
    ///         <u>or</u> an HTTP header named <b>X-Pingback</b>.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
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
    ///     the absolute URI of the pingback XML-RPC server. If pingback server auto-discovery fails, returns <b>null</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         There are two mechanisms used when determining if a web resource is pingback enabled;
    ///         the presence of an HTML/XHTML &lt;link&gt; element with a <i>rel</i> attribute value of <b>pingback</b>
    ///         <u>or</u> an HTTP header named <b>X-Pingback</b>.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.hixie.ch/specs/pingback/pingback">http://www.hixie.ch/specs/pingback/pingback</a>
    ///         for more information about the pingback notification mechanism.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    ///     A collection of <see cref="TrackbackDiscoveryMetadata"/> objects that represent embedded Trackback ping URLs contained within the <paramref name="content"/>.
    /// </returns>
    /// <remarks>
    ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
    ///     further information about the auto-discovery of Trackback ping URLs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static IList<TrackbackDiscoveryMetadata> ExtractTrackbackNotificationServers(string content)
    {
        List<TrackbackDiscoveryMetadata> results = [];
        Regex rdfPattern = new(@"<rdf:RDF\b[^>]*>(.*?)</rdf:RDF>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        XmlNamespaceManager manager = new(new NameTable());

        ArgumentException.ThrowIfNullOrEmpty(content);

        manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        manager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
        manager.AddNamespace("trackback", "http://madskills.com/public/xml/rss/module/trackback/");

        MatchCollection embeddedRdfs = rdfPattern.Matches(content);

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
    ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
    ///     further information about the auto-discovery of Trackback ping URLs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="uri"/>
    ///     is trackback enabled, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The auto-discovery mechanism for trackback utilizes embedded RDF meta-data elements within the web resource.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
    ///         further information about the auto-discovery of Trackback ping URLs.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
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
    ///     A task that represents the asynchronous operation. The task result is <b>true</b> if the <paramref name="uri"/>
    ///     is trackback enabled, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The auto-discovery mechanism for trackback utilizes embedded RDF meta-data elements within the web resource.
    ///     </para>
    ///     <para>
    ///         See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
    ///         further information about the auto-discovery of Trackback ping URLs.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    ///     See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
    ///     further information about the auto-discovery of Trackback ping URLs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
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
    ///         See <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a> for
    ///         further information about the auto-discovery of Trackback ping URLs.
    ///     </para>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///         and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="uri"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
}