using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Configuration;

using Microsoft.Extensions.Options;

namespace Argotic.Net;

/// <summary>
/// Allows applications to send remote procedure calls by using the Extensible Markup Language Remote Procedure Call (XML-RPC) protocol.
/// </summary>
/// <remarks>
///     <para>This implementation of XML-RPC is based on the XML-RPC 1.0 specification which can be found at <a href="http://www.xmlrpc.com/spec">http://www.xmlrpc.com/spec</a>.</para>
///     <para><b>XML-RPC</b> is a Remote Procedure Calling protocol that works over the Internet.</para>
///     <para>
///         An XML-RPC <i>message</i> is an HTTP-POST request. The body of the request is in XML.
///         A procedure executes on the server and the value it returns is also formatted in XML.
///     </para>
///     <para>Procedure parameters can be scalars, numbers, strings, dates and other simple types; and can also be complex record and list structures.</para>
///     <para>
///         For scenarios requiring authentication or proxy configuration, provide a pre-configured <see cref="HttpClient"/>
///         via the constructor. This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the XmlRpcClient class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Net\XmlRpcClientExample.cs"
///             region="XmlRpcClient"
///         />
///     </code>
/// </example>
public class XmlRpcClient
{
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
    /// <exception cref="ArgumentNullException">The <paramref name="options"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="options"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <remarks>
    ///     This constructor is intended for use with dependency injection and the <see cref="IOptions{TOptions}"/> pattern.
    /// </remarks>
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
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    public XmlRpcClient(Uri host, HttpClient httpClient) : this(httpClient)
    {
        this.Host = host;
    }

    /// <summary>
    /// Gets or sets the location of the host computer that client remote procedure calls will be sent to.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for XML-RPC transactions.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <value>A <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
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
    /// <value>Information such as the client application name, version, host operating system, and language. The default value is an agent that describes this syndication framework.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
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
    /// <returns>The scalar type identifier for the supplied <paramref name="type"/>, Otherwise, returns an empty string.</returns>
    /// <example>
    ///     <code
    ///         lang="cs"
    ///         title="The following code example demonstrates the usage of the ScalarTypeAsString method."
    ///     />
    /// </example>
    public static string ScalarTypeAsString(XmlRpcScalarValueType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="XmlRpcScalarValueType"/> enumeration value that corresponds to the specified scalar type name.
    /// </summary>
    /// <param name="name">The name of the scalar type.</param>
    /// <returns>A <see cref="XmlRpcScalarValueType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>XmlRpcScalarValueType.None</b>.</returns>
    /// <remarks>This method disregards case of specified scalar type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <example>
    ///     <code
    ///         lang="cs"
    ///         title="The following code example demonstrates the usage of the ScalarTypeByName method."
    ///     />
    /// </example>
    public static XmlRpcScalarValueType ScalarTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, XmlRpcScalarValueType.None);

    /// <summary>
    /// Constructs a new <see cref="IXmlRpcValue"/> object from the specified <see cref="XPathNavigator"/>.
    /// Parameters specify the XML data source and the variable where the new <see cref="IXmlRpcValue"/> object is returned.
    /// </summary>
    /// <param name="source">A <see cref="XPathNavigator"/> that represents the XML data source to be parsed.</param>
    /// <param name="value">
    ///     When this method returns, contains an object that represents the <see cref="IXmlRpcValue"/> specified by the <paramref name="source"/>, or <b>null</b> if the conversion failed.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     <b>true</b> if <paramref name="source"/> was converted successfully; otherwise, <b>false</b>.
    ///     This operation returns <b>false</b> if the <paramref name="source"/> parameter is a null reference,
    ///     or represents XML data that is not in the expected format.
    /// </returns>
    /// <remarks>
    ///     The <paramref name="source"/> is expected to represent an XML-RPC <b>value</b> node.
    /// </remarks>
    public static bool TryParseValue(XPathNavigator source, [NotNullWhen(true)] out IXmlRpcValue? value)
    {
        if (source == null || !string.Equals(source.Name, "value", StringComparison.OrdinalIgnoreCase))
        {
            value = null;
            return false;
        }

        if (source.HasChildren)
        {
            XPathNavigator navigator = source.CreateNavigator();
            if (navigator.MoveToFirstChild())
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
                    if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(navigator.Value, out DateTime scalar))
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
                    XmlRpcStructureValue structure = new();
                    if (structure.Load(source))
                    {
                        value = structure;
                        return true;
                    }
                }
                else if (string.Equals(navigator.Name, "array", StringComparison.OrdinalIgnoreCase))
                {
                    XmlRpcArrayValue array = new();
                    if (array.Load(source))
                    {
                        value = array;
                        return true;
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            value = new XmlRpcScalarValue(source.Value);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Converts the specified string representation of a logical value to its <see cref="Boolean"/> equivalent. A return value indicates whether the conversion succeeded or failed.
    /// </summary>
    /// <param name="value">A string containing the value to convert.</param>
    /// <param name="result">
    ///     When this method returns, if the conversion succeeded, contains <b>true</b> if value is equivalent to <i>1</i>, <i>true</i> or <i>True</i>;
    ///     or <b>false</b> if value is equivalent to <i>0</i>, <i>false</i> or <i>False</i>. If the conversion failed, contains <b>false</b>.
    ///     The conversion fails if value is a null reference or is not equivalent to <i>1</i>, <i>true</i>, <i>True</i>, <i>0</i>, <i>false</i> or <i>False</i>.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><b>true</b> if <paramref name="value"/> was converted successfully; otherwise, <b>false</b>.</returns>
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
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="XmlRpcResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference.</exception>
    /// <exception cref="InvalidOperationException">The <see cref="Host"/> is a <b>null</b> reference.</exception>
    public async Task<XmlRpcResponse> SendAsync(XmlRpcMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (this.Host == null)
        {
            throw new InvalidOperationException($"Unable to send XML-RPC message. The Host property has not been initialized. \n\r Message payload: {message}");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(this.Timeout);

        using HttpResponseMessage httpResponse = await SendRequestAsync(
            this.Host, this.UserAgent, message, this.httpClient, timeoutCts.Token).ConfigureAwait(false);

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
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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