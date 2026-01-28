using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Pheed;

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
        PheedSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<PheedSyndicationExtension>();
    }

    [TestMethod]
    public void PheedCompareToTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        PheedSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
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
    public void PheedGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        PheedSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        PheedSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    [TestMethod]
    public void PheedCreateXmlTest()
    {
        PheedSyndicationExtension pheed = new()
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
        RssFeed feed = new();
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
        using StringWriter sw = new();
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
    public void PheedContextTest()
    {
        PheedSyndicationExtension target = CreateExtension1();
        PheedSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Source.ShouldBe(new Uri("http://www.example.com"));
        context.Thumbnail.ShouldBe(new Uri("http://www.example.com/thumbnail.jpg"));
    }

    private static PheedSyndicationExtension CreateExtension1()
    {
        PheedSyndicationExtension nyc = new()
        {
            Context =
            {
                Source = new Uri("http://www.example.com"),
                Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
            }
        };

        return nyc;
    }

    private static PheedSyndicationExtension CreateExtension2()
    {
        PheedSyndicationExtension nyc = new()
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
        PheedSyndicationExtensionContext nyc = new()
        {
            Source = new Uri("http://www.example.com"),
            Thumbnail = new Uri("http://www.example.com/thumbnail.jpg")
        };

        return nyc;
    }
}
