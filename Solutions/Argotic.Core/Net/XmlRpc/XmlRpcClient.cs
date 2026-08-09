using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Argotic.Net;

/// <summary>
/// Sends remote procedure calls using the XML-RPC protocol.
/// </summary>
/// <remarks>
///     <para>
///         A call is an HTTP <c>POST</c> of <c>text/xml</c>: a <c>&lt;methodCall&gt;</c> naming a method
///         and carrying its parameters, answered by a <c>&lt;methodResponse&gt;</c> holding either one
///         return value or a fault structure. Parameters are scalars, arrays or structures, and the last
///         two nest. This implementation follows the XML-RPC 1.0 specification at
///         <a href="https://xmlrpc.com/spec.md">https://xmlrpc.com/spec.md</a>.
///     </para>
///     <para>
///         The endpoint this library expects to reach is a Pingback server —
///         <see cref="SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(string)"/> finds one,
///         and <c>pingback.ping</c> is an XML-RPC call. Pingback is the sibling of Trackback and is
///         legacy for the same reasons: the ping asserts a link with no authentication behind it, the
///         spam followed, and most weblog software stopped accepting them years ago. Nothing here is
///         specific to Pingback, though; the client will call any XML-RPC endpoint.
///     </para>
///     <para>
///         A fault is not an exception. The server answers <c>200 OK</c> and puts the failure in the
///         body, so a <see cref="SendAsync"/> that returns normally may still have failed — read
///         <see cref="XmlRpcResponse.Fault"/>.
///     </para>
///     <para>
///         For scenarios requiring authentication or proxy configuration, provide a pre-configured <see cref="HttpClient"/>
///         via the constructor. This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Net\XmlRpcClientExample.cs" language="cs" title="The following code example demonstrates the usage of the XmlRpcClient class." />
/// </example>
public class XmlRpcClient
{
    /// <summary>
    /// The deepest a <c>&lt;struct&gt;</c> or <c>&lt;array&gt;</c> may nest before the parser stops descending.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     A bound, not a preference. Parsing a composite re-enters
    ///     <see cref="TryParseValue(XPathNavigator, out IXmlRpcValue?)"/> — two stack frames per level
    ///     for an array, three for a structure — and the recursion is not tail-recursive, so without a
    ///     limit a sufficiently nested reply overflows the reading process's stack. That is a process
    ///     kill, not a catchable exception, and it is cheap to provoke: a nesting level costs 43 bytes
    ///     of document for an array and 49 for a structure, against the 2 MiB body
    ///     <see cref="XmlRpcResponse.CreateAsync"/> already allows. Roughly 176 KB is enough against the
    ///     1 MiB stack a thread-pool thread gets, and the response is read on one.
    ///     </para>
    ///     <para>
    ///     Sixty-four is generous by three orders of magnitude against real traffic: XML-RPC's own
    ///     vocabulary nests two or three levels, and a <c>metaWeblog.getRecentPosts</c> reply — an array
    ///     of structures of arrays of scalars — is three.
    ///     </para>
    ///     <para>
    ///     A constant rather than a setting because neither <c>SyndicationResourceLoadSettings</c> nor
    ///     <see cref="SyndicationRequestOptions"/> is reachable from these methods:
    ///     <see cref="TryParseValue(XPathNavigator, out IXmlRpcValue?)"/> is <see langword="static"/>
    ///     with no instance state, and nothing under the XML-RPC or Trackback implementations mentions
    ///     either type.
    ///     </para>
    /// </remarks>
    public const int MaxValueNestingDepth = 64;

