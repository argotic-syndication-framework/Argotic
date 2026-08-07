using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummarySyndication;

/// <summary>
/// Covers the RSS syndication module, <c>http://purl.org/rss/1.0/modules/syndication/</c>: the update
/// period, frequency and base its context carries, the period-name mapping in both directions, how the
/// three elements are read from an RSS 2.0 item, and its comparison, equality and ordering contracts.
/// </summary>
[TestClass]
public class SiteSummaryUpdateSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:sy=""http://purl.org/rss/1.0/modules/syndication/""";

    private const string StrExtXml = "<sy:updatePeriod>hourly</sy:updatePeriod>"
                                     + "<sy:updateFrequency>2</sy:updateFrequency>"
                                     + "<sy:updateBase>2010-08-01T00:00:00Z</sy:updateBase>";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor yields an instance of the syndication-module extension type.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateSyndicationExtensionConstructorTest()
    {
        SiteSummaryUpdateSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SiteSummaryUpdateSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateCompareToTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateEqualsTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Hashing a populated extension returns a non-zero value rather than throwing.
    /// </summary>
    [TestMethod]
    public void SiteSummarySyndicationGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// An RSS 2.0 feed whose item carries <c>sy:</c> elements loads without error.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    /// <summary>
    /// Attaching the extension to an item and saving the feed produces non-empty XML.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateCreateXmlTest()
    {
        SiteSummaryUpdateSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// A loaded feed's single item reports that it has extensions, and the syndication-module one is found
    /// both by type argument and through the <c>MatchByType</c> predicate.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        SiteSummaryUpdateSyndicationExtension? itemExtension = item.FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(SiteSummaryUpdateSyndicationExtension.MatchByType) as SiteSummaryUpdateSyndicationExtension)
            .ShouldBeOfType<SiteSummaryUpdateSyndicationExtension>();
    }

    /// <summary>
    /// <c>MatchByType</c> accepts a syndication-module extension.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SiteSummaryUpdateSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> returns a non-empty rendering of a populated extension.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateToStringTest()
    {
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// <c>WriteTo</c> produces non-empty XML for a populated extension; the text itself is not asserted.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different context are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpEqualityTestFailure()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding the same context are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpEqualityTestSuccess()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension with the earlier <c>updateBase</c> is not greater than the other.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpGreaterThanTest()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding different context are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpInequalityTest()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Ordering is decided by <c>updateBase</c> before frequency or period, so the extension based in 2010
    /// is less than the one based in 2020 despite its shorter period.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpLessThanTest()
    {
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The context of a populated extension carries the hourly period, the frequency and the update base
    /// assigned to it.
    /// </summary>
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

    /// <summary>
    /// <c>Daily</c> renders as the element text <c>daily</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdatePeriodAsStringTest()
    {
        SiteSummaryUpdatePeriod value = SiteSummaryUpdatePeriod.Daily;
        string expected = "daily";
        string actual = SiteSummaryUpdateSyndicationExtension.PeriodAsString(value);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The name <c>weekly</c> maps onto <see cref="SiteSummaryUpdatePeriod.Weekly"/>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdatePeriodByNameTest()
    {
        SiteSummaryUpdatePeriod expected = SiteSummaryUpdatePeriod.Weekly;
        SiteSummaryUpdatePeriod actual = SiteSummaryUpdateSyndicationExtension.PeriodByName("weekly");
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// Assigning a <see langword="null"/> context throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateContextSetterThrowsOnNull()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// An RSS 2.0 item carrying <c>sy:updatePeriod</c>, <c>sy:updateFrequency</c> and an ISO 8601
    /// <c>sy:updateBase</c> fills all three onto the extension found on that item.
    /// </summary>
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
        SiteSummaryUpdateSyndicationExtension? itemExtension = item.FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Period.ShouldBe(SiteSummaryUpdatePeriod.Hourly);
        itemExtension.Context.Frequency.ShouldBe(2);
        itemExtension.Context.Base.Year.ShouldBe(2010);
        itemExtension.Context.Base.Month.ShouldBe(8);
        itemExtension.Context.Base.Day.ShouldBe(1);
    }

    /// <summary>
    /// <c>&lt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpLessThanOrEqualTest()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateOpGreaterThanOrEqualTest()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension first = CreateExtension1();
        SiteSummaryUpdateSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>MatchByType</c> rejects an extension from another family.
    /// </summary>
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

    /// <summary>
    /// An extension is not equal to a value of an unrelated type.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

    /// <summary>
    /// An extension sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
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

    /// <summary>
    /// Every period renders as its own lower-case name, and <c>None</c> as an <i>empty</i> string.
    /// </summary>
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

    /// <summary>
    /// Every lower-case period name maps back onto its enumeration value.
    /// </summary>
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

    /// <summary>
    /// A period name the module does not define maps to <c>None</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdatePeriodByNameReturnsNoneForUnknown()
    {
        // Arrange & Act
        SiteSummaryUpdatePeriod actual = SiteSummaryUpdateSyndicationExtension.PeriodByName("unknown");

        // Assert
        actual.ShouldBe(SiteSummaryUpdatePeriod.None);
    }

    /// <summary>
    /// A frequency of <c>0</c> throws <c>ArgumentOutOfRangeException</c>: the module counts at least one
    /// update per period.
    /// </summary>
    [TestMethod]
    public void SiteSummaryUpdateFrequencyThrowsOnInvalidValue()
    {
        // Arrange
        SiteSummaryUpdateSyndicationExtension ext = new();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => ext.Context.Frequency = 0);
    }

    /// <summary>
    /// An item carrying <c>sy:updateFrequency</c> of <c>0</c> is read, with the unusable frequency
    /// skipped and the rest of the feed intact.
    /// </summary>
    /// <remarks>
    ///     A feed is untrusted remote input and an aggregator hint is optional metadata, so losing the
    ///     whole document to one out-of-range integer is a catastrophic response to a trivial fault. The
    ///     house rule this settles on is that a guard throws for programmatic assignment — see
    ///     <see cref="SiteSummaryUpdateFrequencyThrowsOnInvalidValue"/>, which still holds — while a
    ///     loader skips an unusable value, as the two neighbouring branches of this method already did.
    /// </remarks>
    [TestMethod]
    public void AZeroUpdateFrequencyIsSkippedAndTheFeedSurvives()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(
            Namespc,
            "<sy:updatePeriod>hourly</sy:updatePeriod><sy:updateFrequency>0</sy:updateFrequency>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        SiteSummaryUpdateSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Period.ShouldBe(SiteSummaryUpdatePeriod.Hourly);
        itemExtension.Context.Frequency.ShouldBe(int.MinValue);
    }

    /// <summary>
    /// A negative <c>sy:updateFrequency</c> is skipped the same way.
    /// </summary>
    [TestMethod]
    public void ANegativeUpdateFrequencyIsSkippedAndTheFeedSurvives()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(
            Namespc,
            "<sy:updatePeriod>hourly</sy:updatePeriod><sy:updateFrequency>-4</sy:updateFrequency>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        SiteSummaryUpdateSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Frequency.ShouldBe(int.MinValue);
    }

    /// <summary>
    /// A frequency of <c>1</c> is the smallest the module admits, and it is read rather than skipped.
    /// </summary>
    [TestMethod]
    public void AnUpdateFrequencyOfOneIsRead()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(
            Namespc,
            "<sy:updatePeriod>hourly</sy:updatePeriod><sy:updateFrequency>1</sy:updateFrequency>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        SiteSummaryUpdateSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Frequency.ShouldBe(1);
    }

    /// <summary>
    /// An unparseable <c>sy:updateFrequency</c> is skipped and the feed loads — the behaviour the
    /// out-of-range case is measured against.
    /// </summary>
    /// <remarks>
    ///     A guard, not a characterisation: this passes both before and after. It is here because it is
    ///     the argument for the fix. The two neighbouring branches of the same method already skip an
    ///     unusable value rather than rejecting the document, and so does this one — the out-of-range
    ///     integer was the only input on the path that behaved differently.
    /// </remarks>
    [TestMethod]
    public void AnUnparseableUpdateFrequencyIsSkippedAndTheFeedSurvives()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(
            Namespc,
            "<sy:updatePeriod>hourly</sy:updatePeriod><sy:updateFrequency>often</sy:updateFrequency>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        SiteSummaryUpdateSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<SiteSummaryUpdateSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Period.ShouldBe(SiteSummaryUpdatePeriod.Hourly);
        itemExtension.Context.Frequency.ShouldBe(int.MinValue);
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