using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.Trackback;

/// <summary>
/// Covers <c>TrackbackSyndicationExtension</c>, the module whose <c>trackback:ping</c> element names the
/// URL an item accepts trackbacks at.
/// </summary>
[TestClass]
public class TrackbackSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:trackback=""http://madskills.com/public/xml/rss/module/trackback/""";

    private readonly string toStringText = "<ping xmlns=\"http://madskills.com/public/xml/rss/module/trackback/\">http://www.example.com/trackback/1</ping>";

    private const string StrExtXml = "<trackback:ping>http://www.example.com/trackback/1</trackback:ping>";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor produces a non-null <c>TrackbackSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackSyndicationExtensionConstructorTest()
    {
        TrackbackSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<TrackbackSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions built from the same ping URL compare equal, so <c>CompareTo</c> returns <c>0</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackCompareToTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        TrackbackSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same ping URL are equal through the <c>object</c>
    /// overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackEqualsTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Two extensions built from the same ping URL are equal and hash equally, and hashing one twice
    /// gives the same answer.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>hash.ShouldNotBe(0)</c>, which says nothing about any
    ///     implementation: <see cref="HashCode.Combine{T}(T)"/> is seeded per process, so the value is
    ///     unpredictable and only 1 in 2^32 runs would have seen it land on <c>0</c> anyway.
    /// </remarks>
    [TestMethod]
    public void TrackbackGetHashCodeTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension1();

        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// A feed carrying a <c>trackback:ping</c> element yields an extension holding the ping URL the
    /// document declared.
    /// </summary>
    [TestMethod]
    public void TrackbackLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        TrackbackSyndicationExtension extension = item.FindExtension<TrackbackSyndicationExtension>().ShouldNotBeNull();
        extension.Context.Ping.ShouldBe(new Uri("http://www.example.com/trackback/1"));
    }

    /// <summary>
    /// Saving a feed with the extension attached writes a single <c>trackback:ping</c> element, with the
    /// namespace declared on the <c>rss</c> element.
    /// </summary>
    [TestMethod]
    public void TrackbackCreateXmlTest()
    {
        TrackbackSyndicationExtension ext = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the ping URL the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as TrackbackSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected no parsed value.
    /// </remarks>
    [TestMethod]
    public void TrackbackFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        TrackbackSyndicationExtension byType = item.FindExtension<TrackbackSyndicationExtension>().ShouldNotBeNull();
        TrackbackSyndicationExtension byPredicate = item
            .FindExtension(TrackbackSyndicationExtension.MatchByType)
            .ShouldBeOfType<TrackbackSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Ping.ShouldBe(new Uri("http://www.example.com/trackback/1"));
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>TrackbackSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = TrackbackSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders the single <c>ping</c> element declaring the trackback namespace as its
    /// default rather than carrying the prefix.
    /// </summary>
    [TestMethod]
    public void TrackbackToStringTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(toStringText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same element as <c>ToString</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackWriteToTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions naming different ping URLs are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackOpEqualityTestFailure()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions naming the same ping URL are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackOpEqualityTestSuccess()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension pinging <c>trackback/1</c> does not sort above the one pinging <c>trackback/2</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackOpGreaterThanTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions naming different ping URLs are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackOpInequalityTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension pinging <c>trackback/1</c> sorts below the one pinging <c>trackback/2</c>.
    /// </summary>
    [TestMethod]
    public void TrackbackOpLessThanTest()
    {
        TrackbackSyndicationExtension first = CreateExtension1();
        TrackbackSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back the ping URL the extension was built from.
    /// </summary>
    [TestMethod]
    public void TrackbackContextTest()
    {
        TrackbackSyndicationExtension target = CreateExtension1();
        TrackbackSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Ping.ShouldBe(new Uri("http://www.example.com/trackback/1"));
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser, pinging
    /// <c>http://www.example.com/trackback/1</c>.
    /// </summary>
    /// <returns>An extension carrying a ping URL.</returns>
    private static TrackbackSyndicationExtension CreateExtension1()
    {
        TrackbackSyndicationExtension ext = new()
        {
            Context =
            {
                Ping = new Uri("http://www.example.com/trackback/1")
            }
        };

        return ext;
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, pinging
    /// <c>http://www.example.com/trackback/2</c>.
    /// </summary>
    /// <returns>An extension carrying a ping URL.</returns>
    private static TrackbackSyndicationExtension CreateExtension2()
    {
        TrackbackSyndicationExtension ext = new()
        {
            Context =
            {
                Ping = new Uri("http://www.example.com/trackback/2")
            }
        };

        return ext;
    }
}