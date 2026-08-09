using Argotic.Configuration;
using Argotic.Net;

using Microsoft.Extensions.DependencyInjection;
namespace Argotic.Extensions.Tests.Functionality.Core.Configuration;

/// <summary>
/// Verifies that a client resolved from the container actually reads the configured options.
/// </summary>
/// <remarks>
///     The existing registration tests call <c>services.Configure</c> and then resolve
///     <c>IOptions&lt;T&gt;</c>, asserting the options object bound correctly - which tests
///     <c>Microsoft.Extensions.Options</c>. The two that do resolve a client never call
///     <c>Configure</c>, so the container selects the parameterless constructor. Between them the
///     <c>IOptions</c> constructors and <c>ApplyOptions</c> on both clients were at zero coverage: a
///     consumer who puts <c>Argotic:XmlRpc:Host</c> in appsettings.json and injects the client was
///     running code no test had executed.
/// </remarks>
[TestClass]
public class ResolvedClientOptionsTests
{
    /// <summary>
    /// A resolved XML-RPC client carries the configured host, timeout and user agent.
    /// </summary>
    [TestMethod]
    public void AResolvedXmlRpcClient_CarriesTheConfiguredOptions()
    {
        Uri host = new("http://example.com/xmlrpc");
        ServiceCollection services = new();
        services.AddXmlRpcClient(options =>
        {
            options.Host = host;
            options.Timeout = TimeSpan.FromSeconds(45);
            options.UserAgent = "ConfiguredAgent/2.0";
        });

        using ServiceProvider provider = services.BuildServiceProvider();
        XmlRpcClient client = provider.GetRequiredService<XmlRpcClient>();

        client.Host.ShouldBe(host);
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(45));
        client.UserAgent.ShouldBe("ConfiguredAgent/2.0");
    }

    /// <summary>
    /// A resolved Trackback client carries the configured host, timeout and user agent.
    /// </summary>
    [TestMethod]
    public void AResolvedTrackbackClient_CarriesTheConfiguredOptions()
    {
        Uri host = new("http://example.com/trackback");
        ServiceCollection services = new();
        services.AddTrackbackClient(options =>
        {
            options.Host = host;
            options.Timeout = TimeSpan.FromSeconds(45);
            options.UserAgent = "ConfiguredAgent/2.0";
        });

        using ServiceProvider provider = services.BuildServiceProvider();
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();

        client.Host.ShouldBe(host);
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(45));
        client.UserAgent.ShouldBe("ConfiguredAgent/2.0");
    }

    /// <summary>
    /// A resolved client with no configuration keeps its defaults rather than nulling them out.
    /// </summary>
    [TestMethod]
    public void AResolvedClientWithNoConfiguration_KeepsItsDefaults()
    {
        ServiceCollection services = new();
        services.AddTrackbackClient();

        using ServiceProvider provider = services.BuildServiceProvider();
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();

        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        client.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

    /// <summary>
    /// A partially configured client takes the configured value and keeps the rest.
    /// </summary>
    /// <remarks>
    ///     Exercises the guards in <c>ApplyOptions</c>, which ignore a zero timeout and an empty user
    ///     agent rather than overwriting the defaults with them.
    /// </remarks>
    [TestMethod]
    public void APartiallyConfiguredClient_KeepsTheDefaultsItWasNotGiven()
    {
        ServiceCollection services = new();
        services.AddTrackbackClient(options => options.Host = new Uri("http://example.com/trackback"));

        using ServiceProvider provider = services.BuildServiceProvider();
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();

        client.Host.ShouldBe(new Uri("http://example.com/trackback"));
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        client.UserAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }
}