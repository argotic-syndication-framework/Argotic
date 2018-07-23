namespace Argotic.Extensions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Argotic.Syndication;

    internal static class ExtensionTestUtil
    {
        internal static string AddExtensionToXml(SyndicationExtension ext)
        {
            RssFeed feed = new RssFeed(new Uri("http://www.example.com"), "Argotic - Extension Test");
            feed.Channel.Description = "Test of an extension";
            feed.Channel.ManagingEditor = "editor@example.com";
            feed.Channel.Webmaster = "webmaster@example.com";
            feed.Channel.Language = CultureInfo.CreateSpecificCulture("en-US");
#if false
			feed.Channel.Image = new RssImage();
			feed.Channel.Image.Title = "Example Image Title";
			feed.Channel.Image.Url = new Uri("http://www.example.com/sample.png");
			feed.Channel.Image.Link = new Uri("http://www.example.com/");
			feed.Channel.Image.Width = 32;
			feed.Channel.Image.Height = 32;
#endif
            RssItem item = new RssItem();
            item.Title = "Item #1";
            item.Link = new Uri("http://www.example.com/item1.htm");
            item.Description = "text for First Item";

            item.PublicationDate = new DateTime(2010, 8, 1, 0, 0, 1);
            feed.Channel.AddItem(item);

            item.AddExtension(ext);

            using (var sw = new StringWriter())
            using (var tw = new XmlTextWriter(sw))
            {
                feed.Save(tw);
                return sw.ToString();
            }
        }

        private const string strFullXml1 = @"<rss version=""2.0"" {0}><channel><title>Argotic - Extension Test</title><link>http://www.example.com/</link><description>Test of an extension</description><docs>http://www.rssboard.org/rss-specification</docs><generator>Argotic Syndication Framework 2008.0.2.0, http://www.codeplex.com/Argotic</generator><language>en-US</language><managingEditor>editor@example.com</managingEditor><webMaster>webmaster@example.com</webMaster><item><title>Item #1</title><description>text for First Item</description><link>http://www.example.com/item1.htm</link><pubDate>Sun, 01 Aug 2010 00:00:01 GMT</pubDate>{1}" +
                                             "</item></channel></rss>";

        internal static string GetWrappedXml(string namespc, string strExt)
        {
            return string.Format(strFullXml1, namespc, strExt);
        }
    }
}