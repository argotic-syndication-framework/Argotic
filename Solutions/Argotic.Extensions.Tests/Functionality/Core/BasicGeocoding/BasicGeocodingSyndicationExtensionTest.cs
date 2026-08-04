using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.BasicGeocoding;

[TestClass]
public class BasicGeocodingSyndicationExtensionTest
{
    const string namespc = @"xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""";

    private readonly string nycText = "<lat xmlns=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">40.0000000</lat>" + Environment.NewLine +
                                      "<long xmlns=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">-74.0000000</long>";

    private const string strExtXml = "<geo:lat>41.0000000</geo:lat><geo:long>-74.1200000</geo:long>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void BasicGeocodingSyndicationExtensionConstructorTest()
    {
        BasicGeocodingSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<BasicGeocodingSyndicationExtension>();
    }

    [TestMethod]
    public void BasicGeocodingCompareToTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        BasicGeocodingSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void BasicGeocodingConvertDecimalToDegreesMinutesSecondsTest()
    {
        decimal value = new(12.582438888888888888888888888889);
        string expected = "12°34'56.78\"";
        string actual = BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimalTest()
    {
        string degreesMinutesSeconds = "12°34'56.78\"";
        decimal expected = new(12.582438888888888888888888888889);
        decimal actual = BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal(degreesMinutesSeconds);
        ((double)actual).ShouldBe((double)expected, 3e-6);
    }

    [TestMethod]
    public void BasicGeocodingEqualsTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BasicGeocodingGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        BasicGeocodingSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    [TestMethod]
    public void BasicGeocodingCreateXmlTest()
    {
        BasicGeocodingSyndicationExtension geo = new()
        {
            Context =
            {
                Latitude = 41.0m,
                Longitude = -74.12m
            }
        };

        string actual = ExtensionTestUtil.AddExtensionToXml(geo);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void BasicGeocodingFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        BasicGeocodingSyndicationExtension itemExtension = item.FindExtension<BasicGeocodingSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(BasicGeocodingSyndicationExtension.MatchByType) as BasicGeocodingSyndicationExtension)
            .ShouldBeOfType<BasicGeocodingSyndicationExtension>();
    }

    [TestMethod]
    public void BasicGeocodingMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = BasicGeocodingSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BasicGeocodingToStringTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        string expected = nycText;
        string actual = target.ToString();
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void BasicGeocodingWriteToTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(nycText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void BasicGeocodingOpEqualityTestFailure()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void BasicGeocodingOpEqualityTestSuccess()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BasicGeocodingOpGreaterThanTest()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void BasicGeocodingOpInequalityTest()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BasicGeocodingOpLessThanTest()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BasicGeocodingContextTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        BasicGeocodingSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Latitude.ShouldBe(40m);
        context.Longitude.ShouldBe(-74m);
    }

    private static BasicGeocodingSyndicationExtension CreateExtension1()
    {
        BasicGeocodingSyndicationExtension nyc = new()
        {
            Context =
            {
                Latitude = 40,
                Longitude = -74
            }
        };
        return nyc;
    }

    private static BasicGeocodingSyndicationExtension CreateExtension2()
    {
        BasicGeocodingSyndicationExtension nyc = new()
        {
            Context =
            {
                Latitude = 43,
                Longitude = -80
            }
        };
        return nyc;
    }

    public static BasicGeocodingSyndicationExtensionContext CreateContext1()
    {
        BasicGeocodingSyndicationExtensionContext nyc = new()
        {
            Latitude = 40,
            Longitude = -74
        };
        return nyc;
    }
}