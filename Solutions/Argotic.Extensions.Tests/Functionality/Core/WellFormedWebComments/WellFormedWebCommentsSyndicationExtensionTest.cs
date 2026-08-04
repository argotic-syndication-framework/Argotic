using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.WellFormedWebComments;

[TestClass]
public class WellFormedWebCommentsSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:wfw=""http://wellformedweb.org/CommentAPI/""";

    private readonly string toStringText = "<comment xmlns=\"http://wellformedweb.org/CommentAPI/\">http://www.example.com/comments/post/1</comment>" + Environment.NewLine +
                                           "<commentRss xmlns=\"http://wellformedweb.org/CommentAPI/\">http://www.example.com/comments/feed/1</commentRss>";

    private const string StrExtXml = "<wfw:comment>http://www.example.com/comments/post/1</wfw:comment>"
                                     + "<wfw:commentRss>http://www.example.com/comments/feed/1</wfw:commentRss>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void WellFormedWebCommentsSyndicationExtensionConstructorTest()
    {
        WellFormedWebCommentsSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<WellFormedWebCommentsSyndicationExtension>();
    }

    [TestMethod]
    public void WellFormedWebCommentsCompareToTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void WellFormedWebCommentsEqualsTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void WellFormedWebCommentsGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void WellFormedWebCommentsLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void WellFormedWebCommentsCreateXmlTest()
    {
        WellFormedWebCommentsSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void WellFormedWebCommentsFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        WellFormedWebCommentsSyndicationExtension? itemExtension = item.FindExtension<WellFormedWebCommentsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(WellFormedWebCommentsSyndicationExtension.MatchByType) as WellFormedWebCommentsSyndicationExtension)
            .ShouldBeOfType<WellFormedWebCommentsSyndicationExtension>();
    }

    [TestMethod]
    public void WellFormedWebCommentsMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = WellFormedWebCommentsSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void WellFormedWebCommentsToStringTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    [TestMethod]
    public void WellFormedWebCommentsWriteToTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    [TestMethod]
    public void WellFormedWebCommentsOpEqualityTestFailure()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void WellFormedWebCommentsOpEqualityTestSuccess()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void WellFormedWebCommentsOpGreaterThanTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void WellFormedWebCommentsOpInequalityTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void WellFormedWebCommentsOpLessThanTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void WellFormedWebCommentsContextTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        WellFormedWebCommentsSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Comments.ShouldBe(new Uri("http://www.example.com/comments/post/1"));
        context.CommentsFeed.ShouldBe(new Uri("http://www.example.com/comments/feed/1"));
    }

    private static WellFormedWebCommentsSyndicationExtension CreateExtension1()
    {
        WellFormedWebCommentsSyndicationExtension ext = new()
        {
            Context =
            {
                Comments = new Uri("http://www.example.com/comments/post/1"),
                CommentsFeed = new Uri("http://www.example.com/comments/feed/1")
            }
        };

        return ext;
    }

    private static WellFormedWebCommentsSyndicationExtension CreateExtension2()
    {
        WellFormedWebCommentsSyndicationExtension ext = new()
        {
            Context =
            {
                Comments = new Uri("http://www.example.com/comments/post/2"),
                CommentsFeed = new Uri("http://www.example.com/comments/feed/2")
            }
        };

        return ext;
    }
}