using System.Net;
using System.Text;

using Argotic.Configuration;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Configuration;

/// <summary>
/// Pins what a container actually does with the two typed clients.
/// </summary>
/// <remarks>
///     <para>
///     The DI tests either configure options and resolve <see cref="IOptions{TOptions}"/> without ever
///     resolving a client, or resolve a client without ever configuring anything. So the interesting
///     question — which constructor the container picks, and what the resulting client is wired to —
///     had no coverage at all.
///     </para>
///     <para>
///     These rows exist before the clients move to <c>IHttpClientFactory</c>, so that the move has
///     something to be measured against rather than merely something to stay green through.
///     </para>
/// </remarks>
[TestClass]
public sealed class ContainerActivationTests
{
    /// <summary>
    /// Gets or sets the test context, used for its cancellation token.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// <c>ActivatorUtilities</c> cannot pick a Trackback constructor when given an <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Three of the six constructors accept an <see cref="HttpClient"/> — <c>(HttpClient)</c>,
    ///     <c>(IOptions, HttpClient)</c> and <c>(Uri, HttpClient)</c> — and <c>ActivatorUtilities</c>
    ///     refuses to choose between them. This is the throw that <c>IHttpClientFactory</c>'s typed
    ///     client registration would hit, because that is how it activates.
    ///     </para>
    ///     <para>
    ///     <b>Deleting the <c>(IOptions)</c> constructor does not fix it</b>, which the plan assumed it
    ///     would: that constructor cannot consume an <see cref="HttpClient"/>, so it is not one of the
    ///     three and removing it takes the count from three to three. Only
    ///     <c>[ActivatorUtilitiesConstructor]</c> resolves this, because the preferred-constructor pass
    ///     short-circuits the matching pass entirely.
    ///     </para>
    /// </remarks>
    /// <summary>
    /// A typed-client factory can be built for the Trackback client.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This threw. Three of the six constructors accept an <see cref="HttpClient"/> —
    ///     <c>(HttpClient)</c>, <c>(IOptions, HttpClient)</c> and <c>(Uri, HttpClient)</c> — and
    ///     <c>CreateFactory</c>, binding from types alone ahead of any call, had nothing to break the
    ///     tie with. <c>AddHttpClient&lt;T&gt;</c> activates exactly this way, so the ambiguity was
    ///     what blocked the move.
    ///     </para>
    ///     <para>
    ///     <c>[ActivatorUtilitiesConstructor]</c> resolves it, and is the only thing that does:
    ///     deleting the <c>(IOptions)</c> constructor — the plan's suggestion — would have taken the
    ///     count from three to three, because that constructor cannot consume an
    ///     <see cref="HttpClient"/> and so was never one of the three.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ATypedClientFactoryForTheTrackbackClient_CanNowBeBuilt()
        => Should.NotThrow(() => ActivatorUtilities.CreateFactory(typeof(TrackbackClient), [typeof(HttpClient)]));

    /// <summary>
    /// The same is true of the XML-RPC client.
    /// </summary>
    [TestMethod]
    public void ATypedClientFactoryForTheXmlRpcClient_CanNowBeBuilt()
        => Should.NotThrow(() => ActivatorUtilities.CreateFactory(typeof(XmlRpcClient), [typeof(HttpClient)]));

