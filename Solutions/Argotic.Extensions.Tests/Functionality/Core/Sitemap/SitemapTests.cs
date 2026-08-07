using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers the shape of a <c>urlset</c> document: the elements a url may carry, the change-frequency and
/// priority vocabularies, the datetime and entity-escaping rules the protocol inherits from XML, and
/// writing a sitemap element by element.
/// </summary>
/// <remarks>
///     These assert against an <c>XPathDocument</c> built straight from the fixture rather than through
///     <see cref="Argotic.Syndication.Sitemap"/>. Where a test calls <see cref="SitemapUtility"/> it is
///     exercising this library; the rest describe the document format itself.
/// </remarks>
[TestClass]
public class SitemapTests
{
    #region XML Parsing Tests

    /// <summary>
    /// The minimal sitemap fixture holds exactly one <c>url</c>.
    /// </summary>
    [TestMethod]
    public void ParseMinimalSitemap_ContainsOneUrl()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator urls = navigator.Select("//sm:url", manager);

        // Assert
        urls.Count.ShouldBe(1);
    }

    /// <summary>
    /// That url's <c>loc</c> is <c>https://www.example.com/</c>.
    /// </summary>
    [TestMethod]
    public void ParseMinimalSitemap_ExtractsLocation()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNavigator? locNode = navigator.SelectSingleNode("//sm:url/sm:loc", manager);

        // Assert
        locNode.ShouldNotBeNull();
        locNode.Value.ShouldBe("https://www.example.com/");
    }

    /// <summary>
    /// The full sitemap fixture holds three <c>url</c> entries.
    /// </summary>
    [TestMethod]
    public void ParseFullSitemap_ContainsThreeUrls()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator urls = navigator.Select("//sm:url", manager);

        // Assert
        urls.Count.ShouldBe(3);
    }

    /// <summary>
    /// The first url carries all four elements the protocol defines — <c>loc</c>, <c>lastmod</c>,
    /// <c>changefreq</c> and <c>priority</c> — and each reads back as written.
    /// </summary>
    [TestMethod]
    public void ParseFullSitemap_ExtractsAllOptionalElements()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act - Get first URL
        XPathNavigator? firstUrl = navigator.SelectSingleNode("//sm:url[1]", manager);
        firstUrl.ShouldNotBeNull();

        string? loc = firstUrl.SelectSingleNode("sm:loc", manager)?.Value;
        string? lastmod = firstUrl.SelectSingleNode("sm:lastmod", manager)?.Value;
        string? changefreq = firstUrl.SelectSingleNode("sm:changefreq", manager)?.Value;
        string? priority = firstUrl.SelectSingleNode("sm:priority", manager)?.Value;

        // Assert
        loc.ShouldBe("https://www.example.com/");
        lastmod.ShouldBe("2024-01-15");
        changefreq.ShouldBe("daily");
        priority.ShouldBe("1.0");
    }

    /// <summary>
    /// A <c>urlset</c> with no children yields no <c>url</c> nodes.
    /// </summary>
    [TestMethod]
    public void ParseEmptySitemap_ContainsNoUrls()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.EmptySitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator urls = navigator.Select("//sm:url", manager);

        // Assert
        urls.Count.ShouldBe(0);
    }

    #endregion

    #region ChangeFreq Parsing Tests

    /// <summary>
    /// All seven change-frequency tokens appear in the fixture, in the order <c>always</c>, <c>hourly</c>,
    /// <c>daily</c>, <c>weekly</c>, <c>monthly</c>, <c>yearly</c>, <c>never</c>.
    /// </summary>
    [TestMethod]
    public void ParseSitemapWithAllChangeFrequencies_ExtractsAllValues()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithAllChangeFrequencies);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator changefreqs = navigator.Select("//sm:url/sm:changefreq", manager);

        // Assert
        string[] expectedValues = ["always", "hourly", "daily", "weekly", "monthly", "yearly", "never"];
        List<string> actualValues = [];

        while (changefreqs.MoveNext())
        {
            actualValues.Add(changefreqs.Current!.Value);
        }

        actualValues.Count.ShouldBe(7);
        actualValues.ShouldBe(expectedValues);
    }

    /// <summary>
    /// Every one of the seven tokens maps to its matching <see cref="SitemapChangeFrequency"/> member.
    /// </summary>
    [TestMethod]
    public void ParseChangeFrequencies_ConvertToEnumCorrectly()
    {
        // Arrange
        var testCases = new Dictionary<string, SitemapChangeFrequency>
        {
            { "always", SitemapChangeFrequency.Always },
            { "hourly", SitemapChangeFrequency.Hourly },
            { "daily", SitemapChangeFrequency.Daily },
            { "weekly", SitemapChangeFrequency.Weekly },
            { "monthly", SitemapChangeFrequency.Monthly },
            { "yearly", SitemapChangeFrequency.Yearly },
            { "never", SitemapChangeFrequency.Never }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            SitemapChangeFrequency result = SitemapUtility.ChangeFrequencyByName(testCase.Key);
            result.ShouldBe(testCase.Value, $"Failed for changefreq: {testCase.Key}");
        }
    }

    #endregion

    #region Priority Parsing Tests

    /// <summary>
    /// The boundary fixture's three priorities — <c>0.0</c>, <c>1.0</c> and <c>0.5</c> — all parse, so
    /// both ends of the permitted range are inclusive.
    /// </summary>
    [TestMethod]
    public void ParseSitemapWithPriorityBoundaries_ExtractsBoundaryValues()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithPriorityBoundaries);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator priorities = navigator.Select("//sm:url/sm:priority", manager);
        List<decimal> priorityValues = [];

        while (priorities.MoveNext())
        {
            if (SitemapUtility.TryParsePriority(priorities.Current!.Value, out decimal value))
            {
                priorityValues.Add(value);
            }
        }

        // Assert
        priorityValues.Count.ShouldBe(3);
        priorityValues.ShouldContain(0.0m);
        priorityValues.ShouldContain(1.0m);
        priorityValues.ShouldContain(0.5m);
    }

    /// <summary>
    /// Every tenth from <c>0.0</c> to <c>1.0</c> parses, and parses to the value written.
    /// </summary>
    [TestMethod]
    public void ParsePriority_ValidRange_ReturnsTrue()
    {
        // Arrange & Act & Assert
        for (decimal d = 0.0m; d <= 1.0m; d += 0.1m)
        {
            bool result = SitemapUtility.TryParsePriority(d.ToString("0.0", CultureInfo.InvariantCulture), out decimal parsed);
            result.ShouldBeTrue($"Failed for priority: {d}");
            parsed.ShouldBe(d, 0.001m, $"Parsed value mismatch for: {d}");
        }
    }

    /// <summary>
    /// A priority outside <c>0.0</c>–<c>1.0</c> is rejected, and the out parameter is left at the
    /// documented default of <c>0.5</c> rather than at the offending number.
    /// </summary>
    [TestMethod]
    public void ParsePriority_OutOfRange_ReturnsFalse()
    {
        // Arrange
        string[] invalidValues = ["1.1", "2.0", "-0.1", "-1.0", "100"];

        foreach (var value in invalidValues)
        {
            // Act
            bool result = SitemapUtility.TryParsePriority(value, out decimal parsed);

            // Assert
            result.ShouldBeFalse($"Should have failed for: {value}");
            parsed.ShouldBe(0.5m, $"Default value not set for: {value}");
        }
    }

    #endregion

    #region Date Format Parsing Tests

    /// <summary>
    /// The date-format fixture carries four <c>lastmod</c> spellings: a bare date, a UTC instant, and
    /// instants with a positive and a negative offset.
    /// </summary>
    [TestMethod]
    public void ParseSitemapWithDateFormats_ExtractsAllDates()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithDateFormats);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator lastmods = navigator.Select("//sm:url/sm:lastmod", manager);
        List<string> dates = [];

        while (lastmods.MoveNext())
        {
            dates.Add(lastmods.Current!.Value);
        }

        // Assert
        dates.Count.ShouldBe(4);
        dates[0].ShouldBe("2024-01-15");                    // Date only
        dates[1].ShouldBe("2024-01-15T10:30:00Z");          // DateTime UTC
        dates[2].ShouldBe("2024-01-15T10:30:00+05:00");     // DateTime with positive offset
        dates[3].ShouldBe("2024-01-15T10:30:00-08:00");     // DateTime with negative offset
    }

    /// <summary>
    /// A bare <c>2024-01-15</c> parses to that calendar day.
    /// </summary>
    [TestMethod]
    public void ParseDateOnlyFormat_CanBeParsedAsDateTime()
    {
        // Arrange
        string dateOnly = "2024-01-15";

        // Act
        bool result = DateTime.TryParse(dateOnly, out DateTime parsed);

        // Assert
        result.ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
    }

    /// <summary>
    /// Each of the three offset-bearing spellings parses as a <see cref="DateTimeOffset"/> on 15 January
    /// 2024, whichever way the offset leans.
    /// </summary>
    [TestMethod]
    public void ParseDateTimeWithTimezone_CanBeParsedAsDateTimeOffset()
    {
        // Arrange
        var testCases = new[]
        {
            "2024-01-15T10:30:00Z",
            "2024-01-15T10:30:00+05:00",
            "2024-01-15T10:30:00-08:00"
        };

        foreach (var dateTime in testCases)
        {
            // Act
            bool result = DateTimeOffset.TryParse(dateTime, out DateTimeOffset parsed);

            // Assert
            result.ShouldBeTrue($"Failed to parse: {dateTime}");
            parsed.Year.ShouldBe(2024, $"Year mismatch for: {dateTime}");
            parsed.Month.ShouldBe(1, $"Month mismatch for: {dateTime}");
            parsed.Day.ShouldBe(15, $"Day mismatch for: {dateTime}");
        }
    }

    #endregion

    #region URL Entity Escaping Tests

    /// <summary>
    /// An <c>&amp;amp;</c> in a <c>loc</c> is decoded by the XML reader, so the location reaches the
    /// caller as a single <c>&amp;</c> and needs no unescaping of its own.
    /// </summary>
    [TestMethod]
    public void ParseSitemapWithEscapedUrls_HandlesAmpersandCorrectly()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithEscapedUrls);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNavigator? firstLoc = navigator.SelectSingleNode("//sm:url[1]/sm:loc", manager);

        // Assert
        firstLoc.ShouldNotBeNull();
        // XML parser should automatically decode &amp; to &
        firstLoc.Value.ShouldBe("https://www.example.com/page?param1=value1&param2=value2");
    }

    /// <summary>
    /// A percent-encoded space stays percent-encoded: <c>%20</c> is not XML markup, so the reader passes
    /// it through untouched.
    /// </summary>
    [TestMethod]
    public void ParseSitemapWithEscapedUrls_HandlesEncodedSpaceCorrectly()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.SitemapWithEscapedUrls);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNavigator? secondLoc = navigator.SelectSingleNode("//sm:url[2]/sm:loc", manager);

        // Assert
        secondLoc.ShouldNotBeNull();
        secondLoc.Value.ShouldBe("https://www.example.com/search?q=test%20query");
    }

    /// <summary>
    /// A location with two query parameters forms an absolute URI whose query is the whole
    /// <c>?param1=value1&amp;param2=value2</c>.
    /// </summary>
    [TestMethod]
    public void UrlWithSpecialCharacters_CanBeCreatedAsUri()
    {
        // Arrange
        string urlWithParams = "https://www.example.com/page?param1=value1&param2=value2";

        // Act
        bool result = Uri.TryCreate(urlWithParams, UriKind.Absolute, out Uri? uri);

        // Assert
        result.ShouldBeTrue();
        uri.ShouldNotBeNull();
        uri.Query.ShouldBe("?param1=value1&param2=value2");
    }

    #endregion

    #region Sitemap Index Parsing Tests

    /// <summary>
    /// The minimal index fixture holds exactly one <c>sitemap</c> entry.
    /// </summary>
    [TestMethod]
    public void ParseMinimalSitemapIndex_ContainsOneSitemap()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);

        // Assert
        sitemaps.Count.ShouldBe(1);
    }

    /// <summary>
    /// That entry's <c>loc</c> is <c>https://www.example.com/sitemap1.xml</c>.
    /// </summary>
    [TestMethod]
    public void ParseMinimalSitemapIndex_ExtractsLocation()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNavigator? locNode = navigator.SelectSingleNode("//sm:sitemap/sm:loc", manager);

        // Assert
        locNode.ShouldNotBeNull();
        locNode.Value.ShouldBe("https://www.example.com/sitemap1.xml");
    }

    /// <summary>
    /// The full index fixture holds three <c>sitemap</c> entries.
    /// </summary>
    [TestMethod]
    public void ParseFullSitemapIndex_ContainsThreeSitemaps()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);

        // Assert
        sitemaps.Count.ShouldBe(3);
    }

    /// <summary>
    /// Only two of the three entries declare a <c>lastmod</c>, so selecting them yields two dates rather
    /// than three padded with blanks.
    /// </summary>
    [TestMethod]
    public void ParseFullSitemapIndex_ExtractsLastmodDates()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator lastmods = navigator.Select("//sm:sitemap/sm:lastmod", manager);
        List<string> dates = [];

        while (lastmods.MoveNext())
        {
            dates.Add(lastmods.Current!.Value);
        }

        // Assert - Only 2 of 3 sitemaps have lastmod
        dates.Count.ShouldBe(2);
        dates[0].ShouldBe("2024-01-15T10:00:00Z");
        dates[1].ShouldBe("2024-01-14T10:00:00Z");
    }

    /// <summary>
    /// An index with no children yields no <c>sitemap</c> nodes.
    /// </summary>
    [TestMethod]
    public void ParseEmptySitemapIndex_ContainsNoSitemaps()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.EmptySitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);

        // Assert
        sitemaps.Count.ShouldBe(0);
    }

    #endregion

    #region Format Detection Tests

    /// <summary>
    /// A sitemap is told apart from an index by its document element, which is <c>urlset</c>.
    /// </summary>
    [TestMethod]
    public void DetectSitemapFormat_UrlsetRoot_IsSitemap()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();
        string rootElementName = navigator.LocalName;

        // Assert
        rootElementName.ShouldBe("urlset");
    }

    /// <summary>
    /// An index is told apart by its document element, which is <c>sitemapindex</c>.
    /// </summary>
    [TestMethod]
    public void DetectSitemapFormat_SitemapindexRoot_IsSitemapIndex()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();
        string rootElementName = navigator.LocalName;

        // Assert
        rootElementName.ShouldBe("sitemapindex");
    }

    /// <summary>
    /// A sitemap's document element is qualified with the namespace
    /// <see cref="SitemapUtility.SitemapNamespace"/> names, not left unqualified.
    /// </summary>
    [TestMethod]
    public void DetectSitemapNamespace_CorrectNamespace()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemap);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();
        string? namespaceUri = navigator.NamespaceURI;

        // Assert
        namespaceUri.ShouldBe(SitemapUtility.SitemapNamespace);
    }

    #endregion

    #region XML Writing Tests

    /// <summary>
    /// A <c>urlset</c> written by hand with one fully populated url parses back with its <c>loc</c> intact.
    /// </summary>
    [TestMethod]
    public void WriteSitemapUrl_ProducesValidXml()
    {
        // Arrange
        using StringWriter stringWriter = new();
        using XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        });

        // Act
        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", SitemapUtility.SitemapNamespace);

        writer.WriteStartElement("url", SitemapUtility.SitemapNamespace);
        writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, "https://example.com/");
        writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, "2024-01-15");
        writer.WriteElementString("changefreq", SitemapUtility.SitemapNamespace, "daily");
        writer.WriteElementString("priority", SitemapUtility.SitemapNamespace, "1.0");
        writer.WriteEndElement(); // url

        writer.WriteEndElement(); // urlset
        writer.WriteEndDocument();
        writer.Flush();

        string xml = stringWriter.ToString();

        // Assert - Verify XML can be parsed back
        using StringReader documentReader = new(xml);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        XPathNavigator? loc = navigator.SelectSingleNode("//sm:url/sm:loc", manager);
        loc.ShouldNotBeNull();
        loc.Value.ShouldBe("https://example.com/");
    }

    /// <summary>
    /// A <c>sitemapindex</c> written by hand parses back with its entry's <c>loc</c> intact.
    /// </summary>
    [TestMethod]
    public void WriteSitemapIndex_ProducesValidXml()
    {
        // Arrange
        using StringWriter stringWriter = new();
        using XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        });

        // Act
        writer.WriteStartDocument();
        writer.WriteStartElement("sitemapindex", SitemapUtility.SitemapNamespace);

        writer.WriteStartElement("sitemap", SitemapUtility.SitemapNamespace);
        writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, "https://example.com/sitemap1.xml");
        writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, "2024-01-15T10:00:00Z");
        writer.WriteEndElement(); // sitemap

        writer.WriteEndElement(); // sitemapindex
        writer.WriteEndDocument();
        writer.Flush();

        string xml = stringWriter.ToString();

        // Assert - Verify XML can be parsed back
        using StringReader documentReader = new(xml);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        XPathNavigator? loc = navigator.SelectSingleNode("//sm:sitemap/sm:loc", manager);
        loc.ShouldNotBeNull();
        loc.Value.ShouldBe("https://example.com/sitemap1.xml");
    }

    #endregion

    #region Round-Trip Tests

    /// <summary>
    /// Reading the full sitemap, writing every url back, and reading again preserves each location,
    /// <c>lastmod</c>, <c>changefreq</c> and <c>priority</c>, including the ones that were absent.
    /// </summary>
    [TestMethod]
    public void RoundTrip_ParseWriteParse_PreservesUrlData()
    {
        // Arrange - Parse original
        using StringReader originalDocReader = new(FeedTestData.FullSitemap);
        XPathDocument originalDoc = new(originalDocReader);
        XPathNavigator originalNav = originalDoc.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(originalNav.NameTable);

        // Collect original data
        List<(string loc, string? lastmod, string? changefreq, string? priority)> originalUrls = [];
        XPathNodeIterator urlIterator = originalNav.Select("//sm:url", manager);

        while (urlIterator.MoveNext())
        {
            string loc = urlIterator.Current!.SelectSingleNode("sm:loc", manager)?.Value ?? "";
            string? lastmod = urlIterator.Current.SelectSingleNode("sm:lastmod", manager)?.Value;
            string? changefreq = urlIterator.Current.SelectSingleNode("sm:changefreq", manager)?.Value;
            string? priority = urlIterator.Current.SelectSingleNode("sm:priority", manager)?.Value;
            originalUrls.Add((loc, lastmod, changefreq, priority));
        }

        // Act - Write back
        using StringWriter stringWriter = new();
        using XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        });

        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", SitemapUtility.SitemapNamespace);

        foreach (var url in originalUrls)
        {
            writer.WriteStartElement("url", SitemapUtility.SitemapNamespace);
            writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, url.loc);
            if (url.lastmod is not null)
                writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, url.lastmod);
            if (url.changefreq is not null)
                writer.WriteElementString("changefreq", SitemapUtility.SitemapNamespace, url.changefreq);
            if (url.priority is not null)
                writer.WriteElementString("priority", SitemapUtility.SitemapNamespace, url.priority);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        string xml = stringWriter.ToString();

        // Parse again
        using StringReader newDocReader = new(xml);
        XPathDocument newDoc = new(newDocReader);
        XPathNavigator newNav = newDoc.CreateNavigator();
        XmlNamespaceManager newManager = SitemapUtility.CreateNamespaceManager(newNav.NameTable);

        List<(string loc, string? lastmod, string? changefreq, string? priority)> newUrls = [];
        XPathNodeIterator newUrlIterator = newNav.Select("//sm:url", newManager);

        while (newUrlIterator.MoveNext())
        {
            string loc = newUrlIterator.Current!.SelectSingleNode("sm:loc", newManager)?.Value ?? "";
            string? lastmod = newUrlIterator.Current.SelectSingleNode("sm:lastmod", newManager)?.Value;
            string? changefreq = newUrlIterator.Current.SelectSingleNode("sm:changefreq", newManager)?.Value;
            string? priority = newUrlIterator.Current.SelectSingleNode("sm:priority", newManager)?.Value;
            newUrls.Add((loc, lastmod, changefreq, priority));
        }

        // Assert
        // The guard below is relative, so two empty lists satisfy it and the loop then runs zero times:
        // a select that matched nothing round-tripped nothing and reported success. FullSitemap
        // declares exactly three <url> elements.
        originalUrls.Count.ShouldBe(3);
        newUrls.Count.ShouldBe(originalUrls.Count);
        for (int i = 0; i < originalUrls.Count; i++)
        {
            newUrls[i].loc.ShouldBe(originalUrls[i].loc);
            newUrls[i].lastmod.ShouldBe(originalUrls[i].lastmod);
            newUrls[i].changefreq.ShouldBe(originalUrls[i].changefreq);
            newUrls[i].priority.ShouldBe(originalUrls[i].priority);
        }
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Whitespace around an element's text survives the XPath read, and once trimmed <c>daily</c> and
    /// <c>0.8</c> parse.
    /// </summary>
    /// <remarks>
    ///     The trim is the caller's job here, not <c>XPathNavigator.Value</c>'s — a publisher who indents
    ///     the inside of a <c>changefreq</c> produces a value that fails to parse unless it is trimmed first.
    /// </remarks>
    [TestMethod]
    public void ParseSitemap_WithWhitespaceInElements_TrimsValues()
    {
        // Arrange
        const string sitemapWithWhitespace = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>  https://www.example.com/  </loc>
                    <changefreq>  daily  </changefreq>
                    <priority>  0.8  </priority>
                </url>
            </urlset>
            """;

        using StringReader documentReader = new(sitemapWithWhitespace);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        string? loc = navigator.SelectSingleNode("//sm:url/sm:loc", manager)?.Value;
        string? changefreq = navigator.SelectSingleNode("//sm:url/sm:changefreq", manager)?.Value;
        string? priority = navigator.SelectSingleNode("//sm:url/sm:priority", manager)?.Value;

        // Assert - XPath Value includes whitespace, so we need to trim
        loc?.Trim().ShouldBe("https://www.example.com/");
        SitemapUtility.TryParseChangeFrequency(changefreq?.Trim() ?? "", out var freq).ShouldBeTrue();
        freq.ShouldBe(SitemapChangeFrequency.Daily);
        SitemapUtility.TryParsePriority(priority?.Trim() ?? "", out var pri).ShouldBeTrue();
        pri.ShouldBe(0.8m);
    }

    /// <summary>
    /// A percent-encoded non-ASCII path reaches the caller in its encoded form and still forms an
    /// absolute URI.
    /// </summary>
    [TestMethod]
    public void ParseSitemap_UrlWithInternationalCharacters_HandlesCorrectly()
    {
        // Arrange
        const string sitemapWithInternational = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>https://www.example.com/%C3%BCmlaut</loc>
                </url>
            </urlset>
            """;

        using StringReader documentReader = new(sitemapWithInternational);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        string? loc = navigator.SelectSingleNode("//sm:url/sm:loc", manager)?.Value;

        // Assert
        loc.ShouldBe("https://www.example.com/%C3%BCmlaut");
        Uri.TryCreate(loc, UriKind.Absolute, out Uri? uri).ShouldBeTrue();
    }

    /// <summary>
    /// A location of some 1,900 characters is read whole, with nothing truncated.
    /// </summary>
    /// <remarks>
    ///     The protocol caps a location at 2,048 characters; this sits just under it.
    /// </remarks>
    [TestMethod]
    public void ParseSitemap_VeryLongUrl_HandlesCorrectly()
    {
        // Arrange - Create a URL near the 2048 character limit
        string longPath = new('a', 1900);
        string sitemapWithLongUrl = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>https://www.example.com/{longPath}</loc>
                </url>
            </urlset>
            """;

        using StringReader documentReader = new(sitemapWithLongUrl);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        string? loc = navigator.SelectSingleNode("//sm:url/sm:loc", manager)?.Value;

        // Assert
        loc.ShouldNotBeNull();
        loc.Length.ShouldBeGreaterThan(1900);
        loc.ShouldContain(longPath);
    }

    #endregion
}