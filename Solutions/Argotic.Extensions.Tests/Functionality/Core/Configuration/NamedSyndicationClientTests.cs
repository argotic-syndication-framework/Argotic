using System.Net;

using Argotic.Configuration;

using Microsoft.Extensions.DependencyInjection;
namespace Argotic.Extensions.Tests.Functionality.Core.Configuration;

/// <summary>
/// Covers the named <see cref="HttpClient"/> the syndication resource types are meant to be fetched with.
/// </summary>
/// <remarks>
///     The resource types are not services — a caller constructs an <c>RssFeed</c>, they do not resolve
///     one — so the client is registered by name rather than as a typed client. That choice is what
///     makes the constant load-bearing, and the second test here is the reason it exists.
/// </remarks>
[TestClass]
public sealed class NamedSyndicationClientTests
{
    /// <summary>
    /// The registered client carries the framework identity and imposes no deadline.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Asserted on <see cref="HttpClient.DefaultRequestHeaders"/>, never on the wire.
    ///     <c>CreateHttpRequestMessage</c> sets a <c>User-Agent</c> per request and a request-level
    ///     header outranks a client-level default, so a wire assertion would pass whatever this
    ///     registration did — including nothing.
    ///     </para>
    ///     <para>
    ///     The timeout matters more than it looks. <c>IHttpClientFactory</c>'s own default is 100
    ///     seconds; every deadline in this library comes from a <see cref="CancellationTokenSource"/>,
    ///     so a client-level one silently truncates a longer one the caller asked for — including the
    ///     <see langword="null"/> that now means "no deadline at all".
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TheNamedClientCarriesTheFrameworkIdentityAndNoDeadline()
    {
        using ServiceProvider provider = Build();
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

        using HttpClient client = factory.CreateClient(ArgoticHttpClients.Syndication);

        client.DefaultRequestHeaders.UserAgent.ToString()
            .ShouldBe(SyndicationDiscoveryUtility.FrameworkUserAgent);
        client.Timeout.ShouldBe(Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// A mistyped name yields a client with none of the configuration.
    /// </summary>
    /// <remarks>
    ///     The constant's whole purpose. A named client is looked up by string, so a typo is not a
    ///     compile error — the factory hands back a brand-new, entirely default
    ///     <see cref="HttpClient"/>. It works, it fetches feeds, and every decision the registration
    ///     made is simply absent. Nothing anywhere reports it.
    /// </remarks>
    [TestMethod]
    public void AMistypedNameYieldsAnUnconfiguredClient()
    {
        using ServiceProvider provider = Build();
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

        using HttpClient mistyped = factory.CreateClient("Argotic.Syndicaton");

        mistyped.DefaultRequestHeaders.UserAgent.ShouldBeEmpty("no identity");
        mistyped.Timeout.ShouldBe(TimeSpan.FromSeconds(100), "and the factory's deadline, not ours");
    }

    /// <summary>
    /// The named client goes through the handler the container configured.
    /// </summary>
    /// <remarks>
    ///     The point of registering it at all: a consumer can extend this handler, where a process-wide
    ///     singleton offers nowhere to put a proxy, a client certificate or a retry policy. Asserted by
    ///     loading a real feed through a handler supplied at registration.
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AFeedLoadedWithTheNamedClient_GoesThroughTheContainersHandler()
    {
        const string Feed =
            """<?xml version="1.0" encoding="utf-8"?><rss version="2.0"><channel><title>Named</title><link>http://named.invalid/</link><description>d</description></channel></rss>""";

        int seen = 0;
        ServiceCollection services = new();
        services.AddArgoticSyndicationClient();
        services.AddHttpClient(ArgoticHttpClients.Syndication)
            .ConfigurePrimaryHttpMessageHandler(() => new TestDoubles.MockHttpMessageHandler((_, _) =>
            {
                Interlocked.Increment(ref seen);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(Feed, System.Text.Encoding.UTF8, "application/rss+xml"),
                });
            }));

        using ServiceProvider provider = services.BuildServiceProvider();
        using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(ArgoticHttpClients.Syndication);

        Argotic.Syndication.RssFeed feed = new();
        await feed.LoadAsync(
            new Uri("http://named.invalid/feed.xml"),
            client,
            cancellationToken: this.TestContext.CancellationTokenSource.Token);

        seen.ShouldBe(1);
        feed.Channel.Title.ShouldBe("Named");
    }

    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider Build()
    {
        ServiceCollection services = new();
        services.AddArgoticSyndicationClient();
        return services.BuildServiceProvider();
    }
}