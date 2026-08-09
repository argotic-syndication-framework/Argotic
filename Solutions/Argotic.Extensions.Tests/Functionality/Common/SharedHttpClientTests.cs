using System.Net;
namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins what the shared HTTP pipeline negotiates and what it remembers between requests.
/// </summary>
/// <remarks>
///     <para>
///     None of this was testable before the handler settings were named. <c>CreateSharedHttpClient</c>
///     is private, <see cref="HttpClient"/> does not expose the handler it was built over, and a
///     <c>MockHttpMessageHandler</c> replaces the whole pipeline — so a test using one cannot observe
///     <see cref="SocketsHttpHandler.UseCookies"/> at all.
///     </para>
///     <para>
///     Asserting against a handler configured by the same method the singleton uses is what makes the
///     claim checkable. It is weaker than reaching into the singleton, and it is the strongest thing
///     available without opening the type up purely to be inspected.
///     </para>
/// </remarks>
[TestClass]
public sealed class SharedHttpClientTests
{
    /// <summary>
    /// The shared pipeline advertises every encoding the platform can decode.
    /// </summary>
    /// <remarks>
    ///     Brotli has shipped since .NET Core 3.0 and is what most origins prefer. Advertising only
    ///     gzip and deflate declined the smallest encoding on offer.
    /// </remarks>
    [TestMethod]
    public void TheSharedHandler_NegotiatesBrotliAsWellAsGzipAndDeflate()
    {
        using SocketsHttpHandler handler = new();

        SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(handler);

        handler.AutomaticDecompression.ShouldBe(
            DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli);
    }

    /// <summary>
    /// The shared pipeline keeps no cookies.
    /// </summary>
    /// <remarks>
    ///     <see cref="SocketsHttpHandler.UseCookies"/> defaults to <see langword="true"/>, so a
    ///     <c>Set-Cookie</c> from any origin was replayed on the next request to that host. On a
    ///     process-wide singleton that is per-domain session state accumulating for the lifetime of the
    ///     application, with no API to inspect or clear it — and a feed reader has no use for it.
    /// </remarks>
    [TestMethod]
    public void TheSharedHandler_KeepsNoCookies()
    {
        using SocketsHttpHandler handler = new();

        SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(handler);

        handler.UseCookies.ShouldBeFalse();
        handler.CookieContainer.Count.ShouldBe(0);
    }

    /// <summary>
    /// The shared defaults say nothing about connection pooling, deliberately.
    /// </summary>
    /// <remarks>
    ///     Pooling policy is the singleton's own: a static client must rotate its own connections,
    ///     where a client built by <c>IHttpClientFactory</c> has its whole handler rotated for it.
    ///     Sharing the policy would push a 15-minute lifetime onto a handler the factory already
    ///     replaces, which is why it is left at the platform default here and set at the one call site
    ///     that needs it.
    /// </remarks>
    [TestMethod]
    public void TheSharedDefaults_LeavePoolingPolicyAlone()
    {
        using SocketsHttpHandler handler = new();

        SyndicationEncodingUtility.ApplyArgoticHandlerDefaults(handler);

        handler.PooledConnectionLifetime.ShouldBe(Timeout.InfiniteTimeSpan, "the platform default, untouched");
    }

    /// <summary>
    /// The shared client imposes no deadline of its own.
    /// </summary>
    /// <remarks>
    ///     Deadlines are driven by <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> from the
    ///     caller's settings, so an <see cref="HttpClient.Timeout"/> here would silently truncate a
    ///     longer one the caller had asked for. Pinned because it looks like an oversight and is not.
    /// </remarks>
    [TestMethod]
    public void TheSharedClient_ImposesNoTimeoutOfItsOwn()
        => SyndicationEncodingUtility.SharedHttpClient.Timeout.ShouldBe(Timeout.InfiniteTimeSpan);
}