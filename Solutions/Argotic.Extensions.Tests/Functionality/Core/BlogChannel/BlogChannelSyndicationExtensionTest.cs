using static Argotic.Common.ComparisonOperatorExtensions;
namespace Argotic.Extensions.Tests.Functionality.Core.BlogChannel;

/// <summary>
/// Covers <c>BlogChannelSyndicationExtension</c>, the blogChannel module that points a channel at its
/// blogroll, its subscriptions, a promoted link and a changes feed.
/// </summary>
[TestClass]
public class BlogChannelSyndicationExtensionTest
{
    private const string Namespc = """
        xmlns:blogChannel="http://backend.userland.com/blogChannelModule"
        """;

    private readonly string toStringText = """
        <blogRoll xmlns="http://backend.userland.com/blogChannelModule">http://www.example.com/blogroll.opml</blogRoll>
        <mySubscriptions xmlns="http://backend.userland.com/blogChannelModule">http://www.example.com/subscriptions.opml</mySubscriptions>
        <blink xmlns="http://backend.userland.com/blogChannelModule">http://www.example.com/promoted</blink>
        <changes xmlns="http://backend.userland.com/blogChannelModule">http://www.example.com/changes.xml</changes>
        """.ReplaceLineEndings();

    private const string StrExtXml = "<blogChannel:blogRoll>http://www.example.com/blogroll.opml</blogChannel:blogRoll>"
                                     + "<blogChannel:mySubscriptions>http://www.example.com/subscriptions.opml</blogChannel:mySubscriptions>"
                                     + "<blogChannel:blink>http://www.example.com/promoted</blogChannel:blink>"
                                     + "<blogChannel:changes>http://www.example.com/changes.xml</blogChannel:changes>";

    public TestContext? TestContext { get; set; }
    /// <summary>
    /// Two extensions built from the same four URIs compare equal, so <c>CompareTo</c> returns <c>0</c>.
    /// </summary>
    [TestMethod]
    public void BlogChannelCompareToTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        BlogChannelSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same four URIs are equal through the <c>object</c>
    /// overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void BlogChannelEqualsTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Two extensions built from the same four URIs are equal and hash equally, and hashing one twice
    /// gives the same answer.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>hash.ShouldNotBe(0)</c>, which says nothing about any
    ///     implementation: <see cref="HashCode.Combine{T}(T)"/> is seeded per process, so the value is
    ///     unpredictable and only 1 in 2^32 runs would have seen it land on <c>0</c> anyway.
    /// </remarks>
    [TestMethod]
    public void BlogChannelGetHashCodeTest()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension1();

        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// A feed carrying the four blogChannel elements yields an extension holding the four URIs the
    /// document declared.
    /// </summary>
    [TestMethod]
    public void BlogChannelLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        BlogChannelSyndicationExtension extension = item.FindExtension<BlogChannelSyndicationExtension>().ShouldNotBeNull();
        extension.Context.BlogRoll.ShouldBe(new Uri("http://www.example.com/blogroll.opml"));
        extension.Context.MySubscriptions.ShouldBe(new Uri("http://www.example.com/subscriptions.opml"));
        extension.Context.Blink.ShouldBe(new Uri("http://www.example.com/promoted"));
        extension.Context.Changes.ShouldBe(new Uri("http://www.example.com/changes.xml"));
    }

    /// <summary>
    /// Saving a feed with the extension attached writes <c>blogRoll</c>, <c>mySubscriptions</c>,
    /// <c>blink</c> and <c>changes</c> in that order under the <c>blogChannel</c> prefix.
    /// </summary>
    [TestMethod]
    public void BlogChannelCreateXmlTest()
    {
        BlogChannelSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the four URIs the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as BlogChannelSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected no parsed value.
    /// </remarks>
    [TestMethod]
    public void BlogChannelFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        BlogChannelSyndicationExtension byType = item.FindExtension<BlogChannelSyndicationExtension>().ShouldNotBeNull();
        BlogChannelSyndicationExtension byPredicate = item
            .FindExtension(BlogChannelSyndicationExtension.MatchByType)
            .ShouldBeOfType<BlogChannelSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Blink.ShouldBe(new Uri("http://www.example.com/promoted"));
        byPredicate.Context.Changes.ShouldBe(new Uri("http://www.example.com/changes.xml"));
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>BlogChannelSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void BlogChannelMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = BlogChannelSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders the four elements one per line, each declaring the blogChannel namespace
    /// as its default rather than carrying the prefix.
    /// </summary>
    [TestMethod]
    public void BlogChannelToStringTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same four elements as
    /// <c>ToString</c>, without the line breaks between them.
    /// </summary>
    [TestMethod]
    public void BlogChannelWriteToTest()
    {
        BlogChannelSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions pointing at different URIs are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void BlogChannelOpEqualityTestFailure()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions pointing at the same four URIs are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void BlogChannelOpEqualityTestSuccess()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension whose <c>blink</c> is <c>promoted</c> sorts above the one whose <c>blink</c> is
    /// <c>other-promoted</c>, and the reverse comparison agrees.
    /// </summary>
    [TestMethod]
    public void BlogChannelOpGreaterThanTest()
    {
        // Ordering is decided by the first member that differs: extension 1's blink URI ("promoted")
        // sorts after extension 2's ("other-promoted"), so extension 1 is the greater of the two.
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        (first > second).ShouldBeTrue();
        (second > first).ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions pointing at different URIs are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void BlogChannelOpInequalityTest()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&lt;</c> agrees with <c>&gt;</c>: the <c>other-promoted</c> extension is the lesser of the
    /// two, in both directions.
    /// </summary>
    [TestMethod]
    public void BlogChannelOpLessThanTest()
    {
        BlogChannelSyndicationExtension first = CreateExtension1();
        BlogChannelSyndicationExtension second = CreateExtension2();
        (first < second).ShouldBeFalse();
        (second < first).ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the four URIs the extension was built from.
    /// </summary>
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

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, whose <c>blink</c> is
    /// <c>http://www.example.com/promoted</c>.
    /// </summary>
    /// <returns>An extension carrying all four blogChannel URIs.</returns>
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

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser: the same four URIs, each prefixed
    /// <c>other-</c>.
    /// </summary>
    /// <returns>An extension carrying all four blogChannel URIs.</returns>
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