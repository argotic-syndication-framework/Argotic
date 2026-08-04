using System.Buffers;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides methods for encoding and decoding information exposed by syndicated content. This class cannot be inherited.
/// </summary>
public static partial class SyndicationEncodingUtility
{
    /// <summary>
    /// Private member to hold the lazily-initialized shared HttpClient instance.
    /// </summary>
    private static readonly Lazy<HttpClient> sharedHttpClient = new(CreateSharedHttpClient);

    /// <summary>
    /// The default time-out applied to requests made with the shared <see cref="HttpClient"/> when the caller supplies no bound of their own,
    /// matching the 100-second default of the <see cref="HttpWebRequest"/> pipeline this framework previously used.
    /// </summary>
    internal static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Characters that are invalid in directory names.
    /// </summary>
    private static readonly SearchValues<char> s_invalidDirectoryChars = SearchValues.Create(@"\/:*?<>|");

    /// <summary>
    /// The number of leading characters of a document examined when looking for an XML declaration.
    /// </summary>
    /// <remarks>
    ///     The declaration must be the first thing in the document and cannot legally exceed this,
    ///     so nothing beyond it can affect the result.
    /// </remarks>
    private const int XmlDeclarationProbeLength = 512;

    /// <summary>
    /// Matches the <c>encoding</c> pseudo-attribute of an XML declaration.
    /// </summary>
    /// <returns>The compiled regular expression.</returns>
    /// <remarks>
    ///     Source-generated rather than interpreted: the pattern is a compile-time constant, so the
    ///     generator emits a matcher directly instead of the engine parsing the pattern at run time.
    /// </remarks>
    [GeneratedRegex("""^<\?xml.+?encoding\s*=\s*(?:"(?<webName>[^"]*)"|(?<webName>\S+)).*?\?>""", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex XmlDeclarationEncodingRegex();

    /// <summary>
    /// Creates a shared <see cref="HttpClient"/> instance configured for optimal connection pooling.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    private static HttpClient CreateSharedHttpClient()
    {
#pragma warning disable CA2000 // HttpClient takes ownership of the handler; this is an intentional singleton
        SocketsHttpHandler handler = new()
#pragma warning restore CA2000
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
    }

    /// <summary>
    /// Gets the shared <see cref="HttpClient"/> instance for making HTTP requests.
    /// </summary>
    /// <value>A shared <see cref="HttpClient"/> instance configured for optimal connection pooling.</value>
    /// <remarks>
    /// This shared client is intended for use when no custom credentials or proxy settings are needed.
    /// For requests requiring authentication or custom proxy configuration, create an <see cref="HttpClient"/>
    /// with a configured <see cref="HttpClientHandler"/> or use <c>IHttpClientFactory</c>.
    /// </remarks>
    public static HttpClient SharedHttpClient => sharedHttpClient.Value;

    /// <summary>
    /// Creates <see cref="XmlReaderSettings"/> configured for secure XML parsing.
    /// </summary>
    /// <returns>
    ///     An <see cref="XmlReaderSettings"/> instance that parses internal DTD subsets (so entities declared by a feed resolve)
    ///     while preventing XXE attacks: external entity resolution is disabled and entity expansion is capped.
    /// </returns>
    public static XmlReaderSettings CreateSafeXmlReaderSettings()
    {
        return new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Document,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,
            MaxCharactersFromEntities = 10_000_000
        };
    }

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> against the supplied XML data.
    /// </summary>
    /// <param name="xml">The XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
    /// <returns>
    ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied XML data.
    ///     The supplied <paramref name="xml"/> data is parsed to remove invalid XML characters that would normally prevent
    ///     a navigator from being created.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="xml"/> data is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xml"/> data is an empty string.</exception>
    public static XPathNavigator CreateSafeNavigator(string xml)
    {
        ArgumentException.ThrowIfNullOrEmpty(xml);

        string safeXml = SyndicationEncodingUtility.RemoveInvalidXmlHexadecimalCharacters(xml);

        using StringReader stringReader = new(safeXml);
        using XmlReader xmlReader = XmlReader.Create(stringReader, CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);
        XPathNavigator navigator = document.CreateNavigator();

        return navigator;
    }

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> object that contains the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
    /// <returns>
    ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="stream"/>.
    ///     The supplied <paramref name="stream"/> XML data is parsed to remove invalid XML characters that would normally prevent
    ///     a navigator from being created.
    /// </returns>
    /// <remarks>
    ///     The character encoding of the supplied <paramref name="stream"/> is automatically determined based on the <i>encoding</i> attribute of the XML document declaration.
    ///     If the character encoding cannot be determined, a default encoding of <see cref="Encoding.UTF8"/> is used.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    public static XPathNavigator CreateSafeNavigator(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] buffer = SyndicationEncodingUtility.GetStreamBytes(stream);

        Encoding encoding = SyndicationEncodingUtility.GetXmlEncoding(buffer);

        using MemoryStream memoryStream = new(buffer);
        return SyndicationEncodingUtility.CreateSafeNavigator(memoryStream, encoding);
    }

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Stream"/> using the specified <see cref="Encoding"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> object that contains the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
    /// <param name="encoding">A <see cref="Encoding"/> object that indicates the character encoding to use when reading the supplied <paramref name="stream"/>.</param>
    /// <returns>
    ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="stream"/>.
    ///     The supplied <paramref name="stream"/> XML data is parsed to remove invalid XML characters that would normally prevent
    ///     a navigator from being created.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="encoding"/> is a null reference.</exception>
    public static XPathNavigator CreateSafeNavigator(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);

