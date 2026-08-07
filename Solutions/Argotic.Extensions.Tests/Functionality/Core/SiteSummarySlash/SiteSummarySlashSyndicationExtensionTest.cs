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
    /// Two extensions built from the same comment count, section, department and hit parade are equal and
    /// hash equally, and hashing one twice gives the same answer.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>hash.ShouldNotBe(0)</c>, which says nothing about any
    ///     implementation: <see cref="HashCode.Combine{T}(T)"/> is seeded per process, so the value is
    ///     unpredictable and only 1 in 2^32 runs would have seen it land on <c>0</c> anyway.
    /// </remarks>
    [TestMethod]
    public void SiteSummarySlashGetHashCodeTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension1();

        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// An RSS 2.0 feed whose item carries <c>slash:comments</c>, CDATA <c>slash:section</c> and
    /// <c>slash:department</c>, and a comma-separated <c>slash:hit_parade</c> yields an extension holding
    /// all four, the hit parade split into its three integers.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        SiteSummarySlashSyndicationExtension extension = item.FindExtension<SiteSummarySlashSyndicationExtension>().ShouldNotBeNull();
        extension.Context.Comments.ShouldBe(42);
        extension.Context.Section.ShouldBe("Technology");
        extension.Context.Department.ShouldBe("Software");
        extension.Context.HitParade.Count.ShouldBe(3);
        extension.Context.HitParade[0].ShouldBe(100);
        extension.Context.HitParade[1].ShouldBe(200);
        extension.Context.HitParade[2].ShouldBe(300);
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
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying the comment count and the department the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as SiteSummarySlashSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected no parsed value.
    /// </remarks>
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
        SiteSummarySlashSyndicationExtension byType = item.FindExtension<SiteSummarySlashSyndicationExtension>().ShouldNotBeNull();
        SiteSummarySlashSyndicationExtension byPredicate = item
            .FindExtension(SiteSummarySlashSyndicationExtension.MatchByType)
            .ShouldBeOfType<SiteSummarySlashSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Comments.ShouldBe(42);
        byPredicate.Context.Department.ShouldBe("Software");
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
    /// The extension counting 42 comments sorts above the one counting 10, and the reverse comparison
    /// agrees.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpGreaterThanTest()
    {
        // Ordering is decided by the first member that differs, and Context.Comments is compared before
        // department, section or hit parade: 42 against 10 makes extension 1 the greater.
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        (first > second).ShouldBeTrue();
        (second > first).ShouldBeFalse();
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
    /// <c>&lt;</c> agrees with <c>&gt;</c>: the extension counting 10 comments is the lesser of the two,
    /// in both directions.
    /// </summary>
    [TestMethod]
    public void SiteSummarySlashOpLessThanTest()
    {
        SiteSummarySlashSyndicationExtension first = CreateExtension1();
        SiteSummarySlashSyndicationExtension second = CreateExtension2();
        (first < second).ShouldBeFalse();
        (second < first).ShouldBeTrue();
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