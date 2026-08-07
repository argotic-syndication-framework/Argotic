using Argotic.Extensions.Core;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Pins the rule that a percent-encoded reserved character is not the same resource as the literal
/// character it encodes, for every type that orders a <see cref="Uri"/> member.
/// </summary>
/// <remarks>
///     <para>
///         RFC 3986 §2.2 is explicit: <i>"URIs that differ in the replacement of a reserved character
///         with its corresponding percent-encoded octet are not equivalent."</i> In a path segment
///         <c>%2F</c> is data and <c>/</c> is the segment delimiter, so <c>http://example.com/a%2Fb</c>
///         and <c>http://example.com/a/b</c> name two different resources.
///     </para>
///     <para>
///         These comparisons used <see cref="UriFormat.Unescaped"/>, which decodes <i>every</i> escape
///         including reserved ones, so the two collapsed to one and compared equal — while
///         <c>HashCodeUtility.Component(Uri)</c> hashes <see cref="Uri.ToString"/>, which keeps a
///         reserved escape escaped. Equal with different hash codes is a lookup miss: a
///         <see cref="Dictionary{TKey, TValue}"/> probe with an equal-but-distinct instance fails.
///         They now use <see cref="UriFormat.SafeUnescaped"/>, which decodes only the unreserved
///         escapes — RFC 3986 §6.2.2.2's percent-encoding normalization, and exactly what
///         <see cref="Uri.ToString"/> already does.
///     </para>
///     <para>
///         Each test carries its own control: two instances built from the same escaped URI must be
///         equal and hash equally, and a difference of case alone must stay equal and hash equally.
///         Without those a blanket "nothing is ever equal" regression would pass.
///     </para>
/// </remarks>
[TestClass]
public class PercentEncodingContractTests
{
    private static readonly Uri Encoded = new("http://example.com/a%2Fb");

    private static readonly Uri Plain = new("http://example.com/a/b");

    private static readonly Uri UpperCase = new("http://example.com/A%2FB");

    /// <summary>
    /// <c>%2F</c> and <c>/</c> are different resources; the same escaped URI is the same resource;
    /// and case alone never separates two URIs.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="build">Builds an instance carrying the supplied location.</param>
    private static void ShouldSeparateEncodedFromLiteral<T>(Func<Uri, T> build)
        where T : IComparable<T>
    {
        T encoded = build(Encoded);
        T plain = build(Plain);
        T encodedTwin = build(Encoded);
        T upper = build(UpperCase);

        // The defect: these compared equal while hashing differently.
        encoded.Equals(plain).ShouldBeFalse();
        Math.Sign(encoded.CompareTo(plain)).ShouldBe(-Math.Sign(plain.CompareTo(encoded)));
        encoded.CompareTo(plain).ShouldNotBe(0);

        // Control 1: the same URI is still the same resource, and still hashes alike.
        encoded.Equals(encodedTwin).ShouldBeTrue();
        encoded.GetHashCode().ShouldBe(encodedTwin.GetHashCode());

        // Control 2: case is still disregarded, in the comparison and in the hash alike.
        encoded.Equals(upper).ShouldBeTrue();
        encoded.GetHashCode().ShouldBe(upper.GetHashCode());
    }

    [TestMethod]
    public void SitemapVideo_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri =>
            new SitemapVideo { Title = "t", Description = "d", ContentLocation = uri });

    [TestMethod]
    public void SitemapVideoSegment_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new SitemapVideoSegment(uri));

    [TestMethod]
    public void SitemapImage_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new SitemapImage(uri));

    [TestMethod]
    public void SitemapHreflangLink_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new SitemapHreflangLink("en", uri));

    [TestMethod]
    public void PodcastChapters_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new PodcastChapters { Url = uri });

    [TestMethod]
    public void PodcastFunding_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new PodcastFunding { Url = uri });

    [TestMethod]
    public void PodcastLicense_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new PodcastLicense { Url = uri });

    [TestMethod]
    public void PodcastTranscript_SeparatesAnEncodedSlashFromALiteralSlash() =>
        ShouldSeparateEncodedFromLiteral(uri => new PodcastTranscript { Url = uri });

    [TestMethod]
    public void PodcastPerson_SeparatesAnEncodedUrlFromALiteralUrl() =>
        ShouldSeparateEncodedFromLiteral(uri => new PodcastPerson { Url = uri });

    [TestMethod]
    public void PodcastPerson_SeparatesAnEncodedImageUrlFromALiteralImageUrl() =>
        ShouldSeparateEncodedFromLiteral(uri => new PodcastPerson { ImageUrl = uri });

    [TestMethod]
    public void ITunes_SeparatesAnEncodedImageFromALiteralImage() =>
        ShouldSeparateEncodedFromLiteral(uri =>
        {
            ITunesSyndicationExtension extension = new();
            extension.Context.Image = uri;
            return extension;
        });

    [TestMethod]
    public void ITunes_SeparatesAnEncodedNewFeedUrlFromALiteralNewFeedUrl() =>
        ShouldSeparateEncodedFromLiteral(uri =>
        {
            ITunesSyndicationExtension extension = new();
            extension.Context.NewFeedUrl = uri;
            return extension;
        });

    /// <summary>
    /// The four <c>BlogChannel</c> context locations were not in the brief this fix came from; they
    /// carry the identical defect and are pinned for the same reason.
    /// </summary>
    [TestMethod]
    public void BlogChannel_SeparatesAnEncodedBlinkFromALiteralBlink() =>
        ShouldSeparateEncodedFromLiteral(uri =>
        {
            BlogChannelSyndicationExtension extension = new();
            extension.Context.Blink = uri;
            return extension;
        });

    [TestMethod]
    public void BlogChannel_SeparatesAnEncodedBlogRollFromALiteralBlogRoll() =>
        ShouldSeparateEncodedFromLiteral(uri =>
        {
            BlogChannelSyndicationExtension extension = new();
            extension.Context.BlogRoll = uri;
            return extension;
        });

    [TestMethod]
    public void BlogChannel_SeparatesAnEncodedChangesFromALiteralChanges() =>
        ShouldSeparateEncodedFromLiteral(uri =>
        {
            BlogChannelSyndicationExtension extension = new();
            extension.Context.Changes = uri;
            return extension;
        });

    [TestMethod]
    public void BlogChannel_SeparatesAnEncodedMySubscriptionsFromALiteralMySubscriptions() =>
        ShouldSeparateEncodedFromLiteral(uri =>
        {
            BlogChannelSyndicationExtension extension = new();
            extension.Context.MySubscriptions = uri;
            return extension;
        });
}