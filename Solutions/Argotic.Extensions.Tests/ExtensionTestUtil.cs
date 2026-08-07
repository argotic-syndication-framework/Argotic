using System.Globalization;
using System.Text;
using System.Xml;
using Argotic.Syndication;

namespace Argotic.Extensions.Tests;

/// <summary>
/// The shared harness for the extension family tests: writes an extension into a feed, and states what
/// the resulting document should be.
/// </summary>
/// <remarks>
///     <para>
///     The two halves are meant to be used together. <see cref="AddExtensionToXml"/> is the
///     <i>actual</i> — it attaches an extension to a real <see cref="RssItem"/> and serialises the feed
///     through the library's own save path. <see cref="GetWrappedXml"/> is the <i>expected</i> — the
///     same document written out by hand, with the caller supplying only the namespace declarations and
///     the extension elements. Comparing the two is how nearly every <c>WriteTo</c> test in the suite is
///     phrased, across 76 call sites.
///     </para>
///     <para>
///     The wrapper interpolates the running assembly's version into the <c>generator</c> element rather
///     than hard-coding one, because the library writes its own version there. A literal would make
///     every one of those tests fail on the next version bump, for no reason connected to the extension
///     under test.
///     </para>
/// </remarks>
internal static class ExtensionTestUtil
{
    /// <summary>
    /// Attaches an extension to a one-item RSS 2.0 feed and returns what the library serialises.
    /// </summary>
    /// <param name="ext">The extension to attach to the item.</param>
    /// <returns>
    ///     The feed as an XML fragment. The declaration is omitted so that the result lines up with
    ///     <see cref="GetWrappedXml"/>. Most call sites compare it verbatim; two trim it first, so a
    ///     comparison that fails on whitespace alone is worth checking against both.
    /// </returns>
    /// <remarks>
    ///     The channel and the item are fixed, and their values are the ones
    ///     <see cref="GetWrappedXml"/> writes: change one and the other has to change with it.
    /// </remarks>
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

    private const string strFullXml1 = @"<rss version=""2.0"" {0}><channel><title>Argotic - Extension Test</title><link>http://www.example.com/</link><description>Test of an extension</description><docs>https://www.rssboard.org/rss-specification</docs><generator>Argotic Syndication Framework {1}, https://github.com/argotic-syndication-framework/argotic/</generator><language>en-US</language><managingEditor>editor@example.com</managingEditor><webMaster>webmaster@example.com</webMaster><item><title>Item #1</title><description>text for First Item</description><link>http://www.example.com/item1.htm</link><pubDate>Sun, 01 Aug 2010 00:00:01 GMT</pubDate>{2}</item></channel></rss>";

    private static readonly CompositeFormat FullXmlFormat = CompositeFormat.Parse(strFullXml1);

    /// <summary>
    /// Wraps extension elements in the RSS 2.0 feed that <see cref="AddExtensionToXml"/> produces.
    /// </summary>
    /// <param name="namespc">
    ///     The namespace declarations to place on the <c>rss</c> element, as they would be written in the
    ///     document — for example <c>xmlns:dc="http://purl.org/dc/elements/1.1/"</c>.
    /// </param>
    /// <param name="strExt">The extension elements to place inside the item.</param>
    /// <returns>An RSS 2.0 document, with no XML declaration, carrying the supplied extension elements.</returns>
    /// <remarks>
    ///     Serves both directions. As the expected value it is compared against
    ///     <see cref="AddExtensionToXml"/>; as an input it is the document a load test parses, which is
    ///     why the extension elements are a parameter rather than a fixture.
    /// </remarks>
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