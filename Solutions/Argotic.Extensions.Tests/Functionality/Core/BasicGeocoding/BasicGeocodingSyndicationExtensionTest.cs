using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.BasicGeocoding;

/// <summary>
/// Covers <c>BasicGeocodingSyndicationExtension</c>, the W3C Basic Geo vocabulary that locates an item
/// with a <c>geo:lat</c> and a <c>geo:long</c> element.
/// </summary>
[TestClass]
public class BasicGeocodingSyndicationExtensionTest
{
    const string namespc = @"xmlns:geo=""http://www.w3.org/2003/01/geo/wgs84_pos#""";

    private readonly string nycText = "<lat xmlns=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">40.0000000</lat>" + Environment.NewLine +
                                      "<long xmlns=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">-74.0000000</long>";

    private const string strExtXml = "<geo:lat>41.0000000</geo:lat><geo:long>-74.1200000</geo:long>";

    public TestContext? TestContext { get; set; }
    /// <summary>
    /// Two extensions built from the same latitude and longitude compare equal, so <c>CompareTo</c>
    /// returns <c>0</c>.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingCompareToTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        BasicGeocodingSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// A decimal degree converts to the degrees-minutes-seconds spelling <c>12°34'56.78"</c>, with the
    /// arcseconds rounded to two decimal places.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingConvertDecimalToDegreesMinutesSecondsTest()
    {
        decimal value = new(12.582438888888888888888888888889);
        string expected = "12°34'56.78\"";
        string actual = BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(value);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The degrees-minutes-seconds spelling <c>12°34'56.78"</c> converts back to the decimal degrees it
    /// names.
    /// </summary>
    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimalTest()
    {
        string degreesMinutesSeconds = "12°34'56.78\"";
        decimal expected = new(12.582438888888888888888888888889);
        decimal actual = BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal(degreesMinutesSeconds);
        ((double)actual).ShouldBe((double)expected, 3e-6);
    }

    /// <summary>
    /// A negative coordinate carries its sign on the degrees alone, so the minutes and seconds count
    /// away from the equator rather than back towards it: <c>-36°30'0.00"</c> is <c>-36.5</c>.
    /// </summary>
    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimal_SubtractsMinutesFromANegativeDegree()
    {
        decimal actual = BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("-36°30'0.00\"");

        actual.ShouldBe(-36.5m);
    }

    /// <summary>
    /// The minus sign is a property of the text, not of the parsed degrees: <c>-0°30'0.00"</c> is half a
    /// degree south of the equator, not half a degree north of it.
    /// </summary>
    /// <remarks>
    ///     This is the case that rules out the obvious repair. <c>-0</c> parses to a decimal whose
    ///     <c>ToString</c> is <c>0</c>, whose <see cref="Math.Sign(decimal)"/> is <c>0</c> and for which
    ///     <c>&lt; 0</c> is <see langword="false"/> — verified, not assumed — so neither test recovers
    ///     the hemisphere from the parsed value. The sign has to be read off the string first.
    /// </remarks>
    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimal_KeepsTheHemisphereOfNegativeZeroDegrees()
    {
        decimal actual = BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("-0°30'0.00\"");

        actual.ShouldBe(-0.5m);
    }

    /// <summary>
    /// A trailing <c>S</c> or <c>W</c> puts the coordinate south or west of the origin; <c>N</c> and
    /// <c>E</c> leave it north or east of it.
    /// </summary>
    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimal_TakesTheSignFromTheHemisphereLetter()
    {
        BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°34'56.78\"S").ShouldBeLessThan(0m);
        BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°34'56.78\"W").ShouldBeLessThan(0m);
        BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°34'56.78\"N").ShouldBeGreaterThan(0m);
        BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°34'56.78\"E").ShouldBeGreaterThan(0m);
    }

    /// <summary>
    /// A hemisphere letter written inside the seconds field, ahead of the closing delimiter, is honoured
    /// the same way.
    /// </summary>
    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimal_TakesTheSignFromAHemisphereLetterInsideTheSecondsField()
    {
        BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°34'56.78S\"").ShouldBeLessThan(0m);
    }

    /// <summary>
    /// A minutes field carrying its own sign is rejected rather than silently subtracted.
    /// </summary>
    [TestMethod]
    public void ConvertDegreesMinutesSecondsToDecimal_RejectsASignedMinutesField()
    {
        Should.Throw<FormatException>(() => BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°-30'0.00\""));
        Should.Throw<FormatException>(() => BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal("12°30'-1.00\""));
    }

    /// <summary>
    /// A <see cref="decimal"/> whose scale is zero — which is what every <c>int</c> conversion produces,
    /// including the <c>Latitude = 40</c> this file's own fixtures use — converts to a whole number of
    /// degrees rather than to bare delimiters.
    /// </summary>
    [TestMethod]
    public void ConvertDecimalToDegreesMinutesSeconds_ConvertsAScaleZeroDecimal()
    {
        BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(40m).ShouldBe("40°0'0.00\"");
        BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(0m).ShouldBe("0°0'0.00\"");
    }

    /// <summary>
    /// Arcseconds that round up to <c>60</c> carry into the minutes, and a full sixty minutes carries
    /// into the degrees, so neither field is ever emitted at <c>60</c>.
    /// </summary>
    [TestMethod]
    public void ConvertDecimalToDegreesMinutesSeconds_CarriesTheRoundedArcseconds()
    {
        BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(0.9999999999m).ShouldBe("1°0'0.00\"");
        BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(59.999999999999m).ShouldBe("60°0'0.00\"");
    }

    /// <summary>
    /// The arcseconds always carry two decimal places, whatever the scale of the value they came from.
    /// </summary>
    [TestMethod]
    public void ConvertDecimalToDegreesMinutesSeconds_AlwaysEmitsTwoDecimalPlacesOfArcseconds()
    {
        BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(1.5m).ShouldBe("1°30'0.00\"");
    }

    /// <summary>
    /// A southern coordinate survives the trip out to degrees-minutes-seconds and back.
    /// </summary>
    [TestMethod]
    public void ASouthernCoordinate_SurvivesTheTripThroughDegreesMinutesSeconds()
    {
        string degreesMinutesSeconds = BasicGeocodingSyndicationExtension.ConvertDecimalToDegreesMinutesSeconds(-36.5m);

        degreesMinutesSeconds.ShouldBe("-36°30'0.00\"");
        BasicGeocodingSyndicationExtension.ConvertDegreesMinutesSecondsToDecimal(degreesMinutesSeconds).ShouldBe(-36.5m);
    }

    /// <summary>
    /// Two separately built extensions carrying the same coordinates are equal through the <c>object</c>
    /// overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingEqualsTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The hash code is stable across repeated calls, and two equal extensions agree on it.
    /// </summary>
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

    /// <summary>
    /// Saving a feed with the extension attached writes <c>geo:lat</c> and <c>geo:long</c> as separate
    /// elements padded to seven decimal places, with the namespace declared on the <c>rss</c> element.
    /// </summary>
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

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the latitude and longitude the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as BasicGeocodingSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected neither coordinate.
    /// </remarks>
    [TestMethod]
    public void BasicGeocodingFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        BasicGeocodingSyndicationExtension byType = item.FindExtension<BasicGeocodingSyndicationExtension>().ShouldNotBeNull();
        BasicGeocodingSyndicationExtension byPredicate = item
            .FindExtension(BasicGeocodingSyndicationExtension.MatchByType)
            .ShouldBeOfType<BasicGeocodingSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Latitude.ShouldBe(41.0m);
        byPredicate.Context.Longitude.ShouldBe(-74.12m);
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>BasicGeocodingSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = BasicGeocodingSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders <c>lat</c> and then <c>long</c> on separate lines, each declaring the
    /// Basic Geo namespace as its default rather than carrying the <c>geo</c> prefix.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingToStringTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        string expected = nycText;
        string actual = target.ToString();
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same two elements as
    /// <c>ToString</c>, without the line breaks between them.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingWriteToTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions at different coordinates are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingOpEqualityTestFailure()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions at the same coordinates are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingOpEqualityTestSuccess()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension at latitude 40 does not sort above the one at latitude 43 — latitude is the first
    /// member that differs between them.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingOpGreaterThanTest()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions at different coordinates are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingOpInequalityTest()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension at latitude 40 sorts below the one at latitude 43.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingOpLessThanTest()
    {
        BasicGeocodingSyndicationExtension first = CreateExtension1();
        BasicGeocodingSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the latitude and longitude the extension was built from.
    /// </summary>
    [TestMethod]
    public void BasicGeocodingContextTest()
    {
        BasicGeocodingSyndicationExtension target = CreateExtension1();
        BasicGeocodingSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Latitude.ShouldBe(40m);
        context.Longitude.ShouldBe(-74m);
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser: New York, at latitude 40 and
    /// longitude -74.
    /// </summary>
    /// <returns>An extension whose context carries that position.</returns>
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

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, at latitude 43 and longitude -80.
    /// </summary>
    /// <returns>An extension whose context carries that position.</returns>
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

    /// <summary>
    /// Builds a context carrying latitude 40 and longitude -74, without an extension around it.
    /// </summary>
    /// <returns>A context carrying that position.</returns>
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