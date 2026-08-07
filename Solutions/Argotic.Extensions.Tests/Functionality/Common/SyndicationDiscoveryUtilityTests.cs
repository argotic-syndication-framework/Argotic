using System.Text;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers the synchronous half of <see cref="SyndicationDiscoveryUtility"/>: naming a document's
/// format from its root element, naming it from a string, and pulling URLs out of HTML.
/// </summary>
[TestClass]
public class SyndicationDiscoveryUtilityTests
{
    /// <summary>
    /// Gets or sets the test context supplied by MSTest.
    /// </summary>
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A stream whose root element is <c>rss</c> is detected as RSS.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatGet_FromRssStream_ReturnsRss()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// A stream whose root element is <c>feed</c> is detected as Atom.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatGet_FromAtomStream_ReturnsAtom()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.Atom);
    }

    /// <summary>
    /// A stream whose root element is <c>opml</c> is detected as OPML.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatGet_FromOpmlStream_ReturnsOpml()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.Opml);
    }

    /// <summary>
    /// Well-formed XML with an unrecognised root element yields
    /// <see cref="SyndicationContentFormat.None"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatGet_FromUnknownXml_ReturnsNone()
    {
        const string unknownXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <unknown>
                <element>data</element>
            </unknown>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(unknownXml));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.None);
    }

    /// <summary>
    /// The by-name lookup is keyed on root element names, so <c>feed</c> — not <c>atom</c> — is what
    /// names the Atom format.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatByName_ReturnsCorrectFormat()
    {
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("rss").ShouldBe(SyndicationContentFormat.Rss);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("feed").ShouldBe(SyndicationContentFormat.Atom);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("opml").ShouldBe(SyndicationContentFormat.Opml);
    }

    /// <summary>
    /// The by-name lookup matches without regard to case.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatByName_CaseInsensitive()
    {
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("RSS").ShouldBe(SyndicationContentFormat.Rss);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("Rss").ShouldBe(SyndicationContentFormat.Rss);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("FEED").ShouldBe(SyndicationContentFormat.Atom);
    }

    /// <summary>
    /// An unrecognised name yields <see cref="SyndicationContentFormat.None"/>.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatByName_UnknownName_ReturnsNone()
    {
        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatByName("unknown");

        format.ShouldBe(SyndicationContentFormat.None);
    }

    /// <summary>
    /// A <see langword="null"/> or empty name yields <see cref="SyndicationContentFormat.None"/> rather
    /// than throwing.
    /// </summary>
    [TestMethod]
    public void SyndicationContentFormatByName_NullOrEmpty_ReturnsNone()
    {
        SyndicationDiscoveryUtility.SyndicationContentFormatByName(null!).ShouldBe(SyndicationContentFormat.None);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName(string.Empty).ShouldBe(SyndicationContentFormat.None);
    }

    /// <summary>
    /// The user agent sent on every request identifies the library by name, followed by its version.
    /// </summary>
    [TestMethod]
    public void FrameworkUserAgent_ReturnsValidUserAgent()
    {
        string userAgent = SyndicationDiscoveryUtility.FrameworkUserAgent;

        userAgent.ShouldNotBeNullOrEmpty();
        userAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

    /// <summary>
    /// A feed declaring an internal DTD subset is detected as RSS rather than rejected.
    /// </summary>
    /// <remarks>
    ///     Format detection used reader settings of its own that left <c>DtdProcessing</c> at the .NET
    ///     default of <c>Prohibit</c>, while the loader parses internal subsets deliberately so that
    ///     entities a feed declares resolve. A feed that <c>Load</c> accepts must not be one whose format
    ///     detection rejects.
    /// </remarks>
    [TestMethod]
    public void SyndicationContentFormatGet_FeedWithInternalDtd_DetectsFormat()
    {
        // Format detection used reader settings of its own that left DtdProcessing at the .NET
        // default of Prohibit, while the loader parses internal DTD subsets deliberately so that
        // entities a feed declares resolve. A feed that Load accepts must not be one whose format
        // detection rejects.
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE rss [<!ENTITY brand "Example">]>
            <rss version="2.0">
              <channel>
                <title>&brand; Feed</title>
                <link>https://example.com</link>
                <description>Feed with an internal DTD subset</description>
              </channel>
            </rss>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));

        SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream)
            .ShouldBe(SyndicationContentFormat.Rss);
    }

    /// <summary>
    /// Every anchor href in a page body is returned, one entry per anchor.
    /// </summary>
    [TestMethod]
    public void ExtractUrls_FromHtmlWithLinks_ExtractsUrls()
    {
        const string html = """
            <!DOCTYPE html>
            <html>
            <body>
                <p>Check out <a href="http://example.com/link1">this link</a> and
                   <a href="http://example.com/link2">that link</a>!</p>
            </body>
            </html>
            """;

        IList<Uri> urls = SyndicationDiscoveryUtility.ExtractUrls(html);

        urls.Count.ShouldBe(2);
        urls.ShouldContain(new Uri("http://example.com/link1"));
        urls.ShouldContain(new Uri("http://example.com/link2"));
    }

    /// <summary>
    /// Text carrying no markup yields an empty collection rather than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void ExtractUrls_FromTextWithNoUrls_ReturnsEmptyCollection()
    {
        const string text = "This is plain text with no URLs.";

        IList<Uri> urls = SyndicationDiscoveryUtility.ExtractUrls(text);

        urls.Count.ShouldBe(0);
    }

    /// <summary>
    /// Null and empty content are refused rather than treated as a page with no URLs in it.
    /// </summary>
    [TestMethod]
    public void ExtractUrls_NullOrEmpty_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => SyndicationDiscoveryUtility.ExtractUrls(null!));
        Should.Throw<ArgumentException>(() => SyndicationDiscoveryUtility.ExtractUrls(string.Empty));
    }
}