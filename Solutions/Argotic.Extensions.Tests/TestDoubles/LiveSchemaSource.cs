using System.Net;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// Fetches published schemas and validator verdicts over the network, for the integration tier only.
/// </summary>
/// <remarks>
///     <para>
///     <b>Why anything is fetched at all.</b> Google's three sitemap extension schemas each carry
///     <c>Copyright 2010 Google Inc. All Rights Reserved.</c> in their own header. That is an explicit
///     reservation with no licence grant, so they are not committed to this repository — but the
///     defect they catch is real, and checking against a hand-written approximation would be worse than
///     not checking: it would look like conformance to Google's schema while being conformance to our
///     reading of it. Fetching the real file at test time keeps the check honest and the repository
///     clean. See <c>Schemas/NOTICE.md</c>.
///     </para>
///     <para>
///     <b>Unreachable is not the same as invalid, and callers must be able to tell.</b> Everything here
///     returns <see langword="null"/> or a <see cref="LiveResult"/> saying the service could not be
///     reached, so a test can report Inconclusive rather than pass or fail on a network problem. A
///     conformance test that goes green because it never reached the validator is the exact failure this
///     tier exists to avoid.
///     </para>
///     <para>
///     One <see cref="HttpClient"/> for the process, and results cached per URL, so a run that
///     validates several documents against the same schema fetches it once.
///     </para>
/// </remarks>
internal static class LiveSchemaSource
{
    /// <summary>The Google News sitemap extension schema.</summary>
    public const string NewsSchemaUrl = "https://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd";

    /// <summary>The Google image sitemap extension schema.</summary>
    public const string ImageSchemaUrl = "https://www.google.com/schemas/sitemap-image/1.1/sitemap-image.xsd";

    /// <summary>The Google video sitemap extension schema.</summary>
    public const string VideoSchemaUrl = "https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd";

    /// <summary>The W3C Feed Validator's direct-input endpoint.</summary>
    public const string FeedValidatorUrl = "https://validator.w3.org/feed/check.cgi";

    private static readonly HttpClient Client = CreateClient();

    private static readonly Dictionary<string, string?> Cache = [];

    private static readonly Lock CacheLock = new();

    /// <summary>
    /// Downloads a document, returning <see langword="null"/> when the origin cannot be reached.
    /// </summary>
    /// <param name="url">The document's address.</param>
    /// <returns>The document, or <see langword="null"/> if it could not be fetched.</returns>
    public static string? TryFetch(string url)
    {
        lock (CacheLock)
        {
            if (Cache.TryGetValue(url, out string? cached))
            {
                return cached;
            }
        }

        string? fetched = Attempt(() =>
        {
            using HttpResponseMessage response = Client.GetAsync(new Uri(url)).GetAwaiter().GetResult();
            return response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                : null;
        });

        lock (CacheLock)
        {
            Cache[url] = fetched;
        }

        return fetched;
    }

    /// <summary>
    /// Submits a document to the W3C Feed Validator's direct-input endpoint.
    /// </summary>
    /// <param name="document">The feed to validate.</param>
    /// <returns>The verdict, or one reporting the service was unreachable.</returns>
    /// <remarks>
    ///     Direct input rather than by URL, because the documents under test are not published anywhere
    ///     the validator could fetch them from. <c>manual=1</c> selects that mode and
    ///     <c>output=soap12</c> asks for the machine-readable response rather than the HTML page.
    /// </remarks>
    public static LiveResult ValidateFeed(string document)
    {
        ArgumentNullException.ThrowIfNull(document);

        string? soap = Attempt(() =>
        {
            using FormUrlEncodedContent form = new(
            [
                new KeyValuePair<string, string>("rawdata", document),
                new KeyValuePair<string, string>("manual", "1"),
                new KeyValuePair<string, string>("output", "soap12"),
            ]);

            using HttpResponseMessage response = Client.PostAsync(new Uri(FeedValidatorUrl), form).GetAwaiter().GetResult();
            return response.StatusCode == HttpStatusCode.OK
                ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                : null;
        });

        return soap is null ? LiveResult.Unreachable(FeedValidatorUrl) : LiveResult.From(soap);
    }

    private static HttpClient CreateClient()
    {
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(45),
        };

        // Some origins refuse an agent-less request, and a validator is entitled to know who is asking.
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Argotic-ConformanceTests/1.0 (+https://github.com/argotic-syndication-framework/Argotic)");
        return client;
    }

    /// <summary>
    /// Runs a network call, converting the failures that mean "unreachable" into a null result.
    /// </summary>
    /// <remarks>
    ///     Deliberately narrow. A DNS failure, a refused connection or a timeout means the service could
    ///     not be reached and the test is inconclusive; anything else is a real problem and is allowed to
    ///     propagate, because swallowing it would turn a genuine defect into a skipped test.
    /// </remarks>
    private static string? Attempt(Func<string?> call)
    {
        try
        {
            return call();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }
}