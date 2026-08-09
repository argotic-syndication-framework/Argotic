using static Argotic.Common.ComparisonOperatorExtensions;
namespace Argotic.Extensions.Tests.Functionality.Core.WellFormedWebComments;

/// <summary>
/// Covers <c>WellFormedWebCommentsSyndicationExtension</c>, the wfw Comment API module that points an
/// item at the endpoint comments are posted to and at the feed of comments already made.
/// </summary>
[TestClass]
public class WellFormedWebCommentsSyndicationExtensionTest
{
    private const string Namespc = """
                                   xmlns:wfw="http://wellformedweb.org/CommentAPI/"
                                   """;

    private readonly string toStringText = """<comment xmlns="http://wellformedweb.org/CommentAPI/">http://www.example.com/comments/post/1</comment>""" + Environment.NewLine +
                                           """<commentRss xmlns="http://wellformedweb.org/CommentAPI/">http://www.example.com/comments/feed/1</commentRss>""";

    private const string StrExtXml = "<wfw:comment>http://www.example.com/comments/post/1</wfw:comment>"
                                     + "<wfw:commentRss>http://www.example.com/comments/feed/1</wfw:commentRss>";

    public TestContext? TestContext { get; set; }
    /// <summary>
    /// Two extensions built from the same comment and comment-feed URLs compare equal, so
    /// <c>CompareTo</c> returns <c>0</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsCompareToTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same two URLs are equal through the <c>object</c>
    /// overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsEqualsTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Two extensions built from the same comment and comment-feed URLs are equal and hash equally, and
    /// hashing one twice gives the same answer.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>hash.ShouldNotBe(0)</c>, which says nothing about any
    ///     implementation: <see cref="HashCode.Combine{T}(T)"/> is seeded per process, so the value is
    ///     unpredictable and only 1 in 2^32 runs would have seen it land on <c>0</c> anyway.
    /// </remarks>
    [TestMethod]
    public void WellFormedWebCommentsGetHashCodeTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension1();

        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// A feed carrying <c>wfw:comment</c> and <c>wfw:commentRss</c> yields an extension holding both
    /// URLs the document declared.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        WellFormedWebCommentsSyndicationExtension extension = item.FindExtension<WellFormedWebCommentsSyndicationExtension>().ShouldNotBeNull();
        extension.Context.Comments.ShouldBe(new Uri("http://www.example.com/comments/post/1"));
        extension.Context.CommentsFeed.ShouldBe(new Uri("http://www.example.com/comments/feed/1"));
    }

    /// <summary>
    /// Saving a feed with the extension attached writes <c>wfw:comment</c> before
    /// <c>wfw:commentRss</c>, with the namespace declared on the <c>rss</c> element.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsCreateXmlTest()
    {
        WellFormedWebCommentsSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the comment endpoint and the comment feed the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as WellFormedWebCommentsSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected no parsed value.
    /// </remarks>
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
        WellFormedWebCommentsSyndicationExtension byType = item.FindExtension<WellFormedWebCommentsSyndicationExtension>().ShouldNotBeNull();
        WellFormedWebCommentsSyndicationExtension byPredicate = item
            .FindExtension(WellFormedWebCommentsSyndicationExtension.MatchByType)
            .ShouldBeOfType<WellFormedWebCommentsSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Comments.ShouldBe(new Uri("http://www.example.com/comments/post/1"));
        byPredicate.Context.CommentsFeed.ShouldBe(new Uri("http://www.example.com/comments/feed/1"));
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>WellFormedWebCommentsSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = WellFormedWebCommentsSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders <c>comment</c> and then <c>commentRss</c> on separate lines, each
    /// declaring the Comment API namespace as its default rather than carrying the <c>wfw</c> prefix.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsToStringTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same two elements as
    /// <c>ToString</c>, without the line break between them.
    /// </summary>
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

    /// <summary>
    /// Two extensions pointing at different comment URLs are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsOpEqualityTestFailure()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions pointing at the same comment URLs are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsOpEqualityTestSuccess()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension whose comment endpoint is <c>post/1</c> does not sort above the one whose endpoint
    /// is <c>post/2</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsOpGreaterThanTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions pointing at different comment URLs are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsOpInequalityTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension whose comment endpoint is <c>post/1</c> sorts below the one whose endpoint is
    /// <c>post/2</c>.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsOpLessThanTest()
    {
        WellFormedWebCommentsSyndicationExtension first = CreateExtension1();
        WellFormedWebCommentsSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the comment endpoint and comment feed the extension was
    /// built from.
    /// </summary>
    [TestMethod]
    public void WellFormedWebCommentsContextTest()
    {
        WellFormedWebCommentsSyndicationExtension target = CreateExtension1();
        WellFormedWebCommentsSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Comments.ShouldBe(new Uri("http://www.example.com/comments/post/1"));
        context.CommentsFeed.ShouldBe(new Uri("http://www.example.com/comments/feed/1"));
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser, commenting on post 1.
    /// </summary>
    /// <returns>An extension carrying a comment endpoint and a comment feed.</returns>
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

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, commenting on post 2.
    /// </summary>
    /// <returns>An extension carrying a comment endpoint and a comment feed.</returns>
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