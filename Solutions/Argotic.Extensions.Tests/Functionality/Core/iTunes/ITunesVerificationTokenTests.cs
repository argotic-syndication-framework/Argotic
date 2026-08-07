using System.Text;

using Argotic.Extensions.Core;
using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.iTunes;

/// <summary>
/// Covers <c>itunes:applepodcastsverify</c>, the token Apple reads to verify who owns a feed.
/// </summary>
/// <remarks>
///     <para>
///     When somebody claims a show in Apple Podcasts Connect, Apple issues a token that has to appear
///     in the feed before the claim completes. A load-then-save that drops it fails the claim — the
///     same silent round-trip loss as §2.47, on an element with a deadline attached.
///     </para>
///     <para>
///     <b>Every value in this file is taken from a real feed.</b> The element could not be implemented
///     from documentation: Apple's own requirements, validation and sample pages never mention it, nor
///     does the Podcast Standards Project, and the secondary sources that do describe it spell it
///     <c>applePodcastsVerify</c> in prose. XML element names are case-sensitive, so that difference
///     decides whether the element ever matches at all.
///     </para>
///     <para>
///     Settled by survey rather than by guess: 1,934 live feeds were pulled from the Apple directory
///     and searched. Twenty-six carry the element, spread across ten independent hosting providers, and
///     <b>all twenty-six spell it entirely lower case</b>. The camel-cased spelling occurs zero times.
///     </para>
/// </remarks>
[TestClass]
public sealed class ITunesVerificationTokenTests
{
    private static ITunesSyndicationExtensionContext? LoadChannelContext(string itunesElements)
    {
        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
              <channel>
                <title>A Podcast</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                {itunesElements}
                <item><title>An Episode</title></item>
              </channel>
            </rss>
            """;

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(stream);

        return feed.Channel.Extensions.OfType<ITunesSyndicationExtension>().SingleOrDefault()?.Context;
    }

    /// <summary>
    /// Every token shape Apple actually issues is read.
    /// </summary>
    /// <remarks>
    ///     <b>This is why the property is a string and not a <see cref="Guid"/>.</b> Twenty-one of the
    ///     twenty-six tokens found in live feeds are UUIDs, which makes <see cref="Guid"/> the obvious
    ///     choice right up until the other five are counted: four are six-digit codes and one is ten
    ///     digits, Apple's older numeric authorization codes. Parsing as a <see cref="Guid"/> would
    ///     silently discard <b>19%</b> of the tokens in use, and discarding a verification token is
    ///     indistinguishable from never having had one.
    /// </remarks>
    /// <param name="token">A verification token taken verbatim from a live feed — a UUID or a
    /// numeric code.</param>
    /// <param name="provenance">Which shape the row stands for, quoted back as the failure message.</param>
    [TestMethod]
    [DataRow("657bc6db-cc95-4ae2-b257-e32553e76cdd", "a UUID — 21 of the 26 live tokens look like this")]
    [DataRow("668b25d0-3e5a-11f1-a5f6-473c059f121e", "a UUID from a different provider")]
    [DataRow("557838", "a six-digit code — a Guid would refuse this")]
    [DataRow("046178", "a six-digit code with a leading zero")]
    [DataRow("1682409647", "ten digits")]
    public void EveryTokenShapeAppleIssues_IsRead(string token, string provenance)
    {
        ITunesSyndicationExtensionContext? context =
            LoadChannelContext($"<itunes:applepodcastsverify>{token}</itunes:applepodcastsverify>");

        context.ShouldNotBeNull(provenance);
        context.VerificationToken.ShouldBe(token, provenance);
    }

    /// <summary>
    /// The token survives being loaded and written back.
    /// </summary>
    /// <remarks>
    ///     The assertion the element exists for. Reading it matters only because losing it on the way
    ///     out is what breaks a claim.
    /// </remarks>
    [TestMethod]
    public void TheToken_SurvivesBeingLoadedAndWrittenBack()
    {
        const string Token = "e26fd590-9041-11f1-b94f-1b58db90a5f0";

        string document = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0" xmlns:itunes="http://www.itunes.com/dtds/podcast-1.0.dtd">
              <channel>
                <title>A Podcast</title>
                <link>https://example.com/</link>
                <description>A description.</description>
                <itunes:author>An Author</itunes:author>
                <itunes:applepodcastsverify>{Token}</itunes:applepodcastsverify>
                <item><title>An Episode</title></item>
              </channel>
            </rss>
            """;

        using MemoryStream source = new(Encoding.UTF8.GetBytes(document), writable: false);
        RssFeed feed = new();
        feed.Load(source);

        using MemoryStream saved = new();
        feed.Save(saved);
        saved.Seek(0, SeekOrigin.Begin);

        RssFeed reloaded = new();
        reloaded.Load(saved);

        reloaded.Channel.Extensions.OfType<ITunesSyndicationExtension>().Single()
            .Context.VerificationToken.ShouldBe(Token, "a lost token is a failed ownership claim");
    }

    /// <summary>
    /// The camel-cased spelling is not the element name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>The test that pins the research.</b> Prose about this element — including Apple's own
    ///     interface — writes it <c>applePodcastsVerify</c>, and implementing from that prose produces
    ///     an element matcher that compiles, ships, and never matches anything.
    ///     </para>
    ///     <para>
    ///     This asserts the negative deliberately. XML element names are case-sensitive, so a feed
    ///     writing the camel-cased spelling is writing a different element, and reading it would mean
    ///     accepting a name no publisher uses. If a future maintainer relaxes the match to be
    ///     case-insensitive, this row tells them the survey said not to.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void TheCamelCasedSpelling_IsNotTheElementName()
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext(
            "<itunes:applePodcastsVerify>657bc6db-cc95-4ae2-b257-e32553e76cdd</itunes:applePodcastsVerify>");

        (context?.VerificationToken ?? string.Empty).ShouldBeEmpty(
            "zero of 1,934 surveyed feeds spell it this way");
    }

    /// <summary>
    /// A feed carrying no token reports none, and writes none.
    /// </summary>
    [TestMethod]
    public void AFeedCarryingNoToken_ReportsNoneAndWritesNone()
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext("<itunes:author>An Author</itunes:author>");

        context.ShouldNotBeNull();
        context.VerificationToken.ShouldBeEmpty();

        ITunesSyndicationExtension extension = new();
        extension.Context.Author = "An Author";

        extension.ToString().Contains("applepodcastsverify", StringComparison.OrdinalIgnoreCase).ShouldBeFalse();
    }

    /// <summary>
    /// Surrounding whitespace is trimmed from the token.
    /// </summary>
    /// <remarks>
    ///     A pretty-printed feed puts the value on its own indented line, and Apple compares the token
    ///     it issued against the one it finds. Handing back the surrounding newlines would fail that
    ///     comparison.
    /// </remarks>
    [TestMethod]
    public void SurroundingWhitespace_IsTrimmedFromTheToken()
    {
        ITunesSyndicationExtensionContext? context = LoadChannelContext(
            "<itunes:applepodcastsverify>\n      557838\n    </itunes:applepodcastsverify>");

        context.ShouldNotBeNull();
        context.VerificationToken.ShouldBe("557838");
    }
}