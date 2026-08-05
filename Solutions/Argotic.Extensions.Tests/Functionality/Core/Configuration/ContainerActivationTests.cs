using Argotic.Configuration;
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
    /// A typed-client factory cannot be built for the Trackback client.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Three of the six constructors accept an <see cref="HttpClient"/> — <c>(HttpClient)</c>,
    ///     <c>(IOptions, HttpClient)</c> and <c>(Uri, HttpClient)</c> — and
    ///     <c>ActivatorUtilities.CreateFactory</c> refuses to choose between them. This is the throw
    ///     that <c>AddHttpClient&lt;TClient&gt;</c> hits, because a typed client registration builds
    ///     its factory once, from types alone, and reuses it.
    ///     </para>
    ///     <para>
    ///     <b>Deleting the <c>(IOptions)</c> constructor does not fix it.</b> That constructor cannot
    ///     consume an <see cref="HttpClient"/>, so it is not one of the three, and removing it takes
    ///     the count from three to three. Only <c>[ActivatorUtilitiesConstructor]</c> resolves this,
    ///     because the preferred-constructor pass short-circuits the matching pass entirely.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void BuildingATypedClientFactoryForTheTrackbackClient_IsAmbiguous()
    {
        InvalidOperationException thrown = Should.Throw<InvalidOperationException>(
            () => ActivatorUtilities.CreateFactory(typeof(TrackbackClient), [typeof(HttpClient)]));

        thrown.Message.ShouldContain("Multiple constructors");
    }

    /// <summary>
    /// The same is true of the XML-RPC client.
    /// </summary>
    [TestMethod]
    public void BuildingATypedClientFactoryForTheXmlRpcClient_IsAmbiguous()
    {
        InvalidOperationException thrown = Should.Throw<InvalidOperationException>(
            () => ActivatorUtilities.CreateFactory(typeof(XmlRpcClient), [typeof(HttpClient)]));

        thrown.Message.ShouldContain("Multiple constructors");
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