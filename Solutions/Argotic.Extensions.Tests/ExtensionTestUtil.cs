using Argotic.Syndication;
using System.Globalization;
using System.Xml;

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

    private const string strFullXml1 = @"<rss version=""2.0"" {0}><channel><title>Argotic - Extension Test</title><link>http://www.example.com/</link><description>Test of an extension</description><docs>http://www.rssboard.org/rss-specification</docs><generator>Argotic Syndication Framework {1}, http://www.codeplex.com/Argotic</generator><language>en-US</language><managingEditor>editor@example.com</managingEditor><webMaster>webmaster@example.com</webMaster><item><title>Item #1</title><description>text for First Item</description><link>http://www.example.com/item1.htm</link><pubDate>Sun, 01 Aug 2010 00:00:01 GMT</pubDate>{2}</item></channel></rss>";

    internal static string GetWrappedXml(string namespc, string strExt)
    {
        return string.Format(strFullXml1, namespc, typeof(ExtensionTestUtil).Assembly.GetName().Version?.ToString() ?? "0.0.0.0", strExt);
    }
}
