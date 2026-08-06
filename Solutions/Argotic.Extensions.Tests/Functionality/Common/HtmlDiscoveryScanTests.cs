using Argotic.Common;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the HTML scanning behind pingback and trackback discovery, which all runs through one
/// attribute-matching regular expression.
/// </summary>
/// <remarks>
///     <para>
///     Written because <c>ExtractPingbackNotificationServer</c> had <b>no tests at all</b> and was
///     about to be edited. Six <see cref="System.Text.RegularExpressions.Regex"/> instances in
///     <c>SyndicationDiscoveryUtility</c> were constructed as locals — parsed afresh on every call —
///     and moving them to <c>[GeneratedRegex]</c> touches every one of these methods. A green suite
///     that never executes the changed lines is not evidence, so these were written first and
///     confirmed against the unmodified code.
///     </para>
///     <para>
///     The attribute pattern has two alternations — quoted values and bare ones — and the bare branch
///     is the one nothing exercised. Both are covered here, along with single quotes, because real
///     HTML uses all three spellings and a regex change is exactly the kind that keeps one working and
///     silently drops another.
///     </para>
/// </remarks>
[TestClass]
public sealed class HtmlDiscoveryScanTests
{
    private const string PingbackMarkup = """
        <html><head>
        <link rel="pingback" href="https://example.com/xmlrpc.php" type="application/xml" title="Pingback endpoint">
        <link rel="alternate" type="application/rss+xml" href="https://example.com/feed.xml">
        </head><body><a href="https://example.com/post/1">A post</a></body></html>
        """;

    /// <summary>
    /// A conventional pingback link is found, with its optional attributes carried across.
    /// </summary>
    [TestMethod]
    public void APingbackLink_IsFoundWithItsOptionalAttributes()
    {
        HtmlAnchor? anchor = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(PingbackMarkup);

        anchor.ShouldNotBeNull();
        anchor.HRef.ShouldBe("https://example.com/xmlrpc.php");
        anchor.Title.ShouldBe("Pingback endpoint");
        anchor.Attributes["rel"].ShouldBe("pingback");
        anchor.Attributes["type"].ShouldBe("application/xml");
    }

    /// <summary>
    /// The attribute scan reads double-quoted, single-quoted and bare values alike.
    /// </summary>
    /// <remarks>
    ///     The bare branch is the second alternation of the pattern, and no test reached it before.
    ///     Real HTML emits all three, and a regex edit can keep one working while dropping another.
    /// </remarks>
    [TestMethod]
    [DataRow("""<link rel="pingback" href="https://example.com/rpc">""", "double quotes")]
    [DataRow("""<link rel='pingback' href='https://example.com/rpc'>""", "single quotes")]
    [DataRow("""<link rel=pingback href=https://example.com/rpc>""", "bare values")]
    public void TheAttributeScan_ReadsEverySpellingOfAnAttributeValue(string markup, string spelling)
    {
        HtmlAnchor? anchor = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(markup);

        anchor.ShouldNotBeNull(spelling);
        anchor.HRef.ShouldBe("https://example.com/rpc", spelling);
    }

