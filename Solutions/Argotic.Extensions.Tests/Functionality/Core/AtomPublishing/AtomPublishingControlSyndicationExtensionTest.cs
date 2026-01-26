namespace Argotic.Extensions.Tests;

using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class AtomPublishingControlSyndicationExtensionTest
{
    private readonly string namespc = @"xmlns:app=""http://www.w3.org/2007/app""";
    private readonly string nycText = $@"<control xml:base=""http://www.example.com/control.html"" xml:lang=""en-US"" xmlns=""http://www.w3.org/2007/app"">{Environment.NewLine}  <draft>yes</draft>{Environment.NewLine}</control>";
    private readonly string strExtXml = @"<app:control xml:base=""http://www.example.com/control.html"" xml:lang=""en-US""><app:draft>yes</app:draft></app:control>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void AtomPublishingControlSyndicationExtensionConstructorTest()
    {
        AtomPublishingControlSyndicationExtension target = new AtomPublishingControlSyndicationExtension();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<AtomPublishingControlSyndicationExtension>();
    }

    [TestMethod]
    public void AtomPublishingControlCompareToTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void AtomPublishingControlEqualsTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("GetHashCode implementation is not deterministic across runs")]
    public void AtomPublishingControlGetHashCodeTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        int expected = -1862124151;
        int actual = target.GetHashCode();
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void AtomPublishingControlLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);
    }

    [TestMethod]
    public void AtomPublishingControlCreateXmlTest()
    {
        AtomPublishingControlSyndicationExtension itunes = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(itunes).Trim();
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml).Trim();
        actual.ShouldBe(expected);
    }

    [TestMethod]
    [Ignore("Extension not being parsed from RSS feed correctly")]
    public void AtomPublishingControlFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        AtomPublishingControlSyndicationExtension itemExtension = item.FindExtension<AtomPublishingControlSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) as AtomPublishingControlSyndicationExtension)
            .ShouldBeOfType<AtomPublishingControlSyndicationExtension>();
    }

    [TestMethod]
    public void AtomPublishingControlMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = AtomPublishingControlSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void AtomPublishingControlToStringTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    [TestMethod]
    public void AtomPublishingControlWriteToTest()
    {
        using StringWriter sw = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(nycText.Replace(Environment.NewLine + "  ", "").Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void AtomPublishingControlOpEqualityTestFailure()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void AtomPublishingControlOpEqualityTestSuccess()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void AtomPublishingControlOpGreaterThanTest()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void AtomPublishingControlOpInequalityTest()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void AtomPublishingControlOpLessThanTest()
    {
        AtomPublishingControlSyndicationExtension first = CreateExtension1();
        AtomPublishingControlSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("Context equality comparison not implemented")]
    public void AtomPublishingControlContextTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        AtomPublishingControlSyndicationExtensionContext expected = CreateContext1();
        AtomPublishingControlSyndicationExtensionContext actual = target.Context;
        actual.ShouldBe(expected);
    }

    private AtomPublishingControlSyndicationExtension CreateExtension1()
    {
        AtomPublishingControlSyndicationExtension nyc = new AtomPublishingControlSyndicationExtension
        {
            Context =
            {
                BaseUri = new Uri("http://www.example.com/control.html"),
                IsDraft = true,
                Language = new CultureInfo("en-US")
            }
        };

        return nyc;
    }

    private AtomPublishingControlSyndicationExtension CreateExtension2()
    {
        AtomPublishingControlSyndicationExtension nyc = new AtomPublishingControlSyndicationExtension
        {
            Context =
            {
                BaseUri = new Uri("http://www.example.net/control.html"),
                IsDraft = false,
                Language = new CultureInfo("fr-CA")
            }
        };

        return nyc;
    }

    public static AtomPublishingControlSyndicationExtensionContext CreateContext1()
    {
        return new AtomPublishingControlSyndicationExtensionContext();
    }
}
