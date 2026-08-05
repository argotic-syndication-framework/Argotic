using Argotic.Common;
using Argotic.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Argotic.Configuration;

/// <summary>
/// Extension methods for configuring Argotic services in an <see cref="IServiceCollection"/>.
/// </summary>
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
    /// <param name="sectionName">The configuration section name. Defaults to "Argotic:XmlRpc".</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
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
    /// <param name="sectionName">The configuration section name. Defaults to "Argotic:Trackback".</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
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
    ///     Pooling policy is deliberately <b>not</b> shared. A static client has to rotate its own
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