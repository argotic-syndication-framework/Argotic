using Argotic.Configuration;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

namespace Argotic.Examples.Common;

/// <summary>
/// Contains the code examples for registering Argotic's clients with a dependency injection container.
/// </summary>
internal static class DependencyInjectionExample
{
    /// <summary>
    /// Registers the named <see cref="HttpClient"/> that syndication loads should use, and resolves one from the factory.
    /// </summary>
    public static void AddArgoticSyndicationClientExample()
    {
        //  A syndication resource is constructed, not resolved -- you write `new RssFeed()`, you do not
        //  ask the container for one. So the HttpClient it should be fetched with is registered by
        //  name rather than as a typed client, and you hand the resolved client to LoadAsync.

        ServiceCollection services = new();
        services.AddArgoticSyndicationClient();

        using ServiceProvider provider = services.BuildServiceProvider();
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

        using HttpClient client = factory.CreateClient(ArgoticHttpClients.Syndication);

        //  Then:  await feed.LoadAsync(source, client, cancellationToken: token);
        //
        //  What this buys over SyndicationEncodingUtility.SharedHttpClient is a handler the factory
        //  rotates for you, and one you can extend -- a proxy, a client certificate, a delegating
        //  handler for retries. None of that is possible on a process-wide singleton.

        //  Always the constant, never the string. A named client is looked up by name, so a typo is
        //  not a compile error: the factory hands back a brand-new, entirely default HttpClient. It
        //  works, it fetches feeds, and every decision made above is quietly absent.
        using HttpClient mistyped = factory.CreateClient("Argotic.Syndicaton");

        AnsiConsole.MarkupLine($"  [dim]Client name:[/] {ArgoticHttpClients.Syndication}");
        AnsiConsole.MarkupLine($"  [dim]User-Agent:[/] {client.DefaultRequestHeaders.UserAgent}");
        AnsiConsole.MarkupLine($"  [dim]Timeout:[/] {(client.Timeout == Timeout.InfiniteTimeSpan ? "none (deadlines come from the load settings)" : client.Timeout.ToString())}");
        AnsiConsole.MarkupLine($"  [dim]Mistyped name yields User-Agent:[/] [yellow]{(mistyped.DefaultRequestHeaders.UserAgent.Count == 0 ? "(empty)" : mistyped.DefaultRequestHeaders.UserAgent.ToString())}[/]");
    }
}