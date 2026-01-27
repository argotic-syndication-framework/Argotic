using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.DublinCoreElementSet;

[TestClass]
public class DublinCoreElementSetSyndicationExtensionTest
{
    const string namespc = @"xmlns:dc=""http://purl.org/dc/elements/1.1/""";

    private readonly string nycText = "<contributor xmlns=\"http://purl.org/dc/elements/1.1/\">Helper</contributor>" + Environment.NewLine +
                                      "<coverage xmlns=\"http://purl.org/dc/elements/1.1/\">US</coverage>" + Environment.NewLine +
                                      "<creator xmlns=\"http://purl.org/dc/elements/1.1/\">The Big Guy</creator>" + Environment.NewLine +
                                      "<date xmlns=\"http://purl.org/dc/elements/1.1/\">2010-08-01T00:00:00.00Z</date>" + Environment.NewLine +
                                      "<description xmlns=\"http://purl.org/dc/elements/1.1/\">That kind of thing</description>" + Environment.NewLine +
                                      "<format xmlns=\"http://purl.org/dc/elements/1.1/\">CDROM</format>" + Environment.NewLine +
                                      "<identifier xmlns=\"http://purl.org/dc/elements/1.1/\">MYTESTCDROM-1</identifier>" + Environment.NewLine +
                                      "<language xmlns=\"http://purl.org/dc/elements/1.1/\">en-US</language>" + Environment.NewLine +
                                      "<publisher xmlns=\"http://purl.org/dc/elements/1.1/\">MeMeMe</publisher>" + Environment.NewLine +
                                      "<relation xmlns=\"http://purl.org/dc/elements/1.1/\">MYTESTCDROM-2</relation>" + Environment.NewLine +
                                      "<rights xmlns=\"http://purl.org/dc/elements/1.1/\">Copyright 2010</rights>" + Environment.NewLine +
                                      "<source xmlns=\"http://purl.org/dc/elements/1.1/\">Out of Me Head</source>" + Environment.NewLine +
                                      "<subject xmlns=\"http://purl.org/dc/elements/1.1/\">Test data (Stupid variety)</subject>" + Environment.NewLine +
                                      "<title xmlns=\"http://purl.org/dc/elements/1.1/\">Stupid test data</title>" + Environment.NewLine +
                                      "<type xmlns=\"http://purl.org/dc/elements/1.1/\">PhysicalObject</type>";

    private const string strExtXml = "<dc:contributor>Helper</dc:contributor>"
                                     + "<dc:coverage>US</dc:coverage>"
                                     + "<dc:creator>The Big Guy</dc:creator>"
                                     + "<dc:date>2010-08-01T00:00:00.00Z</dc:date>"
                                     + "<dc:description>That kind of thing</dc:description>"
                                     + "<dc:format>CDROM</dc:format>"
                                     + "<dc:identifier>MYTESTCDROM-1</dc:identifier>"
                                     + "<dc:language>en-US</dc:language>"
                                     + "<dc:publisher>MeMeMe</dc:publisher>"
                                     + "<dc:relation>MYTESTCDROM-2</dc:relation>"
                                     + "<dc:rights>Copyright 2010</dc:rights>"
                                     + "<dc:source>Out of Me Head</dc:source>"
                                     + "<dc:subject>Test data (Stupid variety)</dc:subject>"
                                     + "<dc:title>Stupid test data</dc:title>"
                                     + "<dc:type>PhysicalObject</dc:type>";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void DublinCoreElementSetSyndicationExtensionConstructorTest()
    {
        DublinCoreElementSetSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<DublinCoreElementSetSyndicationExtension>();
    }

