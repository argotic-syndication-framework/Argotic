namespace Argotic.Extensions.Tests;

using System;
using System.IO;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using System.Globalization;
using System.Linq;

/// <summary>
///This is a test class for DublinCoreElementSetSyndicationExtensionTest and is intended
///to contain all DublinCoreElementSetSyndicationExtensionTest Unit Tests
///</summary>
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

    public TestContext TestContext { get; set; }

    /// <summary>
    ///A test for DublinCoreElementSetSyndicationExtension Constructor
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSetSyndicationExtensionConstructorTest()
    {
        DublinCoreElementSetSyndicationExtension target = new DublinCoreElementSetSyndicationExtension();
        Assert.IsNotNull(target);
        Assert.IsInstanceOfType(target, typeof(DublinCoreElementSetSyndicationExtension));
    }

    /// <summary>
    ///A test for CompareTo
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_CompareToTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        int expected = 0;
        int actual = target.CompareTo(obj);
        Assert.AreEqual(expected, actual);
    }

    public void DublinCore_TypeVocabularyAsString()
    {
        DublinCoreTypeVocabularies value = DublinCoreTypeVocabularies.MovingImage;
        string expected = "MovingImage";
        string actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyAsString(value);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for ConvertDegreesMinutesSecondsToDecimal
    ///</summary>
    [TestMethod]
    public void DublinCore_TypeVocabularyByName()
    {
        DublinCoreTypeVocabularies expected = DublinCoreTypeVocabularies.MovingImage;
        DublinCoreTypeVocabularies actual = DublinCoreElementSetSyndicationExtension.TypeVocabularyByName("MovingImage");
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for Equals
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_EqualsTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool expected = true;
        bool actual = target.Equals(obj);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for GetHashCode
    ///</summary>
    [TestMethod, Ignore]
    public void DublinCoreElementSet_GetHashCodeTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        int expected = 1398804031;
        int actual = target.GetHashCode();
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for Load
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_LoadTest()
    {
        DublinCoreElementSetSyndicationExtension target = new DublinCoreElementSetSyndicationExtension(); // TODO: Initialize to an appropriate value
        NameTable nt = new NameTable();
        XmlNamespaceManager ns = new XmlNamespaceManager(nt);
        XmlParserContext xpc = new XmlParserContext(nt, ns, "US-en", XmlSpace.Default);
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, xpc);
        RssFeed feed = new RssFeed();
        feed.Load(reader);
    }

    [TestMethod]
    public void DublinCoreElementSet_CreateXmlTest()
    {
        DublinCoreElementSetSyndicationExtension dub = CreateExtension1();

        string actual = ExtensionTestUtil.AddExtensionToXml(dub);
        string expected = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    public void DublinCoreElementSet_FullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(namespc, strExtXml);

        using XmlReader reader = new XmlTextReader(strXml, XmlNodeType.Document, null);
        RssFeed feed = new RssFeed();
        feed.Load(reader);

        Assert.AreEqual(1, feed.Channel.Items.Count());
        RssItem item = feed.Channel.Items.Single();
        Assert.IsTrue(item.HasExtensions);
        DublinCoreElementSetSyndicationExtension itemExtension = item.FindExtension<DublinCoreElementSetSyndicationExtension>();
        Assert.IsNotNull(itemExtension);
        Assert.IsInstanceOfType(
            item.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension,
            typeof(DublinCoreElementSetSyndicationExtension));
    }

    /// <summary>
    ///A test for MatchByType
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_MatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool expected = true;
        bool actual = DublinCoreElementSetSyndicationExtension.MatchByType(extension);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for ToString
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_ToStringTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        string expected = nycText;
        string actual = target.ToString();
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for WriteTo
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_WriteToTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new StringWriter();
        using XmlWriter writer = new XmlTextWriter(sw);
        target.WriteTo(writer);
        string output = sw.ToString();
        Assert.AreEqual(nycText.Replace(Environment.NewLine, ""), output.Replace(Environment.NewLine, ""));
    }

    /// <summary>
    ///A test for op_Equality
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_op_EqualityTest_Failure()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool expected = false;
        bool actual = (first == second);
        Assert.AreEqual(expected, actual);
    }

    public void DublinCoreElementSet_op_EqualityTest_Success()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension1();
        bool expected = true;
        bool actual = (first == second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for op_GreaterThan
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_op_GreaterThanTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool expected = false;
        bool actual = false;
        actual = (first > second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for op_Inequality
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_op_InequalityTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool expected = true;
        bool actual = (first != second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for op_LessThan
    ///</summary>
    [TestMethod]
    public void DublinCoreElementSet_op_LessThanTest()
    {
        DublinCoreElementSetSyndicationExtension first = CreateExtension1();
        DublinCoreElementSetSyndicationExtension second = CreateExtension2();
        bool expected = true;
        bool actual = (first < second);
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for Context
    ///</summary>
    [TestMethod, Ignore]
    public void DublinCoreElementSet_ContextTest()
    {
        DublinCoreElementSetSyndicationExtension target = CreateExtension1();
        DublinCoreElementSetSyndicationExtensionContext expected = CreateContext1();
        DublinCoreElementSetSyndicationExtensionContext actual = target.Context;

        Assert.AreEqual(expected, actual);
        Assert.Inconclusive("Verify the correctness of this test method.");
    }

    private DublinCoreElementSetSyndicationExtension CreateExtension1()
    {
        DublinCoreElementSetSyndicationExtension dub = new DublinCoreElementSetSyndicationExtension
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

    private DublinCoreElementSetSyndicationExtension CreateExtension2()
    {
        DublinCoreElementSetSyndicationExtension dub = new DublinCoreElementSetSyndicationExtension
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
        DublinCoreElementSetSyndicationExtensionContext dub = new DublinCoreElementSetSyndicationExtensionContext
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