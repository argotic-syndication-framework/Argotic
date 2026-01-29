using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.BlogChannel;

[TestClass]
public class BlogChannelSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:blogChannel=""http://backend.userland.com/blogChannelModule""";

    private readonly string toStringText = "<blogRoll xmlns=\"http://backend.userland.com/blogChannelModule\">http://www.example.com/blogroll.opml</blogRoll>" + Environment.NewLine +
                                           "<mySubscriptions xmlns=\"http://backend.userland.com/blogChannelModule\">http://www.example.com/subscriptions.opml</mySubscriptions>" + Environment.NewLine +
                                           "<blink xmlns=\"http://backend.userland.com/blogChannelModule\">http://www.example.com/promoted</blink>" + Environment.NewLine +
                                           "<changes xmlns=\"http://backend.userland.com/blogChannelModule\">http://www.example.com/changes.xml</changes>";

    private const string StrExtXml = "<blogChannel:blogRoll>http://www.example.com/blogroll.opml</blogChannel:blogRoll>"
                                     + "<blogChannel:mySubscriptions>http://www.example.com/subscriptions.opml</blogChannel:mySubscriptions>"
                                     + "<blogChannel:blink>http://www.example.com/promoted</blogChannel:blink>"
                                     + "<blogChannel:changes>http://www.example.com/changes.xml</blogChannel:changes>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void BlogChannelSyndicationExtensionConstructorTest()
    {
        BlogChannelSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<BlogChannelSyndicationExtension>();
    }

    [TestMethod]
    public void BlogChannelCompareToTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        BlogChannelSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void BlogChannelEqualsTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BlogChannelGetHashCodeTest()
    {
        // Verify GetHashCode doesn't throw
        BlogChannelSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void BlogChannelLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void BlogChannelCreateXmlTest()
    {
        BlogChannelSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void BlogChannelFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        BlogChannelSyndicationExtension itemExtension = item.FindExtension<BlogChannelSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(BlogChannelSyndicationExtension.MatchByType) as BlogChannelSyndicationExtension)
            .ShouldBeOfType<BlogChannelSyndicationExtension>();
    }

    [TestMethod]
    public void BlogChannelMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = BlogChannelSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BlogChannelToStringTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    [TestMethod]
    public void BlogChannelWriteToTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void BlogChannelOpEqualityTestFailure()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void BlogChannelOpEqualityTestSuccess()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BlogChannelOpGreaterThanTest()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void BlogChannelOpInequalityTest()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BlogChannelOpLessThanTest()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void BlogChannelContextTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        BlogChannelSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.BlogRoll.ShouldBe(new Uri("http://www.example.com/blogroll.opml"));
        context.MySubscriptions.ShouldBe(new Uri("http://www.example.com/subscriptions.opml"));
        context.Blink.ShouldBe(new Uri("http://www.example.com/promoted"));
        context.Changes.ShouldBe(new Uri("http://www.example.com/changes.xml"));
    }

    private static BlogChannelSyndicationExtension CreateExtension1()
    {
        BlogChannelSyndicationExtension ext = new()
        {
            Context =
            {
                BlogRoll = new Uri("http://www.example.com/blogroll.opml"),
                MySubscriptions = new Uri("http://www.example.com/subscriptions.opml"),
                Blink = new Uri("http://www.example.com/promoted"),
                Changes = new Uri("http://www.example.com/changes.xml")
            }
        };

        return ext;
    }

    private static BlogChannelSyndicationExtension CreateExtension2()
    {
        BlogChannelSyndicationExtension ext = new()
        {
            Context =
            {
                BlogRoll = new Uri("http://www.example.com/other-blogroll.opml"),
                MySubscriptions = new Uri("http://www.example.com/other-subscriptions.opml"),
                Blink = new Uri("http://www.example.com/other-promoted"),
                Changes = new Uri("http://www.example.com/other-changes.xml")
            }
        };

        return ext;
    }
}