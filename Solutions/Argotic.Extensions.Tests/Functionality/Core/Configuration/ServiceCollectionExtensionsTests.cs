using Argotic.Configuration;
using Argotic.Net;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Configuration;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void AddXmlRpcClient_RegistersXmlRpcClient()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddXmlRpcClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        XmlRpcClient client = provider.GetRequiredService<XmlRpcClient>();
        client.ShouldNotBeNull();
    }

    [TestMethod]
    public void AddXmlRpcClient_WithNullConfigure_RegistersXmlRpcClient()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddXmlRpcClient(configure: null);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        XmlRpcClient client = provider.GetRequiredService<XmlRpcClient>();
        client.ShouldNotBeNull();
    }

    [TestMethod]
    public void AddXmlRpcClient_WithOptions_ConfiguresOptions()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        TimeSpan expectedTimeout = TimeSpan.FromSeconds(30);
        string expectedUserAgent = "TestAgent/1.0";
        Uri expectedHost = new Uri("http://example.com/xmlrpc");

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

    [TestMethod]
    public void AddXmlRpcClient_WithConfiguration_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        Dictionary<string, string?> configValues = new Dictionary<string, string?>
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

    [TestMethod]
    public void AddXmlRpcClient_WithConfiguration_CustomSectionName_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        Dictionary<string, string?> configValues = new Dictionary<string, string?>
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

    [TestMethod]
    public void AddXmlRpcClient_ReturnsSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddXmlRpcClient();

        // Assert
        result.ShouldBeSameAs(services);
    }

    [TestMethod]
    public void AddTrackbackClient_RegistersTrackbackClient()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddTrackbackClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();
        client.ShouldNotBeNull();
    }

    [TestMethod]
    public void AddTrackbackClient_WithNullConfigure_RegistersTrackbackClient()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddTrackbackClient(configure: null);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();
        client.ShouldNotBeNull();
    }

    [TestMethod]
    public void AddTrackbackClient_WithOptions_ConfiguresOptions()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        TimeSpan expectedTimeout = TimeSpan.FromSeconds(20);
        string expectedUserAgent = "TrackbackTestAgent/1.0";
        Uri expectedHost = new Uri("http://example.com/trackback/1");

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

    [TestMethod]
    public void AddTrackbackClient_WithConfiguration_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        Dictionary<string, string?> configValues = new Dictionary<string, string?>
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

    [TestMethod]
    public void AddTrackbackClient_WithConfiguration_CustomSectionName_BindsOptions()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        Dictionary<string, string?> configValues = new Dictionary<string, string?>
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

    [TestMethod]
    public void AddTrackbackClient_ReturnsSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddTrackbackClient();

        // Assert
        result.ShouldBeSameAs(services);
    }

    [TestMethod]
    public void AddXmlRpcClient_RegistersAsTransient()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddXmlRpcClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert - Transient services should return different instances
        XmlRpcClient client1 = provider.GetRequiredService<XmlRpcClient>();
        XmlRpcClient client2 = provider.GetRequiredService<XmlRpcClient>();
        client1.ShouldNotBeSameAs(client2);
    }

    [TestMethod]
    public void AddTrackbackClient_RegistersAsTransient()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddTrackbackClient();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert - Transient services should return different instances
        TrackbackClient client1 = provider.GetRequiredService<TrackbackClient>();
        TrackbackClient client2 = provider.GetRequiredService<TrackbackClient>();
        client1.ShouldNotBeSameAs(client2);
    }
}