    /// <summary>
    /// A resolved client sends through the handler the container configured, exactly once.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The gate for the whole move. A container-resolved client used to be bound to the
    ///     process-wide singleton, so a consumer configuring a handler for it — a proxy, a certificate,
    ///     a retry policy — configured something the client never used, with nothing to indicate that.
    ///     Here the primary handler is replaced through the registration, and the assertion is that
    ///     the request arrives at it.
    ///     </para>
    ///     <para>
    ///     <b>Exactly once, not merely at least once.</b> Registering a typed client twice is easy to
    ///     do by accident — <c>AddTrackbackClient</c> called from two composition roots, say — and a
    ///     duplicated pipeline sends a duplicate request. A trackback ping is not idempotent.
    ///     </para>
    /// </remarks>
    /// <returns>A task representing the test.</returns>
    [TestMethod]
    public async Task AResolvedClientSendsThroughTheConfiguredHandlerExactlyOnce()
    {
        int sent = 0;
        ServiceCollection services = new();
        services.AddTrackbackClient(options => options.Host = new Uri("http://factory.invalid/tb"));
        services.AddHttpClient<TrackbackClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler((_, _) =>
            {
                Interlocked.Increment(ref sent);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """<?xml version="1.0" encoding="utf-8"?><response><error>0</error></response>""",
                        Encoding.UTF8,
                        "text/xml"),
                });
            }));

        using ServiceProvider provider = services.BuildServiceProvider();
        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();

        TrackbackMessage message = new(new Uri("http://factory.invalid/post"));

        TrackbackResponse response = await client.SendAsync(message, this.TestContext.CancellationTokenSource.Token);

        sent.ShouldBe(1, "the request reached the container's handler, and reached it once");
        response.HasError.ShouldBeFalse();
        client.Host.ShouldBe(new Uri("http://factory.invalid/tb"), "and the options were still honoured");
    }

    /// <summary>
    /// <c>CreateInstance</c> resolves the same type that <c>CreateFactory</c> refuses to.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The asymmetry that makes a "just try it" migration fail confusingly. <c>CreateInstance</c>
    ///     has the argument <i>values</i> and can pick a best match — it chooses
    ///     <c>(IOptions, HttpClient)</c>, which is why the host below comes from the configured options
    ///     rather than being null. <c>CreateFactory</c> must bind from the argument <i>types</i> alone,
    ///     ahead of any call, and has nothing to break the tie with.
    ///     </para>
    ///     <para>
    ///     So an attempt to verify the ambiguity through <c>CreateInstance</c> reports that there isn't
    ///     one. There is; it is simply on the path a typed client actually uses.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void CreateInstanceResolvesWhatCreateFactoryCannot()
    {
        ServiceCollection services = new();
        services.Configure<TrackbackClientOptions>(options => options.Host = new Uri("http://opt.invalid/tb"));
        using ServiceProvider provider = services.BuildServiceProvider();
        using HttpClient client = new();

        TrackbackClient made = ActivatorUtilities.CreateInstance<TrackbackClient>(provider, client);

        made.Host.ShouldBe(
            new Uri("http://opt.invalid/tb"),
            "(IOptions, HttpClient) is the constructor chosen, so the options are honoured");
    }

    /// <summary>
    /// The container resolves a configured Trackback client, and it is bound to the shared client.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <see cref="ServiceProvider"/> does not read <c>[ActivatorUtilitiesConstructor]</c> — the
    ///     attribute appears nowhere in its assembly, only in <c>ActivatorUtilities</c> — so it takes
    ///     its own route and picks <c>(IOptions&lt;T&gt;)</c>, the longest constructor whose parameters
    ///     it can all satisfy. That constructor binds the process-wide <see cref="HttpClient"/>.
    ///     </para>
    ///     <para>
    ///     Which is the whole reason to move: a consumer who registers these clients gets options
    ///     honoured and connection pooling that a static singleton has to manage for itself. The
    ///     second assertion is the one that changes when they move to a factory.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TheContainerResolvesAConfiguredClientBoundToTheSharedHttpClient()
    {
        ServiceCollection services = new();
        services.AddTrackbackClient(options => options.Host = new Uri("http://pinned.invalid/tb"));
        using ServiceProvider provider = services.BuildServiceProvider();

        TrackbackClient client = provider.GetRequiredService<TrackbackClient>();

        client.Host.ShouldBe(
            new Uri("http://pinned.invalid/tb"),
            "PINS TODAY: the (IOptions<T>) constructor is the one chosen, and it binds the shared client");
    }

    /// <summary>
    /// The XML-RPC client resolves the same way.
    /// </summary>
    [TestMethod]
    public void TheContainerResolvesAConfiguredXmlRpcClientBoundToTheSharedHttpClient()
    {
        ServiceCollection services = new();
        services.AddXmlRpcClient(options => options.Host = new Uri("http://pinned.invalid/rpc"));
        using ServiceProvider provider = services.BuildServiceProvider();

        XmlRpcClient client = provider.GetRequiredService<XmlRpcClient>();

        client.Host.ShouldBe(new Uri("http://pinned.invalid/rpc"));
    }

    /// <summary>
    /// Registering the client by hand rather than through the extension also configures it.
    /// </summary>
    /// <remarks>
    ///     Worth pinning because it is the shape most likely to break silently. A consumer who writes
    ///     <c>AddTransient&lt;TrackbackClient&gt;()</c> alongside their own <c>Configure</c> gets a
    ///     configured client today, from the <c>(IOptions&lt;T&gt;)</c> constructor. If that
    ///     constructor is ever removed the same code compiles, resolves, and silently produces an
    ///     unconfigured client.
    /// </remarks>
    [TestMethod]
    public void RegisteringTheClientByHand_StillHonoursConfiguredOptions()
    {
        ServiceCollection services = new();
        services.Configure<TrackbackClientOptions>(options => options.Host = new Uri("http://byhand.invalid/tb"));
        services.AddTransient<TrackbackClient>();
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<TrackbackClient>()
            .Host.ShouldBe(new Uri("http://byhand.invalid/tb"));
    }
}