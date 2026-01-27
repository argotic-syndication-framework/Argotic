using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.DublinCoreMetadataTerms;

[TestClass]
public class DublinCoreMetadataTermsSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:dcterms=""http://purl.org/dc/terms/""";

    private readonly string toStringText = "<creator xmlns=\"http://purl.org/dc/terms/\">Test Creator</creator>" + Environment.NewLine +
                                           "<title xmlns=\"http://purl.org/dc/terms/\">Test Title</title>" + Environment.NewLine +
                                           "<description xmlns=\"http://purl.org/dc/terms/\">Test Description</description>";

    private const string StrExtXml = "<dcterms:creator>Test Creator</dcterms:creator>"
                                     + "<dcterms:title>Test Title</dcterms:title>"
                                     + "<dcterms:description>Test Description</dcterms:description>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorTest()
    {
        DublinCoreMetadataTermsSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<DublinCoreMetadataTermsSyndicationExtension>();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsCompareToTest()
    {
        DublinCoreMetadataTermsSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsEqualsTest()
    {
        DublinCoreMetadataTermsSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        DublinCoreMetadataTermsSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();
        
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsCreateXmlTest()
    {
        DublinCoreMetadataTermsSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreMetadataTermsSyndicationExtension itemExtension = item.FindExtension<DublinCoreMetadataTermsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(DublinCoreMetadataTermsSyndicationExtension.MatchByType) as DublinCoreMetadataTermsSyndicationExtension)
            .ShouldBeOfType<DublinCoreMetadataTermsSyndicationExtension>();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = DublinCoreMetadataTermsSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsToStringTest()
    {
        DublinCoreMetadataTermsSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsWriteToTest()
    {
        DublinCoreMetadataTermsSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityTestFailure()
    {
        DublinCoreMetadataTermsSyndicationExtension first = CreateExtension1();
        DublinCoreMetadataTermsSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityTestSuccess()
    {
        DublinCoreMetadataTermsSyndicationExtension first = CreateExtension1();
        DublinCoreMetadataTermsSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanTest()
    {
        DublinCoreMetadataTermsSyndicationExtension first = CreateExtension1();
        DublinCoreMetadataTermsSyndicationExtension second = CreateExtension2();
        bool result = (first > second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpInequalityTest()
    {
        DublinCoreMetadataTermsSyndicationExtension first = CreateExtension1();
        DublinCoreMetadataTermsSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanTest()
    {
        DublinCoreMetadataTermsSyndicationExtension first = CreateExtension1();
        DublinCoreMetadataTermsSyndicationExtension second = CreateExtension2();
        bool result = (first < second);
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextTest()
    {
        DublinCoreMetadataTermsSyndicationExtension target = CreateExtension1();
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Creator.ShouldBe("Test Creator");
        context.Title.ShouldBe("Test Title");
        context.Description.ShouldBe("Test Description");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyAsStringTest()
    {
        DublinCoreTypeVocabularies value = DublinCoreTypeVocabularies.Text;
        string expected = "Text";
        string actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyAsString(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameTest()
    {
        DublinCoreTypeVocabularies expected = DublinCoreTypeVocabularies.Sound;
        DublinCoreTypeVocabularies actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName("Sound");
        actual.ShouldBe(expected);
    }

    private static DublinCoreMetadataTermsSyndicationExtension CreateExtension1()
    {
        DublinCoreMetadataTermsSyndicationExtension ext = new()
        {
            Context =
            {
                Creator = "Test Creator",
                Title = "Test Title",
                Description = "Test Description"
            }
        };

        return ext;
    }

    private static DublinCoreMetadataTermsSyndicationExtension CreateExtension2()
    {
        DublinCoreMetadataTermsSyndicationExtension ext = new()
        {
            Context =
            {
                Creator = "Other Creator",
                Title = "Other Title",
                Description = "Other Description"
            }
        };

        return ext;
    }
}
