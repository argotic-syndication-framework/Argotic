using System.Net;

namespace Argotic.Common;

/// <summary>
/// Loads syndication resources conditionally, fetching a body only when the origin says it has changed.
/// </summary>
/// <remarks>
///     <para>
///     The workload this library exists for is polling: many feeds, repeatedly, most of which have not
///     changed since last time. Every <c>LoadAsync</c> overload downloads and parses unconditionally,
///     and conditional GET was available only as a raw <see cref="ConditionalGetResult"/> that the
///     caller had to wire into a resource themselves — which meant knowing to pass the response stream
///     to <c>Load(Stream, settings)</c> and knowing to keep the validators off the result.
///     </para>
///     <para>
///     Static and generic rather than another member on <see cref="ISyndicationResource"/>: a
///     not-modified outcome has no resource in it, so it cannot be expressed by a method that fills an
///     existing instance. That is also why this does not go on the interface — <c>304</c> is a modelled
///     outcome here and an <see cref="HttpRequestException"/> everywhere else, and changing which of
///     those <c>LoadAsync</c> means is a contract change this work does not make.
///     </para>
/// </remarks>
public static class SyndicationResourceReader
{
    /// <summary>
    /// Loads a syndication resource, but only if the origin reports it has changed.
    /// </summary>
    /// <typeparam name="TResource">The type of syndication resource to load.</typeparam>
    /// <param name="source">A <see cref="Uri"/> that points to the resource.</param>
    /// <param name="validators">
    ///     The validators held from a previous fetch, or <b>null</b> — equivalent to
    ///     <see cref="SyndicationValidators.None"/> — to fetch unconditionally.
    /// </param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to fetch with. The caller owns its lifetime.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> to parse with. This value can be <b>null</b>.</param>
    /// <param name="maxResponseContentLength">
    ///     The most of the body to accept when <paramref name="settings"/> names no limit. Defaults to
    ///     <see cref="SyndicationContentLengthLimits.Feed"/>; pass
    ///     <see cref="SyndicationContentLengthLimits.Sitemap"/> for a sitemap or
    ///     <see cref="SyndicationContentLengthLimits.Archive"/> for an export.
    /// </param>
    /// <param name="requestOptions">Request-level options. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task whose result reports either a freshly loaded resource or that the origin sent
    ///     <c>304</c>. Both carry the validators to send next time.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///     The body is bounded, which the raw <see cref="ConditionalGetResult"/> path deliberately is
    ///     not — that type hands the caller a live stream and leaves the decision to them. Here the
    ///     stream is read on the caller's behalf, so the limit that applies to every other load applies
    ///     to this one too.
    ///     </para>
    ///     <para>
    ///     <b>The <c>Loaded</c> event fires on a <c>200</c> and not on a <c>304</c>, with a null
    ///     source.</b> It is raised by the resource's own <c>Load(Stream, settings)</c>, which is
    ///     reached only when there is a body — so a subscriber counting events is counting real
    ///     changes. The event arguments carry the navigator but no <see cref="Uri"/>, because
    ///     <c>Load(Stream, …)</c> has never known one.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="SyndicationContentTooLargeException">The response exceeds the effective size limit.</exception>
    /// <exception cref="HttpRequestException">The response status code indicates neither success nor a lack of modification.</exception>
    public static async Task<ConditionalLoadResult<TResource>> LoadIfModifiedAsync<TResource>(
        Uri source,
        SyndicationValidators? validators,
        HttpClient httpClient,
        SyndicationResourceLoadSettings? settings = null,
        long maxResponseContentLength = SyndicationContentLengthLimits.Feed,
        SyndicationRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
        where TResource : ISyndicationResource, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        using ConditionalGetResult conditional = await SyndicationDiscoveryUtility.ConditionalGetAsync(
            source,
            validators ?? SyndicationValidators.None,
            httpClient,
            requestOptions,
            cancellationToken).ConfigureAwait(false);

        // Taken from the response rather than echoed from what was sent: an origin is entitled to
        // rotate an ETag on a 304, and a caller who keeps sending their original revalidates against a
        // value the origin has stopped recognising.
        SyndicationValidators next = new(conditional.LastModified, conditional.ETag);
        HttpStatusCode status = conditional.StatusCode ?? HttpStatusCode.NotModified;

        if (!conditional.WasModified)
        {
            return new ConditionalLoadResult<TResource>(default, next, status);
        }

        long cap = settings?.MaxResponseContentLength ?? maxResponseContentLength;

        using Stream body = await conditional.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
        using PooledContentBuffer buffered = await SyndicationEncodingUtility.ReadContentAsync(
            body, conditional.ContentLength is >= 0 ? conditional.ContentLength : null, cap, cancellationToken)
            .ConfigureAwait(false);

        TResource resource = new();
        using Stream parseable = buffered.AsStream();
        resource.Load(parseable, settings);

        return new ConditionalLoadResult<TResource>(resource, next, status);
    }

    /// <summary>
    /// Loads a syndication resource using the shared <see cref="HttpClient"/>, but only if the origin reports it has changed.
    /// </summary>
    /// <typeparam name="TResource">The type of syndication resource to load.</typeparam>
    /// <param name="source">A <see cref="Uri"/> that points to the resource.</param>
    /// <param name="validators">The validators held from a previous fetch, or <b>null</b> to fetch unconditionally.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> to parse with. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>
    ///     A task whose result reports either a freshly loaded resource or that the origin sent
    ///     <c>304</c>. Both carry the validators to send next time.
    /// </returns>
    /// <remarks>
    ///     Uses the process-wide client, and is subject to
    ///     <see cref="SyndicationEncodingUtility.DefaultRequestTimeout"/>. Prefer the overload taking an
    ///     <see cref="HttpClient"/> where one is available — a polling caller is exactly the caller who
    ///     benefits from a client whose handler is rotated for them.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public static async Task<ConditionalLoadResult<TResource>> LoadIfModifiedAsync<TResource>(
        Uri source,
        SyndicationValidators? validators = null,
        SyndicationResourceLoadSettings? settings = null,
        CancellationToken cancellationToken = default)
        where TResource : ISyndicationResource, new()
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Not `settings?.Timeout ?? Default`. That reads a null Timeout on a supplied settings object
        // as "no opinion" and imposes the default, which is the opposite of what a null there means -
        // the caller asked for no deadline. Only the absence of settings falls back.
        TimeSpan? deadline = settings is null ? SyndicationEncodingUtility.DefaultRequestTimeout : settings.Timeout;
        if (deadline is { } period)
        {
            timeoutCts.CancelAfter(period);
        }

        return await LoadIfModifiedAsync<TResource>(
            source,
            validators,
            SyndicationEncodingUtility.SharedHttpClient,
            settings,
            SyndicationContentLengthLimits.Feed,
            null,
            timeoutCts.Token).ConfigureAwait(false);
    }
}