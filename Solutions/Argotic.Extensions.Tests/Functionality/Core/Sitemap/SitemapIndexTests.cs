using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Sitemap;

/// <summary>
/// Covers the shape of a <c>sitemapindex</c> document: the entries and locations the fixtures yield,
/// the <c>lastmod</c> spellings the protocol permits, writing an index element by element, and a
/// parse-write-parse round trip.
/// </summary>
/// <remarks>
///     These assert against an <c>XPathDocument</c> built straight from the fixture rather than through
///     <see cref="SitemapIndex"/>, so with the exception of the namespace manager they take from
///     <see cref="SitemapUtility"/> they describe the document format rather than this library's
///     object model. <c>SitemapIndex.Load</c> itself is exercised in <c>SitemapRegressionTests</c>.
/// </remarks>
[TestClass]
public class SitemapIndexTests
{
    #region Parsing Tests

    /// <summary>
    /// The minimal index fixture's document element is <c>sitemapindex</c>, qualified with the sitemap
    /// 0.9 namespace.
    /// </summary>
    [TestMethod]
    public void ParseMinimalSitemapIndex_HasCorrectRootElement()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.MinimalSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();

        // Act
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Assert
        navigator.LocalName.ShouldBe("sitemapindex");
        navigator.NamespaceURI.ShouldBe(SitemapUtility.SitemapNamespace);
    }

    /// <summary>
    /// The minimal index fixture holds exactly one <c>sitemap</c> entry.
    /// </summary>
    [TestMethod]
    public void ParseMinimalSitemapIndex_ContainsOneSitemapEntry()
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
    public void ParseMinimalSitemapIndex_ExtractsSitemapLocation()
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
    public void ParseFullSitemapIndex_ContainsThreeSitemapEntries()
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
    /// The three locations come back in document order, <c>sitemap1.xml</c> through <c>sitemap3.xml</c>.
    /// </summary>
    [TestMethod]
    public void ParseFullSitemapIndex_ExtractsAllSitemapLocations()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator locs = navigator.Select("//sm:sitemap/sm:loc", manager);
        List<string> locations = [];

        while (locs.MoveNext())
        {
            locations.Add(locs.Current!.Value);
        }

        // Assert
        locations.Count.ShouldBe(3);
        locations[0].ShouldBe("https://www.example.com/sitemap1.xml");
        locations[1].ShouldBe("https://www.example.com/sitemap2.xml");
        locations[2].ShouldBe("https://www.example.com/sitemap3.xml");
    }

    /// <summary>
    /// Each entry's <c>lastmod</c> is read alongside its <c>loc</c>, and the third entry, which declares
    /// none, yields <see langword="null"/> rather than a substituted date.
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
        List<(string loc, string? lastmod)> sitemapData = [];
        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);

        while (sitemaps.MoveNext())
        {
            string loc = sitemaps.Current!.SelectSingleNode("sm:loc", manager)?.Value ?? "";
            string? lastmod = sitemaps.Current.SelectSingleNode("sm:lastmod", manager)?.Value;
            sitemapData.Add((loc, lastmod));
        }

        // Assert
        sitemapData.Count.ShouldBe(3);
        sitemapData[0].lastmod.ShouldBe("2024-01-15T10:00:00Z");
        sitemapData[1].lastmod.ShouldBe("2024-01-14T10:00:00Z");
        sitemapData[2].lastmod.ShouldBeNull(); // Third sitemap has no lastmod
    }

    /// <summary>
    /// An index with no children yields no <c>sitemap</c> nodes.
    /// </summary>
    [TestMethod]
    public void ParseEmptySitemapIndex_ContainsNoSitemapEntries()
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

    #region Lastmod Date Format Tests

    /// <summary>
    /// The first entry's <c>lastmod</c> parses as a <see cref="DateTimeOffset"/> of 15 January 2024 at
    /// 10:00 with a zero offset, so the trailing <c>Z</c> is honoured rather than discarded.
    /// </summary>
    [TestMethod]
    public void ParseSitemapIndex_LastmodWithTimezone_ParsesCorrectly()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNavigator? lastmodNode = navigator.SelectSingleNode("//sm:sitemap[1]/sm:lastmod", manager);
        string? lastmodValue = lastmodNode?.Value;

        // Assert
        lastmodValue.ShouldNotBeNull();
        DateTimeOffset.TryParse(lastmodValue, out DateTimeOffset parsed).ShouldBeTrue();
        parsed.Year.ShouldBe(2024);
        parsed.Month.ShouldBe(1);
        parsed.Day.ShouldBe(15);
        parsed.Hour.ShouldBe(10);
        parsed.Offset.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// All three W3C datetime spellings the protocol permits for <c>lastmod</c> — a bare date, a UTC
    /// instant, and an explicit <c>+00:00</c> offset — resolve to the same calendar day.
    /// </summary>
    [TestMethod]
    public void ParseSitemapIndex_LastmodWithVariousFormats_ParsesCorrectly()
    {
        // Arrange
        var dateFormats = new[]
        {
            ("2024-01-15", new DateTime(2024, 1, 15)),
            ("2024-01-15T10:30:00Z", new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)),
            ("2024-01-15T10:30:00+00:00", new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc))
        };

        foreach (var (dateString, expectedDate) in dateFormats)
        {
            // Act
            bool parsed = DateTime.TryParse(dateString, out DateTime result);

            // Assert
            parsed.ShouldBeTrue($"Failed to parse: {dateString}");
            result.Year.ShouldBe(expectedDate.Year);
            result.Month.ShouldBe(expectedDate.Month);
            result.Day.ShouldBe(expectedDate.Day);
        }
    }

    #endregion

    #region Writing Tests

    /// <summary>
    /// An index written with a single entry parses back with that entry's <c>loc</c> intact.
    /// </summary>
    [TestMethod]
    public void WriteSitemapIndex_WithOneSitemap_ProducesValidXml()
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
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        string xml = stringWriter.ToString();

        // Assert - Can be parsed back
        using StringReader documentReader = new(xml);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);
        sitemaps.Count.ShouldBe(1);

        XPathNavigator? loc = navigator.SelectSingleNode("//sm:sitemap/sm:loc", manager);
        loc.ShouldNotBeNull();
        loc.Value.ShouldBe("https://example.com/sitemap1.xml");
    }

    /// <summary>
    /// A written <c>lastmod</c> survives the round trip verbatim, as <c>2024-01-15T10:00:00Z</c>.
    /// </summary>
    [TestMethod]
    public void WriteSitemapIndex_WithLastmod_ProducesValidXml()
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
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        string xml = stringWriter.ToString();

        // Assert
        using StringReader documentReader = new(xml);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        XPathNavigator? lastmod = navigator.SelectSingleNode("//sm:sitemap/sm:lastmod", manager);
        lastmod.ShouldNotBeNull();
        lastmod.Value.ShouldBe("2024-01-15T10:00:00Z");
    }

    /// <summary>
    /// Three entries written in sequence read back in the order they were written.
    /// </summary>
    [TestMethod]
    public void WriteSitemapIndex_WithMultipleSitemaps_ProducesValidXml()
    {
        // Arrange
        var sitemapUrls = new[]
        {
            "https://example.com/sitemap1.xml",
            "https://example.com/sitemap2.xml",
            "https://example.com/sitemap3.xml"
        };

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

        foreach (string url in sitemapUrls)
        {
            writer.WriteStartElement("sitemap", SitemapUtility.SitemapNamespace);
            writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, url);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        string xml = stringWriter.ToString();

        // Assert
        using StringReader documentReader = new(xml);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);
        sitemaps.Count.ShouldBe(3);

        XPathNodeIterator locs = navigator.Select("//sm:sitemap/sm:loc", manager);
        int index = 0;
        while (locs.MoveNext())
        {
            locs.Current!.Value.ShouldBe(sitemapUrls[index++]);
        }

        // The count above is over //sm:sitemap, not over the loc elements the loop walks, so it does
        // not stop this loop being vacuous.
        index.ShouldBe(3);
    }

    #endregion

    #region Round-Trip Tests

    /// <summary>
    /// Reading the full index, writing every entry back, and reading again preserves each location and
    /// its <c>lastmod</c> — including the entry that has none.
    /// </summary>
    [TestMethod]
    public void RoundTrip_ParseWriteParse_PreservesSitemapData()
    {
        // Arrange - Parse original
        using StringReader originalDocReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument originalDoc = new(originalDocReader);
        XPathNavigator originalNav = originalDoc.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(originalNav.NameTable);

        // Collect original data
        List<(string loc, string? lastmod)> originalSitemaps = [];
        XPathNodeIterator sitemapIterator = originalNav.Select("//sm:sitemap", manager);

        while (sitemapIterator.MoveNext())
        {
            string loc = sitemapIterator.Current!.SelectSingleNode("sm:loc", manager)?.Value ?? "";
            string? lastmod = sitemapIterator.Current.SelectSingleNode("sm:lastmod", manager)?.Value;
            originalSitemaps.Add((loc, lastmod));
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
        writer.WriteStartElement("sitemapindex", SitemapUtility.SitemapNamespace);

        foreach (var sitemap in originalSitemaps)
        {
            writer.WriteStartElement("sitemap", SitemapUtility.SitemapNamespace);
            writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, sitemap.loc);
            if (sitemap.lastmod is not null)
                writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, sitemap.lastmod);
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

        List<(string loc, string? lastmod)> newSitemaps = [];
        XPathNodeIterator newSitemapIterator = newNav.Select("//sm:sitemap", newManager);

        while (newSitemapIterator.MoveNext())
        {
            string loc = newSitemapIterator.Current!.SelectSingleNode("sm:loc", newManager)?.Value ?? "";
            string? lastmod = newSitemapIterator.Current.SelectSingleNode("sm:lastmod", newManager)?.Value;
            newSitemaps.Add((loc, lastmod));
        }

        // Assert
        // Relative guard: two empty lists pass it and the loop runs zero times. FullSitemapIndex
        // declares exactly three <sitemap> entries.
        originalSitemaps.Count.ShouldBe(3);
        newSitemaps.Count.ShouldBe(originalSitemaps.Count);
        for (int i = 0; i < originalSitemaps.Count; i++)
        {
            newSitemaps[i].loc.ShouldBe(originalSitemaps[i].loc);
            newSitemaps[i].lastmod.ShouldBe(originalSitemaps[i].lastmod);
        }
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// An index built with a hundred entries yields a hundred <c>sitemap</c> nodes.
    /// </summary>
    /// <remarks>
    ///     The protocol allows up to 50,000 entries in one index. A hundred is what this exercises, chosen
    ///     to keep the test's running time reasonable.
    /// </remarks>
    [TestMethod]
    public void ParseSitemapIndex_WithMaximumSitemaps_HandlesCorrectly()
    {
        // Arrange - Create sitemap index with many entries (sitemap protocol allows up to 50,000)
        // We'll test with 100 for reasonable test performance
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine("""<sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        for (int i = 1; i <= 100; i++)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"  <sitemap>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"    <loc>https://example.com/sitemap{i}.xml</loc>");
            builder.AppendLine(CultureInfo.InvariantCulture, $"  </sitemap>");
        }

        builder.AppendLine("</sitemapindex>");

        using StringReader documentReader = new(builder.ToString());
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator sitemaps = navigator.Select("//sm:sitemap", manager);

        // Assert
        sitemaps.Count.ShouldBe(100);
    }

    /// <summary>
    /// Every location in the full index fixture forms an absolute URI, and every one uses <c>https</c>.
    /// </summary>
    [TestMethod]
    public void ParseSitemapIndex_UrlsAreValidUris()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator locs = navigator.Select("//sm:sitemap/sm:loc", manager);

        // Assert
        // Every assertion in this test lives inside the loop and there was no count anywhere, so a
        // regression in CreateNamespaceManager's prefix binding — the one Argotic call the test makes —
        // empties the iterator and turns the test green.
        locs.Count.ShouldBe(3);

        while (locs.MoveNext())
        {
            string url = locs.Current!.Value;
            Uri.TryCreate(url, UriKind.Absolute, out Uri? uri).ShouldBeTrue($"Invalid URL: {url}");
            uri.ShouldNotBeNull();
            uri.Scheme.ShouldBe("https");
        }
    }

    /// <summary>
    /// An index may point at both a plain <c>.xml</c> sitemap and a gzipped <c>.xml.gz</c> one, and each
    /// suffix is carried through untouched.
    /// </summary>
    [TestMethod]
    public void ParseSitemapIndex_WithDifferentSitemapExtensions_HandlesCorrectly()
    {
        // Arrange
        const string sitemapIndexWithGz = """
            <?xml version="1.0" encoding="UTF-8"?>
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <sitemap>
                    <loc>https://example.com/sitemap1.xml</loc>
                </sitemap>
                <sitemap>
                    <loc>https://example.com/sitemap2.xml.gz</loc>
                </sitemap>
            </sitemapindex>
            """;

        using StringReader documentReader = new(sitemapIndexWithGz);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNodeIterator locs = navigator.Select("//sm:sitemap/sm:loc", manager);
        List<string> locations = [];
        while (locs.MoveNext())
        {
            locations.Add(locs.Current!.Value);
        }

        // Assert
        locations.Count.ShouldBe(2);
        locations[0].ShouldEndWith(".xml");
        locations[1].ShouldEndWith(".xml.gz");
    }

    #endregion

    #region Validation Tests

    /// <summary>
    /// An entry carrying only a <c>lastmod</c> yields no <c>loc</c> node — nothing synthesises the
    /// element the protocol requires.
    /// </summary>
    [TestMethod]
    public void ValidateSitemapIndex_AllSitemapsMustHaveLoc()
    {
        // Arrange - Sitemap without loc (invalid according to spec)
        const string invalidSitemapIndex = """
            <?xml version="1.0" encoding="UTF-8"?>
            <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <sitemap>
                    <lastmod>2024-01-15</lastmod>
                </sitemap>
            </sitemapindex>
            """;

        using StringReader documentReader = new(invalidSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act
        XPathNavigator? locNode = navigator.SelectSingleNode("//sm:sitemap/sm:loc", manager);

        // Assert - No loc element found
        locNode.ShouldBeNull();
    }

    /// <summary>
    /// The third entry of the full index has a <c>loc</c> and no <c>lastmod</c>, which the protocol permits.
    /// </summary>
    [TestMethod]
    public void ValidateSitemapIndex_LastmodIsOptional()
    {
        // Arrange
        using StringReader documentReader = new(FeedTestData.FullSitemapIndex);
        XPathDocument document = new(documentReader);
        XPathNavigator navigator = document.CreateNavigator();
        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(navigator.NameTable);

        // Act - Third sitemap has no lastmod
        XPathNavigator? thirdSitemap = navigator.SelectSingleNode("//sm:sitemap[3]", manager);
        XPathNavigator? thirdLastmod = thirdSitemap?.SelectSingleNode("sm:lastmod", manager);
        XPathNavigator? thirdLoc = thirdSitemap?.SelectSingleNode("sm:loc", manager);

        // Assert
        thirdSitemap.ShouldNotBeNull();
        thirdLoc.ShouldNotBeNull(); // loc is required
        thirdLastmod.ShouldBeNull(); // lastmod is optional
    }

    #endregion
}