using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.CreativeCommons;

[TestClass]
public class CreativeCommonsSyndicationExtensionTest
{
    const string namespc = @"xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule""";

    private const string nycText = "<license xmlns=\"http://backend.userland.com/creativeCommonsRssModule\">http://www.example.com/license1.html</license>" +
                                    "<license xmlns=\"http://backend.userland.com/creativeCommonsRssModule\">http://www.example.com/license2.html</license>";

    private const string strExtXml = "<creativeCommons:license>http://www.example.com/license1.html</creativeCommons:license>"
                                     + "<creativeCommons:license>http://www.example.com/license2.html</creativeCommons:license>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void CreativeCommonsSyndicationExtensionConstructorTest()
    {
        CreativeCommonsSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<CreativeCommonsSyndicationExtension>();
    }

    [TestMethod]
    public void CreativeCommonsCompareToTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        CreativeCommonsSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void CreativeCommonsEqualsTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void CreativeCommonsGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        CreativeCommonsSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    [TestMethod]
    public void CreativeCommonsCreateXmlTest()
    {
        CreativeCommonsSyndicationExtension itunes = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(itunes);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void CreativeCommonsFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        CreativeCommonsSyndicationExtension itemExtension = item.FindExtension<CreativeCommonsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(CreativeCommonsSyndicationExtension.MatchByType) as CreativeCommonsSyndicationExtension)
            .ShouldBeOfType<CreativeCommonsSyndicationExtension>();
    }

    [TestMethod]
    public void CreativeCommonsMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = CreativeCommonsSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void CreativeCommonsToStringTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.Replace(Environment.NewLine, "").ShouldBe(nycText);
    }

    [TestMethod]
    public void CreativeCommonsWriteToTest()
    {
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(nycText.Replace(Environment.NewLine + "  ", "").Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void CreativeCommonsOpEqualityTestFailure()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void CreativeCommonsOpEqualityTestSuccess()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void CreativeCommonsOpGreaterThanTest()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void CreativeCommonsOpInequalityTest()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void CreativeCommonsOpLessThanTest()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void CreativeCommonsContextTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        CreativeCommonsSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Licenses.Count.ShouldBe(2);
        context.Licenses[0].ShouldBe(new Uri("http://www.example.com/license1.html"));
        context.Licenses[1].ShouldBe(new Uri("http://www.example.com/license2.html"));
    }

    private static CreativeCommonsSyndicationExtension CreateExtension1()
    {
        CreativeCommonsSyndicationExtension nyc = new();

        nyc.Context.Licenses.Add(new Uri("http://www.example.com/license1.html"));
        nyc.Context.Licenses.Add(new Uri("http://www.example.com/license2.html"));
        return nyc;
    }

    private static CreativeCommonsSyndicationExtension CreateExtension2()
    {
        CreativeCommonsSyndicationExtension nyc = new();
        nyc.Context.Licenses.Add(new Uri("http://www.example.net/license1.html"));
        nyc.Context.Licenses.Add(new Uri("http://www.example.net/license2.html"));
        return nyc;
    }

    public static CreativeCommonsSyndicationExtensionContext CreateContext1()
    {
        CreativeCommonsSyndicationExtensionContext nyc = new();
        return nyc;
    }
}
