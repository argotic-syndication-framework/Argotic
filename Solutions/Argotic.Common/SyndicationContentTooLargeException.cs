
namespace Argotic.Common;

/// <summary>
/// Thrown when an HTTP response exceeds the number of bytes the caller was willing to accept.
/// </summary>
/// <remarks>
///     <para>
///     Derives from <see cref="HttpRequestException"/> rather than <see cref="IOException"/>, and that
///     is a contract decision rather than a taxonomic one: this library's own documentation tells
///     callers to catch <see cref="HttpRequestException"/> around a load, so an exception outside that
///     hierarchy would escape every handler written against the documented advice.
///     </para>
///     <para>
///     It carries no <c>[Serializable]</c> attribute and no <c>SerializationInfo</c> constructor. That
///     pattern is obsolete (SYSLIB0051) and the 179 attributes that used to be in this tree were
///     removed for the same reason.
///     </para>
/// </remarks>
public sealed class SyndicationContentTooLargeException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContentTooLargeException"/> class.
    /// </summary>
    /// <remarks>
    ///     This and the two <see cref="string"/>-taking constructors below are cold, and stay. CA1032
    ///     requires the standard set on any public exception type, and this solution treats a warning
    ///     as a build failure — so they are not optional, and a coverage report showing them at zero is
    ///     reporting a compliance requirement rather than dead weight.
    /// </remarks>
    public SyndicationContentTooLargeException()
        : this(0, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContentTooLargeException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SyndicationContentTooLargeException(string? message)
        : base(HttpRequestError.ConfigurationLimitExceeded, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContentTooLargeException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of this one.</param>
    public SyndicationContentTooLargeException(string? message, Exception? inner)
        : base(HttpRequestError.ConfigurationLimitExceeded, message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContentTooLargeException"/> class.
    /// </summary>
    /// <param name="maxBytes">The limit that was exceeded.</param>
    /// <param name="declaredLength">
    ///     The length the response declared, or <see langword="null"/> when it declared none — which is
    ///     the case for every decompressed and every chunked response.
    /// </param>
    public SyndicationContentTooLargeException(long maxBytes, long? declaredLength)
        : base(HttpRequestError.ConfigurationLimitExceeded, BuildMessage(maxBytes, declaredLength))
    {
        this.MaxBytes = maxBytes;
        this.DeclaredLength = declaredLength;
    }

    /// <summary>
    /// Gets the limit that was exceeded, in bytes.
    /// </summary>
    public long MaxBytes { get; }

    /// <summary>
    /// Gets the length the response declared, or <see langword="null"/> when it declared none.
    /// </summary>
    /// <remarks>
    ///     A <see langword="null"/> here is the normal case rather than an unusual one, and it tells the
    ///     caller <i>when</i> the limit was hit. With a declared length the request is refused before a
    ///     byte of the body is read; without one there is nothing to check in advance, so the limit is
    ///     enforced during the read and the connection is dropped part-way through.
    /// </remarks>
    public long? DeclaredLength { get; }

    private static string BuildMessage(long maxBytes, long? declaredLength) => declaredLength is { } declared
        ? $"The response declared a length of {declared} bytes, which exceeds the {maxBytes}-byte limit. No part of the body was read."
        : $"The response exceeded the {maxBytes}-byte limit while being read. It declared no length, so the limit could not be applied before reading began.";
}