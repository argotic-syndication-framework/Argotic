using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummarySyndication;

[TestClass]
public class SiteSummaryUpdateSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:sy=""http://purl.org/rss/1.0/modules/syndication/""";

    private readonly string toStringText = "<updatePeriod xmlns=\"http://purl.org/rss/1.0/modules/syndication/\">hourly</updatePeriod>" + Environment.NewLine +
                                           "<updateFrequency xmlns=\"http://purl.org/rss/1.0/modules/syndication/\">2</updateFrequency>" + Environment.NewLine +
                                           "<updateBase xmlns=\"http://purl.org/rss/1.0/modules/syndication/\">2010-08-01T00:00:00Z</updateBase>";

    private const string StrExtXml = "<sy:updatePeriod>hourly</sy:updatePeriod>"
                                     + "<sy:updateFrequency>2</sy:updateFrequency>"
                                     + "<sy:updateBase>2010-08-01T00:00:00Z</sy:updateBase>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void SiteSummaryUpdateSyndicationExtensionConstructorTest()
    {
        SiteSummaryUpdateSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SiteSummaryUpdateSyndicationExtension>();
    }

    [TestMethod]
    public void SiteSummaryUpdateCompareToTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void SiteSummaryUpdateEqualsTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummarySyndicationGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void SiteSummaryUpdateLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void SiteSummaryUpdateCreateXmlTest()
    {
        SiteSummaryUpdateSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummaryUpdateFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        SiteSummaryUpdateSyndicationExtension itemExtension = item.FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(SiteSummaryUpdateSyndicationExtension.MatchByType) as SiteSummaryUpdateSyndicationExtension)
            .ShouldBeOfType<SiteSummaryUpdateSyndicationExtension>();
    }

    [TestMethod]
    public void SiteSummaryUpdateMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SiteSummaryUpdateSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryUpdateToStringTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummaryUpdateWriteToTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummaryUpdateOpEqualityTestFailure()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SiteSummaryUpdateOpEqualityTestSuccess()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryUpdateOpGreaterThanTest()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SiteSummaryUpdateOpInequalityTest()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryUpdateOpLessThanTest()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryUpdateContextTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        SiteSummaryUpdateSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Period.ShouldBe(SiteSummaryUpdatePeriod.Hourly);
        context.Frequency.ShouldBe(2);
        context.Base.ShouldBe(new DateTime(2010, 8, 1));
    }

    [TestMethod]
    public void SiteSummaryUpdatePeriodAsStringTest()
    {
        SiteSummaryUpdatePeriod value = SiteSummaryUpdatePeriod.Daily;
        string expected = "daily";
        string actual = SiteSummaryUpdateSyndicationExtension.PeriodAsString(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void SiteSummaryUpdatePeriodByNameTest()
    {
        SiteSummaryUpdatePeriod expected = SiteSummaryUpdatePeriod.Weekly;
        SiteSummaryUpdatePeriod actual = SiteSummaryUpdateSyndicationExtension.PeriodByName("weekly");
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void SiteSummaryUpdateContextSetterThrowsOnNull()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    [TestMethod]
    public void SiteSummaryUpdateRoundTripTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        SiteSummaryUpdateSyndicationExtension itemExtension = item.FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Period.ShouldBe(SiteSummaryUpdatePeriod.Hourly);
        itemExtension.Context.Frequency.ShouldBe(2);
        itemExtension.Context.Base.Year.ShouldBe(2010);
        itemExtension.Context.Base.Month.ShouldBe(8);
        itemExtension.Context.Base.Day.ShouldBe(1);
    }

    [TestMethod]
    public void SiteSummaryUpdateOpLessThanOrEqualTest()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryUpdateOpGreaterThanOrEqualTest()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryUpdateMatchByTypeReturnsFalseForDifferentType()
    {
        // Arrange
        ISyndicationExtension extension = new SiteSummarySlashSyndicationExtension();

        // Act
        bool actual = SiteSummaryUpdateSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SiteSummaryUpdateEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

    [TestMethod]
    public void SiteSummaryUpdateCompareToNullReturnsPositive()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void SiteSummaryUpdatePeriodAsStringForAllPeriods()
    {
        // Arrange & Act & Assert
        SiteSummaryUpdateSyndicationExtension.PeriodAsString(SiteSummaryUpdatePeriod.Daily).ShouldBe("daily");
        SiteSummaryUpdateSyndicationExtension.PeriodAsString(SiteSummaryUpdatePeriod.Hourly).ShouldBe("hourly");
        SiteSummaryUpdateSyndicationExtension.PeriodAsString(SiteSummaryUpdatePeriod.Monthly).ShouldBe("monthly");
        SiteSummaryUpdateSyndicationExtension.PeriodAsString(SiteSummaryUpdatePeriod.Weekly).ShouldBe("weekly");
        SiteSummaryUpdateSyndicationExtension.PeriodAsString(SiteSummaryUpdatePeriod.Yearly).ShouldBe("yearly");
        SiteSummaryUpdateSyndicationExtension.PeriodAsString(SiteSummaryUpdatePeriod.None).ShouldBe(string.Empty);
    }

    [TestMethod]
    public void SiteSummaryUpdatePeriodByNameForAllPeriods()
    {
        // Arrange & Act & Assert
        SiteSummaryUpdateSyndicationExtension.PeriodByName("daily").ShouldBe(SiteSummaryUpdatePeriod.Daily);
        SiteSummaryUpdateSyndicationExtension.PeriodByName("hourly").ShouldBe(SiteSummaryUpdatePeriod.Hourly);
        SiteSummaryUpdateSyndicationExtension.PeriodByName("monthly").ShouldBe(SiteSummaryUpdatePeriod.Monthly);
        SiteSummaryUpdateSyndicationExtension.PeriodByName("weekly").ShouldBe(SiteSummaryUpdatePeriod.Weekly);
        SiteSummaryUpdateSyndicationExtension.PeriodByName("yearly").ShouldBe(SiteSummaryUpdatePeriod.Yearly);
    }

    [TestMethod]
    public void SiteSummaryUpdatePeriodByNameReturnsNoneForUnknown()
    {
        // Arrange & Act
        SiteSummaryUpdatePeriod actual = SiteSummaryUpdateSyndicationExtension.PeriodByName("unknown");

        // Assert
        actual.ShouldBe(SiteSummaryUpdatePeriod.None);
    }

    [TestMethod]
    public void SiteSummaryUpdateFrequencyThrowsOnInvalidValue()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension ext = new();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => ext.Context.Frequency = 0);
    }

    private static SiteSummaryUpdateSyndicationExtension CreateExtension1()
    {
        SiteSummaryUpdateSyndicationExtension ext = new()
        {
            Context =
            {
                Period = SiteSummaryUpdatePeriod.Hourly,
                Frequency = 2,
                Base = new DateTime(2010, 8, 1)
            }
        };

        return ext;
    }

    private static SiteSummaryUpdateSyndicationExtension CreateExtension2()
    {
        SiteSummaryUpdateSyndicationExtension ext = new()
        {
            Context =
            {
                Period = SiteSummaryUpdatePeriod.Daily,
                Frequency = 1,
                Base = new DateTime(2020, 1, 1)
            }
        };

        return ext;
    }
}