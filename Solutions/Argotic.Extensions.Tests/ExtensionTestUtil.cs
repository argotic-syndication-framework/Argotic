using System.Globalization;
using System.Text;
using System.Xml;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests;

internal static class ExtensionTestUtil
{
    internal static string AddExtensionToXml(SyndicationExtension ext)
    {
        RssFeed feed = new(new Uri("http://www.example.com"), "Argotic - Extension Test")
        {
            Channel =
            {
                Description = "Test of an extension",
                ManagingEditor = "editor@example.com",
                Webmaster = "webmaster@example.com",
                Language = CultureInfo.CreateSpecificCulture("en-US")
            }
        };

        RssItem item = new()
        {
            Title = "Item #1",
            Link = new Uri("http://www.example.com/item1.htm"),
            Description = "text for First Item",
            PublicationDate = new DateTime(2010, 8, 1, 0, 0, 1)
        };

        feed.Channel.Items.Add(item);
        item.Extensions.Add(ext);

        using StringWriter sw = new();
        using XmlWriter tw = XmlWriter.Create(sw, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        });
        feed.Save(tw);
        tw.Flush();
        return sw.ToString();
    }

    private const string strFullXml1 = @"<rss version=""2.0"" {0}><channel><title>Argotic - Extension Test</title><link>http://www.example.com/</link><description>Test of an extension</description><docs>http://www.rssboard.org/rss-specification</docs><generator>Argotic Syndication Framework {1}, https://github.com/argotic-syndication-framework/argotic/</generator><language>en-US</language><managingEditor>editor@example.com</managingEditor><webMaster>webmaster@example.com</webMaster><item><title>Item #1</title><description>text for First Item</description><link>http://www.example.com/item1.htm</link><pubDate>Sun, 01 Aug 2010 00:00:01 GMT</pubDate>{2}</item></channel></rss>";

    private static readonly CompositeFormat FullXmlFormat = CompositeFormat.Parse(strFullXml1);

    internal static string GetWrappedXml(string namespc, string strExt) => string.Format(CultureInfo.InvariantCulture, FullXmlFormat, namespc, typeof(ExtensionTestUtil).Assembly.GetName().Version?.ToString() ?? "0.0.0.0", strExt);

    private const string strFullAtomXml = @"<?xml version=""1.0"" encoding=""utf-8""?><feed xmlns=""http://www.w3.org/2005/Atom"" {0}><id>urn:example:feed</id><title>Argotic - Extension Test</title><updated>2010-08-01T00:00:01Z</updated><entry><id>urn:example:entry:1</id><title>Item #1</title><updated>2010-08-01T00:00:01Z</updated><summary>text for First Item</summary>{1}</entry></feed>";

    private static readonly CompositeFormat FullAtomXmlFormat = CompositeFormat.Parse(strFullAtomXml);

    /// <summary>
    /// Wraps extension elements in an Atom 1.0 feed carrying a single entry.
    /// </summary>
    /// <remarks>
    ///     The counterpart to <see cref="GetWrappedXml"/>, which produces RSS 2.0 and is used at a hundred
    ///     call sites. Atom and RSS are filled by separate adapters
    ///     (<c>Atom10SyndicationResourceAdapter</c> and <c>Rss20SyndicationResourceAdapter</c>), and until
    ///     this existed every extension-attachment test in the suite went through RSS alone - so
    ///     twenty-four of the twenty-five extensions had never been attached to an Atom entry by any test.
    /// </remarks>
    /// <param name="namespc">The namespace declarations to place on the feed element.</param>
    /// <param name="strExt">The extension elements to place inside the entry.</param>
    /// <returns>An Atom 1.0 document carrying the supplied extension elements.</returns>
    internal static string GetWrappedAtomXml(string namespc, string strExt) => string.Format(CultureInfo.InvariantCulture, FullAtomXmlFormat, namespc, strExt);
}