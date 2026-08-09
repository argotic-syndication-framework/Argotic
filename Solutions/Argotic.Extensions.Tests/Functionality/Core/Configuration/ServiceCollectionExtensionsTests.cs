using Argotic.Configuration;
using Argotic.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace Argotic.Extensions.Tests.Functionality.Core.Configuration;

/// <summary>
/// Covers <c>AddXmlRpcClient</c> and <c>AddTrackbackClient</c>: what they register, how they bind options, and with what lifetime.
/// </summary>
[TestClass]
public class ServiceCollectionExtensionsTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// <c>AddXmlRpcClient</c> makes an <c>XmlRpcClient</c> resolvable.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_RegistersXmlRpcClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddXmlRpcClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        // GetRequiredService is contractually non-null and throws when unregistered, so
        // client.ShouldNotBeNull() could not fail. What registration is actually supposed to have done
        // is bind the default options onto the client, so that is what is asserted.
        XmlRpcClient client = provider.GetRequiredService<XmlRpcClient>();
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        provider.GetRequiredService<IOptions<XmlRpcClientOptions>>().Value.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// A <see langword="null"/> configure delegate still registers the client rather than throwing.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_WithNullConfigure_RegistersXmlRpcClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddXmlRpcClient(configure: null);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        // GetRequiredService is contractually non-null and throws when unregistered, so
        // client.ShouldNotBeNull() could not fail. What registration is actually supposed to have done
        // is bind the default options onto the client, so that is what is asserted.
        XmlRpcClient client = provider.GetRequiredService<XmlRpcClient>();
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        provider.GetRequiredService<IOptions<XmlRpcClientOptions>>().Value.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// The configure delegate reaches <c>IOptions&lt;XmlRpcClientOptions&gt;</c> — timeout, user agent and host alike.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_WithOptions_ConfiguresOptions()
    {
        // Arrange
        ServiceCollection services = new();
        TimeSpan expectedTimeout = TimeSpan.FromSeconds(30);
        string expectedUserAgent = "TestAgent/1.0";
        Uri expectedHost = new("http://example.com/xmlrpc");

        // Act
        services.AddXmlRpcClient(options =>
        {
            options.Timeout = expectedTimeout;
            options.UserAgent = expectedUserAgent;
            options.Host = expectedHost;
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IOptions<XmlRpcClientOptions> options = provider.GetRequiredService<IOptions<XmlRpcClientOptions>>();
        options.Value.Timeout.ShouldBe(expectedTimeout);
        options.Value.UserAgent.ShouldBe(expectedUserAgent);
        options.Value.Host.ShouldBe(expectedHost);
    }

    /// <summary>
    /// Options bind from the <c>Argotic:XmlRpc</c> configuration section when no section name is given.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_WithConfiguration_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new();
        Dictionary<string, string?> configValues = new()
        {
            { "Argotic:XmlRpc:Timeout", "00:00:45" },
            { "Argotic:XmlRpc:UserAgent", "ConfiguredAgent/2.0" },
            { "Argotic:XmlRpc:Host", "http://configured-host.com/xmlrpc" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        services.AddXmlRpcClient(configuration);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IOptions<XmlRpcClientOptions> options = provider.GetRequiredService<IOptions<XmlRpcClientOptions>>();
        options.Value.Timeout.ShouldBe(TimeSpan.FromSeconds(45));
        options.Value.UserAgent.ShouldBe("ConfiguredAgent/2.0");
        options.Value.Host.ShouldBe(new Uri("http://configured-host.com/xmlrpc"));
    }

    /// <summary>
    /// A caller-supplied section name is used in place of the default, here <c>CustomSection</c>.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_WithConfiguration_CustomSectionName_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new();
        Dictionary<string, string?> configValues = new()
        {
            { "CustomSection:Timeout", "00:01:00" },
            { "CustomSection:UserAgent", "CustomAgent/3.0" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        services.AddXmlRpcClient(configuration, "CustomSection");
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IOptions<XmlRpcClientOptions> options = provider.GetRequiredService<IOptions<XmlRpcClientOptions>>();
        options.Value.Timeout.ShouldBe(TimeSpan.FromMinutes(1));
        options.Value.UserAgent.ShouldBe("CustomAgent/3.0");
    }

    /// <summary>
    /// The extension returns the very collection it was called on, so registrations chain.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_ReturnsSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddXmlRpcClient();

        // Assert
        result.ShouldBeSameAs(services);
    }

    /// <summary>
    /// <c>AddTrackbackClient</c> makes a <c>TrackbackClient</c> resolvable.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_RegistersTrackbackClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddTrackbackClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        // GetRequiredService is contractually non-null and throws when unregistered, so
        // client.ShouldNotBeNull() could not fail. What registration is actually supposed to have done
        // is bind the default options onto the client, so that is what is asserted.
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        provider.GetRequiredService<IOptions<TrackbackClientOptions>>().Value.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// A <see langword="null"/> configure delegate still registers the client rather than throwing.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_WithNullConfigure_RegistersTrackbackClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddTrackbackClient(configure: null);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        // GetRequiredService is contractually non-null and throws when unregistered, so
        // client.ShouldNotBeNull() could not fail. What registration is actually supposed to have done
        // is bind the default options onto the client, so that is what is asserted.
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();
        client.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
        provider.GetRequiredService<IOptions<TrackbackClientOptions>>().Value.Timeout.ShouldBe(TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// The configure delegate reaches <c>IOptions&lt;TrackbackClientOptions&gt;</c> — timeout, user agent and host alike.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_WithOptions_ConfiguresOptions()
    {
        // Arrange
        ServiceCollection services = new();
        TimeSpan expectedTimeout = TimeSpan.FromSeconds(20);
        string expectedUserAgent = "TrackbackTestAgent/1.0";
        Uri expectedHost = new("http://example.com/trackback/1");

        // Act
        services.AddTrackbackClient(options =>
        {
            options.Timeout = expectedTimeout;
            options.UserAgent = expectedUserAgent;
            options.Host = expectedHost;
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IOptions<TrackbackClientOptions> options = provider.GetRequiredService<IOptions<TrackbackClientOptions>>();
        options.Value.Timeout.ShouldBe(expectedTimeout);
        options.Value.UserAgent.ShouldBe(expectedUserAgent);
        options.Value.Host.ShouldBe(expectedHost);
    }

    /// <summary>
    /// Options bind from the <c>Argotic:Trackback</c> configuration section when no section name is given.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_WithConfiguration_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new();
        Dictionary<string, string?> configValues = new()
        {
            { "Argotic:Trackback:Timeout", "00:00:25" },
            { "Argotic:Trackback:UserAgent", "TrackbackConfiguredAgent/2.0" },
            { "Argotic:Trackback:Host", "http://configured-trackback.com/ping" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        services.AddTrackbackClient(configuration);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IOptions<TrackbackClientOptions> options = provider.GetRequiredService<IOptions<TrackbackClientOptions>>();
        options.Value.Timeout.ShouldBe(TimeSpan.FromSeconds(25));
        options.Value.UserAgent.ShouldBe("TrackbackConfiguredAgent/2.0");
        options.Value.Host.ShouldBe(new Uri("http://configured-trackback.com/ping"));
    }

    /// <summary>
    /// A caller-supplied section name is used in place of the default, here <c>MyTrackback</c>.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_WithConfiguration_CustomSectionName_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new();
        Dictionary<string, string?> configValues = new()
        {
            { "MyTrackback:Timeout", "00:02:00" },
            { "MyTrackback:UserAgent", "CustomTrackbackAgent/3.0" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        services.AddTrackbackClient(configuration, "MyTrackback");
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IOptions<TrackbackClientOptions> options = provider.GetRequiredService<IOptions<TrackbackClientOptions>>();
        options.Value.Timeout.ShouldBe(TimeSpan.FromMinutes(2));
        options.Value.UserAgent.ShouldBe("CustomTrackbackAgent/3.0");
    }

    /// <summary>
    /// The extension returns the very collection it was called on, so registrations chain.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_ReturnsSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddTrackbackClient();

        // Assert
        result.ShouldBeSameAs(services);
    }

    /// <summary>
    /// Two resolutions yield two different <c>XmlRpcClient</c> instances, so the registration is transient rather than shared.
    /// </summary>
    [TestMethod]
    public void AddXmlRpcClient_RegistersAsTransient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddXmlRpcClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert - Transient services should return different instances
        XmlRpcClient client1 = provider.GetRequiredService<XmlRpcClient>();
        XmlRpcClient client2 = provider.GetRequiredService<XmlRpcClient>();
        client1.ShouldNotBeSameAs(client2);
    }

    /// <summary>
    /// Two resolutions yield two different <c>TrackbackClient</c> instances, so the registration is transient rather than shared.
    /// </summary>
    [TestMethod]
    public void AddTrackbackClient_RegistersAsTransient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddTrackbackClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert - Transient services should return different instances
        TrackbackClient client1 = provider.GetRequiredService<TrackbackClient>();
        TrackbackClient client2 = provider.GetRequiredService<TrackbackClient>();
        client1.ShouldNotBeSameAs(client2);
    }
}