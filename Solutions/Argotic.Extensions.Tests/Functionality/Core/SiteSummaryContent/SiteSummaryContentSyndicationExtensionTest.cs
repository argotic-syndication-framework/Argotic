using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummaryContent;

[TestClass]
public class SiteSummaryContentSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:content=""http://purl.org/rss/1.0/modules/content/""";

    private readonly string toStringText = "<encoded xmlns=\"http://purl.org/rss/1.0/modules/content/\"><![CDATA[<p>Test encoded content</p>]]></encoded>";

    private const string StrExtXml = "<content:encoded><![CDATA[<p>Test encoded content</p>]]></content:encoded>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void SiteSummaryContentSyndicationExtensionConstructorTest()
    {
        SiteSummaryContentSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SiteSummaryContentSyndicationExtension>();
    }

    [TestMethod]
    public void SiteSummaryContentCompareToTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        SiteSummaryContentSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void SiteSummaryContentEqualsTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryContentGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void SiteSummaryContentLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void SiteSummaryContentCreateXmlTest()
    {
        SiteSummaryContentSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

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
        SiteSummaryContentSyndicationExtension itemExtension = item.FindExtension<SiteSummaryContentSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(SiteSummaryContentSyndicationExtension.MatchByType) as SiteSummaryContentSyndicationExtension)
            .ShouldBeOfType<SiteSummaryContentSyndicationExtension>();
    }

    [TestMethod]
    public void SiteSummaryContentMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SiteSummaryContentSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryContentToStringTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SiteSummaryContentWriteToTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void SiteSummaryContentOpEqualityTestFailure()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SiteSummaryContentOpEqualityTestSuccess()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryContentOpGreaterThanTest()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        bool result = first > second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SiteSummaryContentOpInequalityTest()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryContentOpLessThanTest()
    {
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension2();
        bool result = first < second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SiteSummaryContentContextTest()
    {
        SiteSummaryContentSyndicationExtension target = CreateExtension1();
        SiteSummaryContentSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Encoded.ShouldBe("<p>Test encoded content</p>");
    }

    [TestMethod]
    public void SiteSummaryContentContextSetterThrowsOnNull()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

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
        SiteSummaryContentSyndicationExtension itemExtension = item.FindExtension<SiteSummaryContentSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Encoded.ShouldBe("<p>Test encoded content</p>");
    }

    [TestMethod]
    public void SiteSummaryContentOpLessThanOrEqualTest()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    [TestMethod]
    public void SiteSummaryContentOpGreaterThanOrEqualTest()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension first = CreateExtension1();
        SiteSummaryContentSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

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

    [TestMethod]
    public void SiteSummaryContentEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        SiteSummaryContentSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

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

    [TestMethod]
    public void SiteSummaryContentCompareSequenceTest()
    {
        // Arrange
        IList<SiteSummaryContentItem> source = new List<SiteSummaryContentItem>
        {
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") }
        };
        IList<SiteSummaryContentItem> target = new List<SiteSummaryContentItem>
        {
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") }
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void SiteSummaryContentCompareSequenceDifferentCountsTest()
    {
        // Arrange
        IList<SiteSummaryContentItem> source = new List<SiteSummaryContentItem>
        {
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") },
            new() { Content = "Content 2", Format = new Uri("http://example.com/2") }
        };
        IList<SiteSummaryContentItem> target = new List<SiteSummaryContentItem>
        {
            new() { Content = "Content 1", Format = new Uri("http://example.com/1") }
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

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