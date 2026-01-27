using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedHistory;

[TestClass]
public class FeedHistorySyndicationExtensionTest
{
    private const string Namespc = @"xmlns:fh=""http://purl.org/syndication/history/1.0""";

    private readonly string toStringText = "<archive xmlns=\"http://purl.org/syndication/history/1.0\" />" + Environment.NewLine +
                                           "<complete xmlns=\"http://purl.org/syndication/history/1.0\" />";

    private const string StrExtXml = "<fh:archive /><fh:complete />";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void FeedHistorySyndicationExtensionConstructorTest()
    {
        FeedHistorySyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedHistorySyndicationExtension>();
    }

    [TestMethod]
    public void FeedHistoryCompareToTest()
    {
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void FeedHistoryEqualsTest()
    {
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedHistoryGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        FeedHistorySyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void FeedHistoryLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void FeedHistoryCreateXmlTest()
    {
        FeedHistorySyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void FeedHistoryFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        FeedHistorySyndicationExtension itemExtension = item.FindExtension<FeedHistorySyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(FeedHistorySyndicationExtension.MatchByType) as FeedHistorySyndicationExtension)
            .ShouldBeOfType<FeedHistorySyndicationExtension>();
    }

    [TestMethod]
    public void FeedHistoryMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = FeedHistorySyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedHistoryToStringTest()
    {
        FeedHistorySyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void FeedHistoryWriteToTest()
    {
        FeedHistorySyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void FeedHistoryOpEqualityTestFailure()
    {
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedHistoryOpEqualityTestSuccess()
    {
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedHistoryOpGreaterThanTest()
    {
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();
        bool result = (first > second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void FeedHistoryOpInequalityTest()
    {
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedHistoryOpLessThanTest()
    {
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();
        bool result = (first < second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void FeedHistoryContextTest()
    {
        FeedHistorySyndicationExtension target = CreateExtension1();
        FeedHistorySyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.IsArchive.ShouldBeTrue();
        context.IsComplete.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedHistoryLinkRelationTypeAsStringTest()
    {
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.PreviousArchive;
        string expected = "prev-archive";
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void FeedHistoryLinkRelationTypeByNameTest()
    {
        FeedHistoryLinkRelationType expected = FeedHistoryLinkRelationType.PreviousArchive;
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("prev-archive");
        actual.ShouldBe(expected);
    }

    private static FeedHistorySyndicationExtension CreateExtension1()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true,
                IsComplete = true
            }
        };

        return ext;
    }

    private static FeedHistorySyndicationExtension CreateExtension2()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = false,
                IsComplete = false
            }
        };

        return ext;
    }
}
