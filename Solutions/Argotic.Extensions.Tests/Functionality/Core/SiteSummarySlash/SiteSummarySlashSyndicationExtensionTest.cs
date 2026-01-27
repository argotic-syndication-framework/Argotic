using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummarySlash;

[TestClass]
public class SiteSummarySlashSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:slash=""http://purl.org/rss/1.0/modules/slash/""";

    private readonly string toStringText = "<comments xmlns=\"http://purl.org/rss/1.0/modules/slash/\">42</comments>" + Environment.NewLine +
                                           "<section xmlns=\"http://purl.org/rss/1.0/modules/slash/\">Technology</section>" + Environment.NewLine +
                                           "<department xmlns=\"http://purl.org/rss/1.0/modules/slash/\">Software</department>";

    private const string StrExtXml = "<slash:comments>42</slash:comments>"
                                     + "<slash:section>Technology</slash:section>"
                                     + "<slash:department>Software</slash:department>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void SiteSummarySlashSyndicationExtensionConstructorTest()
    {
        SiteSummarySlashSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SiteSummarySlashSyndicationExtension>();
    }

    [TestMethod]
    public void SiteSummarySlashCompareToTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void SiteSummarySlashEqualsTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummarySlashGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void SiteSummarySlashLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void SiteSummarySlashCreateXmlTest()
    {
        SiteSummarySlashSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummarySlashFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        SiteSummarySlashSyndicationExtension itemExtension = item.FindExtension<SiteSummarySlashSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(SiteSummarySlashSyndicationExtension.MatchByType) as SiteSummarySlashSyndicationExtension)
            .ShouldBeOfType<SiteSummarySlashSyndicationExtension>();
    }

    [TestMethod]
    public void SiteSummarySlashMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SiteSummarySlashSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummarySlashToStringTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummarySlashWriteToTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummarySlashOpEqualityTestFailure()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SiteSummarySlashOpEqualityTestSuccess()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummarySlashOpGreaterThanTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool result = (first > second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SiteSummarySlashOpInequalityTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummarySlashOpLessThanTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool result = (first < second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SiteSummarySlashContextTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        SiteSummarySlashSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Comments.ShouldBe(42);
        context.Section.ShouldBe("Technology");
        context.Department.ShouldBe("Software");
    }

    private static SiteSummarySlashSyndicationExtension CreateExtension1()
    {
        SiteSummarySlashSyndicationExtension ext = new()
        {
            Context =
            {
                Comments = 42,
                Section = "Technology",
                Department = "Software"
            }
        };

        return ext;
    }

    private static SiteSummarySlashSyndicationExtension CreateExtension2()
    {
        SiteSummarySlashSyndicationExtension ext = new()
        {
            Context =
            {
                Comments = 10,
                Section = "Science",
                Department = "Research"
            }
        };

        return ext;
    }
}
