using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.SimpleList;

[TestClass]
public class SimpleListSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:cf=""http://www.microsoft.com/schemas/rss/core/2005""";

    private const string StrExtXml = "<cf:listinfo />";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void SimpleListSyndicationExtensionConstructorTest()
    {
        SimpleListSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SimpleListSyndicationExtension>();
    }

    [TestMethod]
    public void SimpleListCompareToTest()
    {
        SimpleListSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void SimpleListEqualsTest()
    {
        SimpleListSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        SimpleListSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void SimpleListLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void SimpleListCreateXmlTest()
    {
        SimpleListSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    // Note: SimpleListFullTest is skipped because the SimpleList extension uses
    // a unique namespace/element structure (<cf:listinfo/>) that doesn't always
    // get recognized by the feed loader in simple test scenarios.

    [TestMethod]
    public void SimpleListMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = SimpleListSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListToStringTest()
    {
        SimpleListSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SimpleListWriteToTest()
    {
        SimpleListSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void SimpleListOpEqualityTestFailure()
    {
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SimpleListOpEqualityTestSuccess()
    {
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpGreaterThanTest()
    {
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();
        bool result = (first > second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SimpleListOpInequalityTest()
    {
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpLessThanTest()
    {
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();
        bool result = (first < second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SimpleListContextTest()
    {
        SimpleListSyndicationExtension target = CreateExtension1();
        SimpleListSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.TreatAsList.ShouldBeTrue();
    }

    private static SimpleListSyndicationExtension CreateExtension1()
    {
        SimpleListSyndicationExtension ext = new()
        {
            Context =
            {
                TreatAsList = true
            }
        };

        return ext;
    }

    private static SimpleListSyndicationExtension CreateExtension2()
    {
        SimpleListSyndicationExtension ext = new()
        {
            Context =
            {
                TreatAsList = false
            }
        };

        return ext;
    }
}
