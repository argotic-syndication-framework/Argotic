using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.Pingback;

[TestClass]
public class PingbackSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:pingback=""http://madskills.com/public/xml/rss/module/pingback/""";

    private readonly string toStringText = "<server xmlns=\"http://madskills.com/public/xml/rss/module/pingback/\">http://www.example.com/xmlrpc.php</server>" + Environment.NewLine +
                                           "<target xmlns=\"http://madskills.com/public/xml/rss/module/pingback/\">http://www.example.com/post/1</target>";

    private const string StrExtXml = "<pingback:server>http://www.example.com/xmlrpc.php</pingback:server>"
                                     + "<pingback:target>http://www.example.com/post/1</pingback:target>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void PingbackSyndicationExtensionConstructorTest()
    {
        PingbackSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<PingbackSyndicationExtension>();
    }

    [TestMethod]
    public void PingbackCompareToTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        PingbackSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void PingbackEqualsTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PingbackGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        PingbackSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void PingbackLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void PingbackCreateXmlTest()
    {
        PingbackSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void PingbackFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        PingbackSyndicationExtension itemExtension = item.FindExtension<PingbackSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(PingbackSyndicationExtension.MatchByType) as PingbackSyndicationExtension)
            .ShouldBeOfType<PingbackSyndicationExtension>();
    }

    [TestMethod]
    public void PingbackMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = PingbackSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PingbackToStringTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    [TestMethod]
    public void PingbackWriteToTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void PingbackOpEqualityTestFailure()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void PingbackOpEqualityTestSuccess()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PingbackOpGreaterThanTest()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void PingbackOpInequalityTest()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PingbackOpLessThanTest()
    {
        PingbackSyndicationExtension first = CreateExtension1();
        PingbackSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PingbackContextTest()
    {
        PingbackSyndicationExtension target = CreateExtension1();
        PingbackSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Server.ShouldBe(new Uri("http://www.example.com/xmlrpc.php"));
        context.Target.ShouldBe(new Uri("http://www.example.com/post/1"));
    }

    private static PingbackSyndicationExtension CreateExtension1()
    {
        PingbackSyndicationExtension ext = new()
        {
            Context =
            {
                Server = new Uri("http://www.example.com/xmlrpc.php"),
                Target = new Uri("http://www.example.com/post/1")
            }
        };

        return ext;
    }

    private static PingbackSyndicationExtension CreateExtension2()
    {
        PingbackSyndicationExtension ext = new()
        {
            Context =
            {
                Server = new Uri("http://www.example.com/other-xmlrpc.php"),
                Target = new Uri("http://www.example.com/post/2")
            }
        };

        return ext;
    }
}