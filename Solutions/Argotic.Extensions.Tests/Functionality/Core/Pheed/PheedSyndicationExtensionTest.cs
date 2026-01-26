namespace Argotic.Extensions.Tests;

using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class PheedSyndicationExtensionTest
{
    const string namespc = @"xmlns:photo=""http://www.pheed.com/pheed/""";

    private readonly string nycText = "<thumbnail xmlns=\"http://www.pheed.com/pheed/\">http://www.example.com/thumbnail.jpg</thumbnail>" + Environment.NewLine
                                                                                                                                          + "<imgsrc xmlns=\"http://www.pheed.com/pheed/\">http://www.example.com/</imgsrc>";

    private const string strExtXml = "<photo:thumbnail>http://www.example.com/thumbnail.jpg</photo:thumbnail><photo:imgsrc>http://www.example.com/</photo:imgsrc>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void PheedSyndicationExtensionConstructorTest()
    {
        PheedSyndicationExtension target = new PheedSyndicationExtension();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<PheedSyndicationExtension>();
    }

    [TestMethod]
    public void PheedCompareToTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void PheedEqualsTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("GetHashCode implementation is not deterministic across runs")]
    public void PheedGetHashCodeTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        int expected = -1671096665;
        int actual = target.GetHashCode();
        actual.ShouldBe(expected);
    }

    [TestMethod]
    [Ignore("Test requires manual verification of Load behavior")]
    public void PheedLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);
    }

    [TestMethod]
    public void PheedCreateXmlTest()
    {
        PheedSyndicationExtension pheed = new PheedSyndicationExtension
        {
            Context =
            {
                Source = new Uri("http://www.example.com"),
                Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
            }
        };

        string actual = ExtensionTestUtil.AddExtensionToXml(pheed);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void PheedFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new RssFeed();
        feed.Load(reader);
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        PheedSyndicationExtension itemExtension = item.FindExtension<PheedSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(PheedSyndicationExtension.MatchByType) as PheedSyndicationExtension)
            .ShouldBeOfType<PheedSyndicationExtension>();
    }

    [TestMethod]
    public void PheedMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = PheedSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PheedToStringTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    [TestMethod]
    public void PheedWriteToTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new StringWriter();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(nycText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void PheedOpEqualityTestFailure()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void PheedOpEqualityTestSuccess()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PheedOpGreaterThanTest()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void PheedOpInequalityTest()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void PheedOpLessThanTest()
    {
        PheedSyndicationExtension first = CreateExtension1();
        PheedSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    [Ignore("Context equality comparison not implemented")]
    public void PheedContextTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        PheedSyndicationExtensionContext expected = CreateContext1();
        PheedSyndicationExtensionContext actual = target.Context;
        actual.ShouldBe(expected);
    }

    private PheedSyndicationExtension CreateExtension1()
    {
        PheedSyndicationExtension nyc = new PheedSyndicationExtension
        {
            Context =
            {
                Source = new Uri("http://www.example.com"),
                Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
            }
        };

        return nyc;
    }

    private PheedSyndicationExtension CreateExtension2()
    {
        PheedSyndicationExtension nyc = new PheedSyndicationExtension
        {
            Context =
            {
                Source = new Uri("http://www.example.net"),
                Thumbnail = new Uri("http://www.example.net/thumbnail.png")
            }
        };

        return nyc;
    }

    public static PheedSyndicationExtensionContext CreateContext1()
    {
        PheedSyndicationExtensionContext nyc = new PheedSyndicationExtensionContext
        {
            Source = new Uri("http://www.example.com"),
            Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
        };

        return nyc;
    }
}
