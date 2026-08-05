using System.Buffers;
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
    /// <remarks>
    ///     <para>
    ///     Public because a caller cannot otherwise discover what deadline they are subject to. The
    ///     shared <see cref="HttpClient"/> is deliberately constructed with
    ///     <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> — every deadline in this library
    ///     comes from a <see cref="CancellationTokenSource"/>, so reading
    ///     <see cref="HttpClient.Timeout"/> tells the caller nothing, and there was previously no
    ///     value to read that did.
    ///     </para>
    ///     <para>
    ///     It is also the default of <see cref="SyndicationResourceLoadSettings.Timeout"/>, which used
    ///     to write <c>TimeSpan.FromSeconds(100)</c> out a second time in a different assembly's file.
    ///     Two independent spellings of one number is one edit away from two different numbers.
    ///     </para>
    /// </remarks>
    public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Characters that are invalid in directory names.
    /// </summary>
    private static readonly SearchValues<char> s_invalidDirectoryChars = SearchValues.Create(@"\/:*?<>|");

    /// <summary>
    /// Every code unit in the basic multilingual plane that is not a valid XML character.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Built by asking <see cref="XmlConvert.IsXmlChar(char)"/> rather than by writing the ranges
    ///     out, so the set cannot drift from the predicate it stands in for. It costs one pass over
    ///     65,536 values, once, on first use.
    ///     </para>
    ///     <para>
    ///     <b>This is a candidate finder, not a predicate.</b> It necessarily contains the whole of
    ///     <c>[D800-DFFF]</c> — a surrogate is not a valid XML character on its own — so it fires on
    ///     every astral character, including the perfectly valid ones. The pair rule is stateful and
    ///     stays where it is; what the set buys is skipping the runs in between.
    ///     </para>
    /// </remarks>
    private static readonly SearchValues<char> s_invalidXmlChars = SearchValues.Create(BuildInvalidXmlCharacters());

    /// <summary>
    /// The number of leading characters of a document examined when looking for an XML declaration.
    /// </summary>
    /// <remarks>
    ///     The declaration must be the first thing in the document and cannot legally exceed this,
    ///     so nothing beyond it can affect the result.
    /// </remarks>
    private const int XmlDeclarationProbeLength = 512;

    /// <summary>
    /// The furthest into a document the streaming load will look for the end of an XML declaration.
    /// </summary>
    /// <remarks>
    ///     A declaration may legally carry unbounded whitespace between its pseudo-attributes, so there
    ///     is no length at which one provably cannot still be open. Reading without a bound would put a
    ///     whole document back in memory for a pathological input, which is what the streaming load
    ///     exists to avoid — so the bound is stated rather than left implicit. Beyond it the sniff
    ///     answers <see cref="Encoding.UTF8"/>. <see cref="GetXmlEncoding(byte[])"/> is unbounded and
    ///     unaffected, so the two can disagree only on a declaration longer than this.
    /// </remarks>
    private const int MaxDeclarationProbeLength = 64 * 1024;

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
            // Pooling policy is the singleton's own business and deliberately not shared: a static
            // client must rotate its own connections, where a factory-built one has its whole handler
            // rotated for it.
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        };

        ApplyArgoticHandlerDefaults(handler);
        return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
    }

    /// <summary>
    /// Applies the handler settings every Argotic HTTP pipeline shares.
    /// </summary>
    /// <param name="handler">The handler to configure.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="handler"/> is a null reference.</exception>
    /// <remarks>
    ///     <para>
    ///     Named so that the shared client and anything built by <c>IHttpClientFactory</c> cannot drift
    ///     apart. Keeping them in step by remembering to is a coordination requirement between two
    ///     assemblies with nothing enforcing it, and the failure is silent — a factory-built client that
    ///     keeps cookies while the singleton does not.
    ///     </para>
    ///     <para>
    ///     <b>Cookies are off.</b> <see cref="SocketsHttpHandler.UseCookies"/> defaults to
    ///     <see langword="true"/>, so a <c>Set-Cookie</c> from any origin was replayed on the next
    ///     request to that host — and on a process-wide singleton that means per-domain session state
    ///     accumulating for the lifetime of the application, with no API to inspect or clear it. A feed
    ///     reader has no use for a cookie jar.
    ///     </para>
    ///     <para>
    ///     <b>Brotli is on.</b> It has been in the platform since .NET Core 3.0 and is what most origins
    ///     prefer; advertising only gzip and deflate meant declining the smallest encoding available.
    ///     </para>
    /// </remarks>
    public static void ApplyArgoticHandlerDefaults(SocketsHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
        handler.UseCookies = false;
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
    /// Creates <see cref="XmlWriterSettings"/> for writing a syndication entity as an XML fragment.
    /// </summary>
    /// <param name="encoding">The character encoding to write with, or <b>null</b> to leave the writer's default.</param>
    /// <returns>Settings that indent, omit the XML declaration, and permit a fragment rather than a whole document.</returns>
    /// <remarks>
    ///     Entities are written as fragments because they are composed into a document by their parent.
    ///     This shape was repeated inline at 66 call sites before being named here.
    /// </remarks>
    public static XmlWriterSettings CreateFragmentXmlWriterSettings(Encoding? encoding = null)
    {
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true,
        };

        if (encoding is not null)
        {
            settings.Encoding = encoding;
        }

        return settings;
    }

    /// <summary>
    /// Creates <see cref="XmlWriterSettings"/> for writing a complete syndication document.
    /// </summary>
    /// <param name="encoding">The character encoding to write with, or <b>null</b> to leave the writer's default.</param>
    /// <returns>Settings that indent, emit the XML declaration, and require a well-formed document.</returns>
    /// <remarks>This shape was repeated inline at 12 call sites before being named here.</remarks>
    public static XmlWriterSettings CreateDocumentXmlWriterSettings(Encoding? encoding = null)
    {
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Document,
            Indent = true,
            OmitXmlDeclaration = false,
        };

        if (encoding is not null)
        {
            settings.Encoding = encoding;
        }

        return settings;
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
    /// <remarks>
    ///     Filters through the same streaming reader the other overloads use, so a dirty document is no
    ///     longer rebuilt into a second string before parsing. The explicit guard stays: unlike the
    ///     stream and reader overloads, this one really does have a parameter called <c>xml</c>, so
    ///     rejecting an empty one by name is the caller's own contract rather than a leak from somewhere
    ///     else.
    /// </remarks>
    public static XPathNavigator CreateSafeNavigator(string xml)
    {
        ArgumentException.ThrowIfNullOrEmpty(xml);

        using StringReader stringReader = new(xml);
        using XmlSanitizingTextReader sanitising = new(stringReader, leaveOpen: true);
        using XmlReader xmlReader = XmlReader.Create(sanitising, CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);

        return document.CreateNavigator();
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
    /// <remarks>
    ///     <para>
    ///     Reads a bounded head, sniffs the declaration from it, then decodes the head and the
    ///     remainder as one stream. The document is never buffered whole, and never becomes a string.
    ///     </para>
    ///     <para>
    ///     <b>The head is filled with <c>ReadAtLeast</c>, not one <c>Read</c>.</b> A single read on a
    ///     network stream routinely returns far less than asked for, so a naive read would sniff
    ///     whatever happened to be in the first TCP segment.
    ///     </para>
    ///     <para>
    ///     <b>The head grows when a declaration did not close inside it.</b> Whitespace between the
    ///     pseudo-attributes of a declaration is legal and unbounded, so one can run past any fixed
    ///     window. Growing on that condition — rather than on "no <c>encoding=</c> was found" — is what
    ///     keeps the answer identical to reading the document whole. Past
    ///     <see cref="MaxDeclarationProbeLength"/> the sniff gives up and returns
    ///     <see cref="Encoding.UTF8"/>; <see cref="GetXmlEncoding(byte[])"/> remains unbounded and is
    ///     unaffected.
    ///     </para>
    ///     <para>
    ///     Two deliberate changes come with this. An empty stream now produces
    ///     <see cref="System.Xml.XmlException"/> rather than <see cref="ArgumentException"/> naming
    ///     <c>content</c> — the parameter of a private helper three calls down, which this method's
    ///     caller never supplied. And the stream is consumed lazily, so a parse failure part-way
    ///     through leaves it part-way through rather than drained.
    ///     </para>
    /// </remarks>
    public static XPathNavigator CreateSafeNavigator(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] head = new byte[XmlDeclarationProbeLength];
        int filled = stream.ReadAtLeast(head, head.Length, throwOnEndOfStream: false);

        Encoding encoding = SniffXmlEncoding(head.AsSpan(0, filled), out bool declarationMayOverrun);

        // Only grow while the window is genuinely full: a short fill means the document ended, so
        // there is nothing more to find and re-reading would loop.
        while (declarationMayOverrun && filled == head.Length && head.Length < MaxDeclarationProbeLength)
        {
            byte[] grown = new byte[Math.Min(head.Length * 2, MaxDeclarationProbeLength)];
            head.AsSpan(0, filled).CopyTo(grown);
            head = grown;

            filled += stream.ReadAtLeast(head.AsSpan(filled), head.Length - filled, throwOnEndOfStream: false);
            encoding = SniffXmlEncoding(head.AsSpan(0, filled), out declarationMayOverrun);
        }

        using PrefixedStream prefixed = new(head, filled, stream);
        using StreamReader decoded = new(prefixed, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        using XmlSanitizingTextReader sanitising = new(decoded, leaveOpen: true);
        using XmlReader xmlReader = XmlReader.Create(sanitising, CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);

        return document.CreateNavigator();
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
    /// <remarks>
    ///     <para>
    ///     <b>This overload no longer closes the caller's stream.</b> It wrapped it in a
    ///     <see cref="StreamReader"/> it owned and disposed, which closed the stream as a side effect —
    ///     while the single-argument overload, which looks symmetrical, did not. Nothing documented the
    ///     difference and no test observed it. Callers who relied on it to dispose their stream must now
    ///     do so themselves.
    ///     </para>
    ///     <para>
    ///     A byte-order mark still takes precedence over <paramref name="encoding"/>. That is what the
    ///     two-argument <see cref="StreamReader"/> constructor did, so it is preserved rather than
    ///     quietly corrected, and the flag is now passed explicitly instead of inherited.
    ///     </para>
    ///     <para>
    ///     An empty stream produces <see cref="System.Xml.XmlException"/> rather than
    ///     <see cref="ArgumentException"/> naming <c>xml</c> — the parameter of the string overload this
    ///     one used to delegate to, which this method's caller never supplied.
    ///     </para>
    /// </remarks>
    public static XPathNavigator CreateSafeNavigator(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);

        using StreamReader reader = new(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        using XmlSanitizingTextReader sanitising = new(reader, leaveOpen: true);
        using XmlReader xmlReader = XmlReader.Create(sanitising, CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);

        return document.CreateNavigator();
    }

    /// <summary>
    /// Creates a <see cref="XPathNavigator"/> over a stream, honouring the caller's load settings.
    /// </summary>
    /// <param name="stream">The stream to navigate.</param>
    /// <param name="settings">The load settings. This value can be <b>null</b>.</param>
    /// <returns>A navigator over the supplied <paramref name="stream"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     The synchronous counterpart of the settings-taking <c>CreateSafeNavigatorAsync</c>, and the same
    ///     motivation: twelve <c>Load(Stream, settings)</c> implementations each carried an identical
    ///     branch on <c>settings is not null</c>, so what a settings object means for encoding was
    ///     written down twelve times.
    ///     </para>
    ///     <para>
    ///     <b>Behaviour is unchanged and is presently wrong</b> — a non-null settings object forces
    ///     <see cref="SyndicationResourceLoadSettings.CharacterEncoding"/>, whose default overrides a
    ///     correctly declared <c>iso-8859-1</c>. That is pinned by
    ///     <c>SettingsEncodingCharacterisationTests</c> and fixed by the commit that makes the
    ///     property nullable. Gathering it here first is what lets that commit be one edit.
    ///     </para>
    ///     <para>
    ///     Internal rather than public, and not only for scope. <c>CreateSafeNavigator(stream, null)</c>
    ///     would become ambiguous against the <see cref="Encoding"/> overload, and there is a call of
    ///     exactly that shape in <c>ParseEntryPointGuardTests</c> — which, being in the test project,
    ///     cannot see this one.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    internal static XPathNavigator CreateSafeNavigator(Stream stream, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // The condition is now "did the caller name an encoding", where it used to be "did the caller
        // supply a settings object at all". Those were the same question only because the property
        // could not be left unset.
        return settings?.CharacterEncoding is { } encoding
            ? CreateSafeNavigator(stream, encoding)
            : CreateSafeNavigator(stream);
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
    /// <remarks>
    ///     <para>
    ///     Filters as it reads. The other overloads still drain their input to a string first; this one
    ///     no longer does, so a document arrives at the parser without ever being held twice.
    ///     </para>
    ///     <para>
    ///     Two consequences, both deliberate. An empty reader now produces
    ///     <see cref="System.Xml.XmlException"/> — "Root element is missing" — rather than
    ///     <see cref="ArgumentException"/> naming <c>xml</c>, a parameter this overload does not have and
    ///     the caller never supplied. And the reader is consumed lazily, so a parse failure part-way
    ///     through leaves it part-way through rather than drained.
    ///     </para>
    ///     <para>
    ///     The caller's reader is not disposed. Nothing should close what it did not open.
    ///     </para>
    /// </remarks>
    public static XPathNavigator CreateSafeNavigator(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using XmlSanitizingTextReader sanitising = new(reader, leaveOpen: true);
        using XmlReader xmlReader = XmlReader.Create(sanitising, CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);

        return document.CreateNavigator();
    }

    /// <summary>
    /// Drains a response body asynchronously, refusing to exceed <paramref name="maxBytes"/>.
    /// </summary>
    /// <param name="response">The response whose body to read.</param>
    /// <param name="maxBytes">The most this will accept. Pass <see cref="SyndicationResourceLoadSettings.Unbounded"/> for no limit.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The drained body. The caller owns it and must dispose it.</returns>
    /// <remarks>
    ///     <para>
    ///     The rule this exists to enforce: <b><see cref="HttpCompletionOption.ResponseHeadersRead"/> is
    ///     correct exactly where the body is consumed asynchronously or not at all.</b> Every site that
    ///     hands the stream to a synchronous reader — an <c>XPathDocument</c>, an <c>XmlReader</c> over a
    ///     <see cref="Stream"/>, <c>ReadToEnd</c>, <c>CopyTo</c> — must come through here first, or it
    ///     performs a blocking drain of a socket on a thread-pool thread.
    ///     </para>
    ///     <para>
    ///     The declared-length check is <b>an optimisation, not the defence</b>. Automatic decompression
    ///     strips <c>Content-Length</c> from every response it decompresses, so on a compressing origin
    ///     it never fires at all. The streaming counter below it is the only guaranteed bound, and it
    ///     counts <i>decompressed</i> bytes — which is the right unit, because a few kilobytes of gzip
    ///     can expand to a megabyte.
    ///     </para>
    ///     <para>
    ///     The limit is checked <b>before</b> each chunk is written, so the buffer never holds more than
    ///     the cap. Reading to the end and then comparing totals would have spent the memory the cap
    ///     exists to save.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference.</exception>
    /// <exception cref="SyndicationContentTooLargeException">The body exceeds <paramref name="maxBytes"/>.</exception>
    internal static async Task<PooledContentBuffer> ReadContentAsync(
        HttpResponseMessage response,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await ReadContentAsync(
            responseStream, response.Content.Headers.ContentLength, maxBytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads a stream into a pooled buffer, refusing one longer than <paramref name="maxBytes"/>.
    /// </summary>
    /// <param name="stream">The stream to drain. The caller keeps ownership of it.</param>
    /// <param name="declaredLength">The length the origin declared, or <b>null</b> if it declared none.</param>
    /// <param name="maxBytes">The most to accept.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The drained body. The caller owns it and must dispose it.</returns>
    /// <remarks>
    ///     Split out from the response-taking overload for the conditional load path, which holds a
    ///     stream rather than a response — the body having been left unread on purpose, so that
    ///     deciding not to want it costs nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="SyndicationContentTooLargeException">The stream exceeds <paramref name="maxBytes"/>.</exception>
    internal static async Task<PooledContentBuffer> ReadContentAsync(
        Stream stream,
        long? declaredLength,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (declaredLength is { } declared && declared > maxBytes)
        {
            throw new SyndicationContentTooLargeException(maxBytes, declared);
        }

        // Nulled once ownership passes to the caller, so the finally disposes it on every failure
        // path and on none of the success path.
        PooledContentBuffer? sink = PooledContentBuffer.ForDeclaredLength(declaredLength);
        byte[] chunk = ArrayPool<byte>.Shared.Rent(81_920);

        try
        {
            long total = 0;
            int read;
            while ((read = await stream.ReadAsync(chunk.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
            {
                total += read;
                if (total > maxBytes)
                {
                    // Thrown before the write, so the sink never holds more than the cap. Unwinding
                    // disposes the caller's response, which aborts the connection rather than returning
                    // an undrained one to the pool - so nothing further is read from the socket.
                    throw new SyndicationContentTooLargeException(maxBytes, declaredLength);
                }

                sink.Write(chunk.AsSpan(0, read));
            }

            PooledContentBuffer drained = sink;
            sink = null;
            return drained;
        }
        finally
        {
            sink?.Dispose();
            ArrayPool<byte>.Shared.Return(chunk);
        }
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
    /// Fetches a resource and returns a navigator over it, honouring the caller's load settings.
    /// </summary>
    /// <param name="source">The resource to fetch.</param>
    /// <param name="httpClient">The client to fetch with.</param>
    /// <param name="settings">The load settings. Its encoding and size cap are both honoured.</param>
    /// <param name="defaultMaxResponseContentLength">
    ///     The size cap to apply when <see cref="SyndicationResourceLoadSettings.MaxResponseContentLength"/>
    ///     is unset — the format default for whichever type is loading.
    /// </param>
    /// <param name="requestOptions">Request-level options. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A navigator over the fetched document.</returns>
    /// <remarks>
    ///     <para>
    ///     <b>The cap default comes from the caller, not from the settings object.</b> A settings object
    ///     cannot know what kind of document is being loaded, and the answer differs by a factor of
    ///     eight between a feed and a sitemap. So the loading type passes its own default and the
    ///     settings override it when set — which is what lets a caller construct settings for an
    ///     unrelated reason without silently losing their format's allowance.
    ///     </para>
    ///     <para>
    ///     Taking the settings whole is also what removes the encoding sentinel from thirteen call
    ///     sites. Each one used to translate <c>CharacterEncoding == Encoding.UTF8</c> into
    ///     <see langword="null"/> before calling; reading the property here means those lines are
    ///     deleted rather than rewritten.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="SyndicationContentTooLargeException">The response exceeds the effective size cap.</exception>
    internal static async Task<XPathNavigator> CreateSafeNavigatorAsync(
        Uri source,
        HttpClient httpClient,
        SyndicationResourceLoadSettings settings,
        long defaultMaxResponseContentLength,
        SyndicationRequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(settings);

        long cap = settings.MaxResponseContentLength ?? defaultMaxResponseContentLength;

        // The deadline is applied here rather than at each of the thirteen callers, all of which
        // built this same linked source, applied this same value, and used the token for nothing
        // but the call below. It covers the body read as well as the headers - which is the point
        // of a CancellationTokenSource rather than HttpClient.Timeout, and matters more now that
        // the send completes on headers.
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (settings.Timeout is { } deadline)
        {
            // Left unarmed when null. The linked source is still built, so the caller's own token
            // still cancels the load - what a null removes is the library's deadline, not theirs.
            timeoutCts.CancelAfter(deadline);
        }

        using HttpResponseMessage response = await SendHttpRequestAsync(
            source, httpClient, requestOptions, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using PooledContentBuffer body = await ReadContentAsync(response, cap, timeoutCts.Token).ConfigureAwait(false);
        using Stream stream = body.AsStream();

        return settings.CharacterEncoding is { } encoding
            ? CreateSafeNavigator(stream, encoding)
            : CreateSafeNavigator(stream);
    }

    /// <summary>
    /// Reads at most <paramref name="maxBytes"/> of a response body, treating a longer body as normal.
    /// </summary>
    /// <param name="response">The response whose body to read.</param>
    /// <param name="maxBytes">The most to read.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The leading bytes of the body. The caller owns it and must dispose it.</returns>
    /// <remarks>
    ///     Distinct from <c>ReadContentAsync</c> in what an overrun means. There, exceeding the
    ///     limit is an error and throws; here it is the expected case — the caller wants a head and does
    ///     not care that more exists. Detecting a document's format needs its first element, not its
    ///     contents.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="response"/> is a null reference.</exception>
    internal static async Task<PooledContentBuffer> ReadContentPrefixAsync(
        HttpResponseMessage response,
        int maxBytes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        PooledContentBuffer? sink = PooledContentBuffer.ForDeclaredLength(maxBytes);
        byte[] chunk = ArrayPool<byte>.Shared.Rent(Math.Min(maxBytes, 81_920));

        try
        {
            int total = 0;
            while (total < maxBytes)
            {
                int wanted = Math.Min(chunk.Length, maxBytes - total);
                int read = await stream.ReadAsync(chunk.AsMemory(0, wanted), cancellationToken).ConfigureAwait(false);
                if (read <= 0)
                {
                    break;
                }

                sink.Write(chunk.AsSpan(0, read));
                total += read;
            }

            PooledContentBuffer head = sink;
            sink = null;
            return head;
        }
        finally
        {
            sink?.Dispose();
            ArrayPool<byte>.Shared.Return(chunk);
        }
    }
    /// <summary>
    /// Sends a request, choosing when the returned task completes.
    /// </summary>
    /// <param name="source">The resource to request.</param>
    /// <param name="httpClient">The client to send with.</param>
    /// <param name="requestOptions">Request-level options. This value can be <b>null</b>.</param>
    /// <param name="completionOption">When the returned task completes.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The response. The caller owns it.</returns>
    /// <remarks>
    ///     <para>
    ///     <c>internal</c> rather than public, and no default on the trailing parameters, so that
    ///     <c>SendHttpRequestAsync(uri, client)</c> still binds the public overload uniquely. Nothing
    ///     outside this assembly needs to choose a completion option yet, and adding an optional
    ///     parameter to the public method instead would have been a binary break for a published
    ///     library.
    ///     </para>
    ///     <para>
    ///     <see cref="HttpCompletionOption.ResponseHeadersRead"/> hands back a live network stream, so
    ///     the caller becomes responsible for reading it — asynchronously, and under a bound. See
    ///     <c>ReadContentAsync</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    internal static async Task<HttpResponseMessage> SendHttpRequestAsync(
        Uri source,
        HttpClient httpClient,
        SyndicationRequestOptions? requestOptions,
        HttpCompletionOption completionOption,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        using HttpRequestMessage request = CreateHttpRequestMessage(source, requestOptions);
        return await httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
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

        // The declaration sits at offset 0, so sniffing the head of the document is normally enough.
        // The previous implementation decoded the whole document and ran the regex over all of it,
        // which on a 1000-item feed cost megabytes to read a few dozen bytes.
        if (data.Length > XmlDeclarationProbeLength)
        {
            Encoding sniffed = SniffXmlEncoding(data.AsSpan(0, XmlDeclarationProbeLength), out bool declarationMayOverrun);
            if (!declarationMayOverrun)
            {
                return sniffed;
            }
        }

        // Decoding the whole array, which is what a declaration longer than the probe window costs and
        // why this overload is documented as unbounded. Routed through the string overload rather than
        // the regex directly, so an empty array still produces the ArgumentException that overload's
        // guard raises.
        using MemoryStream stream = new(data);
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
    /// Reads the character encoding out of an XML declaration at the head of a document.
    /// </summary>
    /// <param name="window">The leading bytes of the document.</param>
    /// <param name="declarationMayOverrun">
    ///     On return, <see langword="true"/> when the window opens an XML declaration whose closing
    ///     <c>?&gt;</c> is not inside it, so the answer is not yet known and more bytes are needed.
    /// </param>
    /// <returns>The declared encoding, or <see cref="Encoding.UTF8"/> when none is declared, the name is
    /// unknown, or the window is empty.</returns>
    /// <remarks>
    ///     <para>
    ///     <b>The flag is not optional.</b> Three different situations all produce
    ///     <see cref="Encoding.UTF8"/> — the document declared UTF-8, the document declared nothing, and
    ///     the document opened a declaration this window could not see the end of. Only the third means
    ///     "ask again with more bytes", and a caller cannot tell them apart from the return value alone.
    ///     </para>
    ///     <para>
    ///     The trigger is <b>a declaration that did not close</b>, not <b>an <c>encoding=</c> that was
    ///     not found</b>, and the difference is a silent mis-decode. The declaration regex requires the
    ///     closing <c>?&gt;</c>, so a declaration whose <c>encoding=</c> sits inside the window but whose
    ///     <c>?&gt;</c> does not fails to match even though the answer was right there. Growing on
    ///     "no <c>encoding=</c>" would stop early on exactly that document and answer <c>utf-8</c> for a
    ///     feed that said <c>iso-8859-1</c>. Row 15b of the encoding matrix is that document.
    ///     </para>
    ///     <para>
    ///     Decoding goes through <see cref="StreamReader"/> rather than a hand-rolled preamble scan.
    ///     A byte-order mark has to be detected and stripped before the regex sees the text or a UTF-16
    ///     document decodes to nonsense, and the three byte-order-mark tests in this repository all
    ///     declare an encoding matching their own mark — so a decoder that forgot to strip it would pass
    ///     every one of them. Copying ≤512 bytes to let the framework do it is worth more than the
    ///     allocation it costs.
    ///     </para>
    /// </remarks>
    internal static Encoding SniffXmlEncoding(ReadOnlySpan<byte> window, out bool declarationMayOverrun)
    {
        declarationMayOverrun = false;

        if (window.IsEmpty)
        {
            return Encoding.UTF8;
        }

        using MemoryStream head = new(window.ToArray(), writable: false);
        using StreamReader headReader = new(head, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string prefix = headReader.ReadToEnd();

        Match prefixMatch = XmlDeclarationEncodingRegex().Match(prefix);
        if (prefixMatch.Success)
        {
            return SyndicationEncodingUtility.EncodingFromMatch(prefixMatch);
        }

        declarationMayOverrun = prefix.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase);
        return Encoding.UTF8;
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
        int consumed = 0;

        while (true)
        {
            int candidate = content.AsSpan(consumed).IndexOfAny(s_invalidXmlChars);
            if (candidate < 0)
            {
                return -1;
            }

            int index = consumed + candidate;

            // The set is a candidate finder rather than an answer: it contains every surrogate, so a
            // valid astral character lands here too. Resume the vectorised search past a genuine pair
            // instead of falling into a scalar walk for the remainder - otherwise one emoji early in a
            // document would cost the whole rest of it.
            if (index + 1 < content.Length && XmlConvert.IsXmlSurrogatePair(content[index + 1], content[index]))
            {
                consumed = index + 2;
                continue;
            }

            return index;
        }
    }

    /// <summary>
    /// Enumerates every code unit that <see cref="XmlConvert.IsXmlChar(char)"/> rejects.
    /// </summary>
    /// <returns>The complement of the valid XML character set over the basic multilingual plane.</returns>
    private static string BuildInvalidXmlCharacters()
    {
        StringBuilder builder = new(2_079);

        for (int codeUnit = 0; codeUnit <= 0xFFFF; codeUnit++)
        {
            if (!XmlConvert.IsXmlChar((char)codeUnit))
            {
                builder.Append((char)codeUnit);
            }
        }

        return builder.ToString();
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
}