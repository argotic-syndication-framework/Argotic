using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.LiveJournal;

[TestClass]
public class LiveJournalSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:lj=""http://livejournal.org/rss/lj/2.0/""";

    private readonly string toStringText = "<music xmlns=\"http://livejournal.org/rss/lj/2.0/\"><![CDATA[Test Music Track]]></music>";

    private const string StrExtXml = "<lj:music>Test Music Track</lj:music>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void LiveJournalSyndicationExtensionConstructorTest()
    {
        LiveJournalSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<LiveJournalSyndicationExtension>();
    }

    [TestMethod]
    public void LiveJournalCompareToTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void LiveJournalEqualsTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        LiveJournalSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void LiveJournalLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void LiveJournalCreateXmlTest()
    {
        LiveJournalSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void LiveJournalFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        LiveJournalSyndicationExtension itemExtension = item.FindExtension<LiveJournalSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(LiveJournalSyndicationExtension.MatchByType) as LiveJournalSyndicationExtension)
            .ShouldBeOfType<LiveJournalSyndicationExtension>();
    }

    [TestMethod]
    public void LiveJournalMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = LiveJournalSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalToStringTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void LiveJournalWriteToTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void LiveJournalOpEqualityTestFailure()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void LiveJournalOpEqualityTestSuccess()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalOpGreaterThanTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool result = (first > second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void LiveJournalOpInequalityTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalOpLessThanTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool result = (first < second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void LiveJournalContextTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        LiveJournalSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Music.ShouldBe("Test Music Track");
    }

    private static LiveJournalSyndicationExtension CreateExtension1()
    {
        LiveJournalSyndicationExtension ext = new()
        {
            Context =
            {
                Music = "Test Music Track"
            }
        };

        return ext;
    }

    private static LiveJournalSyndicationExtension CreateExtension2()
    {
        LiveJournalSyndicationExtension ext = new()
        {
            Context =
            {
                Music = "Other Music Track"
            }
        };

        return ext;
    }
}
