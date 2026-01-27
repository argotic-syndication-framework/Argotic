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

        services.AddTransient<XmlRpcClient>();
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
        services.Configure<XmlRpcClientOptions>(configuration.GetSection(sectionName));
        services.AddTransient<XmlRpcClient>();
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

        services.AddTransient<TrackbackClient>();
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
        services.Configure<TrackbackClientOptions>(configuration.GetSection(sectionName));
        services.AddTransient<TrackbackClient>();
        return services;
    }
}
