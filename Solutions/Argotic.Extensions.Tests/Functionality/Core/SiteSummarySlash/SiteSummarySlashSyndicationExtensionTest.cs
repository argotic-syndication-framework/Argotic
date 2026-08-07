using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.SiteSummarySlash;

/// <summary>
/// Covers the Slashdot slash module, <c>http://purl.org/rss/1.0/modules/slash/</c>: the comment count,
/// section, department and comma-separated <c>hit_parade</c> its context carries, how they are read from
/// an RSS 2.0 item, and its comparison, equality and ordering contracts.
/// </summary>
[TestClass]
public class SiteSummarySlashSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:slash=""http://purl.org/rss/1.0/modules/slash/""";

    private const string StrExtXml = "<slash:comments>42</slash:comments>"
                                     + "<slash:section><![CDATA[Technology]]></slash:section>"
                                     + "<slash:department><![CDATA[Software]]></slash:department>"
                                     + "<slash:hit_parade>100,200,300</slash:hit_parade>";

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// The parameterless constructor yields an instance of the slash-module extension type.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashSyndicationExtensionConstructorTest()
    {
        SiteSummarySlashSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SiteSummarySlashSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashCompareToTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        SiteSummarySlashSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashEqualsTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Hashing a populated extension returns a non-zero value rather than throwing.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// An RSS 2.0 feed whose item carries <c>slash:</c> elements loads without error.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    /// <summary>
    /// Attaching the extension to an item and saving the feed produces non-empty XML.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashCreateXmlTest()
    {
        SiteSummarySlashSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// A loaded feed's single item reports that it has extensions, and the slash-module one is found both
    /// by type argument and through the <c>MatchByType</c> predicate.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        SiteSummarySlashSyndicationExtension? itemExtension = item.FindExtension<SiteSummarySlashSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(SiteSummarySlashSyndicationExtension.MatchByType) as SiteSummarySlashSyndicationExtension)
            .ShouldBeOfType<SiteSummarySlashSyndicationExtension>();
    }

    /// <summary>
    /// <c>MatchByType</c> accepts a slash-module extension.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SiteSummarySlashSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> returns a non-empty rendering of a populated extension.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashToStringTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// <c>WriteTo</c> produces non-empty XML for a populated extension; the text itself is not asserted.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different context are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpEqualityTestFailure()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding the same context are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpEqualityTestSuccess()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;</c> yields a boolean for two differing extensions without throwing; the direction is not
    /// asserted.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpGreaterThanTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool result = first > second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// Extensions holding different context are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpInequalityTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&lt;</c> yields a boolean for two differing extensions without throwing; the direction is not
    /// asserted.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpLessThanTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        bool result = first < second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// The context of a populated extension carries the comment count, the section, the department and the
    /// hit parade in the order its entries were added.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashContextTest()
    {
        SiteSummarySlashSyndicationExtension target = CreateExtension1();
        SiteSummarySlashSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Comments.ShouldBe(42);
        context.Section.ShouldBe("Technology");
        context.Department.ShouldBe("Software");
        context.HitParade.Count.ShouldBe(3);
        context.HitParade[0].ShouldBe(100);
        context.HitParade[1].ShouldBe(200);
        context.HitParade[2].ShouldBe(300);
    }

    /// <summary>
    /// Assigning a <see langword="null"/> context throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashContextSetterThrowsOnNull()
    {
        // Arrange
        SiteSummarySlashSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// An RSS 2.0 item carrying <c>slash:comments</c>, a CDATA <c>slash:section</c> and
    /// <c>slash:department</c>, and a comma-separated <c>slash:hit_parade</c>, fills all four — the hit
    /// parade split into its three integers, and the section and department read as written.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashRoundTripTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        SiteSummarySlashSyndicationExtension? itemExtension = item.FindExtension<SiteSummarySlashSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Comments.ShouldBe(42);
        itemExtension.Context.Section.ShouldBe("Technology");
        itemExtension.Context.Department.ShouldBe("Software");
        itemExtension.Context.HitParade.Count.ShouldBe(3);
        itemExtension.Context.HitParade.ShouldContain(100);
        itemExtension.Context.HitParade.ShouldContain(200);
        itemExtension.Context.HitParade.ShouldContain(300);
    }

    /// <summary>
    /// <c>&lt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpLessThanOrEqualTest()
    {
        // Arrange
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpGreaterThanOrEqualTest()
    {
        // Arrange
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>MatchByType</c> rejects an extension from another family.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashMatchByTypeReturnsFalseForDifferentType()
    {
        // Arrange
        ISyndicationExtension extension = new SiteSummaryContentSyndicationExtension();

        // Act
        bool actual = SiteSummarySlashSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// An extension is not equal to a value of an unrelated type.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        SiteSummarySlashSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

    /// <summary>
    /// An extension sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashCompareToNullReturnsPositive()
    {
        // Arrange
        SiteSummarySlashSyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
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
        ext.Context.HitParade.Add(100);
        ext.Context.HitParade.Add(200);
        ext.Context.HitParade.Add(300);

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
        ext.Context.HitParade.Add(50);

        return ext;
    }
}