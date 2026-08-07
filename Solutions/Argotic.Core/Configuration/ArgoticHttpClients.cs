namespace Argotic.Configuration;

/// <summary>
/// The names under which Argotic registers clients with <c>IHttpClientFactory</c>.
/// </summary>
/// <remarks>
///     Named clients are looked up by string, so a typo is not a compile error — it silently yields a
///     brand-new, entirely default <see cref="HttpClient"/>. Everything the registration configured is
///     simply absent, and the caller gets a working client that behaves like nobody's.
/// </remarks>
public static class ArgoticHttpClients
{
    /// <summary>
    /// The client used to fetch syndication resources.
    /// </summary>
    /// <remarks>
    ///     Registered by <see cref="ServiceCollectionExtensions.AddArgoticSyndicationClient"/>. Resolve
    ///     it with <c>IHttpClientFactory.CreateClient</c> and pass it to any <c>LoadAsync</c> or
    ///     <c>CreateAsync</c> overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    /// <seealso cref="ServiceCollectionExtensions.AddArgoticSyndicationClient"/>
    public const string Syndication = "Argotic.Syndication";
}