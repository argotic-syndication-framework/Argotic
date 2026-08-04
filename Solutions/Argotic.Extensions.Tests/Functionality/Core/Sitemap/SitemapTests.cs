using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Unit tests for Sitemap XML parsing and serialization.
/// These tests verify the correct parsing of sitemap XML format.
/// </summary>
[TestClass]
public class SitemapTests
{
    #region XML Parsing Tests

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
        var expectedValues = new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };
        var actualValues = new List<string>();

        while (changefreqs.MoveNext())
        {
            actualValues.Add(changefreqs.Current!.Value);
        }

        actualValues.Count.ShouldBe(7);
        actualValues.ShouldBe(expectedValues);
    }

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
        var priorityValues = new List<decimal>();

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

    [TestMethod]
    public void ParsePriority_OutOfRange_ReturnsFalse()
    {
        // Arrange
        var invalidValues = new[] { "1.1", "2.0", "-0.1", "-1.0", "100" };

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
        var dates = new List<string>();

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
        var dates = new List<string>();

        while (lastmods.MoveNext())
        {
            dates.Add(lastmods.Current!.Value);
        }

        // Assert - Only 2 of 3 sitemaps have lastmod
        dates.Count.ShouldBe(2);
        dates[0].ShouldBe("2024-01-15T10:00:00Z");
        dates[1].ShouldBe("2024-01-14T10:00:00Z");
    }

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

    [TestMethod]
    public void RoundTrip_ParseWriteParse_PreservesUrlData()
    {
        // Arrange - Parse original
        using StringReader originalDocReader = new(FeedTestData.FullSitemap);
        XPathDocument originalDoc = new(originalDocReader);
        XPathNavigator originalNav = originalDoc.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(originalNav.NameTable);

        // Collect original data
        var originalUrls = new List<(string loc, string? lastmod, string? changefreq, string? priority)>();
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
            if (url.lastmod != null)
                writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, url.lastmod);
            if (url.changefreq != null)
                writer.WriteElementString("changefreq", SitemapUtility.SitemapNamespace, url.changefreq);
            if (url.priority != null)
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

        var newUrls = new List<(string loc, string? lastmod, string? changefreq, string? priority)>();
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
        SitemapUtility.TryParseChangeFrequency(changefreq?.Trim() ?? "", out var freq);
        freq.ShouldBe(SitemapChangeFrequency.Daily);
        SitemapUtility.TryParsePriority(priority?.Trim() ?? "", out var pri);
        pri.ShouldBe(0.8m);
    }

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