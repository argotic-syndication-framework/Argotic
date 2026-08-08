namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Tests covering the retrieval and parsing behaviour that the move from <c>WebRequest</c> to
/// <see cref="HttpClient"/> is required to preserve.
/// </summary>
[TestClass]
public class SyndicationRetrievalRegressionTests
{
    /// <summary>
    /// A stream declaring <c>iso-8859-1</c> is decoded as that, so accented text survives the load.
    /// </summary>
    /// <remarks>
    ///     Decoding as UTF-8 regardless would not fail — it would replace each accented byte with
    ///     U+FFFD and parse cleanly, so the damage reaches the caller as data rather than as an error.
    /// </remarks>
    [TestMethod]
    public void CreateSafeNavigator_WithStream_DecodesUsingTheDeclaredEncoding()
    {
        // Arrange
        // The declaration names a non-UTF-8 encoding, so decoding as UTF-8 would replace the accented
        // bytes with U+FFFD instead of round-tripping them.
        byte[] data = Encoding.Latin1.GetBytes(
            "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>"
            + "<rss version=\"2.0\"><channel><title>Café Crème</title></channel></rss>");
        using MemoryStream stream = new(data);

        // Act
        XPathNavigator? titleNode = SyndicationEncodingUtility.CreateSafeNavigator(stream).SelectSingleNode("//title");
        titleNode.ShouldNotBeNull();
        string title = titleNode.Value;

        // Assert
        title.ShouldBe("Café Crème");
    }

    /// <summary>
    /// The load settings default, the framework constant and the documented legacy value are one number.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     There were three independent spellings of 100 seconds: the initialiser on
    ///     <see cref="SyndicationResourceLoadSettings.Timeout"/>, the constant behind the eight
    ///     discovery call sites, and the literal this test used to assert. Nothing tied them together,
    ///     so changing one changed the deadline for half the library.
    ///     </para>
    ///     <para>
    ///     The literal survives deliberately, in exactly one place. Asserting the settings default
    ///     against the constant alone would pass however the constant changed; the second assertion is
    ///     what makes the first mean something.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void SyndicationResourceLoadSettings_DefaultTimeout_MatchesTheLegacyRequestTimeout()
    {
        // Arrange & Act
        SyndicationResourceLoadSettings settings = new();

        // Assert
        settings.Timeout.ShouldBe(SyndicationEncodingUtility.DefaultRequestTimeout);
        SyndicationEncodingUtility.DefaultRequestTimeout.ShouldBe(TimeSpan.FromSeconds(100));
    }

    /// <summary>
    /// A caller's <c>User-Agent</c> replaces the framework's rather than being appended beside it.
    /// </summary>
    [TestMethod]
    public void CreateHttpRequestMessage_WithCustomUserAgent_ReplacesTheFrameworkUserAgent()
    {
        // Arrange
        SyndicationRequestOptions options = new() { UserAgent = "MyApp/1.0" };

        // Act
        using HttpRequestMessage request = SyndicationEncodingUtility.CreateHttpRequestMessage(
            new Uri("http://example.com/feed.xml"), options);
        string userAgent = request.Headers.UserAgent.ToString();

        // Assert
        userAgent.ShouldBe("MyApp/1.0");
        userAgent.ShouldNotContain("Argotic");
    }

    /// <summary>
    /// A request built with no options still identifies the framework to the origin.
    /// </summary>
    [TestMethod]
    public void CreateHttpRequestMessage_WithoutCustomUserAgent_SendsTheFrameworkUserAgent()
    {
        // Arrange & Act
        using HttpRequestMessage request = SyndicationEncodingUtility.CreateHttpRequestMessage(
            new Uri("http://example.com/feed.xml"));

        // Assert
        request.Headers.UserAgent.ToString().ShouldContain("Argotic");
    }

    /// <summary>
    /// A lower-case <c>gmt+02:00</c> zone parses, and its two hours are subtracted rather than ignored.
    /// </summary>
    [TestMethod]
    public void TryParseRfc822DateTime_WithLowerCaseGmtOffset_ParsesTheOffset()
    {
        // Arrange & Act
        bool parsed = SyndicationDateTimeUtility.TryParseRfc822DateTime(
            "Mon, 01 Jan 2024 12:00:00 gmt+02:00", out DateTime result);

        // Assert
        parsed.ShouldBeTrue();
        result.ToUniversalTime().ShouldBe(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// A trackback ping URL is recovered from an RDF island embedded in an HTML page.
    /// </summary>
    [TestMethod]
    public void ExtractTrackbackNotificationServers_FindsEmbeddedRdfBlock()
    {
        // Arrange
        string content = """
            <html><body><p>A post.</p>
            <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                     xmlns:dc="http://purl.org/dc/elements/1.1/"
                     xmlns:trackback="http://madskills.com/public/xml/rss/module/trackback/">
              <rdf:Description rdf:about="http://example.com/post/1"
                               dc:identifier="http://example.com/post/1"
                               dc:title="A post"
                               trackback:ping="http://example.com/trackback/1" />
            </rdf:RDF>
            </body></html>
            """;

        // Act
        IList<TrackbackDiscoveryMetadata> servers =
            SyndicationDiscoveryUtility.ExtractTrackbackNotificationServers(content);

        // Assert
        servers.Count.ShouldBe(1);
        servers[0].PingUrl.ShouldBe(new Uri("http://example.com/trackback/1"));
    }
}