    /// <summary>
    /// The relation is matched without regard to case.
    /// </summary>
    [TestMethod]
    [DataRow("PINGBACK")]
    [DataRow("PingBack")]
    [DataRow("pingback")]
    public void TheRelationIsMatched_WithoutRegardToCase(string relation)
    {
        string markup = $"""<link rel="{relation}" href="https://example.com/rpc">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(markup).ShouldNotBeNull();
    }

    /// <summary>
    /// Markup with no pingback relation yields nothing.
    /// </summary>
    [TestMethod]
    public void MarkupWithNoPingbackRelation_YieldsNothing()
    {
        const string Markup = """<html><head><link rel="alternate" href="https://example.com/feed.xml"></head></html>""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup).ShouldBeNull();
    }

    /// <summary>
    /// A root-relative endpoint is accepted on this platform, and a document-relative one is not.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>Characterisation of a defect, not an endorsement.</b> The guard is
    ///     <c>Uri.TryCreate(href, UriKind.Absolute, out _)</c>, and on Unix that succeeds for a leading
    ///     slash — <c>/xmlrpc.php</c> becomes <c>file:///xmlrpc.php</c>. On Windows, where an absolute
    ///     path looks like <c>C:\…</c>, the same markup is refused. So <b>discovery accepts a different
    ///     set of documents depending on the operating system</b>, and the anchor it hands back carries
    ///     the raw relative string for the caller to try to use.
    ///     </para>
    ///     <para>
    ///     The more damaging instance is <see cref="AProtocolRelativeEndpoint_IsReadAsAFileUri"/>:
    ///     protocol-relative URLs are ordinary in real HTML, and this reads them as file URIs on a
    ///     remote host.
    ///     </para>
    ///     <para>
    ///     Left as-is deliberately. Tightening the guard to require <c>http</c>/<c>https</c> is a
    ///     change to what discovery accepts, which is a decision rather than a bug fix, and it is not
    ///     what the regular-expression work touching this method was for. Pinned so the behaviour is
    ///     visible and any change to it is deliberate.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ARootRelativePingbackEndpoint_IsAcceptedOnThisPlatform()
    {
        const string Markup = """<link rel="pingback" href="/xmlrpc.php">""";

        HtmlAnchor? anchor = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup);

        if (OperatingSystem.IsWindows())
        {
            anchor.ShouldBeNull("a leading slash is not an absolute URI on Windows");
        }
        else
        {
            anchor.ShouldNotBeNull("on Unix a leading slash parses as an absolute file URI");
            anchor.HRef.ShouldBe("/xmlrpc.php", "and the unusable relative string is handed back verbatim");
        }
    }

    /// <summary>
    /// A protocol-relative endpoint is read as a file URI rather than as a web address.
    /// </summary>
    /// <remarks>
    ///     <c>//example.com/xmlrpc.php</c> is a perfectly ordinary way to write a URL in HTML, and
    ///     <see cref="Uri.TryCreate(string, UriKind, out Uri)"/> resolves it to
    ///     <c>file://example.com/xmlrpc.php</c>. The scan therefore reports a pingback server that no
    ///     caller can usefully contact. Pinned as characterisation for the same reason as above.
    /// </remarks>
    [TestMethod]
    public void AProtocolRelativeEndpoint_IsReadAsAFileUri()
    {
        const string Markup = """<link rel="pingback" href="//example.com/xmlrpc.php">""";

        HtmlAnchor? anchor = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup);

        anchor.ShouldNotBeNull();
        new Uri(anchor.HRef).Scheme.ShouldBe("file", "a protocol-relative URL is not recognised as one");
    }

    /// <summary>
    /// A document-relative endpoint, with no leading slash, is refused everywhere.
    /// </summary>
    /// <remarks>
    ///     The control. Without it the two cases above are equally consistent with "the guard does
    ///     nothing at all", which would be a different and larger claim.
    /// </remarks>
    [TestMethod]
    public void ADocumentRelativePingbackEndpoint_IsRefused()
    {
        const string Markup = """<link rel="pingback" href="xmlrpc.php">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup).ShouldBeNull();
    }

    /// <summary>
    /// When a page declares more than one pingback link, the last one wins.
    /// </summary>
    /// <remarks>
    ///     Characterisation, not endorsement: the loop does not stop at the first match, so the final
    ///     declaration overwrites the earlier ones. Pinned so that a rewrite which starts returning the
    ///     first has to say so rather than change behaviour quietly.
    /// </remarks>
    [TestMethod]
    public void WhenAPageDeclaresSeveralPingbackLinks_TheLastOneWins()
    {
        const string Markup = """
            <link rel="pingback" href="https://example.com/first">
            <link rel="pingback" href="https://example.com/second">
            """;

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup)!.HRef
            .ShouldBe("https://example.com/second");
    }

    /// <summary>
    /// Null and empty markup are refused rather than treated as "no links found".
    /// </summary>
    [TestMethod]
    public void NullOrEmptyMarkup_IsRefused()
    {
        Should.Throw<ArgumentException>(() => SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(null!));
        Should.Throw<ArgumentException>(() => SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(string.Empty));
    }

    /// <summary>
    /// The URL scan finds both header links and body anchors.
    /// </summary>
    /// <remarks>
    ///     <c>ExtractUrls</c> runs two patterns over the markup and the attribute scan once per match,
    ///     so it is the method where the per-call construction multiplied hardest.
    /// </remarks>
    [TestMethod]
    public void TheUrlScan_FindsBothHeaderLinksAndBodyAnchors()
    {
        IList<Uri> urls = SyndicationDiscoveryUtility.ExtractUrls(PingbackMarkup);

        urls.ShouldContain(new Uri("https://example.com/feed.xml"));
        urls.ShouldContain(new Uri("https://example.com/post/1"));
    }

    /// <summary>
    /// Discoverable endpoints are still extracted from a conventional head.
    /// </summary>
    [TestMethod]
    public void DiscoverableEndpoints_AreExtractedFromAConventionalHead()
    {
        IList<DiscoverableSyndicationEndpoint> endpoints =
            SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(PingbackMarkup);

        endpoints.ShouldContain(endpoint => endpoint.Source == new Uri("https://example.com/feed.xml"));
    }
}