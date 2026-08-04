using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.Trackback;

[TestClass]
public class TrackbackSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:trackback=""http://madskills.com/public/xml/rss/module/trackback/""";

    private readonly string toStringText = "<ping xmlns=\"http://madskills.com/public/xml/rss/module/trackback/\">http://www.example.com/trackback/1</ping>";

    private const string StrExtXml = "<trackback:ping>http://www.example.com/trackback/1</trackback:ping>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void TrackbackSyndicationExtensionConstructorTest()
    {
        TrackbackSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<TrackbackSyndicationExtension>();
    }

    [TestMethod]
    public void TrackbackCompareToTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        TrackbackSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void TrackbackEqualsTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void TrackbackGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        TrackbackSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void TrackbackLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void TrackbackCreateXmlTest()
    {
        TrackbackSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void TrackbackFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        TrackbackSyndicationExtension? itemExtension = item.FindExtension<TrackbackSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(TrackbackSyndicationExtension.MatchByType) as TrackbackSyndicationExtension)
            .ShouldBeOfType<TrackbackSyndicationExtension>();
    }

    [TestMethod]
    public void TrackbackMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = TrackbackSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void TrackbackToStringTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    [TestMethod]
    public void TrackbackWriteToTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void TrackbackOpEqualityTestFailure()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void TrackbackOpEqualityTestSuccess()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void TrackbackOpGreaterThanTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void TrackbackOpInequalityTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void TrackbackOpLessThanTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void TrackbackContextTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        TrackbackSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Ping.ShouldBe(new Uri("http://www.example.com/trackback/1"));
    }

    private static TrackbackSyndicationExtension CreateExtension1()
    {
        TrackbackSyndicationExtension ext = new()
        {
            Context =
            {
                Ping = new Uri("http://www.example.com/trackback/1")
            }
        };

        return ext;
    }

    private static TrackbackSyndicationExtension CreateExtension2()
    {
        TrackbackSyndicationExtension ext = new()
        {
            Context =
            {
                Ping = new Uri("http://www.example.com/trackback/2")
            }
        };

        return ext;
    }
}