        using StreamReader reader = new(stream, encoding);
        return SyndicationEncodingUtility.CreateSafeNavigator(reader.ReadToEnd());
    }

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> object that contains the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
    /// <returns>
    ///     An <see cref="XPathNavigator"/> that provides a cursor model for navigating the supplied <paramref name="reader"/>.
    ///     The supplied <paramref name="reader"/> XML data is parsed to remove invalid XML characters that would normally prevent
    ///     a navigator from being created.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    public static XPathNavigator CreateSafeNavigator(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return SyndicationEncodingUtility.CreateSafeNavigator(reader.ReadToEnd());
    }

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Uri"/> asynchronously using the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that points to the location of the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
    /// <param name="encoding">A <see cref="Encoding"/> object that indicates the expected character encoding of the supplied <paramref name="source"/>. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="XPathNavigator"/>
    ///     that provides a cursor model for navigating the supplied <paramref name="source"/>.
    /// </returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>
    ///         If the <paramref name="encoding"/> is <b>null</b>, the character encoding of the supplied <paramref name="source"/> is determined automatically.
    ///         Otherwise, the specified <paramref name="encoding"/> is used when reading the XML data represented by the supplied <paramref name="source"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="HttpRequestException">The response status code does not indicate success.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static Task<XPathNavigator> CreateSafeNavigatorAsync(
        Uri source,
        Encoding? encoding,
        CancellationToken cancellationToken = default) => CreateSafeNavigatorAsync(source, SharedHttpClient, encoding, null, cancellationToken);

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> against the supplied <see cref="Uri"/> asynchronously using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that points to the location of the XML data to be navigated by the created <see cref="XPathNavigator"/>.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="encoding">A <see cref="Encoding"/> object that indicates the expected character encoding of the supplied <paramref name="source"/>. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="XPathNavigator"/>
    ///     that provides a cursor model for navigating the supplied <paramref name="source"/>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="encoding"/> is <b>null</b>, the character encoding of the supplied <paramref name="source"/> is determined automatically.
    ///         Otherwise, the specified <paramref name="encoding"/> is used when reading the XML data represented by the supplied <paramref name="source"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="HttpRequestException">The response status code does not indicate success.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<XPathNavigator> CreateSafeNavigatorAsync(
        Uri source,
        HttpClient httpClient,
        Encoding? encoding,
        SyndicationRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpResponseMessage response = await SendHttpRequestAsync(source, httpClient, requestOptions, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return encoding is not null
            ? CreateSafeNavigator(stream, encoding)
            : CreateSafeNavigator(stream);
    }

    /// <summary>
    /// Creates an <see cref="HttpRequestMessage"/> for a resource located at the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). Can be <b>null</b>.</param>
    /// <param name="method">The HTTP method to use. Defaults to <see cref="HttpMethod.Get"/>.</param>
    /// <returns>An <see cref="HttpRequestMessage"/> configured for the request.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public static HttpRequestMessage CreateHttpRequestMessage(Uri source, SyndicationRequestOptions? requestOptions = null, HttpMethod? method = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        HttpRequestMessage request = new(method ?? HttpMethod.Get, source);
        request.Headers.UserAgent.ParseAdd(SyndicationDiscoveryUtility.FrameworkUserAgent);
        requestOptions?.ApplyTo(request);

        return request;
    }

    /// <summary>
    /// Sends an HTTP request using the shared <see cref="HttpClient"/> and returns the response asynchronously.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). Can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="HttpResponseMessage"/>.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.
    ///     For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    ///     Requests made through this overload are subject to a 100-second default time-out, mirroring the legacy <see cref="HttpWebRequest"/> behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="OperationCanceledException">The request was canceled or exceeded the 100-second default time-out.</exception>
    public static async Task<HttpResponseMessage> SendHttpRequestAsync(Uri source, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(DefaultRequestTimeout);
        return await SendHttpRequestAsync(source, SharedHttpClient, requestOptions, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends an HTTP request using the specified <see cref="HttpClient"/> and returns the response asynchronously.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that points to the location of the resource to be retrieved.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). Can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="HttpResponseMessage"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         Configure handler-level settings (credentials, proxy, cookies) on the <see cref="HttpClient"/> itself,
    ///         either when creating it manually or via <c>IHttpClientFactory.ConfigurePrimaryHttpMessageHandler</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    public static async Task<HttpResponseMessage> SendHttpRequestAsync(Uri source, HttpClient httpClient, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpRequestMessage request = CreateHttpRequestMessage(source, requestOptions);
        return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Decodes a base64 encoded string.
    /// </summary>
    /// <param name="encodedValue">The base64 encoded string to decode.</param>
    /// <returns>A <see cref="Stream"/> the represents the decoded result of the base64 encoded value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="encodedValue"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="encodedValue"/> is an empty string.</exception>
    public static Stream DecodeBase64String(string encodedValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(encodedValue);

        byte[] data = Convert.FromBase64String(encodedValue);
        MemoryStream stream = new(data);

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return stream;
    }

    /// <summary>
    /// Decodes an HTML escaped string.
    /// </summary>
    /// <param name="escapedValue">The HTML escaped string to decode.</param>
    /// <returns>A string the represents the unescaped result of the HTML escaped value.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="escapedValue"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="escapedValue"/> is an empty string.</exception>
    public static string DecodeHtmlEscapedString(string escapedValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(escapedValue);

        string decodedResult = System.Web.HttpUtility.HtmlDecode(escapedValue);
        decodedResult = System.Web.HttpUtility.UrlDecode(decodedResult);

        return decodedResult;
    }

    /// <summary>
    /// Encodes the supplied string so that it can be safely represented in XML.
    /// </summary>
    /// <param name="content">A string that represents the XML data to parse for invalid XML hexadecimal characters.</param>
    /// <returns>A string that has been encoded to be safe for XML.</returns>
    /// <remarks>
    ///     <para>The encoding process replaces invalid XML hexadecimal characters with their equivalent decimal representation.</para>
    ///     <para>
    ///         Hexadecimal characters that are valid include: #x9, #xA, #xD, [#x20-#xD7FF], [#xE000-#xFFFD], [#x10000-#x10FFFF],
    ///         and any Unicode character; excluding the surrogate blocks FFFE and FFFF.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static string EncodeInvalidXmlHexadecimalCharacters(string content)
    {
        Regex invalidXmlUnicodeCharacters = new(@"[\x01-\x08\x0B-\x0C\x0E-\x1F\xD800-\xDFFF\xFFFE-\xFFFF]");

        ArgumentException.ThrowIfNullOrEmpty(content);

        string encodedContent = content;

        MatchCollection matches = invalidXmlUnicodeCharacters.Matches(encodedContent);
        foreach (Match match in matches)
        {
            encodedContent = encodedContent.Replace(match.Value, Convert.ToUInt32(match.Value, 16).ToString(NumberFormatInfo.InvariantInfo), StringComparison.Ordinal);
        }

        return encodedContent;
    }

    /// <summary>
    /// Returns an <see cref="Encoding"/> that represents the XML character encoding for the supplied array of bytes.
    /// </summary>
    /// <param name="data">An array of bytes that represents an XML data source to determine the character encoding for.</param>
    /// <returns>
    ///     A <see cref="Encoding"/> that represents the character encoding specified by the XML data source.
    ///     If the character encoding is not specified or unable to be determined, returns <see cref="Encoding.UTF8"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    public static Encoding GetXmlEncoding(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // The declaration sits at offset 0, so decoding the head of the document is normally enough.
        // The previous implementation decoded the whole document and ran the regex over all of it,
        // which on a 1000-item feed cost megabytes to read a few dozen bytes.
        //
        // A StreamReader over the bounded slice, rather than a direct ASCII decode, so that
        // byte-order-mark detection still happens - a UTF-16 document would otherwise decode to
        // nonsense and silently fall back to UTF-8.
        if (data.Length > XmlDeclarationProbeLength)
        {
            using MemoryStream head = new(data, 0, XmlDeclarationProbeLength, writable: false);
            using StreamReader headReader = new(head, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            string prefix = headReader.ReadToEnd();

            Match prefixMatch = XmlDeclarationEncodingRegex().Match(prefix);
            if (prefixMatch.Success)
            {
                return SyndicationEncodingUtility.EncodingFromMatch(prefixMatch);
            }

            // A declaration may legally contain arbitrary whitespace between its pseudo-attributes,
            // so one can run past the probe window. That never happens in practice, but "never in
            // practice" is not the same as "cannot", and the fast path must not change the answer:
            // if the document opens a declaration that did not fit, fall back to reading it whole.
            if (!prefix.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF8;
            }
        }

        // Delegating to the Stream overload preserves the original behaviour exactly, including the
        // ArgumentException an empty array produces by way of the string overload's guard.
        using MemoryStream stream = new(data);
        return SyndicationEncodingUtility.GetXmlEncoding(stream);
    }

    /// <summary>
    /// Returns an <see cref="Encoding"/> that represents the XML character encoding for the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that represents an XML data source to determine the character encoding for.</param>
    /// <returns>
    ///     A <see cref="Encoding"/> that represents the character encoding specified by the XML data source.
    ///     If the character encoding is not specified or unable to be determined, returns <see cref="Encoding.UTF8"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    public static Encoding GetXmlEncoding(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using StreamReader reader = new(stream);
        return SyndicationEncodingUtility.GetXmlEncoding(reader.ReadToEnd());
    }

    /// <summary>
    /// Returns an <see cref="Encoding"/> that represents the XML character encoding for the supplied content.
    /// </summary>
    /// <param name="content">A string that represents the XML data to determine the character encoding for.</param>
    /// <returns>
    ///     A <see cref="Encoding"/> that represents the character encoding specified by the XML data.
    ///     If the character encoding is not specified or unable to be determined, returns <see cref="Encoding.UTF8"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static Encoding GetXmlEncoding(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        // Deliberately unbounded. A declaration may legally contain arbitrary whitespace between its
        // pseudo-attributes, so truncating the input here would change the answer for a document
        // whose declaration runs long - and this overload is public API. The byte[] overload gets
        // the bounded fast path instead, falling back to this one when the declaration overruns it.
        return SyndicationEncodingUtility.EncodingFromMatch(XmlDeclarationEncodingRegex().Match(content));
    }

    /// <summary>
    /// Resolves the <see cref="Encoding"/> named by an XML declaration match.
    /// </summary>
    /// <param name="encodingMatch">The result of matching <see cref="XmlDeclarationEncodingRegex"/>.</param>
    /// <returns>The named encoding, or <see cref="Encoding.UTF8"/> if the match failed or named an unknown encoding.</returns>
    private static Encoding EncodingFromMatch(Match encodingMatch)
    {
        Encoding encoding = Encoding.UTF8;

        if (encodingMatch is { Groups.Count: > 0 })
        {
            Group group = encodingMatch.Groups["webName"];
            if (group is not null)
            {
                try
                {
                    encoding = Encoding.GetEncoding(group.Value);
                }
                catch (ArgumentException)
                {
                    encoding = Encoding.UTF8;
                }
            }
        }

        return encoding;
    }

    /// <summary>
    /// Sanitizes the supplied string so that it can be safely represented in XML.
    /// </summary>
    /// <param name="content">A string that represents the XML data to parse for invalid XML hexadecimal characters.</param>
    /// <returns>A string that has been sanitized to be safe for XML.</returns>
    /// <remarks>
    ///     <para>The sanitation process removes characters that are invalid for XML encoding.</para>
    ///     <para>
    ///         Hexadecimal characters that are valid include: #x9, #xA, #xD, [#x20-#xD7FF], [#xE000-#xFFFD], [#x10000-#x10FFFF],
    ///         and any Unicode character; excluding the surrogate blocks FFFE and FFFF.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="content"/> is an empty string.</exception>
    public static string RemoveInvalidXmlHexadecimalCharacters(string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(content);

        // Almost every real document is already clean, and the old implementation still allocated a
        // StringBuilder the size of the whole document and rebuilt it character by character to
        // discover that. Scan first and hand back the original instance when there is nothing to
        // remove; only pay for a rebuild when a character actually has to go.
        int firstInvalid = IndexOfInvalidXmlCharacter(content);
        if (firstInvalid < 0)
        {
            return content;
        }

        // Adapted from https://stackoverflow.com/a/17735649
        StringBuilder result = new(content.Length);
        result.Append(content.AsSpan(0, firstInvalid));

        for (int i = firstInvalid; i < content.Length; i++)
        {
            if (XmlConvert.IsXmlChar(content[i]))
            {
                result.Append(content[i]);
            }
            else if (i + 1 < content.Length && XmlConvert.IsXmlSurrogatePair(content[i + 1], content[i]))
            {
                result.Append(content[i]);
                result.Append(content[i + 1]);
                i++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Returns the index of the first character that <see cref="RemoveInvalidXmlHexadecimalCharacters(string)"/> would drop.
    /// </summary>
    /// <param name="content">The content to scan.</param>
    /// <returns>The index of the first character that would be removed, or <b>-1</b> if the content is already valid.</returns>
    /// <remarks>
    ///     The two keep conditions mirror the rebuild loop exactly: a character survives if it is a valid
    ///     XML character, or if it opens a valid surrogate pair with the character after it.
    /// </remarks>
    private static int IndexOfInvalidXmlCharacter(string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            if (XmlConvert.IsXmlChar(content[i]))
            {
                continue;
            }

            if (i + 1 < content.Length && XmlConvert.IsXmlSurrogatePair(content[i + 1], content[i]))
            {
                i++;
                continue;
            }

            return i;
        }

        return -1;
    }

    /// <summary>
    /// Converts a string into a value that can be safely used as a <see cref="Directory">directory</see> name.
    /// </summary>
    /// <param name="name">The directory name to encode.</param>
    /// <returns>A string that can be safely used as an argument when <see cref="Directory.CreateDirectory(string)">creating a directory</see>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static string EncodeSafeDirectoryName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        // Fast path: check if any invalid characters exist
        if (!name.ContainsAny(s_invalidDirectoryChars))
        {
            return name;
        }

        // Remove invalid characters using StringBuilder
        StringBuilder result = new(name.Length);
        foreach (char c in name)
        {
            if (!s_invalidDirectoryChars.Contains(c))
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Gets an array of bytes that represent the data of the supplied <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to get an array of bytes for.</param>
    /// <returns>An array of bytes that represent the data of the supplied <paramref name="stream"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <remarks>
    ///     Internal rather than private so the benchmark harness can measure this stage of the load
    ///     pipeline directly. It is deliberately not public: exposing it would make it a permanent
    ///     contract on a published library, and nothing outside this assembly needs it.
    /// </remarks>
    internal static byte[] GetStreamBytes(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // A seekable stream already knows how much is left, so the result array can be sized exactly
        // and filled once. This replaces a read-and-double loop that allocated every intermediate
        // buffer on the way up and copied the whole payload at each growth; Stream.ReadExactly
        // arrived in .NET 7 and makes the loop unnecessary.
        if (stream.CanSeek)
        {
            long remaining = stream.Length - stream.Position;
            if (remaining == 0)
            {
                return [];
            }

            byte[] exact = new byte[remaining];
            stream.ReadExactly(exact);
            return exact;
        }

        // Non-seekable streams still need to be drained; MemoryStream grows with a pooled copy loop
        // rather than a hand-written one.
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}