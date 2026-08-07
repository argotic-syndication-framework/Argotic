using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummaryContent;

/// <summary>
/// Covers the RSS content module, <c>http://purl.org/rss/1.0/modules/content/</c>: the
/// <c>content:encoded</c> markup its context carries, the content items beside it, how the markup is
/// read from an RSS 2.0 item and written back out, and its comparison, equality and ordering contracts.
/// </summary>
[TestClass]
public class SiteSummaryContentSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:content=""http://purl.org/rss/1.0/modules/content/""";

    private readonly string toStringText = "<encoded xmlns=\"http://purl.org/rss/1.0/modules/content/\"><![CDATA[<p>Test encoded content</p>]]></encoded>";

    private const string StrExtXml = "<content:encoded><![CDATA[<p>Test encoded content</p>]]></content:encoded>";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor yields an instance of the content-module extension type.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentSyndicationExtensionConstructorTest()
    {
        SiteSummaryContentSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SiteSummaryContentSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentCompareToTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        SiteSummaryContentSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentEqualsTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Two extensions built from the same encoded markup are equal and hash equally, and hashing one
    /// twice gives the same answer.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>hash.ShouldNotBe(0)</c>, which says nothing about any
    ///     implementation: <see cref="HashCode.Combine{T}(T)"/> is seeded per process, so the value is
    ///     unpredictable and only 1 in 2^32 runs would have seen it land on <c>0</c> anyway.
    /// </remarks>
    [TestMethod]
    public void SiteSummaryContentGetHashCodeTest()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();

        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// An RSS 2.0 feed whose item carries a CDATA <c>content:encoded</c> element yields an extension
    /// holding the markup the CDATA section wrapped, unescaped.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        SiteSummaryContentSyndicationExtension extension = item.FindExtension<SiteSummaryContentSyndicationExtension>().ShouldNotBeNull();
        extension.Context.Encoded.ShouldBe("<p>Test encoded content</p>");
    }

    /// <summary>
    /// Attaching the extension to an item and saving the feed produces non-empty XML.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentCreateXmlTest()
    {
        SiteSummaryContentSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the encoded markup the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as SiteSummaryContentSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected no parsed value.
    /// </remarks>
    [TestMethod]
    public void SiteSummaryContentFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        SiteSummaryContentSyndicationExtension byType = item.FindExtension<SiteSummaryContentSyndicationExtension>().ShouldNotBeNull();
        SiteSummaryContentSyndicationExtension byPredicate = item
            .FindExtension(SiteSummaryContentSyndicationExtension.MatchByType)
            .ShouldBeOfType<SiteSummaryContentSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Encoded.ShouldBe("<p>Test encoded content</p>");
    }

    /// <summary>
    /// <c>MatchByType</c> accepts a content-module extension.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SiteSummaryContentSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> returns a non-empty rendering of a populated extension.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentToStringTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// <c>WriteTo</c> emits one <c>encoded</c> element in the content-module namespace, with the markup
    /// wrapped in CDATA rather than entity-escaped.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentWriteToTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Extensions holding different markup are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpEqualityTestFailure()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding the same markup are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpEqualityTestSuccess()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension encoding <c>Test encoded content</c> sorts above the one encoding
    /// <c>Other encoded content</c>, and the reverse comparison agrees.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpGreaterThanTest()
    {
        // Ordering is decided by Context.Encoded under StringComparison.Ordinal, where the first
        // character that differs is 'T' (84) against 'O' (79) — so extension 1 is the greater.
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        (first > second).ShouldBeTrue();
        (second > first).ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding different markup are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpInequalityTest()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&lt;</c> agrees with <c>&gt;</c>: the <c>Other encoded content</c> extension is the lesser of
    /// the two, in both directions.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpLessThanTest()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        (first < second).ShouldBeFalse();
        (second < first).ShouldBeTrue();
    }

    /// <summary>
    /// The context of a populated extension carries the encoded markup exactly as assigned, angle brackets
    /// and all.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentContextTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        SiteSummaryContentSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Encoded.ShouldBe("<p>Test encoded content</p>");
    }

    /// <summary>
    /// Assigning a <see langword="null"/> context throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentContextSetterThrowsOnNull()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// An RSS 2.0 item carrying a CDATA <c>content:encoded</c> fills the context with the markup the CDATA
    /// section wrapped, unescaped.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentRoundTripTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        SiteSummaryContentSyndicationExtension? itemExtension = item.FindExtension<SiteSummaryContentSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Encoded.ShouldBe("<p>Test encoded content</p>");
    }

    /// <summary>
    /// <c>&lt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpLessThanOrEqualTest()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentOpGreaterThanOrEqualTest()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>MatchByType</c> rejects an extension from another family.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentMatchByTypeReturnsFalseForDifferentType()
    {
        // Arrange
        ISyndicationExtension extension = new SiteSummarySlashSyndicationExtension();

        // Act
        bool actual = SiteSummaryContentSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// An extension is not equal to a value of an unrelated type.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

    /// <summary>
    /// An extension sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentCompareToNullReturnsPositive()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A content item added to the context keeps both its content and the format URI naming what that
    /// content is.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentItemsPropertyTest()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension ext = new();
        SiteSummaryContentItem item = new()
        {
            Content = "Item content",
            Format = new Uri("http://www.w3.org/1999/xhtml")
        };

        // Act
        ext.Context.Items.Add(item);

        // Assert
        ext.Context.Items.Count.ShouldBe(1);
        ext.Context.Items[0].Content.ShouldBe("Item content");
        ext.Context.Items[0].Format.ShouldBe(new Uri("http://www.w3.org/1999/xhtml"));
    }

    /// <summary>
    /// Two single-element sequences holding equal content items compare equal.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentCompareSequenceTest()
    {
        // Arrange
        IList<SiteSummaryContentItem> source =
        [
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") }
        ];
        IList<SiteSummaryContentItem> target =
        [
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A longer sequence sorts after a shorter one on count alone, before any element is compared.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentCompareSequenceDifferentCountsTest()
    {
        // Arrange
        IList<SiteSummaryContentItem> source =
        [
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") },
            new() { Content = "Content 2", Format = new Uri("http://example.com/2") }
        ];
        IList<SiteSummaryContentItem> target =
        [
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// <c>WellFormedXmlEncoding</c> is the W3C well-formedness URI,
    /// <c>http://www.w3.org/TR/REC-xml#dt-wellformed</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummaryContentItemWellFormedXmlEncodingTest()
    {
        // Act
        Uri encoding = SiteSummaryContentItem.WellFormedXmlEncoding;

        // Assert
        encoding.ShouldBe(new Uri("http://www.w3.org/TR/REC-xml#dt-wellformed"));
    }

    private static SiteSummaryContentSyndicationExtension CreateExtension1()
    {
        SiteSummaryContentSyndicationExtension ext = new()
        {
            Context =
            {
                Encoded = "<p>Test encoded content</p>"
            }
        };

        return ext;
    }

    private static SiteSummaryContentSyndicationExtension CreateExtension2()
    {
        SiteSummaryContentSyndicationExtension ext = new()
        {
            Context =
            {
                Encoded = "<p>Other encoded content</p>"
            }
        };

        return ext;
    }
}