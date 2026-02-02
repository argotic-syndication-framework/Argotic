using System.Collections.ObjectModel;
using System.Text;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationDiscoveryUtilityTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void SyndicationContentFormatGet_FromRssStream_ReturnsRss()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalRss));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.Rss);
    }

    [TestMethod]
    public void SyndicationContentFormatGet_FromAtomStream_ReturnsAtom()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalAtom));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.Atom);
    }

    [TestMethod]
    public void SyndicationContentFormatGet_FromOpmlStream_ReturnsOpml()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.MinimalOpml));

        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatGet(stream);

        format.ShouldBe(SyndicationContentFormat.Opml);
    }

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

    [TestMethod]
    public void SyndicationContentFormatByName_ReturnsCorrectFormat()
    {
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("rss").ShouldBe(SyndicationContentFormat.Rss);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("feed").ShouldBe(SyndicationContentFormat.Atom);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("opml").ShouldBe(SyndicationContentFormat.Opml);
    }

    [TestMethod]
    public void SyndicationContentFormatByName_CaseInsensitive()
    {
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("RSS").ShouldBe(SyndicationContentFormat.Rss);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("Rss").ShouldBe(SyndicationContentFormat.Rss);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName("FEED").ShouldBe(SyndicationContentFormat.Atom);
    }

    [TestMethod]
    public void SyndicationContentFormatByName_UnknownName_ReturnsNone()
    {
        SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatByName("unknown");

        format.ShouldBe(SyndicationContentFormat.None);
    }

    [TestMethod]
    public void SyndicationContentFormatByName_NullOrEmpty_ReturnsNone()
    {
        SyndicationDiscoveryUtility.SyndicationContentFormatByName(null!).ShouldBe(SyndicationContentFormat.None);
        SyndicationDiscoveryUtility.SyndicationContentFormatByName(string.Empty).ShouldBe(SyndicationContentFormat.None);
    }

    [TestMethod]
    public void FrameworkUserAgent_ReturnsValidUserAgent()
    {
        string userAgent = SyndicationDiscoveryUtility.FrameworkUserAgent;

        userAgent.ShouldNotBeNullOrEmpty();
        userAgent.ShouldStartWith("Argotic-Syndication-Framework/");
    }

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

        Collection<Uri> urls = SyndicationDiscoveryUtility.ExtractUrls(html);

        urls.Count.ShouldBe(2);
        urls.ShouldContain(new Uri("http://example.com/link1"));
        urls.ShouldContain(new Uri("http://example.com/link2"));
    }

    [TestMethod]
    public void ExtractUrls_FromTextWithNoUrls_ReturnsEmptyCollection()
    {
        const string text = "This is plain text with no URLs.";

        Collection<Uri> urls = SyndicationDiscoveryUtility.ExtractUrls(text);

        urls.Count.ShouldBe(0);
    }

    [TestMethod]
    public void ExtractUrls_NullOrEmpty_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => SyndicationDiscoveryUtility.ExtractUrls(null!));
        Should.Throw<ArgumentException>(() => SyndicationDiscoveryUtility.ExtractUrls(string.Empty));
    }
}