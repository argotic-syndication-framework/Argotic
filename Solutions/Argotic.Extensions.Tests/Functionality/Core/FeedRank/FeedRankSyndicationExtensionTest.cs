namespace Argotic.Extensions.Tests;

using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class FeedRankSyndicationExtensionTest
{
    private const string namespc = @"xmlns:re=""http://purl.org/atompub/rank/1.0""";
    private const string nycText = "<rank p1:scheme=\"http://example.com/scheme.txt\" p1:domain=\"http://example.com/\" label=\"Title\" xmlns:p1=\"http://purl.org/atompub/rank/1.0\" xmlns=\"http://purl.org/atompub/rank/1.0\">1.0</rank>";
    private const string writeToText = "<rank p1:scheme=\"http://example.com/scheme.txt\" p1:domain=\"http://example.com/\" label=\"Title\" xmlns:p1=\"http://purl.org/atompub/rank/1.0\" xmlns=\"http://purl.org/atompub/rank/1.0\">1.0</rank>";
    private const string strExtXml = "<re:rank re:scheme=\"http://example.com/scheme.txt\" re:domain=\"http://example.com/\" label=\"Title\">1.0</re:rank>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void FeedRankSyndicationExtensionConstructorTest()
    {
        FeedRankSyndicationExtension target = new FeedRankSyndicationExtension();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedRankSyndicationExtension>();
    }

    [TestMethod]
    public void FeedRankCompareToTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void FeedRankEqualsTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("GetHashCode implementation is not deterministic across runs")]
    public void FeedRankGetHashCodeTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        int expected = 1719638022;
        int actual = target.GetHashCode();
        actual.ShouldBe(expected);
    }

    [TestMethod]
    [Ignore("Test requires manual verification of Load behavior")]
    public void FeedRankLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);
    }

    [TestMethod]
    public void FeedRankCreateXmlTest()
    {
        FeedRankSyndicationExtension re = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(re);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void FeedRankFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        FeedRankSyndicationExtension itemExtension = item.FindExtension<FeedRankSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(FeedRankSyndicationExtension.MatchByType) as FeedRankSyndicationExtension)
            .ShouldBeOfType<FeedRankSyndicationExtension>();
    }

    [TestMethod]
    public void FeedRankMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = FeedRankSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedRankToStringTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    [TestMethod]
    public void FeedRankWriteToTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(writeToText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void FeedRankOpEqualityTestFailure()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedRankOpEqualityTestSuccess()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedRankOpGreaterThanTest()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedRankOpInequalityTest()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedRankOpLessThanTest()
    {
        FeedRankSyndicationExtension first = CreateExtension1();
        FeedRankSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("Context equality comparison not implemented")]
    public void FeedRankContextTest()
    {
        FeedRankSyndicationExtension target = CreateExtension1();
        FeedRankSyndicationExtensionContext expected = CreateContext1();
        FeedRankSyndicationExtensionContext actual = target.Context;
        actual.ShouldBe(expected);
    }

    private FeedRankSyndicationExtension CreateExtension1()
    {
        FeedRankSyndicationExtension re = new FeedRankSyndicationExtension
        {
            Context =
            {
                Domain = new Uri("http://example.com"),
                Label = "Title",
                Scheme = new Uri("http://example.com/scheme.txt"),
                Value = 1.0m
            }
        };
        return re;
    }

    private FeedRankSyndicationExtension CreateExtension2()
    {
        FeedRankSyndicationExtension re = new FeedRankSyndicationExtension
        {
            Context =
            {
                Domain = new Uri("http://example.net"),
                Label = "label",
                Scheme = new Uri("http://example.net/scheme.html"),
                Value = 2.0m
            }
        };
        return re;
    }

    public static FeedRankSyndicationExtensionContext CreateContext1()
    {
        FeedRankSyndicationExtensionContext re = new FeedRankSyndicationExtensionContext
        {
            Domain = new Uri(""),
            Label = "",
            Scheme = new Uri(""),
            Value = 1.0m
        };
        return re;
    }
}
