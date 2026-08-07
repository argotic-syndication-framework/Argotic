using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.CreativeCommons;

/// <summary>
/// Covers <c>CreativeCommonsSyndicationExtension</c>, the module that attaches one or more
/// <c>creativeCommons:license</c> URIs to a feed or an item.
/// </summary>
[TestClass]
public class CreativeCommonsSyndicationExtensionTest
{
    const string namespc = @"xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule""";

    private const string nycText = "<license xmlns=\"http://backend.userland.com/creativeCommonsRssModule\">http://www.example.com/license1.html</license>" +
                                    "<license xmlns=\"http://backend.userland.com/creativeCommonsRssModule\">http://www.example.com/license2.html</license>";

    private const string strExtXml = "<creativeCommons:license>http://www.example.com/license1.html</creativeCommons:license>"
                                     + "<creativeCommons:license>http://www.example.com/license2.html</creativeCommons:license>";

    public TestContext? TestContext { get; set; }
    /// <summary>
    /// Two extensions carrying the same two licences compare equal, so <c>CompareTo</c> returns
    /// <c>0</c> — the licence list is compared by its contents, not by reference.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsCompareToTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        CreativeCommonsSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// Two separately built extensions carrying the same two licences are equal through the
    /// <c>object</c> overload of <c>Equals</c>.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsEqualsTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The hash code is stable across repeated calls, and two equal extensions agree on it.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        CreativeCommonsSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    /// <summary>
    /// Saving a feed with the extension attached writes one <c>creativeCommons:license</c> element per
    /// licence, in the order they were added.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsCreateXmlTest()
    {
        CreativeCommonsSyndicationExtension itunes = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(itunes);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying both licence URIs the document declared, in document order.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as CreativeCommonsSyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — it was a null check wearing a
    ///     type check's clothes, and it inspected neither licence. A non-null extension proves only that
    ///     <i>one</i> <c>license</c> element parsed, so the count is part of the claim.
    /// </remarks>
    [TestMethod]
    public void CreativeCommonsFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        CreativeCommonsSyndicationExtension byType = item.FindExtension<CreativeCommonsSyndicationExtension>().ShouldNotBeNull();
        CreativeCommonsSyndicationExtension byPredicate = item
            .FindExtension(CreativeCommonsSyndicationExtension.MatchByType)
            .ShouldBeOfType<CreativeCommonsSyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.Licenses.Count.ShouldBe(2);
        byPredicate.Context.Licenses[0].ShouldBe(new Uri("http://www.example.com/license1.html"));
        byPredicate.Context.Licenses[1].ShouldBe(new Uri("http://www.example.com/license2.html"));
    }

    /// <summary>
    /// <c>MatchByType</c> accepts an <c>ISyndicationExtension</c> that is a
    /// <c>CreativeCommonsSyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = CreativeCommonsSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> renders both licences as sibling <c>license</c> elements, each declaring the
    /// Creative Commons module namespace as its default.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsToStringTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText);
    }

    /// <summary>
    /// Writing to a non-indenting fragment <c>XmlWriter</c> emits the same two elements as
    /// <c>ToString</c>, without the line breaks or indentation between them.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsWriteToTest()
    {
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(nycText.Replace(Environment.NewLine + "  ", "", StringComparison.Ordinal).Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Two extensions carrying different licence URIs are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsOpEqualityTestFailure()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions carrying the same licence URIs are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsOpEqualityTestSuccess()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension licensed under <c>example.com</c> does not sort above the one under
    /// <c>example.net</c> — the licence lists are compared element by element.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsOpGreaterThanTest()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = first > second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions carrying different licence URIs are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsOpInequalityTest()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The extension licensed under <c>example.com</c> sorts below the one under <c>example.net</c>.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsOpLessThanTest()
    {
        CreativeCommonsSyndicationExtension first = CreateExtension1();
        CreativeCommonsSyndicationExtension second = CreateExtension2();
        bool actual = first < second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>Context</c> property hands back both licence URIs, in the order they were added.
    /// </summary>
    [TestMethod]
    public void CreativeCommonsContextTest()
    {
        CreativeCommonsSyndicationExtension target = CreateExtension1();
        CreativeCommonsSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Licenses.Count.ShouldBe(2);
        context.Licenses[0].ShouldBe(new Uri("http://www.example.com/license1.html"));
        context.Licenses[1].ShouldBe(new Uri("http://www.example.com/license2.html"));
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the lesser, licensed under two
    /// <c>example.com</c> URIs.
    /// </summary>
    /// <returns>An extension carrying two licences.</returns>
    private static CreativeCommonsSyndicationExtension CreateExtension1()
    {
        CreativeCommonsSyndicationExtension nyc = new();

        nyc.Context.Licenses.Add(new Uri("http://www.example.com/license1.html"));
        nyc.Context.Licenses.Add(new Uri("http://www.example.com/license2.html"));
        return nyc;
    }

    /// <summary>
    /// Builds the extension the comparison tests treat as the greater, licensed under two
    /// <c>example.net</c> URIs.
    /// </summary>
    /// <returns>An extension carrying two licences.</returns>
    private static CreativeCommonsSyndicationExtension CreateExtension2()
    {
        CreativeCommonsSyndicationExtension nyc = new();
        nyc.Context.Licenses.Add(new Uri("http://www.example.net/license1.html"));
        nyc.Context.Licenses.Add(new Uri("http://www.example.net/license2.html"));
        return nyc;
    }

    /// <summary>
    /// Builds an empty context, without an extension around it.
    /// </summary>
    /// <returns>A context carrying no licences.</returns>
    public static CreativeCommonsSyndicationExtensionContext CreateContext1() => new();
}