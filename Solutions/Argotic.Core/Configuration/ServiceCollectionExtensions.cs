using Argotic.Common;
using Argotic.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Argotic.Configuration;

/// <summary>
/// Registers Argotic's network clients, and the <see cref="HttpClient"/> its syndication types fetch with,
/// against an <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>
///     <para>
///     There are two registrations here and they are shaped differently, because the things they serve
///     are shaped differently. <see cref="XmlRpcClient"/> and <see cref="TrackbackClient"/> are services:
///     you resolve one and it arrives configured, so they are registered as <i>typed</i> clients.
///     The syndication resource types are not services — a caller constructs an <c>RssFeed</c>, they do
///     not resolve one — so <see cref="AddArgoticSyndicationClient"/> registers a <i>named</i> client
///     under <see cref="ArgoticHttpClients.Syndication"/> that the caller hands to <c>LoadAsync</c>.
///     </para>
///     <para>
///     None of this is required. Every client and every <c>LoadAsync</c> overload has a parameterless
///     form that binds <see cref="SyndicationEncodingUtility.SharedHttpClient"/>, a process-wide
///     singleton over a <see cref="SocketsHttpHandler"/>. Registering here buys what a singleton
///     structurally cannot give: a handler the factory rotates on a schedule, and one the consumer can
///     extend with a proxy, a client certificate or a delegating handler for retries.
///     </para>
///     <para>
///     Every client registered here is given <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>,
///     replacing the factory's own 100-second default. That is not an absence of a deadline. Every
///     deadline in this library is imposed by <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/>
///     on a token linked to the caller's, so a client-level timeout could only cut short a longer one
///     the caller had asked for — and it would surface as a <see cref="TaskCanceledException"/> naming
///     nothing.
///     </para>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="XmlRpcClient"/> to the service collection with optional inline configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional action to configure the <see cref="XmlRpcClientOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddXmlRpcClient(
        this IServiceCollection services,
        Action<XmlRpcClientOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddArgoticHttpClient<XmlRpcClient>();
        return services;
    }

    /// <summary>
    /// Adds an <see cref="XmlRpcClient"/> to the service collection with configuration from <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to bind options from.</param>
    /// <param name="sectionName">The configuration section to bind. The default is <c>Argotic:XmlRpc</c>. A section that is absent binds nothing, leaving every option at its default.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddXmlRpcClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Argotic:XmlRpc")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<XmlRpcClientOptions>(configuration.GetSection(sectionName));
        services.AddArgoticHttpClient<XmlRpcClient>();
        return services;
    }

    /// <summary>
    /// Adds a <see cref="TrackbackClient"/> to the service collection with optional inline configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional action to configure the <see cref="TrackbackClientOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTrackbackClient(
        this IServiceCollection services,
        Action<TrackbackClientOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddArgoticHttpClient<TrackbackClient>();
        return services;
    }

    /// <summary>
    /// Adds a <see cref="TrackbackClient"/> to the service collection with configuration from <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to bind options from.</param>
    /// <param name="sectionName">The configuration section to bind. The default is <c>Argotic:Trackback</c>. A section that is absent binds nothing, leaving every option at its default.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddTrackbackClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Argotic:Trackback")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TrackbackClientOptions>(configuration.GetSection(sectionName));
        services.AddArgoticHttpClient<TrackbackClient>();
        return services;
    }
    /// <summary>
    /// Registers the <see cref="HttpClient"/> that the syndication resource types should be fetched with.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    ///     <para>
    ///     Resolve it with <c>IHttpClientFactory.CreateClient(ArgoticHttpClients.Syndication)</c> and
    ///     pass it to any <c>LoadAsync</c> or <c>CreateAsync</c> overload taking an
    ///     <see cref="HttpClient"/>. Named rather than typed because the resource types are not
    ///     services — a caller constructs an <c>RssFeed</c>, they do not resolve one.
    ///     </para>
    ///     <para>
    ///     What this buys over <see cref="SyndicationEncodingUtility.SharedHttpClient"/> is a handler
    ///     the factory rotates, and one the consumer can extend — a proxy, a client certificate, a
    ///     delegating handler for retries — none of which is possible on a process-wide singleton.
    ///     </para>
    ///     <para>
    ///     The default <c>User-Agent</c> set here <i>does not</i> appear on Argotic's own requests.
    ///     <see cref="SyndicationEncodingUtility.CreateHttpRequestMessage"/> sets one per request, and
    ///     a request-level header wins over a client-level default. It is set for the caller who uses
    ///     the resolved client directly, and is the reason to assert on
    ///     <see cref="HttpClient.DefaultRequestHeaders"/> rather than on what crosses the wire.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddArgoticSyndicationClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient(ArgoticHttpClients.Syndication, client =>
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.UserAgent.ParseAdd(SyndicationDiscoveryUtility.FrameworkUserAgent);
            })
            .UseSocketsHttpHandler((handler, _) => SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(handler));

        return services;
    }

    /// <summary>
    /// Registers a typed client whose <see cref="HttpClient"/> comes from <c>IHttpClientFactory</c>.
    /// </summary>
    /// <typeparam name="TClient">The client type to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    ///     <para>
    ///     <c>UseSocketsHttpHandler</c> rather than <c>ConfigurePrimaryHttpMessageHandler</c>: it adds
    ///     or updates, so a consumer who has already configured a handler for this client keeps it and
    ///     has the Argotic defaults applied on top, rather than having their configuration replaced.
    ///     </para>
    ///     <para>
    ///     The defaults themselves come from <see cref="SyndicationEncodingUtility.ApplyArgoticHandlerDefaults"/>,
    ///     which is also what the shared <see cref="HttpClient"/> is built with. Two spellings of
    ///     "the Argotic handler" would be a coordination requirement between two assemblies with
    ///     nothing enforcing it, and its failure is silent — a factory-built client that keeps cookies
    ///     while the singleton does not.
    ///     </para>
    ///     <para>
    ///     Pooling policy is deliberately <i>not</i> shared. A static client has to rotate its own
    ///     connections, which is why the singleton sets <c>PooledConnectionLifetime</c>; a
    ///     factory-built one has its whole handler rotated for it, so setting a lifetime here would
    ///     duplicate the mechanism it exists to replace.
    ///     </para>
    /// </remarks>
    private static IServiceCollection AddArgoticHttpClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        services.AddHttpClient<TClient>(client =>
            {
                // The factory's own default is 100 seconds. Every deadline in this library comes from a
                // CancellationTokenSource, so a client-level one here would silently truncate a longer
                // one the caller asked for.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .UseSocketsHttpHandler((handler, _) => SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(handler));

        return services;
    }
}