using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the hashing algorithm used to create a hash value.
/// </summary>
/// <remarks>
///     The two values the Media RSS <c>algo</c> attribute defines. Both are broken for authentication and are
///     here only because the format specifies them; a hash in a feed is an integrity check against accidental
///     corruption, not a signature.
/// </remarks>
/// <seealso cref="YahooMediaHash"/>
public enum YahooMediaHashAlgorithm
{
    /// <summary>
    /// No hashing algorithm was specified. The specification's default when the attribute is absent is <see cref="MD5"/>.
    /// </summary>
    [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
    None = 0,

    /// <summary>
    /// Message-Digest 5.
    /// </summary>
    [EnumerationMetadata(DisplayName = "MD5", AlternateValue = "md5")]
    MD5 = 1,

    /// <summary>
    /// Secure Hash Algorithm 1.
    /// </summary>
    [EnumerationMetadata(DisplayName = "SHA-1", AlternateValue = "sha-1")]
    Sha1 = 2
}