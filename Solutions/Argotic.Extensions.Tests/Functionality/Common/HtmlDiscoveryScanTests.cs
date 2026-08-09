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
    /// <param name="markup">A <c>link rel="pingback"</c> element, spelled one of the three legal ways.</param>
    /// <param name="spelling">Names the spelling under test, and is used as the assertion message.</param>
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
    /// <param name="relation">The <c>rel</c> attribute value, cased differently in each row.</param>
    [TestMethod]
    [DataRow("PINGBACK")]
    [DataRow("PingBack")]
    [DataRow("pingback")]
    public void TheRelationIsMatched_WithoutRegardToCase(string relation)
    {
        string markup = $"""<link rel="{relation}" href="https://example.com/rpc">""";

        HtmlAnchor? anchor = SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(markup);

        anchor.ShouldNotBeNull(relation);
        anchor.HRef.ShouldBe("https://example.com/rpc", relation);
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
    /// A root-relative endpoint is refused on every platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The guard was <c>Uri.TryCreate(href, UriKind.Absolute, out _)</c>, and on Unix that succeeds
    ///     for a leading slash — <c>/xmlrpc.php</c> became <c>file:///xmlrpc.php</c> and was returned as
    ///     a discovered pingback endpoint, with the raw relative string handed back for the caller to
    ///     try to use. On Windows, where an absolute path looks like <c>C:\…</c>, the same markup was
    ///     refused. <b>Discovery accepted a different set of documents depending on the operating
    ///     system</b>, which is what makes this an unambiguous wrong answer rather than a question
    ///     about what the library should accept.
    ///     </para>
    ///     <para>
    ///     The decision recorded here: require <c>http</c> or <c>https</c>. A pingback endpoint is one
    ///     an XML-RPC <c>POST</c> is sent to, so a local filesystem path is never a usable answer, and
    ///     an OS-dependent result is not testable in any honest sense. Resolving the relative reference
    ///     against a base was not on the table — this method is given markup, not the address it came
    ///     from.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void ARootRelativePingbackEndpoint_IsRefused()
    {
        const string Markup = """<link rel="pingback" href="/xmlrpc.php">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup)
            .ShouldBeNull("INVERTED: and refused identically on Windows and on Unix");
    }

    /// <summary>
    /// A protocol-relative endpoint is refused rather than read as a file URI.
    /// </summary>
    /// <remarks>
    ///     The more damaging half of the same defect, because <c>//example.com/xmlrpc.php</c> is a
    ///     perfectly ordinary way to write a URL in HTML.
    ///     <see cref="Uri.TryCreate(string, UriKind, out Uri)"/> resolved it to
    ///     <c>file://example.com/xmlrpc.php</c>, so the scan reported a pingback server on a remote
    ///     <i>file share</i>. Refused now, for the same reason: the scheme is not one a pingback call
    ///     can be made on.
    /// </remarks>
    [TestMethod]
    public void AProtocolRelativeEndpoint_IsRefused()
    {
        const string Markup = """<link rel="pingback" href="//example.com/xmlrpc.php">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup)
            .ShouldBeNull("INVERTED: a protocol-relative URL is not a file URI on a remote host");
    }

    /// <summary>
    /// A document-relative endpoint, with no leading slash, is refused everywhere.
    /// </summary>
    /// <remarks>
    ///     The control that predates the scheme guard, and the one case the old guard already got
    ///     right. Without it the two rows above are equally consistent with "the guard refuses
    ///     everything", which would be a different and larger claim.
    /// </remarks>
    [TestMethod]
    public void ADocumentRelativePingbackEndpoint_IsRefused()
    {
        const string Markup = """<link rel="pingback" href="xmlrpc.php">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup).ShouldBeNull();
    }

    /// <summary>
    /// When a page declares more than one pingback link, the first one wins.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     A decision among undefined behaviours, not a specification fix. The Pingback specification
    ///     (<a href="https://www.hixie.ch/specs/pingback/pingback">§2</a>) says <i>"Pages MUST NOT
    ///     include more than one such element"</i> and defines no client behaviour for a page that
    ///     does, so neither answer is non-conformant.
    ///     </para>
    ///     <para>
    ///     First wins, for two reasons: it is how HTML <c>&lt;link&gt;</c> relations are conventionally
    ///     resolved, and it bounds the work a hostile page can extract from the scan. The loop used to
    ///     run to the end of the document reassigning on every match, so the last declaration won and
    ///     every declaration was paid for.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void WhenAPageDeclaresSeveralPingbackLinks_TheFirstOneWins()
    {
        const string Markup = """
            <link rel="pingback" href="https://example.com/first">
            <link rel="pingback" href="https://example.com/second">
            """;

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(Markup)!.HRef
            .ShouldBe("https://example.com/first", "INVERTED: the scan stops at the first match");
    }

    /// <summary>
    /// A pingback endpoint on a scheme other than HTTP is refused.
    /// </summary>
    /// <param name="href">An absolute URI whose scheme is not one a pingback server can be reached on.</param>
    /// <remarks>
    ///     The generalisation of the two rows above. Pingback is an XML-RPC <c>POST</c>; a
    ///     <c>mailto:</c>, <c>file:</c> or <c>javascript:</c> endpoint is not one the caller can
    ///     contact, and handing one back as a discovered server is an answer that can only mislead.
    /// </remarks>
    [TestMethod]
    [DataRow("mailto:webmaster@example.com")]
    [DataRow("file:///var/www/xmlrpc.php")]
    [DataRow("ftp://example.com/xmlrpc.php")]
    public void AnEndpointOnANonHttpScheme_IsRefused(string href)
    {
        string markup = $"""<link rel="pingback" href="{href}">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(markup)
            .ShouldBeNull($"INVERTED: {href} is not an endpoint a pingback call can reach");
    }

    /// <summary>
    /// An <c>https</c> endpoint is accepted, as an <c>http</c> one is.
    /// </summary>
    /// <remarks>
    ///     The control on the scheme guard. Without it, "requires HTTP" is indistinguishable from
    ///     "requires <c>http</c> exactly", and every endpoint on the modern web would be refused.
    /// </remarks>
    [TestMethod]
    [DataRow("http://example.com/xmlrpc.php")]
    [DataRow("https://example.com/xmlrpc.php")]
    public void AnHttpOrHttpsEndpoint_IsAccepted(string href)
    {
        string markup = $"""<link rel="pingback" href="{href}">""";

        SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(markup)!.HRef.ShouldBe(href);
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