using System.Net.Http.Headers;

using Argotic.Common;
using Argotic.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Argotic.Net;

/// <summary>
/// Sends and receives notification pings using the Trackback peer-to-peer notification protocol.
/// </summary>
/// <remarks>
///     <para>
///         Trackback is legacy. It carries no authentication of any kind — anyone who can reach the ping
///         URL can claim any other site linked to yours — and the spam that followed is why most weblog
///         software stopped accepting pings years ago. Support is here so that an archive of feeds and
///         their <c>trackback:ping</c> elements round-trips; a receiving endpoint you build with it needs
///         its own moderation.
///     </para>
///     <para>
///         The wire format is plain: an HTTP <c>POST</c> whose body is
///         <c>application/x-www-form-urlencoded</c> with the fields <c>url</c>, <c>title</c>,
///         <c>blog_name</c> and <c>excerpt</c>, answered with a small XML document. There is no envelope
///         and no method name — see <see cref="TrackbackMessage"/> and <see cref="TrackbackResponse"/>.
///         The implementation follows the Trackback 1.2 specification at
///         <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a>.
///     </para>
///     <para>
///         For scenarios requiring authentication or proxy configuration, provide a pre-configured <see cref="HttpClient"/>
///         via the constructor. This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Net\TrackbackClientExample.cs" language="cs" title="The following code example demonstrates the usage of the TrackbackClient class." />
/// </example>
public class TrackbackClient
{

    /// <summary>
    /// Private member to hold the HttpClient used for sending requests.
    /// </summary>
    private readonly HttpClient httpClient;

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
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to configure this client.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     This constructor is intended for use with dependency injection and the <see cref="IOptions{TOptions}"/> pattern.
    /// </remarks>
    public TrackbackClient(IOptions<TrackbackClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.httpClient = SyndicationEncodingUtility.SharedHttpClient;
        ApplyOptions(options.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class with the specified options and <see cref="HttpClient"/>.
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
    public TrackbackClient(IOptions<TrackbackClientOptions> options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
        ApplyOptions(options.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackClient"/> class that sends Trackback pings using the specified Trackback server.
    /// </summary>
    /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
    /// <remarks>
    ///     This constructor uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.
    ///     For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public TrackbackClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
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
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public TrackbackClient(Uri host, HttpClient httpClient) : this(httpClient)
    {
        this.Host = host;
    }

    /// <summary>
    /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
    /// </summary>
    /// <value>
    ///     The Trackback ping URL. The default value is <see langword="null"/>, in which case
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
    /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time out.
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
    } = $"Argotic-Syndication-Framework/{System.Reflection.Assembly.GetAssembly(typeof(TrackbackClient))!.GetName().Version!.ToString(4)}";

    /// <summary>
    /// Sends the specified message to a Trackback server to execute a Trackback ping request asynchronously.
    /// </summary>
    /// <param name="message">A <see cref="TrackbackMessage"/> that represents the information needed to execute the Trackback ping request.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task whose result is the server's <see cref="TrackbackResponse"/>. A ping the server
    ///     rejected is still a successful send — read <see cref="TrackbackResponse.HasError"/>, which no
    ///     exception here reports.
    /// </returns>
    /// <remarks>
    ///     Bounded by <see cref="Timeout"/>, applied to a source linked to
    ///     <paramref name="cancellationToken"/>; whichever fires first cancels the send and the read of
    ///     the response body alike.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The <see cref="Host"/> has not been set.</exception>
    public async Task<TrackbackResponse> SendAsync(TrackbackMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        Uri host = this.Host ?? throw new InvalidOperationException($"Unable to send Trackback message. The Host property has not been initialized. \n\r Message payload: {message}");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(this.Timeout);

        using HttpResponseMessage httpResponse = await SendRequestAsync(
            host, this.UserAgent, message, this.httpClient, timeoutCts.Token).ConfigureAwait(false);

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
    /// <exception cref="ArgumentNullException">The <paramref name="host"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="userAgent"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="message"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
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
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            payloadData = stream.ToArray();
        }

        // A form body must not carry a byte-order mark. Encoding.UTF8 emits one, and StreamWriter
        // writes it ahead of the first field, so the receiver saw a first parameter named "﻿url"
        // rather than "url" - which Argotic's own TrackbackMessage.Load would not match either.
        // The charset travels in the Content-Type header below, where it belongs.
        byte[] preamble = message.Encoding.GetPreamble();
        if (preamble.Length > 0 && payloadData.AsSpan().StartsWith(preamble))
        {
            payloadData = payloadData[preamble.Length..];
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
    /// Applies the specified options to this client instance.
    /// </summary>
    /// <param name="options">The options to apply.</param>
    private void ApplyOptions(TrackbackClientOptions options)
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