    [TestMethod]
    public void DublinCoreElementSetCompareToTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int actual = target.CompareTo(obj);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void DublinCoreTypeVocabularyAsString()
    {
        DublinCoreTypeVocabularies value = DublinCoreTypeVocabularies.MovingImage;
        string expected = "MovingImage";
        string actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(value);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreTypeVocabularyByName()
    {
        DublinCoreTypeVocabularies expected = DublinCoreTypeVocabularies.MovingImage;
        DublinCoreTypeVocabularies actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyByName("MovingImage");
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreElementSetEqualsTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreElementSetGetHashCodeTest()
    {
        // Consistency: same object returns same hash
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        target.GetHashCode().ShouldBe(target.GetHashCode());

        // Equality contract: equal objects have equal hashes
        DublinCoreElementSetSyndicationExtension other = CreateExtension1();
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    [TestMethod]
    public void DublinCoreElementSetLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void DublinCoreElementSetCreateXmlTest()
    {
        DublinCoreElementSetSyndicationExtension dub = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(dub);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreElementSetFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreElementSetSyndicationExtension itemExtension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension)
            .ShouldBeOfType<DublinCoreElementSetSyndicationExtension>();
    }

    [TestMethod]
    public void DublinCoreElementSetMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = DublinCoreElementSetSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreElementSetToStringTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldBe(nycText);
    }

    [TestMethod]
    public void DublinCoreElementSetWriteToTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "").ShouldBe(nycText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void DublinCoreElementSetOpEqualityTestFailure()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = (first == second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreElementSetOpEqualityTestSuccess()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension1();
        bool actual = (first == second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreElementSetOpGreaterThanTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = (first > second);
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreElementSetOpInequalityTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = (first != second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreElementSetOpLessThanTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool actual = (first < second);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreElementSetContextTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        DublinCoreElementSetSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Contributor.ShouldBe("Helper");
        context.Coverage.ShouldBe("US");
        context.Creator.ShouldBe("The Big Guy");
        context.Date.ShouldBe(new DateTime(2010, 8, 1));
        context.Description.ShouldBe("That kind of thing");
        context.Format.ShouldBe("CDROM");
        context.Identifier.ShouldBe("MYTESTCDROM-1");
        context.Language.Name.ShouldBe("en-US");
        context.Publisher.ShouldBe("MeMeMe");
        context.Relation.ShouldBe("MYTESTCDROM-2");
        context.Rights.ShouldBe("Copyright 2010");
        context.Source.ShouldBe("Out of Me Head");
        context.Subject.ShouldBe("Test data (Stupid variety)");
        context.Title.ShouldBe("Stupid test data");
        context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.PhysicalObject);
    }

    private static DublinCoreElementSetSyndicationExtension CreateExtension1()
    {
        DublinCoreElementSetSyndicationExtension dub = new()
        {
            Context =
            {
                Contributor = "Helper",
                Coverage = "US",
                Creator = "The Big Guy",
                Date = new DateTime(2010, 8, 1),
                Description = "That kind of thing",
                Format = "CDROM",
                Identifier = "MYTESTCDROM-1",
                Language = new CultureInfo("en-US"),
                Publisher = "MeMeMe",
                Relation = "MYTESTCDROM-2",
                Rights = "Copyright 2010",
                Source = "Out of Me Head",
                Subject = "Test data (Stupid variety)",
                Title = "Stupid test data",
                TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject
            }
        };

        return dub;
    }

    private static DublinCoreElementSetSyndicationExtension CreateExtension2()
    {
        DublinCoreElementSetSyndicationExtension dub = new()
        {
            Context =
            {
                Contributor = "Helper-er",
                Coverage = "US",
                Creator = "The Not-So-Big Guy",
                Date = new DateTime(2010, 8, 1),
                Description = "This kind of thing",
                Format = "CDROM",
                Identifier = "MYTESTCDROM-2",
                Language = new CultureInfo("en-US"),
                Publisher = "MeMyselfI",
                Relation = "MYTESTCDROM-1",
                Rights = "Copyright 2010",
                Source = "Nowheres, man",
                Subject = "Test data (Son of)",
                Title = "More Stupid test data",
                TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject
            }
        };

        return dub;
    }

    public static DublinCoreElementSetSyndicationExtensionContext CreateContext1()
    {
        DublinCoreElementSetSyndicationExtensionContext dub = new()
        {
            Contributor = "",
            Coverage = "",
            Creator = "",
            Date = new DateTime(2010, 8, 1),
            Description = "",
            Format = "",
            Identifier = "",
            Language = new CultureInfo("US-en"),
            Publisher = "",
            Relation = "",
            Rights = "",
            Source = "",
            Subject = "",
            Title = "",
            TypeVocabulary = DublinCoreTypeVocabularies.PhysicalObject
        };

        return dub;
    }
}
