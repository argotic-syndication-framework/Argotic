using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Argotic.Common;

/// <summary>
/// The outcome of loading a syndication resource only if the origin says it has changed.
/// </summary>
/// <typeparam name="TResource">The type of syndication resource that was requested.</typeparam>
/// <remarks>
///     <para>
///     Both outcomes are successes. A <c>304</c> is the origin answering the question that was asked,
///     and it is the answer a polling caller wants most of the time — so it arrives as a result rather
///     than as an exception, which is what it would be if the same request were made through
///     <see cref="ISyndicationResource.LoadAsync(Uri, HttpClient, SyndicationResourceLoadSettings?, SyndicationRequestOptions?, CancellationToken)"/>.
///     </para>
///     <para>
///     <see cref="Validators"/> is always present and is what the caller stores for next time. On a
///     <c>304</c> it may differ from what was sent: an origin is entitled to rotate an <c>ETag</c> on a
///     not-modified response, and a caller who keeps sending the tag they started with revalidates
///     against a value the origin has stopped recognising.
///     </para>
/// </remarks>
public sealed class ConditionalLoadResult<TResource>
    where TResource : ISyndicationResource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalLoadResult{TResource}"/> class.
    /// </summary>
    /// <param name="resource">The loaded resource, or <b>null</b> if the origin reported no change.</param>
    /// <param name="validators">The validators to send on the next request.</param>
    /// <param name="statusCode">The status code the origin returned.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="validators"/> is a null reference.</exception>
    internal ConditionalLoadResult(TResource? resource, SyndicationValidators validators, HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(validators);

        this.Resource = resource;
        this.Validators = validators;
        this.StatusCode = statusCode;
    }

    /// <summary>
    /// Gets a value indicating whether the origin returned a new representation.
    /// </summary>
    /// <value><b>true</b> if <see cref="Resource"/> holds a freshly loaded resource; otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     Annotated with <see cref="MemberNotNullWhenAttribute"/>, so a caller who tests this reaches
    ///     <see cref="Resource"/> without a null check and without a null-forgiving operator. That is
    ///     the whole ergonomic argument for a result type over an out parameter here.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Resource))]
    public bool WasModified => this.Resource is not null;

    /// <summary>
    /// Gets the loaded resource.
    /// </summary>
    /// <value>The resource, or <b>null</b> when the origin reported no change.</value>
    public TResource? Resource { get; }

    /// <summary>
    /// Gets the validators to send on the next request for this resource.
    /// </summary>
    /// <value>
    ///     A <see cref="SyndicationValidators"/>, possibly <see cref="SyndicationValidators.None"/> when
    ///     the origin sent neither an <c>ETag</c> nor a <c>Last-Modified</c>.
    /// </value>
    /// <remarks>
    ///     Never null. An origin that supports no validators at all is a resource that must be
    ///     re-fetched every time, which is a fact worth representing as an empty pair rather than as an
    ///     absent one — the caller's storage and polling code stays the same shape either way.
    /// </remarks>
    public SyndicationValidators Validators { get; }

    /// <summary>
    /// Gets the status code the origin returned.
    /// </summary>
    /// <value>The <see cref="HttpStatusCode"/>, which is <see cref="HttpStatusCode.NotModified"/> exactly when <see cref="WasModified"/> is <b>false</b>.</value>
    public HttpStatusCode StatusCode { get; }
}