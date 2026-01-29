using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class SyndicationResourceMetadataExtensionOperatorTests
    : ComparableExtensionOperatorTestsBase<SyndicationResourceMetadata>
{
    private static XPathNavigator CreateNavigatorFromXml(string xml)
    {
        using var reader = new StringReader(xml);
        var doc = new XPathDocument(reader);
        return doc.CreateNavigator();
    }

    protected override SyndicationResourceMetadata CreateInstance()
    {
        // RSS 2.0 document
        const string rss = """
            <rss version="2.0">
                <channel>
                    <title>Test B</title>
                </channel>
            </rss>
            """;
        return new SyndicationResourceMetadata(CreateNavigatorFromXml(rss));
    }

    protected override SyndicationResourceMetadata CreateLesserInstance()
    {
        // Atom 1.0 document (Atom comes before RSS alphabetically)
        const string atom = """
            <feed xmlns="http://www.w3.org/2005/Atom">
                <title>Test A</title>
            </feed>
            """;
        return new SyndicationResourceMetadata(CreateNavigatorFromXml(atom));
    }

    protected override SyndicationResourceMetadata CreateGreaterInstance()
    {
        // RSS 2.0 document with different content
        const string rss = """
            <rss version="2.0">
                <channel>
                    <title>Test C</title>
                </channel>
            </rss>
            """;
        return new SyndicationResourceMetadata(CreateNavigatorFromXml(rss));
    }
}