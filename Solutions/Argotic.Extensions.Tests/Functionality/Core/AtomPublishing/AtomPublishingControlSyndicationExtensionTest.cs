using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.AtomPublishing;

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
        AtomPublishingControlSyndicationExtension target = new();
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
    public void AtomPublishingControlGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        AtomPublishingControlSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    [TestMethod]
    public void AtomPublishingControlLoadTest()
    {
        // Verify that an RSS feed containing APP extension XML can be loaded without errors
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Basic feed structure should be intact
        feed.Channel.ShouldNotBeNull();
        feed.Channel.Items.Count().ShouldBe(1);
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
        using StringWriter sw = new();
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
    public void AtomPublishingControlContextTest()
    {
        AtomPublishingControlSyndicationExtension target = CreateExtension1();
        AtomPublishingControlSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.BaseUri.ShouldBe(new Uri("http://www.example.com/control.html"));
        context.IsDraft.ShouldBeTrue();
        context.Language.Name.ShouldBe("en-US");
    }

    private static AtomPublishingControlSyndicationExtension CreateExtension1()
    {
        AtomPublishingControlSyndicationExtension nyc = new()
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

    private static AtomPublishingControlSyndicationExtension CreateExtension2()
    {
        AtomPublishingControlSyndicationExtension nyc = new()
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