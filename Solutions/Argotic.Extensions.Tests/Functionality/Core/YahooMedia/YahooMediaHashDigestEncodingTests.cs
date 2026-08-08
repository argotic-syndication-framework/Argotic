using System.Security.Cryptography;
namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

/// <summary>
/// Pins the encoding <see cref="YahooMediaHash.GenerateHash"/> returns a digest in.
/// </summary>
/// <remarks>
///     <para>
///         Media RSS states no encoding rule, so nothing here violates a MUST — but the specification's only
///         example is <c>&lt;media:hash algo="md5"&gt;dfdec888b72151965a34b4b59031290a&lt;/media:hash&gt;</c>,
///         thirty-two lowercase hexadecimal characters, and that is what real feeds carry.
///     </para>
///     <para>
///         <see cref="YahooMediaHash.Value"/> is stored and written verbatim and
///         <see cref="YahooMediaHash.CompareTo(YahooMediaHash)"/> is an ordinal string comparison, so while
///         this method returned base64 a digest generated here could never equal the same bytes read from a
///         feed. The assertions below name the exact expected string rather than checking non-emptiness,
///         which is what the three tests this replaces did.
///     </para>
/// </remarks>
[TestClass]
public class YahooMediaHashDigestEncodingTests
{
    /// <summary>
    /// The MD5 digest comes back as thirty-two lowercase hexadecimal characters.
    /// </summary>
    [TestMethod]
    public void GenerateHash_Md5_ReturnsLowercaseHexadecimal()
    {
        using MemoryStream stream = new("test content"u8.ToArray());

        string result = YahooMediaHash.GenerateHash(stream, YahooMediaHashAlgorithm.MD5);

#pragma warning disable CA5351 // Media RSS names MD5; this asserts the digest's encoding, not its strength
        result.ShouldBe(Convert.ToHexStringLower(MD5.HashData("test content"u8)));
#pragma warning restore CA5351
        result.Length.ShouldBe(32);
        result.ShouldBe(result.ToLowerInvariant());
    }

    /// <summary>
    /// The SHA-1 digest comes back as forty lowercase hexadecimal characters.
    /// </summary>
    [TestMethod]
    public void GenerateHash_Sha1_ReturnsLowercaseHexadecimal()
    {
        using MemoryStream stream = new("test content"u8.ToArray());

        string result = YahooMediaHash.GenerateHash(stream, YahooMediaHashAlgorithm.Sha1);

#pragma warning disable CA5350 // Media RSS names SHA-1; this asserts the digest's encoding, not its strength
        result.ShouldBe(Convert.ToHexStringLower(SHA1.HashData("test content"u8)));
#pragma warning restore CA5350
        result.Length.ShouldBe(40);
        result.ShouldBe(result.ToLowerInvariant());
    }

    /// <summary>
    /// A digest generated here equals the same digest read from a feed, which is the point of the change:
    /// the specification's own example value is what <c>GenerateHash</c> now produces for those bytes.
    /// </summary>
    [TestMethod]
    public void AGeneratedDigest_EqualsTheSameDigestReadFromAFeed()
    {
        using MemoryStream stream = new("test content"u8.ToArray());

        YahooMediaHash generated = new(YahooMediaHash.GenerateHash(stream, YahooMediaHashAlgorithm.MD5)) { Algorithm = YahooMediaHashAlgorithm.MD5 };

#pragma warning disable CA5351 // Media RSS names MD5; this asserts the digest's encoding, not its strength
        YahooMediaHash asAFeedWroteIt = new(Convert.ToHexStringLower(MD5.HashData("test content"u8))) { Algorithm = YahooMediaHashAlgorithm.MD5 };
#pragma warning restore CA5351

        generated.Equals(asAFeedWroteIt).ShouldBeTrue();
    }
}