    /// <summary>
    /// Private member to hold the HttpClient used for sending requests.
    /// </summary>
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClient"/> class using the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    ///     This constructor uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.
    ///     For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    public XmlRpcClient()
    {
        this.httpClient = SyndicationEncodingUtility.SharedHttpClient;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClient"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to configure this client.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     This constructor is intended for use with dependency injection and the <see cref="IOptions{TOptions}"/> pattern.
    /// </remarks>
    public XmlRpcClient(IOptions<XmlRpcClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.httpClient = SyndicationEncodingUtility.SharedHttpClient;
        ApplyOptions(options.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClient"/> class with the specified options and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="options">The options to configure this client.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for sending requests. The caller is responsible for managing the client's lifecycle.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     <para>
    ///     This constructor is intended for use with dependency injection and the <see cref="IOptions{TOptions}"/> pattern.
    ///     </para>
    ///     <para>
    ///     Marked as the preferred constructor because three of this type’s six accept an
    ///     <see cref="HttpClient"/>, and <c>ActivatorUtilities.CreateFactory</c> — which is how a typed
    ///     client is activated — refuses to choose between them. Without the attribute,
    ///     <c>AddHttpClient&lt;T&gt;</c> throws at registration.
    ///     </para>
    /// </remarks>
    [ActivatorUtilitiesConstructor]
    public XmlRpcClient(IOptions<XmlRpcClientOptions> options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
        ApplyOptions(options.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClient"/> class that sends remote procedure calls using the specified XML-RPC server.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</param>
    /// <remarks>
    ///     This constructor uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.
    ///     For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is <see langword="null"/>.</exception>
    public XmlRpcClient(Uri host) : this()
    {
        this.Host = host;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClient"/> class that sends remote procedure calls using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for sending requests. The caller is responsible for managing the client's lifecycle.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public XmlRpcClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlRpcClient"/> class that sends remote procedure calls using the specified <see cref="HttpClient"/> and XML-RPC server.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for sending requests. The caller is responsible for managing the client's lifecycle.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public XmlRpcClient(Uri host, HttpClient httpClient) : this(httpClient)
    {
        this.Host = host;
    }

    /// <summary>
    /// Gets or sets the location of the host computer that client remote procedure calls will be sent to.
    /// </summary>
    /// <value>
    ///     The XML-RPC endpoint URL. The default value is <see langword="null"/>, in which case
    ///     <see cref="SendAsync"/> throws <see cref="InvalidOperationException"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Host
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time-out.
    /// </summary>
    /// <value>The time-out period. The default value is 15 seconds. The permitted range is zero to 365 days, inclusive.</value>
    /// <remarks>
    ///     Enforced by <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> on a source linked to
    ///     the token passed to <see cref="SendAsync"/>, not by <see cref="HttpClient.Timeout"/> — the
    ///     shared client is built with <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>. The
    ///     deadline therefore covers reading the response body as well as the request, and expiry
    ///     surfaces as <see cref="OperationCanceledException"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is greater than a year.</exception>
    public TimeSpan Timeout
    {
        get;

        set
        {
            if (value.TotalMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else if (value > TimeSpan.FromDays(365))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            else
            {
                field = value;
            }
        }
    } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets or sets information such as the client application name, version, host operating system, and language.
    /// </summary>
    /// <value>The <c>User-Agent</c> header value, trimmed. The default value is <c>Argotic-Syndication-Framework/</c> followed by this assembly's four-part version.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    // Both null-forgiving operators in the default value are provable. Assembly.GetAssembly returns
    // null only for a type with no backing assembly, which a typeof() of a type declared here cannot
    // be, and AssemblyName.Version is always populated because the SDK emits an assembly version
    // whether or not one is set explicitly.
    public string UserAgent
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = $"Argotic-Syndication-Framework/{System.Reflection.Assembly.GetAssembly(typeof(XmlRpcClient))!.GetName().Version!.ToString(4)}";

    /// <summary>
    /// Returns the scalar type identifier for the supplied <see cref="XmlRpcScalarValueType"/>.
    /// </summary>
    /// <param name="type">The <see cref="XmlRpcScalarValueType"/> to get the scalar type identifier for.</param>
    /// <returns>
    ///     The element name XML-RPC uses for the type, such as <c>int</c> or <c>dateTime.iso8601</c>;
    ///     an <i>empty</i> string for <see cref="XmlRpcScalarValueType.None"/> or a value outside the
    ///     enumeration.
    /// </returns>
    public static string ScalarTypeAsString(XmlRpcScalarValueType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="XmlRpcScalarValueType"/> enumeration value that corresponds to the specified scalar type name.
    /// </summary>
    /// <param name="name">The XML-RPC element name, such as <c>string</c> or <c>dateTime.iso8601</c>. Matched without regard to case.</param>
    /// <returns>The matching <see cref="XmlRpcScalarValueType"/>; otherwise, <see cref="XmlRpcScalarValueType.None"/>, which is also what an unrecognised, <see langword="null"/> or empty <paramref name="name"/> yields.</returns>
    /// <remarks>
    ///     <c>i4</c> is not recognised here. XML-RPC permits it as a synonym for <c>int</c>, and callers
    ///     that need to accept it map it themselves before asking.
    /// </remarks>
    public static XmlRpcScalarValueType ScalarTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, XmlRpcScalarValueType.None);

    /// <summary>
    /// Constructs a new <see cref="IXmlRpcValue"/> object from the specified <see cref="XPathNavigator"/>.
    /// Parameters specify the XML data source and the variable where the new <see cref="IXmlRpcValue"/> object is returned.
    /// </summary>
    /// <param name="source">A <see cref="XPathNavigator"/> that represents the XML data source to be parsed.</param>
    /// <param name="value">
    ///     When this method returns, contains an object that represents the <see cref="IXmlRpcValue"/> specified by the <paramref name="source"/>, or <see langword="null"/> if the conversion failed.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="source"/> was converted successfully; otherwise, <see langword="false"/>.
    ///     This operation returns <see langword="false"/> if the <paramref name="source"/> parameter is <see langword="null"/>,
    ///     or represents XML data that is not in the expected format.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///     The <paramref name="source"/> is expected to be positioned on an XML-RPC
    ///     <c>&lt;value&gt;</c> element; one positioned anywhere else fails rather than guessing.
    ///     </para>
    ///     <para>
    ///     A typed element whose text will not parse — <c>&lt;value&gt;&lt;i4&gt;abc&lt;/i4&gt;&lt;/value&gt;</c>
    ///     — is a failure, not the string <c>abc</c>. Only an element carrying no type at all falls back
    ///     to a string, which is what the specification says: "If no type is indicated, the type is
    ///     string."
    ///     </para>
    ///     <para>
    ///     A <c>&lt;struct&gt;</c> or <c>&lt;array&gt;</c> nested deeper than
    ///     <see cref="MaxValueNestingDepth"/> is a failure as well, for the reasons given there.
    ///     </para>
    /// </remarks>
    public static bool TryParseValue(XPathNavigator source, [NotNullWhen(true)] out IXmlRpcValue? value) =>
        TryParseValue(source, 0, out value);

    /// <summary>
    /// Constructs a new <see cref="IXmlRpcValue"/> object from the specified <see cref="XPathNavigator"/>, at a known nesting depth.
    /// </summary>
    /// <param name="source">A <see cref="XPathNavigator"/> that represents the XML data source to be parsed.</param>
    /// <param name="depth">How many composite values enclose <paramref name="source"/>. Zero at the outermost <c>value</c>.</param>
    /// <param name="value">
    ///     When this method returns, contains an object that represents the <see cref="IXmlRpcValue"/> specified by the <paramref name="source"/>, or <see langword="null"/> if the conversion failed.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="source"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Only the two composite branches consult <paramref name="depth"/>, and they consult it before
    ///     recursing. A scalar leaf never re-enters this method, so refusing one at the boundary would
    ///     buy no stack and would lose the innermost element of a document nested exactly to the limit.
    /// </remarks>
    internal static bool TryParseValue(XPathNavigator source, int depth, [NotNullWhen(true)] out IXmlRpcValue? value)
    {
        if (source is null || !string.Equals(source.Name, "value", StringComparison.OrdinalIgnoreCase))
        {
            value = null;
            return false;
        }

        // MoveToChild(Element) rather than MoveToFirstChild(). A text node is a child, so an untyped
        // <value>text</value> satisfies HasChildren and lands MoveToFirstChild on a node whose Name is
        // the empty string; nothing matched it and the method returned false. That made the untyped
        // branch below unreachable for every possible input, because it sat in the else of
        // HasChildren -- reachable only for an empty element, which its own guard then rejects.
        if (source.HasChildren)
        {
            XPathNavigator navigator = source.CreateNavigator();
            if (navigator.MoveToChild(XPathNodeType.Element))
            {
                if (string.Equals(navigator.Name, "i4", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(navigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int scalar))
                    {
                        value = new XmlRpcScalarValue(scalar);
                        return true;
                    }
                }
                else if (string.Equals(navigator.Name, "int", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(navigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int scalar))
                    {
                        value = new XmlRpcScalarValue(scalar);
                        return true;
                    }
                }
                else if (string.Equals(navigator.Name, "boolean", StringComparison.OrdinalIgnoreCase))
                {
                    if (XmlRpcClient.TryParseBoolean(navigator.Value, out bool scalar))
                    {
                        value = new XmlRpcScalarValue(scalar);
                        return true;
                    }
                }
                else if (string.Equals(navigator.Name, "string", StringComparison.OrdinalIgnoreCase))
                {
                    value = new XmlRpcScalarValue(navigator.Value);
                    return true;
                }
                else if (string.Equals(navigator.Name, "double", StringComparison.OrdinalIgnoreCase))
                {
                    if (double.TryParse(navigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double scalar))
                    {
                        value = new XmlRpcScalarValue(scalar);
                        return true;
                    }
                }
                else if (string.Equals(navigator.Name, "dateTime.iso8601", StringComparison.OrdinalIgnoreCase))
                {
                    if (XmlRpcClient.TryParseIso8601DateTime(navigator.Value, out DateTime scalar))
                    {
                        value = new XmlRpcScalarValue(scalar);
                        return true;
                    }
                }
                else if (string.Equals(navigator.Name, "base64", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(navigator.Value))
                    {
                        try
                        {
                            byte[] data = Convert.FromBase64String(navigator.Value);
                            value = new XmlRpcScalarValue(data);
                            return true;
                        }
                        catch (FormatException)
                        {
                            value = null;
                            return false;
                        }
                    }
                }
                else if (string.Equals(navigator.Name, "struct", StringComparison.OrdinalIgnoreCase))
                {
                    if (depth < XmlRpcClient.MaxValueNestingDepth)
                    {
                        XmlRpcStructureValue structure = new();
                        if (structure.Load(source, depth))
                        {
                            value = structure;
                            return true;
                        }
                    }
                }
                else if (string.Equals(navigator.Name, "array", StringComparison.OrdinalIgnoreCase))
                {
                    if (depth < XmlRpcClient.MaxValueNestingDepth)
                    {
                        XmlRpcArrayValue array = new();
                        if (array.Load(source, depth))
                        {
                            value = array;
                            return true;
                        }
                    }
                }

                // A typed element that would not parse is a failure, not an untyped string. Falling
                // through to the branch below would turn <value><i4>abc</i4></value> into the string
                // "abc", because that is what source.Value reports for it.
                value = null;
                return false;
            }
        }

        // XML-RPC 1.0, on <value>: "If no type is indicated, the type is string."
        if (!string.IsNullOrEmpty(source.Value))
        {
            value = new XmlRpcScalarValue(source.Value);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// The zoneless spellings of an XML-RPC <c>dateTime.iso8601</c> value.
    /// </summary>
    /// <remarks>
    ///     XML-RPC 1.0 spells its own example <c>19980717T14:08:55</c> — a basic-format date, an
    ///     extended-format time, and no offset. The second entry is the fully basic form, which the
    ///     same paragraph of ISO 8601 permits and which servers do emit.
    /// </remarks>
    private static readonly string[] Iso8601ZonelessFormats =
    [
        "yyyyMMdd'T'HH:mm:ss",
        "yyyyMMdd'T'HHmmss",
    ];

    /// <summary>
    /// The offset-bearing spellings of an XML-RPC <c>dateTime.iso8601</c> value.
    /// </summary>
    /// <remarks>
    ///     Tried only after <see cref="Iso8601ZonelessFormats"/> has failed, and that order is
    ///     load-bearing: <c>K</c> matches the empty string, so a zoneless value offered to these
    ///     patterns under <see cref="DateTimeStyles.AdjustToUniversal"/> would be read as machine-local
    ///     and rebased. That is the defect §2.37 records in <c>TryParseRfc822DateTime</c>.
    /// </remarks>
    private static readonly string[] Iso8601OffsetFormats =
    [
        "yyyyMMdd'T'HH:mm:ssK",
        "yyyyMMdd'T'HHmmssK",
    ];

    /// <summary>
    /// Converts the string representation of an XML-RPC <c>dateTime.iso8601</c> value to its <see cref="DateTime"/> equivalent. A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="value">A string containing the value to convert.</param>
    /// <param name="result">When this method returns, contains the converted value if the conversion succeeded, or <see cref="DateTime.MinValue"/> if it failed. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     RFC 3339 is tried first, unchanged, because that is what this element accepted before and
    ///     because most live servers emit it despite the element's name. The XML-RPC spellings are a
    ///     fallback, so nothing that parsed before parses differently now.
    ///     </para>
    ///     <para>
    ///     A zoneless value comes back <see cref="DateTimeKind.Unspecified"/>. It carries no offset, so
    ///     that is the only honest answer; calling it <see cref="DateTimeKind.Utc"/> would invent one.
    ///     </para>
    /// </remarks>
    private static bool TryParseIso8601DateTime(string value, out DateTime result)
    {
        if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(value, out result))
        {
            return true;
        }

        return DateTime.TryParseExact(value, XmlRpcClient.Iso8601ZonelessFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out result)
            || DateTime.TryParseExact(value, XmlRpcClient.Iso8601OffsetFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out result);
    }

    /// <summary>
    /// Converts the specified string representation of a logical value to its <see cref="Boolean"/> equivalent. A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="value">A string containing the value to convert.</param>
    /// <param name="result">
    ///     When this method returns, if the conversion succeeded, contains <see langword="true"/> if value is equivalent to <i>1</i>, <i>true</i> or <i>True</i>;
    ///     or <see langword="false"/> if value is equivalent to <i>0</i>, <i>false</i> or <i>False</i>. If the conversion failed, contains <see langword="false"/>.
    ///     The conversion fails if value is <see langword="null"/> or is not equivalent to <i>1</i>, <i>true</i>, <i>True</i>, <i>0</i>, <i>false</i> or <i>False</i>.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    internal static bool TryParseBoolean(string value, out bool result)
    {
        if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }
        else if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }
        else if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }
        else if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }
        else
        {
            result = false;
            return false;
        }
    }

    /// <summary>
    /// Sends the specified message to an XML-RPC server to execute a remote procedure call asynchronously.
    /// </summary>
    /// <param name="message">A <see cref="XmlRpcMessage"/> that represents the information needed to execute the remote procedure call.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task whose result is the server's <see cref="XmlRpcResponse"/>. A call the server faulted is
    ///     still a successful send — read <see cref="XmlRpcResponse.Fault"/>, which no exception here
    ///     reports.
    /// </returns>
    /// <remarks>
    ///     Bounded by <see cref="Timeout"/>, applied to a source linked to
    ///     <paramref name="cancellationToken"/>; whichever fires first cancels the send and the read of
    ///     the response body alike.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The <see cref="Host"/> has not been set.</exception>
    public async Task<XmlRpcResponse> SendAsync(XmlRpcMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        Uri host = this.Host ?? throw new InvalidOperationException($"Unable to send XML-RPC message. The Host property has not been initialized. \n\r Message payload: {message}");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(this.Timeout);

        using HttpResponseMessage httpResponse = await SendRequestAsync(
            host, this.UserAgent, message, this.httpClient, timeoutCts.Token).ConfigureAwait(false);

        return await XmlRpcResponse.CreateAsync(httpResponse, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends an XML-RPC request asynchronously using the supplied host, user agent, message, and HttpClient.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</param>
    /// <param name="userAgent">Information such as the application name, version, host operating system, and language.</param>
    /// <param name="message">A <see cref="XmlRpcMessage"/> that represents the information needed to execute the remote procedure call.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="HttpResponseMessage"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="userAgent"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    private static async Task<HttpResponseMessage> SendRequestAsync(
        Uri host,
        string userAgent,
        XmlRpcMessage message,
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentException.ThrowIfNullOrEmpty(userAgent);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(httpClient);

        byte[] payloadData;
        using (MemoryStream stream = new())
        {
            XmlWriterSettings settings = new()
            {
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = message.Encoding,
                Indent = true,
                OmitXmlDeclaration = false
            };

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                message.WriteTo(writer);
            }
            payloadData = stream.ToArray();
        }

        using HttpRequestMessage request = new(HttpMethod.Post, host);
        request.Headers.UserAgent.ParseAdd(userAgent);
        request.Content = new ByteArrayContent(payloadData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml")
        {
            CharSet = message.Encoding.WebName
        };

        return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the specified options to this client instance.
    /// </summary>
    /// <param name="options">The options to apply.</param>
    private void ApplyOptions(XmlRpcClientOptions options)
    {
        if (options.Timeout > TimeSpan.Zero && options.Timeout < TimeSpan.FromDays(365))
        {
            this.Timeout = options.Timeout;
        }

        if (!string.IsNullOrEmpty(options.UserAgent))
        {
            this.UserAgent = options.UserAgent;
        }

        if (options.Host is not null)
        {
            this.Host = options.Host;
        }
    }
}