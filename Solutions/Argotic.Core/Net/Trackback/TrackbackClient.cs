using System.Net.Http.Headers;
using Argotic.Common;
using Argotic.Configuration;

namespace Argotic.Net;

/// <summary>
/// Allows applications to send and received notification pings by using the Trackback peer-to-peer notification protocol.
/// </summary>
/// <remarks>
///     <para>
///         This implementation of Trackback is based on the Trackback 1.2 specification which can be found
///         at <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a>.
///     </para>
///     <para>
///         For scenarios requiring authentication or proxy configuration, provide a pre-configured <see cref="HttpClient"/>
///         via the constructor. This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackClient class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs"
///             region="TrackbackClient"
///         />
///     </code>
/// </example>
public class TrackbackClient
{
    /// <summary>
    /// Private member to hold the location of the host computer that client Trackback pings will be sent to.
    /// </summary>
    private Uri clientHost;
    /// <summary>
    /// Private member to hold information such as the application name, version, host operating system, and language.
    /// </summary>
    private string clientUserAgent = string.Format(null, "Argotic-Syndication-Framework/{0}", System.Reflection.Assembly.GetAssembly(typeof(TrackbackClient))!.GetName().Version!.ToString(4));
    /// <summary>
    /// Private member to hold the HttpClient used for sending requests.
    /// </summary>
    private readonly HttpClient httpClient;
    /// <summary>
    /// Private member to hold a value that specifies the amount of time after which an asynchronous send operation times out.
    /// </summary>
    private TimeSpan clientTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class using the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    ///     This constructor uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.
    ///     For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    public TrackbackClient()
    {
        this.httpClient = SyndicationEncodingUtility.SharedHttpClient;
        this.Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class that sends Trackback pings using the specified Trackback server.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
    /// <remarks>
    ///     This constructor uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.
    ///     For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference.</exception>
    public TrackbackClient(Uri host) : this()
    {
        this.Host = host;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class that sends Trackback pings using the specified <see cref="HttpClient"/>.
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
    public TrackbackClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
        this.Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class that sends Trackback pings using the specified <see cref="HttpClient"/> and Trackback server.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
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
    public TrackbackClient(Uri host, HttpClient httpClient) : this(httpClient)
    {
        this.Host = host;
    }

    /// <summary>
    /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</value>
    /// <remarks>
    ///     If <see cref="Host"/> is a null reference, <see cref="Host"/> is initialized using the settings in the application or machine configuration files.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Host
    {
        get
        {
            return clientHost;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            clientHost = value;
        }
    }

    /// <summary>
    /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time out.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
    /// <remarks>
    ///     If <see cref="Timeout"/> is equal to <see cref="TimeSpan.MinValue"/>, <see cref="Timeout"/> is initialized using the settings in the application or machine configuration files.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The time-out period is greater than a year.</exception>
    public TimeSpan Timeout
    {
        get
        {
            return clientTimeout;
        }

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
                clientTimeout = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets information such as the client application name, version, host operating system, and language.
    /// </summary>
    /// <value>Information such as the client application name, version, host operating system, and language. The default value is an agent that describes this syndication framework.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string UserAgent
    {
        get
        {
            return clientUserAgent;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            clientUserAgent = value.Trim();
        }
    }

    /// <summary>
    /// Sends the specified message to a Trackback server to execute a Trackback ping request asynchronously.
    /// </summary>
    /// <param name="message">A <see cref="TrackbackMessage"/> that represents the information needed to execute the Trackback ping request.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="TrackbackResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference.</exception>
    /// <exception cref="InvalidOperationException">The <see cref="Host"/> is a <b>null</b> reference.</exception>
    public async Task<TrackbackResponse> SendAsync(TrackbackMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (this.Host == null)
        {
            throw new InvalidOperationException(string.Format(null, "Unable to send Trackback message. The Host property has not been initialized. \n\r Message payload: {0}", message));
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(this.Timeout);

        using HttpResponseMessage httpResponse = await SendRequestAsync(
            this.Host, this.UserAgent, message, this.httpClient, timeoutCts.Token).ConfigureAwait(false);

        return await TrackbackResponse.CreateAsync(httpResponse, timeoutCts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a Trackback ping request asynchronously using the supplied host, user agent, message, and HttpClient.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
    /// <param name="userAgent">Information such as the application name, version, host operating system, and language.</param>
    /// <param name="message">A <see cref="TrackbackMessage"/> that represents the information needed to execute the Trackback ping request.</param>
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
        TrackbackMessage message,
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
            using StreamWriter writer = new(stream, message.Encoding, leaveOpen: true);
            message.WriteTo(writer);
            writer.Flush();
            payloadData = stream.ToArray();
        }

        using HttpRequestMessage request = new(HttpMethod.Post, host);
        request.Headers.UserAgent.ParseAdd(userAgent);
        request.Content = new ByteArrayContent(payloadData);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
        {
            CharSet = message.Encoding.WebName
        };

        return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes the current instance using the application configuration settings.
    /// </summary>
    /// <seealso cref="XmlRpcClientSection"/>
    private void Initialize()
    {
        TrackbackClientSection clientConfiguration = PrivilegedConfigurationManager.GetTracbackClientSection();

        if (clientConfiguration != null)
        {
            if (clientConfiguration.Timeout.TotalMilliseconds > 0 && clientConfiguration.Timeout < TimeSpan.FromDays(365))
            {
                this.Timeout = clientConfiguration.Timeout;
            }

            if (!string.IsNullOrEmpty(clientConfiguration.UserAgent))
            {
                this.UserAgent = clientConfiguration.UserAgent;
            }

            if (clientConfiguration.Network?.Host != null)
            {
                this.Host = clientConfiguration.Network.Host;
            }
        }
    }
}